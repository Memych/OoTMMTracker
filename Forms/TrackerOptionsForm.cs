using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OoTMMTracker.Models;

namespace OoTMMTracker.Forms
{
    public class TrackerOptionsForm : Form
    {
        public TrackerConfig Config { get; private set; }

        private CheckBox _chkOotExtraSwords    = null!;
        private CheckBox _chkOotSpinUpgrade    = null!;
        private CheckBox _chkOotBronzeScale    = null!;
        private CheckBox _chkOotPreplantedBeans = null!;
        // Game mode
        private CheckBox _chkGamesOot  = null!;
        private CheckBox _chkGamesMm   = null!;
        // Tabs — for blocking in single game mode
        private TabPage? _tabOot;
        private TabPage? _tabMm;
        private TabPage? _tabSharedEq;
        private TabPage? _tabSharedMask;
        private TabPage? _tabSongs;
        private CheckBox _chkMmStrength        = null!;
        private CheckBox _chkMmScales          = null!;
        private CheckBox _chkMmStoneAgony      = null!;
        private CheckBox _chkMmGoronTunic      = null!;
        private CheckBox _chkMmZoraTunic       = null!;
        private CheckBox _chkMmIronBoots       = null!;
        private CheckBox _chkMmHoverBoots      = null!;
        private CheckBox _chkMmDekuShield      = null!;
        private CheckBox _chkMmSticksNuts     = null!;
        private CheckBox _chkMmShortHookshot  = null!;
        private CheckBox _chkMmHammer         = null!;
        private CheckBox _chkMmSpellFire      = null!;
        private CheckBox _chkMmSpellWind      = null!;
        private CheckBox _chkMmSpellLove      = null!;
        // Songs
        private CheckBox _chkMmSunSong        = null!;
        private CheckBox _chkMmFairyOcarina   = null!;
        private CheckBox _chkOotElegy         = null!;
        private CheckBox _chkProgressiveGoron = null!;
        private CheckBox _chkFreeScarecrowOot = null!;
        private CheckBox _chkFreeScarecrowMm  = null!;
        private CheckBox _chkSharedSongEpona  = null!;
        private CheckBox _chkSharedSongStorms = null!;
        private CheckBox _chkSharedSongTime   = null!;
        private CheckBox _chkSharedSongSun    = null!;
        private CheckBox _chkSharedSongElegy  = null!;
        private CheckBox _chkSharedScarecrow  = null!;
        private CheckBox _chkOotOcarinaButtons    = null!;
        private CheckBox _chkMmOcarinaButtons     = null!;
        private CheckBox _chkSharedOcarinaButtons = null!;
        private CheckBox _chkSongsAsNotes         = null!;
        // Bombchu modes (CheckBox with mutual exclusion; nothing checked = toggle)
        private CheckBox _chkBombchuOotBombBag = null!;
        private CheckBox _chkBombchuOotBag     = null!;
        private CheckBox _chkBombchuMmBombBag  = null!;
        private CheckBox _chkBombchuMmBag      = null!;
        private CheckBox _chkClocksEnabled    = null!;
        private CheckBox _chkOwlShuffle       = null!;
        private CheckBox _chkTingleMaps       = null!;
        private CheckBox _chkChildWallet       = null!;
        private CheckBox _chkColossalWallet    = null!;
        private CheckBox _chkBottomlessWallet  = null!;
        private CheckBox _chkSharedHealth      = null!;
        private CheckBox _chkSharedSwords      = null!;
        private CheckBox _chkSharedShields     = null!;
        private CheckBox _chkSharedStrength    = null!;
        private CheckBox _chkSharedScales      = null!;
        private CheckBox _chkSharedWallets     = null!;
        private CheckBox _chkSharedMagic       = null!;
        private CheckBox _chkSharedTunicGoron  = null!;
        private CheckBox _chkSharedTunicZora   = null!;
        private CheckBox _chkSharedBootsIron   = null!;
        private CheckBox _chkSharedBootsHover  = null!;
        private CheckBox _chkSharedHookshot    = null!;
        private CheckBox _chkSharedBows        = null!;
        private CheckBox _chkSharedBombBags    = null!;
        private CheckBox _chkSharedOcarina     = null!;
        private CheckBox _chkSharedHammer      = null!;
        private CheckBox _chkSharedBottles     = null!;
        private CheckBox _chkSharedNutsSticks  = null!;
        private CheckBox _chkSharedBombchu     = null!;
        private CheckBox _chkSharedFireArrows  = null!;
        private CheckBox _chkSharedIceArrows   = null!;
        private CheckBox _chkSharedLightArrows = null!;
        private CheckBox _chkSharedLens        = null!;
        private CheckBox _chkSharedSpellFire   = null!;
        private CheckBox _chkSharedSpellWind   = null!;
        private CheckBox _chkSharedSpellLove   = null!;
        private CheckBox _chkSharedStoneAgony  = null!;
        private CheckBox _chkSharedSpinUpgrade = null!;
        private CheckBox _chkOotBlastMask      = null!;
        private CheckBox _chkOotStoneMask      = null!;
        private CheckBox _chkSharedMaskGoron   = null!;
        private CheckBox _chkSharedMaskZora    = null!;
        private CheckBox _chkSharedMaskBunny   = null!;
        private CheckBox _chkSharedMaskKeaton  = null!;
        private CheckBox _chkSharedMaskTruth   = null!;
        private CheckBox _chkSharedMaskBlast   = null!;
        private CheckBox _chkSharedMaskStone   = null!;

        // Collectible items
        private CheckBox _chkGoldSkulltulas    = null!;
        private CheckBox _chkMmSkulltulas      = null!;
        private CheckBox _chkPlatinumTokenOot  = null!;
        private CheckBox _chkPlatinumTokenMm   = null!;
        private CheckBox _chkSharedPlatinum    = null!;
        // Stray Fairies
        private CheckBox _chkStrayFairiesDungeons = null!;
        private CheckBox _chkStrayFairyTown       = null!;
        private CheckBox _chkTranscendentFairy    = null!;
        // Coins
        private CheckBox _chkCoinsRed    = null!;
        private CheckBox _chkCoinsGreen  = null!;
        private CheckBox _chkCoinsBlue   = null!;
        private CheckBox _chkCoinsYellow = null!;
        private NumericUpDown _numCoinsRed    = null!;
        private NumericUpDown _numCoinsGreen  = null!;
        private NumericUpDown _numCoinsBlue   = null!;
        private NumericUpDown _numCoinsYellow = null!;
        // Triforce
        private CheckBox _chkTriforceQuest = null!;
        private CheckBox _chkTriforceHunt  = null!;
        private NumericUpDown _numTriforceHuntGoal = null!;
        // Trade Quests OoT
        private CheckBox _chkOotSkipZelda     = null!;
        private CheckBox _chkOotOpenKakariko  = null!;
        private CheckBox _chkOotEggShuffle    = null!;
        // Dungeon options
        private CheckBox _chkMapsCompasses    = null!;
        private CheckBox _chkSmallKeysOot     = null!;
        private CheckBox _chkSmallKeysMm      = null!;
        private CheckBox _chkBossKeysOot      = null!;
        private CheckBox _chkBossKeysMm       = null!;
        private CheckBox _chkKeysanity        = null!;
        private CheckBox _chkSmallKeysHideout = null!;
        private CheckBox _chkSmallKeysTcg     = null!;
        private CheckBox _chkSilverRupees     = null!;
        private CheckBox _chkSrPouchAll       = null!;
        private CheckBox _chkSkeletonKeyOot   = null!;
        private CheckBox _chkSkeletonKeyMm    = null!;
        private CheckBox _chkSharedSkeletonKey = null!;
        private CheckBox _chkMagicalRupee     = null!;
        private readonly Dictionary<string, CheckBox> _chkSrPouchPerPack = new();
        // Master Quest
        private CheckBox _chkMqAll            = null!;
        private readonly Dictionary<string, CheckBox> _chkMqPerDungeon = new();
        private CheckBox _chkKeyRingsOot      = null!;
        private CheckBox _chkKeyRingsMm       = null!;
        // Separate checkboxes for each dungeon with key ring
        private readonly Dictionary<string, CheckBox> _chkKeyRingPerDungeon = new();
        private CheckBox _chkGanonBkAnywhere = null!;
        private CheckBox _chkGanonBkCustom   = null!;
        // Souls
        private CheckBox _chkSoulsBossOot = null!, _chkSoulsBossMm = null!;
        private CheckBox _chkSoulsEnemyOot = null!, _chkSoulsEnemyMm = null!, _chkSharedSoulsEnemy = null!;
        private CheckBox _chkSoulsNpcOot   = null!, _chkSoulsNpcMm   = null!, _chkSharedSoulsNpc   = null!;
        private CheckBox _chkSoulsAnimalOot= null!, _chkSoulsAnimalMm= null!, _chkSharedSoulsAnimal= null!;
        private CheckBox _chkSoulsMiscOot  = null!, _chkSoulsMiscMm  = null!, _chkSharedSoulsMisc  = null!;
        // Special Conditions
        private CheckBox _scStones = null!, _scMedallions = null!, _scRemains = null!;
        private CheckBox _scSkullsGold = null!, _scSkullsSwamp = null!, _scSkullsOcean = null!;
        private CheckBox _scFairiesWF = null!, _scFairiesSH = null!, _scFairiesGB = null!, _scFairiesST = null!, _scFairyTown = null!;
        private CheckBox _scMasksRegular = null!, _scMasksTransform = null!, _scMasksOot = null!;
        private CheckBox _scCoinsRed = null!, _scCoinsGreen = null!, _scCoinsBlue = null!, _scCoinsYellow = null!;
        private CheckBox _scTriforce = null!;
        private NumericUpDown _numScAmount = null!;

        public TrackerOptionsForm(TrackerConfig current)
        {
            Config = Copy(current);
            InitializeComponent();
        }

        private static TrackerConfig Copy(TrackerConfig c) => new()
        {
            Games = c.Games,
            OotExtraChildSwords = c.OotExtraChildSwords, OotSpinUpgrade = c.OotSpinUpgrade,            OotBronzeScale = c.OotBronzeScale, OotPreplantedBeans = c.OotPreplantedBeans, MmStrength = c.MmStrength, MmScales = c.MmScales,
            MmStoneAgony = c.MmStoneAgony, MmGoronTunic = c.MmGoronTunic, MmZoraTunic = c.MmZoraTunic,
            MmIronBoots = c.MmIronBoots, MmHoverBoots = c.MmHoverBoots, MmDekuShield = c.MmDekuShield,
            MmSticksNuts = c.MmSticksNuts, MmShortHookshot = c.MmShortHookshot,
            MmHammer = c.MmHammer, MmFairyFountain = c.MmFairyFountain,
            MmSpellFire = c.MmSpellFire, MmSpellWind = c.MmSpellWind, MmSpellLove = c.MmSpellLove,
            ClocksEnabled = c.ClocksEnabled, OwlShuffle = c.OwlShuffle, TingleMaps = c.TingleMaps,
            MmSunSong = c.MmSunSong, MmFairyOcarina = c.MmFairyOcarina, OotElegy = c.OotElegy,
            ProgressiveGoronLullaby = c.ProgressiveGoronLullaby,
            FreeScarecrowOot = c.FreeScarecrowOot, FreeScarecrowMm = c.FreeScarecrowMm,
            SharedSongEpona = c.SharedSongEpona, SharedSongStorms = c.SharedSongStorms,
            SharedSongTime = c.SharedSongTime, SharedSongSun = c.SharedSongSun,
            SharedSongElegy = c.SharedSongElegy, SharedScarecrow = c.SharedScarecrow,
            OotOcarinaButtons = c.OotOcarinaButtons, MmOcarinaButtons = c.MmOcarinaButtons,
            SharedOcarinaButtons = c.SharedOcarinaButtons, SongsAsNotes = c.SongsAsNotes,
            ChildWallet = c.ChildWallet, ColossalWallet = c.ColossalWallet, BottomlessWallet = c.BottomlessWallet,
            SharedHealth = c.SharedHealth, SharedSwords = c.SharedSwords, SharedShields = c.SharedShields,
            SharedStrength = c.SharedStrength, SharedScales = c.SharedScales, SharedWallets = c.SharedWallets,
            SharedMagic = c.SharedMagic, SharedTunicGoron = c.SharedTunicGoron, SharedTunicZora = c.SharedTunicZora,
            SharedBootsIron = c.SharedBootsIron, SharedBootsHover = c.SharedBootsHover,
            SharedHookshot = c.SharedHookshot, SharedBows = c.SharedBows, SharedBombBags = c.SharedBombBags,
            SharedOcarina = c.SharedOcarina, SharedHammer = c.SharedHammer, SharedBottles = c.SharedBottles,
            SharedNutsSticks = c.SharedNutsSticks, SharedBombchu = c.SharedBombchu,
            SharedFireArrows = c.SharedFireArrows, SharedIceArrows = c.SharedIceArrows,
            SharedLightArrows = c.SharedLightArrows, SharedLens = c.SharedLens,
            SharedSpellFire = c.SharedSpellFire, SharedSpellWind = c.SharedSpellWind, SharedSpellLove = c.SharedSpellLove,
            SharedStoneAgony = c.SharedStoneAgony, SharedSpinUpgrade = c.SharedSpinUpgrade,
            OotBlastMask = c.OotBlastMask, OotStoneMask = c.OotStoneMask,
            SharedMaskGoron = c.SharedMaskGoron, SharedMaskZora = c.SharedMaskZora,
            SharedMaskBunny = c.SharedMaskBunny, SharedMaskKeaton = c.SharedMaskKeaton,
            SharedMaskTruth = c.SharedMaskTruth, SharedMaskBlast = c.SharedMaskBlast,
            SharedMaskStone = c.SharedMaskStone,
            GoldSkulltulas  = c.GoldSkulltulas,
            MmSkulltulas    = c.MmSkulltulas,
            PlatinumTokenOot   = c.PlatinumTokenOot,
            PlatinumTokenMm    = c.PlatinumTokenMm,
            SharedPlatinumToken = c.SharedPlatinumToken,
            StrayFairiesDungeons = c.StrayFairiesDungeons,
            StrayFairyTown       = c.StrayFairyTown,
            TranscendentFairy    = c.TranscendentFairy,
            CoinsRed    = c.CoinsRed,    CoinsRedMax    = c.CoinsRedMax,
            CoinsGreen  = c.CoinsGreen,  CoinsGreenMax  = c.CoinsGreenMax,
            CoinsBlue   = c.CoinsBlue,   CoinsBlueMax   = c.CoinsBlueMax,
            CoinsYellow = c.CoinsYellow, CoinsYellowMax = c.CoinsYellowMax,
            TriforceMode     = c.TriforceMode,
            TriforceHuntGoal = c.TriforceHuntGoal,
            OotSkipZelda    = c.OotSkipZelda,
            OotOpenKakariko = c.OotOpenKakariko,
            OotEggShuffle   = c.OotEggShuffle,
            BombchuBehaviorOot = c.BombchuBehaviorOot,
            BombchuBehaviorMm  = c.BombchuBehaviorMm,
            MapsCompasses   = c.MapsCompasses,
            SmallKeysOot    = c.SmallKeysOot,
            SmallKeysMm     = c.SmallKeysMm,
            BossKeysOot     = c.BossKeysOot,
            BossKeysMm      = c.BossKeysMm,
            Keysanity       = c.Keysanity,
            SmallKeysHideout       = c.SmallKeysHideout,
            SmallKeysTcg           = c.SmallKeysTcg,
            SilverRupees           = c.SilverRupees,
            SkeletonKeyOot         = c.SkeletonKeyOot,
            SkeletonKeyMm          = c.SkeletonKeyMm,
            SharedSkeletonKey      = c.SharedSkeletonKey,
            MagicalRupee           = c.MagicalRupee,
            SoulsBossOot     = c.SoulsBossOot,    SoulsBossMm      = c.SoulsBossMm,
            SoulsEnemyOot    = c.SoulsEnemyOot,   SoulsEnemyMm     = c.SoulsEnemyMm,   SharedSoulsEnemy  = c.SharedSoulsEnemy,
            SoulsNpcOot      = c.SoulsNpcOot,     SoulsNpcMm       = c.SoulsNpcMm,     SharedSoulsNpc    = c.SharedSoulsNpc,
            SoulsAnimalOot   = c.SoulsAnimalOot,  SoulsAnimalMm    = c.SoulsAnimalMm,  SharedSoulsAnimal = c.SharedSoulsAnimal,
            SoulsMiscOot     = c.SoulsMiscOot,    SoulsMiscMm      = c.SoulsMiscMm,    SharedSoulsMisc   = c.SharedSoulsMisc,
            SrPouchPacks           = new List<string>(c.SrPouchPacks),
            MqDungeons             = new List<string>(c.MqDungeons),
            KeyRingsOot            = c.KeyRingsOot,
            KeyRingsMm             = c.KeyRingsMm,
            KeyRingDungeons        = new List<string>(c.KeyRingDungeons),
            GanonBossKey           = c.GanonBossKey,
            GanonBkRequired        = c.GanonBkRequired,
            GanonBk = new GanonBkConditions
            {
                Count          = c.GanonBk?.Count          ?? 0,
                Stones         = c.GanonBk?.Stones         ?? false,
                Medallions     = c.GanonBk?.Medallions     ?? false,
                Remains        = c.GanonBk?.Remains        ?? false,
                SkullsGold     = c.GanonBk?.SkullsGold     ?? false,
                SkullsSwamp    = c.GanonBk?.SkullsSwamp    ?? false,
                SkullsOcean    = c.GanonBk?.SkullsOcean    ?? false,
                FairiesWF      = c.GanonBk?.FairiesWF      ?? false,
                FairiesSH      = c.GanonBk?.FairiesSH      ?? false,
                FairiesGB      = c.GanonBk?.FairiesGB      ?? false,
                FairiesST      = c.GanonBk?.FairiesST      ?? false,
                FairyTown      = c.GanonBk?.FairyTown      ?? false,
                MasksRegular   = c.GanonBk?.MasksRegular   ?? false,
                MasksTransform = c.GanonBk?.MasksTransform ?? false,
                MasksOot       = c.GanonBk?.MasksOot       ?? false,
                CoinsRed       = c.GanonBk?.CoinsRed       ?? false,
                CoinsGreen     = c.GanonBk?.CoinsGreen     ?? false,
                CoinsBlue      = c.GanonBk?.CoinsBlue      ?? false,
                CoinsYellow    = c.GanonBk?.CoinsYellow    ?? false,
                Triforce       = c.GanonBk?.Triforce       ?? false,
            },
        };

