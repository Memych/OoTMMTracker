using System;
using System.Collections.Generic;

namespace OoTMMTracker.Models
{
    /// <summary>
    /// Tracker configuration — which additional items are enabled.
    /// Read from spoiler log settings or set manually.
    /// </summary>
    public class TrackerConfig
    {
        // Game mode: "ootmm" (both), "oot" (OoT only), "mm" (MM only)
        public string Games { get; set; } = "ootmm";
        public bool HasOot => Games != "mm";
        public bool HasMm  => Games != "oot";

        // OoT
        public bool OotExtraChildSwords { get; set; } = false;
        public bool OotSpinUpgrade { get; set; } = false;
        public bool OotBronzeScale { get; set; } = false;
        public bool OotStoneAgony { get; set; } = true;

        // MM
        public bool MmStrength { get; set; } = false;
        public bool MmScales { get; set; } = false;
        public bool MmStoneAgony { get; set; } = false;
        public bool MmGoronTunic { get; set; } = false;
        public bool MmZoraTunic { get; set; } = false;
        public bool MmIronBoots { get; set; } = false;
        public bool MmHoverBoots { get; set; } = false;
        public bool MmDekuShield { get; set; } = false;

        // General
        public bool ColossalWallet { get; set; } = false;
        public bool BottomlessWallet { get; set; } = false;
        public bool ChildWallet { get; set; } = false;

        // Shared — combined items
        public bool SharedSwords { get; set; } = false;
        public bool SharedShields { get; set; } = false;
        public bool SharedStrength { get; set; } = false;
        public bool SharedScales { get; set; } = false;
        public bool SharedWallets { get; set; } = false;
        public bool SharedHealth { get; set; } = false;
        public bool SharedMagic { get; set; } = false;
        public bool SharedBootsIron { get; set; } = false;
        public bool SharedBootsHover { get; set; } = false;
        public bool SharedTunicGoron { get; set; } = false;
        public bool SharedTunicZora { get; set; } = false;
        public bool SharedHookshot { get; set; } = false;
        public bool SharedBows { get; set; } = false;
        public bool SharedBombBags { get; set; } = false;
        public bool SharedOcarina { get; set; } = false;
        public bool SharedHammer { get; set; } = false;
        public bool SharedBottles { get; set; } = false;
        // MM Clocks
        public bool ClocksEnabled { get; set; } = false;      // clocks: true
        public string ProgressiveClocks { get; set; } = "separate"; // progressiveClocks: separate/ascending/descending
        // MM Owls
        public bool OwlShuffle { get; set; } = false;
        public bool OwlPreActivated { get; set; } = false; // owlsActivated: true
        // Tingle Maps
        public bool TingleMaps { get; set; } = false;
        public bool OotPreplantedBeans { get; set; } = false; // ootPreplantedBeans  // tingleShuffle: anywhere/starting         // owlShuffle: anywhere  // sharedBottles
        public bool SharedNutsSticks { get; set; } = false;
        public bool SharedBombchu { get; set; } = false;
        public bool SharedFireArrows { get; set; } = false;
        public bool SharedIceArrows { get; set; } = false;
        public bool SharedLightArrows { get; set; } = false;
        public bool SharedLens { get; set; } = false;
        public bool SharedSpellFire { get; set; } = false;  // sharedSpellFire
        public bool SharedSpellWind { get; set; } = false;  // sharedSpellWind
        public bool SharedSpellLove { get; set; } = false;  // sharedSpellLove

        // All shared items — exclusives also moved to Shared
        public bool AllItemsShared =>
            SharedBows && SharedFireArrows && SharedIceArrows && SharedLightArrows &&
            SharedHookshot && SharedHammer && SharedLens && SharedNutsSticks &&
            SharedBombBags && SharedBombchu &&
            SharedSpellFire && SharedSpellWind && SharedSpellLove;
        // Shared masks
        public bool SharedMaskGoron { get; set; } = false;
        public bool SharedMaskZora { get; set; } = false;
        public bool SharedMaskBunny { get; set; } = false;
        public bool SharedMaskKeaton { get; set; } = false;
        public bool SharedMaskTruth { get; set; } = false;
        public bool SharedMaskBlast { get; set; } = false;
        public bool SharedMaskStone { get; set; } = false;
        // OoT exclusive masks
        public bool OotBlastMask { get; set; } = false;   // blastMaskOot
        public bool OotStoneMask { get; set; } = false;   // stoneMaskOot
        // MM additional items
        public bool MmSticksNuts { get; set; } = false;    // sticksNutsUpgradesMm
        public bool MmShortHookshot { get; set; } = false; // shortHookshotMm
        public bool MmHammer { get; set; } = false;        // hammerMm
        public bool MmFairyFountain { get; set; } = false;
        // MM spells
        public bool MmSpellFire { get; set; } = false;   // spellFireMm
        public bool MmSpellWind { get; set; } = false;   // spellWindMm
        public bool MmSpellLove { get; set; } = false;
        // Songs
        public bool MmSunSong { get; set; } = false;           // sunSongMm
        public bool MmFairyOcarina { get; set; } = false;      // fairyOcarinaMm — ocarina progression MM
        public bool OotElegy { get; set; } = false;            // elegyOot
        public bool ProgressiveGoronLullaby { get; set; } = true; // progressiveGoronLullaby
        public bool FreeScarecrowOot { get; set; } = false;    // freeScarecrowOot
        public bool FreeScarecrowMm { get; set; } = false;     // freeScarecrowMm
        // Shared songs
        public bool SharedSongEpona { get; set; } = false;
        public bool SharedSongStorms { get; set; } = false;
        public bool SharedSongTime { get; set; } = false;
        public bool SharedSongSun { get; set; } = false;
        public bool SharedSongElegy { get; set; } = false;
        public bool SharedScarecrow { get; set; } = false;
        // Ocarina buttons
        public bool OotOcarinaButtons { get; set; } = false;   // ocarinaButtonsShuffleOot
        public bool MmOcarinaButtons { get; set; } = false;    // ocarinaButtonsShuffleMm
        public bool SharedOcarinaButtons { get; set; } = false; // sharedOcarinaButtons
        // Notes mode for songs
        public bool SongsAsNotes { get; set; } = false;        // songs: notes
        // Bombchu behavior: "toggle" (on/off), "bombBag" (lit when bombs available), "bag" (separate bag 20-30-40)
        public string BombchuBehaviorOot { get; set; } = "toggle";
        public string BombchuBehaviorMm { get; set; } = "toggle";

