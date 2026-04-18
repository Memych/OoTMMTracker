using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OoTMMTracker.Models;

namespace OoTMMTracker.Controls
{
    public class ItemTrackerPanel : Panel
    {
        private readonly List<TrackerItem> _items;
        private readonly ToolTip _toolTip;
        private readonly int _columns;
        private static int ItemSize  = 48;
        private static readonly int[] ItemSizeSteps = { 16, 24, 32, 48, 64, 96, 128, 192, 256 };
        private const int ItemGap   = 2;
        private const int HeaderH   = 20;

        public static void SetItemSize(int size) => ItemSize = size;
        public static int GetItemSize() => ItemSize;
        public static int[] GetItemSizeSteps() => ItemSizeSteps;
        private const int PadX      = 4;
        private const int PadTop    = 2;

        // Global registry of all items for cross-panel synchronization (bombBag)
        private static readonly Dictionary<string, TrackerItem> _globalItems = new();
        private static readonly Dictionary<string, PictureBox> _globalPbs = new();
        // Current config for SyncGanonBk
        private static TrackerConfig? _globalCfg;

        public static void ClearGlobal()
        {
            _globalItems.Clear();
            _globalPbs.Clear();
        }

        public static void SetGlobalConfig(TrackerConfig cfg) => _globalCfg = cfg;

        // Fired when any item changes — used by BroadcastForm
        public static event Action? ItemChanged;

        // If true — panel is read-only (broadcast mode)
        public bool IsBroadcast { get; set; } = false;

        public ItemTrackerPanel(string title, List<TrackerItem> items, int columns, ToolTip toolTip)
        {
            _items   = items;
            _toolTip = toolTip;
            _columns = columns;

            int rows   = (int)Math.Ceiling((double)items.Count / columns);

            // If all items are the same size — use it, otherwise ItemSize
            int effectiveSize = items.Count > 0 && items[0].CustomSize > 0 ? items[0].CustomSize : ItemSize;
            int panelW = PadX * 2 + columns * effectiveSize + (columns - 1) * ItemGap;

            // Minimum width — so the header fits
            using var titleFont = new Font("Segoe UI", 8, FontStyle.Bold);
            int titleW = TextRenderer.MeasureText(title, titleFont).Width + 10;
            panelW = Math.Max(panelW, titleW);

            int panelH = HeaderH + PadTop + rows * effectiveSize + (rows - 1) * ItemGap + PadX;

            this.Size        = new Size(panelW, panelH);
            this.BackColor   = Color.FromArgb(40, 40, 40);
            this.Margin      = new Padding(4);
            this.BorderStyle = BorderStyle.FixedSingle;

            // Title
            var lblTitle = new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(55, 55, 55),
                Location  = new Point(0, 0),
                Size      = new Size(panelW, HeaderH),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(3, 0, 0, 0)
            };
            this.Controls.Add(lblTitle);

            // Item icons — start AFTER the title
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                int sz   = item.CustomSize > 0 ? item.CustomSize : ItemSize;
                int col  = i % columns;
                int row  = i / columns;
                int x    = PadX + col * (effectiveSize + ItemGap);
                int y    = HeaderH + PadTop + row * (effectiveSize + ItemGap);