        private void InitializeComponent()
        {
            this.Text = "Tracker Options";
            this.Size = new Size(850, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var tabs = new TabControl { Dock = DockStyle.Fill };
            this.Controls.Add(tabs);

            // ── Tab 1: OoT ──────────────────────────────────────────────────
            var tabOot = new TabPage("OoT");
            _tabOot = tabOot;
            var panelOot = MakeScrollPanel();
            tabOot.Controls.Add(panelOot);
            tabs.TabPages.Add(tabOot);

            int y = 8;
            AddSection(panelOot, "Extra items", ref y);
            _chkOotExtraSwords = AddCheckLeft(panelOot, "Razor Sword + Gilded Sword (extraChildSwordsOot)", Config.OotExtraChildSwords, ref y);
            _chkOotSpinUpgrade = AddCheckRight(panelOot, "Great Spin Attack (spinUpgradeOot)", Config.OotSpinUpgrade, y);
            NextRow(ref y);
            _chkOotBronzeScale = AddCheckLeft(panelOot, "Bronze Scale (bronzeScale)", Config.OotBronzeScale, ref y);
            _chkOotPreplantedBeans = AddCheckRight(panelOot, "Preplanted Beans (ootPreplantedBeans)", Config.OotPreplantedBeans, y);
            NextRow(ref y);

            y += 8;
            AddSection(panelOot, "Bombchu (OoT)", ref y);
            _chkBombchuOotBombBag = AddCheckLeft(panelOot, "Lit when bombs available (bombBag)", Config.BombchuBehaviorOot == "bombBag", ref y);
            _chkBombchuOotBag     = AddCheckRight(panelOot, "Separate bag 20/30/40 (bagSeparate)", Config.BombchuBehaviorOot == "bag", y);
            NextRow(ref y);
            // Mutual exclusion
            _chkBombchuOotBombBag.CheckedChanged += (s, e) => { if (_chkBombchuOotBombBag.Checked) _chkBombchuOotBag.Checked = false; };
            _chkBombchuOotBag.CheckedChanged     += (s, e) => { if (_chkBombchuOotBag.Checked)     _chkBombchuOotBombBag.Checked = false; };

            y += 8;
            AddSection(panelOot, "Extra masks", ref y);
            _chkOotBlastMask = AddCheckLeft(panelOot, "Blast Mask (blastMaskOot)", Config.OotBlastMask, ref y);
            _chkOotStoneMask = AddCheckRight(panelOot, "Stone Mask (stoneMaskOot)", Config.OotStoneMask, y);
            NextRow(ref y);
            // Subscribe to update SC max when extra masks change
            _chkOotBlastMask.CheckedChanged += (s, e) => UpdateScMax();
            _chkOotStoneMask.CheckedChanged += (s, e) => UpdateScMax();

            // ── Tab 2: MM ──────────────────────────────────────────────────
            var tabMm = new TabPage("MM");
            _tabMm = tabMm;
            var panelMm = MakeScrollPanel();
            tabMm.Controls.Add(panelMm);
            tabs.TabPages.Add(tabMm);

            y = 8;
            AddSection(panelMm, "Extra items", ref y);
            _chkMmStrength   = AddCheckLeft(panelMm, "Strength upgrades (strengthMm)", Config.MmStrength, ref y);
            _chkMmScales     = AddCheckRight(panelMm, "Scale upgrades (scalesMm)", Config.MmScales, y);
            NextRow(ref y);
            _chkMmStoneAgony = AddCheckLeft(panelMm, "Stone of Agony (stoneAgonyMm)", Config.MmStoneAgony, ref y);
            _chkMmGoronTunic = AddCheckRight(panelMm, "Goron Tunic (tunicGoronMm)", Config.MmGoronTunic, y);
            NextRow(ref y);
            _chkMmZoraTunic  = AddCheckLeft(panelMm, "Zora Tunic (tunicZoraMm)", Config.MmZoraTunic, ref y);
            _chkMmIronBoots  = AddCheckRight(panelMm, "Iron Boots (bootsIronMm)", Config.MmIronBoots, y);
            NextRow(ref y);
            _chkMmHoverBoots = AddCheckLeft(panelMm, "Hover Boots (bootsHoverMm)", Config.MmHoverBoots, ref y);
            _chkMmDekuShield = AddCheckRight(panelMm, "Deku Shield (dekuShieldMm)", Config.MmDekuShield, y);
            NextRow(ref y);
            _chkMmSticksNuts = AddCheckFull(panelMm, "Sticks & Nuts upgrades (sticksNutsUpgradesMm)", Config.MmSticksNuts, ref y);
            _chkMmShortHookshot = AddCheckLeft(panelMm, "Short Hookshot (shortHookshotMm)", Config.MmShortHookshot, ref y);
            _chkMmHammer     = AddCheckRight(panelMm, "Megaton Hammer (hammerMm)", Config.MmHammer, y);
            NextRow(ref y);
            _chkMmSpellFire  = AddCheckLeft(panelMm, "Din's Fire (spellFireMm)", Config.MmSpellFire, ref y);
            _chkMmSpellWind  = AddCheckRight(panelMm, "Farore's Wind (spellWindMm)", Config.MmSpellWind, y);
            NextRow(ref y);
            _chkMmSpellLove  = AddCheck(panelMm, "Nayru's Love (spellLoveMm)", Config.MmSpellLove, ref y);

            y += 8;
            AddSection(panelMm, "Bombchu (MM)", ref y);
            _chkBombchuMmBombBag  = AddCheckLeft(panelMm, "Lit when bombs available (bombBag)", Config.BombchuBehaviorMm == "bombBag", ref y);
            _chkBombchuMmBag      = AddCheckRight(panelMm, "Separate bag 20/30/40 (bagSeparate)", Config.BombchuBehaviorMm == "bag", y);
            NextRow(ref y);
            _chkBombchuMmBombBag.CheckedChanged += (s, e) => { if (_chkBombchuMmBombBag.Checked) _chkBombchuMmBag.Checked = false; };
            _chkBombchuMmBag.CheckedChanged     += (s, e) => { if (_chkBombchuMmBag.Checked)     _chkBombchuMmBombBag.Checked = false; };

            y += 8;
            AddSection(panelMm, "Clocks, owls and maps", ref y);
            _chkClocksEnabled = AddCheckLeft(panelMm, "Clocks (clocks)", Config.ClocksEnabled, ref y);
            _chkOwlShuffle    = AddCheckRight(panelMm, "Owl statues (owlShuffle: anywhere)", Config.OwlShuffle, y);
            NextRow(ref y);
            _chkTingleMaps    = AddCheck(panelMm, "Tingle Maps (tingleShuffle)", Config.TingleMaps, ref y);

            // ── Tab 3: Shared ─────────────────────────────────────────────
            var tabShared = new TabPage("Shared");
            _tabSharedEq = tabShared;  // Keep reference for game-dependent controls
            _tabSharedMask = tabShared;  // Same tab for both
            var panelShared = MakeScrollPanel();
            tabShared.Controls.Add(panelShared);
            tabs.TabPages.Add(tabShared);

            y = 8;
            AddSection(panelShared, "Combined items", ref y);
            _chkSharedHealth     = AddCheckLeft(panelShared, "Health (sharedHealth)", Config.SharedHealth, ref y);
            _chkSharedSwords     = AddCheckRight(panelShared, "Swords (sharedSwords)", Config.SharedSwords, y);
            NextRow(ref y);
            // Shared swords available if OoT progression enabled (MM always has 3 steps)
            _chkSharedSwords.Enabled = Config.OotExtraChildSwords;
            _chkOotExtraSwords.CheckedChanged += (s, e) => { _chkSharedSwords.Enabled = _chkOotExtraSwords.Checked; if (!_chkOotExtraSwords.Checked) _chkSharedSwords.Checked = false; };
            _chkSharedShields    = AddCheckLeft(panelShared, "Shields (sharedShields)", Config.SharedShields, ref y);
            _chkSharedStrength   = AddCheckRight(panelShared, "Strength (sharedStrength)", Config.SharedStrength, y);
            NextRow(ref y);
            _chkSharedStrength.Enabled = Config.MmStrength;
            _chkMmStrength.CheckedChanged += (s, e) => { _chkSharedStrength.Enabled = _chkMmStrength.Checked; if (!_chkMmStrength.Checked) _chkSharedStrength.Checked = false; };
            _chkSharedScales     = AddCheckLeft(panelShared, "Scales (sharedScales)", Config.SharedScales, ref y);
            _chkSharedWallets    = AddCheckRight(panelShared, "Wallets (sharedWallets)", Config.SharedWallets, y);
            NextRow(ref y);
            _chkSharedScales.Enabled = Config.MmScales;
            _chkMmScales.CheckedChanged += (s, e) => { _chkSharedScales.Enabled = _chkMmScales.Checked; if (!_chkMmScales.Checked) _chkSharedScales.Checked = false; };
            _chkSharedMagic      = AddCheckLeft(panelShared, "Magic (sharedMagic)", Config.SharedMagic, ref y);
            _chkSharedTunicGoron = AddCheckRight(panelShared, "Goron Tunic (sharedTunicGoron)", Config.SharedTunicGoron, y);
            NextRow(ref y);
            _chkSharedTunicGoron.Enabled = Config.MmGoronTunic;
            _chkMmGoronTunic.CheckedChanged += (s, e) => { _chkSharedTunicGoron.Enabled = _chkMmGoronTunic.Checked; if (!_chkMmGoronTunic.Checked) _chkSharedTunicGoron.Checked = false; };
            _chkSharedTunicZora  = AddCheckLeft(panelShared, "Zora Tunic (sharedTunicZora)", Config.SharedTunicZora, ref y);
            _chkSharedBootsIron  = AddCheckRight(panelShared, "Iron Boots (sharedBootsIron)", Config.SharedBootsIron, y);
            NextRow(ref y);
            _chkSharedTunicZora.Enabled = Config.MmZoraTunic;
            _chkMmZoraTunic.CheckedChanged += (s, e) => { _chkSharedTunicZora.Enabled = _chkMmZoraTunic.Checked; if (!_chkMmZoraTunic.Checked) _chkSharedTunicZora.Checked = false; };
            _chkSharedBootsIron.Enabled = Config.MmIronBoots;
            _chkMmIronBoots.CheckedChanged += (s, e) => { _chkSharedBootsIron.Enabled = _chkMmIronBoots.Checked; if (!_chkMmIronBoots.Checked) _chkSharedBootsIron.Checked = false; };
            _chkSharedBootsHover = AddCheckLeft(panelShared, "Hover Boots (sharedBootsHover)", Config.SharedBootsHover, ref y);
            _chkSharedHookshot   = AddCheckRight(panelShared, "Hookshot (sharedHookshot)", Config.SharedHookshot, y);
            NextRow(ref y);
            _chkSharedBootsHover.Enabled = Config.MmHoverBoots;
            _chkMmHoverBoots.CheckedChanged += (s, e) => { _chkSharedBootsHover.Enabled = _chkMmHoverBoots.Checked; if (!_chkMmHoverBoots.Checked) _chkSharedBootsHover.Checked = false; };
            _chkSharedBows       = AddCheckLeft(panelShared, "Bows (sharedBows)", Config.SharedBows, ref y);
            _chkSharedBombBags   = AddCheckRight(panelShared, "Bomb Bags (sharedBombBags)", Config.SharedBombBags, y);
            NextRow(ref y);
            _chkSharedHammer     = AddCheckLeft(panelShared, "Hammer (sharedHammer)", Config.SharedHammer, ref y);
            _chkSharedBottles    = AddCheckRight(panelShared, "Bottles (sharedBottles)", Config.SharedBottles, y);
            NextRow(ref y);
            _chkSharedNutsSticks = AddCheckLeft(panelShared, "Sticks & Nuts (sharedNutsSticks)", Config.SharedNutsSticks, ref y);
            _chkSharedBombchu    = AddCheckRight(panelShared, "Bombchu (sharedBombchu)", Config.SharedBombchu, y);
            NextRow(ref y);
            _chkSharedFireArrows = AddCheckLeft(panelShared, "Fire Arrows (sharedMagicArrowFire)", Config.SharedFireArrows, ref y);
            _chkSharedIceArrows  = AddCheckRight(panelShared, "Ice Arrows (sharedMagicArrowIce)", Config.SharedIceArrows, y);
            NextRow(ref y);
            _chkSharedLightArrows= AddCheckLeft(panelShared, "Light Arrows (sharedMagicArrowLight)", Config.SharedLightArrows, ref y);
            _chkSharedLens       = AddCheckRight(panelShared, "Lens of Truth (sharedLens)", Config.SharedLens, y);
            NextRow(ref y);
            _chkSharedSpellFire  = AddCheckLeft(panelShared, "Din's Fire (sharedSpellFire)", Config.SharedSpellFire, ref y);
            _chkSharedSpellWind  = AddCheckRight(panelShared, "Farore's Wind (sharedSpellWind)", Config.SharedSpellWind, y);
            NextRow(ref y);
            _chkSharedSpellFire.Enabled = Config.MmSpellFire;
            _chkMmSpellFire.CheckedChanged += (s, e) => { _chkSharedSpellFire.Enabled = _chkMmSpellFire.Checked; if (!_chkMmSpellFire.Checked) _chkSharedSpellFire.Checked = false; };
            _chkSharedSpellWind.Enabled = Config.MmSpellWind;
            _chkMmSpellWind.CheckedChanged += (s, e) => { _chkSharedSpellWind.Enabled = _chkMmSpellWind.Checked; if (!_chkMmSpellWind.Checked) _chkSharedSpellWind.Checked = false; };
            _chkSharedSpellLove  = AddCheckLeft(panelShared, "Nayru's Love (sharedSpellLove)", Config.SharedSpellLove, ref y);
            _chkSharedStoneAgony = AddCheckRight(panelShared, "Stone of Agony (sharedStoneAgony)", Config.SharedStoneAgony, y);
            NextRow(ref y);
            _chkSharedSpellLove.Enabled = Config.MmSpellLove;
            _chkMmSpellLove.CheckedChanged += (s, e) => { _chkSharedSpellLove.Enabled = _chkMmSpellLove.Checked; if (!_chkMmSpellLove.Checked) _chkSharedSpellLove.Checked = false; };
            _chkSharedSpinUpgrade= AddCheckFull(panelShared, "Great Spin Attack (sharedSpinUpgrade)", Config.SharedSpinUpgrade, ref y);
            _chkSharedSpinUpgrade.Enabled = Config.OotSpinUpgrade;
            _chkOotSpinUpgrade.CheckedChanged += (s, e) => { _chkSharedSpinUpgrade.Enabled = _chkOotSpinUpgrade.Checked; if (!_chkOotSpinUpgrade.Checked) _chkSharedSpinUpgrade.Checked = false; };

            y += 8;
            AddSection(panelShared, "Combined masks", ref y);
            _chkSharedMaskKeaton = AddCheckLeft(panelShared, "Keaton Mask (sharedMaskKeaton)", Config.SharedMaskKeaton, ref y);
            _chkSharedMaskBunny  = AddCheckRight(panelShared, "Bunny Hood (sharedMaskBunny)", Config.SharedMaskBunny, y);
            NextRow(ref y);
            _chkSharedMaskGoron  = AddCheckLeft(panelShared, "Goron Mask (sharedMaskGoron)", Config.SharedMaskGoron, ref y);
            _chkSharedMaskZora   = AddCheckRight(panelShared, "Zora Mask (sharedMaskZora)", Config.SharedMaskZora, y);
            NextRow(ref y);
            _chkSharedMaskTruth  = AddCheckLeft(panelShared, "Mask of Truth (sharedMaskTruth)", Config.SharedMaskTruth, ref y);
            _chkSharedMaskBlast  = AddCheckRight(panelShared, "Blast Mask (sharedMaskBlast)", Config.SharedMaskBlast, y);
            NextRow(ref y);
            _chkSharedMaskBlast.Enabled = Config.OotBlastMask;
            _chkOotBlastMask.CheckedChanged += (s, e) => { _chkSharedMaskBlast.Enabled = _chkOotBlastMask.Checked; if (!_chkOotBlastMask.Checked) _chkSharedMaskBlast.Checked = false; UpdateScMax(); };
            _chkSharedMaskStone  = AddCheck(panelShared, "Stone Mask (sharedMaskStone)", Config.SharedMaskStone, ref y);
            _chkSharedMaskStone.Enabled = Config.OotStoneMask;
            _chkOotStoneMask.CheckedChanged += (s, e) => { _chkSharedMaskStone.Enabled = _chkOotStoneMask.Checked; if (!_chkOotStoneMask.Checked) _chkSharedMaskStone.Checked = false; UpdateScMax(); };
            
            // Subscribe shared masks to update SC max
            _chkSharedMaskKeaton.CheckedChanged += (s, e) => UpdateScMax();
            _chkSharedMaskBunny.CheckedChanged += (s, e) => UpdateScMax();
            _chkSharedMaskGoron.CheckedChanged += (s, e) => UpdateScMax();
            _chkSharedMaskZora.CheckedChanged += (s, e) => UpdateScMax();
            _chkSharedMaskTruth.CheckedChanged += (s, e) => UpdateScMax();
            _chkSharedMaskBlast.CheckedChanged += (s, e) => UpdateScMax();
            _chkSharedMaskStone.CheckedChanged += (s, e) => UpdateScMax();

            y += 8;
            AddSection(panelShared, "Shared songs", ref y);
            _chkSharedSongEpona  = AddCheckLeft(panelShared, "Epona's Song (sharedSongEpona)", Config.SharedSongEpona, ref y);
            _chkSharedSongSun    = AddCheckRight(panelShared, "Sun's Song (sharedSongSun)", Config.SharedSongSun, y);
            NextRow(ref y);
            _chkSharedSongTime   = AddCheckLeft(panelShared, "Song of Time (sharedSongTime)", Config.SharedSongTime, ref y);
            _chkSharedSongStorms = AddCheckRight(panelShared, "Song of Storms (sharedSongStorms)", Config.SharedSongStorms, y);
            NextRow(ref y);
            _chkSharedSongElegy  = AddCheckFull(panelShared, "Elegy of Emptiness (sharedSongElegy)", Config.SharedSongElegy, ref y);
            _chkSharedScarecrow  = AddCheckFull(panelShared, "Scarecrow's Song (shared, manual)", Config.SharedScarecrow, ref y);
            _chkSharedOcarina    = AddCheckLeft(panelShared, "Ocarina (sharedOcarina)", Config.SharedOcarina, ref y);
            _chkSharedOcarinaButtons = AddCheckRight(panelShared, "Ocarina buttons (sharedOcarinaButtons)", Config.SharedOcarinaButtons, y);
            NextRow(ref y);

            y += 8;
            AddSection(panelShared, "Shared tokens", ref y);
            _chkSharedPlatinum   = AddCheckFull(panelShared, "Platinum Token (if both OoT+MM enabled)", Config.SharedPlatinumToken, ref y);

            y += 8;
            AddSection(panelShared, "Shared keys", ref y);
            _chkSharedSkeletonKey = AddCheckFull(panelShared, "Skeleton Key (combine OoT + MM)", Config.SharedSkeletonKey, ref y);

            y += 8;
            AddSection(panelShared, "Shared souls", ref y);
            _chkSharedSoulsEnemy = AddCheckLeft(panelShared, "Enemy Souls (combine OoT+MM)", Config.SharedSoulsEnemy, ref y);
            _chkSharedSoulsNpc = AddCheckRight(panelShared, "NPC Souls (combine OoT+MM)", Config.SharedSoulsNpc, y);
            NextRow(ref y);
            _chkSharedSoulsAnimal = AddCheckLeft(panelShared, "Animal Souls (combine OoT+MM)", Config.SharedSoulsAnimal, ref y);
            _chkSharedSoulsMisc = AddCheckRight(panelShared, "Misc Souls (combine OoT+MM)", Config.SharedSoulsMisc, y);
            NextRow(ref y);

            // ── Tab 4: Songs ───────────────────────────────────────────────
            var tabSongs = new TabPage("Songs");
            _tabSongs = tabSongs;
            var panelSongs = MakeScrollPanel();
            tabSongs.Controls.Add(panelSongs);
            tabs.TabPages.Add(tabSongs);

            y = 8;
            AddSection(panelSongs, "OoT — additional", ref y);
            _chkOotElegy         = AddCheckLeft(panelSongs, "Elegy of Emptiness (elegyOot)", Config.OotElegy, ref y);
            _chkFreeScarecrowOot = AddCheckRight(panelSongs, "Scarecrow available immediately (freeScarecrowOot)", Config.FreeScarecrowOot, y);
            NextRow(ref y);

            y += 8;
            AddSection(panelSongs, "MM — additional", ref y);
            _chkMmSunSong        = AddCheckLeft(panelSongs, "Sun's Song (sunSongMm)", Config.MmSunSong, ref y);
            _chkMmFairyOcarina   = AddCheckRight(panelSongs, "Ocarina progression Fairy→Time (fairyOcarinaMm)", Config.MmFairyOcarina, y);
            NextRow(ref y);
            _chkProgressiveGoron = AddCheckFull(panelSongs, "Goron Lullaby progression (progressive)", Config.ProgressiveGoronLullaby, ref y);
            _chkFreeScarecrowMm  = AddCheckFull(panelSongs, "Scarecrow available immediately (freeScarecrowMm)", Config.FreeScarecrowMm, ref y);

            y += 8;
            AddSection(panelSongs, "Ocarina buttons", ref y);
            _chkOotOcarinaButtons    = AddCheckLeft(panelSongs, "OoT buttons (ocarinaButtonsShuffleOot)", Config.OotOcarinaButtons, ref y);
            _chkMmOcarinaButtons     = AddCheckRight(panelSongs, "MM buttons (ocarinaButtonsShuffleMm)", Config.MmOcarinaButtons, y);
            NextRow(ref y);

            y += 8;
            AddSection(panelSongs, "Notes mode", ref y);
            _chkSongsAsNotes = AddCheck(panelSongs, "Songs as notes (songs: notes)", Config.SongsAsNotes, ref y);

            // ── Tab 5: Collectibles ───────────────────────────────────────
            var tabCollectibles = new TabPage("Collectibles");
            var panelCollectibles = MakeScrollPanel();
            tabCollectibles.Controls.Add(panelCollectibles);
            tabs.TabPages.Add(tabCollectibles);

            y = 8;
            AddSection(panelCollectibles, "Tokens (OoT)", ref y);
            _chkGoldSkulltulas = AddCheckFull(panelCollectibles, "Gold Skulltula Tokens — 100 pcs. (goldSkulltulaTokens)", Config.GoldSkulltulas, ref y);

            y += 8;
            AddSection(panelCollectibles, "Tokens (MM)", ref y);
            _chkMmSkulltulas = AddCheckFull(panelCollectibles, "Swamp + Ocean Skulltula Tokens — 30+30 pcs. (housesSkulltulaTokens)", Config.MmSkulltulas, ref y);

            y += 8;
            AddSection(panelCollectibles, "Platinum Token", ref y);
            _chkPlatinumTokenOot = AddCheckLeft(panelCollectibles, "Platinum Token OoT (platinumTokenOot)", Config.PlatinumTokenOot, ref y);
            _chkPlatinumTokenMm  = AddCheckRight(panelCollectibles, "Platinum Token MM (platinumTokenMm)", Config.PlatinumTokenMm, y);
            NextRow(ref y);

            y += 8;
            AddSection(panelCollectibles, "Stray Fairies", ref y);
            _chkStrayFairiesDungeons = AddCheckFull(panelCollectibles, "Dungeon fairies — 4×15 pcs. (strayFairyChestShuffle / strayFairyOtherShuffle)", Config.StrayFairiesDungeons, ref y);
            _chkStrayFairyTown       = AddCheckFull(panelCollectibles, "Clock Town Fairy — 1 pc. (townFairyShuffle)", Config.StrayFairyTown, ref y);
            _chkTranscendentFairy    = AddCheckFull(panelCollectibles, "Transcendent Fairy (transcendentFairy)", Config.TranscendentFairy, ref y);

            // ── Tab 6: Dungeons ───────────────────────────────────────────────
            var tabDungeons = new TabPage("Dungeons");
            var panelDungeons = MakeScrollPanel();
            tabDungeons.Controls.Add(panelDungeons);
            tabs.TabPages.Add(tabDungeons);

            y = 8;
            AddSection(panelDungeons, "Maps and compasses", ref y);
            _chkMapsCompasses = AddCheck(panelDungeons, "Maps & Compasses (mapCompassShuffle)", Config.MapsCompasses, ref y);

            y += 8;
            AddSection(panelDungeons, "Small keys", ref y);
            _chkSmallKeysOot = AddCheckLeft(panelDungeons, "Small Keys OoT (smallKeyShuffleOot)", Config.SmallKeysOot, ref y);
            _chkSmallKeysMm  = AddCheckRight(panelDungeons, "Small Keys MM (smallKeyShuffleMm)", Config.SmallKeysMm, y);
            NextRow(ref y);
            _chkKeysanity    = AddCheckFull(panelDungeons, "Keysanity — Fire Temple 8 keys (smallKeyShuffleOot: anywhere)", Config.Keysanity, ref y);
            _chkKeysanity.Enabled = Config.SmallKeysOot;
            _chkSmallKeysOot.CheckedChanged += (s, e) => { _chkKeysanity.Enabled = _chkSmallKeysOot.Checked; if (!_chkSmallKeysOot.Checked) _chkKeysanity.Checked = false; };

            y += 8;
            AddSection(panelDungeons, "Silver Rupees", ref y);
            _chkSilverRupees = AddCheck(panelDungeons, "Silver Rupees (silverRupeeShuffle)", Config.SilverRupees, ref y);

            y += 8;
            AddSection(panelDungeons, "Skeleton Key", ref y);
            var lblSK = new Label { Text = "Replaces all small keys with one item.", ForeColor = Color.Gray, Location = new Point(8, y), AutoSize = true };
            panelDungeons.Controls.Add(lblSK);
            y += 20;
            _chkSkeletonKeyOot = AddCheckLeft(panelDungeons, "Skeleton Key OoT (skeletonKeyOot)", Config.SkeletonKeyOot, ref y);
            _chkSkeletonKeyMm  = AddCheckRight(panelDungeons, "Skeleton Key MM (skeletonKeyMm)", Config.SkeletonKeyMm, y);
            NextRow(ref y);
            // Skeleton Key MM available only if MM enabled
            _chkSkeletonKeyMm.Enabled = Config.HasMm;

            y += 8;
            AddSection(panelDungeons, "Magical Rupee", ref y);
            var lblMR = new Label { Text = "Replaces all Silver Rupees with one item.", ForeColor = Color.Gray, Location = new Point(8, y), AutoSize = true };
            panelDungeons.Controls.Add(lblMR);
            y += 20;
            _chkMagicalRupee = AddCheck(panelDungeons, "Magical Rupee (magicalRupee)", Config.MagicalRupee, ref y);
            _chkMagicalRupee.Enabled = Config.SilverRupees;
            _chkSilverRupees.CheckedChanged += (s, e) => { _chkMagicalRupee.Enabled = _chkSilverRupees.Checked; if (!_chkSilverRupees.Checked) _chkMagicalRupee.Checked = false; };

            y += 8;
            AddSection(panelDungeons, "Silver Rupee Pouches", ref y);
            var lblSrp = new Label { Text = "Pouch replaces all pack rupees with one item. Vanilla (left) / MQ (right).", ForeColor = Color.Gray, Location = new Point(8, y), AutoSize = true };
            panelDungeons.Controls.Add(lblSrp);
            y += 20;
            _chkSrPouchAll = AddCheck(panelDungeons, "All packs (Silver Rupee Pouches: all)", Config.SrPouchPacks.Count > 0, ref y);

            // Define packs: Vanilla left, MQ right
            // Each row: (vanillaKey, vanillaName, vanillaDung, mqKey, mqName, mqDung)
            var srPouchRows = new List<(string vKey, string vName, string vDung, string mKey, string mName, string mDung)>
            {
                // Dodongo - only MQ has pack
                (null, null, null, "dodongo_mq_staircase", "Dodongo (Staircase)", "dodongo"),
                // Shadow Temple
                ("shadow_temple_scythe", "Shadow (Scythe)", "shadow_temple", "shadow_temple_mq_scythe", "Shadow (Scythe)", "shadow_temple"),
                ("shadow_temple_pit", "Shadow (Pit)", "shadow_temple", "shadow_temple_mq_pit", "Shadow (Pit)", "shadow_temple"),
                ("shadow_temple_spikes", "Shadow (Spikes)", "shadow_temple", "shadow_temple_mq_spikes", "Shadow (Spikes)", "shadow_temple"),
                (null, null, null, "shadow_temple_mq_blades", "Shadow (Blades)", "shadow_temple"),
                // Spirit Temple
                ("spirit_temple_child", "Spirit (Child)", "spirit_temple", "spirit_temple_mq_lobby", "Spirit (Lobby)", "spirit_temple"),
                ("spirit_temple_sun", "Spirit (Sun)", "spirit_temple", "spirit_temple_mq_adult", "Spirit (Adult)", "spirit_temple"),
                ("spirit_temple_boulders", "Spirit (Boulders)", "spirit_temple", null, null, null),
                // Ice Cavern
                ("ice_cavern_scythe", "Ice Cavern (Scythe)", "ice_cavern", null, null, null),
                ("ice_cavern_block", "Ice Cavern (Block)", "ice_cavern", null, null, null),
                // Bottom of the Well
                ("botw_basement", "BotW (Basement)", "botw", null, null, null),
                // GTG
                ("gtg_slopes", "GTG (Slopes)", "gtg", "gtg_mq_slopes", "GTG (Slopes)", "gtg"),
                ("gtg_lava", "GTG (Lava)", "gtg", "gtg_mq_lava", "GTG (Lava)", "gtg"),
                ("gtg_water", "GTG (Water)", "gtg", "gtg_mq_water", "GTG (Water)", "gtg"),
                // Ganon's Castle
                ("ganons_castle_spirit", "Ganon (Spirit)", "ganons_castle", "ganons_castle_mq_fire", "Ganon (Fire)", "ganons_castle"),
                ("ganons_castle_light", "Ganon (Light)", "ganons_castle", "ganons_castle_mq_shadow", "Ganon (Shadow)", "ganons_castle"),
                ("ganons_castle_fire", "Ganon (Fire)", "ganons_castle", "ganons_castle_mq_water", "Ganon (Water)", "ganons_castle"),
                ("ganons_castle_forest", "Ganon (Forest)", "ganons_castle", null, null, null),
            };

            foreach (var (vKey, vName, vDung, mKey, mName, mDung) in srPouchRows)
            {
                // Add vanilla (left)
                if (vKey != null && vName != null)
                {
                    var chkV = AddCheckLeft(panelDungeons, vName, Config.SrPouchPacks.Contains(vKey), ref y);
                    _chkSrPouchPerPack[vKey] = chkV;
                    // Enable when dungeon is NOT MQ
                    bool dungIsMq = Config.MqDungeons.Contains(vDung);
                    chkV.Enabled = !dungIsMq;
                }

                // Add MQ (right)
                if (mKey != null && mName != null)
                {
                    var chkM = AddCheckRight(panelDungeons, mName, Config.SrPouchPacks.Contains(mKey), y);
                    _chkSrPouchPerPack[mKey] = chkM;
                    // Enable when dungeon IS MQ
                    bool dungIsMq = Config.MqDungeons.Contains(mDung);
                    chkM.Enabled = dungIsMq;
                }

                NextRow(ref y);
            }

            // Subscribe after creating all
            _chkSrPouchAll.CheckedChanged += (s, e) => UpdateSrPouchAll(_chkSrPouchAll.Checked);
            foreach (var kv in _chkSrPouchPerPack)
                kv.Value.CheckedChanged += (s, e) => SyncSrPouchAll();

            y += 8;
            AddSection(panelDungeons, "Boss keys", ref y);
            _chkBossKeysOot = AddCheckLeft(panelDungeons, "Boss Keys OoT (bossKeyShuffleOot)", Config.BossKeysOot, ref y);
            _chkBossKeysMm  = AddCheckRight(panelDungeons, "Boss Keys MM (bossKeyShuffleMm)", Config.BossKeysMm, y);
            NextRow(ref y);

            y += 8;
            AddSection(panelDungeons, "Ganon's key (ganonBossKey)", ref y);
            _chkGanonBkAnywhere = AddCheckLeft(panelDungeons, "Anywhere", Config.GanonBossKey == "anywhere", ref y);
            _chkGanonBkCustom   = AddCheckRight(panelDungeons, "Custom", Config.GanonBossKey == "custom", y);
            NextRow(ref y);
            // Mutual exclusion: if one is checked, uncheck the other
            _chkGanonBkAnywhere.CheckedChanged += (s, e) => { if (_chkGanonBkAnywhere.Checked) _chkGanonBkCustom.Checked = false; };
            _chkGanonBkCustom.CheckedChanged   += (s, e) => { if (_chkGanonBkCustom.Checked) _chkGanonBkAnywhere.Checked = false; };
            // If neither is checked = vanilla (default)

            y += 8;
            AddSection(panelDungeons, "Additional blocks", ref y);
            _chkSmallKeysHideout      = AddCheckLeft(panelDungeons, "Thieves' Hideout keys (smallKeyShuffleHideout)", Config.SmallKeysHideout, ref y);
            _chkSmallKeysTcg          = AddCheckRight(panelDungeons, "Treasure Chest Game keys (manual option)", Config.SmallKeysTcg, y);
            NextRow(ref y);

            y += 8;
            AddSection(panelDungeons, "Key Rings (Small Key Ring)", ref y);
            var lblKR = new Label { Text = "Key Ring replaces all dungeon keys with one item.", ForeColor = Color.Gray, Location = new Point(8, y), AutoSize = true };
            panelDungeons.Controls.Add(lblKR);
            y += 20;
            _chkKeyRingsOot = AddCheckLeft(panelDungeons, "All OoT dungeons (Small Key Ring (OoT): all)", Config.KeyRingsOot, ref y);
            _chkKeyRingsMm  = AddCheckRight(panelDungeons, "All MM dungeons (Small Key Ring (MM): all)", Config.KeyRingsMm, y);
            NextRow(ref y);

            y += 4;
            // Individual dungeons according to your layout:
            // Woodfall - Snowhead
            // Great Bay - Stone Tower
            // Forest - Fire
            // Water - Shadow
            // Spirit - BotW
            // GTG - Ganon
            // TH - TCG
            var krRows = new (string leftId, string leftName, string rightId, string rightName)[]
            {
                ("woodfall",      "Woodfall Temple",         "snowhead",       "Snowhead Temple"),
                ("great_bay",     "Great Bay Temple",        "stone_tower",    "Stone Tower Temple"),
                ("forest_temple", "Forest Temple",           "fire_temple",    "Fire Temple"),
                ("water_temple",  "Water Temple",            "shadow_temple",  "Shadow Temple"),
                ("spirit_temple", "Spirit Temple",           "botw",           "Bottom of the Well"),
                ("gtg",           "Gerudo Training Ground",  "ganons_castle",  "Ganon's Castle"),
                ("thieves_hideout","Thieves' Hideout",       "tcg",            "Treasure Chest Game"),
            };
            
            foreach (var (leftId, leftName, rightId, rightName) in krRows)
            {
                // Left dungeon
                if (leftId != null && leftName != null)
                {
                    var chk = AddCheckLeft(panelDungeons, leftName, Config.KeyRingDungeons.Contains(leftId), ref y);
                    _chkKeyRingPerDungeon[leftId] = chk;
                }
                
                // Right dungeon
                if (rightId != null && rightName != null)
                {
                    var chk = AddCheckRight(panelDungeons, rightName, Config.KeyRingDungeons.Contains(rightId), y);
                    _chkKeyRingPerDungeon[rightId] = chk;
                }
                
                NextRow(ref y);
            }
            
            // Subscribe handlers AFTER creating all checkboxes
            _chkKeyRingsOot.CheckedChanged += (s, e) => UpdateKeyRingAll(true,  _chkKeyRingsOot.Checked);
            _chkKeyRingsMm.CheckedChanged  += (s, e) => UpdateKeyRingAll(false, _chkKeyRingsMm.Checked);
            foreach (var kv in _chkKeyRingPerDungeon)
                kv.Value.CheckedChanged += (s, e) => SyncKeyRingAllCheckboxes();

            // Set up dependencies for TH and TCG Key Rings
            if (_chkKeyRingPerDungeon.TryGetValue("thieves_hideout", out var krTH))
            {
                krTH.Enabled = Config.SmallKeysHideout;
                _chkSmallKeysHideout.CheckedChanged += (s, e) => 
                { 
                    krTH.Enabled = _chkSmallKeysHideout.Checked; 
                    if (!_chkSmallKeysHideout.Checked) krTH.Checked = false; 
                };
            }
            if (_chkKeyRingPerDungeon.TryGetValue("tcg", out var krTCG))
            {
                krTCG.Enabled = Config.SmallKeysTcg;
                _chkSmallKeysTcg.CheckedChanged += (s, e) => 
                { 
                    krTCG.Enabled = _chkSmallKeysTcg.Checked; 
                    if (!_chkSmallKeysTcg.Checked) krTCG.Checked = false; 
                };
            }

            // ── Tab 7: Special Conditions (Ganon BK) ─────────────────────
            var tabSC = new TabPage("Special Cond.");
            var panelSC = MakeScrollPanel();
            tabSC.Controls.Add(panelSC);
            tabs.TabPages.Add(tabSC);

            y = 8;
            var lblSC = new Label { Text = "Conditions for Custom Ganon BK.\nFilled automatically from log.\nCan be configured manually for testing.", ForeColor = Color.Gray, Location = new Point(8, y), AutoSize = true };
            panelSC.Controls.Add(lblSC);
            y += 48;

            AddSection(panelSC, "Amount (threshold)", ref y);
            var lblAmount = new Label { Text = "Need to collect:", Location = new Point(16, y), AutoSize = true };
            panelSC.Controls.Add(lblAmount);
            _numScAmount = new NumericUpDown { Minimum = 0, Maximum = 9999, Value = Config.GanonBkRequired > 0 ? Config.GanonBkRequired : 9, Location = new Point(130, y - 2), Size = new Size(80, 22) };
            panelSC.Controls.Add(_numScAmount);
            y += 26;

            AddSection(panelSC, "Dungeons", ref y);
            _scStones     = AddCheckLeft(panelSC, "Stones (3)", Config.GanonBk.Stones, ref y);
            _scMedallions = AddCheckRight(panelSC, "Medallions (6)", Config.GanonBk.Medallions, y);
            NextRow(ref y);
            _scRemains    = AddCheck(panelSC, "Remains (4)", Config.GanonBk.Remains, ref y);

            y += 8;
            AddSection(panelSC, "Tokens", ref y);
            _scSkullsGold  = AddCheckLeft(panelSC, "Gold Skulltulas (100)", Config.GanonBk.SkullsGold, ref y);
            _scSkullsSwamp = AddCheckRight(panelSC, "Swamp Skulltulas (30)", Config.GanonBk.SkullsSwamp, y);
            NextRow(ref y);
            _scSkullsOcean = AddCheck(panelSC, "Ocean Skulltulas (30)", Config.GanonBk.SkullsOcean, ref y);

            y += 8;
            AddSection(panelSC, "Fairies", ref y);
            _scFairiesWF   = AddCheckLeft(panelSC, "Woodfall Fairies (15)", Config.GanonBk.FairiesWF, ref y);
            _scFairiesSH   = AddCheckRight(panelSC, "Snowhead Fairies (15)", Config.GanonBk.FairiesSH, y);
            NextRow(ref y);
            _scFairiesGB   = AddCheckLeft(panelSC, "Great Bay Fairies (15)", Config.GanonBk.FairiesGB, ref y);
            _scFairiesST   = AddCheckRight(panelSC, "Stone Tower Fairies (15)", Config.GanonBk.FairiesST, y);
            NextRow(ref y);
            _scFairyTown   = AddCheck(panelSC, "Clock Town Fairy (1)", Config.GanonBk.FairyTown, ref y);

            y += 8;
            AddSection(panelSC, "Masks", ref y);
            _scMasksRegular   = AddCheckLeft(panelSC, "Regular Masks (MM)", Config.GanonBk.MasksRegular, ref y);
            _scMasksTransform = AddCheckRight(panelSC, "Transform Masks (MM)", Config.GanonBk.MasksTransform, y);
            NextRow(ref y);
            _scMasksOot       = AddCheck(panelSC, "Masks (OoT)", Config.GanonBk.MasksOot, ref y);

            y += 8;
            AddSection(panelSC, "Coins", ref y);
            _scCoinsRed    = AddCheckLeft(panelSC, "Red Coins", Config.GanonBk.CoinsRed, ref y);
            _scCoinsGreen  = AddCheckRight(panelSC, "Green Coins", Config.GanonBk.CoinsGreen, y);
            NextRow(ref y);
            _scCoinsBlue   = AddCheckLeft(panelSC, "Blue Coins", Config.GanonBk.CoinsBlue, ref y);
            _scCoinsYellow = AddCheckRight(panelSC, "Yellow Coins", Config.GanonBk.CoinsYellow, y);
            NextRow(ref y);

            y += 8;
            AddSection(panelSC, "Triforce", ref y);
            _scTriforce = AddCheck(panelSC, "Triforce Hunt (triforce)", Config.GanonBk.Triforce, ref y);

            // Subscribe all SC checkboxes to recalculate maximum
            foreach (var scChk in new[] { _scStones, _scMedallions, _scRemains,
                _scSkullsGold, _scSkullsSwamp, _scSkullsOcean,
                _scFairiesWF, _scFairiesSH, _scFairiesGB, _scFairiesST, _scFairyTown,
                _scMasksRegular, _scMasksTransform, _scMasksOot,
                _scCoinsRed, _scCoinsGreen, _scCoinsBlue, _scCoinsYellow, _scTriforce })
                scChk.CheckedChanged += (s, e) => UpdateScMax();

            // Initialize maximum will be done later after all tabs are created

            // ── Tab 8: Souls ──────────────────────────────────────────────
            var tabSouls = new TabPage("Souls");
            var panelSouls = MakeScrollPanel();
            tabSouls.Controls.Add(panelSouls);
            tabs.TabPages.Add(tabSouls);

            y = 8;
            AddSection(panelSouls, "Boss Souls (always separate)", ref y);
            _chkSoulsBossOot = AddCheckLeft(panelSouls, "Boss Souls OoT (soulsBossOot)", Config.SoulsBossOot, ref y);
            _chkSoulsBossMm  = AddCheckRight(panelSouls, "Boss Souls MM (soulsBossMm)", Config.SoulsBossMm, y);
            NextRow(ref y);

            y += 8;
            AddSection(panelSouls, "Enemy Souls", ref y);
            _chkSoulsEnemyOot    = AddCheckLeft(panelSouls, "Enemy OoT (soulsEnemyOot)", Config.SoulsEnemyOot, ref y);
            _chkSoulsEnemyMm     = AddCheckRight(panelSouls, "Enemy MM (soulsEnemyMm)", Config.SoulsEnemyMm, y);
            NextRow(ref y);

            y += 8;
            AddSection(panelSouls, "NPC Souls", ref y);
            _chkSoulsNpcOot    = AddCheckLeft(panelSouls, "NPC OoT (soulsNpcOot)", Config.SoulsNpcOot, ref y);
            _chkSoulsNpcMm     = AddCheckRight(panelSouls, "NPC MM (soulsNpcMm)", Config.SoulsNpcMm, y);
            NextRow(ref y);

            y += 8;
            AddSection(panelSouls, "Animal Souls", ref y);
            _chkSoulsAnimalOot    = AddCheckLeft(panelSouls, "Animal OoT (soulsAnimalOot)", Config.SoulsAnimalOot, ref y);
            _chkSoulsAnimalMm     = AddCheckRight(panelSouls, "Animal MM (soulsAnimalMm)", Config.SoulsAnimalMm, y);
            NextRow(ref y);

            y += 8;
            AddSection(panelSouls, "Misc Souls", ref y);
            _chkSoulsMiscOot    = AddCheckLeft(panelSouls, "Misc OoT (soulsMiscOot)", Config.SoulsMiscOot, ref y);
            _chkSoulsMiscMm     = AddCheckRight(panelSouls, "Misc MM (soulsMiscMm)", Config.SoulsMiscMm, y);
            NextRow(ref y);

            // ── Tab 9: Misc ───────────────────────────────────────────────
            var tabMisc = new TabPage("Misc");
            var panelMisc = MakeScrollPanel();
            tabMisc.Controls.Add(panelMisc);
            tabs.TabPages.Add(tabMisc);

            y = 8;
            AddSection(panelMisc, "Game mode (games)", ref y);
            _chkGamesOot = AddCheckLeft(panelMisc, "OoT enabled", Config.HasOot, ref y);
            _chkGamesMm  = AddCheckRight(panelMisc, "MM enabled", Config.HasMm, y);
            NextRow(ref y);
            // At least one game must be enabled
            _chkGamesOot.CheckedChanged += (s, e) => { if (!_chkGamesOot.Checked && !_chkGamesMm.Checked) _chkGamesMm.Checked = true; UpdateGameDependentControls(); };
            _chkGamesMm.CheckedChanged  += (s, e) => { if (!_chkGamesMm.Checked && !_chkGamesOot.Checked) _chkGamesOot.Checked = true; UpdateGameDependentControls(); };

            y += 8;
            AddSection(panelMisc, "Wallets", ref y);
            _chkChildWallet      = AddCheckLeft(panelMisc, "Child's Wallet (childWallets)", Config.ChildWallet, ref y);
            _chkColossalWallet   = AddCheckRight(panelMisc, "Colossal Wallet (colossalWallets)", Config.ColossalWallet, y);
            NextRow(ref y);
            _chkBottomlessWallet = AddCheckFull(panelMisc, "Bottomless Wallet (bottomlessWallets)", Config.BottomlessWallet, ref y);
            // Bottomless available only if Colossal enabled
            _chkBottomlessWallet.Enabled = Config.ColossalWallet;
            _chkColossalWallet.CheckedChanged += (s, e) =>
            {
                _chkBottomlessWallet.Enabled = _chkColossalWallet.Checked;
                if (!_chkColossalWallet.Checked) _chkBottomlessWallet.Checked = false;
            };

            y += 8;
            AddSection(panelMisc, "Trade Quest (OoT — optional items)", ref y);
            _chkOotSkipZelda    = AddCheckFull(panelMisc, "Skip Child Zelda (skipZelda) — hide Child Cucco", Config.OotSkipZelda, ref y);
            _chkOotOpenKakariko = AddCheckFull(panelMisc, "Open Kakariko Gate (kakarikoGate: open) — hide Letter", Config.OotOpenKakariko, ref y);
            _chkOotEggShuffle   = AddCheckFull(panelMisc, "Egg Shuffle (eggShuffle) — show Weird/Pocket Egg", Config.OotEggShuffle, ref y);

            y += 8;
            AddSection(panelMisc, "Coins (Misc)", ref y);
            var lblCoinsHint = new Label { Text = "When loading log, maximum is taken from settings.\nManually: enter number from 1 to 999.", ForeColor = Color.Gray, Location = new Point(8, y), AutoSize = true };
            panelMisc.Controls.Add(lblCoinsHint);
            y += 32;
            _chkCoinsRed    = AddCheckWithNumLeft(panelMisc, "Red Coins (coinsRed)", Config.CoinsRed, Config.CoinsRedMax, out _numCoinsRed, ref y);
            _chkCoinsGreen  = AddCheckWithNumRight(panelMisc, "Green Coins (coinsGreen)", Config.CoinsGreen, Config.CoinsGreenMax, out _numCoinsGreen, y);
            NextRow(ref y);
            _chkCoinsBlue   = AddCheckWithNumLeft(panelMisc, "Blue Coins (coinsBlue)", Config.CoinsBlue, Config.CoinsBlueMax, out _numCoinsBlue, ref y);
            _chkCoinsYellow = AddCheckWithNumRight(panelMisc, "Yellow Coins (coinsYellow)", Config.CoinsYellow, Config.CoinsYellowMax, out _numCoinsYellow, y);
            NextRow(ref y);

            y += 8;
            AddSection(panelMisc, "Triforce (goal)", ref y);
            _chkTriforceQuest = AddCheckFull(panelMisc, "Quest — 3 pieces (goal: triforce3)", Config.TriforceMode == "quest", ref y);
            _chkTriforceHunt  = AddCheck(panelMisc, "Hunt — piece count (goal: triforce)", Config.TriforceMode == "hunt", ref y);
            var lblHuntGoal = new Label { Text = "Goal:", Location = new Point(32, y), AutoSize = true };
            panelMisc.Controls.Add(lblHuntGoal);
            _numTriforceHuntGoal = new NumericUpDown { Minimum = 1, Maximum = 999, Value = Config.TriforceHuntGoal, Location = new Point(80, y - 2), Size = new Size(70, 22), Enabled = Config.TriforceMode == "hunt" };
            panelMisc.Controls.Add(_numTriforceHuntGoal);
            y += 26;
            // Mutual exclusion Quest/Hunt
            _chkTriforceQuest.CheckedChanged += (s, e) => { if (_chkTriforceQuest.Checked) { _chkTriforceHunt.Checked = false; _numTriforceHuntGoal.Enabled = false; } };
            _chkTriforceHunt.CheckedChanged  += (s, e) => { if (_chkTriforceHunt.Checked)  { _chkTriforceQuest.Checked = false; _numTriforceHuntGoal.Enabled = true; } else { _numTriforceHuntGoal.Enabled = false; } };

            y += 8;
            AddSection(panelMisc, "Master Quest Dungeons", ref y);
            _chkMqAll = AddCheckFull(panelMisc, "All OoT dungeons (Master Quest Dungeons: all)", Config.MqDungeons.Count >= 12, ref y);
            var mqDungeons = new (string id, string name)[]
            {
                ("deku_tree",     "Deku Tree"),
                ("dodongo",       "Dodongo's Cavern"),
                ("jabu",          "Inside Jabu-Jabu"),
                ("forest_temple", "Forest Temple"),
                ("fire_temple",   "Fire Temple"),
                ("water_temple",  "Water Temple"),
                ("shadow_temple", "Shadow Temple"),
                ("spirit_temple", "Spirit Temple"),
                ("botw",          "Bottom of the Well"),
                ("ice_cavern",    "Ice Cavern"),
                ("gtg",           "Gerudo Training Ground"),
                ("ganons_castle", "Ganon's Castle"),
            };
            // Add MQ dungeons in 2 columns
            int col = 0;
            int startY = y;
            foreach (var (dungId, dungName) in mqDungeons)
            {
                CheckBox chk;
                if (col == 0)
                {
                    chk = AddCheckLeft(panelMisc, dungName, Config.MqDungeons.Contains(dungId), ref y);
                    col = 1;
                }
                else
                {
                    chk = AddCheckRight(panelMisc, dungName, Config.MqDungeons.Contains(dungId), y);
                    NextRow(ref y);
                    col = 0;
                }
                _chkMqPerDungeon[dungId] = chk;
            }
            if (col == 1) NextRow(ref y); // If odd number, advance to next row
            // Subscribe after creating all
            _chkMqAll.CheckedChanged += (s, e) => UpdateMqAll(_chkMqAll.Checked);
            foreach (var kv in _chkMqPerDungeon)
                kv.Value.CheckedChanged += (s, e) => SyncMqAll();

            // ── Wire up Shared tab event handlers ──────────────────────────
            // These must be set up after all tabs are created
            
            // Shared songs dependencies
            _chkSharedSongSun.Enabled = Config.MmSunSong;
            _chkMmSunSong.CheckedChanged += (s, e) => { _chkSharedSongSun.Enabled = _chkMmSunSong.Checked; if (!_chkMmSunSong.Checked) _chkSharedSongSun.Checked = false; };
            _chkSharedSongElegy.Enabled = Config.OotElegy;
            _chkOotElegy.CheckedChanged += (s, e) => { _chkSharedSongElegy.Enabled = _chkOotElegy.Checked; if (!_chkOotElegy.Checked) _chkSharedSongElegy.Checked = false; };
            
            // Shared platinum token dependencies
            _chkSharedPlatinum.Enabled = Config.PlatinumTokenOot && Config.PlatinumTokenMm;
            EventHandler updateSharedPlatinum = (s, e) =>
            {
                _chkSharedPlatinum.Enabled = _chkPlatinumTokenOot.Checked && _chkPlatinumTokenMm.Checked;
                if (!_chkSharedPlatinum.Enabled) _chkSharedPlatinum.Checked = false;
            };
            _chkPlatinumTokenOot.CheckedChanged += updateSharedPlatinum;
            _chkPlatinumTokenMm.CheckedChanged  += updateSharedPlatinum;
            
            // Shared skeleton key dependencies
            _chkSharedSkeletonKey.Enabled = Config.SkeletonKeyOot && Config.SkeletonKeyMm && Config.HasMm;
            EventHandler updateSharedSK = (s, e) =>
            {
                bool both = _chkSkeletonKeyOot.Checked && _chkSkeletonKeyMm.Checked;
                _chkSharedSkeletonKey.Enabled = both;
                if (!both) _chkSharedSkeletonKey.Checked = false;
            };
            _chkSkeletonKeyOot.CheckedChanged += updateSharedSK;
            _chkSkeletonKeyMm.CheckedChanged  += updateSharedSK;
            
            // Shared souls dependencies
            _chkSharedSoulsEnemy.Enabled = Config.SoulsEnemyOot && Config.SoulsEnemyMm && Config.HasMm;
            _chkSharedSoulsNpc.Enabled = Config.SoulsNpcOot && Config.SoulsNpcMm && Config.HasMm;
            _chkSharedSoulsAnimal.Enabled = Config.SoulsAnimalOot && Config.SoulsAnimalMm && Config.HasMm;
            _chkSharedSoulsMisc.Enabled = Config.SoulsMiscOot && Config.SoulsMiscMm && Config.HasMm;
            void WireSharedSouls(CheckBox chkOot, CheckBox chkMm, CheckBox chkShared)
            {
                EventHandler upd = (s, e) =>
                {
                    bool both = chkOot.Checked && chkMm.Checked;
                    chkShared.Enabled = both;
                    if (!both) chkShared.Checked = false;
                };
                chkOot.CheckedChanged += upd;
                chkMm.CheckedChanged  += upd;
            }
            WireSharedSouls(_chkSoulsEnemyOot, _chkSoulsEnemyMm, _chkSharedSoulsEnemy);
            WireSharedSouls(_chkSoulsNpcOot, _chkSoulsNpcMm, _chkSharedSoulsNpc);
            WireSharedSouls(_chkSoulsAnimalOot, _chkSoulsAnimalMm, _chkSharedSoulsAnimal);
            WireSharedSouls(_chkSoulsMiscOot, _chkSoulsMiscMm, _chkSharedSoulsMisc);

            // ── Wire up Special Conditions dependencies ────────────────────
            // Enable/disable SC checkboxes based on whether items are in randomizer
            void UpdateScEnabled()
            {
                // Null checks to prevent errors during initialization
                if (_chkGoldSkulltulas == null || _chkMmSkulltulas == null || 
                    _chkStrayFairiesDungeons == null || _chkStrayFairyTown == null ||
                    _chkCoinsRed == null || _chkCoinsGreen == null || 
                    _chkCoinsBlue == null || _chkCoinsYellow == null ||
                    _chkTriforceHunt == null) return;
                
                // Tokens
                _scSkullsGold.Enabled  = _chkGoldSkulltulas.Checked;
                _scSkullsSwamp.Enabled = _chkMmSkulltulas.Checked;
                _scSkullsOcean.Enabled = _chkMmSkulltulas.Checked;
                if (!_scSkullsGold.Enabled)  _scSkullsGold.Checked  = false;
                if (!_scSkullsSwamp.Enabled) _scSkullsSwamp.Checked = false;
                if (!_scSkullsOcean.Enabled) _scSkullsOcean.Checked = false;
                
                // Fairies
                _scFairiesWF.Enabled = _chkStrayFairiesDungeons.Checked;
                _scFairiesSH.Enabled = _chkStrayFairiesDungeons.Checked;
                _scFairiesGB.Enabled = _chkStrayFairiesDungeons.Checked;
                _scFairiesST.Enabled = _chkStrayFairiesDungeons.Checked;
                _scFairyTown.Enabled = _chkStrayFairyTown.Checked;
                if (!_scFairiesWF.Enabled) _scFairiesWF.Checked = false;
                if (!_scFairiesSH.Enabled) _scFairiesSH.Checked = false;
                if (!_scFairiesGB.Enabled) _scFairiesGB.Checked = false;
                if (!_scFairiesST.Enabled) _scFairiesST.Checked = false;
                if (!_scFairyTown.Enabled) _scFairyTown.Checked = false;
                
                // Coins
                _scCoinsRed.Enabled    = _chkCoinsRed.Checked;
                _scCoinsGreen.Enabled  = _chkCoinsGreen.Checked;
                _scCoinsBlue.Enabled   = _chkCoinsBlue.Checked;
                _scCoinsYellow.Enabled = _chkCoinsYellow.Checked;
                if (!_scCoinsRed.Enabled)    _scCoinsRed.Checked    = false;
                if (!_scCoinsGreen.Enabled)  _scCoinsGreen.Checked  = false;
                if (!_scCoinsBlue.Enabled)   _scCoinsBlue.Checked   = false;
                if (!_scCoinsYellow.Enabled) _scCoinsYellow.Checked = false;
                
                // Triforce
                _scTriforce.Enabled = _chkTriforceHunt.Checked;
                if (!_scTriforce.Enabled) _scTriforce.Checked = false;
                
                UpdateScMax();
            }
            
            // Wire up dependencies
            _chkGoldSkulltulas.CheckedChanged       += (s, e) => UpdateScEnabled();
            _chkMmSkulltulas.CheckedChanged         += (s, e) => UpdateScEnabled();
            _chkStrayFairiesDungeons.CheckedChanged += (s, e) => UpdateScEnabled();
            _chkStrayFairyTown.CheckedChanged       += (s, e) => UpdateScEnabled();
            _chkCoinsRed.CheckedChanged             += (s, e) => UpdateScEnabled();
            _chkCoinsGreen.CheckedChanged           += (s, e) => UpdateScEnabled();
            _chkCoinsBlue.CheckedChanged            += (s, e) => UpdateScEnabled();
            _chkCoinsYellow.CheckedChanged          += (s, e) => UpdateScEnabled();
            _chkTriforceHunt.CheckedChanged         += (s, e) => UpdateScEnabled();
            
            // Initialize enabled state (now safe since all tabs are created)
            UpdateScEnabled();

            // ── Buttons ─────────────────────────────────────────────────────────
            var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            var btnOk = new Button { Text = "Apply", Size = new Size(100, 28), Location = new Point(16, 6), DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Cancel", Size = new Size(80, 28), Location = new Point(124, 6), DialogResult = DialogResult.Cancel };
            btnOk.Click += BtnOk_Click;
            btnPanel.Controls.Add(btnOk);
            btnPanel.Controls.Add(btnCancel);
            this.Controls.Add(btnPanel);

            // Apply initial state of game-dependent controls
            UpdateGameDependentControls();
            
            // Apply initial state of SR Pouches based on MQ settings
            UpdateSrPouchEnabled();
        }

        private static readonly string[] _ootKrIds = { "forest_temple","fire_temple","water_temple","shadow_temple","spirit_temple","botw","gtg","ganons_castle","thieves_hideout","tcg" };
        private static readonly string[] _mmKrIds  = { "woodfall","snowhead","great_bay","stone_tower" };
        private bool _updatingKeyRings = false;

        private void UpdateMqAll(bool check)
        {
            if (_updatingKeyRings) return;
            _updatingKeyRings = true;
            foreach (var kv in _chkMqPerDungeon) kv.Value.Checked = check;
            _updatingKeyRings = false;
            UpdateSrPouchEnabled(); // Update SR Pouches when MQ changes
        }

        private void SyncMqAll()
        {
            if (_updatingKeyRings) return;
            _updatingKeyRings = true;
            bool all = _chkMqPerDungeon.Values.All(c => c.Checked);
            if (_chkMqAll != null) _chkMqAll.Checked = all;
            _updatingKeyRings = false;
            UpdateSrPouchEnabled(); // Update SR Pouches when MQ changes
        }

        private void UpdateSrPouchEnabled()
        {
            // Update enabled/disabled state of SR Pouches based on MQ settings
            // Vanilla packs enabled when dungeon is NOT MQ, MQ packs enabled when dungeon IS MQ
            var mqDungeonMap = new Dictionary<string, string>
            {
                {"dodongo_mq_staircase", "dodongo"},
                {"shadow_temple_scythe", "shadow_temple"}, {"shadow_temple_mq_scythe", "shadow_temple"},
                {"shadow_temple_pit", "shadow_temple"}, {"shadow_temple_mq_pit", "shadow_temple"},
                {"shadow_temple_spikes", "shadow_temple"}, {"shadow_temple_mq_spikes", "shadow_temple"},
                {"shadow_temple_mq_blades", "shadow_temple"},
                {"spirit_temple_child", "spirit_temple"}, {"spirit_temple_mq_lobby", "spirit_temple"},
                {"spirit_temple_sun", "spirit_temple"}, {"spirit_temple_mq_adult", "spirit_temple"},
                {"spirit_temple_boulders", "spirit_temple"},
                {"ice_cavern_scythe", "ice_cavern"}, {"ice_cavern_block", "ice_cavern"},
                {"botw_basement", "botw"},
                {"gtg_slopes", "gtg"}, {"gtg_mq_slopes", "gtg"},
                {"gtg_lava", "gtg"}, {"gtg_mq_lava", "gtg"},
                {"gtg_water", "gtg"}, {"gtg_mq_water", "gtg"},
                {"ganons_castle_spirit", "ganons_castle"}, {"ganons_castle_mq_fire", "ganons_castle"},
                {"ganons_castle_light", "ganons_castle"}, {"ganons_castle_mq_shadow", "ganons_castle"},
                {"ganons_castle_fire", "ganons_castle"}, {"ganons_castle_mq_water", "ganons_castle"},
                {"ganons_castle_forest", "ganons_castle"},
            };

            var mqPacks = new HashSet<string>
            {
                "dodongo_mq_staircase",
                "shadow_temple_mq_scythe", "shadow_temple_mq_blades", "shadow_temple_mq_pit", "shadow_temple_mq_spikes",
                "spirit_temple_mq_lobby", "spirit_temple_mq_adult",
                "gtg_mq_slopes", "gtg_mq_lava", "gtg_mq_water",
                "ganons_castle_mq_fire", "ganons_castle_mq_shadow", "ganons_castle_mq_water",
            };

            foreach (var kv in _chkSrPouchPerPack)
            {
                if (mqDungeonMap.TryGetValue(kv.Key, out var dungId))
                {
                    bool dungIsMq = _chkMqPerDungeon.TryGetValue(dungId, out var mqChk) && mqChk.Checked;
                    bool isMqPack = mqPacks.Contains(kv.Key);
                    kv.Value.Enabled = isMqPack ? dungIsMq : !dungIsMq;
                    // Don't reset Checked state - preserve user's selection even when disabled
                }
            }
        }

        private void UpdateSrPouchAll(bool check)        {
            if (_updatingKeyRings) return;
            _updatingKeyRings = true;
            foreach (var kv in _chkSrPouchPerPack)
                kv.Value.Checked = check;
            _updatingKeyRings = false;
        }

        private void SyncSrPouchAll()
        {
            if (_updatingKeyRings) return;
            _updatingKeyRings = true;
            bool all = _chkSrPouchPerPack.Values.All(c => c.Checked);
            if (_chkSrPouchAll != null) _chkSrPouchAll.Checked = all;
            _updatingKeyRings = false;
        }

        private void UpdateKeyRingAll(bool isOot, bool check)
        {
            if (_updatingKeyRings) return;
            _updatingKeyRings = true;
            var ids = isOot ? _ootKrIds : _mmKrIds;
            foreach (var id in ids)
                if (_chkKeyRingPerDungeon.TryGetValue(id, out var chk))
                    chk.Checked = check;
            _updatingKeyRings = false;
        }

        private void SyncKeyRingAllCheckboxes()
        {
            if (_updatingKeyRings) return;
            _updatingKeyRings = true;
            bool allOot = _ootKrIds.All(id => _chkKeyRingPerDungeon.TryGetValue(id, out var c) && c.Checked);
            bool allMm  = _mmKrIds.All(id  => _chkKeyRingPerDungeon.TryGetValue(id, out var c) && c.Checked);
            if (_chkKeyRingsOot != null) _chkKeyRingsOot.Checked = allOot;
            if (_chkKeyRingsMm  != null) _chkKeyRingsMm.Checked  = allMm;
            _updatingKeyRings = false;
        }

        private void UpdateScMax()        {
            if (_numScAmount == null) return;
            int max = 0;
            if (_scStones?.Checked     == true) max += 3;
            if (_scMedallions?.Checked == true) max += 6;
            if (_scRemains?.Checked    == true) max += 4;
            if (_scSkullsGold?.Checked  == true) max += 100;
            if (_scSkullsSwamp?.Checked == true) max += 30;
            if (_scSkullsOcean?.Checked == true) max += 30;
            if (_scFairiesWF?.Checked  == true) max += 15;
            if (_scFairiesSH?.Checked  == true) max += 15;
            if (_scFairiesGB?.Checked  == true) max += 15;
            if (_scFairiesST?.Checked  == true) max += 15;
            if (_scFairyTown?.Checked  == true) max += 1;
            
            // Masks — calculate with shared mask deduplication
            int regularMasks = 0;
            int transformMasks = 0;
            int ootMasks = 0;
            
            if (_scMasksRegular?.Checked == true) 
            {
                regularMasks = 20; // ~20 regular MM masks
                
                // Subtract shared regular masks (duplicates between OoT and MM)
                // Base shared: Keaton, Bunny, Truth = 3
                int sharedRegular = 0;
                if (_chkSharedMaskKeaton?.Checked == true) sharedRegular++;
                if (_chkSharedMaskBunny?.Checked == true) sharedRegular++;
                if (_chkSharedMaskTruth?.Checked == true) sharedRegular++;
                // Extra shared masks if enabled
                if (_chkOotBlastMask?.Checked == true && _chkSharedMaskBlast?.Checked == true) sharedRegular++;
                if (_chkOotStoneMask?.Checked == true && _chkSharedMaskStone?.Checked == true) sharedRegular++;
                
                regularMasks -= sharedRegular;
            }
            
            if (_scMasksTransform?.Checked == true) 
            {
                transformMasks = 4; // 4 transform masks
                
                // Subtract shared transform masks (duplicates between OoT and MM)
                // Goron and Zora = 2
                int sharedTransform = 0;
                if (_chkSharedMaskGoron?.Checked == true) sharedTransform++;
                if (_chkSharedMaskZora?.Checked == true) sharedTransform++;
                
                transformMasks -= sharedTransform;
            }
            
            if (_scMasksOot?.Checked == true) 
            {
                // Base OoT masks: 8 (Keaton, Skull, Spooky, Bunny, Goron, Zora, Gerudo, Truth)
                ootMasks = 8;
                // Add extra masks if enabled
                if (_chkOotBlastMask?.Checked == true) ootMasks++;
                if (_chkOotStoneMask?.Checked == true) ootMasks++;
            }
            
            max += regularMasks + transformMasks + ootMasks;
            
            // Coins — take from collectibles numeric fields
            if (_scCoinsRed?.Checked    == true) max += (int)(_numCoinsRed?.Value    ?? 100);
            if (_scCoinsGreen?.Checked  == true) max += (int)(_numCoinsGreen?.Value  ?? 100);
            if (_scCoinsBlue?.Checked   == true) max += (int)(_numCoinsBlue?.Value   ?? 100);
            if (_scCoinsYellow?.Checked == true) max += (int)(_numCoinsYellow?.Value ?? 100);
            if (_scTriforce?.Checked    == true) max += (int)(_numTriforceHuntGoal?.Value ?? 20);

            // If nothing is checked (max = 0), disable amount field (key will always be lit)
            if (max == 0)
            {
                _numScAmount.Enabled = false;
                _numScAmount.Value = 0;
            }
            else
            {
                _numScAmount.Enabled = true;
                if (max < 1) max = 1;
                _numScAmount.Maximum = max;
                if (_numScAmount.Value > max) _numScAmount.Value = max;
                // Don't force Value to 1 if user wants 0 (0 means key is always lit)
            }
        }

        private void UpdateGameDependentControls()
        {
            bool hasOot = _chkGamesOot.Checked;
            bool hasMm  = _chkGamesMm.Checked;
            bool both   = hasOot && hasMm;

            // ── Tabs ────────────────────────────────────────────────────────
            // OoT tab — disable if only MM
            if (_tabOot != null) _tabOot.Enabled = hasOot;
            
            // MM tab — disable if only OoT
            if (_tabMm != null) _tabMm.Enabled = hasMm;
            
            // Shared tabs — disable if only one game
            if (_tabSharedEq   != null) _tabSharedEq.Enabled   = both;
            if (_tabSharedMask != null) _tabSharedMask.Enabled  = both;
            // Songs tab is ALWAYS enabled (individual controls are managed below)
            // if (_tabSongs != null) _tabSongs.Enabled = both; // DON'T disable Songs tab

            // ── Songs tab ───────────────────────────────────────────────────
            // OoT-additional songs — disable if only MM
            if (_chkOotElegy != null) { _chkOotElegy.Enabled = hasOot; if (!hasOot) _chkOotElegy.Checked = false; }
            if (_chkFreeScarecrowOot != null) { _chkFreeScarecrowOot.Enabled = hasOot; if (!hasOot) _chkFreeScarecrowOot.Checked = false; }
            
            // MM-additional songs — disable if only OoT
            if (_chkMmSunSong       != null) { _chkMmSunSong.Enabled       = hasMm; if (!hasMm) _chkMmSunSong.Checked       = false; }
            if (_chkMmFairyOcarina  != null) { _chkMmFairyOcarina.Enabled  = hasMm; if (!hasMm) _chkMmFairyOcarina.Checked  = false; }
            if (_chkProgressiveGoron != null) { _chkProgressiveGoron.Enabled = hasMm; if (!hasMm) _chkProgressiveGoron.Checked = false; }
            if (_chkFreeScarecrowMm != null) { _chkFreeScarecrowMm.Enabled = hasMm; if (!hasMm) _chkFreeScarecrowMm.Checked = false; }
            
            // Ocarina buttons
            if (_chkOotOcarinaButtons != null) { _chkOotOcarinaButtons.Enabled = hasOot; if (!hasOot) _chkOotOcarinaButtons.Checked = false; }
            if (_chkMmOcarinaButtons  != null) { _chkMmOcarinaButtons.Enabled  = hasMm;  if (!hasMm)  _chkMmOcarinaButtons.Checked  = false; }
            // Notes Mode (_chkSongsAsNotes) is ALWAYS enabled

            // ── Misc tab ────────────────────────────────────────────────────
            // Trade Quest (OoT) — disable if only MM
            if (_chkOotSkipZelda    != null) { _chkOotSkipZelda.Enabled    = hasOot; if (!hasOot) _chkOotSkipZelda.Checked    = false; }
            if (_chkOotOpenKakariko != null) { _chkOotOpenKakariko.Enabled = hasOot; if (!hasOot) _chkOotOpenKakariko.Checked = false; }
            if (_chkOotEggShuffle   != null) { _chkOotEggShuffle.Enabled   = hasOot; if (!hasOot) _chkOotEggShuffle.Checked   = false; }

            // ── Dungeons tab ────────────────────────────────────────────────
            // Key Rings
            if (_chkKeyRingsOot != null) { _chkKeyRingsOot.Enabled = hasOot; if (!hasOot) _chkKeyRingsOot.Checked = false; }
            if (_chkKeyRingsMm  != null) { _chkKeyRingsMm.Enabled  = hasMm;  if (!hasMm)  _chkKeyRingsMm.Checked  = false; }
            
            // Key Rings OoT dungeons
            foreach (var id in new[]{"forest_temple","fire_temple","water_temple","shadow_temple",
                                     "spirit_temple","botw","gtg","ganons_castle","thieves_hideout","tcg"})
                if (_chkKeyRingPerDungeon.TryGetValue(id, out var chk))
                    { chk.Enabled = hasOot; if (!hasOot) chk.Checked = false; }
            
            // Key Rings MM dungeons
            foreach (var id in new[]{"woodfall","snowhead","great_bay","stone_tower"})
                if (_chkKeyRingPerDungeon.TryGetValue(id, out var chk))
                    { chk.Enabled = hasMm; if (!hasMm) chk.Checked = false; }
            
            // Ganon's Key — disable if only MM (OoT-specific)
            if (_chkGanonBkAnywhere != null) { _chkGanonBkAnywhere.Enabled = hasOot; if (!hasOot) _chkGanonBkAnywhere.Checked = false; }
            if (_chkGanonBkCustom   != null) { _chkGanonBkCustom.Enabled   = hasOot; if (!hasOot) _chkGanonBkCustom.Checked   = false; }
            
            // Special Conditions — ALL disabled if only MM (Ganon BK is OoT-specific item)
            // If OoT is enabled, enable based on specific game requirements
            if (_scStones     != null) { _scStones.Enabled     = hasOot; if (!hasOot) _scStones.Checked     = false; }
            if (_scMedallions != null) { _scMedallions.Enabled = hasOot; if (!hasOot) _scMedallions.Checked = false; }
            if (_scRemains    != null) { _scRemains.Enabled    = hasOot && hasMm; if (!hasOot || !hasMm) _scRemains.Checked = false; }
            if (_scSkullsGold  != null) { _scSkullsGold.Enabled  = hasOot; if (!hasOot) _scSkullsGold.Checked  = false; }
            if (_scSkullsSwamp != null) { _scSkullsSwamp.Enabled = hasOot && hasMm; if (!hasOot || !hasMm) _scSkullsSwamp.Checked = false; }
            if (_scSkullsOcean != null) { _scSkullsOcean.Enabled = hasOot && hasMm; if (!hasOot || !hasMm) _scSkullsOcean.Checked = false; }
            if (_scFairiesWF != null) { _scFairiesWF.Enabled = hasOot && hasMm; if (!hasOot || !hasMm) _scFairiesWF.Checked = false; }
            if (_scFairiesSH != null) { _scFairiesSH.Enabled = hasOot && hasMm; if (!hasOot || !hasMm) _scFairiesSH.Checked = false; }
            if (_scFairiesGB != null) { _scFairiesGB.Enabled = hasOot && hasMm; if (!hasOot || !hasMm) _scFairiesGB.Checked = false; }
            if (_scFairiesST != null) { _scFairiesST.Enabled = hasOot && hasMm; if (!hasOot || !hasMm) _scFairiesST.Checked = false; }
            if (_scFairyTown != null) { _scFairyTown.Enabled = hasOot && hasMm; if (!hasOot || !hasMm) _scFairyTown.Checked = false; }
            if (_scMasksRegular   != null) { _scMasksRegular.Enabled   = hasOot && hasMm; if (!hasOot || !hasMm) _scMasksRegular.Checked   = false; }
            if (_scMasksTransform != null) { _scMasksTransform.Enabled = hasOot && hasMm; if (!hasOot || !hasMm) _scMasksTransform.Checked = false; }
            if (_scMasksOot != null) { _scMasksOot.Enabled = hasOot; if (!hasOot) _scMasksOot.Checked = false; }
            if (_scCoinsRed    != null) { _scCoinsRed.Enabled    = hasOot; if (!hasOot) _scCoinsRed.Checked    = false; }
            if (_scCoinsGreen  != null) { _scCoinsGreen.Enabled  = hasOot; if (!hasOot) _scCoinsGreen.Checked  = false; }
            if (_scCoinsBlue   != null) { _scCoinsBlue.Enabled   = hasOot; if (!hasOot) _scCoinsBlue.Checked   = false; }
            if (_scCoinsYellow != null) { _scCoinsYellow.Enabled = hasOot; if (!hasOot) _scCoinsYellow.Checked = false; }
            if (_scTriforce    != null) { _scTriforce.Enabled    = hasOot; if (!hasOot) _scTriforce.Checked    = false; }
            
            // SC Amount field — disable if only MM
            if (_numScAmount != null) { _numScAmount.Enabled = hasOot; if (!hasOot) _numScAmount.Value = 0; }

            // ── Shared items ────────────────────────────────────────────────
            // If only one game — uncheck all Shared checkboxes
            if (!both)
            {
                foreach (var chk in new[] {
                    _chkSharedHealth, _chkSharedSwords, _chkSharedShields, _chkSharedStrength,
                    _chkSharedScales, _chkSharedWallets, _chkSharedMagic, _chkSharedTunicGoron,
                    _chkSharedTunicZora, _chkSharedBootsIron, _chkSharedBootsHover,
                    _chkSharedHookshot, _chkSharedBows, _chkSharedBombBags, _chkSharedOcarina,
                    _chkSharedHammer, _chkSharedBottles, _chkSharedNutsSticks, _chkSharedBombchu,
                    _chkSharedFireArrows, _chkSharedIceArrows, _chkSharedLightArrows, _chkSharedLens,
                    _chkSharedSpellFire, _chkSharedSpellWind, _chkSharedSpellLove,
                    _chkSharedStoneAgony, _chkSharedSpinUpgrade,
                    _chkSharedMaskGoron, _chkSharedMaskZora, _chkSharedMaskBunny,
                    _chkSharedMaskKeaton, _chkSharedMaskTruth, _chkSharedMaskBlast, _chkSharedMaskStone,
                    _chkSharedSongEpona, _chkSharedSongStorms, _chkSharedSongTime,
                    _chkSharedSongSun, _chkSharedSongElegy, _chkSharedScarecrow, _chkSharedOcarina,
                    _chkSharedOcarinaButtons,
                })
                {
                    if (chk != null) chk.Checked = false;
                }
            }

            // ── Collectibles ────────────────────────────────────────────────
            // Gold Skulltulas → OoT, others → MM
            if (_chkGoldSkulltulas  != null) { _chkGoldSkulltulas.Enabled  = hasOot; if (!hasOot) _chkGoldSkulltulas.Checked  = false; }
            if (_chkMmSkulltulas    != null) { _chkMmSkulltulas.Enabled    = hasMm;  if (!hasMm)  _chkMmSkulltulas.Checked    = false; }
            if (_chkPlatinumTokenOot!= null) { _chkPlatinumTokenOot.Enabled = hasOot; if (!hasOot) _chkPlatinumTokenOot.Checked = false; }
            if (_chkPlatinumTokenMm != null) { _chkPlatinumTokenMm.Enabled  = hasMm;  if (!hasMm)  _chkPlatinumTokenMm.Checked  = false; }
            if (_chkStrayFairiesDungeons != null) { _chkStrayFairiesDungeons.Enabled = hasMm; if (!hasMm) _chkStrayFairiesDungeons.Checked = false; }
            if (_chkStrayFairyTown  != null) { _chkStrayFairyTown.Enabled  = hasMm;  if (!hasMm)  _chkStrayFairyTown.Checked  = false; }
            if (_chkTranscendentFairy != null) { _chkTranscendentFairy.Enabled = hasMm; if (!hasMm) _chkTranscendentFairy.Checked = false; }

            // MM-specific in MM tab
            if (_chkClocksEnabled != null) { _chkClocksEnabled.Enabled = hasMm; if (!hasMm) _chkClocksEnabled.Checked = false; }
            if (_chkOwlShuffle    != null) { _chkOwlShuffle.Enabled    = hasMm; if (!hasMm) _chkOwlShuffle.Checked    = false; }
            if (_chkTingleMaps    != null) { _chkTingleMaps.Enabled    = hasMm; if (!hasMm) _chkTingleMaps.Checked    = false; }

            // ── Dungeon keys ────────────────────────────────────────────────
            if (_chkSmallKeysMm  != null) { _chkSmallKeysMm.Enabled  = hasMm;  if (!hasMm)  _chkSmallKeysMm.Checked  = false; }
            if (_chkBossKeysMm   != null) { _chkBossKeysMm.Enabled   = hasMm;  if (!hasMm)  _chkBossKeysMm.Checked   = false; }
            if (_chkSmallKeysOot != null) { _chkSmallKeysOot.Enabled = hasOot; if (!hasOot) _chkSmallKeysOot.Checked = false; }
            if (_chkBossKeysOot  != null) { _chkBossKeysOot.Enabled  = hasOot; if (!hasOot) _chkBossKeysOot.Checked  = false; }
            if (_chkSkeletonKeyOot != null) { _chkSkeletonKeyOot.Enabled = hasOot; if (!hasOot) _chkSkeletonKeyOot.Checked = false; }
            if (_chkSkeletonKeyMm  != null) { _chkSkeletonKeyMm.Enabled  = hasMm;  if (!hasMm)  _chkSkeletonKeyMm.Checked  = false; }
            if (_chkSharedSkeletonKey != null)
            {
                bool bothSK = (_chkSkeletonKeyOot?.Checked ?? false) && (_chkSkeletonKeyMm?.Checked ?? false);
                _chkSharedSkeletonKey.Enabled = bothSK;
                if (!bothSK) _chkSharedSkeletonKey.Checked = false;
            }

            // OoT-specific dungeons and SR — unavailable if OoT not in rando
            void DisableOot(CheckBox? chk) { if (chk != null) { chk.Enabled = hasOot; if (!hasOot) chk.Checked = false; } }

            DisableOot(_chkKeysanity);
            DisableOot(_chkSmallKeysHideout);
            DisableOot(_chkSmallKeysTcg);
            DisableOot(_chkSilverRupees);
            DisableOot(_chkMagicalRupee);
            DisableOot(_chkSrPouchAll);
            DisableOot(_chkMqAll);

            // SR packs — all OoT dungeons
            foreach (var kv in _chkSrPouchPerPack)
                { kv.Value.Enabled = hasOot; if (!hasOot) kv.Value.Checked = false; }

            // MQ checkboxes
            foreach (var kv in _chkMqPerDungeon)
                { kv.Value.Enabled = hasOot; if (!hasOot) kv.Value.Checked = false; }

            // Keysanity also depends on SmallKeysOot
            if (_chkKeysanity != null && hasOot)
            {
                _chkKeysanity.Enabled = _chkSmallKeysOot?.Checked ?? false;
                if (!(_chkSmallKeysOot?.Checked ?? false)) _chkKeysanity.Checked = false;
            }

            // ── Souls ───────────────────────────────────────────────────────
            void DisableSoul(CheckBox? chk, bool enabled) { if (chk != null) { chk.Enabled = enabled; if (!enabled) chk.Checked = false; } }
            DisableSoul(_chkSoulsBossOot,      hasOot);
            DisableSoul(_chkSoulsBossMm,       hasMm);
            DisableSoul(_chkSoulsEnemyOot,     hasOot);
            DisableSoul(_chkSoulsEnemyMm,      hasMm);
            DisableSoul(_chkSoulsNpcOot,       hasOot);
            DisableSoul(_chkSoulsNpcMm,        hasMm);
            DisableSoul(_chkSoulsAnimalOot,    hasOot);
            DisableSoul(_chkSoulsAnimalMm,     hasMm);
            DisableSoul(_chkSoulsMiscOot,      hasOot);
            DisableSoul(_chkSoulsMiscMm,       hasMm);
            // Shared souls — only if both enabled
            foreach (var (chkOot, chkMm, chkSh) in new[]{
                (_chkSoulsEnemyOot,  _chkSoulsEnemyMm,  _chkSharedSoulsEnemy),
                (_chkSoulsNpcOot,    _chkSoulsNpcMm,    _chkSharedSoulsNpc),
                (_chkSoulsAnimalOot, _chkSoulsAnimalMm, _chkSharedSoulsAnimal),
                (_chkSoulsMiscOot,   _chkSoulsMiscMm,   _chkSharedSoulsMisc),
            })
            {
                if (chkSh == null) continue;
                bool bothSouls = (chkOot?.Checked ?? false) && (chkMm?.Checked ?? false);
                chkSh.Enabled = bothSouls;
                if (!bothSouls) chkSh.Checked = false;
            }
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            Config.Games = (_chkGamesOot.Checked && _chkGamesMm.Checked) ? "ootmm"
                         : _chkGamesOot.Checked ? "oot"
                         : "mm";
            Config.OotExtraChildSwords = _chkOotExtraSwords.Checked;
            Config.OotSpinUpgrade      = _chkOotSpinUpgrade.Checked;
            Config.OotBronzeScale      = _chkOotBronzeScale.Checked;
            Config.OotPreplantedBeans  = _chkOotPreplantedBeans.Checked;
            Config.MmStrength          = _chkMmStrength.Checked;
            Config.MmScales            = _chkMmScales.Checked;
            Config.MmStoneAgony        = _chkMmStoneAgony.Checked;
            Config.MmGoronTunic        = _chkMmGoronTunic.Checked;
            Config.MmZoraTunic         = _chkMmZoraTunic.Checked;
            Config.MmIronBoots         = _chkMmIronBoots.Checked;
            Config.MmHoverBoots        = _chkMmHoverBoots.Checked;
            Config.MmDekuShield        = _chkMmDekuShield.Checked;
            Config.MmSticksNuts        = _chkMmSticksNuts.Checked;
            Config.MmShortHookshot     = _chkMmShortHookshot.Checked;
            Config.MmHammer            = _chkMmHammer.Checked;
            Config.MmSpellFire         = _chkMmSpellFire.Checked;
            Config.MmSpellWind         = _chkMmSpellWind.Checked;
            Config.MmSpellLove         = _chkMmSpellLove.Checked;
            Config.MmSunSong           = _chkMmSunSong.Checked;
            Config.MmFairyOcarina      = _chkMmFairyOcarina.Checked;
            Config.OotElegy            = _chkOotElegy.Checked;
            Config.ProgressiveGoronLullaby = _chkProgressiveGoron.Checked;
            Config.FreeScarecrowOot    = _chkFreeScarecrowOot.Checked;
            Config.FreeScarecrowMm     = _chkFreeScarecrowMm.Checked;
            Config.SharedSongEpona     = _chkSharedSongEpona.Checked;
            Config.SharedSongStorms    = _chkSharedSongStorms.Checked;
            Config.SharedSongTime      = _chkSharedSongTime.Checked;
            Config.SharedSongSun       = _chkSharedSongSun.Checked;
            Config.SharedSongElegy     = _chkSharedSongElegy.Checked;
            Config.SharedScarecrow     = _chkSharedScarecrow.Checked;
            Config.SharedOcarina       = _chkSharedOcarina.Checked;
            Config.OotOcarinaButtons   = _chkOotOcarinaButtons.Checked;
            Config.MmOcarinaButtons    = _chkMmOcarinaButtons.Checked;
            Config.SharedOcarinaButtons = _chkSharedOcarinaButtons.Checked;
            Config.SongsAsNotes        = _chkSongsAsNotes.Checked;
            Config.MmFairyFountain     = false;
            Config.ClocksEnabled       = _chkClocksEnabled.Checked;
            Config.OwlShuffle          = _chkOwlShuffle.Checked;
            Config.TingleMaps          = _chkTingleMaps.Checked;
            Config.BombchuBehaviorOot  = _chkBombchuOotBag.Checked ? "bag" : _chkBombchuOotBombBag.Checked ? "bombBag" : "toggle";
            Config.BombchuBehaviorMm   = _chkBombchuMmBag.Checked  ? "bag" : _chkBombchuMmBombBag.Checked  ? "bombBag" : "toggle";
            // If modes match — Bombchu shared automatically
            Config.SharedBombchu = Config.BombchuBehaviorOot == Config.BombchuBehaviorMm;
            Config.ChildWallet         = _chkChildWallet.Checked;
            Config.ColossalWallet      = _chkColossalWallet.Checked;
            Config.BottomlessWallet    = _chkBottomlessWallet.Checked;
            Config.SharedHealth        = _chkSharedHealth.Checked;
            Config.SharedSwords        = _chkSharedSwords.Checked;
            Config.SharedShields       = _chkSharedShields.Checked;
            Config.SharedStrength      = _chkSharedStrength.Checked;
            Config.SharedScales        = _chkSharedScales.Checked;
            Config.SharedWallets       = _chkSharedWallets.Checked;
            Config.SharedMagic         = _chkSharedMagic.Checked;
            Config.SharedTunicGoron    = _chkSharedTunicGoron.Checked;
            Config.SharedTunicZora     = _chkSharedTunicZora.Checked;
            Config.SharedBootsIron     = _chkSharedBootsIron.Checked;
            Config.SharedBootsHover    = _chkSharedBootsHover.Checked;
            Config.SharedHookshot      = _chkSharedHookshot.Checked;
            Config.SharedBows          = _chkSharedBows.Checked;
            Config.SharedBombBags      = _chkSharedBombBags.Checked;
            Config.SharedOcarina       = _chkSharedOcarina.Checked;
            Config.SharedHammer        = _chkSharedHammer.Checked;
            Config.SharedBottles       = _chkSharedBottles.Checked;
            Config.SharedNutsSticks    = _chkSharedNutsSticks.Checked;
            Config.SharedBombchu       = _chkSharedBombchu.Checked;
            Config.SharedFireArrows    = _chkSharedFireArrows.Checked;
            Config.SharedIceArrows     = _chkSharedIceArrows.Checked;
            Config.SharedLightArrows   = _chkSharedLightArrows.Checked;
            Config.SharedLens          = _chkSharedLens.Checked;
            Config.SharedSpellFire     = _chkSharedSpellFire.Checked;
            Config.SharedSpellWind     = _chkSharedSpellWind.Checked;
            Config.SharedSpellLove     = _chkSharedSpellLove.Checked;
            Config.SharedStoneAgony    = _chkSharedStoneAgony.Checked;
            Config.SharedSpinUpgrade   = _chkSharedSpinUpgrade.Checked;
            Config.OotBlastMask        = _chkOotBlastMask.Checked;
            Config.OotStoneMask        = _chkOotStoneMask.Checked;
            Config.SharedMaskGoron     = _chkSharedMaskGoron.Checked;
            Config.SharedMaskZora      = _chkSharedMaskZora.Checked;
            Config.SharedMaskBunny     = _chkSharedMaskBunny.Checked;
            Config.SharedMaskKeaton    = _chkSharedMaskKeaton.Checked;
            Config.SharedMaskTruth     = _chkSharedMaskTruth.Checked;
            Config.SharedMaskBlast     = _chkSharedMaskBlast.Checked;
            Config.SharedMaskStone     = _chkSharedMaskStone.Checked;
            Config.GoldSkulltulas      = _chkGoldSkulltulas.Checked;
            Config.MmSkulltulas        = _chkMmSkulltulas.Checked;
            Config.PlatinumTokenOot    = _chkPlatinumTokenOot.Checked;
            Config.PlatinumTokenMm     = _chkPlatinumTokenMm.Checked;
            Config.SharedPlatinumToken = _chkSharedPlatinum.Checked;
            Config.StrayFairiesDungeons = _chkStrayFairiesDungeons.Checked;
            Config.StrayFairyTown       = _chkStrayFairyTown.Checked;
            Config.TranscendentFairy    = _chkTranscendentFairy.Checked;
            Config.CoinsRed    = _chkCoinsRed.Checked;    Config.CoinsRedMax    = (int)_numCoinsRed.Value;
            Config.CoinsGreen  = _chkCoinsGreen.Checked;  Config.CoinsGreenMax  = (int)_numCoinsGreen.Value;
            Config.CoinsBlue   = _chkCoinsBlue.Checked;   Config.CoinsBlueMax   = (int)_numCoinsBlue.Value;
            Config.CoinsYellow = _chkCoinsYellow.Checked; Config.CoinsYellowMax = (int)_numCoinsYellow.Value;
            Config.TriforceMode     = _chkTriforceQuest.Checked ? "quest" : _chkTriforceHunt.Checked ? "hunt" : "none";
            Config.TriforceHuntGoal = (int)_numTriforceHuntGoal.Value;
            Config.OotSkipZelda    = _chkOotSkipZelda.Checked;
            Config.OotOpenKakariko = _chkOotOpenKakariko.Checked;
            Config.OotEggShuffle   = _chkOotEggShuffle.Checked;
            Config.MapsCompasses   = _chkMapsCompasses.Checked;
            Config.SmallKeysOot    = _chkSmallKeysOot.Checked;
            Config.SmallKeysMm     = _chkSmallKeysMm.Checked;
            Config.BossKeysOot     = _chkBossKeysOot.Checked;
            Config.BossKeysMm      = _chkBossKeysMm.Checked;
            Config.Keysanity       = _chkKeysanity.Checked;
            Config.SmallKeysHideout      = _chkSmallKeysHideout.Checked;
            Config.SmallKeysTcg          = _chkSmallKeysTcg.Checked;
            Config.SilverRupees          = _chkSilverRupees.Checked;
            Config.SkeletonKeyOot        = _chkSkeletonKeyOot.Checked;
            Config.SkeletonKeyMm         = _chkSkeletonKeyMm.Checked;
            Config.SharedSkeletonKey     = _chkSharedSkeletonKey.Checked;
            Config.MagicalRupee          = _chkMagicalRupee.Checked;
            Config.SoulsBossOot      = _chkSoulsBossOot.Checked;
            Config.SoulsBossMm       = _chkSoulsBossMm.Checked;
            Config.SoulsEnemyOot     = _chkSoulsEnemyOot.Checked;
            Config.SoulsEnemyMm      = _chkSoulsEnemyMm.Checked;
            Config.SharedSoulsEnemy  = _chkSharedSoulsEnemy.Checked;
            Config.SoulsNpcOot       = _chkSoulsNpcOot.Checked;
            Config.SoulsNpcMm        = _chkSoulsNpcMm.Checked;
            Config.SharedSoulsNpc    = _chkSharedSoulsNpc.Checked;
            Config.SoulsAnimalOot    = _chkSoulsAnimalOot.Checked;
            Config.SoulsAnimalMm     = _chkSoulsAnimalMm.Checked;
            Config.SharedSoulsAnimal = _chkSharedSoulsAnimal.Checked;
            Config.SoulsMiscOot      = _chkSoulsMiscOot.Checked;
            Config.SoulsMiscMm       = _chkSoulsMiscMm.Checked;
            Config.SharedSoulsMisc   = _chkSharedSoulsMisc.Checked;
            Config.SrPouchPacks          = new List<string>(_chkSrPouchPerPack.Where(kv => kv.Value.Checked).Select(kv => kv.Key));
            Config.MqDungeons            = new List<string>(_chkMqPerDungeon.Where(kv => kv.Value.Checked).Select(kv => kv.Key));
            Config.KeyRingsOot           = _chkKeyRingsOot.Checked;
            Config.KeyRingsMm            = _chkKeyRingsMm.Checked;
            Config.KeyRingDungeons       = new List<string>(_chkKeyRingPerDungeon.Where(kv => kv.Value.Checked).Select(kv => kv.Key));
            Config.GanonBossKey = _chkGanonBkCustom.Checked  ? "custom"
                                : _chkGanonBkAnywhere.Checked ? "anywhere"
                                : "vanilla"; // vanilla if nothing selected
            // Special Conditions
            Config.GanonBkRequired = (int)_numScAmount.Value;
            Config.GanonBk = new GanonBkConditions
            {
                Count          = (int)_numScAmount.Value,
                Stones         = _scStones.Checked,
                Medallions     = _scMedallions.Checked,
                Remains        = _scRemains.Checked,
                SkullsGold     = _scSkullsGold.Checked,
                SkullsSwamp    = _scSkullsSwamp.Checked,
                SkullsOcean    = _scSkullsOcean.Checked,
                FairiesWF      = _scFairiesWF.Checked,
                FairiesSH      = _scFairiesSH.Checked,
                FairiesGB      = _scFairiesGB.Checked,
                FairiesST      = _scFairiesST.Checked,
                FairyTown      = _scFairyTown.Checked,
                MasksRegular   = _scMasksRegular.Checked,
                MasksTransform = _scMasksTransform.Checked,
                MasksOot       = _scMasksOot.Checked,
                CoinsRed       = _scCoinsRed.Checked,
                CoinsGreen     = _scCoinsGreen.Checked,
                CoinsBlue      = _scCoinsBlue.Checked,
                CoinsYellow    = _scCoinsYellow.Checked,
                Triforce       = _scTriforce.Checked,
            };
        }

        private static Panel MakeScrollPanel()
        {
            var p = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            return p;
        }

        private static void AddSection(Panel panel, string text, ref int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(8, y),
                AutoSize = true
            };
            panel.Controls.Add(lbl);
            y += 22;
        }

        private static CheckBox AddCheck(Panel panel, string text, bool value, ref int y)
        {
            var chk = new CheckBox
            {
                Text = text,
                Checked = value,
                Location = new Point(16, y),
                AutoSize = true
            };
            panel.Controls.Add(chk);
            y += 22;
            return chk;
        }

        // Add checkbox in two-column layout (left column)
        private static CheckBox AddCheckLeft(Panel panel, string text, bool value, ref int y)
        {
            var chk = new CheckBox
            {
                Text = text,
                Checked = value,
                Location = new Point(16, y),
                AutoSize = true,
                MaximumSize = new Size(360, 0) // Limit width for wrapping
            };
            panel.Controls.Add(chk);
            return chk;
        }

        // Add checkbox in two-column layout (right column)
        private static CheckBox AddCheckRight(Panel panel, string text, bool value, int y)
        {
            var chk = new CheckBox
            {
                Text = text,
                Checked = value,
                Location = new Point(390, y),
                AutoSize = true,
                MaximumSize = new Size(360, 0) // Limit width for wrapping
            };
            panel.Controls.Add(chk);
            return chk;
        }

        // Add checkbox spanning full width (for long text)
        private static CheckBox AddCheckFull(Panel panel, string text, bool value, ref int y)
        {
            var chk = new CheckBox
            {
                Text = text,
                Checked = value,
                Location = new Point(16, y),
                AutoSize = true,
                MaximumSize = new Size(780, 0) // Full width
            };
            panel.Controls.Add(chk);
            y += 22;
            return chk;
        }

        // Advance to next row in two-column layout
        private static void NextRow(ref int y)
        {
            y += 22;
        }

        // Checkbox + numeric field for coins
        private static CheckBox AddCheckWithNum(Panel panel, string text, bool enabled, int maxVal, out NumericUpDown num, ref int y)
        {
            var chk = new CheckBox { Text = text, Checked = enabled, Location = new Point(16, y), AutoSize = true };
            panel.Controls.Add(chk);
            var n = new NumericUpDown { Minimum = 1, Maximum = 999, Value = Math.Max(1, Math.Min(maxVal, 999)), Location = new Point(280, y - 1), Size = new Size(70, 22), Enabled = enabled };
            panel.Controls.Add(n);
            chk.CheckedChanged += (s, e) => n.Enabled = chk.Checked;
            num = n;
            y += 26;
            return chk;
        }

        // Checkbox + numeric field for coins (left column)
        private static CheckBox AddCheckWithNumLeft(Panel panel, string text, bool enabled, int maxVal, out NumericUpDown num, ref int y)
        {
            var chk = new CheckBox { Text = text, Checked = enabled, Location = new Point(16, y), AutoSize = true, MaximumSize = new Size(200, 0) };
            panel.Controls.Add(chk);
            var n = new NumericUpDown { Minimum = 1, Maximum = 999, Value = Math.Max(1, Math.Min(maxVal, 999)), Location = new Point(230, y - 1), Size = new Size(70, 22), Enabled = enabled };
            panel.Controls.Add(n);
            chk.CheckedChanged += (s, e) => n.Enabled = chk.Checked;
            num = n;
            return chk;
        }

        // Checkbox + numeric field for coins (right column)
        private static CheckBox AddCheckWithNumRight(Panel panel, string text, bool enabled, int maxVal, out NumericUpDown num, int y)
        {
            var chk = new CheckBox { Text = text, Checked = enabled, Location = new Point(390, y), AutoSize = true, MaximumSize = new Size(200, 0) };
            panel.Controls.Add(chk);
            var n = new NumericUpDown { Minimum = 1, Maximum = 999, Value = Math.Max(1, Math.Min(maxVal, 999)), Location = new Point(604, y - 1), Size = new Size(70, 22), Enabled = enabled };
            panel.Controls.Add(n);
            chk.CheckedChanged += (s, e) => n.Enabled = chk.Checked;
            num = n;
            return chk;
        }

        private static RadioButton AddRadio(Panel panel, string text, bool value, ref int y)
        {
            var rb = new RadioButton { Text = text, Checked = value, Location = new Point(8, y), AutoSize = true };
            panel.Controls.Add(rb);
            y += 22;
            return rb;
        }

        private static RadioButton AddRadio(GroupBox grp, string text, bool value, ref int y)
        {
            var rb = new RadioButton { Text = text, Checked = value, Location = new Point(8, y), AutoSize = true };
            grp.Controls.Add(rb);
            y += 22;
            return rb;
        }
    }
}