        // Collectible items — tokens
        public bool GoldSkulltulas { get; set; } = false;   // Gold Skulltula Tokens (OoT, 100 pcs.)
        public bool MmSkulltulas { get; set; } = false;     // Swamp + Ocean Skulltula Tokens (MM, 30+30 pcs.)
        public bool PlatinumTokenOot { get; set; } = false; // Platinum Token (OoT)
        public bool PlatinumTokenMm { get; set; } = false;  // Platinum Token (MM)
        public bool SharedPlatinumToken { get; set; } = false; // Shared Platinum Token (if both enabled)
        // Coins (Misc)
        public bool CoinsRed    { get; set; } = false;
        public bool CoinsGreen  { get; set; } = false;
        public bool CoinsBlue   { get; set; } = false;
        public bool CoinsYellow { get; set; } = false;
        public int CoinsRedMax    { get; set; } = 100;
        public int CoinsGreenMax  { get; set; } = 100;
        public int CoinsBlueMax   { get; set; } = 100;
        public int CoinsYellowMax { get; set; } = 100;
        // Triforce
        public string TriforceMode { get; set; } = "none"; // "none", "quest" (3 pieces), "hunt" (1-999)
        public int TriforceHuntGoal { get; set; } = 20;    // triforceGoal for hunt
        // Stray Fairies
        public bool StrayFairiesDungeons { get; set; } = false; // 4 dungeons × 15 (strayFairyChestShuffle/strayFairyOtherShuffle)
        public bool StrayFairyTown { get; set; } = false;       // Clock Town (townFairyShuffle)
        public bool TranscendentFairy { get; set; } = false;    // Platinum (transcendentFairy)
        // Trade Quests OoT
        public bool OotSkipZelda { get; set; } = false;      // skipZelda: true → hide Child Cucco
        public bool OotOpenKakariko { get; set; } = false;   // kakarikoGate: open → hide Zelda's Letter
        public bool OotEggShuffle { get; set; } = false;
        // Dungeon Keys
        public bool SmallKeysOot { get; set; } = false;
        public bool SmallKeysMm { get; set; } = false;
        public bool BossKeysOot { get; set; } = false;
        public bool BossKeysMm { get; set; } = false;
        public bool Keysanity { get; set; } = false;
        public bool MapsCompasses { get; set; } = false;
        // Sub-dungeons
        public bool SmallKeysHideout { get; set; } = false;  // smallKeyShuffleHideout
        public bool SmallKeysTcg { get; set; } = false;      // Treasure Chest Game
        // Souls
        public bool SoulsBossOot    { get; set; } = false;  // soulsBossOot
        public bool SoulsBossMm     { get; set; } = false;  // soulsBossMm
        public bool SoulsEnemyOot   { get; set; } = false;  // soulsEnemyOot
        public bool SoulsEnemyMm    { get; set; } = false;  // soulsEnemyMm
        public bool SharedSoulsEnemy { get; set; } = false; // sharedSoulsEnemy
        public bool SoulsNpcOot     { get; set; } = false;  // soulsNpcOot
        public bool SoulsNpcMm      { get; set; } = false;  // soulsNpcMm
        public bool SharedSoulsNpc  { get; set; } = false;  // sharedSoulsNpc
        public bool SoulsAnimalOot  { get; set; } = false;  // soulsAnimalOot
        public bool SoulsAnimalMm   { get; set; } = false;  // soulsAnimalMm
        public bool SharedSoulsAnimal { get; set; } = false; // sharedSoulsAnimal
        public bool SoulsMiscOot    { get; set; } = false;  // soulsMiscOot
        public bool SoulsMiscMm     { get; set; } = false;  // soulsMiscMm
        public bool SharedSoulsMisc { get; set; } = false;  // sharedSoulsMisc

