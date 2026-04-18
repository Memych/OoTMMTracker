using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OoTMMTracker.Controls;
using OoTMMTracker.Models;
using OoTMMTracker.Services;

namespace OoTMMTracker.Forms
{
    public class BroadcastForm : Form
    {
        private Panel _scrollPanel = null!;
        private readonly ToolTip _toolTip = new();
        private readonly List<ItemTrackerPanel> _panels = new();
        private TrackerConfig? _lastCfg;
        private Dictionary<string, int>? _lastProgress;
        private SpoilerLog? _lastSpoilerLog;
        private System.Windows.Forms.Timer? _resizeTimer;
        private int _broadcastItemSize = 48;
        private Label _lblZoom = null!;

        public BroadcastForm()
        {
            this.Text = "OoTMM Tracker — Broadcast";
            this.Size = new Size(1400, 900);
            this.MinimumSize = new Size(400, 300);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.StartPosition = FormStartPosition.Manual;

            // Top bar with zoom controls
            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(45, 45, 45)
            };

            var btnZoomOut = new Button
            {
                Text = "−",
                Location = new Point(6, 4),
                Size = new Size(26, 24),
                BackColor = Color.FromArgb(50, 50, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 11, FontStyle.Bold)
            };
            btnZoomOut.Click += (s, e) =>
            {
                var steps = ItemTrackerPanel.GetItemSizeSteps();
                int cur = Array.IndexOf(steps, _broadcastItemSize);
                if (cur > 0)
                {
                    _broadcastItemSize = steps[cur - 1];
                    UpdateZoomLabel();
                    RebuildWithOwnSize();
                }
            };
            topBar.Controls.Add(btnZoomOut);

            _lblZoom = new Label
            {
                Location = new Point(36, 7),
                Size = new Size(50, 18),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8f)
            };
            UpdateZoomLabel();
            topBar.Controls.Add(_lblZoom);

            var btnZoomIn = new Button
            {
                Text = "+",
                Location = new Point(90, 4),
                Size = new Size(26, 24),
                BackColor = Color.FromArgb(50, 50, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 11, FontStyle.Bold)
            };
            btnZoomIn.Click += (s, e) =>
            {
                var steps = ItemTrackerPanel.GetItemSizeSteps();
                int cur = Array.IndexOf(steps, _broadcastItemSize);
                if (cur < steps.Length - 1)
                {
                    _broadcastItemSize = steps[cur + 1];
                    UpdateZoomLabel();
                    RebuildWithOwnSize();
                }
            };
            topBar.Controls.Add(btnZoomIn);

            var btnZoomReset = new Button
            {
                Text = "↺",
                Location = new Point(120, 4),
                Size = new Size(26, 24),
                BackColor = Color.FromArgb(50, 50, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 11, FontStyle.Bold)
            };
            btnZoomReset.Click += (s, e) =>
            {
                _broadcastItemSize = 48;
                UpdateZoomLabel();
                RebuildWithOwnSize();
            };
            topBar.Controls.Add(btnZoomReset);