                var pb = CreateItemBox(item, x, y, sz);
                this.Controls.Add(pb);
            }
        }

        private PictureBox CreateItemBox(TrackerItem item, int x, int y, int size = 0)
        {
            if (size == 0) size = ItemSize;
            var pb = new PictureBox
            {
                Size      = new Size(size, size),
                Location  = new Point(x, y),
                BackColor = GetBaseColor(item),
                Cursor    = IsBroadcast ? Cursors.Default : Cursors.Hand,
                Tag       = item,
                SizeMode  = PictureBoxSizeMode.StretchImage
            };

            RefreshIcon(pb, item);
            UpdateTooltip(pb, item);
            pb.Paint     += ItemBox_Paint;
            pb.MouseDown += ItemBox_MouseDown;

            // Only register in global registry for main tracker (not broadcast)
            if (!IsBroadcast)
            {
                _globalItems[item.Id] = item;
                _globalPbs[item.Id]   = pb;
            }

            return pb;
        }

        private static void RefreshIcon(PictureBox pb, TrackerItem item)
        {
            Image? img = null;

            if (item.IsDungeonReward)
            {
                // Show icon of selected reward (0 = unknown)
                var path = item.RewardIndex < item.RewardIcons!.Length
                    ? item.RewardIcons[item.RewardIndex]
                    : item.RewardIcons[0];
                img = LoadIconPath(path);
            }
            else if (item.StepIconPaths != null && item.StepIconPaths.Length > 0)
            {
                int idx = item.CurrentCount > 0 ? Math.Min(item.CurrentCount - 1, item.StepIconPaths.Length - 1) : 0;
                img = LoadIconPath(item.StepIconPaths[idx]);
            }

            if (img == null && item.IconPath != null)
                img = LoadIconPath(item.IconPath);

            pb.Image = img;
        }

        // ─── Icon Loading ──────────────────────────────────────────────────────
        // Cache of loaded images — don't re-read from disk/resources every time
        private static readonly Dictionary<string, Image?> _imageCache = new();
        private static HashSet<string>? _resourceNames;

        private static HashSet<string> GetResourceNames()
        {
            if (_resourceNames != null) return _resourceNames;
            var names = System.Reflection.Assembly.GetExecutingAssembly()
                              .GetManifestResourceNames();
            _resourceNames = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
            return _resourceNames;
        }

        private static Image? LoadIconPath(string? iconPath)
        {
            if (iconPath == null) return null;
            if (_imageCache.TryGetValue(iconPath, out var cached)) return cached;

            Image? img = null;

            // 1. Try loading from embedded resources
            // LogicalName = "%(RecursiveDir)%(Filename)%(Extension)" → name = "subdir\file.png"
            // iconPath uses "/" → normalize to "\"
            try
            {
                var resKey = iconPath.Replace('/', '\\');
                if (GetResourceNames().Contains(resKey))
                {
                    var asm = System.Reflection.Assembly.GetExecutingAssembly();
                    using var stream = asm.GetManifestResourceStream(resKey);
                    if (stream != null)
                        img = Image.FromStream(new MemoryStream(ReadAllBytes(stream)));
                }
            }
            catch { }

            // 2. Fallback — file system (for development without rebuild)
            if (img == null)
            {
                try
                {
                    var exeDir = Path.GetDirectoryName(Application.ExecutablePath) ?? ".";
                    var path   = Path.Combine(exeDir, "Resources", "Images", iconPath);
                    if (File.Exists(path))
                        img = Image.FromFile(path);
                }
                catch { }
            }

            _imageCache[iconPath] = img;
            return img;
        }

        private static byte[] ReadAllBytes(Stream stream)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        private void UpdateTooltip(PictureBox pb, TrackerItem item)
        {
            string tip;
            if (item.IsDungeonReward)
            {
                var reward = item.RewardIndex > 0 && item.RewardNames != null
                    ? item.RewardNames[item.RewardIndex]
                    : "?";
                tip = $"{item.Name} — {reward}{(item.DungeonCleared ? " ✓" : "")}";
            }
            else if (item.IsBottle)
            {
                var status = item.BottleContent == "Empty" ? "Empty" :
                             item.CurrentCount == 0 ? $"{item.BottleContent} (not found)" :
                             item.CurrentCount == 2 ? $"{item.BottleContent} (used)" :
                             item.BottleContent;
                tip = $"{item.Name}: {status}\nRight-click to change content";
            }
            else if (item.IsAutoKey)
            {
                int threshold = item.AutoKeyThreshold > 0 ? item.AutoKeyThreshold : item.MaxCount;
                tip = $"{item.Name}: {item.CurrentCount}/{threshold} (max {item.MaxCount})";
            }
            else
            {
                tip = item.StepNames != null && item.CurrentCount > 0
                    ? $"{item.CurrentStepName} ({item.CurrentCount}/{item.MaxCount})"
                    : item.Name;
            }
            _toolTip.SetToolTip(pb, tip);
        }

        private void ItemBox_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not PictureBox pb || pb.Tag is not TrackerItem item)
                return;

            var g    = e.Graphics;
            var rect = pb.ClientRectangle;

            // Dimming
            bool isDim;
            if (item.IsDungeonReward)
                isDim = !item.DungeonCleared;
            else if (item.IsAutoKey)
                isDim = !item.AutoKeyLit;
            else if (item.CollectedWhenFull)
                isDim = item.CurrentCount < item.MaxCount;
            else if (item.PartialCollectedAt > 0)
                isDim = item.CurrentCount < item.PartialCollectedAt;
            else
                isDim = !item.IsCollected;

            if (isDim)
            {
                using var dim = new SolidBrush(Color.FromArgb(160, 0, 0, 0));
                g.FillRectangle(dim, rect);
            }

            // Text on icon (for hearts, wallets) — only when collected
            string? label = null;
            if (item.StepLabels != null && item.CurrentCount > 0 && item.CurrentCount <= item.StepLabels.Length)
                label = item.StepLabels[item.CurrentCount - 1];

            // Permanent label — always visible
            if (label == null && item.StaticLabel != null)
                label = item.StaticLabel;

            if (label != null)
            {
                bool isMax = item.CurrentCount >= item.MaxCount && item.MaxCount > 0;

                // For notes mode — hide label at max (works for both regular songs and Goron Lullaby with Intro)
                if (isMax && item.Type == TrackerItemType.Song && item.StepLabels != null)
                    label = null;
            }

            if (label != null)
            {
                bool isMax = item.CurrentCount >= item.MaxCount && item.MaxCount > 0;

                // Adaptive font: reduce if label is long
                float fontSize = label.Length > 10 ? 4.5f : label.Length > 7 ? 5.5f : label.Length > 4 ? 6.5f : 7f;
                using var font = new Font("Arial", fontSize, FontStyle.Bold);
                var sz = g.MeasureString(label, font);
                float tx = (rect.Width - sz.Width) / 2f;
                float ty = rect.Height - sz.Height;

                using var shadow = new SolidBrush(Color.Black);
                g.DrawString(label, font, shadow, tx + 1, ty + 1);

                // At max — draw label in green, otherwise white
                using var textBrush = new SolidBrush(isMax ? Color.FromArgb(100, 220, 100) : Color.White);
                g.DrawString(label, font, textBrush, tx, ty);
            }

            // For Letter/Gold Dust bottle used (CurrentCount == 2) — draw checkmark
            if (item.IsBottle && item.CurrentCount == 2)
            {
                using var checkFont = new Font("Arial", 9f, FontStyle.Bold);
                using var shadow = new SolidBrush(Color.Black);
                using var green  = new SolidBrush(Color.FromArgb(100, 220, 100));
                g.DrawString("✓", checkFont, shadow, rect.Width - 11, 1);
                g.DrawString("✓", checkFont, green,  rect.Width - 12, 0);
            }

            // Border
            bool isLit = item.AlwaysCollected ||
                (item.IsDungeonReward ? item.DungeonCleared :
                 item.IsAutoKey ? item.AutoKeyLit :
                 item.IsBottle ? item.CurrentCount > 0 :
                 item.CollectedWhenFull ? item.CurrentCount >= item.MaxCount :
                 item.PartialCollectedAt > 0 ? item.CurrentCount >= item.PartialCollectedAt :
                 item.CurrentCount > 0);
            Color borderColor = item.AlwaysCollected
                ? Color.FromArgb(200, 180, 50)
                : isLit ? Color.FromArgb(80, 200, 80) : Color.FromArgb(70, 70, 70);
            using var pen = new Pen(borderColor, item.AlwaysCollected ? 2 : 1);
            g.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);

            // For dungeon reward — draw dungeon icon in top left corner
            if (item.IsDungeonReward && item.IconPath != null)
            {
                // Dungeon label is now in block title — don't draw anything
            }
        }

        private void ItemBox_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is not PictureBox pb || pb.Tag is not TrackerItem item)
                return;

            // Block all clicks in broadcast mode
            if (IsBroadcast) return;

            if (item.IsDungeonReward)
            {
                if (e.Button == MouseButtons.Left)
                {
                    item.DungeonCleared = !item.DungeonCleared;
                    SyncGanonBk();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    // Skip rewards already assigned to other dungeons
                    var usedIndices = new HashSet<int>(
                        _globalItems.Values
                            .Where(x => x.IsDungeonReward && x.Id != item.Id && x.RewardIndex > 0)
                            .Select(x => x.RewardIndex));
                    int next = (item.RewardIndex + 1) % item.RewardIcons!.Length;
                    while (next != 0 && usedIndices.Contains(next))
                        next = (next + 1) % item.RewardIcons.Length;
                    item.RewardIndex = next;
                    SyncGanonBk();
                }

                RefreshIcon(pb, item);
                UpdateTooltip(pb, item);
                pb.Invalidate();
                ItemChanged?.Invoke();
                return;
            }

            // IsAutoKey — not clickable
            if (item.IsAutoKey) return;

            // Bottle handling
            if (item.IsBottle)
            {
                if (e.Button == MouseButtons.Left)
                {
                    // Letter/Gold Dust: cycle 0 → 1 → 2 → 0
                    if (item.BottleContent is "Ruto's Letter" or "Gold Dust")
                    {
                        item.CurrentCount = (item.CurrentCount + 1) % 3;
                    }
                    else
                    {
                        // Regular bottle: toggle collected
                        item.CurrentCount = item.CurrentCount > 0 ? 0 : 1;
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    // Right click: cycle through content options
                    if (item.BottleContentNames != null)
                    {
                        int idx = Array.IndexOf(item.BottleContentNames, item.BottleContent);
                        idx = (idx + 1) % item.BottleContentNames.Length;
                        item.BottleContent = item.BottleContentNames[idx];
                        if (item.BottleContentIcons != null)
                            item.IconPath = item.BottleContentIcons[idx];
                        item.MaxCount = (item.BottleContent is "Ruto's Letter" or "Gold Dust") ? 2 : 1;
                        item.CurrentCount = 0;
                    }
                }
                RefreshIcon(pb, item);
                UpdateTooltip(pb, item);
                pb.Invalidate();
                ItemChanged?.Invoke();
                return;
            }

            // In bombBag mode Bombchu is not clickable
            if (item.StepNames?.Length == 1 && item.StepNames[0].Contains("bomb_bag"))
                return;

            if (e.Button == MouseButtons.Left)
                item.Increment();
            else if (e.Button == MouseButtons.Right)
                item.Decrement();

            RefreshIcon(pb, item);
            UpdateTooltip(pb, item);
            pb.Invalidate();

            SyncBombchu(item);
            // If collectible changed — recalculate Ganon BK
            if (item.Type == TrackerItemType.Mask
                || item.Id is "coin_red" or "coin_green" or "coin_blue" or "coin_yellow" or "triforce"
                or "gold_skulltula_tokens" or "swamp_skulltula_tokens" or "ocean_skulltula_tokens"
                or "sf_woodfall" or "sf_snowhead" or "sf_great_bay" or "sf_stone_tower" or "sf_clock_town"
                or "transcendent_fairy" or "platinum_token" or "platinum_token_oot" or "platinum_token_mm"
                || item.Id.StartsWith("reward_"))
                SyncGanonBk();
            ItemChanged?.Invoke();
        }

        private static string GetBottleLabelFromContent(string content) => content switch
        {
            "Empty"            => "",
            "Ruto's Letter"    => "Letter",
            "Gold Dust"        => "G.Dust",
            "Milk"             => "Milk",
            "Chateau Romani"   => "Chateau",
            "Red Potion"       => "Red",
            "Green Potion"     => "Green",
            "Blue Potion"      => "Blue",
            "Poe"              => "Poe",
            "Big Poe"          => "Big Poe",
            "Blue Fire"        => "B.Fire",
            "Fairy"            => "Fairy",
            "Fish"             => "Fish",
            "Bugs"             => "Bugs",
            "Zora Egg"         => "Egg",
            "Seahorse"         => "Horse",
            "Deku Princess"    => "Princess",
            "Magic Mushroom"   => "Mushroom",
            "Spring Water"     => "Spring",
            "Hot Spring Water" => "Hot",
            _ => content.Length > 6 ? content.Substring(0, 6) : content,
        };

        // Synchronizes Bombchu with bombs in bombBag mode (global search)
        private static void SyncBombchu(TrackerItem changedItem)        {
            foreach (var kv in _globalItems)
            {
                var bombchu = kv.Value;
                if (bombchu.StepNames?.Length == 1 && bombchu.StepNames[0] == changedItem.Id)
                {
                    bombchu.CurrentCount = changedItem.IsCollected ? 1 : 0;
                    if (_globalPbs.TryGetValue(bombchu.Id, out var pb))
                        pb.Invalidate();
                }
            }
        }

        // Recalculates Ganon BK custom based on all Special Conditions
        private static void SyncGanonBk()
        {
            if (!_globalItems.TryGetValue("ganons_castle_bk", out var bk)) return;
            if (!bk.IsAutoKey) return;
            var cond = _globalCfg?.GanonBk;
            if (cond == null) return;

            int score = 0;

            // ── Dungeons (stones/medallions/remains) ──────────────────────────────
            // AllRewardIcons indices: 1-3=stones, 4-9=medallions, 10-13=remains
            if (cond.Stones || cond.Medallions || cond.Remains)
            {
                // Standard mode: dungeon blocks with reward selection
                foreach (var kv in _globalItems)
                {
                    var item = kv.Value;
                    if (!item.IsDungeonReward || !item.DungeonCleared || item.RewardIndex == 0) continue;
                    bool isStone     = item.RewardIndex >= 1 && item.RewardIndex <= 3;
                    bool isMedallion = item.RewardIndex >= 4 && item.RewardIndex <= 9;
                    bool isRemains   = item.RewardIndex >= 10 && item.RewardIndex <= 13;
                    if ((cond.Stones && isStone) || (cond.Medallions && isMedallion) || (cond.Remains && isRemains))
                        score++;
                }
                // Rewards anywhere mode: standalone reward items
                if (cond.Stones)
                {
                    if (_globalItems.TryGetValue("reward_kokiris_emerald",  out var r) && r.IsCollected) score++;
                    if (_globalItems.TryGetValue("reward_gorons_ruby",      out r)     && r.IsCollected) score++;
                    if (_globalItems.TryGetValue("reward_zoras_sapphire",   out r)     && r.IsCollected) score++;
                }
                if (cond.Medallions)
                {
                    if (_globalItems.TryGetValue("reward_light_medallion",  out var r) && r.IsCollected) score++;
                    if (_globalItems.TryGetValue("reward_forest_medallion", out r)     && r.IsCollected) score++;
                    if (_globalItems.TryGetValue("reward_fire_medallion",   out r)     && r.IsCollected) score++;
                    if (_globalItems.TryGetValue("reward_water_medallion",  out r)     && r.IsCollected) score++;
                    if (_globalItems.TryGetValue("reward_shadow_medallion", out r)     && r.IsCollected) score++;
                    if (_globalItems.TryGetValue("reward_spirit_medallion", out r)     && r.IsCollected) score++;
                }
                if (cond.Remains)
                {
                    if (_globalItems.TryGetValue("reward_odolwas_remains",   out var r) && r.IsCollected) score++;
                    if (_globalItems.TryGetValue("reward_gohts_remains",     out r)     && r.IsCollected) score++;
                    if (_globalItems.TryGetValue("reward_gyorgs_remains",    out r)     && r.IsCollected) score++;
                    if (_globalItems.TryGetValue("reward_twinmolds_remains", out r)     && r.IsCollected) score++;
                }
            }

            // ── Tokens ────────────────────────────────────────────────────────
            // Platinum OoT replaces Gold (100), platinum MM replaces Swamp+Ocean (60)
            bool platOot = _globalCfg?.PlatinumTokenOot == true || _globalCfg?.SharedPlatinumToken == true;
            bool platMm  = _globalCfg?.PlatinumTokenMm  == true || _globalCfg?.SharedPlatinumToken == true;

            if (cond.SkullsGold)
            {
                if (platOot && _globalItems.TryGetValue("platinum_token", out var pt) && pt.IsCollected)
                    score += 100;
                else if (platOot && _globalItems.TryGetValue("platinum_token_oot", out var ptOot) && ptOot.IsCollected)
                    score += 100;
                else if (_globalItems.TryGetValue("gold_skulltula_tokens", out var gst))
                    score += gst.CurrentCount;
            }
            if (cond.SkullsSwamp || cond.SkullsOcean)
            {
                if (platMm && _globalItems.TryGetValue("platinum_token", out var pt) && pt.IsCollected)
                    score += 60;
                else if (platMm && _globalItems.TryGetValue("platinum_token_mm", out var ptMm) && ptMm.IsCollected)
                    score += 60;
                else
                {
                    if (cond.SkullsSwamp && _globalItems.TryGetValue("swamp_skulltula_tokens", out var sst))
                        score += sst.CurrentCount;
                    if (cond.SkullsOcean && _globalItems.TryGetValue("ocean_skulltula_tokens", out var ost))
                        score += ost.CurrentCount;
                }
            }

            // ── Fairies ───────────────────────────────────────────────────────────
            // Transcendent Fairy = all 61 (15+15+15+15+1), others are ignored
            bool anyFairy = cond.FairiesWF || cond.FairiesSH || cond.FairiesGB || cond.FairiesST || cond.FairyTown;
            if (anyFairy)
            {
                bool transcendent = _globalCfg?.TranscendentFairy == true
                    && _globalItems.TryGetValue("transcendent_fairy", out var tfairy) && tfairy.IsCollected;
                if (transcendent)
                    score += 61;
                else
                {
                    if (cond.FairiesWF && _globalItems.TryGetValue("sf_woodfall",    out var wf)) score += wf.CurrentCount;
                    if (cond.FairiesSH && _globalItems.TryGetValue("sf_snowhead",    out var sh)) score += sh.CurrentCount;
                    if (cond.FairiesGB && _globalItems.TryGetValue("sf_great_bay",   out var gb)) score += gb.CurrentCount;
                    if (cond.FairiesST && _globalItems.TryGetValue("sf_stone_tower", out var st)) score += st.CurrentCount;
                    if (cond.FairyTown && _globalItems.TryGetValue("sf_clock_town",  out var ct) && ct.IsCollected) score += 1;
                }
            }

            // ── Masks ─────────────────────────────────────────────────────────            // Shared mask counts as 1 (not 2)
            if (cond.MasksRegular || cond.MasksTransform || cond.MasksOot)
            {
                var counted = new HashSet<string>();
                foreach (var kv in _globalItems)
                {
                    var item = kv.Value;
                    if (item.Type != TrackerItemType.Mask || !item.IsCollected) continue;
                    // Normalize Id: remove oot_/mm_ prefix for shared masks
                    string baseId = item.Id.Replace("oot_", "").Replace("mm_", "");
                    if (counted.Contains(baseId)) continue;
                    counted.Add(baseId);

                    bool isTransform = baseId is "deku_mask" or "goron_mask" or "zora_mask" or "fierce_deity";
                    bool isOot       = item.Id.StartsWith("oot_") && !isTransform;
                    bool isRegular   = !isTransform && !isOot;

                    if ((cond.MasksTransform && isTransform) ||
                        (cond.MasksOot       && isOot)       ||
                        (cond.MasksRegular   && isRegular))
                        score++;
                }
            }

            // ── Coins ────────────────────────────────────────────────────────
            if (cond.CoinsRed    && _globalItems.TryGetValue("coin_red",    out var cr)) score += cr.CurrentCount;
            if (cond.CoinsGreen  && _globalItems.TryGetValue("coin_green",  out var cg)) score += cg.CurrentCount;
            if (cond.CoinsBlue   && _globalItems.TryGetValue("coin_blue",   out var cb)) score += cb.CurrentCount;
            if (cond.CoinsYellow && _globalItems.TryGetValue("coin_yellow", out var cy)) score += cy.CurrentCount;
            // Triforce hunt
            if (cond.Triforce && _globalCfg?.TriforceMode == "hunt"
                && _globalItems.TryGetValue("triforce", out var tfItem)) score += tfItem.CurrentCount;

            // Calculate maximum (sum of all enabled items)
            int maxScore = 0;
            if (cond.Stones)     maxScore += 3;
            if (cond.Medallions) maxScore += 6;
            if (cond.Remains)    maxScore += 4;
            if (cond.SkullsGold)
            {
                bool platOotMax = _globalCfg?.PlatinumTokenOot == true || _globalCfg?.SharedPlatinumToken == true;
                maxScore += platOotMax ? 1 : (_globalCfg?.GoldSkulltulas == true ? 100 : 0);
            }
            if (cond.SkullsSwamp || cond.SkullsOcean)
            {
                bool platMmMax = _globalCfg?.PlatinumTokenMm == true || _globalCfg?.SharedPlatinumToken == true;
                if (platMmMax) maxScore += 1;
                else { if (cond.SkullsSwamp) maxScore += 30; if (cond.SkullsOcean) maxScore += 30; }
            }
            if (cond.FairiesWF || cond.FairiesSH || cond.FairiesGB || cond.FairiesST || cond.FairyTown)
            {
                bool transMax = _globalCfg?.TranscendentFairy == true;
                if (transMax) maxScore += 61;
                else { if (cond.FairiesWF) maxScore += 15; if (cond.FairiesSH) maxScore += 15; if (cond.FairiesGB) maxScore += 15; if (cond.FairiesST) maxScore += 15; if (cond.FairyTown) maxScore += 1; }
            }
            // Masks — count how many are in tracker
            if (cond.MasksRegular || cond.MasksTransform || cond.MasksOot)
            {
                foreach (var kv in _globalItems)
                {
                    var item2 = kv.Value;
                    if (item2.Type != TrackerItemType.Mask) continue;
                    string baseId = item2.Id.Replace("oot_", "").Replace("mm_", "");
                    bool isTransform2 = baseId is "deku_mask" or "goron_mask" or "zora_mask" or "fierce_deity";
                    bool isOot2       = item2.Id.StartsWith("oot_") && !isTransform2;
                    bool isRegular2   = !isTransform2 && !isOot2;
                    if ((cond.MasksTransform && isTransform2) || (cond.MasksOot && isOot2) || (cond.MasksRegular && isRegular2))
                        maxScore++;
                }
            }
            if (cond.CoinsRed    && _globalCfg != null) maxScore += _globalCfg.CoinsRedMax;
            if (cond.CoinsGreen  && _globalCfg != null) maxScore += _globalCfg.CoinsGreenMax;
            if (cond.CoinsBlue   && _globalCfg != null) maxScore += _globalCfg.CoinsBlueMax;
            if (cond.CoinsYellow && _globalCfg != null) maxScore += _globalCfg.CoinsYellowMax;
            if (cond.Triforce    && _globalCfg?.TriforceMode == "hunt") maxScore += _globalCfg.TriforceHuntGoal;

            // Update MaxCount and CurrentCount
            // If GanonBkRequired is 0 (nothing checked), key is always lit
            if (_globalCfg?.GanonBkRequired == 0)
            {
                bk.MaxCount = 1;
                bk.CurrentCount = 1;
                bk.AutoKeyThreshold = 0;
            }
            else
            {
                bk.MaxCount = maxScore > 0 ? maxScore : bk.AutoKeyThreshold > 0 ? bk.AutoKeyThreshold : 1;
                bk.CurrentCount = score;
            }
            if (_globalPbs.TryGetValue("ganons_castle_bk", out var bkPb))
                bkPb.Invalidate();
        }

        private Color GetBaseColor(TrackerItem item) => item.Type switch
        {
            TrackerItemType.Equipment => Color.FromArgb(60, 70, 100),
            TrackerItemType.Mask      => Color.FromArgb(80, 55, 90),
            TrackerItemType.Song      => Color.FromArgb(50, 80, 60),
            TrackerItemType.Item      => Color.FromArgb(70, 90, 60),
            TrackerItemType.Dungeon   => Color.FromArgb(90, 55, 55),
            _                         => Color.FromArgb(60, 60, 60)
        };

        public void LockStartingItems(Dictionary<string, int> startingProgress)
        {
            foreach (var item in _items)
            {
                if (startingProgress.TryGetValue(item.Id, out var startVal) && startVal > 0)
                {
                    // Set MinCount = starting value
                    item.MinCount = Math.Max(item.MinCount, startVal);
                    // Ensure CurrentCount is not below MinCount
                    if (item.CurrentCount < item.MinCount)
                        item.CurrentCount = item.MinCount;
                }
            }
            foreach (Control c in this.Controls)
                if (c is PictureBox pb && pb.Tag is TrackerItem item2)
                {
                    RefreshIcon(pb, item2);
                    pb.Invalidate();
                }
        }

        public void ResetAll()
        {
            foreach (var item in _items)
                item.Reset(); // Reset resets to MinCount (starting items are preserved)
            foreach (Control c in this.Controls)
                if (c is PictureBox pb) pb.Invalidate();
        }
        
        public void SaveProgress(Dictionary<string, int> progress)
        {
            foreach (var item in _items)
            {
                if (item.IsDungeonReward)
                    progress[item.Id] = (item.DungeonCleared ? 100 : 0) + item.RewardIndex;
                else if (item.IsBottle)
                {
                    // Encode: contentIndex * 10 + currentCount
                    int contentIdx = item.BottleContentNames != null
                        ? Array.IndexOf(item.BottleContentNames, item.BottleContent)
                        : 0;
                    if (contentIdx < 0) contentIdx = item.BottleContentNames!.Length - 1; // empty
                    progress[item.Id] = contentIdx * 10 + item.CurrentCount;
                }
                else
                    progress[item.Id] = item.CurrentCount;
            }
        }
        
        public void LoadProgress(Dictionary<string, int> progress)
        {
            foreach (var item in _items)
            {
                if (progress.TryGetValue(item.Id, out var val))
                {
                    if (item.IsDungeonReward)
                    {
                        item.DungeonCleared = val >= 100;
                        item.RewardIndex = val % 100;
                    }
                    else if (item.IsBottle)
                    {
                        int contentIdx = val / 10;
                        int count = val % 10;
                        if (item.BottleContentNames != null && contentIdx < item.BottleContentNames.Length)
                        {
                            item.BottleContent = item.BottleContentNames[contentIdx];
                            if (item.BottleContentIcons != null)
                                item.IconPath = item.BottleContentIcons[contentIdx];
                            item.MaxCount = (item.BottleContent is "Ruto's Letter" or "Gold Dust") ? 2 : 1;
                        }
                        item.CurrentCount = Math.Min(count, item.MaxCount);
                    }
                    else
                        item.CurrentCount = Math.Min(val, item.MaxCount);
                }
            }
            foreach (Control c in this.Controls)
                if (c is PictureBox pb && pb.Tag is TrackerItem item2)
                {
                    RefreshIcon(pb, item2);
                    pb.Invalidate();
                }
            if (!IsBroadcast)
                SyncGanonBk();
        }
    }
}