        // Skeleton Key — replaces all small keys with one item
        public bool SkeletonKeyOot { get; set; } = false;  // skeletonKeyOot
        public bool SkeletonKeyMm  { get; set; } = false;  // skeletonKeyMm
        public bool SharedSkeletonKey { get; set; } = false; // manual option: combine OoT+MM into one
        // Magical Rupee — replaces all Silver Rupees with one item
        public bool MagicalRupee { get; set; } = false;    // magicalRupee
        // Silver Rupees
        public bool SilverRupees { get; set; } = false;
        // Silver Rupee Pouches — replace all pack rupees with one item
        public List<string> SrPouchPacks { get; set; } = new List<string>();
        public bool HasSrPouch(string dungeonId, string packId) => SrPouchPacks.Contains($"{dungeonId}_{packId}");
        // Master Quest Dungeons
        public List<string> MqDungeons { get; set; } = new List<string>();
        public bool IsMq(string id) => MqDungeons.Contains(id);  // silverRupeeShuffle != vanilla/ownDungeon
        // Key Rings — replace all dungeon keys with one item
        public bool KeyRingsOot { get; set; } = false;  // Small Key Ring (OoT): all
        public bool KeyRingsMm  { get; set; } = false;  // Small Key Ring (MM): all
        // Specific dungeons with key ring (dungeon id → true)
        // List for JSON serialization compatibility
        public List<string> KeyRingDungeons { get; set; } = new List<string>();
        public bool HasKeyRing(string id) => KeyRingDungeons.Contains(id);
        // Ganon's Castle boss key: "vanilla", "anywhere", "custom"
        public string GanonBossKey { get; set; } = "vanilla";
        // Dungeon reward shuffle: not "dungeonBlueWarps"
        public bool DungeonRewardShuffle { get; set; } = false;
        // Song Events Shuffle (OoT)
        public bool SongEventsShuffleOot { get; set; } = false;
        // Special Conditions for Ganon BK custom
        public int GanonBkRequired { get; set; } = 0;
        public GanonBkConditions GanonBk { get; set; } = new();
        // These items are always shared if enabled
        public bool SharedStoneAgony { get; set; } = false;   // sharedStoneAgony
        public bool SharedSpinUpgrade { get; set; } = false;  // sharedSpinUpgrade