            _scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(30, 30, 30)
            };
            this.Controls.Add(_scrollPanel);  // Fill — add first
            this.Controls.Add(topBar);        // Top — add second

            // Rearrange columns dynamically when window is resized (debounced)
            _resizeTimer = new System.Windows.Forms.Timer { Interval = 80 };
            _resizeTimer.Tick += (s, e) =>
            {
                _resizeTimer.Stop();
                if (_lastCfg != null && _lastProgress != null)
                    RebuildWithOwnSize();
            };

            this.Resize += (s, e) =>
            {
                _resizeTimer.Stop();
                _resizeTimer.Start();
            };
        }

        private void UpdateZoomLabel() => _lblZoom.Text = $"{_broadcastItemSize}px";

        /// Rebuild using broadcast's own item size.
        private void RebuildWithOwnSize()
        {
            if (_lastCfg == null || _lastProgress == null) return;
            Rebuild(_lastCfg, _lastProgress, _lastSpoilerLog);
        }

        public void Rebuild(TrackerConfig cfg, Dictionary<string, int> progress, SpoilerLog? spoilerLog)
        {
            _lastCfg = cfg;
            _lastProgress = progress;
            _lastSpoilerLog = spoilerLog;

            // Use broadcast's own item size
            int mainSize = ItemTrackerPanel.GetItemSize();
            ItemTrackerPanel.SetItemSize(_broadcastItemSize);

            _scrollPanel.Controls.Clear();
            _panels.Clear();
            ItemTrackerPanel.ClearGlobal();
            _toolTip.RemoveAll();
            ItemTrackerPanel.SetGlobalConfig(cfg);

            var allPanels = new List<ItemTrackerPanel>();

            bool hasShared = TrackerItemsData.GetSharedItems(cfg).Count > 0
                          || TrackerItemsData.GetSharedItems_Items(cfg).Count > 0
                          || TrackerItemsData.GetSharedBottles(cfg).Count > 0;

            void Add(string title, List<TrackerItem> items, int cols = 6)
            {
                if (items.Count == 0) return;
                int actualCols = Math.Min(cols, items.Count);
                var p = new ItemTrackerPanel(title, items, actualCols, _toolTip);
                p.IsBroadcast = true;
                DisableClicks(p);
                _panels.Add(p);
                allPanels.Add(p);
            }

            if (hasShared)
                Add("Equipment (Shared)", TrackerItemsData.GetSharedItems(cfg), 6);
            if (cfg.HasOot) Add("Equipment (OoT)", TrackerItemsData.GetOotEquipmentItems(cfg), 4);
            if (cfg.HasMm)  Add("Equipment (MM)",  TrackerItemsData.GetMmEquipmentItems(cfg), 5);
            Add("Masks (Shared)", TrackerItemsData.GetSharedMasks(cfg), 6);
            if (cfg.HasMm)  Add("Masks (MM)",  TrackerItemsData.GetMmMaskItems(cfg), 6);
            if (cfg.HasOot) Add("Masks (OoT)", TrackerItemsData.GetOotMaskItems(cfg), 4);
            if (hasShared)
                Add("Items (Shared)", TrackerItemsData.GetSharedItems_Items(cfg), 7);
            if (cfg.HasOot) Add("Items (OoT)", TrackerItemsData.GetOotItems(cfg), 6);
            if (cfg.HasMm)  Add("Items (MM)",  TrackerItemsData.GetMmItems(cfg), 5);
            if (hasShared)
                Add("Bottles (Shared)", TrackerItemsData.GetSharedBottles(cfg), 6);
            if (cfg.HasOot) Add("Bottles (OoT)", TrackerItemsData.GetOotBottles(cfg), 4);
            if (cfg.HasMm)  Add("Bottles (MM)",  TrackerItemsData.GetMmBottles(cfg), 6);
            if (cfg.HasOot) Add("Trade Quests (OoT)", TrackerItemsData.GetOotTradeItems(cfg), 5);
            if (cfg.HasMm)  Add("Trade Quests (MM)",  TrackerItemsData.GetMmTradeItems(), 9);
            Add("Skulltula Tokens", TrackerItemsData.GetSkulltulas(cfg), 10);
            if (cfg.HasMm) Add("Stray Fairies", TrackerItemsData.GetStrayFairies(cfg), 6);
            Add("Misc",             TrackerItemsData.GetMiscItems(cfg), 4);
            Add("Songs (Shared)", TrackerItemsData.GetSharedSongItems(cfg), 6);
            if (cfg.HasOot) Add("Songs (OoT)", TrackerItemsData.GetOotSongItems(cfg), 6);
            if (cfg.HasMm)  Add("Songs (MM)",  TrackerItemsData.GetMmSongItems(cfg), 5);
            if (cfg.HasMm) Add("Owl Statues", TrackerItemsData.GetOwlStatues(cfg), 5);
            if (cfg.HasMm) Add("Clocks", TrackerItemsData.GetClocks(cfg, spoilerLog), 6);
            if (cfg.HasMm) Add("Tingle Maps", TrackerItemsData.GetTingleMaps(cfg), 6);
            Add("Dungeon Rewards", TrackerItemsData.GetDungeonRewards(cfg), 9);
            foreach (var (id, name, label) in TrackerItemsData.DungeonList)
            {
                bool isMm = id is "woodfall" or "snowhead" or "great_bay" or "stone_tower";
                if (isMm && !cfg.HasMm) continue;
                if (!isMm && !cfg.HasOot) continue;
                var blockName = (!isMm && cfg.IsMq(id)) ? $"{name} (MQ)" : name;
                Add(blockName, TrackerItemsData.GetDungeonSingle(id, name, label, cfg), int.MaxValue);
            }
            foreach (var (id, name, label) in TrackerItemsData.SubDungeonList)
            {
                if (!cfg.HasOot) continue;
                var blockName = cfg.IsMq(id) ? $"{name} (MQ)" : name;
                Add(blockName, TrackerItemsData.GetDungeonSingle(id, name, label, cfg), int.MaxValue);
            }
            if (cfg.HasOot && cfg.SmallKeysHideout)
                Add("Thieves' Hideout", TrackerItemsData.GetDungeonSingle("thieves_hideout", "Thieves' Hideout", "dungeons/labels/thieves_hideout.png", cfg), int.MaxValue);
            if (cfg.HasOot && cfg.SmallKeysTcg)
                Add("Treasure Chest Game", TrackerItemsData.GetDungeonSingle("tcg", "Treasure Chest Game", "dungeons/labels/treasure_chest_game.png", cfg), int.MaxValue);
            Add("Dungeon Items", TrackerItemsData.GetDungeonSpecialItems(cfg), 4);
            Add("Boss Souls",        SoulsData.GetBossSouls(cfg),        8);
            Add("Enemy Souls",       SoulsData.GetEnemySoulsShared(cfg), 8);
            Add("Enemy Souls (OoT)", SoulsData.GetEnemySoulsOot(cfg),    8);
            Add("Enemy Souls (MM)",  SoulsData.GetEnemySoulsMm(cfg),     8);
            Add("NPC Souls",         SoulsData.GetNpcSoulsShared(cfg),   8);
            Add("NPC Souls (OoT)",   SoulsData.GetNpcSoulsOot(cfg),      8);
            Add("NPC Souls (MM)",    SoulsData.GetNpcSoulsMm(cfg),       8);
            Add("Animal Souls",      SoulsData.GetAnimalSoulsShared(cfg),4);
            Add("Animal Souls (OoT)",SoulsData.GetAnimalSoulsOot(cfg),   4);
            Add("Animal Souls (MM)", SoulsData.GetAnimalSoulsMm(cfg),    4);
            Add("Misc Souls",        SoulsData.GetMiscSoulsShared(cfg),  4);
            Add("Misc Souls (OoT)",  SoulsData.GetMiscSoulsOot(cfg),     4);
            Add("Misc Souls (MM)",   SoulsData.GetMiscSoulsMm(cfg),      4);

            ArrangeInColumns(allPanels);

            // Load progress AFTER panels are added to container
            foreach (var panel in _panels)
                panel.LoadProgress(progress);

            // Restore main tracker item size
            ItemTrackerPanel.SetItemSize(mainSize);

            this.Refresh();
        }

        private void ArrangeInColumns(List<ItemTrackerPanel> panels)
        {
            if (panels.Count == 0) return;

            int gap = 4;
            int availWidth = _scrollPanel.ClientSize.Width;

            // Find the widest panel to use as column width
            int colWidth = panels.Max(p => p.Width) + gap;
            int colCount = Math.Max(1, availWidth / colWidth);

            // Calculate total height and target height per column
            int totalHeight = panels.Sum(p => p.Height + gap);
            int targetColHeight = totalHeight / colCount;

            // Fill columns sequentially top-to-bottom
            var colPanels = new List<List<ItemTrackerPanel>>();
            for (int i = 0; i < colCount; i++)
                colPanels.Add(new List<ItemTrackerPanel>());

            int currentCol = 0;
            int currentColHeight = 0;
            foreach (var panel in panels)
            {
                if (currentCol < colCount - 1 && currentColHeight >= targetColHeight)
                {
                    currentCol++;
                    currentColHeight = 0;
                }
                colPanels[currentCol].Add(panel);
                currentColHeight += panel.Height + gap;
            }

            int maxColHeight = colPanels.Max(col => col.Sum(p => p.Height + gap));
            var container = new Panel
            {
                Size = new Size(colCount * colWidth, maxColHeight + gap),
                BackColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 0)
            };

            for (int col = 0; col < colCount; col++)
            {
                int x = col * colWidth + gap;
                int y = gap;
                foreach (var panel in colPanels[col])
                {
                    panel.Location = new Point(x, y);
                    container.Controls.Add(panel);
                    y += panel.Height + gap;
                }
            }

            _scrollPanel.Controls.Add(container);
        }

        public void UpdateProgress(Dictionary<string, int> progress)
        {
            foreach (var panel in _panels)
                panel.LoadProgress(progress);
        }
        private static void DisableClicks(Control control)
        {
            control.Cursor = Cursors.Default;
            foreach (Control child in control.Controls)
            {
                child.Cursor = Cursors.Default;
                child.MouseDown += (s, e) => { /* block */ };
                DisableClicks(child);
            }
        }
    }
}