        public static TrackerConfig FromSpoilerLog(SpoilerLog log)
        {
            var cfg = new TrackerConfig();
            cfg.Games = log.Settings.TryGetValue("games", out var g) ? g : "ootmm";
            // If only one game — all Shared flags inactive
            bool bothGames = cfg.Games == "ootmm";
            cfg.OotExtraChildSwords = GetBool(log, "extraChildSwordsOot");
            cfg.OotSpinUpgrade      = GetBool(log, "spinUpgradeOot");
            cfg.OotBronzeScale      = GetBool(log, "bronzeScale");
            cfg.MmStrength          = GetBool(log, "strengthMm");
            cfg.MmScales            = GetBool(log, "scalesMm");
            cfg.MmStoneAgony        = GetBool(log, "stoneAgonyMm");
            cfg.MmGoronTunic        = GetBool(log, "tunicGoronMm");
            cfg.MmZoraTunic         = GetBool(log, "tunicZoraMm");
            cfg.MmIronBoots         = GetBool(log, "bootsIronMm");
            cfg.MmHoverBoots        = GetBool(log, "bootsHoverMm");
            cfg.MmDekuShield        = GetBool(log, "dekuShieldMm");
            cfg.ColossalWallet      = GetBool(log, "colossalWallets");
            cfg.BottomlessWallet    = GetBool(log, "bottomlessWallets");
            cfg.ChildWallet         = GetBool(log, "childWallets");
            if (bothGames)
            {
            cfg.SharedSwords        = GetBool(log, "sharedSwords");
            cfg.SharedShields       = GetBool(log, "sharedShields");
            cfg.SharedStrength      = GetBool(log, "sharedStrength");
            cfg.SharedScales        = GetBool(log, "sharedScales");
            cfg.SharedWallets       = GetBool(log, "sharedWallets");
            cfg.SharedHealth        = GetBool(log, "sharedHealth");
            cfg.SharedMagic         = GetBool(log, "sharedMagic");
            cfg.SharedBootsIron     = GetBool(log, "sharedBootsIron");
            cfg.SharedBootsHover    = GetBool(log, "sharedBootsHover");
            cfg.SharedTunicGoron    = GetBool(log, "sharedTunicGoron");
            cfg.SharedTunicZora     = GetBool(log, "sharedTunicZora");
            cfg.SharedHookshot      = GetBool(log, "sharedHookshot");
            cfg.SharedBows          = GetBool(log, "sharedBows");
            cfg.SharedBombBags      = GetBool(log, "sharedBombBags");
            cfg.SharedOcarina       = GetBool(log, "sharedOcarina");
            cfg.SharedHammer        = GetBool(log, "sharedHammer");
            cfg.SharedBottles       = GetBool(log, "sharedBottles");
            cfg.SharedNutsSticks    = GetBool(log, "sharedNutsSticks");
            cfg.SharedBombchu       = GetBool(log, "sharedBombchu");
            cfg.SharedFireArrows    = GetBool(log, "sharedMagicArrowFire");
            cfg.SharedIceArrows     = GetBool(log, "sharedMagicArrowIce");
            cfg.SharedLightArrows   = GetBool(log, "sharedMagicArrowLight");
            cfg.SharedLens          = GetBool(log, "sharedLens");
            cfg.SharedSpellFire     = GetBool(log, "sharedSpellFire");
            cfg.SharedSpellWind     = GetBool(log, "sharedSpellWind");
            cfg.SharedSpellLove     = GetBool(log, "sharedSpellLove");
            cfg.SharedStoneAgony    = GetBool(log, "sharedStoneAgony");
            cfg.SharedSpinUpgrade   = GetBool(log, "sharedSpinUpgrade");
            cfg.SharedSongEpona     = GetBool(log, "sharedSongEpona");
            cfg.SharedSongStorms    = GetBool(log, "sharedSongStorms");
            cfg.SharedSongTime      = GetBool(log, "sharedSongTime");
            cfg.SharedSongSun       = GetBool(log, "sharedSongSun");
            cfg.SharedSongElegy     = GetBool(log, "sharedSongElegy");
            cfg.SharedOcarinaButtons = GetBool(log, "sharedOcarinaButtons");
            cfg.SharedMaskGoron     = GetBool(log, "sharedMaskGoron");
            cfg.SharedMaskZora      = GetBool(log, "sharedMaskZora");
            cfg.SharedMaskBunny     = GetBool(log, "sharedMaskBunny");
            cfg.SharedMaskKeaton    = GetBool(log, "sharedMaskKeaton");
            cfg.SharedMaskTruth     = GetBool(log, "sharedMaskTruth");
            cfg.SharedMaskBlast     = GetBool(log, "sharedMaskBlast");
            cfg.SharedMaskStone     = GetBool(log, "sharedMaskStone");
            cfg.SharedScarecrow     = GetBool(log, "sharedScarecrow");
            } // end bothGames
            cfg.ClocksEnabled       = cfg.HasMm && GetBool(log, "clocks");
            cfg.ProgressiveClocks   = log.Settings.TryGetValue("progressiveClocks", out var pc) ? pc : "separate";
            cfg.OwlShuffle          = cfg.HasMm && log.Settings.TryGetValue("owlShuffle", out var os) && os != "none";
            cfg.OwlPreActivated     = cfg.HasMm && GetBool(log, "owlsActivated");
            cfg.TingleMaps          = cfg.HasMm && log.Settings.TryGetValue("tingleShuffle", out var ts) && ts != "removed";
            cfg.OotPreplantedBeans  = GetBool(log, "ootPreplantedBeans");
            cfg.MmSpellFire         = GetBool(log, "spellFireMm");
            cfg.MmSpellWind         = GetBool(log, "spellWindMm");
            cfg.MmSpellLove         = GetBool(log, "spellLoveMm");
            cfg.MmSunSong           = GetBool(log, "sunSongMm");
            cfg.MmFairyOcarina      = GetBool(log, "fairyOcarinaMm");
            cfg.OotElegy            = GetBool(log, "elegyOot");
            cfg.ProgressiveGoronLullaby = log.Settings.TryGetValue("progressiveGoronLullaby", out var pgl) && pgl == "progressive";
            cfg.FreeScarecrowOot    = GetBool(log, "freeScarecrowOot");
            cfg.FreeScarecrowMm     = GetBool(log, "freeScarecrowMm");
            cfg.OotOcarinaButtons   = GetBool(log, "ocarinaButtonsShuffleOot");
            cfg.MmOcarinaButtons    = GetBool(log, "ocarinaButtonsShuffleMm");
            cfg.SongsAsNotes        = log.Settings.TryGetValue("songs", out var sv) && sv == "notes";
            cfg.SharedMaskGoron     = GetBool(log, "sharedMaskGoron");
            cfg.SharedMaskZora      = GetBool(log, "sharedMaskZora");
            cfg.SharedMaskBunny     = GetBool(log, "sharedMaskBunny");
            cfg.SharedMaskKeaton    = GetBool(log, "sharedMaskKeaton");
            cfg.SharedMaskTruth     = GetBool(log, "sharedMaskTruth");
            cfg.SharedMaskBlast     = GetBool(log, "sharedMaskBlast");
            cfg.SharedMaskStone     = GetBool(log, "sharedMaskStone");
            cfg.OotBlastMask        = GetBool(log, "blastMaskOot");
            cfg.OotStoneMask        = GetBool(log, "stoneMaskOot");
            cfg.MmSticksNuts        = GetBool(log, "sticksNutsUpgradesMm");
            cfg.MmShortHookshot     = GetBool(log, "shortHookshotMm");
            cfg.MmHammer            = GetBool(log, "hammerMm");
            cfg.MmFairyFountain     = GetBool(log, "fairyFountainFairyShuffleMm");
            // Bombchu behavior — read from log (priority over saved settings)
            if (log.Settings.TryGetValue("bombchuBehaviorOot", out var bcOot))
                cfg.BombchuBehaviorOot = bcOot == "bagSeparate" ? "bag" : bcOot == "bombBag" ? "bombBag" : "toggle";
            if (log.Settings.TryGetValue("bombchuBehaviorMm", out var bcMm))
                cfg.BombchuBehaviorMm = bcMm == "bagSeparate" ? "bag" : bcMm == "bombBag" ? "bombBag" : "toggle";
            cfg.SharedStoneAgony    = bothGames && GetBool(log, "sharedStoneAgony");
            cfg.SharedSpinUpgrade   = bothGames && GetBool(log, "sharedSpinUpgrade");
            // Tokens — considering game
            cfg.GoldSkulltulas  = cfg.HasOot && log.Settings.TryGetValue("goldSkulltulaTokens",   out var gst) && gst != "none";
            cfg.MmSkulltulas    = cfg.HasMm  && log.Settings.TryGetValue("housesSkulltulaTokens", out var hst) && hst != "none";
            cfg.PlatinumTokenOot = cfg.HasOot && GetBool(log, "platinumTokenOot");
            cfg.PlatinumTokenMm  = cfg.HasMm  && GetBool(log, "platinumTokenMm");
            cfg.SharedPlatinumToken = bothGames && cfg.PlatinumTokenOot && cfg.PlatinumTokenMm;
            // Coins
            bool coinsEnabled = GetBool(log, "coins");
            if (coinsEnabled)
            {
                int CoinMax(string key) => log.Settings.TryGetValue(key, out var v) && int.TryParse(v, out var n) && n > 0 ? (n < 999 ? n : 999) : 100;
                cfg.CoinsRed    = true; cfg.CoinsRedMax    = CoinMax("coinsRed");
                cfg.CoinsGreen  = true; cfg.CoinsGreenMax  = CoinMax("coinsGreen");
                cfg.CoinsBlue   = true; cfg.CoinsBlueMax   = CoinMax("coinsBlue");
                cfg.CoinsYellow = true; cfg.CoinsYellowMax = CoinMax("coinsYellow");
            }
            // Triforce
            if (log.Settings.TryGetValue("goal", out var goal))
            {
                if (goal == "triforce3")
                    cfg.TriforceMode = "quest";
                else if (goal == "triforce")
                {
                    cfg.TriforceMode = "hunt";
                    if (log.Settings.TryGetValue("triforceGoal", out var tg) && int.TryParse(tg, out var tgn) && tgn > 0)
                        cfg.TriforceHuntGoal = tgn < 999 ? tgn : 999;
                }
            }
            // Stray Fairies — MM only
            cfg.StrayFairiesDungeons = cfg.HasMm && ((log.Settings.TryGetValue("strayFairyChestShuffle", out var sfcs) && sfcs != "none" && sfcs != "vanilla")
                                    || (log.Settings.TryGetValue("strayFairyOtherShuffle", out var sfos) && sfos != "none" && sfos != "vanilla"));
            cfg.StrayFairyTown    = cfg.HasMm && log.Settings.TryGetValue("townFairyShuffle", out var tfs) && tfs != "none" && tfs != "vanilla";
            cfg.TranscendentFairy = cfg.HasMm && GetBool(log, "transcendentFairy");
            // Trade Quests OoT
            cfg.OotSkipZelda     = GetBool(log, "skipZelda");
            cfg.OotOpenKakariko  = log.Settings.TryGetValue("kakarikoGate", out var kg) && kg == "open";
            cfg.OotEggShuffle    = GetBool(log, "eggShuffle");
            // Dungeon Keys
            cfg.SmallKeysOot = log.Settings.TryGetValue("smallKeyShuffleOot", out var skoot) && skoot != "vanilla" && skoot != "ownDungeon";
            cfg.SmallKeysMm  = log.Settings.TryGetValue("smallKeyShuffleMm",  out var skmm)  && skmm  != "vanilla" && skmm  != "ownDungeon";
            cfg.BossKeysOot  = log.Settings.TryGetValue("bossKeyShuffleOot",  out var bkoot) && bkoot != "vanilla" && bkoot != "ownDungeon";
            cfg.BossKeysMm   = log.Settings.TryGetValue("bossKeyShuffleMm",   out var bkmm)  && bkmm  != "vanilla" && bkmm  != "ownDungeon";
            cfg.Keysanity    = log.Settings.TryGetValue("smallKeyShuffleOot", out var ks) && ks == "anywhere";
            cfg.MapsCompasses = log.Settings.TryGetValue("mapCompassShuffle", out var mc) && mc != "vanilla" && mc != "ownDungeon";
            cfg.SmallKeysHideout = cfg.HasOot && log.Settings.TryGetValue("smallKeyShuffleHideout",   out var skh)  && skh  != "vanilla" && skh  != "ownDungeon";
            cfg.SmallKeysTcg     = cfg.HasOot && log.Settings.TryGetValue("smallKeyShuffleChestGame", out var sktcg) && sktcg != "vanilla" && sktcg != "ownDungeon";
            cfg.SilverRupees     = cfg.HasOot && log.Settings.TryGetValue("silverRupeeShuffle", out var sr) && sr != "vanilla" && sr != "ownDungeon";
            cfg.SkeletonKeyOot   = cfg.HasOot && GetBool(log, "skeletonKeyOot");
            cfg.SkeletonKeyMm    = cfg.HasMm  && GetBool(log, "skeletonKeyMm");
            cfg.MagicalRupee     = cfg.SilverRupees && GetBool(log, "magicalRupee");
            // Souls
            cfg.SoulsBossOot     = cfg.HasOot && GetBool(log, "soulsBossOot");
            cfg.SoulsBossMm      = cfg.HasMm  && GetBool(log, "soulsBossMm");
            cfg.SoulsEnemyOot    = cfg.HasOot && GetBool(log, "soulsEnemyOot");
            cfg.SoulsEnemyMm     = cfg.HasMm  && GetBool(log, "soulsEnemyMm");
            cfg.SharedSoulsEnemy = bothGames && cfg.SoulsEnemyOot && cfg.SoulsEnemyMm && GetBool(log, "sharedSoulsEnemy");
            cfg.SoulsNpcOot      = cfg.HasOot && GetBool(log, "soulsNpcOot");
            cfg.SoulsNpcMm       = cfg.HasMm  && GetBool(log, "soulsNpcMm");
            cfg.SharedSoulsNpc   = bothGames && cfg.SoulsNpcOot && cfg.SoulsNpcMm && GetBool(log, "sharedSoulsNpc");
            cfg.SoulsAnimalOot   = cfg.HasOot && GetBool(log, "soulsAnimalOot");
            cfg.SoulsAnimalMm    = cfg.HasMm  && GetBool(log, "soulsAnimalMm");
            cfg.SharedSoulsAnimal= bothGames && cfg.SoulsAnimalOot && cfg.SoulsAnimalMm && GetBool(log, "sharedSoulsAnimal");
            cfg.SoulsMiscOot     = cfg.HasOot && GetBool(log, "soulsMiscOot");
            cfg.SoulsMiscMm      = cfg.HasMm  && GetBool(log, "soulsMiscMm");
            cfg.SharedSoulsMisc  = bothGames && cfg.SoulsMiscOot && cfg.SoulsMiscMm && GetBool(log, "sharedSoulsMisc");
            // Master Quest Dungeons (World Flags) — only if OoT in rando
            if (cfg.HasOot && log.WorldFlags.TryGetValue("Master Quest Dungeons", out var mq))
            {
                if (mq == "all")
                    foreach (var id in new[]{"deku_tree","dodongo","jabu","forest_temple","fire_temple","water_temple","shadow_temple","spirit_temple","botw","ice_cavern","gtg","ganons_castle"})
                        cfg.MqDungeons.Add(id);
                else if (!string.IsNullOrEmpty(mq) && mq != "none")
                    foreach (var name in mq.Split('|'))
                        AddMqDungeonByName(cfg, name.Trim());
            }
            if (cfg.SilverRupees && log.WorldFlags.TryGetValue("Silver Rupee Pouches", out var srp))
            {
                if (srp == "all")
                {
                    // Vanilla packs for non-MQ dungeons, MQ packs for MQ dungeons
                    // Vanilla dungeons
                    foreach (var (dId, packs) in new (string, string[])[]
                    {
                        ("shadow_temple",  new[]{"scythe","pit","spikes"}),
                        ("spirit_temple",  new[]{"child","sun","boulders"}),
                        ("ice_cavern",     new[]{"scythe","block"}),
                        ("botw",           new[]{"basement"}),
                        ("gtg",            new[]{"slopes","lava","water"}),
                        ("ganons_castle",  new[]{"spirit","light","fire","forest"}),
                    })
                    {
                        if (!cfg.IsMq(dId))
                            foreach (var pId in packs)
                                cfg.SrPouchPacks.Add($"{dId}_{pId}");
                    }
                    // MQ packs
                    foreach (var (dId, packs) in new (string, string[])[]
                    {
                        ("dodongo",       new[]{"mq_staircase"}),
                        ("shadow_temple", new[]{"mq_scythe","mq_blades","mq_pit","mq_spikes"}),
                        ("spirit_temple", new[]{"mq_lobby","mq_adult"}),
                        ("gtg",           new[]{"mq_slopes","mq_lava","mq_water"}),
                        ("ganons_castle", new[]{"mq_fire","mq_shadow","mq_water"}),
                    })
                    {
                        if (cfg.IsMq(dId))
                            foreach (var pId in packs)
                                cfg.SrPouchPacks.Add($"{dId}_{pId}");
                    }
                }
                else if (!string.IsNullOrEmpty(srp) && srp != "none")
                {
                    // Custom: "Bottom of the Well|Spirit Temple (Sun)|..."
                    foreach (var entry in srp.Split('|'))
                        AddSrPouchByName(cfg, entry.Trim());
                }
            }
            cfg.KeyRingsOot = cfg.HasOot && log.WorldFlags.TryGetValue("Small Key Ring (OoT)", out var krOot) && krOot == "all";
            cfg.KeyRingsMm  = cfg.HasMm  && log.WorldFlags.TryGetValue("Small Key Ring (MM)",  out var krMm)  && krMm  == "all";
            // Fill KeyRingDungeons
            if (cfg.KeyRingsOot)
                foreach (var id in new[]{"forest_temple","fire_temple","water_temple","shadow_temple","spirit_temple","botw","gtg","ganons_castle","thieves_hideout","tcg"})
                    cfg.KeyRingDungeons.Add(id);
            if (cfg.KeyRingsMm)
                foreach (var id in new[]{"woodfall","snowhead","great_bay","stone_tower"})
                    cfg.KeyRingDungeons.Add(id);
            // Custom Key Rings — dungeon list via |
            if (log.WorldFlags.TryGetValue("Small Key Ring (OoT)", out var krOotCustom) && !string.IsNullOrEmpty(krOotCustom) && krOotCustom != "all" && krOotCustom != "none")
                foreach (var name in krOotCustom.Split('|'))
                    AddKeyRingByName(cfg, name.Trim());
            if (log.WorldFlags.TryGetValue("Small Key Ring (MM)", out var krMmCustom) && !string.IsNullOrEmpty(krMmCustom) && krMmCustom != "all" && krMmCustom != "none")
                foreach (var name in krMmCustom.Split('|'))
                    AddKeyRingByName(cfg, name.Trim());
            cfg.GanonBossKey     = log.Settings.TryGetValue("ganonBossKey", out var gbk) ? gbk : "vanilla";
            cfg.DungeonRewardShuffle = log.Settings.TryGetValue("dungeonRewardShuffle", out var drs)
                                      && drs != "dungeonBlueWarps"
                                      && !string.IsNullOrEmpty(drs);
            cfg.SongEventsShuffleOot = cfg.HasOot && GetBool(log, "songEventsShuffleOot");
            // GANON_BK Special Conditions
            if (cfg.GanonBossKey == "custom" && log.SpecialConditions.TryGetValue("GANON_BK", out var ganonBk))
            {
                cfg.GanonBk = GanonBkConditions.FromSpecialCondition(ganonBk);
                cfg.GanonBkRequired = cfg.GanonBk.Count;
            }
            return cfg;
        }

        private static bool GetBool(SpoilerLog log, string key)
            => log.Settings.TryGetValue(key, out var v) && v == "true";

        // Mapping dungeon names from log → id
        private static void AddMqDungeonByName(TrackerConfig cfg, string name)
        {
            var id = name.ToLower() switch
            {
                "deku tree"              => "deku_tree",
                "dodongo's cavern"       => "dodongo",
                "inside jabu-jabu"       => "jabu",
                "forest temple"          => "forest_temple",
                "fire temple"            => "fire_temple",
                "water temple"           => "water_temple",
                "shadow temple"          => "shadow_temple",
                "spirit temple"          => "spirit_temple",
                "bottom of the well"     => "botw",
                "ice cavern"             => "ice_cavern",
                "gerudo training grounds"=> "gtg",
                "ganon's castle"         => "ganons_castle",
                _ => null
            };
            if (id != null) cfg.MqDungeons.Add(id);
        }

        private static void AddKeyRingByName(TrackerConfig cfg, string name)        {
            var id = name.ToLower() switch
            {
                "forest temple"          => "forest_temple",
                "fire temple"            => "fire_temple",
                "water temple"           => "water_temple",
                "shadow temple"          => "shadow_temple",
                "spirit temple"          => "spirit_temple",
                "bottom of the well"     => "botw",
                "gerudo training grounds"=> "gtg",
                "ganon's castle"         => "ganons_castle",
                "hideout"                => "thieves_hideout",
                "chest game"             => "tcg",
                "woodfall temple"        => "woodfall",
                "snowhead temple"        => "snowhead",
                "great bay temple"       => "great_bay",
                "stone tower temple"     => "stone_tower",
                _ => null
            };
            if (id != null) cfg.KeyRingDungeons.Add(id);
        }

        // Mapping pack names from log → key "dungeonId_packId"
        // Vanilla packs: "dungeonId_packId"
        // MQ packs: "dungeonId_mq_packId"
        private static void AddSrPouchByName(TrackerConfig cfg, string entry)
        {
            // Format: "Shadow Temple (Scythe)" or "Bottom of the Well"
            // MQ packs have mq_ prefix in packId
            var key = entry.ToLower() switch
            {
                // Vanilla packs
                "shadow temple (scythe)"          => "shadow_temple_scythe",
                "shadow temple (pit)"             => "shadow_temple_pit",
                "shadow temple (spikes)"          => "shadow_temple_spikes",
                "spirit temple (child)"           => "spirit_temple_child",
                "spirit temple (sun)"             => "spirit_temple_sun",
                "spirit temple (lobby)"           => "spirit_temple_boulders",
                "spirit temple (boulders)"        => "spirit_temple_boulders",
                "ice cavern (scythe)"             => "ice_cavern_scythe",
                "ice cavern (block)"              => "ice_cavern_block",
                "bottom of the well"              => "botw_basement",
                "gtg (slopes)"                    => "gtg_slopes",
                "gtg (lava)"                      => "gtg_lava",
                "gtg (water)"                     => "gtg_water",
                "ganon's castle (spirit)"         => "ganons_castle_spirit",
                "ganon's castle (spirit trial)"   => "ganons_castle_spirit",
                "ganon's castle (light)"          => "ganons_castle_light",
                "ganon's castle (light trial)"    => "ganons_castle_light",
                "ganon's castle (fire)"           => "ganons_castle_fire",
                "ganon's castle (fire trial)"     => "ganons_castle_fire",
                "ganon's castle (forest)"         => "ganons_castle_forest",
                "ganon's castle (forest trial)"   => "ganons_castle_forest",
                // MQ packs (with mq_ prefix in packId)
                "dodongo's cavern (staircase)"    => "dodongo_mq_staircase",
                "shadow temple (blades)"          => "shadow_temple_mq_blades",
                "shadow temple mq (scythe)"       => "shadow_temple_mq_scythe",
                "shadow temple mq (pit)"          => "shadow_temple_mq_pit",
                "shadow temple mq (spikes)"       => "shadow_temple_mq_spikes",
                "spirit temple (adult)"           => "spirit_temple_mq_adult",
                "spirit temple mq (lobby)"        => "spirit_temple_mq_lobby",
                "spirit temple mq (adult)"        => "spirit_temple_mq_adult",
                "gtg mq (slopes)"                 => "gtg_mq_slopes",
                "gtg mq (lava)"                   => "gtg_mq_lava",
                "gtg mq (water)"                  => "gtg_mq_water",
                "ganon's castle (shadow)"         => "ganons_castle_mq_shadow",
                "ganon's castle (shadow trial)"   => "ganons_castle_mq_shadow",
                "ganon's castle (water)"          => "ganons_castle_mq_water",
                "ganon's castle (water trial)"    => "ganons_castle_mq_water",
                "ganon's castle mq (fire)"        => "ganons_castle_mq_fire",
                "ganon's castle mq (fire trial)"  => "ganons_castle_mq_fire",
                _ => null
            };
            if (key != null) cfg.SrPouchPacks.Add(key);
        }
    }

    /// <summary>Conditions for Custom Ganon BK — read from Special Conditions log</summary>
    public class GanonBkConditions
    {
        public int  Count          { get; set; } = 0;
        // Dungeons
        public bool Stones         { get; set; } = false; // 3 OoT stones
        public bool Medallions     { get; set; } = false; // 6 OoT medallions
        public bool Remains        { get; set; } = false; // 4 MM remains
        // Tokens
        public bool SkullsGold     { get; set; } = false; // 100 Gold Skulltulas
        public bool SkullsSwamp    { get; set; } = false; // 30 Swamp
        public bool SkullsOcean    { get; set; } = false; // 30 Ocean
        // Fairies
        public bool FairiesWF      { get; set; } = false; // 15 Woodfall
        public bool FairiesSH      { get; set; } = false; // 15 Snowhead
        public bool FairiesGB      { get; set; } = false; // 15 Great Bay
        public bool FairiesST      { get; set; } = false; // 15 Stone Tower
        public bool FairyTown      { get; set; } = false; // 1 Clock Town
        // Masks
        public bool MasksRegular   { get; set; } = false; // regular MM masks
        public bool MasksTransform { get; set; } = false; // transform MM masks
        public bool MasksOot       { get; set; } = false; // OoT masks
        public bool CoinsRed    { get; set; } = false;
        public bool CoinsGreen  { get; set; } = false;
        public bool CoinsBlue   { get; set; } = false;
        public bool CoinsYellow { get; set; } = false;
        public bool Triforce    { get; set; } = false; // triforce hunt in Special Conditions

        public static GanonBkConditions FromSpecialCondition(SpecialCondition sc)
        {
            bool B(string k) => sc.Requirements.TryGetValue(k, out var v) && v == "true";
            int  I(string k) => sc.Requirements.TryGetValue(k, out var v) && int.TryParse(v, out var n) ? n : 0;
            return new()
            {
                Count          = I("count"),
                Stones         = B("stones"),
                Medallions     = B("medallions"),
                Remains        = B("remains"),
                SkullsGold     = B("skullsGold"),
                SkullsSwamp    = B("skullsSwamp"),
                SkullsOcean    = B("skullsOcean"),
                FairiesWF      = B("fairiesWF"),
                FairiesSH      = B("fairiesSH"),
                FairiesGB      = B("fairiesGB"),
                FairiesST      = B("fairiesST"),
                FairyTown      = B("fairyTown"),
                MasksRegular   = B("masksRegular"),
                MasksTransform = B("masksTransform"),
                MasksOot       = B("masksOot"),
                CoinsRed       = B("coinsRed"),
                CoinsGreen     = B("coinsGreen"),
                CoinsBlue      = B("coinsBlue"),
                CoinsYellow    = B("coinsYellow"),
                Triforce       = B("triforce"),
            };
        }
    }
}
