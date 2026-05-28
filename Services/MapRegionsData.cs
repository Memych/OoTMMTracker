using System.Collections.Generic;
using OoTMMTracker.Models;

namespace OoTMMTracker.Services
{
    /// <summary>
    /// Registry of all map regions and their sub-maps with location marks.
    ///
    /// HOW TO ADD/POSITION MARKS:
    ///   1. Open the background PNG in any image editor (e.g. Paint, Photoshop).
    ///   2. Hover over the spot where you want the mark center.
    ///   3. Note the X, Y pixel coordinates shown in the status bar.
    ///   4. Add an M(...) entry with those coordinates.
    ///   5. Rebuild the project — the mark will appear scaled to the map.
    ///
    /// M(icon, x, y, size, tooltip, locationNames...)
    ///   icon          — "marks/Chest.png", "marks/NPC.png", etc.
    ///   x, y          — center position in original image pixels
    ///   size          — icon size in original pixels (default 24)
    ///   tooltip       — text shown on hover
    ///   locationNames — spoiler log location names WITHOUT "OOT"/"MM" prefix
    ///
    /// ME(icon, x, y, tooltip, entranceFromId)
    /// ME(icon, x, y, tooltip, ageOrMmState, entranceFromId)
    /// MEJ(icon, x, y, tooltip, jpRegion, entranceFromId) — JP layouts: same rule as <c>MJ</c> (setting "all" OR list contains <c>jpRegion</c>).
    /// MENJ(icon, x, y, tooltip, jpRegion, entranceFromId) — JP layouts: same rule as <c>MNJ</c> (invert: not "all" and list does not contain <c>jpRegion</c>).
    ///   entrance shuffle: fixed on-map size 40; entranceFromId = token inside ( ) on the <b>From</b> (left) column of Entrances.
    ///   See <see cref="SpoilerEntranceIdDatabase.IsKnownId"/> for ids seen in sample spoiler logs (typos vs. generator).
    ///   Mark visible only if that row exists in the loaded spoiler. The <b>To</b> (right) column id maps to a sub-map
    ///   via that sub-map's <see cref="MapSubRegion.DestinationEntranceIds"/>.
    ///   Optional overload: ageOrMmState = OoT "child"|"adult" or MM "cursed"|"cleared" — same as <see cref="MapMark.AgeFilter"/> / MA(...).
    /// </summary>
    public static class MapRegionsData
    {
        private static MapMark M(string icon, int x, int y, int size, string tooltip, params string[] locations) => new()
        {
            IconPath = $"marks/{icon}",
            X = x,
            Y = y,
            Size = size,
            Tooltip = tooltip,
            LocationNames = new List<string>(locations)
        };

        // Overload with age filter: age = "child" | "adult"
        private static MapMark MA(string icon, int x, int y, int size, string tooltip, string age, params string[] locations) => new()
        {
            IconPath = $"marks/{icon}",
            X = x,
            Y = y,
            Size = size,
            Tooltip = tooltip,
            AgeFilter = age,
            LocationNames = new List<string>(locations)
        };

        // JP layout condition: shown only when JP layouts = "all" OR contains jpRegion
        private static readonly string JpLayoutKey = "Majora's Mask JP Layouts";
        private static MapMark MJ(string icon, int x, int y, int size, string tooltip, string jpRegion, params string[] locations) => new()
        {
            IconPath = $"marks/{icon}",
            X = x,
            Y = y,
            Size = size,
            Tooltip = tooltip,
            RequiredSettingKey = JpLayoutKey,
            RequiredSettingValue = "all",
            RequiredSettingContains = jpRegion,
            LocationNames = new List<string>(locations)
        };

        // Not-JP: shown only when JP layouts is NOT "all" and does NOT contain jpRegion
        private static MapMark MNJ(string icon, int x, int y, int size, string tooltip, string jpRegion, params string[] locations) => new()
        {
            IconPath = $"marks/{icon}",
            X = x,
            Y = y,
            Size = size,
            Tooltip = tooltip,
            RequiredSettingKey = JpLayoutKey,
            RequiredSettingValue = "all",
            RequiredSettingContains = jpRegion,
            RequiredSettingInvert = true,
            LocationNames = new List<string>(locations)
        };

        private const int EntranceMarkDisplaySize = 40;

        /// <summary>Entrance shuffle mark: <see cref="MapMark.EntranceFromId"/> = From (left) id; target sub-map lists the To-side id in <see cref="MapSubRegion.DestinationEntranceIds"/>.</summary>
        private static MapMark ME(string icon, int x, int y, string tooltip, string entranceFromId) => new()
        {
            IconPath = $"marks/{icon}",
            X = x,
            Y = y,
            Size = EntranceMarkDisplaySize,
            Tooltip = tooltip,
            EntranceFromId = entranceFromId,
            LocationNames = new List<string>()
        };

        /// <summary>
        /// Entrance shuffle mark with age (OoT: child/adult) or world state (MM: cursed/cleared). Matches the map toolbar checkbox like <c>MA(...)</c>.
        /// </summary>
        private static MapMark MEA(string icon, int x, int y, string tooltip, string ageOrMmState, string entranceFromId) => new()
        {
            IconPath = $"marks/{icon}",
            X = x,
            Y = y,
            Size = EntranceMarkDisplaySize,
            Tooltip = tooltip,
            AgeFilter = ageOrMmState,
            EntranceFromId = entranceFromId,
            LocationNames = new List<string>()
        };

        /// <summary>Entrance mark visible under JP layout rules like <see cref="MJ"/>.</summary>
        private static MapMark MEJ(string icon, int x, int y, string tooltip, string jpRegion, string entranceFromId) => new()
        {
            IconPath = $"marks/{icon}",
            X = x,
            Y = y,
            Size = EntranceMarkDisplaySize,
            Tooltip = tooltip,
            RequiredSettingKey = JpLayoutKey,
            RequiredSettingValue = "all",
            RequiredSettingContains = jpRegion,
            EntranceFromId = entranceFromId,
            LocationNames = new List<string>()
        };

        /// <summary>Entrance mark visible under JP layout rules like <see cref="MNJ"/>.</summary>
        private static MapMark MENJ(string icon, int x, int y, string tooltip, string jpRegion, string entranceFromId) => new()
        {
            IconPath = $"marks/{icon}",
            X = x,
            Y = y,
            Size = EntranceMarkDisplaySize,
            Tooltip = tooltip,
            RequiredSettingKey = JpLayoutKey,
            RequiredSettingValue = "all",
            RequiredSettingContains = jpRegion,
            RequiredSettingInvert = true,
            EntranceFromId = entranceFromId,
            LocationNames = new List<string>()
        };

        private static string OoT(string folder, string file) =>
            $"region maps/OoT/{folder}/{file}.png";

        private static string MM(string folder, string file) =>
            $"region maps/MM/{folder}/{file}.png";

        private static MapRegion DeathMountainCrater()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Death Mountain Crater";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Death Mountain Crater",
                    BackgroundImage = OoT("Death_Mountain_Crater", "Crater"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_DEATH_CRATER_FROM_TEMPLE_FIRE",
						"OOT_DEATH_MOUNTAIN_CRATER",
						"OOT_CRATER_FROM_GORON_CITY",
						"OOT_WARP_SONG_CRATER",
						"OOT_GROTTO_EXIT_SCRUBS3_DMC",
						"OOT_GROTTO_EXIT_GENERIC_DMC",
						"OOT_DEATH_CRATER_FROM_FAIRY"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 640, 8, "Entrance shuffle (Death Mountain Trail)", "OOT_TRAIL_SUMMIT_FROM_CRATER"),
                        ME("Entrance.png", 773, 433, "Entrance shuffle (Goron City)", "OOT_GORON_CITY_FROM_CRATER"),
                        ME("Entrance.png", 451, 563, "Entrance shuffle (Fire Temple)", "OOT_TEMPLE_FIRE"),
                        ME("Entrance.png", 696, 182, "Entrance shuffle (Great Fairy Fountain)", "OOT_FAIRY_MAGIC2"),
                        ME("Entrance.png", 720, 489, "Entrance shuffle (Deku Scrubs Grotto)", "OOT_GROTTO_SCRUBS3_DMC"),
                        ME("Entrance.png", 430, 79, "Entrance shuffle (Generic Grotto)", "OOT_GROTTO_GENERIC_DMC"),

                        M("Collectible.png", 450, 197, 40, "OOT Death Mountain Crater Alcove HP", "Death Mountain Crater Alcove HP"),
                        M("Collectible.png", 573, 347, 40, "OOT Death Mountain Crater Volcano HP", "Death Mountain Crater Volcano HP"),
                        MA("Gold_Skulltula.png", 469, 393, 40, "OOT Death Mountain Crater GS Soil", "child", "Death Mountain Crater GS Soil"),
                        MA("Gold_Skulltula.png", 619, 42, 40, "OOT Death Mountain Crater GS Crate", "child", "Death Mountain Crater GS Crate"),
                        MA("Soil.png", 476, 417, 24, "OOT Death Mountain Crater Soil 1", "child", "Death Mountain Crater Soil 1"),
                        MA("Soil.png", 499, 399, 24, "OOT Death Mountain Crater Soil 2", "child", "Death Mountain Crater Soil 2"),
                        MA("Soil.png", 454, 399, 24, "OOT Death Mountain Crater Soil 3", "child", "Death Mountain Crater Soil 3"),
                        MA("Rupee.png", 360, 482, 24, "OOT Death Mountain Crater Rupee Child 1", "child", "Death Mountain Crater Rupee Child 1"),
                        MA("Rupee.png", 296, 403, 24, "OOT Death Mountain Crater Rupee Child 2", "child", "Death Mountain Crater Rupee Child 2"),
                        MA("Rupee.png", 296, 426, 24, "OOT Death Mountain Crater Rupee Child 3", "child", "Death Mountain Crater Rupee Child 3"),
                        MA("Rupee.png", 296, 379, 24, "OOT Death Mountain Crater Rupee Child 4", "child", "Death Mountain Crater Rupee Child 4"),
                        MA("Rupee.png", 280, 389, 24, "OOT Death Mountain Crater Rupee Child 5", "child", "Death Mountain Crater Rupee Child 5"),
                        MA("Rupee.png", 280, 414, 24, "OOT Death Mountain Crater Rupee Child 6", "child", "Death Mountain Crater Rupee Child 6"),
                        MA("Rupee.png", 312, 414, 24, "OOT Death Mountain Crater Rupee Child 7", "child", "Death Mountain Crater Rupee Child 7"),
                        MA("Rupee.png", 312, 389, 24, "OOT Death Mountain Crater Rupee Child 8", "child", "Death Mountain Crater Rupee Child 8"),
                        MA("Rupee.png", 298, 288, 24, "OOT Death Mountain Crater Rupee Adult 1", "adult", "Death Mountain Crater Rupee Adult 1"),
                        MA("Rupee.png", 282, 298, 24, "OOT Death Mountain Crater Rupee Adult 2", "adult", "Death Mountain Crater Rupee Adult 2"),
                        MA("Rupee.png", 282, 323, 24, "OOT Death Mountain Crater Rupee Adult 3", "adult", "Death Mountain Crater Rupee Adult 3"),
                        MA("Rupee.png", 298, 335, 24, "OOT Death Mountain Crater Rupee Adult 4", "adult", "Death Mountain Crater Rupee Adult 4"),
                        MA("Rupee.png", 314, 323, 24, "OOT Death Mountain Crater Rupee Adult 5", "adult", "Death Mountain Crater Rupee Adult 5"),
                        MA("Rupee.png", 314, 298, 24, "OOT Death Mountain Crater Rupee Adult 6", "adult", "Death Mountain Crater Rupee Adult 6"),
                        MA("Rupee.png", 298, 312, 24, "OOT Death Mountain Crater Rupee Adult 7", "adult", "Death Mountain Crater Rupee Adult 7"),
                        MA("NPC.png", 517, 416, 40, "OOT Death Mountain Crater Sheik Song", "adult", "Death Mountain Crater Sheik Song"),
                        M("Pot.png", 700, 398, 24, "OOT Death Mountain Crater Pot 1", "Death Mountain Crater Pot 1"),
                        M("Pot.png", 723, 388, 24, "OOT Death Mountain Crater Pot 2", "Death Mountain Crater Pot 2"),
                        M("Pot.png", 717, 350, 24, "OOT Death Mountain Crater Pot 3", "Death Mountain Crater Pot 3"),
                        M("Pot.png", 692, 348, 24, "OOT Death Mountain Crater Pot 4", "Death Mountain Crater Pot 4"),
                        MA("Scrub.png", 586, 178, 40, "OOT Death Mountain Crater Scrub Child", "child", "Death Mountain Crater Scrub Child"),
                        M("Red_Boulder.png", 629, 228, 24, "OOT Death Mountain Crater Red Boulder Shortcut", "Death Mountain Crater Red Boulder Shortcut"),
                        M("Red_Boulder.png", 716, 448, 24, "OOT Death Mountain Crater Red Boulder Grotto", "Death Mountain Crater Red Boulder Grotto"),
                        M("Red_Boulder.png", 680, 229, 24, "OOT Death Mountain Crater Red Boulder Great Fairy 1", "Death Mountain Crater Red Boulder Great Fairy 1"),
                        M("Red_Boulder.png", 663, 212, 24, "OOT Death Mountain Crater Red Boulder Great Fairy 2", "Death Mountain Crater Red Boulder Great Fairy 2"),
                        M("Rock.png", 420, 63, 24, "OOT Death Mountain Crater Rock Circle Rock 1", "Death Mountain Crater Rock Circle Rock 1"),
                        M("Rock.png", 405, 82, 24, "OOT Death Mountain Crater Rock Circle Rock 2", "Death Mountain Crater Rock Circle Rock 2"),
                        M("Rock.png", 413, 106, 24, "OOT Death Mountain Crater Rock Circle Rock 3", "Death Mountain Crater Rock Circle Rock 3"),
                        M("Rock.png", 435, 119, 24, "OOT Death Mountain Crater Rock Circle Rock 4", "Death Mountain Crater Rock Circle Rock 4"),
                        M("Rock.png", 459, 113, 24, "OOT Death Mountain Crater Rock Circle Rock 5", "Death Mountain Crater Rock Circle Rock 5"),
                        M("Rock.png", 474, 93, 24, "OOT Death Mountain Crater Rock Circle Rock 6", "Death Mountain Crater Rock Circle Rock 6"),
                        M("Rock.png", 465, 68, 24, "OOT Death Mountain Crater Rock Circle Rock 7", "Death Mountain Crater Rock Circle Rock 7"),
                        M("Rock.png", 444, 57, 24, "OOT Death Mountain Crater Rock Circle Rock 8", "Death Mountain Crater Rock Circle Rock 8"),
                        MA("Rock.png", 239, 99, 24, "OOT Death Mountain Crater Rock Child 1", "child", "Death Mountain Crater Rock Child 1"),
                        MA("Rock.png", 205, 99, 24, "OOT Death Mountain Crater Rock Child 2", "child", "Death Mountain Crater Rock Child 2"),
                        MA("Rock.png", 469, 473, 24, "OOT Death Mountain Crater Rock Adult 1", "adult", "Death Mountain Crater Rock Adult 1"),
                        MA("Rock.png", 471, 499, 24, "OOT Death Mountain Crater Rock Adult 2", "adult", "Death Mountain Crater Rock Adult 2"),
                        MA("Rock.png", 450, 487, 24, "OOT Death Mountain Crater Rock Adult 3", "adult", "Death Mountain Crater Rock Adult 3"),
                        MA("Rock.png", 454, 438, 24, "OOT Death Mountain Crater Rock Adult 4", "adult", "Death Mountain Crater Rock Adult 4"),
                        MA("Rock.png", 448, 462, 24, "OOT Death Mountain Crater Rock Adult 5", "adult", "Death Mountain Crater Rock Adult 5")
                    }
                },
                new MapSubRegion
                {
                    Name = "Deku Scrubs Grotto",
                    BackgroundImage = OoT("Death_Mountain_Crater", "Scrubs"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_SCRUBS3_DMC" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 676, 690, "Entrance shuffle (Death Mountain Crater)", "OOT_GROTTO_EXIT_SCRUBS3_DMC"),
                        M("Hive.png", 962, 194, 40, "OOT Death Mountain Crater Scrub Grotto Hive", "Death Mountain Crater Scrub Grotto Hive"),
                        M("Scrub.png", 450, 335, 40, "OOT Death Mountain Crater Grotto Left Scrub", "Death Mountain Crater Grotto Left Scrub"),
                        M("Scrub.png", 672, 94, 40, "OOT Death Mountain Crater Grotto Center Scrub", "Death Mountain Crater Grotto Center Scrub"),
                        M("Scrub.png", 922, 335, 40, "OOT Death Mountain Crater Grotto Right Scrub", "Death Mountain Crater Grotto Right Scrub")
                    }
                },
                new MapSubRegion
                {
                    Name = "Generic Grotto",
                    BackgroundImage = OoT("Death_Mountain_Crater", "Generic"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_GENERIC_DMC" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 651, 571, "Entrance shuffle (Death Mountain Crater)", "OOT_GROTTO_EXIT_GENERIC_DMC"),
                        M("Butterfly.png", 699, 613, 24, "OOT Death Mountain Crater Grotto Butterfly 1", "Death Mountain Crater Grotto Butterfly 1"),
                        M("Butterfly.png", 615, 613, 24, "OOT Death Mountain Crater Grotto Butterfly 2", "Death Mountain Crater Grotto Butterfly 2"),
                        M("Butterfly.png", 657, 626, 24, "OOT Death Mountain Crater Grotto Butterfly 3", "Death Mountain Crater Grotto Butterfly 3"),
                        M("Chest.png", 669, 162, 40, "OOT Death Mountain Crater Grotto", "Death Mountain Crater Grotto"),
                        M("Grass.png", 658, 134, 24, "OOT Death Mountain Crater Grotto Grass 1", "Death Mountain Crater Grotto Grass 1"),
                        M("Grass.png", 643, 186, 24, "OOT Death Mountain Crater Grotto Grass 2", "Death Mountain Crater Grotto Grass 2"),
                        M("Grass.png", 716, 186, 24, "OOT Death Mountain Crater Grotto Grass 3", "Death Mountain Crater Grotto Grass 3"),
                        M("Grass.png", 648, 316, 24, "OOT Death Mountain Crater Grotto Grass 4", "Death Mountain Crater Grotto Grass 4"),
                        M("Hive.png", 530, 141, 40, "OOT Death Mountain Crater Grotto Hive 1", "Death Mountain Crater Grotto Hive 1"),
                        M("Hive.png", 717, 79, 40, "OOT Death Mountain Crater Grotto Hive 2", "Death Mountain Crater Grotto Hive 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Great Fairy Fountain",
                    BackgroundImage = OoT("Fairy", "Great_Fairy"),
                    DestinationEntranceIds = new List<string> { "OOT_FAIRY_MAGIC2" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 480, 535, "Entrance shuffle (Death Mountain Crater)", "OOT_DEATH_CRATER_FROM_FAIRY"),
                        M("NPC.png", 482, 329, 40, "OOT Great Fairy Magic Upgrade 2", "Great Fairy Magic Upgrade 2")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion DeathMountainTrail()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Death Mountain Trail";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Death Mountain Trail - Bottom",
                    BackgroundImage = OoT("Death_Mountain_Trail", "Bottom"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_DEATH_MOUNTAIN_FROM_KAKARIKO",
						"OOT_MOUNTAIN_TRAIL_FROM_DODONGO_CAVERN",
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 20, 301, "Entrance shuffle (Dodongo Cavern)", "OOT_DODONGO_CAVERN"),
                        ME("Entrance.png", 877, 530, "Entrance shuffle (Kakariko)", "OOT_KAKARIKO_FROM_DEATH_MOUNTAIN"),
                        M("Gold_Skulltula.png", 783, 498, 40, "OOT Death Mountain Trail GS Entrance", "Death Mountain Trail GS Entrance"),
                        MA("Gold_Skulltula.png", 138, 66, 40, "OOT Death Mountain Trail GS Above Dodongo", "adult", "Death Mountain Trail GS Above Dodongo"),
                        MA("Gold_Skulltula.png", 103, 326, 40, "OOT Death Mountain Trail GS Soil", "child", "Death Mountain Trail GS Soil"),
                        MA("Soil.png", 79, 336, 24, "OOT Death Mountain Trail Soil 1", "child", "Death Mountain Trail Soil 1"),
                        MA("Soil.png", 87, 307, 24, "OOT Death Mountain Trail Soil 2", "child", "Death Mountain Trail Soil 2"),
                        MA("Soil.png", 113, 301, 24, "OOT Death Mountain Trail Soil 3", "child", "Death Mountain Trail Soil 3"),
                        M("Rock.png", 133, 375, 24, "OOT Death Mountain Trail Rock 1", "Death Mountain Trail Rock 1"),
                        M("Rock.png", 63, 354, 24, "OOT Death Mountain Trail Rock 2", "Death Mountain Trail Rock 2"),
                        M("Rock.png", 110, 368, 24, "OOT Death Mountain Trail Rock 3", "Death Mountain Trail Rock 3"),
                        M("Rock.png", 157, 382, 24, "OOT Death Mountain Trail Rock 4", "Death Mountain Trail Rock 4"),
                        M("Rock.png", 86, 361, 24, "OOT Death Mountain Trail Rock 5", "Death Mountain Trail Rock 5"),
                        M("Collectible.png", 29, 197, 40, "OOT Death Mountain Trail HP", "Death Mountain Trail HP"),
                        MA("Red_Boulder.png", 178, 72, 24, "OOT Death Mountain Trail Red Boulder Near Lower GS", "adult", "Death Mountain Trail Red Boulder Near Lower GS"),
                        MA("Red_Boulder.png", 909, 597, 24, "OOT Death Mountain Trail Red Boulder Lower 1", "adult", "Death Mountain Trail Red Boulder Lower 1"),
                        MA("Red_Boulder.png", 757, 593, 24, "OOT Death Mountain Trail Red Boulder Lower 2", "adult", "Death Mountain Trail Red Boulder Lower 2"),
                        MA("Red_Boulder.png", 213, 321, 24, "OOT Death Mountain Trail Red Boulder Lower 3", "adult", "Death Mountain Trail Red Boulder Lower 3"),
                        MA("Red_Boulder.png", 301, 326, 24, "OOT Death Mountain Trail Red Boulder Lower 4", "adult", "Death Mountain Trail Red Boulder Lower 4"),
                        MA("Red_Boulder.png", 575, 538, 24, "OOT Death Mountain Trail Red Boulder Lower 5", "adult", "Death Mountain Trail Red Boulder Lower 5"),
                        MA("Red_Boulder.png", 475, 275, 24, "OOT Death Mountain Trail Red Boulder Lower 6", "adult", "Death Mountain Trail Red Boulder Lower 6"),
                        MA("Red_Boulder.png", 820, 186, 24, "OOT Death Mountain Trail Red Boulder Lower 7", "adult", "Death Mountain Trail Red Boulder Lower 7")
                    }
                },
                new MapSubRegion
                {
                    Name = "Death Mountain Trail - Middle",
                    BackgroundImage = OoT("Death_Mountain_Trail", "Middle"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_GROTTO_EXIT_TRAIL_COW",
						"OOT_GROTTO_EXIT_GENERIC_DMT",
						"OOT_DEATH_MOUNTAIN_FROM_GORON_CITY"
					},
                    Marks = new List<MapMark>
                    {
						ME("Entrance.png", 718, 258, "Entrance shuffle (Song of Storms Grotto)", "OOT_GROTTO_GENERIC_DMT"),
                        ME("Entrance.png", 477, 144, "Entrance shuffle (Cow Grotto)", "OOT_GROTTO_TRAIL_COW"),
						ME("Entrance.png", 783, 226, "Entrance shuffle (Goron City)", "OOT_GORON_CITY"),
                        M("Rock.png", 708, 296, 24, "OOT Death Mountain Trail Rock Circle Rock 1", "Death Mountain Trail Rock Circle Rock 1"),
                        M("Rock.png", 732, 298, 24, "OOT Death Mountain Trail Rock Circle Rock 2", "Death Mountain Trail Rock Circle Rock 2"),
                        M("Rock.png", 753, 284, 24, "OOT Death Mountain Trail Rock Circle Rock 3", "Death Mountain Trail Rock Circle Rock 3"),
                        M("Rock.png", 761, 260, 24, "OOT Death Mountain Trail Rock Circle Rock 4", "Death Mountain Trail Rock Circle Rock 4"),
                        M("Rock.png", 745, 240, 24, "OOT Death Mountain Trail Rock Circle Rock 5", "Death Mountain Trail Rock Circle Rock 5"),
                        M("Rock.png", 718, 239, 24, "OOT Death Mountain Trail Rock Circle Rock 6", "Death Mountain Trail Rock Circle Rock 6"),
                        M("Rock.png", 695, 253, 24, "OOT Death Mountain Trail Rock Circle Rock 7", "Death Mountain Trail Rock Circle Rock 7"),
                        M("Rock.png", 688, 278, 24, "OOT Death Mountain Trail Rock Circle Rock 8", "Death Mountain Trail Rock Circle Rock 8"),
                        M("Chest.png", 482, 356, 40, "OOT Death Mountain Trail Chest", "Death Mountain Trail Chest"),
                        M("Fairy_Spot.png", 257, 470, 40, "OOT Death Mountain Trail Big Fairy", "Death Mountain Trail Big Fairy"),
                        MA("Red_Boulder.png", 871, 94, 24, "OOT Death Mountain Trail Red Boulder Upper 1", "adult", "Death Mountain Trail Red Boulder Upper 1"),
                        MA("Red_Boulder.png", 858, 47, 24, "OOT Death Mountain Trail Red Boulder Upper 2", "adult", "Death Mountain Trail Red Boulder Upper 2"),
                        MA("Rupee.png", 330, 239, 24, "OOT Death Mountain Trail Rupee Lower", "child", "Death Mountain Trail Rupee Lower"),
                        MA("Rupee.png", 456, 247, 24, "OOT Death Mountain Trail Rupee Upper", "child", "Death Mountain Trail Rupee Upper")
                    }
                },
                new MapSubRegion
                {
                    Name = "Death Mountain Trail - Top",
                    BackgroundImage = OoT("Death_Mountain_Trail", "Top"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_DEATH_MOUNTAIN_FROM_FAIRY",
						"OOT_TRAIL_SUMMIT_FROM_CRATER"
					},
                    Marks = new List<MapMark>
                    {
						ME("Entrance.png", 456, 55, "Entrance shuffle (Death Mountain Crater)", "OOT_DEATH_MOUNTAIN_CRATER"),
                        ME("Entrance.png", 379, 55, "Entrance shuffle (Fairy Fountain)", "OOT_FAIRY_MAGIC"),
                        MA("Rock.png", 432, 84, 24, "OOT Death Mountain Trail Rock Child", "child", "Death Mountain Trail Rock Child"),
                        MA("NPC.png", 560, 48, 40, "OOT Death Mountain Trail Biggoron Sword", "adult", "Death Mountain Trail Biggoron Sword"),
                        MA("NPC.png", 540, 86, 40, "OOT Death Mountain Trail Claim Check", "adult", "Death Mountain Trail Claim Check"),
                        MA("NPC.png", 583, 86, 40, "OOT Death Mountain Trail Prescription", "adult", "Death Mountain Trail Prescription"),
                        MA("Gold_Skulltula.png", 675, 509, 40, "OOT Death Mountain Trail GS Before Climb", "adult", "Death Mountain Trail GS Before Climb"),
                        MA("Red_Boulder.png", 660, 496, 24, "OOT Death Mountain Trail Red Boulder Near Upper GS", "adult", "Death Mountain Trail Red Boulder Near Upper GS")
                    }
                },
                new MapSubRegion
                {
                    Name = "Cow Grotto",
                    BackgroundImage = OoT("Death_Mountain_Trail", "Cow"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_TRAIL_COW" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 677, 600, "Entrance shuffle (Death Mountain Trail)", "OOT_GROTTO_EXIT_TRAIL_COW"),
                        M("Rupee.png", 688, 456, 24, "OOT Death Mountain Trail Cow Grotto Rupee 1", "Death Mountain Trail Cow Grotto Rupee 1"),
                        M("Rupee.png", 709, 442, 24, "OOT Death Mountain Trail Cow Grotto Rupee 2", "Death Mountain Trail Cow Grotto Rupee 2"),
                        M("Rupee.png", 709, 412, 24, "OOT Death Mountain Trail Cow Grotto Rupee 3", "Death Mountain Trail Cow Grotto Rupee 3"),
                        M("Rupee.png", 688, 398, 24, "OOT Death Mountain Trail Cow Grotto Rupee 4", "Death Mountain Trail Cow Grotto Rupee 4"),
                        M("Rupee.png", 666, 412, 24, "OOT Death Mountain Trail Cow Grotto Rupee 5", "Death Mountain Trail Cow Grotto Rupee 5"),
                        M("Rupee.png", 666, 442, 24, "OOT Death Mountain Trail Cow Grotto Rupee 6", "Death Mountain Trail Cow Grotto Rupee 6"),
                        M("Rupee.png", 688, 427, 24, "OOT Death Mountain Trail Cow Grotto Rupee 7", "Death Mountain Trail Cow Grotto Rupee 7"),
                        M("Cow.png", 680, 189, 40, "OOT Death Mountain Trail Cow", "Death Mountain Trail Cow"),
                        M("Heart.png", 614, 208, 24, "OOT Death Mountain Trail Cow Grotto Heart 1", "Death Mountain Trail Cow Grotto Heart 1"),
                        M("Heart.png", 659, 151, 24, "OOT Death Mountain Trail Cow Grotto Heart 2", "Death Mountain Trail Cow Grotto Heart 2"),
                        M("Heart.png", 726, 151, 24, "OOT Death Mountain Trail Cow Grotto Heart 3", "Death Mountain Trail Cow Grotto Heart 3"),
                        M("Heart.png", 780, 208, 24, "OOT Death Mountain Trail Cow Grotto Heart 4", "Death Mountain Trail Cow Grotto Heart 4"),
                        M("Hive.png", 805, 145, 40, "OOT Death Mountain Trail Cow Grotto Hive", "Death Mountain Trail Cow Grotto Hive"),
                        M("Grass.png", 704, 235, 24, "OOT Death Mountain Trail Cow Grotto Grass 1", "Death Mountain Trail Cow Grotto Grass 1"),
                        M("Grass.png", 735, 207, 24, "OOT Death Mountain Trail Cow Grotto Grass 2", "Death Mountain Trail Cow Grotto Grass 2"),
                        M("Fairy_Spot.png", 730, 282, 40, "OOT Death Mountain Trail Cow Grotto Big Fairy", "Death Mountain Trail Cow Grotto Big Fairy")
                    }
                },
                new MapSubRegion
                {
                    Name = "Song of Storms Grotto",
                    BackgroundImage = OoT("Death_Mountain_Trail", "Storms"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_GENERIC_DMT" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 651, 571, "Entrance shuffle (Death Mountain Trail)", "OOT_GROTTO_EXIT_GENERIC_DMT"),
                        M("Butterfly.png", 699, 613, 24, "OOT Death Mountain Trail Grotto Butterfly 1", "Death Mountain Trail Grotto Butterfly 1"),
                        M("Butterfly.png", 615, 613, 24, "OOT Death Mountain Trail Grotto Butterfly 2", "Death Mountain Trail Grotto Butterfly 2"),
                        M("Butterfly.png", 657, 626, 24, "OOT Death Mountain Trail Grotto Butterfly 3", "Death Mountain Trail Grotto Butterfly 3"),
                        M("Chest.png", 669, 162, 40, "OOT Death Mountain Trail Grotto", "Death Mountain Trail Grotto"),
                        M("Grass.png", 658, 134, 24, "OOT Death Mountain Trail Grotto Grass 1", "Death Mountain Trail Grotto Grass 1"),
                        M("Grass.png", 643, 186, 24, "OOT Death Mountain Trail Grotto Grass 2", "Death Mountain Trail Grotto Grass 2"),
                        M("Grass.png", 716, 186, 24, "OOT Death Mountain Trail Grotto Grass 3", "Death Mountain Trail Grotto Grass 3"),
                        M("Grass.png", 648, 316, 24, "OOT Death Mountain Trail Grotto Grass 4", "Death Mountain Trail Grotto Grass 4"),
                        M("Hive.png", 530, 141, 40, "OOT Death Mountain Trail Grotto Hive 1", "Death Mountain Trail Grotto Hive 1"),
                        M("Hive.png", 717, 79, 40, "OOT Death Mountain Trail Grotto Hive 2", "Death Mountain Trail Grotto Hive 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Great Fairy Fountain",
                    BackgroundImage = OoT("Fairy", "Great_Fairy"),
                    DestinationEntranceIds = new List<string> { "OOT_FAIRY_MAGIC" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 480, 535, "Entrance shuffle (Death Mountain Trail)", "OOT_DEATH_MOUNTAIN_FROM_FAIRY"),
                        M("NPC.png", 482, 329, 40, "OOT Great Fairy Magic Upgrade", "Great Fairy Magic Upgrade")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion DesertColossus()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Desert Colossus";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Desert Colossus",
                    BackgroundImage = OoT("Desert_Colossus", "Desert_Colossus"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_DESERT_COLOSSUS_FROM_FAIRY",
						"OOT_GROTTO_EXIT_SCRUBS2_COLOSSUS",
						"OOT_DESERT_COLOSSUS_FROM_TEMPLE_SPIRIT",
						"OOT_WARP_SONG_DESERT",
						"OOT_COLOSSUS"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 703, 179, "Entrance shuffle (Spirit Temple)", "OOT_TEMPLE_SPIRIT"),
                        ME("Entrance.png", 85, 529, "Entrance shuffle (Haunted Wasteland)", "OOT_WASTELAND_FROM_COLOSSUS"),
                        ME("Entrance.png", 361, 582, "Entrance shuffle (Great Fairy Fountain)", "OOT_FAIRY_NAYRU"),
                        ME("Entrance.png", 747, 404, "Entrance shuffle (Deku Scrubs Grotto)", "OOT_GROTTO_SCRUBS2_COLOSSUS"),
                        
                        MA("Wonder.png", 361, 38, 24, "OOT Desert Colossus Wonder Item Oasis Child", "child", "Desert Colossus Wonder Item Oasis Child"),
                        M("Wonder.png", 271, 45, 24, "OOT Desert Colossus Wonder Item Oasis 1", "Desert Colossus Wonder Item Oasis 1"),
                        M("Wonder.png", 277, 69, 24, "OOT Desert Colossus Wonder Item Oasis 2", "Desert Colossus Wonder Item Oasis 2"),
                        M("Wonder.png", 378, 572, 24, "OOT Desert Colossus Wonder Item Fountain 1", "Desert Colossus Wonder Item Fountain 1"),
                        M("Wonder.png", 343, 592, 24, "OOT Desert Colossus Wonder Item Fountain 2", "Desert Colossus Wonder Item Fountain 2"),
                        M("Chest.png", 726, 123, 40, "OOT Spirit Temple Silver Gauntlets", "Spirit Temple Silver Gauntlets"),
                        M("Chest.png", 760, 184, 40, "OOT Spirit Temple Mirror Shield", "Spirit Temple Mirror Shield"),
                        M("NPC.png", 685, 194, 40, "OOT Desert Colossus Song Spirit", "Desert Colossus Song Spirit"),
                        MA("Gold_Skulltula.png", 660, 160, 40, "OOT Desert Colossus GS Soil", "child", "Desert Colossus GS Soil"),
                        MA("Gold_Skulltula.png", 354, 30, 40, "OOT Desert Colossus GS Tree", "adult", "Desert Colossus GS Tree"),
                        MA("Gold_Skulltula.png", 412, 401, 40, "OOT Desert Colossus GS Plateau", "adult", "Desert Colossus GS Plateau"),
                        MA("Soil.png", 658, 199, 24, "OOT Desert Colossus Soil 1", "child", "Desert Colossus Soil 1"),
                        MA("Soil.png", 635, 184, 24, "OOT Desert Colossus Soil 2", "child", "Desert Colossus Soil 2"),
                        MA("Soil.png", 635, 155, 24, "OOT Desert Colossus Soil 3", "child", "Desert Colossus Soil 3"),
                        M("Collectible.png", 616, 225, 40, "OOT Desert Colossus HP", "Desert Colossus HP"),
                        M("Rock.png", 300, 333, 24, "OOT Desert Colossus Rock", "Desert Colossus Rock"),
                        M("Rock.png", 682, 309, 24, "OOT Desert Colossus Rock Circle 1 Rock 1", "Desert Colossus Rock Circle 1 Rock 1"),
                        M("Rock.png", 665, 332, 24, "OOT Desert Colossus Rock Circle 1 Rock 2", "Desert Colossus Rock Circle 1 Rock 2"),
                        M("Rock.png", 671, 361, 24, "OOT Desert Colossus Rock Circle 1 Rock 3", "Desert Colossus Rock Circle 1 Rock 3"),
                        M("Rock.png", 695, 380, 24, "OOT Desert Colossus Rock Circle 1 Rock 4", "Desert Colossus Rock Circle 1 Rock 4"),
                        M("Rock.png", 725, 374, 24, "OOT Desert Colossus Rock Circle 1 Rock 5", "Desert Colossus Rock Circle 1 Rock 5"),
                        M("Rock.png", 742, 351, 24, "OOT Desert Colossus Rock Circle 1 Rock 6", "Desert Colossus Rock Circle 1 Rock 6"),
                        M("Rock.png", 740, 321, 24, "OOT Desert Colossus Rock Circle 1 Rock 7", "Desert Colossus Rock Circle 1 Rock 7"),
                        M("Rock.png", 714, 302, 24, "OOT Desert Colossus Rock Circle 1 Rock 8", "Desert Colossus Rock Circle 1 Rock 8"),
                        M("Rock.png", 631, 392, 24, "OOT Desert Colossus Rock Circle 2 Rock 1", "Desert Colossus Rock Circle 2 Rock 1"),
                        M("Rock.png", 614, 415, 24, "OOT Desert Colossus Rock Circle 2 Rock 2", "Desert Colossus Rock Circle 2 Rock 2"),
                        M("Rock.png", 620, 444, 24, "OOT Desert Colossus Rock Circle 2 Rock 3", "Desert Colossus Rock Circle 2 Rock 3"),
                        M("Rock.png", 644, 463, 24, "OOT Desert Colossus Rock Circle 2 Rock 4", "Desert Colossus Rock Circle 2 Rock 4"),
                        M("Rock.png", 674, 457, 24, "OOT Desert Colossus Rock Circle 2 Rock 5", "Desert Colossus Rock Circle 2 Rock 5"),
                        M("Rock.png", 691, 434, 24, "OOT Desert Colossus Rock Circle 2 Rock 6", "Desert Colossus Rock Circle 2 Rock 6"),
                        M("Rock.png", 689, 404, 24, "OOT Desert Colossus Rock Circle 2 Rock 7", "Desert Colossus Rock Circle 2 Rock 7"),
                        M("Rock.png", 663, 385, 24, "OOT Desert Colossus Rock Circle 2 Rock 8", "Desert Colossus Rock Circle 2 Rock 8"),
                        M("Fairy.png", 332, 106, 24, "OOT Desert Colossus Oasis Fairy 1", "Desert Colossus Oasis Fairy 1"),
                        M("Fairy.png", 355, 103, 24, "OOT Desert Colossus Oasis Fairy 2", "Desert Colossus Oasis Fairy 2"),
                        M("Fairy.png", 309, 97, 24, "OOT Desert Colossus Oasis Fairy 3", "Desert Colossus Oasis Fairy 3"),
                        M("Fairy.png", 334, 82, 24, "OOT Desert Colossus Oasis Fairy 4", "Desert Colossus Oasis Fairy 4"),
                        M("Fairy.png", 364, 81, 24, "OOT Desert Colossus Oasis Fairy 5", "Desert Colossus Oasis Fairy 5"),
                        M("Fairy.png", 305, 74, 24, "OOT Desert Colossus Oasis Fairy 6", "Desert Colossus Oasis Fairy 6"),
                        M("Fairy.png", 346, 62, 24, "OOT Desert Colossus Oasis Fairy 7", "Desert Colossus Oasis Fairy 7"),
                        M("Fairy.png", 322, 56, 24, "OOT Desert Colossus Oasis Fairy 8", "Desert Colossus Oasis Fairy 8")
                    }
                },
                new MapSubRegion
                {
                    Name = "Deku Scrubs Grotto",
                    BackgroundImage = OoT("Desert_Colossus", "Scrubs"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_SCRUBS2_COLOSSUS" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 655, 630, "Entrance shuffle (Desert Colossus)", "OOT_GROTTO_EXIT_SCRUBS2_COLOSSUS"),
                        M("Hive.png", 529, 134, 40, "OOT Desert Colossus Grotto Hive", "Desert Colossus Grotto Hive"),
                        M("Scrub.png", 698, 282, 40, "OOT Desert Colossus Grotto Front Scrub", "Desert Colossus Grotto Front Scrub"),
                        M("Scrub.png", 632, 188, 40, "OOT Desert Colossus Grotto Back Scrub", "Desert Colossus Grotto Back Scrub")
                    }
                },
                new MapSubRegion
                {
                    Name = "Great Fairy Fountain",
                    BackgroundImage = OoT("Fairy", "Great_Fairy"),
                    DestinationEntranceIds = new List<string> { "OOT_FAIRY_NAYRU" },
                    Marks = new List<MapMark> 
                    { 
                        ME("Entrance.png", 480, 535, "Entrance shuffle (Desert Colossus)", "OOT_DESERT_COLOSSUS_FROM_FAIRY"),
                        M("NPC.png", 482, 329, 40, "OOT Great Fairy Nayru's Love", "Great Fairy Nayru's Love") 
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion GerudoFortress()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Gerudo Fortress";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Gerudo Fortress",
                    BackgroundImage = OoT("Gerudo_Fortress", "Fortress"),
					DestinationEntranceIds = new List<string>
					{
						"OOT_GROTTO_EXIT_FAIRY_FORTRESS",
						"OOT_GERUDO_FORTRESS_FROM_VALLEY",
						"OOT_GERUDO_FORTRESS_FROM_GERUDO_TRAINING",
						"OOT_FORTRESS_FROM_WASTELAND"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 574, 508, "Entrance shuffle (Gerudo Training Grounds)", "OOT_GERUDO_TRAINING_GROUNDS"),
                        ME("Entrance.png", 648, 575, "Entrance shuffle (Gerudo Valley)", "OOT_VALLEY_FROM_GERUDO_FORTRESS"),
                        ME("Entrance.png", 377, 581, "Entrance shuffle (Haunted Wasteland)", "OOT_WASTELAND"),
                        ME("Entrance.png", 515, 435, "Entrance shuffle (Fairy Fountain)", "OOT_GROTTO_FAIRY_FORTRESS"),
                        
                        M("Wonder.png", 595, 583, 24, "OOT Gerudo Fortress Wonder Item Sign Entrance", "Gerudo Fortress Wonder Item Sign Entrance"),
                        M("Wonder.png", 640, 401, 24, "OOT Gerudo Fortress Wonder Item Sign Archery", "Gerudo Fortress Wonder Item Sign Archery"),
                        M("Chest.png", 420, 408, 40, "OOT Gerudo Fortress Chest", "Gerudo Fortress Chest"),
                        MA("Gold_Skulltula.png", 477, 341, 40, "OOT Gerudo Fortress GS Wall", "adult", "Gerudo Fortress GS Wall"),
                        MA("Gold_Skulltula.png", 156, 142, 40, "OOT Gerudo Fortress GS Target", "adult", "Gerudo Fortress GS Target"),
                        MA("NPC.png", 627, 138, 40, "OOT Gerudo Fortress Archery Reward 1", "adult", "Gerudo Fortress Archery Reward 1"),
                        MA("NPC.png", 627, 180, 40, "OOT Gerudo Fortress Archery Reward 2", "adult", "Gerudo Fortress Archery Reward 2"),
                        M("Crate.png", 168, 180, 24, "OOT Gerudo Fortress Crate Archery 01", "Gerudo Fortress Crate Archery 01"),
                        M("Crate.png", 741, 171, 24, "OOT Gerudo Fortress Crate Archery 02", "Gerudo Fortress Crate Archery 02"),
                        M("Crate.png", 741, 147, 24, "OOT Gerudo Fortress Crate Archery 03", "Gerudo Fortress Crate Archery 03"),
                        M("Crate.png", 195, 159, 24, "OOT Gerudo Fortress Crate Archery 04", "Gerudo Fortress Crate Archery 04"),
                        M("Crate.png", 670, 157, 24, "OOT Gerudo Fortress Crate Archery 05", "Gerudo Fortress Crate Archery 05"),
                        M("Crate.png", 670, 133, 24, "OOT Gerudo Fortress Crate Archery 06", "Gerudo Fortress Crate Archery 06"),
                        M("Crate.png", 384, 91, 24, "OOT Gerudo Fortress Crate Archery 07", "Gerudo Fortress Crate Archery 07"),
                        M("Crate.png", 408, 91, 24, "OOT Gerudo Fortress Crate Archery 08", "Gerudo Fortress Crate Archery 08"),
                        M("Crate.png", 432, 91, 24, "OOT Gerudo Fortress Crate Archery 09", "Gerudo Fortress Crate Archery 09"),
                        M("Crate.png", 456, 91, 24, "OOT Gerudo Fortress Crate Archery 10", "Gerudo Fortress Crate Archery 10"),
                        M("Crate.png", 480, 91, 24, "OOT Gerudo Fortress Crate Archery 11", "Gerudo Fortress Crate Archery 11"),
                        M("Crate.png", 504, 91, 24, "OOT Gerudo Fortress Crate Archery 12", "Gerudo Fortress Crate Archery 12"),
                        M("Crate.png", 528, 91, 24, "OOT Gerudo Fortress Crate Archery 13", "Gerudo Fortress Crate Archery 13"),
                        M("Crate.png", 387, 516, 24, "OOT Gerudo Fortress Crate Jail Top", "Gerudo Fortress Crate Jail Top"),
                        M("Crate.png", 463, 532, 24, "OOT Gerudo Fortress Crate Main 1", "Gerudo Fortress Crate Main 1"),
                        M("Crate.png", 463, 508, 24, "OOT Gerudo Fortress Crate Main 2", "Gerudo Fortress Crate Main 2"),
                        M("Crate.png", 477, 470, 24, "OOT Gerudo Fortress Crate Main 3", "Gerudo Fortress Crate Main 3"),
                        M("Crate.png", 501, 470, 24, "OOT Gerudo Fortress Crate Main 4", "Gerudo Fortress Crate Main 4"),
                        M("Crate.png", 531, 470, 24, "OOT Gerudo Fortress Crate Main 5", "Gerudo Fortress Crate Main 5"),
                        M("Crate.png", 555, 470, 24, "OOT Gerudo Fortress Crate Main 6", "Gerudo Fortress Crate Main 6")
                    }
                },
                new MapSubRegion
                {
                    Name = "Thieves Hideout",
                    BackgroundImage = OoT("Gerudo_Fortress", "Hideout"),
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 596, 398, 40, "OOT Gerudo Fortress Jail 1", "Gerudo Fortress Jail 1"),
                        M("Collectible.png", 506, 48, 40, "OOT Gerudo Fortress Jail 2", "Gerudo Fortress Jail 2"),
                        M("Collectible.png", 292, 463, 40, "OOT Gerudo Fortress Jail 3", "Gerudo Fortress Jail 3"),
                        M("Collectible.png", 542, 225, 40, "OOT Gerudo Fortress Jail 4", "Gerudo Fortress Jail 4"),
                        M("NPC.png", 252, 459, 40, "OOT Gerudo Member Card", "Gerudo Member Card"),
                        M("Fairy_Spot.png", 251, 132, 40, "OOT Gerudo Fortress Kitchen Big Fairy", "Gerudo Fortress Kitchen Big Fairy"),
                        M("Wonder.png", 609, 371, 24, "OOT Gerudo Fortress Wonder Item Jail 1 1", "Gerudo Fortress Wonder Item Jail 1 1"),
                        M("Wonder.png", 609, 432, 24, "OOT Gerudo Fortress Wonder Item Jail 1 2", "Gerudo Fortress Wonder Item Jail 1 2"),
                        M("Wonder.png", 571, 50, 24, "OOT Gerudo Fortress Wonder Item Jail 2 1", "Gerudo Fortress Wonder Item Jail 2 1"),
                        M("Wonder.png", 468, 50, 24, "OOT Gerudo Fortress Wonder Item Jail 2 2", "Gerudo Fortress Wonder Item Jail 2 2"),
                        M("Wonder.png", 334, 388, 24, "OOT Gerudo Fortress Wonder Item Jail 3 1", "Gerudo Fortress Wonder Item Jail 3 1"),
                        M("Wonder.png", 373, 460, 24, "OOT Gerudo Fortress Wonder Item Jail 3 2", "Gerudo Fortress Wonder Item Jail 3 2"),
                        M("Wonder.png", 552, 261, 24, "OOT Gerudo Fortress Wonder Item Jail 4 1", "Gerudo Fortress Wonder Item Jail 4 1"),
                        M("Wonder.png", 552, 199, 24, "OOT Gerudo Fortress Wonder Item Jail 4 2", "Gerudo Fortress Wonder Item Jail 4 2"),
                        M("Wonder.png", 451, 562, 24, "OOT Gerudo Fortress Wonder Item Break Room Bottom", "Gerudo Fortress Wonder Item Break Room Bottom"),
                        M("Wonder.png", 569, 562, 24, "OOT Gerudo Fortress Wonder Item Break Room Top", "Gerudo Fortress Wonder Item Break Room Top"),
                        M("Wonder.png", 400, 148, 24, "OOT Gerudo Fortress Wonder Item Kitchen Skull", "Gerudo Fortress Wonder Item Kitchen Skull"),
                        M("Wonder.png", 300, 148, 24, "OOT Gerudo Fortress Wonder Item Kitchen Soup", "Gerudo Fortress Wonder Item Kitchen Soup"),
                        M("Crate.png", 636, 379, 24, "OOT Thieves Hideout Crate Jail 1", "Thieves Hideout Crate Jail 1"),
                        M("Crate.png", 547, 76, 24, "OOT Thieves Hideout Crate Jail 2 1", "Thieves Hideout Crate Jail 2 1"),
                        M("Crate.png", 571, 76, 24, "OOT Thieves Hideout Crate Jail 2 2", "Thieves Hideout Crate Jail 2 2"),
                        M("Crate.png", 418, 373, 24, "OOT Thieves Hideout Crate Jail 3", "Thieves Hideout Crate Jail 3"),
                        M("Crate.png", 340, 157, 24, "OOT Thieves Hideout Crate Kitchen", "Thieves Hideout Crate Kitchen"),
                        M("Crate.png", 392, 171, 24, "OOT Thieves Hideout Crate Kitchen Corridor 1", "Thieves Hideout Crate Kitchen Corridor 1"),
                        M("Crate.png", 392, 195, 24, "OOT Thieves Hideout Crate Kitchen Corridor 2", "Thieves Hideout Crate Kitchen Corridor 2"),
                        M("Crate.png", 409, 249, 24, "OOT Thieves Hideout Crate Kitchen Corridor 3", "Thieves Hideout Crate Kitchen Corridor 3"),
                        M("Crate.png", 409, 225, 24, "OOT Thieves Hideout Crate Kitchen Corridor 4", "Thieves Hideout Crate Kitchen Corridor 4"),
                        M("Crate.png", 340, 566, 24, "OOT Thieves Hideout Crate Break Room 1", "Thieves Hideout Crate Break Room 1"),
                        M("Crate.png", 340, 542, 24, "OOT Thieves Hideout Crate Break Room 2", "Thieves Hideout Crate Break Room 2"),
                        M("Crate.png", 374, 528, 24, "OOT Thieves Hideout Crate Break Room 3", "Thieves Hideout Crate Break Room 3"),
                        M("Crate.png", 394, 502, 24, "OOT Thieves Hideout Crate Break Room 4", "Thieves Hideout Crate Break Room 4"),
                        M("Pot.png", 631, 431, 24, "OOT Gerudo Fortress Pot Jail 1 1", "Gerudo Fortress Pot Jail 1 1"),
                        M("Pot.png", 654, 406, 24, "OOT Gerudo Fortress Pot Jail 1 2", "Gerudo Fortress Pot Jail 1 2"),
                        M("Pot.png", 654, 431, 24, "OOT Gerudo Fortress Pot Jail 1 3", "Gerudo Fortress Pot Jail 1 3"),
                        M("Pot.png", 482, 99, 24, "OOT Gerudo Fortress Pot Jail 2 1", "Gerudo Fortress Pot Jail 2 1"),
                        M("Pot.png", 459, 74, 24, "OOT Gerudo Fortress Pot Jail 2 2", "Gerudo Fortress Pot Jail 2 2"),
                        M("Pot.png", 459, 99, 24, "OOT Gerudo Fortress Pot Jail 2 3", "Gerudo Fortress Pot Jail 2 3"),
                        M("Pot.png", 509, 4, 24, "OOT Gerudo Fortress Pot Jail 2 4", "Gerudo Fortress Pot Jail 2 4"),
                        M("Pot.png", 532, 4, 24, "OOT Gerudo Fortress Pot Jail 2 5", "Gerudo Fortress Pot Jail 2 5"),
                        M("Pot.png", 578, 4, 24, "OOT Gerudo Fortress Pot Jail 2 6", "Gerudo Fortress Pot Jail 2 6"),
                        M("Pot.png", 555, 4, 24, "OOT Gerudo Fortress Pot Jail 2 7", "Gerudo Fortress Pot Jail 2 7"),
                        M("Pot.png", 545, 340, 24, "OOT Gerudo Fortress Pot Jail 3 1", "Gerudo Fortress Pot Jail 3 1"),
                        M("Pot.png", 566, 322, 24, "OOT Gerudo Fortress Pot Jail 3 2", "Gerudo Fortress Pot Jail 3 2"),
                        M("Pot.png", 312, 169, 24, "OOT Gerudo Fortress Pot Kitchen 1", "Gerudo Fortress Pot Kitchen 1"),
                        M("Pot.png", 293, 185, 24, "OOT Gerudo Fortress Pot Kitchen 2", "Gerudo Fortress Pot Kitchen 2"),
                        M("Pot.png", 376, 562, 24, "OOT Gerudo Fortress Pot Break Room 1", "Gerudo Fortress Pot Break Room 1"),
                        M("Pot.png", 361, 584, 24, "OOT Gerudo Fortress Pot Break Room 2", "Gerudo Fortress Pot Break Room 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Fairy Fountain",
                    BackgroundImage = OoT("Fairy", "Fountain"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_FAIRY_FORTRESS" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 465, 502, "Entrance shuffle (Gerudo Fortress)", "OOT_GROTTO_EXIT_FAIRY_FORTRESS"),
                        M("Fairy.png", 470, 204, 24, "OOT Gerudo Fortress Fairy Fountain Fairy 1", "Gerudo Fortress Fairy Fountain Fairy 1"),
                        M("Fairy.png", 490, 200, 24, "OOT Gerudo Fortress Fairy Fountain Fairy 2", "Gerudo Fortress Fairy Fountain Fairy 2"),
                        M("Fairy.png", 450, 194, 24, "OOT Gerudo Fortress Fairy Fountain Fairy 3", "Gerudo Fortress Fairy Fountain Fairy 3"),
                        M("Fairy.png", 471, 181, 24, "OOT Gerudo Fortress Fairy Fountain Fairy 4", "Gerudo Fortress Fairy Fountain Fairy 4"),
                        M("Fairy.png", 493, 178, 24, "OOT Gerudo Fortress Fairy Fountain Fairy 5", "Gerudo Fortress Fairy Fountain Fairy 5"),
                        M("Fairy.png", 449, 172, 24, "OOT Gerudo Fortress Fairy Fountain Fairy 6", "Gerudo Fortress Fairy Fountain Fairy 6"),
                        M("Fairy.png", 485, 155, 24, "OOT Gerudo Fortress Fairy Fountain Fairy 7", "Gerudo Fortress Fairy Fountain Fairy 7"),
                        M("Fairy.png", 464, 153, 24, "OOT Gerudo Fortress Fairy Fountain Fairy 8", "Gerudo Fortress Fairy Fountain Fairy 8")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion GerudoValley()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Gerudo Valley";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Gerudo Valley",
                    BackgroundImage = OoT("Gerudo_Valley", "Valley"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_GROTTO_EXIT_OCTOROK",
						"OOT_GERUDO_VALLEY_FROM_FIELD",
						"OOT_VALLEY_FROM_GERUDO_FORTRESS",
						"OOT_GROTTO_EXIT_SCRUBS2_VALLEY",
						"OOT_GERUDO_VALLEY_FROM_TENT"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 886, 443, "Entrance shuffle (Hyrule Field)", "OOT_FIELD_FROM_GERUDO_VALLEY"),
                        ME("Entrance.png", 6, 274, "Entrance shuffle (Gerudo Fortress)", "OOT_GERUDO_FORTRESS_FROM_VALLEY"),
                        MEA("Entrance.png", 335, 239, "Entrance shuffle (Carpenter Tent)", "adult", "OOT_VALLEY_TENT"),
                        MEA("Entrance.png", 296, 148, "Entrance shuffle (Song of Storms Grotto)", "adult", "OOT_GROTTO_SCRUBS2_VALLEY"),
                        ME("Entrance.png", 499, 529, "Entrance shuffle (Octorok Grotto)", "OOT_GROTTO_OCTOROK"),
                        ME("Entrance.png", 470, 577, "Entrance shuffle (Lake Hylia - one-way)", "OOT_LAKE_HYLIA_FROM_VALLEY"),
                        
                        MA("Gold_Skulltula.png", 732, 269, 40, "OOT Gerudo Valley GS Wall", "child", "Gerudo Valley GS Wall"),
                        MA("Gold_Skulltula.png", 372, 333, 40, "OOT Gerudo Valley GS Soil", "child", "Gerudo Valley GS Soil"),
                        MA("Gold_Skulltula.png", 255, 126, 40, "OOT Gerudo Valley GS Tent", "adult", "Gerudo Valley GS Tent"),
                        MA("Gold_Skulltula.png", 218, 379, 40, "OOT Gerudo Valley GS Pillar", "adult", "Gerudo Valley GS Pillar"),
                        M("Crate.png", 413, 557, 24, "OOT Gerudo Valley Crate Ledge", "Gerudo Valley Crate Ledge"),
                        MA("Crate.png", 413, 343, 24, "OOT Gerudo Valley Crate Child Bottom", "child", "Gerudo Valley Crate Child Bottom"),
                        M("Collectible.png", 402, 518, 40, "OOT Gerudo Valley Crate HP", "Gerudo Valley Crate HP"),
                        M("Collectible.png", 453, 21, 40, "OOT Gerudo Valley Waterfall HP", "Gerudo Valley Waterfall HP"),
                        MA("Soil.png", 404, 315, 24, "OOT Gerudo Valley Soil 1", "child", "Gerudo Valley Soil 1"),
                        MA("Soil.png", 357, 315, 24, "OOT Gerudo Valley Soil 2", "child", "Gerudo Valley Soil 2"),
                        MA("Soil.png", 381, 310, 24, "OOT Gerudo Valley Soil 3", "child", "Gerudo Valley Soil 3"),
                        MA("Cow.png", 394, 275, 40, "OOT Gerudo Valley Cow", "child", "Gerudo Valley Cow"),
                        MA("Wonder.png", 461, 313, 24, "OOT Gerudo Valley Wonder Item Lower", "adult", "Gerudo Valley Wonder Item Lower"),
                        MA("Wonder.png", 466, 63, 24, "OOT Gerudo Valley Wonder Item Upper", "adult", "Gerudo Valley Wonder Item Upper"),
                        M("Rock.png", 880, 367, 24, "OOT Gerudo Valley Rock Entrance Ground 1", "Gerudo Valley Rock Entrance Ground 1"),
                        M("Rock.png", 873, 393, 24, "OOT Gerudo Valley Rock Entrance Ground 2", "Gerudo Valley Rock Entrance Ground 2"),
                        M("Rock.png", 855, 374, 24, "OOT Gerudo Valley Rock Entrance Ground 3", "Gerudo Valley Rock Entrance Ground 3"),
                        M("Rock.png", 707, 302, 24, "OOT Gerudo Valley Rock Entrance Water 1", "Gerudo Valley Rock Entrance Water 1"),
                        M("Rock.png", 710, 341, 24, "OOT Gerudo Valley Rock Entrance Water 2", "Gerudo Valley Rock Entrance Water 2"),
                        M("Rock.png", 741, 319, 24, "OOT Gerudo Valley Rock Entrance Water 3", "Gerudo Valley Rock Entrance Water 3"),
                        MA("Rock.png", 311, 193, 24, "OOT Gerudo Valley Rock Adult 1", "adult", "Gerudo Valley Rock Adult 1"),
                        MA("Rock.png", 349, 185, 24, "OOT Gerudo Valley Rock Adult 2", "adult", "Gerudo Valley Rock Adult 2"),
                        MA("Rock.png", 339, 220, 24, "OOT Gerudo Valley Rock Adult 3", "adult", "Gerudo Valley Rock Adult 3"),
                        MA("Rock.png", 377, 208, 24, "OOT Gerudo Valley Rock Adult 4", "adult", "Gerudo Valley Rock Adult 4"),
                        MA("NPC.png", 310, 262, 40, "OOT Gerudo Valley Broken Goron Sword", "adult", "Gerudo Valley Broken Goron Sword"),
                        MA("Chest.png", 234, 465, 40, "OOT Gerudo Valley Chest", "adult", "Gerudo Valley Chest"),
                        MA("Red_Boulder.png", 242, 438, 24, "OOT Gerudo Valley Red Boulder Around Chest 1", "adult", "Gerudo Valley Red Boulder Around Chest 1"),
                        MA("Red_Boulder.png", 269, 444, 24, "OOT Gerudo Valley Red Boulder Around Chest 2", "adult", "Gerudo Valley Red Boulder Around Chest 2"),
                        MA("Red_Boulder.png", 215, 445, 24, "OOT Gerudo Valley Red Boulder Around Chest 3", "adult", "Gerudo Valley Red Boulder Around Chest 3"),
                        MA("Red_Boulder.png", 280, 470, 24, "OOT Gerudo Valley Red Boulder Around Chest 4", "adult", "Gerudo Valley Red Boulder Around Chest 4"),
                        MA("Red_Boulder.png", 605, 199, 24, "OOT Gerudo Valley Red Boulder Before Bridge 1", "adult", "Gerudo Valley Red Boulder Before Bridge 1"),
                        MA("Red_Boulder.png", 586, 393, 24, "OOT Gerudo Valley Red Boulder Before Bridge 2", "adult", "Gerudo Valley Red Boulder Before Bridge 2"),
                        MA("Red_Boulder.png", 140, 278, 24, "OOT Gerudo Valley Red Boulder After Bridge 1", "adult", "Gerudo Valley Red Boulder After Bridge 1"),
                        MA("Red_Boulder.png", 306, 426, 24, "OOT Gerudo Valley Red Boulder After Bridge 2", "adult", "Gerudo Valley Red Boulder After Bridge 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Carpenter Tent",
                    BackgroundImage = OoT("Gerudo_Valley", "Tent"),
                    DestinationEntranceIds = new List<string> { "OOT_VALLEY_TENT" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 555, 294, "Entrance shuffle (Gerudo Valley)", "OOT_GERUDO_VALLEY_FROM_TENT")
                    }
                },
                new MapSubRegion
                {
                    Name = "Song of Storms Grotto",
                    BackgroundImage = OoT("Gerudo_Valley", "Storms"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_SCRUBS2_VALLEY" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 655, 630, "Entrance shuffle (Gerudo Valley)", "OOT_GROTTO_EXIT_SCRUBS2_VALLEY"),
                        M("Hive.png", 529, 134, 40, "OOT Gerudo Valley Grotto Hive", "Gerudo Valley Grotto Hive"),
                        M("Scrub.png", 698, 282, 40, "OOT Gerudo Valley Grotto Front Scrub", "Gerudo Valley Grotto Front Scrub"),
                        M("Scrub.png", 632, 188, 40, "OOT Gerudo Valley Grotto Back Scrub", "Gerudo Valley Grotto Back Scrub")
                    }
                },
                new MapSubRegion
                {
                    Name = "Octorok Grotto",
                    BackgroundImage = OoT("Gerudo_Valley", "Octorok"),
					DestinationEntranceIds = new List<string> { "OOT_GROTTO_OCTOROK" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 654, 587, "Entrance shuffle (Gerudo Valley)", "OOT_GROTTO_EXIT_OCTOROK"),
                        M("Rupee.png", 628, 291, 24, "OOT Gerudo Valley Octorok Grotto Rupee 1", "Gerudo Valley Octorok Grotto Rupee 1"),
                        M("Rupee.png", 691, 247, 24, "OOT Gerudo Valley Octorok Grotto Rupee 2", "Gerudo Valley Octorok Grotto Rupee 2"),
                        M("Rupee.png", 714, 315, 24, "OOT Gerudo Valley Octorok Grotto Rupee 3", "Gerudo Valley Octorok Grotto Rupee 3"),
                        M("Rupee.png", 612, 341, 24, "OOT Gerudo Valley Octorok Grotto Rupee 4", "Gerudo Valley Octorok Grotto Rupee 4"),
                        M("Rupee.png", 738, 348, 24, "OOT Gerudo Valley Octorok Grotto Rupee 5", "Gerudo Valley Octorok Grotto Rupee 5"),
                        M("Rupee.png", 634, 226, 24, "OOT Gerudo Valley Octorok Grotto Rupee 6", "Gerudo Valley Octorok Grotto Rupee 6"),
                        M("Rupee.png", 736, 234, 24, "OOT Gerudo Valley Octorok Grotto Rupee 7", "Gerudo Valley Octorok Grotto Rupee 7"),
                        M("Rupee.png", 675, 278, 24, "OOT Gerudo Valley Octorok Grotto Rupee 8", "Gerudo Valley Octorok Grotto Rupee 8")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion GoronCity()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Goron City";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Goron City",
                    BackgroundImage = OoT("Goron_City", "Goron_City"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_GORON_CITY_FROM_SHOP",
						"OOT_GROTTO_EXIT_SCRUBS3_GORON_CITY",
						"OOT_GORON_CITY_FROM_LOST_WOODS",
						"OOT_GORON_CITY",
						"OOT_GORON_CITY_FROM_CRATER"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 192, 421, "Entrance shuffle (Lost Woods)", "OOT_LOST_WOODS_FROM_GORON_CITY"),
                        ME("Entrance.png", 80, 333, "Entrance shuffle (Death Mountain Trail)", "OOT_DEATH_MOUNTAIN_FROM_GORON_CITY"),
                        ME("Entrance.png", 781, 338, "Entrance shuffle (Death Mountain Crater)", "OOT_CRATER_FROM_GORON_CITY"),
                        ME("Entrance.png", 455, 287, "Entrance shuffle (Shop)", "OOT_SHOP_GORON"),
                        ME("Entrance.png", 721, 553, "Entrance shuffle (Deku Scrubs Grotto)", "OOT_GROTTO_SCRUBS3_GORON_CITY"),
                        
                        M("Chest.png", 720, 39, 40, "OOT Goron City Maze Center 1", "Goron City Maze Center 1"),
                        M("Chest.png", 720, 75, 40, "OOT Goron City Maze Center 2", "Goron City Maze Center 2"),
                        M("Chest.png", 720, 3, 40, "OOT Goron City Maze Left", "Goron City Maze Left"),
                        MA("Gold_Skulltula.png", 757, 80, 40, "OOT Goron City GS Maze", "child", "Goron City GS Maze"),
                        MA("Gold_Skulltula.png", 417, 322, 40, "OOT Goron City GS Platform", "adult", "Goron City GS Platform"),
                        M("Rock.png", 760, 59, 24, "OOT Goron City Rock", "Goron City Rock"),
                        M("Red_Boulder.png", 579, 13, 24, "OOT Goron City Red Boulder 1", "Goron City Red Boulder 1"),
                        M("Red_Boulder.png", 607, 13, 24, "OOT Goron City Red Boulder 2", "Goron City Red Boulder 2"),
                        M("Red_Boulder.png", 635, 13, 24, "OOT Goron City Red Boulder 3", "Goron City Red Boulder 3"),
                        M("Red_Boulder.png", 663, 13, 24, "OOT Goron City Red Boulder 4", "Goron City Red Boulder 4"),
                        M("Red_Boulder.png", 690, 13, 24, "OOT Goron City Red Boulder 5", "Goron City Red Boulder 5"),
                        MA("Collectible.png", 460, 328, 40, "OOT Goron City Big Pot HP", "child", "Goron City Big Pot HP"),
                        MA("NPC.png", 630, 326, 40, "OOT Goron City Bomb Bag", "child", "Goron City Bomb Bag"),
                        MA("NPC.png", 706, 337, 40, "OOT Darunia", "child", "Darunia"),
                        MA("NPC.png", 361, 224, 40, "OOT Goron City Tunic", "adult", "Goron City Tunic"),
                        MA("NPC.png", 179, 165, 40, "OOT Goron City Medigoron Giant Knife", "adult", "Goron City Medigoron Giant Knife"),
                        M("Pot.png", 303, 286, 24, "OOT Goron City Pot Stairs 1", "Goron City Pot Stairs 1"),
                        M("Pot.png", 444, 88, 24, "OOT Goron City Pot Stairs 2", "Goron City Pot Stairs 2"),
                        M("Pot.png", 444, 114, 24, "OOT Goron City Pot Stairs 3", "Goron City Pot Stairs 3"),
                        M("Pot.png", 420, 88, 24, "OOT Goron City Pot Stairs 4", "Goron City Pot Stairs 4"),
                        M("Pot.png", 321, 267, 24, "OOT Goron City Pot Stairs 5", "Goron City Pot Stairs 5"),
                        M("Pot.png", 685, 381, 24, "OOT Goron City Pot Darunia Room 1", "Goron City Pot Darunia Room 1"),
                        M("Pot.png", 709, 381, 24, "OOT Goron City Pot Darunia Room 2", "Goron City Pot Darunia Room 2"),
                        M("Pot.png", 733, 381, 24, "OOT Goron City Pot Darunia Room 3", "Goron City Pot Darunia Room 3"),
                        M("Pot.png", 220, 191, 24, "OOT Goron City Pot Medigoron Room", "Goron City Pot Medigoron Room")
                    }
                },
                new MapSubRegion
                {
                    Name = "Shop",
                    BackgroundImage = OoT("Goron_City", "Shop"),
                    DestinationEntranceIds = new List<string> { "OOT_SHOP_GORON" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 447, 557, "Entrance shuffle (Goron City)", "OOT_GORON_CITY_FROM_SHOP"),
                        M("Shop.png", 229, 207, 40, "OOT Goron Shop Item 1", "Goron Shop Item 1"),
                        M("Shop.png", 324, 211, 40, "OOT Goron Shop Item 2", "Goron Shop Item 2"),
                        M("Shop.png", 229, 268, 40, "OOT Goron Shop Item 3", "Goron Shop Item 3"),
                        M("Shop.png", 324, 269, 40, "OOT Goron Shop Item 4", "Goron Shop Item 4"),
                        M("Shop.png", 563, 269, 40, "OOT Goron Shop Item 5", "Goron Shop Item 5"),
                        M("Shop.png", 563, 211, 40, "OOT Goron Shop Item 6", "Goron Shop Item 6"),
                        M("Shop.png", 662, 268, 40, "OOT Goron Shop Item 7", "Goron Shop Item 7"),
                        M("Shop.png", 662, 207, 40, "OOT Goron Shop Item 8", "Goron Shop Item 8")
                    }
                },
                new MapSubRegion
                {
                    Name = "Deku Scrubs Grotto",
                    BackgroundImage = OoT("Goron_City", "Scrubs"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_SCRUBS3_GORON_CITY" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 676, 691, "Entrance shuffle (Goron City)", "OOT_GROTTO_EXIT_SCRUBS3_GORON_CITY"),
                        M("Hive.png", 962, 194, 40, "OOT Goron City Grotto Hive", "Goron City Grotto Hive"),
                        M("Scrub.png", 450, 335, 40, "OOT Goron City Grotto Left Scrub", "Goron City Grotto Left Scrub"),
                        M("Scrub.png", 672, 94, 40, "OOT Goron City Grotto Center Scrub", "Goron City Grotto Center Scrub"),
                        M("Scrub.png", 922, 335, 40, "OOT Goron City Grotto Right Scrub", "Goron City Grotto Right Scrub")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion Graveyard()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Graveyard";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Graveyard",
                    BackgroundImage = OoT("Graveyard", "Graveyard"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_GRAVEYARD",
						"OOT_GRAVEYARD_FROM_DAMPE",
						"OOT_GRAVE_EXIT_REDEAD",
						"OOT_WARP_SONG_GRAVE",
						"OOT_GRAVE_EXIT_ROYAL",
						"OOT_GRAVEYARD_FROM_TEMPLE_SHADOW",
						"OOT_GRAVE_EXIT_SHIELD",
						"OOT_GRAVE_EXIT_DAMPE"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 881, 248, "Entrance shuffle (Kakariko)", "OOT_KAKARIKO_FROM_GRAVEYARD"),
                        ME("Entrance.png", 11, 303, "Entrance shuffle (Shadow Temple)", "OOT_TEMPLE_SHADOW"),
                        ME("Entrance.png", 623, 173, "Entrance shuffle (Dampe Hut)", "OOT_HOUSE_DAMPE"),
                        ME("Entrance.png", 581, 419, "Entrance shuffle (Dampe Tomb)", "OOT_GRAVE_DAMPE"),
                        ME("Entrance.png", 220, 308, "Entrance shuffle (Royal Family Tomb)", "OOT_GRAVE_ROYAL"),
                        ME("Entrance.png", 388, 242, "Entrance shuffle (ReDead Tomb)", "OOT_GRAVE_REDEAD"),
                        ME("Entrance.png", 515, 284, "Entrance shuffle (Fairy Fountain Tomb)", "OOT_GRAVE_SHIELD"),
                        
                        M("Grass.png", 706, 176, 24, "OOT Graveyard Grass 01", "Graveyard Grass 01"),
                        M("Grass.png", 718, 168, 24, "OOT Graveyard Grass 02", "Graveyard Grass 02"),
                        M("Grass.png", 729, 178, 24, "OOT Graveyard Grass 03", "Graveyard Grass 03"),
                        M("Grass.png", 717, 186, 24, "OOT Graveyard Grass 04", "Graveyard Grass 04"),
                        M("Grass.png", 705, 196, 24, "OOT Graveyard Grass 05", "Graveyard Grass 05"),
                        M("Grass.png", 693, 185, 24, "OOT Graveyard Grass 06", "Graveyard Grass 06"),
                        M("Grass.png", 694, 166, 24, "OOT Graveyard Grass 07", "Graveyard Grass 07"),
                        M("Grass.png", 706, 155, 24, "OOT Graveyard Grass 08", "Graveyard Grass 08"),
                        M("Grass.png", 728, 154, 24, "OOT Graveyard Grass 09", "Graveyard Grass 09"),
                        M("Grass.png", 740, 169, 24, "OOT Graveyard Grass 10", "Graveyard Grass 10"),
                        M("Grass.png", 741, 188, 24, "OOT Graveyard Grass 11", "Graveyard Grass 11"),
                        M("Grass.png", 728, 196, 24, "OOT Graveyard Grass 12", "Graveyard Grass 12"),
                        M("Rock.png", 756, 199, 24, "OOT Graveyard Rock", "Graveyard Rock"),
                        M("Collectible.png", 688, 435, 40, "OOT Graveyard Crate HP", "Graveyard Crate HP"),
                        MA("Collectible.png", 547, 283, 40, "OOT Graveyard Dampe Game", "child", "Graveyard Dampe Game"),
                        M("Crate.png", 731, 437, 24, "OOT Graveyard Crate", "Graveyard Crate"),
                        MA("Butterfly.png", 519, 312, 24, "OOT Graveyard Butterfly 1", "child", "Graveyard Butterfly 1"),
                        MA("Butterfly.png", 496, 292, 24, "OOT Graveyard Butterfly 2", "child", "Graveyard Butterfly 2"),
                        MA("Butterfly.png", 519, 270, 24, "OOT Graveyard Butterfly 3", "child", "Graveyard Butterfly 3"),
                        MA("NPC.png", 550, 208, 40, "OOT Graveyard Sell Spooky Mask", "child", "Graveyard Sell Spooky Mask"),
                        MA("Gold_Skulltula.png", 381, 114, 40, "OOT Graveyard GS Wall", "child", "Graveyard GS Wall"),
                        MA("Gold_Skulltula.png", 619, 421, 40, "OOT Graveyard GS Soil", "child", "Graveyard GS Soil"),
                        MA("Soil.png", 660, 428, 24, "OOT Graveyard Soil 1", "child", "Graveyard Soil 1"),
                        MA("Soil.png", 644, 399, 24, "OOT Graveyard Soil 2", "child", "Graveyard Soil 2"),
                        MA("Soil.png", 644, 457, 24, "OOT Graveyard Soil 3", "child", "Graveyard Soil 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Dampe Hut",
                    BackgroundImage = OoT("Graveyard", "Dampe_Hut"),
                    DestinationEntranceIds = new List<string> { "OOT_HOUSE_DAMPE" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 496, 280, "Entrance shuffle (Graveyard)", "OOT_GRAVEYARD_FROM_DAMPE")
                    }
                },
                new MapSubRegion
                {
                    Name = "Dampe Tomb",
                    BackgroundImage = OoT("Graveyard", "Dampe_Tomb"),
                    DestinationEntranceIds = new List<string> { "OOT_GRAVE_DAMPE" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 238, 68, "Entrance shuffle (Graveyard)", "OOT_GRAVE_EXIT_DAMPE"),
                        M("Pot.png", 281, 134, 24, "OOT Graveyard Dampe Tomb Pot 1", "Graveyard Dampe Tomb Pot 1"),
                        M("Pot.png", 257, 134, 24, "OOT Graveyard Dampe Tomb Pot 2", "Graveyard Dampe Tomb Pot 2"),
                        M("Pot.png", 281, 28, 24, "OOT Graveyard Dampe Tomb Pot 3", "Graveyard Dampe Tomb Pot 3"),
                        M("Pot.png", 233, 28, 24, "OOT Graveyard Dampe Tomb Pot 4", "Graveyard Dampe Tomb Pot 4"),
                        M("Pot.png", 233, 134, 24, "OOT Graveyard Dampe Tomb Pot 5", "Graveyard Dampe Tomb Pot 5"),
                        M("Pot.png", 257, 28, 24, "OOT Graveyard Dampe Tomb Pot 6", "Graveyard Dampe Tomb Pot 6"),
                        M("Chest.png", 178, 705, 40, "OOT Graveyard Dampe Tomb Reward 1", "Graveyard Dampe Tomb Reward 1"),
                        M("Collectible.png", 178, 744, 40, "OOT Graveyard Dampe Tomb Reward 2", "Graveyard Dampe Tomb Reward 2"),
                        M("Rupee.png", 498, 174, 24, "OOT Graveyard Dampe Tomb Rupee 1", "Graveyard Dampe Tomb Rupee 1"),
                        M("Rupee.png", 588, 201, 24, "OOT Graveyard Dampe Tomb Rupee 2", "Graveyard Dampe Tomb Rupee 2"),
                        M("Rupee.png", 932, 243, 24, "OOT Graveyard Dampe Tomb Rupee 3", "Graveyard Dampe Tomb Rupee 3"),
                        M("Rupee.png", 1100, 208, 24, "OOT Graveyard Dampe Tomb Rupee 4", "Graveyard Dampe Tomb Rupee 4"),
                        M("Rupee.png", 1116, 399, 24, "OOT Graveyard Dampe Tomb Rupee 5", "Graveyard Dampe Tomb Rupee 5"),
                        M("Rupee.png", 897, 590, 24, "OOT Graveyard Dampe Tomb Rupee 6", "Graveyard Dampe Tomb Rupee 6"),
                        M("Rupee.png", 707, 629, 24, "OOT Graveyard Dampe Tomb Rupee 7", "Graveyard Dampe Tomb Rupee 7"),
                        M("Rupee.png", 572, 399, 24, "OOT Graveyard Dampe Tomb Rupee 8", "Graveyard Dampe Tomb Rupee 8"),
                        M("Wonder.png", 477, 174, 24, "OOT Graveyard Dampe Tomb Wonder Item 01", "Graveyard Dampe Tomb Wonder Item 01"),
                        M("Wonder.png", 457, 174, 24, "OOT Graveyard Dampe Tomb Wonder Item 02", "Graveyard Dampe Tomb Wonder Item 02"),
                        M("Wonder.png", 457, 194, 24, "OOT Graveyard Dampe Tomb Wonder Item 03", "Graveyard Dampe Tomb Wonder Item 03"),
                        M("Wonder.png", 457, 214, 24, "OOT Graveyard Dampe Tomb Wonder Item 04", "Graveyard Dampe Tomb Wonder Item 04"),
                        M("Wonder.png", 588, 224, 24, "OOT Graveyard Dampe Tomb Wonder Item 05", "Graveyard Dampe Tomb Wonder Item 05"),
                        M("Wonder.png", 1138, 208, 24, "OOT Graveyard Dampe Tomb Wonder Item 06", "Graveyard Dampe Tomb Wonder Item 06"),
                        M("Wonder.png", 1138, 169, 24, "OOT Graveyard Dampe Tomb Wonder Item 07", "Graveyard Dampe Tomb Wonder Item 07"),
                        M("Wonder.png", 1116, 435, 24, "OOT Graveyard Dampe Tomb Wonder Item 08", "Graveyard Dampe Tomb Wonder Item 08"),
                        M("Wonder.png", 897, 628, 24, "OOT Graveyard Dampe Tomb Wonder Item 09", "Graveyard Dampe Tomb Wonder Item 09"),
                        M("Wonder.png", 937, 628, 24, "OOT Graveyard Dampe Tomb Wonder Item 10", "Graveyard Dampe Tomb Wonder Item 10"),
                        M("Wonder.png", 690, 670, 24, "OOT Graveyard Dampe Tomb Wonder Item 11", "Graveyard Dampe Tomb Wonder Item 11"),
                        M("Wonder.png", 630, 609, 24, "OOT Graveyard Dampe Tomb Wonder Item 12", "Graveyard Dampe Tomb Wonder Item 12"),
                        M("Wonder.png", 630, 650, 24, "OOT Graveyard Dampe Tomb Wonder Item 13", "Graveyard Dampe Tomb Wonder Item 13"),
                        M("Wonder.png", 649, 670, 24, "OOT Graveyard Dampe Tomb Wonder Item 14", "Graveyard Dampe Tomb Wonder Item 14"),
                        M("Wonder.png", 572, 369, 24, "OOT Graveyard Dampe Tomb Wonder Item 15", "Graveyard Dampe Tomb Wonder Item 15")
                    }
                },
                new MapSubRegion
                {
                    Name = "Royal Family Tomb",
                    BackgroundImage = OoT("Graveyard", "Royal_Tomb"),
                    DestinationEntranceIds = new List<string> { "OOT_GRAVE_ROYAL" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 130, 573, "Entrance shuffle (Graveyard)", "OOT_GRAVE_EXIT_ROYAL"),
                        M("Chest.png", 132, 494, 40, "OOT Graveyard Royal Tomb Chest", "Graveyard Royal Tomb Chest"),
                        M("NPC.png", 135, 18, 40, "OOT Graveyard Royal Tomb Song", "Graveyard Royal Tomb Song"),
                        M("Fairy_Spot.png", 92, 480, 40, "OOT Graveyard Royal Tomb Big Fairy", "Graveyard Royal Tomb Big Fairy")
                    }
                },
                new MapSubRegion
                {
                    Name = "ReDead Tomb",
                    BackgroundImage = OoT("Graveyard", "Redead_Tomb"),
                    DestinationEntranceIds = new List<string> { "OOT_GRAVE_REDEAD" },
                    Marks = new List<MapMark> 
                    { 
                        ME("Entrance.png", 452, 69, "Entrance shuffle (Graveyard)", "OOT_GRAVE_EXIT_REDEAD"),
                        M("Chest.png", 453, 472, 40, "OOT Graveyard ReDead Tomb", "Graveyard ReDead Tomb") 
                    }
                },
                new MapSubRegion
                {
                    Name = "Fairy Fountain Tomb",
                    BackgroundImage = OoT("Graveyard", "Fairy_Fountain_Tomb"),
                    DestinationEntranceIds = new List<string> { "OOT_GRAVE_SHIELD" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 631, 8, "Entrance shuffle (Graveyard)", "OOT_GRAVE_EXIT_SHIELD"),
                        M("Chest.png", 632, 228, 40, "OOT Graveyard Fairy Tomb", "Graveyard Fairy Tomb"),
                        M("Fairy.png", 289, 432, 24, "OOT Graveyard Fairy Fountain Fairy 1", "Graveyard Fairy Fountain Fairy 1"),
                        M("Fairy.png", 310, 427, 24, "OOT Graveyard Fairy Fountain Fairy 2", "Graveyard Fairy Fountain Fairy 2"),
                        M("Fairy.png", 268, 422, 24, "OOT Graveyard Fairy Fountain Fairy 3", "Graveyard Fairy Fountain Fairy 3"),
                        M("Fairy.png", 291, 407, 24, "OOT Graveyard Fairy Fountain Fairy 4", "Graveyard Fairy Fountain Fairy 4"),
                        M("Fairy.png", 319, 407, 24, "OOT Graveyard Fairy Fountain Fairy 5", "Graveyard Fairy Fountain Fairy 5"),
                        M("Fairy.png", 266, 400, 24, "OOT Graveyard Fairy Fountain Fairy 6", "Graveyard Fairy Fountain Fairy 6"),
                        M("Fairy.png", 302, 385, 24, "OOT Graveyard Fairy Fountain Fairy 7", "Graveyard Fairy Fountain Fairy 7"),
                        M("Fairy.png", 280, 382, 24, "OOT Graveyard Fairy Fountain Fairy 8", "Graveyard Fairy Fountain Fairy 8")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion HauntedWasteland()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Haunted Wasteland";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Haunted Wasteland",
                    BackgroundImage = OoT("Haunted_Wasteland", "Haunted_Wasteland"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_WASTELAND",
						"OOT_WASTELAND_FROM_COLOSSUS"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 892, 141, "Entrance shuffle (Gerudo Fortress)", "OOT_FORTRESS_FROM_WASTELAND"),
                        ME("Entrance.png", 51, 534, "Entrance shuffle (Desert Colossus)", "OOT_COLOSSUS"),
                        
                        M("NPC.png", 787, 459, 40, "OOT Haunted Wasteland Carpet Merchant", "Haunted Wasteland Carpet Merchant"),
                        M("Gold_Skulltula.png", 141, 182, 40, "OOT Haunted Wasteland GS", "Haunted Wasteland GS"),
                        M("Chest.png", 179, 203, 40, "OOT Haunted Wasteland Chest", "Haunted Wasteland Chest"),
                        M("Pot.png", 168, 165, 24, "OOT Haunted Wasteland Pot 1", "Haunted Wasteland Pot 1"),
                        M("Pot.png", 189, 181, 24, "OOT Haunted Wasteland Pot 2", "Haunted Wasteland Pot 2"),
                        M("Pot.png", 139, 222, 24, "OOT Haunted Wasteland Pot 3", "Haunted Wasteland Pot 3"),
                        M("Pot.png", 160, 236, 24, "OOT Haunted Wasteland Pot 4", "Haunted Wasteland Pot 4"),
                        M("Crate.png", 860, 189, 24, "OOT Haunted Wasteland Crate Entrance", "Haunted Wasteland Crate Entrance"),
                        M("Crate.png", 772, 248, 24, "OOT Haunted Wasteland Crate After Pit 1", "Haunted Wasteland Crate After Pit 1"),
                        M("Crate.png", 784, 272, 24, "OOT Haunted Wasteland Crate After Pit 2", "Haunted Wasteland Crate After Pit 2"),
                        M("Crate.png", 760, 224, 24, "OOT Haunted Wasteland Crate After Pit 3", "Haunted Wasteland Crate After Pit 3"),
                        M("Crate.png", 152, 507, 24, "OOT Haunted Wasteland Crate Deep", "Haunted Wasteland Crate Deep")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion HyruleCastle()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Hyrule / Ganon Castle";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Hyrule Castle",
                    BackgroundImage = OoT("Hyrule", "Hyrule_Castle"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_HYRULE_CASTLE",
						"OOT_HYRULE_CASTLE_FROM_FAIRY",
						"OOT_GROTTO_EXIT_CASTLE"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 2, 197, "Entrance shuffle (Market)", "OOT_MARKET_FROM_CASTLE"),
                        ME("Entrance.png", 193, 581, "Entrance shuffle (Fairy Fountain Din)", "OOT_FAIRY_DIN"),
                        ME("Entrance.png", 584, 367, "Entrance shuffle (Song of Storms Grotto)", "OOT_GROTTO_CASTLE"),
                        
                        M("NPC.png", 104, 215, 40, "OOT Malon Egg", "Malon Egg"),
						M("NPC.png", 104, 175, 40, "OOT Hatch Chicken", "Hatch Chicken"),
                        M("Gold_Skulltula.png", 193, 199, 40, "OOT Hyrule Castle GS Tree", "Hyrule Castle GS Tree"),
                        M("Rock.png", 172, 193, 24, "OOT Hyrule Castle Rock 1", "Hyrule Castle Rock 1"),
                        M("Rock.png", 188, 231, 24, "OOT Hyrule Castle Rock 2", "Hyrule Castle Rock 2"),
                        M("Rock.png", 230, 213, 24, "OOT Hyrule Castle Rock 3", "Hyrule Castle Rock 3"),
                        M("Pot.png", 165, 425, 24, "OOT Hyrule Castle Pot 1", "Hyrule Castle Pot 1"),
                        M("Pot.png", 143, 425, 24, "OOT Hyrule Castle Pot 2", "Hyrule Castle Pot 2"),
                        M("Wonder.png", 605, 62, 24, "OOT Hyrule Castle Wonder Item Moat 01", "Hyrule Castle Wonder Item Moat 01"),
                        M("Wonder.png", 613, 97, 24, "OOT Hyrule Castle Wonder Item Moat 02", "Hyrule Castle Wonder Item Moat 02"),
                        M("Wonder.png", 621, 132, 24, "OOT Hyrule Castle Wonder Item Moat 03", "Hyrule Castle Wonder Item Moat 03"),
                        M("Wonder.png", 627, 173, 24, "OOT Hyrule Castle Wonder Item Moat 04", "Hyrule Castle Wonder Item Moat 04"),
                        M("Wonder.png", 635, 211, 24, "OOT Hyrule Castle Wonder Item Moat 05", "Hyrule Castle Wonder Item Moat 05"),
                        M("Wonder.png", 639, 250, 24, "OOT Hyrule Castle Wonder Item Moat 06", "Hyrule Castle Wonder Item Moat 06"),
                        M("Wonder.png", 732, 293, 24, "OOT Hyrule Castle Wonder Item Moat 07", "Hyrule Castle Wonder Item Moat 07"),
                        M("Wonder.png", 677, 299, 24, "OOT Hyrule Castle Wonder Item Moat 08", "Hyrule Castle Wonder Item Moat 08"),
                        M("Wonder.png", 779, 292, 24, "OOT Hyrule Castle Wonder Item Moat 09", "Hyrule Castle Wonder Item Moat 09"),
                        M("Wonder.png", 878, 290, 24, "OOT Hyrule Castle Wonder Item Moat 10", "Hyrule Castle Wonder Item Moat 10"),
                        M("Wonder.png", 663, 135, 24, "OOT Hyrule Castle Wonder Item Torch 1", "Hyrule Castle Wonder Item Torch 1"),
                        M("Wonder.png", 651, 79, 24, "OOT Hyrule Castle Wonder Item Torch 2", "Hyrule Castle Wonder Item Torch 2"),
                        M("Tree.png", 480, 349, 24, "OOT Hyrule Castle Tree Guarded", "Hyrule Castle Tree Guarded"),
                        M("Tree.png", 465, 147, 24, "OOT Hyrule Castle Tree 1", "Hyrule Castle Tree 1"),
                        M("Tree.png", 463, 191, 24, "OOT Hyrule Castle Tree 2", "Hyrule Castle Tree 2"),
                        M("Tree.png", 478, 257, 24, "OOT Hyrule Castle Tree 3", "Hyrule Castle Tree 3"),
                        M("Tree.png", 480, 303, 24, "OOT Hyrule Castle Tree 4", "Hyrule Castle Tree 4"),
                        M("Tree.png", 359, 338, 24, "OOT Hyrule Castle Tree 5", "Hyrule Castle Tree 5"),
                        M("Tree.png", 318, 398, 24, "OOT Hyrule Castle Tree 6", "Hyrule Castle Tree 6"),
                        M("Tree.png", 596, 339, 24, "OOT Hyrule Castle Tree 7", "Hyrule Castle Tree 7"),
                        M("Tree.png", 348, 453, 24, "OOT Hyrule Castle Tree 8", "Hyrule Castle Tree 8"),
                        M("Grass.png", 595, 315, 24, "OOT Hyrule Castle Grass 1", "Hyrule Castle Grass 1"),
                        M("Grass.png", 618, 341, 24, "OOT Hyrule Castle Grass 2", "Hyrule Castle Grass 2"),
                        M("Butterfly.png", 277, 307, 24, "OOT Hyrule Castle Butterfly Pack 1 Butterfly 1", "Hyrule Castle Butterfly Pack 1 Butterfly 1"),
                        M("Butterfly.png", 286, 285, 24, "OOT Hyrule Castle Butterfly Pack 1 Butterfly 2", "Hyrule Castle Butterfly Pack 1 Butterfly 2"),
                        M("Butterfly.png", 367, 506, 24, "OOT Hyrule Castle Butterfly Pack 2 Butterfly 1", "Hyrule Castle Butterfly Pack 2 Butterfly 1"),
                        M("Butterfly.png", 341, 489, 24, "OOT Hyrule Castle Butterfly Pack 2 Butterfly 2", "Hyrule Castle Butterfly Pack 2 Butterfly 2"),
                        M("Butterfly.png", 408, 219, 24, "OOT Hyrule Castle Butterfly Pack 3 Butterfly 1", "Hyrule Castle Butterfly Pack 3 Butterfly 1"),
                        M("Butterfly.png", 420, 196, 24, "OOT Hyrule Castle Butterfly Pack 3 Butterfly 2", "Hyrule Castle Butterfly Pack 3 Butterfly 2"),
                        M("Butterfly.png", 444, 180, 24, "OOT Hyrule Castle Butterfly Pack 3 Butterfly 3", "Hyrule Castle Butterfly Pack 3 Butterfly 3"),
                        M("Butterfly.png", 333, 162, 24, "OOT Hyrule Castle Butterfly Pack 4 Butterfly 1", "Hyrule Castle Butterfly Pack 4 Butterfly 1"),
                        M("Butterfly.png", 310, 148, 24, "OOT Hyrule Castle Butterfly Pack 4 Butterfly 2", "Hyrule Castle Butterfly Pack 4 Butterfly 2"),
                        M("Butterfly.png", 342, 141, 24, "OOT Hyrule Castle Butterfly Pack 4 Butterfly 3", "Hyrule Castle Butterfly Pack 4 Butterfly 3"),
                        M("Butterfly.png", 316, 126, 24, "OOT Hyrule Castle Butterfly Pack 4 Butterfly 4", "Hyrule Castle Butterfly Pack 4 Butterfly 4"),
                        M("Butterfly.png", 341, 118, 24, "OOT Hyrule Castle Butterfly Pack 4 Butterfly 5", "Hyrule Castle Butterfly Pack 4 Butterfly 5")
                    }
                },
                new MapSubRegion
                {
                    Name = "Castle Courtyard",
                    BackgroundImage = OoT("Hyrule", "Castle_Courtyard"),
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Wonder.png", 257, 195, 24, "OOT Castle Courtyard Wonder Item", "Castle Courtyard Wonder Item"),
                        M("NPC.png", 735, 291, 40, "OOT Zelda's Letter", "Zelda's Letter"),
                        M("NPC.png", 240, 526, 40, "OOT Zelda's Song", "Zelda's Song")
                    }
                },
                new MapSubRegion
                {
                    Name = "Outside Ganon Castle",
                    BackgroundImage = OoT("Ganon", "Exterior"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_OUTSIDE_GANON_FROM_FAIRY",
						"OOT_GANON_CASTLE_EXTERIOR_FROM_CASTLE"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 19, 240, "Entrance shuffle (Fairy Fountain Defense)", "OOT_FAIRY_DEFENSE"),
						ME("Entrance.png", 626, 546, "Entrance shuffle (Ganon Castle)", "OOT_GANON_CASTLE"),
						ME("Entrance.png", 867, 155, "Entrance shuffle (Market)", "OOT_MARKET_FROM_CASTLE"),
                        
                        M("Gold_Skulltula.png", 345, 391, 40, "OOT Ganon Castle Exterior GS", "Ganon Castle Exterior GS"),
                        M("Red_Boulder.png", 118, 306, 24, "OOT Ganon Castle Exterior Red Boulder 1", "Ganon Castle Exterior Red Boulder 1"),
                        M("Red_Boulder.png", 242, 354, 24, "OOT Ganon Castle Exterior Red Boulder 2", "Ganon Castle Exterior Red Boulder 2"),
                        M("Red_Boulder.png", 219, 348, 24, "OOT Ganon Castle Exterior Red Boulder 3", "Ganon Castle Exterior Red Boulder 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Song of Storms Grotto",
                    BackgroundImage = OoT("Hyrule", "Castle_Storms"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_CASTLE" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 669, 635, "Entrance shuffle (Hyrule Castle)", "OOT_GROTTO_EXIT_CASTLE"),
                        
                        M("Gold_Skulltula.png", 514, 691, 40, "OOT Hyrule Castle GS Grotto", "Hyrule Castle GS Grotto"),
                        M("Pot.png", 704, 411, 24, "OOT Hyrule Castle Grotto Pot 1", "Hyrule Castle Grotto Pot 1"),
                        M("Pot.png", 718, 340, 24, "OOT Hyrule Castle Grotto Pot 2", "Hyrule Castle Grotto Pot 2"),
                        M("Pot.png", 655, 379, 24, "OOT Hyrule Castle Grotto Pot 3", "Hyrule Castle Grotto Pot 3"),
                        M("Pot.png", 652, 304, 24, "OOT Hyrule Castle Grotto Pot 4", "Hyrule Castle Grotto Pot 4"),
                        M("Rock.png", 685, 246, 24, "OOT Hyrule Castle Grotto Rock Circle Rock 1", "Hyrule Castle Grotto Rock Circle Rock 1"),
                        M("Rock.png", 711, 238, 24, "OOT Hyrule Castle Grotto Rock Circle Rock 2", "Hyrule Castle Grotto Rock Circle Rock 2"),
                        M("Rock.png", 723, 213, 24, "OOT Hyrule Castle Grotto Rock Circle Rock 3", "Hyrule Castle Grotto Rock Circle Rock 3"),
                        M("Rock.png", 711, 185, 24, "OOT Hyrule Castle Grotto Rock Circle Rock 4", "Hyrule Castle Grotto Rock Circle Rock 4"),
                        M("Rock.png", 685, 174, 24, "OOT Hyrule Castle Grotto Rock Circle Rock 5", "Hyrule Castle Grotto Rock Circle Rock 5"),
                        M("Rock.png", 659, 183, 24, "OOT Hyrule Castle Grotto Rock Circle Rock 6", "Hyrule Castle Grotto Rock Circle Rock 6"),
                        M("Rock.png", 648, 209, 24, "OOT Hyrule Castle Grotto Rock Circle Rock 7", "Hyrule Castle Grotto Rock Circle Rock 7"),
                        M("Rock.png", 659, 237, 24, "OOT Hyrule Castle Grotto Rock Circle Rock 8", "Hyrule Castle Grotto Rock Circle Rock 8")
                    }
                },
                new MapSubRegion
                {
                    Name = "Great Fairy Fountain",
                    BackgroundImage = OoT("Fairy", "Great_Fairy"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_FAIRY_DIN",
						"OOT_FAIRY_DEFENSE"
					},
                    Marks = new List<MapMark>
                    {
                        MEA("Entrance.png", 480, 535, "Entrance shuffle (Hyrule Castle)", "child", "OOT_HYRULE_CASTLE_FROM_FAIRY"),
                        MEA("Entrance.png", 480, 535, "Entrance shuffle (Outside Ganon Castle)", "adult", "OOT_OUTSIDE_GANON_FROM_FAIRY"),
                        
                        MA("NPC.png", 482, 329, 40, "OOT Great Fairy Din's Fire", "child", "Great Fairy Din's Fire"),
                        MA("NPC.png", 482, 329, 40, "OOT Great Fairy Defense Upgrade", "adult", "Great Fairy Defense Upgrade")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion HyruleField()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Hyrule Field";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Hyrule Field",
                    BackgroundImage = OoT("Hyrule", "Hyrule_Field"),
					DestinationEntranceIds = new List<string> 
					{
						"OOT_FIELD_OWL",
						"OOT_FIELD_FROM_MARKET_ENTRANCE",
						"OOT_GROTTO_EXIT_SCRUB_HEART_PIECE",
						"OOT_GROTTO_EXIT_FAIRY_HF",
						"OOT_FIELD_FROM_LON_LON_RANCH",
						"OOT_FIELD_FROM_ZORA_RIVER",
						"OOT_FIELD_FROM_LAKE_HYLIA",
						"OOT_FIELD_FROM_LOST_WOODS_BRIDGE",
						"OOT_GROTTO_EXIT_GENERIC_HF_MARKET",
						"OOT_GROTTO_EXIT_FIELD_COW",
						"OOT_GROTTO_EXIT_GENERIC_HF_OPEN",
						"OOT_FIELD_FROM_GERUDO_VALLEY",
						"OOT_GROTTO_EXIT_FIELD_TREE",
						"OOT_GROTTO_EXIT_TEKTITE",
						"OOT_GROTTO_EXIT_GENERIC_HF_SOUTHEAST",
						"OOT_FIELD_FROM_KAKARIKO"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 1026, 536, "Entrance shuffle (Lost Woods Bridge)", "OOT_LOST_WOODS_BRIDGE_FROM_FIELD"),
                        ME("Entrance.png", 1068, 265, "Entrance shuffle (Zora River)", "OOT_ZORA_RIVER_FROM_FIELD"),
                        ME("Entrance.png", 972, 59, "Entrance shuffle (Kakariko)", "OOT_KAKARIKO_FROM_FIELD"),
                        ME("Entrance.png", 625, 409, "Entrance shuffle (Lon Lon Ranch)", "OOT_LON_LON_RANCH_FROM_FIELD"),
                        ME("Entrance.png", 436, 870, "Entrance shuffle (Lake Hylia)", "OOT_LAKE_HYLIA_FROM_FIELD"),
                        ME("Entrance.png", 216, 490, "Entrance shuffle (Gerudo Valley)", "OOT_GERUDO_VALLEY_FROM_FIELD"),
                        ME("Entrance.png", 743, 67, "Entrance shuffle (Market Entrance)", "OOT_MARKET_ENTRANCE_FROM_FIELD"),
						ME("Entrance.png", 791, 815, "Entrance shuffle (HF Southeast Grotto)", "OOT_GROTTO_GENERIC_HF_SOUTHEAST"),
                        ME("Entrance.png", 540, 855, "Entrance shuffle (HF Open Grotto)", "OOT_GROTTO_GENERIC_HF_OPEN"),
                        ME("Entrance.png", 642, 154, "Entrance shuffle (HF Market Grotto)", "OOT_GROTTO_GENERIC_HF_MARKET"),
                        ME("Entrance.png", 483, 36, "Entrance shuffle (HF Fairy Grotto)", "OOT_GROTTO_FAIRY_HF"),
                        ME("Entrance.png", 469, 828, "Entrance shuffle (HF Scrub Heart Piece Grotto)", "OOT_GROTTO_SCRUB_HEART_PIECE"),
                        ME("Entrance.png", 464, 274, "Entrance shuffle (HF Tektite Grotto)", "OOT_GROTTO_TEKTITE"),
                        ME("Entrance.png", 298, 392, "Entrance shuffle (HF Cow Grotto)", "OOT_GROTTO_FIELD_COW"),
                        ME("Entrance.png", 844, 48, "Entrance shuffle (HF Tree Grotto)", "OOT_GROTTO_FIELD_TREE"),
                        
                        MA("Wonder.png", 731, 106, 24, "OOT Hyrule Field Wonder Item 1", "child", "Hyrule Field Wonder Item 1"),
                        MA("Wonder.png", 751, 106, 24, "OOT Hyrule Field Wonder Item 2", "child", "Hyrule Field Wonder Item 2"),
                        MA("Wonder.png", 771, 106, 24, "OOT Hyrule Field Wonder Item 3", "child", "Hyrule Field Wonder Item 3"),
                        MA("NPC.png", 778, 132, 40, "OOT Hyrule Field Ocarina of Time", "child", "Hyrule Field Ocarina of Time"),
                        MA("NPC.png", 778, 169, 40, "OOT Hyrule Field Song of Time", "child", "Hyrule Field Song of Time"),
                        MA("NPC.png", 697, 396, 40, "OOT Hyrule Field Sell Bunny Mask", "child", "Hyrule Field Sell Bunny Mask"),
                        MA("Red_Boulder.png", 315, 457, 24, "OOT Hyrule Field Red Boulder 1", "adult", "Hyrule Field Red Boulder 1"),
                        MA("Red_Boulder.png", 345, 521, 24, "OOT Hyrule Field Red Boulder 2", "adult", "Hyrule Field Red Boulder 2"),
                        MA("Red_Boulder.png", 293, 508, 24, "OOT Hyrule Field Red Boulder 3", "adult", "Hyrule Field Red Boulder 3"),
                        MA("Red_Boulder.png", 396, 520, 24, "OOT Hyrule Field Red Boulder 4", "adult", "Hyrule Field Red Boulder 4"),
                        M("Rock.png", 315, 482, 24, "OOT Hyrule Field Rock Circle Rock 1", "Hyrule Field Rock Circle Rock 1"),
                        M("Rock.png", 332, 472, 24, "OOT Hyrule Field Rock Circle Rock 2", "Hyrule Field Rock Circle Rock 2"),
                        M("Rock.png", 339, 456, 24, "OOT Hyrule Field Rock Circle Rock 3", "Hyrule Field Rock Circle Rock 3"),
                        M("Rock.png", 332, 441, 24, "OOT Hyrule Field Rock Circle Rock 4", "Hyrule Field Rock Circle Rock 4"),
                        M("Rock.png", 315, 431, 24, "OOT Hyrule Field Rock Circle Rock 5", "Hyrule Field Rock Circle Rock 5"),
                        M("Rock.png", 297, 440, 24, "OOT Hyrule Field Rock Circle Rock 6", "Hyrule Field Rock Circle Rock 6"),
                        M("Rock.png", 290, 456, 24, "OOT Hyrule Field Rock Circle Rock 7", "Hyrule Field Rock Circle Rock 7"),
                        M("Rock.png", 296, 473, 24, "OOT Hyrule Field Rock Circle Rock 8", "Hyrule Field Rock Circle Rock 8"),
                        M("Fairy_Spot.png", 1021, 347, 40, "OOT Hyrule Field River Big Fairy", "Hyrule Field River Big Fairy"),
                        MA("Tree.png", 432, 475, 24, "OOT Hyrule Field Tree Adult", "adult", "Hyrule Field Tree Adult"),
                        MA("Tree.png", 831, 724, 24, "OOT Hyrule Field Tree Child Near Lake 1", "child", "Hyrule Field Tree Child Near Lake 1"),
                        MA("Tree.png", 861, 758, 24, "OOT Hyrule Field Tree Child Near Lake 2", "child", "Hyrule Field Tree Child Near Lake 2"),
                        MA("Tree.png", 865, 724, 24, "OOT Hyrule Field Tree Child Near Lake 3", "child", "Hyrule Field Tree Child Near Lake 3"),
                        MA("Tree.png", 844, 706, 24, "OOT Hyrule Field Tree Child Near Lake 4", "child", "Hyrule Field Tree Child Near Lake 4"),
                        MA("Tree.png", 810, 712, 24, "OOT Hyrule Field Tree Child Near Lake 5", "child", "Hyrule Field Tree Child Near Lake 5"),
                        MA("Tree.png", 799, 758, 24, "OOT Hyrule Field Tree Child Near Lake 6", "child", "Hyrule Field Tree Child Near Lake 6"),
                        MA("Tree.png", 414, 540, 24, "OOT Hyrule Field Tree Child Near Valley", "child", "Hyrule Field Tree Child Near Valley"),
                        M("Tree.png", 476, 243, 24, "OOT Hyrule Field Tree 01", "Hyrule Field Tree 01"),
                        M("Tree.png", 662, 120, 24, "OOT Hyrule Field Tree 02", "Hyrule Field Tree 02"),
                        M("Tree.png", 505, 96, 24, "OOT Hyrule Field Tree 03", "Hyrule Field Tree 03"),
                        M("Tree.png", 546, 138, 24, "OOT Hyrule Field Tree 04", "Hyrule Field Tree 04"),
                        M("Tree.png", 555, 94, 24, "OOT Hyrule Field Tree 05", "Hyrule Field Tree 05"),
                        M("Tree.png", 523, 78, 24, "OOT Hyrule Field Tree 06", "Hyrule Field Tree 06"),
                        M("Tree.png", 485, 75, 24, "OOT Hyrule Field Tree 07", "Hyrule Field Tree 07"),
                        M("Tree.png", 473, 108, 24, "OOT Hyrule Field Tree 08", "Hyrule Field Tree 08"),
                        M("Tree.png", 738, 758, 24, "OOT Hyrule Field Tree 09", "Hyrule Field Tree 09"),
                        M("Tree.png", 759, 825, 24, "OOT Hyrule Field Tree 10", "Hyrule Field Tree 10"),
                        M("Tree.png", 759, 767, 24, "OOT Hyrule Field Tree 11", "Hyrule Field Tree 11"),
                        M("Tree.png", 746, 744, 24, "OOT Hyrule Field Tree 12", "Hyrule Field Tree 12"),
                        M("Tree.png", 721, 743, 24, "OOT Hyrule Field Tree 13", "Hyrule Field Tree 13"),
                        M("Tree.png", 716, 767, 24, "OOT Hyrule Field Tree 14", "Hyrule Field Tree 14"),
                        M("Tree.png", 771, 698, 24, "OOT Hyrule Field Tree 15", "Hyrule Field Tree 15"),
                        M("Tree.png", 786, 755, 24, "OOT Hyrule Field Tree 16", "Hyrule Field Tree 16"),
                        M("Tree.png", 795, 698, 24, "OOT Hyrule Field Tree 17", "Hyrule Field Tree 17"),
                        M("Tree.png", 780, 683, 24, "OOT Hyrule Field Tree 18", "Hyrule Field Tree 18"),
                        M("Tree.png", 758, 686, 24, "OOT Hyrule Field Tree 19", "Hyrule Field Tree 19"),
                        M("Tree.png", 738, 737, 24, "OOT Hyrule Field Tree 20", "Hyrule Field Tree 20"),
                        M("Tree.png", 783, 767, 24, "OOT Hyrule Field Tree 21", "Hyrule Field Tree 21"),
                        M("Tree.png", 825, 803, 24, "OOT Hyrule Field Tree 22", "Hyrule Field Tree 22"),
                        M("Tree.png", 812, 771, 24, "OOT Hyrule Field Tree 23", "Hyrule Field Tree 23"),
                        M("Tree.png", 796, 750, 24, "OOT Hyrule Field Tree 24", "Hyrule Field Tree 24"),
                        M("Tree.png", 767, 760, 24, "OOT Hyrule Field Tree 25", "Hyrule Field Tree 25"),
                        M("Tree.png", 761, 811, 24, "OOT Hyrule Field Tree 26", "Hyrule Field Tree 26"),
                        M("Tree.png", 964, 476, 24, "OOT Hyrule Field Tree 27", "Hyrule Field Tree 27"),
                        M("Tree.png", 972, 432, 24, "OOT Hyrule Field Tree 28", "Hyrule Field Tree 28"),
                        M("Tree.png", 946, 441, 24, "OOT Hyrule Field Tree 29", "Hyrule Field Tree 29"),
                        M("Tree.png", 941, 467, 24, "OOT Hyrule Field Tree 30", "Hyrule Field Tree 30"),
                        M("Tree.png", 960, 497, 24, "OOT Hyrule Field Tree 31", "Hyrule Field Tree 31"),
                        M("Tree.png", 988, 464, 24, "OOT Hyrule Field Tree 32", "Hyrule Field Tree 32"),
                        M("Tree.png", 678, 133, 24, "OOT Hyrule Field Tree 33", "Hyrule Field Tree 33"),
                        M("Tree.png", 694, 145, 24, "OOT Hyrule Field Tree 34", "Hyrule Field Tree 34"),
                        M("Tree.png", 852, 87, 24, "OOT Hyrule Field Tree 35", "Hyrule Field Tree 35"),
                        M("Tree.png", 507, 824, 24, "OOT Hyrule Field Tree 36", "Hyrule Field Tree 36"),
                        M("Tree.png", 642, 372, 24, "OOT Hyrule Field Tree 37", "Hyrule Field Tree 37"),
                        M("Tree.png", 709, 684, 24, "OOT Hyrule Field Tree 38", "Hyrule Field Tree 38"),
                        M("Tree.png", 918, 325, 24, "OOT Hyrule Field Tree 39", "Hyrule Field Tree 39"),
                        M("Tree.png", 922, 149, 24, "OOT Hyrule Field Tree 40", "Hyrule Field Tree 40"),
                        M("Bush.png", 420, 477, 24, "OOT Hyrule Field Bush 01", "Hyrule Field Bush 01"),
                        M("Bush.png", 445, 512, 24, "OOT Hyrule Field Bush 02", "Hyrule Field Bush 02"),
                        M("Bush.png", 455, 477, 24, "OOT Hyrule Field Bush 03", "Hyrule Field Bush 03"),
                        M("Bush.png", 426, 434, 24, "OOT Hyrule Field Bush 04", "Hyrule Field Bush 04"),
                        M("Bush.png", 393, 453, 24, "OOT Hyrule Field Bush 05", "Hyrule Field Bush 05"),
                        M("Bush.png", 387, 490, 24, "OOT Hyrule Field Bush 06", "Hyrule Field Bush 06"),
                        M("Bush.png", 459, 788, 24, "OOT Hyrule Field Bush 07", "Hyrule Field Bush 07"),
                        M("Bush.png", 485, 812, 24, "OOT Hyrule Field Bush 08", "Hyrule Field Bush 08"),
                        M("Bush.png", 483, 778, 24, "OOT Hyrule Field Bush 09", "Hyrule Field Bush 09"),
                        M("Bush.png", 464, 762, 24, "OOT Hyrule Field Bush 10", "Hyrule Field Bush 10"),
                        M("Bush.png", 431, 788, 24, "OOT Hyrule Field Bush 11", "Hyrule Field Bush 11"),
                        M("Bush.png", 434, 817, 24, "OOT Hyrule Field Bush 12", "Hyrule Field Bush 12"),
                        M("Bush.png", 467, 177, 24, "OOT Hyrule Field Bush 13", "Hyrule Field Bush 13"),
                        M("Bush.png", 507, 212, 24, "OOT Hyrule Field Bush 14", "Hyrule Field Bush 14"),
                        M("Bush.png", 505, 178, 24, "OOT Hyrule Field Bush 15", "Hyrule Field Bush 15"),
                        M("Bush.png", 483, 160, 24, "OOT Hyrule Field Bush 16", "Hyrule Field Bush 16"),
                        M("Bush.png", 453, 158, 24, "OOT Hyrule Field Bush 17", "Hyrule Field Bush 17"),
                        M("Bush.png", 442, 192, 24, "OOT Hyrule Field Bush 18", "Hyrule Field Bush 18"),
                        M("Bush.png", 530, 821, 24, "OOT Hyrule Field Bush 19", "Hyrule Field Bush 19"),
                        M("Bush.png", 556, 826, 24, "OOT Hyrule Field Bush 20", "Hyrule Field Bush 20"),
                        M("Bush.png", 535, 795, 24, "OOT Hyrule Field Bush 21", "Hyrule Field Bush 21"),
                        M("Bush.png", 507, 804, 24, "OOT Hyrule Field Bush 22", "Hyrule Field Bush 22"),
                        M("Bush.png", 499, 848, 24, "OOT Hyrule Field Bush 23", "Hyrule Field Bush 23"),
                        M("Bush.png", 744, 792, 24, "OOT Hyrule Field Bush 24", "Hyrule Field Bush 24"),
                        M("Bush.png", 773, 835, 24, "OOT Hyrule Field Bush 25", "Hyrule Field Bush 25"),
                        M("Bush.png", 775, 799, 24, "OOT Hyrule Field Bush 26", "Hyrule Field Bush 26"),
                        M("Bush.png", 746, 771, 24, "OOT Hyrule Field Bush 27", "Hyrule Field Bush 27"),
                        M("Bush.png", 726, 768, 24, "OOT Hyrule Field Bush 28", "Hyrule Field Bush 28"),
                        M("Bush.png", 731, 813, 24, "OOT Hyrule Field Bush 29", "Hyrule Field Bush 29"),
                        M("Bush.png", 774, 666, 24, "OOT Hyrule Field Bush 30", "Hyrule Field Bush 30"),
                        M("Bush.png", 797, 712, 24, "OOT Hyrule Field Bush 31", "Hyrule Field Bush 31"),
                        M("Bush.png", 808, 666, 24, "OOT Hyrule Field Bush 32", "Hyrule Field Bush 32"),
                        M("Bush.png", 794, 645, 24, "OOT Hyrule Field Bush 33", "Hyrule Field Bush 33"),
                        M("Bush.png", 763, 645, 24, "OOT Hyrule Field Bush 34", "Hyrule Field Bush 34"),
                        M("Bush.png", 756, 699, 24, "OOT Hyrule Field Bush 35", "Hyrule Field Bush 35"),
                        MA("Bush.png", 483, 92, 24, "OOT Hyrule Field Bush Child 01", "child", "Hyrule Field Bush Child 01"),
                        MA("Bush.png", 457, 97, 24, "OOT Hyrule Field Bush Child 02", "child", "Hyrule Field Bush Child 02"),
                        MA("Bush.png", 471, 122, 24, "OOT Hyrule Field Bush Child 03", "child", "Hyrule Field Bush Child 03"),
                        MA("Bush.png", 505, 112, 24, "OOT Hyrule Field Bush Child 04", "child", "Hyrule Field Bush Child 04"),
                        MA("Bush.png", 503, 73, 24, "OOT Hyrule Field Bush Child 05", "child", "Hyrule Field Bush Child 05"),
                        MA("Bush.png", 541, 73, 24, "OOT Hyrule Field Bush Child 06", "child", "Hyrule Field Bush Child 06"),
                        MA("Bush.png", 594, 101, 24, "OOT Hyrule Field Bush Child 07", "child", "Hyrule Field Bush Child 07"),
                        MA("Bush.png", 578, 66, 24, "OOT Hyrule Field Bush Child 08", "child", "Hyrule Field Bush Child 08"),
                        MA("Bush.png", 551, 48, 24, "OOT Hyrule Field Bush Child 09", "child", "Hyrule Field Bush Child 09"),
                        MA("Bush.png", 523, 59, 24, "OOT Hyrule Field Bush Child 10", "child", "Hyrule Field Bush Child 10"),
                        MA("Bush.png", 523, 95, 24, "OOT Hyrule Field Bush Child 11", "child", "Hyrule Field Bush Child 11"),
                        MA("Bush.png", 672, 813, 24, "OOT Hyrule Field Bush Child 12", "child", "Hyrule Field Bush Child 12"),
                        MA("Bush.png", 696, 846, 24, "OOT Hyrule Field Bush Child 13", "child", "Hyrule Field Bush Child 13"),
                        MA("Bush.png", 701, 790, 24, "OOT Hyrule Field Bush Child 14", "child", "Hyrule Field Bush Child 14"),
                        MA("Bush.png", 675, 781, 24, "OOT Hyrule Field Bush Child 15", "child", "Hyrule Field Bush Child 15"),
                        MA("Bush.png", 646, 790, 24, "OOT Hyrule Field Bush Child 16", "child", "Hyrule Field Bush Child 16"),
                        MA("Bush.png", 645, 831, 24, "OOT Hyrule Field Bush Child 17", "child", "Hyrule Field Bush Child 17"),
                        MA("Bush.png", 812, 750, 24, "OOT Hyrule Field Bush Child 18", "child", "Hyrule Field Bush Child 18"),
                        MA("Bush.png", 846, 778, 24, "OOT Hyrule Field Bush Child 19", "child", "Hyrule Field Bush Child 19"),
                        MA("Bush.png", 837, 754, 24, "OOT Hyrule Field Bush Child 20", "child", "Hyrule Field Bush Child 20"),
                        MA("Bush.png", 830, 713, 24, "OOT Hyrule Field Bush Child 21", "child", "Hyrule Field Bush Child 21"),
                        MA("Bush.png", 790, 735, 24, "OOT Hyrule Field Bush Child 22", "child", "Hyrule Field Bush Child 22"),
                        MA("Bush.png", 794, 769, 24, "OOT Hyrule Field Bush Child 23", "child", "Hyrule Field Bush Child 23"),
                        M("Grass.png", 684, 624, 24, "OOT Hyrule Field Grass Pack 1 Bush 01", "Hyrule Field Grass Pack 1 Bush 01"),
                        M("Grass.png", 684, 611, 24, "OOT Hyrule Field Grass Pack 1 Bush 02", "Hyrule Field Grass Pack 1 Bush 02"),
                        M("Grass.png", 700, 611, 24, "OOT Hyrule Field Grass Pack 1 Bush 03", "Hyrule Field Grass Pack 1 Bush 03"),
                        M("Grass.png", 700, 624, 24, "OOT Hyrule Field Grass Pack 1 Bush 04", "Hyrule Field Grass Pack 1 Bush 04"),
                        M("Grass.png", 692, 639, 24, "OOT Hyrule Field Grass Pack 1 Bush 05", "Hyrule Field Grass Pack 1 Bush 05"),
                        M("Grass.png", 676, 631, 24, "OOT Hyrule Field Grass Pack 1 Bush 06", "Hyrule Field Grass Pack 1 Bush 06"),
                        M("Grass.png", 669, 616, 24, "OOT Hyrule Field Grass Pack 1 Bush 07", "Hyrule Field Grass Pack 1 Bush 07"),
                        M("Grass.png", 676, 601, 24, "OOT Hyrule Field Grass Pack 1 Bush 08", "Hyrule Field Grass Pack 1 Bush 08"),
                        M("Grass.png", 692, 593, 24, "OOT Hyrule Field Grass Pack 1 Bush 09", "Hyrule Field Grass Pack 1 Bush 09"),
                        M("Grass.png", 707, 601, 24, "OOT Hyrule Field Grass Pack 1 Bush 10", "Hyrule Field Grass Pack 1 Bush 10"),
                        M("Grass.png", 714, 616, 24, "OOT Hyrule Field Grass Pack 1 Bush 11", "Hyrule Field Grass Pack 1 Bush 11"),
                        M("Grass.png", 708, 632, 24, "OOT Hyrule Field Grass Pack 1 Bush 12", "Hyrule Field Grass Pack 1 Bush 12"),
                        M("Grass.png", 727, 674, 24, "OOT Hyrule Field Grass Pack 2 Bush 01", "Hyrule Field Grass Pack 2 Bush 01"),
                        M("Grass.png", 727, 661, 24, "OOT Hyrule Field Grass Pack 2 Bush 02", "Hyrule Field Grass Pack 2 Bush 02"),
                        M("Grass.png", 743, 661, 24, "OOT Hyrule Field Grass Pack 2 Bush 03", "Hyrule Field Grass Pack 2 Bush 03"),
                        M("Grass.png", 743, 674, 24, "OOT Hyrule Field Grass Pack 2 Bush 04", "Hyrule Field Grass Pack 2 Bush 04"),
                        M("Grass.png", 735, 689, 24, "OOT Hyrule Field Grass Pack 2 Bush 05", "Hyrule Field Grass Pack 2 Bush 05"),
                        M("Grass.png", 719, 681, 24, "OOT Hyrule Field Grass Pack 2 Bush 06", "Hyrule Field Grass Pack 2 Bush 06"),
                        M("Grass.png", 712, 666, 24, "OOT Hyrule Field Grass Pack 2 Bush 07", "Hyrule Field Grass Pack 2 Bush 07"),
                        M("Grass.png", 719, 651, 24, "OOT Hyrule Field Grass Pack 2 Bush 08", "Hyrule Field Grass Pack 2 Bush 08"),
                        M("Grass.png", 735, 643, 24, "OOT Hyrule Field Grass Pack 2 Bush 09", "Hyrule Field Grass Pack 2 Bush 09"),
                        M("Grass.png", 750, 651, 24, "OOT Hyrule Field Grass Pack 2 Bush 10", "Hyrule Field Grass Pack 2 Bush 10"),
                        M("Grass.png", 757, 666, 24, "OOT Hyrule Field Grass Pack 2 Bush 11", "Hyrule Field Grass Pack 2 Bush 11"),
                        M("Grass.png", 751, 682, 24, "OOT Hyrule Field Grass Pack 2 Bush 12", "Hyrule Field Grass Pack 2 Bush 12"),
                        M("Grass.png", 794, 294, 24, "OOT Hyrule Field Grass Pack 3 Bush 01", "Hyrule Field Grass Pack 3 Bush 01"),
                        M("Grass.png", 794, 281, 24, "OOT Hyrule Field Grass Pack 3 Bush 02", "Hyrule Field Grass Pack 3 Bush 02"),
                        M("Grass.png", 810, 281, 24, "OOT Hyrule Field Grass Pack 3 Bush 03", "Hyrule Field Grass Pack 3 Bush 03"),
                        M("Grass.png", 810, 294, 24, "OOT Hyrule Field Grass Pack 3 Bush 04", "Hyrule Field Grass Pack 3 Bush 04"),
                        M("Grass.png", 802, 309, 24, "OOT Hyrule Field Grass Pack 3 Bush 05", "Hyrule Field Grass Pack 3 Bush 05"),
                        M("Grass.png", 786, 301, 24, "OOT Hyrule Field Grass Pack 3 Bush 06", "Hyrule Field Grass Pack 3 Bush 06"),
                        M("Grass.png", 779, 286, 24, "OOT Hyrule Field Grass Pack 3 Bush 07", "Hyrule Field Grass Pack 3 Bush 07"),
                        M("Grass.png", 786, 271, 24, "OOT Hyrule Field Grass Pack 3 Bush 08", "Hyrule Field Grass Pack 3 Bush 08"),
                        M("Grass.png", 802, 263, 24, "OOT Hyrule Field Grass Pack 3 Bush 09", "Hyrule Field Grass Pack 3 Bush 09"),
                        M("Grass.png", 817, 271, 24, "OOT Hyrule Field Grass Pack 3 Bush 10", "Hyrule Field Grass Pack 3 Bush 10"),
                        M("Grass.png", 824, 286, 24, "OOT Hyrule Field Grass Pack 3 Bush 11", "Hyrule Field Grass Pack 3 Bush 11"),
                        M("Grass.png", 818, 302, 24, "OOT Hyrule Field Grass Pack 3 Bush 12", "Hyrule Field Grass Pack 3 Bush 12"),
                        M("Grass.png", 902, 507, 24, "OOT Hyrule Field Grass Pack 4 Bush 01", "Hyrule Field Grass Pack 4 Bush 01"),
                        M("Grass.png", 902, 494, 24, "OOT Hyrule Field Grass Pack 4 Bush 02", "Hyrule Field Grass Pack 4 Bush 02"),
                        M("Grass.png", 918, 494, 24, "OOT Hyrule Field Grass Pack 4 Bush 03", "Hyrule Field Grass Pack 4 Bush 03"),
                        M("Grass.png", 918, 507, 24, "OOT Hyrule Field Grass Pack 4 Bush 04", "Hyrule Field Grass Pack 4 Bush 04"),
                        M("Grass.png", 910, 522, 24, "OOT Hyrule Field Grass Pack 4 Bush 05", "Hyrule Field Grass Pack 4 Bush 05"),
                        M("Grass.png", 894, 514, 24, "OOT Hyrule Field Grass Pack 4 Bush 06", "Hyrule Field Grass Pack 4 Bush 06"),
                        M("Grass.png", 887, 499, 24, "OOT Hyrule Field Grass Pack 4 Bush 07", "Hyrule Field Grass Pack 4 Bush 07"),
                        M("Grass.png", 894, 484, 24, "OOT Hyrule Field Grass Pack 4 Bush 08", "Hyrule Field Grass Pack 4 Bush 08"),
                        M("Grass.png", 910, 476, 24, "OOT Hyrule Field Grass Pack 4 Bush 09", "Hyrule Field Grass Pack 4 Bush 09"),
                        M("Grass.png", 925, 484, 24, "OOT Hyrule Field Grass Pack 4 Bush 10", "Hyrule Field Grass Pack 4 Bush 10"),
                        M("Grass.png", 932, 499, 24, "OOT Hyrule Field Grass Pack 4 Bush 11", "Hyrule Field Grass Pack 4 Bush 11"),
                        M("Grass.png", 926, 515, 24, "OOT Hyrule Field Grass Pack 4 Bush 12", "Hyrule Field Grass Pack 4 Bush 12")
                    }
                },
                new MapSubRegion
                {
                    Name = "Deku Scrubs Grotto",
                    BackgroundImage = OoT("Hyrule", "Field_Scrubs"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_SCRUB_HEART_PIECE" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 675, 599, "Entrance shuffle (Hyrule Field)", "OOT_GROTTO_EXIT_SCRUB_HEART_PIECE"),
                        
                        M("Scrub.png", 687, 180, 40, "OOT Hyrule Field Grotto Scrub HP", "Hyrule Field Grotto Scrub HP"),
                        M("Hive.png", 820, 162, 40, "OOT Hyrule Field Grotto Scrub Hive", "Hyrule Field Grotto Scrub Hive"),
                        M("Fairy_Spot.png", 745, 292, 40, "OOT Hyrule Field Grotto Scrub Big Fairy", "Hyrule Field Grotto Scrub Big Fairy")
                    }
                },
                new MapSubRegion
                {
                    Name = "Open Grotto",
                    BackgroundImage = OoT("Hyrule", "Field_Open"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_GENERIC_HF_OPEN" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 651, 571, "Entrance shuffle (Hyrule Field)", "OOT_GROTTO_EXIT_GENERIC_HF_OPEN"),
                        
                        M("Butterfly.png", 699, 613, 24, "OOT Hyrule Field Grotto Open Butterfly 1", "Hyrule Field Grotto Open Butterfly 1"),
                        M("Butterfly.png", 615, 613, 24, "OOT Hyrule Field Grotto Open Butterfly 2", "Hyrule Field Grotto Open Butterfly 2"),
                        M("Butterfly.png", 657, 626, 24, "OOT Hyrule Field Grotto Open Butterfly 3", "Hyrule Field Grotto Open Butterfly 3"),
                        M("Chest.png", 669, 162, 40, "OOT Hyrule Field Grotto Open", "Hyrule Field Grotto Open"),
                        M("Grass.png", 658, 134, 24, "OOT Hyrule Field Grotto Open Grass 1", "Hyrule Field Grotto Open Grass 1"),
                        M("Grass.png", 643, 186, 24, "OOT Hyrule Field Grotto Open Grass 2", "Hyrule Field Grotto Open Grass 2"),
                        M("Grass.png", 716, 186, 24, "OOT Hyrule Field Grotto Open Grass 3", "Hyrule Field Grotto Open Grass 3"),
                        M("Grass.png", 648, 316, 24, "OOT Hyrule Field Grotto Open Grass 4", "Hyrule Field Grotto Open Grass 4"),
                        M("Hive.png", 530, 141, 40, "OOT Hyrule Field Grotto Open Hive 1", "Hyrule Field Grotto Open Hive 1"),
                        M("Hive.png", 717, 79, 40, "OOT Hyrule Field Grotto Open Hive 2", "Hyrule Field Grotto Open Hive 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Southeast Grotto",
                    BackgroundImage = OoT("Hyrule", "Field_SE"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_GENERIC_HF_SOUTHEAST" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 651, 571, "Entrance shuffle (Hyrule Field)", "OOT_GROTTO_EXIT_GENERIC_HF_SOUTHEAST"),
                        
                        M("Butterfly.png", 699, 613, 24, "OOT Hyrule Field Grotto Southeast Butterfly 1", "Hyrule Field Grotto Southeast Butterfly 1"),
                        M("Butterfly.png", 615, 613, 24, "OOT Hyrule Field Grotto Southeast Butterfly 2", "Hyrule Field Grotto Southeast Butterfly 2"),
                        M("Butterfly.png", 657, 626, 24, "OOT Hyrule Field Grotto Southeast Butterfly 3", "Hyrule Field Grotto Southeast Butterfly 3"),
                        M("Chest.png", 669, 162, 40, "OOT Hyrule Field Grotto Southeast", "Hyrule Field Grotto Southeast"),
                        M("Grass.png", 658, 134, 24, "OOT Hyrule Field Grotto Southeast Grass 1", "Hyrule Field Grotto Southeast Grass 1"),
                        M("Grass.png", 643, 186, 24, "OOT Hyrule Field Grotto Southeast Grass 2", "Hyrule Field Grotto Southeast Grass 2"),
                        M("Grass.png", 716, 186, 24, "OOT Hyrule Field Grotto Southeast Grass 3", "Hyrule Field Grotto Southeast Grass 3"),
                        M("Grass.png", 648, 316, 24, "OOT Hyrule Field Grotto Southeast Grass 4", "Hyrule Field Grotto Southeast Grass 4"),
                        M("Hive.png", 530, 141, 40, "OOT Hyrule Field Grotto Southeast Hive 1", "Hyrule Field Grotto Southeast Hive 1"),
                        M("Hive.png", 717, 79, 40, "OOT Hyrule Field Grotto Southeast Hive 2", "Hyrule Field Grotto Southeast Hive 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Market Side Grotto",
                    BackgroundImage = OoT("Hyrule", "Field_Market"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_GENERIC_HF_MARKET" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 651, 571, "Entrance shuffle (Hyrule Field)", "OOT_GROTTO_EXIT_GENERIC_HF_MARKET"),
                        
                        M("Butterfly.png", 699, 613, 24, "OOT Hyrule Field Grotto Market Butterfly 1", "Hyrule Field Grotto Market Butterfly 1"),
                        M("Butterfly.png", 615, 613, 24, "OOT Hyrule Field Grotto Market Butterfly 2", "Hyrule Field Grotto Market Butterfly 2"),
                        M("Butterfly.png", 657, 626, 24, "OOT Hyrule Field Grotto Market Butterfly 3", "Hyrule Field Grotto Market Butterfly 3"),
                        M("Chest.png", 669, 162, 40, "OOT Hyrule Field Grotto Market", "Hyrule Field Grotto Market"),
                        M("Grass.png", 658, 134, 24, "OOT Hyrule Field Grotto Market Grass 1", "Hyrule Field Grotto Market Grass 1"),
                        M("Grass.png", 643, 186, 24, "OOT Hyrule Field Grotto Market Grass 2", "Hyrule Field Grotto Market Grass 2"),
                        M("Grass.png", 716, 186, 24, "OOT Hyrule Field Grotto Market Grass 3", "Hyrule Field Grotto Market Grass 3"),
                        M("Grass.png", 648, 316, 24, "OOT Hyrule Field Grotto Market Grass 4", "Hyrule Field Grotto Market Grass 4"),
                        M("Hive.png", 530, 141, 40, "OOT Hyrule Field Grotto Market Hive 1", "Hyrule Field Grotto Market Hive 1"),
                        M("Hive.png", 717, 79, 40, "OOT Hyrule Field Grotto Market Hive 2", "Hyrule Field Grotto Market Hive 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Kakariko Side Grotto",
                    BackgroundImage = OoT("Hyrule", "Field_Kakariko"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_FIELD_TREE" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 677, 640, "Entrance shuffle (Hyrule Field)", "OOT_GROTTO_EXIT_FIELD_TREE"),
                        
                        M("Gold_Skulltula.png", 784, 149, 40, "OOT Hyrule Field Grotto Near Kakariko GS", "Hyrule Field Grotto Near Kakariko GS")
                    }
                },
                new MapSubRegion
                {
                    Name = "Gerudo Side Grotto",
                    BackgroundImage = OoT("Hyrule", "Field_Gerudo"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_FIELD_COW" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 708, 482, "Entrance shuffle (Hyrule Field)", "OOT_GROTTO_EXIT_FIELD_COW"),
                        
                        M("Gold_Skulltula.png", 156, 655, 40, "OOT Hyrule Field Grotto Near Gerudo GS", "Hyrule Field Grotto Near Gerudo GS"),
                        M("Cow.png", 350, 670, 40, "OOT Hyrule Field Cow", "Hyrule Field Cow"),
                        M("Pot.png", 494, 634, 24, "OOT Hyrule Field Cow Grotto Pot 1", "Hyrule Field Cow Grotto Pot 1"),
                        M("Pot.png", 461, 591, 24, "OOT Hyrule Field Cow Grotto Pot 2", "Hyrule Field Cow Grotto Pot 2"),
                        M("Grass.png", 331, 623, 24, "OOT Hyrule Field Grotto Near Gerudo Grass 1", "Hyrule Field Grotto Near Gerudo Grass 1"),
                        M("Grass.png", 434, 643, 24, "OOT Hyrule Field Grotto Near Gerudo Grass 2", "Hyrule Field Grotto Near Gerudo Grass 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Tektite Grotto",
                    BackgroundImage = OoT("Hyrule", "Field_Tektite"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_TEKTITE" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 700, 606, "Entrance shuffle (Hyrule Field)", "OOT_GROTTO_EXIT_TEKTITE"),
                        
                        M("Collectible.png", 706, 238, 40, "OOT Hyrule Field Grotto Tektite HP", "Hyrule Field Grotto Tektite HP")
                    }
                },
                new MapSubRegion
                {
                    Name = "Fairy Fountain",
                    BackgroundImage = OoT("Fairy", "Fountain"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_FAIRY_HF" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 465, 502, "Entrance shuffle (Hyrule Field)", "OOT_GROTTO_EXIT_FAIRY_HF"),
                        
                        M("Fairy.png", 470, 204, 24, "OOT Hyrule Field Fairy Fountain Fairy 1", "Hyrule Field Fairy Fountain Fairy 1"),
                        M("Fairy.png", 490, 200, 24, "OOT Hyrule Field Fairy Fountain Fairy 2", "Hyrule Field Fairy Fountain Fairy 2"),
                        M("Fairy.png", 450, 194, 24, "OOT Hyrule Field Fairy Fountain Fairy 3", "Hyrule Field Fairy Fountain Fairy 3"),
                        M("Fairy.png", 471, 181, 24, "OOT Hyrule Field Fairy Fountain Fairy 4", "Hyrule Field Fairy Fountain Fairy 4"),
                        M("Fairy.png", 493, 178, 24, "OOT Hyrule Field Fairy Fountain Fairy 5", "Hyrule Field Fairy Fountain Fairy 5"),
                        M("Fairy.png", 449, 172, 24, "OOT Hyrule Field Fairy Fountain Fairy 6", "Hyrule Field Fairy Fountain Fairy 6"),
                        M("Fairy.png", 485, 155, 24, "OOT Hyrule Field Fairy Fountain Fairy 7", "Hyrule Field Fairy Fountain Fairy 7"),
                        M("Fairy.png", 464, 153, 24, "OOT Hyrule Field Fairy Fountain Fairy 8", "Hyrule Field Fairy Fountain Fairy 8")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion Kakariko()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Kakariko";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Kakariko Village",
                    BackgroundImage = OoT("Kakariko", "Kakariko"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_VILLAGE_OWL",
						"OOT_KAKARIKO_FROM_BAZAAR",
						"OOT_KAKARIKO_FROM_IMPA",
						"OOT_KAKARIKO_FROM_CARPENTER",
						"OOT_KAKARIKO_FROM_DEATH_MOUNTAIN",
						"OOT_GROTTO_EXIT_GENERIC_KAKARIKO",
						"OOT_KAKARIKO_FROM_SHOP_POTION_BACK",
						"OOT_KAKARIKO_FROM_IMPA_BACK",
						"OOT_KAKARIKO_FROM_FIELD",
						"OOT_GROTTO_EXIT_REDEAD",
						"OOT_KAKARIKO_FROM_GRANNY",
						"OOT_KAKARIKO_FROM_BOTTOM_OF_THE_WELL",
						"OOT_KAKARIKO_FROM_WINDMILL",
						"OOT_KAKARIKO_FROM_SKULLTULA",
						"OOT_KAKARIKO_FROM_ARCHERY",
						"OOT_KAKARIKO_FROM_SHOP_POTION",
						"OOT_KAKARIKO_FROM_GRAVEYARD"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 863, 175, "Entrance shuffle (Hyrule Field)", "OOT_FIELD_FROM_KAKARIKO"),
                        ME("Entrance.png", 139, 129, "Entrance shuffle (Graveyard)", "OOT_GRAVEYARD"),
                        ME("Entrance.png", 437, 578, "Entrance shuffle (Death Mountain)", "OOT_DEATH_MOUNTAIN_FROM_KAKARIKO"),
                        ME("Entrance.png", 439, 309, "Entrance shuffle (Carpenter House)", "OOT_HOUSE_CARPENTER"),
                        ME("Entrance.png", 504, 181, "Entrance shuffle (Skulltula House)", "OOT_HOUSE_SKULLTULA"),
                        ME("Entrance.png", 484, 91, "Entrance shuffle (Impa House Front)", "OOT_HOUSE_IMPA"),
                        ME("Entrance.png", 411, 79, "Entrance shuffle (Impa House Back)", "OOT_HOUSE_IMPA_BACK"),
                        ME("Entrance.png", 363, 318, "Entrance shuffle (Granny Shop)", "OOT_SHOP_GRANNY"),
                        MEA("Entrance.png", 510, 425, "Entrance shuffle (Bazaar)", "adult", "OOT_KAKARIKO_BAZAAR"),
                        MEA("Entrance.png", 398, 224, "Entrance shuffle (Shooting Gallery Adult)", "adult", "OOT_ADULT_ARCHERY"),
                        ME("Entrance.png", 246, 253, "Entrance shuffle (Windmill)", "OOT_WINDMILL"),
                        ME("Entrance.png", 399, 442, "Entrance shuffle (Potion Shop)", "OOT_SHOP_POTION_KAKARIKO"),
                        MEA("Entrance.png", 360, 424, "Entrance shuffle (Potion Shop Back)", "adult", "OOT_SHOP_POTION_KAKARIKO_BACK"),
                        ME("Entrance.png", 312, 397, "Entrance shuffle (Kakariko Grotto)", "OOT_GROTTO_GENERIC_KAKARIKO"),
                        ME("Entrance.png", 508, 281, "Entrance shuffle (ReDead Grotto)", "OOT_GROTTO_REDEAD"),
                        ME("Entrance.png", 331, 258, "Entrance shuffle (Bottom of the Well)", "OOT_BOTTOM_OF_THE_WELL"),
                        
                        MA("Gold_Skulltula.png", 534, 467, 40, "OOT Kakariko GS Bazaar", "child", "Kakariko GS Bazaar"),
                        MA("Gold_Skulltula.png", 539, 167, 40, "OOT Kakariko GS House of Skulltula", "child", "Kakariko GS House of Skulltula"),
                        MA("Gold_Skulltula.png", 452, 368, 40, "OOT Kakariko GS Ladder", "child", "Kakariko GS Ladder"),
                        MA("Gold_Skulltula.png", 578, 272, 40, "OOT Kakariko GS Tree", "child", "Kakariko GS Tree"),
                        MA("Gold_Skulltula.png", 405, 191, 40, "OOT Kakariko GS Shooting Gallery", "child", "Kakariko GS Shooting Gallery"),
                        MA("Gold_Skulltula.png", 462, 47, 40, "OOT Kakariko GS Roof", "adult", "Kakariko GS Roof"),
                        MA("NPC.png", 374, 256, 40, "OOT Kakariko Song Shadow", "adult", "Kakariko Song Shadow"),
                        M("NPC.png", 390, 310, 40, "OOT Kakariko Man on Roof", "Kakariko Man on Roof"),
                        MA("NPC.png", 382, 128, 40, "OOT Kakariko Anju Bottle", "child", "Kakariko Anju Bottle"),
                        MA("NPC.png", 382, 128, 40, "OOT Kakariko Anju Egg", "adult", "Kakariko Anju Egg"),
                        MA("NPC.png", 348, 128, 40, "OOT Kakariko Anju Cojiro", "adult", "Kakariko Anju Cojiro"),
						M("NPC.png", 382, 88, 40, "OOT Hatch Pocket Cucco", "Hatch Pocket Cucco"),
                        MA("NPC.png", 479, 544, 40, "OOT Kakariko Sell Keaton Mask", "child", "Kakariko Sell Keaton Mask"),
                        M("Rock.png", 426, 547, 24, "OOT Kakariko Rock Near Gate", "Kakariko Rock Near Gate"),
                        M("Rock.png", 547, 146, 24, "OOT Kakariko Rock Near Spider House", "Kakariko Rock Near Spider House"),
                        MA("Wonder.png", 434, 171, 24, "OOT Kakariko Wonder Item", "child", "Kakariko Wonder Item"),
                        MA("Butterfly.png", 433, 367, 24, "OOT Kakariko Butterfly 1", "child", "Kakariko Butterfly 1"),
                        MA("Butterfly.png", 464, 357, 24, "OOT Kakariko Butterfly 2", "child", "Kakariko Butterfly 2"),
                        M("Grass.png", 599, 299, 24, "OOT Kakariko Grass 1", "Kakariko Grass 1"),
                        M("Grass.png", 598, 254, 24, "OOT Kakariko Grass 2", "Kakariko Grass 2"),
                        M("Grass.png", 570, 299, 24, "OOT Kakariko Grass 3", "Kakariko Grass 3"),
                        M("Grass.png", 570, 255, 24, "OOT Kakariko Grass 4", "Kakariko Grass 4"),
                        M("Grass.png", 557, 276, 24, "OOT Kakariko Grass 5", "Kakariko Grass 5"),
                        M("Grass.png", 311, 109, 24, "OOT Kakariko Grass 6", "Kakariko Grass 6"),
                        M("Grass.png", 303, 123, 24, "OOT Kakariko Grass 7", "Kakariko Grass 7"),
                        M("Grass.png", 290, 115, 24, "OOT Kakariko Grass 8", "Kakariko Grass 8"),
                        MA("Crate.png", 383, 368, 24, "OOT Kakariko Crate Adult Back 1", "adult", "Kakariko Crate Adult Back 1"),
                        MA("Crate.png", 371, 392, 24, "OOT Kakariko Crate Adult Back 2", "adult", "Kakariko Crate Adult Back 2"),
                        MA("Crate.png", 276, 349, 24, "OOT Kakariko Crate Adult Back 3", "adult", "Kakariko Crate Adult Back 3"),
                        MA("Crate.png", 300, 349, 24, "OOT Kakariko Crate Adult Back 4", "adult", "Kakariko Crate Adult Back 4"),
                        MA("Crate.png", 455, 283, 24, "OOT Kakariko Crate Adult Center 1", "adult", "Kakariko Crate Adult Center 1"),
                        MA("Crate.png", 431, 283, 24, "OOT Kakariko Crate Adult Center 2", "adult", "Kakariko Crate Adult Center 2"),
                        MA("Crate.png", 429, 235, 24, "OOT Kakariko Crate Adult Near Archery", "adult", "Kakariko Crate Adult Near Archery"),
                        MA("Crate.png", 581, 214, 24, "OOT Kakariko Crate Adult Near First House", "adult", "Kakariko Crate Adult Near First House"),
                        MA("Crate.png", 528, 120, 24, "OOT Kakariko Crate Adult Near Impa 1", "adult", "Kakariko Crate Adult Near Impa 1"),
                        MA("Crate.png", 528, 96, 24, "OOT Kakariko Crate Adult Near Impa 2", "adult", "Kakariko Crate Adult Near Impa 2"),
                        MA("Crate.png", 435, 471, 24, "OOT Kakariko Crate Adult Near Potion Shop", "adult", "Kakariko Crate Adult Near Potion Shop"),
                        MA("Crate.png", 547, 405, 24, "OOT Kakariko Crate Adult Near Trail 1", "adult", "Kakariko Crate Adult Near Trail 1"),
                        MA("Crate.png", 523, 394, 24, "OOT Kakariko Crate Adult Near Trail 2", "adult", "Kakariko Crate Adult Near Trail 2"),
                        MA("Crate.png", 203, 134, 24, "OOT Kakariko Crate Child Near Graveyard", "child", "Kakariko Crate Child Near Graveyard"),
                        MA("Crate.png", 478, 344, 24, "OOT Kakariko Crate Child Near House", "child", "Kakariko Crate Child Near House"),
                        MA("Crate.png", 559, 410, 24, "OOT Kakariko Crate Child Near Trail", "child", "Kakariko Crate Child Near Trail"),
                        MA("Crate.png", 287, 325, 24, "OOT Kakariko Crate Child Near Windmill Bottom", "child", "Kakariko Crate Child Near Windmill Bottom"),
                        MA("Crate.png", 286, 243, 24, "OOT Kakariko Crate Child Near Windmill Top", "child", "Kakariko Crate Child Near Windmill Top"),
                        MA("Pot.png", 337, 328, 24, "OOT Kakariko Back Pot 1", "child", "Kakariko Back Pot 1"),
                        MA("Pot.png", 315, 328, 24, "OOT Kakariko Back Pot 2", "child", "Kakariko Back Pot 2"),
                        MA("Pot.png", 438, 405, 24, "OOT Kakariko Pot 01", "child", "Kakariko Pot 01"),
                        MA("Pot.png", 421, 399, 24, "OOT Kakariko Pot 02", "child", "Kakariko Pot 02"),
                        MA("Pot.png", 404, 393, 24, "OOT Kakariko Pot 03", "child", "Kakariko Pot 03"),
                        MA("Pot.png", 527, 98, 24, "OOT Kakariko Pot 04", "child", "Kakariko Pot 04"),
                        MA("Pot.png", 527, 110, 24, "OOT Kakariko Pot 05", "child", "Kakariko Pot 05"),
                        MA("Pot.png", 527, 122, 24, "OOT Kakariko Pot 06", "child", "Kakariko Pot 06"),
                        MA("Pot.png", 536, 496, 24, "OOT Kakariko Pot 07", "child", "Kakariko Pot 07"),
                        MA("Pot.png", 519, 490, 24, "OOT Kakariko Pot 08", "child", "Kakariko Pot 08"),
                        MA("Pot.png", 503, 484, 24, "OOT Kakariko Pot 09", "child", "Kakariko Pot 09")
                    }
                },
                new MapSubRegion
                {
                    Name = "Bazaar",
                    BackgroundImage = OoT("Kakariko", "Bazaar"),
                    DestinationEntranceIds = new List<string> { "OOT_KAKARIKO_BAZAAR" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 543, 556, "Entrance shuffle (Kakariko Village)", "OOT_KAKARIKO_FROM_BAZAAR"),
                        
                        M("Shop.png", 358, 325, 40, "OOT Kakariko Bazaar Item 1", "Kakariko Bazaar Item 1"),
                        M("Shop.png", 306, 273, 40, "OOT Kakariko Bazaar Item 2", "Kakariko Bazaar Item 2"),
                        M("Shop.png", 358, 273, 40, "OOT Kakariko Bazaar Item 3", "Kakariko Bazaar Item 3"),
                        M("Shop.png", 306, 325, 40, "OOT Kakariko Bazaar Item 4", "Kakariko Bazaar Item 4"),
                        M("Shop.png", 585, 273, 40, "OOT Kakariko Bazaar Item 5", "Kakariko Bazaar Item 5"),
                        M("Shop.png", 637, 273, 40, "OOT Kakariko Bazaar Item 6", "Kakariko Bazaar Item 6"),
                        M("Shop.png", 637, 325, 40, "OOT Kakariko Bazaar Item 7", "Kakariko Bazaar Item 7"),
                        M("Shop.png", 585, 325, 40, "OOT Kakariko Bazaar Item 8", "Kakariko Bazaar Item 8")
                    }
                },
                new MapSubRegion
                {
                    Name = "Potion Shop",
                    BackgroundImage = OoT("Kakariko", "Potion_Shop"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_SHOP_POTION_KAKARIKO",
						"OOT_SHOP_POTION_KAKARIKO_BACK"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 610, 497, "Entrance shuffle (Kakariko Village Front)", "OOT_KAKARIKO_FROM_SHOP_POTION"),
                        ME("Entrance.png", 295, 324, "Entrance shuffle (Kakariko Village Back)", "OOT_KAKARIKO_FROM_SHOP_POTION_BACK"),
                        
                        M("Shop.png", 439, 251, 40, "OOT Kakariko Potion Shop Item 1", "Kakariko Potion Shop Item 1"),
                        M("Shop.png", 491, 251, 40, "OOT Kakariko Potion Shop Item 2", "Kakariko Potion Shop Item 2"),
                        M("Shop.png", 439, 303, 40, "OOT Kakariko Potion Shop Item 3", "Kakariko Potion Shop Item 3"),
                        M("Shop.png", 491, 303, 40, "OOT Kakariko Potion Shop Item 4", "Kakariko Potion Shop Item 4"),
                        M("Shop.png", 612, 251, 40, "OOT Kakariko Potion Shop Item 5", "Kakariko Potion Shop Item 5"),
                        M("Shop.png", 664, 251, 40, "OOT Kakariko Potion Shop Item 6", "Kakariko Potion Shop Item 6"),
                        M("Shop.png", 612, 303, 40, "OOT Kakariko Potion Shop Item 7", "Kakariko Potion Shop Item 7"),
                        M("Shop.png", 664, 303, 40, "OOT Kakariko Potion Shop Item 8", "Kakariko Potion Shop Item 8")
                    }
                },
                new MapSubRegion
                {
                    Name = "Granny Potion Shop",
                    BackgroundImage = OoT("Kakariko", "Granny_Potion_Shop"),
                    DestinationEntranceIds = new List<string> { "OOT_SHOP_GRANNY" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 468, 561, "Entrance shuffle (Kakariko Village)", "OOT_KAKARIKO_FROM_GRANNY"),
                        
                        M("NPC.png", 417, 379, 40, "OOT Kakariko Potion Shop Odd Potion", "Kakariko Potion Shop Odd Potion"),
                        M("NPC.png", 518, 379, 40, "OOT Kakariko Potion Shop Buy Blue Potion", "Kakariko Potion Shop Buy Blue Potion")
                    }
                },
                new MapSubRegion
                {
                    Name = "Shooting Gallery",
                    BackgroundImage = OoT("Kakariko", "Shooting"),
                    DestinationEntranceIds = new List<string> { "OOT_ADULT_ARCHERY" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 790, 329, "Entrance shuffle (Kakariko Village)", "OOT_KAKARIKO_FROM_ARCHERY"),
                        
                        M("NPC.png", 606, 337, 40, "OOT Shooting Gallery Adult", "Shooting Gallery Adult")
                    }
                },
                new MapSubRegion
                {
                    Name = "Windmill",
                    BackgroundImage = OoT("Kakariko", "Windmill"),
                    DestinationEntranceIds = new List<string> { "OOT_WINDMILL" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 847, 441, "Entrance shuffle (Kakariko Village)", "OOT_KAKARIKO_FROM_WINDMILL"),
                        
                        MA("NPC.png", 413, 387, 40, "OOT Windmill Song of Storms", "adult", "Windmill Song of Storms"),
                        M("Collectible.png", 676, 508, 40, "OOT Windmill HP", "Windmill HP")
                    }
                },
                new MapSubRegion
                {
                    Name = "Skulltula House",
                    BackgroundImage = OoT("Kakariko", "House_Skulltula"),
                    DestinationEntranceIds = new List<string> { "OOT_HOUSE_SKULLTULA" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 454, 548, "Entrance shuffle (Kakariko Village)", "OOT_KAKARIKO_FROM_SKULLTULA"),
                        
                        M("NPC.png", 639, 410, 40, "OOT Skulltula House 10 Tokens", "Skulltula House 10 Tokens"),
                        M("NPC.png", 271, 410, 40, "OOT Skulltula House 20 Tokens", "Skulltula House 20 Tokens"),
                        M("NPC.png", 454, 95, 40, "OOT Skulltula House 30 Tokens", "Skulltula House 30 Tokens"),
                        M("NPC.png", 270, 210, 40, "OOT Skulltula House 40 Tokens", "Skulltula House 40 Tokens"),
                        M("NPC.png", 630, 210, 40, "OOT Skulltula House 50 Tokens", "Skulltula House 50 Tokens")
                    }
                },
                new MapSubRegion
                {
                    Name = "Impa House",
                    BackgroundImage = OoT("Kakariko", "Impa_House"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_HOUSE_IMPA",
						"OOT_HOUSE_IMPA_BACK"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 472, 528, "Entrance shuffle (Kakariko Village Front)", "OOT_KAKARIKO_FROM_IMPA"),
                        ME("Entrance.png", 545, 306, "Entrance shuffle (Kakariko Village Back)", "OOT_KAKARIKO_FROM_IMPA_BACK"),
                        
                        M("Collectible.png", 566, 363, 40, "OOT Kakariko Impa House HP", "Kakariko Impa House HP"),
                        M("Cow.png", 630, 357, 40, "OOT Kakariko Cow", "Kakariko Cow"),
                        M("Wonder.png", 612, 264, 24, "OOT Kakariko Impa House Wonder Item", "Kakariko Impa House Wonder Item")
                    }
                },
                new MapSubRegion
                {
                    Name = "Carpenter House",
                    BackgroundImage = OoT("Kakariko", "Carpenter_Boss"),
                    DestinationEntranceIds = new List<string> { "OOT_HOUSE_CARPENTER" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 578, 224, "Entrance shuffle (Kakariko Village)", "OOT_KAKARIKO_FROM_CARPENTER")
                    }
                },
                new MapSubRegion
                {
                    Name = "Open Grotto",
                    BackgroundImage = OoT("Kakariko", "Open"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_GENERIC_KAKARIKO" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 651, 571, "Entrance shuffle (Kakariko Village)", "OOT_GROTTO_EXIT_GENERIC_KAKARIKO"),
                        
                        M("Butterfly.png", 699, 613, 24, "OOT Kakariko Grotto Butterfly 1", "Kakariko Grotto Butterfly 1"),
                        M("Butterfly.png", 615, 613, 24, "OOT Kakariko Grotto Butterfly 2", "Kakariko Grotto Butterfly 2"),
                        M("Butterfly.png", 657, 626, 24, "OOT Kakariko Grotto Butterfly 3", "Kakariko Grotto Butterfly 3"),
                        M("Chest.png", 669, 162, 40, "OOT Kakariko Grotto Back", "Kakariko Grotto Back"),
                        M("Grass.png", 658, 134, 24, "OOT Kakariko Grotto Back Grass 1", "Kakariko Grotto Back Grass 1"),
                        M("Grass.png", 643, 186, 24, "OOT Kakariko Grotto Back Grass 2", "Kakariko Grotto Back Grass 2"),
                        M("Grass.png", 716, 186, 24, "OOT Kakariko Grotto Back Grass 3", "Kakariko Grotto Back Grass 3"),
                        M("Grass.png", 648, 316, 24, "OOT Kakariko Grotto Back Grass 4", "Kakariko Grotto Back Grass 4"),
                        M("Hive.png", 530, 141, 40, "OOT Kakariko Grotto Back Hive 1", "Kakariko Grotto Back Hive 1"),
                        M("Hive.png", 717, 79, 40, "OOT Kakariko Grotto Back Hive 2", "Kakariko Grotto Back Hive 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "ReDead Grotto",
                    BackgroundImage = OoT("Kakariko", "Redead"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_REDEAD" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 676, 600, "Entrance shuffle (Kakariko Village)", "OOT_GROTTO_EXIT_REDEAD"),
                        
                        M("Chest.png", 679, 261, 40, "OOT Kakariko Grotto Front", "Kakariko Grotto Front")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion KokiriForest()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Kokiri Forest";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Kokiri Forest",
                    BackgroundImage = OoT("Kokiri_Forest", "Kokiri_Forest"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_KOKIRI_FOREST_FROM_SHOP",
						"OOT_KOKIRI_FOREST_FROM_KNOW_IT_ALL",
						"OOT_KOKIRI_FOREST_FROM_MIDO",
						"OOT_KOKIRI_FOREST_FROM_SARIA",
						"OOT_GROTTO_EXIT_GENERIC_KOKIRI_FOREST",
						"OOT_KOKIRI_FOREST_FROM_TWINS",
						"OOT_KOKIRI_FOREST_FROM_LINK",
						"OOT_KOKIRI_FOREST_FROM_DEKU_TREE",
						"OOT_KOKIRI_FOREST_FROM_LOST_WOODS",
						"OOT_FOREST_FROM_LOST_WOODS_BRIDGE"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 1210, 299, "Entrance shuffle (Deku Tree)", "OOT_DEKU_TREE"),
                        ME("Entrance.png", 519, 15, "Entrance shuffle (Lost Woods)", "OOT_LOST_WOODS_FROM_KOKIRI_FOREST"),
                        ME("Entrance.png", 319, 486, "Entrance shuffle (Lost Woods Bridge)", "OOT_LOST_WOODS_BRIDGE_FROM_FOREST"),
                        ME("Entrance.png", 581, 315, "Entrance shuffle (Mido's House)", "OOT_HOUSE_MIDO"),
                        ME("Entrance.png", 969, 571, "Entrance shuffle (Saria's House)", "OOT_HOUSE_SARIA"),
                        ME("Entrance.png", 1116, 475, "Entrance shuffle (Twins House)", "OOT_HOUSE_TWINS"),
                        ME("Entrance.png", 472, 644, "Entrance shuffle (Know-It-All House)", "OOT_HOUSE_KNOW_IT_ALL"),
                        ME("Entrance.png", 965, 246, "Entrance shuffle (Kokiri Shop)", "OOT_KOKIRI_SHOP"),
                        ME("Entrance.png", 875, 763, "Entrance shuffle (Link's House)", "OOT_HOUSE_LINK"),
                        ME("Entrance.png", 490, 80, "Entrance shuffle (Song of Storms Grotto)", "OOT_GROTTO_GENERIC_KOKIRI_FOREST"),
                        
                        MA("Chest.png", 934, 1385, 40, "OOT Kokiri Forest Kokiri Sword Chest", "child", "Kokiri Forest Kokiri Sword Chest"),
                        MA("Gold_Skulltula.png", 1186, 479, 40, "OOT Kokiri Forest GS Night Adult", "adult", "Kokiri Forest GS Night Adult"),
                        MA("Gold_Skulltula.png", 396, 675, 40, "OOT Kokiri Forest GS Night Child", "child", "Kokiri Forest GS Night Child"),
                        MA("Gold_Skulltula.png", 1023, 171, 40, "OOT Kokiri Forest GS Soil", "child", "Kokiri Forest GS Soil"),
                        MA("Grass.png", 1047, 530, 24, "OOT Kokiri Forest Grass Adult 01", "adult", "Kokiri Forest Grass Adult 01"),
                        MA("Grass.png", 1047, 516, 24, "OOT Kokiri Forest Grass Adult 02", "adult", "Kokiri Forest Grass Adult 02"),
                        MA("Grass.png", 1063, 516, 24, "OOT Kokiri Forest Grass Adult 03", "adult", "Kokiri Forest Grass Adult 03"),
                        MA("Grass.png", 1063, 530, 24, "OOT Kokiri Forest Grass Adult 04", "adult", "Kokiri Forest Grass Adult 04"),
                        MA("Grass.png", 1055, 539, 24, "OOT Kokiri Forest Grass Adult 05", "adult", "Kokiri Forest Grass Adult 05"),
                        MA("Grass.png", 1036, 535, 24, "OOT Kokiri Forest Grass Adult 06", "adult", "Kokiri Forest Grass Adult 06"),
                        MA("Grass.png", 1034, 521, 24, "OOT Kokiri Forest Grass Adult 07", "adult", "Kokiri Forest Grass Adult 07"),
                        MA("Grass.png", 1036, 506, 24, "OOT Kokiri Forest Grass Adult 08", "adult", "Kokiri Forest Grass Adult 08"),
                        MA("Grass.png", 1055, 502, 24, "OOT Kokiri Forest Grass Adult 09", "adult", "Kokiri Forest Grass Adult 09"),
                        MA("Grass.png", 1071, 506, 24, "OOT Kokiri Forest Grass Adult 10", "adult", "Kokiri Forest Grass Adult 10"),
                        MA("Grass.png", 1074, 521, 24, "OOT Kokiri Forest Grass Adult 11", "adult", "Kokiri Forest Grass Adult 11"),
                        MA("Grass.png", 1071, 535, 24, "OOT Kokiri Forest Grass Adult 12", "adult", "Kokiri Forest Grass Adult 12"),
                        MA("Grass.png", 585, 720, 24, "OOT Kokiri Forest Grass Adult Near Crawl 1", "adult", "Kokiri Forest Grass Adult Near Crawl 1"),
                        MA("Grass.png", 585, 694, 24, "OOT Kokiri Forest Grass Adult Near Crawl 2", "adult", "Kokiri Forest Grass Adult Near Crawl 2"),
                        MA("Grass.png", 596, 707, 24, "OOT Kokiri Forest Grass Adult Near Crawl 3", "adult", "Kokiri Forest Grass Adult Near Crawl 3"),
                        MA("Grass.png", 607, 719, 24, "OOT Kokiri Forest Grass Adult Near Crawl 4", "adult", "Kokiri Forest Grass Adult Near Crawl 4"),
                        MA("Grass.png", 607, 693, 24, "OOT Kokiri Forest Grass Adult Near Crawl 5", "adult", "Kokiri Forest Grass Adult Near Crawl 5"),
                        MA("Grass.png", 618, 706, 24, "OOT Kokiri Forest Grass Adult Near Crawl 6", "adult", "Kokiri Forest Grass Adult Near Crawl 6"),
                        MA("Grass.png", 692, 711, 24, "OOT Kokiri Forest Grass Adult Near Crawl 7", "adult", "Kokiri Forest Grass Adult Near Crawl 7"),
                        MA("Grass.png", 692, 688, 24, "OOT Kokiri Forest Grass Adult Near Crawl 8", "adult", "Kokiri Forest Grass Adult Near Crawl 8"),
                        MA("Grass.png", 948, 562, 24, "OOT Kokiri Forest Grass Child 1", "child", "Kokiri Forest Grass Child 1"),
                        MA("Grass.png", 992, 548, 24, "OOT Kokiri Forest Grass Child 2", "child", "Kokiri Forest Grass Child 2"),
                        MA("Grass.png", 993, 528, 24, "OOT Kokiri Forest Grass Child 3", "child", "Kokiri Forest Grass Child 3"),
                        MA("Grass.png", 1014, 541, 24, "OOT Kokiri Forest Grass Child 4", "child", "Kokiri Forest Grass Child 4"),
                        MA("Grass.png", 550, 1020, 24, "OOT Kokiri Forest Grass Child Crawl 1", "child", "Kokiri Forest Grass Child Crawl 1"),
                        MA("Grass.png", 708, 1207, 24, "OOT Kokiri Forest Grass Child Crawl 2", "child", "Kokiri Forest Grass Child Crawl 2"),
                        MA("Grass.png", 915, 1418, 24, "OOT Kokiri Forest Grass Child Crawl 3", "child", "Kokiri Forest Grass Child Crawl 3"),
                        MA("Grass.png", 1010, 521, 24, "OOT Kokiri Forest Grass Child Kokiri", "child", "Kokiri Forest Grass Child Kokiri"),
                        MA("Grass.png", 586, 695, 24, "OOT Kokiri Forest Grass Child Near Crawl 1", "child", "Kokiri Forest Grass Child Near Crawl 1"),
                        MA("Grass.png", 595, 713, 24, "OOT Kokiri Forest Grass Child Near Crawl 2", "child", "Kokiri Forest Grass Child Near Crawl 2"),
                        MA("Grass.png", 622, 716, 24, "OOT Kokiri Forest Grass Child Near Crawl 3", "child", "Kokiri Forest Grass Child Near Crawl 3"),
                        MA("Grass.png", 615, 693, 24, "OOT Kokiri Forest Grass Child Near Crawl 4", "child", "Kokiri Forest Grass Child Near Crawl 4"),
                        MA("Grass.png", 643, 698, 24, "OOT Kokiri Forest Grass Child Near Crawl 5", "child", "Kokiri Forest Grass Child Near Crawl 5"),
                        MA("Grass.png", 665, 709, 24, "OOT Kokiri Forest Grass Child Near Crawl 6", "child", "Kokiri Forest Grass Child Near Crawl 6"),
                        MA("Grass.png", 692, 711, 24, "OOT Kokiri Forest Grass Child Near Crawl 7", "child", "Kokiri Forest Grass Child Near Crawl 7"),
                        MA("Grass.png", 692, 688, 24, "OOT Kokiri Forest Grass Child Near Crawl 8", "child", "Kokiri Forest Grass Child Near Crawl 8"),
                        MA("Heart.png", 989, 632, 24, "OOT Kokiri Forest Heart 1", "child", "Kokiri Forest Heart 1"),
                        MA("Heart.png", 1002, 595, 24, "OOT Kokiri Forest Heart 2", "child", "Kokiri Forest Heart 2"),
                        MA("Heart.png", 1031, 620, 24, "OOT Kokiri Forest Heart 3", "child", "Kokiri Forest Heart 3"),
                        MA("Rock.png", 590, 797, 24, "OOT Kokiri Forest Rock Child 1", "child", "Kokiri Forest Rock Child 1"),
                        MA("Rock.png", 589, 771, 24, "OOT Kokiri Forest Rock Child 2", "child", "Kokiri Forest Rock Child 2"),
                        MA("Rock.png", 699, 773, 24, "OOT Kokiri Forest Rock Child 3", "child", "Kokiri Forest Rock Child 3"),
                        MA("Rock.png", 649, 355, 24, "OOT Kokiri Forest Rock Child 4", "child", "Kokiri Forest Rock Child 4"),
                        MA("Rock.png", 386, 596, 24, "OOT Kokiri Forest Rock Child 5", "child", "Kokiri Forest Rock Child 5"),
                        MA("Rock.png", 502, 302, 24, "OOT Kokiri Forest Rock Child 6", "child", "Kokiri Forest Rock Child 6"),
                        MA("Rock.png", 913, 593, 24, "OOT Kokiri Forest Rock Child 7", "child", "Kokiri Forest Rock Child 7"),
                        MA("Rock.png", 1071, 651, 24, "OOT Kokiri Forest Rock Child 8", "child", "Kokiri Forest Rock Child 8"),
                        M("Rock.png", 649, 379, 24, "OOT Kokiri Forest Rock Circle Rock 1", "Kokiri Forest Rock Circle Rock 1"),
                        M("Rock.png", 666, 372, 24, "OOT Kokiri Forest Rock Circle Rock 2", "Kokiri Forest Rock Circle Rock 2"),
                        M("Rock.png", 673, 355, 24, "OOT Kokiri Forest Rock Circle Rock 3", "Kokiri Forest Rock Circle Rock 3"),
                        M("Rock.png", 666, 338, 24, "OOT Kokiri Forest Rock Circle Rock 4", "Kokiri Forest Rock Circle Rock 4"),
                        M("Rock.png", 649, 331, 24, "OOT Kokiri Forest Rock Circle Rock 5", "Kokiri Forest Rock Circle Rock 5"),
                        M("Rock.png", 632, 338, 24, "OOT Kokiri Forest Rock Circle Rock 6", "Kokiri Forest Rock Circle Rock 6"),
                        M("Rock.png", 625, 355, 24, "OOT Kokiri Forest Rock Circle Rock 7", "Kokiri Forest Rock Circle Rock 7"),
                        M("Rock.png", 632, 372, 24, "OOT Kokiri Forest Rock Circle Rock 8", "Kokiri Forest Rock Circle Rock 8"),
                        MA("Rupee.png", 1096, 144, 24, "OOT Kokiri Forest Rupee Adult 1", "adult", "Kokiri Forest Rupee Adult 1"),
                        MA("Rupee.png", 1114, 132, 24, "OOT Kokiri Forest Rupee Adult 2", "adult", "Kokiri Forest Rupee Adult 2"),
                        MA("Rupee.png", 1114, 108, 24, "OOT Kokiri Forest Rupee Adult 3", "adult", "Kokiri Forest Rupee Adult 3"),
                        MA("Rupee.png", 1096, 96, 24, "OOT Kokiri Forest Rupee Adult 4", "adult", "Kokiri Forest Rupee Adult 4"),
                        MA("Rupee.png", 1078, 108, 24, "OOT Kokiri Forest Rupee Adult 5", "adult", "Kokiri Forest Rupee Adult 5"),
                        MA("Rupee.png", 1078, 132, 24, "OOT Kokiri Forest Rupee Adult 6", "adult", "Kokiri Forest Rupee Adult 6"),
                        MA("Rupee.png", 1096, 120, 24, "OOT Kokiri Forest Rupee Adult 7", "adult", "Kokiri Forest Rupee Adult 7"),
                        MA("Rupee.png", 578, 238, 24, "OOT Kokiri Forest Rupee Child 1", "child", "Kokiri Forest Rupee Child 1"),
                        MA("Rupee.png", 771, 423, 24, "OOT Kokiri Forest Rupee Child 2", "child", "Kokiri Forest Rupee Child 2"),
                        MA("Rupee.png", 535, 1035, 24, "OOT Kokiri Forest Rupee Crawl 1", "child", "Kokiri Forest Rupee Crawl 1"),
                        MA("Rupee.png", 693, 1195, 24, "OOT Kokiri Forest Rupee Crawl 2", "child", "Kokiri Forest Rupee Crawl 2"),
                        MA("Soil.png", 1047, 201, 24, "OOT Kokiri Forest Soil 1", "child", "Kokiri Forest Soil 1"),
                        MA("Soil.png", 1063, 175, 24, "OOT Kokiri Forest Soil 2", "child", "Kokiri Forest Soil 2"),
                        MA("Soil.png", 1020, 201, 24, "OOT Kokiri Forest Soil 3", "child", "Kokiri Forest Soil 3"),
                        MA("Wonder.png", 767, 1086, 24, "OOT Kokiri Forest Wonder Item Crawl 1", "child", "Kokiri Forest Wonder Item Crawl 1"),
                        MA("Wonder.png", 767, 1164, 24, "OOT Kokiri Forest Wonder Item Crawl 2", "child", "Kokiri Forest Wonder Item Crawl 2"),
                        MA("Wonder.png", 644, 525, 24, "OOT Kokiri Forest Wonder Item Invisible Rupee 1", "child", "Kokiri Forest Wonder Item Invisible Rupee 1"),
                        MA("Wonder.png", 671, 539, 24, "OOT Kokiri Forest Wonder Item Invisible Rupee 2", "child", "Kokiri Forest Wonder Item Invisible Rupee 2"),
                        MA("Wonder.png", 727, 300, 24, "OOT Kokiri Forest Wonder Item Invisible Rupee 3", "child", "Kokiri Forest Wonder Item Invisible Rupee 3"),
                        MA("Wonder.png", 759, 294, 24, "OOT Kokiri Forest Wonder Item Invisible Rupee 4", "child", "Kokiri Forest Wonder Item Invisible Rupee 4"),
                        MA("Wonder.png", 640, 802, 24, "OOT Kokiri Forest Wonder Item Midair 1", "child", "Kokiri Forest Wonder Item Midair 1"),
                        MA("Wonder.png", 643, 751, 24, "OOT Kokiri Forest Wonder Item Midair 2", "child", "Kokiri Forest Wonder Item Midair 2"),
                        MA("Wonder.png", 659, 777, 24, "OOT Kokiri Forest Wonder Item Midair 3", "child", "Kokiri Forest Wonder Item Midair 3"),
                        MA("Wonder.png", 1080, 373, 24, "OOT Kokiri Forest Wonder Item Platforms 1", "child", "Kokiri Forest Wonder Item Platforms 1"),
                        MA("Wonder.png", 850, 349, 24, "OOT Kokiri Forest Wonder Item Platforms 2", "child", "Kokiri Forest Wonder Item Platforms 2"),
                        MA("Wonder.png", 691, 659, 24, "OOT Kokiri Forest Wonder Item Sign", "child", "Kokiri Forest Wonder Item Sign")
                    }
                },
                new MapSubRegion
                {
                    Name = "Shop",
                    BackgroundImage = OoT("Kokiri_Forest", "Shop"),
                    DestinationEntranceIds = new List<string> { "OOT_KOKIRI_SHOP" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 410, 559, "Entrance shuffle (Kokiri Forest)", "OOT_KOKIRI_FOREST_FROM_SHOP"),
                        
                        M("Wonder.png", 676, 448, 24, "OOT Kokiri Shop Wonder Item", "Kokiri Shop Wonder Item"),
                        M("Shop.png", 324, 318, 40, "OOT Kokiri Shop Item 1", "Kokiri Shop Item 1"),
                        M("Shop.png", 393, 318, 40, "OOT Kokiri Shop Item 2", "Kokiri Shop Item 2"),
                        M("Shop.png", 324, 378, 40, "OOT Kokiri Shop Item 3", "Kokiri Shop Item 3"),
                        M("Shop.png", 393, 378, 40, "OOT Kokiri Shop Item 4", "Kokiri Shop Item 4"),
                        M("Shop.png", 499, 318, 40, "OOT Kokiri Shop Item 5", "Kokiri Shop Item 5"),
                        M("Shop.png", 568, 378, 40, "OOT Kokiri Shop Item 6", "Kokiri Shop Item 6"),
                        M("Shop.png", 568, 318, 40, "OOT Kokiri Shop Item 7", "Kokiri Shop Item 7"),
                        M("Shop.png", 499, 378, 40, "OOT Kokiri Shop Item 8", "Kokiri Shop Item 8")
                    }
                },
                new MapSubRegion
                {
                    Name = "Link House",
                    BackgroundImage = OoT("Kokiri_Forest", "Link_House"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_HOUSE_LINK",
						"OOT_SPAWN_CHILD"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 694, 779, "Entrance shuffle (Kokiri Forest)", "OOT_KOKIRI_FOREST_FROM_LINK"),
                        
                        M("Pot.png", 988, 292, 24, "OOT Link's House Pot", "Link's House Pot"),
                        MA("Cow.png", 884, 581, 40, "OOT Kokiri Forest Cow", "adult", "Kokiri Forest Cow")
                    }
                },
                new MapSubRegion
                {
                    Name = "Mido House",
                    BackgroundImage = OoT("Kokiri_Forest", "Mido_House"),
                    DestinationEntranceIds = new List<string> { "OOT_HOUSE_MIDO" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 693, 750, "Entrance shuffle (Kokiri Forest)", "OOT_KOKIRI_FOREST_FROM_MIDO"),
                        
                        M("Chest.png", 796, 523, 40, "OOT Mido's House Bottom Left", "Mido's House Bottom Left"),
                        M("Chest.png", 796, 397, 40, "OOT Mido's House Bottom Right", "Mido's House Bottom Right"),
                        M("Chest.png", 588, 523, 40, "OOT Mido's House Top Left", "Mido's House Top Left"),
                        M("Chest.png", 588, 397, 40, "OOT Mido's House Top Right", "Mido's House Top Right")
                    }
                },
                new MapSubRegion
                {
                    Name = "Saria House",
                    BackgroundImage = OoT("Kokiri_Forest", "Saria_House"),
                    DestinationEntranceIds = new List<string> { "OOT_HOUSE_SARIA" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 452, 523, "Entrance shuffle (Kokiri Forest)", "OOT_KOKIRI_FOREST_FROM_SARIA"),
                        
                        M("Heart.png", 414, 365, 24, "OOT Saria's House Heart 1", "Saria's House Heart 1"),
                        M("Heart.png", 414, 257, 24, "OOT Saria's House Heart 2", "Saria's House Heart 2"),
                        M("Heart.png", 508, 257, 24, "OOT Saria's House Heart 3", "Saria's House Heart 3"),
                        M("Heart.png", 508, 365, 24, "OOT Saria's House Heart 4", "Saria's House Heart 4")
                    }
                },
                new MapSubRegion
                {
                    Name = "Know-It-All Brothers House",
                    BackgroundImage = OoT("Kokiri_Forest", "Know_It_All"),
                    DestinationEntranceIds = new List<string> { "OOT_HOUSE_KNOW_IT_ALL" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 692, 734, "Entrance shuffle (Kokiri Forest)", "OOT_KOKIRI_FOREST_FROM_KNOW_IT_ALL"),
                        
                        M("Pot.png", 957, 494, 24, "OOT Know It All House Pot 1", "Know It All House Pot 1"),
                        M("Pot.png", 823, 224, 24, "OOT Know It All House Pot 2", "Know It All House Pot 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Twins House",
                    BackgroundImage = OoT("Kokiri_Forest", "Twins_House"),
                    DestinationEntranceIds = new List<string> { "OOT_HOUSE_TWINS" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 693, 739, "Entrance shuffle (Kokiri Forest)", "OOT_KOKIRI_FOREST_FROM_TWINS"),
                        
                        M("Pot.png", 647, 541, 24, "OOT Twins House Pot 1", "Twins House Pot 1"),
                        M("Pot.png", 647, 382, 24, "OOT Twins House Pot 2", "Twins House Pot 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Song of Storms Grotto",
                    BackgroundImage = OoT("Kokiri_Forest", "Storms"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_GENERIC_KOKIRI_FOREST" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 651, 571, "Entrance shuffle (Kokiri Forest)", "OOT_GROTTO_EXIT_GENERIC_KOKIRI_FOREST"),
                        
                        M("Butterfly.png", 699, 613, 24, "OOT Kokiri Forest Storms Grotto Butterfly 1", "Kokiri Forest Storms Grotto Butterfly 1"),
                        M("Butterfly.png", 615, 613, 24, "OOT Kokiri Forest Storms Grotto Butterfly 2", "Kokiri Forest Storms Grotto Butterfly 2"),
                        M("Butterfly.png", 657, 626, 24, "OOT Kokiri Forest Storms Grotto Butterfly 3", "Kokiri Forest Storms Grotto Butterfly 3"),
                        M("Chest.png", 669, 162, 40, "OOT Kokiri Forest Storms Grotto", "Kokiri Forest Storms Grotto"),
                        M("Grass.png", 658, 134, 24, "OOT Kokiri Forest Storms Grotto Grass 1", "Kokiri Forest Storms Grotto Grass 1"),
                        M("Grass.png", 643, 186, 24, "OOT Kokiri Forest Storms Grotto Grass 2", "Kokiri Forest Storms Grotto Grass 2"),
                        M("Grass.png", 716, 186, 24, "OOT Kokiri Forest Storms Grotto Grass 3", "Kokiri Forest Storms Grotto Grass 3"),
                        M("Grass.png", 648, 316, 24, "OOT Kokiri Forest Storms Grotto Grass 4", "Kokiri Forest Storms Grotto Grass 4"),
                        M("Hive.png", 530, 141, 40, "OOT Kokiri Forest Storms Grotto Hive 1", "Kokiri Forest Storms Grotto Hive 1"),
                        M("Hive.png", 717, 79, 40, "OOT Kokiri Forest Storms Grotto Hive 2", "Kokiri Forest Storms Grotto Hive 2")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion LakeHylia()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Lake Hylia";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Lake Hylia",
                    BackgroundImage = OoT("Lake_Hylia", "Lake_Hylia"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_LAKE_HYLIA_FROM_FIELD",
						"OOT_LAKE_HYLIA_FROM_TEMPLE_WATER",
						"OOT_LAKE_HYLIA_FROM_LABORATORY",
						"OOT_GROTTO_EXIT_SCRUBS3_LAKE",
						"OOT_LAKE_HYLIA_FROM_ZORA_DOMAIN",
						"OOT_WARP_SONG_LAKE",
						"OOT_LAKE_HYLIA_FROM_FISHING_POND",
						"OOT_LAKE_HYLIA_FROM_VALLEY"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 666, 325, "Entrance shuffle (Water Temple)", "OOT_TEMPLE_WATER"),
                        ME("Entrance.png", 787, 845, "Entrance shuffle (Hyrule Field)", "OOT_FIELD_FROM_LAKE_HYLIA"),
                        ME("Entrance.png", 663, 591, "Entrance shuffle (Zora Domain)", "OOT_ZORA_DOMAIN_FROM_LAKE_HYLIA"),
                        ME("Entrance.png", 857, 574, "Entrance shuffle (Laboratory)", "OOT_LABORATORY"),
                        ME("Entrance.png", 401, 574, "Entrance shuffle (Fishing Pond)", "OOT_FISHING_POND"),
                        ME("Entrance.png", 889, 334, "Entrance shuffle (Scrubs Grotto)", "OOT_GROTTO_SCRUBS3_LAKE"),
                        
                        MA("Butterfly.png", 584, 662, 24, "OOT Lake Hylia Butterfly 1", "child", "Lake Hylia Butterfly 1"),
                        MA("Butterfly.png", 593, 678, 24, "OOT Lake Hylia Butterfly 2", "child", "Lake Hylia Butterfly 2"),
                        MA("Butterfly.png", 603, 695, 24, "OOT Lake Hylia Butterfly 3", "child", "Lake Hylia Butterfly 3"),
                        MA("NPC.png", 515, 219, 40, "OOT Lake Hylia Fire Arrow", "adult", "Lake Hylia Fire Arrow"),
                        M("Grass.png", 650, 223, 24, "OOT Lake Hylia Grass 1", "Lake Hylia Grass 1"),
                        M("Grass.png", 630, 213, 24, "OOT Lake Hylia Grass 2", "Lake Hylia Grass 2"),
                        MA("Grass.png", 609, 661, 24, "OOT Lake Hylia Grass Child 1", "child", "Lake Hylia Grass Child 1"),
                        MA("Grass.png", 613, 678, 24, "OOT Lake Hylia Grass Child 2", "child", "Lake Hylia Grass Child 2"),
                        MA("Grass.png", 579, 688, 24, "OOT Lake Hylia Grass Child 3", "child", "Lake Hylia Grass Child 3"),
                        MA("Grass.png", 570, 713, 24, "OOT Lake Hylia Grass Child 4", "child", "Lake Hylia Grass Child 4"),
                        MA("Grass.png", 590, 723, 24, "OOT Lake Hylia Grass Child 5", "child", "Lake Hylia Grass Child 5"),
                        M("Grass.png", 806, 678, 24, "OOT Lake Hylia Grass Pack 1 Bush 01", "Lake Hylia Grass Pack 1 Bush 01"),
                        M("Grass.png", 806, 666, 24, "OOT Lake Hylia Grass Pack 1 Bush 02", "Lake Hylia Grass Pack 1 Bush 02"),
                        M("Grass.png", 822, 666, 24, "OOT Lake Hylia Grass Pack 1 Bush 03", "Lake Hylia Grass Pack 1 Bush 03"),
                        M("Grass.png", 822, 678, 24, "OOT Lake Hylia Grass Pack 1 Bush 04", "Lake Hylia Grass Pack 1 Bush 04"),
                        M("Grass.png", 814, 690, 24, "OOT Lake Hylia Grass Pack 1 Bush 05", "Lake Hylia Grass Pack 1 Bush 05"),
                        M("Grass.png", 795, 685, 24, "OOT Lake Hylia Grass Pack 1 Bush 06", "Lake Hylia Grass Pack 1 Bush 06"),
                        M("Grass.png", 791, 671, 24, "OOT Lake Hylia Grass Pack 1 Bush 07", "Lake Hylia Grass Pack 1 Bush 07"),
                        M("Grass.png", 795, 656, 24, "OOT Lake Hylia Grass Pack 1 Bush 08", "Lake Hylia Grass Pack 1 Bush 08"),
                        M("Grass.png", 814, 650, 24, "OOT Lake Hylia Grass Pack 1 Bush 09", "Lake Hylia Grass Pack 1 Bush 09"),
                        M("Grass.png", 832, 656, 24, "OOT Lake Hylia Grass Pack 1 Bush 10", "Lake Hylia Grass Pack 1 Bush 10"),
                        M("Grass.png", 836, 671, 24, "OOT Lake Hylia Grass Pack 1 Bush 11", "Lake Hylia Grass Pack 1 Bush 11"),
                        M("Grass.png", 832, 685, 24, "OOT Lake Hylia Grass Pack 1 Bush 12", "Lake Hylia Grass Pack 1 Bush 12"),
                        M("Grass.png", 786, 739, 24, "OOT Lake Hylia Grass Pack 2 Bush 01", "Lake Hylia Grass Pack 2 Bush 01"),
                        M("Grass.png", 786, 727, 24, "OOT Lake Hylia Grass Pack 2 Bush 02", "Lake Hylia Grass Pack 2 Bush 02"),
                        M("Grass.png", 802, 727, 24, "OOT Lake Hylia Grass Pack 2 Bush 03", "Lake Hylia Grass Pack 2 Bush 03"),
                        M("Grass.png", 802, 739, 24, "OOT Lake Hylia Grass Pack 2 Bush 04", "Lake Hylia Grass Pack 2 Bush 04"),
                        M("Grass.png", 794, 751, 24, "OOT Lake Hylia Grass Pack 2 Bush 05", "Lake Hylia Grass Pack 2 Bush 05"),
                        M("Grass.png", 775, 746, 24, "OOT Lake Hylia Grass Pack 2 Bush 06", "Lake Hylia Grass Pack 2 Bush 06"),
                        M("Grass.png", 771, 732, 24, "OOT Lake Hylia Grass Pack 2 Bush 07", "Lake Hylia Grass Pack 2 Bush 07"),
                        M("Grass.png", 775, 717, 24, "OOT Lake Hylia Grass Pack 2 Bush 08", "Lake Hylia Grass Pack 2 Bush 08"),
                        M("Grass.png", 794, 711, 24, "OOT Lake Hylia Grass Pack 2 Bush 09", "Lake Hylia Grass Pack 2 Bush 09"),
                        M("Grass.png", 812, 717, 24, "OOT Lake Hylia Grass Pack 2 Bush 10", "Lake Hylia Grass Pack 2 Bush 10"),
                        M("Grass.png", 816, 732, 24, "OOT Lake Hylia Grass Pack 2 Bush 11", "Lake Hylia Grass Pack 2 Bush 11"),
                        M("Grass.png", 812, 746, 24, "OOT Lake Hylia Grass Pack 2 Bush 12", "Lake Hylia Grass Pack 2 Bush 12"),
                        M("Grass.png", 729, 703, 24, "OOT Lake Hylia Grass Pack 3 Bush 01", "Lake Hylia Grass Pack 3 Bush 01"),
                        M("Grass.png", 729, 691, 24, "OOT Lake Hylia Grass Pack 3 Bush 02", "Lake Hylia Grass Pack 3 Bush 02"),
                        M("Grass.png", 745, 691, 24, "OOT Lake Hylia Grass Pack 3 Bush 03", "Lake Hylia Grass Pack 3 Bush 03"),
                        M("Grass.png", 745, 703, 24, "OOT Lake Hylia Grass Pack 3 Bush 04", "Lake Hylia Grass Pack 3 Bush 04"),
                        M("Grass.png", 737, 715, 24, "OOT Lake Hylia Grass Pack 3 Bush 05", "Lake Hylia Grass Pack 3 Bush 05"),
                        M("Grass.png", 718, 710, 24, "OOT Lake Hylia Grass Pack 3 Bush 06", "Lake Hylia Grass Pack 3 Bush 06"),
                        M("Grass.png", 714, 696, 24, "OOT Lake Hylia Grass Pack 3 Bush 07", "Lake Hylia Grass Pack 3 Bush 07"),
                        M("Grass.png", 718, 681, 24, "OOT Lake Hylia Grass Pack 3 Bush 08", "Lake Hylia Grass Pack 3 Bush 08"),
                        M("Grass.png", 737, 675, 24, "OOT Lake Hylia Grass Pack 3 Bush 09", "Lake Hylia Grass Pack 3 Bush 09"),
                        M("Grass.png", 755, 681, 24, "OOT Lake Hylia Grass Pack 3 Bush 10", "Lake Hylia Grass Pack 3 Bush 10"),
                        M("Grass.png", 759, 696, 24, "OOT Lake Hylia Grass Pack 3 Bush 11", "Lake Hylia Grass Pack 3 Bush 11"),
                        M("Grass.png", 755, 710, 24, "OOT Lake Hylia Grass Pack 3 Bush 12", "Lake Hylia Grass Pack 3 Bush 12"),
                        MA("Gold_Skulltula.png", 643, 183, 40, "OOT Lake Hylia GS Big Tree", "adult", "Lake Hylia GS Big Tree"),
                        MA("Gold_Skulltula.png", 489, 231, 40, "OOT Lake Hylia GS Island", "child", "Lake Hylia GS Island"),
                        MA("Gold_Skulltula.png", 881, 558, 40, "OOT Lake Hylia GS Lab Wall", "child", "Lake Hylia GS Lab Wall"),
                        MA("Gold_Skulltula.png", 842, 601, 40, "OOT Lake Hylia GS Soil", "child", "Lake Hylia GS Soil"),
                        M("Collectible.png", 847, 564, 40, "OOT Lake Hylia HP", "Lake Hylia HP"),
                        M("Fairy_Spot.png", 490, 202, 40, "OOT Lake Hylia Island Big Fairy", "Lake Hylia Island Big Fairy"),
                        MA("Pot.png", 825, 579, 24, "OOT Lake Hylia Pot 1", "child", "Lake Hylia Pot 1"),
                        MA("Pot.png", 825, 567, 24, "OOT Lake Hylia Pot 2", "child", "Lake Hylia Pot 2"),
                        M("Rock.png", 449, 558, 24, "OOT Lake Hylia Rock", "Lake Hylia Rock"),
                        MA("Rupee.png", 683, 567, 24, "OOT Lake Hylia Rupee 1", "child", "Lake Hylia Rupee 1"),
                        MA("Rupee.png", 673, 556, 24, "OOT Lake Hylia Rupee 2", "child", "Lake Hylia Rupee 2"),
                        MA("Rupee.png", 662, 546, 24, "OOT Lake Hylia Rupee 3", "child", "Lake Hylia Rupee 3"),
                        MA("Soil.png", 850, 630, 24, "OOT Lake Hylia Soil 1", "child", "Lake Hylia Soil 1"),
                        MA("Soil.png", 871, 614, 24, "OOT Lake Hylia Soil 2", "child", "Lake Hylia Soil 2"),
                        MA("Soil.png", 828, 614, 24, "OOT Lake Hylia Soil 3", "child", "Lake Hylia Soil 3"),
                        MA("NPC.png", 658, 514, 40, "OOT Lake Hylia Underwater Bottle", "child", "Lake Hylia Underwater Bottle")
                    }
                },
                new MapSubRegion
                {
                    Name = "Fishing Pond",
                    BackgroundImage = OoT("Lake_Hylia", "Pond"),
                    DestinationEntranceIds = new List<string> { "OOT_FISHING_POND" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 670, 22, "Entrance shuffle (Lake Hylia)", "OOT_LAKE_HYLIA_FROM_FISHING_POND"),
                        
                        MA("NPC.png", 618, 31, 40, "OOT Fishing Pond Adult", "adult", "Fishing Pond Adult"),
                        MA("NPC.png", 618, 31, 40, "OOT Fishing Pond Child", "child", "Fishing Pond Child"),
                        MA("Fish.png", 542, 260, 24, "OOT Fishing Pond Adult Fish 1", "adult", "Fishing Pond Adult Fish 1"),
                        MA("Fish.png", 494, 292, 24, "OOT Fishing Pond Adult Fish 2", "adult", "Fishing Pond Adult Fish 2"),
                        MA("Fish.png", 463, 355, 24, "OOT Fishing Pond Adult Fish 3", "adult", "Fishing Pond Adult Fish 3"),
                        MA("Fish.png", 487, 552, 24, "OOT Fishing Pond Adult Fish 4", "adult", "Fishing Pond Adult Fish 4"),
                        MA("Fish.png", 528, 568, 24, "OOT Fishing Pond Adult Fish 5", "adult", "Fishing Pond Adult Fish 5"),
                        MA("Fish.png", 499, 613, 24, "OOT Fishing Pond Adult Fish 6", "adult", "Fishing Pond Adult Fish 6"),
                        MA("Fish.png", 721, 638, 24, "OOT Fishing Pond Adult Fish 7", "adult", "Fishing Pond Adult Fish 7"),
                        MA("Fish.png", 769, 620, 24, "OOT Fishing Pond Adult Fish 8", "adult", "Fishing Pond Adult Fish 8"),
                        MA("Fish.png", 808, 590, 24, "OOT Fishing Pond Adult Fish 9", "adult", "Fishing Pond Adult Fish 9"),
                        MA("Fish.png", 733, 514, 24, "OOT Fishing Pond Adult Fish 10", "adult", "Fishing Pond Adult Fish 10"),
                        MA("Fish.png", 858, 353, 24, "OOT Fishing Pond Adult Fish 11", "adult", "Fishing Pond Adult Fish 11"),
                        MA("Fish.png", 542, 531, 24, "OOT Fishing Pond Adult Fish 12", "adult", "Fishing Pond Adult Fish 12"),
                        MA("Fish.png", 603, 525, 24, "OOT Fishing Pond Adult Fish 13", "adult", "Fishing Pond Adult Fish 13"),
                        MA("Fish.png", 571, 551, 24, "OOT Fishing Pond Adult Fish 14", "adult", "Fishing Pond Adult Fish 14"),
                        MA("Fish.png", 704, 491, 24, "OOT Fishing Pond Adult Fish 15", "adult", "Fishing Pond Adult Fish 15"),
                        MA("Fish.png", 750, 584, 24, "OOT Fishing Pond Adult Loach", "adult", "Fishing Pond Adult Loach"),
                        MA("Fish.png", 542, 260, 24, "OOT Fishing Pond Child Fish 1", "child", "Fishing Pond Child Fish 1"),
                        MA("Fish.png", 494, 292, 24, "OOT Fishing Pond Child Fish 2", "child", "Fishing Pond Child Fish 2"),
                        MA("Fish.png", 463, 355, 24, "OOT Fishing Pond Child Fish 3", "child", "Fishing Pond Child Fish 3"),
                        MA("Fish.png", 487, 552, 24, "OOT Fishing Pond Child Fish 4", "child", "Fishing Pond Child Fish 4"),
                        MA("Fish.png", 528, 568, 24, "OOT Fishing Pond Child Fish 5", "child", "Fishing Pond Child Fish 5"),
                        MA("Fish.png", 499, 613, 24, "OOT Fishing Pond Child Fish 6", "child", "Fishing Pond Child Fish 6"),
                        MA("Fish.png", 721, 638, 24, "OOT Fishing Pond Child Fish 7", "child", "Fishing Pond Child Fish 7"),
                        MA("Fish.png", 769, 620, 24, "OOT Fishing Pond Child Fish 8", "child", "Fishing Pond Child Fish 8"),
                        MA("Fish.png", 808, 590, 24, "OOT Fishing Pond Child Fish 9", "child", "Fishing Pond Child Fish 9"),
                        MA("Fish.png", 678, 431, 24, "OOT Fishing Pond Child Fish 10", "child", "Fishing Pond Child Fish 10"),
                        MA("Fish.png", 858, 353, 24, "OOT Fishing Pond Child Fish 11", "child", "Fishing Pond Child Fish 11"),
                        MA("Fish.png", 542, 531, 24, "OOT Fishing Pond Child Fish 12", "child", "Fishing Pond Child Fish 12"),
                        MA("Fish.png", 603, 525, 24, "OOT Fishing Pond Child Fish 13", "child", "Fishing Pond Child Fish 13"),
                        MA("Fish.png", 571, 551, 24, "OOT Fishing Pond Child Fish 14", "child", "Fishing Pond Child Fish 14"),
                        MA("Fish.png", 704, 491, 24, "OOT Fishing Pond Child Fish 15", "child", "Fishing Pond Child Fish 15"),
                        MA("Fish.png", 750, 584, 24, "OOT Fishing Pond Child Loach 1", "child", "Fishing Pond Child Loach 1"),
                        MA("Fish.png", 536, 323, 24, "OOT Fishing Pond Child Loach 2", "child", "Fishing Pond Child Loach 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Laboratory",
                    BackgroundImage = OoT("Lake_Hylia", "Laboratory"),
                    DestinationEntranceIds = new List<string> { "OOT_LABORATORY" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 484, 194, "Entrance shuffle (Lake Hylia)", "OOT_LAKE_HYLIA_FROM_LABORATORY"),
                        
                        M("Gold_Skulltula.png", 407, 614, 40, "OOT Laboratory GS Crate", "Laboratory GS Crate"),
                        M("NPC.png", 481, 456, 40, "OOT Laboratory Dive", "Laboratory Dive"),
                        M("NPC.png", 577, 421, 40, "OOT Laboratory Eye Drops", "Laboratory Eye Drops"),
                        M("Rupee.png", 442, 682, 24, "OOT Laboratory Rupee 1", "Laboratory Rupee 1"),
                        M("Rupee.png", 526, 556, 24, "OOT Laboratory Rupee 2", "Laboratory Rupee 2"),
                        M("Rupee.png", 621, 619, 24, "OOT Laboratory Rupee 3", "Laboratory Rupee 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Deku Scrubs Grotto",
                    BackgroundImage = OoT("Lake_Hylia", "Scrubs"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_SCRUBS3_LAKE" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 676, 690, "Entrance shuffle (Lake Hylia)", "OOT_GROTTO_EXIT_SCRUBS3_LAKE"),
                        
                        M("Hive.png", 962, 194, 40, "OOT Lake Hylia Grotto Hive", "Lake Hylia Grotto Hive"),
                        M("Scrub.png", 450, 335, 40, "OOT Lake Hylia Grotto Left Scrub", "Lake Hylia Grotto Left Scrub"),
                        M("Scrub.png", 672, 94, 40, "OOT Lake Hylia Grotto Center Scrub", "Lake Hylia Grotto Center Scrub"),
                        M("Scrub.png", 922, 335, 40, "OOT Lake Hylia Grotto Right Scrub", "Lake Hylia Grotto Right Scrub")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion LonLonRanch()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Lon Lon Ranch";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Lon Lon Ranch",
                    BackgroundImage = OoT("Ranch", "Lon_Lon_Ranch"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_LON_LON_RANCH_FROM_SILO",
						"OOT_LON_LON_RANCH_FROM_FIELD",
						"OOT_LON_LON_RANCH_FROM_STABLES",
						"OOT_LON_LON_RANCH_FROM_HOUSE",
						"OOT_GROTTO_EXIT_SCRUBS3_RANCH"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 833, 412, "Entrance shuffle (Hyrule Field)", "OOT_FIELD_FROM_LON_LON_RANCH"),
                        ME("Entrance.png", 755, 450, "Entrance shuffle (Talon House)", "OOT_HOUSE_LON_LON"),
                        ME("Entrance.png", 731, 382, "Entrance shuffle (Stable)", "OOT_STABLES"),
                        ME("Entrance.png", 176, 94, "Entrance shuffle (Silo)", "OOT_SILO"),
                        ME("Entrance.png", 203, 471, "Entrance shuffle (Scrubs Grotto)", "OOT_GROTTO_SCRUBS3_RANCH"),
                        
                        MA("Crate.png", 676, 452, 24, "OOT Lon Lon Ranch Crate", "child", "Lon Lon Ranch Crate"),
                        MA("Gold_Skulltula.png", 260, 11, 40, "OOT Lon Lon Ranch GS Back Wall", "child", "Lon Lon Ranch GS Back Wall"),
                        MA("Gold_Skulltula.png", 727, 428, 40, "OOT Lon Lon Ranch GS House", "child", "Lon Lon Ranch GS House"),
                        MA("Gold_Skulltula.png", 308, 399, 40, "OOT Lon Lon Ranch GS Rain Shed", "child", "Lon Lon Ranch GS Rain Shed"),
                        MA("Gold_Skulltula.png", 645, 476, 40, "OOT Lon Lon Ranch GS Tree", "child", "Lon Lon Ranch GS Tree"),
                        MA("NPC.png", 441, 291, 40, "OOT Lon Lon Ranch Malon Song", "child", "Lon Lon Ranch Malon Song"),
                        MA("Pot.png", 796, 401, 24, "OOT Lon Lon Ranch Pot 1", "child", "Lon Lon Ranch Pot 1"),
                        MA("Pot.png", 752, 401, 24, "OOT Lon Lon Ranch Pot 2", "child", "Lon Lon Ranch Pot 2"),
                        MA("Pot.png", 818, 401, 24, "OOT Lon Lon Ranch Pot 3", "child", "Lon Lon Ranch Pot 3"),
                        MA("Pot.png", 774, 401, 24, "OOT Lon Lon Ranch Pot 4", "child", "Lon Lon Ranch Pot 4"),
                        MA("Pot.png", 367, 413, 24, "OOT Lon Lon Ranch Pot 5", "child", "Lon Lon Ranch Pot 5"),
                        MA("Pot.png", 347, 425, 24, "OOT Lon Lon Ranch Pot 6", "child", "Lon Lon Ranch Pot 6"),
                        MA("Pot.png", 347, 401, 24, "OOT Lon Lon Ranch Pot 7", "child", "Lon Lon Ranch Pot 7"),
                        MA("Wonder.png", 498, 351, 24, "OOT Lon Lon Ranch Wonder Item 1", "adult", "Lon Lon Ranch Wonder Item 1"),
                        MA("Wonder.png", 396, 245, 24, "OOT Lon Lon Ranch Wonder Item 2", "adult", "Lon Lon Ranch Wonder Item 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Talon House",
                    BackgroundImage = OoT("Ranch", "Ranch_House_Silo"),
                    DestinationEntranceIds = new List<string> { "OOT_HOUSE_LON_LON" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 510, 236, "Entrance shuffle (Lon Lon Ranch)", "OOT_LON_LON_RANCH_FROM_HOUSE"),
                        
                        M("NPC.png", 449, 377, 40, "OOT Lon Lon Ranch Talon Bottle", "Lon Lon Ranch Talon Bottle"),
                        M("NPC.png", 491, 377, 40, "OOT Lon Lon Ranch Talon Buy Milk", "Lon Lon Ranch Talon Buy Milk"),
                        M("Pot.png", 348, 431, 24, "OOT Lon Lon Ranch Talon House Pot 1", "Lon Lon Ranch Talon House Pot 1"),
                        M("Pot.png", 348, 407, 24, "OOT Lon Lon Ranch Talon House Pot 2", "Lon Lon Ranch Talon House Pot 2"),
                        M("Pot.png", 348, 343, 24, "OOT Lon Lon Ranch Talon House Pot 3", "Lon Lon Ranch Talon House Pot 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Stable",
                    BackgroundImage = OoT("Ranch", "Stable"),
                    DestinationEntranceIds = new List<string> { "OOT_STABLES" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 433, 112, "Entrance shuffle (Lon Lon Ranch)", "OOT_LON_LON_RANCH_FROM_STABLES"),
                        
                        M("Cow.png", 686, 513, 40, "OOT Lon Lon Ranch Stables Cow Left", "Lon Lon Ranch Stables Cow Left"),
                        M("Cow.png", 587, 513, 40, "OOT Lon Lon Ranch Stables Cow Right", "Lon Lon Ranch Stables Cow Right")
                    }
                },
                new MapSubRegion
                {
                    Name = "Silo",
                    BackgroundImage = OoT("Ranch", "Silo"),
                    DestinationEntranceIds = new List<string> { "OOT_SILO" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 331, 386, "Entrance shuffle (Lon Lon Ranch)", "OOT_LON_LON_RANCH_FROM_SILO"),
                        
                        M("Cow.png", 536, 357, 40, "OOT Lon Lon Ranch Silo Cow Back", "Lon Lon Ranch Silo Cow Back"),
                        M("Cow.png", 489, 386, 40, "OOT Lon Lon Ranch Silo Cow Front", "Lon Lon Ranch Silo Cow Front"),
                        M("Collectible.png", 733, 252, 40, "OOT Lon Lon Ranch Silo HP", "Lon Lon Ranch Silo HP")
                    }
                },
                new MapSubRegion
                {
                    Name = "Deku Scrubs Grotto",
                    BackgroundImage = OoT("Ranch", "Scrubs"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_SCRUBS3_RANCH" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 676, 690, "Entrance shuffle (Lon Lon Ranch)", "OOT_GROTTO_EXIT_SCRUBS3_RANCH"),
                        
                        M("Hive.png", 962, 194, 40, "OOT Lon Lon Ranch Grotto Hive", "Lon Lon Ranch Grotto Hive"),
                        M("Scrub.png", 450, 335, 40, "OOT Lon Lon Ranch Grotto Left Scrub", "Lon Lon Ranch Grotto Left Scrub"),
                        M("Scrub.png", 672, 94, 40, "OOT Lon Lon Ranch Grotto Center Scrub", "Lon Lon Ranch Grotto Center Scrub"),
                        M("Scrub.png", 922, 335, 40, "OOT Lon Lon Ranch Grotto Right Scrub", "Lon Lon Ranch Grotto Right Scrub")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion LostWoods()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Lost Woods";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Lost Woods",
                    BackgroundImage = OoT("Lost_Woods", "Lost_Woods"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_GROTTO_EXIT_GENERIC_LOST_WOODS",
						"OOT_LOST_WOODS_FROM_GORON_CITY",
						"OOT_GROTTO_EXIT_SCRUB_UPGRADE",
						"OOT_LOST_WOODS_BRIDGE_FROM_FIELD",
						"OOT_GROTTO_EXIT_DEKU_THEATER",
						"OOT_LOST_WOODS_FROM_ZORA_RIVER",
						"OOT_LOST_WOODS_FROM_MEADOW",
						"OOT_LOST_WOODS_FROM_KOKIRI_FOREST",
						"OOT_LOST_WOODS_BRIDGE_FROM_FOREST",
						"OOT_LOST_WOODS_FROM_LOST_WOODS_NORTH",
						"OOT_LOST_WOODS_FROM_LOST_WOODS_EAST",
						"OOT_LOST_WOODS_FROM_LOST_WOODS_SOUTH",
						"OOT_LOST_WOODS_FROM_LOST_WOODS_WEST"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 94, 2766, "Entrance shuffle (Hyrule Field Bridge)", "OOT_FIELD_FROM_LOST_WOODS_BRIDGE"),
                        ME("Entrance.png", 2583, 1446, "Entrance shuffle (Zora River)", "OOT_ZORA_RIVER_FROM_LOST_WOODS"),
                        ME("Entrance.png", 1628, 1289, "Entrance shuffle (Goron City)", "OOT_GORON_CITY_FROM_LOST_WOODS"),
                        ME("Entrance.png", 1433, 92, "Entrance shuffle (Sacred Meadow)", "OOT_SACRED_FOREST_MEADOW"),
                        ME("Entrance.png", 1024, 2183, "Entrance shuffle (Kokiri Forest)", "OOT_KOKIRI_FOREST_FROM_LOST_WOODS"),
                        ME("Entrance.png", 295, 2766, "Entrance shuffle (Kokiri Forest Bridge)", "OOT_FOREST_FROM_LOST_WOODS_BRIDGE"),
                        ME("Entrance.png", 1744, 1360, "Entrance shuffle (Generic Grotto)", "OOT_GROTTO_GENERIC_LOST_WOODS"),
                        ME("Entrance.png", 1307, 164, "Entrance shuffle (Scrub Upgrade Grotto)", "OOT_GROTTO_SCRUB_UPGRADE"),
                        ME("Entrance.png", 1021, 921, "Entrance shuffle (Deku Theater)", "OOT_GROTTO_DEKU_THEATER"),
                        ME("Entrance.png", 1036, 1956, "Entrance shuffle (Lost Woods North)", "OOT_LOST_WOODS_FROM_LOST_WOODS_NORTH"),
                        ME("Entrance.png", 1076, 1996, "Entrance shuffle (Lost Woods East)", "OOT_LOST_WOODS_FROM_LOST_WOODS_EAST"),
                        ME("Entrance.png", 1036, 2036, "Entrance shuffle (Lost Woods South)", "OOT_LOST_WOODS_FROM_LOST_WOODS_SOUTH"),
                        ME("Entrance.png", 996, 1996, "Entrance shuffle (Lost Woods West)", "OOT_LOST_WOODS_FROM_LOST_WOODS_WEST"),
                        
                        MA("Butterfly.png", 1023, 893, 24, "OOT Lost Woods Butterfly 1", "child", "Lost Woods Butterfly 1"),
                        MA("Butterfly.png", 989, 900, 24, "OOT Lost Woods Butterfly 2", "child", "Lost Woods Butterfly 2"),
                        MA("Butterfly.png", 978, 930, 24, "OOT Lost Woods Butterfly 3", "child", "Lost Woods Butterfly 3"),
                        MA("Butterfly.png", 988, 962, 24, "OOT Lost Woods Butterfly 4", "child", "Lost Woods Butterfly 4"),
                        MA("Butterfly.png", 1024, 966, 24, "OOT Lost Woods Butterfly 5", "child", "Lost Woods Butterfly 5"),
                        M("NPC.png", 194, 2769, 40, "OOT Lost Woods Gift from Saria", "Lost Woods Gift from Saria"),
                        M("Grass.png", 1494, 1631, 24, "OOT Lost Woods Grass 1", "Lost Woods Grass 1"),
                        M("Grass.png", 1525, 1625, 24, "OOT Lost Woods Grass 2", "Lost Woods Grass 2"),
                        M("Grass.png", 1490, 1599, 24, "OOT Lost Woods Grass 3", "Lost Woods Grass 3"),
                        M("Grass.png", 1996, 489, 24, "OOT Lost Woods Grass Deep 1", "Lost Woods Grass Deep 1"),
                        M("Grass.png", 2039, 483, 24, "OOT Lost Woods Grass Deep 2", "Lost Woods Grass Deep 2"),
                        M("Grass.png", 1992, 454, 24, "OOT Lost Woods Grass Deep 3", "Lost Woods Grass Deep 3"),
                        M("Grass.png", 1614, 435, 24, "OOT Lost Woods Grass Deep 4", "Lost Woods Grass Deep 4"),
                        M("Grass.png", 1620, 470, 24, "OOT Lost Woods Grass Deep 5", "Lost Woods Grass Deep 5"),
                        M("Grass.png", 1577, 474, 24, "OOT Lost Woods Grass Deep 6", "Lost Woods Grass Deep 6"),
                        MA("Gold_Skulltula.png", 1509, 741, 40, "OOT Lost Woods GS Bean Ride", "adult", "Lost Woods GS Bean Ride"),
                        MA("Gold_Skulltula.png", 194, 2439, 40, "OOT Lost Woods GS Soil Bridge", "child", "Lost Woods GS Soil Bridge"),
                        MA("Gold_Skulltula.png", 1360, 819, 40, "OOT Lost Woods GS Soil Theater", "child", "Lost Woods GS Soil Theater"),
                        MA("NPC.png", 1882, 2173, 40, "OOT Lost Woods Memory Game", "child", "Lost Woods Memory Game"),
                        MA("NPC.png", 257, 1973, 40, "OOT Lost Woods Odd Mushroom", "adult", "Lost Woods Odd Mushroom"),
                        MA("NPC.png", 281, 1938, 40, "OOT Lost Woods Poacher's Saw", "adult", "Lost Woods Poacher's Saw"),
                        M("Fairy_Spot.png", 2347, 1467, 40, "OOT Lost Woods Pool Big Fairy", "Lost Woods Pool Big Fairy"),
                        MA("Rupee.png", 2543, 1453, 24, "OOT Lost Woods Rupee Arrow 1", "child", "Lost Woods Rupee Arrow 1"),
                        MA("Rupee.png", 2513, 1453, 24, "OOT Lost Woods Rupee Arrow 2", "child", "Lost Woods Rupee Arrow 2"),
                        MA("Rupee.png", 2480, 1453, 24, "OOT Lost Woods Rupee Arrow 3", "child", "Lost Woods Rupee Arrow 3"),
                        MA("Rupee.png", 2446, 1453, 24, "OOT Lost Woods Rupee Arrow 4", "child", "Lost Woods Rupee Arrow 4"),
                        MA("Rupee.png", 2528, 1421, 24, "OOT Lost Woods Rupee Arrow 5", "child", "Lost Woods Rupee Arrow 5"),
                        MA("Rupee.png", 2508, 1393, 24, "OOT Lost Woods Rupee Arrow 6", "child", "Lost Woods Rupee Arrow 6"),
                        MA("Rupee.png", 2528, 1480, 24, "OOT Lost Woods Rupee Arrow 7", "child", "Lost Woods Rupee Arrow 7"),
                        MA("Rupee.png", 2508, 1507, 24, "OOT Lost Woods Rupee Arrow 8", "child", "Lost Woods Rupee Arrow 8"),
                        M("Rupee.png", 2277, 195, 24, "OOT Lost Woods Rupee Boulder", "Lost Woods Rupee Boulder"),
                        MA("Scrub.png", 1260, 1071, 40, "OOT Lost Woods Scrub Near Theater Left", "child", "Lost Woods Scrub Near Theater Left"),
                        MA("Scrub.png", 1406, 842, 40, "OOT Lost Woods Scrub Near Theater Right", "child", "Lost Woods Scrub Near Theater Right"),
                        MA("Scrub.png", 219, 3053, 40, "OOT Lost Woods Scrub Sticks Upgrade", "child", "Lost Woods Scrub Sticks Upgrade"),
                        MA("NPC.png", 246, 1935, 40, "OOT Lost Woods Sell Skull Mask", "child", "Lost Woods Sell Skull Mask"),
                        MA("NPC.png", 363, 2047, 40, "OOT Lost Woods Skull Kid", "child", "Lost Woods Skull Kid"),
                        MA("Soil.png", 203, 2418, 24, "OOT Lost Woods Soil Early 1", "child", "Lost Woods Soil Early 1"),
                        MA("Soil.png", 232, 2430, 24, "OOT Lost Woods Soil Early 2", "child", "Lost Woods Soil Early 2"),
                        MA("Soil.png", 172, 2429, 24, "OOT Lost Woods Soil Early 3", "child", "Lost Woods Soil Early 3"),
                        MA("Soil.png", 1380, 795, 24, "OOT Lost Woods Soil Late 1", "child", "Lost Woods Soil Late 1"),
                        MA("Soil.png", 1401, 811, 24, "OOT Lost Woods Soil Late 2", "child", "Lost Woods Soil Late 2"),
                        MA("Soil.png", 1355, 797, 24, "OOT Lost Woods Soil Late 3", "child", "Lost Woods Soil Late 3"),
                        MA("NPC.png", 2053, 1943, 40, "OOT Lost Woods Target", "child", "Lost Woods Target"),
                        MA("Wonder.png", 1962, 2087, 24, "OOT Lost Woods Wonder Item 1", "child", "Lost Woods Wonder Item 1"),
                        MA("Wonder.png", 1841, 2302, 24, "OOT Lost Woods Wonder Item 2", "child", "Lost Woods Wonder Item 2"),
                        MA("Wonder.png", 2029, 2273, 24, "OOT Lost Woods Wonder Item 3", "child", "Lost Woods Wonder Item 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Deku Scrubs Grotto",
                    BackgroundImage = OoT("Lost_Woods", "Scrub_Upgrade"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_SCRUB_UPGRADE" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 655, 644, "Entrance shuffle (Lost Woods)", "OOT_GROTTO_EXIT_SCRUB_UPGRADE"),
                        
                        M("Fairy_Spot.png", 653, 223, 40, "OOT Lost Woods Grotto Scrub Big Fairy", "Lost Woods Grotto Scrub Big Fairy"),
                        M("Hive.png", 750, 392, 40, "OOT Lost Woods Grotto Scrub Hive", "Lost Woods Grotto Scrub Hive"),
                        M("Scrub.png", 697, 232, 40, "OOT Lost Woods Grotto Scrub Back", "Lost Woods Grotto Scrub Back"),
                        M("Scrub.png", 620, 312, 40, "OOT Lost Woods Grotto Scrub Nuts Upgrade", "Lost Woods Grotto Scrub Nuts Upgrade")
                    }
                },
                new MapSubRegion
                {
                    Name = "Deku Theater",
                    BackgroundImage = OoT("Lost_Woods", "Theater"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_DEKU_THEATER" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 690, 684, "Entrance shuffle (Lost Woods)", "OOT_GROTTO_EXIT_DEKU_THEATER"),
                        
                        M("NPC.png", 661, 539, 40, "OOT Deku Theater Nuts Upgrade", "Deku Theater Nuts Upgrade"),
                        M("NPC.png", 711, 539, 40, "OOT Deku Theater Sticks Upgrade", "Deku Theater Sticks Upgrade")
                    }
                },
                new MapSubRegion
                {
                    Name = "Generic Grotto",
                    BackgroundImage = OoT("Lost_Woods", "Generic"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_GENERIC_LOST_WOODS" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 651, 571, "Entrance shuffle (Lost Woods)", "OOT_GROTTO_EXIT_GENERIC_LOST_WOODS"),
                        
                        M("Butterfly.png", 699, 613, 24, "OOT Lost Woods Grotto Generic Butterfly 1", "Lost Woods Grotto Generic Butterfly 1"),
                        M("Butterfly.png", 615, 613, 24, "OOT Lost Woods Grotto Generic Butterfly 2", "Lost Woods Grotto Generic Butterfly 2"),
                        M("Butterfly.png", 657, 626, 24, "OOT Lost Woods Grotto Generic Butterfly 3", "Lost Woods Grotto Generic Butterfly 3"),
                        M("Chest.png", 669, 162, 40, "OOT Lost Woods Grotto Generic", "Lost Woods Grotto Generic"),
                        M("Grass.png", 658, 134, 24, "OOT Lost Woods Grotto Generic Grass 1", "Lost Woods Grotto Generic Grass 1"),
                        M("Grass.png", 643, 186, 24, "OOT Lost Woods Grotto Generic Grass 2", "Lost Woods Grotto Generic Grass 2"),
                        M("Grass.png", 716, 186, 24, "OOT Lost Woods Grotto Generic Grass 3", "Lost Woods Grotto Generic Grass 3"),
                        M("Grass.png", 648, 316, 24, "OOT Lost Woods Grotto Generic Grass 4", "Lost Woods Grotto Generic Grass 4"),
                        M("Hive.png", 530, 141, 40, "OOT Lost Woods Grotto Generic Hive 1", "Lost Woods Grotto Generic Hive 1"),
                        M("Hive.png", 717, 79, 40, "OOT Lost Woods Grotto Generic Hive 2", "Lost Woods Grotto Generic Hive 2")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion Market()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Market";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Market - Day",
                    BackgroundImage = OoT("Market", "Market_Day"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_MARKET_FROM_MASK_SHOP",
						"OOT_MARKET_FROM_BOWLING",						
						"OOT_MARKET_FROM_ARCHERY",
						"OOT_MARKET_FROM_BAZAAR",
						"OOT_MARKET_FROM_POTION",
						"OOT_MARKET_FROM_CASTLE",
						"OOT_MARKET_FROM_MARKET_ENTRANCE",
						"OOT_MARKET_FROM_TEMPLE_OF_TIME_ENTRYWAY"
					},
                    Marks = new List<MapMark>
                    {
                        M("Wonder.png", 178, 123, 24, "OOT Market Wonder Item Day 1", "Market Wonder Item Day 1"),
                        M("Wonder.png", 178, 202, 24, "OOT Market Wonder Item Day 2", "Market Wonder Item Day 2"),
                        M("Wonder.png", 178, 291, 24, "OOT Market Wonder Item Day 3", "Market Wonder Item Day 3"),
                        M("Wonder.png", 178, 379, 24, "OOT Market Wonder Item Day 4", "Market Wonder Item Day 4"),
                        M("Wonder.png", 178, 467, 24, "OOT Market Wonder Item Day 5", "Market Wonder Item Day 5"),
                        M("Crate.png", 424, 30, 24, "OOT Market Crate 1", "Market Crate 1"),
                        M("Crate.png", 400, 30, 24, "OOT Market Crate 2", "Market Crate 2"),
                        M("Crate.png", 712, 358, 24, "OOT Market Crate 3", "Market Crate 3"),
                        M("Crate.png", 715, 458, 24, "OOT Market Crate 4", "Market Crate 4"),
                        M("Grass.png", 507, 198, 24, "OOT Market Grass 1", "Market Grass 1"),
                        M("Grass.png", 507, 141, 24, "OOT Market Grass 2", "Market Grass 2"),
                        M("Grass.png", 507, 85, 24, "OOT Market Grass 3", "Market Grass 3"),
                        M("Grass.png", 663, 42, 24, "OOT Market Grass 4", "Market Grass 4"),
                        M("Grass.png", 683, 42, 24, "OOT Market Grass 5", "Market Grass 5"),
                        M("Grass.png", 703, 42, 24, "OOT Market Grass 6", "Market Grass 6"),
                        M("Grass.png", 402, 412, 24, "OOT Market Grass 7", "Market Grass 7"),
                        M("Grass.png", 420, 433, 24, "OOT Market Grass 8", "Market Grass 8"),
                        M("Tree.png", 423, 411, 24, "OOT Market Tree", "Market Tree"),
						
						ME("Entrance.png", 614, 16, "Entrance shuffle (Mask Shop)", "OOT_SHOP_MASKS"),
						ME("Entrance.png", 726, 225, "Entrance shuffle (Potion Shop)", "OOT_MARKET_POTION"),
						ME("Entrance.png", 726, 400, "Entrance shuffle (Bazaar)", "OOT_MARKET_BAZAAR"),
						ME("Entrance.png", 347, 2, "Entrance shuffle (Shooting Gallery)", "OOT_CHILD_ARCHERY"),
						ME("Entrance.png", 230, 286, "Entrance shuffle (Bombchu Bowling)", "OOT_BOMBCHU_BOWLING"),
						ME("Entrance.png", 461, 4, "Entrance shuffle (Hyrule Castle)", "OOT_HYRULE_CASTLE"),
						ME("Entrance.png", 463, 572, "Entrance shuffle (Market Gate Side)", "OOT_MARKET_ENTRANCE_FROM_MARKET"),
						ME("Entrance.png", 882, 62, "Entrance shuffle (Market Temple Side)", "OOT_TEMPLE_OF_TIME_ENTRYWAY_FROM_MARKET")
                    }
                },
                new MapSubRegion
                {
                    Name = "Market - Night",
                    BackgroundImage = OoT("Market", "Market_Night"),
					DestinationEntranceIds = new List<string>
					{
						"OOT_MARKET_FROM_TREASURE_GAME",
						"OOT_MARKET_FROM_BOMBCHU_SHOP",
						"OOT_MARKET_FROM_ALLEY_HOUSE"
					},
                    Marks = new List<MapMark>
                    {
                        M("Wonder.png", 197, 170, 24, "OOT Market Wonder Item Night 1", "Market Wonder Item Night 1"),
                        M("Wonder.png", 197, 296, 24, "OOT Market Wonder Item Night 2", "Market Wonder Item Night 2"),
						
						ME("Entrance.png", 211, 581, "Entrance shuffle (Treasure Chest Game)", "OOT_TREASURE_GAME"),
						ME("Entrance.png", 461, 4, "Entrance shuffle (Hyrule Castle)", "OOT_HYRULE_CASTLE"),
						ME("Entrance.png", 230, 286, "Entrance shuffle (Bombchu Bowling)", "OOT_BOMBCHU_BOWLING"),
						ME("Entrance.png", 81, 574, "Entrance shuffle (Bombchu Shop)", "OOT_BOMBCHU_SHOP"),
						ME("Entrance.png", 81, 15, "Entrance shuffle (Back Alley House)", "OOT_ALLEY_HOUSE"),
						ME("Entrance.png", 483, 579, "Entrance shuffle (Market Gate Side)", "OOT_MARKET_ENTRANCE_FROM_MARKET"),
						ME("Entrance.png", 882, 62, "Entrance shuffle (Market Temple Side)", "OOT_TEMPLE_OF_TIME_ENTRYWAY_FROM_MARKET")

                    }
                },
				new MapSubRegion
                {
                    Name = "Market Gate Side",
                    BackgroundImage = OoT("Market", "Gate"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_MARKET_ENTRANCE_FROM_FIELD",
						"OOT_MARKET_ENTRANCE_FROM_POTS",
						"OOT_MARKET_ENTRANCE_FROM_MARKET"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 675, 134, "Entrance shuffle (Hyrule Field)", "OOT_FIELD_FROM_MARKET_ENTRANCE"),
                        ME("Entrance.png", 470, 171, "Entrance shuffle (Pot House)", "OOT_HOUSE_POTS"),
                        ME("Entrance.png", 224, 429, "Entrance shuffle (Market)", "OOT_MARKET_FROM_MARKET_ENTRANCE")
                    }
                },
				new MapSubRegion
                {
                    Name = "Market Temple Side",
                    BackgroundImage = OoT("Market", "Temple_Outside"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_TEMPLE_OF_TIME_ENTRYWAY_FROM_MARKET",
						"OOT_TEMPLE_OF_TIME_ENTRYWAY_FROM_TEMPLE"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 692, 510, "Entrance shuffle (Market)", "OOT_MARKET_FROM_TEMPLE_OF_TIME_ENTRYWAY"),
                        ME("Entrance.png", 421, 326, "Entrance shuffle (Temple of Time)", "OOT_TEMPLE_OF_TIME")
                    }
                },
                new MapSubRegion
                {
                    Name = "Temple of Time",
                    BackgroundImage = OoT("Market", "Temple_of_Time"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_WARP_SONG_TEMPLE",
						"OOT_TEMPLE_OF_TIME",
						"OOT_SPAWN_ADULT"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 90, 309, "Entrance shuffle (Market Temple Side)", "OOT_TEMPLE_OF_TIME_ENTRYWAY_FROM_TEMPLE"),
                        
                        M("NPC.png", 139, 309, 40, "OOT Temple of Time Light Arrows", "Temple of Time Light Arrows"),
                        M("NPC.png", 570, 309, 40, "OOT Temple of Time Master Sword", "Temple of Time Master Sword"),
                        M("NPC.png", 530, 309, 40, "OOT Temple of Time Medallion", "Temple of Time Medallion"),
                        M("NPC.png", 489, 309, 40, "OOT Temple of Time Sheik Song", "Temple of Time Sheik Song")
                    }
                },
                new MapSubRegion
                {
                    Name = "Bazaar",
                    BackgroundImage = OoT("Market", "Bazaar"),
                    DestinationEntranceIds = new List<string> { "OOT_MARKET_BAZAAR" },
                    Marks = new List<MapMark>
                    {
						ME("Entrance.png", 543, 556, "Entrance shuffle (Market)", "OOT_MARKET_FROM_BAZAAR"),

                        M("Shop.png", 358, 325, 40, "OOT Market Bazaar Item 1", "Market Bazaar Item 1"),
                        M("Shop.png", 306, 273, 40, "OOT Market Bazaar Item 2", "Market Bazaar Item 2"),
                        M("Shop.png", 358, 273, 40, "OOT Market Bazaar Item 3", "Market Bazaar Item 3"),
                        M("Shop.png", 306, 325, 40, "OOT Market Bazaar Item 4", "Market Bazaar Item 4"),
                        M("Shop.png", 585, 273, 40, "OOT Market Bazaar Item 5", "Market Bazaar Item 5"),
                        M("Shop.png", 637, 273, 40, "OOT Market Bazaar Item 6", "Market Bazaar Item 6"),
                        M("Shop.png", 637, 325, 40, "OOT Market Bazaar Item 7", "Market Bazaar Item 7"),
                        M("Shop.png", 585, 325, 40, "OOT Market Bazaar Item 8", "Market Bazaar Item 8")
                    }
                },
                new MapSubRegion
                {
                    Name = "Potion Shop",
                    BackgroundImage = OoT("Market", "Potion_Shop"),
                    DestinationEntranceIds = new List<string> { "OOT_MARKET_POTION" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 486, 555, "Entrance shuffle (Market)", "OOT_MARKET_FROM_POTION"),
                        
                        M("Shop.png", 311, 278, 40, "OOT Market Potion Shop Item 1", "Market Potion Shop Item 1"),
                        M("Shop.png", 368, 278, 40, "OOT Market Potion Shop Item 2", "Market Potion Shop Item 2"),
                        M("Shop.png", 311, 335, 40, "OOT Market Potion Shop Item 3", "Market Potion Shop Item 3"),
                        M("Shop.png", 368, 335, 40, "OOT Market Potion Shop Item 4", "Market Potion Shop Item 4"),
                        M("Shop.png", 463, 278, 40, "OOT Market Potion Shop Item 5", "Market Potion Shop Item 5"),
                        M("Shop.png", 520, 278, 40, "OOT Market Potion Shop Item 6", "Market Potion Shop Item 6"),
                        M("Shop.png", 463, 335, 40, "OOT Market Potion Shop Item 7", "Market Potion Shop Item 7"),
                        M("Shop.png", 520, 335, 40, "OOT Market Potion Shop Item 8", "Market Potion Shop Item 8")
                    }
                },
                new MapSubRegion
                {
                    Name = "Bombchu Shop",
                    BackgroundImage = OoT("Market", "Bombchu_Shop"),
                    DestinationEntranceIds = new List<string> { "OOT_BOMBCHU_SHOP" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 480, 532, "Entrance shuffle (Market)", "OOT_MARKET_FROM_BOMBCHU_SHOP"),
                        
                        M("Shop.png", 384, 220, 40, "OOT Market Bombchu Shop Item 1", "Market Bombchu Shop Item 1"),
                        M("Shop.png", 313, 220, 40, "OOT Market Bombchu Shop Item 2", "Market Bombchu Shop Item 2"),
                        M("Shop.png", 384, 291, 40, "OOT Market Bombchu Shop Item 3", "Market Bombchu Shop Item 3"),
                        M("Shop.png", 313, 291, 40, "OOT Market Bombchu Shop Item 4", "Market Bombchu Shop Item 4"),
                        M("Shop.png", 608, 220, 40, "OOT Market Bombchu Shop Item 5", "Market Bombchu Shop Item 5"),
                        M("Shop.png", 537, 291, 40, "OOT Market Bombchu Shop Item 6", "Market Bombchu Shop Item 6"),
                        M("Shop.png", 608, 291, 40, "OOT Market Bombchu Shop Item 7", "Market Bombchu Shop Item 7"),
                        M("Shop.png", 537, 220, 40, "OOT Market Bombchu Shop Item 8", "Market Bombchu Shop Item 8")
                    }
                },
                new MapSubRegion
                {
                    Name = "Bombchu Bowling",
                    BackgroundImage = OoT("Market", "Bombchu_Bowling"),
                    DestinationEntranceIds = new List<string> { "OOT_BOMBCHU_BOWLING" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 838, 323, "Entrance shuffle (Market)", "OOT_MARKET_FROM_BOWLING"),
                        
                        M("NPC.png", 797, 230, 40, "OOT Bombchu Bowling Reward 1", "Bombchu Bowling Reward 1"),
                        M("NPC.png", 731, 230, 40, "OOT Bombchu Bowling Reward 2", "Bombchu Bowling Reward 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Treasure Game",
                    BackgroundImage = OoT("Market", "Treasure_Game"),
                    DestinationEntranceIds = new List<string> { "OOT_TREASURE_GAME" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 887, 74, "Entrance shuffle (Market)", "OOT_MARKET_FROM_TREASURE_GAME"),
                        
                        M("NPC.png", 854, 39, 40, "OOT Treasure Chest Game Buy Key", "Treasure Chest Game Buy Key"),
                        M("Chest.png", 66, 54, 40, "OOT Treasure Chest Game HP", "Treasure Chest Game HP"),
                        M("Chest.png", 735, 93, 40, "OOT Treasure Chest Game Room 1 Chest Left", "Treasure Chest Game Room 1 Chest Left"),
                        M("Chest.png", 731, 23, 40, "OOT Treasure Chest Game Room 1 Chest Right", "Treasure Chest Game Room 1 Chest Right"),
                        M("Chest.png", 604, 91, 40, "OOT Treasure Chest Game Room 2 Chest Left", "Treasure Chest Game Room 2 Chest Left"),
                        M("Chest.png", 603, 22, 40, "OOT Treasure Chest Game Room 2 Chest Right", "Treasure Chest Game Room 2 Chest Right"),
                        M("Chest.png", 464, 91, 40, "OOT Treasure Chest Game Room 3 Chest Left", "Treasure Chest Game Room 3 Chest Left"),
                        M("Chest.png", 464, 22, 40, "OOT Treasure Chest Game Room 3 Chest Right", "Treasure Chest Game Room 3 Chest Right"),
                        M("Chest.png", 344, 90, 40, "OOT Treasure Chest Game Room 4 Chest Left", "Treasure Chest Game Room 4 Chest Left"),
                        M("Chest.png", 346, 21, 40, "OOT Treasure Chest Game Room 4 Chest Right", "Treasure Chest Game Room 4 Chest Right"),
                        M("Chest.png", 213, 89, 40, "OOT Treasure Chest Game Room 5 Chest Left", "Treasure Chest Game Room 5 Chest Left"),
                        M("Chest.png", 216, 20, 40, "OOT Treasure Chest Game Room 5 Chest Right", "Treasure Chest Game Room 5 Chest Right")
                    }
                },
                new MapSubRegion
                {
                    Name = "Shooting Gallery",
                    BackgroundImage = OoT("Market", "Shooting"),
                    DestinationEntranceIds = new List<string> { "OOT_CHILD_ARCHERY" },
                    Marks = new List<MapMark> 
                    {
                        ME("Entrance.png", 790, 329, "Entrance shuffle (Market)", "OOT_MARKET_FROM_ARCHERY"),
                        M("NPC.png", 606, 337, 40, "OOT Shooting Gallery Child", "Shooting Gallery Child")
                    }
                },
                new MapSubRegion
                {
                    Name = "Dog Lady House",
                    BackgroundImage = OoT("Market", "Dog_Lady_House"),
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 428, 205, 40, "OOT Market Dog Lady HP", "Market Dog Lady HP"),
                        M("Crate.png", 658, 86, 24, "OOT Market Dog Lady Crate", "Market Dog Lady Crate")
                    }
                },
                new MapSubRegion
                {
                    Name = "Back Alley House",
                    BackgroundImage = OoT("Market", "Back_Alley_House"),
                    DestinationEntranceIds = new List<string> { "OOT_ALLEY_HOUSE" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 243, 538, "Entrance shuffle (Market)", "OOT_MARKET_FROM_ALLEY_HOUSE"),
                        
                        M("Pot.png", 756, 465, 24, "OOT Market Back Alley East House Pot 1", "Market Back Alley East House Pot 1"),
                        M("Pot.png", 432, 426, 24, "OOT Market Back Alley East House Pot 2", "Market Back Alley East House Pot 2"),
                        M("Pot.png", 520, 426, 24, "OOT Market Back Alley East House Pot 3", "Market Back Alley East House Pot 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Pot House",
                    BackgroundImage = OoT("Market", "Pot_House"),
                    DestinationEntranceIds = new List<string> { "OOT_HOUSE_POTS" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 856, 409, "Entrance shuffle (Market Gate Side)", "OOT_MARKET_ENTRANCE_FROM_POTS"),
                        
                        MA("NPC.png", 154, 491, 40, "OOT Market Pot House Big Poes", "adult", "Market Pot House Big Poes"),
                        MA("Gold_Skulltula.png", 172, 410, 40, "OOT Market Pot House GS", "child", "Market Pot House GS"),
                        MA("Crate.png", 860, 576, 24, "OOT Market Pot House Crate 1", "child", "Market Pot House Crate 1"),
                        MA("Crate.png", 790, 576, 24, "OOT Market Pot House Crate 2", "child", "Market Pot House Crate 2"),
                        MA("Crate.png", 720, 576, 24, "OOT Market Pot House Crate 3", "child", "Market Pot House Crate 3"),
                        MA("Crate.png", 154, 450, 24, "OOT Market Pot House Crate 4", "child", "Market Pot House Crate 4"),
                        MA("Pot.png", 241, 412, 24, "OOT Market Pot House Adult Pot 1", "adult", "Market Pot House Adult Pot 1"),
                        MA("Pot.png", 639, 412, 24, "OOT Market Pot House Adult Pot 2", "adult", "Market Pot House Adult Pot 2"),
                        MA("Pot.png", 739, 412, 24, "OOT Market Pot House Adult Pot 3", "adult", "Market Pot House Adult Pot 3"),
                        MA("Pot.png", 217, 322, 24, "OOT Market Pot House Adult Pot 4", "adult", "Market Pot House Adult Pot 4"),
                        MA("Pot.png", 307, 322, 24, "OOT Market Pot House Adult Pot 5", "adult", "Market Pot House Adult Pot 5"),
                        MA("Pot.png", 701, 322, 24, "OOT Market Pot House Adult Pot 6", "adult", "Market Pot House Adult Pot 6"),
                        MA("Pot.png", 504, 412, 24, "OOT Market Pot House Adult Pot 7", "adult", "Market Pot House Adult Pot 7"),
                        MA("Pot.png", 187, 437, 24, "OOT Market Pot House Adult Pot 8", "adult", "Market Pot House Adult Pot 8"),
                        MA("Pot.png", 290, 437, 24, "OOT Market Pot House Adult Pot 9", "adult", "Market Pot House Adult Pot 9"),
                        MA("Pot.png", 461, 437, 24, "OOT Market Pot House Adult Pot 10", "adult", "Market Pot House Adult Pot 10"),
                        MA("Pot.png", 420, 412, 24, "OOT Market Pot House Adult Pot 11", "adult", "Market Pot House Adult Pot 11"),
                        MA("Pot.png", 365, 218, 24, "OOT Market Pot House Child Pot Above 1", "child", "Market Pot House Child Pot Above 1"),
                        MA("Pot.png", 562, 218, 24, "OOT Market Pot House Child Pot Above 2", "child", "Market Pot House Child Pot Above 2"),
                        MA("Pot.png", 531, 537, 24, "OOT Market Pot House Child Pot Ground 1", "child", "Market Pot House Child Pot Ground 1"),
                        MA("Pot.png", 345, 435, 24, "OOT Market Pot House Child Pot Ground 2", "child", "Market Pot House Child Pot Ground 2"),
                        MA("Pot.png", 572, 420, 24, "OOT Market Pot House Child Pot Ground 3", "child", "Market Pot House Child Pot Ground 3"),
                        MA("Pot.png", 373, 587, 24, "OOT Market Pot House Child Pot Ground 4", "child", "Market Pot House Child Pot Ground 4"),
                        MA("Pot.png", 317, 574, 24, "OOT Market Pot House Child Pot Ground 5", "child", "Market Pot House Child Pot Ground 5"),
                        MA("Pot.png", 624, 322, 24, "OOT Market Pot House Child Pot Ground 6", "child", "Market Pot House Child Pot Ground 6"),
                        MA("Pot.png", 717, 322, 24, "OOT Market Pot House Child Pot Ground 7", "child", "Market Pot House Child Pot Ground 7"),
                        MA("Pot.png", 479, 587, 24, "OOT Market Pot House Child Pot Ground 8", "child", "Market Pot House Child Pot Ground 8"),
                        MA("Pot.png", 410, 330, 24, "OOT Market Pot House Child Pot Ground 9", "child", "Market Pot House Child Pot Ground 9"),
                        MA("Pot.png", 520, 330, 24, "OOT Market Pot House Child Pot Ground 10", "child", "Market Pot House Child Pot Ground 10"),
                        MA("Pot.png", 478, 516, 24, "OOT Market Pot House Child Pot Ground 11", "child", "Market Pot House Child Pot Ground 11"),
                        MA("Pot.png", 211, 322, 24, "OOT Market Pot House Child Pot Ground 12", "child", "Market Pot House Child Pot Ground 12"),
                        MA("Pot.png", 306, 322, 24, "OOT Market Pot House Child Pot Ground 13", "child", "Market Pot House Child Pot Ground 13"),
                        MA("Pot.png", 463, 460, 24, "OOT Market Pot House Child Pot Ground 14", "child", "Market Pot House Child Pot Ground 14"),
                        MA("Pot.png", 526, 485, 24, "OOT Market Pot House Child Pot Ground 15", "child", "Market Pot House Child Pot Ground 15"),
                        MA("Pot.png", 395, 460, 24, "OOT Market Pot House Child Pot Ground 16", "child", "Market Pot House Child Pot Ground 16"),
                        MA("Pot.png", 333, 485, 24, "OOT Market Pot House Child Pot Ground 17", "child", "Market Pot House Child Pot Ground 17"),
                        MA("Pot.png", 294, 460, 24, "OOT Market Pot House Child Pot Ground 18", "child", "Market Pot House Child Pot Ground 18"),
                        MA("Pot.png", 531, 574, 24, "OOT Market Pot House Child Pot Ground 19", "child", "Market Pot House Child Pot Ground 19"),
                        MA("Pot.png", 259, 322, 24, "OOT Market Pot House Child Pot Ground 20", "child", "Market Pot House Child Pot Ground 20"),
                        MA("Pot.png", 345, 460, 24, "OOT Market Pot House Child Pot Ground 21", "child", "Market Pot House Child Pot Ground 21"),
                        MA("Pot.png", 317, 537, 24, "OOT Market Pot House Child Pot Ground 22", "child", "Market Pot House Child Pot Ground 22"),
                        MA("Pot.png", 464, 330, 24, "OOT Market Pot House Child Pot Ground 23", "child", "Market Pot House Child Pot Ground 23"),
                        MA("Pot.png", 384, 553, 24, "OOT Market Pot House Child Pot Ground 24", "child", "Market Pot House Child Pot Ground 24"),
                        MA("Pot.png", 671, 322, 24, "OOT Market Pot House Child Pot Ground 25", "child", "Market Pot House Child Pot Ground 25"),
                        MA("Pot.png", 756, 421, 24, "OOT Market Pot House Child Pot Ground 26", "child", "Market Pot House Child Pot Ground 26"),
                        MA("Pot.png", 655, 444, 24, "OOT Market Pot House Child Pot Ground 27", "child", "Market Pot House Child Pot Ground 27"),
                        MA("Pot.png", 395, 435, 24, "OOT Market Pot House Child Pot Ground 28", "child", "Market Pot House Child Pot Ground 28"),
                        MA("Pot.png", 682, 420, 24, "OOT Market Pot House Child Pot Ground 29", "child", "Market Pot House Child Pot Ground 29"),
                        MA("Pot.png", 572, 468, 24, "OOT Market Pot House Child Pot Ground 30", "child", "Market Pot House Child Pot Ground 30"),
                        MA("Pot.png", 536, 435, 24, "OOT Market Pot House Child Pot Ground 31", "child", "Market Pot House Child Pot Ground 31"),
                        MA("Pot.png", 436, 435, 24, "OOT Market Pot House Child Pot Ground 32", "child", "Market Pot House Child Pot Ground 32"),
                        MA("Pot.png", 628, 420, 24, "OOT Market Pot House Child Pot Ground 33", "child", "Market Pot House Child Pot Ground 33"),
                        MA("Pot.png", 600, 444, 24, "OOT Market Pot House Child Pot Ground 34", "child", "Market Pot House Child Pot Ground 34"),
                        MA("Pot.png", 463, 414, 24, "OOT Market Pot House Child Pot Ground 35", "child", "Market Pot House Child Pot Ground 35"),
                        MA("Pot.png", 512, 460, 24, "OOT Market Pot House Child Pot Ground 36", "child", "Market Pot House Child Pot Ground 36"),
                        MA("Pot.png", 437, 319, 24, "OOT Market Pot House Child Pot Ground 37", "child", "Market Pot House Child Pot Ground 37"),
                        MA("Pot.png", 512, 414, 24, "OOT Market Pot House Child Pot Ground 38", "child", "Market Pot House Child Pot Ground 38"),
                        MA("Pot.png", 492, 319, 24, "OOT Market Pot House Child Pot Ground 39", "child", "Market Pot House Child Pot Ground 39"),
                        MA("Pot.png", 452, 485, 24, "OOT Market Pot House Child Pot Ground 40", "child", "Market Pot House Child Pot Ground 40"),
                        MA("Pot.png", 373, 516, 24, "OOT Market Pot House Child Pot Ground 41", "child", "Market Pot House Child Pot Ground 41"),
                        MA("Pot.png", 469, 553, 24, "OOT Market Pot House Child Pot Ground 42", "child", "Market Pot House Child Pot Ground 42")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion SacredForestMeadow()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Sacred Forest Meadow";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Sacred Forest Meadow",
                    BackgroundImage = OoT("Sacred_Forest_Meadow", "Sacred_Forest_Meadow"),
					DestinationEntranceIds = new List<string>
					{
						"OOT_GROTTO_EXIT_SCRUBS2_SFM",
						"OOT_SACRED_MEADOW_FROM_TEMPLE_FOREST",
						"OOT_WARP_SONG_MEADOW",
						"OOT_GROTTO_EXIT_WOLFOS",
						"OOT_GROTTO_EXIT_FAIRY_SFM",
						"OOT_SACRED_FOREST_MEADOW"
					},
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 1710, 363, "Entrance shuffle (Lost Woods)", "OOT_LOST_WOODS_FROM_MEADOW"),
                        ME("Entrance.png", 109, 282, "Entrance shuffle (Forest Temple)", "OOT_TEMPLE_FOREST"),
                        ME("Entrance.png", 503, 168, "Entrance shuffle (Song of Storms Grotto)", "OOT_GROTTO_SCRUBS2_SFM"),
                        ME("Entrance.png", 1599, 363, "Entrance shuffle (Wolfos Grotto)", "OOT_GROTTO_WOLFOS"),
                        ME("Entrance.png", 988, 274, "Entrance shuffle (Fairy Fountain)", "OOT_GROTTO_FAIRY_SFM"),
                        
                        MA("Gold_Skulltula.png", 1000, 79, 40, "OOT Sacred Meadow GS Night Adult", "adult", "Sacred Meadow GS Night Adult"),
                        MA("NPC.png", 523, 287, 40, "OOT Sacred Meadow Sheik Song", "adult", "Sacred Meadow Sheik Song"),
                        MA("NPC.png", 234, 230, 40, "OOT Saria's Song", "child", "Saria's Song"),
                        M("Wonder.png", 1680, 426, 24, "OOT Sacred Meadow Wonder Item Entrance", "Sacred Meadow Wonder Item Entrance"),
                        M("Wonder.png", 1374, 373, 24, "OOT Sacred Meadow Wonder Item Maze 1", "Sacred Meadow Wonder Item Maze 1"),
                        M("Wonder.png", 1413, 264, 24, "OOT Sacred Meadow Wonder Item Maze 2", "Sacred Meadow Wonder Item Maze 2"),
                        M("Wonder.png", 1016, 191, 24, "OOT Sacred Meadow Wonder Item Maze 3", "Sacred Meadow Wonder Item Maze 3"),
                        M("Wonder.png", 972, 373, 24, "OOT Sacred Meadow Wonder Item Maze 4", "Sacred Meadow Wonder Item Maze 4"),
                        M("Wonder.png", 1302, 264, 24, "OOT Sacred Meadow Wonder Item Maze 5", "Sacred Meadow Wonder Item Maze 5")
                    }
                },
                new MapSubRegion
                {
                    Name = "Song of Storms Grotto",
                    BackgroundImage = OoT("Sacred_Forest_Meadow", "Storms"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_SCRUBS2_SFM" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 655, 630, "Entrance shuffle (Sacred Forest Meadow)", "OOT_GROTTO_EXIT_SCRUBS2_SFM"),
                        
                        M("Hive.png", 529, 134, 40, "OOT Sacred Meadow Storms Grotto Hive", "Sacred Meadow Storms Grotto Hive"),
                        M("Scrub.png", 698, 282, 40, "OOT Sacred Meadow Storms Grotto Front Scrub", "Sacred Meadow Storms Grotto Front Scrub"),
                        M("Scrub.png", 632, 188, 40, "OOT Sacred Meadow Storms Grotto Back Scrub", "Sacred Meadow Storms Grotto Back Scrub")
                    }
                },
                new MapSubRegion
                {
                    Name = "Wolfos Grotto",
                    BackgroundImage = OoT("Sacred_Forest_Meadow", "Wolfos"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_WOLFOS" },
                    Marks = new List<MapMark> 
                    {
                        ME("Entrance.png", 670, 641, "Entrance shuffle (Sacred Forest Meadow)", "OOT_GROTTO_EXIT_WOLFOS"),
                        M("Chest.png", 661, 287, 40, "OOT Sacred Meadow Grotto", "Sacred Meadow Grotto")
                    }
                },
                new MapSubRegion
                {
                    Name = "Fairy Fountain",
                    BackgroundImage = OoT("Fairy", "Fountain"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_FAIRY_SFM" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 465, 502, "Entrance shuffle (Sacred Forest Meadow)", "OOT_GROTTO_EXIT_FAIRY_SFM"),
                        
                        M("Fairy.png", 470, 204, 24, "OOT Sacred Meadow Fairy Fountain Fairy 1", "Sacred Meadow Fairy Fountain Fairy 1"),
                        M("Fairy.png", 490, 200, 24, "OOT Sacred Meadow Fairy Fountain Fairy 2", "Sacred Meadow Fairy Fountain Fairy 2"),
                        M("Fairy.png", 450, 194, 24, "OOT Sacred Meadow Fairy Fountain Fairy 3", "Sacred Meadow Fairy Fountain Fairy 3"),
                        M("Fairy.png", 471, 181, 24, "OOT Sacred Meadow Fairy Fountain Fairy 4", "Sacred Meadow Fairy Fountain Fairy 4"),
                        M("Fairy.png", 493, 178, 24, "OOT Sacred Meadow Fairy Fountain Fairy 5", "Sacred Meadow Fairy Fountain Fairy 5"),
                        M("Fairy.png", 449, 172, 24, "OOT Sacred Meadow Fairy Fountain Fairy 6", "Sacred Meadow Fairy Fountain Fairy 6"),
                        M("Fairy.png", 485, 155, 24, "OOT Sacred Meadow Fairy Fountain Fairy 7", "Sacred Meadow Fairy Fountain Fairy 7"),
                        M("Fairy.png", 464, 153, 24, "OOT Sacred Meadow Fairy Fountain Fairy 8", "Sacred Meadow Fairy Fountain Fairy 8")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion ZoraDomain()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Zora Domain";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Zora Domain",
                    BackgroundImage = OoT("Zora_Domain", "Zora_Domain"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_ZORA_DOMAIN_FROM_LAKE_HYLIA",
						"OOT_ZORA_DOMAIN_FROM_SHOP",
						"OOT_GROTTO_EXIT_FAIRY_DOMAIN",
						"OOT_ZORA_DOMAIN",
						"OOT_DOMAIN_FROM_FOUNTAIN"
					},
                    Marks = new List<MapMark>
                    {
                        M("Pot.png", 554, 517, 24, "OOT Zora Domain Pot 1", "Zora Domain Pot 1"),
                        M("Pot.png", 510, 528, 24, "OOT Zora Domain Pot 2", "Zora Domain Pot 2"),
                        M("Pot.png", 571, 496, 24, "OOT Zora Domain Pot 3", "Zora Domain Pot 3"),
                        M("Pot.png", 502, 496, 24, "OOT Zora Domain Pot 4", "Zora Domain Pot 4"),
                        M("Pot.png", 489, 517, 24, "OOT Zora Domain Pot 5", "Zora Domain Pot 5"),
                        M("Rock.png", 493, 365, 24, "OOT Zora Domain Rock Circle Rock 1", "Zora Domain Rock Circle Rock 1"),
                        M("Rock.png", 517, 357, 24, "OOT Zora Domain Rock Circle Rock 2", "Zora Domain Rock Circle Rock 2"),
                        M("Rock.png", 531, 339, 24, "OOT Zora Domain Rock Circle Rock 3", "Zora Domain Rock Circle Rock 3"),
                        M("Rock.png", 520, 317, 24, "OOT Zora Domain Rock Circle Rock 4", "Zora Domain Rock Circle Rock 4"),
                        M("Rock.png", 500, 302, 24, "OOT Zora Domain Rock Circle Rock 5", "Zora Domain Rock Circle Rock 5"),
                        M("Rock.png", 478, 312, 24, "OOT Zora Domain Rock Circle Rock 6", "Zora Domain Rock Circle Rock 6"),
                        M("Rock.png", 462, 330, 24, "OOT Zora Domain Rock Circle Rock 7", "Zora Domain Rock Circle Rock 7"),
                        M("Rock.png", 471, 353, 24, "OOT Zora Domain Rock Circle Rock 8", "Zora Domain Rock Circle Rock 8"),
                        MA("Gold_Skulltula.png", 442, 276, 40, "OOT Zora Domain GS Waterfall", "adult", "Zora Domain GS Waterfall"),
                        MA("NPC.png", 537, 102, 40, "OOT Zora Domain Eyeball Frog", "adult", "Zora Domain Eyeball Frog"),
                        MA("NPC.png", 561, 162, 40, "OOT Zora Domain Tunic", "adult", "Zora Domain Tunic"),
                        MA("Hive.png", 582, 37, 40, "OOT Zora Domain Hive Back", "child", "Zora Domain Hive Back"),
                        MA("Hive.png", 513, 189, 40, "OOT Zora Domain Hive Front 1", "child", "Zora Domain Hive Front 1"),
                        MA("Hive.png", 631, 149, 40, "OOT Zora Domain Hive Front 2", "child", "Zora Domain Hive Front 2"),
                        MA("Chest.png", 399, 280, 40, "OOT Zora Domain Waterfall Chest", "child", "Zora Domain Waterfall Chest"),
                        MA("NPC.png", 411, 244, 40, "OOT Zora Domain Diving Game", "child", "Zora Domain Diving Game"),
                        MA("Rupee.png", 365, 366, 24, "OOT Zora Domain Diving Game Green Rupee", "child", "Zora Domain Diving Game Green Rupee"),
                        MA("Rupee.png", 412, 368, 24, "OOT Zora Domain Diving Game Blue Rupee", "child", "Zora Domain Diving Game Blue Rupee"),
                        MA("Rupee.png", 388, 347, 24, "OOT Zora Domain Diving Game Red Rupee", "child", "Zora Domain Diving Game Red Rupee"),
                        MA("Rupee.png", 415, 326, 24, "OOT Zora Domain Diving Game Purple Rupee", "child", "Zora Domain Diving Game Purple Rupee"),
                        MA("Rupee.png", 353, 329, 24, "OOT Zora Domain Diving Game Huge Rupee", "child", "Zora Domain Diving Game Huge Rupee"),
                        
                        ME("Entrance.png", 194, 439, "Entrance shuffle (Zora River)", "OOT_RIVER_FROM_DOMAIN"),
                        ME("Entrance.png", 543, 17, "Entrance shuffle (Zora Fountain)", "OOT_FOUNTAIN_ZORA"),
                        ME("Entrance.png", 413, 406, "Entrance shuffle (Lake Hylia)", "OOT_LAKE_HYLIA_FROM_ZORA_DOMAIN"),
                        ME("Entrance.png", 513, 467, "Entrance shuffle (Zora Shop)", "OOT_SHOP_ZORA"),
                        ME("Entrance.png", 315, 360, "Entrance shuffle (Fairy Grotto)", "OOT_GROTTO_FAIRY_DOMAIN")
                    }
                },
                new MapSubRegion
                {
                    Name = "Shop",
                    BackgroundImage = OoT("Zora_Domain", "Shop"),
                    DestinationEntranceIds = new List<string> { "OOT_SHOP_ZORA" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 483, 557, "Entrance shuffle (Zora Domain)", "OOT_ZORA_DOMAIN_FROM_SHOP"),
                        
                        M("Shop.png", 314, 280, 40, "OOT Zora Shop Item 1", "Zora Shop Item 1"),
                        M("Shop.png", 392, 280, 40, "OOT Zora Shop Item 2", "Zora Shop Item 2"),
                        M("Shop.png", 314, 325, 40, "OOT Zora Shop Item 3", "Zora Shop Item 3"),
                        M("Shop.png", 392, 325, 40, "OOT Zora Shop Item 4", "Zora Shop Item 4"),
                        M("Shop.png", 576, 280, 40, "OOT Zora Shop Item 5", "Zora Shop Item 5"),
                        M("Shop.png", 654, 280, 40, "OOT Zora Shop Item 6", "Zora Shop Item 6"),
                        M("Shop.png", 576, 325, 40, "OOT Zora Shop Item 7", "Zora Shop Item 7"),
                        M("Shop.png", 654, 325, 40, "OOT Zora Shop Item 8", "Zora Shop Item 8")
                    }
                },
                new MapSubRegion
                {
                    Name = "Fairy Fountain",
                    BackgroundImage = OoT("Fairy", "Fountain"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_FAIRY_DOMAIN" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 465, 502, "Entrance shuffle (Zora Domain)", "OOT_GROTTO_EXIT_FAIRY_DOMAIN"),
                        
                        M("Fairy.png", 470, 204, 24, "OOT Zora Domain Fairy Fountain Fairy 1", "Zora Domain Fairy Fountain Fairy 1"),
                        M("Fairy.png", 490, 200, 24, "OOT Zora Domain Fairy Fountain Fairy 2", "Zora Domain Fairy Fountain Fairy 2"),
                        M("Fairy.png", 450, 194, 24, "OOT Zora Domain Fairy Fountain Fairy 3", "Zora Domain Fairy Fountain Fairy 3"),
                        M("Fairy.png", 471, 181, 24, "OOT Zora Domain Fairy Fountain Fairy 4", "Zora Domain Fairy Fountain Fairy 4"),
                        M("Fairy.png", 493, 178, 24, "OOT Zora Domain Fairy Fountain Fairy 5", "Zora Domain Fairy Fountain Fairy 5"),
                        M("Fairy.png", 449, 172, 24, "OOT Zora Domain Fairy Fountain Fairy 6", "Zora Domain Fairy Fountain Fairy 6"),
                        M("Fairy.png", 485, 155, 24, "OOT Zora Domain Fairy Fountain Fairy 7", "Zora Domain Fairy Fountain Fairy 7"),
                        M("Fairy.png", 464, 153, 24, "OOT Zora Domain Fairy Fountain Fairy 8", "Zora Domain Fairy Fountain Fairy 8")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion ZoraFountain()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Zora Fountain";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Zora Fountain",
                    BackgroundImage = OoT("Zora_Fountain", "Zora_Fountain"),
                    DestinationEntranceIds = new List<string>
					{
						"OOT_ZORA_FOUNTAIN_FROM_ICE_CAVERN",
						"OOT_ZORA_FOUNTAIN_FROM_JABU_JABU",
						"OOT_ZORA_FOUNTAIN_FROM_FAIRY",
						"OOT_FOUNTAIN_ZORA"
					},
                    Marks = new List<MapMark>
                    {
                        MA("Pot.png", 439, 79, 24, "OOT Zora Fountain Adult Pot 1", "adult", "Zora Fountain Adult Pot 1"),
                        MA("Pot.png", 455, 66, 24, "OOT Zora Fountain Adult Pot 2", "adult", "Zora Fountain Adult Pot 2"),
                        MA("Pot.png", 436, 56, 24, "OOT Zora Fountain Adult Pot 3", "adult", "Zora Fountain Adult Pot 3"),
                        MA("Pot.png", 673, 399, 24, "OOT Zora Fountain Child Pot 1", "child", "Zora Fountain Child Pot 1"),
                        MA("Pot.png", 651, 399, 24, "OOT Zora Fountain Child Pot 2", "child", "Zora Fountain Child Pot 2"),
                        MA("Pot.png", 651, 362, 24, "OOT Zora Fountain Child Pot 3", "child", "Zora Fountain Child Pot 3"),
                        MA("Pot.png", 673, 362, 24, "OOT Zora Fountain Child Pot 4", "child", "Zora Fountain Child Pot 4"),
                        MA("Gold_Skulltula.png", 312, 90, 40, "OOT Zora Fountain GS Upper", "adult", "Zora Fountain GS Upper"),
                        MA("Gold_Skulltula.png", 425, 100, 40, "OOT Zora Fountain GS Tree", "child", "Zora Fountain GS Tree"),
                        MA("Gold_Skulltula.png", 684, 134, 40, "OOT Zora Fountain GS Wall", "child", "Zora Fountain GS Wall"),
                        MA("Bush.png", 384, 124, 24, "OOT Zora Fountain Bush 1", "child", "Zora Fountain Bush 1"),
                        MA("Bush.png", 405, 116, 24, "OOT Zora Fountain Bush 2", "child", "Zora Fountain Bush 2"),
                        MA("Bush.png", 394, 94, 24, "OOT Zora Fountain Bush 3", "child", "Zora Fountain Bush 3"),
                        MA("Bush.png", 432, 86, 24, "OOT Zora Fountain Bush 4", "child", "Zora Fountain Bush 4"),
                        MA("Bush.png", 414, 74, 24, "OOT Zora Fountain Bush 5", "child", "Zora Fountain Bush 5"),
                        MA("Bush.png", 441, 71, 24, "OOT Zora Fountain Bush 6", "child", "Zora Fountain Bush 6"),
                        MA("Butterfly.png", 620, 188, 24, "OOT Zora Fountain Butterfly 1", "child", "Zora Fountain Butterfly 1"),
                        MA("Butterfly.png", 647, 188, 24, "OOT Zora Fountain Butterfly 2", "child", "Zora Fountain Butterfly 2"),
                        MA("Collectible.png", 454, 386, 40, "OOT Zora Fountain Bottom HP", "adult", "Zora Fountain Bottom HP"),
                        MA("Collectible.png", 329, 365, 40, "OOT Zora Fountain Iceberg HP", "adult", "Zora Fountain Iceberg HP"),
                        MA("Rupee.png", 449, 428, 24, "OOT Zora Fountain Rupee 01", "adult", "Zora Fountain Rupee 01"),
                        MA("Rupee.png", 431, 398, 24, "OOT Zora Fountain Rupee 02", "adult", "Zora Fountain Rupee 02"),
                        MA("Rupee.png", 443, 367, 24, "OOT Zora Fountain Rupee 03", "adult", "Zora Fountain Rupee 03"),
                        MA("Rupee.png", 469, 359, 24, "OOT Zora Fountain Rupee 04", "adult", "Zora Fountain Rupee 04"),
                        MA("Rupee.png", 496, 382, 24, "OOT Zora Fountain Rupee 05", "adult", "Zora Fountain Rupee 05"),
                        MA("Rupee.png", 486, 417, 24, "OOT Zora Fountain Rupee 06", "adult", "Zora Fountain Rupee 06"),
                        MA("Rupee.png", 434, 456, 24, "OOT Zora Fountain Rupee 07", "adult", "Zora Fountain Rupee 07"),
                        MA("Rupee.png", 392, 404, 24, "OOT Zora Fountain Rupee 08", "adult", "Zora Fountain Rupee 08"),
                        MA("Rupee.png", 419, 348, 24, "OOT Zora Fountain Rupee 09", "adult", "Zora Fountain Rupee 09"),
                        MA("Rupee.png", 476, 334, 24, "OOT Zora Fountain Rupee 10", "adult", "Zora Fountain Rupee 10"),
                        MA("Rupee.png", 525, 371, 24, "OOT Zora Fountain Rupee 11", "adult", "Zora Fountain Rupee 11"),
                        MA("Rupee.png", 519, 440, 24, "OOT Zora Fountain Rupee 12", "adult", "Zora Fountain Rupee 12"),
                        MA("Rupee.png", 418, 487, 24, "OOT Zora Fountain Rupee 13", "adult", "Zora Fountain Rupee 13"),
                        MA("Rupee.png", 353, 409, 24, "OOT Zora Fountain Rupee 14", "adult", "Zora Fountain Rupee 14"),
                        MA("Rupee.png", 396, 334, 24, "OOT Zora Fountain Rupee 15", "adult", "Zora Fountain Rupee 15"),
                        MA("Rupee.png", 481, 308, 24, "OOT Zora Fountain Rupee 16", "adult", "Zora Fountain Rupee 16"),
                        MA("Rupee.png", 556, 357, 24, "OOT Zora Fountain Rupee 17", "adult", "Zora Fountain Rupee 17"),
                        MA("Rupee.png", 555, 466, 24, "OOT Zora Fountain Rupee 18", "adult", "Zora Fountain Rupee 18"),
                        
                        ME("Entrance.png", 742, 277, "Entrance shuffle (Zora Domain)", "OOT_DOMAIN_FROM_FOUNTAIN"),
                        ME("Entrance.png", 599, 372, "Entrance shuffle (Jabu-Jabu)", "OOT_JABU_JABU"),
                        ME("Entrance.png", 493, 578, "Entrance shuffle (Ice Cavern)", "OOT_ICE_CAVERN"),
                        ME("Entrance.png", 417, 17, "Entrance shuffle (Fairy Fountain)", "OOT_FAIRY_FARORE")
                    }
                },
                new MapSubRegion
                {
                    Name = "Great Fairy Fountain",
                    BackgroundImage = OoT("Fairy", "Great_Fairy"),
                    DestinationEntranceIds = new List<string> { "OOT_FAIRY_FARORE" },
                    Marks = new List<MapMark> 
                    { 
                        ME("Entrance.png", 480, 535, "Entrance shuffle (Zora Fountain)", "OOT_ZORA_FOUNTAIN_FROM_FAIRY"),
                        M("NPC.png", 482, 329, 40, "OOT Great Fairy Farore's Wind", "Great Fairy Farore's Wind") 
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion ZoraRiver()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Zora River";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Zora River",
                    BackgroundImage = OoT("Zora_River", "Zora_River"),
                    DestinationEntranceIds = new List<string>
                    {
                        "OOT_GROTTO_EXIT_FAIRY_RIVER",
						"OOT_GROTTO_EXIT_SCRUBS2_RIVER",
						"OOT_ZORA_RIVER_FROM_LOST_WOODS",
						"OOT_ZORA_RIVER_FROM_FIELD",
						"OOT_GROTTO_EXIT_GENERIC_RIVER",
						"OOT_RIVER_FROM_DOMAIN"
                    },
                    Marks = new List<MapMark>
                    {
                        MA("Butterfly.png", 1319, 365, 24, "OOT Zora River Butterfly Near Beans 1", "child", "Zora River Butterfly Near Beans 1"),
                        MA("Butterfly.png", 1293, 353, 24, "OOT Zora River Butterfly Near Beans 2", "child", "Zora River Butterfly Near Beans 2"),
                        MA("Butterfly.png", 100, 712, 24, "OOT Zora River Butterfly Near Waterfall 1", "child", "Zora River Butterfly Near Waterfall 1"),
                        MA("Butterfly.png", 110, 688, 24, "OOT Zora River Butterfly Near Waterfall 2", "child", "Zora River Butterfly Near Waterfall 2"),
                        MA("Butterfly.png", 100, 665, 24, "OOT Zora River Butterfly Near Waterfall 3", "child", "Zora River Butterfly Near Waterfall 3"),
                        MA("Gold_Skulltula.png", 203, 723, 40, "OOT Zora River GS Ladder", "child", "Zora River GS Ladder"),
                        MA("Gold_Skulltula.png", 1302, 287, 40, "OOT Zora River GS Tree", "child", "Zora River GS Tree"),
                        MA("Gold_Skulltula.png", 422, 699, 40, "OOT Zora River GS Near Bridge", "adult", "Zora River GS Near Bridge"),
                        MA("Gold_Skulltula.png", 998, 324, 40, "OOT Zora River GS Near Grotto", "adult", "Zora River GS Near Grotto"),
                        MA("NPC.png", 1118, 447, 40, "OOT Zora River Bean Seller", "child", "Zora River Bean Seller"),
                        MA("NPC.png", 793, 623, 40, "OOT Zora River Frogs Game", "child", "Zora River Frogs Game"),
                        MA("NPC.png", 777, 598, 40, "OOT Zora River Frogs Zeldas Lullaby", "child", "Zora River Frogs Zeldas Lullaby"),
                        MA("NPC.png", 807, 598, 40, "OOT Zora River Frogs Eponas Song", "child", "Zora River Frogs Eponas Song"),
                        MA("NPC.png", 823, 623, 40, "OOT Zora River Frogs Sarias Song", "child", "Zora River Frogs Sarias Song"),
                        MA("NPC.png", 806, 648, 40, "OOT Zora River Frogs Suns Song", "child", "Zora River Frogs Suns Song"),
                        MA("NPC.png", 777, 648, 40, "OOT Zora River Frogs Song of Time", "child", "Zora River Frogs Song of Time"),
                        MA("NPC.png", 763, 623, 40, "OOT Zora River Frogs Storms", "child", "Zora River Frogs Storms"),
                        M("Grass.png", 958, 691, 24, "OOT Zora River Grass", "Zora River Grass"),
                        M("Grass.png", 1263, 261, 24, "OOT Zora River Grass Pack Bush 01", "Zora River Grass Pack Bush 01"),
                        M("Grass.png", 1263, 248, 24, "OOT Zora River Grass Pack Bush 02", "Zora River Grass Pack Bush 02"),
                        M("Grass.png", 1279, 248, 24, "OOT Zora River Grass Pack Bush 03", "Zora River Grass Pack Bush 03"),
                        M("Grass.png", 1279, 261, 24, "OOT Zora River Grass Pack Bush 04", "Zora River Grass Pack Bush 04"),
                        M("Grass.png", 1271, 274, 24, "OOT Zora River Grass Pack Bush 05", "Zora River Grass Pack Bush 05"),
                        M("Grass.png", 1251, 268, 24, "OOT Zora River Grass Pack Bush 06", "Zora River Grass Pack Bush 06"),
                        M("Grass.png", 1247, 254, 24, "OOT Zora River Grass Pack Bush 07", "Zora River Grass Pack Bush 07"),
                        M("Grass.png", 1251, 238, 24, "OOT Zora River Grass Pack Bush 08", "Zora River Grass Pack Bush 08"),
                        M("Grass.png", 1271, 234, 24, "OOT Zora River Grass Pack Bush 09", "Zora River Grass Pack Bush 09"),
                        M("Grass.png", 1290, 238, 24, "OOT Zora River Grass Pack Bush 10", "Zora River Grass Pack Bush 10"),
                        M("Grass.png", 1295, 254, 24, "OOT Zora River Grass Pack Bush 11", "Zora River Grass Pack Bush 11"),
                        M("Grass.png", 1290, 268, 24, "OOT Zora River Grass Pack Bush 12", "Zora River Grass Pack Bush 12"),
                        M("Collectible.png", 919, 636, 40, "OOT Zora River HP Pillar", "Zora River HP Pillar"),
                        M("Collectible.png", 208, 756, 40, "OOT Zora River HP Platofrm", "Zora River HP Platform"),
                        M("Rock.png", 1278, 377, 24, "OOT Zora River Rock Circle Lower Rock 1", "Zora River Rock Circle Lower Rock 1"),
                        M("Rock.png", 1253, 384, 24, "OOT Zora River Rock Circle Lower Rock 2", "Zora River Rock Circle Lower Rock 2"),
                        M("Rock.png", 1242, 408, 24, "OOT Zora River Rock Circle Lower Rock 3", "Zora River Rock Circle Lower Rock 3"),
                        M("Rock.png", 1252, 434, 24, "OOT Zora River Rock Circle Lower Rock 4", "Zora River Rock Circle Lower Rock 4"),
                        M("Rock.png", 1275, 444, 24, "OOT Zora River Rock Circle Lower Rock 5", "Zora River Rock Circle Lower Rock 5"),
                        M("Rock.png", 1301, 437, 24, "OOT Zora River Rock Circle Lower Rock 6", "Zora River Rock Circle Lower Rock 6"),
                        M("Rock.png", 1310, 412, 24, "OOT Zora River Rock Circle Lower Rock 7", "Zora River Rock Circle Lower Rock 7"),
                        M("Rock.png", 1303, 385, 24, "OOT Zora River Rock Circle Lower Rock 8", "Zora River Rock Circle Lower Rock 8"),
                        M("Rock.png", 878, 427, 24, "OOT Zora River Rock Circle Upper Rock 1", "Zora River Rock Circle Upper Rock 1"),
                        M("Rock.png", 851, 436, 24, "OOT Zora River Rock Circle Upper Rock 2", "Zora River Rock Circle Upper Rock 2"),
                        M("Rock.png", 835, 460, 24, "OOT Zora River Rock Circle Upper Rock 3", "Zora River Rock Circle Upper Rock 3"),
                        M("Rock.png", 848, 487, 24, "OOT Zora River Rock Circle Upper Rock 4", "Zora River Rock Circle Upper Rock 4"),
                        M("Rock.png", 871, 496, 24, "OOT Zora River Rock Circle Upper Rock 5", "Zora River Rock Circle Upper Rock 5"),
                        M("Rock.png", 896, 488, 24, "OOT Zora River Rock Circle Upper Rock 6", "Zora River Rock Circle Upper Rock 6"),
                        M("Rock.png", 911, 466, 24, "OOT Zora River Rock Circle Upper Rock 7", "Zora River Rock Circle Upper Rock 7"),
                        M("Rock.png", 901, 440, 24, "OOT Zora River Rock Circle Upper Rock 8", "Zora River Rock Circle Upper Rock 8"),
                        M("Rock.png", 598, 544, 24, "OOT Zora River Rock Ground", "Zora River Rock Ground"),
                        M("Rock.png", 491, 506, 24, "OOT Zora River Rock Water 1", "Zora River Rock Water 1"),
                        M("Rock.png", 519, 503, 24, "OOT Zora River Rock Water 2", "Zora River Rock Water 2"),
                        M("Rock.png", 520, 481, 24, "OOT Zora River Rock Water 3", "Zora River Rock Water 3"),
                        M("Rock.png", 483, 473, 24, "OOT Zora River Rock Water 4", "Zora River Rock Water 4"),
                        MA("Rupee.png", 127, 706, 24, "OOT Zora River Rupee 1", "adult", "Zora River Rupee 1"),
                        MA("Rupee.png", 147, 695, 24, "OOT Zora River Rupee 2", "adult", "Zora River Rupee 2"),
                        MA("Rupee.png", 147, 676, 24, "OOT Zora River Rupee 3", "adult", "Zora River Rupee 3"),
                        MA("Rupee.png", 127, 666, 24, "OOT Zora River Rupee 4", "adult", "Zora River Rupee 4"),
                        MA("Soil.png", 1148, 431, 24, "OOT Zora River Soil 1", "child", "Zora River Soil 1"),
                        MA("Soil.png", 1122, 423, 24, "OOT Zora River Soil 2", "child", "Zora River Soil 2"),
                        MA("Soil.png", 1161, 458, 24, "OOT Zora River Soil 3", "child", "Zora River Soil 3"),
                        MA("Wonder.png", 1207, 241, 24, "OOT Zora River Wonder Item Front 1", "child", "Zora River Wonder Item Front 1"),
                        MA("Wonder.png", 1211, 270, 24, "OOT Zora River Wonder Item Front 2", "child", "Zora River Wonder Item Front 2"),
                        MA("Wonder.png", 1204, 210, 24, "OOT Zora River Wonder Item Front 3", "child", "Zora River Wonder Item Front 3"),
                        MA("Wonder.png", 1214, 298, 24, "OOT Zora River Wonder Item Front 4", "child", "Zora River Wonder Item Front 4"),
                        MA("Wonder.png", 1221, 351, 24, "OOT Zora River Wonder Item Back 01", "child", "Zora River Wonder Item Back 01"),
                        MA("Wonder.png", 1177, 402, 24, "OOT Zora River Wonder Item Back 02", "child", "Zora River Wonder Item Back 02"),
                        MA("Wonder.png", 1139, 405, 24, "OOT Zora River Wonder Item Back 03", "child", "Zora River Wonder Item Back 03"),
                        MA("Wonder.png", 1019, 589, 24, "OOT Zora River Wonder Item Back 04", "child", "Zora River Wonder Item Back 04"),
                        MA("Wonder.png", 1008, 475, 24, "OOT Zora River Wonder Item Back 05", "child", "Zora River Wonder Item Back 05"),
                        MA("Wonder.png", 1010, 504, 24, "OOT Zora River Wonder Item Back 06", "child", "Zora River Wonder Item Back 06"),
                        MA("Wonder.png", 985, 629, 24, "OOT Zora River Wonder Item Back 07", "child", "Zora River Wonder Item Back 07"),
                        MA("Wonder.png", 955, 390, 24, "OOT Zora River Wonder Item Back 08", "child", "Zora River Wonder Item Back 08"),
                        MA("Wonder.png", 934, 593, 24, "OOT Zora River Wonder Item Back 09", "child", "Zora River Wonder Item Back 09"),
                        MA("Wonder.png", 924, 389, 24, "OOT Zora River Wonder Item Back 10", "child", "Zora River Wonder Item Back 10"),
                        MA("Wonder.png", 884, 642, 24, "OOT Zora River Wonder Item Back 11", "child", "Zora River Wonder Item Back 11"),
                        MA("Wonder.png", 894, 389, 24, "OOT Zora River Wonder Item Back 12", "child", "Zora River Wonder Item Back 12"),
                        MA("Wonder.png", 862, 390, 24, "OOT Zora River Wonder Item Back 13", "child", "Zora River Wonder Item Back 13"),
                        MA("Wonder.png", 815, 419, 24, "OOT Zora River Wonder Item Back 14", "child", "Zora River Wonder Item Back 14"),
                        MA("Wonder.png", 792, 580, 24, "OOT Zora River Wonder Item Back 15", "child", "Zora River Wonder Item Back 15"),
                        MA("Wonder.png", 788, 439, 24, "OOT Zora River Wonder Item Back 16", "child", "Zora River Wonder Item Back 16"),
                        MA("Wonder.png", 773, 548, 24, "OOT Zora River Wonder Item Back 17", "child", "Zora River Wonder Item Back 17"),
                        MA("Wonder.png", 759, 457, 24, "OOT Zora River Wonder Item Back 18", "child", "Zora River Wonder Item Back 18"),
                        MA("Wonder.png", 755, 518, 24, "OOT Zora River Wonder Item Back 19", "child", "Zora River Wonder Item Back 19"),
                        MA("Wonder.png", 739, 484, 24, "OOT Zora River Wonder Item Back 20", "child", "Zora River Wonder Item Back 20"),
                        MA("Wonder.png", 1013, 532, 24, "OOT Zora River Wonder Item Back 21", "child", "Zora River Wonder Item Back 21"),
                        MA("Wonder.png", 839, 404, 24, "OOT Zora River Wonder Item Back 22", "child", "Zora River Wonder Item Back 22"),
                        MA("Wonder.png", 1007, 448, 24, "OOT Zora River Wonder Item Back 23", "child", "Zora River Wonder Item Back 23"),
                        MA("Wonder.png", 424, 677, 24, "OOT Zora River Wonder Item Back 24", "child", "Zora River Wonder Item Back 24"),
                        MA("Wonder.png", 389, 702, 24, "OOT Zora River Wonder Item Back 25", "child", "Zora River Wonder Item Back 25"),
                        MA("Wonder.png", 352, 702, 24, "OOT Zora River Wonder Item Back 26", "child", "Zora River Wonder Item Back 26"),
                        MA("Wonder.png", 316, 702, 24, "OOT Zora River Wonder Item Back 27", "child", "Zora River Wonder Item Back 27"),
                        
                        ME("Entrance.png", 1235, 149, "Entrance shuffle (Hyrule Field)", "OOT_FIELD_FROM_ZORA_RIVER"),
                        ME("Entrance.png", 101, 591, "Entrance shuffle (Lost Woods)", "OOT_LOST_WOODS_FROM_ZORA_RIVER"),
                        ME("Entrance.png", 42, 680, "Entrance shuffle (Zora Domain)", "OOT_ZORA_DOMAIN"),
                        ME("Entrance.png", 934, 344, "Entrance shuffle (Generic Grotto)", "OOT_GROTTO_GENERIC_RIVER"),
                        ME("Entrance.png", 866, 451, "Entrance shuffle (Fairy Grotto)", "OOT_GROTTO_FAIRY_RIVER"),
                        ME("Entrance.png", 1268, 401, "Entrance shuffle (Scrubs Grotto)", "OOT_GROTTO_SCRUBS2_RIVER")
                    }
                },
                new MapSubRegion
                {
                    Name = "Song of Storms Grotto",
                    BackgroundImage = OoT("Zora_River", "Storms"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_SCRUBS2_RIVER" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 655, 630, "Entrance shuffle (Zora River)", "OOT_GROTTO_EXIT_SCRUBS2_RIVER"),
                        M("Hive.png", 529, 134, 40, "OOT Zora River Storms Grotto Hive", "Zora River Storms Grotto Hive"),
                        M("Scrub.png", 698, 282, 40, "OOT Zora River Storms Grotto Front Scrub", "Zora River Storms Grotto Front Scrub"),
                        M("Scrub.png", 632, 188, 40, "OOT Zora River Storms Grotto Back Scrub", "Zora River Storms Grotto Back Scrub")
                    }
                },
                new MapSubRegion
                {
                    Name = "Generic Grotto",
                    BackgroundImage = OoT("Zora_River", "Generic"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_GENERIC_RIVER" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 651, 571, "Entrance shuffle (Zora River)", "OOT_GROTTO_EXIT_GENERIC_RIVER"),
                        M("Butterfly.png", 699, 613, 24, "OOT Zora River Grotto Butterfly 1", "Zora River Grotto Butterfly 1"),
                        M("Butterfly.png", 615, 613, 24, "OOT Zora River Grotto Butterfly 2", "Zora River Grotto Butterfly 2"),
                        M("Butterfly.png", 657, 626, 24, "OOT Zora River Grotto Butterfly 3", "Zora River Grotto Butterfly 3"),
                        M("Chest.png", 669, 162, 40, "OOT Zora River Grotto", "Zora River Grotto"),
                        M("Grass.png", 658, 134, 24, "OOT Zora River Grotto Grass 1", "Zora River Grotto Grass 1"),
                        M("Grass.png", 643, 186, 24, "OOT Zora River Grotto Grass 2", "Zora River Grotto Grass 2"),
                        M("Grass.png", 716, 186, 24, "OOT Zora River Grotto Grass 3", "Zora River Grotto Grass 3"),
                        M("Grass.png", 648, 316, 24, "OOT Zora River Grotto Grass 4", "Zora River Grotto Grass 4"),
                        M("Hive.png", 530, 141, 40, "OOT Zora River Grotto Hive 1", "Zora River Grotto Hive 1"),
                        M("Hive.png", 717, 79, 40, "OOT Zora River Grotto Hive 2", "Zora River Grotto Hive 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Fairy Fountain",
                    BackgroundImage = OoT("Fairy", "Fountain"),
                    DestinationEntranceIds = new List<string> { "OOT_GROTTO_FAIRY_RIVER" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 465, 502, "Entrance shuffle (Zora River)", "OOT_GROTTO_EXIT_FAIRY_RIVER"),
                        M("Fairy.png", 470, 204, 24, "OOT Zora River Fairy Fountain Fairy 1", "Zora River Fairy Fountain Fairy 1"),
                        M("Fairy.png", 490, 200, 24, "OOT Zora River Fairy Fountain Fairy 2", "Zora River Fairy Fountain Fairy 2"),
                        M("Fairy.png", 450, 194, 24, "OOT Zora River Fairy Fountain Fairy 3", "Zora River Fairy Fountain Fairy 3"),
                        M("Fairy.png", 471, 181, 24, "OOT Zora River Fairy Fountain Fairy 4", "Zora River Fairy Fountain Fairy 4"),
                        M("Fairy.png", 493, 178, 24, "OOT Zora River Fairy Fountain Fairy 5", "Zora River Fairy Fountain Fairy 5"),
                        M("Fairy.png", 449, 172, 24, "OOT Zora River Fairy Fountain Fairy 6", "Zora River Fairy Fountain Fairy 6"),
                        M("Fairy.png", 485, 155, 24, "OOT Zora River Fairy Fountain Fairy 7", "Zora River Fairy Fountain Fairy 7"),
                        M("Fairy.png", 464, 153, 24, "OOT Zora River Fairy Fountain Fairy 8", "Zora River Fairy Fountain Fairy 8")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion ClockTown()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Clock Town";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "South Clock Town",
                    BackgroundImage = MM("Clock_Town", "South"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_CLOCK_TOWN_FROM_CLOCK_TOWER",
						"MM_CLOCK_TOWN_FROM_CLOCK_TOWER_ROOF",
						"MM_WARP_OWL_CLOCK_TOWN",
						"MM_CLOCK_TOWN_SOUTH_BOTTOM_FROM_WEST",
						"MM_CLOCK_TOWN_SOUTH_BOTTOM_FROM_EAST",
						"MM_CLOCK_TOWN_SOUTH_FROM_NORTH",
						"MM_CLOCK_TOWN_SOUTH_TOP_FROM_EAST",
						"MM_CLOCK_TOWN_SOUTH_TOP_FROM_WEST",
						"MM_CLOCK_TOWN_SOUTH_FROM_FIELD",
						"MM_CLOCK_TOWN_SOUTH_FROM_LAUNDRY_POOL"
					},
                    Marks = new List<MapMark>
                    {
                        M("Scrub.png", 347, 400, 40, "MM Clock Town Business Scrub", "Clock Town Business Scrub"),
                        M("NPC.png", 175, 248, 40, "MM Clock Town Owl Statue", "Clock Town Owl Statue"),
                        M("NPC.png", 390, 295, 40, "MM Initial Song of Healing", "Initial Song of Healing"),
                        M("Collectible.png", 329, 204, 40, "MM Clock Town Platform HP", "Clock Town Platform HP"),
                        M("Collectible.png", 630, 544, 40, "MM Clock Town Post Box", "Clock Town Post Box"),
						M("Collectible.png", 317, 154, 40, "MM Clock Town Post Box", "Clock Town Post Box"),
                        M("Chest.png", 863, 220, 40, "MM Clock Town South Chest Lower", "Clock Town South Chest Lower"),
                        M("Chest.png", 629, 379, 40, "MM Clock Town South Chest Upper", "Clock Town South Chest Upper"),
                        M("Wonder.png", 287, 109, 24, "MM Clock Town South Wonder Item 1", "Clock Town South Wonder Item 1"),
                        M("Wonder.png", 314, 102, 24, "MM Clock Town South Wonder Item 2", "Clock Town South Wonder Item 2"),
                        M("Wonder.png", 298, 84, 24, "MM Clock Town South Wonder Item 3", "Clock Town South Wonder Item 3"),
						
						ME("Entrance.png", 332, 265, "Entrance shuffle (Clock Tower)", "MM_CLOCK_TOWER_FROM_CLOCK_TOWN"),
						ME("Entrance.png", 362, 523, "Entrance shuffle (West Clock Town Bottom)", "MM_CLOCK_TOWN_WEST_FROM_SOUTH_BOTTOM"),
						ME("Entrance.png", 649, 193, "Entrance shuffle (East Clock Town Bottom)", "MM_CLOCK_TOWN_EAST_FROM_SOUTH_BOTTOM"),
						ME("Entrance.png", 54, 119, "Entrance shuffle (North Clock Town)", "MM_CLOCK_TOWN_NORTH_FROM_SOUTH"),
						ME("Entrance.png", 350, 156, "Entrance shuffle (East Clock Town Top)", "MM_CLOCK_TOWN_EAST_FROM_SOUTH_TOP"),
						ME("Entrance.png", 32, 329, "Entrance shuffle (West Clock Town Top)", "MM_CLOCK_TOWN_WEST_FROM_SOUTH_TOP"),
						ME("Entrance.png", 826, 433, "Entrance shuffle (Termina Field)", "MM_TERMINA_FIELD_FROM_CLOCK_TOWN_SOUTH"),
						ME("Entrance.png", 573, 578, "Entrance shuffle (Laundry Pool)", "MM_LAUNDRY_POOL"),
						ME("Entrance.png", 260, 145, "Entrance shuffle (Clock Town Rooftop)", "MM_CLOCK_TOWER_ROOF")
                    }
                },
                new MapSubRegion
                {
                    Name = "North Clock Town",
                    BackgroundImage = MM("Clock_Town", "North"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_GROTTO_EXIT_DEKU_PLAYGROUND",
						"MM_CLOCK_TOWN_NORTH_FROM_FAIRY_FOUNTAIN",
						"MM_CLOCK_TOWN_NORTH_FROM_EAST",
						"MM_CLOCK_TOWN_NORTH_FROM_SOUTH",
						"MM_CLOCK_TOWN_NORTH_FROM_FIELD"
					},
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 355, 331, 40, "MM Clock Town Blast Mask", "Clock Town Blast Mask"),
                        M("NPC.png", 434, 225, 40, "MM Clock Town Bomber Notebook", "Clock Town Bomber Notebook"),
                        M("NPC.png", 676, 335, 40, "MM Clock Town Keaton HP", "Clock Town Keaton HP"),
						M("NPC.png", 568, 263, 40, "MM Tingle Map Clock Town", "Tingle Map Clock Town"),
						M("NPC.png", 568, 303, 40, "MM Tingle Map Woodfall", "Tingle Map Woodfall"),
						M("Collectible.png", 417, 332, 40, "MM Clock Town Post Box", "Clock Town Post Box"),
                        M("Collectible.png", 600, 379, 40, "MM Clock Town Tree HP", "Clock Town Tree HP"),
                        M("Tree.png", 601, 350, 24, "MM Clock Town North Forked Tree 1", "Clock Town North Forked Tree 1"),
                        M("Tree.png", 373, 151, 24, "MM Clock Town North Forked Tree 2", "Clock Town North Forked Tree 2"),
                        M("Grass.png", 674, 394, 24, "MM Clock Town Keaton Grass Reward 1", "Clock Town Keaton Grass Reward 1"),
                        M("Grass.png", 698, 394, 24, "MM Clock Town Keaton Grass Reward 2", "Clock Town Keaton Grass Reward 2"),
                        M("Grass.png", 692, 409, 24, "MM Clock Town Keaton Grass Reward 3", "Clock Town Keaton Grass Reward 3"),
                        M("Grass.png", 674, 416, 24, "MM Clock Town Keaton Grass Reward 4", "Clock Town Keaton Grass Reward 4"),
                        M("Grass.png", 656, 409, 24, "MM Clock Town Keaton Grass Reward 5", "Clock Town Keaton Grass Reward 5"),
                        M("Grass.png", 650, 394, 24, "MM Clock Town Keaton Grass Reward 6", "Clock Town Keaton Grass Reward 6"),
                        M("Grass.png", 656, 378, 24, "MM Clock Town Keaton Grass Reward 7", "Clock Town Keaton Grass Reward 7"),
                        M("Grass.png", 674, 371, 24, "MM Clock Town Keaton Grass Reward 8", "Clock Town Keaton Grass Reward 8"),
                        M("Grass.png", 692, 378, 24, "MM Clock Town Keaton Grass Reward 9", "Clock Town Keaton Grass Reward 9"),
						
						ME("Entrance.png", 331, 579, "Entrance shuffle (East Clock Town)", "MM_CLOCK_TOWN_EAST_FROM_NORTH"),
						ME("Entrance.png", 320, 210, "Entrance shuffle (South Clock Town)", "MM_CLOCK_TOWN_SOUTH_FROM_NORTH"),
						ME("Entrance.png", 598, 210, "Entrance shuffle (Termina Field)", "MM_TERMINA_FIELD_FROM_CLOCK_TOWN_NORTH"),
						ME("Entrance.png", 350, 55, "Entrance shuffle (Deku Playground)", "MM_GROTTO_DEKU_PLAYGROUND"),
						ME("Entrance.png", 443, 20, "Entrance shuffle (Fairy Fountain)", "MM_FAIRY_FOUNTAIN_TOWN")
                    }
                },
                new MapSubRegion
                {
                    Name = "West Clock Town",
                    BackgroundImage = MM("Clock_Town", "West"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_CLOCK_TOWN_WEST_FROM_BOMB_SHOP",
						"MM_CLOCK_TOWN_WEST_FROM_CURIOSITY_SHOP",
						"MM_CLOCK_TOWN_WEST_FROM_TRADING_POST",
						"MM_CLOCK_TOWN_WEST_FROM_SWORDSMAN_SCHOOL",
						"MM_CLOCK_TOWN_WEST_FROM_POST_OFFICE",
						"MM_CLOCK_TOWN_WEST_FROM_LOTTERY",
						"MM_CLOCK_TOWN_WEST_FROM_SOUTH_BOTTOM",
						"MM_CLOCK_TOWN_WEST_FROM_SOUTH_TOP",
						"MM_CLOCK_TOWN_WEST_FROM_FIELD"
					},
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 616, 335, 40, "MM Clock Town Bank Reward 1", "Clock Town Bank Reward 1"),
                        M("NPC.png", 650, 313, 40, "MM Clock Town Bank Reward 2", "Clock Town Bank Reward 2"),
                        M("NPC.png", 681, 283, 40, "MM Clock Town Bank Reward 3", "Clock Town Bank Reward 3"),
                        M("NPC.png", 378, 269, 40, "MM Clock Town Rosa Sisters HP", "Clock Town Rosa Sisters HP"),
						
						ME("Entrance.png", 617, 462, "Entrance shuffle (Bomb Shop)", "MM_BOMB_SHOP"),
						ME("Entrance.png", 842, 248, "Entrance shuffle (Curiosity Shop)", "MM_CURIOSITY_SHOP"),
						ME("Entrance.png", 791, 365, "Entrance shuffle (Trading Post)", "MM_TRADING_POST"),
						ME("Entrance.png", 12, 431, "Entrance shuffle (Swordsman School)", "MM_SWORDSMAN_SCHOOL"),
						ME("Entrance.png", 69, 287, "Entrance shuffle (Post Office)", "MM_POST_OFFICE"),
						ME("Entrance.png", 428, 217, "Entrance shuffle (Lottery)", "MM_LOTTERY"),
						ME("Entrance.png", 745, 128, "Entrance shuffle (South Clock Town Bottom)", "MM_CLOCK_TOWN_SOUTH_BOTTOM_FROM_WEST"),
						ME("Entrance.png", 279, 141, "Entrance shuffle (South Clock Town Top)", "MM_CLOCK_TOWN_SOUTH_TOP_FROM_WEST"),
						ME("Entrance.png", 208, 575, "Entrance shuffle (Termina Field)", "MM_TERMINA_FIELD_FROM_CLOCK_TOWN_WEST")
                    }
                },
                new MapSubRegion
                {
                    Name = "East Clock Town",
                    BackgroundImage = MM("Clock_Town", "East"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_CLOCK_TOWN_EAST_FROM_MAYORS_OFFICE",
						"MM_CLOCK_TOWN_EAST_FROM_ASTRAL_OBSERVATORY",
						"MM_CLOCK_TOWN_EAST_FROM_STOCK_POT_INN_ROOF",
						"MM_CLOCK_TOWN_EAST_FROM_HONEY_AND_DARLING",
						"MM_CLOCK_TOWN_EAST_FROM_MILK_BAR",
						"MM_CLOCK_TOWN_EAST_FROM_STOCK_POT_INN",
						"MM_CLOCK_TOWN_EAST_FROM_TOWN_ARCHERY",
						"MM_CLOCK_TOWN_EAST_FROM_CHEST_GAME",
						"MM_CLOCK_TOWN_EAST_FROM_SOUTH_BOTTOM",
						"MM_CLOCK_TOWN_EAST_FROM_SOUTH_TOP",
						"MM_CLOCK_TOWN_EAST_FROM_NORTH",
						"MM_CLOCK_TOWN_EAST_FROM_FIELD"
					},
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 775, 536, 40, "MM Clock Town Silver Rupee Chest", "Clock Town Silver Rupee Chest"),
						M("Collectible.png", 574, 313, 40, "MM Clock Town Post Box", "Clock Town Post Box"),
						M("Collectible.png", 150, 220, 40, "MM Clock Town Post Box", "Clock Town Post Box"),
                        M("NPC.png", 342, 274, 40, "MM Clock Town Postman Hat", "Clock Town Postman Hat"),
                        M("Crate.png", 439, 330, 24, "MM Clock Town East Small Crate 1", "Clock Town East Small Crate 1"),
                        M("Crate.png", 396, 406, 24, "MM Clock Town East Small Crate 2", "Clock Town East Small Crate 2"),
                        M("Wonder.png", 551, 274, 24, "MM Clock Town East Wonder Item Basket 1", "Clock Town East Wonder Item Basket 1"),
                        M("Wonder.png", 532, 261, 24, "MM Clock Town East Wonder Item Basket 2", "Clock Town East Wonder Item Basket 2"),
                        M("Wonder.png", 512, 269, 24, "MM Clock Town East Wonder Item Basket 3", "Clock Town East Wonder Item Basket 3"),
                        M("Wonder.png", 526, 240, 24, "MM Clock Town East Wonder Item Target Left 1", "Clock Town East Wonder Item Target Left 1"),
                        M("Wonder.png", 505, 237, 24, "MM Clock Town East Wonder Item Target Left 2", "Clock Town East Wonder Item Target Left 2"),
                        M("Wonder.png", 490, 251, 24, "MM Clock Town East Wonder Item Target Left 3", "Clock Town East Wonder Item Target Left 3"),
                        M("Wonder.png", 566, 248, 24, "MM Clock Town East Wonder Item Target Right 1", "Clock Town East Wonder Item Target Right 1"),
                        M("Wonder.png", 577, 266, 24, "MM Clock Town East Wonder Item Target Right 2", "Clock Town East Wonder Item Target Right 2"),
                        M("Wonder.png", 546, 244, 24, "MM Clock Town East Wonder Item Target Right 3", "Clock Town East Wonder Item Target Right 3"),
                        M("Stray_Fairy.png", 387, 352, 40, "MM Clock Town Stray Fairy", "Clock Town Stray Fairy"),
						
						ME("Entrance.png", 1, 170, "Entrance shuffle (Mayor's Office)", "MM_MAYORS_OFFICE"),
						ME("Entrance.png", 81, 152, "Entrance shuffle (Astral Observatory)", "MM_ASTRAL_OBSERVATORY_FROM_CLOCK_TOWN_EAST"),
						ME("Entrance.png", 37, 318, "Entrance shuffle (Stock Pot Inn Roof)", "MM_STOCK_POT_INN_ROOF"),
						ME("Entrance.png", 522, 290, "Entrance shuffle (Honey & Darling)", "MM_HONEY_AND_DARLING_GAME"),
						ME("Entrance.png", 287, 293, "Entrance shuffle (Milk Bar)", "MM_MILK_BAR"),
						ME("Entrance.png", 233, 381, "Entrance shuffle (Stock Pot Inn)", "MM_STOCK_POT_INN"),
						ME("Entrance.png", 685, 384, "Entrance shuffle (Town Archery)", "MM_TOWN_ARCHERY"),
						ME("Entrance.png", 474, 468, "Entrance shuffle (Chest Game)", "MM_CHEST_GAME"),
						ME("Entrance.png", 629, 561, "Entrance shuffle (South Clock Town Bottom)", "MM_CLOCK_TOWN_SOUTH_BOTTOM_FROM_EAST"),
						ME("Entrance.png", 233, 491, "Entrance shuffle (South Clock Town Top)", "MM_CLOCK_TOWN_SOUTH_TOP_FROM_EAST"),
						ME("Entrance.png", 1, 235, "Entrance shuffle (North Clock Town)", "MM_CLOCK_TOWN_NORTH_FROM_EAST"),
						ME("Entrance.png", 399, 249, "Entrance shuffle (Termina Field)", "MM_TERMINA_FIELD_FROM_CLOCK_TOWN_EAST")
                    }
                },
                new MapSubRegion
                {
                    Name = "Clock Tower Rooftop",
                    BackgroundImage = MM("Clock_Town", "Tower_Rooftop"),
                    DestinationEntranceIds = new List<string> { "MM_CLOCK_TOWER_ROOF" },
                    Marks = new List<MapMark>
                    {
                        M("Pot.png", 568, 200, 24, "MM Clock Tower Roof Pot 1", "Clock Tower Roof Pot 1"),
                        M("Pot.png", 353, 200, 24, "MM Clock Tower Roof Pot 2", "Clock Tower Roof Pot 2"),
                        M("Pot.png", 353, 410, 24, "MM Clock Tower Roof Pot 3", "Clock Tower Roof Pot 3"),
                        M("Pot.png", 568, 410, 24, "MM Clock Tower Roof Pot 4", "Clock Tower Roof Pot 4"),
                        M("NPC.png", 430, 190, 40, "MM Clock Tower Roof Skull Kid Ocarina", "Clock Tower Roof Skull Kid Ocarina"),
                        M("NPC.png", 480, 190, 40, "MM Clock Tower Roof Skull Kid Song of Time", "Clock Tower Roof Skull Kid Song of Time")
                    }
                },
                new MapSubRegion
                {
                    Name = "Laundry Pool",
                    BackgroundImage = MM("Clock_Town", "Pool"),
                    DestinationEntranceIds = new List<string> 
					{ 
						"MM_LAUNDRY_POOL_FROM_KAFEI_HIDEOUT",
						"MM_LAUNDRY_POOL"
					},
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 754, 110, 40, "MM Clock Tower Guru Guru Mask Bremen", "Clock Town Guru Guru Mask Bremen"),
                        M("Stray_Fairy.png", 628, 394, 40, "MM Clock Town Stray Fairy", "Clock Town Stray Fairy"),
                        M("Crate.png", 791, 60, 24, "MM Clock Town Laundry Pool Small Crate", "Clock Town Laundry Pool Small Crate"),
                        M("Grass.png", 543, 237, 24, "MM Clock Town Laundry Pool Grass 1", "Clock Town Laundry Pool Grass 1"),
                        M("Grass.png", 457, 246, 24, "MM Clock Town Laundry Pool Grass 2", "Clock Town Laundry Pool Grass 2"),
                        M("Grass.png", 448, 194, 24, "MM Clock Town Laundry Pool Grass 3", "Clock Town Laundry Pool Grass 3"),
                        M("Rupee.png", 426, 411, 24, "MM Clock Town Laundry Pool Rupee 1", "Clock Town Laundry Pool Rupee 1"),
                        M("Rupee.png", 446, 378, 24, "MM Clock Town Laundry Pool Rupee 2", "Clock Town Laundry Pool Rupee 2"),
                        M("Rupee.png", 463, 343, 24, "MM Clock Town Laundry Pool Rupee 3", "Clock Town Laundry Pool Rupee 3"),
						
						ME("Entrance.png", 527, 3, "Entrance shuffle (South Clock Town)", "MM_CLOCK_TOWN_SOUTH_FROM_LAUNDRY_POOL"),
						ME("Entrance.png", 156, 390, "Entrance shuffle (Kafei Hideout)", "MM_KAFEI_HIDEOUT")
                    }
                },
                new MapSubRegion
                {
                    Name = "Bomb Shop",
                    BackgroundImage = MM("Clock_Town", "Bomb_Shop"),
                    DestinationEntranceIds = new List<string> { "MM_BOMB_SHOP" },
                    Marks = new List<MapMark>
                    {
                        M("Shop.png", 409, 256, 40, "MM Bomb Shop Item 1", "Bomb Shop Item 1"),
                        M("Shop.png", 459, 266, 40, "MM Bomb Shop Item 2", "Bomb Shop Item 2"),
                        M("Shop.png", 507, 280, 40, "MM Bomb Shop Bomb Bag", "Bomb Shop Bomb Bag"),
                        M("Shop.png", 517, 230, 40, "MM Bomb Shop Bomb Bag 2", "Bomb Shop Bomb Bag 2"),
						
						ME("Entrance.png", 304, 563, "Entrance shuffle (West Clock Town)", "MM_CLOCK_TOWN_WEST_FROM_BOMB_SHOP")
                    }
                },
                new MapSubRegion
                {
                    Name = "Trading Post",
                    BackgroundImage = MM("Clock_Town", "Trading_Post"),
                    DestinationEntranceIds = new List<string> { "MM_TRADING_POST" },
                    Marks = new List<MapMark>
                    {
                        M("Shop.png", 367, 228, 40, "MM Trading Post Item 1", "Trading Post Item 1"),
                        M("Shop.png", 425, 228, 40, "MM Trading Post Item 2", "Trading Post Item 2"),
                        M("Shop.png", 367, 286, 40, "MM Trading Post Item 3", "Trading Post Item 3"),
                        M("Shop.png", 425, 286, 40, "MM Trading Post Item 4", "Trading Post Item 4"),
                        M("Shop.png", 503, 228, 40, "MM Trading Post Item 5", "Trading Post Item 5"),
                        M("Shop.png", 561, 228, 40, "MM Trading Post Item 6", "Trading Post Item 6"),
                        M("Shop.png", 503, 286, 40, "MM Trading Post Item 7", "Trading Post Item 7"),
                        M("Shop.png", 561, 286, 40, "MM Trading Post Item 8", "Trading Post Item 8"),
                        M("Bush.png", 149, 416, 24, "MM Trading Post Bush 1", "Trading Post Bush 1"),
                        M("Bush.png", 206, 388, 24, "MM Trading Post Bush 2", "Trading Post Bush 2"),
                        M("Bush.png", 837, 399, 24, "MM Trading Post Bush 3", "Trading Post Bush 3"),
                        M("Bush.png", 754, 570, 24, "MM Trading Post Bush 4", "Trading Post Bush 4"),
                        M("Pot.png", 741, 407, 24, "MM Trading Post Pot", "Trading Post Pot"),
						
						ME("Entrance.png", 483, 573, "Entrance shuffle (West Clock Town)", "MM_CLOCK_TOWN_WEST_FROM_TRADING_POST")
                    }
                },
                new MapSubRegion
                {
                    Name = "Curiosity Shop / Kafei Hideout",
                    BackgroundImage = MM("Clock_Town", "Curiosity_Shop"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_KAFEI_HIDEOUT",
						"MM_CURIOSITY_SHOP"
					},
                    Marks = new List<MapMark>
                    {
                        M("Shop.png", 293, 374, 40, "MM Curiosity Shop All-Night Mask", "Curiosity Shop All-Night Mask"),
                        M("NPC.png", 532, 239, 40, "MM Kafei Hideout Owner Reward 1", "Kafei Hideout Owner Reward 1"),
                        M("NPC.png", 577, 239, 40, "MM Kafei Hideout Owner Reward 2", "Kafei Hideout Owner Reward 2"),
                        M("NPC.png", 532, 284, 40, "MM Kafei Hideout Pendant of Memories", "Kafei Hideout Pendant of Memories"),
						M("Shop.png", 339, 374, 40, "MM Bomb Shop Bomb Bag 2", "Bomb Shop Bomb Bag 2"),
						
						ME("Entrance.png", 747, 153, "Entrance shuffle (Laundry Pool)", "MM_LAUNDRY_POOL_FROM_KAFEI_HIDEOUT"),
						ME("Entrance.png", 21, 371, "Entrance shuffle (West Clock Town)", "MM_CLOCK_TOWN_WEST_FROM_CURIOSITY_SHOP")
                    }
                },
                new MapSubRegion
                {
                    Name = "Swordsman School",
                    BackgroundImage = MM("Clock_Town", "Swordsman_School"),
                    DestinationEntranceIds = new List<string> { "MM_SWORDSMAN_SCHOOL" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 492, 337, 40, "MM Swordsman School HP", "Swordsman School HP"),
                        M("Pot.png", 740, 419, 24, "MM Swordsman School Pot 1", "Swordsman School Pot 1"),
                        M("Pot.png", 740, 374, 24, "MM Swordsman School Pot 2", "Swordsman School Pot 2"),
                        M("Pot.png", 791, 374, 24, "MM Swordsman School Pot 3", "Swordsman School Pot 3"),
                        M("Pot.png", 791, 332, 24, "MM Swordsman School Pot 4", "Swordsman School Pot 4"),
                        M("Pot.png", 740, 332, 24, "MM Swordsman School Pot 5", "Swordsman School Pot 5"),
						
						ME("Entrance.png", 173, 342, "Entrance shuffle (West Clock Town)", "MM_CLOCK_TOWN_WEST_FROM_SWORDSMAN_SCHOOL")
                    }
                },
                new MapSubRegion
                {
                    Name = "Post Office",
                    BackgroundImage = MM("Clock_Town", "Post_Office"),
                    DestinationEntranceIds = new List<string> { "MM_POST_OFFICE" },
                    Marks = new List<MapMark> 
                    { 
                        M("NPC.png", 212, 169, 40, "MM Post Office HP", "Post Office HP"),
						ME("Entrance.png", 755, 299, "Entrance shuffle (West Clock Town)", "MM_CLOCK_TOWN_WEST_FROM_POST_OFFICE")
                    }
                },
                new MapSubRegion
                {
                    Name = "Lottery",
                    BackgroundImage = MM("Clock_Town", "Lottery"),
                    DestinationEntranceIds = new List<string> { "MM_LOTTERY" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 133, 202, 40, "MM Lottery Prize Night 1", "Lottery Prize Night 1"),
                        M("NPC.png", 173, 202, 40, "MM Lottery Prize Night 2", "Lottery Prize Night 2"),
                        M("NPC.png", 211, 202, 40, "MM Lottery Prize Night 3", "Lottery Prize Night 3"),
						
						ME("Entrance.png", 173, 338, "Entrance shuffle (West Clock Town)", "MM_CLOCK_TOWN_WEST_FROM_LOTTERY")
                    }
                },
                new MapSubRegion
                {
                    Name = "Shooting Gallery",
                    BackgroundImage = MM("Clock_Town", "Shooting"),
                    DestinationEntranceIds = new List<string> { "MM_TOWN_ARCHERY" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 602, 316, 40, "MM Town Archery Reward 1", "Town Archery Reward 1"),
                        M("NPC.png", 602, 375, 40, "MM Town Archery Reward 2", "Town Archery Reward 2"),
						
						ME("Entrance.png", 782, 336, "Entrance shuffle (East Clock Town)", "MM_CLOCK_TOWN_EAST_FROM_TOWN_ARCHERY")
                    }
                },
                new MapSubRegion
                {
                    Name = "Treasure Chest Game",
                    BackgroundImage = MM("Clock_Town", "Treasure"),
					DestinationEntranceIds = new List<string> { "MM_CHEST_GAME" },
                    Marks = new List<MapMark> 
                    { 
                        M("NPC.png", 212, 425, 40, "MM Chest Game HP", "Chest Game HP"),
						ME("Entrance.png", 777, 428, "Entrance shuffle (East Clock Town)", "MM_CLOCK_TOWN_EAST_FROM_CHEST_GAME")
                    }
                },
                new MapSubRegion
                {
                    Name = "Honey & Darling",
                    BackgroundImage = MM("Clock_Town", "Honey_Darling"),
					DestinationEntranceIds = new List<string> { "MM_HONEY_AND_DARLING_GAME" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 427, 202, 40, "MM Honey & Darling Reward Any Day", "Honey & Darling Reward Any Day"),
                        M("NPC.png", 481, 202, 40, "MM Honey & Darling Reward All Days", "Honey & Darling Reward All Days"),
						
						ME("Entrance.png", 452, 70, "Entrance shuffle (East Clock Town)", "MM_CLOCK_TOWN_EAST_FROM_HONEY_AND_DARLING")
                    }
                },
                new MapSubRegion
                {
                    Name = "Town Hall",
                    BackgroundImage = MM("Clock_Town", "Town_Hall"),
                    DestinationEntranceIds = new List<string> { "MM_MAYORS_OFFICE" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 334, 142, 40, "MM Mayor's Office HP", "Mayor's Office HP"),
                        M("NPC.png", 524, 56, 40, "MM Mayor's Office Kafei's Mask", "Mayor's Office Kafei's Mask"),
						ME("Entrance.png", 443, 576, "Entrance shuffle (East Clock Town)", "MM_CLOCK_TOWN_EAST_FROM_MAYORS_OFFICE")
                    }
                },
                new MapSubRegion
                {
                    Name = "Milk Bar",
                    BackgroundImage = MM("Clock_Town", "Milk_Bar"),
                    DestinationEntranceIds = new List<string> { "MM_MILK_BAR" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 551, 202, 40, "MM Milk Bar Purchase Milk", "Milk Bar Purchase Milk"),
                        M("NPC.png", 597, 202, 40, "MM Milk Bar Purchase Chateau", "Milk Bar Purchase Chateau"),
                        M("NPC.png", 533, 254, 40, "MM Milk Bar Madame Aroma Bottle", "Milk Bar Madame Aroma Bottle"),
                        M("NPC.png", 335, 304, 40, "MM Milk Bar Troupe Leader Mask", "Milk Bar Troupe Leader Mask"),
						ME("Entrance.png", 816, 424, "Entrance shuffle (East Clock Town)", "MM_CLOCK_TOWN_EAST_FROM_MILK_BAR")
                    }
                },
                new MapSubRegion
                {
                    Name = "Astral Observatory - Passage",
                    BackgroundImage = MM("Clock_Town", "Passage"),
                    DestinationEntranceIds = new List<string> { "MM_ASTRAL_OBSERVATORY_FROM_CLOCK_TOWN_EAST" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 670, 511, 40, "MM Astral Observatory Passage Chest", "Astral Observatory Passage Chest"),
                        M("Pot.png", 78, 342, 24, "MM Astral Observatory Passage Pot 1", "Astral Observatory Passage Pot 1"),
                        M("Pot.png", 77, 367, 24, "MM Astral Observatory Passage Pot 2", "Astral Observatory Passage Pot 2"),
                        M("Pot.png", 82, 264, 24, "MM Astral Observatory Passage Pot 3", "Astral Observatory Passage Pot 3"),
                        M("Pot.png", 83, 239, 24, "MM Astral Observatory Passage Pot 4", "Astral Observatory Passage Pot 4"),
						ME("Entrance.png", 868, 135, "Entrance shuffle (East Clock Town)", "MM_CLOCK_TOWN_EAST_FROM_ASTRAL_OBSERVATORY")
                    }
                },
                new MapSubRegion
                {
                    Name = "Astral Observatory",
                    BackgroundImage = MM("Clock_Town", "Observatory"),
                    DestinationEntranceIds = new List<string> { "MM_ASTRAL_OBSERVATORY_FROM_FIELD" },
                    Marks = new List<MapMark>
                    {
                        M("Pot.png", 502, 455, 24, "MM Astral Observatory Pot 1", "Astral Observatory Pot 1"),
                        M("Pot.png", 470, 468, 24, "MM Astral Observatory Pot 2", "Astral Observatory Pot 2"),
                        M("Pot.png", 472, 443, 24, "MM Astral Observatory Pot 3", "Astral Observatory Pot 3"),
						ME("Entrance.png", 310, 244, "Entrance shuffle (Termina Field)", "MM_FIELD_FROM_ASTRAL_OBSERVATORY")
                    }
                },
                new MapSubRegion
                {
                    Name = "Stock Pot Inn - Lobby",
                    BackgroundImage = MM("Clock_Town", "SPI_Lobby"),
                    DestinationEntranceIds = new List<string> { "MM_STOCK_POT_INN" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 293, 292, 40, "MM Stock Pot Inn Room Key", "Stock Pot Inn Room Key"),
                        M("Wonder.png", 679, 163, 24, "MM Stock Pot Inn Wonder Item 1", "Stock Pot Inn Wonder Item 1"),
                        M("Wonder.png", 723, 168, 24, "MM Stock Pot Inn Wonder Item 2", "Stock Pot Inn Wonder Item 2"),
                        M("Wonder.png", 699, 142, 24, "MM Stock Pot Inn Wonder Item 3", "Stock Pot Inn Wonder Item 3"),
						ME("Entrance.png", 505, 493, "Entrance shuffle (East Clock Town)", "MM_CLOCK_TOWN_EAST_FROM_STOCK_POT_INN")
                    }
                },
                new MapSubRegion
                {
                    Name = "Stock Pot Inn - Back",
                    BackgroundImage = MM("Clock_Town", "SPI_Back"),
                    DestinationEntranceIds = new List<string> { "MM_STOCK_POT_INN_ROOF" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 142, 253, 40, "MM Stock Pot Inn ??? HP", "Stock Pot Inn ??? HP"),
                        M("NPC.png", 603, 328, 40, "MM Stock Pot Inn Letter to Kafei", "Stock Pot Inn Letter to Kafei"),
						ME("Entrance.png", 187, 70, "Entrance shuffle (East Clock Town)", "MM_CLOCK_TOWN_EAST_FROM_STOCK_POT_INN_ROOF")
                    }
                },
                new MapSubRegion
                {
                    Name = "Stock Pot Inn - Rooms",
                    BackgroundImage = MM("Clock_Town", "SPI_Rooms"),
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 200, 292, 40, "MM Stock Pot Inn Couple's Mask", "Stock Pot Inn Couple's Mask"),
                        M("NPC.png", 280, 448, 40, "MM Stock Pot Inn Grandma HP 1", "Stock Pot Inn Grandma HP 1"),
                        M("NPC.png", 321, 430, 40, "MM Stock Pot Inn Grandma HP 2", "Stock Pot Inn Grandma HP 2"),
                        M("Chest.png", 696, 283, 40, "MM Stock Pot Inn Guest Room Chest", "Stock Pot Inn Guest Room Chest"),
                        M("Chest.png", 390, 264, 40, "MM Stock Pot Inn Staff Room Chest", "Stock Pot Inn Staff Room Chest")
                    }
                },
                new MapSubRegion
                {
                    Name = "Deku Playground",
                    BackgroundImage = MM("Clock_Town", "Deku_Playground"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_DEKU_PLAYGROUND" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 373, 273, 40, "MM Deku Playground Reward Any Day", "Deku Playground Reward Any Day"),
                        M("NPC.png", 373, 323, 40, "MM Deku Playground Reward All Days", "Deku Playground Reward All Days"),
						ME("Entrance.png", 123, 295, "Entrance shuffle (North Clock Town)", "MM_GROTTO_EXIT_DEKU_PLAYGROUND")
                    }
                },
                new MapSubRegion
                {
                    Name = "Fairy Fountain",
                    BackgroundImage = MM("Clock_Town", "Fairy"),
                    DestinationEntranceIds = new List<string> { "MM_FAIRY_FOUNTAIN_TOWN" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 433, 360, 40, "MM Clock Town Great Fairy", "Clock Town Great Fairy"),
                        M("NPC.png", 483, 360, 40, "MM Clock Town Great Fairy Alt", "Clock Town Great Fairy Alt"),
						ME("Entrance.png", 456, 561, "Entrance shuffle (North Clock Town)", "MM_CLOCK_TOWN_NORTH_FROM_FAIRY_FOUNTAIN")
                    }
                }
            };
            return mapRegion;
        }

        private static MapRegion DekuPalace()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Deku Palace";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Deku Palace",
                    BackgroundImage = MM("Deku_Palace", "Outside"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_GROTTO_EXIT_JP_CLIMB_LEFT",
						"MM_GROTTO_EXIT_BEAN",
						"MM_GROTTO_EXIT_JP_LINE_END",
						"MM_DEKU_PALACE_EXTERIOR_FROM_THRONE_CAGE",
						"MM_DEKU_PALACE_EXTERIOR_FROM_THRONE",
						"MM_DEKU_PALACE_EXTERIOR_FROM_SHRINE",
						"MM_GROTTO_EXIT_JP_LINE_START",
						"MM_GROTTO_EXIT_JP_CLIMB_RIGHT",
						"MM_DEKU_PALACE_MAIN_ENTRANCE",
						"MM_DEKU_PALACE_LEDGE"
					},
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 187, 116, 40, "MM Deku Palace HP", "Deku Palace HP"),
                        M("Pot.png", 560, 321, 24, "MM Deku Palace Pot 1", "Deku Palace Pot 1"),
                        M("Pot.png", 560, 297, 24, "MM Deku Palace Pot 2", "Deku Palace Pot 2"),
                        M("Red_Boulder.png", 685, 249, 24, "MM Deku Palace Red Boulder 1", "Deku Palace Red Boulder 1"),
                        M("Red_Boulder.png", 624, 215, 24, "MM Deku Palace Red Boulder 2", "Deku Palace Red Boulder 2"),
                        M("Red_Boulder.png", 635, 24, 24, "MM Deku Palace Red Boulder 3", "Deku Palace Red Boulder 3"),
                        M("Soil.png", 874, 111, 24, "MM Deku Palace Soil Item 1", "Deku Palace Soil Item 1"),
                        M("Soil.png", 895, 90, 24, "MM Deku Palace Soil Item 2", "Deku Palace Soil Item 2"),
                        M("Soil.png", 874, 68, 24, "MM Deku Palace Soil Item 3", "Deku Palace Soil Item 3"),
                        MNJ("Rupee.png", 343, 153, 24, "MM Deku Palace Rupee Layout-Dependant 1", "Deku Palace", "Deku Palace Rupee Layout-Dependant 1"),
                        MNJ("Rupee.png", 374, 153, 24, "MM Deku Palace Rupee Layout-Dependant 2", "Deku Palace", "Deku Palace Rupee Layout-Dependant 2"),
                        MJ("Rupee.png", 563, 37, 24, "MM Deku Palace Rupee Layout-Dependant 1", "Deku Palace", "Deku Palace Rupee Layout-Dependant 1"),
                        MJ("Rupee.png", 575, 20, 24, "MM Deku Palace Rupee Layout-Dependant 2", "Deku Palace", "Deku Palace Rupee Layout-Dependant 2"),
                        M("Rupee.png", 256, 282, 24, "MM Deku Palace Rupee Left 1", "Deku Palace Rupee Left 1"),
                        M("Rupee.png", 274, 282, 24, "MM Deku Palace Rupee Left 2", "Deku Palace Rupee Left 2"),
                        M("Rupee.png", 238, 282, 24, "MM Deku Palace Rupee Left 3", "Deku Palace Rupee Left 3"),
                        M("Rupee.png", 383, 200, 24, "MM Deku Palace Rupee Left 5", "Deku Palace Rupee Left 5"),
                        M("Rupee.png", 365, 200, 24, "MM Deku Palace Rupee Left 6", "Deku Palace Rupee Left 6"),
                        M("Rupee.png", 347, 200, 24, "MM Deku Palace Rupee Left 7", "Deku Palace Rupee Left 7"),
                        M("Rupee.png", 218, 65, 24, "MM Deku Palace Rupee Left Far 1", "Deku Palace Rupee Left Far 1"),
                        M("Rupee.png", 200, 65, 24, "MM Deku Palace Rupee Left Far 2", "Deku Palace Rupee Left Far 2"),
                        M("Rupee.png", 182, 65, 24, "MM Deku Palace Rupee Left Far 3", "Deku Palace Rupee Left Far 3"),
                        M("Rupee.png", 692, 106, 24, "MM Deku Palace Rupee Right 01", "Deku Palace Rupee Right 01"),
                        M("Rupee.png", 673, 87, 24, "MM Deku Palace Rupee Right 02", "Deku Palace Rupee Right 02"),
                        M("Rupee.png", 763, 95, 24, "MM Deku Palace Rupee Right 03", "Deku Palace Rupee Right 03"),
                        M("Rupee.png", 763, 119, 24, "MM Deku Palace Rupee Right 04", "Deku Palace Rupee Right 04"),
                        M("Rupee.png", 736, 119, 24, "MM Deku Palace Rupee Right 05", "Deku Palace Rupee Right 05"),
                        M("Rupee.png", 736, 95, 24, "MM Deku Palace Rupee Right 06", "Deku Palace Rupee Right 06"),
                        MNJ("Rupee.png", 580, 179, 24, "MM Deku Palace Rupee Right 07", "Deku Palace", "Deku Palace Rupee Right 07"),
                        MNJ("Rupee.png", 598, 167, 24, "MM Deku Palace Rupee Right 08", "Deku Palace", "Deku Palace Rupee Right 08"),
                        MNJ("Rupee.png", 598, 143, 24, "MM Deku Palace Rupee Right 09", "Deku Palace", "Deku Palace Rupee Right 09"),
                        MNJ("Rupee.png", 580, 131, 24, "MM Deku Palace Rupee Right 10", "Deku Palace", "Deku Palace Rupee Right 10"),
                        MNJ("Rupee.png", 562, 143, 24, "MM Deku Palace Rupee Right 11", "Deku Palace", "Deku Palace Rupee Right 11"),
                        MNJ("Rupee.png", 562, 167, 24, "MM Deku Palace Rupee Right 12", "Deku Palace", "Deku Palace Rupee Right 12"),
                        MNJ("Rupee.png", 580, 155, 24, "MM Deku Palace Rupee Right 13", "Deku Palace", "Deku Palace Rupee Right 13"),
						MJ("Rupee.png", 574, 229, 24, "MM Deku Palace Rupee Right 07", "Deku Palace", "Deku Palace Rupee Right 07"),
                        MJ("Rupee.png", 592, 217, 24, "MM Deku Palace Rupee Right 08", "Deku Palace", "Deku Palace Rupee Right 08"),
                        MJ("Rupee.png", 592, 193, 24, "MM Deku Palace Rupee Right 09", "Deku Palace", "Deku Palace Rupee Right 09"),
                        MJ("Rupee.png", 574, 181, 24, "MM Deku Palace Rupee Right 10", "Deku Palace", "Deku Palace Rupee Right 10"),
                        MJ("Rupee.png", 556, 193, 24, "MM Deku Palace Rupee Right 11", "Deku Palace", "Deku Palace Rupee Right 11"),
                        MJ("Rupee.png", 556, 217, 24, "MM Deku Palace Rupee Right 12", "Deku Palace", "Deku Palace Rupee Right 12"),
                        MJ("Rupee.png", 574, 205, 24, "MM Deku Palace Rupee Right 13", "Deku Palace", "Deku Palace Rupee Right 13"),
						
						MEJ("Entrance.png", 356, 147, "Entrance shuffle (JP Climb Left Grotto)", "Deku Palace", "MM_GROTTO_JP_CLIMB_LEFT"),
						MENJ("Entrance.png", 733, 74, "Entrance shuffle (Bean Grotto)", "Deku Palace", "MM_GROTTO_BEAN"),
						MEJ("Entrance.png", 596, 1, "Entrance shuffle (Bean Grotto)", "Deku Palace", "MM_GROTTO_BEAN"),
						MEJ("Entrance.png", 539, 2, "Entrance shuffle (JP Line End Grotto)", "Deku Palace", "MM_GROTTO_JP_LINE_END"),
						ME("Entrance.png", 392, 53, "Entrance shuffle (Deku King Chamber Cage)", "MM_DEKU_PALACE_THRONE_CAGE"),
						ME("Entrance.png", 465, 163, "Entrance shuffle (Deku King Chamber)", "MM_DEKU_PALACE_THRONE"),
						ME("Entrance.png", 52, 2, "Entrance shuffle (Deku Shrine)", "MM_DEKU_SHRINE"),
						MEJ("Entrance.png", 235, 10, "Entrance shuffle (JP Line Start Grotto)", "Deku Palace", "MM_GROTTO_JP_LINE_START"),
						MEJ("Entrance.png", 570, 147, "Entrance shuffle (JP Climb Right Grotto)", "Deku Palace", "MM_GROTTO_JP_CLIMB_RIGHT"),
						ME("Entrance.png", 463, 571, "Entrance shuffle (Swamp Main Entrance)", "MM_SWAMP_FROM_PALACE_MAIN_ENTRANCE"),
						ME("Entrance.png", 641, 486, "Entrance shuffle (Swamp Ledge)", "MM_SWAMP_FROM_PALACE_LEDGE")
                    }
                },
                new MapSubRegion
                {
                    Name = "Deku King Chamber",
                    BackgroundImage = MM("Deku_Palace", "King"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_DEKU_PALACE_THRONE",
						"MM_DEKU_PALACE_THRONE_CAGE"
					},
                    Marks = new List<MapMark> 
					{ 
						M("NPC.png", 159, 291, 40, "MM Deku Palace Sonata of Awakening", "Deku Palace Sonata of Awakening"),
						ME("Entrance.png", 483, 485, "Entrance shuffle (Deku Palace)", "MM_DEKU_PALACE_EXTERIOR_FROM_THRONE"),
						ME("Entrance.png", 25, 310, "Entrance shuffle (Deku Palace Cage)", "MM_DEKU_PALACE_EXTERIOR_FROM_THRONE_CAGE")
					}
                },
                new MapSubRegion
                {
                    Name = "Deku Shrine",
                    BackgroundImage = MM("Deku_Palace", "Shrine"),
                    DestinationEntranceIds = new List<string> { "MM_DEKU_SHRINE" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 146, 177, 40, "MM Deku Shrine Mask of Scents", "Deku Shrine Mask of Scents"),
                        M("Pot.png", 769, 534, 24, "MM Deku Shrine Pot 1", "Deku Shrine Pot 1"),
                        M("Pot.png", 825, 417, 24, "MM Deku Shrine Pot 2", "Deku Shrine Pot 2"),
                        M("Rupee.png", 318, 413, 24, "MM Deku Shrine Rupee Main 01", "Deku Shrine Rupee Main 01"),
                        M("Rupee.png", 328, 447, 24, "MM Deku Shrine Rupee Main 02", "Deku Shrine Rupee Main 02"),
                        M("Rupee.png", 340, 387, 24, "MM Deku Shrine Rupee Main 03", "Deku Shrine Rupee Main 03"),
                        M("Rupee.png", 343, 359, 24, "MM Deku Shrine Rupee Main 04", "Deku Shrine Rupee Main 04"),
                        M("Rupee.png", 361, 456, 24, "MM Deku Shrine Rupee Main 05", "Deku Shrine Rupee Main 05"),
                        M("Rupee.png", 383, 435, 24, "MM Deku Shrine Rupee Main 06", "Deku Shrine Rupee Main 06"),
                        M("Rupee.png", 382, 337, 24, "MM Deku Shrine Rupee Main 07", "Deku Shrine Rupee Main 07"),
                        M("Rupee.png", 393, 365, 24, "MM Deku Shrine Rupee Main 08", "Deku Shrine Rupee Main 08"),
                        M("Rupee.png", 346, 153, 24, "MM Deku Shrine Rupee Main 09", "Deku Shrine Rupee Main 09"),
                        M("Rupee.png", 362, 148, 24, "MM Deku Shrine Rupee Main 10", "Deku Shrine Rupee Main 10"),
                        M("Rupee.png", 378, 148, 24, "MM Deku Shrine Rupee Main 11", "Deku Shrine Rupee Main 11"),
                        M("Rupee.png", 400, 167, 24, "MM Deku Shrine Rupee Main 12", "Deku Shrine Rupee Main 12"),
                        M("Rupee.png", 416, 167, 24, "MM Deku Shrine Rupee Main 13", "Deku Shrine Rupee Main 13"),
                        M("Rupee.png", 432, 161, 24, "MM Deku Shrine Rupee Main 14", "Deku Shrine Rupee Main 14"),
                        M("Rupee.png", 515, 94, 24, "MM Deku Shrine Rupee Main 15", "Deku Shrine Rupee Main 15"),
                        M("Rupee.png", 533, 94, 24, "MM Deku Shrine Rupee Main 16", "Deku Shrine Rupee Main 16"),
                        M("Rupee.png", 551, 94, 24, "MM Deku Shrine Rupee Main 17", "Deku Shrine Rupee Main 17"),
                        M("Rupee.png", 567, 28, 24, "MM Deku Shrine Rupee Main 18", "Deku Shrine Rupee Main 18"),
                        M("Rupee.png", 567, 76, 24, "MM Deku Shrine Rupee Main 19", "Deku Shrine Rupee Main 19"),
                        M("Rupee.png", 567, 52, 24, "MM Deku Shrine Rupee Main 20", "Deku Shrine Rupee Main 20"),
                        M("Rupee.png", 145, 368, 24, "MM Deku Shrine Rupee End 01", "Deku Shrine Rupee End 01"),
                        M("Rupee.png", 146, 344, 24, "MM Deku Shrine Rupee End 02", "Deku Shrine Rupee End 02"),
                        M("Rupee.png", 147, 320, 24, "MM Deku Shrine Rupee End 03", "Deku Shrine Rupee End 03"),
                        M("Rupee.png", 167, 376, 24, "MM Deku Shrine Rupee End 04", "Deku Shrine Rupee End 04"),
                        M("Rupee.png", 185, 376, 24, "MM Deku Shrine Rupee End 05", "Deku Shrine Rupee End 05"),
                        M("Rupee.png", 172, 315, 24, "MM Deku Shrine Rupee End 06", "Deku Shrine Rupee End 06"),
                        M("Rupee.png", 190, 315, 24, "MM Deku Shrine Rupee End 07", "Deku Shrine Rupee End 07"),
                        M("Rupee.png", 208, 315, 24, "MM Deku Shrine Rupee End 08", "Deku Shrine Rupee End 08"),
                        M("Rupee.png", 231, 331, 24, "MM Deku Shrine Rupee End 09", "Deku Shrine Rupee End 09"),
                        M("Rupee.png", 230, 355, 24, "MM Deku Shrine Rupee End 10", "Deku Shrine Rupee End 10"),
						ME("Entrance.png", 722, 581, "Entrance shuffle (Deku Palace)", "MM_DEKU_PALACE_EXTERIOR_FROM_SHRINE")
                    }
                },
                new MapSubRegion
                {
                    Name = "Beans Grotto",
                    BackgroundImage = MM("Deku_Palace", "Beans"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_BEAN" },
                    Marks = new List<MapMark>
                    {
                        M("Butterfly.png", 865, 515, 24, "MM Deku Palace Grotto Butterfly 1", "Deku Palace Grotto Butterfly 1"),
                        M("Butterfly.png", 793, 507, 24, "MM Deku Palace Grotto Butterfly 2", "Deku Palace Grotto Butterfly 2"),
                        M("Butterfly.png", 858, 467, 24, "MM Deku Palace Grotto Butterfly 3", "Deku Palace Grotto Butterfly 3"),
                        M("Butterfly.png", 782, 462, 24, "MM Deku Palace Grotto Butterfly 4", "Deku Palace Grotto Butterfly 4"),
                        M("Chest.png", 128, 201, 40, "MM Deku Palace Grotto Chest", "Deku Palace Grotto Chest"),
                        M("Soil.png", 282, 527, 24, "MM Beans Grotto Soil Item 1", "Beans Grotto Soil Item 1"),
                        M("Soil.png", 296, 503, 24, "MM Beans Grotto Soil Item 2", "Beans Grotto Soil Item 2"),
                        M("Soil.png", 282, 479, 24, "MM Beans Grotto Soil Item 3", "Beans Grotto Soil Item 3"),
                        M("Grass.png", 213, 509, 24, "MM Deku Palace Grotto Grass 01", "Deku Palace Grotto Grass 01"),
                        M("Grass.png", 213, 499, 24, "MM Deku Palace Grotto Grass 02", "Deku Palace Grotto Grass 02"),
                        M("Grass.png", 229, 499, 24, "MM Deku Palace Grotto Grass 03", "Deku Palace Grotto Grass 03"),
                        M("Grass.png", 229, 509, 24, "MM Deku Palace Grotto Grass 04", "Deku Palace Grotto Grass 04"),
                        M("Grass.png", 221, 518, 24, "MM Deku Palace Grotto Grass 05", "Deku Palace Grotto Grass 05"),
                        M("Grass.png", 202, 515, 24, "MM Deku Palace Grotto Grass 06", "Deku Palace Grotto Grass 06"),
                        M("Grass.png", 198, 504, 24, "MM Deku Palace Grotto Grass 07", "Deku Palace Grotto Grass 07"),
                        M("Grass.png", 201, 493, 24, "MM Deku Palace Grotto Grass 08", "Deku Palace Grotto Grass 08"),
                        M("Grass.png", 221, 489, 24, "MM Deku Palace Grotto Grass 09", "Deku Palace Grotto Grass 09"),
                        M("Grass.png", 239, 493, 24, "MM Deku Palace Grotto Grass 10", "Deku Palace Grotto Grass 10"),
                        M("Grass.png", 242, 504, 24, "MM Deku Palace Grotto Grass 11", "Deku Palace Grotto Grass 11"),
                        M("Grass.png", 239, 515, 24, "MM Deku Palace Grotto Grass 12", "Deku Palace Grotto Grass 12"),
						ME("Entrance.png", 822, 486, "Entrance shuffle (Deku Palace)", "MM_GROTTO_EXIT_BEAN")
                    }
                },
                new MapSubRegion
                {
                    Name = "JP Climb Grotto",
                    BackgroundImage = "region maps/MM/Deku_Palace/JP_Grotto_1.jpg",
					DestinationEntranceIds = new List<string>
					{
						"MM_GROTTO_JP_CLIMB_RIGHT",
						"MM_GROTTO_JP_CLIMB_LEFT"
					},
                    RequiredSettingKey = "Majora's Mask JP Layouts",
                    RequiredSettingValue = "all",
                    RequiredSettingContains = "Deku Palace",
                    Marks = new List<MapMark>
					{
						ME("Entrance.png", 172, 675, "Entrance shuffle (Deku Palace JP Climb Right)", "MM_GROTTO_EXIT_JP_CLIMB_RIGHT"),
						ME("Entrance.png", 184, 248, "Entrance shuffle (Deku Palace JP Climb Left)", "MM_GROTTO_EXIT_JP_CLIMB_LEFT")
					}
                },
                new MapSubRegion
                {
                    Name = "JP Line Grotto",
                    BackgroundImage = "region maps/MM/Deku_Palace/JP_Grotto_2.jpg",
					DestinationEntranceIds = new List<string>
					{
						"MM_GROTTO_JP_LINE_START",
						"MM_GROTTO_JP_LINE_END"
					},
                    RequiredSettingKey = "Majora's Mask JP Layouts",
                    RequiredSettingValue = "all",
                    RequiredSettingContains = "Deku Palace",
                    Marks = new List<MapMark>
					{
						M("Butterfly.png", 158, 403, 24, "MM Deku Palace JP Line Grotto Butterfly 1", "Deku Palace JP Line Grotto Butterfly 1"),
						M("Butterfly.png", 122, 403, 24, "MM Deku Palace JP Line Grotto Butterfly 2", "Deku Palace JP Line Grotto Butterfly 2"),
						M("Butterfly.png", 112, 373, 24, "MM Deku Palace JP Line Grotto Butterfly 3", "Deku Palace JP Line Grotto Butterfly 3"),
						M("Butterfly.png", 166, 373, 24, "MM Deku Palace JP Line Grotto Butterfly 4", "Deku Palace JP Line Grotto Butterfly 4"),
						M("Butterfly.png", 140, 346, 24, "MM Deku Palace JP Line Grotto Butterfly 5", "Deku Palace JP Line Grotto Butterfly 5"),
						M("Grass.png", 188, 518, 24, "MM Deku Palace JP Line Grotto Grass 1", "Deku Palace JP Line Grotto Grass 1"),
						M("Grass.png", 188, 600, 24, "MM Deku Palace JP Line Grotto Grass 2", "Deku Palace JP Line Grotto Grass 2"),
						M("Grass.png", 97, 518, 24, "MM Deku Palace JP Line Grotto Grass 3", "Deku Palace JP Line Grotto Grass 3"),
						M("Grass.png", 97, 600, 24, "MM Deku Palace JP Line Grotto Grass 4", "Deku Palace JP Line Grotto Grass 4"),
						M("Grass.png", 92, 230, 24, "MM Deku Palace JP Line Grotto Grass 5", "Deku Palace JP Line Grotto Grass 5"),
						ME("Entrance.png", 133, 555, "Entrance shuffle (Deku Palace JP Line Start)", "MM_GROTTO_EXIT_JP_LINE_START"),
						ME("Entrance.png", 135, 181, "Entrance shuffle (Deku Palace JP Line End)", "MM_GROTTO_EXIT_JP_LINE_END")
					}
                }
            };
            return mapRegion;
        }

        public static MapRegion GoronVillage()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Goron Village";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Goron Village",
                    BackgroundImage = MM("Goron_Village", "Village"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_GORON_VILLAGE_FROM_LONE_PEAK_SHRINE",
						"MM_GORON_VILLAGE_FROM_GORON_SHRINE",
						"MM_GORON_VILLAGE_FROM_TWIN_ISLANDS"
					},
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 104, 299, 40, "MM Goron Powder Keg", "Goron Powder Keg"),
                        M("Crate.png", 167, 336, 24, "MM Goron Village Crate", "Goron Village Crate"),
                        M("Collectible.png", 706, 300, 40, "MM Goron Village HP", "Goron Village HP"),
                        M("Shop.png", 664, 286, 40, "MM Goron Village Scrub Bomb Bag", "Goron Village Scrub Bomb Bag"),
                        M("Scrub.png", 664, 310, 40, "MM Goron Village Scrub Deed", "Goron Village Scrub Deed"),
                        MA("Snowball.png", 657, 261, 24, "MM Goron Village Big Snowball 1", "cursed", "Goron Village Big Snowball 1"),
                        MA("Snowball.png", 349, 433, 24, "MM Goron Village Big Snowball 2", "cursed", "Goron Village Big Snowball 2"),
                        MA("Snowball.png", 553, 543, 24, "MM Goron Village Big Snowball 3", "cursed", "Goron Village Big Snowball 3"),
                        MA("Snowball.png", 433, 226, 24, "MM Goron Village Big Snowball 4", "cursed", "Goron Village Big Snowball 4"),
                        MA("Snowball.png", 418, 551, 24, "MM Goron Village Big Snowball 5", "cursed", "Goron Village Big Snowball 5"),
                        MA("Snowball.png", 617, 355, 24, "MM Goron Village Big Snowball 6", "cursed", "Goron Village Big Snowball 6"),
                        MA("Snowball.png", 368, 337, 24, "MM Goron Village Big Snowball 7", "cursed", "Goron Village Big Snowball 7"),
                        MA("Snowball.png", 624, 468, 24, "MM Goron Village Big Snowball 8", "cursed", "Goron Village Big Snowball 8"),
                        MA("Snowball.png", 418, 313, 24, "MM Goron Village Big Snowball 9", "cursed", "Goron Village Big Snowball 9"),
                        MA("Snowball.png", 494, 318, 24, "MM Goron Village Small Snowball 01", "cursed", "Goron Village Small Snowball 01"),
                        MA("Snowball.png", 443, 514, 24, "MM Goron Village Small Snowball 02", "cursed", "Goron Village Small Snowball 02"),
                        MA("Snowball.png", 468, 525, 24, "MM Goron Village Small Snowball 03", "cursed", "Goron Village Small Snowball 03"),
                        MA("Snowball.png", 493, 188, 24, "MM Goron Village Small Snowball 04", "cursed", "Goron Village Small Snowball 04"),
                        MA("Snowball.png", 641, 447, 24, "MM Goron Village Small Snowball 05", "cursed", "Goron Village Small Snowball 05"),
                        MA("Snowball.png", 382, 277, 24, "MM Goron Village Small Snowball 06", "cursed", "Goron Village Small Snowball 06"),
                        MA("Snowball.png", 655, 365, 24, "MM Goron Village Small Snowball 07", "cursed", "Goron Village Small Snowball 07"),
                        MA("Snowball.png", 378, 389, 24, "MM Goron Village Small Snowball 08", "cursed", "Goron Village Small Snowball 08"),
                        MA("Snowball.png", 640, 283, 24, "MM Goron Village Small Snowball 09", "cursed", "Goron Village Small Snowball 09"),
                        MA("Snowball.png", 328, 404, 24, "MM Goron Village Small Snowball 10", "cursed", "Goron Village Small Snowball 10"),
                        MA("Snowball.png", 602, 209, 24, "MM Goron Village Small Snowball 11", "cursed", "Goron Village Small Snowball 11"),
                        MA("Snowball.png", 554, 339, 24, "MM Goron Village Small Snowball 12", "cursed", "Goron Village Small Snowball 12"),
						
						ME("Entrance.png", 646, 567, "Entrance shuffle (Twin Islands)", "MM_TWIN_ISLANDS_FROM_GORON_VILLAGE"),
						ME("Entrance.png", 573, 424, "Entrance shuffle (Goron Shrine)", "MM_GORON_SHRINE"),
						ME("Entrance.png", 527, 7, "Entrance shuffle (Lone Peak Shrine)", "MM_LONE_PEAK_SHRINE")
                    }
                },
                new MapSubRegion
                {
                    Name = "Goron Shrine",
                    BackgroundImage = MM("Goron_Village", "City"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_GORON_SHRINE",
						"MM_GORON_SHRINE_FROM_SHOP"
					},
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 850, 263, 40, "MM Goron Baby", "Goron Baby"),
                        M("Pot.png", 450, 394, 24, "MM Goron Shrine Pot 01", "Goron Shrine Pot 01"),
                        M("Pot.png", 442, 418, 24, "MM Goron Shrine Pot 02", "Goron Shrine Pot 02"),
                        M("Pot.png", 254, 362, 24, "MM Goron Shrine Pot 03", "Goron Shrine Pot 03"),
                        M("Pot.png", 232, 354, 24, "MM Goron Shrine Pot 04", "Goron Shrine Pot 04"),
                        M("Pot.png", 188, 338, 24, "MM Goron Shrine Pot 05", "Goron Shrine Pot 05"),
                        M("Pot.png", 472, 400, 24, "MM Goron Shrine Pot 06", "Goron Shrine Pot 06"),
                        M("Pot.png", 464, 424, 24, "MM Goron Shrine Pot 07", "Goron Shrine Pot 07"),
                        M("Pot.png", 210, 346, 24, "MM Goron Shrine Pot 08", "Goron Shrine Pot 08"),
                        M("Pot.png", 781, 318, 24, "MM Goron Shrine Pot 09", "Goron Shrine Pot 09"),
                        M("Pot.png", 803, 318, 24, "MM Goron Shrine Pot 10", "Goron Shrine Pot 10"),
                        M("Pot.png", 825, 318, 24, "MM Goron Shrine Pot 11", "Goron Shrine Pot 11"),
                        M("Rock.png", 389, 225, 24, "MM Goron Shrine Rock 01", "Goron Shrine Rock 01"),
                        M("Rock.png", 419, 398, 24, "MM Goron Shrine Rock 02", "Goron Shrine Rock 02"),
                        M("Rock.png", 394, 389, 24, "MM Goron Shrine Rock 03", "Goron Shrine Rock 03"),
                        M("Rock.png", 416, 373, 24, "MM Goron Shrine Rock 04", "Goron Shrine Rock 04"),
                        M("Rock.png", 232, 142, 24, "MM Goron Shrine Rock 05", "Goron Shrine Rock 05"),
                        M("Rock.png", 133, 273, 24, "MM Goron Shrine Rock 06", "Goron Shrine Rock 06"),
                        M("Rock.png", 128, 348, 24, "MM Goron Shrine Rock 07", "Goron Shrine Rock 07"),
                        M("Rock.png", 138, 379, 24, "MM Goron Shrine Rock 08", "Goron Shrine Rock 08"),
                        M("Rock.png", 450, 368, 24, "MM Goron Shrine Rock 09", "Goron Shrine Rock 09"),
                        M("Rock.png", 465, 345, 24, "MM Goron Shrine Rock 10", "Goron Shrine Rock 10"),
                        M("Rock.png", 587, 345, 24, "MM Goron Shrine Rock 11", "Goron Shrine Rock 11"),
                        M("Rock.png", 512, 390, 24, "MM Goron Shrine Rock 12", "Goron Shrine Rock 12"),
                        M("Rock.png", 509, 150, 24, "MM Goron Shrine Rock 13", "Goron Shrine Rock 13"),
                        M("Rock.png", 472, 173, 24, "MM Goron Shrine Rock 14", "Goron Shrine Rock 14"),
                        M("Rock.png", 544, 173, 24, "MM Goron Shrine Rock 15", "Goron Shrine Rock 15"),
                        M("Rock.png", 549, 345, 24, "MM Goron Shrine Rock 16", "Goron Shrine Rock 16"),
						
						ME("Entrance.png", 154, 445, "Entrance shuffle (Goron Village)", "MM_GORON_VILLAGE_FROM_GORON_SHRINE"),
						ME("Entrance.png", 238, 195, "Entrance shuffle (Goron Shop)", "MM_GORON_SHOP")
                    }
                },
                new MapSubRegion
                {
                    Name = "Shop",
                    BackgroundImage = MM("Goron_Village", "Shop"),
                    DestinationEntranceIds = new List<string> { "MM_GORON_SHOP" },
                    Marks = new List<MapMark>
                    {
                        M("Shop.png", 282, 168, 40, "MM Goron Shop Item 1", "Goron Shop Item 1"),
                        M("Shop.png", 328, 168, 40, "MM Goron Shop Item 2", "Goron Shop Item 2"),
                        M("Shop.png", 374, 168, 40, "MM Goron Shop Item 3", "Goron Shop Item 3"),
						ME("Entrance.png", 432, 517, "Entrance shuffle (Goron Shrine)", "MM_GORON_SHRINE_FROM_SHOP")
                    }
                },
                new MapSubRegion
                {
                    Name = "Lone Peak Shrine",
                    BackgroundImage = MM("Goron_Village", "Lone_Peak_Shrine"),
                    DestinationEntranceIds = new List<string> { "MM_LONE_PEAK_SHRINE" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 313, 392, 40, "MM Lone Peak Shrine Boulder Chest", "Lone Peak Shrine Boulder Chest"),
                        M("Chest.png", 499, 211, 40, "MM Lone Peak Shrine Invisible Chest", "Lone Peak Shrine Invisible Chest"),
                        M("Chest.png", 499, 326, 40, "MM Lone Peak Shrine Lens Chest", "Lone Peak Shrine Lens Chest"),
                        M("Grass.png", 552, 464, 24, "MM Lone Peak Shrine Grass Pack 1 Grass 01", "Lone Peak Shrine Grass Pack 1 Grass 01"),
                        M("Grass.png", 552, 450, 24, "MM Lone Peak Shrine Grass Pack 1 Grass 02", "Lone Peak Shrine Grass Pack 1 Grass 02"),
                        M("Grass.png", 570, 450, 24, "MM Lone Peak Shrine Grass Pack 1 Grass 03", "Lone Peak Shrine Grass Pack 1 Grass 03"),
                        M("Grass.png", 570, 464, 24, "MM Lone Peak Shrine Grass Pack 1 Grass 04", "Lone Peak Shrine Grass Pack 1 Grass 04"),
                        M("Grass.png", 561, 478, 24, "MM Lone Peak Shrine Grass Pack 1 Grass 05", "Lone Peak Shrine Grass Pack 1 Grass 05"),
                        M("Grass.png", 542, 471, 24, "MM Lone Peak Shrine Grass Pack 1 Grass 06", "Lone Peak Shrine Grass Pack 1 Grass 06"),
                        M("Grass.png", 536, 456, 24, "MM Lone Peak Shrine Grass Pack 1 Grass 07", "Lone Peak Shrine Grass Pack 1 Grass 07"),
                        M("Grass.png", 542, 441, 24, "MM Lone Peak Shrine Grass Pack 1 Grass 08", "Lone Peak Shrine Grass Pack 1 Grass 08"),
                        M("Grass.png", 561, 434, 24, "MM Lone Peak Shrine Grass Pack 1 Grass 09", "Lone Peak Shrine Grass Pack 1 Grass 09"),
                        M("Grass.png", 580, 441, 24, "MM Lone Peak Shrine Grass Pack 1 Grass 10", "Lone Peak Shrine Grass Pack 1 Grass 10"),
                        M("Grass.png", 586, 456, 24, "MM Lone Peak Shrine Grass Pack 1 Grass 11", "Lone Peak Shrine Grass Pack 1 Grass 11"),
                        M("Grass.png", 580, 471, 24, "MM Lone Peak Shrine Grass Pack 1 Grass 12", "Lone Peak Shrine Grass Pack 1 Grass 12"),
                        M("Grass.png", 415, 286, 24, "MM Lone Peak Shrine Grass Pack 2 Grass 01", "Lone Peak Shrine Grass Pack 2 Grass 01"),
                        M("Grass.png", 415, 272, 24, "MM Lone Peak Shrine Grass Pack 2 Grass 02", "Lone Peak Shrine Grass Pack 2 Grass 02"),
                        M("Grass.png", 433, 272, 24, "MM Lone Peak Shrine Grass Pack 2 Grass 03", "Lone Peak Shrine Grass Pack 2 Grass 03"),
                        M("Grass.png", 433, 286, 24, "MM Lone Peak Shrine Grass Pack 2 Grass 04", "Lone Peak Shrine Grass Pack 2 Grass 04"),
                        M("Grass.png", 424, 300, 24, "MM Lone Peak Shrine Grass Pack 2 Grass 05", "Lone Peak Shrine Grass Pack 2 Grass 05"),
                        M("Grass.png", 405, 293, 24, "MM Lone Peak Shrine Grass Pack 2 Grass 06", "Lone Peak Shrine Grass Pack 2 Grass 06"),
                        M("Grass.png", 399, 278, 24, "MM Lone Peak Shrine Grass Pack 2 Grass 07", "Lone Peak Shrine Grass Pack 2 Grass 07"),
                        M("Grass.png", 405, 263, 24, "MM Lone Peak Shrine Grass Pack 2 Grass 08", "Lone Peak Shrine Grass Pack 2 Grass 08"),
                        M("Grass.png", 424, 256, 24, "MM Lone Peak Shrine Grass Pack 2 Grass 09", "Lone Peak Shrine Grass Pack 2 Grass 09"),
                        M("Grass.png", 443, 263, 24, "MM Lone Peak Shrine Grass Pack 2 Grass 10", "Lone Peak Shrine Grass Pack 2 Grass 10"),
                        M("Grass.png", 449, 278, 24, "MM Lone Peak Shrine Grass Pack 2 Grass 11", "Lone Peak Shrine Grass Pack 2 Grass 11"),
                        M("Grass.png", 443, 293, 24, "MM Lone Peak Shrine Grass Pack 2 Grass 12", "Lone Peak Shrine Grass Pack 2 Grass 12"),
						
						ME("Entrance.png", 490, 571, "Entrance shuffle (Goron Village)", "MM_GORON_VILLAGE_FROM_LONE_PEAK_SHRINE")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion GreatBayCoast()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Great Bay Coast";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Great Bay Coast",
                    BackgroundImage = MM("Great_Bay_Coast", "Great_Bay"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_GREAT_BAY_FROM_PIRATE_FORTRESS",
						"MM_GREAT_BAY_COAST_FROM_FISHER_HUT",
						"MM_GROTTO_EXIT_COW_COAST",
						"MM_WARP_OWL_GREAT_BAY",
						"MM_GREAT_BAY_FROM_SPIDER_HOUSE",
						"MM_GROTTO_EXIT_GENERIC_GREAT_BAY_COAST",
						"MM_GREAT_BAY_COAST_FROM_FIELD",
						"MM_GREAT_BAY_COAST_FROM_LABORATORY",
						"MM_GREAT_BAY_COAST_FROM_ZORA_CAPE",
						"MM_GREAT_BAY_FROM_PINNACLE_ROCK",
						"MM_VOID_GREAT_BAY",
						"MM_VOID_GREAT_BAY_BY_PINNACLE_ROCK"
					},
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 662, 59, 40, "MM Great Bay Coast HP", "Great Bay Coast HP"),
                        MA("Butterfly.png", 643, 592, 24, "MM Great Bay Coast Cleared Butterfly 1", "cleared", "Great Bay Coast Cleared Butterfly 1"),
                        MA("Butterfly.png", 657, 592, 24, "MM Great Bay Coast Cleared Butterfly 2", "cleared", "Great Bay Coast Cleared Butterfly 2"),
                        M("Butterfly.png", 650, 581, 24, "MM Great Bay Coast Common Butterfly 1", "Great Bay Coast Common Butterfly 1"),
                        M("Butterfly.png", 664, 581, 24, "MM Great Bay Coast Common Butterfly 2", "Great Bay Coast Common Butterfly 2"),
                        MA("NPC.png", 467, 157, 40, "MM Great Bay Coast Fisherman HP", "cleared", "Great Bay Coast Fisherman HP"),
                        M("Grass.png", 645, 548, 24, "MM Great Bay Coast Grass 1", "Great Bay Coast Grass 1"),
                        M("Grass.png", 635, 548, 24, "MM Great Bay Coast Grass 2", "Great Bay Coast Grass 2"),
                        M("Grass.png", 645, 568, 24, "MM Great Bay Coast Grass 3", "Great Bay Coast Grass 3"),
                        M("Grass.png", 635, 568, 24, "MM Great Bay Coast Grass 4", "Great Bay Coast Grass 4"),
                        M("Grass.png", 635, 558, 24, "MM Great Bay Coast Grass 5", "Great Bay Coast Grass 5"),
                        M("NPC.png", 305, 472, 40, "MM Great Bay Coast Owl Statue", "Great Bay Coast Owl Statue"),
						M("NPC.png", 261, 509, 40, "MM Tingle Map Great Bay", "Tingle Map Great Bay"),
						M("NPC.png", 301, 509, 40, "MM Tingle Map Ikana", "Tingle Map Ikana"),
                        M("Tree.png", 589, 423, 24, "MM Great Bay Coast Palm Tree Beach 1", "Great Bay Coast Palm Tree Beach 1"),
                        M("Tree.png", 583, 509, 24, "MM Great Bay Coast Palm Tree Beach 2", "Great Bay Coast Palm Tree Beach 2"),
                        M("Tree.png", 546, 554, 24, "MM Great Bay Coast Palm Tree Beach 3", "Great Bay Coast Palm Tree Beach 3"),
                        M("Tree.png", 506, 164, 24, "MM Great Bay Coast Palm Tree Island", "Great Bay Coast Palm Tree Island"),
                        M("Pot.png", 473, 70, 24, "MM Great Bay Coast Pot 01", "Great Bay Coast Pot 01"),
                        M("Pot.png", 381, 235, 24, "MM Great Bay Coast Pot 02", "Great Bay Coast Pot 02"),
                        M("Pot.png", 666, 131, 24, "MM Great Bay Coast Pot 03", "Great Bay Coast Pot 03"),
                        M("Pot.png", 682, 181, 24, "MM Great Bay Coast Pot 04", "Great Bay Coast Pot 04"),
                        M("Pot.png", 248, 502, 24, "MM Great Bay Coast Pot 05", "Great Bay Coast Pot 05"),
                        M("Pot.png", 252, 474, 24, "MM Great Bay Coast Pot 06", "Great Bay Coast Pot 06"),
                        M("Pot.png", 250, 488, 24, "MM Great Bay Coast Pot 07", "Great Bay Coast Pot 07"),
                        M("Pot.png", 465, 51, 24, "MM Great Bay Coast Pot 08", "Great Bay Coast Pot 08"),
                        M("Pot.png", 386, 252, 24, "MM Great Bay Coast Pot 09", "Great Bay Coast Pot 09"),
                        M("Pot.png", 657, 108, 24, "MM Great Bay Coast Pot 10", "Great Bay Coast Pot 10"),
                        M("Pot.png", 709, 228, 24, "MM Great Bay Coast Pot 11", "Great Bay Coast Pot 11"),
                        M("Pot.png", 254, 460, 24, "MM Great Bay Coast Pot 12", "Great Bay Coast Pot 12"),
                        M("Pot.png", 715, 197, 24, "MM Great Bay Coast Pot Ledge 1", "Great Bay Coast Pot Ledge 1"),
                        M("Pot.png", 698, 182, 24, "MM Great Bay Coast Pot Ledge 2", "Great Bay Coast Pot Ledge 2"),
                        M("Pot.png", 709, 147, 24, "MM Great Bay Coast Pot Ledge 3", "Great Bay Coast Pot Ledge 3"),
                        M("Soil.png", 688, 116, 24, "MM Great Bay Coast Soil", "Great Bay Coast Soil"),
                        M("NPC.png", 518, 556, 40, "MM Great Bay Coast Zora Mask", "Great Bay Coast Zora Mask"),
                        M("Rock.png", 681, 214, 24, "MM Great Bay Coast Rock Ledge 1", "Great Bay Coast Rock Ledge 1"),
                        M("Rock.png", 624, 98, 24, "MM Great Bay Coast Rock Ledge 2", "Great Bay Coast Rock Ledge 2"),
                        M("Rock.png", 662, 291, 24, "MM Great Bay Coast Rock Ledge 3", "Great Bay Coast Rock Ledge 3"),
                        M("Rock.png", 579, 599, 24, "MM Great Bay Coast Rock Sand 1", "Great Bay Coast Rock Sand 1"),
                        M("Rock.png", 636, 422, 24, "MM Great Bay Coast Rock Sand 2", "Great Bay Coast Rock Sand 2"),
                        M("Rock.png", 560, 405, 24, "MM Great Bay Coast Rock Sand 3", "Great Bay Coast Rock Sand 3"),
                        M("Rock.png", 598, 577, 24, "MM Great Bay Coast Rock Sand 4", "Great Bay Coast Rock Sand 4"),
                        M("Rock.png", 543, 596, 24, "MM Great Bay Coast Rock Sand 5", "Great Bay Coast Rock Sand 5"),
                        M("Rock.png", 466, 556, 24, "MM Great Bay Coast Rock Water 01", "Great Bay Coast Rock Water 01"),
                        M("Rock.png", 450, 527, 24, "MM Great Bay Coast Rock Water 02", "Great Bay Coast Rock Water 02"),
                        M("Rock.png", 230, 217, 24, "MM Great Bay Coast Rock Water 03", "Great Bay Coast Rock Water 03"),
                        M("Rock.png", 270, 217, 24, "MM Great Bay Coast Rock Water 04", "Great Bay Coast Rock Water 04"),
                        M("Rock.png", 322, 180, 24, "MM Great Bay Coast Rock Water 05", "Great Bay Coast Rock Water 05"),
                        M("Rock.png", 337, 309, 24, "MM Great Bay Coast Rock Water 06", "Great Bay Coast Rock Water 06"),
                        M("Rock.png", 295, 348, 24, "MM Great Bay Coast Rock Water 07", "Great Bay Coast Rock Water 07"),
                        M("Rock.png", 380, 410, 24, "MM Great Bay Coast Rock Water 08", "Great Bay Coast Rock Water 08"),
                        M("Rock.png", 342, 383, 24, "MM Great Bay Coast Rock Water 09", "Great Bay Coast Rock Water 09"),
                        M("Rock.png", 216, 284, 24, "MM Great Bay Coast Rock Water 10", "Great Bay Coast Rock Water 10"),
                        M("Rock.png", 226, 412, 24, "MM Great Bay Coast Rock Water 11", "Great Bay Coast Rock Water 11"),
                        M("Rock.png", 354, 209, 24, "MM Great Bay Coast Rock Water 12", "Great Bay Coast Rock Water 12"),
                        M("Rock.png", 437, 209, 24, "MM Great Bay Coast Rock Water 13", "Great Bay Coast Rock Water 13"),
                        M("Rock.png", 474, 209, 24, "MM Great Bay Coast Rock Water 14", "Great Bay Coast Rock Water 14"),
                        M("Rock.png", 533, 107, 24, "MM Great Bay Coast Rock Water 15", "Great Bay Coast Rock Water 15"),
                        M("Rock.png", 581, 171, 24, "MM Great Bay Coast Rock Water 16", "Great Bay Coast Rock Water 16"),
                        M("Rock.png", 490, 260, 24, "MM Great Bay Coast Rock Water 17", "Great Bay Coast Rock Water 17"),
                        M("Rock.png", 537, 298, 24, "MM Great Bay Coast Rock Water 18", "Great Bay Coast Rock Water 18"),
                        M("Rock.png", 277, 156, 24, "MM Great Bay Coast Rock Water 19", "Great Bay Coast Rock Water 19"),
                        M("Rock.png", 374, 82, 24, "MM Great Bay Coast Rock Water 20", "Great Bay Coast Rock Water 20"),
                        M("Rock.png", 446, 128, 24, "MM Great Bay Coast Rock Water 21", "Great Bay Coast Rock Water 21"),
                        M("Rock.png", 362, 494, 24, "MM Great Bay Coast Rock Water 22", "Great Bay Coast Rock Water 22"),
                        M("Rock.png", 403, 522, 24, "MM Great Bay Coast Rock Water 23", "Great Bay Coast Rock Water 23"),
                        M("Rock.png", 356, 578, 24, "MM Great Bay Coast Rock Water 24", "Great Bay Coast Rock Water 24"),
                        M("Rock.png", 421, 559, 24, "MM Great Bay Coast Rock Water 25", "Great Bay Coast Rock Water 25"),
                        M("Rock.png", 445, 582, 24, "MM Great Bay Coast Rock Water 26", "Great Bay Coast Rock Water 26"),
                        M("Rock.png", 277, 554, 24, "MM Great Bay Coast Rock Water 27", "Great Bay Coast Rock Water 27"),
                        M("Rock.png", 451, 303, 24, "MM Great Bay Coast Rock Water 28", "Great Bay Coast Rock Water 28"),
                        MA("Rock.png", 356, 538, 24, "MM Great Bay Coast Rock Water Cleared 1", "cleared", "Great Bay Coast Rock Water Cleared 1"),
                        MA("Rock.png", 411, 587, 24, "MM Great Bay Coast Rock Water Cleared 2", "cleared", "Great Bay Coast Rock Water Cleared 2"),
						
						ME("Entrance.png", 700, 495, "Entrance shuffle (Ocean Spider House)", "MM_SPIDER_HOUSE_OCEAN"),
						ME("Entrance.png", 528, 59, "Entrance shuffle (Pirate Fortress)", "MM_PIRATE_FORTRESS"),
						ME("Entrance.png", 784, 430, "Entrance shuffle (Termina Field)", "MM_TERMINA_FIELD_FROM_GREAT_BAY_COAST"),
						ME("Entrance.png", 552, 583, "Entrance shuffle (Zora Cape)", "MM_ZORA_CAPE_FROM_GREAT_BAY_COAST"),
						ME("Entrance.png", 173, 175, "Entrance shuffle (Pinnacle Rock)", "MM_PINNACLE_ROCK"),
						ME("Entrance.png", 673, 543, "Entrance shuffle (Fisherman Hut)", "MM_FISHER_HUT"),
						ME("Entrance.png", 270, 468, "Entrance shuffle (Laboratory)", "MM_LABORATORY"),
						ME("Entrance.png", 724, 157, "Entrance shuffle (Cow Grotto)", "MM_GROTTO_COW_COAST"),
						ME("Entrance.png", 702, 583, "Entrance shuffle (Open Grotto)", "MM_GROTTO_GENERIC_GREAT_BAY_COAST"),
						ME("Entrance.png", 164, 265, "Entrance shuffle (Great Bay Coast)", "MM_VOID_GREAT_BAY"),
						ME("Entrance.png", 200, 94, "Entrance shuffle (Great Bay Coast)", "MM_VOID_GREAT_BAY_BY_PINNACLE_ROCK")
                    }
                },
                new MapSubRegion
                {
                    Name = "Pinnacle Rock",
                    BackgroundImage = MM("Great_Bay_Coast", "Rock"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_PINNACLE_ROCK",
						"MM_VOID_PINNACLE_ROCK"
					},
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 243, 556, 40, "MM Pinnacle Rock Chest 1", "Pinnacle Rock Chest 1"),
                        M("Chest.png", 758, 292, 40, "MM Pinnacle Rock Chest 2", "Pinnacle Rock Chest 2"),
                        M("NPC.png", 287, 197, 40, "MM Pinnacle Rock HP", "Pinnacle Rock HP"),
                        M("Pot.png", 518, 431, 24, "MM Pinnacle Rock Pot 01", "Pinnacle Rock Pot 01"),
                        M("Pot.png", 578, 416, 24, "MM Pinnacle Rock Pot 02", "Pinnacle Rock Pot 02"),
                        M("Pot.png", 491, 464, 24, "MM Pinnacle Rock Pot 03", "Pinnacle Rock Pot 03"),
                        M("Pot.png", 266, 296, 24, "MM Pinnacle Rock Pot 04", "Pinnacle Rock Pot 04"),
                        M("Pot.png", 712, 144, 24, "MM Pinnacle Rock Pot 05", "Pinnacle Rock Pot 05"),
                        M("Pot.png", 619, 143, 24, "MM Pinnacle Rock Pot 06", "Pinnacle Rock Pot 06"),
                        M("Pot.png", 594, 156, 24, "MM Pinnacle Rock Pot 07", "Pinnacle Rock Pot 07"),
                        M("Pot.png", 572, 165, 24, "MM Pinnacle Rock Pot 08", "Pinnacle Rock Pot 08"),
                        M("Pot.png", 455, 430, 24, "MM Pinnacle Rock Pot 09", "Pinnacle Rock Pot 09"),
                        M("Pot.png", 399, 414, 24, "MM Pinnacle Rock Pot 10", "Pinnacle Rock Pot 10"),
                        M("Pot.png", 458, 396, 24, "MM Pinnacle Rock Pot 11", "Pinnacle Rock Pot 11"),
						
						ME("Entrance.png", 334, 4, "Entrance shuffle (Great Bay Coast)", "MM_GREAT_BAY_FROM_PINNACLE_ROCK"),
						ME("Entrance.png", 471, 4, "Entrance shuffle (Pinnacle Rock)", "MM_VOID_PINNACLE_ROCK")
                    }
                },
                new MapSubRegion
                {
                    Name = "Laboratory",
                    BackgroundImage = MM("Great_Bay_Coast", "Laboratory"),
                    DestinationEntranceIds = new List<string> { "MM_LABORATORY" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 608, 184, 40, "MM Laboratory Fish HP", "Laboratory Fish HP"),
                        M("NPC.png", 249, 422, 40, "MM Laboratory Zora Song", "Laboratory Zora Song"),
						
						ME("Entrance.png", 747, 299, "Entrance shuffle (Great Bay Coast)", "MM_GREAT_BAY_COAST_FROM_LABORATORY")
                    }
                },
                new MapSubRegion
                {
                    Name = "Fisherman Hut",
                    BackgroundImage = MM("Great_Bay_Coast", "Fisherman"),
                    DestinationEntranceIds = new List<string> { "MM_FISHER_HUT" },
                    Marks = new List<MapMark>
					{
						ME("Entrance.png", 188, 331, "Entrance shuffle (Great Bay Coast)", "MM_GREAT_BAY_COAST_FROM_FISHER_HUT")
					}
                },
                new MapSubRegion
                {
                    Name = "Cow Grotto",
                    BackgroundImage = MM("Grottos", "Cow"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_COW_COAST" },
                    Marks = new List<MapMark>
                    {
                        M("Butterfly.png", 127, 492, 24, "MM Great Bay Cow Grotto Butterfly 1", "Great Bay Cow Grotto Butterfly 1"),
                        M("Butterfly.png", 135, 461, 24, "MM Great Bay Cow Grotto Butterfly 2", "Great Bay Cow Grotto Butterfly 2"),
                        M("Butterfly.png", 160, 433, 24, "MM Great Bay Cow Grotto Butterfly 3", "Great Bay Cow Grotto Butterfly 3"),
                        M("Cow.png", 601, 348, 40, "MM Great Bay Coast Cow Back", "Great Bay Coast Cow Back"),
                        M("Cow.png", 577, 387, 40, "MM Great Bay Coast Cow Front", "Great Bay Coast Cow Front"),
                        M("Hive.png", 516, 60, 40, "MM Great Bay Cow Grotto Hive", "Great Bay Cow Grotto Hive"),
                        M("Grass.png", 549, 317, 24, "MM Great Bay Cow Grotto Grass 01", "Great Bay Cow Grotto Grass 01"),
                        M("Grass.png", 549, 301, 24, "MM Great Bay Cow Grotto Grass 02", "Great Bay Cow Grotto Grass 02"),
                        M("Grass.png", 569, 301, 24, "MM Great Bay Cow Grotto Grass 03", "Great Bay Cow Grotto Grass 03"),
                        M("Grass.png", 569, 317, 24, "MM Great Bay Cow Grotto Grass 04", "Great Bay Cow Grotto Grass 04"),
                        M("Grass.png", 559, 333, 24, "MM Great Bay Cow Grotto Grass 05", "Great Bay Cow Grotto Grass 05"),
                        M("Grass.png", 536, 325, 24, "MM Great Bay Cow Grotto Grass 06", "Great Bay Cow Grotto Grass 06"),
                        M("Grass.png", 530, 307, 24, "MM Great Bay Cow Grotto Grass 07", "Great Bay Cow Grotto Grass 07"),
                        M("Grass.png", 536, 293, 24, "MM Great Bay Cow Grotto Grass 08", "Great Bay Cow Grotto Grass 08"),
                        M("Grass.png", 559, 285, 24, "MM Great Bay Cow Grotto Grass 09", "Great Bay Cow Grotto Grass 09"),
                        M("Grass.png", 582, 293, 24, "MM Great Bay Cow Grotto Grass 10", "Great Bay Cow Grotto Grass 10"),
                        M("Grass.png", 588, 307, 24, "MM Great Bay Cow Grotto Grass 11", "Great Bay Cow Grotto Grass 11"),
                        M("Grass.png", 582, 325, 24, "MM Great Bay Cow Grotto Grass 12", "Great Bay Cow Grotto Grass 12"),
                        M("Grass.png", 408, 354, 24, "MM Great Bay Cow Grotto Grass 13", "Great Bay Cow Grotto Grass 13"),
                        M("Grass.png", 408, 338, 24, "MM Great Bay Cow Grotto Grass 14", "Great Bay Cow Grotto Grass 14"),
                        M("Grass.png", 428, 338, 24, "MM Great Bay Cow Grotto Grass 15", "Great Bay Cow Grotto Grass 15"),
                        M("Grass.png", 428, 354, 24, "MM Great Bay Cow Grotto Grass 16", "Great Bay Cow Grotto Grass 16"),
                        M("Grass.png", 418, 370, 24, "MM Great Bay Cow Grotto Grass 17", "Great Bay Cow Grotto Grass 17"),
                        M("Grass.png", 395, 362, 24, "MM Great Bay Cow Grotto Grass 18", "Great Bay Cow Grotto Grass 18"),
                        M("Grass.png", 389, 344, 24, "MM Great Bay Cow Grotto Grass 19", "Great Bay Cow Grotto Grass 19"),
                        M("Grass.png", 395, 330, 24, "MM Great Bay Cow Grotto Grass 20", "Great Bay Cow Grotto Grass 20"),
                        M("Grass.png", 418, 322, 24, "MM Great Bay Cow Grotto Grass 21", "Great Bay Cow Grotto Grass 21"),
                        M("Grass.png", 441, 330, 24, "MM Great Bay Cow Grotto Grass 22", "Great Bay Cow Grotto Grass 22"),
                        M("Grass.png", 447, 344, 24, "MM Great Bay Cow Grotto Grass 23", "Great Bay Cow Grotto Grass 23"),
                        M("Grass.png", 441, 362, 24, "MM Great Bay Cow Grotto Grass 24", "Great Bay Cow Grotto Grass 24"),
                        M("Grass.png", 499, 387, 24, "MM Great Bay Cow Grotto Grass 25", "Great Bay Cow Grotto Grass 25"),
                        M("Grass.png", 499, 371, 24, "MM Great Bay Cow Grotto Grass 26", "Great Bay Cow Grotto Grass 26"),
                        M("Grass.png", 519, 371, 24, "MM Great Bay Cow Grotto Grass 27", "Great Bay Cow Grotto Grass 27"),
                        M("Grass.png", 519, 387, 24, "MM Great Bay Cow Grotto Grass 28", "Great Bay Cow Grotto Grass 28"),
                        M("Grass.png", 509, 403, 24, "MM Great Bay Cow Grotto Grass 29", "Great Bay Cow Grotto Grass 29"),
                        M("Grass.png", 486, 395, 24, "MM Great Bay Cow Grotto Grass 30", "Great Bay Cow Grotto Grass 30"),
                        M("Grass.png", 480, 377, 24, "MM Great Bay Cow Grotto Grass 31", "Great Bay Cow Grotto Grass 31"),
                        M("Grass.png", 486, 363, 24, "MM Great Bay Cow Grotto Grass 32", "Great Bay Cow Grotto Grass 32"),
                        M("Grass.png", 509, 355, 24, "MM Great Bay Cow Grotto Grass 33", "Great Bay Cow Grotto Grass 33"),
                        M("Grass.png", 532, 363, 24, "MM Great Bay Cow Grotto Grass 34", "Great Bay Cow Grotto Grass 34"),
                        M("Grass.png", 538, 377, 24, "MM Great Bay Cow Grotto Grass 35", "Great Bay Cow Grotto Grass 35"),
                        M("Grass.png", 532, 395, 24, "MM Great Bay Cow Grotto Grass 36", "Great Bay Cow Grotto Grass 36"),
                        M("Grass.png", 679, 358, 24, "MM Great Bay Cow Grotto Grass 37", "Great Bay Cow Grotto Grass 37"),
                        M("Grass.png", 679, 342, 24, "MM Great Bay Cow Grotto Grass 38", "Great Bay Cow Grotto Grass 38"),
                        M("Grass.png", 699, 342, 24, "MM Great Bay Cow Grotto Grass 39", "Great Bay Cow Grotto Grass 39"),
                        M("Grass.png", 699, 358, 24, "MM Great Bay Cow Grotto Grass 40", "Great Bay Cow Grotto Grass 40"),
                        M("Grass.png", 689, 374, 24, "MM Great Bay Cow Grotto Grass 41", "Great Bay Cow Grotto Grass 41"),
                        M("Grass.png", 666, 366, 24, "MM Great Bay Cow Grotto Grass 42", "Great Bay Cow Grotto Grass 42"),
                        M("Grass.png", 660, 348, 24, "MM Great Bay Cow Grotto Grass 43", "Great Bay Cow Grotto Grass 43"),
                        M("Grass.png", 666, 334, 24, "MM Great Bay Cow Grotto Grass 44", "Great Bay Cow Grotto Grass 44"),
                        M("Grass.png", 689, 326, 24, "MM Great Bay Cow Grotto Grass 45", "Great Bay Cow Grotto Grass 45"),
                        M("Grass.png", 712, 334, 24, "MM Great Bay Cow Grotto Grass 46", "Great Bay Cow Grotto Grass 46"),
                        M("Grass.png", 718, 348, 24, "MM Great Bay Cow Grotto Grass 47", "Great Bay Cow Grotto Grass 47"),
                        M("Grass.png", 712, 366, 24, "MM Great Bay Cow Grotto Grass 48", "Great Bay Cow Grotto Grass 48"),
                        M("Grass.png", 655, 448, 24, "MM Great Bay Cow Grotto Grass 49", "Great Bay Cow Grotto Grass 49"),
                        M("Grass.png", 655, 432, 24, "MM Great Bay Cow Grotto Grass 50", "Great Bay Cow Grotto Grass 50"),
                        M("Grass.png", 675, 432, 24, "MM Great Bay Cow Grotto Grass 51", "Great Bay Cow Grotto Grass 51"),
                        M("Grass.png", 675, 448, 24, "MM Great Bay Cow Grotto Grass 52", "Great Bay Cow Grotto Grass 52"),
                        M("Grass.png", 665, 464, 24, "MM Great Bay Cow Grotto Grass 53", "Great Bay Cow Grotto Grass 53"),
                        M("Grass.png", 642, 456, 24, "MM Great Bay Cow Grotto Grass 54", "Great Bay Cow Grotto Grass 54"),
                        M("Grass.png", 636, 438, 24, "MM Great Bay Cow Grotto Grass 55", "Great Bay Cow Grotto Grass 55"),
                        M("Grass.png", 642, 424, 24, "MM Great Bay Cow Grotto Grass 56", "Great Bay Cow Grotto Grass 56"),
                        M("Grass.png", 665, 416, 24, "MM Great Bay Cow Grotto Grass 57", "Great Bay Cow Grotto Grass 57"),
                        M("Grass.png", 688, 424, 24, "MM Great Bay Cow Grotto Grass 58", "Great Bay Cow Grotto Grass 58"),
                        M("Grass.png", 694, 438, 24, "MM Great Bay Cow Grotto Grass 59", "Great Bay Cow Grotto Grass 59"),
                        M("Grass.png", 688, 456, 24, "MM Great Bay Cow Grotto Grass 60", "Great Bay Cow Grotto Grass 60"),
                        M("Grass.png", 489, 489, 24, "MM Great Bay Cow Grotto Grass 61", "Great Bay Cow Grotto Grass 61"),
                        M("Grass.png", 489, 473, 24, "MM Great Bay Cow Grotto Grass 62", "Great Bay Cow Grotto Grass 62"),
                        M("Grass.png", 509, 473, 24, "MM Great Bay Cow Grotto Grass 63", "Great Bay Cow Grotto Grass 63"),
                        M("Grass.png", 509, 489, 24, "MM Great Bay Cow Grotto Grass 64", "Great Bay Cow Grotto Grass 64"),
                        M("Grass.png", 499, 505, 24, "MM Great Bay Cow Grotto Grass 65", "Great Bay Cow Grotto Grass 65"),
                        M("Grass.png", 476, 497, 24, "MM Great Bay Cow Grotto Grass 66", "Great Bay Cow Grotto Grass 66"),
                        M("Grass.png", 470, 479, 24, "MM Great Bay Cow Grotto Grass 67", "Great Bay Cow Grotto Grass 67"),
                        M("Grass.png", 476, 465, 24, "MM Great Bay Cow Grotto Grass 68", "Great Bay Cow Grotto Grass 68"),
                        M("Grass.png", 499, 457, 24, "MM Great Bay Cow Grotto Grass 69", "Great Bay Cow Grotto Grass 69"),
                        M("Grass.png", 522, 465, 24, "MM Great Bay Cow Grotto Grass 70", "Great Bay Cow Grotto Grass 70"),
                        M("Grass.png", 528, 479, 24, "MM Great Bay Cow Grotto Grass 71", "Great Bay Cow Grotto Grass 71"),
                        M("Grass.png", 522, 497, 24, "MM Great Bay Cow Grotto Grass 72", "Great Bay Cow Grotto Grass 72"),
						
						ME("Entrance.png", 220, 458, "Entrance shuffle (Great Bay Coast)", "MM_GROTTO_EXIT_COW_COAST")
                    }
                },
                new MapSubRegion
                {
                    Name = "Open Grotto",
                    BackgroundImage = MM("Grottos", "Generic"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GENERIC_GREAT_BAY_COAST" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 208, 394, 40, "MM Great Bay Coast Fisherman Grotto", "Great Bay Coast Fisherman Grotto"),
                        M("Grass.png", 605, 531, 24, "MM Great Bay Coast Fisherman Grotto Grass 01", "Great Bay Coast Fisherman Grotto Grass 01"),
                        M("Grass.png", 363, 479, 24, "MM Great Bay Coast Fisherman Grotto Grass 02", "Great Bay Coast Fisherman Grotto Grass 02"),
                        M("Grass.png", 270, 451, 24, "MM Great Bay Coast Fisherman Grotto Grass 03", "Great Bay Coast Fisherman Grotto Grass 03"),
                        M("Grass.png", 330, 467, 24, "MM Great Bay Coast Fisherman Grotto Grass 04", "Great Bay Coast Fisherman Grotto Grass 04"),
                        M("Grass.png", 290, 433, 24, "MM Great Bay Coast Fisherman Grotto Grass 05", "Great Bay Coast Fisherman Grotto Grass 05"),
                        M("Grass.png", 255, 427, 24, "MM Great Bay Coast Fisherman Grotto Grass 06", "Great Bay Coast Fisherman Grotto Grass 06"),
                        M("Grass.png", 689, 480, 24, "MM Great Bay Coast Fisherman Grotto Grass 07", "Great Bay Coast Fisherman Grotto Grass 07"),
                        M("Grass.png", 383, 417, 24, "MM Great Bay Coast Fisherman Grotto Grass 08", "Great Bay Coast Fisherman Grotto Grass 08"),
                        M("Grass.png", 557, 439, 24, "MM Great Bay Coast Fisherman Grotto Grass 09", "Great Bay Coast Fisherman Grotto Grass 09"),
                        M("Grass.png", 642, 449, 24, "MM Great Bay Coast Fisherman Grotto Grass 10", "Great Bay Coast Fisherman Grotto Grass 10"),
                        M("Grass.png", 245, 375, 24, "MM Great Bay Coast Fisherman Grotto Grass 11", "Great Bay Coast Fisherman Grotto Grass 11"),
                        M("Grass.png", 375, 397, 24, "MM Great Bay Coast Fisherman Grotto Grass 12", "Great Bay Coast Fisherman Grotto Grass 12"),
                        M("Grass.png", 587, 432, 24, "MM Great Bay Coast Fisherman Grotto Grass 13", "Great Bay Coast Fisherman Grotto Grass 13"),
                        M("Grass.png", 408, 402, 24, "MM Great Bay Coast Fisherman Grotto Grass 14", "Great Bay Coast Fisherman Grotto Grass 14"),
						
						ME("Entrance.png", 621, 483, "Entrance shuffle (Great Bay Coast)", "MM_GROTTO_EXIT_GENERIC_GREAT_BAY_COAST")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion IkanaCanyon()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Ikana Canyon";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Ikana Canyon",
                    BackgroundImage = MM("Ikana_Canyon", "Ikana_Canyon"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_IKANA_CANYON_FROM_CAVERN",
						"MM_IKANA_VALLEY_FROM_ROAD",
						"MM_GROTTO_EXIT_GENERIC_VALLEY",
						"MM_IKANA_CANYON_FROM_WELL",
						"MM_IKANA_VALLEY_FROM_SHRINE",
						"MM_IKANA_CANYON_FROM_FAIRY_FOUNTAIN",
						"MM_IKANA_CANYON_FROM_GHOST_HUT",
						"MM_IKANA_CANYON_FROM_MUSIC_BOX_HOUSE",
						"MM_WARP_OWL_IKANA_CANYON",
						"MM_IKANA_CANYON_FROM_CASTLE_GARDENS",
						"MM_IKANA_CANYON_FROM_STONE_TOWER",
						"MM_IKANA_CANYON_FROM_SAKON_HIDEOUT"
					},
                    Marks = new List<MapMark>
                    {
                        M("Grass.png", 423, 404, 24, "MM Ikana Canyon Grass 1", "Ikana Canyon Grass 1"),
                        M("Grass.png", 388, 391, 24, "MM Ikana Canyon Grass 2", "Ikana Canyon Grass 2"),
                        M("Grass.png", 401, 410, 24, "MM Ikana Canyon Grass 3", "Ikana Canyon Grass 3"),
                        M("Grass.png", 384, 470, 24, "MM Ikana Canyon Grass 4", "Ikana Canyon Grass 4"),
                        M("NPC.png", 365, 438, 40, "MM Ikana Canyon Owl Statue", "Ikana Canyon Owl Statue"),
						M("NPC.png", 334, 483, 40, "MM Tingle Map Ikana", "Tingle Map Ikana"),
						M("NPC.png", 334, 443, 40, "MM Tingle Map Clock Town", "Tingle Map Clock Town"),
                        M("Collectible.png", 778, 187, 40, "MM Ikana Valley Scrub HP", "Ikana Valley Scrub HP"),
                        M("Scrub.png", 756, 259, 40, "MM Ikana Valley Scrub Rupee", "Ikana Valley Scrub Rupee"),
                        M("Shop.png", 727, 248, 40, "MM Ikana Valley Scrub Shop", "Ikana Valley Scrub Shop"),
						
						ME("Entrance.png", 193, 469, "Entrance shuffle (Beneath The Well)", "MM_BENEATH_THE_WELL"),
						ME("Entrance.png", 327, 579, "Entrance shuffle (Secret Shrine)", "MM_SECRET_SHRINE"),
						ME("Entrance.png", 653, 549, "Entrance shuffle (Road to Ikana)", "MM_IKANA_ROAD_FROM_VALLEY"),
						ME("Entrance.png", 409, 218, "Entrance shuffle (Ikana Castle Gardens)", "MM_IKANA_CASTLE_GARDENS"),
						ME("Entrance.png", 271, 42, "Entrance shuffle (Stone Tower)", "MM_STONE_TOWER"),
						ME("Entrance.png", 84, 267, "Entrance shuffle (Ghost Hut)", "MM_GHOST_HUT"),
						ME("Entrance.png", 254, 270, "Entrance shuffle (Music Box House)", "MM_MUSIC_BOX_HOUSE"),
						ME("Entrance.png", 314, 163, "Entrance shuffle (Fairy Fountain)", "MM_FAIRY_FOUNTAIN_IKANA"),
						ME("Entrance.png", 90, 143, "Entrance shuffle (Spring Water Cave)", "MM_IKANA_CAVERN"),
						ME("Entrance.png", 765, 312, "Entrance shuffle (Sakon Hideout)", "MM_SAKON_HIDEOUT"),
						ME("Entrance.png", 309, 537, "Entrance shuffle (Open Grotto)", "MM_GROTTO_GENERIC_VALLEY"),
						ME("Entrance.png", 856, 298, "Entrance shuffle (Southern Swamp)", "MM_SWAMP_FROM_IKANA_CANYON")
                    }
                },
                new MapSubRegion
                {
                    Name = "Ghost Hut",
                    BackgroundImage = MM("Ikana_Canyon", "Ghost_Hut"),
                    DestinationEntranceIds = new List<string> { "MM_GHOST_HUT" },
                    Marks = new List<MapMark> 
                    { 
                        M("NPC.png", 454, 146, 40, "MM Ghost Hut HP", "Ghost Hut HP"),
						
						ME("Entrance.png", 453, 567, "Entrance shuffle (Ikana Canyon)", "MM_IKANA_CANYON_FROM_GHOST_HUT")
                    }
                },
                new MapSubRegion
                {
                    Name = "Music Box House",
                    BackgroundImage = MM("Ikana_Canyon", "Music_Box_House"),
                    DestinationEntranceIds = new List<string> { "MM_MUSIC_BOX_HOUSE" },
                    Marks = new List<MapMark> 
                    { 
                        M("NPC.png", 677, 420, 40, "MM Music Box House Gibdo Mask", "Music Box House Gibdo Mask"),
						
						ME("Entrance.png", 706, 239, "Entrance shuffle (Ikana Canyon)", "MM_IKANA_CANYON_FROM_MUSIC_BOX_HOUSE")
                    }
                },
                new MapSubRegion
                {
                    Name = "Spring Water Cave",
                    BackgroundImage = MM("Ikana_Canyon", "Cave"),
                    DestinationEntranceIds = new List<string> { "MM_IKANA_CAVERN" },
                    Marks = new List<MapMark>
                    {
                        ME("Entrance.png", 579, 519, "Entrance shuffle (Ikana Canyon)", "MM_IKANA_CANYON_FROM_CAVERN")
                    }
                },
                new MapSubRegion
                {
                    Name = "Sakon Hideout",
                    BackgroundImage = MM("Ikana_Canyon", "Sakon_Hideout"),
                    DestinationEntranceIds = new List<string> { "MM_SAKON_HIDEOUT" },
                    Marks = new List<MapMark>
                    {
                        M("Pot.png", 519, 241, 24, "MM Sakon Hideout Pot First Room 1", "Sakon Hideout Pot First Room 1"),
                        M("Pot.png", 459, 241, 24, "MM Sakon Hideout Pot First Room 2", "Sakon Hideout Pot First Room 2"),
                        M("Pot.png", 619, 241, 24, "MM Sakon Hideout Pot Second Room 1", "Sakon Hideout Pot Second Room 1"),
                        M("Pot.png", 559, 241, 24, "MM Sakon Hideout Pot Second Room 2", "Sakon Hideout Pot Second Room 2"),
                        M("Pot.png", 687, 241, 24, "MM Sakon Hideout Pot Third Room", "Sakon Hideout Pot Third Room"),
						
						ME("Entrance.png", 170, 314, "Entrance shuffle (Ikana Canyon)", "MM_IKANA_CANYON_FROM_SAKON_HIDEOUT")
                    }
                },
                new MapSubRegion
                {
                    Name = "Open Grotto",
                    BackgroundImage = MM("Grottos", "Generic"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GENERIC_VALLEY" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 208, 394, 40, "MM Ikana Valley Grotto", "Ikana Valley Grotto"),
                        M("Grass.png", 605, 531, 24, "MM Ikana Valley Grotto Grass 01", "Ikana Valley Grotto Grass 01"),
                        M("Grass.png", 363, 479, 24, "MM Ikana Valley Grotto Grass 02", "Ikana Valley Grotto Grass 02"),
                        M("Grass.png", 270, 451, 24, "MM Ikana Valley Grotto Grass 03", "Ikana Valley Grotto Grass 03"),
                        M("Grass.png", 330, 467, 24, "MM Ikana Valley Grotto Grass 04", "Ikana Valley Grotto Grass 04"),
                        M("Grass.png", 290, 433, 24, "MM Ikana Valley Grotto Grass 05", "Ikana Valley Grotto Grass 05"),
                        M("Grass.png", 255, 427, 24, "MM Ikana Valley Grotto Grass 06", "Ikana Valley Grotto Grass 06"),
                        M("Grass.png", 689, 480, 24, "MM Ikana Valley Grotto Grass 07", "Ikana Valley Grotto Grass 07"),
                        M("Grass.png", 383, 417, 24, "MM Ikana Valley Grotto Grass 08", "Ikana Valley Grotto Grass 08"),
                        M("Grass.png", 557, 439, 24, "MM Ikana Valley Grotto Grass 09", "Ikana Valley Grotto Grass 09"),
                        M("Grass.png", 642, 449, 24, "MM Ikana Valley Grotto Grass 10", "Ikana Valley Grotto Grass 10"),
                        M("Grass.png", 245, 375, 24, "MM Ikana Valley Grotto Grass 11", "Ikana Valley Grotto Grass 11"),
                        M("Grass.png", 375, 397, 24, "MM Ikana Valley Grotto Grass 12", "Ikana Valley Grotto Grass 12"),
                        M("Grass.png", 587, 432, 24, "MM Ikana Valley Grotto Grass 13", "Ikana Valley Grotto Grass 13"),
                        M("Grass.png", 408, 402, 24, "MM Ikana Valley Grotto Grass 14", "Ikana Valley Grotto Grass 14"),
						
						ME("Entrance.png", 621, 483, "Entrance shuffle (Ikana Canyon)", "MM_GROTTO_EXIT_GENERIC_VALLEY")
                    }
                },
                new MapSubRegion
                {
                    Name = "Fairy Fountain",
                    BackgroundImage = MM("Ikana_Canyon", "Fairy"),
                    DestinationEntranceIds = new List<string> { "MM_FAIRY_FOUNTAIN_IKANA" },
                    Marks = new List<MapMark> 
                    { 
                        M("NPC.png", 440, 171, 40, "MM Ikana Great Fairy", "Ikana Great Fairy"),
						
                        ME("Entrance.png", 433, 544, "Entrance shuffle (Ikana Canyon)", "MM_IKANA_CANYON_FROM_FAIRY_FOUNTAIN")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion IkanaGraveyard()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Ikana Graveyard";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Ikana Graveyard",
                    BackgroundImage = MM("Graveyard", "Graveyard"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_IKANA_GRAVEYARD",
						"MM_GROTTO_EXIT_GENERIC_GRAVEYARD",
						"MM_GRAVE_EXIT_NIGHT1",
						"MM_GRAVE_EXIT_NIGHT2",
						"MM_GRAVE_EXIT_NIGHT3"
					},
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 580, 197, 40, "MM Ikana Graveyard Captain Mask", "Ikana Graveyard Captain Mask"),
                        M("Grass.png", 298, 341, 24, "MM Ikana Graveyard Grass 1", "Ikana Graveyard Grass 1"),
                        M("Grass.png", 280, 323, 24, "MM Ikana Graveyard Grass 2", "Ikana Graveyard Grass 2"),
                        M("Grass.png", 222, 249, 24, "MM Ikana Graveyard Grass 3", "Ikana Graveyard Grass 3"),
                        M("Grass.png", 203, 261, 24, "MM Ikana Graveyard Grass 4", "Ikana Graveyard Grass 4"),
                        M("Grass.png", 249, 318, 24, "MM Ikana Graveyard Grass 5", "Ikana Graveyard Grass 5"),
                        M("Grass.png", 513, 191, 24, "MM Ikana Graveyard Grass Pack Grass 1", "Ikana Graveyard Grass Pack Grass 1"),
                        M("Grass.png", 513, 211, 24, "MM Ikana Graveyard Grass Pack Grass 2", "Ikana Graveyard Grass Pack Grass 2"),
                        M("Grass.png", 504, 201, 24, "MM Ikana Graveyard Grass Pack Grass 3", "Ikana Graveyard Grass Pack Grass 3"),
                        M("Grass.png", 495, 191, 24, "MM Ikana Graveyard Grass Pack Grass 4", "Ikana Graveyard Grass Pack Grass 4"),
                        M("Grass.png", 504, 181, 24, "MM Ikana Graveyard Grass Pack Grass 5", "Ikana Graveyard Grass Pack Grass 5"),
                        M("Grass.png", 513, 171, 24, "MM Ikana Graveyard Grass Pack Grass 6", "Ikana Graveyard Grass Pack Grass 6"),
                        M("Grass.png", 522, 181, 24, "MM Ikana Graveyard Grass Pack Grass 7", "Ikana Graveyard Grass Pack Grass 7"),
                        M("Grass.png", 531, 191, 24, "MM Ikana Graveyard Grass Pack Grass 8", "Ikana Graveyard Grass Pack Grass 8"),
                        M("Grass.png", 522, 201, 24, "MM Ikana Graveyard Grass Pack Grass 9", "Ikana Graveyard Grass Pack Grass 9"),
                        M("Rock.png", 492, 290, 24, "MM Ikana Graveyard Rock Circle Rock 1", "Ikana Graveyard Rock Circle Rock 1"),
                        M("Rock.png", 498, 314, 24, "MM Ikana Graveyard Rock Circle Rock 2", "Ikana Graveyard Rock Circle Rock 2"),
                        M("Rock.png", 520, 324, 24, "MM Ikana Graveyard Rock Circle Rock 3", "Ikana Graveyard Rock Circle Rock 3"),
                        M("Rock.png", 545, 321, 24, "MM Ikana Graveyard Rock Circle Rock 4", "Ikana Graveyard Rock Circle Rock 4"),
                        M("Rock.png", 554, 298, 24, "MM Ikana Graveyard Rock Circle Rock 5", "Ikana Graveyard Rock Circle Rock 5"),
                        M("Rock.png", 548, 272, 24, "MM Ikana Graveyard Rock Circle Rock 6", "Ikana Graveyard Rock Circle Rock 6"),
                        M("Rock.png", 526, 264, 24, "MM Ikana Graveyard Rock Circle Rock 7", "Ikana Graveyard Rock Circle Rock 7"),
                        M("Rock.png", 502, 267, 24, "MM Ikana Graveyard Rock Circle Rock 8", "Ikana Graveyard Rock Circle Rock 8"),
                        M("Rock.png", 693, 185, 24, "MM Ikana Graveyard Rock Wall 1", "Ikana Graveyard Rock Wall 1"),
                        M("Rock.png", 669, 210, 24, "MM Ikana Graveyard Rock Wall 2", "Ikana Graveyard Rock Wall 2"),
                        M("Rock.png", 709, 201, 24, "MM Ikana Graveyard Rock Wall 3", "Ikana Graveyard Rock Wall 3"),
                        M("Rock.png", 728, 204, 24, "MM Ikana Graveyard Rock Wall 4", "Ikana Graveyard Rock Wall 4"),
                        M("Rock.png", 676, 194, 24, "MM Ikana Graveyard Rock Wall 5", "Ikana Graveyard Rock Wall 5"),
                        M("Wonder.png", 286, 253, 24, "MM Ikana Graveyard Wonder Item 01", "Ikana Graveyard Wonder Item 01"),
                        M("Wonder.png", 260, 229, 24, "MM Ikana Graveyard Wonder Item 02", "Ikana Graveyard Wonder Item 02"),
                        M("Wonder.png", 280, 234, 24, "MM Ikana Graveyard Wonder Item 03", "Ikana Graveyard Wonder Item 03"),
                        M("Wonder.png", 172, 349, 24, "MM Ikana Graveyard Wonder Item 04", "Ikana Graveyard Wonder Item 04"),
                        M("Wonder.png", 155, 339, 24, "MM Ikana Graveyard Wonder Item 05", "Ikana Graveyard Wonder Item 05"),
                        M("Wonder.png", 172, 329, 24, "MM Ikana Graveyard Wonder Item 06", "Ikana Graveyard Wonder Item 06"),
                        M("Wonder.png", 285, 377, 24, "MM Ikana Graveyard Wonder Item 07", "Ikana Graveyard Wonder Item 07"),
                        M("Wonder.png", 305, 372, 24, "MM Ikana Graveyard Wonder Item 08", "Ikana Graveyard Wonder Item 08"),
                        M("Wonder.png", 265, 372, 24, "MM Ikana Graveyard Wonder Item 09", "Ikana Graveyard Wonder Item 09"),
                        M("Wonder.png", 560, 219, 24, "MM Ikana Graveyard Wonder Item 10", "Ikana Graveyard Wonder Item 10"),
                        M("Wonder.png", 550, 205, 24, "MM Ikana Graveyard Wonder Item 11", "Ikana Graveyard Wonder Item 11"),
                        M("Wonder.png", 559, 188, 24, "MM Ikana Graveyard Wonder Item 12", "Ikana Graveyard Wonder Item 12"),
						
						ME("Entrance.png", 101, 310, "Entrance shuffle (Road to Ikana)", "MM_IKANA_ROAD_FROM_IKANA_GRAVEYARD"),
						ME("Entrance.png", 202, 338, "Entrance shuffle (Beneath The Graveyard Night 1)", "MM_GRAVE_NIGHT1"),
						ME("Entrance.png", 261, 331, "Entrance shuffle (Beneath The Graveyard Night 2)", "MM_GRAVE_NIGHT2"),
						ME("Entrance.png", 252, 253, "Entrance shuffle (Dampe House)", "MM_GRAVE_NIGHT3"),
						ME("Entrance.png", 516, 285, "Entrance shuffle (Generic Grotto)", "MM_GROTTO_GENERIC_GRAVEYARD")
                    }
                },
                new MapSubRegion
                {
                    Name = "Beneath the Graveyard",
                    BackgroundImage = MM("Graveyard", "Beneath_Graveyard"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_GRAVE_NIGHT2",
						"MM_GRAVE_NIGHT1"
					},
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 586, 458, 40, "MM Beneath The Graveyard Chest", "Beneath The Graveyard Chest"),
                        M("Chest.png", 93, 314, 40, "MM Beneath The Graveyard HP", "Beneath The Graveyard HP"),
                        M("NPC.png", 874, 473, 40, "MM Beneath The Graveyard Song of Storms", "Beneath The Graveyard Song of Storms"),
                        M("Pot.png", 537, 441, 24, "MM Beneath The Graveyard Pot Night 1 Bats 1", "Beneath The Graveyard Pot Night 1 Bats 1"),
                        M("Pot.png", 537, 484, 24, "MM Beneath The Graveyard Pot Night 1 Bats 2", "Beneath The Graveyard Pot Night 1 Bats 2"),
                        M("Pot.png", 637, 480, 24, "MM Beneath The Graveyard Pot Night 1 Bats 3", "Beneath The Graveyard Pot Night 1 Bats 3"),
                        M("Pot.png", 418, 443, 24, "MM Beneath The Graveyard Pot Night 1 Early 1", "Beneath The Graveyard Pot Night 1 Early 1"),
                        M("Pot.png", 426, 472, 24, "MM Beneath The Graveyard Pot Night 1 Early 2", "Beneath The Graveyard Pot Night 1 Early 2"),
                        M("Pot.png", 111, 71, 24, "MM Beneath The Graveyard Pot Night 2 After Pit 1", "Beneath The Graveyard Pot Night 2 After Pit 1"),
                        M("Pot.png", 133, 71, 24, "MM Beneath The Graveyard Pot Night 2 After Pit 2", "Beneath The Graveyard Pot Night 2 After Pit 2"),
                        M("Pot.png", 102, 176, 24, "MM Beneath The Graveyard Pot Night 2 After Pit 3", "Beneath The Graveyard Pot Night 2 After Pit 3"),
                        M("Pot.png", 124, 176, 24, "MM Beneath The Graveyard Pot Night 2 After Pit 4", "Beneath The Graveyard Pot Night 2 After Pit 4"),
                        M("Pot.png", 319, 103, 24, "MM Beneath The Graveyard Pot Night 2 Before Pit 1", "Beneath The Graveyard Pot Night 2 Before Pit 1"),
                        M("Pot.png", 336, 119, 24, "MM Beneath The Graveyard Pot Night 2 Before Pit 2", "Beneath The Graveyard Pot Night 2 Before Pit 2"),
                        M("Pot.png", 312, 320, 24, "MM Beneath The Graveyard Pot Night 2 Early", "Beneath The Graveyard Pot Night 2 Early"),
                        M("Rupee.png", 333, 309, 24, "MM Beneath The Graveyard Rupee 1", "Beneath The Graveyard Rupee 1"),
                        M("Rupee.png", 351, 296, 24, "MM Beneath The Graveyard Rupee 2", "Beneath The Graveyard Rupee 2"),
                        M("Rupee.png", 351, 272, 24, "MM Beneath The Graveyard Rupee 3", "Beneath The Graveyard Rupee 3"),
                        M("Rupee.png", 333, 261, 24, "MM Beneath The Graveyard Rupee 4", "Beneath The Graveyard Rupee 4"),
                        M("Rupee.png", 315, 272, 24, "MM Beneath The Graveyard Rupee 5", "Beneath The Graveyard Rupee 5"),
                        M("Rupee.png", 315, 296, 24, "MM Beneath The Graveyard Rupee 6", "Beneath The Graveyard Rupee 6"),
                        M("Rupee.png", 333, 285, 24, "MM Beneath The Graveyard Rupee 7", "Beneath The Graveyard Rupee 7"),
						
						ME("Entrance.png", 313, 457, "Entrance shuffle (Ikana Graveyard)", "MM_GRAVE_EXIT_NIGHT1"),
						ME("Entrance.png", 333, 360, "Entrance shuffle (Ikana Graveyard)", "MM_GRAVE_EXIT_NIGHT2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Dampe House",
                    BackgroundImage = MM("Graveyard", "Dampe"),
                    DestinationEntranceIds = new List<string> { "MM_GRAVE_NIGHT3" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 255, 292, 40, "MM Beneath The Graveyard Dampe Chest", "Beneath The Graveyard Dampe Chest"),
                        M("Pot.png", 125, 331, 24, "MM Beneath The Graveyard Pot Dampe 01", "Beneath The Graveyard Pot Dampe 01"),
                        M("Pot.png", 125, 272, 24, "MM Beneath The Graveyard Pot Dampe 02", "Beneath The Graveyard Pot Dampe 02"),
                        M("Pot.png", 166, 331, 24, "MM Beneath The Graveyard Pot Dampe 03", "Beneath The Graveyard Pot Dampe 03"),
                        M("Pot.png", 166, 272, 24, "MM Beneath The Graveyard Pot Dampe 04", "Beneath The Graveyard Pot Dampe 04"),
                        M("Pot.png", 207, 331, 24, "MM Beneath The Graveyard Pot Dampe 05", "Beneath The Graveyard Pot Dampe 05"),
                        M("Pot.png", 207, 272, 24, "MM Beneath The Graveyard Pot Dampe 06", "Beneath The Graveyard Pot Dampe 06"),
                        M("Pot.png", 807, 339, 24, "MM Beneath The Graveyard Pot Dampe 07", "Beneath The Graveyard Pot Dampe 07"),
                        M("Pot.png", 807, 254, 24, "MM Beneath The Graveyard Pot Dampe 08", "Beneath The Graveyard Pot Dampe 08"),
                        M("Pot.png", 757, 339, 24, "MM Beneath The Graveyard Pot Dampe 09", "Beneath The Graveyard Pot Dampe 09"),
                        M("Pot.png", 757, 254, 24, "MM Beneath The Graveyard Pot Dampe 10", "Beneath The Graveyard Pot Dampe 10"),
						
						ME("Entrance.png", 832, 287, "Entrance shuffle (Ikana Graveyard)", "MM_GRAVE_EXIT_NIGHT3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Generic Grotto",
                    BackgroundImage = MM("Grottos", "Generic"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GENERIC_GRAVEYARD" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 208, 394, 40, "MM Ikana Graveyard Grotto", "Ikana Graveyard Grotto"),
                        M("Grass.png", 605, 531, 24, "MM Ikana Graveyard Grotto Grass 01", "Ikana Graveyard Grotto Grass 01"),
                        M("Grass.png", 363, 479, 24, "MM Ikana Graveyard Grotto Grass 02", "Ikana Graveyard Grotto Grass 02"),
                        M("Grass.png", 270, 451, 24, "MM Ikana Graveyard Grotto Grass 03", "Ikana Graveyard Grotto Grass 03"),
                        M("Grass.png", 330, 467, 24, "MM Ikana Graveyard Grotto Grass 04", "Ikana Graveyard Grotto Grass 04"),
                        M("Grass.png", 290, 433, 24, "MM Ikana Graveyard Grotto Grass 05", "Ikana Graveyard Grotto Grass 05"),
                        M("Grass.png", 255, 427, 24, "MM Ikana Graveyard Grotto Grass 06", "Ikana Graveyard Grotto Grass 06"),
                        M("Grass.png", 689, 480, 24, "MM Ikana Graveyard Grotto Grass 07", "Ikana Graveyard Grotto Grass 07"),
                        M("Grass.png", 383, 417, 24, "MM Ikana Graveyard Grotto Grass 08", "Ikana Graveyard Grotto Grass 08"),
                        M("Grass.png", 557, 439, 24, "MM Ikana Graveyard Grotto Grass 09", "Ikana Graveyard Grotto Grass 09"),
                        M("Grass.png", 642, 449, 24, "MM Ikana Graveyard Grotto Grass 10", "Ikana Graveyard Grotto Grass 10"),
                        M("Grass.png", 245, 375, 24, "MM Ikana Graveyard Grotto Grass 11", "Ikana Graveyard Grotto Grass 11"),
                        M("Grass.png", 375, 397, 24, "MM Ikana Graveyard Grotto Grass 12", "Ikana Graveyard Grotto Grass 12"),
                        M("Grass.png", 587, 432, 24, "MM Ikana Graveyard Grotto Grass 13", "Ikana Graveyard Grotto Grass 13"),
                        M("Grass.png", 408, 402, 24, "MM Ikana Graveyard Grotto Grass 14", "Ikana Graveyard Grotto Grass 14"),
						
						ME("Entrance.png", 621, 483, "Entrance shuffle (Ikana Graveyard)", "MM_GROTTO_EXIT_GENERIC_GRAVEYARD")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion MilkRoad()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Milk Road";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Milk Road",
                    BackgroundImage = MM("Milk_Road", "Milk_Road"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_WARP_OWL_MILK_ROAD",
						"MM_MILK_ROAD_FROM_ROMANI_RANCH",
						"MM_MILK_ROAD_FROM_FIELD",
						"MM_MILK_ROAD_FROM_GORMAN_TRACK",
						"MM_MILK_ROAD_FROM_GORMAN_BACK"
					},
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 376, 195, 40, "MM Milk Road Owl Statue", "Milk Road Owl Statue"),
						M("NPC.png", 491, 219, 40, "MM Tingle Map Ranch", "Tingle Map Ranch"),
						M("NPC.png", 531, 219, 40, "MM Tingle Map Great Bay", "Tingle Map Great Bay"),
                        M("Grass.png", 396, 248, 24, "MM Milk Road Grass 1", "Milk Road Grass 1"),
                        M("Grass.png", 440, 239, 24, "MM Milk Road Grass 2", "Milk Road Grass 2"),
                        M("Grass.png", 407, 230, 24, "MM Milk Road Grass 3", "Milk Road Grass 3"),
                        M("Grass.png", 625, 364, 24, "MM Milk Road Keaton Grass Reward 1", "Milk Road Keaton Grass Reward 1"),
                        M("Grass.png", 625, 388, 24, "MM Milk Road Keaton Grass Reward 2", "Milk Road Keaton Grass Reward 2"),
                        M("Grass.png", 609, 380, 24, "MM Milk Road Keaton Grass Reward 3", "Milk Road Keaton Grass Reward 3"),
                        M("Grass.png", 601, 364, 24, "MM Milk Road Keaton Grass Reward 4", "Milk Road Keaton Grass Reward 4"),
                        M("Grass.png", 609, 348, 24, "MM Milk Road Keaton Grass Reward 5", "Milk Road Keaton Grass Reward 5"),
                        M("Grass.png", 625, 340, 24, "MM Milk Road Keaton Grass Reward 6", "Milk Road Keaton Grass Reward 6"),
                        M("Grass.png", 641, 348, 24, "MM Milk Road Keaton Grass Reward 7", "Milk Road Keaton Grass Reward 7"),
                        M("Grass.png", 649, 364, 24, "MM Milk Road Keaton Grass Reward 8", "Milk Road Keaton Grass Reward 8"),
                        M("Grass.png", 641, 380, 24, "MM Milk Road Keaton Grass Reward 9", "Milk Road Keaton Grass Reward 9"),
						M("NPC.png", 690, 377, 40, "MM Clock Town Keaton HP", "Clock Town Keaton HP"),
						
						ME("Entrance.png", 112, 371, "Entrance shuffle (Termina Field)", "MM_TERMINA_FIELD_FROM_MILK_ROAD"),
						ME("Entrance.png", 892, 280, "Entrance shuffle (Romani Ranch)", "MM_ROMANI_RANCH_FROM_MILK_ROAD"),
						ME("Entrance.png", 236, 43, "Entrance shuffle (Gorman Track)", "MM_GORMAN_TRACK_FROM_MILK_ROAD"),
						ME("Entrance.png", 758, 43, "Entrance shuffle (Gorman Track)", "MM_GORMAN_BACK_FROM_MILK_ROAD")
                    }
                },
                new MapSubRegion
                {
                    Name = "Gorman Track",
                    BackgroundImage = MM("Milk_Road", "Gorman_Track"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_GORMAN_TRACK_FROM_MILK_ROAD",
						"MM_GORMAN_BACK_FROM_MILK_ROAD"
					},
                    Marks = new List<MapMark>
                    {
                        M("Crate.png", 633, 240, 24, "MM Gorman Track Crate", "Gorman Track Crate"),
                        M("NPC.png", 610, 201, 40, "MM Gorman Track Garo Mask", "Gorman Track Garo Mask"),
                        M("NPC.png", 655, 207, 40, "MM Gorman Track Milk Purchase", "Gorman Track Milk Purchase"),
                        M("Grass.png", 69, 387, 24, "MM Gorman Track Grass Pack 1 Grass 01", "Gorman Track Grass Pack 1 Grass 01"),
                        M("Grass.png", 69, 371, 24, "MM Gorman Track Grass Pack 1 Grass 02", "Gorman Track Grass Pack 1 Grass 02"),
                        M("Grass.png", 85, 371, 24, "MM Gorman Track Grass Pack 1 Grass 03", "Gorman Track Grass Pack 1 Grass 03"),
                        M("Grass.png", 85, 387, 24, "MM Gorman Track Grass Pack 1 Grass 04", "Gorman Track Grass Pack 1 Grass 04"),
                        M("Grass.png", 77, 407, 24, "MM Gorman Track Grass Pack 1 Grass 05", "Gorman Track Grass Pack 1 Grass 05"),
                        M("Grass.png", 57, 399, 24, "MM Gorman Track Grass Pack 1 Grass 06", "Gorman Track Grass Pack 1 Grass 06"),
                        M("Grass.png", 49, 377, 24, "MM Gorman Track Grass Pack 1 Grass 07", "Gorman Track Grass Pack 1 Grass 07"),
                        M("Grass.png", 57, 359, 24, "MM Gorman Track Grass Pack 1 Grass 08", "Gorman Track Grass Pack 1 Grass 08"),
                        M("Grass.png", 77, 351, 24, "MM Gorman Track Grass Pack 1 Grass 09", "Gorman Track Grass Pack 1 Grass 09"),
                        M("Grass.png", 97, 359, 24, "MM Gorman Track Grass Pack 1 Grass 10", "Gorman Track Grass Pack 1 Grass 10"),
                        M("Grass.png", 105, 377, 24, "MM Gorman Track Grass Pack 1 Grass 11", "Gorman Track Grass Pack 1 Grass 11"),
                        M("Grass.png", 97, 399, 24, "MM Gorman Track Grass Pack 1 Grass 12", "Gorman Track Grass Pack 1 Grass 12"),
                        M("Grass.png", 160, 424, 24, "MM Gorman Track Grass Pack 2 Grass 01", "Gorman Track Grass Pack 2 Grass 01"),
                        M("Grass.png", 160, 408, 24, "MM Gorman Track Grass Pack 2 Grass 02", "Gorman Track Grass Pack 2 Grass 02"),
                        M("Grass.png", 176, 408, 24, "MM Gorman Track Grass Pack 2 Grass 03", "Gorman Track Grass Pack 2 Grass 03"),
                        M("Grass.png", 176, 424, 24, "MM Gorman Track Grass Pack 2 Grass 04", "Gorman Track Grass Pack 2 Grass 04"),
                        M("Grass.png", 168, 444, 24, "MM Gorman Track Grass Pack 2 Grass 05", "Gorman Track Grass Pack 2 Grass 05"),
                        M("Grass.png", 148, 436, 24, "MM Gorman Track Grass Pack 2 Grass 06", "Gorman Track Grass Pack 2 Grass 06"),
                        M("Grass.png", 140, 414, 24, "MM Gorman Track Grass Pack 2 Grass 07", "Gorman Track Grass Pack 2 Grass 07"),
                        M("Grass.png", 148, 396, 24, "MM Gorman Track Grass Pack 2 Grass 08", "Gorman Track Grass Pack 2 Grass 08"),
                        M("Grass.png", 168, 388, 24, "MM Gorman Track Grass Pack 2 Grass 09", "Gorman Track Grass Pack 2 Grass 09"),
                        M("Grass.png", 188, 396, 24, "MM Gorman Track Grass Pack 2 Grass 10", "Gorman Track Grass Pack 2 Grass 10"),
                        M("Grass.png", 196, 414, 24, "MM Gorman Track Grass Pack 2 Grass 11", "Gorman Track Grass Pack 2 Grass 11"),
                        M("Grass.png", 188, 436, 24, "MM Gorman Track Grass Pack 2 Grass 12", "Gorman Track Grass Pack 2 Grass 12"),
                        M("Tree.png", 97, 87, 24, "MM Gorman Track Tree 01", "Gorman Track Tree 01"),
                        M("Tree.png", 95, 72, 24, "MM Gorman Track Tree 02", "Gorman Track Tree 02"),
                        M("Tree.png", 125, 78, 24, "MM Gorman Track Tree 03", "Gorman Track Tree 03"),
                        M("Tree.png", 125, 61, 24, "MM Gorman Track Tree 04", "Gorman Track Tree 04"),
                        M("Tree.png", 154, 72, 24, "MM Gorman Track Tree 05", "Gorman Track Tree 05"),
                        M("Tree.png", 209, 55, 24, "MM Gorman Track Tree 06", "Gorman Track Tree 06"),
                        M("Tree.png", 157, 62, 24, "MM Gorman Track Tree 07", "Gorman Track Tree 07"),
                        M("Tree.png", 157, 51, 24, "MM Gorman Track Tree 08", "Gorman Track Tree 08"),
                        M("Tree.png", 176, 62, 24, "MM Gorman Track Tree 09", "Gorman Track Tree 09"),
                        M("Tree.png", 193, 45, 24, "MM Gorman Track Tree 10", "Gorman Track Tree 10"),
                        M("Tree.png", 221, 36, 24, "MM Gorman Track Tree 11", "Gorman Track Tree 11"),
                        M("Tree.png", 239, 48, 24, "MM Gorman Track Tree 12", "Gorman Track Tree 12"),
                        M("Tree.png", 817, 527, 24, "MM Gorman Track Tree 13", "Gorman Track Tree 13"),
                        M("Tree.png", 726, 542, 24, "MM Gorman Track Tree 14", "Gorman Track Tree 14"),
                        M("Tree.png", 645, 526, 24, "MM Gorman Track Tree 15", "Gorman Track Tree 15"),
                        M("Tree.png", 480, 515, 24, "MM Gorman Track Tree 16", "Gorman Track Tree 16"),
                        M("Tree.png", 351, 509, 24, "MM Gorman Track Tree 17", "Gorman Track Tree 17"),
                        M("Tree.png", 843, 469, 24, "MM Gorman Track Tree 18", "Gorman Track Tree 18"),
                        M("Tree.png", 691, 579, 24, "MM Gorman Track Tree 19", "Gorman Track Tree 19"),
                        M("Tree.png", 619, 567, 24, "MM Gorman Track Tree 20", "Gorman Track Tree 20"),
                        M("Tree.png", 566, 502, 24, "MM Gorman Track Tree 21", "Gorman Track Tree 21"),
                        M("Tree.png", 498, 537, 24, "MM Gorman Track Tree 22", "Gorman Track Tree 22"),
                        M("Tree.png", 399, 481, 24, "MM Gorman Track Tree 23", "Gorman Track Tree 23"),
                        M("Tree.png", 372, 549, 24, "MM Gorman Track Tree 24", "Gorman Track Tree 24"),
                        M("Tree.png", 295, 475, 24, "MM Gorman Track Tree 25", "Gorman Track Tree 25"),
						
						ME("Entrance.png", 694, 161, "Entrance shuffle (Milk Road)", "MM_MILK_ROAD_FROM_GORMAN_TRACK"),
						ME("Entrance.png", 314, 0, "Entrance shuffle (Milk Road)", "MM_MILK_ROAD_FROM_GORMAN_BACK")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion MountainVillage()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Mountain Village";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Mountain Village",
                    BackgroundImage = MM("Mountain_Village", "Village"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_MOUNTAIN_VILLAGE_FROM_BLACKSMITH",
						"MM_WARP_OWL_MOUNTAIN_VILLAGE",
						"MM_MOUNTAIN_VILLAGE_FROM_PATH",
						"MM_GROTTO_EXIT_GENERIC_MOUNTAIN_VILLAGE",
						"MM_MOUNTAIN_VILLAGE_FROM_GORON_GRAVEYARD",
						"MM_MOUNTAIN_VILLAGE_FROM_SNOWHEAD_PATH",
						"MM_MOUNTAIN_VILLAGE_FROM_TWIN_ISLANDS"
					},
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 325, 304, 40, "MM Mountain Village Owl Statue", "Mountain Village Owl Statue"),
                        MA("NPC.png", 225, 359, 40, "MM Mountain Village Don Gero Mask", "cursed", "Mountain Village Don Gero Mask"),
                        MA("NPC.png", 307, 411, 40, "MM Mountain Village Frog Choir HP", "cleared", "Mountain Village Frog Choir HP"),
                        M("Pot.png", 557, 359, 24, "MM Mountain Village Pot", "Mountain Village Pot"),
                        M("Pot.png", 477, 398, 24, "MM Mountain Village Potted Plant 1 Pot", "Mountain Village Potted Plant 1 Pot"),
                        M("Pot.png", 479, 420, 24, "MM Mountain Village Potted Plant 2 Pot", "Mountain Village Potted Plant 2 Pot"),
                        M("Grass.png", 493, 398, 24, "MM Mountain Village Potted Plant 1 Grass", "Mountain Village Potted Plant 1 Grass"),
                        M("Grass.png", 495, 420, 24, "MM Mountain Village Potted Plant 2 Grass", "Mountain Village Potted Plant 2 Grass"),
                        MA("Grass.png", 407, 523, 24, "MM Mountain Village Grass 1", "cleared", "Mountain Village Grass 1"),
                        MA("Grass.png", 423, 456, 24, "MM Mountain Village Grass 2", "cleared", "Mountain Village Grass 2"),
                        MA("Grass.png", 355, 419, 24, "MM Mountain Village Grass 3", "cleared", "Mountain Village Grass 3"),
                        MA("Grass.png", 494, 344, 24, "MM Mountain Village Grass Pack 1 Grass 1", "cleared", "Mountain Village Grass Pack 1 Grass 1"),
                        MA("Grass.png", 494, 364, 24, "MM Mountain Village Grass Pack 1 Grass 2", "cleared", "Mountain Village Grass Pack 1 Grass 2"),
                        MA("Grass.png", 485, 354, 24, "MM Mountain Village Grass Pack 1 Grass 3", "cleared", "Mountain Village Grass Pack 1 Grass 3"),
                        MA("Grass.png", 476, 344, 24, "MM Mountain Village Grass Pack 1 Grass 4", "cleared", "Mountain Village Grass Pack 1 Grass 4"),
                        MA("Grass.png", 485, 334, 24, "MM Mountain Village Grass Pack 1 Grass 5", "cleared", "Mountain Village Grass Pack 1 Grass 5"),
                        MA("Grass.png", 494, 324, 24, "MM Mountain Village Grass Pack 1 Grass 6", "cleared", "Mountain Village Grass Pack 1 Grass 6"),
                        MA("Grass.png", 503, 334, 24, "MM Mountain Village Grass Pack 1 Grass 7", "cleared", "Mountain Village Grass Pack 1 Grass 7"),
                        MA("Grass.png", 512, 344, 24, "MM Mountain Village Grass Pack 1 Grass 8", "cleared", "Mountain Village Grass Pack 1 Grass 8"),
                        MA("Grass.png", 503, 354, 24, "MM Mountain Village Grass Pack 1 Grass 9", "cleared", "Mountain Village Grass Pack 1 Grass 9"),
                        MA("Grass.png", 439, 329, 24, "MM Mountain Village Grass Pack 2 Grass 1", "cleared", "Mountain Village Grass Pack 2 Grass 1"),
                        MA("Grass.png", 439, 349, 24, "MM Mountain Village Grass Pack 2 Grass 2", "cleared", "Mountain Village Grass Pack 2 Grass 2"),
                        MA("Grass.png", 430, 339, 24, "MM Mountain Village Grass Pack 2 Grass 3", "cleared", "Mountain Village Grass Pack 2 Grass 3"),
                        MA("Grass.png", 421, 329, 24, "MM Mountain Village Grass Pack 2 Grass 4", "cleared", "Mountain Village Grass Pack 2 Grass 4"),
                        MA("Grass.png", 430, 319, 24, "MM Mountain Village Grass Pack 2 Grass 5", "cleared", "Mountain Village Grass Pack 2 Grass 5"),
                        MA("Grass.png", 439, 309, 24, "MM Mountain Village Grass Pack 2 Grass 6", "cleared", "Mountain Village Grass Pack 2 Grass 6"),
                        MA("Grass.png", 448, 319, 24, "MM Mountain Village Grass Pack 2 Grass 7", "cleared", "Mountain Village Grass Pack 2 Grass 7"),
                        MA("Grass.png", 457, 329, 24, "MM Mountain Village Grass Pack 2 Grass 8", "cleared", "Mountain Village Grass Pack 2 Grass 8"),
                        MA("Grass.png", 448, 339, 24, "MM Mountain Village Grass Pack 2 Grass 9", "cleared", "Mountain Village Grass Pack 2 Grass 9"),
                        MA("Grass.png", 560, 312, 24, "MM Mountain Village Grass Pack 3 Grass 1", "cleared", "Mountain Village Grass Pack 3 Grass 1"),
                        MA("Grass.png", 560, 332, 24, "MM Mountain Village Grass Pack 3 Grass 2", "cleared", "Mountain Village Grass Pack 3 Grass 2"),
                        MA("Grass.png", 551, 322, 24, "MM Mountain Village Grass Pack 3 Grass 3", "cleared", "Mountain Village Grass Pack 3 Grass 3"),
                        MA("Grass.png", 542, 312, 24, "MM Mountain Village Grass Pack 3 Grass 4", "cleared", "Mountain Village Grass Pack 3 Grass 4"),
                        MA("Grass.png", 551, 302, 24, "MM Mountain Village Grass Pack 3 Grass 5", "cleared", "Mountain Village Grass Pack 3 Grass 5"),
                        MA("Grass.png", 560, 292, 24, "MM Mountain Village Grass Pack 3 Grass 6", "cleared", "Mountain Village Grass Pack 3 Grass 6"),
                        MA("Grass.png", 569, 302, 24, "MM Mountain Village Grass Pack 3 Grass 7", "cleared", "Mountain Village Grass Pack 3 Grass 7"),
                        MA("Grass.png", 578, 312, 24, "MM Mountain Village Grass Pack 3 Grass 8", "cleared", "Mountain Village Grass Pack 3 Grass 8"),
                        MA("Grass.png", 569, 322, 24, "MM Mountain Village Grass Pack 3 Grass 9", "cleared", "Mountain Village Grass Pack 3 Grass 9"),
                        MA("Grass.png", 612, 472, 24, "MM Mountain Village Keaton Grass Reward 1", "cleared", "Mountain Village Keaton Grass Reward 1"),
                        MA("Grass.png", 612, 492, 24, "MM Mountain Village Keaton Grass Reward 2", "cleared", "Mountain Village Keaton Grass Reward 2"),
                        MA("Grass.png", 603, 482, 24, "MM Mountain Village Keaton Grass Reward 3", "cleared", "Mountain Village Keaton Grass Reward 3"),
                        MA("Grass.png", 594, 472, 24, "MM Mountain Village Keaton Grass Reward 4", "cleared", "Mountain Village Keaton Grass Reward 4"),
                        MA("Grass.png", 603, 462, 24, "MM Mountain Village Keaton Grass Reward 5", "cleared", "Mountain Village Keaton Grass Reward 5"),
                        MA("Grass.png", 612, 452, 24, "MM Mountain Village Keaton Grass Reward 6", "cleared", "Mountain Village Keaton Grass Reward 6"),
                        MA("Grass.png", 621, 462, 24, "MM Mountain Village Keaton Grass Reward 7", "cleared", "Mountain Village Keaton Grass Reward 7"),
                        MA("Grass.png", 630, 472, 24, "MM Mountain Village Keaton Grass Reward 8", "cleared", "Mountain Village Keaton Grass Reward 8"),
                        MA("Grass.png", 621, 482, 24, "MM Mountain Village Keaton Grass Reward 9", "cleared", "Mountain Village Keaton Grass Reward 9"),
                        MA("Butterfly.png", 479, 311, 24, "MM Mountain Village Butterfly Pack 1 Butterfly 1", "cleared", "Mountain Village Butterfly Pack 1 Butterfly 1"),
                        MA("Butterfly.png", 504, 307, 24, "MM Mountain Village Butterfly Pack 1 Butterfly 2", "cleared", "Mountain Village Butterfly Pack 1 Butterfly 2"),
                        MA("Butterfly.png", 456, 294, 24, "MM Mountain Village Butterfly Pack 1 Butterfly 3", "cleared", "Mountain Village Butterfly Pack 1 Butterfly 3"),
                        MA("Butterfly.png", 482, 285, 24, "MM Mountain Village Butterfly Pack 1 Butterfly 4", "cleared", "Mountain Village Butterfly Pack 1 Butterfly 4"),
                        MA("Butterfly.png", 424, 504, 24, "MM Mountain Village Butterfly Pack 2 Butterfly 1", "cleared", "Mountain Village Butterfly Pack 2 Butterfly 1"),
                        MA("Butterfly.png", 398, 504, 24, "MM Mountain Village Butterfly Pack 2 Butterfly 2", "cleared", "Mountain Village Butterfly Pack 2 Butterfly 2"),
                        MA("Butterfly.png", 434, 484, 24, "MM Mountain Village Butterfly Pack 2 Butterfly 3", "cleared", "Mountain Village Butterfly Pack 2 Butterfly 3"),
                        MA("Butterfly.png", 388, 484, 24, "MM Mountain Village Butterfly Pack 2 Butterfly 4", "cleared", "Mountain Village Butterfly Pack 2 Butterfly 4"),
                        MA("Butterfly.png", 411, 475, 24, "MM Mountain Village Butterfly Pack 2 Butterfly 5", "cleared", "Mountain Village Butterfly Pack 2 Butterfly 5"),
                        MA("Hive.png", 652, 351, 40, "MM Mountain Village Hive", "cleared", "Mountain Village Hive"),
                        MA("Rock.png", 470, 36, 24, "MM Mountain Village Rock Cliff 1", "cleared", "Mountain Village Rock Cliff 1"),
                        MA("Rock.png", 538, 36, 24, "MM Mountain Village Rock Cliff 2", "cleared", "Mountain Village Rock Cliff 2"),
                        MA("Rock.png", 623, 425, 24, "MM Mountain Village Rock Ground 1", "cleared", "Mountain Village Rock Ground 1"),
                        MA("Rock.png", 602, 415, 24, "MM Mountain Village Rock Ground 2", "cleared", "Mountain Village Rock Ground 2"),
                        MA("Rock.png", 581, 405, 24, "MM Mountain Village Rock Ground 3", "cleared", "Mountain Village Rock Ground 3"),
                        MA("Rock.png", 595, 387, 24, "MM Mountain Village Rock Ground 4", "cleared", "Mountain Village Rock Ground 4"),
                        MA("Rock.png", 609, 369, 24, "MM Mountain Village Rock Ground 5", "cleared", "Mountain Village Rock Ground 5"),
                        MA("Rupee.png", 450, 414, 24, "MM Mountain Village Rupee", "cleared", "Mountain Village Rupee"),
                        MA("Red_Boulder.png", 459, 423, 24, "MM Mountain Village Spring Red Boulder 1", "cleared", "Mountain Village Spring Red Boulder 1"),
                        MA("Red_Boulder.png", 454, 402, 24, "MM Mountain Village Spring Red Boulder 2", "cleared", "Mountain Village Spring Red Boulder 2"),
                        MA("Red_Boulder.png", 438, 417, 24, "MM Mountain Village Spring Red Boulder 3", "cleared", "Mountain Village Spring Red Boulder 3"),
                        MA("Chest.png", 201, 374, 40, "MM Mountain Village Waterfall Chest", "cleared", "Mountain Village Waterfall Chest"),
                        M("Snowball.png", 304, 346, 24, "MM Mountain Village Small Snowball 1", "Mountain Village Small Snowball 1"),
                        M("Snowball.png", 312, 318, 24, "MM Mountain Village Small Snowball 2", "Mountain Village Small Snowball 2"),
                        M("Snowball.png", 343, 336, 24, "MM Mountain Village Small Snowball 3", "Mountain Village Small Snowball 3"),
                        M("Snowball.png", 354, 309, 24, "MM Mountain Village Small Snowball 4", "Mountain Village Small Snowball 4"),
                        M("Snowball.png", 386, 323, 24, "MM Mountain Village Small Snowball 5", "Mountain Village Small Snowball 5"),
                        MA("Snowball.png", 470, 36, 24, "MM Mountain Village Small Snowball Cliff 1", "cursed", "Mountain Village Small Snowball Cliff 1"),
                        MA("Snowball.png", 538, 36, 24, "MM Mountain Village Small Snowball Cliff 2", "cursed", "Mountain Village Small Snowball Cliff 2"),
                        MA("Snowball.png", 607, 308, 24, "MM Mountain Village Small Snowball Winter 1", "cursed", "Mountain Village Small Snowball Winter 1"),
                        MA("Snowball.png", 581, 405, 24, "MM Mountain Village Small Snowball Winter 2", "cursed", "Mountain Village Small Snowball Winter 2"),
                        MA("Snowball.png", 729, 374, 24, "MM Mountain Village Small Snowball Winter 3", "cursed", "Mountain Village Small Snowball Winter 3"),
                        MA("Snowball.png", 579, 312, 24, "MM Mountain Village Small Snowball Winter 4", "cursed", "Mountain Village Small Snowball Winter 4"),
                        MA("Snowball.png", 258, 441, 24, "MM Mountain Village Big Snowball 1", "cursed", "Mountain Village Big Snowball 1"),
                        MA("Snowball.png", 279, 350, 24, "MM Mountain Village Big Snowball 2", "cursed", "Mountain Village Big Snowball 2"),
                        MA("Snowball.png", 416, 308, 24, "MM Mountain Village Big Snowball 3", "cursed", "Mountain Village Big Snowball 3"),
                        MA("Snowball.png", 725, 404, 24, "MM Mountain Village Big Snowball 4", "cursed", "Mountain Village Big Snowball 4"),
                        MA("Snowball.png", 792, 390, 24, "MM Mountain Village Big Snowball 5", "cursed", "Mountain Village Big Snowball 5"),
						MA("NPC.png", 542, 489, 40, "MM Clock Town Keaton HP", "cleared", "Clock Town Keaton HP"),
						
						ME("Entrance.png", 837, 393, "Entrance shuffle (Twin Islands)", "MM_TWIN_ISLANDS_FROM_MOUNTAIN_VILLAGE"),
						ME("Entrance.png", 419, 573, "Entrance shuffle (Path to Mountain Village)", "MM_PATH_FROM_MOUNTAIN_VILLAGE"),
						ME("Entrance.png", 260, 200, "Entrance shuffle (Path to Snowhead)", "MM_SNOWHEAD_PATH_FROM_MOUNTAIN_VILLAGE"),
						ME("Entrance.png", 514, 388, "Entrance shuffle (Blacksmith)", "MM_BLACKSMITH"),
						ME("Entrance.png", 494, 3, "Entrance shuffle (Goron Graveyard)", "MM_GORON_GRAVEYARD"),
						MEA("Entrance.png", 724, 148, "Entrance shuffle (Generic Grotto)", "cleared", "MM_GROTTO_GENERIC_MOUNTAIN_VILLAGE")
                    }
                },
                new MapSubRegion
                {
                    Name = "Blacksmith",
                    BackgroundImage = MM("Mountain_Village", "Blacksmith"),
                    DestinationEntranceIds = new List<string> { "MM_BLACKSMITH" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 455, 290, 40, "MM Blacksmith Razor Blade", "Blacksmith Razor Blade"),
                        M("NPC.png", 455, 340, 40, "MM Blacksmith Gilded Sword", "Blacksmith Gilded Sword"),
                        M("Pot.png", 676, 302, 24, "MM Blacksmith Potted Plant 1 Pot", "Blacksmith Potted Plant 1 Pot"),
                        M("Pot.png", 676, 375, 24, "MM Blacksmith Potted Plant 2 Pot", "Blacksmith Potted Plant 2 Pot"),
                        M("Grass.png", 676, 278, 24, "MM Blacksmith Potted Plant 1 Grass", "Blacksmith Potted Plant 1 Grass"),
                        M("Grass.png", 676, 351, 24, "MM Blacksmith Potted Plant 2 Grass", "Blacksmith Potted Plant 2 Grass"),
						
						ME("Entrance.png", 721, 318, "Entrance shuffle (Mountain Village)", "MM_MOUNTAIN_VILLAGE_FROM_BLACKSMITH")
                    }
                },
                new MapSubRegion
                {
                    Name = "Goron Graveyard",
                    BackgroundImage = MM("Mountain_Village", "Graveyard"),
                    DestinationEntranceIds = new List<string> { "MM_GORON_GRAVEYARD" },
                    Marks = new List<MapMark>
					{
						M("NPC.png", 455, 271, 40, "MM Goron Graveyard Mask", "Goron Graveyard Mask"),
						ME("Entrance.png", 472, 558, "Entrance shuffle (Mountain Village)", "MM_MOUNTAIN_VILLAGE_FROM_GORON_GRAVEYARD")
					}
                },
                new MapSubRegion
                {
                    Name = "Generic Grotto",
                    BackgroundImage = MM("Grottos", "Generic"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GENERIC_MOUNTAIN_VILLAGE" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 208, 394, 40, "MM Mountain Village Tunnel Grotto", "Mountain Village Tunnel Grotto"),
                        M("Grass.png", 605, 531, 24, "MM Mountain Village Tunnel Grotto Grass 01", "Mountain Village Tunnel Grotto Grass 01"),
                        M("Grass.png", 363, 479, 24, "MM Mountain Village Tunnel Grotto Grass 02", "Mountain Village Tunnel Grotto Grass 02"),
                        M("Grass.png", 270, 451, 24, "MM Mountain Village Tunnel Grotto Grass 03", "Mountain Village Tunnel Grotto Grass 03"),
                        M("Grass.png", 330, 467, 24, "MM Mountain Village Tunnel Grotto Grass 04", "Mountain Village Tunnel Grotto Grass 04"),
                        M("Grass.png", 290, 433, 24, "MM Mountain Village Tunnel Grotto Grass 05", "Mountain Village Tunnel Grotto Grass 05"),
                        M("Grass.png", 255, 427, 24, "MM Mountain Village Tunnel Grotto Grass 06", "Mountain Village Tunnel Grotto Grass 06"),
                        M("Grass.png", 689, 480, 24, "MM Mountain Village Tunnel Grotto Grass 07", "Mountain Village Tunnel Grotto Grass 07"),
                        M("Grass.png", 383, 417, 24, "MM Mountain Village Tunnel Grotto Grass 08", "Mountain Village Tunnel Grotto Grass 08"),
                        M("Grass.png", 557, 439, 24, "MM Mountain Village Tunnel Grotto Grass 09", "Mountain Village Tunnel Grotto Grass 09"),
                        M("Grass.png", 642, 449, 24, "MM Mountain Village Tunnel Grotto Grass 10", "Mountain Village Tunnel Grotto Grass 10"),
                        M("Grass.png", 245, 375, 24, "MM Mountain Village Tunnel Grotto Grass 11", "Mountain Village Tunnel Grotto Grass 11"),
                        M("Grass.png", 375, 397, 24, "MM Mountain Village Tunnel Grotto Grass 12", "Mountain Village Tunnel Grotto Grass 12"),
                        M("Grass.png", 587, 432, 24, "MM Mountain Village Tunnel Grotto Grass 13", "Mountain Village Tunnel Grotto Grass 13"),
                        M("Grass.png", 408, 402, 24, "MM Mountain Village Tunnel Grotto Grass 14", "Mountain Village Tunnel Grotto Grass 14"),
						
						ME("Entrance.png", 621, 483, "Entrance shuffle (Mountain Village)", "MM_GROTTO_EXIT_GENERIC_MOUNTAIN_VILLAGE")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion PathtoMountainVillage()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Path to Mountain Village";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Path to Mountain Village",
                    BackgroundImage = MM("Path_to_Mountain_Village", "Path"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_PATH_FROM_MOUNTAIN_VILLAGE",
						"MM_MOUNTAIN_VILLAGE_PATH_FROM_TERMINA_FIELD"
					},
                    Marks = new List<MapMark>
                    {
                        M("Tree.png", 595, 174, 24, "MM Path to Mountain Village Snow Tree Lower 1", "Path to Mountain Village Snow Tree Lower 1"),
                        M("Tree.png", 418, 340, 24, "MM Path to Mountain Village Snow Tree Lower 2", "Path to Mountain Village Snow Tree Lower 2"),
                        M("Tree.png", 254, 364, 24, "MM Path to Mountain Village Snow Tree Upper 1", "Path to Mountain Village Snow Tree Upper 1"),
                        M("Tree.png", 147, 299, 24, "MM Path to Mountain Village Snow Tree Upper 2", "Path to Mountain Village Snow Tree Upper 2"),
                        MA("Snowball.png", 629, 164, 24, "MM Path to Mountain Village Big Snowball Lower 1", "cursed", "Path to Mountain Village Big Snowball Lower 1"),
                        MA("Snowball.png", 628, 90, 24, "MM Path to Mountain Village Big Snowball Lower 2", "cursed", "Path to Mountain Village Big Snowball Lower 2"),
                        MA("Snowball.png", 504, 331, 24, "MM Path to Mountain Village Big Snowball Lower 3", "cursed", "Path to Mountain Village Big Snowball Lower 3"),
                        MA("Snowball.png", 451, 412, 24, "MM Path to Mountain Village Big Snowball Lower 4", "cursed", "Path to Mountain Village Big Snowball Lower 4"),
                        MA("Snowball.png", 354, 399, 24, "MM Path to Mountain Village Big Snowball Middle 1", "cursed", "Path to Mountain Village Big Snowball Middle 1"),
                        MA("Snowball.png", 355, 373, 24, "MM Path to Mountain Village Big Snowball Middle 2", "cursed", "Path to Mountain Village Big Snowball Middle 2"),
                        MA("Snowball.png", 339, 356, 24, "MM Path to Mountain Village Big Snowball Middle 3", "cursed", "Path to Mountain Village Big Snowball Middle 3"),
                        MA("Snowball.png", 338, 331, 24, "MM Path to Mountain Village Big Snowball Middle 4", "cursed", "Path to Mountain Village Big Snowball Middle 4"),
                        MA("Snowball.png", 192, 206, 24, "MM Path to Mountain Village Big Snowball Upper 1", "cursed", "Path to Mountain Village Big Snowball Upper 1"),
                        MA("Snowball.png", 167, 200, 24, "MM Path to Mountain Village Big Snowball Upper 2", "cursed", "Path to Mountain Village Big Snowball Upper 2"),
                        MA("Snowball.png", 142, 200, 24, "MM Path to Mountain Village Big Snowball Upper 3", "cursed", "Path to Mountain Village Big Snowball Upper 3"),
                        MA("Snowball.png", 582, 127, 24, "MM Path to Mountain Village Small Snowball Lower Winter 1", "cursed", "Path to Mountain Village Small Snowball Lower Winter 1"),
                        MA("Snowball.png", 458, 325, 24, "MM Path to Mountain Village Small Snowball Lower Winter 2", "cursed", "Path to Mountain Village Small Snowball Lower Winter 2"),
                        MA("Snowball.png", 492, 379, 24, "MM Path to Mountain Village Small Snowball Lower Winter 3", "cursed", "Path to Mountain Village Small Snowball Lower Winter 3"),
                        MA("Snowball.png", 176, 270, 24, "MM Path to Mountain Village Small Snowball Upper Winter", "cursed", "Path to Mountain Village Small Snowball Upper Winter"),
                        MA("Snowball.png", 628, 90, 24, "MM Path to Mountain Village Small Snowball Lower Spring 1", "cleared", "Path to Mountain Village Small Snowball Lower Spring 1"),
                        MA("Snowball.png", 619, 165, 24, "MM Path to Mountain Village Small Snowball Lower Spring 2", "cleared", "Path to Mountain Village Small Snowball Lower Spring 2"),
                        MA("Snowball.png", 503, 337, 24, "MM Path to Mountain Village Small Snowball Lower Spring 3", "cleared", "Path to Mountain Village Small Snowball Lower Spring 3"),
                        MA("Snowball.png", 451, 412, 24, "MM Path to Mountain Village Small Snowball Lower Spring 4", "cleared", "Path to Mountain Village Small Snowball Lower Spring 4"),
                        MA("Snowball.png", 192, 206, 24, "MM Path to Mountain Village Small Snowball Upper Spring 1", "cleared", "Path to Mountain Village Small Snowball Upper Spring 1"),
                        MA("Snowball.png", 167, 200, 24, "MM Path to Mountain Village Small Snowball Upper Spring 2", "cleared", "Path to Mountain Village Small Snowball Upper Spring 2"),
                        MA("Snowball.png", 142, 200, 24, "MM Path to Mountain Village Small Snowball Upper Spring 3", "cleared", "Path to Mountain Village Small Snowball Upper Spring 3"),
						
						ME("Entrance.png", 60, 297, "Entrance shuffle (Mountain Village)", "MM_MOUNTAIN_VILLAGE_FROM_PATH"),
						ME("Entrance.png", 795, 223, "Entrance shuffle (Termina Field)", "MM_TERMINA_FIELD_FROM_PATH_TO_MOUNTAIN_VILLAGE")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion PathtoSnowhead()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Path to Snowhead";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Path to Snowhead",
                    BackgroundImage = MM("Path_to_Snowhead", "Path"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_GROTTO_EXIT_GENERIC_PATH_SNOWHEAD",
						"MM_SNOWHEAD_PATH_FROM_MOUNTAIN_VILLAGE",
						"MM_PATH_SNOWHEAD_FROM_SNOWHEAD"
					},
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 404, 332, 40, "MM Path to Snowhead HP", "Path to Snowhead HP"),
                        M("Tree.png", 87, 235, 24, "MM Path to Snowhead Snow Tree Entrance", "Path to Snowhead Snow Tree Entrance"),
                        M("Tree.png", 858, 94, 24, "MM Path to Snowhead Snow Tree End 1", "Path to Snowhead Snow Tree End 1"),
                        M("Tree.png", 783, 128, 24, "MM Path to Snowhead Snow Tree End 2", "Path to Snowhead Snow Tree End 2"),
                        M("Tree.png", 720, 167, 24, "MM Path to Snowhead Snow Tree End 3", "Path to Snowhead Snow Tree End 3"),
                        MA("Snowball.png", 808, 136, 24, "MM Path to Snowhead Big Snowball Back 1", "cursed", "Path to Snowhead Big Snowball Back 1"),
                        MA("Snowball.png", 710, 148, 24, "MM Path to Snowhead Big Snowball Back 2", "cursed", "Path to Snowhead Big Snowball Back 2"),
                        MA("Snowball.png", 580, 150, 24, "MM Path to Snowhead Big Snowball Middle 1", "cursed", "Path to Snowhead Big Snowball Middle 1"),
                        MA("Snowball.png", 199, 223, 24, "MM Path to Snowhead Big Snowball Middle 2", "cursed", "Path to Snowhead Big Snowball Middle 2"),
                        MA("Snowball.png", 717, 193, 24, "MM Path to Snowhead Small Snowball Back 1", "cursed", "Path to Snowhead Small Snowball Back 1"),
                        MA("Snowball.png", 674, 204, 24, "MM Path to Snowhead Small Snowball Back 2", "cursed", "Path to Snowhead Small Snowball Back 2"),
                        MA("Snowball.png", 676, 169, 24, "MM Path to Snowhead Small Snowball Back 3", "cursed", "Path to Snowhead Small Snowball Back 3"),
                        MA("Snowball.png", 710, 148, 24, "MM Path to Snowhead Small Snowball Spring Back 1", "cleared", "Path to Snowhead Small Snowball Spring Back 1"),
                        MA("Snowball.png", 808, 136, 24, "MM Path to Snowhead Small Snowball Spring Back 2", "cleared", "Path to Snowhead Small Snowball Spring Back 2"),
                        MA("Snowball.png", 580, 150, 24, "MM Path to Snowhead Small Snowball Spring Middle 1", "cleared", "Path to Snowhead Small Snowball Spring Middle 1"),
                        MA("Snowball.png", 199, 223, 24, "MM Path to Snowhead Small Snowball Spring Middle 2", "cleared", "Path to Snowhead Small Snowball Spring Middle 2"),
						
						ME("Entrance.png", 10, 103, "Entrance shuffle (Mountain Village)", "MM_MOUNTAIN_VILLAGE_FROM_SNOWHEAD_PATH"),
						ME("Entrance.png", 880, 68, "Entrance shuffle (Snowhead)", "MM_SNOWHEAD_FROM_SNOWHEAD_PATH"),
						ME("Entrance.png", 686, 177, "Entrance shuffle (Generic Grotto)", "MM_GROTTO_GENERIC_PATH_SNOWHEAD")
                    }
                },
                new MapSubRegion
                {
                    Name = "Generic Grotto",
                    BackgroundImage = MM("Grottos", "Generic"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GENERIC_PATH_SNOWHEAD" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 208, 394, 40, "MM Path to Snowhead Grotto", "Path to Snowhead Grotto"),
                        M("Grass.png", 605, 531, 24, "MM Path to Snowhead Grotto Grass 01", "Path to Snowhead Grotto Grass 01"),
                        M("Grass.png", 363, 479, 24, "MM Path to Snowhead Grotto Grass 02", "Path to Snowhead Grotto Grass 02"),
                        M("Grass.png", 270, 451, 24, "MM Path to Snowhead Grotto Grass 03", "Path to Snowhead Grotto Grass 03"),
                        M("Grass.png", 330, 467, 24, "MM Path to Snowhead Grotto Grass 04", "Path to Snowhead Grotto Grass 04"),
                        M("Grass.png", 290, 433, 24, "MM Path to Snowhead Grotto Grass 05", "Path to Snowhead Grotto Grass 05"),
                        M("Grass.png", 255, 427, 24, "MM Path to Snowhead Grotto Grass 06", "Path to Snowhead Grotto Grass 06"),
                        M("Grass.png", 689, 480, 24, "MM Path to Snowhead Grotto Grass 07", "Path to Snowhead Grotto Grass 07"),
                        M("Grass.png", 383, 417, 24, "MM Path to Snowhead Grotto Grass 08", "Path to Snowhead Grotto Grass 08"),
                        M("Grass.png", 557, 439, 24, "MM Path to Snowhead Grotto Grass 09", "Path to Snowhead Grotto Grass 09"),
                        M("Grass.png", 642, 449, 24, "MM Path to Snowhead Grotto Grass 10", "Path to Snowhead Grotto Grass 10"),
                        M("Grass.png", 245, 375, 24, "MM Path to Snowhead Grotto Grass 11", "Path to Snowhead Grotto Grass 11"),
                        M("Grass.png", 375, 397, 24, "MM Path to Snowhead Grotto Grass 12", "Path to Snowhead Grotto Grass 12"),
                        M("Grass.png", 587, 432, 24, "MM Path to Snowhead Grotto Grass 13", "Path to Snowhead Grotto Grass 13"),
                        M("Grass.png", 408, 402, 24, "MM Path to Snowhead Grotto Grass 14", "Path to Snowhead Grotto Grass 14"),
						
						ME("Entrance.png", 621, 483, "Entrance shuffle (Path to Snowhead)", "MM_GROTTO_EXIT_GENERIC_PATH_SNOWHEAD")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion RoadtoIkana()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Road to Ikana";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Road to Ikana",
                    BackgroundImage = MM("Road_to_Ikana", "Road"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_GROTTO_EXIT_GENERIC_PATH_IKANA",
						"MM_IKANA_ROAD_FROM_VALLEY",
						"MM_IKANA_ROAD_FROM_IKANA_GRAVEYARD",
						"MM_IKANA_ROAD_FROM_FIELD"
					},
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 678, 225, 40, "MM Road to Ikana Chest", "Road to Ikana Chest"),
                        M("NPC.png", 242, 200, 40, "MM Road to Ikana Stone Mask", "Road to Ikana Stone Mask"),
                        M("Pot.png", 297, 226, 24, "MM Road to Ikana Pot", "Road to Ikana Pot"),
                        M("Red_Boulder.png", 443, 257, 24, "MM Road to Ikana Red Boulder", "Road to Ikana Red Boulder"),
                        M("Rock.png", 250, 240, 24, "MM Road to Ikana Rock Circle Rock 1", "Road to Ikana Rock Circle Rock 1"),
                        M("Rock.png", 226, 232, 24, "MM Road to Ikana Rock Circle Rock 2", "Road to Ikana Rock Circle Rock 2"),
                        M("Rock.png", 218, 208, 24, "MM Road to Ikana Rock Circle Rock 3", "Road to Ikana Rock Circle Rock 3"),
                        M("Rock.png", 226, 184, 24, "MM Road to Ikana Rock Circle Rock 4", "Road to Ikana Rock Circle Rock 4"),
                        M("Rock.png", 250, 176, 24, "MM Road to Ikana Rock Circle Rock 5", "Road to Ikana Rock Circle Rock 5"),
                        M("Rock.png", 274, 184, 24, "MM Road to Ikana Rock Circle Rock 6", "Road to Ikana Rock Circle Rock 6"),
                        M("Rock.png", 282, 208, 24, "MM Road to Ikana Rock Circle Rock 7", "Road to Ikana Rock Circle Rock 7"),
                        M("Rock.png", 274, 232, 24, "MM Road to Ikana Rock Circle Rock 8", "Road to Ikana Rock Circle Rock 8"),
						
						ME("Entrance.png", 14, 260, "Entrance shuffle (Ikana Canyon)", "MM_IKANA_VALLEY_FROM_ROAD"),
						ME("Entrance.png", 778, 279, "Entrance shuffle (Termina Field)", "MM_TERMINA_FIELD_FROM_ROAD_TO_IKANA"),
						ME("Entrance.png", 278, 437, "Entrance shuffle (Ikana Graveyard)", "MM_IKANA_GRAVEYARD"),
						ME("Entrance.png", 457, 208, "Entrance shuffle (Generic Grotto)", "MM_GROTTO_GENERIC_PATH_IKANA")
                    }
                },
                new MapSubRegion
                {
                    Name = "Generic Grotto",
                    BackgroundImage = MM("Grottos", "Generic"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GENERIC_PATH_IKANA" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 208, 394, 40, "MM Road to Ikana Grotto", "Road to Ikana Grotto"),
                        M("Grass.png", 605, 531, 24, "MM Road to Ikana Grotto Grass 01", "Road to Ikana Grotto Grass 01"),
                        M("Grass.png", 363, 479, 24, "MM Road to Ikana Grotto Grass 02", "Road to Ikana Grotto Grass 02"),
                        M("Grass.png", 270, 451, 24, "MM Road to Ikana Grotto Grass 03", "Road to Ikana Grotto Grass 03"),
                        M("Grass.png", 330, 467, 24, "MM Road to Ikana Grotto Grass 04", "Road to Ikana Grotto Grass 04"),
                        M("Grass.png", 290, 433, 24, "MM Road to Ikana Grotto Grass 05", "Road to Ikana Grotto Grass 05"),
                        M("Grass.png", 255, 427, 24, "MM Road to Ikana Grotto Grass 06", "Road to Ikana Grotto Grass 06"),
                        M("Grass.png", 689, 480, 24, "MM Road to Ikana Grotto Grass 07", "Road to Ikana Grotto Grass 07"),
                        M("Grass.png", 383, 417, 24, "MM Road to Ikana Grotto Grass 08", "Road to Ikana Grotto Grass 08"),
                        M("Grass.png", 557, 439, 24, "MM Road to Ikana Grotto Grass 09", "Road to Ikana Grotto Grass 09"),
                        M("Grass.png", 642, 449, 24, "MM Road to Ikana Grotto Grass 10", "Road to Ikana Grotto Grass 10"),
                        M("Grass.png", 245, 375, 24, "MM Road to Ikana Grotto Grass 11", "Road to Ikana Grotto Grass 11"),
                        M("Grass.png", 375, 397, 24, "MM Road to Ikana Grotto Grass 12", "Road to Ikana Grotto Grass 12"),
                        M("Grass.png", 587, 432, 24, "MM Road to Ikana Grotto Grass 13", "Road to Ikana Grotto Grass 13"),
                        M("Grass.png", 408, 402, 24, "MM Road to Ikana Grotto Grass 14", "Road to Ikana Grotto Grass 14"),
						
						ME("Entrance.png", 621, 483, "Entrance shuffle (Road to Ikana)", "MM_GROTTO_EXIT_GENERIC_PATH_IKANA")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion RoadtoSouthernSwamp()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Road to Southern Swamp";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Road to Southern Swamp",
                    BackgroundImage = MM("Road_to_Southern_Swamp", "Road"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_SWAMP_ROAD_FROM_SWAMP",
						"MM_GROTTO_EXIT_GENERIC_PATH_SWAMP",
						"MM_SWAMP_ROAD_FROM_ARCHERY",
						"MM_SWAMP_ROAD_FROM_FIELD"
					},
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 593, 275, 40, "MM Road to Southern Swamp HP", "Road to Southern Swamp HP"),
						M("NPC.png", 470, 197, 40, "MM Tingle Map Woodfall", "Tingle Map Woodfall"),
						M("NPC.png", 470, 157, 40, "MM Tingle Map Snowhead", "Tingle Map Snowhead"),
                        M("Pot.png", 68, 304, 24, "MM Road to Southern Swamp Potted Plant 1 Pot", "Road to Southern Swamp Potted Plant 1 Pot"),
                        M("Pot.png", 56, 335, 24, "MM Road to Southern Swamp Potted Plant 2 Pot", "Road to Southern Swamp Potted Plant 2 Pot"),
                        M("Grass.png", 84, 304, 24, "MM Road to Southern Swamp Potted Plant 1 Grass", "Road to Southern Swamp Potted Plant 1 Grass"),
                        M("Grass.png", 72, 335, 24, "MM Road to Southern Swamp Potted Plant 2 Grass", "Road to Southern Swamp Potted Plant 2 Grass"),
                        M("Grass.png", 95, 287, 24, "MM Road to Southern Swamp Grass 1", "Road to Southern Swamp Grass 1"),
                        M("Grass.png", 78, 276, 24, "MM Road to Southern Swamp Grass 2", "Road to Southern Swamp Grass 2"),
                        M("Grass.png", 662, 281, 24, "MM Road to Southern Swamp Grass Pack 1 Grass 01", "Road to Southern Swamp Grass Pack 1 Grass 01"),
                        M("Grass.png", 662, 299, 24, "MM Road to Southern Swamp Grass Pack 1 Grass 02", "Road to Southern Swamp Grass Pack 1 Grass 02"),
                        M("Grass.png", 647, 294, 24, "MM Road to Southern Swamp Grass Pack 1 Grass 03", "Road to Southern Swamp Grass Pack 1 Grass 03"),
                        M("Grass.png", 642, 281, 24, "MM Road to Southern Swamp Grass Pack 1 Grass 04", "Road to Southern Swamp Grass Pack 1 Grass 04"),
                        M("Grass.png", 647, 271, 24, "MM Road to Southern Swamp Grass Pack 1 Grass 05", "Road to Southern Swamp Grass Pack 1 Grass 05"),
                        M("Grass.png", 662, 263, 24, "MM Road to Southern Swamp Grass Pack 1 Grass 06", "Road to Southern Swamp Grass Pack 1 Grass 06"),
                        M("Grass.png", 677, 271, 24, "MM Road to Southern Swamp Grass Pack 1 Grass 07", "Road to Southern Swamp Grass Pack 1 Grass 07"),
                        M("Grass.png", 682, 281, 24, "MM Road to Southern Swamp Grass Pack 1 Grass 08", "Road to Southern Swamp Grass Pack 1 Grass 08"),
                        M("Grass.png", 677, 294, 24, "MM Road to Southern Swamp Grass Pack 1 Grass 09", "Road to Southern Swamp Grass Pack 1 Grass 09"),
                        M("Grass.png", 636, 337, 24, "MM Road to Southern Swamp Grass Pack 2 Grass 01", "Road to Southern Swamp Grass Pack 2 Grass 01"),
                        M("Grass.png", 636, 355, 24, "MM Road to Southern Swamp Grass Pack 2 Grass 02", "Road to Southern Swamp Grass Pack 2 Grass 02"),
                        M("Grass.png", 621, 350, 24, "MM Road to Southern Swamp Grass Pack 2 Grass 03", "Road to Southern Swamp Grass Pack 2 Grass 03"),
                        M("Grass.png", 616, 337, 24, "MM Road to Southern Swamp Grass Pack 2 Grass 04", "Road to Southern Swamp Grass Pack 2 Grass 04"),
                        M("Grass.png", 621, 327, 24, "MM Road to Southern Swamp Grass Pack 2 Grass 05", "Road to Southern Swamp Grass Pack 2 Grass 05"),
                        M("Grass.png", 636, 319, 24, "MM Road to Southern Swamp Grass Pack 2 Grass 06", "Road to Southern Swamp Grass Pack 2 Grass 06"),
                        M("Grass.png", 651, 327, 24, "MM Road to Southern Swamp Grass Pack 2 Grass 07", "Road to Southern Swamp Grass Pack 2 Grass 07"),
                        M("Grass.png", 656, 337, 24, "MM Road to Southern Swamp Grass Pack 2 Grass 08", "Road to Southern Swamp Grass Pack 2 Grass 08"),
                        M("Grass.png", 651, 350, 24, "MM Road to Southern Swamp Grass Pack 2 Grass 09", "Road to Southern Swamp Grass Pack 2 Grass 09"),
                        M("Tree.png", 363, 360, 24, "MM Road to Southern Swamp Forked Tree 1", "Road to Southern Swamp Forked Tree 1"),
                        M("Tree.png", 436, 346, 24, "MM Road to Southern Swamp Forked Tree 2", "Road to Southern Swamp Forked Tree 2"),
                        M("Tree.png", 324, 340, 24, "MM Road to Southern Swamp Forked Tree 3", "Road to Southern Swamp Forked Tree 3"),
                        M("Tree.png", 246, 361, 24, "MM Road to Southern Swamp Forked Tree 4", "Road to Southern Swamp Forked Tree 4"),
                        M("Tree.png", 401, 327, 24, "MM Road to Southern Swamp Forked Tree 5", "Road to Southern Swamp Forked Tree 5"),
                        M("Tree.png", 290, 378, 24, "MM Road to Southern Swamp Forked Tree 6", "Road to Southern Swamp Forked Tree 6"),
						
						ME("Entrance.png", 421, 31, "Entrance shuffle (Southern Swamp)", "MM_SWAMP_FROM_ROAD"),
						ME("Entrance.png", 745, 485, "Entrance shuffle (Termina Field)", "MM_TERMINA_FIELD_FROM_ROAD_TO_SWAMP"),
						ME("Entrance.png", 26, 305, "Entrance shuffle (Swamp Archery)", "MM_SWAMP_ARCHERY"),
						ME("Entrance.png", 606, 199, "Entrance shuffle (Open Grotto)", "MM_GROTTO_GENERIC_PATH_SWAMP")
                    }
                },
                new MapSubRegion
                {
                    Name = "Shooting Gallery",
                    BackgroundImage = MM("Road_to_Southern_Swamp", "Shooting"),
                    DestinationEntranceIds = new List<string> { "MM_SWAMP_ARCHERY" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 438, 155, 40, "MM Swamp Archery Reward 1", "Swamp Archery Reward 1"),
                        M("NPC.png", 506, 155, 40, "MM Swamp Archery Reward 2", "Swamp Archery Reward 2"),
						
						ME("Entrance.png", 465, 473, "Entrance shuffle (Road to Southern Swamp)", "MM_SWAMP_ROAD_FROM_ARCHERY")
                    }
                },
                new MapSubRegion
                {
                    Name = "Open Grotto",
                    BackgroundImage = MM("Grottos", "Generic"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GENERIC_PATH_SWAMP" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 208, 394, 40, "MM Road to Southern Swamp Grotto", "Road to Southern Swamp Grotto"),
                        M("Grass.png", 605, 531, 24, "MM Road to Southern Swamp Grotto Grass 01", "Road to Southern Swamp Grotto Grass 01"),
                        M("Grass.png", 363, 479, 24, "MM Road to Southern Swamp Grotto Grass 02", "Road to Southern Swamp Grotto Grass 02"),
                        M("Grass.png", 270, 451, 24, "MM Road to Southern Swamp Grotto Grass 03", "Road to Southern Swamp Grotto Grass 03"),
                        M("Grass.png", 330, 467, 24, "MM Road to Southern Swamp Grotto Grass 04", "Road to Southern Swamp Grotto Grass 04"),
                        M("Grass.png", 290, 433, 24, "MM Road to Southern Swamp Grotto Grass 05", "Road to Southern Swamp Grotto Grass 05"),
                        M("Grass.png", 255, 427, 24, "MM Road to Southern Swamp Grotto Grass 06", "Road to Southern Swamp Grotto Grass 06"),
                        M("Grass.png", 689, 480, 24, "MM Road to Southern Swamp Grotto Grass 07", "Road to Southern Swamp Grotto Grass 07"),
                        M("Grass.png", 383, 417, 24, "MM Road to Southern Swamp Grotto Grass 08", "Road to Southern Swamp Grotto Grass 08"),
                        M("Grass.png", 557, 439, 24, "MM Road to Southern Swamp Grotto Grass 09", "Road to Southern Swamp Grotto Grass 09"),
                        M("Grass.png", 642, 449, 24, "MM Road to Southern Swamp Grotto Grass 10", "Road to Southern Swamp Grotto Grass 10"),
                        M("Grass.png", 245, 375, 24, "MM Road to Southern Swamp Grotto Grass 11", "Road to Southern Swamp Grotto Grass 11"),
                        M("Grass.png", 375, 397, 24, "MM Road to Southern Swamp Grotto Grass 12", "Road to Southern Swamp Grotto Grass 12"),
                        M("Grass.png", 587, 432, 24, "MM Road to Southern Swamp Grotto Grass 13", "Road to Southern Swamp Grotto Grass 13"),
                        M("Grass.png", 408, 402, 24, "MM Road to Southern Swamp Grotto Grass 14", "Road to Southern Swamp Grotto Grass 14"),
						
						ME("Entrance.png", 621, 483, "Entrance shuffle (Road to Southern Swamp)", "MM_GROTTO_EXIT_GENERIC_PATH_SWAMP")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion RomaniRanch()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Romani Ranch";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Romani Ranch",
                    BackgroundImage = MM("Ranch", "Romani_Ranch"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_ROMANI_RANCH_FROM_MILK_ROAD",
						"MM_ROMANI_RANCH_FROM_DOGGY_RACETRACK",
						"MM_ROMANI_RANCH_FROM_RANCH_HOUSE",
						"MM_ROMANI_RANCH_FROM_CUCCO_SHACK",
						"MM_ROMANI_RANCH_FROM_STABLES"
					},
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 319, 149, 40, "MM Romani Ranch Aliens", "Romani Ranch Aliens"),
                        M("NPC.png", 328, 100, 40, "MM Romani Ranch Cremia Escort", "Romani Ranch Cremia Escort"),
                        M("NPC.png", 238, 153, 40, "MM Romani Ranch Epona Song", "Romani Ranch Epona Song"),
                        M("Bush.png", 189, 289, 24, "MM Romani Ranch Bush 1", "Romani Ranch Bush 1"),
                        M("Bush.png", 397, 383, 24, "MM Romani Ranch Bush 2", "Romani Ranch Bush 2"),
                        M("Bush.png", 545, 501, 24, "MM Romani Ranch Bush 3", "Romani Ranch Bush 3"),
                        M("Bush.png", 175, 131, 24, "MM Romani Ranch Bush 4", "Romani Ranch Bush 4"),
                        M("Crate.png", 355, 167, 24, "MM Romani Ranch Crate", "Romani Ranch Crate"),
                        M("Soil.png", 220, 42, 24, "MM Romani Ranch Soil Day 1", "Romani Ranch Soil Day 1"),
                        M("Soil.png", 206, 81, 24, "MM Romani Ranch Soil Days 2-3 Item 1", "Romani Ranch Soil Days 2-3 Item 1"),
                        M("Soil.png", 231, 71, 24, "MM Romani Ranch Soil Days 2-3 Item 2", "Romani Ranch Soil Days 2-3 Item 2"),
                        M("Soil.png", 256, 61, 24, "MM Romani Ranch Soil Days 2-3 Item 3", "Romani Ranch Soil Days 2-3 Item 3"),
                        M("Tree.png", 451, 350, 24, "MM Romani Ranch Tree 1", "Romani Ranch Tree 1"),
                        M("Tree.png", 433, 260, 24, "MM Romani Ranch Tree 2", "Romani Ranch Tree 2"),
                        M("Tree.png", 631, 272, 24, "MM Romani Ranch Tree 3", "Romani Ranch Tree 3"),
                        M("Tree.png", 625, 345, 24, "MM Romani Ranch Tree 4", "Romani Ranch Tree 4"),
                        M("Tree.png", 322, 395, 24, "MM Romani Ranch Tree 5", "Romani Ranch Tree 5"),
                        M("Tree.png", 172, 429, 24, "MM Romani Ranch Tree 6", "Romani Ranch Tree 6"),
                        M("Tree.png", 358, 545, 24, "MM Romani Ranch Tree 7", "Romani Ranch Tree 7"),
                        M("Wonder.png", 750, 317, 24, "MM Romani Ranch Wonder Item Fence 1", "Romani Ranch Wonder Item Fence 1"),
                        M("Wonder.png", 746, 339, 24, "MM Romani Ranch Wonder Item Fence 2", "Romani Ranch Wonder Item Fence 2"),
                        M("Wonder.png", 745, 275, 24, "MM Romani Ranch Wonder Item Fence 3", "Romani Ranch Wonder Item Fence 3"),
                        M("Wonder.png", 746, 298, 24, "MM Romani Ranch Wonder Item Fence 4", "Romani Ranch Wonder Item Fence 4"),
                        M("Wonder.png", 737, 360, 24, "MM Romani Ranch Wonder Item Fence 5", "Romani Ranch Wonder Item Fence 5"),
                        M("Wonder.png", 739, 400, 24, "MM Romani Ranch Wonder Item Fence 6", "Romani Ranch Wonder Item Fence 6"),
                        M("Grass.png", 431, 512, 24, "MM Romani Ranch Grass Pack 1 Grass 01", "Romani Ranch Grass Pack 1 Grass 01"),
                        M("Grass.png", 431, 494, 24, "MM Romani Ranch Grass Pack 1 Grass 02", "Romani Ranch Grass Pack 1 Grass 02"),
                        M("Grass.png", 451, 494, 24, "MM Romani Ranch Grass Pack 1 Grass 03", "Romani Ranch Grass Pack 1 Grass 03"),
                        M("Grass.png", 451, 512, 24, "MM Romani Ranch Grass Pack 1 Grass 04", "Romani Ranch Grass Pack 1 Grass 04"),
                        M("Grass.png", 441, 530, 24, "MM Romani Ranch Grass Pack 1 Grass 05", "Romani Ranch Grass Pack 1 Grass 05"),
                        M("Grass.png", 419, 522, 24, "MM Romani Ranch Grass Pack 1 Grass 06", "Romani Ranch Grass Pack 1 Grass 06"),
                        M("Grass.png", 411, 503, 24, "MM Romani Ranch Grass Pack 1 Grass 07", "Romani Ranch Grass Pack 1 Grass 07"),
                        M("Grass.png", 419, 484, 24, "MM Romani Ranch Grass Pack 1 Grass 08", "Romani Ranch Grass Pack 1 Grass 08"),
                        M("Grass.png", 441, 476, 24, "MM Romani Ranch Grass Pack 1 Grass 09", "Romani Ranch Grass Pack 1 Grass 09"),
                        M("Grass.png", 463, 484, 24, "MM Romani Ranch Grass Pack 1 Grass 10", "Romani Ranch Grass Pack 1 Grass 10"),
                        M("Grass.png", 471, 503, 24, "MM Romani Ranch Grass Pack 1 Grass 11", "Romani Ranch Grass Pack 1 Grass 11"),
                        M("Grass.png", 463, 522, 24, "MM Romani Ranch Grass Pack 1 Grass 12", "Romani Ranch Grass Pack 1 Grass 12"),
                        M("Grass.png", 261, 510, 24, "MM Romani Ranch Grass Pack 2 Grass 01", "Romani Ranch Grass Pack 2 Grass 01"),
                        M("Grass.png", 261, 492, 24, "MM Romani Ranch Grass Pack 2 Grass 02", "Romani Ranch Grass Pack 2 Grass 02"),
                        M("Grass.png", 281, 492, 24, "MM Romani Ranch Grass Pack 2 Grass 03", "Romani Ranch Grass Pack 2 Grass 03"),
                        M("Grass.png", 281, 510, 24, "MM Romani Ranch Grass Pack 2 Grass 04", "Romani Ranch Grass Pack 2 Grass 04"),
                        M("Grass.png", 271, 528, 24, "MM Romani Ranch Grass Pack 2 Grass 05", "Romani Ranch Grass Pack 2 Grass 05"),
                        M("Grass.png", 249, 520, 24, "MM Romani Ranch Grass Pack 2 Grass 06", "Romani Ranch Grass Pack 2 Grass 06"),
                        M("Grass.png", 241, 501, 24, "MM Romani Ranch Grass Pack 2 Grass 07", "Romani Ranch Grass Pack 2 Grass 07"),
                        M("Grass.png", 249, 482, 24, "MM Romani Ranch Grass Pack 2 Grass 08", "Romani Ranch Grass Pack 2 Grass 08"),
                        M("Grass.png", 271, 474, 24, "MM Romani Ranch Grass Pack 2 Grass 09", "Romani Ranch Grass Pack 2 Grass 09"),
                        M("Grass.png", 293, 482, 24, "MM Romani Ranch Grass Pack 2 Grass 10", "Romani Ranch Grass Pack 2 Grass 10"),
                        M("Grass.png", 301, 501, 24, "MM Romani Ranch Grass Pack 2 Grass 11", "Romani Ranch Grass Pack 2 Grass 11"),
                        M("Grass.png", 293, 520, 24, "MM Romani Ranch Grass Pack 2 Grass 12", "Romani Ranch Grass Pack 2 Grass 12"),
                        M("Grass.png", 659, 412, 24, "MM Romani Ranch Grass Pack 3 Grass 01", "Romani Ranch Grass Pack 3 Grass 01"),
                        M("Grass.png", 659, 394, 24, "MM Romani Ranch Grass Pack 3 Grass 02", "Romani Ranch Grass Pack 3 Grass 02"),
                        M("Grass.png", 679, 394, 24, "MM Romani Ranch Grass Pack 3 Grass 03", "Romani Ranch Grass Pack 3 Grass 03"),
                        M("Grass.png", 679, 412, 24, "MM Romani Ranch Grass Pack 3 Grass 04", "Romani Ranch Grass Pack 3 Grass 04"),
                        M("Grass.png", 669, 430, 24, "MM Romani Ranch Grass Pack 3 Grass 05", "Romani Ranch Grass Pack 3 Grass 05"),
                        M("Grass.png", 647, 422, 24, "MM Romani Ranch Grass Pack 3 Grass 06", "Romani Ranch Grass Pack 3 Grass 06"),
                        M("Grass.png", 639, 403, 24, "MM Romani Ranch Grass Pack 3 Grass 07", "Romani Ranch Grass Pack 3 Grass 07"),
                        M("Grass.png", 647, 384, 24, "MM Romani Ranch Grass Pack 3 Grass 08", "Romani Ranch Grass Pack 3 Grass 08"),
                        M("Grass.png", 669, 376, 24, "MM Romani Ranch Grass Pack 3 Grass 09", "Romani Ranch Grass Pack 3 Grass 09"),
                        M("Grass.png", 691, 384, 24, "MM Romani Ranch Grass Pack 3 Grass 10", "Romani Ranch Grass Pack 3 Grass 10"),
                        M("Grass.png", 699, 403, 24, "MM Romani Ranch Grass Pack 3 Grass 11", "Romani Ranch Grass Pack 3 Grass 11"),
                        M("Grass.png", 691, 422, 24, "MM Romani Ranch Grass Pack 3 Grass 12", "Romani Ranch Grass Pack 3 Grass 12"),
                        M("Grass.png", 538, 135, 24, "MM Romani Ranch Grass Pack 4 Grass 01", "Romani Ranch Grass Pack 4 Grass 01"),
                        M("Grass.png", 538, 117, 24, "MM Romani Ranch Grass Pack 4 Grass 02", "Romani Ranch Grass Pack 4 Grass 02"),
                        M("Grass.png", 558, 117, 24, "MM Romani Ranch Grass Pack 4 Grass 03", "Romani Ranch Grass Pack 4 Grass 03"),
                        M("Grass.png", 558, 135, 24, "MM Romani Ranch Grass Pack 4 Grass 04", "Romani Ranch Grass Pack 4 Grass 04"),
                        M("Grass.png", 548, 153, 24, "MM Romani Ranch Grass Pack 4 Grass 05", "Romani Ranch Grass Pack 4 Grass 05"),
                        M("Grass.png", 526, 145, 24, "MM Romani Ranch Grass Pack 4 Grass 06", "Romani Ranch Grass Pack 4 Grass 06"),
                        M("Grass.png", 518, 126, 24, "MM Romani Ranch Grass Pack 4 Grass 07", "Romani Ranch Grass Pack 4 Grass 07"),
                        M("Grass.png", 526, 107, 24, "MM Romani Ranch Grass Pack 4 Grass 08", "Romani Ranch Grass Pack 4 Grass 08"),
                        M("Grass.png", 548, 99, 24, "MM Romani Ranch Grass Pack 4 Grass 09", "Romani Ranch Grass Pack 4 Grass 09"),
                        M("Grass.png", 570, 107, 24, "MM Romani Ranch Grass Pack 4 Grass 10", "Romani Ranch Grass Pack 4 Grass 10"),
                        M("Grass.png", 578, 126, 24, "MM Romani Ranch Grass Pack 4 Grass 11", "Romani Ranch Grass Pack 4 Grass 11"),
                        M("Grass.png", 570, 145, 24, "MM Romani Ranch Grass Pack 4 Grass 12", "Romani Ranch Grass Pack 4 Grass 12"),
                        M("Grass.png", 392, 20, 24, "MM Romani Ranch Grass Pack 5 Grass 01", "Romani Ranch Grass Pack 5 Grass 01"),
                        M("Grass.png", 392, 38, 24, "MM Romani Ranch Grass Pack 5 Grass 02", "Romani Ranch Grass Pack 5 Grass 02"),
                        M("Grass.png", 376, 32, 24, "MM Romani Ranch Grass Pack 5 Grass 03", "Romani Ranch Grass Pack 5 Grass 03"),
                        M("Grass.png", 372, 20, 24, "MM Romani Ranch Grass Pack 5 Grass 04", "Romani Ranch Grass Pack 5 Grass 04"),
                        M("Grass.png", 376, 9, 24, "MM Romani Ranch Grass Pack 5 Grass 05", "Romani Ranch Grass Pack 5 Grass 05"),
                        M("Grass.png", 392, 2, 24, "MM Romani Ranch Grass Pack 5 Grass 06", "Romani Ranch Grass Pack 5 Grass 06"),
                        M("Grass.png", 408, 9, 24, "MM Romani Ranch Grass Pack 5 Grass 07", "Romani Ranch Grass Pack 5 Grass 07"),
                        M("Grass.png", 412, 20, 24, "MM Romani Ranch Grass Pack 5 Grass 08", "Romani Ranch Grass Pack 5 Grass 08"),
                        M("Grass.png", 408, 32, 24, "MM Romani Ranch Grass Pack 5 Grass 09", "Romani Ranch Grass Pack 5 Grass 09"),
						
						ME("Entrance.png", 871, 351, "Entrance shuffle (Milk Road)", "MM_MILK_ROAD_FROM_ROMANI_RANCH"),
						ME("Entrance.png", 268, 115, "Entrance shuffle (Ranch House)", "MM_RANCH_HOUSE"),
						ME("Entrance.png", 366, 70, "Entrance shuffle (Ranch Barn)", "MM_STABLES"),
						ME("Entrance.png", 52, 420, "Entrance shuffle (Dog Racetrack)", "MM_DOGGY_RACETRACK"),
						ME("Entrance.png", 21, 320, "Entrance shuffle (Cucco Shack)", "MM_CUCCO_SHACK")
                    }
                },
                new MapSubRegion
                {
                    Name = "Ranch Barn",
                    BackgroundImage = MM("Ranch", "Stable"),
                    DestinationEntranceIds = new List<string> { "MM_STABLES" },
                    Marks = new List<MapMark>
                    {
                        M("Cow.png", 347, 371, 40, "MM Romani Ranch Barn Cow Left", "Romani Ranch Barn Cow Left"),
                        M("Cow.png", 323, 207, 40, "MM Romani Ranch Barn Cow Right Back", "Romani Ranch Barn Cow Right Back"),
                        M("Cow.png", 436, 207, 40, "MM Romani Ranch Barn Cow Right Front", "Romani Ranch Barn Cow Right Front"),
                        M("Wonder.png", 522, 536, 24, "MM Romani Ranch Barn Wonder Item 1", "Romani Ranch Barn Wonder Item 1"),
                        M("Wonder.png", 570, 536, 24, "MM Romani Ranch Barn Wonder Item 2", "Romani Ranch Barn Wonder Item 2"),
						
						ME("Entrance.png", 638, 374, "Entrance shuffle (Romani Ranch)", "MM_ROMANI_RANCH_FROM_STABLES")
                    }
                },
                new MapSubRegion
                {
                    Name = "Ranch House",
                    BackgroundImage = MM("Ranch", "House"),
                    DestinationEntranceIds = new List<string> { "MM_RANCH_HOUSE" },
                    Marks = new List<MapMark> { ME("Entrance.png", 154, 374, "Entrance shuffle (Romani Ranch)", "MM_ROMANI_RANCH_FROM_RANCH_HOUSE") }
                },
                new MapSubRegion
                {
                    Name = "Cucco Shack",
                    BackgroundImage = MM("Ranch", "Shack"),
                    DestinationEntranceIds = new List<string> { "MM_CUCCO_SHACK" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 396, 297, 40, "MM Cucco Shack Bunny Mask", "Cucco Shack Bunny Mask"),
                        M("Bush.png", 348, 324, 24, "MM Cucco Shack Bush 1", "Cucco Shack Bush 1"),
                        M("Bush.png", 636, 460, 24, "MM Cucco Shack Bush 2", "Cucco Shack Bush 2"),
                        M("Bush.png", 553, 247, 24, "MM Cucco Shack Bush 3", "Cucco Shack Bush 3"),
                        M("Bush.png", 381, 166, 24, "MM Cucco Shack Bush 4", "Cucco Shack Bush 4"),
                        M("Bush.png", 229, 296, 24, "MM Cucco Shack Bush 5", "Cucco Shack Bush 5"),
                        M("Bush.png", 255, 476, 24, "MM Cucco Shack Bush 6", "Cucco Shack Bush 6"),
                        M("Bush.png", 636, 185, 24, "MM Cucco Shack Bush 7", "Cucco Shack Bush 7"),
                        M("Crate.png", 859, 308, 24, "MM Cucco Shack Crate 1", "Cucco Shack Crate 1"),
                        M("Crate.png", 835, 308, 24, "MM Cucco Shack Crate 2", "Cucco Shack Crate 2"),
                        M("Crate.png", 811, 308, 24, "MM Cucco Shack Crate 3", "Cucco Shack Crate 3"),
                        M("Pot.png", 740, 492, 24, "MM Cucco Shack Potted Plant 1 Pot", "Cucco Shack Potted Plant 1 Pot"),
                        M("Pot.png", 699, 492, 24, "MM Cucco Shack Potted Plant 2 Pot", "Cucco Shack Potted Plant 2 Pot"),
                        M("Grass.png", 740, 468, 24, "MM Cucco Shack Potted Plant 1 Grass", "Cucco Shack Potted Plant 1 Grass"),
                        M("Grass.png", 699, 468, 24, "MM Cucco Shack Potted Plant 2 Grass", "Cucco Shack Potted Plant 2 Grass"),
                        M("Tree.png", 374, 305, 24, "MM Cucco Shack Tree", "Cucco Shack Tree"),
                        M("Wonder.png", 268, 277, 24, "MM Cucco Shack Wonder Item 1", "Cucco Shack Wonder Item 1"),
                        M("Wonder.png", 275, 296, 24, "MM Cucco Shack Wonder Item 2", "Cucco Shack Wonder Item 2"),
                        M("Wonder.png", 275, 258, 24, "MM Cucco Shack Wonder Item 3", "Cucco Shack Wonder Item 3"),
                        M("Wonder.png", 275, 354, 24, "MM Cucco Shack Wonder Item 4", "Cucco Shack Wonder Item 4"),
                        M("Wonder.png", 268, 335, 24, "MM Cucco Shack Wonder Item 5", "Cucco Shack Wonder Item 5"),
                        M("Wonder.png", 275, 316, 24, "MM Cucco Shack Wonder Item 6", "Cucco Shack Wonder Item 6"),
						
						ME("Entrance.png", 887, 447, "Entrance shuffle (Romani Ranch)", "MM_ROMANI_RANCH_FROM_CUCCO_SHACK")
                    }
                },
                new MapSubRegion
                {
                    Name = "Dog Racetrack",
                    BackgroundImage = MM("Ranch", "Dog_Race"),
                    DestinationEntranceIds = new List<string> { "MM_DOGGY_RACETRACK" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 255, 237, 40, "MM Doggy Racetrack Chest", "Doggy Racetrack Chest"),
                        M("NPC.png", 624, 345, 40, "MM Doggy Racetrack HP", "Doggy Racetrack HP"),
                        M("Soil.png", 370, 386, 24, "MM Doggy Racetrack Soil", "Doggy Racetrack Soil"),
                        M("Pot.png", 315, 256, 24, "MM Doggy Racetrack Pot 1", "Doggy Racetrack Pot 1"),
                        M("Pot.png", 291, 272, 24, "MM Doggy Racetrack Pot 2", "Doggy Racetrack Pot 2"),
                        M("Pot.png", 267, 288, 24, "MM Doggy Racetrack Pot 3", "Doggy Racetrack Pot 3"),
                        M("Pot.png", 243, 304, 24, "MM Doggy Racetrack Pot 4", "Doggy Racetrack Pot 4"),
						
						ME("Entrance.png", 859, 377, "Entrance shuffle (Romani Ranch)", "MM_ROMANI_RANCH_FROM_DOGGY_RACETRACK")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion Snowhead()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Snowhead";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Snowhead",
                    BackgroundImage = MM("Snowhead", "Snowhead"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_SNOWHEAD_FROM_TEMPLE",
						"MM_SNOWHEAD_FROM_FAIRY_FOUNTAIN",
						"MM_WARP_OWL_SNOWHEAD",
						"MM_SNOWHEAD_FROM_SNOWHEAD_PATH"
					},
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 168, 486, 40, "MM Snowhead Owl Statue", "Snowhead Owl Statue"),
                        MA("Snowball.png", 609, 299, 24, "MM Snowhead Big Snowball 1", "cursed", "Snowhead Big Snowball 1"),
                        MA("Snowball.png", 596, 217, 24, "MM Snowhead Big Snowball 2", "cursed", "Snowhead Big Snowball 2"),
                        MA("Snowball.png", 690, 351, 24, "MM Snowhead Big Snowball 3", "cursed", "Snowhead Big Snowball 3"),
                        MA("Snowball.png", 747, 338, 24, "MM Snowhead Big Snowball 4", "cursed", "Snowhead Big Snowball 4"),
                        MA("Snowball.png", 799, 281, 24, "MM Snowhead Big Snowball 5", "cursed", "Snowhead Big Snowball 5"),
                        MA("Snowball.png", 625, 213, 24, "MM Snowhead Big Snowball 6", "cursed", "Snowhead Big Snowball 6"),
                        M("Snowball.png", 206, 471, 24, "MM Snowhead Small Snowball 1", "Snowhead Small Snowball 1"),
                        M("Snowball.png", 142, 491, 24, "MM Snowhead Small Snowball 2", "Snowhead Small Snowball 2"),
                        M("Snowball.png", 211, 446, 24, "MM Snowhead Small Snowball 3", "Snowhead Small Snowball 3"),
                        M("Snowball.png", 185, 454, 24, "MM Snowhead Small Snowball 4", "Snowhead Small Snowball 4"),
                        MA("Snowball.png", 609, 299, 24, "MM Snowhead Small Snowball Spring 1", "cleared", "Snowhead Small Snowball Spring 1"),
                        MA("Snowball.png", 596, 217, 24, "MM Snowhead Small Snowball Spring 2", "cleared", "Snowhead Small Snowball Spring 2"),
                        MA("Snowball.png", 690, 351, 24, "MM Snowhead Small Snowball Spring 3", "cleared", "Snowhead Small Snowball Spring 3"),
                        MA("Snowball.png", 747, 338, 24, "MM Snowhead Small Snowball Spring 4", "cleared", "Snowhead Small Snowball Spring 4"),
                        MA("Snowball.png", 625, 213, 24, "MM Snowhead Small Snowball Spring 5", "cleared", "Snowhead Small Snowball Spring 5"),
						
						ME("Entrance.png", 92, 517, "Entrance shuffle (Path to Snowhead)", "MM_PATH_SNOWHEAD_FROM_SNOWHEAD"),
						ME("Entrance.png", 658, 232, "Entrance shuffle (Snowhead Temple)", "MM_TEMPLE_SNOWHEAD"),
						ME("Entrance.png", 767, 305, "Entrance shuffle (Fairy Fountain)", "MM_FAIRY_FOUNTAIN_SNOWHEAD")
                    }
                },
                new MapSubRegion
                {
                    Name = "Fairy Fountain",
                    BackgroundImage = MM("Snowhead", "Fairy"),
                    DestinationEntranceIds = new List<string> { "MM_FAIRY_FOUNTAIN_SNOWHEAD" },
                    Marks = new List<MapMark>
					{
						M("NPC.png", 458, 151, 40, "MM Snowhead Great Fairy", "Snowhead Great Fairy"),
						ME("Entrance.png", 449, 520, "Entrance shuffle (Snowhead)", "MM_SNOWHEAD_FROM_FAIRY_FOUNTAIN")
					}
                }
            };
            return mapRegion;
        }

        public static MapRegion SouthernSwamp()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Southern Swamp";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Southern Swamp",
                    BackgroundImage = MM("Southern_Swamp", "Swamp"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_SWAMP_FROM_SPIDER_HOUSE",
						"MM_GROTTO_EXIT_GENERIC_SWAMP",
						"MM_SWAMP_FROM_TOURIST_INFORMATION",
						"MM_WARP_OWL_SOUTHERN_SWAMP",
						"MM_SWAMP_FROM_ROAD",
						"MM_SWAMP_FROM_POTION_SHOP",
						"MM_SWAMP_FROM_IKANA_CANYON",
						"MM_SWAMP_FROM_MYSTERY_WOODS",
						"MM_SWAMP_FROM_WOODFALL",
						"MM_SWAMP_FROM_PALACE_LEDGE",
						"MM_SWAMP_FROM_PALACE_MAIN_ENTRANCE"
					},
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 582, 641, 40, "MM Southern Swamp HP", "Southern Swamp HP"),
                        M("Hive.png", 529, 338, 40, "MM Southern Swamp Hive", "Southern Swamp Hive"),
                        M("NPC.png", 623, 753, 40, "MM Southern Swamp Owl Statue", "Southern Swamp Owl Statue"),
                        M("NPC.png", 827, 461, 40, "MM Southern Swamp Song of Soaring", "Southern Swamp Song of Soaring"),
                        M("Scrub.png", 542, 647, 40, "MM Southern Swamp Scrub Deep", "Southern Swamp Scrub Deed"),
                        M("Shop.png", 542, 615, 40, "MM Southern Swamp Scrub Shop", "Southern Swamp Scrub Shop"),
                        M("Pot.png", 177, 398, 24, "MM Southern Swamp Pot 1", "Southern Swamp Pot 1"),
                        M("Pot.png", 199, 398, 24, "MM Southern Swamp Pot 2", "Southern Swamp Pot 2"),
                        M("Pot.png", 221, 398, 24, "MM Southern Swamp Pot 3", "Southern Swamp Pot 3"),
                        M("Rupee.png", 736, 223, 24, "MM Southern Swamp Rupee 1", "Southern Swamp Rupee 1"),
                        M("Rupee.png", 767, 220, 24, "MM Southern Swamp Rupee 2", "Southern Swamp Rupee 2"),
                        M("Grass.png", 589, 772, 24, "MM Southern Swamp Grass Owl 1", "Southern Swamp Grass Owl 1"),
                        M("Grass.png", 608, 782, 24, "MM Southern Swamp Grass Owl 2", "Southern Swamp Grass Owl 2"),
                        M("Grass.png", 473, 726, 24, "MM Southern Swamp Grass Front 01", "Southern Swamp Grass Front 01"),
                        M("Grass.png", 473, 706, 24, "MM Southern Swamp Grass Front 02", "Southern Swamp Grass Front 02"),
                        M("Grass.png", 493, 706, 24, "MM Southern Swamp Grass Front 03", "Southern Swamp Grass Front 03"),
                        M("Grass.png", 493, 726, 24, "MM Southern Swamp Grass Front 04", "Southern Swamp Grass Front 04"),
                        M("Grass.png", 483, 746, 24, "MM Southern Swamp Grass Front 05", "Southern Swamp Grass Front 05"),
                        M("Grass.png", 463, 736, 24, "MM Southern Swamp Grass Front 06", "Southern Swamp Grass Front 06"),
                        M("Grass.png", 453, 716, 24, "MM Southern Swamp Grass Front 07", "Southern Swamp Grass Front 07"),
                        M("Grass.png", 463, 696, 24, "MM Southern Swamp Grass Front 08", "Southern Swamp Grass Front 08"),
                        M("Grass.png", 483, 686, 24, "MM Southern Swamp Grass Front 09", "Southern Swamp Grass Front 09"),
                        M("Grass.png", 503, 696, 24, "MM Southern Swamp Grass Front 10", "Southern Swamp Grass Front 10"),
                        M("Grass.png", 513, 716, 24, "MM Southern Swamp Grass Front 11", "Southern Swamp Grass Front 11"),
                        M("Grass.png", 503, 736, 24, "MM Southern Swamp Grass Front 12", "Southern Swamp Grass Front 12"),
                        M("Grass.png", 120, 420, 24, "MM Southern Swamp Grass Near Witch 1", "Southern Swamp Grass Near Witch 1"),
                        M("Grass.png", 120, 441, 24, "MM Southern Swamp Grass Near Witch 2", "Southern Swamp Grass Near Witch 2"),
                        M("Grass.png", 279, 472, 24, "MM Southern Swamp Grass Near Witch Pack 1 Grass 01", "Southern Swamp Grass Near Witch Pack 1 Grass 01"),
                        M("Grass.png", 279, 492, 24, "MM Southern Swamp Grass Near Witch Pack 1 Grass 02", "Southern Swamp Grass Near Witch Pack 1 Grass 02"),
                        M("Grass.png", 265, 486, 24, "MM Southern Swamp Grass Near Witch Pack 1 Grass 03", "Southern Swamp Grass Near Witch Pack 1 Grass 03"),
                        M("Grass.png", 259, 472, 24, "MM Southern Swamp Grass Near Witch Pack 1 Grass 04", "Southern Swamp Grass Near Witch Pack 1 Grass 04"),
                        M("Grass.png", 265, 458, 24, "MM Southern Swamp Grass Near Witch Pack 1 Grass 05", "Southern Swamp Grass Near Witch Pack 1 Grass 05"),
                        M("Grass.png", 279, 452, 24, "MM Southern Swamp Grass Near Witch Pack 1 Grass 06", "Southern Swamp Grass Near Witch Pack 1 Grass 06"),
                        M("Grass.png", 293, 458, 24, "MM Southern Swamp Grass Near Witch Pack 1 Grass 07", "Southern Swamp Grass Near Witch Pack 1 Grass 07"),
                        M("Grass.png", 299, 472, 24, "MM Southern Swamp Grass Near Witch Pack 1 Grass 08", "Southern Swamp Grass Near Witch Pack 1 Grass 08"),
                        M("Grass.png", 293, 486, 24, "MM Southern Swamp Grass Near Witch Pack 1 Grass 09", "Southern Swamp Grass Near Witch Pack 1 Grass 09"),
                        M("Grass.png", 230, 520, 24, "MM Southern Swamp Grass Near Witch Pack 2 Grass 01", "Southern Swamp Grass Near Witch Pack 2 Grass 01"),
                        M("Grass.png", 230, 540, 24, "MM Southern Swamp Grass Near Witch Pack 2 Grass 02", "Southern Swamp Grass Near Witch Pack 2 Grass 02"),
                        M("Grass.png", 216, 534, 24, "MM Southern Swamp Grass Near Witch Pack 2 Grass 03", "Southern Swamp Grass Near Witch Pack 2 Grass 03"),
                        M("Grass.png", 210, 520, 24, "MM Southern Swamp Grass Near Witch Pack 2 Grass 04", "Southern Swamp Grass Near Witch Pack 2 Grass 04"),
                        M("Grass.png", 216, 506, 24, "MM Southern Swamp Grass Near Witch Pack 2 Grass 05", "Southern Swamp Grass Near Witch Pack 2 Grass 05"),
                        M("Grass.png", 230, 500, 24, "MM Southern Swamp Grass Near Witch Pack 2 Grass 06", "Southern Swamp Grass Near Witch Pack 2 Grass 06"),
                        M("Grass.png", 244, 506, 24, "MM Southern Swamp Grass Near Witch Pack 2 Grass 07", "Southern Swamp Grass Near Witch Pack 2 Grass 07"),
                        M("Grass.png", 250, 520, 24, "MM Southern Swamp Grass Near Witch Pack 2 Grass 08", "Southern Swamp Grass Near Witch Pack 2 Grass 08"),
                        M("Grass.png", 244, 534, 24, "MM Southern Swamp Grass Near Witch Pack 2 Grass 09", "Southern Swamp Grass Near Witch Pack 2 Grass 09"),
						
						ME("Entrance.png", 424, 788, "Entrance shuffle (Road to Southern Swamp)", "MM_SWAMP_ROAD_FROM_SWAMP"),
						ME("Entrance.png", 558, 663, "Entrance shuffle (Tourist Information)", "MM_TOURIST_INFORMATION"),
						ME("Entrance.png", 88, 494, "Entrance shuffle (Woods of Mystery)", "MM_MYSTERY_WOODS"),
						ME("Entrance.png", 176, 412, "Entrance shuffle (Potion Shop)", "MM_POTION_SHOP"),
						ME("Entrance.png", 970, 404, "Entrance shuffle (Deku Palace Main)", "MM_DEKU_PALACE_MAIN_ENTRANCE"),
						ME("Entrance.png", 802, 702, "Entrance shuffle (Deku Palace Ledge)", "MM_DEKU_PALACE_LEDGE"),
						ME("Entrance.png", 805, 379, "Entrance shuffle (Woodfall)", "MM_WOODFALL"),
						ME("Entrance.png", 810, 585, "Entrance shuffle (Swamp Spider House)", "MM_SPIDER_HOUSE_SWAMP"),
						ME("Entrance.png", 858, 650, "Entrance shuffle (Generic Grotto)", "MM_GROTTO_GENERIC_SWAMP")
                    }
                },
                new MapSubRegion
                {
                    Name = "Tourist Information",
                    BackgroundImage = MM("Southern_Swamp", "Tourist_Information"),
                    DestinationEntranceIds = new List<string> { "MM_TOURIST_INFORMATION" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 600, 211, 40, "MM Tourist Information Boat Archery", "Tourist Information Boat Archery"),
                        M("NPC.png", 330, 266, 40, "MM Tourist Information Pictobox", "Tourist Information Pictobox"),
                        M("NPC.png", 330, 325, 40, "MM Tourist Information Tingle Picture", "Tourist Information Tingle Picture"),
						
						ME("Entrance.png", 443, 502, "Entrance shuffle (Southern Swamp)", "MM_SWAMP_FROM_TOURIST_INFORMATION")
                    }
                },
                new MapSubRegion
                {
                    Name = "Potion Shop",
                    BackgroundImage = MM("Southern_Swamp", "Potion_Shop"),
					DestinationEntranceIds = new List<string> { "MM_POTION_SHOP" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 454, 331, 40, "MM Swamp Potion Shop Kotake", "Swamp Potion Shop Kotake"),
                        M("Rupee.png", 559, 455, 24, "MM Swamp Potion Shop Rupee", "Swamp Potion Shop Rupee"),
                        M("Shop.png", 410, 278, 40, "MM Swamp Potion Shop Item 1", "Swamp Potion Shop Item 1"),
                        M("Shop.png", 454, 278, 40, "MM Swamp Potion Shop Item 2", "Swamp Potion Shop Item 2"),
                        M("Shop.png", 498, 278, 40, "MM Swamp Potion Shop Item 3", "Swamp Potion Shop Item 3"),
						
						ME("Entrance.png", 457, 515, "Entrance shuffle (Southern Swamp)", "MM_SWAMP_FROM_POTION_SHOP")
                    }
                },
                new MapSubRegion
                {
                    Name = "Open Grotto",
                    BackgroundImage = MM("Grottos", "Generic"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GENERIC_SWAMP" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 208, 394, 40, "MM Southern Swamp Grotto", "Southern Swamp Grotto"),
                        M("Grass.png", 605, 531, 24, "MM Southern Swamp Grotto Grass 01", "Southern Swamp Grotto Grass 01"),
                        M("Grass.png", 363, 479, 24, "MM Southern Swamp Grotto Grass 02", "Southern Swamp Grotto Grass 02"),
                        M("Grass.png", 270, 451, 24, "MM Southern Swamp Grotto Grass 03", "Southern Swamp Grotto Grass 03"),
                        M("Grass.png", 330, 467, 24, "MM Southern Swamp Grotto Grass 04", "Southern Swamp Grotto Grass 04"),
                        M("Grass.png", 290, 433, 24, "MM Southern Swamp Grotto Grass 05", "Southern Swamp Grotto Grass 05"),
                        M("Grass.png", 255, 427, 24, "MM Southern Swamp Grotto Grass 06", "Southern Swamp Grotto Grass 06"),
                        M("Grass.png", 689, 480, 24, "MM Southern Swamp Grotto Grass 07", "Southern Swamp Grotto Grass 07"),
                        M("Grass.png", 383, 417, 24, "MM Southern Swamp Grotto Grass 08", "Southern Swamp Grotto Grass 08"),
                        M("Grass.png", 557, 439, 24, "MM Southern Swamp Grotto Grass 09", "Southern Swamp Grotto Grass 09"),
                        M("Grass.png", 642, 449, 24, "MM Southern Swamp Grotto Grass 10", "Southern Swamp Grotto Grass 10"),
                        M("Grass.png", 245, 375, 24, "MM Southern Swamp Grotto Grass 11", "Southern Swamp Grotto Grass 11"),
                        M("Grass.png", 375, 397, 24, "MM Southern Swamp Grotto Grass 12", "Southern Swamp Grotto Grass 12"),
                        M("Grass.png", 587, 432, 24, "MM Southern Swamp Grotto Grass 13", "Southern Swamp Grotto Grass 13"),
                        M("Grass.png", 408, 402, 24, "MM Southern Swamp Grotto Grass 14", "Southern Swamp Grotto Grass 14"),
						
						ME("Entrance.png", 621, 483, "Entrance shuffle (Southern Swamp)", "MM_GROTTO_EXIT_GENERIC_SWAMP")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion StoneTower()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Stone Tower";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Stone Tower",
                    BackgroundImage = MM("Stone_Tower", "Tower"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_STONE_TOWER_FROM_TEMPLE",
						"MM_WARP_OWL_STONE_TOWER",
						"MM_STONE_TOWER"
					},
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 410, 16, 40, "MM Stone Tower Owl Statue", "Stone Tower Owl Statue"),
                        M("Pot.png", 386, 24, 24, "MM Stone Tower Pot Owl Statue 1", "Stone Tower Pot Owl Statue 1"),
                        M("Pot.png", 450, 24, 24, "MM Stone Tower Pot Owl Statue 2", "Stone Tower Pot Owl Statue 2"),
                        M("Pot.png", 472, 24, 24, "MM Stone Tower Pot Owl Statue 3", "Stone Tower Pot Owl Statue 3"),
                        M("Pot.png", 364, 24, 24, "MM Stone Tower Pot Owl Statue 4", "Stone Tower Pot Owl Statue 4"),
                        M("Pot.png", 568, 486, 24, "MM Stone Tower Pot Climb 1", "Stone Tower Pot Climb 1"),
                        M("Pot.png", 568, 510, 24, "MM Stone Tower Pot Climb 2", "Stone Tower Pot Climb 2"),
                        M("Pot.png", 145, 408, 24, "MM Stone Tower Pot Higher Scarecrow 1", "Stone Tower Pot Higher Scarecrow 1"),
                        M("Pot.png", 145, 368, 24, "MM Stone Tower Pot Higher Scarecrow 2", "Stone Tower Pot Higher Scarecrow 2"),
                        M("Pot.png", 145, 388, 24, "MM Stone Tower Pot Higher Scarecrow 3", "Stone Tower Pot Higher Scarecrow 3"),
                        M("Pot.png", 116, 343, 24, "MM Stone Tower Pot Higher Scarecrow 4", "Stone Tower Pot Higher Scarecrow 4"),
                        M("Pot.png", 116, 323, 24, "MM Stone Tower Pot Higher Scarecrow 5", "Stone Tower Pot Higher Scarecrow 5"),
                        M("Pot.png", 116, 303, 24, "MM Stone Tower Pot Higher Scarecrow 6", "Stone Tower Pot Higher Scarecrow 6"),
                        M("Pot.png", 122, 278, 24, "MM Stone Tower Pot Higher Scarecrow 7", "Stone Tower Pot Higher Scarecrow 7"),
                        M("Pot.png", 122, 258, 24, "MM Stone Tower Pot Higher Scarecrow 8", "Stone Tower Pot Higher Scarecrow 8"),
                        M("Pot.png", 122, 238, 24, "MM Stone Tower Pot Higher Scarecrow 9", "Stone Tower Pot Higher Scarecrow 9"),
                        M("Pot.png", 695, 510, 24, "MM Stone Tower Pot Lower Scarecrow 01", "Stone Tower Pot Lower Scarecrow 01"),
                        M("Pot.png", 691, 445, 24, "MM Stone Tower Pot Lower Scarecrow 02", "Stone Tower Pot Lower Scarecrow 02"),
                        M("Pot.png", 691, 465, 24, "MM Stone Tower Pot Lower Scarecrow 03", "Stone Tower Pot Lower Scarecrow 03"),
                        M("Pot.png", 691, 485, 24, "MM Stone Tower Pot Lower Scarecrow 04", "Stone Tower Pot Lower Scarecrow 04"),
                        M("Pot.png", 687, 400, 24, "MM Stone Tower Pot Lower Scarecrow 05", "Stone Tower Pot Lower Scarecrow 05"),
                        M("Pot.png", 687, 420, 24, "MM Stone Tower Pot Lower Scarecrow 06", "Stone Tower Pot Lower Scarecrow 06"),
                        M("Pot.png", 687, 380, 24, "MM Stone Tower Pot Lower Scarecrow 07", "Stone Tower Pot Lower Scarecrow 07"),
                        M("Pot.png", 683, 315, 24, "MM Stone Tower Pot Lower Scarecrow 08", "Stone Tower Pot Lower Scarecrow 08"),
                        M("Pot.png", 683, 335, 24, "MM Stone Tower Pot Lower Scarecrow 09", "Stone Tower Pot Lower Scarecrow 09"),
                        M("Pot.png", 695, 530, 24, "MM Stone Tower Pot Lower Scarecrow 10", "Stone Tower Pot Lower Scarecrow 10"),
                        M("Pot.png", 695, 550, 24, "MM Stone Tower Pot Lower Scarecrow 11", "Stone Tower Pot Lower Scarecrow 11"),
                        M("Pot.png", 683, 355, 24, "MM Stone Tower Pot Lower Scarecrow 12", "Stone Tower Pot Lower Scarecrow 12"),
						
						ME("Entrance.png", 445, 314, "Entrance shuffle (Ikana Canyon)", "MM_IKANA_CANYON_FROM_STONE_TOWER"),
						ME("Entrance.png", 429, 560, "Entrance shuffle (Stone Tower Temple)", "MM_TEMPLE_STONE_TOWER")
                    }
                },
                new MapSubRegion
                {
                    Name = "Inverted Stone Tower",
                    BackgroundImage = MM("Stone_Tower", "Tower_Inverted"),
                    DestinationEntranceIds = new List<string> { "MM_STONE_TOWER_INVERTED_FROM_TEMPLE" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 249, 323, 40, "MM Stone Tower Inverted Chest 1", "Stone Tower Inverted Chest 1"),
                        M("Chest.png", 269, 360, 40, "MM Stone Tower Inverted Chest 2", "Stone Tower Inverted Chest 2"),
                        M("Chest.png", 289, 397, 40, "MM Stone Tower Inverted Chest 3", "Stone Tower Inverted Chest 3"),
                        M("Soil.png", 191, 277, 24, "MM Stone Tower Inverted Soil Lower", "Stone Tower Inverted Soil Lower"),
                        M("Soil.png", 259, 270, 24, "MM Stone Tower Inverted Soil Upper", "Stone Tower Inverted Soil Upper"),
                        M("Rock.png", 292, 253, 24, "MM Stone Tower Inverted Rock 1", "Stone Tower Inverted Rock 1"),
                        M("Rock.png", 251, 568, 24, "MM Stone Tower Inverted Rock 2", "Stone Tower Inverted Rock 2"),
                        M("Pot.png", 348, 348, 24, "MM Stone Tower Inverted Pot 1", "Stone Tower Inverted Pot 1"),
                        M("Pot.png", 329, 314, 24, "MM Stone Tower Inverted Pot 2", "Stone Tower Inverted Pot 2"),
                        M("Pot.png", 322, 341, 24, "MM Stone Tower Inverted Pot 3", "Stone Tower Inverted Pot 3"),
                        M("Pot.png", 296, 334, 24, "MM Stone Tower Inverted Pot 4", "Stone Tower Inverted Pot 4"),
                        M("Pot.png", 315, 368, 24, "MM Stone Tower Inverted Pot 5", "Stone Tower Inverted Pot 5"),
						
						ME("Entrance.png", 760, 150, "Entrance shuffle (Inverted Stone Tower Temple)", "MM_TEMPLE_STONE_TOWER_INVERTED")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion TerminaField()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Termina Field";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Termina Field",
                    BackgroundImage = MM("Termina", "Field"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_GROTTO_EXIT_GOSSIPS_SWAMP",
						"MM_GROTTO_EXIT_GOSSIPS_OCEAN",
						"MM_GROTTO_EXIT_BIO_BABA",
						"MM_GROTTO_EXIT_GOSSIPS_MOUNTAIN",
						"MM_GROTTO_EXIT_DODONGO",
						"MM_TERMINA_FIELD_FROM_GREAT_BAY_COAST",
						"MM_GROTTO_EXIT_GENERIC_GRASS",
						"MM_GROTTO_EXIT_COW_FIELD",
						"MM_GROTTO_EXIT_GENERIC_FIELD_PILLAR",
						"MM_GROTTO_EXIT_SCRUB",
						"MM_FIELD_FROM_ASTRAL_OBSERVATORY",
						"MM_GROTTO_EXIT_PEAHAT",
						"MM_GROTTO_EXIT_GOSSIPS_CANYON",
						"MM_TERMINA_FIELD_FROM_CLOCK_TOWN_WEST",
						"MM_TERMINA_FIELD_FROM_CLOCK_TOWN_SOUTH",
						"MM_TERMINA_FIELD_FROM_CLOCK_TOWN_EAST",
						"MM_TERMINA_FIELD_FROM_CLOCK_TOWN_NORTH",
						"MM_TERMINA_FIELD_FROM_ROAD_TO_IKANA",
						"MM_TERMINA_FIELD_FROM_PATH_TO_MOUNTAIN_VILLAGE",
						"MM_TERMINA_FIELD_FROM_ROAD_TO_SWAMP",
						"MM_TERMINA_FIELD_FROM_MILK_ROAD"
					},
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 2181, 1054, 40, "MM Astral Observatory Moon Tear", "Astral Observatory Moon Tear"),
                        M("Butterfly.png", 1118, 1445, 24, "MM Termina Field Butterfly 1", "Termina Field Butterfly 1"),
                        M("Butterfly.png", 1112, 1399, 24, "MM Termina Field Butterfly 2", "Termina Field Butterfly 2"),
                        M("NPC.png", 1064, 301, 40, "MM Termina Field Kamaro Mask", "Termina Field Kamaro Mask"),
                        M("Pot.png", 2257, 613, 24, "MM Termina Field Pot", "Termina Field Pot"),
                        M("Rock.png", 1072, 341, 24, "MM Termina Field Rock Circle Rock 1", "Termina Field Rock Circle Rock 1"),
                        M("Rock.png", 1096, 333, 24, "MM Termina Field Rock Circle Rock 2", "Termina Field Rock Circle Rock 2"),
                        M("Rock.png", 1104, 309, 24, "MM Termina Field Rock Circle Rock 3", "Termina Field Rock Circle Rock 3"),
                        M("Rock.png", 1096, 285, 24, "MM Termina Field Rock Circle Rock 4", "Termina Field Rock Circle Rock 4"),
                        M("Rock.png", 1072, 277, 24, "MM Termina Field Rock Circle Rock 5", "Termina Field Rock Circle Rock 5"),
                        M("Rock.png", 1048, 285, 24, "MM Termina Field Rock Circle Rock 6", "Termina Field Rock Circle Rock 6"),
                        M("Rock.png", 1040, 309, 24, "MM Termina Field Rock Circle Rock 7", "Termina Field Rock Circle Rock 7"),
                        M("Rock.png", 1048, 333, 24, "MM Termina Field Rock Circle Rock 8", "Termina Field Rock Circle Rock 8"),
                        M("Rock.png", 349, 697, 24, "MM Termina Field Rock Near Ocean", "Termina Field Rock Near Ocean"),
                        M("Rock.png", 916, 571, 24, "MM Termina Field Rock Wall Mountain 1", "Termina Field Rock Wall Mountain 1"),
                        M("Rock.png", 930, 553, 24, "MM Termina Field Rock Wall Mountain 2", "Termina Field Rock Wall Mountain 2"),
                        M("Rock.png", 944, 535, 24, "MM Termina Field Rock Wall Mountain 3", "Termina Field Rock Wall Mountain 3"),
                        M("Rock.png", 958, 517, 24, "MM Termina Field Rock Wall Mountain 4", "Termina Field Rock Wall Mountain 4"),
                        M("Rock.png", 972, 499, 24, "MM Termina Field Rock Wall Mountain 5", "Termina Field Rock Wall Mountain 5"),
                        M("Rock.png", 986, 481, 24, "MM Termina Field Rock Wall Mountain 6", "Termina Field Rock Wall Mountain 6"),
                        M("Rock.png", 1000, 463, 24, "MM Termina Field Rock Wall Mountain 7", "Termina Field Rock Wall Mountain 7"),
                        M("Rock.png", 970, 1098, 24, "MM Termina Field Rock Wall Ocean 1", "Termina Field Rock Wall Ocean 1"),
                        M("Rock.png", 984, 1116, 24, "MM Termina Field Rock Wall Ocean 2", "Termina Field Rock Wall Ocean 2"),
                        M("Rock.png", 998, 1134, 24, "MM Termina Field Rock Wall Ocean 3", "Termina Field Rock Wall Ocean 3"),
                        M("Rock.png", 1012, 1152, 24, "MM Termina Field Rock Wall Ocean 4", "Termina Field Rock Wall Ocean 4"),
                        M("Rock.png", 1026, 1170, 24, "MM Termina Field Rock Wall Ocean 5", "Termina Field Rock Wall Ocean 5"),
                        M("Rupee.png", 2160, 635, 24, "MM Termina Field Rupee", "Termina Field Rupee"),
                        M("Soil.png", 2256, 1138, 24, "MM Termina Field Soil Observatory Item 1", "Termina Field Soil Observatory Item 1"),
                        M("Soil.png", 2206, 1120, 24, "MM Termina Field Soil Observatory Item 2", "Termina Field Soil Observatory Item 2"),
                        M("Soil.png", 2229, 1138, 24, "MM Termina Field Soil Observatory Item 3", "Termina Field Soil Observatory Item 3"),
                        M("Soil.png", 1645, 1056, 24, "MM Termina Field Soil Wall Item 1", "Termina Field Soil Wall Item 1"),
                        M("Soil.png", 1595, 1080, 24, "MM Termina Field Soil Wall Item 2", "Termina Field Soil Wall Item 2"),
                        M("Soil.png", 1620, 1068, 24, "MM Termina Field Soil Wall Item 3", "Termina Field Soil Wall Item 3"),
                        M("Soil.png", 1938, 653, 24, "MM Termina Field Soil Pillar", "Termina Field Soil Pillar"),
                        M("Soil.png", 1596, 1496, 24, "MM Termina Field Soil Tree Stump", "Termina Field Soil Tree Stump"),
                        M("Chest.png", 1912, 1518, 40, "MM Termina Field Tall Grass Chest", "Termina Field Tall Grass Chest"),
                        M("Chest.png", 1581, 1456, 40, "MM Termina Field Tree Stump Chest", "Termina Field Tree Stump Chest"),
                        M("Chest.png", 733, 1156, 40, "MM Termina Field Water Chest", "Termina Field Water Chest"),
                        M("Tree.png", 2013, 1079, 24, "MM Termina Field Tree 1", "Termina Field Tree 1"),
                        M("Tree.png", 2121, 991, 24, "MM Termina Field Tree 2", "Termina Field Tree 2"),
                        M("Wonder.png", 910, 671, 24, "MM Termina Field Wonder Item Fountains 1", "Termina Field Wonder Item Fountains 1"),
                        M("Wonder.png", 910, 823, 24, "MM Termina Field Wonder Item Fountains 2", "Termina Field Wonder Item Fountains 2"),
                        M("Wonder.png", 1436, 1728, 24, "MM Termina Field Wonder Item Graffiti 1", "Termina Field Wonder Item Graffiti 1"),
                        M("Wonder.png", 1476, 1728, 24, "MM Termina Field Wonder Item Graffiti 2", "Termina Field Wonder Item Graffiti 2"),
                        M("Wonder.png", 1456, 1725, 24, "MM Termina Field Wonder Item Graffiti 3", "Termina Field Wonder Item Graffiti 3"),
                        M("Wonder.png", 1152, 1385, 24, "MM Termina Field Wonder Item Grass 1", "Termina Field Wonder Item Grass 1"),
                        M("Wonder.png", 1169, 1448, 24, "MM Termina Field Wonder Item Grass 2", "Termina Field Wonder Item Grass 2"),
                        M("Wonder.png", 1328, 1639, 24, "MM Termina Field Wonder Item Grass 3", "Termina Field Wonder Item Grass 3"),
                        M("Wonder.png", 1703, 1615, 24, "MM Termina Field Wonder Item Grass 4", "Termina Field Wonder Item Grass 4"),
                        M("Wonder.png", 1822, 1472, 24, "MM Termina Field Wonder Item Grass 5", "Termina Field Wonder Item Grass 5"),
                        M("Wonder.png", 1469, 1519, 24, "MM Termina Field Wonder Item Hollow Trunk", "Termina Field Wonder Item Hollow Trunk"),
                        M("Wonder.png", 1442, 156, 24, "MM Termina Field Wonder Item North Ramp", "Termina Field Wonder Item North Ramp"),
                        M("Wonder.png", 1078, 715, 24, "MM Termina Field Wonder Item Shell 1", "Termina Field Wonder Item Shell 1"),
                        M("Wonder.png", 1075, 735, 24, "MM Termina Field Wonder Item Shell 2", "Termina Field Wonder Item Shell 2"),
                        M("Wonder.png", 1078, 755, 24, "MM Termina Field Wonder Item Shell 3", "Termina Field Wonder Item Shell 3"),
                        M("Wonder.png", 1081, 688, 24, "MM Termina Field Wonder Item Shell Side 1", "Termina Field Wonder Item Shell Side 1"),
                        M("Wonder.png", 1078, 668, 24, "MM Termina Field Wonder Item Shell Side 2", "Termina Field Wonder Item Shell Side 2"),
                        M("Wonder.png", 1081, 648, 24, "MM Termina Field Wonder Item Shell Side 3", "Termina Field Wonder Item Shell Side 3"),
                        M("Wonder.png", 807, 1114, 24, "MM Termina Field Wonder Item South West Ramp", "Termina Field Wonder Item South West Ramp"),
                        M("Wonder.png", 947, 745, 24, "MM Termina Field Wonder Item West Ramp", "Termina Field Wonder Item West Ramp"),
                        M("Grass.png", 835, 1201, 24, "MM Termina Field Grass Pack 01 Grass 01", "Termina Field Grass Pack 01 Grass 01"),
                        M("Grass.png", 835, 1181, 24, "MM Termina Field Grass Pack 01 Grass 02", "Termina Field Grass Pack 01 Grass 02"),
                        M("Grass.png", 855, 1181, 24, "MM Termina Field Grass Pack 01 Grass 03", "Termina Field Grass Pack 01 Grass 03"),
                        M("Grass.png", 855, 1201, 24, "MM Termina Field Grass Pack 01 Grass 04", "Termina Field Grass Pack 01 Grass 04"),
                        M("Grass.png", 845, 1221, 24, "MM Termina Field Grass Pack 01 Grass 05", "Termina Field Grass Pack 01 Grass 05"),
                        M("Grass.png", 825, 1211, 24, "MM Termina Field Grass Pack 01 Grass 06", "Termina Field Grass Pack 01 Grass 06"),
                        M("Grass.png", 815, 1191, 24, "MM Termina Field Grass Pack 01 Grass 07", "Termina Field Grass Pack 01 Grass 07"),
                        M("Grass.png", 825, 1171, 24, "MM Termina Field Grass Pack 01 Grass 08", "Termina Field Grass Pack 01 Grass 08"),
                        M("Grass.png", 845, 1161, 24, "MM Termina Field Grass Pack 01 Grass 09", "Termina Field Grass Pack 01 Grass 09"),
                        M("Grass.png", 865, 1171, 24, "MM Termina Field Grass Pack 01 Grass 10", "Termina Field Grass Pack 01 Grass 10"),
                        M("Grass.png", 875, 1191, 24, "MM Termina Field Grass Pack 01 Grass 11", "Termina Field Grass Pack 01 Grass 11"),
                        M("Grass.png", 865, 1211, 24, "MM Termina Field Grass Pack 01 Grass 12", "Termina Field Grass Pack 01 Grass 12"),
                        M("Grass.png", 987, 1303, 24, "MM Termina Field Grass Pack 02 Grass 01", "Termina Field Grass Pack 02 Grass 01"),
                        M("Grass.png", 987, 1283, 24, "MM Termina Field Grass Pack 02 Grass 02", "Termina Field Grass Pack 02 Grass 02"),
                        M("Grass.png", 1007, 1283, 24, "MM Termina Field Grass Pack 02 Grass 03", "Termina Field Grass Pack 02 Grass 03"),
                        M("Grass.png", 1007, 1303, 24, "MM Termina Field Grass Pack 02 Grass 04", "Termina Field Grass Pack 02 Grass 04"),
                        M("Grass.png", 997, 1323, 24, "MM Termina Field Grass Pack 02 Grass 05", "Termina Field Grass Pack 02 Grass 05"),
                        M("Grass.png", 977, 1313, 24, "MM Termina Field Grass Pack 02 Grass 06", "Termina Field Grass Pack 02 Grass 06"),
                        M("Grass.png", 967, 1293, 24, "MM Termina Field Grass Pack 02 Grass 07", "Termina Field Grass Pack 02 Grass 07"),
                        M("Grass.png", 977, 1273, 24, "MM Termina Field Grass Pack 02 Grass 08", "Termina Field Grass Pack 02 Grass 08"),
                        M("Grass.png", 997, 1263, 24, "MM Termina Field Grass Pack 02 Grass 09", "Termina Field Grass Pack 02 Grass 09"),
                        M("Grass.png", 1017, 1273, 24, "MM Termina Field Grass Pack 02 Grass 10", "Termina Field Grass Pack 02 Grass 10"),
                        M("Grass.png", 1027, 1293, 24, "MM Termina Field Grass Pack 02 Grass 11", "Termina Field Grass Pack 02 Grass 11"),
                        M("Grass.png", 1017, 1313, 24, "MM Termina Field Grass Pack 02 Grass 12", "Termina Field Grass Pack 02 Grass 12"),
                        M("Grass.png", 1015, 1085, 24, "MM Termina Field Grass Pack 03 Grass 01", "Termina Field Grass Pack 03 Grass 01"),
                        M("Grass.png", 1015, 1065, 24, "MM Termina Field Grass Pack 03 Grass 02", "Termina Field Grass Pack 03 Grass 02"),
                        M("Grass.png", 1035, 1065, 24, "MM Termina Field Grass Pack 03 Grass 03", "Termina Field Grass Pack 03 Grass 03"),
                        M("Grass.png", 1035, 1085, 24, "MM Termina Field Grass Pack 03 Grass 04", "Termina Field Grass Pack 03 Grass 04"),
                        M("Grass.png", 1025, 1105, 24, "MM Termina Field Grass Pack 03 Grass 05", "Termina Field Grass Pack 03 Grass 05"),
                        M("Grass.png", 1005, 1095, 24, "MM Termina Field Grass Pack 03 Grass 06", "Termina Field Grass Pack 03 Grass 06"),
                        M("Grass.png", 995, 1075, 24, "MM Termina Field Grass Pack 03 Grass 07", "Termina Field Grass Pack 03 Grass 07"),
                        M("Grass.png", 1005, 1055, 24, "MM Termina Field Grass Pack 03 Grass 08", "Termina Field Grass Pack 03 Grass 08"),
                        M("Grass.png", 1025, 1045, 24, "MM Termina Field Grass Pack 03 Grass 09", "Termina Field Grass Pack 03 Grass 09"),
                        M("Grass.png", 1045, 1055, 24, "MM Termina Field Grass Pack 03 Grass 10", "Termina Field Grass Pack 03 Grass 10"),
                        M("Grass.png", 1055, 1075, 24, "MM Termina Field Grass Pack 03 Grass 11", "Termina Field Grass Pack 03 Grass 11"),
                        M("Grass.png", 1045, 1095, 24, "MM Termina Field Grass Pack 03 Grass 12", "Termina Field Grass Pack 03 Grass 12"),
                        M("Grass.png", 1011, 610, 24, "MM Termina Field Grass Pack 04 Grass 01", "Termina Field Grass Pack 04 Grass 01"),
                        M("Grass.png", 1011, 590, 24, "MM Termina Field Grass Pack 04 Grass 02", "Termina Field Grass Pack 04 Grass 02"),
                        M("Grass.png", 1031, 590, 24, "MM Termina Field Grass Pack 04 Grass 03", "Termina Field Grass Pack 04 Grass 03"),
                        M("Grass.png", 1031, 610, 24, "MM Termina Field Grass Pack 04 Grass 04", "Termina Field Grass Pack 04 Grass 04"),
                        M("Grass.png", 1021, 630, 24, "MM Termina Field Grass Pack 04 Grass 05", "Termina Field Grass Pack 04 Grass 05"),
                        M("Grass.png", 1001, 620, 24, "MM Termina Field Grass Pack 04 Grass 06", "Termina Field Grass Pack 04 Grass 06"),
                        M("Grass.png", 991, 600, 24, "MM Termina Field Grass Pack 04 Grass 07", "Termina Field Grass Pack 04 Grass 07"),
                        M("Grass.png", 1001, 580, 24, "MM Termina Field Grass Pack 04 Grass 08", "Termina Field Grass Pack 04 Grass 08"),
                        M("Grass.png", 1021, 570, 24, "MM Termina Field Grass Pack 04 Grass 09", "Termina Field Grass Pack 04 Grass 09"),
                        M("Grass.png", 1041, 580, 24, "MM Termina Field Grass Pack 04 Grass 10", "Termina Field Grass Pack 04 Grass 10"),
                        M("Grass.png", 1051, 600, 24, "MM Termina Field Grass Pack 04 Grass 11", "Termina Field Grass Pack 04 Grass 11"),
                        M("Grass.png", 1041, 620, 24, "MM Termina Field Grass Pack 04 Grass 12", "Termina Field Grass Pack 04 Grass 12"),
                        M("Grass.png", 1037, 1587, 24, "MM Termina Field Grass Pack 05 Grass 01", "Termina Field Grass Pack 05 Grass 01"),
                        M("Grass.png", 1037, 1567, 24, "MM Termina Field Grass Pack 05 Grass 02", "Termina Field Grass Pack 05 Grass 02"),
                        M("Grass.png", 1057, 1567, 24, "MM Termina Field Grass Pack 05 Grass 03", "Termina Field Grass Pack 05 Grass 03"),
                        M("Grass.png", 1057, 1587, 24, "MM Termina Field Grass Pack 05 Grass 04", "Termina Field Grass Pack 05 Grass 04"),
                        M("Grass.png", 1047, 1607, 24, "MM Termina Field Grass Pack 05 Grass 05", "Termina Field Grass Pack 05 Grass 05"),
                        M("Grass.png", 1027, 1597, 24, "MM Termina Field Grass Pack 05 Grass 06", "Termina Field Grass Pack 05 Grass 06"),
                        M("Grass.png", 1017, 1577, 24, "MM Termina Field Grass Pack 05 Grass 07", "Termina Field Grass Pack 05 Grass 07"),
                        M("Grass.png", 1027, 1557, 24, "MM Termina Field Grass Pack 05 Grass 08", "Termina Field Grass Pack 05 Grass 08"),
                        M("Grass.png", 1047, 1547, 24, "MM Termina Field Grass Pack 05 Grass 09", "Termina Field Grass Pack 05 Grass 09"),
                        M("Grass.png", 1067, 1557, 24, "MM Termina Field Grass Pack 05 Grass 10", "Termina Field Grass Pack 05 Grass 10"),
                        M("Grass.png", 1077, 1577, 24, "MM Termina Field Grass Pack 05 Grass 11", "Termina Field Grass Pack 05 Grass 11"),
                        M("Grass.png", 1067, 1597, 24, "MM Termina Field Grass Pack 05 Grass 12", "Termina Field Grass Pack 05 Grass 12"),
                        M("Grass.png", 1262, 347, 24, "MM Termina Field Grass Pack 06 Grass 01", "Termina Field Grass Pack 06 Grass 01"),
                        M("Grass.png", 1262, 327, 24, "MM Termina Field Grass Pack 06 Grass 02", "Termina Field Grass Pack 06 Grass 02"),
                        M("Grass.png", 1282, 327, 24, "MM Termina Field Grass Pack 06 Grass 03", "Termina Field Grass Pack 06 Grass 03"),
                        M("Grass.png", 1282, 347, 24, "MM Termina Field Grass Pack 06 Grass 04", "Termina Field Grass Pack 06 Grass 04"),
                        M("Grass.png", 1272, 367, 24, "MM Termina Field Grass Pack 06 Grass 05", "Termina Field Grass Pack 06 Grass 05"),
                        M("Grass.png", 1252, 357, 24, "MM Termina Field Grass Pack 06 Grass 06", "Termina Field Grass Pack 06 Grass 06"),
                        M("Grass.png", 1242, 337, 24, "MM Termina Field Grass Pack 06 Grass 07", "Termina Field Grass Pack 06 Grass 07"),
                        M("Grass.png", 1252, 317, 24, "MM Termina Field Grass Pack 06 Grass 08", "Termina Field Grass Pack 06 Grass 08"),
                        M("Grass.png", 1272, 307, 24, "MM Termina Field Grass Pack 06 Grass 09", "Termina Field Grass Pack 06 Grass 09"),
                        M("Grass.png", 1292, 317, 24, "MM Termina Field Grass Pack 06 Grass 10", "Termina Field Grass Pack 06 Grass 10"),
                        M("Grass.png", 1302, 337, 24, "MM Termina Field Grass Pack 06 Grass 11", "Termina Field Grass Pack 06 Grass 11"),
                        M("Grass.png", 1292, 357, 24, "MM Termina Field Grass Pack 06 Grass 12", "Termina Field Grass Pack 06 Grass 12"),
                        M("Grass.png", 1265, 1518, 24, "MM Termina Field Grass Pack 07 Grass 01", "Termina Field Grass Pack 07 Grass 01"),
                        M("Grass.png", 1265, 1498, 24, "MM Termina Field Grass Pack 07 Grass 02", "Termina Field Grass Pack 07 Grass 02"),
                        M("Grass.png", 1285, 1498, 24, "MM Termina Field Grass Pack 07 Grass 03", "Termina Field Grass Pack 07 Grass 03"),
                        M("Grass.png", 1285, 1518, 24, "MM Termina Field Grass Pack 07 Grass 04", "Termina Field Grass Pack 07 Grass 04"),
                        M("Grass.png", 1275, 1538, 24, "MM Termina Field Grass Pack 07 Grass 05", "Termina Field Grass Pack 07 Grass 05"),
                        M("Grass.png", 1255, 1528, 24, "MM Termina Field Grass Pack 07 Grass 06", "Termina Field Grass Pack 07 Grass 06"),
                        M("Grass.png", 1245, 1508, 24, "MM Termina Field Grass Pack 07 Grass 07", "Termina Field Grass Pack 07 Grass 07"),
                        M("Grass.png", 1255, 1488, 24, "MM Termina Field Grass Pack 07 Grass 08", "Termina Field Grass Pack 07 Grass 08"),
                        M("Grass.png", 1275, 1478, 24, "MM Termina Field Grass Pack 07 Grass 09", "Termina Field Grass Pack 07 Grass 09"),
                        M("Grass.png", 1295, 1488, 24, "MM Termina Field Grass Pack 07 Grass 10", "Termina Field Grass Pack 07 Grass 10"),
                        M("Grass.png", 1305, 1508, 24, "MM Termina Field Grass Pack 07 Grass 11", "Termina Field Grass Pack 07 Grass 11"),
                        M("Grass.png", 1295, 1528, 24, "MM Termina Field Grass Pack 07 Grass 12", "Termina Field Grass Pack 07 Grass 12"),
                        M("Grass.png", 1463, 1411, 24, "MM Termina Field Grass Pack 08 Grass 01", "Termina Field Grass Pack 08 Grass 01"),
                        M("Grass.png", 1463, 1391, 24, "MM Termina Field Grass Pack 08 Grass 02", "Termina Field Grass Pack 08 Grass 02"),
                        M("Grass.png", 1483, 1391, 24, "MM Termina Field Grass Pack 08 Grass 03", "Termina Field Grass Pack 08 Grass 03"),
                        M("Grass.png", 1483, 1411, 24, "MM Termina Field Grass Pack 08 Grass 04", "Termina Field Grass Pack 08 Grass 04"),
                        M("Grass.png", 1473, 1431, 24, "MM Termina Field Grass Pack 08 Grass 05", "Termina Field Grass Pack 08 Grass 05"),
                        M("Grass.png", 1453, 1421, 24, "MM Termina Field Grass Pack 08 Grass 06", "Termina Field Grass Pack 08 Grass 06"),
                        M("Grass.png", 1443, 1401, 24, "MM Termina Field Grass Pack 08 Grass 07", "Termina Field Grass Pack 08 Grass 07"),
                        M("Grass.png", 1453, 1381, 24, "MM Termina Field Grass Pack 08 Grass 08", "Termina Field Grass Pack 08 Grass 08"),
                        M("Grass.png", 1473, 1371, 24, "MM Termina Field Grass Pack 08 Grass 09", "Termina Field Grass Pack 08 Grass 09"),
                        M("Grass.png", 1493, 1381, 24, "MM Termina Field Grass Pack 08 Grass 10", "Termina Field Grass Pack 08 Grass 10"),
                        M("Grass.png", 1503, 1401, 24, "MM Termina Field Grass Pack 08 Grass 11", "Termina Field Grass Pack 08 Grass 11"),
                        M("Grass.png", 1493, 1421, 24, "MM Termina Field Grass Pack 08 Grass 12", "Termina Field Grass Pack 08 Grass 12"),
                        M("Grass.png", 1644, 365, 24, "MM Termina Field Grass Pack 09 Grass 01", "Termina Field Grass Pack 09 Grass 01"),
                        M("Grass.png", 1644, 345, 24, "MM Termina Field Grass Pack 09 Grass 02", "Termina Field Grass Pack 09 Grass 02"),
                        M("Grass.png", 1664, 345, 24, "MM Termina Field Grass Pack 09 Grass 03", "Termina Field Grass Pack 09 Grass 03"),
                        M("Grass.png", 1664, 365, 24, "MM Termina Field Grass Pack 09 Grass 04", "Termina Field Grass Pack 09 Grass 04"),
                        M("Grass.png", 1654, 385, 24, "MM Termina Field Grass Pack 09 Grass 05", "Termina Field Grass Pack 09 Grass 05"),
                        M("Grass.png", 1634, 375, 24, "MM Termina Field Grass Pack 09 Grass 06", "Termina Field Grass Pack 09 Grass 06"),
                        M("Grass.png", 1624, 355, 24, "MM Termina Field Grass Pack 09 Grass 07", "Termina Field Grass Pack 09 Grass 07"),
                        M("Grass.png", 1634, 335, 24, "MM Termina Field Grass Pack 09 Grass 08", "Termina Field Grass Pack 09 Grass 08"),
                        M("Grass.png", 1654, 325, 24, "MM Termina Field Grass Pack 09 Grass 09", "Termina Field Grass Pack 09 Grass 09"),
                        M("Grass.png", 1674, 335, 24, "MM Termina Field Grass Pack 09 Grass 10", "Termina Field Grass Pack 09 Grass 10"),
                        M("Grass.png", 1684, 355, 24, "MM Termina Field Grass Pack 09 Grass 11", "Termina Field Grass Pack 09 Grass 11"),
                        M("Grass.png", 1674, 375, 24, "MM Termina Field Grass Pack 09 Grass 12", "Termina Field Grass Pack 09 Grass 12"),
                        M("Grass.png", 1744, 1383, 24, "MM Termina Field Grass Pack 10 Grass 01", "Termina Field Grass Pack 10 Grass 01"),
                        M("Grass.png", 1744, 1363, 24, "MM Termina Field Grass Pack 10 Grass 02", "Termina Field Grass Pack 10 Grass 02"),
                        M("Grass.png", 1764, 1363, 24, "MM Termina Field Grass Pack 10 Grass 03", "Termina Field Grass Pack 10 Grass 03"),
                        M("Grass.png", 1764, 1383, 24, "MM Termina Field Grass Pack 10 Grass 04", "Termina Field Grass Pack 10 Grass 04"),
                        M("Grass.png", 1754, 1403, 24, "MM Termina Field Grass Pack 10 Grass 05", "Termina Field Grass Pack 10 Grass 05"),
                        M("Grass.png", 1734, 1393, 24, "MM Termina Field Grass Pack 10 Grass 06", "Termina Field Grass Pack 10 Grass 06"),
                        M("Grass.png", 1724, 1373, 24, "MM Termina Field Grass Pack 10 Grass 07", "Termina Field Grass Pack 10 Grass 07"),
                        M("Grass.png", 1734, 1353, 24, "MM Termina Field Grass Pack 10 Grass 08", "Termina Field Grass Pack 10 Grass 08"),
                        M("Grass.png", 1754, 1343, 24, "MM Termina Field Grass Pack 10 Grass 09", "Termina Field Grass Pack 10 Grass 09"),
                        M("Grass.png", 1774, 1353, 24, "MM Termina Field Grass Pack 10 Grass 10", "Termina Field Grass Pack 10 Grass 10"),
                        M("Grass.png", 1784, 1373, 24, "MM Termina Field Grass Pack 10 Grass 11", "Termina Field Grass Pack 10 Grass 11"),
                        M("Grass.png", 1774, 1393, 24, "MM Termina Field Grass Pack 10 Grass 12", "Termina Field Grass Pack 10 Grass 12"),
                        M("Grass.png", 1781, 1169, 24, "MM Termina Field Grass Pack 11 Grass 01", "Termina Field Grass Pack 11 Grass 01"),
                        M("Grass.png", 1781, 1149, 24, "MM Termina Field Grass Pack 11 Grass 02", "Termina Field Grass Pack 11 Grass 02"),
                        M("Grass.png", 1801, 1149, 24, "MM Termina Field Grass Pack 11 Grass 03", "Termina Field Grass Pack 11 Grass 03"),
                        M("Grass.png", 1801, 1169, 24, "MM Termina Field Grass Pack 11 Grass 04", "Termina Field Grass Pack 11 Grass 04"),
                        M("Grass.png", 1791, 1189, 24, "MM Termina Field Grass Pack 11 Grass 05", "Termina Field Grass Pack 11 Grass 05"),
                        M("Grass.png", 1771, 1179, 24, "MM Termina Field Grass Pack 11 Grass 06", "Termina Field Grass Pack 11 Grass 06"),
                        M("Grass.png", 1761, 1159, 24, "MM Termina Field Grass Pack 11 Grass 07", "Termina Field Grass Pack 11 Grass 07"),
                        M("Grass.png", 1771, 1139, 24, "MM Termina Field Grass Pack 11 Grass 08", "Termina Field Grass Pack 11 Grass 08"),
                        M("Grass.png", 1791, 1129, 24, "MM Termina Field Grass Pack 11 Grass 09", "Termina Field Grass Pack 11 Grass 09"),
                        M("Grass.png", 1811, 1139, 24, "MM Termina Field Grass Pack 11 Grass 10", "Termina Field Grass Pack 11 Grass 10"),
                        M("Grass.png", 1821, 1159, 24, "MM Termina Field Grass Pack 11 Grass 11", "Termina Field Grass Pack 11 Grass 11"),
                        M("Grass.png", 1811, 1179, 24, "MM Termina Field Grass Pack 11 Grass 12", "Termina Field Grass Pack 11 Grass 12"),
                        M("Grass.png", 1837, 1306, 24, "MM Termina Field Grass Pack 12 Grass 01", "Termina Field Grass Pack 12 Grass 01"),
                        M("Grass.png", 1837, 1286, 24, "MM Termina Field Grass Pack 12 Grass 02", "Termina Field Grass Pack 12 Grass 02"),
                        M("Grass.png", 1857, 1286, 24, "MM Termina Field Grass Pack 12 Grass 03", "Termina Field Grass Pack 12 Grass 03"),
                        M("Grass.png", 1857, 1306, 24, "MM Termina Field Grass Pack 12 Grass 04", "Termina Field Grass Pack 12 Grass 04"),
                        M("Grass.png", 1847, 1326, 24, "MM Termina Field Grass Pack 12 Grass 05", "Termina Field Grass Pack 12 Grass 05"),
                        M("Grass.png", 1827, 1316, 24, "MM Termina Field Grass Pack 12 Grass 06", "Termina Field Grass Pack 12 Grass 06"),
                        M("Grass.png", 1817, 1296, 24, "MM Termina Field Grass Pack 12 Grass 07", "Termina Field Grass Pack 12 Grass 07"),
                        M("Grass.png", 1827, 1276, 24, "MM Termina Field Grass Pack 12 Grass 08", "Termina Field Grass Pack 12 Grass 08"),
                        M("Grass.png", 1847, 1266, 24, "MM Termina Field Grass Pack 12 Grass 09", "Termina Field Grass Pack 12 Grass 09"),
                        M("Grass.png", 1867, 1276, 24, "MM Termina Field Grass Pack 12 Grass 10", "Termina Field Grass Pack 12 Grass 10"),
                        M("Grass.png", 1877, 1296, 24, "MM Termina Field Grass Pack 12 Grass 11", "Termina Field Grass Pack 12 Grass 11"),
                        M("Grass.png", 1867, 1316, 24, "MM Termina Field Grass Pack 12 Grass 12", "Termina Field Grass Pack 12 Grass 12"),
                        M("Grass.png", 1841, 566, 24, "MM Termina Field Grass Pack 13 Grass 01", "Termina Field Grass Pack 13 Grass 01"),
                        M("Grass.png", 1841, 546, 24, "MM Termina Field Grass Pack 13 Grass 02", "Termina Field Grass Pack 13 Grass 02"),
                        M("Grass.png", 1861, 546, 24, "MM Termina Field Grass Pack 13 Grass 03", "Termina Field Grass Pack 13 Grass 03"),
                        M("Grass.png", 1861, 566, 24, "MM Termina Field Grass Pack 13 Grass 04", "Termina Field Grass Pack 13 Grass 04"),
                        M("Grass.png", 1851, 586, 24, "MM Termina Field Grass Pack 13 Grass 05", "Termina Field Grass Pack 13 Grass 05"),
                        M("Grass.png", 1831, 576, 24, "MM Termina Field Grass Pack 13 Grass 06", "Termina Field Grass Pack 13 Grass 06"),
                        M("Grass.png", 1821, 556, 24, "MM Termina Field Grass Pack 13 Grass 07", "Termina Field Grass Pack 13 Grass 07"),
                        M("Grass.png", 1831, 536, 24, "MM Termina Field Grass Pack 13 Grass 08", "Termina Field Grass Pack 13 Grass 08"),
                        M("Grass.png", 1851, 526, 24, "MM Termina Field Grass Pack 13 Grass 09", "Termina Field Grass Pack 13 Grass 09"),
                        M("Grass.png", 1871, 536, 24, "MM Termina Field Grass Pack 13 Grass 10", "Termina Field Grass Pack 13 Grass 10"),
                        M("Grass.png", 1881, 556, 24, "MM Termina Field Grass Pack 13 Grass 11", "Termina Field Grass Pack 13 Grass 11"),
                        M("Grass.png", 1871, 576, 24, "MM Termina Field Grass Pack 13 Grass 12", "Termina Field Grass Pack 13 Grass 12"),
                        M("Grass.png", 1962, 919, 24, "MM Termina Field Grass Pack 14 Grass 01", "Termina Field Grass Pack 14 Grass 01"),
                        M("Grass.png", 1962, 899, 24, "MM Termina Field Grass Pack 14 Grass 02", "Termina Field Grass Pack 14 Grass 02"),
                        M("Grass.png", 1982, 899, 24, "MM Termina Field Grass Pack 14 Grass 03", "Termina Field Grass Pack 14 Grass 03"),
                        M("Grass.png", 1982, 919, 24, "MM Termina Field Grass Pack 14 Grass 04", "Termina Field Grass Pack 14 Grass 04"),
                        M("Grass.png", 1972, 939, 24, "MM Termina Field Grass Pack 14 Grass 05", "Termina Field Grass Pack 14 Grass 05"),
                        M("Grass.png", 1952, 929, 24, "MM Termina Field Grass Pack 14 Grass 06", "Termina Field Grass Pack 14 Grass 06"),
                        M("Grass.png", 1942, 909, 24, "MM Termina Field Grass Pack 14 Grass 07", "Termina Field Grass Pack 14 Grass 07"),
                        M("Grass.png", 1952, 889, 24, "MM Termina Field Grass Pack 14 Grass 08", "Termina Field Grass Pack 14 Grass 08"),
                        M("Grass.png", 1972, 879, 24, "MM Termina Field Grass Pack 14 Grass 09", "Termina Field Grass Pack 14 Grass 09"),
                        M("Grass.png", 1992, 889, 24, "MM Termina Field Grass Pack 14 Grass 10", "Termina Field Grass Pack 14 Grass 10"),
                        M("Grass.png", 2002, 909, 24, "MM Termina Field Grass Pack 14 Grass 11", "Termina Field Grass Pack 14 Grass 11"),
                        M("Grass.png", 1992, 929, 24, "MM Termina Field Grass Pack 14 Grass 12", "Termina Field Grass Pack 14 Grass 12"),
                        M("Grass.png", 2006, 1218, 24, "MM Termina Field Grass Pack 15 Grass 01", "Termina Field Grass Pack 15 Grass 01"),
                        M("Grass.png", 2006, 1198, 24, "MM Termina Field Grass Pack 15 Grass 02", "Termina Field Grass Pack 15 Grass 02"),
                        M("Grass.png", 2026, 1198, 24, "MM Termina Field Grass Pack 15 Grass 03", "Termina Field Grass Pack 15 Grass 03"),
                        M("Grass.png", 2026, 1218, 24, "MM Termina Field Grass Pack 15 Grass 04", "Termina Field Grass Pack 15 Grass 04"),
                        M("Grass.png", 2016, 1238, 24, "MM Termina Field Grass Pack 15 Grass 05", "Termina Field Grass Pack 15 Grass 05"),
                        M("Grass.png", 1996, 1228, 24, "MM Termina Field Grass Pack 15 Grass 06", "Termina Field Grass Pack 15 Grass 06"),
                        M("Grass.png", 1986, 1208, 24, "MM Termina Field Grass Pack 15 Grass 07", "Termina Field Grass Pack 15 Grass 07"),
                        M("Grass.png", 1996, 1188, 24, "MM Termina Field Grass Pack 15 Grass 08", "Termina Field Grass Pack 15 Grass 08"),
                        M("Grass.png", 2016, 1178, 24, "MM Termina Field Grass Pack 15 Grass 09", "Termina Field Grass Pack 15 Grass 09"),
                        M("Grass.png", 2036, 1188, 24, "MM Termina Field Grass Pack 15 Grass 10", "Termina Field Grass Pack 15 Grass 10"),
                        M("Grass.png", 2046, 1208, 24, "MM Termina Field Grass Pack 15 Grass 11", "Termina Field Grass Pack 15 Grass 11"),
                        M("Grass.png", 2036, 1228, 24, "MM Termina Field Grass Pack 15 Grass 12", "Termina Field Grass Pack 15 Grass 12"),
                        M("Grass.png", 2052, 1129, 24, "MM Termina Field Grass Pack 16 Grass 01", "Termina Field Grass Pack 16 Grass 01"),
                        M("Grass.png", 2052, 1109, 24, "MM Termina Field Grass Pack 16 Grass 02", "Termina Field Grass Pack 16 Grass 02"),
                        M("Grass.png", 2072, 1109, 24, "MM Termina Field Grass Pack 16 Grass 03", "Termina Field Grass Pack 16 Grass 03"),
                        M("Grass.png", 2072, 1129, 24, "MM Termina Field Grass Pack 16 Grass 04", "Termina Field Grass Pack 16 Grass 04"),
                        M("Grass.png", 2062, 1149, 24, "MM Termina Field Grass Pack 16 Grass 05", "Termina Field Grass Pack 16 Grass 05"),
                        M("Grass.png", 2042, 1139, 24, "MM Termina Field Grass Pack 16 Grass 06", "Termina Field Grass Pack 16 Grass 06"),
                        M("Grass.png", 2032, 1119, 24, "MM Termina Field Grass Pack 16 Grass 07", "Termina Field Grass Pack 16 Grass 07"),
                        M("Grass.png", 2042, 1099, 24, "MM Termina Field Grass Pack 16 Grass 08", "Termina Field Grass Pack 16 Grass 08"),
                        M("Grass.png", 2062, 1089, 24, "MM Termina Field Grass Pack 16 Grass 09", "Termina Field Grass Pack 16 Grass 09"),
                        M("Grass.png", 2082, 1099, 24, "MM Termina Field Grass Pack 16 Grass 10", "Termina Field Grass Pack 16 Grass 10"),
                        M("Grass.png", 2092, 1119, 24, "MM Termina Field Grass Pack 16 Grass 11", "Termina Field Grass Pack 16 Grass 11"),
                        M("Grass.png", 2082, 1139, 24, "MM Termina Field Grass Pack 16 Grass 12", "Termina Field Grass Pack 16 Grass 12"),
                        M("Grass.png", 2208, 929, 24, "MM Termina Field Grass Pack 17 Grass 01", "Termina Field Grass Pack 17 Grass 01"),
                        M("Grass.png", 2208, 909, 24, "MM Termina Field Grass Pack 17 Grass 02", "Termina Field Grass Pack 17 Grass 02"),
                        M("Grass.png", 2228, 909, 24, "MM Termina Field Grass Pack 17 Grass 03", "Termina Field Grass Pack 17 Grass 03"),
                        M("Grass.png", 2228, 929, 24, "MM Termina Field Grass Pack 17 Grass 04", "Termina Field Grass Pack 17 Grass 04"),
                        M("Grass.png", 2218, 949, 24, "MM Termina Field Grass Pack 17 Grass 05", "Termina Field Grass Pack 17 Grass 05"),
                        M("Grass.png", 2198, 939, 24, "MM Termina Field Grass Pack 17 Grass 06", "Termina Field Grass Pack 17 Grass 06"),
                        M("Grass.png", 2188, 919, 24, "MM Termina Field Grass Pack 17 Grass 07", "Termina Field Grass Pack 17 Grass 07"),
                        M("Grass.png", 2198, 899, 24, "MM Termina Field Grass Pack 17 Grass 08", "Termina Field Grass Pack 17 Grass 08"),
                        M("Grass.png", 2218, 889, 24, "MM Termina Field Grass Pack 17 Grass 09", "Termina Field Grass Pack 17 Grass 09"),
                        M("Grass.png", 2238, 899, 24, "MM Termina Field Grass Pack 17 Grass 10", "Termina Field Grass Pack 17 Grass 10"),
                        M("Grass.png", 2248, 919, 24, "MM Termina Field Grass Pack 17 Grass 11", "Termina Field Grass Pack 17 Grass 11"),
                        M("Grass.png", 2238, 939, 24, "MM Termina Field Grass Pack 17 Grass 12", "Termina Field Grass Pack 17 Grass 12"),
                        M("Grass.png", 2274, 877, 24, "MM Termina Field Grass Pack 18 Grass 01", "Termina Field Grass Pack 18 Grass 01"),
                        M("Grass.png", 2274, 857, 24, "MM Termina Field Grass Pack 18 Grass 02", "Termina Field Grass Pack 18 Grass 02"),
                        M("Grass.png", 2294, 857, 24, "MM Termina Field Grass Pack 18 Grass 03", "Termina Field Grass Pack 18 Grass 03"),
                        M("Grass.png", 2294, 877, 24, "MM Termina Field Grass Pack 18 Grass 04", "Termina Field Grass Pack 18 Grass 04"),
                        M("Grass.png", 2284, 897, 24, "MM Termina Field Grass Pack 18 Grass 05", "Termina Field Grass Pack 18 Grass 05"),
                        M("Grass.png", 2264, 887, 24, "MM Termina Field Grass Pack 18 Grass 06", "Termina Field Grass Pack 18 Grass 06"),
                        M("Grass.png", 2254, 867, 24, "MM Termina Field Grass Pack 18 Grass 07", "Termina Field Grass Pack 18 Grass 07"),
                        M("Grass.png", 2264, 847, 24, "MM Termina Field Grass Pack 18 Grass 08", "Termina Field Grass Pack 18 Grass 08"),
                        M("Grass.png", 2284, 837, 24, "MM Termina Field Grass Pack 18 Grass 09", "Termina Field Grass Pack 18 Grass 09"),
                        M("Grass.png", 2304, 847, 24, "MM Termina Field Grass Pack 18 Grass 10", "Termina Field Grass Pack 18 Grass 10"),
                        M("Grass.png", 2314, 867, 24, "MM Termina Field Grass Pack 18 Grass 11", "Termina Field Grass Pack 18 Grass 11"),
                        M("Grass.png", 2304, 887, 24, "MM Termina Field Grass Pack 18 Grass 12", "Termina Field Grass Pack 18 Grass 12"),
						
						ME("Entrance.png", 1449, 1046, "Entrance shuffle (Clock Town South)", "MM_CLOCK_TOWN_SOUTH_FROM_FIELD"),
						ME("Entrance.png", 1434, 407, "Entrance shuffle (Clock Town North)", "MM_CLOCK_TOWN_NORTH_FROM_FIELD"),
						ME("Entrance.png", 1115, 722, "Entrance shuffle (Clock Town West)", "MM_CLOCK_TOWN_WEST_FROM_FIELD"),
						ME("Entrance.png", 1768, 711, "Entrance shuffle (Clock Town East)", "MM_CLOCK_TOWN_EAST_FROM_FIELD"),
						ME("Entrance.png", 2246, 1035, "Entrance shuffle (Astral Observatory)", "MM_ASTRAL_OBSERVATORY_FROM_FIELD"),
						ME("Entrance.png", 1491, 1780, "Entrance shuffle (Road to Southern Swamp)", "MM_SWAMP_ROAD_FROM_FIELD"),
						ME("Entrance.png", 1470, 15, "Entrance shuffle (Path to Mountain Village)", "MM_MOUNTAIN_VILLAGE_PATH_FROM_TERMINA_FIELD"),
						ME("Entrance.png", 243, 689, "Entrance shuffle (Great Bay Coast)", "MM_GREAT_BAY_COAST_FROM_FIELD"),
						ME("Entrance.png", 2403, 638, "Entrance shuffle (Road to Ikana)", "MM_IKANA_ROAD_FROM_FIELD"),
						ME("Entrance.png", 801, 1574, "Entrance shuffle (Milk Road)", "MM_MILK_ROAD_FROM_FIELD"),
						ME("Entrance.png", 1460, 1548, "Entrance shuffle (Cow Grotto)", "MM_GROTTO_COW_FIELD"),
						ME("Entrance.png", 1716, 1447, "Entrance shuffle (Tall Grass Grotto)", "MM_GROTTO_GENERIC_GRASS"),
						ME("Entrance.png", 1938, 752, "Entrance shuffle (Pillar Grotto)", "MM_GROTTO_GENERIC_FIELD_PILLAR"),
						ME("Entrance.png", 2069, 1022, "Entrance shuffle (Deku Scrub Grotto)", "MM_GROTTO_SCRUB"),
						ME("Entrance.png", 1136, 1416, "Entrance shuffle (Peahat Grotto)", "MM_GROTTO_PEAHAT"),
						ME("Entrance.png", 1142, 289, "Entrance shuffle (Dodongo Grotto)", "MM_GROTTO_DODONGO"),
						ME("Entrance.png", 685, 701, "Entrance shuffle (Bio Baba Grotto)", "MM_GROTTO_BIO_BABA"),
						ME("Entrance.png", 1231, 1624, "Entrance shuffle (Swamp Gossip Stone Grotto)", "MM_GROTTO_GOSSIPS_SWAMP"),
						ME("Entrance.png", 1547, 282, "Entrance shuffle (Mountain Gossip Stone Grotto)", "MM_GROTTO_GOSSIPS_MOUNTAIN"),
						ME("Entrance.png", 1010, 519, "Entrance shuffle (Ocean Gossip Stone Grotto)", "MM_GROTTO_GOSSIPS_OCEAN"),
						ME("Entrance.png", 2301, 922, "Entrance shuffle (Canyon Gossip Stone Grotto)", "MM_GROTTO_GOSSIPS_CANYON")
                    }
                },
                new MapSubRegion
                {
                    Name = "Cow Grotto",
                    BackgroundImage = MM("Grottos", "Cow"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_COW_FIELD" },
                    Marks = new List<MapMark>
                    {
                        M("Butterfly.png", 127, 492, 24, "MM Termina Field Cow Grotto Butterfly 1", "Termina Field Cow Grotto Butterfly 1"),
                        M("Butterfly.png", 135, 461, 24, "MM Termina Field Cow Grotto Butterfly 2", "Termina Field Cow Grotto Butterfly 2"),
                        M("Butterfly.png", 160, 433, 24, "MM Termina Field Cow Grotto Butterfly 3", "Termina Field Cow Grotto Butterfly 3"),
                        M("Cow.png", 601, 348, 40, "MM Termina Field Cow Back", "Termina Field Cow Back"),
                        M("Cow.png", 577, 387, 40, "MM Termina Field Cow Front", "Termina Field Cow Front"),
                        M("Hive.png", 516, 60, 40, "MM Termina Field Cow Grotto Hive", "Termina Field Cow Grotto Hive"),
                        M("Grass.png", 549, 317, 24, "MM Termina Field Cow Grotto Grass 01", "Termina Field Cow Grotto Grass 01"),
                        M("Grass.png", 549, 301, 24, "MM Termina Field Cow Grotto Grass 02", "Termina Field Cow Grotto Grass 02"),
                        M("Grass.png", 569, 301, 24, "MM Termina Field Cow Grotto Grass 03", "Termina Field Cow Grotto Grass 03"),
                        M("Grass.png", 569, 317, 24, "MM Termina Field Cow Grotto Grass 04", "Termina Field Cow Grotto Grass 04"),
                        M("Grass.png", 559, 333, 24, "MM Termina Field Cow Grotto Grass 05", "Termina Field Cow Grotto Grass 05"),
                        M("Grass.png", 536, 325, 24, "MM Termina Field Cow Grotto Grass 06", "Termina Field Cow Grotto Grass 06"),
                        M("Grass.png", 530, 307, 24, "MM Termina Field Cow Grotto Grass 07", "Termina Field Cow Grotto Grass 07"),
                        M("Grass.png", 536, 293, 24, "MM Termina Field Cow Grotto Grass 08", "Termina Field Cow Grotto Grass 08"),
                        M("Grass.png", 559, 285, 24, "MM Termina Field Cow Grotto Grass 09", "Termina Field Cow Grotto Grass 09"),
                        M("Grass.png", 582, 293, 24, "MM Termina Field Cow Grotto Grass 10", "Termina Field Cow Grotto Grass 10"),
                        M("Grass.png", 588, 307, 24, "MM Termina Field Cow Grotto Grass 11", "Termina Field Cow Grotto Grass 11"),
                        M("Grass.png", 582, 325, 24, "MM Termina Field Cow Grotto Grass 12", "Termina Field Cow Grotto Grass 12"),
                        M("Grass.png", 408, 354, 24, "MM Termina Field Cow Grotto Grass 13", "Termina Field Cow Grotto Grass 13"),
                        M("Grass.png", 408, 338, 24, "MM Termina Field Cow Grotto Grass 14", "Termina Field Cow Grotto Grass 14"),
                        M("Grass.png", 428, 338, 24, "MM Termina Field Cow Grotto Grass 15", "Termina Field Cow Grotto Grass 15"),
                        M("Grass.png", 428, 354, 24, "MM Termina Field Cow Grotto Grass 16", "Termina Field Cow Grotto Grass 16"),
                        M("Grass.png", 418, 370, 24, "MM Termina Field Cow Grotto Grass 17", "Termina Field Cow Grotto Grass 17"),
                        M("Grass.png", 395, 362, 24, "MM Termina Field Cow Grotto Grass 18", "Termina Field Cow Grotto Grass 18"),
                        M("Grass.png", 389, 344, 24, "MM Termina Field Cow Grotto Grass 19", "Termina Field Cow Grotto Grass 19"),
                        M("Grass.png", 395, 330, 24, "MM Termina Field Cow Grotto Grass 20", "Termina Field Cow Grotto Grass 20"),
                        M("Grass.png", 418, 322, 24, "MM Termina Field Cow Grotto Grass 21", "Termina Field Cow Grotto Grass 21"),
                        M("Grass.png", 441, 330, 24, "MM Termina Field Cow Grotto Grass 22", "Termina Field Cow Grotto Grass 22"),
                        M("Grass.png", 447, 344, 24, "MM Termina Field Cow Grotto Grass 23", "Termina Field Cow Grotto Grass 23"),
                        M("Grass.png", 441, 362, 24, "MM Termina Field Cow Grotto Grass 24", "Termina Field Cow Grotto Grass 24"),
                        M("Grass.png", 499, 387, 24, "MM Termina Field Cow Grotto Grass 25", "Termina Field Cow Grotto Grass 25"),
                        M("Grass.png", 499, 371, 24, "MM Termina Field Cow Grotto Grass 26", "Termina Field Cow Grotto Grass 26"),
                        M("Grass.png", 519, 371, 24, "MM Termina Field Cow Grotto Grass 27", "Termina Field Cow Grotto Grass 27"),
                        M("Grass.png", 519, 387, 24, "MM Termina Field Cow Grotto Grass 28", "Termina Field Cow Grotto Grass 28"),
                        M("Grass.png", 509, 403, 24, "MM Termina Field Cow Grotto Grass 29", "Termina Field Cow Grotto Grass 29"),
                        M("Grass.png", 486, 395, 24, "MM Termina Field Cow Grotto Grass 30", "Termina Field Cow Grotto Grass 30"),
                        M("Grass.png", 480, 377, 24, "MM Termina Field Cow Grotto Grass 31", "Termina Field Cow Grotto Grass 31"),
                        M("Grass.png", 486, 363, 24, "MM Termina Field Cow Grotto Grass 32", "Termina Field Cow Grotto Grass 32"),
                        M("Grass.png", 509, 355, 24, "MM Termina Field Cow Grotto Grass 33", "Termina Field Cow Grotto Grass 33"),
                        M("Grass.png", 532, 363, 24, "MM Termina Field Cow Grotto Grass 34", "Termina Field Cow Grotto Grass 34"),
                        M("Grass.png", 538, 377, 24, "MM Termina Field Cow Grotto Grass 35", "Termina Field Cow Grotto Grass 35"),
                        M("Grass.png", 532, 395, 24, "MM Termina Field Cow Grotto Grass 36", "Termina Field Cow Grotto Grass 36"),
                        M("Grass.png", 679, 358, 24, "MM Termina Field Cow Grotto Grass 37", "Termina Field Cow Grotto Grass 37"),
                        M("Grass.png", 679, 342, 24, "MM Termina Field Cow Grotto Grass 38", "Termina Field Cow Grotto Grass 38"),
                        M("Grass.png", 699, 342, 24, "MM Termina Field Cow Grotto Grass 39", "Termina Field Cow Grotto Grass 39"),
                        M("Grass.png", 699, 358, 24, "MM Termina Field Cow Grotto Grass 40", "Termina Field Cow Grotto Grass 40"),
                        M("Grass.png", 689, 374, 24, "MM Termina Field Cow Grotto Grass 41", "Termina Field Cow Grotto Grass 41"),
                        M("Grass.png", 666, 366, 24, "MM Termina Field Cow Grotto Grass 42", "Termina Field Cow Grotto Grass 42"),
                        M("Grass.png", 660, 348, 24, "MM Termina Field Cow Grotto Grass 43", "Termina Field Cow Grotto Grass 43"),
                        M("Grass.png", 666, 334, 24, "MM Termina Field Cow Grotto Grass 44", "Termina Field Cow Grotto Grass 44"),
                        M("Grass.png", 689, 326, 24, "MM Termina Field Cow Grotto Grass 45", "Termina Field Cow Grotto Grass 45"),
                        M("Grass.png", 712, 334, 24, "MM Termina Field Cow Grotto Grass 46", "Termina Field Cow Grotto Grass 46"),
                        M("Grass.png", 718, 348, 24, "MM Termina Field Cow Grotto Grass 47", "Termina Field Cow Grotto Grass 47"),
                        M("Grass.png", 712, 366, 24, "MM Termina Field Cow Grotto Grass 48", "Termina Field Cow Grotto Grass 48"),
                        M("Grass.png", 655, 448, 24, "MM Termina Field Cow Grotto Grass 49", "Termina Field Cow Grotto Grass 49"),
                        M("Grass.png", 655, 432, 24, "MM Termina Field Cow Grotto Grass 50", "Termina Field Cow Grotto Grass 50"),
                        M("Grass.png", 675, 432, 24, "MM Termina Field Cow Grotto Grass 51", "Termina Field Cow Grotto Grass 51"),
                        M("Grass.png", 675, 448, 24, "MM Termina Field Cow Grotto Grass 52", "Termina Field Cow Grotto Grass 52"),
                        M("Grass.png", 665, 464, 24, "MM Termina Field Cow Grotto Grass 53", "Termina Field Cow Grotto Grass 53"),
                        M("Grass.png", 642, 456, 24, "MM Termina Field Cow Grotto Grass 54", "Termina Field Cow Grotto Grass 54"),
                        M("Grass.png", 636, 438, 24, "MM Termina Field Cow Grotto Grass 55", "Termina Field Cow Grotto Grass 55"),
                        M("Grass.png", 642, 424, 24, "MM Termina Field Cow Grotto Grass 56", "Termina Field Cow Grotto Grass 56"),
                        M("Grass.png", 665, 416, 24, "MM Termina Field Cow Grotto Grass 57", "Termina Field Cow Grotto Grass 57"),
                        M("Grass.png", 688, 424, 24, "MM Termina Field Cow Grotto Grass 58", "Termina Field Cow Grotto Grass 58"),
                        M("Grass.png", 694, 438, 24, "MM Termina Field Cow Grotto Grass 59", "Termina Field Cow Grotto Grass 59"),
                        M("Grass.png", 688, 456, 24, "MM Termina Field Cow Grotto Grass 60", "Termina Field Cow Grotto Grass 60"),
                        M("Grass.png", 489, 489, 24, "MM Termina Field Cow Grotto Grass 61", "Termina Field Cow Grotto Grass 61"),
                        M("Grass.png", 489, 473, 24, "MM Termina Field Cow Grotto Grass 62", "Termina Field Cow Grotto Grass 62"),
                        M("Grass.png", 509, 473, 24, "MM Termina Field Cow Grotto Grass 63", "Termina Field Cow Grotto Grass 63"),
                        M("Grass.png", 509, 489, 24, "MM Termina Field Cow Grotto Grass 64", "Termina Field Cow Grotto Grass 64"),
                        M("Grass.png", 499, 505, 24, "MM Termina Field Cow Grotto Grass 65", "Termina Field Cow Grotto Grass 65"),
                        M("Grass.png", 476, 497, 24, "MM Termina Field Cow Grotto Grass 66", "Termina Field Cow Grotto Grass 66"),
                        M("Grass.png", 470, 479, 24, "MM Termina Field Cow Grotto Grass 67", "Termina Field Cow Grotto Grass 67"),
                        M("Grass.png", 476, 465, 24, "MM Termina Field Cow Grotto Grass 68", "Termina Field Cow Grotto Grass 68"),
                        M("Grass.png", 499, 457, 24, "MM Termina Field Cow Grotto Grass 69", "Termina Field Cow Grotto Grass 69"),
                        M("Grass.png", 522, 465, 24, "MM Termina Field Cow Grotto Grass 70", "Termina Field Cow Grotto Grass 70"),
                        M("Grass.png", 528, 479, 24, "MM Termina Field Cow Grotto Grass 71", "Termina Field Cow Grotto Grass 71"),
                        M("Grass.png", 522, 497, 24, "MM Termina Field Cow Grotto Grass 72", "Termina Field Cow Grotto Grass 72"),
						
						ME("Entrance.png", 220, 458, "Entrance shuffle (Termina Field)", "MM_GROTTO_EXIT_COW_FIELD")
                    }
                },
                new MapSubRegion
                {
                    Name = "Bio Baba Grotto",
                    BackgroundImage = MM("Termina", "Bio_Baba"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_BIO_BABA" },
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 214, 115, 40, "MM Termina Field Bio Baba Grotto", "Termina Field Bio Baba Grotto"),
                        M("Hive.png", 258, 120, 40, "MM Termina Field Bio Baba Grotto Hive Left", "Termina Field Bio Baba Grotto Hive Left"),
                        M("Hive.png", 148, 144, 40, "MM Termina Field Bio Baba Grotto Hive Middle", "Termina Field Bio Baba Grotto Hive Middle"),
                        M("Rock.png", 728, 367, 24, "MM Bio Baba Grotto Rock", "Bio Baba Grotto Rock"),
                        M("Grass.png", 672, 354, 24, "MM Bio Baba Grotto Grass 1", "Bio Baba Grotto Grass 1"),
                        M("Grass.png", 787, 354, 24, "MM Bio Baba Grotto Grass 2", "Bio Baba Grotto Grass 2"),
						
						ME("Entrance.png", 712, 313, "Entrance shuffle (Termina Field)", "MM_GROTTO_EXIT_BIO_BABA")
                    }
                },
                new MapSubRegion
                {
                    Name = "Peahat Grotto",
                    BackgroundImage = MM("Termina", "Peahat"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_PEAHAT" },
                    Marks = new List<MapMark>
                    {
                        M("Grass.png", 418, 356, 24, "MM Peahat Grotto Grass 01", "Peahat Grotto Grass 01"),
                        M("Grass.png", 418, 336, 24, "MM Peahat Grotto Grass 02", "Peahat Grotto Grass 02"),
                        M("Grass.png", 438, 336, 24, "MM Peahat Grotto Grass 03", "Peahat Grotto Grass 03"),
                        M("Grass.png", 438, 356, 24, "MM Peahat Grotto Grass 04", "Peahat Grotto Grass 04"),
                        M("Grass.png", 428, 376, 24, "MM Peahat Grotto Grass 05", "Peahat Grotto Grass 05"),
                        M("Grass.png", 408, 366, 24, "MM Peahat Grotto Grass 06", "Peahat Grotto Grass 06"),
                        M("Grass.png", 398, 346, 24, "MM Peahat Grotto Grass 07", "Peahat Grotto Grass 07"),
                        M("Grass.png", 408, 326, 24, "MM Peahat Grotto Grass 08", "Peahat Grotto Grass 08"),
                        M("Grass.png", 428, 316, 24, "MM Peahat Grotto Grass 09", "Peahat Grotto Grass 09"),
                        M("Grass.png", 448, 326, 24, "MM Peahat Grotto Grass 10", "Peahat Grotto Grass 10"),
                        M("Grass.png", 458, 346, 24, "MM Peahat Grotto Grass 11", "Peahat Grotto Grass 11"),
                        M("Grass.png", 448, 366, 24, "MM Peahat Grotto Grass 12", "Peahat Grotto Grass 12"),
                        M("Chest.png", 361, 375, 40, "MM Termina Field Peahat Grotto", "Termina Field Peahat Grotto"),
						
						ME("Entrance.png", 760, 468, "Entrance shuffle (Termina Field)", "MM_GROTTO_EXIT_PEAHAT")
                    }
                },
                new MapSubRegion
                {
                    Name = "Dodongo Grotto",
                    BackgroundImage = MM("Termina", "Dodongo"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_DODONGO" },
                    Marks = new List<MapMark>
					{
						M("Chest.png", 631, 381, 40, "MM Termina Field Dodongo Grotto", "Termina Field Dodongo Grotto"),
						ME("Entrance.png", 209, 469, "Entrance shuffle (Termina Field)", "MM_GROTTO_EXIT_DODONGO")
					}
                },
                new MapSubRegion
                {
                    Name = "Deku Scrub Grotto",
                    BackgroundImage = MM("Termina", "Scrub"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_SCRUB" },
                    Marks = new List<MapMark>
                    {
                        M("Scrub.png", 212, 406, 40, "MM Termina Field Scrub", "Termina Field Scrub"),
                        M("Crate.png", 260, 461, 24, "MM Termina Field Scrub Crate", "Termina Field Scrub Crate"),
                        M("Pot.png", 212, 379, 24, "MM Termina Field Scrub Pot", "Termina Field Scrub Pot"),
						
						ME("Entrance.png", 643, 401, "Entrance shuffle (Termina Field)", "MM_GROTTO_EXIT_SCRUB")
                    }
                },
                new MapSubRegion
                {
                    Name = "Tall Grass Grotto",
                    BackgroundImage = MM("Grottos", "Generic"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GENERIC_GRASS" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 208, 394, 40, "MM Termina Field Tall Grass Grotto", "Termina Field Tall Grass Grotto"),
                        M("Grass.png", 605, 531, 24, "MM Termina Field Tall Grass Grotto Grass 01", "Termina Field Tall Grass Grotto Grass 01"),
                        M("Grass.png", 363, 479, 24, "MM Termina Field Tall Grass Grotto Grass 02", "Termina Field Tall Grass Grotto Grass 02"),
                        M("Grass.png", 270, 451, 24, "MM Termina Field Tall Grass Grotto Grass 03", "Termina Field Tall Grass Grotto Grass 03"),
                        M("Grass.png", 330, 467, 24, "MM Termina Field Tall Grass Grotto Grass 04", "Termina Field Tall Grass Grotto Grass 04"),
                        M("Grass.png", 290, 433, 24, "MM Termina Field Tall Grass Grotto Grass 05", "Termina Field Tall Grass Grotto Grass 05"),
                        M("Grass.png", 255, 427, 24, "MM Termina Field Tall Grass Grotto Grass 06", "Termina Field Tall Grass Grotto Grass 06"),
                        M("Grass.png", 689, 480, 24, "MM Termina Field Tall Grass Grotto Grass 07", "Termina Field Tall Grass Grotto Grass 07"),
                        M("Grass.png", 383, 417, 24, "MM Termina Field Tall Grass Grotto Grass 08", "Termina Field Tall Grass Grotto Grass 08"),
                        M("Grass.png", 557, 439, 24, "MM Termina Field Tall Grass Grotto Grass 09", "Termina Field Tall Grass Grotto Grass 09"),
                        M("Grass.png", 642, 449, 24, "MM Termina Field Tall Grass Grotto Grass 10", "Termina Field Tall Grass Grotto Grass 10"),
                        M("Grass.png", 245, 375, 24, "MM Termina Field Tall Grass Grotto Grass 11", "Termina Field Tall Grass Grotto Grass 11"),
                        M("Grass.png", 375, 397, 24, "MM Termina Field Tall Grass Grotto Grass 12", "Termina Field Tall Grass Grotto Grass 12"),
                        M("Grass.png", 587, 432, 24, "MM Termina Field Tall Grass Grotto Grass 13", "Termina Field Tall Grass Grotto Grass 13"),
                        M("Grass.png", 408, 402, 24, "MM Termina Field Tall Grass Grotto Grass 14", "Termina Field Tall Grass Grotto Grass 14"),
						
						ME("Entrance.png", 621, 483, "Entrance shuffle (Termina Field)", "MM_GROTTO_EXIT_GENERIC_GRASS")
                    }
                },
                new MapSubRegion
                {
                    Name = "Pillar Grotto",
                    BackgroundImage = MM("Grottos", "Generic"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GENERIC_FIELD_PILLAR" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 208, 394, 40, "MM Termina Field Pillar Grotto", "Termina Field Pillar Grotto"),
                        M("Grass.png", 605, 531, 24, "MM Termina Field Pillar Grotto Grass 01", "Termina Field Pillar Grotto Grass 01"),
                        M("Grass.png", 363, 479, 24, "MM Termina Field Pillar Grotto Grass 02", "Termina Field Pillar Grotto Grass 02"),
                        M("Grass.png", 270, 451, 24, "MM Termina Field Pillar Grotto Grass 03", "Termina Field Pillar Grotto Grass 03"),
                        M("Grass.png", 330, 467, 24, "MM Termina Field Pillar Grotto Grass 04", "Termina Field Pillar Grotto Grass 04"),
                        M("Grass.png", 290, 433, 24, "MM Termina Field Pillar Grotto Grass 05", "Termina Field Pillar Grotto Grass 05"),
                        M("Grass.png", 255, 427, 24, "MM Termina Field Pillar Grotto Grass 06", "Termina Field Pillar Grotto Grass 06"),
                        M("Grass.png", 689, 480, 24, "MM Termina Field Pillar Grotto Grass 07", "Termina Field Pillar Grotto Grass 07"),
                        M("Grass.png", 383, 417, 24, "MM Termina Field Pillar Grotto Grass 08", "Termina Field Pillar Grotto Grass 08"),
                        M("Grass.png", 557, 439, 24, "MM Termina Field Pillar Grotto Grass 09", "Termina Field Pillar Grotto Grass 09"),
                        M("Grass.png", 642, 449, 24, "MM Termina Field Pillar Grotto Grass 10", "Termina Field Pillar Grotto Grass 10"),
                        M("Grass.png", 245, 375, 24, "MM Termina Field Pillar Grotto Grass 11", "Termina Field Pillar Grotto Grass 11"),
                        M("Grass.png", 375, 397, 24, "MM Termina Field Pillar Grotto Grass 12", "Termina Field Pillar Grotto Grass 12"),
                        M("Grass.png", 587, 432, 24, "MM Termina Field Pillar Grotto Grass 13", "Termina Field Pillar Grotto Grass 13"),
                        M("Grass.png", 408, 402, 24, "MM Termina Field Pillar Grotto Grass 14", "Termina Field Pillar Grotto Grass 14"),
						
						ME("Entrance.png", 621, 483, "Entrance shuffle (Termina Field)", "MM_GROTTO_EXIT_GENERIC_FIELD_PILLAR")
                    }
                },
                new MapSubRegion
                {
                    Name = "Swamp Gossip Stone Grotto",
                    BackgroundImage = MM("Termina", "Swamp_Gossip"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GOSSIPS_SWAMP" },
                    Marks = new List<MapMark>
					{
						M("NPC.png", 509, 539, 40, "MM Termina Field Gossip Stones HP", "Termina Field Gossip Stones HP"),
						ME("Entrance.png", 151, 287, "Entrance shuffle (Termina Field)", "MM_GROTTO_EXIT_GOSSIPS_SWAMP")
					}
                },
                new MapSubRegion
                {
                    Name = "Mountain Gossip Stone Grotto",
                    BackgroundImage = MM("Termina", "Mountain_Gossip"),
					DestinationEntranceIds = new List<string> { "MM_GROTTO_GOSSIPS_MOUNTAIN" },
                    Marks = new List<MapMark>
					{
						M("NPC.png", 464, 553, 40, "MM Termina Field Gossip Stones HP", "Termina Field Gossip Stones HP"),
						ME("Entrance.png", 636, 300, "Entrance shuffle (Termina Field)", "MM_GROTTO_EXIT_GOSSIPS_MOUNTAIN")
					}
                },
                new MapSubRegion
                {
                    Name = "Ocean Gossip Stone Grotto",
                    BackgroundImage = MM("Termina", "Ocean_Gossip"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GOSSIPS_OCEAN" },
                    Marks = new List<MapMark>
                    {
                        M("Grass.png", 388, 403, 24, "MM Ocean Gossip Grotto Grass 1", "Ocean Gossip Grotto Grass 1"),
                        M("Grass.png", 440, 472, 24, "MM Ocean Gossip Grotto Grass 2", "Ocean Gossip Grotto Grass 2"),
                        M("Grass.png", 523, 466, 24, "MM Ocean Gossip Grotto Grass 3", "Ocean Gossip Grotto Grass 3"),
                        M("Grass.png", 380, 488, 24, "MM Ocean Gossip Grotto Grass 4", "Ocean Gossip Grotto Grass 4"),
                        M("Grass.png", 317, 517, 24, "MM Ocean Gossip Grotto Grass 5", "Ocean Gossip Grotto Grass 5"),
                        M("Butterfly.png", 247, 412, 24, "MM Ocean Gossip Grotto Butterfly 1", "Ocean Gossip Grotto Butterfly 1"),
                        M("Butterfly.png", 287, 410, 24, "MM Ocean Gossip Grotto Butterfly 2", "Ocean Gossip Grotto Butterfly 2"),
                        M("Hive.png", 559, 201, 40, "MM Ocean Gossip Grotto Hive", "Ocean Gossip Grotto Hive"),
                        M("NPC.png", 746, 403, 40, "MM Termina Field Gossip Stones HP", "Termina Field Gossip Stones HP"),
						
						ME("Entrance.png", 237, 458, "Entrance shuffle (Termina Field)", "MM_GROTTO_EXIT_GOSSIPS_OCEAN")
                    }
                },
                new MapSubRegion
                {
                    Name = "Canyon Gossip Stone Grotto",
                    BackgroundImage = MM("Termina", "Canyon_Gossip"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GOSSIPS_CANYON" },
                    Marks = new List<MapMark>
                    {
                        M("Grass.png", 817, 530, 24, "MM Canyon Gossip Grotto Grass 1", "Canyon Gossip Grotto Grass 1"),
                        M("Grass.png", 240, 417, 24, "MM Canyon Gossip Grotto Grass 2", "Canyon Gossip Grotto Grass 2"),
                        M("Grass.png", 254, 378, 24, "MM Canyon Gossip Grotto Grass 3", "Canyon Gossip Grotto Grass 3"),
                        M("Grass.png", 608, 413, 24, "MM Canyon Gossip Grotto Grass 4", "Canyon Gossip Grotto Grass 4"),
                        M("Grass.png", 683, 407, 24, "MM Canyon Gossip Grotto Grass 5", "Canyon Gossip Grotto Grass 5"),
                        M("NPC.png", 202, 362, 40, "MM Termina Field Gossip Stones HP", "Termina Field Gossip Stones HP"),
						
						ME("Entrance.png", 723, 462, "Entrance shuffle (Termina Field)", "MM_GROTTO_EXIT_GOSSIPS_CANYON")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion TwinIslands()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Twin Islands";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Twin Islands",
                    BackgroundImage = MM("Twin_Islands", "Twins"),
                    DestinationEntranceIds = new List<string>
                    {
                        "MM_TWIN_ISLANDS_FROM_GORON_RACETRACK",
                        "MM_TWIN_ISLANDS_FROM_GORON_VILLAGE",
                        "MM_TWIN_ISLANDS_FROM_MOUNTAIN_VILLAGE",
						"MM_GROTTO_EXIT_GENERIC_TWIN_ISLANDS",
						"MM_GROTTO_EXIT_HOT_WATER"
                    },
                    Marks = new List<MapMark>
                    {
                        MA("NPC.png", 713, 234, 40, "MM Goron Elder", "cursed", "Goron Elder"),
						M("NPC.png", 514, 207, 40, "MM Tingle Map Snowhead", "Tingle Map Snowhead"),
						M("NPC.png", 554, 207, 40, "MM Tingle Map Ranch", "Tingle Map Ranch"),
                        MA("Snowball.png", 400, 428, 24, "MM Twin Islands Big Snowball 01", "cursed", "Twin Islands Big Snowball 01"),
                        MA("Snowball.png", 309, 358, 24, "MM Twin Islands Big Snowball 02", "cursed", "Twin Islands Big Snowball 02"),
                        MA("Snowball.png", 302, 257, 24, "MM Twin Islands Big Snowball 03", "cursed", "Twin Islands Big Snowball 03"),
                        MA("Snowball.png", 426, 177, 24, "MM Twin Islands Big Snowball 04", "cursed", "Twin Islands Big Snowball 04"),
                        MA("Snowball.png", 519, 121, 24, "MM Twin Islands Big Snowball 05", "cursed", "Twin Islands Big Snowball 05"),
                        MA("Snowball.png", 544, 195, 24, "MM Twin Islands Big Snowball 06", "cursed", "Twin Islands Big Snowball 06"),
                        MA("Snowball.png", 507, 383, 24, "MM Twin Islands Big Snowball 07", "cursed", "Twin Islands Big Snowball 07"),
                        MA("Snowball.png", 739, 275, 24, "MM Twin Islands Big Snowball 08", "cursed", "Twin Islands Big Snowball 08"),
                        MA("Snowball.png", 759, 377, 24, "MM Twin Islands Big Snowball 09", "cursed", "Twin Islands Big Snowball 09"),
                        MA("Snowball.png", 662, 485, 24, "MM Twin Islands Big Snowball 10", "cursed", "Twin Islands Big Snowball 10"),
                        MA("Snowball.png", 547, 485, 24, "MM Twin Islands Big Snowball 11", "cursed", "Twin Islands Big Snowball 11"),
                        MA("Snowball.png", 636, 199, 24, "MM Twin Islands Big Snowball 12", "cursed", "Twin Islands Big Snowball 12"),
                        MA("Snowball.png", 529, 296, 24, "MM Twin Islands Big Snowball 13", "cursed", "Twin Islands Big Snowball 13"),
                        MA("Snowball.png", 110, 498, 24, "MM Twin Islands Small Snowball 1", "cursed", "Twin Islands Small Snowball 1"),
                        MA("Snowball.png", 189, 389, 24, "MM Twin Islands Small Snowball 2", "cursed", "Twin Islands Small Snowball 2"),
                        MA("Snowball.png", 546, 244, 24, "MM Twin Islands Small Snowball 3", "cursed", "Twin Islands Small Snowball 3"),
                        MA("Snowball.png", 206, 251, 24, "MM Twin Islands Small Snowball Ramp 1", "cursed", "Twin Islands Small Snowball Ramp 1"),
                        MA("Snowball.png", 234, 284, 24, "MM Twin Islands Small Snowball Ramp 2", "cursed", "Twin Islands Small Snowball Ramp 2"),
                        MA("Snowball.png", 208, 278, 24, "MM Twin Islands Small Snowball Ramp 3", "cursed", "Twin Islands Small Snowball Ramp 3"),
                        MA("Tree.png", 414, 291, 24, "MM Twin Islands Snow Tree Island 1", "cursed", "Twin Islands Snow Tree Island 1"),
                        MA("Tree.png", 663, 324, 24, "MM Twin Islands Snow Tree Island 2", "cursed", "Twin Islands Snow Tree Island 2"),
                        MA("Tree.png", 228, 256, 24, "MM Twin Islands Snow Tree Ramp", "cursed", "Twin Islands Snow Tree Ramp"),
                        MA("Tree.png", 414, 291, 24, "MM Twin Islands Tree Island 1", "cleared", "Twin Islands Tree Island 1"),
                        MA("Tree.png", 663, 324, 24, "MM Twin Islands Tree Island 2", "cleared", "Twin Islands Tree Island 2"),
                        MA("Tree.png", 228, 256, 24, "MM Twin Islands Tree Ramp", "cleared", "Twin Islands Tree Ramp"),
                        MA("Grass.png", 150, 448, 24, "MM Twin Islands Grass 01", "cleared", "Twin Islands Grass 01"),
                        MA("Grass.png", 150, 428, 24, "MM Twin Islands Grass 02", "cleared", "Twin Islands Grass 02"),
                        MA("Grass.png", 170, 428, 24, "MM Twin Islands Grass 03", "cleared", "Twin Islands Grass 03"),
                        MA("Grass.png", 170, 448, 24, "MM Twin Islands Grass 04", "cleared", "Twin Islands Grass 04"),
                        MA("Grass.png", 160, 468, 24, "MM Twin Islands Grass 05", "cleared", "Twin Islands Grass 05"),
                        MA("Grass.png", 140, 458, 24, "MM Twin Islands Grass 06", "cleared", "Twin Islands Grass 06"),
                        MA("Grass.png", 130, 438, 24, "MM Twin Islands Grass 07", "cleared", "Twin Islands Grass 07"),
                        MA("Grass.png", 140, 418, 24, "MM Twin Islands Grass 08", "cleared", "Twin Islands Grass 08"),
                        MA("Grass.png", 160, 408, 24, "MM Twin Islands Grass 09", "cleared", "Twin Islands Grass 09"),
                        MA("Grass.png", 180, 418, 24, "MM Twin Islands Grass 10", "cleared", "Twin Islands Grass 10"),
                        MA("Grass.png", 190, 438, 24, "MM Twin Islands Grass 11", "cleared", "Twin Islands Grass 11"),
                        MA("Grass.png", 180, 458, 24, "MM Twin Islands Grass 12", "cleared", "Twin Islands Grass 12"),
                        MA("Rock.png", 208, 278, 24, "MM Twin Islands Rock 1", "cleared", "Twin Islands Rock 1"),
                        MA("Rock.png", 206, 251, 24, "MM Twin Islands Rock 2", "cleared", "Twin Islands Rock 2"),
                        MA("Rock.png", 234, 284, 24, "MM Twin Islands Rock 3", "cleared", "Twin Islands Rock 3"),
                        MA("Rupee.png", 670, 199, 24, "MM Twin Islands Rupee 1", "cleared", "Twin Islands Rupee 1"),
                        MA("Rupee.png", 487, 171, 24, "MM Twin Islands Rupee 2", "cleared", "Twin Islands Rupee 2"),
                        MA("Rupee.png", 590, 496, 24, "MM Twin Islands Rupee 3", "cleared", "Twin Islands Rupee 3"),
                        MA("Rupee.png", 768, 329, 24, "MM Twin Islands Rupee 4", "cleared", "Twin Islands Rupee 4"),
                        MA("Chest.png", 388, 438, 40, "MM Twin Islands Underwater Chest 1", "cleared", "Twin Islands Underwater Chest 1"),
                        MA("Chest.png", 608, 56, 40, "MM Twin Islands Underwater Chest 2", "cleared", "Twin Islands Underwater Chest 2"),
						
						ME("Entrance.png", 388, 25, "Entrance shuffle (Goron Racetrack)", "MM_GORON_RACETRACK"),
						ME("Entrance.png", 636, 337, "Entrance shuffle (Frozen Grotto)", "MM_GROTTO_HOT_WATER"),
						ME("Entrance.png", 250, 228, "Entrance shuffle (Ramp Grotto)", "MM_GROTTO_GENERIC_TWIN_ISLANDS"),
						ME("Entrance.png", 23, 469, "Entrance shuffle (Mountain Village)", "MM_MOUNTAIN_VILLAGE_FROM_TWIN_ISLANDS"),
						ME("Entrance.png", 823, 436, "Entrance shuffle (Goron Village)", "MM_GORON_VILLAGE_FROM_TWIN_ISLANDS")
                    }
                },
                new MapSubRegion
                {
                    Name = "Goron Racetrack",
                    BackgroundImage = MM("Twin_Islands", "Goron_Racetrack"),
                    DestinationEntranceIds = new List<string> { "MM_GORON_RACETRACK" },
                    Marks = new List<MapMark>
                    {
                        MA("NPC.png", 574, 471, 40, "MM Goron Race Reward", "cleared", "Goron Race Reward"),
                        M("Tree.png", 126, 322, 24, "MM Goron Race Forked Tree 1", "Goron Race Forked Tree 1"),
                        M("Tree.png", 97, 341, 24, "MM Goron Race Forked Tree 2", "Goron Race Forked Tree 2"),
                        M("Tree.png", 160, 308, 24, "MM Goron Race Forked Tree 3", "Goron Race Forked Tree 3"),
                        M("Tree.png", 146, 313, 24, "MM Goron Race Forked Tree 4", "Goron Race Forked Tree 4"),
                        M("Tree.png", 126, 336, 24, "MM Goron Race Forked Tree 5", "Goron Race Forked Tree 5"),
                        M("Tree.png", 143, 326, 24, "MM Goron Race Forked Tree 6", "Goron Race Forked Tree 6"),
                        M("Tree.png", 181, 300, 24, "MM Goron Race Forked Tree 7", "Goron Race Forked Tree 7"),
                        M("Tree.png", 173, 323, 24, "MM Goron Race Forked Tree 8", "Goron Race Forked Tree 8"),
                        M("Tree.png", 157, 328, 24, "MM Goron Race Forked Tree 9", "Goron Race Forked Tree 9"),
                        M("Tree.png", 139, 339, 24, "MM Goron Race Forked Tree 10", "Goron Race Forked Tree 10"),
                        M("Pot.png", 319, 228, 24, "MM Goron Race Pot 01", "Goron Race Pot 01"),
                        M("Pot.png", 273, 262, 24, "MM Goron Race Pot 02", "Goron Race Pot 02"),
                        M("Pot.png", 334, 200, 24, "MM Goron Race Pot 03", "Goron Race Pot 03"),
                        M("Pot.png", 81, 308, 24, "MM Goron Race Pot 04", "Goron Race Pot 04"),
                        M("Pot.png", 231, 193, 24, "MM Goron Race Pot 05", "Goron Race Pot 05"),
                        M("Pot.png", 239, 224, 24, "MM Goron Race Pot 06", "Goron Race Pot 06"),
                        M("Pot.png", 86, 291, 24, "MM Goron Race Pot 07", "Goron Race Pot 07"),
                        M("Pot.png", 64, 315, 24, "MM Goron Race Pot 08", "Goron Race Pot 08"),
                        M("Pot.png", 70, 280, 24, "MM Goron Race Pot 09", "Goron Race Pot 09"),
                        M("Pot.png", 48, 304, 24, "MM Goron Race Pot 10", "Goron Race Pot 10"),
                        M("Pot.png", 53, 287, 24, "MM Goron Race Pot 11", "Goron Race Pot 11"),
                        M("Pot.png", 737, 416, 24, "MM Goron Race Pot 12", "Goron Race Pot 12"),
                        M("Pot.png", 749, 431, 24, "MM Goron Race Pot 13", "Goron Race Pot 13"),
                        M("Pot.png", 757, 415, 24, "MM Goron Race Pot 14", "Goron Race Pot 14"),
                        M("Pot.png", 305, 275, 24, "MM Goron Race Pot 15", "Goron Race Pot 15"),
                        M("Pot.png", 433, 330, 24, "MM Goron Race Pot 16", "Goron Race Pot 16"),
                        M("Pot.png", 439, 316, 24, "MM Goron Race Pot 17", "Goron Race Pot 17"),
                        M("Pot.png", 457, 315, 24, "MM Goron Race Pot 18", "Goron Race Pot 18"),
                        M("Pot.png", 464, 327, 24, "MM Goron Race Pot 19", "Goron Race Pot 19"),
                        M("Pot.png", 449, 340, 24, "MM Goron Race Pot 20", "Goron Race Pot 20"),
                        M("Pot.png", 540, 180, 24, "MM Goron Race Pot 21", "Goron Race Pot 21"),
                        M("Pot.png", 524, 170, 24, "MM Goron Race Pot 22", "Goron Race Pot 22"),
                        M("Pot.png", 547, 165, 24, "MM Goron Race Pot 23", "Goron Race Pot 23"),
                        M("Pot.png", 531, 155, 24, "MM Goron Race Pot 24", "Goron Race Pot 24"),
                        M("Pot.png", 349, 264, 24, "MM Goron Race Pot 25", "Goron Race Pot 25"),
                        M("Pot.png", 535, 254, 24, "MM Goron Race Pot 26", "Goron Race Pot 26"),
                        M("Pot.png", 538, 273, 24, "MM Goron Race Pot 27", "Goron Race Pot 27"),
                        M("Pot.png", 673, 233, 24, "MM Goron Race Pot 28", "Goron Race Pot 28"),
                        M("Pot.png", 677, 212, 24, "MM Goron Race Pot 29", "Goron Race Pot 29"),
                        M("Pot.png", 657, 220, 24, "MM Goron Race Pot 30", "Goron Race Pot 30"),
						
						ME("Entrance.png", 574, 552, "Entrance shuffle (Twin Islands)", "MM_TWIN_ISLANDS_FROM_GORON_RACETRACK")
                    }
                },
                new MapSubRegion
                {
                    Name = "Frozen Grotto",
                    BackgroundImage = MM("Twin_Islands", "Frozen"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_HOT_WATER" },
                    Marks = new List<MapMark>
					{
						M("Chest.png", 140, 416, 40, "MM Twin Islands Frozen Grotto Chest", "Twin Islands Frozen Grotto Chest"),
						
						ME("Entrance.png", 690, 484, "Entrance shuffle (Twin Islands)", "MM_GROTTO_EXIT_HOT_WATER")
					}
                },
                new MapSubRegion
                {
                    Name = "Ramp Grotto",
                    BackgroundImage = MM("Grottos", "Generic"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GENERIC_TWIN_ISLANDS" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 208, 394, 40, "MM Twin Islands Ramp Grotto Chest", "Twin Islands Ramp Grotto Chest"),
                        M("Grass.png", 605, 531, 24, "MM Twin Islands Ramp Grotto Grass 01", "Twin Islands Ramp Grotto Grass 01"),
                        M("Grass.png", 363, 479, 24, "MM Twin Islands Ramp Grotto Grass 02", "Twin Islands Ramp Grotto Grass 02"),
                        M("Grass.png", 270, 451, 24, "MM Twin Islands Ramp Grotto Grass 03", "Twin Islands Ramp Grotto Grass 03"),
                        M("Grass.png", 330, 467, 24, "MM Twin Islands Ramp Grotto Grass 04", "Twin Islands Ramp Grotto Grass 04"),
                        M("Grass.png", 290, 433, 24, "MM Twin Islands Ramp Grotto Grass 05", "Twin Islands Ramp Grotto Grass 05"),
                        M("Grass.png", 255, 427, 24, "MM Twin Islands Ramp Grotto Grass 06", "Twin Islands Ramp Grotto Grass 06"),
                        M("Grass.png", 689, 480, 24, "MM Twin Islands Ramp Grotto Grass 07", "Twin Islands Ramp Grotto Grass 07"),
                        M("Grass.png", 383, 417, 24, "MM Twin Islands Ramp Grotto Grass 08", "Twin Islands Ramp Grotto Grass 08"),
                        M("Grass.png", 557, 439, 24, "MM Twin Islands Ramp Grotto Grass 09", "Twin Islands Ramp Grotto Grass 09"),
                        M("Grass.png", 642, 449, 24, "MM Twin Islands Ramp Grotto Grass 10", "Twin Islands Ramp Grotto Grass 10"),
                        M("Grass.png", 245, 375, 24, "MM Twin Islands Ramp Grotto Grass 11", "Twin Islands Ramp Grotto Grass 11"),
                        M("Grass.png", 375, 397, 24, "MM Twin Islands Ramp Grotto Grass 12", "Twin Islands Ramp Grotto Grass 12"),
                        M("Grass.png", 587, 432, 24, "MM Twin Islands Ramp Grotto Grass 13", "Twin Islands Ramp Grotto Grass 13"),
                        M("Grass.png", 408, 402, 24, "MM Twin Islands Ramp Grotto Grass 14", "Twin Islands Ramp Grotto Grass 14"),
						
						ME("Entrance.png", 622, 483, "Entrance shuffle (Twin Islands)", "MM_GROTTO_EXIT_GENERIC_TWIN_ISLANDS")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion Woodfall()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Woodfall";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Woodfall",
                    BackgroundImage = MM("Woodfall", "Woodfall"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_WOODFALL_FROM_TEMPLE",
						"MM_WOODFALL_FROM_FAIRY_FOUNTAIN",
						"MM_WARP_OWL_WOODFALL",
						"MM_WOODFALL"
					},
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 361, 465, 40, "MM Woodfall Entrance Chest", "Woodfall Entrance Chest"),
                        M("Chest.png", 259, 410, 40, "MM Woodfall HP Chest", "Woodfall HP Chest"),
                        M("Chest.png", 445, 36, 40, "MM Woodfall Near Owl Chest", "Woodfall Near Owl Chest"),
                        M("Grass.png", 495, 497, 24, "MM Woodfall Grass 1", "Woodfall Grass 1"),
                        M("Grass.png", 480, 485, 24, "MM Woodfall Grass 2", "Woodfall Grass 2"),
                        M("Grass.png", 492, 473, 24, "MM Woodfall Grass 3", "Woodfall Grass 3"),
                        M("Grass.png", 440, 473, 24, "MM Woodfall Grass 4", "Woodfall Grass 4"),
                        M("Grass.png", 437, 497, 24, "MM Woodfall Grass 5", "Woodfall Grass 5"),
                        M("Grass.png", 452, 485, 24, "MM Woodfall Grass 6", "Woodfall Grass 6"),
                        M("NPC.png", 453, 88, 40, "MM Woodfall Owl Statue", "Woodfall Owl Statue"),
                        M("Pot.png", 435, 115, 24, "MM Woodfall Pot 1", "Woodfall Pot 1"),
                        M("Pot.png", 435, 91, 24, "MM Woodfall Pot 2", "Woodfall Pot 2"),
                        M("Pot.png", 435, 139, 24, "MM Woodfall Pot 3", "Woodfall Pot 3"),
                        M("Rupee.png", 585, 311, 24, "MM Woodfall Rupee", "Woodfall Rupee"),
						
						ME("Entrance.png", 311, 57, "Entrance shuffle (Fairy Fountain)", "MM_FAIRY_FOUNTAIN_WOODFALL"),
						ME("Entrance.png", 451, 219, "Entrance shuffle (Woodfall Temple)", "MM_TEMPLE_WOODFALL"),
						ME("Entrance.png", 457, 524, "Entrance shuffle (Southern Swamp)", "MM_SWAMP_FROM_WOODFALL")
                    }
                },
                new MapSubRegion
                {
                    Name = "Deku Princess Prison",
                    BackgroundImage = MM("Woodfall", "Prison"),
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>()
                },
                new MapSubRegion
                {
                    Name = "Fairy Fountain",
                    BackgroundImage = MM("Woodfall", "Fairy"),
                    DestinationEntranceIds = new List<string> { "MM_FAIRY_FOUNTAIN_WOODFALL" },
                    Marks = new List<MapMark>
					{
						M("NPC.png", 457, 136, 40, "MM Woodfall Great Fairy", "Woodfall Great Fairy"),
						
						ME("Entrance.png", 447, 541, "Entrance shuffle (Woodfall)", "MM_WOODFALL_FROM_FAIRY_FOUNTAIN")
					}
                }
            };
            return mapRegion;
        }

        public static MapRegion WoodsofMystery()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Woods of Mystery";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Woods of Mystery",
                    BackgroundImage = MM("Woods_of_Mystery", "Woods_Mystery"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_GROTTO_EXIT_GENERIC_WOODS",
						"MM_MYSTERY_WOODS"
					},
                    Marks = new List<MapMark>
                    {
                        M("Grass.png", 444, 335, 24, "MM Woods of Mystery Grass Center 1", "Woods of Mystery Grass Center 1"),
                        M("Grass.png", 467, 317, 24, "MM Woods of Mystery Grass Center 2", "Woods of Mystery Grass Center 2"),
                        M("Grass.png", 452, 290, 24, "MM Woods of Mystery Grass Center 3", "Woods of Mystery Grass Center 3"),
                        M("Grass.png", 483, 302, 24, "MM Woods of Mystery Grass Center 4", "Woods of Mystery Grass Center 4"),
                        M("Grass.png", 613, 296, 24, "MM Woods of Mystery Grass E 1", "Woods of Mystery Grass E 1"),
                        M("Grass.png", 673, 309, 24, "MM Woods of Mystery Grass E 2", "Woods of Mystery Grass E 2"),
                        M("Grass.png", 458, 110, 24, "MM Woods of Mystery Grass N 1", "Woods of Mystery Grass N 1"),
                        M("Grass.png", 470, 154, 24, "MM Woods of Mystery Grass N 2", "Woods of Mystery Grass N 2"),
                        M("Grass.png", 652, 125, 24, "MM Woods of Mystery Grass NE 1", "Woods of Mystery Grass NE 1"),
                        M("Grass.png", 632, 125, 24, "MM Woods of Mystery Grass NE 2", "Woods of Mystery Grass NE 2"),
                        M("Grass.png", 647, 177, 24, "MM Woods of Mystery Grass NE 3", "Woods of Mystery Grass NE 3"),
                        M("Grass.png", 674, 186, 24, "MM Woods of Mystery Grass NE 4", "Woods of Mystery Grass NE 4"),
                        M("Grass.png", 617, 182, 24, "MM Woods of Mystery Grass NE 5", "Woods of Mystery Grass NE 5"),
                        M("Grass.png", 280, 105, 24, "MM Woods of Mystery Grass NW 1", "Woods of Mystery Grass NW 1"),
                        M("Grass.png", 329, 168, 24, "MM Woods of Mystery Grass NW 2", "Woods of Mystery Grass NW 2"),
                        M("Grass.png", 448, 513, 24, "MM Woods of Mystery Grass S 1", "Woods of Mystery Grass S 1"),
                        M("Grass.png", 435, 503, 24, "MM Woods of Mystery Grass S 2", "Woods of Mystery Grass S 2"),
                        M("Grass.png", 422, 513, 24, "MM Woods of Mystery Grass S 3", "Woods of Mystery Grass S 3"),
                        M("Grass.png", 670, 531, 24, "MM Woods of Mystery Grass SE", "Woods of Mystery Grass SE"),
                        M("Grass.png", 302, 472, 24, "MM Woods of Mystery Grass SW 1", "Woods of Mystery Grass SW 1"),
                        M("Grass.png", 250, 484, 24, "MM Woods of Mystery Grass SW 2", "Woods of Mystery Grass SW 2"),
                        M("Grass.png", 312, 340, 24, "MM Woods of Mystery Grass W 1", "Woods of Mystery Grass W 1"),
                        M("Grass.png", 263, 275, 24, "MM Woods of Mystery Grass W 2", "Woods of Mystery Grass W 2"),
						
						ME("Entrance.png", 633, 508, "Entrance shuffle (Open Grotto)", "MM_GROTTO_GENERIC_WOODS"),
						ME("Entrance.png", 441, 577, "Entrance shuffle (Southern Swamp)", "MM_SWAMP_FROM_MYSTERY_WOODS")
                    }
                },
                new MapSubRegion
                {
                    Name = "Open Grotto",
                    BackgroundImage = MM("Grottos", "Generic"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GENERIC_WOODS" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 208, 394, 40, "MM Woods of Mystery Grotto", "Woods of Mystery Grotto"),
                        M("Grass.png", 605, 531, 24, "MM Woods of Mystery Grotto Grass 01", "Woods of Mystery Grotto Grass 01"),
                        M("Grass.png", 363, 479, 24, "MM Woods of Mystery Grotto Grass 02", "Woods of Mystery Grotto Grass 02"),
                        M("Grass.png", 270, 451, 24, "MM Woods of Mystery Grotto Grass 03", "Woods of Mystery Grotto Grass 03"),
                        M("Grass.png", 330, 467, 24, "MM Woods of Mystery Grotto Grass 04", "Woods of Mystery Grotto Grass 04"),
                        M("Grass.png", 290, 433, 24, "MM Woods of Mystery Grotto Grass 05", "Woods of Mystery Grotto Grass 05"),
                        M("Grass.png", 255, 427, 24, "MM Woods of Mystery Grotto Grass 06", "Woods of Mystery Grotto Grass 06"),
                        M("Grass.png", 689, 480, 24, "MM Woods of Mystery Grotto Grass 07", "Woods of Mystery Grotto Grass 07"),
                        M("Grass.png", 383, 417, 24, "MM Woods of Mystery Grotto Grass 08", "Woods of Mystery Grotto Grass 08"),
                        M("Grass.png", 557, 439, 24, "MM Woods of Mystery Grotto Grass 09", "Woods of Mystery Grotto Grass 09"),
                        M("Grass.png", 642, 449, 24, "MM Woods of Mystery Grotto Grass 10", "Woods of Mystery Grotto Grass 10"),
                        M("Grass.png", 245, 375, 24, "MM Woods of Mystery Grotto Grass 11", "Woods of Mystery Grotto Grass 11"),
                        M("Grass.png", 375, 397, 24, "MM Woods of Mystery Grotto Grass 12", "Woods of Mystery Grotto Grass 12"),
                        M("Grass.png", 587, 432, 24, "MM Woods of Mystery Grotto Grass 13", "Woods of Mystery Grotto Grass 13"),
                        M("Grass.png", 408, 402, 24, "MM Woods of Mystery Grotto Grass 14", "Woods of Mystery Grotto Grass 14"),
						
						ME("Entrance.png", 622, 483, "Entrance shuffle (Woods of Mystery)", "MM_GROTTO_EXIT_GENERIC_WOODS")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion ZoraCape()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Zora Cape";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Zora Cape",
                    BackgroundImage = MM("Zora_Cape", "Cape"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_ZORA_CAPE_FROM_BEAVERS",
						"MM_GREAT_BAY_FROM_TEMPLE",
						"MM_WARP_OWL_ZORA_CAPE",
						"MM_GREAT_BAY_FROM_FAIRY_FOUNTAIN",
						"MM_GROTTO_EXIT_GENERIC_ZORA_CAPE",
						"MM_VOID_ZORA_CAPE",
						"MM_ZORA_CAPE_PENINSULA",
						"MM_ZORA_CAPE_FROM_HALL_WATER",
						"MM_ZORA_CAPE_FROM_GREAT_BAY_COAST"
					},
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 769, 225, 40, "MM Zora Cape Ledge Chest 1", "Zora Cape Ledge Chest 1"),
                        M("Chest.png", 731, 346, 40, "MM Zora Cape Ledge Chest 2", "Zora Cape Ledge Chest 2"),
                        M("Chest.png", 335, 459, 40, "MM Zora Cape Underwater Chest", "Zora Cape Underwater Chest"),
                        M("Collectible.png", 792, 383, 40, "MM Zora Cape Waterfall HP", "Zora Cape Waterfall HP"),
                        M("NPC.png", 58, 310, 40, "MM Zora Cape Owl Statue", "Zora Cape Owl Statue"),
                        M("Tree.png", 511, 159, 24, "MM Zora Cape Palm Tree Beach 1", "Zora Cape Palm Tree Beach 1"),
                        M("Tree.png", 564, 187, 24, "MM Zora Cape Palm Tree Beach 2", "Zora Cape Palm Tree Beach 2"),
                        M("Tree.png", 286, 392, 24, "MM Zora Cape Palm Tree Island Center", "Zora Cape Palm Tree Island Center"),
                        M("Tree.png", 339, 356, 24, "MM Zora Cape Palm Tree Island Near Beach", "Zora Cape Palm Tree Island Near Beach"),
                        M("Tree.png", 320, 431, 24, "MM Zora Cape Palm Tree Island Near Fountain", "Zora Cape Palm Tree Island Near Fountain"),
                        M("Pot.png", 544, 149, 24, "MM Zora Cape Pot Game 1", "Zora Cape Pot Game 1"),
                        M("Pot.png", 529, 159, 24, "MM Zora Cape Pot Game 2", "Zora Cape Pot Game 2"),
                        M("Pot.png", 564, 163, 24, "MM Zora Cape Pot Game 3", "Zora Cape Pot Game 3"),
                        M("Pot.png", 554, 182, 24, "MM Zora Cape Pot Game 4", "Zora Cape Pot Game 4"),
                        M("Pot.png", 546, 167, 24, "MM Zora Cape Pot Game 5", "Zora Cape Pot Game 5"),
                        M("Pot.png", 767, 354, 24, "MM Zora Cape Pot Near Beavers 1", "Zora Cape Pot Near Beavers 1"),
                        M("Pot.png", 787, 347, 24, "MM Zora Cape Pot Near Beavers 2", "Zora Cape Pot Near Beavers 2"),
                        M("Pot.png", 78, 346, 24, "MM Zora Cape Pot Near Owl Statue 1", "Zora Cape Pot Near Owl Statue 1"),
                        M("Pot.png", 87, 328, 24, "MM Zora Cape Pot Near Owl Statue 2", "Zora Cape Pot Near Owl Statue 2"),
                        M("Pot.png", 100, 346, 24, "MM Zora Cape Pot Near Owl Statue 3", "Zora Cape Pot Near Owl Statue 3"),
                        M("Pot.png", 109, 328, 24, "MM Zora Cape Pot Near Owl Statue 4", "Zora Cape Pot Near Owl Statue 4"),
                        M("Rock.png", 336, 413, 24, "MM Zora Cape Rock Island 1", "Zora Cape Rock Island 1"),
                        M("Rock.png", 339, 438, 24, "MM Zora Cape Rock Island 2", "Zora Cape Rock Island 2"),
                        M("Rock.png", 308, 411, 24, "MM Zora Cape Rock Island 3", "Zora Cape Rock Island 3"),
                        M("Rock.png", 317, 456, 24, "MM Zora Cape Rock Island 4", "Zora Cape Rock Island 4"),
                        M("Rock.png", 299, 436, 24, "MM Zora Cape Rock Island 5", "Zora Cape Rock Island 5"),
                        M("Rock.png", 722, 290, 24, "MM Zora Cape Rock Water 1", "Zora Cape Rock Water 1"),
                        M("Rock.png", 725, 273, 24, "MM Zora Cape Rock Water 2", "Zora Cape Rock Water 2"),
                        M("Rock.png", 740, 289, 24, "MM Zora Cape Rock Water 3", "Zora Cape Rock Water 3"),
                        M("Rock.png", 733, 256, 24, "MM Zora Cape Rock Water 4", "Zora Cape Rock Water 4"),
                        M("Rock.png", 758, 285, 24, "MM Zora Cape Rock Water 5", "Zora Cape Rock Water 5"),
                        MA("Rock.png", 490, 159, 24, "MM Zora Cape Rock Sand 1", "cursed", "Zora Cape Rock Sand 1"),
                        MA("Rock.png", 450, 171, 24, "MM Zora Cape Rock Sand 2", "cursed", "Zora Cape Rock Sand 2"),
                        MA("Rock.png", 582, 189, 24, "MM Zora Cape Rock Sand 3", "cursed", "Zora Cape Rock Sand 3"),
                        MA("Rock.png", 617, 207, 24, "MM Zora Cape Rock Sand 4", "cursed", "Zora Cape Rock Sand 4"),
                        MA("Rock.png", 741, 234, 24, "MM Zora Cape Rock Sand 5", "cursed", "Zora Cape Rock Sand 5"),
						
						ME("Entrance.png", 418, 337, "Entrance shuffle (Generic Grotto)", "MM_GROTTO_GENERIC_ZORA_CAPE"),
						ME("Entrance.png", 370, 478, "Entrance shuffle (Fairy Fountain)", "MM_FAIRY_FOUNTAIN_GREAT_BAY"),
						ME("Entrance.png", 841, 445, "Entrance shuffle (Waterfall Rapids)", "MM_BEAVERS"),
						ME("Entrance.png", 4, 325, "Entrance shuffle (Great Bay Temple)", "MM_TEMPLE_GREAT_BAY"),
						ME("Entrance.png", 438, 116, "Entrance shuffle (Great Bay Coast)", "MM_GREAT_BAY_COAST_FROM_ZORA_CAPE"),
						ME("Entrance.png", 166, 268, "Entrance shuffle (Zora Hall Main)", "MM_ZORA_HALL_UNDERWATER"),
						ME("Entrance.png", 84, 282, "Entrance shuffle (Zora Hall Ledge)", "MM_ZORA_HALL_LEDGE"),
						ME("Entrance.png", 110, 138, "Entrance shuffle (Zora Cape)", "MM_VOID_ZORA_CAPE")
                    }
                },
                new MapSubRegion
                {
                    Name = "Waterfall Rapids",
                    BackgroundImage = MM("Zora_Cape", "Waterfall_Rapids"),
                    DestinationEntranceIds = new List<string> { "MM_BEAVERS" },
                    Marks = new List<MapMark>
                    {
                        M("NPC.png", 623, 326, 40, "MM Waterfall Rapids Beaver Race 1", "Waterfall Rapids Beaver Race 1"),
                        M("NPC.png", 675, 326, 40, "MM Waterfall Rapids Beaver Race 2", "Waterfall Rapids Beaver Race 2"),
						
						ME("Entrance.png", 238, 481, "Entrance shuffle (Zora Cape)", "MM_ZORA_CAPE_FROM_BEAVERS")
                    }
                },
                new MapSubRegion
                {
                    Name = "Generic Grotto",
                    BackgroundImage = MM("Grottos", "Generic"),
                    DestinationEntranceIds = new List<string> { "MM_GROTTO_GENERIC_ZORA_CAPE" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 208, 394, 40, "MM Zora Cape Grotto", "Zora Cape Grotto"),
                        M("Grass.png", 605, 531, 24, "MM Zora Cape Grotto Grass 01", "Zora Cape Grotto Grass 01"),
                        M("Grass.png", 363, 479, 24, "MM Zora Cape Grotto Grass 02", "Zora Cape Grotto Grass 02"),
                        M("Grass.png", 270, 451, 24, "MM Zora Cape Grotto Grass 03", "Zora Cape Grotto Grass 03"),
                        M("Grass.png", 330, 467, 24, "MM Zora Cape Grotto Grass 04", "Zora Cape Grotto Grass 04"),
                        M("Grass.png", 290, 433, 24, "MM Zora Cape Grotto Grass 05", "Zora Cape Grotto Grass 05"),
                        M("Grass.png", 255, 427, 24, "MM Zora Cape Grotto Grass 06", "Zora Cape Grotto Grass 06"),
                        M("Grass.png", 689, 480, 24, "MM Zora Cape Grotto Grass 07", "Zora Cape Grotto Grass 07"),
                        M("Grass.png", 383, 417, 24, "MM Zora Cape Grotto Grass 08", "Zora Cape Grotto Grass 08"),
                        M("Grass.png", 557, 439, 24, "MM Zora Cape Grotto Grass 09", "Zora Cape Grotto Grass 09"),
                        M("Grass.png", 642, 449, 24, "MM Zora Cape Grotto Grass 10", "Zora Cape Grotto Grass 10"),
                        M("Grass.png", 245, 375, 24, "MM Zora Cape Grotto Grass 11", "Zora Cape Grotto Grass 11"),
                        M("Grass.png", 375, 397, 24, "MM Zora Cape Grotto Grass 12", "Zora Cape Grotto Grass 12"),
                        M("Grass.png", 587, 432, 24, "MM Zora Cape Grotto Grass 13", "Zora Cape Grotto Grass 13"),
                        M("Grass.png", 408, 402, 24, "MM Zora Cape Grotto Grass 14", "Zora Cape Grotto Grass 14"),
						
						ME("Entrance.png", 622, 483, "Entrance shuffle (Zora Cape)", "MM_GROTTO_EXIT_GENERIC_ZORA_CAPE")
                    }
                },
                new MapSubRegion
                {
                    Name = "Fairy Fountain",
                    BackgroundImage = MM("Zora_Cape", "Fairy"),
                    DestinationEntranceIds = new List<string> { "MM_FAIRY_FOUNTAIN_GREAT_BAY" },
                    Marks = new List<MapMark>
					{
						M("NPC.png", 470, 169, 40, "MM Great Bay Great Fairy", "Great Bay Great Fairy"),
						
						ME("Entrance.png", 476, 541, "Entrance shuffle (Zora Cape)", "MM_GREAT_BAY_FROM_FAIRY_FOUNTAIN")
					}
                }
            };
            return mapRegion;
        }

        public static MapRegion ZoraHall()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Zora Hall";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Zora Hall",
                    BackgroundImage = MM("Zora_Hall", "Zora_Hall"),
                    DestinationEntranceIds = new List<string>
					{
						"MM_ZORA_HALL_FROM_SHOP",
						"MM_ZORA_HALL_FROM_TIJO",
						"MM_ZORA_HALL_FROM_LULU",
						"MM_ZORA_HALL_FROM_EVANS",
						"MM_ZORA_HALL_FROM_JAPAS",
						"MM_ZORA_HALL_UNDERWATER",
						"MM_ZORA_HALL_LEDGE"
					},
                    Marks = new List<MapMark>
					{
						M("NPC.png", 492, 201, 40, "MM Zora Hall Scene Lights", "Zora Hall Scene Lights"),
						
						ME("Entrance.png", 481, 52, "Entrance shuffle (Evan Room)", "MM_ROOM_EVANS"),
						ME("Entrance.png", 667, 198, "Entrance shuffle (Japas Room)", "MM_ROOM_JAPAS"),
						ME("Entrance.png", 398, 82, "Entrance shuffle (Lulu Room)", "MM_ROOM_LULU"),
						ME("Entrance.png", 660, 284, "Entrance shuffle (Mikau and Tijo Room)", "MM_ROOM_TIJO"),
						ME("Entrance.png", 674, 425, "Entrance shuffle (Zora Shop)", "MM_ZORA_SHOP"),
						ME("Entrance.png", 82, 512, "Entrance shuffle (Zora Cape Water)", "MM_ZORA_CAPE_FROM_HALL_WATER"),
						ME("Entrance.png", 754, 374, "Entrance shuffle (Zora Cape Peninsula)", "MM_ZORA_CAPE_PENINSULA")
					}
                },
                new MapSubRegion
                {
                    Name = "Shop",
                    BackgroundImage = MM("Zora_Hall", "Zora_Shop"),
					DestinationEntranceIds = new List<string> { "MM_ZORA_SHOP" },
                    Marks = new List<MapMark>
                    {
                        M("Shop.png", 516, 232, 40, "MM Zora Shop Item 1", "Zora Shop Item 1"),
                        M("Shop.png", 474, 232, 40, "MM Zora Shop Item 2", "Zora Shop Item 2"),
                        M("Shop.png", 432, 232, 40, "MM Zora Shop Item 3", "Zora Shop Item 3"),
						
						ME("Entrance.png", 424, 23, "Entrance shuffle (Zora Hall)", "MM_ZORA_HALL_FROM_SHOP")
                    }
                },
                new MapSubRegion
                {
                    Name = "Mikau and Tijo Room",
                    BackgroundImage = MM("Zora_Hall", "Mikau_Room"),
                    DestinationEntranceIds = new List<string> { "MM_ROOM_TIJO" },
                    Marks = new List<MapMark> { ME("Entrance.png", 462, 549, "Entrance shuffle (Zora Hall)", "MM_ZORA_HALL_FROM_TIJO") }
                },
                new MapSubRegion
                {
                    Name = "Japas Room",
                    BackgroundImage = MM("Zora_Hall", "Japas_Room"),
                    DestinationEntranceIds = new List<string> { "MM_ROOM_JAPAS" },
                    Marks = new List<MapMark> { ME("Entrance.png", 507, 497, "Entrance shuffle (Zora Hall)", "MM_ZORA_HALL_FROM_JAPAS") }
                },
                new MapSubRegion
                {
                    Name = "Evan Room",
                    BackgroundImage = MM("Zora_Hall", "Evan_Room"),
                    DestinationEntranceIds = new List<string> { "MM_ROOM_EVANS" },
                    Marks = new List<MapMark>
					{
						M("NPC.png", 389, 395, 40, "MM Zora Hall Evan HP", "Zora Hall Evan HP"),
						
						ME("Entrance.png", 448, 39, "Entrance shuffle (Zora Hall)", "MM_ZORA_HALL_FROM_EVANS")
					}
                },
                new MapSubRegion
                {
                    Name = "Lulu Room",
                    BackgroundImage = MM("Zora_Hall", "Lulu_Room"),
                    DestinationEntranceIds = new List<string> { "MM_ROOM_LULU" },
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 591, 238, 40, "MM Zora Hall Scrub HP", "Zora Hall Scrub HP"),
                        M("Scrub.png", 463, 277, 40, "MM Zora Hall Scrub Deed", "Zora Hall Scrub Deed"),
                        M("Shop.png", 463, 253, 40, "MM Zora Hall Scrub Shop", "Zora Hall Scrub Shop"),
						
						ME("Entrance.png", 223, 151, "Entrance shuffle (Zora Hall)", "MM_ZORA_HALL_FROM_LULU")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion DekuTree()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Deku Tree";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Lobby",
                    BackgroundImage = "region maps/OoT/Dungeons/Deku_Tree/Lobby.png",
                    DestinationEntranceIds = new List<string> { "OOT_DEKU_TREE" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 887, 517, 40, "OOT Deku Tree Map Chest", "Deku Tree Map Chest"),
                        M("Heart.png", 664, 601, 24, "OOT Deku Tree Heart Main Room Lower", "Deku Tree Heart Main Room Lower"),
                        M("Heart.png", 712, 304, 24, "OOT Deku Tree Heart Main Room Upper", "Deku Tree Heart Main Room Upper"),
                        M("Grass.png", 659, 724, 24, "OOT Deku Tree Grass Lobby 1", "Deku Tree Grass Lobby 1"),
                        M("Grass.png", 613, 731, 24, "OOT Deku Tree Grass Lobby 2", "Deku Tree Grass Lobby 2"),
                        M("Grass.png", 572, 741, 24, "OOT Deku Tree Grass Lobby 3", "Deku Tree Grass Lobby 3"),
                        M("Grass.png", 462, 592, 24, "OOT Deku Tree Grass Lobby 4", "Deku Tree Grass Lobby 4"),
                        M("Grass.png", 488, 582, 24, "OOT Deku Tree Grass Lobby 5", "Deku Tree Grass Lobby 5"),
						
						M("Gold_Skulltula.png", 930, 524, 40, "OOT MQ Deku Tree GS Lobby Crate", "MQ Deku Tree GS Lobby Crate"),
						M("Chest.png", 887, 517, 40, "OOT MQ Deku Tree Map Chest", "MQ Deku Tree Map Chest"),
						M("Heart.png", 664, 601, 24, "OOT MQ Deku Tree Heart Lobby", "MQ Deku Tree Heart Lobby"),
						M("Crate.png", 920, 538, 24, "OOT MQ Deku Tree Main Room 2nd Floor Crate", "MQ Deku Tree Main Room 2nd Floor Crate"),
						M("Grass.png", 659, 724, 24, "OOT MQ Deku Tree Grass Entrance Lower 1", "MQ Deku Tree Grass Entrance Lower 1"),
						M("Grass.png", 613, 731, 24, "OOT MQ Deku Tree Grass Entrance Lower 2", "MQ Deku Tree Grass Entrance Lower 2"),
						M("Grass.png", 572, 741, 24, "OOT MQ Deku Tree Grass Entrance Lower 3", "MQ Deku Tree Grass Entrance Lower 3"),
						M("Grass.png", 458, 792, 24, "OOT MQ Deku Tree Grass Entrance Lower 4", "MQ Deku Tree Grass Entrance Lower 4"),
						M("Grass.png", 458, 824, 24, "OOT MQ Deku Tree Grass Entrance Lower 5", "MQ Deku Tree Grass Entrance Lower 5"),
						M("Grass.png", 488, 582, 24, "OOT MQ Deku Tree Grass Entrance Upper 1", "MQ Deku Tree Grass Entrance Upper 1"),
						M("Grass.png", 462, 592, 24, "OOT MQ Deku Tree Grass Entrance Upper 2", "MQ Deku Tree Grass Entrance Upper 2"),
						
						ME("Entrance.png", 1307, 768, "Entrance shuffle (Kokiri Forest)", "OOT_KOKIRI_FOREST_FROM_DEKU_TREE")
                    }
                },
				new MapSubRegion
                {
                    Name = "Deku Scrub Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Deku_Tree/Deku.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Heart.png", 741, 434, 24, "OOT MQ Deku Tree Heart Before Compass", "MQ Deku Tree Heart Before Compass"),
                        M("Grass.png", 751, 467, 24, "OOT MQ Deku Tree Grass Room Before Compass 1", "MQ Deku Tree Grass Room Before Compass 1"),
						M("Grass.png", 589, 573, 24, "OOT MQ Deku Tree Grass Room Before Compass 2", "MQ Deku Tree Grass Room Before Compass 2"),
						M("Grass.png", 806, 573, 24, "OOT MQ Deku Tree Grass Room Before Compass 3", "MQ Deku Tree Grass Room Before Compass 3"),
						M("Grass.png", 642, 467, 24, "OOT MQ Deku Tree Grass Room Before Compass 4", "MQ Deku Tree Grass Room Before Compass 4"),
						M("Grass.png", 565, 514, 24, "OOT MQ Deku Tree Grass Room Before Compass 5", "MQ Deku Tree Grass Room Before Compass 5"),
						M("Grass.png", 827, 514, 24, "OOT MQ Deku Tree Grass Room Before Compass 6", "MQ Deku Tree Grass Room Before Compass 6"),
						M("Grass.png", 701, 606, 24, "OOT MQ Deku Tree Grass Room Before Compass 7", "MQ Deku Tree Grass Room Before Compass 7")
                    }
                },
                new MapSubRegion
                {
                    Name = "Slingshot Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Deku_Tree/Sling_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 1008, 487, 40, "OOT Deku Tree Slingshot Chest", "Deku Tree Slingshot Chest"),
                        M("Chest.png", 878, 270, 40, "OOT Deku Tree Slingshot Side Chest", "Deku Tree Slingshot Side Chest"),
                        M("Grass.png", 549, 839, 24, "OOT Deku Tree Grass Slingshot Room 1", "Deku Tree Grass Slingshot Room 1"),
                        M("Grass.png", 602, 849, 24, "OOT Deku Tree Grass Slingshot Room 2", "Deku Tree Grass Slingshot Room 2"),
                        M("Grass.png", 549, 701, 24, "OOT Deku Tree Grass Slingshot Room 3", "Deku Tree Grass Slingshot Room 3"),
                        M("Grass.png", 602, 691, 24, "OOT Deku Tree Grass Slingshot Room 4", "Deku Tree Grass Slingshot Room 4"),
						
						M("Gold_Skulltula.png", 867, 189, 40, "OOT MQ Deku Tree GS Compass Room", "MQ Deku Tree GS Compass Room"),
						M("Chest.png", 1008, 487, 40, "OOT MQ Deku Tree Compass Chest", "MQ Deku Tree Compass Chest"),
						M("Heart.png", 886, 278, 24, "OOT MQ Deku Tree Heart Compass Room", "MQ Deku Tree Heart Compass Room"),
						M("Grass.png", 549, 701, 24, "OOT MQ Deku Tree Grass Compass Room 1", "MQ Deku Tree Grass Compass Room 1"),
						M("Grass.png", 602, 691, 24, "OOT MQ Deku Tree Grass Compass Room 2", "MQ Deku Tree Grass Compass Room 2"),
						M("Grass.png", 549, 839, 24, "OOT MQ Deku Tree Grass Compass Room 3", "MQ Deku Tree Grass Compass Room 3"),
						M("Grass.png", 602, 849, 24, "OOT MQ Deku Tree Grass Compass Room 4", "MQ Deku Tree Grass Compass Room 4")
                    }
                },
                new MapSubRegion
                {
                    Name = "Compass Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Deku_Tree/Compass_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 709, 486, 40, "OOT Deku Tree GS Compass", "Deku Tree GS Compass"),
                        M("Chest.png", 1158, 565, 40, "OOT Deku Tree Compass Chest", "Deku Tree Compass Chest"),
                        M("Chest.png", 711, 526, 40, "OOT Deku Tree Compass Room Side Chest", "Deku Tree Compass Room Side Chest"),
                        M("Grass.png", 1131, 550, 24, "OOT Deku Tree Grass Compass Room 1", "Deku Tree Grass Compass Room 1"),
                        M("Grass.png", 1052, 546, 24, "OOT Deku Tree Grass Compass Room 2", "Deku Tree Grass Compass Room 2"),
						
						M("Chest.png", 711, 526, 40, "OOT MQ Deku Tree Slingshot Chest", "MQ Deku Tree Slingshot Chest"),
                        M("Chest.png", 1158, 565, 40, "OOT MQ Deku Tree Slingshot Room Far Chest", "MQ Deku Tree Slingshot Room Far Chest"),
						M("Heart.png", 782, 540, 24, "OOT MQ Deku Tree Heart Slingshot Room", "MQ Deku Tree Heart Slingshot Room"),
						M("Crate.png", 426, 681, 24, "OOT MQ Deku Tree Slingshot Room Large Crate 1", "MQ Deku Tree Slingshot Room Large Crate 1"),
						M("Crate.png", 411, 705, 24, "OOT MQ Deku Tree Slingshot Room Large Crate 2", "MQ Deku Tree Slingshot Room Large Crate 2"),
						M("Grass.png", 1052, 546, 24, "OOT MQ Deku Tree Grass Slingshot Room Back 1", "MQ Deku Tree Grass Slingshot Room Back 1"),
						M("Grass.png", 1131, 550, 24, "OOT MQ Deku Tree Grass Slingshot Room Back 2", "MQ Deku Tree Grass Slingshot Room Back 2"),
						M("Grass.png", 318, 582, 24, "OOT MQ Deku Tree Grass Slingshot Room Front 1", "MQ Deku Tree Grass Slingshot Room Front 1"),
						M("Grass.png", 374, 582, 24, "OOT MQ Deku Tree Grass Slingshot Room Front 2", "MQ Deku Tree Grass Slingshot Room Front 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Basement",
                    BackgroundImage = "region maps/OoT/Dungeons/Deku_Tree/Basement.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 442, 560, 40, "OOT Deku Tree GS Basement Gate", "Deku Tree GS Basement Gate"),
                        M("Gold_Skulltula.png", 850, 480, 40, "OOT Deku Tree GS Basement Vines", "Deku Tree GS Basement Vines"),
                        M("Chest.png", 699, 555, 40, "OOT Deku Tree Basement Chest", "Deku Tree Basement Chest"),
                        M("Grass.png", 964, 633, 24, "OOT Deku Tree Grass Basement Main 1", "Deku Tree Grass Basement Main 1"),
                        M("Grass.png", 1009, 642, 24, "OOT Deku Tree Grass Basement Main 2", "Deku Tree Grass Basement Main 2"),
						
						M("Scrub.png", 857, 769, 40, "OOT MQ Deku Tree Scrub", "MQ Deku Tree Scrub"),
						M("Chest.png", 699, 555, 40, "OOT MQ Deku Tree Basement Chest", "MQ Deku Tree Basement Chest"),
						M("Grass.png", 911, 651, 24, "OOT MQ Deku Tree Grass Basement Lower 1", "MQ Deku Tree Grass Basement Lower 1"),
						M("Grass.png", 938, 638, 24, "OOT MQ Deku Tree Grass Basement Lower 2", "MQ Deku Tree Grass Basement Lower 2"),
						M("Grass.png", 640, 566, 24, "OOT MQ Deku Tree Grass Basement Lower 3", "MQ Deku Tree Grass Basement Lower 3"),
						M("Grass.png", 670, 580, 24, "OOT MQ Deku Tree Grass Basement Lower 4", "MQ Deku Tree Grass Basement Lower 4"),
						M("Grass.png", 141, 709, 24, "OOT MQ Deku Tree Grass Basement Upper 1", "MQ Deku Tree Grass Basement Upper 1"),
						M("Grass.png", 608, 876, 24, "OOT MQ Deku Tree Grass Basement Upper 2", "MQ Deku Tree Grass Basement Upper 2"),
						M("Grass.png", 527, 876, 24, "OOT MQ Deku Tree Grass Basement Upper 3", "MQ Deku Tree Grass Basement Upper 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Eye Switch Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Deku_Tree/Eye_Switch_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Grass.png", 912, 596, 24, "OOT Deku Tree Grass Eye Switch Room 1", "Deku Tree Grass Eye Switch Room 1"),
                        M("Grass.png", 970, 608, 24, "OOT Deku Tree Grass Eye Switch Room 2", "Deku Tree Grass Eye Switch Room 2"),
                        M("Grass.png", 559, 596, 24, "OOT Deku Tree Grass Eye Switch Room 3", "Deku Tree Grass Eye Switch Room 3"),
                        M("Grass.png", 501, 608, 24, "OOT Deku Tree Grass Eye Switch Room 4", "Deku Tree Grass Eye Switch Room 4"),
						
						M("Grass.png", 912, 596, 24, "OOT MQ Deku Tree Grass Room Before Spike 1", "MQ Deku Tree Grass Room Before Spike 1"),
						M("Grass.png", 970, 608, 24, "OOT MQ Deku Tree Grass Room Before Spike 2", "MQ Deku Tree Grass Room Before Spike 2"),
						M("Grass.png", 559, 596, 24, "OOT MQ Deku Tree Grass Room Before Spike 3", "MQ Deku Tree Grass Room Before Spike 3"),
						M("Grass.png", 501, 608, 24, "OOT MQ Deku Tree Grass Room Before Spike 4", "MQ Deku Tree Grass Room Before Spike 4")
                    }
                },
                new MapSubRegion
                {
                    Name = "Water Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Deku_Tree/Water_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Grass.png", 373, 543, 24, "OOT Deku Tree Grass Water Room 1", "Deku Tree Grass Water Room 1"),
                        M("Grass.png", 422, 543, 24, "OOT Deku Tree Grass Water Room 2", "Deku Tree Grass Water Room 2"),
						
						M("Chest.png", 1211, 563, 40, "OOT MQ Deku Tree Before Water Platform Chest", "MQ Deku Tree Before Water Platform Chest"),
						M("Chest.png", 285, 570, 40, "OOT MQ Deku Tree After Water Platform Chest", "MQ Deku Tree After Water Platform Chest"),
						M("Grass.png", 422, 543, 24, "OOT MQ Deku Tree Grass Spike Room Back 1", "MQ Deku Tree Grass Spike Room Back 1"),
						M("Grass.png", 373, 543, 24, "OOT MQ Deku Tree Grass Spike Room Back 2", "MQ Deku Tree Grass Spike Room Back 2"),
						M("Grass.png", 1051, 558, 24, "OOT MQ Deku Tree Grass Spike Room Front 1", "MQ Deku Tree Grass Spike Room Front 1"),
						M("Grass.png", 1081, 558, 24, "OOT MQ Deku Tree Grass Spike Room Front 2", "MQ Deku Tree Grass Spike Room Front 2"),
						M("Grass.png", 1089, 544, 24, "OOT MQ Deku Tree Grass Spike Room Front 3", "MQ Deku Tree Grass Spike Room Front 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Torch Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Deku_Tree/Torch_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Grass.png", 401, 646, 24, "OOT Deku Tree Grass Torch Room 1", "Deku Tree Grass Torch Room 1"),
                        M("Grass.png", 496, 616, 24, "OOT Deku Tree Grass Torch Room 2", "Deku Tree Grass Torch Room 2"),
						
						M("Grass.png", 496, 616, 24, "OOT MQ Deku Tree Grass Larvae Room 1", "MQ Deku Tree Grass Larvae Room 1"),
                        M("Grass.png", 401, 646, 24, "OOT MQ Deku Tree Grass Larvae Room 2", "MQ Deku Tree Grass Larvae Room 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Larva Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Deku_Tree/Larva_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Grass.png", 428, 614, 24, "OOT Deku Tree Grass Larva Room 1", "Deku Tree Grass Larva Room 1"),
                        M("Grass.png", 467, 602, 24, "OOT Deku Tree Grass Larva Room 2", "Deku Tree Grass Larva Room 2"),
						
						M("Gold_Skulltula.png", 720, 616, 40, "OOT MQ Deku Tree GS Song of Time Blocks", "MQ Deku Tree GS Song of Time Blocks"),
						M("Grass.png", 292, 658, 24, "OOT MQ Deku Tree Grass Gravestone Room 1", "MQ Deku Tree Grass Gravestone Room 1"),
						M("Grass.png", 647, 590, 24, "OOT MQ Deku Tree Grass Gravestone Room 2", "MQ Deku Tree Grass Gravestone Room 2"),
						M("Grass.png", 463, 610, 24, "OOT MQ Deku Tree Grass Gravestone Room 3", "MQ Deku Tree Grass Gravestone Room 3"),
						M("Grass.png", 987, 655, 24, "OOT MQ Deku Tree Grass Gravestone Room 4", "MQ Deku Tree Grass Gravestone Room 4"),
						M("Grass.png", 1050, 621, 24, "OOT MQ Deku Tree Grass Gravestone Room 5", "MQ Deku Tree Grass Gravestone Room 5"),
						M("Wonder.png", 580, 587, 24, "OOT MQ Deku Tree Wonder Item 1", "MQ Deku Tree Wonder Item 1"),
						M("Wonder.png", 765, 590, 24, "OOT MQ Deku Tree Wonder Item 2", "MQ Deku Tree Wonder Item 2"),
						M("Wonder.png", 622, 677, 24, "OOT MQ Deku Tree Wonder Item 3", "MQ Deku Tree Wonder Item 3"),
						M("Wonder.png", 1016, 610, 24, "OOT MQ Deku Tree Wonder Item 4", "MQ Deku Tree Wonder Item 4")
                    }
                },
                new MapSubRegion
                {
                    Name = "Back Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Deku_Tree/Basement_Back_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
					{
						M("Gold_Skulltula.png", 494, 354, 40, "OOT Deku Tree GS Basement Back Room", "Deku Tree GS Basement Back Room"),
						
						M("Gold_Skulltula.png", 1031, 445, 40, "OOT MQ Deku Tree GS Back Room", "MQ Deku Tree GS Back Room"),
						M("Grass.png", 476, 565, 24, "OOT MQ Deku Tree Grass Back Room 1", "MQ Deku Tree Grass Back Room 1"),
						M("Grass.png", 366, 647, 24, "OOT MQ Deku Tree Grass Back Room 2", "MQ Deku Tree Grass Back Room 2"),
						M("Grass.png", 905, 571, 24, "OOT MQ Deku Tree Grass Back Room 3", "MQ Deku Tree Grass Back Room 3")
					}
                },
                new MapSubRegion
                {
                    Name = "Pre-Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Deku_Tree/Pre-Boss_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Heart.png", 410, 704, 24, "OOT Deku Tree Heart Pre-Boss 1", "Deku Tree Heart Pre-Boss 1"),
                        M("Heart.png", 520, 751, 24, "OOT Deku Tree Heart Pre-Boss 2", "Deku Tree Heart Pre-Boss 2"),
                        M("Heart.png", 1027, 624, 24, "OOT Deku Tree Heart Pre-Boss 3", "Deku Tree Heart Pre-Boss 3"),
                        M("Grass.png", 884, 485, 24, "OOT Deku Tree Grass Pre-Boss Room 1", "Deku Tree Grass Pre-Boss Room 1"),
                        M("Grass.png", 907, 492, 24, "OOT Deku Tree Grass Pre-Boss Room 2", "Deku Tree Grass Pre-Boss Room 2"),
                        M("Grass.png", 930, 499, 24, "OOT Deku Tree Grass Pre-Boss Room 3", "Deku Tree Grass Pre-Boss Room 3"),
						
						M("Heart.png", 410, 704, 24, "OOT MQ Deku Tree Heart Before Boss 1", "MQ Deku Tree Heart Before Boss 1"),
                        M("Heart.png", 520, 751, 24, "OOT MQ Deku Tree Heart Before Boss 2", "MQ Deku Tree Heart Before Boss 2"),
                        M("Heart.png", 1027, 624, 24, "OOT MQ Deku Tree Heart Before Boss 3", "MQ Deku Tree Heart Before Boss 3"),
                        M("Grass.png", 884, 485, 24, "OOT MQ Deku Tree Grass Room Before Boss 1", "MQ Deku Tree Grass Room Before Boss 1"),
                        M("Grass.png", 907, 492, 24, "OOT MQ Deku Tree Grass Room Before Boss 2", "MQ Deku Tree Grass Room Before Boss 2"),
                        M("Grass.png", 930, 499, 24, "OOT MQ Deku Tree Grass Room Before Boss 3", "MQ Deku Tree Grass Room Before Boss 3"),
						
						ME("Entrance.png", 378, 435, "Entrance shuffle (Queen Gohma)", "OOT_BOSS_DEKU_TREE")
                    }
                },
                new MapSubRegion
                {
                    Name = "Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Deku_Tree/Boss_Room.png",
                    DestinationEntranceIds = new List<string> { "OOT_BOSS_DEKU_TREE" },
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 592, 578, 40, "OOT Deku Tree Boss Container", "Deku Tree Boss Container"),
                        M("NPC.png", 670, 578, 40, "OOT Deku Tree Boss", "Deku Tree Boss"),
                        M("Grass.png", 314, 638, 24, "OOT Deku Tree Boss Grass 1", "Deku Tree Boss Grass 1"),
                        M("Grass.png", 300, 588, 24, "OOT Deku Tree Boss Grass 2", "Deku Tree Boss Grass 2"),
                        M("Grass.png", 1043, 638, 24, "OOT Deku Tree Boss Grass 3", "Deku Tree Boss Grass 3"),
                        M("Grass.png", 1059, 588, 24, "OOT Deku Tree Boss Grass 4", "Deku Tree Boss Grass 4"),
                        M("Grass.png", 900, 547, 24, "OOT Deku Tree Boss Grass 5", "Deku Tree Boss Grass 5"),
                        M("Grass.png", 971, 563, 24, "OOT Deku Tree Boss Grass 6", "Deku Tree Boss Grass 6"),
                        M("Grass.png", 386, 563, 24, "OOT Deku Tree Boss Grass 7", "Deku Tree Boss Grass 7"),
                        M("Grass.png", 454, 547, 24, "OOT Deku Tree Boss Grass 8", "Deku Tree Boss Grass 8")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion DodongoCavern()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Dodongo Cavern";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Lobby",
                    BackgroundImage = "region maps/OoT/Dungeons/Cavern/Lobby.png",
                    DestinationEntranceIds = new List<string> { "OOT_DODONGO_CAVERN" },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 396, 269, 40, "OOT Dodongo Cavern Map Chest", "Dodongo Cavern Map Chest"),
                        M("Chest.png", 544, 145, 40, "OOT Dodongo Cavern Bridge Chest", "Dodongo Cavern Bridge Chest"),
                        M("Scrub.png", 255, 276, 40, "OOT Dodongo Cavern Lobby Scrub", "Dodongo Cavern Lobby Scrub"),
                        M("Grass.png", 292, 165, 24, "OOT Dodongo Cavern Grass Lobby", "Dodongo Cavern Grass Lobby"),
						
						M("Scrub.png", 234, 285, 40, "OOT MQ Dodongo Cavern Lobby Scrub Front", "MQ Dodongo Cavern Lobby Scrub Front"),
						M("Scrub.png", 269, 269, 40, "OOT MQ Dodongo Cavern Lobby Scrub Back", "MQ Dodongo Cavern Lobby Scrub Back"),
						M("Chest.png", 788, 443, 40, "OOT MQ Dodongo Cavern Map Chest", "MQ Dodongo Cavern Map Chest"),
						M("Chest.png", 529, 438, 40, "OOT MQ Dodongo Cavern Bomb Bag Chest", "MQ Dodongo Cavern Bomb Bag Chest"),
						
						ME("Entrance.png", 27, 365, "Entrance shuffle (Death Mountain Trail)", "OOT_MOUNTAIN_TRAIL_FROM_DODONGO_CAVERN")
                    }
                },
                new MapSubRegion
                {
                    Name = "Right Corridor",
                    BackgroundImage = "region maps/OoT/Dungeons/Cavern/Right_Corridor.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 327, 112, 40, "OOT Dodongo Cavern GS Scarecrow", "Dodongo Cavern GS Scarecrow"),
						M("Gold_Skulltula.png", 438, 541, 40, "OOT Dodongo Cavern GS Side Room", "Dodongo Cavern GS Side Room"),
                        M("Grass.png", 384, 403, 24, "OOT Dodongo Cavern Grass East Corridor Side Room", "Dodongo Cavern Grass East Corridor Side Room"),
                        M("Pot.png", 202, 219, 24, "OOT Dodongo Cavern Pot Right Corridor Pot 1", "Dodongo Cavern Pot Right Corridor Pot 1"),
                        M("Pot.png", 217, 237, 24, "OOT Dodongo Cavern Pot Right Corridor Pot 2", "Dodongo Cavern Pot Right Corridor Pot 2"),
                        M("Pot.png", 452, 224, 24, "OOT Dodongo Cavern Pot Right Corridor Pot 3", "Dodongo Cavern Pot Right Corridor Pot 3"),
                        M("Pot.png", 474, 224, 24, "OOT Dodongo Cavern Pot Right Corridor Pot 4", "Dodongo Cavern Pot Right Corridor Pot 4"),
                        M("Pot.png", 724, 106, 24, "OOT Dodongo Cavern Pot Right Corridor Pot 5", "Dodongo Cavern Pot Right Corridor Pot 5"),
                        M("Pot.png", 764, 127, 24, "OOT Dodongo Cavern Pot Right Corridor Pot 6", "Dodongo Cavern Pot Right Corridor Pot 6"),
						
						M("Scrub.png", 445, 460, 40, "OOT MQ Dodongo Cavern Tunnel Side Scrub", "MQ Dodongo Cavern Tunnel Side Scrub"),
						M("Pot.png", 173, 288, 24, "OOT MQ Dodongo Cavern Pot East Corridor 1", "MQ Dodongo Cavern Pot East Corridor 1"),
						M("Pot.png", 219, 288, 24, "OOT MQ Dodongo Cavern Pot East Corridor 2", "MQ Dodongo Cavern Pot East Corridor 2"),
						M("Pot.png", 731, 218, 24, "OOT MQ Dodongo Cavern Pot East Corridor 3", "MQ Dodongo Cavern Pot East Corridor 3"),
						M("Pot.png", 811, 218, 24, "OOT MQ Dodongo Cavern Pot East Corridor 4", "MQ Dodongo Cavern Pot East Corridor 4")
                    }
                },
                new MapSubRegion
                {
                    Name = "Lizalfos Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Cavern/Lizalfos.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Heart.png", 702, 391, 24, "OOT Dodongo Cavern Heart Miniboss Lava", "Dodongo Cavern Heart Miniboss Lava"),
                        M("Heart.png", 401, 194, 24, "OOT Dodongo Cavern Heart Miniboss Upper 1", "Dodongo Cavern Heart Miniboss Upper 1"),
                        M("Heart.png", 368, 228, 24, "OOT Dodongo Cavern Heart Miniboss Upper 2", "Dodongo Cavern Heart Miniboss Upper 2"),
                        M("Pot.png", 80, 262, 24, "OOT Dodongo Cavern Pot Miniboss 1", "Dodongo Cavern Pot Miniboss 1"),
                        M("Pot.png", 98, 233, 24, "OOT Dodongo Cavern Pot Miniboss 2", "Dodongo Cavern Pot Miniboss 2"),
                        M("Pot.png", 416, 52, 24, "OOT Dodongo Cavern Pot Miniboss 3", "Dodongo Cavern Pot Miniboss 3"),
                        M("Pot.png", 447, 52, 24, "OOT Dodongo Cavern Pot Miniboss 4", "Dodongo Cavern Pot Miniboss 4"),
						
						M("Gold_Skulltula.png", 374, 202, 40, "OOT MQ Dodongo Cavern GS Upper Lizalfos", "MQ Dodongo Cavern GS Upper Lizalfos"),
						M("Heart.png", 702, 391, 24, "OOT MQ Dodongo Cavern Heart Lizalfos Room", "MQ Dodongo Cavern Heart Lizalfos Room"),
						M("Pot.png", 521, 756, 24, "OOT MQ Dodongo Cavern Pot Miniboss 1", "MQ Dodongo Cavern Pot Miniboss 1"),
						M("Pot.png", 521, 686, 24, "OOT MQ Dodongo Cavern Pot Miniboss 2", "MQ Dodongo Cavern Pot Miniboss 2"),
						M("Pot.png", 906, 629, 24, "OOT MQ Dodongo Cavern Pot Miniboss 3", "MQ Dodongo Cavern Pot Miniboss 3"),
						M("Pot.png", 906, 564, 24, "OOT MQ Dodongo Cavern Pot Miniboss 4", "MQ Dodongo Cavern Pot Miniboss 4")
                    }
                },
                new MapSubRegion
                {
                    Name = "Green Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Cavern/Blue_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Scrub.png", 769, 207, 40, "OOT Dodongo Cavern Green Side Room Scrub", "Dodongo Cavern Green Side Room Scrub"),
                        M("Pot.png", 445, 316, 24, "OOT Dodongo Cavern Pot Green Room Pot 1", "Dodongo Cavern Pot Green Room Pot 1"),
                        M("Pot.png", 224, 384, 24, "OOT Dodongo Cavern Pot Green Room Pot 2", "Dodongo Cavern Pot Green Room Pot 2"),
                        M("Pot.png", 389, 284, 24, "OOT Dodongo Cavern Pot Green Room Pot 3", "Dodongo Cavern Pot Green Room Pot 3"),
                        M("Pot.png", 439, 390, 24, "OOT Dodongo Cavern Pot Green Room Pot 4", "Dodongo Cavern Pot Green Room Pot 4"),
						
						M("Gold_Skulltula.png", 781, 85, 40, "OOT MQ Dodongo Cavern GS Poe Room Side", "MQ Dodongo Cavern GS Poe Room Side"),
						M("Grass.png", 835, 213, 24, "OOT MQ Dodongo Cavern Grass Green Corridor Side Room 1", "MQ Dodongo Cavern Grass Green Corridor Side Room 1"),
						M("Grass.png", 840, 236, 24, "OOT MQ Dodongo Cavern Grass Green Corridor Side Room 2", "MQ Dodongo Cavern Grass Green Corridor Side Room 2"),
						M("Pot.png", 389, 284, 24, "OOT MQ Dodongo Cavern Pot Green Room 1", "MQ Dodongo Cavern Pot Green Room 1"),
						M("Pot.png", 541, 318, 24, "OOT MQ Dodongo Cavern Pot Green Room 2", "MQ Dodongo Cavern Pot Green Room 2"),
						M("Pot.png", 322, 445, 24, "OOT MQ Dodongo Cavern Pot Green Room 3", "MQ Dodongo Cavern Pot Green Room 3"),
						M("Pot.png", 209, 304, 24, "OOT MQ Dodongo Cavern Pot Green Room 4", "MQ Dodongo Cavern Pot Green Room 4"),
						M("Crate.png", 273, 484, 24, "OOT MQ Dodongo Cavern Poe Room Large Crate 1", "MQ Dodongo Cavern Poe Room Large Crate 1"),
						M("Crate.png", 249, 499, 24, "OOT MQ Dodongo Cavern Poe Room Large Crate 2", "MQ Dodongo Cavern Poe Room Large Crate 2"),
						M("Crate.png", 475, 366, 24, "OOT MQ Dodongo Cavern Poe Room Large Crate 3", "MQ Dodongo Cavern Poe Room Large Crate 3"),
						M("Crate.png", 451, 378, 24, "OOT MQ Dodongo Cavern Poe Room Large Crate 4", "MQ Dodongo Cavern Poe Room Large Crate 4"),
						M("Crate.png", 256, 284, 24, "OOT MQ Dodongo Cavern Poe Room Large Crate 5", "MQ Dodongo Cavern Poe Room Large Crate 5"),
						M("Crate.png", 232, 293, 24, "OOT MQ Dodongo Cavern Poe Room Large Crate 6", "MQ Dodongo Cavern Poe Room Large Crate 6"),
						M("Crate.png", 78, 451, 24, "OOT MQ Dodongo Cavern Poe Room Large Crate 7", "MQ Dodongo Cavern Poe Room Large Crate 7"),
						M("Crate.png", 90, 475, 24, "OOT MQ Dodongo Cavern Poe Room Large Crate 8", "MQ Dodongo Cavern Poe Room Large Crate 8")
                    }
                },
                new MapSubRegion
                {
                    Name = "Bombable Staris",
                    BackgroundImage = "region maps/OoT/Dungeons/Cavern/Stairs.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 162, 95, 40, "OOT Dodongo Cavern GS Stairs Top", "Dodongo Cavern GS Stairs Top"),
                        M("Gold_Skulltula.png", 385, 103, 40, "OOT Dodongo Cavern GS Stairs Vines", "Dodongo Cavern GS Stairs Vines"),
                        M("Chest.png", 805, 503, 40, "OOT Dodongo Cavern Compass Chest", "Dodongo Cavern Compass Chest"),
                        M("Pot.png", 85, 249, 24, "OOT Dodongo Cavern Pot Stairs Pot 1", "Dodongo Cavern Pot Stairs Pot 1"),
                        M("Pot.png", 104, 236, 24, "OOT Dodongo Cavern Pot Stairs Pot 2", "Dodongo Cavern Pot Stairs Pot 2"),
                        M("Pot.png", 335, 141, 24, "OOT Dodongo Cavern Pot Stairs Pot 3", "Dodongo Cavern Pot Stairs Pot 3"),
                        M("Pot.png", 357, 141, 24, "OOT Dodongo Cavern Pot Stairs Pot 4", "Dodongo Cavern Pot Stairs Pot 4"),
						
						M("Gold_Skulltula.png", 809, 462, 40, "OOT MQ Dodongo Cavern GS Time Blocks", "MQ Dodongo Cavern GS Time Blocks"),
						M("Scrub.png", 185, 119, 40, "OOT MQ Dodongo Cavern Staircase Scrub", "MQ Dodongo Cavern Staircase Scrub"),
						M("Pot.png", 140, 383, 24, "OOT MQ Dodongo Cavern Pot Stairs 1", "MQ Dodongo Cavern Pot Stairs 1"),
						M("Pot.png", 367, 458, 24, "OOT MQ Dodongo Cavern Pot Stairs 2", "MQ Dodongo Cavern Pot Stairs 2"),
						M("Pot.png", 345, 254, 24, "OOT MQ Dodongo Cavern Pot Stairs 3", "MQ Dodongo Cavern Pot Stairs 3"),
						M("Pot.png", 511, 321, 24, "OOT MQ Dodongo Cavern Pot Stairs 4", "MQ Dodongo Cavern Pot Stairs 4"),
						M("Crate.png", 189, 237, 24, "OOT MQ Dodongo Cavern Staircase Room Lower Large Crate 1", "MQ Dodongo Cavern Staircase Room Lower Large Crate 1"),
						M("Crate.png", 298, 190, 24, "OOT MQ Dodongo Cavern Staircase Room Lower Large Crate 2", "MQ Dodongo Cavern Staircase Room Lower Large Crate 2"),
						M("Crate.png", 88, 246, 24, "OOT MQ Dodongo Cavern Staircase Room Upper Large Crate 1", "MQ Dodongo Cavern Staircase Room Upper Large Crate 1"),
						M("Crate.png", 312, 266, 24, "OOT MQ Dodongo Cavern Staircase Room Upper Large Crate 2", "MQ Dodongo Cavern Staircase Room Upper Large Crate 2"),
						M("Crate.png", 535, 144, 24, "OOT MQ Dodongo Cavern Staircase Room Upper Large Crate 3", "MQ Dodongo Cavern Staircase Room Upper Large Crate 3"),
						M("Crate.png", 345, 142, 24, "OOT MQ Dodongo Cavern Staircase Room Upper Large Crate 4", "MQ Dodongo Cavern Staircase Room Upper Large Crate 4"),
						M("Silver_Rupee.png", 288, 421, 24, "OOT MQ Dodongo Cavern SR Beamos", "MQ Dodongo Cavern SR Beamos"),
						M("Silver_Rupee.png", 213, 237, 24, "OOT MQ Dodongo Cavern SR Crate", "MQ Dodongo Cavern SR Crate"),
						M("Silver_Rupee.png", 411, 121, 24, "OOT MQ Dodongo Cavern SR Vines", "MQ Dodongo Cavern SR Vines"),
						M("Silver_Rupee.png", 88, 222, 24, "OOT MQ Dodongo Cavern SR Upper Corner Low", "MQ Dodongo Cavern SR Upper Corner Low"),
						M("Silver_Rupee.png", 535, 120, 24, "OOT MQ Dodongo Cavern SR Upper Corner High", "MQ Dodongo Cavern SR Upper Corner High")
                    }
                },
				new MapSubRegion
                {
                    Name = "After Stairs Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Cavern/After_Climb.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 676, 281, 40, "OOT MQ Dodongo Cavern Compass Chest", "MQ Dodongo Cavern Compass Chest"),
						M("Grass.png", 836, 632, 24, "OOT MQ Dodongo Cavern Grass Compass Room 1", "MQ Dodongo Cavern Grass Compass Room 1"),
						M("Grass.png", 836, 337, 24, "OOT MQ Dodongo Cavern Grass Compass Room 2", "MQ Dodongo Cavern Grass Compass Room 2"),
						M("Grass.png", 535, 337, 24, "OOT MQ Dodongo Cavern Grass Compass Room 3", "MQ Dodongo Cavern Grass Compass Room 3"),
						M("Grass.png", 535, 632, 24, "OOT MQ Dodongo Cavern Grass Compass Room 4", "MQ Dodongo Cavern Grass Compass Room 4")
                    }
                },
                new MapSubRegion
                {
                    Name = "Bomb Bag Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Cavern/Bomb_Bag_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 514, 170, 40, "OOT Dodongo Cavern Bomb Bag Chest", "Dodongo Cavern Bomb Bag Chest"),
                        M("Chest.png", 135, 265, 40, "OOT Dodongo Cavern Bomb Bag Side Chest", "Dodongo Cavern Bomb Bag Side Chest"),
                        M("Scrub.png", 794, 212, 40, "OOT Dodongo Cavern Bomb Bag Side Room Left Scrub", "Dodongo Cavern Bomb Bag Side Room Left Scrub"),
                        M("Scrub.png", 803, 351, 40, "OOT Dodongo Cavern Bomb Bag Side Room Right Scrub", "Dodongo Cavern Bomb Bag Side Room Right Scrub"),
                        M("Heart.png", 195, 363, 24, "OOT Dodongo Cavern Heart Bomb Bag Room", "Dodongo Cavern Heart Bomb Bag Room"),
                        M("Grass.png", 539, 244, 24, "OOT Dodongo Cavern Grass Bomb Bag Room", "Dodongo Cavern Grass Bomb Bag Room"),
                        M("Pot.png", 257, 206, 24, "OOT Dodongo Cavern Pot Bomb Bag Room 1", "Dodongo Cavern Pot Bomb Bag Room 1"),
                        M("Pot.png", 281, 279, 24, "OOT Dodongo Cavern Pot Bomb Bag Room 2", "Dodongo Cavern Pot Bomb Bag Room 2"),
						
						M("Gold_Skulltula.png", 773, 357, 40, "OOT MQ Dodongo Cavern GS Larve Room", "MQ Dodongo Cavern GS Larve Room"),
						M("Chest.png", 249, 198, 40, "OOT MQ Dodongo Cavern Upper Ledge Chest", "MQ Dodongo Cavern Upper Ledge Chest"),
						M("Chest.png", 732, 282, 40, "OOT MQ Dodongo Cavern Larvae Room Chest", "MQ Dodongo Cavern Larvae Room Chest"),
						M("Crate.png", 750, 214, 24, "OOT MQ Dodongo Cavern Larve Room Large Crate 1", "MQ Dodongo Cavern Larve Room Large Crate 1"),
						M("Crate.png", 726, 214, 24, "OOT MQ Dodongo Cavern Larve Room Large Crate 2", "MQ Dodongo Cavern Larve Room Large Crate 2"),
						M("Crate.png", 816, 276, 24, "OOT MQ Dodongo Cavern Larve Room Large Crate 3", "MQ Dodongo Cavern Larve Room Large Crate 3"),
						M("Crate.png", 816, 300, 24, "OOT MQ Dodongo Cavern Larve Room Large Crate 4", "MQ Dodongo Cavern Larve Room Large Crate 4"),
						M("Crate.png", 750, 371, 24, "OOT MQ Dodongo Cavern Larve Room Large Crate 5", "MQ Dodongo Cavern Larve Room Large Crate 5"),
						M("Crate.png", 726, 371, 24, "OOT MQ Dodongo Cavern Larve Room Large Crate 6", "MQ Dodongo Cavern Larve Room Large Crate 6"),
						M("Heart.png", 288, 333, 24, "OOT MQ Dodongo Cavern Heart Vanilla Bomb Bag Room", "MQ Dodongo Cavern Heart Vanilla Bomb Bag Room"),
						M("Grass.png", 542, 243, 24, "OOT MQ Dodongo Cavern Grass Vanilla Bomb Bag Room", "MQ Dodongo Cavern Grass Vanilla Bomb Bag Room"),
						M("Pot.png", 281, 280, 24, "OOT MQ Dodongo Cavern Pot Vanilla Bomb Bag Room 1", "MQ Dodongo Cavern Pot Vanilla Bomb Bag Room 1"),
						M("Pot.png", 542, 158, 24, "OOT MQ Dodongo Cavern Pot Vanilla Bomb Bag Room 2", "MQ Dodongo Cavern Pot Vanilla Bomb Bag Room 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Side Mini-Boss Corridors",
                    BackgroundImage = "region maps/OoT/Dungeons/Cavern/Side_Mini-Boss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Grass.png", 516, 209, 24, "OOT Dodongo Cavern Grass Pre-Miniboss", "Dodongo Cavern Grass Pre-Miniboss"),
                        M("Pot.png", 223, 450, 24, "OOT Dodongo Cavern Pot Room After Miniboss 1", "Dodongo Cavern Pot Room After Miniboss 1"),
                        M("Pot.png", 386, 149, 24, "OOT Dodongo Cavern Pot Room After Miniboss 2", "Dodongo Cavern Pot Room After Miniboss 2"),
                        M("Pot.png", 677, 208, 24, "OOT Dodongo Cavern Pot Room Before Miniboss 1", "Dodongo Cavern Pot Room Before Miniboss 1"),
                        M("Pot.png", 521, 443, 24, "OOT Dodongo Cavern Pot Room Before Miniboss 2", "Dodongo Cavern Pot Room Before Miniboss 2"),
						
						M("Grass.png", 516, 209, 24, "OOT MQ Dodongo Cavern Grass Room Before Miniboss", "MQ Dodongo Cavern Grass Room Before Miniboss"),
						M("Crate.png", 214, 138, 24, "OOT MQ Dodongo Cavern Room After Upper Lizalfos Large Crate 1", "MQ Dodongo Cavern Room After Upper Lizalfos Large Crate 1"),
						M("Crate.png", 394, 450, 24, "OOT MQ Dodongo Cavern Room After Upper Lizalfos Large Crate 2", "MQ Dodongo Cavern Room After Upper Lizalfos Large Crate 2"),
						M("Pot.png", 359, 145, 24, "OOT MQ Dodongo Cavern Pot After Miniboss 1", "MQ Dodongo Cavern Pot After Miniboss 1"),
						M("Pot.png", 386, 145, 24, "OOT MQ Dodongo Cavern Pot After Miniboss 2", "MQ Dodongo Cavern Pot After Miniboss 2"),
						M("Pot.png", 521, 443, 24, "OOT MQ Dodongo Cavern Pot Before Miniboss 1", "MQ Dodongo Cavern Pot Before Miniboss 1"),
						M("Pot.png", 677, 208, 24, "OOT MQ Dodongo Cavern Pot Before Miniboss 2", "MQ Dodongo Cavern Pot Before Miniboss 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Pre-Boss Rooms",
                    BackgroundImage = "region maps/OoT/Dungeons/Cavern/Pre-Boss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 818, 477, 40, "OOT Dodongo Cavern GS Near Boss", "Dodongo Cavern GS Near Boss"),
                        M("Grass.png", 226, 79, 24, "OOT Dodongo Cavern Grass Pre-Boss", "Dodongo Cavern Grass Pre-Boss"),
                        M("Pot.png", 530, 125, 24, "OOT Dodongo Cavern Pot Skull 1", "Dodongo Cavern Pot Skull 1"),
                        M("Pot.png", 542, 146, 24, "OOT Dodongo Cavern Pot Skull 2", "Dodongo Cavern Pot Skull 2"),
                        M("Pot.png", 503, 348, 24, "OOT Dodongo Cavern Pot Skull 3", "Dodongo Cavern Pot Skull 3"),
                        M("Pot.png", 503, 372, 24, "OOT Dodongo Cavern Pot Skull 4", "Dodongo Cavern Pot Skull 4"),
						
						M("Gold_Skulltula.png", 496, 504, 40, "OOT MQ Dodongo Cavern GS Near Boss", "MQ Dodongo Cavern GS Near Boss"),
						M("Chest.png", 739, 482, 40, "OOT MQ Dodongo Cavern Chest Under Grave", "MQ Dodongo Cavern Chest Under Grave"),
						M("Grass.png", 571, 577, 24, "OOT MQ Dodongo Cavern Grass Boss Loop", "MQ Dodongo Cavern Grass Boss Loop"),
						M("Grass.png", 789, 433, 24, "OOT MQ Dodongo Cavern Grass Boss Loop Side Room", "MQ Dodongo Cavern Grass Boss Loop Side Room"),
						M("Pot.png", 338, 185, 24, "OOT MQ Dodongo Cavern Pot Before Boss 1", "MQ Dodongo Cavern Pot Before Boss 1"),
						M("Pot.png", 224, 81, 24, "OOT MQ Dodongo Cavern Pot Before Boss 2", "MQ Dodongo Cavern Pot Before Boss 2"),
						M("Pot.png", 503, 348, 24, "OOT MQ Dodongo Cavern Pot Before Boss Loop 1", "MQ Dodongo Cavern Pot Before Boss Loop 1"),
						M("Pot.png", 503, 372, 24, "OOT MQ Dodongo Cavern Pot Before Boss Loop 2", "MQ Dodongo Cavern Pot Before Boss Loop 2"),
						M("Pot.png", 358, 573, 24, "OOT MQ Dodongo Cavern Pot Before Boss Loop 3", "MQ Dodongo Cavern Pot Before Boss Loop 3"),
						M("Pot.png", 358, 415, 24, "OOT MQ Dodongo Cavern Pot Before Boss Loop 4", "MQ Dodongo Cavern Pot Before Boss Loop 4"),
						M("Pot.png", 691, 550, 24, "OOT MQ Dodongo Cavern Pot Before Boss Loop Side Room 1", "MQ Dodongo Cavern Pot Before Boss Loop Side Room 1"),
						M("Pot.png", 807, 550, 24, "OOT MQ Dodongo Cavern Pot Before Boss Loop Side Room 2", "MQ Dodongo Cavern Pot Before Boss Loop Side Room 2"),
						
						ME("Entrance.png", 272, 42, "Entrance shuffle (King Dodongo)", "OOT_BOSS_DODONGO_CAVERN")
                    }
                },
                new MapSubRegion
                {
                    Name = "Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Cavern/Boss_Room.png",
                    DestinationEntranceIds = new List<string> { "OOT_BOSS_DODONGO_CAVERN" },
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 438, 427, 40, "OOT Dodongo Cavern Boss Container", "Dodongo Cavern Boss Container"),
                        M("Chest.png", 317, 110, 40, "OOT Dodongo Cavern Boss Chest", "Dodongo Cavern Boss Chest"),
                        M("NPC.png", 469, 371, 40, "OOT Dodongo Cavern Boss", "Dodongo Cavern Boss")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion JabuJabuBelly()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Jabu-Jabu Belly";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Entrance",
                    BackgroundImage = "region maps/OoT/Dungeons/Jabu-Jabu/Entrance.png",
                    DestinationEntranceIds = new List<string> { "OOT_JABU_JABU" },
                    Marks = new List<MapMark>
					{
						M("Chest.png", 553, 325, 40, "OOT MQ Jabu-Jabu Map Chest", "MQ Jabu-Jabu Map Chest"),
						M("Chest.png", 495, 234, 40, "OOT MQ Jabu-Jabu Entry Chest", "MQ Jabu-Jabu Entry Chest"),
						M("Grass.png", 418, 240, 24, "OOT MQ Jabu-Jabu Grass Entrance 1", "MQ Jabu-Jabu Grass Entrance 1"),
						M("Grass.png", 580, 433, 24, "OOT MQ Jabu-Jabu Grass Entrance 2", "MQ Jabu-Jabu Grass Entrance 2"),
						M("Pot.png", 394, 416, 24, "OOT MQ Jabu-Jabu Pot Entrance 1", "MQ Jabu-Jabu Pot Entrance 1"),
						M("Pot.png", 576, 240, 24, "OOT MQ Jabu-Jabu Pot Entrance 2", "MQ Jabu-Jabu Pot Entrance 2"),
						M("Wonder.png", 342, 428, 24, "OOT MQ Jabu-Jabu Wonder Item Entrance Left Cow", "MQ Jabu-Jabu Wonder Item Entrance Left Cow"),
						M("Wonder.png", 342, 232, 24, "OOT MQ Jabu-Jabu Wonder Item Entrance Right Cow", "MQ Jabu-Jabu Wonder Item Entrance Right Cow"),
						
						ME("Entrance.png", 833, 319, "Entrance shuffle (Zora Fountain)", "OOT_ZORA_FOUNTAIN_FROM_JABU_JABU")
					}
                },
                new MapSubRegion
                {
                    Name = "Lobby",
                    BackgroundImage = "region maps/OoT/Dungeons/Jabu-Jabu/First_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Scrub.png", 345, 425, 40, "OOT Jabu-Jabu Scrub", "Jabu-Jabu Scrub"),
                        M("Crate.png", 477, 236, 24, "OOT Jabu-Jabu Small Crate 1", "Jabu-Jabu Small Crate 1"),
                        M("Crate.png", 501, 236, 24, "OOT Jabu-Jabu Small Crate 2", "Jabu-Jabu Small Crate 2"),
						
						M("Chest.png", 589, 262, 40, "OOT MQ Jabu-Jabu Second Room 1F Chest", "MQ Jabu-Jabu Second Room 1F Chest"),
						M("Chest.png", 511, 503, 40, "OOT MQ Jabu-Jabu Second Room B1 Chest", "MQ Jabu-Jabu Second Room B1 Chest"),
						M("Chest.png", 351, 429, 40, "OOT MQ Jabu-Jabu Compass Chest", "MQ Jabu-Jabu Compass Chest"),
						M("Heart.png", 417, 245, 24, "OOT MQ Jabu-Jabu Heart 1", "MQ Jabu-Jabu Heart 1"),
						M("Heart.png", 458, 232, 24, "OOT MQ Jabu-Jabu Heart 2", "MQ Jabu-Jabu Heart 2"),
						M("Rupee.png", 406, 528, 24, "OOT MQ Jabu-Jabu Rupee Bottom", "MQ Jabu-Jabu Rupee Bottom"),
						M("Rupee.png", 386, 507, 24, "OOT MQ Jabu-Jabu Rupee Middle", "MQ Jabu-Jabu Rupee Middle"),
						M("Rupee.png", 371, 479, 24, "OOT MQ Jabu-Jabu Rupee Top", "MQ Jabu-Jabu Rupee Top"),
						M("Pot.png", 376, 408, 24, "OOT MQ Jabu-Jabu Pot Underwater Alcove 1", "MQ Jabu-Jabu Pot Underwater Alcove 1"),
						M("Pot.png", 325, 444, 24, "OOT MQ Jabu-Jabu Pot Underwater Alcove 2", "MQ Jabu-Jabu Pot Underwater Alcove 2"),
						M("Wonder.png", 572, 227, 24, "OOT MQ Jabu-Jabu Wonder Item Elevator Cow", "MQ Jabu-Jabu Wonder Item Elevator Cow")
                    }
                },
				new MapSubRegion
                {
                    Name = "Holes Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Jabu-Jabu/Hole.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Grass.png", 464, 398, 24, "OOT MQ Jabu-Jabu Grass Main Room Top 1", "MQ Jabu-Jabu Grass Main Room Top 1"),
						M("Grass.png", 312, 141, 24, "OOT MQ Jabu-Jabu Grass Main Room Top 2", "MQ Jabu-Jabu Grass Main Room Top 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Under Holes Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Jabu-Jabu/Holes_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 549, 246, 40, "OOT Jabu-Jabu GS Bottom Lower", "Jabu-Jabu GS Bottom Lower"),
                        M("Gold_Skulltula.png", 415, 221, 40, "OOT Jabu-Jabu GS Bottom Upper", "Jabu-Jabu GS Bottom Upper"),
						
						M("Chest.png", 229, 457, 40, "OOT MQ Jabu-Jabu Third Room West Chest", "MQ Jabu-Jabu Third Room West Chest"),
						M("Chest.png", 701, 384, 40, "OOT MQ Jabu-Jabu Third Room East Chest", "MQ Jabu-Jabu Third Room East Chest"),
						M("Grass.png", 743, 403, 24, "OOT MQ Jabu-Jabu Grass Main Room Bottom 1", "MQ Jabu-Jabu Grass Main Room Bottom 1"),
						M("Grass.png", 677, 378, 24, "OOT MQ Jabu-Jabu Grass Main Room Bottom 2", "MQ Jabu-Jabu Grass Main Room Bottom 2"),
						M("Grass.png", 304, 584, 24, "OOT MQ Jabu-Jabu Grass Main Room Bottom 3", "MQ Jabu-Jabu Grass Main Room Bottom 3"),
						M("Wonder.png", 265, 228, 24, "OOT MQ Jabu-Jabu Wonder Item Basement Left Cow 1", "MQ Jabu-Jabu Wonder Item Basement Left Cow 1"),
						M("Wonder.png", 295, 228, 24, "OOT MQ Jabu-Jabu Wonder Item Basement Left Cow 2", "MQ Jabu-Jabu Wonder Item Basement Left Cow 2"),
						M("Wonder.png", 280, 243, 24, "OOT MQ Jabu-Jabu Wonder Item Basement Left Cow 3", "MQ Jabu-Jabu Wonder Item Basement Left Cow 3"),
						M("Wonder.png", 412, 213, 24, "OOT MQ Jabu-Jabu Wonder Item Basement Right Cow 1", "MQ Jabu-Jabu Wonder Item Basement Right Cow 1"),
						M("Wonder.png", 427, 228, 24, "OOT MQ Jabu-Jabu Wonder Item Basement Right Cow 2", "MQ Jabu-Jabu Wonder Item Basement Right Cow 2"),
						M("Wonder.png", 442, 213, 24, "OOT MQ Jabu-Jabu Wonder Item Basement Right Cow 3", "MQ Jabu-Jabu Wonder Item Basement Right Cow 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Muscle Block Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Jabu-Jabu/Muscle_Block.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Pot.png", 296, 418, 24, "OOT Jabu-Jabu Pot Muscle Block Room 1", "Jabu-Jabu Pot Muscle Block Room 1"),
                        M("Pot.png", 322, 410, 24, "OOT Jabu-Jabu Pot Muscle Block Room 2", "Jabu-Jabu Pot Muscle Block Room 2"),
                        M("Pot.png", 288, 390, 24, "OOT Jabu-Jabu Pot Muscle Block Room 3", "Jabu-Jabu Pot Muscle Block Room 3"),
                        M("Pot.png", 304, 446, 24, "OOT Jabu-Jabu Pot Muscle Block Room 4", "Jabu-Jabu Pot Muscle Block Room 4"),
                        M("Pot.png", 270, 426, 24, "OOT Jabu-Jabu Pot Muscle Block Room 5", "Jabu-Jabu Pot Muscle Block Room 5"),
						
						M("Gold_Skulltula.png", 197, 410, 40, "OOT MQ Jabu-Jabu GS Basement Side Room", "MQ Jabu-Jabu GS Basement Side Room")
                    }
                },
                new MapSubRegion
                {
                    Name = "Water Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Jabu-Jabu/Water_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 441, 266, 40, "OOT Jabu-Jabu GS Water Switch", "Jabu-Jabu GS Water Switch"),
                        M("Pot.png", 469, 217, 24, "OOT Jabu-Jabu Pot Alcove 1", "Jabu-Jabu Pot Alcove 1"),
                        M("Pot.png", 450, 232, 24, "OOT Jabu-Jabu Pot Alcove 2", "Jabu-Jabu Pot Alcove 2"),
                        M("Pot.png", 431, 217, 24, "OOT Jabu-Jabu Pot Alcove 3", "Jabu-Jabu Pot Alcove 3"),
						
						M("Gold_Skulltula.png", 533, 364, 40, "OOT MQ Jabu-Jabu GS SoT Block", "MQ Jabu-Jabu GS SoT Block"),
						M("Chest.png", 442, 226, 40, "OOT MQ Jabu-Jabu Boomerang Chest", "MQ Jabu-Jabu Boomerang Chest"),
						M("Chest.png", 577, 431, 40, "OOT MQ Jabu-Jabu SoT Room Lower Chest", "MQ Jabu-Jabu SoT Room Lower Chest"),
						M("Grass.png", 577, 409, 24, "OOT MQ Jabu-Jabu Grass Boomerang Room", "MQ Jabu-Jabu Grass Boomerang Room"),
						M("Pot.png", 431, 217, 24, "OOT MQ Jabu-Jabu Pot Boomerang Room 1", "MQ Jabu-Jabu Pot Boomerang Room 1"),
						M("Pot.png", 469, 217, 24, "OOT MQ Jabu-Jabu Pot Boomerang Room 2", "MQ Jabu-Jabu Pot Boomerang Room 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Main Corridor",
                    BackgroundImage = "region maps/OoT/Dungeons/Jabu-Jabu/Main_Corridor.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 790, 381, 40, "OOT Jabu-Jabu Boomerang Chest", "Jabu-Jabu Boomerang Chest"),
                        M("Chest.png", 328, 160, 40, "OOT Jabu-Jabu Compass Chest", "Jabu-Jabu Compass Chest"),
                        M("Chest.png", 133, 371, 40, "OOT Jabu-Jabu Map Chest", "Jabu-Jabu Map Chest"),
						
						M("Gold_Skulltula.png", 390, 135, 40, "OOT MQ Jabu-Jabu GS Back", "MQ Jabu-Jabu GS Back"),
						M("Chest.png", 603, 114, 40, "OOT MQ Jabu-Jabu Back Chest", "MQ Jabu-Jabu Back Chest"),
						M("Crate.png", 327, 380, 24, "OOT MQ Jabu-Jabu Back Forked Paths Main Room Small Crate 1", "MQ Jabu-Jabu Back Forked Paths Main Room Small Crate 1"),
						M("Crate.png", 364, 400, 24, "OOT MQ Jabu-Jabu Back Forked Paths Main Room Small Crate 2", "MQ Jabu-Jabu Back Forked Paths Main Room Small Crate 2"),
						M("Grass.png", 610, 192, 24, "OOT MQ Jabu-Jabu Grass Torch Room", "MQ Jabu-Jabu Grass Torch Room"),
						M("Pot.png", 577, 122, 24, "OOT MQ Jabu-Jabu Pot Like-Like Room 1", "MQ Jabu-Jabu Pot Like-Like Room 1"),
						M("Pot.png", 644, 122, 24, "OOT MQ Jabu-Jabu Pot Like-Like Room 2", "MQ Jabu-Jabu Pot Like-Like Room 2"),
						M("Wonder.png", 687, 170, 24, "OOT MQ Jabu-Jabu Wonder Item Falling Like-Likes Cow Right 1", "MQ Jabu-Jabu Wonder Item Falling Like-Likes Cow Right 1"),
						M("Wonder.png", 695, 150, 24, "OOT MQ Jabu-Jabu Wonder Item Falling Like-Likes Cow Right 2", "MQ Jabu-Jabu Wonder Item Falling Like-Likes Cow Right 2"),
						M("Wonder.png", 695, 190, 24, "OOT MQ Jabu-Jabu Wonder Item Falling Like-Likes Cow Right 3", "MQ Jabu-Jabu Wonder Item Falling Like-Likes Cow Right 3"),
						M("Wonder.png", 548, 170, 24, "OOT MQ Jabu-Jabu Wonder Item Falling Like-Likes Cow Left 1", "MQ Jabu-Jabu Wonder Item Falling Like-Likes Cow Left 1"),
						M("Wonder.png", 540, 190, 24, "OOT MQ Jabu-Jabu Wonder Item Falling Like-Likes Cow Left 2", "MQ Jabu-Jabu Wonder Item Falling Like-Likes Cow Left 2"),
						M("Wonder.png", 540, 150, 24, "OOT MQ Jabu-Jabu Wonder Item Falling Like-Likes Cow Left 3", "MQ Jabu-Jabu Wonder Item Falling Like-Likes Cow Left 3"),
						M("Wonder.png", 611, 169, 24, "OOT MQ Jabu-Jabu Wonder Item Falling Like-Likes Explosion 1", "MQ Jabu-Jabu Wonder Item Falling Like-Likes Explosion 1"),
						M("Wonder.png", 631, 174, 24, "OOT MQ Jabu-Jabu Wonder Item Falling Like-Likes Explosion 2", "MQ Jabu-Jabu Wonder Item Falling Like-Likes Explosion 2"),
						M("Wonder.png", 591, 174, 24, "OOT MQ Jabu-Jabu Wonder Item Falling Like-Likes Explosion 3", "MQ Jabu-Jabu Wonder Item Falling Like-Likes Explosion 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Mini-Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Jabu-Jabu/Mini-Boss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Pot.png", 571, 160, 24, "OOT Jabu-Jabu Pot Big Octo Room 1", "Jabu-Jabu Pot Big Octo Room 1"),
                        M("Pot.png", 545, 152, 24, "OOT Jabu-Jabu Pot Big Octo Room 2", "Jabu-Jabu Pot Big Octo Room 2"),
                        M("Pot.png", 597, 168, 24, "OOT Jabu-Jabu Pot Big Octo Room 3", "Jabu-Jabu Pot Big Octo Room 3"),
						
						M("Grass.png", 591, 204, 24, "OOT MQ Jabu-Jabu Grass Big Octo Top 1", "MQ Jabu-Jabu Grass Big Octo Top 1"),
						M("Grass.png", 571, 160, 24, "OOT MQ Jabu-Jabu Grass Big Octo Top 2", "MQ Jabu-Jabu Grass Big Octo Top 2"),
						M("Wonder.png", 426, 131, 24, "OOT MQ Jabu-Jabu Wonder Item After Big Octo", "MQ Jabu-Jabu Wonder Item After Big Octo")
                    }
                },
				new MapSubRegion
                {
                    Name = "After Mini-Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Jabu-Jabu/After_MiniBoss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Cow.png", 504, 234, 40, "OOT MQ Jabu-Jabu Cow", "MQ Jabu-Jabu Cow"),
						M("Crate.png", 540, 498, 24, "OOT MQ Jabu-Jabu Room After Above Big Octo Small Crate 1", "MQ Jabu-Jabu Room After Above Big Octo Small Crate 1"),
						M("Crate.png", 572, 498, 24, "OOT MQ Jabu-Jabu Room After Above Big Octo Small Crate 2", "MQ Jabu-Jabu Room After Above Big Octo Small Crate 2"),
						M("Grass.png", 422, 200, 24, "OOT MQ Jabu-Jabu Grass Room After Big Octo", "MQ Jabu-Jabu Grass Room After Big Octo"),
						M("Wonder.png", 545, 48, 24, "OOT MQ Jabu-Jabu Wonder Item Platforms Cow", "MQ Jabu-Jabu Wonder Item Platforms Cow")
                    }
                },
                new MapSubRegion
                {
                    Name = "Pre-Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Jabu-Jabu/Pre-Boss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
					{
						M("Gold_Skulltula.png", 323, 369, 40, "OOT Jabu-Jabu GS Near Boss", "Jabu-Jabu GS Near Boss"),
						
						M("Gold_Skulltula.png", 534, 421, 40, "OOT MQ Jabu-Jabu GS Pre-Boss", "MQ Jabu-Jabu GS Pre-Boss"),
						M("Chest.png", 676, 397, 40, "OOT MQ Jabu-Jabu Pre-Boss Chest", "MQ Jabu-Jabu Pre-Boss Chest"),
						M("Grass.png", 383, 376, 24, "OOT MQ Jabu-Jabu Grass Before Boss 1", "MQ Jabu-Jabu Grass Before Boss 1"),
						M("Grass.png", 348, 488, 24, "OOT MQ Jabu-Jabu Grass Before Boss 2", "MQ Jabu-Jabu Grass Before Boss 2"),
						M("Pot.png", 718, 429, 24, "OOT MQ Jabu-Jabu Pot Before Boss", "MQ Jabu-Jabu Pot Before Boss"),
						M("Wonder.png", 351, 344, 24, "OOT MQ Jabu-Jabu Wonder Item Before Boss Left Cow", "MQ Jabu-Jabu Wonder Item Before Boss Left Cow"),
						M("Wonder.png", 358, 238, 24, "OOT MQ Jabu-Jabu Wonder Item Before Boss Right Cow 1", "MQ Jabu-Jabu Wonder Item Before Boss Right Cow 1"),
						M("Wonder.png", 339, 248, 24, "OOT MQ Jabu-Jabu Wonder Item Before Boss Right Cow 2", "MQ Jabu-Jabu Wonder Item Before Boss Right Cow 2"),
						
						ME("Entrance.png", 750, 438, "Entrance shuffle (Barinade)", "OOT_BOSS_JABU_JABU")
					}
                },
                new MapSubRegion
                {
                    Name = "Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Jabu-Jabu/Boss_Room.png",
                    DestinationEntranceIds = new List<string> { "OOT_BOSS_JABU_JABU" },
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 418, 258, 40, "OOT Jabu-Jabu Boss Container", "Jabu-Jabu Boss Container"),
                        M("NPC.png", 457, 292, 40, "OOT Jabu-Jabu Boss", "Jabu-Jabu Boss"),
                        M("Pot.png", 155, 303, 24, "OOT Jabu-Jabu Boss Pot 1", "Jabu-Jabu Boss Pot 1"),
                        M("Pot.png", 234, 471, 24, "OOT Jabu-Jabu Boss Pot 2", "Jabu-Jabu Boss Pot 2"),
                        M("Pot.png", 499, 519, 24, "OOT Jabu-Jabu Boss Pot 3", "Jabu-Jabu Boss Pot 3"),
                        M("Pot.png", 243, 216, 24, "OOT Jabu-Jabu Boss Pot 4", "Jabu-Jabu Boss Pot 4"),
                        M("Pot.png", 432, 168, 24, "OOT Jabu-Jabu Boss Pot 5", "Jabu-Jabu Boss Pot 5"),
                        M("Pot.png", 764, 390, 24, "OOT Jabu-Jabu Boss Pot 6", "Jabu-Jabu Boss Pot 6")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion ForestTemple()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Forest Temple";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Entrance",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Entrance.png",
                    DestinationEntranceIds = new List<string> { "OOT_TEMPLE_FOREST" },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 117, 137, 40, "OOT Forest Temple GS Entrance", "Forest Temple GS Entrance"),
                        M("Chest.png", 679, 225, 40, "OOT Forest Temple Tree Small Key", "Forest Temple Tree Small Key"),
						
						M("Chest.png", 218, 366, 40, "OOT MQ Forest Temple First Room Chest", "MQ Forest Temple First Room Chest"),
						
						ME("Entrance.png", 452, 100, "Entrance shuffle (Sacred Forest Meadow)", "OOT_SACRED_MEADOW_FROM_TEMPLE_FOREST")
                    }
                },
				new MapSubRegion
                {
                    Name = "Entrance Corridor",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Entrance_Corridor.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 568, 39, 40, "OOT MQ Forest Temple GS Entryway", "MQ Forest Temple GS Entryway")
                    }
                },
                new MapSubRegion
                {
                    Name = "Lobby",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Lobby.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 557, 61, 40, "OOT Forest Temple GS Main", "Forest Temple GS Main"),
                        M("Pot.png", 604, 486, 24, "OOT Forest Temple Pot Main Room 1", "Forest Temple Pot Main Room 1"),
                        M("Pot.png", 333, 486, 24, "OOT Forest Temple Pot Main Room 2", "Forest Temple Pot Main Room 2"),
                        M("Pot.png", 628, 462, 24, "OOT Forest Temple Pot Main Room 3", "Forest Temple Pot Main Room 3"),
                        M("Pot.png", 309, 462, 24, "OOT Forest Temple Pot Main Room 4", "Forest Temple Pot Main Room 4"),
                        M("Pot.png", 580, 510, 24, "OOT Forest Temple Pot Main Room 5", "Forest Temple Pot Main Room 5"),
                        M("Pot.png", 357, 510, 24, "OOT Forest Temple Pot Main Room 6", "Forest Temple Pot Main Room 6"),
						
						M("Pot.png", 604, 486, 24, "OOT MQ Forest Temple Pot Main Room 1", "MQ Forest Temple Pot Main Room 1"),
						M("Pot.png", 333, 486, 24, "OOT MQ Forest Temple Pot Main Room 2", "MQ Forest Temple Pot Main Room 2"),
						M("Pot.png", 628, 462, 24, "OOT MQ Forest Temple Pot Main Room 3", "MQ Forest Temple Pot Main Room 3"),
						M("Pot.png", 309, 462, 24, "OOT MQ Forest Temple Pot Main Room 4", "MQ Forest Temple Pot Main Room 4"),
						M("Pot.png", 580, 510, 24, "OOT MQ Forest Temple Pot Main Room 5", "MQ Forest Temple Pot Main Room 5"),
						M("Pot.png", 357, 510, 24, "OOT MQ Forest Temple Pot Main Room 6", "MQ Forest Temple Pot Main Room 6")
                    }
                },
                new MapSubRegion
                {
                    Name = "Stalfos Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Mini-Boss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 450, 494, 40, "OOT Forest Temple Mini-Boss Key", "Forest Temple Mini-Boss Key"),
                        M("Chest.png", 450, 291, 40, "OOT Forest Temple Bow", "Forest Temple Bow"),
                        M("Pot.png", 443, 452, 24, "OOT Forest Temple Pot Miniboss Lower 1", "Forest Temple Pot Miniboss Lower 1"),
                        M("Pot.png", 473, 452, 24, "OOT Forest Temple Pot Miniboss Lower 2", "Forest Temple Pot Miniboss Lower 2"),
                        M("Pot.png", 511, 275, 24, "OOT Forest Temple Pot Miniboss Upper 1", "Forest Temple Pot Miniboss Upper 1"),
                        M("Pot.png", 383, 328, 24, "OOT Forest Temple Pot Miniboss Upper 2", "Forest Temple Pot Miniboss Upper 2"),
                        M("Pot.png", 405, 275, 24, "OOT Forest Temple Pot Miniboss Upper 3", "Forest Temple Pot Miniboss Upper 3"),
                        M("Pot.png", 531, 328, 24, "OOT Forest Temple Pot Miniboss Upper 4", "Forest Temple Pot Miniboss Upper 4"),
						
						M("Chest.png", 450, 494, 40, "OOT MQ Forest Temple Wolfos Chest", "MQ Forest Temple Wolfos Chest"),
						M("Chest.png", 450, 291, 40, "OOT MQ Forest Temple Bow Chest", "MQ Forest Temple Bow Chest"),
						M("Pot.png", 443, 452, 24, "OOT MQ Forest Temple Pot Bow Room Lower 1", "MQ Forest Temple Pot Bow Room Lower 1"),
						M("Pot.png", 473, 452, 24, "OOT MQ Forest Temple Pot Bow Room Lower 2", "MQ Forest Temple Pot Bow Room Lower 2"),
						M("Pot.png", 511, 275, 24, "OOT MQ Forest Temple Pot Bow Room Upper 1", "MQ Forest Temple Pot Bow Room Upper 1"),
						M("Pot.png", 383, 328, 24, "OOT MQ Forest Temple Pot Bow Room Upper 2", "MQ Forest Temple Pot Bow Room Upper 2"),
						M("Pot.png", 405, 275, 24, "OOT MQ Forest Temple Pot Bow Room Upper 3", "MQ Forest Temple Pot Bow Room Upper 3"),
						M("Pot.png", 531, 328, 24, "OOT MQ Forest Temple Pot Bow Room Upper 4", "MQ Forest Temple Pot Bow Room Upper 4")
						
                    }
                },
                new MapSubRegion
                {
                    Name = "Garden West",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Garden_East.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 472, 25, 40, "OOT Forest Temple GS Garden West", "Forest Temple GS Garden West"),
                        M("Heart.png", 478, 147, 24, "OOT Forest Temple Heart Garden 1", "Forest Temple Heart Garden 1"),
                        M("Heart.png", 454, 189, 24, "OOT Forest Temple Heart Garden 2", "Forest Temple Heart Garden 2"),
						
						M("Gold_Skulltula.png", 454, 577, 40, "OOT MQ Forest Temple GS West Garden", "MQ Forest Temple GS West Garden"),
						M("Heart.png", 478, 147, 24, "OOT MQ Forest Temple Heart Garden 1", "MQ Forest Temple Heart Garden 1"),
						M("Heart.png", 466, 168, 24, "OOT MQ Forest Temple Heart Garden 2", "MQ Forest Temple Heart Garden 2"),
						M("Heart.png", 454, 189, 24, "OOT MQ Forest Temple Heart Garden 3", "MQ Forest Temple Heart Garden 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Map Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Map.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark> { M("Chest.png", 455, 410, 40, "OOT Forest Temple Map", "Forest Temple Map") }
                },
                new MapSubRegion
                {
                    Name = "Garden East",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Garden_West.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 473, 203, 40, "OOT Forest Temple GS Garden East", "Forest Temple GS Garden East"),
                        M("Chest.png", 561, 211, 40, "OOT Forest Temple Garden", "Forest Temple Garden"),
						
						M("Gold_Skulltula.png", 457, 460, 40, "OOT MQ Forest Temple GS East Garden", "MQ Forest Temple GS East Garden"),
						M("Chest.png", 466, 225, 40, "OOT MQ Forest Temple East Garden Ledge Chest", "MQ Forest Temple East Garden Ledge Chest"),
						M("Chest.png", 203, 276, 40, "OOT MQ Forest Temple East Garden High Ledge Chest", "MQ Forest Temple East Garden High Ledge Chest")
                    }
                },
                new MapSubRegion
                {
                    Name = "Well",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Underwater.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 826, 102, 40, "OOT Forest Temple Well", "Forest Temple Well"),
                        M("Heart.png", 404, 113, 24, "OOT Forest Temple Heart Well 1", "Forest Temple Heart Well 1"),
                        M("Heart.png", 438, 113, 24, "OOT Forest Temple Heart Well 2", "Forest Temple Heart Well 2"),
						
						M("Gold_Skulltula.png", 876, 124, 40, "OOT MQ Forest Temple GS Well", "MQ Forest Temple GS Well"),
						M("Chest.png", 101, 107, 40, "OOT MQ Forest Temple Well Chest", "MQ Forest Temple Well Chest"),
						M("Heart.png", 498, 113, 24, "OOT MQ Forest Temple Heart Well 1", "MQ Forest Temple Heart Well 1"),
						M("Heart.png", 432, 113, 24, "OOT MQ Forest Temple Heart Well 2", "MQ Forest Temple Heart Well 2"),
						M("Heart.png", 465, 113, 24, "OOT MQ Forest Temple Heart Well 3", "MQ Forest Temple Heart Well 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Blocks Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Blocks_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
					{
						M("Chest.png", 408, 305, 40, "OOT Forest Temple Maze", "Forest Temple Maze"),
						
						M("Gold_Skulltula.png", 228, 540, 40, "OOT MQ Forest Temple GS Climb Room", "MQ Forest Temple GS Climb Room")
					}
                },
                new MapSubRegion
                {
                    Name = "Boss Key Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Boss_Key.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
					{
						M("Chest.png", 530, 482, 40, "OOT Forest Temple Boss Key", "Forest Temple Boss Key"),
						
						M("Chest.png", 530, 482, 40, "OOT MQ Forest Temple Boss Key Chest", "MQ Forest Temple Boss Key Chest")
					}
                },
                new MapSubRegion
                {
                    Name = "Floormaster Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Wallmaster.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
					{
						M("Chest.png", 444, 509, 40, "OOT Forest Temple Floormaster", "Forest Temple Floormaster"),
						
						M("Chest.png", 370, 509, 40, "OOT MQ Forest Temple ReDead Chest", "MQ Forest Temple ReDead Chest")
					}
                },
                new MapSubRegion
                {
                    Name = "Red Poe Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Red_Poe.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
					{
						M("Chest.png", 744, 467, 40, "OOT Forest Temple Poe Key", "Forest Temple Poe Key"),
						
						M("Chest.png", 744, 467, 40, "OOT MQ Forest Temple Map Chest", "MQ Forest Temple Map Chest")
					}
                },
                new MapSubRegion
                {
                    Name = "Blue Poe Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Blue_Poe.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 151, 470, 40, "OOT Forest Temple Compass", "Forest Temple Compass"),
                        M("Pot.png", 324, 454, 24, "OOT Forest Temple Pot Blue Poe 1", "Forest Temple Pot Blue Poe 1"),
                        M("Pot.png", 376, 454, 24, "OOT Forest Temple Pot Blue Poe 2", "Forest Temple Pot Blue Poe 2"),
                        M("Pot.png", 350, 454, 24, "OOT Forest Temple Pot Blue Poe 3", "Forest Temple Pot Blue Poe 3"),
						
						M("Chest.png", 151, 470, 40, "OOT MQ Forest Temple Compass Chest", "MQ Forest Temple Compass Chest"),
						M("Pot.png", 324, 454, 24, "OOT MQ Forest Temple Pot Blue Poe 1", "MQ Forest Temple Pot Blue Poe 1"),
                        M("Pot.png", 376, 454, 24, "OOT MQ Forest Temple Pot Blue Poe 2", "MQ Forest Temple Pot Blue Poe 2"),
                        M("Pot.png", 350, 454, 24, "OOT MQ Forest Temple Pot Blue Poe 3", "MQ Forest Temple Pot Blue Poe 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Spinning Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Spinning_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Pot.png", 726, 578, 24, "OOT Forest Temple Pot Rotating Room 1", "Forest Temple Pot Rotating Room 1"),
                        M("Pot.png", 766, 578, 24, "OOT Forest Temple Pot Rotating Room 2", "Forest Temple Pot Rotating Room 2"),
						
						M("Crate.png", 726, 578, 24, "OOT MQ Forest Temple Spinning Red Pool Room Small Crate 1", "MQ Forest Temple Spinning Red Pool Room Small Crate 1"),
						M("Crate.png", 766, 578, 24, "OOT MQ Forest Temple Spinning Red Pool Room Small Crate 2", "MQ Forest Temple Spinning Red Pool Room Small Crate 2"),
						M("Crate.png", 221, 285, 24, "OOT MQ Forest Temple Spinning Red Pool Room Small Crate 3", "MQ Forest Temple Spinning Red Pool Room Small Crate 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Checkerboard Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Check_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
					{
						M("Chest.png", 249, 424, 40, "OOT Forest Temple Checkerboard", "Forest Temple Checkerboard"),
						
						M("Chest.png", 384, 465, 40, "OOT MQ Forest Temple Falling Ceiling Chest", "MQ Forest Temple Falling Ceiling Chest")
					}
                },
                new MapSubRegion
                {
                    Name = "Green Poe Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Green_Poe.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Pot.png", 518, 284, 24, "OOT Forest Temple Pot Green Poe Pot 1", "Forest Temple Pot Green Poe Pot 1"),
                        M("Pot.png", 544, 284, 24, "OOT Forest Temple Pot Green Poe Pot 2", "Forest Temple Pot Green Poe Pot 2"),
						
						M("Pot.png", 518, 284, 24, "OOT MQ Forest Temple Pot Green Poe 1", "MQ Forest Temple Pot Green Poe 1"),
                        M("Pot.png", 544, 284, 24, "OOT MQ Forest Temple Pot Green Poe 2", "MQ Forest Temple Pot Green Poe 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Pre-Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Pre-Boss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 231, 300, 40, "OOT Forest Temple GS Antichamber", "Forest Temple GS Antichamber"),
                        M("Chest.png", 203, 386, 40, "OOT Forest Temple Antichamber", "Forest Temple Antichamber"),
						
						M("Chest.png", 415, 197, 40, "OOT MQ Forest Temple Antichamber", "MQ Forest Temple Antichamber"),
						M("Pot.png", 192, 361, 24, "OOT MQ Forest Temple Pot Antichamber 1", "MQ Forest Temple Pot Antichamber 1"),
						M("Pot.png", 195, 433, 24, "OOT MQ Forest Temple Pot Antichamber 2", "MQ Forest Temple Pot Antichamber 2"),
						M("Pot.png", 194, 409, 24, "OOT MQ Forest Temple Pot Antichamber 3", "MQ Forest Temple Pot Antichamber 3"),
						M("Pot.png", 193, 385, 24, "OOT MQ Forest Temple Pot Antichamber 4", "MQ Forest Temple Pot Antichamber 4"),
						
						ME("Entrance.png", 629, 120, "Entrance shuffle (Phantom Ganon)", "OOT_BOSS_TEMPLE_FOREST")
                    }
                },
                new MapSubRegion
                {
                    Name = "Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Forest/Boss.png",
                    DestinationEntranceIds = new List<string> { "OOT_BOSS_TEMPLE_FOREST" },
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 471, 278, 40, "OOT Forest Temple Boss Container", "Forest Temple Boss Container"),
                        M("NPC.png", 471, 329, 40, "OOT Forest Temple Boss", "Forest Temple Boss")
                    }
                }
            };
            return mapRegion;
        }

        public static MapRegion FireTemple()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Fire Temple";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Lobby",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Lobby.png",
                    DestinationEntranceIds = new List<string> { "OOT_TEMPLE_FIRE" },
                    Marks = new List<MapMark>
					{
						M("Pot.png", 371, 488, 24, "OOT MQ Fire Temple Pot Entrance 1", "MQ Fire Temple Pot Entrance 1"),
						M("Pot.png", 551, 488, 24, "OOT MQ Fire Temple Pot Entrance 2", "MQ Fire Temple Pot Entrance 2"),
						
						ME("Entrance.png", 454, 535, "Entrance shuffle (Death Mountain Crater)", "OOT_DEATH_CRATER_FROM_TEMPLE_FIRE")
					}
                },
                new MapSubRegion
                {
                    Name = "Darunia Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Darunia.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 122, 335, 40, "OOT Fire Temple Jail 1 Chest", "Fire Temple Jail 1 Chest"),
                        M("Pot.png", 739, 173, 24, "OOT Fire Temple Pot Pre-Boss Room 1", "Fire Temple Pot Pre-Boss Room 1"),
                        M("Pot.png", 717, 185, 24, "OOT Fire Temple Pot Pre-Boss Room 2", "Fire Temple Pot Pre-Boss Room 2"),
                        M("Pot.png", 761, 185, 24, "OOT Fire Temple Pot Pre-Boss Room 3", "Fire Temple Pot Pre-Boss Room 3"),
                        M("Pot.png", 739, 197, 24, "OOT Fire Temple Pot Pre-Boss Room 4", "Fire Temple Pot Pre-Boss Room 4"),
						
						M("Chest.png", 122, 335, 40, "OOT MQ Fire Temple Pre-Boss Chest", "MQ Fire Temple Pre-Boss Chest"),
						M("Pot.png", 665, 206, 24, "OOT MQ Fire Temple Pot Pre-Boss 1", "MQ Fire Temple Pot Pre-Boss 1"),
						M("Pot.png", 643, 212, 24, "OOT MQ Fire Temple Pot Pre-Boss 2", "MQ Fire Temple Pot Pre-Boss 2"),
						M("Crate.png", 242, 339, 24, "OOT MQ Fire Temple Pre-Boss Room Near Cage Large Crate 1", "MQ Fire Temple Pre-Boss Room Near Cage Large Crate 1"),
						M("Crate.png", 218, 345, 24, "OOT MQ Fire Temple Pre-Boss Room Near Cage Large Crate 2", "MQ Fire Temple Pre-Boss Room Near Cage Large Crate 2"),
						M("Crate.png", 599, 222, 24, "OOT MQ Fire Temple Pre-Boss Room Tall Ledge Low Large Crate 1", "MQ Fire Temple Pre-Boss Room Tall Ledge Low Large Crate 1"),
						M("Crate.png", 575, 228, 24, "OOT MQ Fire Temple Pre-Boss Room Tall Ledge Low Large Crate 2", "MQ Fire Temple Pre-Boss Room Tall Ledge Low Large Crate 2"),
						M("Crate.png", 709, 213, 24, "OOT MQ Fire Temple Pre-Boss Room Tall Ledge Middle Large Crate ", "MQ Fire Temple Pre-Boss Room Tall Ledge Middle Large Crate"),
						M("Crate.png", 709, 189, 24, "OOT MQ Fire Temple Pre-Boss Room Tall Ledge Top Large Crate ", "MQ Fire Temple Pre-Boss Room Tall Ledge Top Large Crate"),
						
						ME("Entrance.png", 440, 208, "Entrance shuffle (Volvagia)", "OOT_BOSS_TEMPLE_FIRE")
                    }
                },
                new MapSubRegion
                {
                    Name = "Lava Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Lava_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 41, 327, 40, "OOT Fire Temple Lava Room North Jail Chest", "Fire Temple Lava Room North Jail Chest"),
                        M("Chest.png", 884, 248, 40, "OOT Fire Temple Lava Room South Jail Chest", "Fire Temple Lava Room South Jail Chest"),
                        M("Pot.png", 265, 231, 24, "OOT Fire Temple Pot Lava Room 1", "Fire Temple Pot Lava Room 1"),
                        M("Pot.png", 287, 229, 24, "OOT Fire Temple Pot Lava Room 2", "Fire Temple Pot Lava Room 2"),
                        M("Pot.png", 309, 227, 24, "OOT Fire Temple Pot Lava Room 3", "Fire Temple Pot Lava Room 3"),
						
						M("Gold_Skulltula.png", 43, 317, 40, "OOT MQ Fire Temple GS 1f Lava Room", "MQ Fire Temple GS 1f Lava Room"),
						M("Chest.png", 884, 248, 40, "OOT MQ Fire Temple 1f Lava Room Goron Chest", "MQ Fire Temple 1f Lava Room Goron Chest"),
						M("Pot.png", 193, 356, 24, "OOT MQ Fire Temple Pot Lava Room Left", "MQ Fire Temple Pot Lava Room Left"),
						M("Pot.png", 768, 267, 24, "OOT MQ Fire Temple Pot Lava Room Right", "MQ Fire Temple Pot Lava Room Right"),
						M("Pot.png", 287, 229, 24, "OOT MQ Fire Temple Pot Lava Room Alcove", "MQ Fire Temple Pot Lava Room Alcove")
                    }
                },
                new MapSubRegion
                {
                    Name = "Lava Side Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Lava_Side.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
					{
						M("Gold_Skulltula.png", 451, 170, 40, "OOT Fire Temple GS Lava Side Room", "Fire Temple GS Lava Side Room"),
						
						M("Chest.png", 513, 212, 40, "OOT MQ Fire Temple Boss Key Chest", "MQ Fire Temple Boss Key Chest"),
						M("Pot.png", 362, 213, 24, "OOT MQ Fire Temple Pot Boss Key Room 1", "MQ Fire Temple Pot Boss Key Room 1"),
						M("Pot.png", 350, 237, 24, "OOT MQ Fire Temple Pot Boss Key Room 2", "MQ Fire Temple Pot Boss Key Room 2"),
						M("Wonder.png", 449, 171, 24, "OOT MQ Fire Temple Wonder Item Boss Key Room Hookshot", "MQ Fire Temple Wonder Item Boss Key Room Hookshot"),
						M("Wonder.png", 469, 171, 24, "OOT MQ Fire Temple Wonder Item Boss Key Room Bow", "MQ Fire Temple Wonder Item Boss Key Room Bow")
					}
                },
                new MapSubRegion
                {
                    Name = "Fire Column Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Fire_Column.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Heart.png", 293, 180, 24, "OOT Fire Temple Heart Elevator 1", "Fire Temple Heart Elevator 1"),
                        M("Heart.png", 324, 180, 24, "OOT Fire Temple Heart Elevator 2", "Fire Temple Heart Elevator 2"),
                        M("Heart.png", 321, 154, 24, "OOT Fire Temple Heart Elevator 3", "Fire Temple Heart Elevator 3"),
						
						M("Heart.png", 315, 170, 24, "OOT MQ Fire Temple Heart 1", "MQ Fire Temple Heart 1"),
						M("Heart.png", 565, 284, 24, "OOT MQ Fire Temple Heart 2", "MQ Fire Temple Heart 2"),
						M("Heart.png", 635, 170, 24, "OOT MQ Fire Temple Heart 3", "MQ Fire Temple Heart 3")
                    }
                },
                new MapSubRegion
                {
                    Name = "Boulder Maze",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Boulder_Maze.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 704, 305, 40, "OOT Fire Temple GS Maze", "Fire Temple GS Maze"),
                        M("Chest.png", 35, 231, 40, "OOT Fire Temple Above Maze Chest", "Fire Temple Above Maze Chest"),
                        M("Chest.png", 329, 489, 40, "OOT Fire Temple Below Maze Chest", "Fire Temple Below Maze Chest"),
                        M("Chest.png", 67, 152, 40, "OOT Fire Temple Maze Chest", "Fire Temple Maze Chest"),
                        M("Chest.png", 863, 222, 40, "OOT Fire Temple Maze Jail Chest", "Fire Temple Maze Jail Chest"),
						
						M("Chest.png", 329, 489, 40, "OOT MQ Fire Temple Compass Chest", "MQ Fire Temple Compass Chest"),
						M("Chest.png", 35, 231, 40, "OOT MQ Fire Temple Maze Lower Chest", "MQ Fire Temple Maze Lower Chest"),
						M("Chest.png", 67, 152, 40, "OOT MQ Fire Temple Maze Upper Chest", "MQ Fire Temple Maze Upper Chest"),
						M("Chest.png", 863, 222, 40, "OOT MQ Fire Temple Maze Side Room Chest", "MQ Fire Temple Maze Side Room Chest"),
						M("Crate.png", 353, 415, 24, "OOT MQ Fire Temple Cell Below Maze Large Crate 1", "MQ Fire Temple Cell Below Maze Large Crate 1"),
						M("Crate.png", 345, 431, 24, "OOT MQ Fire Temple Cell Below Maze Large Crate 2", "MQ Fire Temple Cell Below Maze Large Crate 2"),
						M("Crate.png", 372, 439, 24, "OOT MQ Fire Temple Cell Below Maze Large Crate 3", "MQ Fire Temple Cell Below Maze Large Crate 3"),
						M("Crate.png", 337, 447, 24, "OOT MQ Fire Temple Cell Below Maze Large Crate 4", "MQ Fire Temple Cell Below Maze Large Crate 4"),
						M("Crate.png", 364, 455, 24, "OOT MQ Fire Temple Cell Below Maze Large Crate 5", "MQ Fire Temple Cell Below Maze Large Crate 5"),
						M("Crate.png", 356, 471, 24, "OOT MQ Fire Temple Cell Below Maze Large Crate 6", "MQ Fire Temple Cell Below Maze Large Crate 6"),
						M("Crate.png", 104, 131, 24, "OOT MQ Fire Temple Maze Room Upper Cage Small Crate 1", "MQ Fire Temple Maze Room Upper Cage Small Crate 1"),
						M("Crate.png", 128, 137, 24, "OOT MQ Fire Temple Maze Room Upper Cage Small Crate 2", "MQ Fire Temple Maze Room Upper Cage Small Crate 2"),
						M("Crate.png", 104, 155, 24, "OOT MQ Fire Temple Maze Room Upper Cage Large Crate 1", "MQ Fire Temple Maze Room Upper Cage Large Crate 1"),
						M("Crate.png", 128, 161, 24, "OOT MQ Fire Temple Maze Room Upper Cage Large Crate 2", "MQ Fire Temple Maze Room Upper Cage Large Crate 2"),
						M("Crate.png", 43, 165, 24, "OOT MQ Fire Temple Maze Room Upper Cage Large Crate 3", "MQ Fire Temple Maze Room Upper Cage Large Crate 3"),
						M("Crate.png", 78, 237, 24, "OOT MQ Fire Temple Maze Room Lower Cage Large Crate 1", "MQ Fire Temple Maze Room Lower Cage Large Crate 1"),
						M("Crate.png", 48, 267, 24, "OOT MQ Fire Temple Maze Room Lower Cage Large Crate 2", "MQ Fire Temple Maze Room Lower Cage Large Crate 2"),
						M("Crate.png", 78, 267, 24, "OOT MQ Fire Temple Maze Room Lower Cage Large Crate 3", "MQ Fire Temple Maze Room Lower Cage Large Crate 3"),
						M("Wonder.png", 243, 490, 24, "OOT MQ Fire Temple Wonder Item Shortcut Room 1", "MQ Fire Temple Wonder Item Shortcut Room 1"),
						M("Wonder.png", 223, 500, 24, "OOT MQ Fire Temple Wonder Item Shortcut Room 2", "MQ Fire Temple Wonder Item Shortcut Room 2"),
						M("Wonder.png", 223, 480, 24, "OOT MQ Fire Temple Wonder Item Shortcut Room 3", "MQ Fire Temple Wonder Item Shortcut Room 3"),
						M("Wonder.png", 252, 343, 24, "OOT MQ Fire Temple Wonder Item East Maze", "MQ Fire Temple Wonder Item East Maze")
                    }
                },
                new MapSubRegion
                {
                    Name = "Above Lava Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Above_Lava.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 377, 168, 40, "OOT Fire Temple GS Scarecrow Top", "Fire Temple GS Scarecrow Top"),
                        M("Gold_Skulltula.png", 232, 208, 40, "OOT Fire Temple GS Scarecrow Wall", "Fire Temple GS Scarecrow Wall"),
                        M("Chest.png", 334, 102, 40, "OOT Fire Temple Scarecrow Chest", "Fire Temple Scarecrow Chest"),
                        M("Heart.png", 449, 507, 24, "OOT Fire Temple Heart Ledge Above Main 1", "Fire Temple Heart Ledge Above Main 1"),
                        M("Heart.png", 427, 518, 24, "OOT Fire Temple Heart Ledge Above Main 2", "Fire Temple Heart Ledge Above Main 2"),
                        M("Heart.png", 409, 534, 24, "OOT Fire Temple Heart Ledge Above Main 3", "Fire Temple Heart Ledge Above Main 3"),
						
						M("Gold_Skulltula.png", 334, 102, 40, "OOT MQ Fire Temple GS Burning Block", "MQ Fire Temple GS Burning Block"),
						M("Pot.png", 409, 534, 24, "OOT MQ Fire Temple Pot Bridge Above Lava Room 1", "MQ Fire Temple Pot Bridge Above Lava Room 1"),
						M("Pot.png", 449, 507, 24, "OOT MQ Fire Temple Pot Bridge Above Lava Room 2", "MQ Fire Temple Pot Bridge Above Lava Room 2"),
						M("Pot.png", 427, 518, 24, "OOT MQ Fire Temple Pot Bridge Above Lava Room 3", "MQ Fire Temple Pot Bridge Above Lava Room 3"),
						M("Wonder.png", 356, 302, 24, "OOT MQ Fire Temple Wonder Item East Climb First 1", "MQ Fire Temple Wonder Item East Climb First 1"),
						M("Wonder.png", 356, 282, 24, "OOT MQ Fire Temple Wonder Item East Climb First 2", "MQ Fire Temple Wonder Item East Climb First 2"),
						M("Wonder.png", 251, 301, 24, "OOT MQ Fire Temple Wonder Item East Climb Second 1", "MQ Fire Temple Wonder Item East Climb Second 1"),
						M("Wonder.png", 271, 301, 24, "OOT MQ Fire Temple Wonder Item East Climb Second 2", "MQ Fire Temple Wonder Item East Climb Second 2")
                    }
                },
                new MapSubRegion
                {
                    Name = "Fire Wall Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Fire_Wall.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 369, 279, 40, "OOT Fire Temple Map", "Fire Temple Map"),
                        M("Heart.png", 373, 432, 24, "OOT Fire Temple Heart Map Room 1", "Fire Temple Heart Map Room 1"),
                        M("Heart.png", 389, 351, 24, "OOT Fire Temple Heart Map Room 2", "Fire Temple Heart Map Room 2"),
                        M("Heart.png", 110, 263, 24, "OOT Fire Temple Heart Map Room 3", "Fire Temple Heart Map Room 3"),
						
						M("Pot.png", 577, 232, 24, "OOT MQ Fire Temple Pot Grids Above Lava 1", "MQ Fire Temple Pot Grids Above Lava 1"),
						M("Pot.png", 599, 240, 24, "OOT MQ Fire Temple Pot Grids Above Lava 2", "MQ Fire Temple Pot Grids Above Lava 2"),
						M("Crate.png", 515, 263, 24, "OOT MQ Fire Temple 3F Lava Room Near Cage Large Crate", "MQ Fire Temple 3F Lava Room Near Cage Large Crate"),
						M("Crate.png", 539, 253, 24, "OOT MQ Fire Temple 3F Lava Room Near Cage Small Crate 1", "MQ Fire Temple 3F Lava Room Near Cage Small Crate 1"),
						M("Crate.png", 523, 287, 24, "OOT MQ Fire Temple 3F Lava Room Near Cage Small Crate 2", "MQ Fire Temple 3F Lava Room Near Cage Small Crate 2"),
						M("Crate.png", 624, 511, 24, "OOT MQ Fire Temple 3F Lava Room High Ledge Large Crate", "MQ Fire Temple 3F Lava Room High Ledge Large Crate"),
						M("Crate.png", 624, 479, 24, "OOT MQ Fire Temple 3F Lava Room High Ledge Small Crate", "MQ Fire Temple 3F Lava Room High Ledge Small Crate"),
						M("Crate.png", 321, 359, 24, "OOT MQ Fire Temple 3F Lava Room Lower Large Crate 1", "MQ Fire Temple 3F Lava Room Lower Large Crate 1"),
						M("Crate.png", 96, 261, 24, "OOT MQ Fire Temple 3F Lava Room Lower Large Crate 2", "MQ Fire Temple 3F Lava Room Lower Large Crate 2"),
						M("Crate.png", 216, 251, 24, "OOT MQ Fire Temple 3F Lava Room Lower Large Crate 3", "MQ Fire Temple 3F Lava Room Lower Large Crate 3"),
						M("Crate.png", 527, 444, 24, "OOT MQ Fire Temple 3F Lava Room Lower Small Crate 1", "MQ Fire Temple 3F Lava Room Lower Small Crate 1"),
						M("Crate.png", 154, 261, 24, "OOT MQ Fire Temple 3F Lava Room Lower Small Crate 2", "MQ Fire Temple 3F Lava Room Lower Small Crate 2"),
						M("Wonder.png", 565, 463, 24, "OOT MQ Fire Temple Wonder Item Torch Room", "MQ Fire Temple Wonder Item Torch Room")
                    }
                },
                new MapSubRegion
                {
                    Name = "Fire Maze",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Fire_Maze.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 36, 379, 40, "OOT Fire Temple Compass", "Fire Temple Compass"),
                        M("Pot.png", 760, 472, 24, "OOT Fire Temple Pot Before Miniboss 1", "Fire Temple Pot Before Miniboss 1"),
                        M("Pot.png", 767, 444, 24, "OOT Fire Temple Pot Before Miniboss 2", "Fire Temple Pot Before Miniboss 2"),
                        M("Pot.png", 659, 330, 24, "OOT Fire Temple Pot Before Miniboss 3", "Fire Temple Pot Before Miniboss 3"),
                        M("Pot.png", 633, 321, 24, "OOT Fire Temple Pot Before Miniboss 4", "Fire Temple Pot Before Miniboss 4"),
                        M("Pot.png", 752, 233, 24, "OOT Fire Temple Pot Ring 1", "Fire Temple Pot Ring 1"),
                        M("Pot.png", 725, 238, 24, "OOT Fire Temple Pot Ring 2", "Fire Temple Pot Ring 2"),
                        M("Pot.png", 761, 209, 24, "OOT Fire Temple Pot Ring 3", "Fire Temple Pot Ring 3"),
                        M("Pot.png", 739, 194, 24, "OOT Fire Temple Pot Ring 4", "Fire Temple Pot Ring 4"),
						
						M("Gold_Skulltula.png", 4, 374, 40, "OOT MQ Fire Temple GS Fire Walls Side Room", "MQ Fire Temple GS Fire Walls Side Room"),
						M("Gold_Skulltula.png", 362, 14, 40, "OOT MQ Fire Temple GS Topmost", "MQ Fire Temple GS Topmost"),
						M("Pot.png", 758, 221, 24, "OOT MQ Fire Temple Pot Fire Maze Room Left 1", "MQ Fire Temple Pot Fire Maze Room Left 1"),
						M("Pot.png", 736, 225, 24, "OOT MQ Fire Temple Pot Fire Maze Room Left 2", "MQ Fire Temple Pot Fire Maze Room Left 2"),
						M("Pot.png", 367, 482, 24, "OOT MQ Fire Temple Pot Fire Maze Room Right 1", "MQ Fire Temple Pot Fire Maze Room Right 1"),
						M("Pot.png", 561, 398, 24, "OOT MQ Fire Temple Pot Fire Maze Room Right 2", "MQ Fire Temple Pot Fire Maze Room Right 2"),
						M("Pot.png", 646, 325, 24, "OOT MQ Fire Temple Pot Fire Maze Room Back Right 1", "MQ Fire Temple Pot Fire Maze Room Back Right 1"),
						M("Pot.png", 763, 458, 24, "OOT MQ Fire Temple Pot Fire Maze Room Back Right 2", "MQ Fire Temple Pot Fire Maze Room Back Right 2"),
						M("Wonder.png", 741, 316, 24, "OOT MQ Fire Temple Wonder Item Fire Maze", "MQ Fire Temple Wonder Item Fire Maze")
                    }
                },
				new MapSubRegion
                {
                    Name = "Flare Dancer Rooms",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Flare.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 771, 576, 40, "OOT MQ Fire Temple Wonder Item After Flare Dancer", "MQ Fire Temple Wonder Item After Flare Dancer")
                    }
                },
				new MapSubRegion
                {
                    Name = "Climb After Flare Dancer Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Climb.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Wonder.png", 1020, 539, 24, "OOT MQ Fire Temple Wonder Item After Flare Dancer", "MQ Fire Temple Wonder Item After Flare Dancer")
                    }
                },
                new MapSubRegion
                {
                    Name = "Hammer Rooms",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Hammer.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Chest.png", 624, 114, 40, "OOT Fire Temple Hammer", "Fire Temple Hammer"),
                        M("Chest.png", 428, 524, 40, "OOT Fire Temple Ring Jail", "Fire Temple Ring Jail"),
                        M("Crate.png", 214, 424, 24, "OOT Fire Temple Crate 1", "Fire Temple Crate 1"),
                        M("Crate.png", 190, 424, 24, "OOT Fire Temple Crate 2", "Fire Temple Crate 2"),
						
						M("Gold_Skulltula.png", 428, 524, 40, "OOT MQ Fire Temple GS Fire Walls Middle", "MQ Fire Temple GS Fire Walls Middle"),
						M("Chest.png", 624, 114, 40, "OOT MQ Fire Temple Topmost Chest", "MQ Fire Temple Topmost Chest"),
						M("Wonder.png", 316, 319, 24, "OOT MQ Fire Temple Wonder Item Staircase", "MQ Fire Temple Wonder Item Staircase")
                    }
                },
				new MapSubRegion
                {
                    Name = "Boss Key Rooms",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Boss_Key.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
                        M("Gold_Skulltula.png", 802, 481, 40, "OOT Fire Temple GS Hammer Statues", "Fire Temple GS Hammer Statues"),
                        M("Chest.png", 259, 215, 40, "OOT Fire Temple Boss Key Chest", "Fire Temple Boss Key Chest"),
                        M("Chest.png", 668, 160, 40, "OOT Fire Temple Boss Key Side Chest", "Fire Temple Boss Key Side Chest"),
						
						M("Chest.png", 668, 160, 40, "OOT MQ Fire Temple Hammer Chest", "MQ Fire Temple Hammer Chest"),
						M("Chest.png", 259, 215, 40, "OOT MQ Fire Temple Map Chest", "MQ Fire Temple Map Chest"),
						M("Chest.png", 196, 215, 40, "OOT MQ Fire Temple Early Lower Left Chest", "MQ Fire Temple Early Lower Left Chest"),
						M("Pot.png", 739, 420, 24, "OOT MQ Fire Temple Pot Hammer Loop 1", "MQ Fire Temple Pot Hammer Loop 1"),
						M("Pot.png", 769, 420, 24, "OOT MQ Fire Temple Pot Hammer Loop 2", "MQ Fire Temple Pot Hammer Loop 2"),
						M("Pot.png", 739, 550, 24, "OOT MQ Fire Temple Pot Hammer Loop 3", "MQ Fire Temple Pot Hammer Loop 3"),
						M("Pot.png", 769, 550, 24, "OOT MQ Fire Temple Pot Hammer Loop 4", "MQ Fire Temple Pot Hammer Loop 4"),
						M("Pot.png", 621, 350, 24, "OOT MQ Fire Temple Pot Hammer Loop 5", "MQ Fire Temple Pot Hammer Loop 5"),
						M("Pot.png", 671, 350, 24, "OOT MQ Fire Temple Pot Hammer Loop 6", "MQ Fire Temple Pot Hammer Loop 6"),
						M("Pot.png", 517, 559, 24, "OOT MQ Fire Temple Pot Hammer Loop 7", "MQ Fire Temple Pot Hammer Loop 7"),
						M("Pot.png", 517, 350, 24, "OOT MQ Fire Temple Pot Hammer Loop 8", "MQ Fire Temple Pot Hammer Loop 8"),
						M("Fairy_Spot.png", 331, 364, 40, "OOT MQ Fire Temple Hammer Loop Stalfos Big Fairy", "MQ Fire Temple Hammer Loop Stalfos Big Fairy"),
						M("Fairy_Spot.png", 786, 480, 40, "OOT MQ Fire Temple Hammer Loop Iron Knuckle Big Fairy", "MQ Fire Temple Hammer Loop Iron Knuckle Big Fairy")
                    }
                },
                new MapSubRegion
                {
                    Name = "Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Fire/Boss.png",
                    DestinationEntranceIds = new List<string> { "OOT_BOSS_TEMPLE_FIRE" },
                    Marks = new List<MapMark>
                    {
                        M("Collectible.png", 472, 255, 40, "OOT Fire Temple Boss Container", "Fire Temple Boss Container"),
                        M("NPC.png", 472, 295, 40, "OOT Fire Temple Boss", "Fire Temple Boss")
                    }
                }
            };
            return mapRegion;
        }
        public static MapRegion WaterTemple()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Water Temple";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
                new MapSubRegion
                {
                    Name = "Lobby",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/Lobby.png",
                    DestinationEntranceIds = new List<string> { "OOT_TEMPLE_WATER" },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 29, 240, 40, "OOT Water Temple GS Cage", "Water Temple GS Cage"),
						M("Chest.png", 570, 519, 40, "OOT Water Temple Corridor Chest", "Water Temple Corridor Chest"),
						M("Pot.png", 650, 331, 24, "OOT Water Temple Pot Main Room Near Block 1", "Water Temple Pot Main Room Near Block 1"),
						M("Pot.png", 666, 316, 24, "OOT Water Temple Pot Main Room Near Block 2", "Water Temple Pot Main Room Near Block 2"),
						M("Pot.png", 680, 155, 24, "OOT Water Temple Pot Main Room Near Boss 1", "Water Temple Pot Main Room Near Boss 1"),
						M("Pot.png", 646, 148, 24, "OOT Water Temple Pot Main Room Near Boss 2", "Water Temple Pot Main Room Near Boss 2"),
						M("Pot.png", 563, 569, 24, "OOT Water Temple Pot Corridor 1", "Water Temple Pot Corridor 1"),
						M("Pot.png", 533, 551, 24, "OOT Water Temple Pot Corridor 2", "Water Temple Pot Corridor 2"),
						M("Pot.png", 89, 255, 24, "OOT Water Temple Pot Skull Cage 1", "Water Temple Pot Skull Cage 1"),
						M("Pot.png", 67, 262, 24, "OOT Water Temple Pot Skull Cage 2", "Water Temple Pot Skull Cage 2"),
						M("Pot.png", 23, 276, 24, "OOT Water Temple Pot Skull Cage 3", "Water Temple Pot Skull Cage 3"),
						M("Pot.png", 45, 269, 24, "OOT Water Temple Pot Skull Cage 4", "Water Temple Pot Skull Cage 4"),
						
						M("Gold_Skulltula.png", 36, 223, 40, "OOT MQ Water Temple GS Three Torch", "MQ Water Temple GS Three Torch"),
						M("Gold_Skulltula.png", 717, 481, 40, "OOT MQ Water Temple GS Lizalfos Hallway", "MQ Water Temple GS Lizalfos Hallway"),
						M("Pot.png", 89, 255, 24, "OOT MQ Water Temple Pot Skull Cage 1", "MQ Water Temple Pot Skull Cage 1"),
						M("Pot.png", 67, 262, 24, "OOT MQ Water Temple Pot Skull Cage 2", "MQ Water Temple Pot Skull Cage 2"),
						M("Pot.png", 23, 276, 24, "OOT MQ Water Temple Pot Skull Cage 3", "MQ Water Temple Pot Skull Cage 3"),
						M("Pot.png", 45, 269, 24, "OOT MQ Water Temple Pot Skull Cage 4", "MQ Water Temple Pot Skull Cage 4"),
						M("Pot.png", 598, 442, 24, "OOT MQ Water Temple Pot Twisted Room Entrance", "MQ Water Temple Pot Twisted Room Entrance"),
						M("Pot.png", 529, 548, 24, "OOT MQ Water Temple Pot Twisted Room Right 1", "MQ Water Temple Pot Twisted Room Right 1"),
						M("Pot.png", 617, 565, 24, "OOT MQ Water Temple Pot Twisted Room Right 2", "MQ Water Temple Pot Twisted Room Right 2"),
						M("Pot.png", 673, 485, 24, "OOT MQ Water Temple Pot Twisted Room Cage 1", "MQ Water Temple Pot Twisted Room Cage 1"),
						M("Pot.png", 685, 465, 24, "OOT MQ Water Temple Pot Twisted Room Cage 2", "MQ Water Temple Pot Twisted Room Cage 2"),
						M("Crate.png", 154, 361, 24, "OOT MQ Water Temple Three Torch Room Large Crate 1", "MQ Water Temple Three Torch Room Large Crate 1"),
						M("Crate.png", 114, 383, 24, "OOT MQ Water Temple Three Torch Room Large Crate 2", "MQ Water Temple Three Torch Room Large Crate 2"),
						M("Crate.png", 134, 372, 24, "OOT MQ Water Temple Three Torch Room Large Crate 3", "MQ Water Temple Three Torch Room Large Crate 3"),
						M("Crate.png", 128, 396, 24, "OOT MQ Water Temple Three Torch Room Large Crate 4", "MQ Water Temple Three Torch Room Large Crate 4"),
						M("Crate.png", 188, 435, 24, "OOT MQ Water Temple Three Torch Room Large Crate 5", "MQ Water Temple Three Torch Room Large Crate 5"),
						M("Crate.png", 230, 411, 24, "OOT MQ Water Temple Three Torch Room Large Crate 6", "MQ Water Temple Three Torch Room Large Crate 6"),
						M("Crate.png", 92, 230, 24, "OOT MQ Water Temple Three Torch Room Behind Gate Large Crate 1", "MQ Water Temple Three Torch Room Behind Gate Large Crate 1"),
						M("Crate.png", 110, 248, 24, "OOT MQ Water Temple Three Torch Room Behind Gate Large Crate 2", "MQ Water Temple Three Torch Room Behind Gate Large Crate 2"),
						M("Crate.png", 6, 286, 24, "OOT MQ Water Temple Three Torch Room Behind Gate Large Crate 3", "MQ Water Temple Three Torch Room Behind Gate Large Crate 3"),
						M("Crate.png", 534, 496, 24, "OOT MQ Water Temple Lizalfos Hallway Large Crate 1", "MQ Water Temple Lizalfos Hallway Large Crate 1"),
						M("Crate.png", 511, 519, 24, "OOT MQ Water Temple Lizalfos Hallway Large Crate 2", "MQ Water Temple Lizalfos Hallway Large Crate 2"),
						M("Crate.png", 651, 531, 24, "OOT MQ Water Temple Lizalfos Hallway Large Crate 3", "MQ Water Temple Lizalfos Hallway Large Crate 3"),
						M("Crate.png", 635, 547, 24, "OOT MQ Water Temple Lizalfos Hallway Large Crate 4", "MQ Water Temple Lizalfos Hallway Large Crate 4"),
						M("Crate.png", 557, 564, 24, "OOT MQ Water Temple Lizalfos Hallway Large Crate 5", "MQ Water Temple Lizalfos Hallway Large Crate 5"),
						M("Crate.png", 532, 405, 24, "OOT MQ Water Temple Lizalfos Hallway Large Crate 6", "MQ Water Temple Lizalfos Hallway Large Crate 6"),
						M("Crate.png", 575, 432, 24, "OOT MQ Water Temple Lizalfos Hallway Large Crate 7", "MQ Water Temple Lizalfos Hallway Large Crate 7"),
						M("Crate.png", 679, 440, 24, "OOT MQ Water Temple Lizalfos Hallway Large Crate 8", "MQ Water Temple Lizalfos Hallway Large Crate 8"),
						M("Crate.png", 708, 472, 24, "OOT MQ Water Temple Lizalfos Hallway Behind Gate Large Crate 1", "MQ Water Temple Lizalfos Hallway Behind Gate Large Crate 1"),
						M("Crate.png", 696, 492, 24, "OOT MQ Water Temple Lizalfos Hallway Behind Gate Large Crate 2", "MQ Water Temple Lizalfos Hallway Behind Gate Large Crate 2"),
						M("Wonder.png", 549, 538, 24, "OOT MQ Water Temple Wonder Item Lizalfos Hallway", "MQ Water Temple Wonder Item Lizalfos Hallway"),
						M("Wonder.png", 38, 251, 24, "OOT MQ Water Temple Wonder Item Triple Torches", "MQ Water Temple Wonder Item Triple Torches"),
						
						ME("Entrance.png", 267, 368, "Entrance shuffle (Lake Hylia)", "OOT_LAKE_HYLIA_FROM_TEMPLE_WATER")
                    }
                },
				new MapSubRegion
                {
                    Name = "Ruto Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/Ruto.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 216, 134, 40, "OOT Water Temple Map", "Water Temple Map"),
						M("Chest.png", 277, 253, 40, "OOT Water Temple Bombable Chest", "Water Temple Bombable Chest"),
						M("Chest.png", 253, 396, 40, "OOT Water Temple Shell Chest", "Water Temple Shell Chest"),
						M("Pot.png", 384, 439, 24, "OOT Water Temple Pot Ruto Room 1", "Water Temple Pot Ruto Room 1"),
						M("Pot.png", 402, 397, 24, "OOT Water Temple Pot Ruto Room 2", "Water Temple Pot Ruto Room 2"),
						
						M("Chest.png", 253, 396, 40, "OOT MQ Water Temple Compass Chest", "MQ Water Temple Compass Chest"),
						M("Chest.png", 277, 253, 40, "OOT MQ Water Temple Longshot Chest", "MQ Water Temple Longshot Chest"),
						M("Chest.png", 216, 134, 40, "OOT MQ Water Temple Map Chest", "MQ Water Temple Map Chest"),
						M("Pot.png", 384, 439, 24, "OOT MQ Water Temple Pot Ruto 1", "MQ Water Temple Pot Ruto 1"),
						M("Pot.png", 402, 397, 24, "OOT MQ Water Temple Pot Ruto 2", "MQ Water Temple Pot Ruto 2"),
						M("Wonder.png", 157, 364, 24, "OOT MQ Water Temple Wonder Item Lizalfos Room", "MQ Water Temple Wonder Item Lizalfos Room"),
						M("Wonder.png", 201, 229, 24, "OOT MQ Water Temple Wonder Item Longshot Room", "MQ Water Temple Wonder Item Longshot Room"),
						M("Wonder.png", 105, 95, 24, "OOT MQ Water Temple Wonder Item Stalfos Room", "MQ Water Temple Wonder Item Stalfos Room")
                    }
                },
				new MapSubRegion
                {
                    Name = "Compass Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/Compass.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 193, 196, 40, "OOT Water Temple Compass", "Water Temple Compass"),
						M("Pot.png", 401, 197, 24, "OOT Water Temple Pot Compass Room 1", "Water Temple Pot Compass Room 1"),
						M("Pot.png", 457, 207, 24, "OOT Water Temple Pot Compass Room 2", "Water Temple Pot Compass Room 2"),
						M("Pot.png", 429, 202, 24, "OOT Water Temple Pot Compass Room 3", "Water Temple Pot Compass Room 3"),
						
						M("Pot.png", 417, 195, 24, "OOT MQ Water Temple Pot Storage Room 1", "MQ Water Temple Pot Storage Room 1"),
						M("Pot.png", 469, 203, 24, "OOT MQ Water Temple Pot Storage Room 2", "MQ Water Temple Pot Storage Room 2"),
						M("Pot.png", 443, 199, 24, "OOT MQ Water Temple Pot Storage Room 3", "MQ Water Temple Pot Storage Room 3"),
						M("Crate.png", 215, 258, 24, "OOT MQ Water Temple Storage Room Large Crate 1", "MQ Water Temple Storage Room Large Crate 1"),
						M("Crate.png", 187, 254, 24, "OOT MQ Water Temple Storage Room Large Crate 2", "MQ Water Temple Storage Room Large Crate 2"),
						M("Crate.png", 159, 250, 24, "OOT MQ Water Temple Storage Room Large Crate 3", "MQ Water Temple Storage Room Large Crate 3"),
						M("Crate.png", 160, 203, 24, "OOT MQ Water Temple Storage Room Large Crate 4", "MQ Water Temple Storage Room Large Crate 4"),
						M("Crate.png", 188, 199, 24, "OOT MQ Water Temple Storage Room Large Crate 5", "MQ Water Temple Storage Room Large Crate 5"),
						M("Crate.png", 216, 195, 24, "OOT MQ Water Temple Storage Room Large Crate 6", "MQ Water Temple Storage Room Large Crate 6"),
						M("Crate.png", 131, 246, 24, "OOT MQ Water Temple Storage Room Large Crate 7", "MQ Water Temple Storage Room Large Crate 7"),
						M("Crate.png", 373, 192, 24, "OOT MQ Water Temple Storage Room Small Crate 1", "MQ Water Temple Storage Room Small Crate 1"),
						M("Crate.png", 317, 184, 24, "OOT MQ Water Temple Storage Room Small Crate 2", "MQ Water Temple Storage Room Small Crate 2"),
						M("Crate.png", 345, 188, 24, "OOT MQ Water Temple Storage Room Small Crate 3", "MQ Water Temple Storage Room Small Crate 3"),
						M("Crate.png", 289, 180, 24, "OOT MQ Water Temple Storage Room Small Crate 4", "MQ Water Temple Storage Room Small Crate 4")
                    }
                },
				new MapSubRegion
                {
                    Name = "Central Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/Central_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 214, 26, 40, "OOT Water Temple GS Center", "Water Temple GS Center"),
						M("Chest.png", 621, 439, 40, "OOT Water Temple Under Center", "Water Temple Under Center"),
						
						M("Chest.png", 621, 439, 40, "OOT MQ Water Temple Central Pillar Chest", "MQ Water Temple Central Pillar Chest"),
						M("Crate.png", 255, 118, 24, "OOT MQ Water Temple Central Tower Top Large Crate 1", "MQ Water Temple Central Tower Top Large Crate 1"),
						M("Crate.png", 279, 116, 24, "OOT MQ Water Temple Central Tower Top Large Crate 2", "MQ Water Temple Central Tower Top Large Crate 2"),
						M("Crate.png", 460, 581, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 1", "MQ Water Temple Central Tower Under Gate Large Crate 1"),
						M("Crate.png", 446, 569, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 2", "MQ Water Temple Central Tower Under Gate Large Crate 2"),
						M("Crate.png", 490, 587, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 3", "MQ Water Temple Central Tower Under Gate Large Crate 3"),
						M("Crate.png", 424, 507, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 4", "MQ Water Temple Central Tower Under Gate Large Crate 4"),
						M("Crate.png", 506, 581, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 5", "MQ Water Temple Central Tower Under Gate Large Crate 5"),
						M("Crate.png", 474, 593, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 6", "MQ Water Temple Central Tower Under Gate Large Crate 6"),
						M("Crate.png", 440, 501, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 7", "MQ Water Temple Central Tower Under Gate Large Crate 7"),
						M("Crate.png", 478, 520, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 8", "MQ Water Temple Central Tower Under Gate Large Crate 8"),
						M("Crate.png", 492, 532, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 9", "MQ Water Temple Central Tower Under Gate Large Crate 9"),
						M("Crate.png", 536, 503, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 10", "MQ Water Temple Central Tower Under Gate Large Crate 10"),
						M("Crate.png", 550, 515, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 11", "MQ Water Temple Central Tower Under Gate Large Crate 11"),
						M("Crate.png", 456, 495, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 12", "MQ Water Temple Central Tower Under Gate Large Crate 12"),
						M("Crate.png", 506, 544, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 13", "MQ Water Temple Central Tower Under Gate Large Crate 13"),
						M("Crate.png", 522, 538, 24, "OOT MQ Water Temple Central Tower Under Gate Large Crate 14", "MQ Water Temple Central Tower Under Gate Large Crate 14"),
						M("Wonder.png", 653, 424, 24, "OOT MQ Water Temple Wonder Item Under Pillar Room", "MQ Water Temple Wonder Item Under Pillar Room")
                    }
                },
				new MapSubRegion
                {
                    Name = "High Level Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/High_Level.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 464, 185, 40, "OOT MQ Water Temple GS High Water Changer", "MQ Water Temple GS High Water Changer"),
						M("Pot.png", 289, 507, 24, "OOT MQ Water Temple Pot Room Before High Water 1", "MQ Water Temple Pot Room Before High Water 1"),
						M("Pot.png", 297, 483, 24, "OOT MQ Water Temple Pot Room Before High Water 2", "MQ Water Temple Pot Room Before High Water 2"),
						M("Pot.png", 305, 459, 24, "OOT MQ Water Temple Pot Room Before High Water 3", "MQ Water Temple Pot Room Before High Water 3"),
						M("Crate.png", 671, 450, 24, "OOT MQ Water Temple Room Before High Water Lower Large Crate 1", "MQ Water Temple Room Before High Water Lower Large Crate 1"),
						M("Crate.png", 558, 425, 24, "OOT MQ Water Temple Room Before High Water Lower Large Crate 2", "MQ Water Temple Room Before High Water Lower Large Crate 2"),
						M("Crate.png", 698, 530, 24, "OOT MQ Water Temple Room Before High Water Lower Large Crate 3", "MQ Water Temple Room Before High Water Lower Large Crate 3"),
						M("Crate.png", 606, 425, 24, "OOT MQ Water Temple Room Before High Water Lower Large Crate 4", "MQ Water Temple Room Before High Water Lower Large Crate 4"),
						M("Crate.png", 365, 425, 24, "OOT MQ Water Temple Room Before High Water Lower Large Crate 5", "MQ Water Temple Room Before High Water Lower Large Crate 5"),
						M("Crate.png", 536, 553, 24, "OOT MQ Water Temple Room Before High Water Lower Large Crate 6", "MQ Water Temple Room Before High Water Lower Large Crate 6"),
						M("Crate.png", 412, 425, 24, "OOT MQ Water Temple Room Before High Water Lower Small Crate", "MQ Water Temple Room Before High Water Lower Small Crate"),
						M("Crate.png", 439, 199, 24, "OOT MQ Water Temple Room Before High Water Upper Large Crate 1", "MQ Water Temple Room Before High Water Upper Large Crate 1"),
						M("Crate.png", 412, 283, 24, "OOT MQ Water Temple Room Before High Water Upper Large Crate 2", "MQ Water Temple Room Before High Water Upper Large Crate 2"),
						M("Crate.png", 149, 241, 24, "OOT MQ Water Temple Room Before High Water Upper Small Crate", "MQ Water Temple Room Before High Water Upper Small Crate"),
						M("Wonder.png", 269, 441, 24, "OOT MQ Water Temple Wonder Item Above 2F West Room", "MQ Water Temple Wonder Item Above 2F West Room")
                    }
                },
				new MapSubRegion
                {
                    Name = "Waterfalls Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/Waterfalls.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 475, 184, 40, "OOT Water Temple GS Large Pit", "Water Temple GS Large Pit"),
						
						M("Wonder.png", 149, 222, 24, "OOT MQ Water Temple Wonder Item Hookshot Staircase Right 1", "MQ Water Temple Wonder Item Hookshot Staircase Right 1"),
						M("Wonder.png", 129, 217, 24, "OOT MQ Water Temple Wonder Item Hookshot Staircase Right 2", "MQ Water Temple Wonder Item Hookshot Staircase Right 2"),
						M("Wonder.png", 169, 217, 24, "OOT MQ Water Temple Wonder Item Hookshot Staircase Right 3", "MQ Water Temple Wonder Item Hookshot Staircase Right 3"),
						M("Wonder.png", 43, 239, 24, "OOT MQ Water Temple Wonder Item Hookshot Staircase Left 1", "MQ Water Temple Wonder Item Hookshot Staircase Left 1"),
						M("Wonder.png", 63, 244, 24, "OOT MQ Water Temple Wonder Item Hookshot Staircase Left 2", "MQ Water Temple Wonder Item Hookshot Staircase Left 2"),
						M("Wonder.png", 83, 239, 24, "OOT MQ Water Temple Wonder Item Hookshot Staircase Left 3", "MQ Water Temple Wonder Item Hookshot Staircase Left 3")
                    }
                },
				new MapSubRegion
                {
                    Name = "Pre-Mini-Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/Pre-Mini-Boss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Pot.png", 472, 115, 24, "OOT Water Temple Pot Before Dark Link 1", "Water Temple Pot Before Dark Link 1"),
						M("Pot.png", 513, 115, 24, "OOT Water Temple Pot Before Dark Link 2", "Water Temple Pot Before Dark Link 2"),
						
						M("Pot.png", 336, 297, 24, "OOT MQ Water Temple Pot Before Dark Link Ledge 1", "MQ Water Temple Pot Before Dark Link Ledge 1"),
						M("Pot.png", 328, 313, 24, "OOT MQ Water Temple Pot Before Dark Link Ledge 2", "MQ Water Temple Pot Before Dark Link Ledge 2"),
						M("Pot.png", 344, 281, 24, "OOT MQ Water Temple Pot Before Dark Link Ledge 3", "MQ Water Temple Pot Before Dark Link Ledge 3"),
						M("Pot.png", 472, 115, 24, "OOT MQ Water Temple Pot Before Dark Link Near Door 1", "MQ Water Temple Pot Before Dark Link Near Door 1"),
						M("Pot.png", 513, 115, 24, "OOT MQ Water Temple Pot Before Dark Link Near Door 2", "MQ Water Temple Pot Before Dark Link Near Door 2"),
						M("Fairy_Spot.png", 474, 348, 40, "OOT MQ Water Temple Big Fairy Before Dark Link Ledge", "MQ Water Temple Big Fairy Before Dark Link Ledge"),
						M("Fairy_Spot.png", 453, 125, 40, "OOT MQ Water Temple Big Fairy Before Dark Link Near Door 1", "MQ Water Temple Big Fairy Before Dark Link Near Door 1"),
						M("Fairy_Spot.png", 518, 125, 40, "OOT MQ Water Temple Big Fairy Before Dark Link Near Door 2", "MQ Water Temple Big Fairy Before Dark Link Near Door 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Dragon Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/Dragon.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 414, 278, 40, "OOT Water Temple GS River", "Water Temple GS River"),
						M("Chest.png", 136, 152, 40, "OOT Water Temple Longshot", "Water Temple Longshot"),
						M("Chest.png", 736, 486, 40, "OOT Water Temple Dragon Chest", "Water Temple Dragon Chest"),
						M("Chest.png", 659, 299, 40, "OOT Water Temple River Chest", "Water Temple River Chest"),
						M("Heart.png", 513, 494, 24, "OOT Water Temple Heart 1", "Water Temple Heart 1"),
						M("Heart.png", 513, 431, 24, "OOT Water Temple Heart 2", "Water Temple Heart 2"),
						M("Heart.png", 413, 344, 24, "OOT Water Temple Heart 3", "Water Temple Heart 3"),
						M("Heart.png", 549, 339, 24, "OOT Water Temple Heart 4", "Water Temple Heart 4"),
						M("Pot.png", 610, 397, 24, "OOT Water Temple Pot River 1", "Water Temple Pot River 1"),
						M("Pot.png", 632, 397, 24, "OOT Water Temple Pot River 2", "Water Temple Pot River 2"),
						
						M("Gold_Skulltula.png", 463, 390, 40, "OOT MQ Water Temple GS River", "MQ Water Temple GS River"),
						M("Pot.png", 160, 153, 24, "OOT MQ Water Temple Pot After Dark Link 1", "MQ Water Temple Pot After Dark Link 1"),
						M("Pot.png", 128, 163, 24, "OOT MQ Water Temple Pot After Dark Link 2", "MQ Water Temple Pot After Dark Link 2"),
						M("Pot.png", 610, 397, 24, "OOT MQ Water Temple Pot River 1", "MQ Water Temple Pot River 1"),
						M("Pot.png", 632, 397, 24, "OOT MQ Water Temple Pot River 2", "MQ Water Temple Pot River 2"),
						M("Crate.png", 848, 376, 24, "OOT MQ Water Temple Dragon Room At Door Large Crate 1", "MQ Water Temple Dragon Room At Door Large Crate 1"),
						M("Crate.png", 864, 360, 24, "OOT MQ Water Temple Dragon Room At Door Large Crate 2", "MQ Water Temple Dragon Room At Door Large Crate 2"),
						M("Crate.png", 754, 469, 24, "OOT MQ Water Temple Dragon Room Underwater Large Crate 1", "MQ Water Temple Dragon Room Underwater Large Crate 1"),
						M("Crate.png", 778, 475, 24, "OOT MQ Water Temple Dragon Room Underwater Large Crate 2", "MQ Water Temple Dragon Room Underwater Large Crate 2"),
						M("Crate.png", 760, 453, 24, "OOT MQ Water Temple Dragon Room Underwater Large Crate 3", "MQ Water Temple Dragon Room Underwater Large Crate 3"),
						M("Crate.png", 766, 437, 24, "OOT MQ Water Temple Dragon Room Underwater Large Crate 4", "MQ Water Temple Dragon Room Underwater Large Crate 4"),
						M("Crate.png", 724, 511, 24, "OOT MQ Water Temple Dragon Room At Torches Large Crate 1", "MQ Water Temple Dragon Room At Torches Large Crate 1"),
						M("Crate.png", 748, 517, 24, "OOT MQ Water Temple Dragon Room At Torches Large Crate 2", "MQ Water Temple Dragon Room At Torches Large Crate 2"),
						M("Crate.png", 719, 487, 24, "OOT MQ Water Temple Dragon Room At Torches Small Crate 1", "MQ Water Temple Dragon Room At Torches Small Crate 1"),
						M("Crate.png", 767, 499, 24, "OOT MQ Water Temple Dragon Room At Torches Small Crate 2", "MQ Water Temple Dragon Room At Torches Small Crate 2"),
						M("Crate.png", 743, 493, 24, "OOT MQ Water Temple Dragon Room At Torches Small Crate 3", "MQ Water Temple Dragon Room At Torches Small Crate 3"),
						M("Wonder.png", 96, 112, 24, "OOT MQ Water Temple Wonder Item After Dark Link", "MQ Water Temple Wonder Item After Dark Link"),
						M("Wonder.png", 728, 535, 24, "OOT MQ Water Temple Wonder Item Dragon Room Portrait", "MQ Water Temple Wonder Item Dragon Room Portrait"),
						M("Wonder.png", 820, 444, 24, "OOT MQ Water Temple Wonder Item Dragon Room Eyes 1", "MQ Water Temple Wonder Item Dragon Room Eyes 1"),
						M("Wonder.png", 839, 452, 24, "OOT MQ Water Temple Wonder Item Dragon Room Eyes 2", "MQ Water Temple Wonder Item Dragon Room Eyes 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "First Geyser Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/Geyser.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 645, 375, 40, "OOT MQ Water Temple Boss Key Chest", "MQ Water Temple Boss Key Chest"),
						M("Pot.png", 709, 135, 24, "OOT MQ Water Temple Pot Boss Key Room", "MQ Water Temple Pot Boss Key Room"),
						M("Crate.png", 599, 135, 24, "OOT MQ Water Temple Boss Key Room By Switch Large Crate", "MQ Water Temple Boss Key Room By Switch Large Crate"),
						M("Crate.png", 598, 333, 24, "OOT MQ Water Temple Boss Key Room In Center Large Crate 1", "MQ Water Temple Boss Key Room In Center Large Crate 1"),
						M("Crate.png", 698, 249, 24, "OOT MQ Water Temple Boss Key Room In Center Large Crate 2", "MQ Water Temple Boss Key Room In Center Large Crate 2"),
						M("Crate.png", 721, 272, 24, "OOT MQ Water Temple Boss Key Room In Center Large Crate 3", "MQ Water Temple Boss Key Room In Center Large Crate 3"),
						M("Crate.png", 598, 290, 24, "OOT MQ Water Temple Boss Key Room In Center Large Crate 4", "MQ Water Temple Boss Key Room In Center Large Crate 4")
						
                    }
                },
				new MapSubRegion
                {
                    Name = "Boulder Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/Boulder.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 637, 165, 40, "OOT Water Temple GS Waterfalls", "Water Temple GS Waterfalls"),
						M("Chest.png", 757, 370, 40, "OOT Water Temple Boss Key Chest", "Water Temple Boss Key Chest"),
						M("Pot.png", 854, 429, 24, "OOT Water Temple Pot Boss Key Room 1", "Water Temple Pot Boss Key Room 1"),
						M("Pot.png", 721, 436, 24, "OOT Water Temple Pot Boss Key Room 2", "Water Temple Pot Boss Key Room 2"),
						
						M("Gold_Skulltula.png", 326, 493, 40, "OOT MQ Water Temple GS Side Loop", "MQ Water Temple GS Side Loop"),
						M("Collectible.png", 778, 322, 40, "OOT MQ Water Temple Side Loop Key", "MQ Water Temple Side Loop Key"),
						M("Pot.png", 858, 428, 24, "OOT MQ Water Temple Pot Stalfos Room 1", "MQ Water Temple Pot Stalfos Room 1"),
						M("Pot.png", 719, 437, 24, "OOT MQ Water Temple Pot Stalfos Room 2", "MQ Water Temple Pot Stalfos Room 2"),
						M("Crate.png", 334, 528, 24, "OOT MQ Water Temple Side Loop First Room In Gate Large Crate 1", "MQ Water Temple Side Loop First Room In Gate Large Crate 1"),
						M("Crate.png", 284, 531, 24, "OOT MQ Water Temple Side Loop First Room In Gate Large Crate 2", "MQ Water Temple Side Loop First Room In Gate Large Crate 2"),
						M("Crate.png", 181, 537, 24, "OOT MQ Water Temple Side Loop First Room In Gate Large Crate 3", "MQ Water Temple Side Loop First Room In Gate Large Crate 3"),
						M("Crate.png", 131, 540, 24, "OOT MQ Water Temple Side Loop First Room In Gate Large Crate 4", "MQ Water Temple Side Loop First Room In Gate Large Crate 4"),
						M("Crate.png", 343, 206, 24, "OOT MQ Water Temple Side Loop First Room At Door Large Crate 1", "MQ Water Temple Side Loop First Room At Door Large Crate 1"),
						M("Crate.png", 233, 209, 24, "OOT MQ Water Temple Side Loop First Room At Door Large Crate 2", "MQ Water Temple Side Loop First Room At Door Large Crate 2"),
						M("Crate.png", 163, 352, 24, "OOT MQ Water Temple Side Loop First Room Water Large Crate 1", "MQ Water Temple Side Loop First Room Water Large Crate 1"),
						M("Crate.png", 258, 344, 24, "OOT MQ Water Temple Side Loop First Room Water Large Crate 2", "MQ Water Temple Side Loop First Room Water Large Crate 2"),
						M("Crate.png", 300, 418, 24, "OOT MQ Water Temple Side Loop First Room Water Large Crate 3", "MQ Water Temple Side Loop First Room Water Large Crate 3"),
						M("Crate.png", 192, 429, 24, "OOT MQ Water Temple Side Loop First Room Water Large Crate 4", "MQ Water Temple Side Loop First Room Water Large Crate 4"),
						M("Crate.png", 402, 398, 24, "OOT MQ Water Temple Side Loop First Room Water Large Crate 5", "MQ Water Temple Side Loop First Room Water Large Crate 5"),
						M("Crate.png", 386, 319, 24, "OOT MQ Water Temple Side Loop First Room Water Large Crate 6", "MQ Water Temple Side Loop First Room Water Large Crate 6"),
						M("Crate.png", 829, 365, 24, "OOT MQ Water Temple Side Loop Stalfos Room Large Crate 1", "MQ Water Temple Side Loop Stalfos Room Large Crate 1"),
						M("Crate.png", 837, 389, 24, "OOT MQ Water Temple Side Loop Stalfos Room Large Crate 2", "MQ Water Temple Side Loop Stalfos Room Large Crate 2"),
						M("Crate.png", 701, 397, 24, "OOT MQ Water Temple Side Loop Stalfos Room Large Crate 3", "MQ Water Temple Side Loop Stalfos Room Large Crate 3"),
						M("Crate.png", 693, 373, 24, "OOT MQ Water Temple Side Loop Stalfos Room Large Crate 4", "MQ Water Temple Side Loop Stalfos Room Large Crate 4"),
						M("Crate.png", 814, 331, 24, "OOT MQ Water Temple Side Loop Stalfos Room Large Crate 5", "MQ Water Temple Side Loop Stalfos Room Large Crate 5"),
						M("Wonder.png", 798, 430, 24, "OOT MQ Water Temple Wonder Item Freestanding Room", "MQ Water Temple Wonder Item Freestanding Room")
                    }
                },
				new MapSubRegion
                {
                    Name = "Block Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/Block.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Pot.png", 447, 506, 24, "OOT Water Temple Pot Blocks Room 1", "Water Temple Pot Blocks Room 1"),
						M("Pot.png", 479, 481, 24, "OOT Water Temple Pot Blocks Room 2", "Water Temple Pot Blocks Room 2"),
						
						M("Pot.png", 447, 506, 24, "OOT MQ Water Temple Pot Loop 1", "MQ Water Temple Pot Loop 1"),
						M("Pot.png", 479, 481, 24, "OOT MQ Water Temple Pot Loop 2", "MQ Water Temple Pot Loop 2"),
						M("Crate.png", 385, 553, 24, "OOT MQ Water Temple Dodongo Room Large Crate 1", "MQ Water Temple Dodongo Room Large Crate 1"),
						M("Crate.png", 574, 469, 24, "OOT MQ Water Temple Dodongo Room Large Crate 2", "MQ Water Temple Dodongo Room Large Crate 2"),
						M("Crate.png", 615, 364, 24, "OOT MQ Water Temple Dodongo Room Large Crate 3", "MQ Water Temple Dodongo Room Large Crate 3"),
						M("Crate.png", 180, 340, 24, "OOT MQ Water Temple Dodongo Room Large Crate 4", "MQ Water Temple Dodongo Room Large Crate 4"),
						M("Crate.png", 332, 431, 24, "OOT MQ Water Temple Dodongo Room Large Crate 5", "MQ Water Temple Dodongo Room Large Crate 5")
                    }
                },
				new MapSubRegion
                {
                    Name = "Second Geyser Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/Geyser_2.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Wonder.png", 653, 526, 24, "OOT MQ Water Temple Wonder Item Water Sprouts 1", "MQ Water Temple Wonder Item Water Sprouts 1"),
						M("Wonder.png", 755, 569, 24, "OOT MQ Water Temple Wonder Item Water Sprouts 2", "MQ Water Temple Wonder Item Water Sprouts 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Pre-Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/Pre-Boss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Wonder.png", 83, 509, 24, "OOT MQ Water Temple Wonder Item Before Boss 1", "MQ Water Temple Wonder Item Before Boss 1"),
						M("Wonder.png", 139, 416, 24, "OOT MQ Water Temple Wonder Item Before Boss 2", "MQ Water Temple Wonder Item Before Boss 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Water/Boss.png",
                    DestinationEntranceIds = new List<string> { "OOT_BOSS_TEMPLE_WATER" },
                    Marks = new List<MapMark>
                    {
						M("Collectible.png", 390, 299, 40, "OOT Water Temple Boss HC", "Water Temple Boss HC"),
						M("NPC.png", 453, 299, 40, "OOT Water Temple Boss", "Water Temple Boss"),
						
						ME("Entrance.png", 788, 282, "Entrance shuffle (Morpha)", "OOT_BOSS_TEMPLE_WATER")
                    }
                }
            };
            return mapRegion;
        }
		public static MapRegion ShadowTemple()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Shadow Temple";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Entrance",
                    BackgroundImage = "region maps/OoT/Dungeons/Shadow/Entrance.png",
                    DestinationEntranceIds = new List<string> { "OOT_TEMPLE_SHADOW" },
                    Marks = new List<MapMark>
                    {
						M("Crate.png", 789, 581, 24, "MQ OOT Shadow Temple Truth Spinner Room Small Crate 1", "MQ Shadow Temple Truth Spinner Room Small Crate 1"),
						M("Crate.png", 789, 533, 24, "MQ OOT Shadow Temple Truth Spinner Room Small Crate 2", "MQ Shadow Temple Truth Spinner Room Small Crate 2"),
						M("Crate.png", 789, 509, 24, "MQ OOT Shadow Temple Truth Spinner Room Small Crate 3", "MQ Shadow Temple Truth Spinner Room Small Crate 3"),
						M("Crate.png", 789, 557, 24, "MQ OOT Shadow Temple Truth Spinner Room Small Crate 4", "MQ Shadow Temple Truth Spinner Room Small Crate 4"),
						
						ME("Entrance.png", 554, 802, "Entrance shuffle (Graveyard)", "OOT_GRAVEYARD_FROM_TEMPLE_SHADOW")
                    }
                },
				new MapSubRegion
                {
                    Name = "Lobby",
                    BackgroundImage = "region maps/OoT/Dungeons/Shadow/Lobby.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 668, 244, 40, "OOT Shadow Temple Map", "Shadow Temple Map"),
						M("Chest.png", 138, 121, 40, "OOT Shadow Temple Hover Boots", "Shadow Temple Hover Boots"),
						M("Pot.png", 334, 263, 24, "OOT Shadow Temple Pot Early Maze 1", "Shadow Temple Pot Early Maze 1"),
						M("Pot.png", 268, 364, 24, "OOT Shadow Temple Pot Early Maze 2", "Shadow Temple Pot Early Maze 2"),
						M("Pot.png", 361, 415, 24, "OOT Shadow Temple Pot Early Maze 3", "Shadow Temple Pot Early Maze 3"),
						M("Pot.png", 299, 381, 24, "OOT Shadow Temple Pot Early Maze 4", "Shadow Temple Pot Early Maze 4"),
						M("Pot.png", 513, 389, 24, "OOT Shadow Temple Pot Early Maze 5", "Shadow Temple Pot Early Maze 5"),
						M("Pot.png", 490, 432, 24, "OOT Shadow Temple Pot Early Maze 6", "Shadow Temple Pot Early Maze 6"),
						M("Pot.png", 676, 323, 24, "OOT Shadow Temple Pot Map Room 1", "Shadow Temple Pot Map Room 1"),
						M("Pot.png", 614, 289, 24, "OOT Shadow Temple Pot Map Room 2", "Shadow Temple Pot Map Room 2"),
						M("Pot.png", 330, 398, 24, "OOT Shadow Temple Flying Pot Early Maze", "Shadow Temple Flying Pot Early Maze"),
						
						M("Chest.png", 668, 244, 40, "OOT MQ Shadow Temple Compass Chest", "MQ Shadow Temple Compass Chest"),
						M("Chest.png", 138, 121, 40, "OOT MQ Shadow Temple Hover Boots Chest", "MQ Shadow Temple Hover Boots Chest"),
						M("Pot.png", 490, 432, 24, "MQ OOT Shadow Temple Pot Entrance Maze 1", "MQ Shadow Temple Pot Entrance Maze 1"),
						M("Pot.png", 513, 389, 24, "MQ OOT Shadow Temple Pot Entrance Maze 2", "MQ Shadow Temple Pot Entrance Maze 2"),
						M("Pot.png", 708, 273, 24, "MQ OOT Shadow Temple Pot Compass Room 1", "MQ Shadow Temple Pot Compass Room 1"),
						M("Pot.png", 708, 249, 24, "MQ OOT Shadow Temple Pot Compass Room 2", "MQ Shadow Temple Pot Compass Room 2"),
						M("Pot.png", 312, 389, 24, "MQ OOT Shadow Temple Flying Pot Entrance Maze 1", "MQ Shadow Temple Flying Pot Entrance Maze 1"),
						M("Pot.png", 549, 530, 24, "MQ OOT Shadow Temple Flying Pot Entrance Maze 2", "MQ Shadow Temple Flying Pot Entrance Maze 2"),
						M("Pot.png", 349, 410, 24, "MQ OOT Shadow Temple Flying Pot Entrance Maze 3", "MQ Shadow Temple Flying Pot Entrance Maze 3"),
						M("Pot.png", 512, 509, 24, "MQ OOT Shadow Temple Flying Pot Entrance Maze 4", "MQ Shadow Temple Flying Pot Entrance Maze 4")						
                    }
                },
				new MapSubRegion
                {
                    Name = "Scythe Rooms",
                    BackgroundImage = "region maps/OoT/Dungeons/Shadow/Scythe.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 856, 345, 40, "OOT Shadow Temple Compass", "Shadow Temple Compass"),
						M("Chest.png", 290, 125, 40, "OOT Shadow Temple Silver Rupees", "Shadow Temple Silver Rupees"),
						M("Fairy_Spot.png", 578, 341, 40, "OOT Shadow Temple Beamos Big Fairy", "Shadow Temple Beamos Big Fairy"),
						M("Silver_Rupee.png", 193, 384, 24, "OOT Shadow Temple SR Scythe 1", "Shadow Temple SR Scythe 1"),
						M("Silver_Rupee.png", 109, 472, 24, "OOT Shadow Temple SR Scythe 2", "Shadow Temple SR Scythe 2"),
						M("Silver_Rupee.png", 62, 271, 24, "OOT Shadow Temple SR Scythe 3", "Shadow Temple SR Scythe 3"),
						M("Silver_Rupee.png", 219, 538, 24, "OOT Shadow Temple SR Scythe 4", "Shadow Temple SR Scythe 4"),
						M("Silver_Rupee.png", 277, 325, 24, "OOT Shadow Temple SR Scythe 5", "Shadow Temple SR Scythe 5"),
						
						M("Chest.png", 856, 345, 40, "OOT MQ Shadow Temple First Gibdos Chest", "MQ Shadow Temple First Gibdos Chest"),
						M("Chest.png", 290, 125, 40, "OOT MQ Shadow Temple Map Chest", "MQ Shadow Temple Map Chest"),
						M("Fairy_Spot.png", 578, 341, 40, "OOT MQ Shadow Temple Beamos Big Fairy", "MQ Shadow Temple Beamos Big Fairy"),
						M("Silver_Rupee.png", 219, 538, 24, "OOT MQ Shadow Temple SR Scythe 1", "MQ Shadow Temple SR Scythe 1"),
						M("Silver_Rupee.png", 109, 472, 24, "OOT MQ Shadow Temple SR Scythe 2", "MQ Shadow Temple SR Scythe 2"),
						M("Silver_Rupee.png", 193, 384, 24, "OOT MQ Shadow Temple SR Scythe 3", "MQ Shadow Temple SR Scythe 3"),
						M("Silver_Rupee.png", 277, 325, 24, "OOT MQ Shadow Temple SR Scythe 4", "MQ Shadow Temple SR Scythe 4"),
						M("Silver_Rupee.png", 62, 271, 24, "OOT MQ Shadow Temple SR Scythe 5", "MQ Shadow Temple SR Scythe 5")
                    }
                },
				new MapSubRegion
                {
                    Name = "Stalfos Rooms",
                    BackgroundImage = "region maps/OoT/Dungeons/Shadow/Stalfos.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 732, 525, 40, "OOT Shadow Temple GS Skull Pot", "Shadow Temple GS Skull Pot"),
						M("Collectible.png", 689, 529, 40, "OOT Shadow Temple Skull", "Shadow Temple Skull"),
						M("Chest.png", 503, 505, 40, "OOT Shadow Temple Invisible Spike Room", "Shadow Temple Invisible Spike Room"),
						M("Fairy_Spot.png", 351, 147, 40, "OOT Shadow Temple Stalfos Big Fairy", "Shadow Temple Stalfos Big Fairy"),
						M("Silver_Rupee.png", 585, 229, 24, "OOT Shadow Temple SR Pit 1", "Shadow Temple SR Pit 1"),
						M("Silver_Rupee.png", 597, 159, 24, "OOT Shadow Temple SR Pit 2", "Shadow Temple SR Pit 2"),
						M("Silver_Rupee.png", 591, 194, 24, "OOT Shadow Temple SR Pit 3", "Shadow Temple SR Pit 3"),
						M("Silver_Rupee.png", 567, 189, 24, "OOT Shadow Temple SR Pit 4", "Shadow Temple SR Pit 4"),
						M("Silver_Rupee.png", 615, 199, 24, "OOT Shadow Temple SR Pit 5", "Shadow Temple SR Pit 5"),
						M("Silver_Rupee.png", 590, 562, 24, "OOT Shadow Temple SR Spikes Back Left", "Shadow Temple SR Spikes Back Left"),
						M("Silver_Rupee.png", 581, 471, 24, "OOT Shadow Temple SR Spikes Front Left", "Shadow Temple SR Spikes Front Left"),
						M("Silver_Rupee.png", 513, 485, 24, "OOT Shadow Temple SR Spikes Center", "Shadow Temple SR Spikes Center"),
						M("Silver_Rupee.png", 552, 529, 24, "OOT Shadow Temple SR Spikes Midair", "Shadow Temple SR Spikes Midair"),
						M("Silver_Rupee.png", 431, 502, 24, "OOT Shadow Temple SR Spikes Right", "Shadow Temple SR Spikes Right"),
						
						M("Chest.png", 563, 147, 40, "OOT MQ Shadow Temple Huge Pit Silver Rupee Chest", "MQ Shadow Temple Huge Pit Silver Rupee Chest"),
						M("Chest.png", 761, 522, 40, "OOT MQ Shadow Temple Stalfos Room Chest", "MQ Shadow Temple Stalfos Room Chest"),
						M("Chest.png", 503, 505, 40, "OOT MQ Shadow Temple Invisible Spike Floor Chest", "MQ Shadow Temple Invisible Spike Floor Chest"),
						M("Fairy_Spot.png", 351, 147, 40, "OOT MQ Shadow Temple Guillotines Room Big Fairy", "MQ Shadow Temple Guillotines Room Big Fairy"),
						M("Silver_Rupee.png", 615, 199, 24, "OOT MQ Shadow Temple SR Pit Back", "MQ Shadow Temple SR Pit Back"),
						M("Silver_Rupee.png", 591, 194, 24, "OOT MQ Shadow Temple SR Pit Midair Low", "MQ Shadow Temple SR Pit Midair Low"),
						M("Silver_Rupee.png", 597, 159, 24, "OOT MQ Shadow Temple SR Pit Midair High", "MQ Shadow Temple SR Pit Midair High"),
						M("Silver_Rupee.png", 585, 229, 24, "OOT MQ Shadow Temple SR Pit Right", "MQ Shadow Temple SR Pit Right"),
						M("Silver_Rupee.png", 567, 189, 24, "OOT MQ Shadow Temple SR Pit Front", "MQ Shadow Temple SR Pit Front"),
						M("Silver_Rupee.png", 593, 567, 24, "OOT MQ Shadow Temple SR Spikes Northwest Corner", "MQ Shadow Temple SR Spikes Northwest Corner"),
						M("Silver_Rupee.png", 581, 471, 24, "OOT MQ Shadow Temple SR Spikes Southwest Wall", "MQ Shadow Temple SR Spikes Southwest Wall"),
						M("Silver_Rupee.png", 554, 515, 24, "OOT MQ Shadow Temple SR Spikes West Midair", "MQ Shadow Temple SR Spikes West Midair"),
						M("Silver_Rupee.png", 507, 551, 24, "OOT MQ Shadow Temple SR Spikes Ceiling", "MQ Shadow Temple SR Spikes Ceiling"),
						M("Silver_Rupee.png", 513, 491, 24, "OOT MQ Shadow Temple SR Spikes Center Ground", "MQ Shadow Temple SR Spikes Center Ground"),
						M("Silver_Rupee.png", 515, 468, 24, "OOT MQ Shadow Temple SR Spikes Center Midair", "MQ Shadow Temple SR Spikes Center Midair"),
						M("Silver_Rupee.png", 517, 445, 24, "OOT MQ Shadow Temple SR Spikes South Midair", "MQ Shadow Temple SR Spikes South Midair"),
						M("Silver_Rupee.png", 466, 506, 24, "OOT MQ Shadow Temple SR Spikes East Ground", "MQ Shadow Temple SR Spikes East Ground"),
						M("Silver_Rupee.png", 458, 548, 24, "OOT MQ Shadow Temple SR Spikes Northeast Wall", "MQ Shadow Temple SR Spikes Northeast Wall"),
						M("Silver_Rupee.png", 428, 498, 24, "OOT MQ Shadow Temple SR Spikes East Wall", "MQ Shadow Temple SR Spikes East Wall")
                    }
                },
				new MapSubRegion
                {
                    Name = "Invisible Scythe Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Shadow/Invisible_Scythe.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 451, 8, 40, "OOT Shadow Temple GS Invisible Scythe", "Shadow Temple GS Invisible Scythe"),
						M("Chest.png", 476, 53, 40, "OOT Shadow Temple Spinning Blades Invisible", "Shadow Temple Spinning Blades Invisible"),
						M("Chest.png", 426, 53, 40, "OOT Shadow Temple Spinning Blades Visible", "Shadow Temple Spinning Blades Visible"),
						M("Heart.png", 217, 176, 24, "OOT Shadow Temple Heart Scythe 1", "Shadow Temple Heart Scythe 1"),
						M("Heart.png", 238, 155, 24, "OOT Shadow Temple Heart Scythe 2", "Shadow Temple Heart Scythe 2"),
						
						M("Chest.png", 476, 53, 40, "OOT MQ Shadow Temple Second Silver Rupee Invisible Chest", "MQ Shadow Temple Second Silver Rupee Invisible Chest"),
						M("Chest.png", 426, 53, 40, "OOT MQ Shadow Temple Second Silver Rupee Visible Chest", "MQ Shadow Temple Second Silver Rupee Visible Chest"),
						M("Heart.png", 217, 176, 24, "OOT MQ Shadow Temple Heart Invisible Blades 1", "MQ Shadow Temple Heart Invisible Blades 1"),
						M("Heart.png", 238, 155, 24, "OOT MQ Shadow Temple Heart Invisible Blades 2", "MQ Shadow Temple Heart Invisible Blades 2"),
						M("Silver_Rupee.png", 425, 409, 24, "OOT MQ Shadow Temple SR Invisible Blades Ground 1", "MQ Shadow Temple SR Invisible Blades Ground 1"),
						M("Silver_Rupee.png", 531, 381, 24, "OOT MQ Shadow Temple SR Invisible Blades Ground 2", "MQ Shadow Temple SR Invisible Blades Ground 2"),
						M("Silver_Rupee.png", 340, 354, 24, "OOT MQ Shadow Temple SR Invisible Blades Ground 3", "MQ Shadow Temple SR Invisible Blades Ground 3"),
						M("Silver_Rupee.png", 594, 331, 24, "OOT MQ Shadow Temple SR Invisible Blades Ground 4", "MQ Shadow Temple SR Invisible Blades Ground 4"),
						M("Silver_Rupee.png", 393, 270, 24, "OOT MQ Shadow Temple SR Invisible Blades Ground 5", "MQ Shadow Temple SR Invisible Blades Ground 5"),
						M("Silver_Rupee.png", 503, 250, 24, "OOT MQ Shadow Temple SR Invisible Blades Ground 6", "MQ Shadow Temple SR Invisible Blades Ground 6"),
						M("Silver_Rupee.png", 600, 243, 24, "OOT MQ Shadow Temple SR Invisible Blades Ground 7", "MQ Shadow Temple SR Invisible Blades Ground 7"),
						M("Silver_Rupee.png", 363, 198, 24, "OOT MQ Shadow Temple SR Invisible Blades Ground 8", "MQ Shadow Temple SR Invisible Blades Ground 8"),
						M("Silver_Rupee.png", 459, 180, 24, "OOT MQ Shadow Temple SR Invisible Blades Ground 9", "MQ Shadow Temple SR Invisible Blades Ground 9"),
						M("Silver_Rupee.png", 692, 165, 24, "OOT MQ Shadow Temple SR Invisible Blades Time Block", "MQ Shadow Temple SR Invisible Blades Time Block")
                    }
                },
				new MapSubRegion
                {
                    Name = "Falling Spikes Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Shadow/Falling_Spikes.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 421, 407, 40, "OOT Shadow Temple GS Falling Spikes", "Shadow Temple GS Falling Spikes"),
						M("Chest.png", 622, 534, 40, "OOT Shadow Temple Falling Spikes Lower", "Shadow Temple Falling Spikes Lower"),
						M("Chest.png", 72, 393, 40, "OOT Shadow Temple Falling Spikes Upper 1", "Shadow Temple Falling Spikes Upper 1"),
						M("Chest.png", 752, 342, 40, "OOT Shadow Temple Falling Spikes Upper 2", "Shadow Temple Falling Spikes Upper 2"),
						M("Pot.png", 547, 559, 24, "OOT Shadow Temple Pot Falling Spikes Bottom 1", "Shadow Temple Pot Falling Spikes Bottom 1"),
						M("Pot.png", 380, 559, 24, "OOT Shadow Temple Pot Falling Spikes Bottom 2", "Shadow Temple Pot Falling Spikes Bottom 2"),
						M("Pot.png", 113, 392, 24, "OOT Shadow Temple Pot Falling Spikes Top 1", "Shadow Temple Pot Falling Spikes Top 1"),
						M("Pot.png", 48, 411, 24, "OOT Shadow Temple Pot Falling Spikes Top 2", "Shadow Temple Pot Falling Spikes Top 2"),
						
						M("Gold_Skulltula.png", 421, 407, 40, "OOT MQ Shadow Temple GS Spike Curtain", "MQ Shadow Temple GS Spike Curtain"),
						M("Chest.png", 752, 342, 40, "OOT MQ Shadow Temple Spike Curtain Upper Cage Chest", "MQ Shadow Temple Spike Curtain Upper Cage Chest"),
						M("Chest.png", 72, 393, 40, "OOT MQ Shadow Temple Spike Curtain Upper Switch Chest", "MQ Shadow Temple Spike Curtain Upper Switch Chest"),
						M("Chest.png", 622, 534, 40, "OOT MQ Shadow Temple Spike Curtain Ground Chest", "MQ Shadow Temple Spike Curtain Ground Chest"),
						M("Pot.png", 547, 559, 24, "OOT MQ Shadow Temple Pot Guillotines Room Lower 1", "MQ Shadow Temple Pot Guillotines Room Lower 1"),
						M("Pot.png", 380, 559, 24, "OOT MQ Shadow Temple Pot Guillotines Room Lower 2", "MQ Shadow Temple Pot Guillotines Room Lower 2"),
						M("Pot.png", 113, 392, 24, "OOT MQ Shadow Temple Pot Guillotines Room Upper 1", "MQ Shadow Temple Pot Guillotines Room Upper 1"),
						M("Pot.png", 48, 411, 24, "OOT MQ Shadow Temple Pot Guillotines Room Upper 2", "MQ Shadow Temple Pot Guillotines Room Upper 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Boat Rooms",
                    BackgroundImage = "region maps/OoT/Dungeons/Shadow/Boat.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 691, 199, 40, "OOT Shadow Temple GS Near Boat", "Shadow Temple GS Near Boat"),
						M("Chest.png", 49, 447, 40, "OOT Shadow Temple Wind Room Hint", "Shadow Temple Wind Room Hint"),
						M("Chest.png", 527, 381, 40, "OOT Shadow Temple After Wind", "Shadow Temple After Wind"),
						M("Chest.png", 378, 384, 40, "OOT Shadow Temple After Wind Invisible", "Shadow Temple After Wind Invisible"),
						M("Fairy_Spot.png", 113, 420, 40, "OOT Shadow Temple Big Fairy After Wind", "Shadow Temple Big Fairy After Wind"),
						M("Pot.png", 468, 332, 24, "OOT Shadow Temple Pot After Wind 1", "Shadow Temple Pot After Wind 1"),
						M("Pot.png", 440, 415, 24, "OOT Shadow Temple Pot After Wind 2", "Shadow Temple Pot After Wind 2"),
						M("Pot.png", 550, 353, 24, "OOT Shadow Temple Flying Pot After Wind 1", "Shadow Temple Flying Pot After Wind 1"),
						M("Pot.png", 522, 436, 24, "OOT Shadow Temple Flying Pot After Wind 2", "Shadow Temple Flying Pot After Wind 2"),
						M("Heart.png", 742, 258, 24, "OOT Shadow Temple Heart Shortcut 1", "Shadow Temple Heart Shortcut 1"),
						M("Heart.png", 704, 250, 24, "OOT Shadow Temple Heart Shortcut 2", "Shadow Temple Heart Shortcut 2"),
						
						M("Gold_Skulltula.png", 86, 462, 40, "OOT MQ Shadow Temple GS Wind Hint", "MQ Shadow Temple GS Wind Hint"),
						M("Gold_Skulltula.png", 577, 345, 40, "OOT MQ Shadow Temple GS After Wind Bomb", "MQ Shadow Temple GS After Wind Bomb"),
						M("Chest.png", 631, 266, 40, "OOT MQ Shadow Temple Boat Passage Chest", "MQ Shadow Temple Boat Passage Chest"),
						M("Chest.png", 49, 447, 40, "OOT MQ Shadow Temple Wind Hint Chest", "MQ Shadow Temple Wind Hint Chest"),
						M("Chest.png", 378, 384, 40, "OOT MQ Shadow Temple After Wind Bomb Chest", "MQ Shadow Temple After Wind Bomb Chest"),
						M("Chest.png", 487, 373, 40, "OOT MQ Shadow Temple After Wind Gibdos Chest", "MQ Shadow Temple After Wind Gibdos Chest"),
						M("Fairy_Spot.png", 113, 420, 40, "OOT MQ Shadow Temple Big Fairy After Wind", "MQ Shadow Temple Big Fairy After Wind"),
						M("Pot.png", 468, 332, 24, "OOT MQ Shadow Temple Pot Room Before Boat 1", "MQ Shadow Temple Pot Room Before Boat 1"),
						M("Pot.png", 440, 415, 24, "OOT MQ Shadow Temple Pot Room Before Boat 2", "MQ Shadow Temple Pot Room Before Boat 2"),
						M("Pot.png", 550, 353, 24, "OOT MQ Shadow Temple Flying Pot Room Before Boat 1", "MQ Shadow Temple Flying Pot Room Before Boat 1"),
						M("Pot.png", 522, 436, 24, "OOT MQ Shadow Temple Flying Pot Room Before Boat 2", "MQ Shadow Temple Flying Pot Room Before Boat 2"),
						M("Heart.png", 742, 258, 24, "OOT MQ Shadow Temple Heart Shortcut 1", "MQ Shadow Temple Heart Shortcut 1"),
						M("Heart.png", 704, 250, 24, "OOT MQ Shadow Temple Heart Shortcut 2", "MQ Shadow Temple Heart Shortcut 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "After Boat Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Shadow/After_Boat.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Pot.png", 500, 153, 24, "OOT Shadow Temple Pot Boat After Bridge 1", "Shadow Temple Pot Boat After Bridge 1"),
						M("Pot.png", 416, 153, 24, "OOT Shadow Temple Pot Boat After Bridge 2", "Shadow Temple Pot Boat After Bridge 2"),
						M("Pot.png", 321, 440, 24, "OOT Shadow Temple Pot Boat Before Bridge 1", "Shadow Temple Pot Boat Before Bridge 1"),
						M("Pot.png", 250, 440, 24, "OOT Shadow Temple Pot Boat Before Bridge 2", "Shadow Temple Pot Boat Before Bridge 2"),
						M("Heart.png", 767, 185, 24, "OOT Shadow Temple Heart Pre-Boss 1", "Shadow Temple Heart Pre-Boss 1"),
						M("Heart.png", 761, 166, 24, "OOT Shadow Temple Heart Pre-Boss 2", "Shadow Temple Heart Pre-Boss 2"),
						M("Heart.png", 605, 154, 24, "OOT Shadow Temple Heart Pre-Boss 3", "Shadow Temple Heart Pre-Boss 3"),
						
						M("Gold_Skulltula.png", 177, 427, 40, "OOT MQ Shadow Temple GS After Boat", "MQ Shadow Temple GS After Boat"),
						M("Pot.png", 500, 153, 24, "OOT MQ Shadow Temple Pot After Boat After Bridge 1", "MQ Shadow Temple Pot After Boat After Bridge 1"),
						M("Pot.png", 416, 153, 24, "OOT MQ Shadow Temple Pot After Boat After Bridge 2", "MQ Shadow Temple Pot After Boat After Bridge 2"),
						M("Pot.png", 321, 440, 24, "OOT MQ Shadow Temple Pot After Boat Before Bridge 1", "MQ Shadow Temple Pot After Boat Before Bridge 1"),
						M("Pot.png", 250, 440, 24, "OOT MQ Shadow Temple Pot After Boat Before Bridge 2", "MQ Shadow Temple Pot After Boat Before Bridge 2"),
						M("Heart.png", 767, 185, 24, "OOT MQ Shadow Temple Heart Pre-Boss Upper 1", "MQ Shadow Temple Heart Pre-Boss Upper 1"),
						M("Heart.png", 761, 166, 24, "OOT MQ Shadow Temple Heart Pre-Boss Upper 2", "MQ Shadow Temple Heart Pre-Boss Upper 2"),
						M("Heart.png", 605, 154, 24, "OOT MQ Shadow Temple Heart Pre-Boss Lower", "MQ Shadow Temple Heart Pre-Boss Lower")
                    }
                },
				new MapSubRegion
                {
                    Name = "Maze Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Shadow/Maze.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 463, 105, 40, "OOT Shadow Temple GS Triple Skull Pot", "Shadow Temple GS Triple Skull Pot"),
						M("Chest.png", 140, 408, 40, "OOT Shadow Temple Invisible Floormaster", "Shadow Temple Invisible Floormaster"),
						M("Chest.png", 728, 329, 40, "OOT Shadow Temple Boss Key Room 1", "Shadow Temple Boss Key Room 1"),
						M("Chest.png", 772, 495, 40, "OOT Shadow Temple Boss Key Room 2", "Shadow Temple Boss Key Room 2"),
						M("Pot.png", 268, 361, 24, "OOT Shadow Temple Pot Invisible Floormaster Room 1", "Shadow Temple Pot Invisible Floormaster Room 1"),
						M("Pot.png", 249, 482, 24, "OOT Shadow Temple Pot Invisible Floormaster Room 2", "Shadow Temple Pot Invisible Floormaster Room 2"),
						M("Pot.png", 763, 416, 24, "OOT Shadow Temple Pot Boss Key Room", "Shadow Temple Pot Boss Key Room"),
						M("Wonder.png", 471, 188, 24, "OOT Shadow Temple Wonder Item", "Shadow Temple Wonder Item"),
						
						M("Collectible.png", 463, 131, 40, "OOT MQ Shadow Temple Triple Pot Key", "MQ Shadow Temple Triple Pot Key"),
						M("Chest.png", 140, 408, 40, "OOT MQ Shadow Temple Hidden Dead Hand Chest", "MQ Shadow Temple Hidden Dead Hand Chest"),
						M("Chest.png", 772, 495, 40, "OOT MQ Shadow Temple Boss Key Chest", "MQ Shadow Temple Boss Key Chest"),
						M("Chest.png", 728, 329, 40, "OOT MQ Shadow Temple Crushing Wall Left Chest", "MQ Shadow Temple Crushing Wall Left Chest"),
						M("Pot.png", 268, 361, 24, "OOT MQ Shadow Temple Pot Bomb Flowers Room 1", "MQ Shadow Temple Pot Bomb Flowers Room 1"),
						M("Pot.png", 249, 482, 24, "OOT MQ Shadow Temple Pot Bomb Flowers Room 2", "MQ Shadow Temple Pot Bomb Flowers Room 2"),
						M("Pot.png", 763, 416, 24, "OOT MQ Shadow Temple Pot Boss Key Room", "MQ Shadow Temple Pot Boss Key Room"),
						M("Wonder.png", 471, 188, 24, "OOT MQ Shadow Temple Wonder Item", "MQ Shadow Temple Wonder Item")
                    }
                },
				new MapSubRegion
                {
                    Name = "Pre-Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Shadow/Pre-Boss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 630, 443, 40, "OOT MQ Shadow Temple GS Pre-Boss", "MQ Shadow Temple GS Pre-Boss"),
						
						ME("Entrance.png", 373, 511, "Entrance shuffle (Bongo Bongo)", "OOT_BOSS_TEMPLE_SHADOW")
                    }
                },
				new MapSubRegion
                {
                    Name = "Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Shadow/Boss.png",
                    DestinationEntranceIds = new List<string> { "OOT_BOSS_TEMPLE_SHADOW" },
                    Marks = new List<MapMark>
                    {
						M("Collectible.png", 469, 307, 40, "OOT Shadow Temple Boss HC", "Shadow Temple Boss HC"),
						M("NPC.png", 469, 367, 40, "OOT Shadow Temple Boss", "Shadow Temple Boss")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion SpiritTemple()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Spirit Temple";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Entrance",
                    BackgroundImage = "region maps/OoT/Dungeons/Spirit/Entrance.png",
                    DestinationEntranceIds = new List<string> { "OOT_TEMPLE_SPIRIT" },
                    Marks = new List<MapMark>
                    {
						M("Pot.png", 648, 606, 24, "OOT Spirit Temple Pot Lobby 1", "Spirit Temple Pot Lobby 1"),
						M("Pot.png", 768, 606, 24, "OOT Spirit Temple Pot Lobby 2", "Spirit Temple Pot Lobby 2"),
						M("Pot.png", 667, 704, 24, "OOT Spirit Temple Flying Pot Lobby 1", "Spirit Temple Flying Pot Lobby 1"),
						M("Pot.png", 752, 704, 24, "OOT Spirit Temple Flying Pot Lobby 2", "Spirit Temple Flying Pot Lobby 2"),
						
						M("Chest.png", 646, 510, 40, "OOT MQ Spirit Temple Entrance Initial Chest", "MQ Spirit Temple Entrance Initial Chest"),
						M("Chest.png", 646, 427, 40, "OOT MQ Spirit Temple Lobby Back-Left Chest", "MQ Spirit Temple Lobby Back-Left Chest"),
						M("Chest.png", 755, 427, 40, "OOT MQ Spirit Temple Lobby Back-Right Chest", "MQ Spirit Temple Lobby Back-Right Chest"),
						M("Chest.png", 755, 510, 40, "OOT MQ Spirit Temple Lobby Front-Right Chest", "MQ Spirit Temple Lobby Front-Right Chest"),
						M("Pot.png", 606, 816, 24, "OOT MQ Spirit Temple Pot Entrance 1", "MQ Spirit Temple Pot Entrance 1"),
						M("Pot.png", 837, 791, 24, "OOT MQ Spirit Temple Pot Entrance 2", "MQ Spirit Temple Pot Entrance 2"),
						M("Pot.png", 580, 791, 24, "OOT MQ Spirit Temple Pot Entrance 3", "MQ Spirit Temple Pot Entrance 3"),
						M("Pot.png", 811, 816, 24, "OOT MQ Spirit Temple Pot Entrance 4", "MQ Spirit Temple Pot Entrance 4"),
						M("Silver_Rupee.png", 1132, 473, 24, "OOT MQ Spirit Temple SR Lobby After Water Near Stairs", "MQ Spirit Temple SR Lobby After Water Near Stairs"),
						M("Silver_Rupee.png", 1121, 265, 24, "OOT MQ Spirit Temple SR Lobby After Water Near Door", "MQ Spirit Temple SR Lobby After Water Near Door"),
						M("Silver_Rupee.png", 1017, 505, 24, "OOT MQ Spirit Temple SR Lobby In Water", "MQ Spirit Temple SR Lobby In Water"),
						M("Silver_Rupee.png", 777, 620, 24, "OOT MQ Spirit Temple SR Lobby Rock Right", "MQ Spirit Temple SR Lobby Rock Right"),
						M("Silver_Rupee.png", 637, 620, 24, "OOT MQ Spirit Temple SR Lobby Rock Left", "MQ Spirit Temple SR Lobby Rock Left"),
						
						ME("Entrance.png", 702, 845, "Entrance shuffle (Desert Colossus)", "OOT_DESERT_COLOSSUS_FROM_TEMPLE_SPIRIT")
                    }
                },
				new MapSubRegion
                {
                    Name = "Child Lobby",
                    BackgroundImage = "region maps/OoT/Dungeons/Spirit/Lobby_Child.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 702, 257, 40, "OOT Spirit Temple GS Child Fence", "Spirit Temple GS Child Fence"),
						M("Chest.png", 265, 170, 40, "OOT Spirit Temple Child First Chest", "Spirit Temple Child First Chest"),
						M("Chest.png", 681, 210, 40, "OOT Spirit Temple Child Second Chest", "Spirit Temple Child Second Chest"),
						M("Crate.png", 439, 300, 24, "OOT Spirit Temple Crate 1", "Spirit Temple Crate 1"),
						M("Crate.png", 500, 300, 24, "OOT Spirit Temple Crate 2", "Spirit Temple Crate 2"),
						M("Pot.png", 499, 85, 24, "OOT Spirit Temple Pot Child Anubis Room 1", "Spirit Temple Pot Child Anubis Room 1"),
						M("Pot.png", 519, 67, 24, "OOT Spirit Temple Pot Child Anubis Room 2", "Spirit Temple Pot Child Anubis Room 2"),
						M("Pot.png", 415, 148, 24, "OOT Spirit Temple Pot Child Anubis Room 3", "Spirit Temple Pot Child Anubis Room 3"),
						M("Pot.png", 472, 106, 24, "OOT Spirit Temple Pot Child Anubis Room 4", "Spirit Temple Pot Child Anubis Room 4"),
						M("Pot.png", 240, 172, 24, "OOT Spirit Temple Flying Pot Child After Bridge 1", "Spirit Temple Flying Pot Child After Bridge 1"),
						M("Pot.png", 306, 172, 24, "OOT Spirit Temple Flying Pot Child After Bridge 2", "Spirit Temple Flying Pot Child After Bridge 2"),
						M("Silver_Rupee.png", 579, 238, 24, "OOT Spirit Temple SR Child 1", "Spirit Temple SR Child 1"),
						M("Silver_Rupee.png", 603, 262, 24, "OOT Spirit Temple SR Child 2", "Spirit Temple SR Child 2"),
						M("Silver_Rupee.png", 659, 262, 24, "OOT Spirit Temple SR Child 3", "Spirit Temple SR Child 3"),
						M("Silver_Rupee.png", 724, 238, 24, "OOT Spirit Temple SR Child 4", "Spirit Temple SR Child 4"),
						M("Silver_Rupee.png", 695, 181, 24, "OOT Spirit Temple SR Child 5", "Spirit Temple SR Child 5"),
						
						M("Chest.png", 436, 581, 40, "OOT MQ Spirit Temple Paradox Chest", "MQ Spirit Temple Paradox Chest"),
						M("Chest.png", 281, 364, 40, "OOT MQ Spirit Temple Map Chest", "MQ Spirit Temple Map Chest"),
						M("Chest.png", 296, 230, 40, "OOT MQ Spirit Temple Map Room Back Chest", "MQ Spirit Temple Map Room Back Chest"),
						M("Pot.png", 525, 596, 24, "OOT MQ Spirit Temple Pot Child Entrance", "MQ Spirit Temple Pot Child Entrance"),
						M("Pot.png", 687, 208, 24, "OOT MQ Spirit Temple Pot Child Boulders 1", "MQ Spirit Temple Pot Child Boulders 1"),
						M("Pot.png", 691, 232, 24, "OOT MQ Spirit Temple Pot Child Boulders 2", "MQ Spirit Temple Pot Child Boulders 2"),
						M("Pot.png", 444, 44, 24, "OOT MQ Spirit Temple Pot Child Back 1", "MQ Spirit Temple Pot Child Back 1"),
						M("Pot.png", 504, 44, 24, "OOT MQ Spirit Temple Pot Child Back 2", "MQ Spirit Temple Pot Child Back 2"),
						M("Pot.png", 531, 64, 24, "OOT MQ Spirit Temple Pot Child Back 3", "MQ Spirit Temple Pot Child Back 3"),
						M("Pot.png", 416, 64, 24, "OOT MQ Spirit Temple Pot Child Back 4", "MQ Spirit Temple Pot Child Back 4"),
						M("Heart.png", 438, 443, 24, "OOT MQ Spirit Temple Heart 1", "MQ Spirit Temple Heart 1"),
						M("Heart.png", 498, 443, 24, "OOT MQ Spirit Temple Heart 2", "MQ Spirit Temple Heart 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Child Climb Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Spirit/Climb_Child.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 624, 324, 40, "OOT Spirit Temple GS Child Climb", "Spirit Temple GS Child Climb"),
						M("Chest.png", 328, 340, 40, "OOT Spirit Temple Child Climb 1", "Spirit Temple Child Climb 1"),
						M("Chest.png", 539, 336, 40, "OOT Spirit Temple Child Climb 2", "Spirit Temple Child Climb 2"),
						M("Pot.png", 620, 584, 24, "OOT Spirit Temple Pot Child Climb", "Spirit Temple Pot Child Climb"),
						
						M("Chest.png", 328, 330, 40, "OOT MQ Spirit Temple Child Upper Ground Chest", "MQ Spirit Temple Child Upper Ground Chest"),
						M("Chest.png", 318, 136, 40, "OOT MQ Spirit Temple Child Upper Ledge Chest", "MQ Spirit Temple Child Upper Ledge Chest"),
						M("Pot.png", 565, 584, 24, "OOT MQ Spirit Temple Pot Child Climb", "MQ Spirit Temple Pot Child Climb")
                    }
                },
				new MapSubRegion
                {
                    Name = "Adult Lobby",
                    BackgroundImage = "region maps/OoT/Dungeons/Spirit/Lobby_Adult.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 486, 340, 40, "OOT Spirit Temple GS Boulders", "Spirit Temple GS Boulders"),
						M("Chest.png", 571, 98, 40, "OOT Spirit Temple Adult Silver Rupees", "Spirit Temple Adult Silver Rupees"),
						M("Chest.png", 175, 243, 40, "OOT Spirit Temple Adult Lullaby", "Spirit Temple Adult Lullaby"),
						M("Fairy_Spot.png", 574, 144, 40, "OOT Spirit Temple Adult Silver Rupees Big Fairy", "Spirit Temple Adult Silver Rupees Big Fairy"),
						M("Silver_Rupee.png", 493, 259, 24, "OOT Spirit Temple SR Boulders 1", "Spirit Temple SR Boulders 1"),
						M("Silver_Rupee.png", 493, 444, 24, "OOT Spirit Temple SR Boulders 2", "Spirit Temple SR Boulders 2"),
						M("Silver_Rupee.png", 684, 395, 24, "OOT Spirit Temple SR Boulders 3", "Spirit Temple SR Boulders 3"),
						M("Silver_Rupee.png", 684, 303, 24, "OOT Spirit Temple SR Boulders 4", "Spirit Temple SR Boulders 4"),
						M("Silver_Rupee.png", 588, 404, 24, "OOT Spirit Temple SR Boulders 5", "Spirit Temple SR Boulders 5"),
						
						M("Gold_Skulltula.png", 227, 344, 40, "OOT MQ Spirit Temple GS Leever Room", "MQ Spirit Temple GS Leever Room"),
						M("Gold_Skulltula.png", 633, 140, 40, "OOT MQ Spirit Temple GS Symphony Room", "MQ Spirit Temple GS Symphony Room"),
						M("Chest.png", 175, 243, 40, "OOT MQ Spirit Temple Purple Leever Chest", "MQ Spirit Temple Purple Leever Chest"),
						M("Chest.png", 570, 62, 40, "OOT MQ Spirit Temple Symphony Room Chest", "MQ Spirit Temple Symphony Room Chest")
                    }
                },
				new MapSubRegion
                {
                    Name = "Adult Climb Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Spirit/Climb_Adult.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 335, 311, 40, "OOT Spirit Temple Adult Suns on Wall 1", "Spirit Temple Adult Suns on Wall 1"),
						M("Chest.png", 402, 311, 40, "OOT Spirit Temple Adult Suns on Wall 2", "Spirit Temple Adult Suns on Wall 2"),
						M("Pot.png", 633, 232, 24, "OOT Spirit Temple Pot Adult Upper", "Spirit Temple Pot Adult Upper"),
						M("Pot.png", 328, 516, 24, "OOT Spirit Temple Flying Pot Adult Climb 1", "Spirit Temple Flying Pot Adult Climb 1"),
						M("Pot.png", 278, 516, 24, "OOT Spirit Temple Flying Pot Adult Climb 2", "Spirit Temple Flying Pot Adult Climb 2"),
						
						M("Pot.png", 328, 516, 24, "OOT MQ Spirit Temple Pot Adult Climb 1", "MQ Spirit Temple Pot Adult Climb 1"),
						M("Pot.png", 278, 516, 24, "OOT MQ Spirit Temple Pot Adult Climb 2", "MQ Spirit Temple Pot Adult Climb 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Main Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Spirit/Main.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 116, 172, 40, "OOT Spirit Temple GS Statue", "Spirit Temple GS Statue"),
						M("Chest.png", 465, 487, 40, "OOT Spirit Temple Statue Base", "Spirit Temple Statue Base"),
						M("Chest.png", 257, 350, 40, "OOT Spirit Temple Statue Hands", "Spirit Temple Statue Hands"),
						M("Chest.png", 785, 212, 40, "OOT Spirit Temple Statue Upper Right", "Spirit Temple Statue Upper Right"),
						M("Pot.png", 155, 477, 24, "OOT Spirit Temple Pot Statue Room 1", "Spirit Temple Pot Statue Room 1"),
						M("Pot.png", 177, 494, 24, "OOT Spirit Temple Pot Statue Room 2", "Spirit Temple Pot Statue Room 2"),
						M("Pot.png", 199, 477, 24, "OOT Spirit Temple Pot Statue Room 3", "Spirit Temple Pot Statue Room 3"),
						M("Pot.png", 749, 487, 24, "OOT Spirit Temple Pot Statue Room 4", "Spirit Temple Pot Statue Room 4"),
						M("Pot.png", 771, 504, 24, "OOT Spirit Temple Pot Statue Room 5", "Spirit Temple Pot Statue Room 5"),
						M("Pot.png", 793, 487, 24, "OOT Spirit Temple Pot Statue Room 6", "Spirit Temple Pot Statue Room 6"),
						M("Pot.png", 299, 361, 24, "OOT Spirit Temple Flying Pot Statue Room 1", "Spirit Temple Flying Pot Statue Room 1"),
						M("Pot.png", 649, 361, 24, "OOT Spirit Temple Flying Pot Statue Room 2", "Spirit Temple Flying Pot Statue Room 2"),
						
						M("Chest.png", 465, 487, 40, "OOT MQ Spirit Temple Compass Chest", "MQ Spirit Temple Compass Chest"),
						M("Chest.png", 658, 348, 40, "OOT MQ Spirit Temple Chest in Box", "MQ Spirit Temple Chest in Box"),
						M("Chest.png", 785, 212, 40, "OOT MQ Spirit Temple Statue Room Ledge Chest", "MQ Spirit Temple Statue Room Ledge Chest"),
						M("Pot.png", 176, 484, 24, "OOT MQ Spirit Temple Pot Statue Room Lower 1", "MQ Spirit Temple Pot Statue Room Lower 1"),
						M("Pot.png", 748, 484, 24, "OOT MQ Spirit Temple Pot Statue Room Lower 2", "MQ Spirit Temple Pot Statue Room Lower 2"),
						M("Pot.png", 784, 484, 24, "OOT MQ Spirit Temple Pot Statue Room Lower 3", "MQ Spirit Temple Pot Statue Room Lower 3"),
						M("Pot.png", 189, 200, 24, "OOT MQ Spirit Temple Pot Statue Room Upper 1", "MQ Spirit Temple Pot Statue Room Upper 1"),
						M("Pot.png", 153, 200, 24, "OOT MQ Spirit Temple Pot Statue Room Upper 2", "MQ Spirit Temple Pot Statue Room Upper 2"),
						M("Pot.png", 766, 500, 24, "OOT MQ Spirit Temple Flying Pot Statue Room Lower", "MQ Spirit Temple Flying Pot Statue Room Lower"),
						M("Pot.png", 171, 216, 24, "OOT MQ Spirit Temple Flying Pot Statue Room Upper", "MQ Spirit Temple Flying Pot Statue Room Upper"),
						M("Pot.png", 341, 595, 24, "OOT MQ Spirit Temple Flying Pot Statue Room Stairs", "MQ Spirit Temple Flying Pot Statue Room Stairs"),
						M("Crate.png", 678, 386, 24, "OOT MQ Spirit Temple Giant Statue Room Large Crate 1", "MQ Spirit Temple Giant Statue Room Large Crate 1"),
						M("Crate.png", 654, 386, 24, "OOT MQ Spirit Temple Giant Statue Room Large Crate 2", "MQ Spirit Temple Giant Statue Room Large Crate 2"),
						M("Crate.png", 248, 567, 24, "OOT MQ Spirit Temple Giant Statue Room Small Crate", "MQ Spirit Temple Giant Statue Room Small Crate"),
						
						ME("Entrance.png", 470, 70, "Entrance shuffle (Twinrova)", "OOT_BOSS_TEMPLE_SPIRIT")
                    }
                },
				new MapSubRegion
                {
                    Name = "Elevator Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Spirit/Elevator.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 657, 370, 40, "OOT MQ Spirit Temple Silver Block Room Target Chest", "MQ Spirit Temple Silver Block Room Target Chest")
                    }
                },
				new MapSubRegion
                {
                    Name = "Sun Block Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Spirit/Sun_Block.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 518, 382, 40, "OOT Spirit Temple GS Iron Knuckle", "Spirit Temple GS Iron Knuckle"),
						M("Chest.png", 481, 256, 40, "OOT Spirit Temple Sun Block Room Torches", "Spirit Temple Sun Block Room Torches"),
						M("Pot.png", 690, 436, 24, "OOT Spirit Temple Pot Child Hand Stairway 1", "Spirit Temple Pot Child Hand Stairway 1"),
						M("Pot.png", 673, 370, 24, "OOT Spirit Temple Pot Child Hand Stairway 2", "Spirit Temple Pot Child Hand Stairway 2"),
						M("Silver_Rupee.png", 75, 467, 24, "OOT Spirit Temple SR Sun 1", "Spirit Temple SR Sun 1"),
						M("Silver_Rupee.png", 209, 325, 24, "OOT Spirit Temple SR Sun 2", "Spirit Temple SR Sun 2"),
						M("Silver_Rupee.png", 227, 264, 24, "OOT Spirit Temple SR Sun 3", "Spirit Temple SR Sun 3"),
						M("Silver_Rupee.png", 489, 218, 24, "OOT Spirit Temple SR Sun 4", "Spirit Temple SR Sun 4"),
						M("Silver_Rupee.png", 170, 186, 24, "OOT Spirit Temple SR Sun 5", "Spirit Temple SR Sun 5"),
						
						M("Gold_Skulltula.png", 481, 223, 40, "OOT MQ Spirit Temple GS Sun Block Room", "MQ Spirit Temple GS Sun Block Room"),
						M("Chest.png", 156, 195, 40, "OOT MQ Spirit Temple Sun Block Room Chest", "MQ Spirit Temple Sun Block Room Chest"),
						M("Pot.png", 491, 260, 24, "OOT MQ Spirit Temple Pot Child Sun Room 1", "MQ Spirit Temple Pot Child Sun Room 1"),
						M("Pot.png", 101, 385, 24, "OOT MQ Spirit Temple Pot Child Sun Room 2", "MQ Spirit Temple Pot Child Sun Room 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Anubis Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Spirit/Anubis.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 927, 362, 40, "OOT MQ Spirit Temple Beamos Room Chest", "MQ Spirit Temple Beamos Room Chest"),
						M("Crate.png", 717, 620, 24, "OOT MQ Spirit Temple Beamos Room Small Crate", "MQ Spirit Temple Beamos Room Small Crate")
                    }
                },
				new MapSubRegion
                {
                    Name = "Mirror Shiled Rooms",
                    BackgroundImage = "region maps/OoT/Dungeons/Spirit/Mirror_Shield.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 449, 574, 40, "OOT Spirit Temple Adult Late Sun on Wall", "Spirit Temple Adult Late Sun on Wall"),
						M("Chest.png", 467, 120, 40, "OOT Spirit Temple Adult Invisible 1", "Spirit Temple Adult Invisible 1"),
						M("Chest.png", 403, 120, 40, "OOT Spirit Temple Adult Invisible 2", "Spirit Temple Adult Invisible 2"),
						M("Fairy_Spot.png", 397, 402, 40, "OOT Spirit Temple Adult Sunlight Big Fairy", "Spirit Temple Adult Sunlight Big Fairy"),
						
						M("Chest.png", 449, 574, 40, "OOT MQ Spirit Temple Boss Key Chest", "MQ Spirit Temple Boss Key Chest"),
						M("Chest.png", 388, 307, 40, "OOT MQ Spirit Temple Dinolfos Room Chest", "MQ Spirit Temple Dinolfos Room Chest"),
						M("Fairy_Spot.png", 397, 402, 40, "OOT MQ Spirit Temple Dinolfos Room Big Fairy", "MQ Spirit Temple Dinolfos Room Big Fairy"),
						M("Wonder.png", 363, 300, 24, "OOT MQ Spirit Temple Wonder Item Chest Hammer", "MQ Spirit Temple Wonder Item Chest Hammer"),
						M("Wonder.png", 363, 330, 24, "OOT MQ Spirit Temple Wonder Item Chest Slash", "MQ Spirit Temple Wonder Item Chest Slash")
                    }
                },
				new MapSubRegion
                {
                    Name = "Boss Key Rooms",
                    BackgroundImage = "region maps/OoT/Dungeons/Spirit/Boss_Key.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 801, 242, 40, "OOT Spirit Temple Adult Boss Key Chest", "Spirit Temple Adult Boss Key Chest"),
						M("Heart.png", 102, 239, 24, "OOT Spirit Temple Heart 1", "Spirit Temple Heart 1"),
						M("Heart.png", 117, 222, 24, "OOT Spirit Temple Heart 2", "Spirit Temple Heart 2"),
						
						M("Gold_Skulltula.png", 642, 180, 40, "OOT MQ Spirit Temple GS Top Floor Left Wall", "MQ Spirit Temple GS Top Floor Left Wall"),
						M("Gold_Skulltula.png", 824, 224, 40, "OOT MQ Spirit Temple GS Top Floor Back Wall", "MQ Spirit Temple GS Top Floor Back Wall"),
						M("Pot.png", 152, 500, 24, "OOT MQ Spirit Temple Pot Topmost Climb 1", "MQ Spirit Temple Pot Topmost Climb 1"),
						M("Pot.png", 185, 434, 24, "OOT MQ Spirit Temple Pot Topmost Climb 2", "MQ Spirit Temple Pot Topmost Climb 2"),
						M("Pot.png", 524, 274, 24, "OOT MQ Spirit Temple Pot Top Near Triforce Symbol 1", "MQ Spirit Temple Pot Top Near Triforce Symbol 1"),
						M("Pot.png", 517, 231, 24, "OOT MQ Spirit Temple Pot Top Near Triforce Symbol 2", "MQ Spirit Temple Pot Top Near Triforce Symbol 2"),
						M("Silver_Rupee.png", 249, 444, 24, "OOT MQ Spirit Temple SR Adult Bottom", "MQ Spirit Temple SR Adult Bottom"),
						M("Silver_Rupee.png", 268, 389, 24, "OOT MQ Spirit Temple SR Adult Bottom-Center", "MQ Spirit Temple SR Adult Bottom-Center"),
						M("Silver_Rupee.png", 230, 389, 24, "OOT MQ Spirit Temple SR Adult Center-Top", "MQ Spirit Temple SR Adult Center-Top"),
						M("Silver_Rupee.png", 249, 334, 24, "OOT MQ Spirit Temple SR Adult Top", "MQ Spirit Temple SR Adult Top"),
						M("Silver_Rupee.png", 249, 389, 24, "OOT MQ Spirit Temple SR Adult Skulltula", "MQ Spirit Temple SR Adult Skulltula")
                    }
                },
				new MapSubRegion
                {
                    Name = "Topmost Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Spirit/Topmost.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 749, 391, 40, "OOT Spirit Temple Adult Topmost Sun on Wall", "Spirit Temple Adult Topmost Sun on Wall"),
						M("Pot.png", 373, 576, 24, "OOT Spirit Temple Flying Pot Topmost 1", "Spirit Temple Flying Pot Topmost 1"),
						M("Pot.png", 362, 357, 24, "OOT Spirit Temple Flying Pot Topmost 2", "Spirit Temple Flying Pot Topmost 2"),
						M("Pot.png", 256, 357, 24, "OOT Spirit Temple Flying Pot Topmost 3", "Spirit Temple Flying Pot Topmost 3"),
						M("Pot.png", 131, 576, 24, "OOT Spirit Temple Flying Pot Topmost 4", "Spirit Temple Flying Pot Topmost 4"),
						M("Pot.png", 411, 576, 24, "OOT Spirit Temple Flying Pot Topmost 5", "Spirit Temple Flying Pot Topmost 5"),
						M("Pot.png", 93, 576, 24, "OOT Spirit Temple Flying Pot Topmost 6", "Spirit Temple Flying Pot Topmost 6"),
						
						M("Chest.png", 318, 297, 40, "OOT MQ Spirit Temple Topmost Chest", "MQ Spirit Temple Topmost Chest"),
						M("Pot.png", 160, 390, 24, "OOT MQ Spirit Temple Pot Top Near Lowering Platform 1", "MQ Spirit Temple Pot Top Near Lowering Platform 1"),
						M("Pot.png", 414, 355, 24, "OOT MQ Spirit Temple Pot Top Near Lowering Platform 2", "MQ Spirit Temple Pot Top Near Lowering Platform 2"),
						M("Pot.png", 191, 355, 24, "OOT MQ Spirit Temple Pot Top Near Lowering Platform 3", "MQ Spirit Temple Pot Top Near Lowering Platform 3"),
						M("Pot.png", 412, 390, 24, "OOT MQ Spirit Temple Pot Top Near Lowering Platform 4", "MQ Spirit Temple Pot Top Near Lowering Platform 4"),
						M("Crate.png", 126, 543, 24, "OOT MQ Spirit Temple Top Near Lowering Platform Large Crate 1", "MQ Spirit Temple Top Near Lowering Platform Large Crate 1"),
						M("Crate.png", 398, 552, 24, "OOT MQ Spirit Temple Top Near Lowering Platform Large Crate 2", "MQ Spirit Temple Top Near Lowering Platform Large Crate 2"),
						M("Crate.png", 137, 519, 24, "OOT MQ Spirit Temple Top Near Lowering Platform Large Crate 3", "MQ Spirit Temple Top Near Lowering Platform Large Crate 3"),
						M("Crate.png", 402, 528, 24, "OOT MQ Spirit Temple Top Near Lowering Platform Large Crate 4", "MQ Spirit Temple Top Near Lowering Platform Large Crate 4")
                    }
                },
				new MapSubRegion
                {
                    Name = "Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Spirit/Boss.png",
                    DestinationEntranceIds = new List<string> { "OOT_BOSS_TEMPLE_SPIRIT" },
                    Marks = new List<MapMark>
                    {
						M("Collectible.png", 345, 395, 40, "OOT Spirit Temple Boss HC", "Spirit Temple Boss HC"),
						M("NPC.png", 609, 139, 40, "OOT Spirit Temple Boss", "Spirit Temple Boss")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion IceCavern()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Ice Cavern";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "First Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Ice/First_Room.png",
                    DestinationEntranceIds = new List<string> { "OOT_ICE_CAVERN" },
                    Marks = new List<MapMark>
                    {
						M("Fairy_Spot.png", 489, 328, 40, "OOT Ice Cavern Entrance Big Fairy", "Ice Cavern Entrance Big Fairy"),
						M("Red_Ice.png", 141, 556, 24, "OOT Ice Cavern Red Ice Entrance", "Ice Cavern Red Ice Entrance"),
						M("Red_Ice.png", 636, 306, 24, "OOT Ice Cavern Red Ice Freezard 1", "Ice Cavern Red Ice Freezard 1"),
						M("Red_Ice.png", 636, 364, 24, "OOT Ice Cavern Red Ice Freezard 2", "Ice Cavern Red Ice Freezard 2"),
						M("Icicle.png", 125, 534, 24, "OOT Ice Cavern Icicle Before Entrance 1", "Ice Cavern Icicle Before Entrance 1"),
						M("Icicle.png", 107, 556, 24, "OOT Ice Cavern Icicle Before Entrance 2", "Ice Cavern Icicle Before Entrance 2"),
						M("Icicle.png", 125, 578, 24, "OOT Ice Cavern Icicle Before Entrance 3", "Ice Cavern Icicle Before Entrance 3"),
						M("Icicle.png", 285, 304, 24, "OOT Ice Cavern Icicle Entrance 1", "Ice Cavern Icicle Entrance 1"),
						M("Icicle.png", 267, 326, 24, "OOT Ice Cavern Icicle Entrance 2", "Ice Cavern Icicle Entrance 2"),
						M("Icicle.png", 285, 348, 24, "OOT Ice Cavern Icicle Entrance 3", "Ice Cavern Icicle Entrance 3"),
						M("Icicle.png", 784, 204, 24, "OOT Ice Cavern Icicle Before Scythe 1", "Ice Cavern Icicle Before Scythe 1"),
						M("Icicle.png", 788, 220, 24, "OOT Ice Cavern Icicle Before Scythe 2", "Ice Cavern Icicle Before Scythe 2"),
						M("Icicle.png", 792, 236, 24, "OOT Ice Cavern Icicle Before Scythe 3", "Ice Cavern Icicle Before Scythe 3"),
						M("Icicle.png", 796, 252, 24, "OOT Ice Cavern Icicle Before Scythe 4", "Ice Cavern Icicle Before Scythe 4"),
						M("Rupee.png", 612, 295, 24, "OOT Ice Cavern Rupee Ice", "Ice Cavern Rupee Ice"),
						M("Pot.png", 773, 258, 24, "OOT Ice Cavern Pot First Corridor 1", "Ice Cavern Pot First Corridor 1"),
						M("Pot.png", 751, 258, 24, "OOT Ice Cavern Pot First Corridor 2", "Ice Cavern Pot First Corridor 2"),
						
						M("Icicle.png", 96, 508, 24, "OOT MQ Ice Cavern Icicle Entrance 1", "MQ Ice Cavern Icicle Entrance 1"),
						M("Icicle.png", 62, 503, 24, "OOT MQ Ice Cavern Icicle Entrance 2", "MQ Ice Cavern Icicle Entrance 2"),
						M("Icicle.png", 783, 235, 24, "OOT MQ Ice Cavern Icicle Before Main Room 1", "MQ Ice Cavern Icicle Before Main Room 1"),
						M("Icicle.png", 789, 259, 24, "OOT MQ Ice Cavern Icicle Before Main Room 2", "MQ Ice Cavern Icicle Before Main Room 2"),
						M("Icicle.png", 777, 211, 24, "OOT MQ Ice Cavern Icicle Before Main Room 3", "MQ Ice Cavern Icicle Before Main Room 3"),
						M("Icicle.png", 759, 239, 24, "OOT MQ Ice Cavern Icicle Before Main Room 4", "MQ Ice Cavern Icicle Before Main Room 4"),
						M("Icicle.png", 871, 294, 24, "OOT MQ Ice Cavern Icicle Before Main Room 5", "MQ Ice Cavern Icicle Before Main Room 5"),
						M("Pot.png", 81, 482, 24, "OOT MQ Ice Cavern Pot Entrance", "MQ Ice Cavern Pot Entrance"),
						M("Pot.png", 683, 349, 24, "OOT MQ Ice Cavern Pot First Room 1", "MQ Ice Cavern Pot First Room 1"),
						M("Pot.png", 661, 349, 24, "OOT MQ Ice Cavern Pot First Room 2", "MQ Ice Cavern Pot First Room 2"),
						
						ME("Entrance.png", 48, 532, "Entrance shuffle (Zora Fountain)", "OOT_ZORA_FOUNTAIN_FROM_ICE_CAVERN")
                    }
                },
				new MapSubRegion
                {
                    Name = "Central Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Ice/Central_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 188, 329, 40, "OOT Ice Cavern GS Scythe Room", "Ice Cavern GS Scythe Room"),
						M("Icicle.png", 854, 85, 24, "OOT Ice Cavern Icicle Before Map 1", "Ice Cavern Icicle Before Map 1"),
						M("Icicle.png", 886, 85, 24, "OOT Ice Cavern Icicle Before Map 2", "Ice Cavern Icicle Before Map 2"),
						M("Icicle.png", 918, 85, 24, "OOT Ice Cavern Icicle Before Map 3", "Ice Cavern Icicle Before Map 3"),
						M("Icicle.png", 219, 449, 24, "OOT Ice Cavern Icicle Scythe 1", "Ice Cavern Icicle Scythe 1"),
						M("Icicle.png", 253, 446, 24, "OOT Ice Cavern Icicle Scythe 2", "Ice Cavern Icicle Scythe 2"),
						M("Icicle.png", 270, 473, 24, "OOT Ice Cavern Icicle Scythe 3", "Ice Cavern Icicle Scythe 3"),
						M("Pot.png", 514, 226, 24, "OOT Ice Cavern Pot Scythe Room 1", "Ice Cavern Pot Scythe Room 1"),
						M("Pot.png", 656, 435, 24, "OOT Ice Cavern Pot Scythe Room 2", "Ice Cavern Pot Scythe Room 2"),
						M("Pot.png", 630, 468, 24, "OOT Ice Cavern Pot Scythe Room 3", "Ice Cavern Pot Scythe Room 3"),
						M("Pot.png", 321, 253, 24, "OOT Ice Cavern Flying Pot Scythe Room", "Ice Cavern Flying Pot Scythe Room"),
						M("Silver_Rupee.png", 435, 258, 24, "OOT Ice Cavern SR Scythe Back", "Ice Cavern SR Scythe Back"),
						M("Silver_Rupee.png", 399, 349, 24, "OOT Ice Cavern SR Scythe Center Left", "Ice Cavern SR Scythe Center Left"),
						M("Silver_Rupee.png", 513, 349, 24, "OOT Ice Cavern SR Scythe Center Right", "Ice Cavern SR Scythe Center Right"),
						M("Silver_Rupee.png", 240, 476, 24, "OOT Ice Cavern SR Scythe Left", "Ice Cavern SR Scythe Left"),
						M("Silver_Rupee.png", 515, 179, 24, "OOT Ice Cavern SR Scythe Midair", "Ice Cavern SR Scythe Midair"),
						
						M("Red_Ice.png", 200, 314, 24, "OOT MQ Ice Cavern Red Ice Main Room 1", "MQ Ice Cavern Red Ice Main Room 1"),
						M("Red_Ice.png", 210, 338, 24, "OOT MQ Ice Cavern Red Ice Main Room 2", "MQ Ice Cavern Red Ice Main Room 2"),
						M("Red_Ice.png", 186, 355, 24, "OOT MQ Ice Cavern Red Ice Main Room 3", "MQ Ice Cavern Red Ice Main Room 3"),
						M("Red_Ice.png", 560, 133, 24, "OOT MQ Ice Cavern Red Ice Main Room 4", "MQ Ice Cavern Red Ice Main Room 4"),
						M("Red_Ice.png", 584, 148, 24, "OOT MQ Ice Cavern Red Ice Main Room 5", "MQ Ice Cavern Red Ice Main Room 5"),
						M("Red_Ice.png", 608, 144, 24, "OOT MQ Ice Cavern Red Ice Main Room 6", "MQ Ice Cavern Red Ice Main Room 6"),
						M("Icicle.png", 688, 358, 24, "OOT MQ Ice Cavern Icicle Main Room 1", "MQ Ice Cavern Icicle Main Room 1"),
						M("Icicle.png", 672, 374, 24, "OOT MQ Ice Cavern Icicle Main Room 2", "MQ Ice Cavern Icicle Main Room 2"),
						M("Icicle.png", 656, 393, 24, "OOT MQ Ice Cavern Icicle Main Room 3", "MQ Ice Cavern Icicle Main Room 3"),
						M("Icicle.png", 672, 412, 24, "OOT MQ Ice Cavern Icicle Main Room 4", "MQ Ice Cavern Icicle Main Room 4"),
						M("Icicle.png", 688, 428, 24, "OOT MQ Ice Cavern Icicle Main Room 5", "MQ Ice Cavern Icicle Main Room 5"),
						M("Pot.png", 514, 225, 24, "OOT MQ Ice Cavern Pot Main Room 1", "MQ Ice Cavern Pot Main Room 1"),
						M("Pot.png", 641, 430, 24, "OOT MQ Ice Cavern Pot Main Room 2", "MQ Ice Cavern Pot Main Room 2"),
						M("Pot.png", 616, 466, 24, "OOT MQ Ice Cavern Pot Main Room 3", "MQ Ice Cavern Pot Main Room 3"),
						M("Pot.png", 359, 240, 24, "OOT MQ Ice Cavern Pot Main Room 4", "MQ Ice Cavern Pot Main Room 4")
                    }
                },
				new MapSubRegion
                {
                    Name = "Back Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Ice/Back_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 516, 102, 40, "OOT Ice Cavern Map", "Ice Cavern Map"),
						M("Red_Ice.png", 318, 182, 24, "OOT Ice Cavern Red Ice Map 1", "Ice Cavern Red Ice Map 1"),
						M("Red_Ice.png", 487, 113, 24, "OOT Ice Cavern Red Ice Map 2", "Ice Cavern Red Ice Map 2"),
						M("Pot.png", 292, 182, 24, "OOT Ice Cavern Pot Map Room", "Ice Cavern Pot Map Room"),
						M("Heart.png", 622, 402, 24,"OOT Ice Cavern Heart 1", "Ice Cavern Heart 1"),
						M("Heart.png", 609, 437, 24,"OOT Ice Cavern Heart 2", "Ice Cavern Heart 2"),
						M("Heart.png", 558, 427, 24,"OOT Ice Cavern Heart 3", "Ice Cavern Heart 3"),
						
						M("Gold_Skulltula.png", 285, 179, 40, "OOT MQ Ice Cavern GS Compass Room", "MQ Ice Cavern GS Compass Room"),
						M("Collectible.png", 580, 400, 40, "OOT MQ Ice Cavern Piece of Heart", "MQ Ice Cavern Piece of Heart"),
						M("Chest.png", 516, 102, 40, "OOT MQ Ice Cavern Compass Chest", "MQ Ice Cavern Compass Chest"),
						M("Red_Ice.png", 324, 184, 24, "OOT MQ Ice Cavern Red Ice Compass Room", "MQ Ice Cavern Red Ice Compass Room"),
						M("Icicle.png", 567, 294, 24, "OOT MQ Ice Cavern Icicle Compass Room 1", "MQ Ice Cavern Icicle Compass Room 1"),
						M("Icicle.png", 575, 318, 24, "OOT MQ Ice Cavern Icicle Compass Room 2", "MQ Ice Cavern Icicle Compass Room 2"),
						M("Icicle.png", 556, 358, 24, "OOT MQ Ice Cavern Icicle Compass Room 3", "MQ Ice Cavern Icicle Compass Room 3"),
						M("Icicle.png", 532, 364, 24, "OOT MQ Ice Cavern Icicle Compass Room 4", "MQ Ice Cavern Icicle Compass Room 4"),
						M("Pot.png", 732, 343, 24, "OOT MQ Ice Cavern Pot Compass Room 1", "MQ Ice Cavern Pot Compass Room 1"),
						M("Pot.png", 607, 239, 24, "OOT MQ Ice Cavern Pot Compass Room 2", "MQ Ice Cavern Pot Compass Room 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Right Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Ice/Right_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 325, 222, 40, "OOT Ice Cavern GS HP Room", "Ice Cavern GS HP Room"),
						M("Collectible.png", 439, 519, 40, "OOT Ice Cavern HP", "Ice Cavern HP"),
						M("Chest.png", 439, 292, 40, "OOT Ice Cavern Compass", "Ice Cavern Compass"),
						M("Red_Ice.png", 483, 300, 24, "OOT Ice Cavern Red Ice Compass 1", "Ice Cavern Red Ice Compass 1"),
						M("Red_Ice.png", 484, 525, 24, "OOT Ice Cavern Red Ice Compass 2", "Ice Cavern Red Ice Compass 2"),
						M("Icicle.png", 494, 327, 24, "OOT Ice Cavern Icicle Compass 01", "Ice Cavern Icicle Compass 01"),
						M("Icicle.png", 461, 327, 24, "OOT Ice Cavern Icicle Compass 02", "Ice Cavern Icicle Compass 02"),
						M("Icicle.png", 492, 428, 24, "OOT Ice Cavern Icicle Compass 03", "Ice Cavern Icicle Compass 03"),
						M("Icicle.png", 428, 327, 24, "OOT Ice Cavern Icicle Compass 04", "Ice Cavern Icicle Compass 04"),
						M("Icicle.png", 446, 428, 24, "OOT Ice Cavern Icicle Compass 05", "Ice Cavern Icicle Compass 05"),
						M("Icicle.png", 395, 327, 24, "OOT Ice Cavern Icicle Compass 06", "Ice Cavern Icicle Compass 06"),
						M("Icicle.png", 400, 428, 24, "OOT Ice Cavern Icicle Compass 07", "Ice Cavern Icicle Compass 07"),
						M("Icicle.png", 365, 337, 24, "OOT Ice Cavern Icicle Compass 08", "Ice Cavern Icicle Compass 08"),
						M("Icicle.png", 365, 369, 24, "OOT Ice Cavern Icicle Compass 09", "Ice Cavern Icicle Compass 09"),
						M("Icicle.png", 365, 401, 24, "OOT Ice Cavern Icicle Compass 10", "Ice Cavern Icicle Compass 10"),
						M("Icicle.png", 365, 353, 24, "OOT Ice Cavern Icicle Compass 11", "Ice Cavern Icicle Compass 11"),
						M("Icicle.png", 365, 385, 24, "OOT Ice Cavern Icicle Compass 12", "Ice Cavern Icicle Compass 12"),
						M("Icicle.png", 365, 417, 24, "OOT Ice Cavern Icicle Compass 13", "Ice Cavern Icicle Compass 13"),
						
						M("Chest.png", 482, 292, 40, "OOT MQ Ice Cavern Map Chest", "MQ Ice Cavern Map Chest"),
						M("Red_Ice.png", 455, 300, 24, "OOT MQ Ice Cavern Red Ice Map Room", "MQ Ice Cavern Red Ice Map Room"),
						M("Icicle.png", 368, 427, 24, "OOT MQ Ice Cavern Icicle Map Room 1", "MQ Ice Cavern Icicle Map Room 1"),
						M("Icicle.png", 312, 415, 24, "OOT MQ Ice Cavern Icicle Map Room 2", "MQ Ice Cavern Icicle Map Room 2"),
						M("Icicle.png", 324, 391, 24, "OOT MQ Ice Cavern Icicle Map Room 3", "MQ Ice Cavern Icicle Map Room 3"),
						M("Icicle.png", 336, 367, 24, "OOT MQ Ice Cavern Icicle Map Room 4", "MQ Ice Cavern Icicle Map Room 4"),
						M("Icicle.png", 336, 343, 24, "OOT MQ Ice Cavern Icicle Map Room 5", "MQ Ice Cavern Icicle Map Room 5"),
						M("Icicle.png", 324, 319, 24, "OOT MQ Ice Cavern Icicle Map Room 6", "MQ Ice Cavern Icicle Map Room 6"),
						M("Icicle.png", 312, 295, 24, "OOT MQ Ice Cavern Icicle Map Room 7", "MQ Ice Cavern Icicle Map Room 7"),
						M("Icicle.png", 600, 570, 24, "OOT MQ Ice Cavern Icicle Map Room 8", "MQ Ice Cavern Icicle Map Room 8"),
						M("Icicle.png", 571, 529, 24, "OOT MQ Ice Cavern Icicle Map Room 9", "MQ Ice Cavern Icicle Map Room 9")
                    }
                },
				new MapSubRegion
                {
                    Name = "Pre-Mini-Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Ice/Pre-Mini-Boss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 425, 405, 40, "OOT Ice Cavern GS Block Room", "Ice Cavern GS Block Room"),
						M("Red_Ice.png", 95, 238, 24, "OOT Ice Cavern Red Ice After Block 1", "Ice Cavern Red Ice After Block 1"),
						M("Red_Ice.png", 70, 223, 24, "OOT Ice Cavern Red Ice After Block 2", "Ice Cavern Red Ice After Block 2"),
						M("Red_Ice.png", 120, 253, 24, "OOT Ice Cavern Red Ice After Block 3", "Ice Cavern Red Ice After Block 3"),
						M("Red_Ice.png", 683, 300, 24, "OOT Ice Cavern Red Ice Block", "Ice Cavern Red Ice Block"),
						M("Icicle.png", 465, 478, 24, "OOT Ice Cavern Icicle Before Block 1", "Ice Cavern Icicle Before Block 1"),
						M("Icicle.png", 469, 494, 24, "OOT Ice Cavern Icicle Before Block 2", "Ice Cavern Icicle Before Block 2"),
						M("Icicle.png", 473, 510, 24, "OOT Ice Cavern Icicle Before Block 3", "Ice Cavern Icicle Before Block 3"),
						M("Icicle.png", 477, 526, 24, "OOT Ice Cavern Icicle Before Block 4", "Ice Cavern Icicle Before Block 4"),
						M("Icicle.png", 481, 542, 24, "OOT Ice Cavern Icicle Before Block 5", "Ice Cavern Icicle Before Block 5"),
						M("Icicle.png", 206, 207, 24, "OOT Ice Cavern Icicle After Block 1", "Ice Cavern Icicle After Block 1"),
						M("Icicle.png", 152, 185, 24, "OOT Ice Cavern Icicle After Block 2", "Ice Cavern Icicle After Block 2"),
						M("Icicle.png", 179, 201, 24, "OOT Ice Cavern Icicle After Block 3", "Ice Cavern Icicle After Block 3"),
						M("Icicle.png", 217, 234, 24, "OOT Ice Cavern Icicle After Block 4", "Ice Cavern Icicle After Block 4"),
						M("Icicle.png", 184, 226, 24, "OOT Ice Cavern Icicle After Block 5", "Ice Cavern Icicle After Block 5"),
						M("Icicle.png", 267, 275, 24, "OOT Ice Cavern Icicle After Block 6", "Ice Cavern Icicle After Block 6"),
						M("Icicle.png", 244, 291, 24, "OOT Ice Cavern Icicle After Block 7", "Ice Cavern Icicle After Block 7"),
						M("Icicle.png", 288, 301, 24, "OOT Ice Cavern Icicle After Block 8", "Ice Cavern Icicle After Block 8"),
						M("Pot.png", 120, 229, 24, "OOT Ice Cavern Pot Red Ice 1", "Ice Cavern Pot Red Ice 1"),
						M("Pot.png", 95, 214, 24, "OOT Ice Cavern Pot Red Ice 2", "Ice Cavern Pot Red Ice 2"),
						M("Rupee.png", 475, 155, 24, "OOT Ice Cavern Rupee Midair 1", "Ice Cavern Rupee Midair 1"),
						M("Rupee.png", 488, 178, 24, "OOT Ice Cavern Rupee Midair 2", "Ice Cavern Rupee Midair 2"),
						M("Rupee.png", 462, 178, 24, "OOT Ice Cavern Rupee Midair 3", "Ice Cavern Rupee Midair 3"),
						M("Silver_Rupee.png", 705, 313, 24, "OOT Ice Cavern SR Blocks Alcove", "Ice Cavern SR Blocks Alcove"),
						M("Silver_Rupee.png", 440, 175, 24, "OOT Ice Cavern SR Blocks Back Left", "Ice Cavern SR Blocks Back Left"),
						M("Silver_Rupee.png", 540, 209, 24, "OOT Ice Cavern SR Blocks Back Right", "Ice Cavern SR Blocks Back Right"),
						M("Silver_Rupee.png", 532, 271, 24, "OOT Ice Cavern SR Blocks Center", "Ice Cavern SR Blocks Center"),
						M("Silver_Rupee.png", 422, 338, 24, "OOT Ice Cavern SR Blocks Front Left", "Ice Cavern SR Blocks Front Left"),
						
						M("Gold_Skulltula.png", 717, 298, 40, "OOT MQ Ice Cavern GS Scarecrow", "MQ Ice Cavern GS Scarecrow"),
						M("Gold_Skulltula.png", 364, 243, 40, "OOT MQ Ice Cavern GS Clear Blocks", "MQ Ice Cavern GS Clear Blocks"),
						M("Red_Ice.png", 337, 229, 24, "OOT MQ Ice Cavern Red Ice Block Room 1", "MQ Ice Cavern Red Ice Block Room 1"),
						M("Red_Ice.png", 337, 249, 24, "OOT MQ Ice Cavern Red Ice Block Room 2", "MQ Ice Cavern Red Ice Block Room 2"),
						M("Red_Ice.png", 337, 269, 24, "OOT MQ Ice Cavern Red Ice Block Room 3", "MQ Ice Cavern Red Ice Block Room 3"),
						M("Icicle.png", 517, 533, 24, "OOT MQ Ice Cavern Icicle Before Map Room", "MQ Ice Cavern Icicle Before Map Room"),
						M("Pot.png", 106, 252, 24, "OOT MQ Ice Cavern Pot Final Corridor 1", "MQ Ice Cavern Pot Final Corridor 1"),
						M("Pot.png", 125, 235, 24, "OOT MQ Ice Cavern Pot Final Corridor 2", "MQ Ice Cavern Pot Final Corridor 2"),
                    }
                },
				new MapSubRegion
                {
                    Name = "Mini-Boss Room",
                    BackgroundImage = "region maps/OoT/Dungeons/Ice/Mini-Boss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 539, 327, 40, "OOT Ice Cavern Iron Boots", "Ice Cavern Iron Boots"),
						M("NPC.png", 467, 365, 40, "OOT Ice Cavern Sheik Song", "Ice Cavern Sheik Song"),
						
						M("Chest.png", 539, 327, 40, "OOT MQ Ice Cavern Iron Boots", "MQ Ice Cavern Iron Boots"),
						M("NPC.png", 467, 365, 40, "OOT MQ Ice Cavern Sheik Song", "MQ Ice Cavern Sheik Song")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion BottomoftheWell()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Bottom of the Well";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Main",
                    BackgroundImage = "region maps/OoT/Dungeons/Well/Main.png",
                    DestinationEntranceIds = new List<string> { "OOT_BOTTOM_OF_THE_WELL" },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 1178, 351, 40, "OOT Bottom of the Well GS East Cage", "Bottom of the Well GS East Cage"),
						M("Gold_Skulltula.png", 855, 209, 40, "OOT Bottom of the Well GS Inner East", "Bottom of the Well GS Inner East"),
						M("Gold_Skulltula.png", 722, 184, 40, "OOT Bottom of the Well GS Inner West", "Bottom of the Well GS Inner West"),
						M("Collectible.png", 99, 384, 40, "OOT Bottom of the Well Coffin", "Bottom of the Well Coffin"),
						M("Chest.png", 495, 125, 40, "OOT Bottom of the Well Back West", "Bottom of the Well Back West"),
						M("Chest.png", 922, 450, 40, "OOT Bottom of the Well Blood Chest", "Bottom of the Well Blood Chest"),
						M("Chest.png", 726, 450, 40, "OOT Bottom of the Well Compass", "Bottom of the Well Compass"),
						M("Chest.png", 967, 525, 40, "OOT Bottom of the Well East", "Bottom of the Well East"),
						M("Chest.png", 1145, 355, 40, "OOT Bottom of the Well East Cage", "Bottom of the Well East Cage"),
						M("Chest.png", 686, 523, 40, "OOT Bottom of the Well Front West", "Bottom of the Well Front West"),
						M("Chest.png", 1236, 651, 40, "OOT Bottom of the Well Lens", "Bottom of the Well Lens"),
						M("Chest.png", 1319, 651, 40, "OOT Bottom of the Well Lens Side Chest", "Bottom of the Well Lens Side Chest"),
						M("Chest.png", 1174, 298, 40, "OOT Bottom of the Well Pits", "Bottom of the Well Pits"),
						M("Chest.png", 785, 504, 40, "OOT Bottom of the Well Under Debris", "Bottom of the Well Under Debris"),
						M("Chest.png", 448, 360, 40, "OOT Bottom of the Well Underwater", "Bottom of the Well Underwater"),
						M("Chest.png", 803, 650, 40, "OOT Bottom of the Well Underwater 2", "Bottom of the Well Underwater 2"),
						M("Heart.png", 230, 392, 24, "OOT Bottom of the Well Heart 1", "Bottom of the Well Heart 1"),
						M("Heart.png", 173, 319, 24, "OOT Bottom of the Well Heart 2", "Bottom of the Well Heart 2"),
						M("Pot.png", 760, 216, 24, "OOT Bottom of the Well Flying Pot 1", "Bottom of the Well Flying Pot 1"),
						M("Pot.png", 698, 216, 24, "OOT Bottom of the Well Flying Pot 2", "Bottom of the Well Flying Pot 2"),
						M("Pot.png", 729, 216, 24, "OOT Bottom of the Well Flying Pot 3", "Bottom of the Well Flying Pot 3"),
						M("Pot.png", 534, 615, 24, "OOT Bottom of the Well Pot Main Room 1", "Bottom of the Well Pot Main Room 1"),
						M("Pot.png", 534, 591, 24, "OOT Bottom of the Well Pot Main Room 2", "Bottom of the Well Pot Main Room 2"),
						M("Pot.png", 534, 567, 24, "OOT Bottom of the Well Pot Main Room 3", "Bottom of the Well Pot Main Room 3"),
						M("Pot.png", 809, 704, 24, "OOT Bottom of the Well Pot Main Room 4", "Bottom of the Well Pot Main Room 4"),
						M("Pot.png", 852, 704, 24, "OOT Bottom of the Well Pot Main Room 5", "Bottom of the Well Pot Main Room 5"),
						M("Pot.png", 829, 110, 24, "OOT Bottom of the Well Pot Main Room Underwater", "Bottom of the Well Pot Main Room Underwater"),
						M("Pot.png", 1093, 205, 24, "OOT Bottom of the Well Pot Side Room", "Bottom of the Well Pot Side Room"),
						
						M("Gold_Skulltula.png", 45, 300, 40, "OOT MQ Bottom of the Well GS Coffin Room", "MQ Bottom of the Well GS Coffin Room"),
						M("Gold_Skulltula.png", 720, 240, 40, "OOT MQ Bottom of the Well GS West Middle Room", "MQ Bottom of the Well GS West Middle Room"),
						M("Collectible.png", 1324, 570, 40, "OOT MQ Bottom of the Well Dead Hand Key", "MQ Bottom of the Well Dead Hand Key"),
						M("Collectible.png", 860, 225, 40, "OOT MQ Bottom of the Well East Middle Room Key", "MQ Bottom of the Well East Middle Room Key"),
						M("Fairy_Spot.png", 922, 435, 40, "OOT MQ Bottom of the Well Lobby Cage Big Fairy", "MQ Bottom of the Well Lobby Cage Big Fairy"),
						M("Chest.png", 821, 323, 40, "OOT MQ Bottom of the Well Map Chest", "MQ Bottom of the Well Map Chest"),
						M("Chest.png", 1236, 651, 40, "OOT MQ Bottom of the Well Compass Chest", "MQ Bottom of the Well Compass Chest"),
						M("Heart.png", 782, 507, 24, "OOT MQ Bottom of the Well Heart Main Room 1", "MQ Bottom of the Well Heart Main Room 1"),
						M("Heart.png", 782, 531, 24, "OOT MQ Bottom of the Well Heart Main Room 2", "MQ Bottom of the Well Heart Main Room 2"),
						M("Heart.png", 235, 320, 24, "OOT MQ Bottom of the Well Heart Coffin 1", "MQ Bottom of the Well Heart Coffin 1"),
						M("Heart.png", 168, 392, 24, "OOT MQ Bottom of the Well Heart Coffin 2", "MQ Bottom of the Well Heart Coffin 2"),
						M("Grass.png", 1281, 556, 24, "OOT MQ Bottom of the Well Grass Dead-Hand 1", "MQ Bottom of the Well Grass Dead-Hand 1"),
						M("Grass.png", 1290, 582, 24, "OOT MQ Bottom of the Well Grass Dead-Hand 2", "MQ Bottom of the Well Grass Dead-Hand 2"),
						M("Grass.png", 1307, 605, 24, "OOT MQ Bottom of the Well Grass Dead-Hand 3", "MQ Bottom of the Well Grass Dead-Hand 3"),
						M("Grass.png", 1332, 612, 24, "OOT MQ Bottom of the Well Grass Dead-Hand 4", "MQ Bottom of the Well Grass Dead-Hand 4"),
						M("Pot.png", 968, 533, 24, "OOT MQ Bottom of the Well Pot Lobby Alcove", "MQ Bottom of the Well Pot Lobby Alcove"),
						M("Pot.png", 929, 476, 24, "OOT MQ Bottom of the Well Pot Lobby Cage 1", "MQ Bottom of the Well Pot Lobby Cage 1"),
						M("Pot.png", 957, 476, 24, "OOT MQ Bottom of the Well Pot Lobby Cage 2", "MQ Bottom of the Well Pot Lobby Cage 2"),
						M("Pot.png", 901, 476, 24, "OOT MQ Bottom of the Well Pot Lobby Cage 3", "MQ Bottom of the Well Pot Lobby Cage 3"),
						M("Pot.png", 912, 220, 24, "OOT MQ Bottom of the Well Pot Side Room 1", "MQ Bottom of the Well Pot Side Room 1"),
						M("Pot.png", 970, 220, 24, "OOT MQ Bottom of the Well Pot Side Room 2", "MQ Bottom of the Well Pot Side Room 2"),
						M("Pot.png", 970, 263, 24, "OOT MQ Bottom of the Well Pot Side Room 3", "MQ Bottom of the Well Pot Side Room 3"),
						M("Wonder.png", 641, 97, 24, "OOT MQ Bottom of the Well Wonder Item Main Room Left 1", "MQ Bottom of the Well Wonder Item Main Room Left 1"),
						M("Wonder.png", 641, 117, 24, "OOT MQ Bottom of the Well Wonder Item Main Room Left 2", "MQ Bottom of the Well Wonder Item Main Room Left 2"),
						M("Wonder.png", 659, 107, 24, "OOT MQ Bottom of the Well Wonder Item Main Room Left 3", "MQ Bottom of the Well Wonder Item Main Room Left 3"),
						M("Wonder.png", 623, 107, 24, "OOT MQ Bottom of the Well Wonder Item Main Room Left 4", "MQ Bottom of the Well Wonder Item Main Room Left 4"),
						M("Wonder.png", 1028, 100, 24, "OOT MQ Bottom of the Well Wonder Item Main Room Right 1", "MQ Bottom of the Well Wonder Item Main Room Right 1"),
						M("Wonder.png", 1046, 110, 24, "OOT MQ Bottom of the Well Wonder Item Main Room Right 2", "MQ Bottom of the Well Wonder Item Main Room Right 2"),
						M("Wonder.png", 1028, 120, 24, "OOT MQ Bottom of the Well Wonder Item Main Room Right 3", "MQ Bottom of the Well Wonder Item Main Room Right 3"),
						M("Wonder.png", 1010, 110, 24, "OOT MQ Bottom of the Well Wonder Item Main Room Right 4", "MQ Bottom of the Well Wonder Item Main Room Right 4"),
						M("Wonder.png", 881, 183, 24, "OOT MQ Bottom of the Well Wonder Item Side Room 1", "MQ Bottom of the Well Wonder Item Side Room 1"),
						M("Wonder.png", 899, 193, 24, "OOT MQ Bottom of the Well Wonder Item Side Room 2", "MQ Bottom of the Well Wonder Item Side Room 2"),
						M("Wonder.png", 863, 193, 24, "OOT MQ Bottom of the Well Wonder Item Side Room 3", "MQ Bottom of the Well Wonder Item Side Room 3"),
						M("Wonder.png", 881, 203, 24, "OOT MQ Bottom of the Well Wonder Item Side Room 4", "MQ Bottom of the Well Wonder Item Side Room 4"),
						
						ME("Entrance.png", 824, 832, "Entrance shuffle (Kakariko Village)", "OOT_KAKARIKO_FROM_BOTTOM_OF_THE_WELL")
                    }
                },
				new MapSubRegion
                {
                    Name = "Underground",
                    BackgroundImage = "region maps/OoT/Dungeons/Well/Underground.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Fairy_Spot.png", 849, 198, 40, "OOT Bottom of the Well Basement Big Fairy", "Bottom of the Well Basement Big Fairy"),
						M("Chest.png", 864, 663, 40, "OOT Bottom of the Well Map", "Bottom of the Well Map"),
						M("Grass.png", 407, 239, 24, "OOT Bottom of the Well Grass 01", "Bottom of the Well Grass 01"),
						M("Grass.png", 408, 255, 24, "OOT Bottom of the Well Grass 02", "Bottom of the Well Grass 02"),
						M("Grass.png", 411, 271, 24, "OOT Bottom of the Well Grass 03", "Bottom of the Well Grass 03"),
						M("Grass.png", 417, 322, 24, "OOT Bottom of the Well Grass 04", "Bottom of the Well Grass 04"),
						M("Grass.png", 423, 284, 24, "OOT Bottom of the Well Grass 05", "Bottom of the Well Grass 05"),
						M("Grass.png", 429, 232, 24, "OOT Bottom of the Well Grass 06", "Bottom of the Well Grass 06"),
						M("Grass.png", 431, 252, 24, "OOT Bottom of the Well Grass 07", "Bottom of the Well Grass 07"),
						M("Grass.png", 431, 354, 24, "OOT Bottom of the Well Grass 08", "Bottom of the Well Grass 08"),
						M("Grass.png", 436, 269, 24, "OOT Bottom of the Well Grass 09", "Bottom of the Well Grass 09"),
						M("Grass.png", 498, 468, 24, "OOT Bottom of the Well Grass 10", "Bottom of the Well Grass 10"),
						M("Grass.png", 515, 451, 24, "OOT Bottom of the Well Grass 11", "Bottom of the Well Grass 11"),
						M("Grass.png", 518, 432, 24, "OOT Bottom of the Well Grass 12", "Bottom of the Well Grass 12"),
						M("Pot.png", 640, 445, 24, "OOT Bottom of the Well Pot Basement 01", "Bottom of the Well Pot Basement 01"),
						M("Pot.png", 695, 467, 24, "OOT Bottom of the Well Pot Basement 02", "Bottom of the Well Pot Basement 02"),
						M("Pot.png", 746, 306, 24, "OOT Bottom of the Well Pot Basement 03", "Bottom of the Well Pot Basement 03"),
						M("Pot.png", 821, 288, 24, "OOT Bottom of the Well Pot Basement 04", "Bottom of the Well Pot Basement 04"),
						M("Pot.png", 790, 330, 24, "OOT Bottom of the Well Pot Basement 05", "Bottom of the Well Pot Basement 05"),
						M("Pot.png", 865, 288, 24, "OOT Bottom of the Well Pot Basement 06", "Bottom of the Well Pot Basement 06"),
						M("Pot.png", 821, 240, 24, "OOT Bottom of the Well Pot Basement 07", "Bottom of the Well Pot Basement 07"),
						M("Pot.png", 821, 264, 24, "OOT Bottom of the Well Pot Basement 08", "Bottom of the Well Pot Basement 08"),
						M("Pot.png", 768, 330, 24, "OOT Bottom of the Well Pot Basement 09", "Bottom of the Well Pot Basement 09"),
						M("Pot.png", 843, 288, 24, "OOT Bottom of the Well Pot Basement 10", "Bottom of the Well Pot Basement 10"),
						M("Pot.png", 746, 330, 24, "OOT Bottom of the Well Pot Basement 11", "Bottom of the Well Pot Basement 11"),
						M("Pot.png", 746, 282, 24, "OOT Bottom of the Well Pot Basement 12", "Bottom of the Well Pot Basement 12"),
						M("Rupee.png", 682, 408, 24, "OOT Bottom of the Well Rupee 1", "Bottom of the Well Rupee 1"),
						M("Rupee.png", 702, 416, 24, "OOT Bottom of the Well Rupee 2", "Bottom of the Well Rupee 2"),
						M("Rupee.png", 694, 442, 24, "OOT Bottom of the Well Rupee 3", "Bottom of the Well Rupee 3"),
						M("Rupee.png", 654, 426, 24, "OOT Bottom of the Well Rupee 4", "Bottom of the Well Rupee 4"),
						M("Rupee.png", 662, 400, 24, "OOT Bottom of the Well Rupee 5", "Bottom of the Well Rupee 5"),
						M("Silver_Rupee.png", 385, 604, 24, "OOT Bottom of the Well SR 1", "Bottom of the Well SR 1"),
						M("Silver_Rupee.png", 481, 554, 24, "OOT Bottom of the Well SR 2", "Bottom of the Well SR 2"),
						M("Silver_Rupee.png", 511, 554, 24, "OOT Bottom of the Well SR 3", "Bottom of the Well SR 3"),
						M("Silver_Rupee.png", 557, 524, 24, "OOT Bottom of the Well SR 4", "Bottom of the Well SR 4"),
						M("Silver_Rupee.png", 598, 574, 24, "OOT Bottom of the Well SR 5", "Bottom of the Well SR 5"),
						
						M("Gold_Skulltula.png", 408, 198, 40, "OOT MQ Bottom of the Well GS Basement", "MQ Bottom of the Well GS Basement"),
						M("Fairy_Spot.png", 849, 198, 40, "OOT MQ Bottom of the Well Basement Big Fairy", "MQ Bottom of the Well Basement Big Fairy"),
						M("Chest.png", 864, 663, 40, "OOT MQ Bottom of the Well Lens Chest", "MQ Bottom of the Well Lens Chest"),
						M("Heart.png", 976, 320, 24, "OOT MQ Bottom of the Well Heart Basement 1", "MQ Bottom of the Well Heart Basement 1"),
						M("Heart.png", 960, 300, 24, "OOT MQ Bottom of the Well Heart Basement 2", "MQ Bottom of the Well Heart Basement 2"),
						M("Heart.png", 992, 300, 24, "OOT MQ Bottom of the Well Heart Basement 3", "MQ Bottom of the Well Heart Basement 3")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion GerudoTrainingGround()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Gerudo Training Ground";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Entrance",
                    BackgroundImage = "region maps/OoT/Dungeons/GTG/Entrance.png",
                    DestinationEntranceIds = new List<string> { "OOT_GERUDO_TRAINING_GROUNDS" },
                    Marks = new List<MapMark>
                    {
						M("Fairy_Spot.png", 464, 225, 40, "OOT Gerudo Training Grounds Entrance Big Fairy", "Gerudo Training Grounds Entrance Big Fairy"),
						M("Chest.png", 535, 234, 40, "OOT Gerudo Training Grounds Entrance 1", "Gerudo Training Grounds Entrance 1"),
						M("Chest.png", 395, 234, 40, "OOT Gerudo Training Grounds Entrance 2", "Gerudo Training Grounds Entrance 2"),
						
						M("Chest.png", 395, 234, 40, "OOT MQ Gerudo Training Grounds Entryway Left Chest", "MQ Gerudo Training Grounds Entryway Left Chest"),
						M("Chest.png", 535, 234, 40, "OOT MQ Gerudo Training Grounds Entryway Right Chest", "MQ Gerudo Training Grounds Entryway Right Chest"),
						M("Pot.png", 369, 314, 24, "OOT MQ Gerudo Training Grounds Pot 1", "MQ Gerudo Training Grounds Pot 1"),
						M("Pot.png", 365, 346, 24, "OOT MQ Gerudo Training Grounds Pot 2", "MQ Gerudo Training Grounds Pot 2"),
						M("Pot.png", 575, 314, 24, "OOT MQ Gerudo Training Grounds Pot 3", "MQ Gerudo Training Grounds Pot 3"),
						M("Pot.png", 579, 346, 24, "OOT MQ Gerudo Training Grounds Pot 4", "MQ Gerudo Training Grounds Pot 4"),
						
						ME("Entrance.png", 463, 493, "Entrance shuffle (Gerudo Fortress)", "OOT_GERUDO_FORTRESS_FROM_GERUDO_TRAINING")
                    }
                },
				new MapSubRegion
                {
                    Name = "Stalfos Room",
                    BackgroundImage = "region maps/OoT/Dungeons/GTG/Stalfos.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 409, 251, 40, "OOT Gerudo Training Grounds Stalfos", "Gerudo Training Grounds Stalfos"),
						
						M("Chest.png", 409, 251, 40, "OOT MQ Gerudo Training Grounds Left Side Iron Knuckle Chest", "MQ Gerudo Training Grounds Left Side Iron Knuckle Chest")
                    }
                },
				new MapSubRegion
                {
                    Name = "Boulders Room",
                    BackgroundImage = "region maps/OoT/Dungeons/GTG/Boulders.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Silver_Rupee.png", 367, 160, 24, "OOT Gerudo Training Grounds SR Slope Back", "Gerudo Training Grounds SR Slope Back"),
						M("Silver_Rupee.png", 442, 284, 24, "OOT Gerudo Training Grounds SR Slope Center", "Gerudo Training Grounds SR Slope Center"),
						M("Silver_Rupee.png", 461, 552, 24, "OOT Gerudo Training Grounds SR Slope Front Above", "Gerudo Training Grounds SR Slope Front Above"),
						M("Silver_Rupee.png", 335, 496, 24, "OOT Gerudo Training Grounds SR Slope Front Left", "Gerudo Training Grounds SR Slope Front Left"),
						M("Silver_Rupee.png", 556, 485, 24, "OOT Gerudo Training Grounds SR Slope Front Right", "Gerudo Training Grounds SR Slope Front Right"),
						
						M("Silver_Rupee.png", 553, 147, 24, "OOT MQ Gerudo Training Grounds SR Slopes Top Right", "MQ Gerudo Training Grounds SR Slopes Top Right"),
						M("Silver_Rupee.png", 477, 345, 24, "OOT MQ Gerudo Training Grounds SR Slopes Middle", "MQ Gerudo Training Grounds SR Slopes Middle"),
						M("Silver_Rupee.png", 504, 494, 24, "OOT MQ Gerudo Training Grounds SR Slopes Front", "MQ Gerudo Training Grounds SR Slopes Front"),
						M("Silver_Rupee.png", 337, 494, 24, "OOT MQ Gerudo Training Grounds SR Slopes Front-Left", "MQ Gerudo Training Grounds SR Slopes Front-Left"),
						M("Silver_Rupee.png", 584, 507, 24, "OOT MQ Gerudo Training Grounds SR Slopes Front-Right", "MQ Gerudo Training Grounds SR Slopes Front-Right"),
						M("Icicle.png", 579, 133, 24, "OOT MQ Gerudo Training Grounds Icicle Slopes 1", "MQ Gerudo Training Grounds Icicle Slopes 1"),
						M("Icicle.png", 585, 157, 24, "OOT MQ Gerudo Training Grounds Icicle Slopes 2", "MQ Gerudo Training Grounds Icicle Slopes 2"),
						M("Icicle.png", 603, 140, 24, "OOT MQ Gerudo Training Grounds Icicle Slopes 3", "MQ Gerudo Training Grounds Icicle Slopes 3"),
						M("Icicle.png", 541, 161, 24, "OOT MQ Gerudo Training Grounds Icicle Slopes 4", "MQ Gerudo Training Grounds Icicle Slopes 4"),
						M("Icicle.png", 565, 168, 24, "OOT MQ Gerudo Training Grounds Icicle Slopes 5", "MQ Gerudo Training Grounds Icicle Slopes 5")
                    }
                },
				new MapSubRegion
                {
                    Name = "Wolfos Room",
                    BackgroundImage = "region maps/OoT/Dungeons/GTG/Wolfos.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 439, 233, 40, "OOT Gerudo Training Grounds Near Block", "Gerudo Training Grounds Near Block"),
						
						M("Chest.png", 439, 233, 40, "OOT MQ Gerudo Training Grounds Stalfos Room Chest", "MQ Gerudo Training Grounds Stalfos Room Chest")
                    }
                },
				new MapSubRegion
                {
                    Name = "Like-Likes Room",
                    BackgroundImage = "region maps/OoT/Dungeons/GTG/Like_Like.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 376, 285, 40, "OOT Gerudo Training Grounds Behind Block Enemy Back", "Gerudo Training Grounds Behind Block Enemy Back"),
						M("Chest.png", 581, 358, 40, "OOT Gerudo Training Grounds Behind Block Enemy Front", "Gerudo Training Grounds Behind Block Enemy Front"),
						M("Chest.png", 578, 246, 40, "OOT Gerudo Training Grounds Behind Block Invisible", "Gerudo Training Grounds Behind Block Invisible"),
						M("Chest.png", 404, 178, 40, "OOT Gerudo Training Grounds Behind Block Visible", "Gerudo Training Grounds Behind Block Visible"),
						
						M("Chest.png", 404, 178, 40, "OOT MQ Gerudo Training Grounds Silver Block Room Chest", "MQ Gerudo Training Grounds Silver Block Room Chest")
                    }
                },
				new MapSubRegion
                {
                    Name = "Statue Room",
                    BackgroundImage = "region maps/OoT/Dungeons/GTG/Statue.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 409, 363, 40, "OOT Gerudo Training Grounds Eye Statue", "Gerudo Training Grounds Eye Statue"),
						M("Wonder.png", 473, 323, 24, "OOT Gerudo Training Grounds Wonder Item Eye Statue Room", "Gerudo Training Grounds Wonder Item Eye Statue Room"),
						
						M("Chest.png", 452, 559, 40, "OOT MQ Gerudo Training Grounds Spinning Statue Chest", "MQ Gerudo Training Grounds Spinning Statue Chest"),
						M("Wonder.png", 473, 323, 24, "OOT MQ Gerudo Training Grounds Wonder Item Eye Statue", "MQ Gerudo Training Grounds Wonder Item Eye Statue")
                    }
                },
				new MapSubRegion
                {
                    Name = "Flamming Room",
                    BackgroundImage = "region maps/OoT/Dungeons/GTG/Flamming.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 226, 367, 40, "OOT Gerudo Training Grounds Hammer Room", "Gerudo Training Grounds Hammer Room"),
						M("Chest.png", 421, 300, 40, "OOT Gerudo Training Grounds Hammer Room Switch", "Gerudo Training Grounds Hammer Room Switch"),
						M("Wonder.png", 608, 167, 24, "OOT Gerudo Training Grounds Wonder Item Torch Slugs Room", "Gerudo Training Grounds Wonder Item Torch Slugs Room"),
						
						M("Chest.png", 226, 367, 40, "OOT MQ Gerudo Training Grounds Torch Slug Room Clear Chest", "MQ Gerudo Training Grounds Torch Slug Room Clear Chest"),
						M("Chest.png", 421, 300, 40, "OOT MQ Gerudo Training Grounds Torch Slug Room Switch Chest", "MQ Gerudo Training Grounds Torch Slug Room Switch Chest")
                    }
                },
				new MapSubRegion
                {
                    Name = "Lava Room",
                    BackgroundImage = "region maps/OoT/Dungeons/GTG/Lava.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Collectible.png", 417, 177, 40, "OOT Gerudo Training Freestanding Key", "Gerudo Training Freestanding Key"),
						M("Silver_Rupee.png", 801, 217, 24, "OOT Gerudo Training Grounds SR Lava Back Center", "Gerudo Training Grounds SR Lava Back Center"),
						M("Silver_Rupee.png", 568, 266, 24, "OOT Gerudo Training Grounds SR Lava Back Left", "Gerudo Training Grounds SR Lava Back Left"),
						M("Silver_Rupee.png", 607, 382, 24, "OOT Gerudo Training Grounds SR Lava Back Right", "Gerudo Training Grounds SR Lava Back Right"),
						M("Silver_Rupee.png", 289, 309, 24, "OOT Gerudo Training Grounds SR Lava Front Left", "Gerudo Training Grounds SR Lava Front Left"),
						M("Silver_Rupee.png", 337, 383, 24, "OOT Gerudo Training Grounds SR Lava Front Right", "Gerudo Training Grounds SR Lava Front Right"),
						
						M("Silver_Rupee.png", 608, 382, 24, "OOT MQ Gerudo Training Grounds SR Lava Back-Right", "MQ Gerudo Training Grounds SR Lava Back-Right"),
						M("Silver_Rupee.png", 569, 266, 24, "OOT MQ Gerudo Training Grounds SR Lava Back-Left", "MQ Gerudo Training Grounds SR Lava Back-Left"),
						M("Silver_Rupee.png", 490, 332, 24, "OOT MQ Gerudo Training Grounds SR Lava Center", "MQ Gerudo Training Grounds SR Lava Center"),
						M("Silver_Rupee.png", 338, 382, 24, "OOT MQ Gerudo Training Grounds SR Lava Front-Right", "MQ Gerudo Training Grounds SR Lava Front-Right"),
						M("Silver_Rupee.png", 361, 255, 24, "OOT MQ Gerudo Training Grounds SR Lava Front-Left", "MQ Gerudo Training Grounds SR Lava Front-Left"),
						M("Silver_Rupee.png", 289, 308, 24, "OOT MQ Gerudo Training Grounds SR Lava Front", "MQ Gerudo Training Grounds SR Lava Front")
                    }
                },
				new MapSubRegion
                {
                    Name = "Water Room",
                    BackgroundImage = "region maps/OoT/Dungeons/GTG/Water.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 514, 136, 40, "OOT Gerudo Training Water", "Gerudo Training Water"),
						M("Silver_Rupee.png", 454, 494, 24, "OOT Gerudo Training Grounds SR Water 1", "Gerudo Training Grounds SR Water 1"),
						M("Silver_Rupee.png", 443, 426, 24, "OOT Gerudo Training Grounds SR Water 2", "Gerudo Training Grounds SR Water 2"),
						M("Silver_Rupee.png", 531, 527, 24, "OOT Gerudo Training Grounds SR Water 3", "Gerudo Training Grounds SR Water 3"),
						M("Silver_Rupee.png", 457, 383, 24, "OOT Gerudo Training Grounds SR Water 4", "Gerudo Training Grounds SR Water 4"),
						M("Silver_Rupee.png", 389, 441, 24, "OOT Gerudo Training Grounds SR Water 5", "Gerudo Training Grounds SR Water 5"),
						
						M("Chest.png", 514, 136, 40, "OOT MQ Gerudo Training Grounds Water Room Chest", "MQ Gerudo Training Grounds Water Room Chest"),
						M("Silver_Rupee.png", 380, 426, 24, "OOT MQ Gerudo Training Grounds SR Water Top-Left", "MQ Gerudo Training Grounds SR Water Top-Left"),
						M("Silver_Rupee.png", 452, 426, 24, "OOT MQ Gerudo Training Grounds SR Water Center", "MQ Gerudo Training Grounds SR Water Center"),
						M("Silver_Rupee.png", 540, 562, 24, "OOT MQ Gerudo Training Grounds SR Water Bottom-Right", "MQ Gerudo Training Grounds SR Water Bottom-Right")
                    }
                },
				new MapSubRegion
                {
                    Name = "Dinolfos Room",
                    BackgroundImage = "region maps/OoT/Dungeons/GTG/Dinalfos.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 508, 282, 40, "OOT Gerudo Training Grounds Lizalfos", "Gerudo Training Grounds Lizalfos"),
						M("Wonder.png", 315, 167, 24, "OOT Gerudo Training Grounds Wonder Item Beamos Room", "Gerudo Training Grounds Wonder Item Beamos Room"),
						M("Heart.png", 755, 333, 24, "OOT Gerudo Training Grounds Heart 1", "Gerudo Training Grounds Heart 1"),
						M("Heart.png", 592, 203, 24, "OOT Gerudo Training Grounds Heart 2", "Gerudo Training Grounds Heart 2"),
						
						M("Chest.png", 508, 282, 40, "OOT MQ Gerudo Training Grounds Right Side Dinolfos Chest", "MQ Gerudo Training Grounds Right Side Dinolfos Chest"),
						M("Wonder.png", 315, 167, 24, "OOT MQ Gerudo Training Grounds Wonder Item Dodongo Room Wall", "MQ Gerudo Training Grounds Wonder Item Dodongo Room Wall")
                    }
                },
				new MapSubRegion
                {
                    Name = "Maze",
                    BackgroundImage = "region maps/OoT/Dungeons/GTG/Maze.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 438, 207, 40, "OOT Gerudo Training Maze Chest 1", "Gerudo Training Maze Chest 1"),
						M("Chest.png", 633, 231, 40, "OOT Gerudo Training Maze Chest 2", "Gerudo Training Maze Chest 2"),
						M("Chest.png", 629, 258, 40, "OOT Gerudo Training Maze Chest 3", "Gerudo Training Maze Chest 3"),
						M("Chest.png", 454, 300, 40, "OOT Gerudo Training Maze Chest 4", "Gerudo Training Maze Chest 4"),
						M("Chest.png", 401, 353, 40, "OOT Gerudo Training Maze Side Chest 1", "Gerudo Training Maze Side Chest 1"),
						M("Chest.png", 504, 409, 40, "OOT Gerudo Training Maze Side Chest 2", "Gerudo Training Maze Side Chest 2"),
						M("Chest.png", 450, 156, 40, "OOT Gerudo Training Maze Upper Cage", "Gerudo Training Maze Upper Cage"),
						M("Chest.png", 266, 161, 40, "OOT Gerudo Training Maze Upper Fake Ceiling", "Gerudo Training Maze Upper Fake Ceiling"),
						
						M("Chest.png", 266, 161, 40, "OOT MQ Gerudo Training Grounds Maze First Chest", "MQ Gerudo Training Grounds Maze First Chest"),
						M("Chest.png", 438, 207, 40, "OOT MQ Gerudo Training Grounds Maze Second Chest", "MQ Gerudo Training Grounds Maze Second Chest"),
						M("Chest.png", 633, 231, 40, "OOT MQ Gerudo Training Grounds Maze Third Chest", "MQ Gerudo Training Grounds Maze Third Chest"),
						M("Chest.png", 629, 258, 40, "OOT MQ Gerudo Training Grounds Maze Fourth Chest", "MQ Gerudo Training Grounds Maze Fourth Chest"),			
						M("Chest.png", 401, 353, 40, "OOT MQ Gerudo Training Grounds Maze Right Side Middle Chest", "MQ Gerudo Training Grounds Maze Right Side Middle Chest"),
						M("Chest.png", 504, 409, 40, "OOT MQ Gerudo Training Grounds Maze Right Side Right Chest", "MQ Gerudo Training Grounds Maze Right Side Right Chest"),						
						M("Chest.png", 650, 143, 40, "OOT MQ Gerudo Training Grounds Ice Arrows Chest", "MQ Gerudo Training Grounds Ice Arrows Chest"),
						M("Crate.png", 463, 311, 24, "OOT MQ Gerudo Training Grounds Maze Center Room Large Crate", "MQ Gerudo Training Grounds Maze Center Room Large Crate")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion InsideGanonCastle()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Inside Ganon Castle";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Lobby",
                    BackgroundImage = "region maps/OoT/Ganon/Lobby.png",
                    DestinationEntranceIds = new List<string>
					{
						"OOT_GANON_CASTLE",
						"OOT_GANON_CASTLE_FROM_TOWER"
					},
                    Marks = new List<MapMark>
                    {
						M("Fairy.png", 82, 452, 24, "OOT Ganon Castle Fairy Fountain Fairy 1", "Ganon Castle Fairy Fountain Fairy 1"),
						M("Fairy.png", 82, 472, 24, "OOT Ganon Castle Fairy Fountain Fairy 2", "Ganon Castle Fairy Fountain Fairy 2"),
						M("Fairy.png", 61, 463, 24, "OOT Ganon Castle Fairy Fountain Fairy 3", "Ganon Castle Fairy Fountain Fairy 3"),
						M("Fairy.png", 63, 443, 24, "OOT Ganon Castle Fairy Fountain Fairy 4", "Ganon Castle Fairy Fountain Fairy 4"),
						M("Fairy.png", 78, 428, 24, "OOT Ganon Castle Fairy Fountain Fairy 5", "Ganon Castle Fairy Fountain Fairy 5"),
						M("Fairy.png", 95, 436, 24, "OOT Ganon Castle Fairy Fountain Fairy 6", "Ganon Castle Fairy Fountain Fairy 6"),
						M("Fairy.png", 104, 454, 24, "OOT Ganon Castle Fairy Fountain Fairy 7", "Ganon Castle Fairy Fountain Fairy 7"),
						M("Fairy.png", 101, 471, 24, "OOT Ganon Castle Fairy Fountain Fairy 8", "Ganon Castle Fairy Fountain Fairy 8"),
						M("Scrub.png", 23, 471, 40, "OOT Ganon Castle Left-Center Scrub", "Ganon Castle Left-Center Scrub"),
						M("Scrub.png", 59, 504, 40, "OOT Ganon Castle Leftmost Scrub", "Ganon Castle Leftmost Scrub"),
						M("Scrub.png", 23, 423, 40, "OOT Ganon Castle Right-Center Scrub", "Ganon Castle Right-Center Scrub"),
						M("Scrub.png", 59, 389, 40, "OOT Ganon Castle Rightmost Scrub", "Ganon Castle Rightmost Scrub"),
						
						M("Fairy.png", 82, 452, 24, "OOT MQ Ganon Castle Fairy Fountain Fairy 1", "MQ Ganon Castle Fairy Fountain Fairy 1"),
						M("Fairy.png", 82, 472, 24, "OOT MQ Ganon Castle Fairy Fountain Fairy 2", "MQ Ganon Castle Fairy Fountain Fairy 2"),
						M("Fairy.png", 61, 463, 24, "OOT MQ Ganon Castle Fairy Fountain Fairy 3", "MQ Ganon Castle Fairy Fountain Fairy 3"),
						M("Fairy.png", 63, 443, 24, "OOT MQ Ganon Castle Fairy Fountain Fairy 4", "MQ Ganon Castle Fairy Fountain Fairy 4"),
						M("Fairy.png", 78, 428, 24, "OOT MQ Ganon Castle Fairy Fountain Fairy 5", "MQ Ganon Castle Fairy Fountain Fairy 5"),
						M("Fairy.png", 95, 436, 24, "OOT MQ Ganon Castle Fairy Fountain Fairy 6", "MQ Ganon Castle Fairy Fountain Fairy 6"),
						M("Fairy.png", 104, 454, 24, "OOT MQ Ganon Castle Fairy Fountain Fairy 7", "MQ Ganon Castle Fairy Fountain Fairy 7"),
						M("Fairy.png", 101, 471, 24, "OOT MQ Ganon Castle Fairy Fountain Fairy 8", "MQ Ganon Castle Fairy Fountain Fairy 8"),
						M("Scrub.png", 23, 423, 40, "OOT MQ Ganon Castle Center Scrub", "MQ Ganon Castle Center Scrub"),
						M("Scrub.png", 23, 471, 40, "OOT MQ Ganon Castle Left-Center Scrub", "MQ Ganon Castle Left-Center Scrub"),
						M("Scrub.png", 59, 504, 40, "OOT MQ Ganon Castle Leftmost Scrub", "MQ Ganon Castle Leftmost Scrub"),
						M("Scrub.png", 59, 389, 40, "OOT MQ Ganon Castle Right-Center Scrub", "MQ Ganon Castle Right-Center Scrub"),
						M("Scrub.png", 105, 389, 40, "OOT MQ Ganon Castle Rightmost Scrub", "MQ Ganon Castle Rightmost Scrub"),
						
						ME("Entrance.png", 397, 435, "Entrance shuffle (Ganon Tower)", "OOT_GANON_TOWER"),
						ME("Entrance.png", 160, 437, "Entrance shuffle (Outside Ganon Castle)", "OOT_GANON_CASTLE_EXTERIOR_FROM_CASTLE")
                    }
                },
				new MapSubRegion
                {
                    Name = "Forest Trial",
                    BackgroundImage = "region maps/OoT/Ganon/Forest.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 1148, 490, 40, "OOT Ganon Castle Forest Chest", "Ganon Castle Forest Chest"),
						M("Pot.png", 325, 460, 24, "OOT Ganon Castle Pot Forest End 1", "Ganon Castle Pot Forest End 1"),
						M("Pot.png", 325, 545, 24, "OOT Ganon Castle Pot Forest End 2", "Ganon Castle Pot Forest End 2"),
						M("Silver_Rupee.png", 474, 503, 24, "OOT Ganon Castle SR Forest Back Middle", "Ganon Castle SR Forest Back Middle"),
						M("Silver_Rupee.png", 410, 412, 24, "OOT Ganon Castle SR Forest Back Right", "Ganon Castle SR Forest Back Right"),
						M("Silver_Rupee.png", 647, 604, 24, "OOT Ganon Castle SR Forest Center Left", "Ganon Castle SR Forest Center Left"),
						M("Silver_Rupee.png", 647, 403, 24, "OOT Ganon Castle SR Forest Center Right", "Ganon Castle SR Forest Center Right"),
						M("Silver_Rupee.png", 841, 572, 24, "OOT Ganon Castle SR Forest Front", "Ganon Castle SR Forest Front"),
						
						M("Chest.png", 873, 525, 40, "OOT MQ Ganon Castle Forest Trial First Chest", "MQ Ganon Castle Forest Trial First Chest"),
						M("Chest.png", 618, 606, 40, "OOT MQ Ganon Castle Forest Trial Second Chest", "MQ Ganon Castle Forest Trial Second Chest"),
						M("Collectible.png", 938, 524, 40, "OOT MQ Ganon Castle Forest Trial Key", "MQ Ganon Castle Forest Trial Key"),
						M("Pot.png", 325, 545, 24, "OOT MQ Ganon Pot Forest End 1", "MQ Ganon Pot Forest End 1"),
						M("Pot.png", 325, 460, 24, "OOT MQ Ganon Pot Forest End 2", "MQ Ganon Pot Forest End 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Fire Trial",
                    BackgroundImage = "region maps/OoT/Ganon/Fire.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Heart.png", 735, 650, 24, "OOT Ganon Castle Heart Fire", "Ganon Castle Heart Fire"),
						M("Pot.png", 336, 535, 24, "OOT Ganon Castle Pot Fire End 1", "Ganon Castle Pot Fire End 1"),
						M("Pot.png", 336, 453, 24, "OOT Ganon Castle Pot Fire End 2", "Ganon Castle Pot Fire End 2"),
						M("Silver_Rupee.png", 659, 159, 24, "OOT Ganon Castle SR Fire Back Right", "Ganon Castle SR Fire Back Right"),
						M("Silver_Rupee.png", 791, 717, 24, "OOT Ganon Castle SR Fire Black Pillar", "Ganon Castle SR Fire Black Pillar"),
						M("Silver_Rupee.png", 889, 116, 24, "OOT Ganon Castle SR Fire Far Right", "Ganon Castle SR Fire Far Right"),
						M("Silver_Rupee.png", 1114, 267, 24, "OOT Ganon Castle SR Fire Front Right", "Ganon Castle SR Fire Front Right"),
						M("Silver_Rupee.png", 1019, 638, 24, "OOT Ganon Castle SR Fire Left", "Ganon Castle SR Fire Left"),
						
						M("Pot.png", 336, 453, 24, "OOT MQ Ganon Pot Fire End 1", "MQ Ganon Pot Fire End 1"),
						M("Pot.png", 336, 535, 24, "OOT MQ Ganon Pot Fire End 2", "MQ Ganon Pot Fire End 2"),
						M("Silver_Rupee.png", 506, 654, 24, "OOT MQ Ganon Castle SR Fire Back-Left", "MQ Ganon Castle SR Fire Back-Left"),
						M("Silver_Rupee.png", 791, 717, 24, "OOT MQ Ganon Castle SR Fire Center-Left", "MQ Ganon Castle SR Fire Center-Left"),
						M("Silver_Rupee.png", 1069, 717, 24, "OOT MQ Ganon Castle SR Fire Front-Left", "MQ Ganon Castle SR Fire Front-Left"),
						M("Silver_Rupee.png", 1000, 210, 24, "OOT MQ Ganon Castle SR Fire High Above Lava", "MQ Ganon Castle SR Fire High Above Lava"),
						M("Silver_Rupee.png", 609, 161, 24, "OOT MQ Ganon Castle SR Fire Under Pillar", "MQ Ganon Castle SR Fire Under Pillar")
                    }
                },
				new MapSubRegion
                {
                    Name = "Water Trial",
                    BackgroundImage = "region maps/OoT/Ganon/Water.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 676, 235, 40, "OOT Ganon Castle Water Chest 1", "Ganon Castle Water Chest 1"),
						M("Chest.png", 676, 380, 40, "OOT Ganon Castle Water Chest 2", "Ganon Castle Water Chest 2"),
						M("Pot.png", 220, 345, 24, "OOT Ganon Castle Pot Water End 1", "Ganon Castle Pot Water End 1"),
						M("Pot.png", 220, 295, 24, "OOT Ganon Castle Pot Water End 2", "Ganon Castle Pot Water End 2"),
						M("Pot.png", 441, 225, 24, "OOT Ganon Castle Pot Water", "Ganon Castle Pot Water"),
						M("Icicle.png", 766, 290, 24, "OOT Ganon Castle Icicle Water 01", "Ganon Castle Icicle Water 01"),
						M("Icicle.png", 802, 302, 24, "OOT Ganon Castle Icicle Water 02", "Ganon Castle Icicle Water 02"),
						M("Icicle.png", 742, 314, 24, "OOT Ganon Castle Icicle Water 03", "Ganon Castle Icicle Water 03"),
						M("Icicle.png", 766, 338, 24, "OOT Ganon Castle Icicle Water 04", "Ganon Castle Icicle Water 04"),
						M("Icicle.png", 790, 338, 24, "OOT Ganon Castle Icicle Water 05", "Ganon Castle Icicle Water 05"),
						M("Icicle.png", 802, 326, 24, "OOT Ganon Castle Icicle Water 06", "Ganon Castle Icicle Water 06"),
						M("Icicle.png", 754, 302, 24, "OOT Ganon Castle Icicle Water 07", "Ganon Castle Icicle Water 07"),
						M("Icicle.png", 790, 290, 24, "OOT Ganon Castle Icicle Water 08", "Ganon Castle Icicle Water 08"),
						M("Icicle.png", 754, 326, 24, "OOT Ganon Castle Icicle Water 09", "Ganon Castle Icicle Water 09"),
						M("Icicle.png", 814, 314, 24, "OOT Ganon Castle Icicle Water 10", "Ganon Castle Icicle Water 10"),
						M("Icicle.png", 715, 405, 24, "OOT Ganon Castle Icicle Water 11", "Ganon Castle Icicle Water 11"),
						M("Icicle.png", 652, 361, 24, "OOT Ganon Castle Icicle Water 12", "Ganon Castle Icicle Water 12"),
						M("Icicle.png", 715, 225, 24, "OOT Ganon Castle Icicle Water 13", "Ganon Castle Icicle Water 13"),
						M("Icicle.png", 652, 266, 24, "OOT Ganon Castle Icicle Water 14", "Ganon Castle Icicle Water 14"),
						
						M("Chest.png", 764, 402, 40, "OOT MQ Ganon Castle Water Trial Chest", "MQ Ganon Castle Water Trial Chest"),
						M("Heart.png", 771, 225, 24, "OOT MQ Ganon Castle Heart Water", "MQ Ganon Castle Heart Water"),
						M("Pot.png", 220, 345, 24, "OOT MQ Ganon Pot Water End 1", "MQ Ganon Pot Water End 1"),
						M("Pot.png", 220, 295, 24, "OOT MQ Ganon Pot Water End 2", "MQ Ganon Pot Water End 2"),
						M("Icicle.png", 705, 242, 24, "OOT MQ Ganon Castle Icicle Water 1", "MQ Ganon Castle Icicle Water 1"),
						M("Icicle.png", 729, 236, 24, "OOT MQ Ganon Castle Icicle Water 2", "MQ Ganon Castle Icicle Water 2"),
						M("Icicle.png", 739, 212, 24, "OOT MQ Ganon Castle Icicle Water 3", "MQ Ganon Castle Icicle Water 3"),
						M("Icicle.png", 681, 236, 24, "OOT MQ Ganon Castle Icicle Water 4", "MQ Ganon Castle Icicle Water 4"),
						M("Icicle.png", 705, 387, 24, "OOT MQ Ganon Castle Icicle Water 5", "MQ Ganon Castle Icicle Water 5"),
						M("Icicle.png", 681, 393, 24, "OOT MQ Ganon Castle Icicle Water 6", "MQ Ganon Castle Icicle Water 6"),
						M("Icicle.png", 739, 417, 24, "OOT MQ Ganon Castle Icicle Water 7", "MQ Ganon Castle Icicle Water 7"),
						M("Icicle.png", 729, 393, 24, "OOT MQ Ganon Castle Icicle Water 8", "MQ Ganon Castle Icicle Water 8"),
						M("Red_Ice.png", 408, 496, 24, "OOT MQ Ganon Castle Red Ice Block Silver Rupee Room 1", "MQ Ganon Castle Red Ice Block Silver Rupee Room 1"),
						M("Red_Ice.png", 272, 357, 24, "OOT MQ Ganon Castle Red Ice Block Silver Rupee Room 2", "MQ Ganon Castle Red Ice Block Silver Rupee Room 2"),
						M("Red_Ice.png", 272, 337, 24, "OOT MQ Ganon Castle Red Ice Block Silver Rupee Room 3", "MQ Ganon Castle Red Ice Block Silver Rupee Room 3"),
						M("Red_Ice.png", 272, 317, 24, "OOT MQ Ganon Castle Red Ice Block Silver Rupee Room 4", "MQ Ganon Castle Red Ice Block Silver Rupee Room 4"),
						M("Red_Ice.png", 272, 297, 24, "OOT MQ Ganon Castle Red Ice Block Silver Rupee Room 5", "MQ Ganon Castle Red Ice Block Silver Rupee Room 5"),
						M("Red_Ice.png", 272, 277, 24, "OOT MQ Ganon Castle Red Ice Block Silver Rupee Room 6", "MQ Ganon Castle Red Ice Block Silver Rupee Room 6"),
						M("Red_Ice.png", 806, 409, 24, "OOT MQ Ganon Castle Red Ice Water 1", "MQ Ganon Castle Red Ice Water 1"),
						M("Red_Ice.png", 806, 216, 24, "OOT MQ Ganon Castle Red Ice Water 2", "MQ Ganon Castle Red Ice Water 2"),
						M("Red_Ice.png", 666, 372, 24, "OOT MQ Ganon Castle Red Ice Water 3", "MQ Ganon Castle Red Ice Water 3"),
						M("Red_Ice.png", 639, 351, 24, "OOT MQ Ganon Castle Red Ice Water 4", "MQ Ganon Castle Red Ice Water 4"),
						M("Red_Ice.png", 647, 331, 24, "OOT MQ Ganon Castle Red Ice Water 5", "MQ Ganon Castle Red Ice Water 5"),
						M("Red_Ice.png", 647, 311, 24, "OOT MQ Ganon Castle Red Ice Water 6", "MQ Ganon Castle Red Ice Water 6"),
						M("Red_Ice.png", 639, 291, 24, "OOT MQ Ganon Castle Red Ice Water 7", "MQ Ganon Castle Red Ice Water 7"),
						M("Silver_Rupee.png", 417, 364, 24, "OOT MQ Ganon Castle SR Water Above Ground", "MQ Ganon Castle SR Water Above Ground"),
						M("Silver_Rupee.png", 408, 521, 24, "OOT MQ Ganon Castle SR Water Alcove", "MQ Ganon Castle SR Water Alcove"),
						M("Silver_Rupee.png", 314, 259, 24, "OOT MQ Ganon Castle SR Water Deep Hole", "MQ Ganon Castle SR Water Deep Hole"),
						M("Silver_Rupee.png", 454, 300, 24, "OOT MQ Ganon Castle SR Water Shallow Hole", "MQ Ganon Castle SR Water Shallow Hole"),
						M("Silver_Rupee.png", 413, 447, 24, "OOT MQ Ganon Castle SR Water Under Alcove", "MQ Ganon Castle SR Water Under Alcove")
                    }
                },
				new MapSubRegion
                {
                    Name = "Shadow Trial",
                    BackgroundImage = "region maps/OoT/Ganon/Shadow.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 1210, 595, 40, "OOT Ganon Castle Shadow Chest 1", "Ganon Castle Shadow Chest 1"),
						M("Chest.png", 651, 412, 40, "OOT Ganon Castle Shadow Chest 2", "Ganon Castle Shadow Chest 2"),
						M("Pot.png", 284, 501, 24, "OOT Ganon Castle Pot Shadow End 1", "Ganon Castle Pot Shadow End 1"),
						M("Pot.png", 284, 437, 24, "OOT Ganon Castle Pot Shadow End 2", "Ganon Castle Pot Shadow End 2"),
						M("Pot.png", 821, 538, 24, "OOT Ganon Castle Pot Shadow 1", "Ganon Castle Pot Shadow 1"),
						M("Pot.png", 821, 410, 24, "OOT Ganon Castle Pot Shadow 2", "Ganon Castle Pot Shadow 2"),
						M("Heart.png", 529, 450, 24, "OOT Ganon Castle Heart Shadow 1", "Ganon Castle Heart Shadow 1"),
						M("Heart.png", 505, 474, 24, "OOT Ganon Castle Heart Shadow 2", "Ganon Castle Heart Shadow 2"),
						M("Heart.png", 481, 498, 24, "OOT Ganon Castle Heart Shadow 3", "Ganon Castle Heart Shadow 3"),
						
						M("Chest.png", 1210, 595, 40, "OOT MQ Ganon Castle Shadow Trial Bomb Flower Chest", "MQ Ganon Castle Shadow Trial Bomb Flower Chest"),
						M("Chest.png", 651, 412, 40, "OOT MQ Ganon Castle Shadow Trial Switch Chest", "MQ Ganon Castle Shadow Trial Switch Chest"),
						M("Pot.png", 284, 501, 24, "OOT MQ Ganon Pot Shadow End 1", "MQ Ganon Pot Shadow End 1"),
						M("Pot.png", 284, 437, 24, "OOT MQ Ganon Pot Shadow End 2", "MQ Ganon Pot Shadow End 2"),
						M("Silver_Rupee.png", 504, 477, 24, "OOT MQ Ganon Castle SR Shadow Back-Center", "MQ Ganon Castle SR Shadow Back-Center"),
						M("Silver_Rupee.png", 533, 584, 24, "OOT MQ Ganon Castle SR Shadow Back-Left", "MQ Ganon Castle SR Shadow Back-Left"),
						M("Silver_Rupee.png", 1094, 448, 24, "OOT MQ Ganon Castle SR Shadow Front-Center", "MQ Ganon Castle SR Shadow Front-Center"),
						M("Silver_Rupee.png", 1150, 329, 24, "OOT MQ Ganon Castle SR Shadow Front-Right", "MQ Ganon Castle SR Shadow Front-Right"),
						M("Silver_Rupee.png", 896, 485, 24, "OOT MQ Ganon Castle SR Shadow Middle", "MQ Ganon Castle SR Shadow Middle"),
						M("Wonder.png", 1150, 353, 24, "OOT MQ Ganon Castle Wonder Item", "MQ Ganon Castle Wonder Item")
                    }
                },
				new MapSubRegion
                {
                    Name = "Spirit Trial",
                    BackgroundImage = "region maps/OoT/Ganon/Spirit.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 860, 162, 40, "OOT Ganon Castle Spirit Chest 1", "Ganon Castle Spirit Chest 1"),
						M("Chest.png", 584, 162, 40, "OOT Ganon Castle Spirit Chest 2", "Ganon Castle Spirit Chest 2"),
						M("Pot.png", 444, 532, 24, "OOT Ganon Castle Pot Spirit End 1", "Ganon Castle Pot Spirit End 1"),
						M("Pot.png", 444, 662, 24, "OOT Ganon Castle Pot Spirit End 2", "Ganon Castle Pot Spirit End 2"),
						M("Heart.png", 1182, 772, 24, "OOT Ganon Castle Heart Spirit", "Ganon Castle Heart Spirit"),
						M("Silver_Rupee.png", 915, 773, 24, "OOT Ganon Castle SR Spirit Back Left", "Ganon Castle SR Spirit Back Left"),
						M("Silver_Rupee.png", 941, 438, 24, "OOT Ganon Castle SR Spirit Back Right", "Ganon Castle SR Spirit Back Right"),
						M("Silver_Rupee.png", 1040, 595, 24, "OOT Ganon Castle SR Spirit Center Bottom", "Ganon Castle SR Spirit Center Bottom"),
						M("Silver_Rupee.png", 1103, 595, 24, "OOT Ganon Castle SR Spirit Center Midair", "Ganon Castle SR Spirit Center Midair"),
						M("Silver_Rupee.png", 1210, 429, 24, "OOT Ganon Castle SR Spirit Front Right", "Ganon Castle SR Spirit Front Right"),
						M("Fairy_Spot.png", 1064, 587, 40, "OOT Ganon Castle Spirit Big Fairy", "Ganon Castle Spirit Big Fairy"),
						
						M("Chest.png", 860, 162, 40, "OOT MQ Ganon Castle Spirit Trial First Chest", "MQ Ganon Castle Spirit Trial First Chest"),
						M("Chest.png", 584, 162, 40, "OOT MQ Ganon Castle Spirit Trial Second Chest", "MQ Ganon Castle Spirit Trial Second Chest"),
						M("Chest.png", 700, 672, 40, "OOT MQ Ganon Castle Spirit Trial Back Left Sun Chest", "MQ Ganon Castle Spirit Trial Back Left Sun Chest"),
						M("Chest.png", 700, 505, 40, "OOT MQ Ganon Castle Spirit Trial Back Right Sun Chest", "MQ Ganon Castle Spirit Trial Back Right Sun Chest"),
						M("Chest.png", 533, 672, 40, "OOT MQ Ganon Castle Spirit Trial Front Left Sun Chest", "MQ Ganon Castle Spirit Trial Front Left Sun Chest"),
						M("Chest.png", 533, 505, 40, "OOT MQ Ganon Castle Spirit Trial Gold Gauntlets Chest", "MQ Ganon Castle Spirit Trial Gold Gauntlets Chest"),
						M("Pot.png", 444, 532, 24, "OOT MQ Ganon Pot Spirit End 1", "MQ Ganon Pot Spirit End 1"),
						M("Pot.png", 444, 662, 24, "OOT MQ Ganon Pot Spirit End 2", "MQ Ganon Pot Spirit End 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Light Trial",
                    BackgroundImage = "region maps/OoT/Ganon/Light.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 1223, 388, 40, "OOT Ganon Castle Light Chest Around 1", "Ganon Castle Light Chest Around 1"),
						M("Chest.png", 1223, 581, 40, "OOT Ganon Castle Light Chest Around 2", "Ganon Castle Light Chest Around 2"),
						M("Chest.png", 1297, 552, 40, "OOT Ganon Castle Light Chest Around 3", "Ganon Castle Light Chest Around 3"),
						M("Chest.png", 1151, 552, 40, "OOT Ganon Castle Light Chest Around 4", "Ganon Castle Light Chest Around 4"),
						M("Chest.png", 1297, 414, 40, "OOT Ganon Castle Light Chest Around 5", "Ganon Castle Light Chest Around 5"),
						M("Chest.png", 1151, 414, 40, "OOT Ganon Castle Light Chest Around 6", "Ganon Castle Light Chest Around 6"),
						M("Chest.png", 1223, 482, 40, "OOT Ganon Castle Light Chest Center", "Ganon Castle Light Chest Center"),
						M("Chest.png", 1020, 434, 40, "OOT Ganon Castle Light Chest Lullaby", "Ganon Castle Light Chest Lullaby"),
						M("Pot.png", 807, 490, 24, "OOT Ganon Castle Pot Light", "Ganon Castle Pot Light"),
						M("Pot.png", 235, 451, 24, "OOT Ganon Castle Pot Light End 1", "Ganon Castle Pot Light End 1"),
						M("Pot.png", 235, 530, 24, "OOT Ganon Castle Pot Light End 2", "Ganon Castle Pot Light End 2"),
						M("Silver_Rupee.png", 775, 625, 24, "OOT Ganon Castle SR Light Alcove Left", "Ganon Castle SR Light Alcove Left"),
						M("Silver_Rupee.png", 839, 379, 24, "OOT Ganon Castle SR Light Alcove Right", "Ganon Castle SR Light Alcove Right"),
						M("Silver_Rupee.png", 761, 516, 24, "OOT Ganon Castle SR Light Center Left", "Ganon Castle SR Light Center Left"),
						M("Silver_Rupee.png", 761, 464, 24, "OOT Ganon Castle SR Light Center Right", "Ganon Castle SR Light Center Right"),
						M("Silver_Rupee.png", 779, 489, 24, "OOT Ganon Castle SR Light Center Top", "Ganon Castle SR Light Center Top"),
						
						M("Chest.png", 1020, 434, 40, "OOT MQ Ganon Castle Light Trial Chest", "MQ Ganon Castle Light Trial Chest"),
						M("Pot.png", 235, 530, 24, "OOT MQ Ganon Pot Light End 1", "MQ Ganon Pot Light End 1"),
						M("Pot.png", 235, 451, 24, "OOT MQ Ganon Pot Light End 2", "MQ Ganon Pot Light End 2"),
						M("Heart.png", 775, 625, 24, "OOT MQ Ganon Castle Heart Light 1", "MQ Ganon Castle Heart Light 1"),
						M("Heart.png", 839, 379, 24, "OOT MQ Ganon Castle Heart Light 2", "MQ Ganon Castle Heart Light 2")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion GanonTower()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Ganon Tower";
            mapRegion.Game = "OOT";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Entrance",
                    BackgroundImage = "region maps/OoT/Ganon/Entrance.png",
                    DestinationEntranceIds = new List<string> { "OOT_GANON_TOWER" },
                    Marks = new List<MapMark>
                    {
						ME("Entrance.png", 251, 263, "Entrance shuffle (Inside Ganon Castle)", "OOT_GANON_CASTLE_FROM_TOWER")
                    }
                },
				new MapSubRegion
                {
                    Name = "Boss Key Room",
                    BackgroundImage = "region maps/OoT/Ganon/Boss_Key_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 453, 302, 40, "OOT Ganon Castle Boss Key", "Ganon Castle Boss Key")
                    }
                },
				new MapSubRegion
                {
                    Name = "Pots Room",
                    BackgroundImage = "region maps/OoT/Ganon/Pots_Room.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Pot.png", 239, 160, 24, "OOT Ganon Tower Pot 01", "Ganon Tower Pot 01"),
						M("Pot.png", 261, 136, 24, "OOT Ganon Tower Pot 02", "Ganon Tower Pot 02"),
						M("Pot.png", 283, 112, 24, "OOT Ganon Tower Pot 03", "Ganon Tower Pot 03"),
						M("Pot.png", 305, 88, 24, "OOT Ganon Tower Pot 04", "Ganon Tower Pot 04"),
						M("Pot.png", 327, 64, 24, "OOT Ganon Tower Pot 05", "Ganon Tower Pot 05"),
						M("Pot.png", 261, 160, 24, "OOT Ganon Tower Pot 06", "Ganon Tower Pot 06"),
						M("Pot.png", 283, 136, 24, "OOT Ganon Tower Pot 07", "Ganon Tower Pot 07"),
						M("Pot.png", 305, 112, 24, "OOT Ganon Tower Pot 08", "Ganon Tower Pot 08"),
						M("Pot.png", 327, 88, 24, "OOT Ganon Tower Pot 09", "Ganon Tower Pot 09"),
						M("Pot.png", 676, 160, 24, "OOT Ganon Tower Pot 10", "Ganon Tower Pot 10"),
						M("Pot.png", 654, 136, 24, "OOT Ganon Tower Pot 11", "Ganon Tower Pot 11"),
						M("Pot.png", 632, 112, 24, "OOT Ganon Tower Pot 12", "Ganon Tower Pot 12"),
						M("Pot.png", 610, 88, 24, "OOT Ganon Tower Pot 13", "Ganon Tower Pot 13"),
						M("Pot.png", 588, 64, 24, "OOT Ganon Tower Pot 14", "Ganon Tower Pot 14"),
						M("Pot.png", 654, 160, 24, "OOT Ganon Tower Pot 15", "Ganon Tower Pot 15"),
						M("Pot.png", 632, 136, 24, "OOT Ganon Tower Pot 16", "Ganon Tower Pot 16"),
						M("Pot.png", 610, 112, 24, "OOT Ganon Tower Pot 17", "Ganon Tower Pot 17"),
						M("Pot.png", 588, 88, 24, "OOT Ganon Tower Pot 18", "Ganon Tower Pot 18")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion WoodfallTemple()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Woodfall Temple";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Entrance",
                    BackgroundImage = "region maps/MM/Dungeons/Woodfall/Entrance.png",
                    DestinationEntranceIds = new List<string> { "MM_TEMPLE_WOODFALL" },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 449, 388, 40, "MM Woodfall Temple SF Entrance", "Woodfall Temple SF Entrance"),
						M("Hive.png", 429, 333, 40, "MM Woodfall Temple Hive Entrance", "Woodfall Temple Hive Entrance"),
						M("Chest.png", 590, 423, 40, "MM Woodfall Temple Entrance Chest", "Woodfall Temple Entrance Chest"),
						M("Pot.png", 488, 502, 24, "MM Woodfall Temple Pot Entrance", "Woodfall Temple Pot Entrance"),
						M("Grass.png", 434, 398, 24, "MM Woodfall Temple Grass Entrance Bottom 1", "Woodfall Temple Grass Entrance Bottom 1"),
						M("Grass.png", 450, 368, 24, "MM Woodfall Temple Grass Entrance Bottom 2", "Woodfall Temple Grass Entrance Bottom 2"),
						M("Grass.png", 426, 375, 24, "MM Woodfall Temple Grass Entrance Bottom 3", "Woodfall Temple Grass Entrance Bottom 3"),
						M("Grass.png", 425, 148, 24, "MM Woodfall Temple Grass Entrance Ledge 1", "Woodfall Temple Grass Entrance Ledge 1"),
						M("Grass.png", 487, 148, 24, "MM Woodfall Temple Grass Entrance Ledge 2", "Woodfall Temple Grass Entrance Ledge 2"),
						
						ME("Entrance.png", 449, 459, "Entrance shuffle (Odolwa)", "MM_BOSS_TEMPLE_WOODFALL"),
						ME("Entrance.png", 449, 578, "Entrance shuffle (Woodfall)", "MM_WOODFALL_FROM_TEMPLE")
                    }
                },
				new MapSubRegion
                {
                    Name = "Central Room",
                    BackgroundImage = "region maps/MM/Dungeons/Woodfall/Central.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 210, 378, 40, "MM Woodfall Temple SF Main Bubble", "Woodfall Temple SF Main Bubble"),
						M("Stray_Fairy.png", 483, 547, 40, "MM Woodfall Temple SF Main Deku Baba", "Woodfall Temple SF Main Deku Baba"),
						M("Stray_Fairy.png", 667, 121, 40, "MM Woodfall Temple SF Main Pot", "Woodfall Temple SF Main Pot"),
						M("Chest.png", 642, 290, 40, "MM Woodfall Temple Center Chest", "Woodfall Temple Center Chest"),
						M("Grass.png", 439, 573, 24, "MM Woodfall Temple Grass Main Room 1", "Woodfall Temple Grass Main Room 1"),
						M("Grass.png", 727, 344, 24, "MM Woodfall Temple Grass Main Room 2", "Woodfall Temple Grass Main Room 2"),
						M("Grass.png", 432, 550, 24, "MM Woodfall Temple Grass Main Room 3", "Woodfall Temple Grass Main Room 3"),
						M("Pot.png", 629, 177, 24, "MM Woodfall Temple Pot Main Room Lower 1", "Woodfall Temple Pot Main Room Lower 1"),
						M("Pot.png", 438, 139, 24, "MM Woodfall Temple Pot Main Room Lower 2", "Woodfall Temple Pot Main Room Lower 2"),
						M("Pot.png", 504, 139, 24, "MM Woodfall Temple Pot Main Room Lower 3", "Woodfall Temple Pot Main Room Lower 3"),
						M("Pot.png", 649, 157, 24, "MM Woodfall Temple Pot Main Room Lower 4", "Woodfall Temple Pot Main Room Lower 4"),
						M("Pot.png", 669, 177, 24, "MM Woodfall Temple Pot Main Room Lower 5", "Woodfall Temple Pot Main Room Lower 5"),
						M("Pot.png", 629, 137, 24, "MM Woodfall Temple Pot Main Room Lower 6", "Woodfall Temple Pot Main Room Lower 6"),
						M("Pot.png", 552, 73, 24, "MM Woodfall Temple Pot Main Room Upper 1", "Woodfall Temple Pot Main Room Upper 1"),
						M("Pot.png", 552, 97, 24, "MM Woodfall Temple Pot Main Room Upper 2", "Woodfall Temple Pot Main Room Upper 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Water Room",
                    BackgroundImage = "region maps/MM/Dungeons/Woodfall/Water.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 350, 174, 40, "MM Woodfall Temple SF Water Room Beehive", "Woodfall Temple SF Water Room Beehive"),
						M("Chest.png", 449, 201, 40, "MM Woodfall Temple Water Chest", "Woodfall Temple Water Chest"),
						M("Grass.png", 635, 372, 24, "MM Woodfall Temple Grass Water Room 1", "Woodfall Temple Grass Water Room 1"),
						M("Grass.png", 668, 388, 24, "MM Woodfall Temple Grass Water Room 2", "Woodfall Temple Grass Water Room 2"),
						M("Pot.png", 512, 516, 24, "MM Woodfall Temple Pot Water Room 1", "Woodfall Temple Pot Water Room 1"),
						M("Pot.png", 514, 540, 24, "MM Woodfall Temple Pot Water Room 2", "Woodfall Temple Pot Water Room 2"),
						M("Pot.png", 516, 564, 24, "MM Woodfall Temple Pot Water Room 3", "Woodfall Temple Pot Water Room 3"),
						M("Pot.png", 518, 588, 24, "MM Woodfall Temple Pot Water Room 4", "Woodfall Temple Pot Water Room 4")
                    }
                },
				new MapSubRegion
                {
                    Name = "Dinolfos Room",
                    BackgroundImage = "region maps/MM/Dungeons/Woodfall/Dinalfos.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 450, 200, 40, "MM Woodfall Temple Bow", "Woodfall Temple Bow")
                    }
                },
				new MapSubRegion
                {
                    Name = "Map Room",
                    BackgroundImage = "region maps/MM/Dungeons/Woodfall/Map.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 451, 274, 40, "MM Woodfall Temple Map", "Woodfall Temple Map"),
						M("Grass.png", 547, 103, 24, "MM Woodfall Temple Grass Map Room 1", "Woodfall Temple Grass Map Room 1"),
						M("Grass.png", 556, 419, 24, "MM Woodfall Temple Grass Map Room 2", "Woodfall Temple Grass Map Room 2"),
						M("Grass.png", 586, 128, 24, "MM Woodfall Temple Grass Map Room 3", "Woodfall Temple Grass Map Room 3"),
						M("Grass.png", 610, 457, 24, "MM Woodfall Temple Grass Map Room 4", "Woodfall Temple Grass Map Room 4"),
						M("Grass.png", 591, 400, 24, "MM Woodfall Temple Grass Map Room 5", "Woodfall Temple Grass Map Room 5")
                    }
                },
				new MapSubRegion
                {
                    Name = "Gekko Room",
                    BackgroundImage = "region maps/MM/Dungeons/Woodfall/Geeko.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 428, 546, 40, "MM Woodfall Temple Boss Key Chest", "Woodfall Temple Boss Key Chest"),
						M("Pot.png", 485, 483, 24, "MM Woodfall Temple Pot Miniboss Room 1", "Woodfall Temple Pot Miniboss Room 1"),
						M("Pot.png", 463, 483, 24, "MM Woodfall Temple Pot Miniboss Room 2", "Woodfall Temple Pot Miniboss Room 2"),
						M("Pot.png", 386, 483, 24, "MM Woodfall Temple Pot Miniboss Room 3", "Woodfall Temple Pot Miniboss Room 3"),
						M("Pot.png", 408, 483, 24, "MM Woodfall Temple Pot Miniboss Room 4", "Woodfall Temple Pot Miniboss Room 4")
                    }
                },
				new MapSubRegion
                {
                    Name = "Maze Room",
                    BackgroundImage = "region maps/MM/Dungeons/Woodfall/Maze.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 435, 339, 40, "MM Woodfall Temple SF Maze Beehive", "Woodfall Temple SF Maze Beehive"),
						M("Stray_Fairy.png", 493, 447, 40, "MM Woodfall Temple SF Maze Bubble", "Woodfall Temple SF Maze Bubble"),
						M("Stray_Fairy.png", 179, 400, 40, "MM Woodfall Temple SF Maze Skulltula", "Woodfall Temple SF Maze Skulltula"),
						M("Pot.png", 434, 273, 24, "MM Woodfall Temple Pot Maze 1", "Woodfall Temple Pot Maze 1"),
						M("Pot.png", 456, 273, 24, "MM Woodfall Temple Pot Maze 2", "Woodfall Temple Pot Maze 2"),
						M("Hive.png", 327, 339, 40, "MM Woodfall Temple Hive Maze", "Woodfall Temple Hive Maze")
                    }
                },
				new MapSubRegion
                {
                    Name = "Compass Room",
                    BackgroundImage = "region maps/MM/Dungeons/Woodfall/Compass.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 448, 143, 40, "MM Woodfall Temple Compass", "Woodfall Temple Compass"),
						M("Grass.png", 289, 409, 24, "MM Woodfall Temple Grass Compass Room 1", "Woodfall Temple Grass Compass Room 1"),
						M("Grass.png", 257, 350, 24, "MM Woodfall Temple Grass Compass Room 2", "Woodfall Temple Grass Compass Room 2"),
						M("Grass.png", 272, 304, 24, "MM Woodfall Temple Grass Compass Room 3", "Woodfall Temple Grass Compass Room 3")
                    }
                },
				new MapSubRegion
                {
                    Name = "Dark Room",
                    BackgroundImage = "region maps/MM/Dungeons/Woodfall/Dark.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 221, 321, 40, "MM Woodfall Temple Dark Chest", "Woodfall Temple Dark Chest")
                    }
                },
				new MapSubRegion
                {
                    Name = "Pits Room",
                    BackgroundImage = "region maps/MM/Dungeons/Woodfall/Pits.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Grass.png", 513, 298, 24, "MM Woodfall Temple Grass Pits Room 01", "Woodfall Temple Grass Pits Room 01"),
						M("Grass.png", 392, 292, 24, "MM Woodfall Temple Grass Pits Room 02", "Woodfall Temple Grass Pits Room 02"),
						M("Grass.png", 582, 282, 24, "MM Woodfall Temple Grass Pits Room 03", "Woodfall Temple Grass Pits Room 03"),
						M("Grass.png", 333, 329, 24, "MM Woodfall Temple Grass Pits Room 04", "Woodfall Temple Grass Pits Room 04"),
						M("Grass.png", 419, 347, 24, "MM Woodfall Temple Grass Pits Room 05", "Woodfall Temple Grass Pits Room 05"),
						M("Grass.png", 363, 354, 24, "MM Woodfall Temple Grass Pits Room 06", "Woodfall Temple Grass Pits Room 06"),
						M("Grass.png", 514, 447, 24, "MM Woodfall Temple Grass Pits Room 07", "Woodfall Temple Grass Pits Room 07"),
						M("Grass.png", 347, 508, 24, "MM Woodfall Temple Grass Pits Room 08", "Woodfall Temple Grass Pits Room 08"),
						M("Grass.png", 489, 481, 24, "MM Woodfall Temple Grass Pits Room 09", "Woodfall Temple Grass Pits Room 09"),
						M("Grass.png", 380, 475, 24, "MM Woodfall Temple Grass Pits Room 10", "Woodfall Temple Grass Pits Room 10"),
						M("Grass.png", 544, 501, 24, "MM Woodfall Temple Grass Pits Room 11", "Woodfall Temple Grass Pits Room 11")
                    }
                },
				new MapSubRegion
                {
                    Name = "Pre-Boss Room",
                    BackgroundImage = "region maps/MM/Dungeons/Woodfall/Pre_Boss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 370, 108, 40, "MM Woodfall Temple SF Pre-Boss Bottom Right", "Woodfall Temple SF Pre-Boss Bottom Right"),
						M("Stray_Fairy.png", 730, 165, 40, "MM Woodfall Temple SF Pre-Boss Left", "Woodfall Temple SF Pre-Boss Left"),
						M("Stray_Fairy.png", 531, 283, 40, "MM Woodfall Temple SF Pre-Boss Pillar", "Woodfall Temple SF Pre-Boss Pillar"),
						M("Stray_Fairy.png", 362, 39, 40, "MM Woodfall Temple SF Pre-Boss Top Right", "Woodfall Temple SF Pre-Boss Top Right"),
						M("Grass.png", 265, 128, 24, "MM Woodfall Temple Grass Pre-Boss 1", "Woodfall Temple Grass Pre-Boss 1"),
						M("Grass.png", 385, 69, 24, "MM Woodfall Temple Grass Pre-Boss 2", "Woodfall Temple Grass Pre-Boss 2"),
						M("Grass.png", 359, 78, 24, "MM Woodfall Temple Grass Pre-Boss 3", "Woodfall Temple Grass Pre-Boss 3"),
						M("Grass.png", 344, 52, 24, "MM Woodfall Temple Grass Pre-Boss 4", "Woodfall Temple Grass Pre-Boss 4"),
						M("Grass.png", 281, 103, 24, "MM Woodfall Temple Grass Pre-Boss 5", "Woodfall Temple Grass Pre-Boss 5"),
						M("Rupee.png", 444, 370, 24, "MM Woodfall Temple Rupee Lower 1", "Woodfall Temple Rupee Lower 1"),
						M("Rupee.png", 477, 370, 24, "MM Woodfall Temple Rupee Lower 2", "Woodfall Temple Rupee Lower 2"),
						M("Rupee.png", 444, 401, 24, "MM Woodfall Temple Rupee Lower 3", "Woodfall Temple Rupee Lower 3"),
						M("Rupee.png", 477, 401, 24, "MM Woodfall Temple Rupee Lower 4", "Woodfall Temple Rupee Lower 4"),
						M("Rupee.png", 632, 463, 24, "MM Woodfall Temple Rupee Upper Left", "Woodfall Temple Rupee Upper Left"),
						M("Rupee.png", 289, 463, 24, "MM Woodfall Temple Rupee Upper Right", "Woodfall Temple Rupee Upper Right"),
						M("Pot.png", 527, 592, 24, "MM Woodfall Temple Pot Pre-Boss 1", "Woodfall Temple Pot Pre-Boss 1"),
						M("Pot.png", 395, 592, 24, "MM Woodfall Temple Pot Pre-Boss 2", "Woodfall Temple Pot Pre-Boss 2"),
						
						ME("Entrance.png", 453, 580, "Entrance shuffle (Odolwa)", "MM_BOSS_TEMPLE_WOODFALL")
                    }
                },
				new MapSubRegion
                {
                    Name = "Boss Room",
                    BackgroundImage = "region maps/MM/Dungeons/Woodfall/Boss.png",
                    DestinationEntranceIds = new List<string> { "MM_BOSS_TEMPLE_WOODFALL" },
                    Marks = new List<MapMark>
                    {
						M("Collectible.png", 544, 283, 40, "MM Woodfall Temple Boss Container", "Woodfall Temple Boss Container"),
						M("NPC.png", 449, 283, 40, "MM Woodfall Temple Boss", "Woodfall Temple Boss"),
						M("NPC.png", 354, 283, 40, "MM Oath to Order", "Oath to Order"),
						M("Grass.png", 369, 89, 24, "MM Woodfall Temple Boss Grass 01", "Woodfall Temple Boss Grass 01"),
						M("Grass.png", 546, 89, 24, "MM Woodfall Temple Boss Grass 02", "Woodfall Temple Boss Grass 02"),
						M("Grass.png", 335, 105, 24, "MM Woodfall Temple Boss Grass 03", "Woodfall Temple Boss Grass 03"),
						M("Grass.png", 580, 105, 24, "MM Woodfall Temple Boss Grass 04", "Woodfall Temple Boss Grass 04"),
						M("Grass.png", 262, 177, 24, "MM Woodfall Temple Boss Grass 05", "Woodfall Temple Boss Grass 05"),
						M("Grass.png", 658, 177, 24, "MM Woodfall Temple Boss Grass 06", "Woodfall Temple Boss Grass 06"),
						M("Grass.png", 250, 202, 24, "MM Woodfall Temple Boss Grass 07", "Woodfall Temple Boss Grass 07"),
						M("Grass.png", 670, 202, 24, "MM Woodfall Temple Boss Grass 08", "Woodfall Temple Boss Grass 08"),
						M("Grass.png", 250, 388, 24, "MM Woodfall Temple Boss Grass 09", "Woodfall Temple Boss Grass 09"),
						M("Grass.png", 670, 388, 24, "MM Woodfall Temple Boss Grass 10", "Woodfall Temple Boss Grass 10"),
						M("Grass.png", 262, 413, 24, "MM Woodfall Temple Boss Grass 11", "Woodfall Temple Boss Grass 11"),
						M("Grass.png", 658, 413, 24, "MM Woodfall Temple Boss Grass 12", "Woodfall Temple Boss Grass 12"),
						M("Grass.png", 580, 488, 24, "MM Woodfall Temple Boss Grass 13", "Woodfall Temple Boss Grass 13"),
						M("Grass.png", 335, 488, 24, "MM Woodfall Temple Boss Grass 14", "Woodfall Temple Boss Grass 14"),
						M("Grass.png", 546, 504, 24, "MM Woodfall Temple Boss Grass 15", "Woodfall Temple Boss Grass 15"),
						M("Grass.png", 369, 504, 24, "MM Woodfall Temple Boss Grass 16", "Woodfall Temple Boss Grass 16")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion SnowheadTemple()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Snowhead Temple";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Entrance",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Entrance.png",
                    DestinationEntranceIds = new List<string> { "MM_TEMPLE_SNOWHEAD" },
                    Marks = new List<MapMark>
                    {
						M("Icicle.png", 473, 312, 24, "MM Snowhead Temple Icicle Entrance 1", "Snowhead Temple Icicle Entrance 1"),
						M("Icicle.png", 473, 280, 24, "MM Snowhead Temple Icicle Entrance 2", "Snowhead Temple Icicle Entrance 2"),
						M("Icicle.png", 473, 248, 24, "MM Snowhead Temple Icicle Entrance 3", "Snowhead Temple Icicle Entrance 3"),
						M("Icicle.png", 473, 296, 24, "MM Snowhead Temple Icicle Entrance 4", "Snowhead Temple Icicle Entrance 4"),
						M("Icicle.png", 473, 264, 24, "MM Snowhead Temple Icicle Entrance 5", "Snowhead Temple Icicle Entrance 5"),
						M("Icicle.png", 174, 391, 24, "MM Snowhead Temple Icicle Entrance After Block 1", "Snowhead Temple Icicle Entrance After Block 1"),
						M("Icicle.png", 224, 391, 24, "MM Snowhead Temple Icicle Entrance After Block 2", "Snowhead Temple Icicle Entrance After Block 2"),
						M("Pot.png", 184, 168, 24, "MM Snowhead Temple Pot Entrance 1", "Snowhead Temple Pot Entrance 1"),
						M("Pot.png", 239, 168, 24, "MM Snowhead Temple Pot Entrance 2", "Snowhead Temple Pot Entrance 2"),
						
						ME("Entrance.png", 572, 269, "Entrance shuffle (Goht)", "MM_BOSS_TEMPLE_SNOWHEAD"),
						ME("Entrance.png", 832, 269, "Entrance shuffle (Snowhead)", "MM_SNOWHEAD_FROM_TEMPLE")
                    }
                },
				new MapSubRegion
                {
                    Name = "Bridge Room",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Bridge.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 564, 406, 40, "MM Snowhead Temple SF Bridge Pillar", "Snowhead Temple SF Bridge Pillar"),
						M("Stray_Fairy.png", 821, 355, 40, "MM Snowhead Temple SF Bridge Under Platform", "Snowhead Temple SF Bridge Under Platform"),
						M("Chest.png", 391, 184, 40, "MM Snowhead Temple Bridge Room", "Snowhead Temple Bridge Room"),
						M("Crate.png", 814, 264, 24, "MM Snowhead Temple Crate Bridge", "Snowhead Temple Crate Bridge"),
						M("Snowball.png", 101, 291, 24, "MM Snowhead Temple Small Snowball Bridge Room 1", "Snowhead Temple Small Snowball Bridge Room 1"),
						M("Snowball.png", 85, 270, 24, "MM Snowhead Temple Small Snowball Bridge Room 2", "Snowhead Temple Small Snowball Bridge Room 2"),
						M("Pot.png", 279, 498, 24, "MM Snowhead Temple Pot Bridge Room 1", "Snowhead Temple Pot Bridge Room 1"),
						M("Pot.png", 303, 550, 24, "MM Snowhead Temple Pot Bridge Room 2", "Snowhead Temple Pot Bridge Room 2"),
						M("Pot.png", 317, 516, 24, "MM Snowhead Temple Pot Bridge Room 3", "Snowhead Temple Pot Bridge Room 3"),
						M("Pot.png", 265, 532, 24, "MM Snowhead Temple Pot Bridge Room 4", "Snowhead Temple Pot Bridge Room 4"),
						M("Pot.png", 291, 524, 24, "MM Snowhead Temple Pot Bridge Room 5", "Snowhead Temple Pot Bridge Room 5"),
						M("Pot.png", 732, 196, 24, "MM Snowhead Temple Pot Bridge Room After 1", "Snowhead Temple Pot Bridge Room After 1"),
						M("Pot.png", 716, 179, 24, "MM Snowhead Temple Pot Bridge Room After 2", "Snowhead Temple Pot Bridge Room After 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Map Room",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Map.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 339, 191, 40, "MM Snowhead Temple SF Map Room", "Snowhead Temple SF Map Room"),
						M("Chest.png", 554, 215, 40, "MM Snowhead Temple Map", "Snowhead Temple Map"),
						M("Chest.png", 438, 26, 40, "MM Snowhead Temple Map Alcove", "Snowhead Temple Map Alcove"),
						M("Crate.png", 554, 442, 24, "MM Snowhead Temple Crate Map Room 1", "Snowhead Temple Crate Map Room 1"),
						M("Crate.png", 458, 442, 24, "MM Snowhead Temple Crate Map Room 2", "Snowhead Temple Crate Map Room 2"),
						M("Crate.png", 482, 442, 24, "MM Snowhead Temple Crate Map Room 3", "Snowhead Temple Crate Map Room 3"),
						M("Crate.png", 530, 442, 24, "MM Snowhead Temple Crate Map Room 4", "Snowhead Temple Crate Map Room 4"),
						M("Crate.png", 506, 442, 24, "MM Snowhead Temple Crate Map Room 5", "Snowhead Temple Crate Map Room 5")
                    }
                },
				new MapSubRegion
                {
                    Name = "Central Room",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Central.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 550, 226, 40, "MM Snowhead Temple Central Room Alcove", "Snowhead Temple Central Room Alcove"),
						M("Chest.png", 382, 536, 40, "MM Snowhead Temple Central Room Bottom", "Snowhead Temple Central Room Bottom"),
						M("Chest.png", 619, 76, 40, "MM Snowhead Temple Boss Key", "Snowhead Temple Boss Key"),
						M("Icicle.png", 601, 3, 24, "MM Snowhead Temple Icicle Central Room Near Boss 1", "Snowhead Temple Icicle Central Room Near Boss 1"),
						M("Icicle.png", 577, 3, 24, "MM Snowhead Temple Icicle Central Room Near Boss 2", "Snowhead Temple Icicle Central Room Near Boss 2"),
						M("Icicle.png", 625, 3, 24, "MM Snowhead Temple Icicle Central Room Near Boss 3", "Snowhead Temple Icicle Central Room Near Boss 3"),
						M("Icicle.png", 649, 3, 24, "MM Snowhead Temple Icicle Central Room Near Boss 4", "Snowhead Temple Icicle Central Room Near Boss 4"),
						M("Icicle.png", 458, 97, 24, "MM Snowhead Temple Icicle Central Room Near Boss Key 1", "Snowhead Temple Icicle Central Room Near Boss Key 1"),
						M("Icicle.png", 473, 75, 24, "MM Snowhead Temple Icicle Central Room Near Boss Key 2", "Snowhead Temple Icicle Central Room Near Boss Key 2"),
						M("Snowball.png", 406, 165, 24, "MM Snowhead Temple Big Snowball Central Room 1", "Snowhead Temple Big Snowball Central Room 1"),
						M("Snowball.png", 384, 165, 24, "MM Snowhead Temple Big Snowball Central Room 2", "Snowhead Temple Big Snowball Central Room 2"),
						M("Snowball.png", 362, 165, 24, "MM Snowhead Temple Big Snowball Central Room 3", "Snowhead Temple Big Snowball Central Room 3"),
						M("Snowball.png", 340, 165, 24, "MM Snowhead Temple Big Snowball Central Room 4", "Snowhead Temple Big Snowball Central Room 4"),
						M("Snowball.png", 463, 283, 24, "MM Snowhead Temple Small Snowball Central Room 1", "Snowhead Temple Small Snowball Central Room 1"),
						M("Snowball.png", 474, 260, 24, "MM Snowhead Temple Small Snowball Central Room 2", "Snowhead Temple Small Snowball Central Room 2"),
						M("Snowball.png", 452, 260, 24, "MM Snowhead Temple Small Snowball Central Room 3", "Snowhead Temple Small Snowball Central Room 3"),
						M("Snowball.png", 384, 381, 24, "MM Snowhead Temple Small Snowball Central Room 4", "Snowhead Temple Small Snowball Central Room 4"),
						M("Snowball.png", 360, 373, 24, "MM Snowhead Temple Small Snowball Central Room 5", "Snowhead Temple Small Snowball Central Room 5"),
						M("Snowball.png", 379, 357, 24, "MM Snowhead Temple Small Snowball Central Room 6", "Snowhead Temple Small Snowball Central Room 6"),
						M("Pot.png", 570, 452, 24, "MM Snowhead Temple Pot Central Room Bottom 1", "Snowhead Temple Pot Central Room Bottom 1"),
						M("Pot.png", 539, 432, 24, "MM Snowhead Temple Pot Central Room Bottom 2", "Snowhead Temple Pot Central Room Bottom 2"),
						M("Pot.png", 468, 379, 24, "MM Snowhead Temple Pot Central Room Level 2 1", "Snowhead Temple Pot Central Room Level 2 1"),
						M("Pot.png", 468, 403, 24, "MM Snowhead Temple Pot Central Room Level 2 2", "Snowhead Temple Pot Central Room Level 2 2"),
						M("Pot.png", 600, 75, 24, "MM Snowhead Temple Pot Central Room Near Boss Key 1", "Snowhead Temple Pot Central Room Near Boss Key 1"),
						M("Pot.png", 579, 82, 24, "MM Snowhead Temple Pot Central Room Near Boss Key 2", "Snowhead Temple Pot Central Room Near Boss Key 2"),
						M("Pot.png", 351, 403, 24, "MM Snowhead Temple Pot Central Room Scarecrow 1", "Snowhead Temple Pot Central Room Scarecrow 1"),
						M("Pot.png", 326, 410, 24, "MM Snowhead Temple Pot Central Room Scarecrow 2", "Snowhead Temple Pot Central Room Scarecrow 2"),
						M("Grass.png", 560, 558, 24, "MM Snowhead Temple Grass 01", "Snowhead Temple Grass 01"),
						M("Grass.png", 535, 545, 24, "MM Snowhead Temple Grass 02", "Snowhead Temple Grass 02"),
						M("Grass.png", 509, 541, 24, "MM Snowhead Temple Grass 03", "Snowhead Temple Grass 03"),
						M("Grass.png", 546, 570, 24, "MM Snowhead Temple Grass 04", "Snowhead Temple Grass 04"),
						M("Grass.png", 535, 558, 24, "MM Snowhead Temple Grass 05", "Snowhead Temple Grass 05"),
						M("Grass.png", 513, 556, 24, "MM Snowhead Temple Grass 06", "Snowhead Temple Grass 06"),
						M("Grass.png", 523, 570, 24, "MM Snowhead Temple Grass 07", "Snowhead Temple Grass 07"),
						M("Grass.png", 488, 551, 24, "MM Snowhead Temple Grass 08", "Snowhead Temple Grass 08"),
						M("Grass.png", 492, 535, 24, "MM Snowhead Temple Grass 09", "Snowhead Temple Grass 09"),
						M("Grass.png", 473, 539, 24, "MM Snowhead Temple Grass 10", "Snowhead Temple Grass 10"),
						
						ME("Entrance.png", 636, 1, "Entrance shuffle (Goht)", "MM_BOSS_TEMPLE_SNOWHEAD")
                    }
                },
				new MapSubRegion
                {
                    Name = "Blocks Room",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Blocks.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 293, 97, 40, "MM Snowhead Temple Block Room", "Snowhead Temple Block Room"),
						M("Chest.png", 455, 421, 40, "MM Snowhead Temple Block Room Ledge", "Snowhead Temple Block Room Ledge"),
						M("Pot.png", 412, 204, 24, "MM Snowhead Temple Pot Block Room 1", "Snowhead Temple Pot Block Room 1"),
						M("Pot.png", 386, 196, 24, "MM Snowhead Temple Pot Block Room 2", "Snowhead Temple Pot Block Room 2"),
						M("Pot.png", 523, 565, 24, "MM Snowhead Temple Flying Pot 1", "Snowhead Temple Flying Pot 1"),
						M("Pot.png", 623, 524, 24, "MM Snowhead Temple Flying Pot 2", "Snowhead Temple Flying Pot 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Compass Room",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Compass.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 110, 258, 40, "MM Snowhead Temple SF Compass Room Crate", "Snowhead Temple SF Compass Room Crate"),
						M("Chest.png", 335, 283, 40, "MM Snowhead Temple Compass", "Snowhead Temple Compass"),
						M("Chest.png", 631, 450, 40, "MM Snowhead Temple Compass Room Ledge", "Snowhead Temple Compass Room Ledge"),
						M("Pot.png", 146, 202, 24, "MM Snowhead Temple Pot Compass Room 1", "Snowhead Temple Pot Compass Room 1"),
						M("Pot.png", 180, 166, 24, "MM Snowhead Temple Pot Compass Room 2", "Snowhead Temple Pot Compass Room 2"),
						M("Pot.png", 765, 413, 24, "MM Snowhead Temple Pot Compass Room 3", "Snowhead Temple Pot Compass Room 3"),
						M("Pot.png", 743, 425, 24, "MM Snowhead Temple Pot Compass Room 4", "Snowhead Temple Pot Compass Room 4"),
						M("Pot.png", 163, 184, 24, "MM Snowhead Temple Pot Compass Room 5", "Snowhead Temple Pot Compass Room 5")
                    }
                },
				new MapSubRegion
                {
                    Name = "Icicle Room",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Icicle.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 278, 246, 40, "MM Snowhead Temple Icicle Room", "Snowhead Temple Icicle Room"),
						M("Chest.png", 796, 310, 40, "MM Snowhead Temple Icicle Room Alcove", "Snowhead Temple Icicle Room Alcove"),
						M("Snowball.png", 282, 284, 24, "MM Snowhead Temple Big Snowball Icicle Room", "Snowhead Temple Big Snowball Icicle Room"),
						M("Snowball.png", 243, 256, 24, "MM Snowhead Temple Small Snowball Icicles Room 1", "Snowhead Temple Small Snowball Icicles Room 1"),
						M("Snowball.png", 291, 222, 24, "MM Snowhead Temple Small Snowball Icicles Room 2", "Snowhead Temple Small Snowball Icicles Room 2"),
						M("Snowball.png", 327, 250, 24, "MM Snowhead Temple Small Snowball Icicles Room 3", "Snowhead Temple Small Snowball Icicles Room 3"),
						M("Snowball.png", 442, 222, 24, "MM Snowhead Temple Small Snowball Icicles Room 4", "Snowhead Temple Small Snowball Icicles Room 4"),
						M("Snowball.png", 411, 226, 24, "MM Snowhead Temple Small Snowball Icicles Room 5", "Snowhead Temple Small Snowball Icicles Room 5"),
						M("Rupee.png", 567, 305, 24, "MM Snowhead Temple Rupee 1", "Snowhead Temple Rupee 1"),
						M("Rupee.png", 313, 342, 24, "MM Snowhead Temple Rupee 2", "Snowhead Temple Rupee 2"),
						M("Rupee.png", 527, 272, 24, "MM Snowhead Temple Rupee 3", "Snowhead Temple Rupee 3")
                    }
                },
				new MapSubRegion
                {
                    Name = "Dual Switches Room",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Dual_Switches.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 477, 84, 40, "MM Snowhead Temple SF Dual Switches", "Snowhead Temple SF Dual Switches"),
						M("Crate.png", 664, 317, 24, "MM Snowhead Temple Crate Dual Switches 1", "Snowhead Temple Crate Dual Switches 1"),
						M("Crate.png", 687, 321, 24, "MM Snowhead Temple Crate Dual Switches 2", "Snowhead Temple Crate Dual Switches 2"),
						M("Pot.png", 114, 421, 24, "MM Snowhead Temple Pot Dual Switches 1", "Snowhead Temple Pot Dual Switches 1"),
						M("Pot.png", 106, 455, 24, "MM Snowhead Temple Pot Dual Switches 2", "Snowhead Temple Pot Dual Switches 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Fire Arrow Room",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Fire_Arrow.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 452, 311, 40, "MM Snowhead Temple Fire Arrow", "Snowhead Temple Fire Arrow")
                    }
                },
				new MapSubRegion
                {
                    Name = "Pillars Room",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Pillars.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 320, 507, 40, "MM Snowhead Temple Pillars Room", "Snowhead Temple Pillars Room"),
						M("Pot.png", 397, 356, 24, "MM Snowhead Temple Pot Pillars Room Lower 1", "Snowhead Temple Pot Pillars Room Lower 1"),
						M("Pot.png", 419, 356, 24, "MM Snowhead Temple Pot Pillars Room Lower 2", "Snowhead Temple Pot Pillars Room Lower 2"),
						M("Pot.png", 441, 356, 24, "MM Snowhead Temple Pot Pillars Room Lower 3", "Snowhead Temple Pot Pillars Room Lower 3"),
						M("Pot.png", 419, 332, 24, "MM Snowhead Temple Pot Pillars Room Lower 4", "Snowhead Temple Pot Pillars Room Lower 4"),
						M("Pot.png", 612, 536, 24, "MM Snowhead Temple Pot Pillars Room Lower 5", "Snowhead Temple Pot Pillars Room Lower 5"),
						M("Pot.png", 612, 512, 24, "MM Snowhead Temple Pot Pillars Room Lower 6", "Snowhead Temple Pot Pillars Room Lower 6"),
						M("Pot.png", 612, 488, 24, "MM Snowhead Temple Pot Pillars Room Lower 7", "Snowhead Temple Pot Pillars Room Lower 7"),
						M("Pot.png", 318, 130, 24, "MM Snowhead Temple Pot Pillars Room Upper 1", "Snowhead Temple Pot Pillars Room Upper 1"),
						M("Pot.png", 718, 252, 24, "MM Snowhead Temple Pot Pillars Room Upper 2", "Snowhead Temple Pot Pillars Room Upper 2"),
						M("Pot.png", 294, 146, 24, "MM Snowhead Temple Pot Pillars Room Upper 3", "Snowhead Temple Pot Pillars Room Upper 3"),
						M("Pot.png", 735, 272, 24, "MM Snowhead Temple Pot Pillars Room Upper 4", "Snowhead Temple Pot Pillars Room Upper 4"),
						M("Pot.png", 373, 535, 24, "MM Snowhead Temple Pot Pillars Room Upper 5", "Snowhead Temple Pot Pillars Room Upper 5"),
						M("Pot.png", 408, 552, 24, "MM Snowhead Temple Pot Pillars Room Upper 6", "Snowhead Temple Pot Pillars Room Upper 6")
                    }
                },
				new MapSubRegion
                {
                    Name = "Snow Room",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Snow.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 256, 169, 40, "MM Snowhead Temple SF Snow Room", "Snowhead Temple SF Snow Room"),
						M("Icicle.png", 301, 348, 24, "MM Snowhead Temple Icicle Snow Room 1", "Snowhead Temple Icicle Snow Room 1"),
						M("Icicle.png", 243, 336, 24, "MM Snowhead Temple Icicle Snow Room 2", "Snowhead Temple Icicle Snow Room 2"),
						M("Icicle.png", 281, 302, 24, "MM Snowhead Temple Icicle Snow Room 3", "Snowhead Temple Icicle Snow Room 3"),
						M("Snowball.png", 212, 477, 24, "MM Snowhead Temple Small Snowball Snow Room 1", "Snowhead Temple Small Snowball Snow Room 1"),
						M("Snowball.png", 224, 446, 24, "MM Snowhead Temple Small Snowball Snow Room 2", "Snowhead Temple Small Snowball Snow Room 2"),
						M("Snowball.png", 177, 492, 24, "MM Snowhead Temple Small Snowball Snow Room 3", "Snowhead Temple Small Snowball Snow Room 3"),
						M("Snowball.png", 177, 460, 24, "MM Snowhead Temple Small Snowball Snow Room 4", "Snowhead Temple Small Snowball Snow Room 4"),
						M("Snowball.png", 150, 441, 24, "MM Snowhead Temple Small Snowball Snow Room 5", "Snowhead Temple Small Snowball Snow Room 5"),
						M("Snowball.png", 755, 382, 24, "MM Snowhead Temple Small Snowball Snow Room 6", "Snowhead Temple Small Snowball Snow Room 6"),
						M("Snowball.png", 737, 351, 24, "MM Snowhead Temple Small Snowball Snow Room 7", "Snowhead Temple Small Snowball Snow Room 7"),
						M("Snowball.png", 719, 372, 24, "MM Snowhead Temple Small Snowball Snow Room 8", "Snowhead Temple Small Snowball Snow Room 8")
                    }
                },
				new MapSubRegion
                {
                    Name = "Dinolfos Room",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Dinalfos.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 552, 404, 40, "MM Snowhead Temple SF Dinolfos 1", "Snowhead Temple SF Dinolfos 1"),
						M("Stray_Fairy.png", 411, 220, 40, "MM Snowhead Temple SF Dinolfos 2", "Snowhead Temple SF Dinolfos 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Wizzrobe Room",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Wizzrobe.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Pot.png", 138, 220, 24, "MM Snowhead Temple Pot Wizzrobe 1", "Snowhead Temple Pot Wizzrobe 1"),
						M("Pot.png", 190, 164, 24, "MM Snowhead Temple Pot Wizzrobe 2", "Snowhead Temple Pot Wizzrobe 2"),
						M("Pot.png", 138, 164, 24, "MM Snowhead Temple Pot Wizzrobe 3", "Snowhead Temple Pot Wizzrobe 3"),
						M("Pot.png", 138, 192, 24, "MM Snowhead Temple Pot Wizzrobe 4", "Snowhead Temple Pot Wizzrobe 4"),
						M("Pot.png", 164, 164, 24, "MM Snowhead Temple Pot Wizzrobe 5", "Snowhead Temple Pot Wizzrobe 5")						
                    }
                },
				new MapSubRegion
                {
                    Name = "Boss Room",
                    BackgroundImage = "region maps/MM/Dungeons/Snowhead/Boss.png",
                    DestinationEntranceIds = new List<string> { "MM_BOSS_TEMPLE_SNOWHEAD" },
                    Marks = new List<MapMark>
                    {
						M("Collectible.png", 507, 490, 40, "MM Snowhead Temple Boss HC", "Snowhead Temple Boss HC"),
						M("NPC.png", 447, 490, 40, "MM Snowhead Temple Boss", "Snowhead Temple Boss"),
						M("NPC.png", 387, 490, 40, "MM Oath to Order", "Oath to Order"),
						M("Pot.png", 652, 281, 24, "MM Snowhead Temple Boss Pot 01", "Snowhead Temple Boss Pot 01"),
						M("Pot.png", 270, 281, 24, "MM Snowhead Temple Boss Pot 02", "Snowhead Temple Boss Pot 02"),
						M("Pot.png", 703, 281, 24, "MM Snowhead Temple Boss Pot 03", "Snowhead Temple Boss Pot 03"),
						M("Pot.png", 214, 281, 24, "MM Snowhead Temple Boss Pot 04", "Snowhead Temple Boss Pot 04"),
						M("Pot.png", 455, 98, 24, "MM Snowhead Temple Boss Pot 05", "Snowhead Temple Boss Pot 05"),
						M("Pot.png", 455, 41, 24, "MM Snowhead Temple Boss Pot 06", "Snowhead Temple Boss Pot 06"),
						M("Pot.png", 666, 472, 24, "MM Snowhead Temple Boss Pot 07", "Snowhead Temple Boss Pot 07"),
						M("Pot.png", 666, 74, 24, "MM Snowhead Temple Boss Pot 08", "Snowhead Temple Boss Pot 08"),
						M("Pot.png", 245, 74, 24, "MM Snowhead Temple Boss Pot 09", "Snowhead Temple Boss Pot 09"),
						M("Pot.png", 245, 472, 24, "MM Snowhead Temple Boss Pot 10", "Snowhead Temple Boss Pot 10"),
						M("Pot.png", 432, 439, 24, "MM Snowhead Temple Boss Pot Early 1", "Snowhead Temple Boss Pot Early 1"),
						M("Pot.png", 478, 439, 24, "MM Snowhead Temple Boss Pot Early 2", "Snowhead Temple Boss Pot Early 2"),
						M("Pot.png", 455, 531, 24, "MM Snowhead Temple Boss Pot Early 3", "Snowhead Temple Boss Pot Early 3"),
						M("Pot.png", 455, 465, 24, "MM Snowhead Temple Boss Pot Early 4", "Snowhead Temple Boss Pot Early 4")
                    }
                },
			};
			return mapRegion;
		}
		public static MapRegion GreatBayTemple()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Great Bay Temple";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Entrance",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Entrance.png",
                    DestinationEntranceIds = new List<string> { "MM_TEMPLE_GREAT_BAY" },
                    Marks = new List<MapMark>
                    {
						M("Barrel.png", 499, 491, 24, "MM Great Bay Temple Barrel Entrance 1", "Great Bay Temple Barrel Entrance 1"),
						M("Barrel.png", 499, 467, 24, "MM Great Bay Temple Barrel Entrance 2", "Great Bay Temple Barrel Entrance 2"),
						M("Barrel.png", 523, 491, 24, "MM Great Bay Temple Barrel Entrance 3", "Great Bay Temple Barrel Entrance 3"),
						M("Barrel.png", 523, 467, 24, "MM Great Bay Temple Barrel Entrance 4", "Great Bay Temple Barrel Entrance 4"),
						M("Barrel.png", 523, 164, 24, "MM Great Bay Temple Barrel Entrance 5", "Great Bay Temple Barrel Entrance 5"),
						M("Barrel.png", 523, 140, 24, "MM Great Bay Temple Barrel Entrance 6", "Great Bay Temple Barrel Entrance 6"),
						M("Barrel.png", 499, 164, 24, "MM Great Bay Temple Barrel Entrance 7", "Great Bay Temple Barrel Entrance 7"),
						M("Barrel.png", 499, 140, 24, "MM Great Bay Temple Barrel Entrance 8", "Great Bay Temple Barrel Entrance 8"),
						M("Chest.png", 365, 304, 40, "MM Great Bay Temple Entrance Chest", "Great Bay Temple Entrance Chest"),
						
						ME("Entrance.png", 215, 299, "Entrance shuffle (Gyorg)", "MM_BOSS_TEMPLE_GREAT_BAY"),
						ME("Entrance.png", 653, 310, "Entrance shuffle (Zora Cape)", "MM_GREAT_BAY_FROM_TEMPLE")
                    }
                },
				new MapSubRegion
                {
                    Name = "Water Wheel Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Water_Wheel.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 460, 495, 40, "MM Great Bay Temple SF Water Wheel Platform", "Great Bay Temple SF Water Wheel Platform"),
						M("Stray_Fairy.png", 125, 244, 40, "MM Great Bay Temple SF Water Wheel Skulltula", "Great Bay Temple SF Water Wheel Skulltula"),
						M("Rupee.png", 149, 403, 24, "MM Great Bay Temple Rupee Entrance 1", "Great Bay Temple Rupee Entrance 1"),
						M("Rupee.png", 183, 379, 24, "MM Great Bay Temple Rupee Entrance 2", "Great Bay Temple Rupee Entrance 2"),
						M("Rupee.png", 183, 427, 24, "MM Great Bay Temple Rupee Entrance 3", "Great Bay Temple Rupee Entrance 3"),
						M("Rupee.png", 166, 391, 24, "MM Great Bay Temple Rupee Entrance 4", "Great Bay Temple Rupee Entrance 4"),
						M("Rupee.png", 166, 415, 24, "MM Great Bay Temple Rupee Entrance 5", "Great Bay Temple Rupee Entrance 5")
                    }
                },
				new MapSubRegion
                {
                    Name = "Central Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Central.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 247, 484, 40, "MM Great Bay Temple SF Central Room Barrel", "Great Bay Temple SF Central Room Barrel"),
						M("Stray_Fairy.png", 459, 439, 40, "MM Great Bay Temple SF Central Room Underwater Pot", "Great Bay Temple SF Central Room Underwater Pot"),
						M("Pot.png", 637, 130, 24, "MM Great Bay Temple Pot Central Room 1", "Great Bay Temple Pot Central Room 1"),
						M("Pot.png", 657, 145, 24, "MM Great Bay Temple Pot Central Room 2", "Great Bay Temple Pot Central Room 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Before Wart Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Before_Wart.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Pot.png", 316, 567, 24, "MM Great Bay Temple Pot Red Pipe Before Wart 1", "Great Bay Temple Pot Red Pipe Before Wart 1"),
						M("Pot.png", 337, 554, 24, "MM Great Bay Temple Pot Red Pipe Before Wart 2", "Great Bay Temple Pot Red Pipe Before Wart 2"),
						M("Pot.png", 594, 554, 24, "MM Great Bay Temple Pot Red Pipe Before Wart 3", "Great Bay Temple Pot Red Pipe Before Wart 3"),
						M("Pot.png", 615, 567, 24, "MM Great Bay Temple Pot Red Pipe Before Wart 4", "Great Bay Temple Pot Red Pipe Before Wart 4")
                    }
                },
				new MapSubRegion
                {
                    Name = "Chuchu Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Chuchu.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Pot.png", 424, 152, 24, "MM Great Bay Temple Pot Before Wart 01", "Great Bay Temple Pot Before Wart 01"),
						M("Pot.png", 424, 474, 24, "MM Great Bay Temple Pot Before Wart 02", "Great Bay Temple Pot Before Wart 02"),
						M("Pot.png", 521, 152, 24, "MM Great Bay Temple Pot Before Wart 03", "Great Bay Temple Pot Before Wart 03"),
						M("Pot.png", 521, 474, 24, "MM Great Bay Temple Pot Before Wart 04", "Great Bay Temple Pot Before Wart 04"),
						M("Pot.png", 424, 96, 24, "MM Great Bay Temple Pot Before Wart 05", "Great Bay Temple Pot Before Wart 05"),
						M("Pot.png", 424, 124, 24, "MM Great Bay Temple Pot Before Wart 06", "Great Bay Temple Pot Before Wart 06"),
						M("Pot.png", 521, 96, 24, "MM Great Bay Temple Pot Before Wart 07", "Great Bay Temple Pot Before Wart 07"),
						M("Pot.png", 521, 124, 24, "MM Great Bay Temple Pot Before Wart 08", "Great Bay Temple Pot Before Wart 08"),
						M("Pot.png", 424, 530, 24, "MM Great Bay Temple Pot Before Wart 09", "Great Bay Temple Pot Before Wart 09"),
						M("Pot.png", 521, 530, 24, "MM Great Bay Temple Pot Before Wart 10", "Great Bay Temple Pot Before Wart 10"),
						M("Pot.png", 424, 502, 24, "MM Great Bay Temple Pot Before Wart 11", "Great Bay Temple Pot Before Wart 11"),
						M("Pot.png", 521, 502, 24, "MM Great Bay Temple Pot Before Wart 12", "Great Bay Temple Pot Before Wart 12")
                    }
                },
				new MapSubRegion
                {
                    Name = "Wart Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Wart.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 454, 313, 40, "MM Great Bay Temple Ice Arrow", "Great Bay Temple Ice Arrow"),
						M("Pot.png", 286, 470, 24, "MM Great Bay Temple Pot Wart 1", "Great Bay Temple Pot Wart 1"),
						M("Pot.png", 306, 145, 24, "MM Great Bay Temple Pot Wart 2", "Great Bay Temple Pot Wart 2"),
						M("Pot.png", 306, 490, 24, "MM Great Bay Temple Pot Wart 3", "Great Bay Temple Pot Wart 3"),
						M("Pot.png", 620, 490, 24, "MM Great Bay Temple Pot Wart 4", "Great Bay Temple Pot Wart 4"),
						M("Pot.png", 640, 165, 24, "MM Great Bay Temple Pot Wart 5", "Great Bay Temple Pot Wart 5"),
						M("Pot.png", 640, 470, 24, "MM Great Bay Temple Pot Wart 6", "Great Bay Temple Pot Wart 6"),
						M("Pot.png", 286, 165, 24, "MM Great Bay Temple Pot Wart 7", "Great Bay Temple Pot Wart 7"),
						M("Pot.png", 620, 145, 24, "MM Great Bay Temple Pot Wart 8", "Great Bay Temple Pot Wart 8")
                    }
                },
				new MapSubRegion
                {
                    Name = "Map Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Map.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 536, 177, 40, "MM Great Bay Temple SF Map Room Pot", "Great Bay Temple SF Map Room Pot"),
						M("Chest.png", 387, 247, 40, "MM Great Bay Temple Map", "Great Bay Temple Map"),
						M("Pot.png", 116, 265, 24, "MM Great Bay Temple Pot Map Room Surface 1", "Great Bay Temple Pot Map Room Surface 1"),
						M("Pot.png", 139, 247, 24, "MM Great Bay Temple Pot Map Room Surface 2", "Great Bay Temple Pot Map Room Surface 2"),
						M("Pot.png", 507, 193, 24, "MM Great Bay Temple Pot Map Room Surface 3", "Great Bay Temple Pot Map Room Surface 3"),
						M("Pot.png", 240, 493, 24, "MM Great Bay Temple Pot Map Room Water 1", "Great Bay Temple Pot Map Room Water 1"),
						M("Pot.png", 218, 493, 24, "MM Great Bay Temple Pot Map Room Water 2", "Great Bay Temple Pot Map Room Water 2"),
						M("Pot.png", 414, 519, 24, "MM Great Bay Temple Pot Map Room Water 3", "Great Bay Temple Pot Map Room Water 3"),
						M("Pot.png", 519, 495, 24, "MM Great Bay Temple Pot Map Room Water 4", "Great Bay Temple Pot Map Room Water 4"),
						M("Pot.png", 713, 538, 24, "MM Great Bay Temple Pot Map Room Water 5", "Great Bay Temple Pot Map Room Water 5"),
						M("Pot.png", 689, 482, 24, "MM Great Bay Temple Pot Map Room Water 6", "Great Bay Temple Pot Map Room Water 6"),
						M("Pot.png", 715, 486, 24, "MM Great Bay Temple Pot Map Room Water 7", "Great Bay Temple Pot Map Room Water 7"),
						M("Pot.png", 739, 534, 24, "MM Great Bay Temple Pot Map Room Water 8", "Great Bay Temple Pot Map Room Water 8")
                    }
                },
				new MapSubRegion
                {
                    Name = "Bio Baba Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Bio_baba.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 453, 137, 40, "MM Great Bay Temple Baba Chest", "Great Bay Temple Baba Chest")
                    }
                },
				new MapSubRegion
                {
                    Name = "Boss Key Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Boss_Key.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 543, 554, 40, "MM Great Bay Temple SF Compass Room Pot", "Great Bay Temple SF Compass Room Pot"),
						M("Chest.png", 345, 149, 40, "MM Great Bay Temple Compass", "Great Bay Temple Compass"),
						M("Chest.png", 442, 371, 40, "MM Great Bay Temple Compass Room Underwater", "Great Bay Temple Compass Room Underwater"),
						M("Chest.png", 690, 324, 40, "MM Great Bay Temple Boss Key", "Great Bay Temple Boss Key"),
						M("Rupee.png", 549, 525, 24, "MM Great Bay Temple Rupee Compass Room 1", "Great Bay Temple Rupee Compass Room 1"),
						M("Rupee.png", 549, 597, 24, "MM Great Bay Temple Rupee Compass Room 2", "Great Bay Temple Rupee Compass Room 2"),
						M("Pot.png", 312, 478, 24, "MM Great Bay Temple Pot Compass Room Surface 1", "Great Bay Temple Pot Compass Room Surface 1"),
						M("Pot.png", 312, 454, 24, "MM Great Bay Temple Pot Compass Room Surface 2", "Great Bay Temple Pot Compass Room Surface 2"),
						M("Pot.png", 587, 478, 24, "MM Great Bay Temple Pot Compass Room Surface 3", "Great Bay Temple Pot Compass Room Surface 3"),
						M("Pot.png", 587, 454, 24, "MM Great Bay Temple Pot Compass Room Surface 4", "Great Bay Temple Pot Compass Room Surface 4"),
						M("Pot.png", 401, 381, 24, "MM Great Bay Temple Pot Compass Room Water 1", "Great Bay Temple Pot Compass Room Water 1"),
						M("Pot.png", 450, 414, 24, "MM Great Bay Temple Pot Compass Room Water 2", "Great Bay Temple Pot Compass Room Water 2"),
						M("Pot.png", 502, 381, 24, "MM Great Bay Temple Pot Compass Room Water 3", "Great Bay Temple Pot Compass Room Water 3"),
						M("Icicle.png", 702, 78, 24, "MM Great Bay Temple Icicle Compass Room 1", "Great Bay Temple Icicle Compass Room 1"),
						M("Icicle.png", 596, 148, 24, "MM Great Bay Temple Icicle Compass Room 2", "Great Bay Temple Icicle Compass Room 2"),
						M("Icicle.png", 686, 122, 24, "MM Great Bay Temple Icicle Compass Room 3", "Great Bay Temple Icicle Compass Room 3"),
						M("Icicle.png", 666, 92, 24, "MM Great Bay Temple Icicle Compass Room 4", "Great Bay Temple Icicle Compass Room 4"),
						M("Icicle.png", 652, 137, 24, "MM Great Bay Temple Icicle Compass Room 5", "Great Bay Temple Icicle Compass Room 5")
                    }
                },
				new MapSubRegion
                {
                    Name = "Gekko Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Geeko.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Crate.png", 279, 172, 24, "MM Great Bay Temple Crate Before Boss Key 01", "Great Bay Temple Crate Before Boss Key 01"),
						M("Crate.png", 279, 481, 24, "MM Great Bay Temple Crate Before Boss Key 02", "Great Bay Temple Crate Before Boss Key 02"),
						M("Crate.png", 303, 148, 24, "MM Great Bay Temple Crate Before Boss Key 03", "Great Bay Temple Crate Before Boss Key 03"),
						M("Crate.png", 303, 505, 24, "MM Great Bay Temple Crate Before Boss Key 04", "Great Bay Temple Crate Before Boss Key 04"),
						M("Crate.png", 623, 148, 24, "MM Great Bay Temple Crate Before Boss Key 05", "Great Bay Temple Crate Before Boss Key 05"),
						M("Crate.png", 623, 505, 24, "MM Great Bay Temple Crate Before Boss Key 06", "Great Bay Temple Crate Before Boss Key 06"),
						M("Crate.png", 647, 172, 24, "MM Great Bay Temple Crate Before Boss Key 07", "Great Bay Temple Crate Before Boss Key 07"),
						M("Crate.png", 647, 481, 24, "MM Great Bay Temple Crate Before Boss Key 08", "Great Bay Temple Crate Before Boss Key 08"),
						M("Crate.png", 279, 148, 24, "MM Great Bay Temple Crate Before Boss Key 09", "Great Bay Temple Crate Before Boss Key 09"),
						M("Crate.png", 279, 505, 24, "MM Great Bay Temple Crate Before Boss Key 10", "Great Bay Temple Crate Before Boss Key 10"),
						M("Crate.png", 303, 172, 24, "MM Great Bay Temple Crate Before Boss Key 11", "Great Bay Temple Crate Before Boss Key 11"),
						M("Crate.png", 303, 481, 24, "MM Great Bay Temple Crate Before Boss Key 12", "Great Bay Temple Crate Before Boss Key 12"),
						M("Crate.png", 623, 172, 24, "MM Great Bay Temple Crate Before Boss Key 13", "Great Bay Temple Crate Before Boss Key 13"),
						M("Crate.png", 623, 481, 24, "MM Great Bay Temple Crate Before Boss Key 14", "Great Bay Temple Crate Before Boss Key 14"),
						M("Crate.png", 647, 148, 24, "MM Great Bay Temple Crate Before Boss Key 15", "Great Bay Temple Crate Before Boss Key 15"),
						M("Crate.png", 647, 505, 24, "MM Great Bay Temple Crate Before Boss Key 16", "Great Bay Temple Crate Before Boss Key 16")
                    }
                },
				new MapSubRegion
                {
                    Name = "Red Pipe Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Red_Pipe.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Barrel.png", 321, 390, 24, "MM Great Bay Temple Barrel Chu Room 1", "Great Bay Temple Barrel Chu Room 1"),
						M("Barrel.png", 587, 492, 24, "MM Great Bay Temple Barrel Chu Room 2", "Great Bay Temple Barrel Chu Room 2"),
						M("Barrel.png", 587, 460, 24, "MM Great Bay Temple Barrel Chu Room 3", "Great Bay Temple Barrel Chu Room 3"),
						M("Barrel.png", 321, 444, 24, "MM Great Bay Temple Barrel Chu Room 4", "Great Bay Temple Barrel Chu Room 4"),
						M("Barrel.png", 321, 480, 24, "MM Great Bay Temple Barrel Chu Room 5", "Great Bay Temple Barrel Chu Room 5"),
						M("Crate.png", 619, 524, 24, "MM Great Bay Temple Crate Chu Room 1", "Great Bay Temple Crate Chu Room 1"),
						M("Crate.png", 587, 524, 24, "MM Great Bay Temple Crate Chu Room 2", "Great Bay Temple Crate Chu Room 2"),
						M("Crate.png", 290, 524, 24, "MM Great Bay Temple Crate Chu Room 3", "Great Bay Temple Crate Chu Room 3"),
						M("Crate.png", 290, 460, 24, "MM Great Bay Temple Crate Chu Room 4", "Great Bay Temple Crate Chu Room 4"),
						M("Crate.png", 290, 492, 24, "MM Great Bay Temple Crate Chu Room 5", "Great Bay Temple Crate Chu Room 5")
                    }
                },
				new MapSubRegion
                {
                    Name = "Green Pipe Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Green_Pipe.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Barrel.png", 437, 280, 24, "MM Great Bay Temple Barrel Green Pipe 1 1", "Great Bay Temple Barrel Green Pipe 1 1"),
						M("Barrel.png", 496, 286, 24, "MM Great Bay Temple Barrel Green Pipe 1 2", "Great Bay Temple Barrel Green Pipe 1 2"),
						M("Barrel.png", 487, 270, 24, "MM Great Bay Temple Barrel Green Pipe 1 3", "Great Bay Temple Barrel Green Pipe 1 3"),
						M("Chest.png", 461, 155, 40, "MM Great Bay Temple Green Pipe 1 Chest", "Great Bay Temple Green Pipe 1 Chest"),
						M("Rupee.png", 305, 108, 24, "MM Great Bay Temple Rupee Hookshot 1", "Great Bay Temple Rupee Hookshot 1"),
						M("Rupee.png", 665, 105, 24, "MM Great Bay Temple Rupee Hookshot 2", "Great Bay Temple Rupee Hookshot 2"),
						M("Pot.png", 224, 565, 24, "MM Great Bay Temple Pot Green Pipe 1 1", "Great Bay Temple Pot Green Pipe 1 1"),
						M("Pot.png", 706, 565, 24, "MM Great Bay Temple Pot Green Pipe 1 2", "Great Bay Temple Pot Green Pipe 1 2"),
						M("Pot.png", 316, 409, 24, "MM Great Bay Temple Pot Green Pipe 1 3", "Great Bay Temple Pot Green Pipe 1 3"),
						M("Pot.png", 628, 409, 24, "MM Great Bay Temple Pot Green Pipe 1 4", "Great Bay Temple Pot Green Pipe 1 4")
                    }
                },
				new MapSubRegion
                {
                    Name = "Green Pipe Water Wheel Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Green_Pipe_2.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Barrel.png", 380, 272, 24, "MM Great Bay Temple Barrel Green Pipe 2 1", "Great Bay Temple Barrel Green Pipe 2 1"),
						M("Barrel.png", 357, 284, 24, "MM Great Bay Temple Barrel Green Pipe 2 2", "Great Bay Temple Barrel Green Pipe 2 2"),
						M("Chest.png", 649, 408, 40, "MM Great Bay Temple Green Pipe 2 Lower Chest", "Great Bay Temple Green Pipe 2 Lower Chest"),
						M("Chest.png", 558, 112, 40, "MM Great Bay Temple Green Pipe 2 Upper Chest", "Great Bay Temple Green Pipe 2 Upper Chest"),
						M("Pot.png", 549, 500, 24, "MM Great Bay Temple Pot Green Pipe 2 1", "Great Bay Temple Pot Green Pipe 2 1"),
						M("Pot.png", 531, 525, 24, "MM Great Bay Temple Pot Green Pipe 2 2", "Great Bay Temple Pot Green Pipe 2 2"),
						M("Pot.png", 252, 442, 24, "MM Great Bay Temple Pot Green Pipe 2 3", "Great Bay Temple Pot Green Pipe 2 3"),
						M("Pot.png", 230, 458, 24, "MM Great Bay Temple Pot Green Pipe 2 4", "Great Bay Temple Pot Green Pipe 2 4"),
						M("Pot.png", 274, 454, 24, "MM Great Bay Temple Pot Green Pipe 2 5", "Great Bay Temple Pot Green Pipe 2 5"),
						M("Pot.png", 252, 470, 24, "MM Great Bay Temple Pot Green Pipe 2 6", "Great Bay Temple Pot Green Pipe 2 6"),
						M("Pot.png", 331, 374, 24, "MM Great Bay Temple Pot Green Pipe 2 7", "Great Bay Temple Pot Green Pipe 2 7"),
						M("Pot.png", 353, 380, 24, "MM Great Bay Temple Pot Green Pipe 2 8", "Great Bay Temple Pot Green Pipe 2 8")
                    }
                },
				new MapSubRegion
                {
                    Name = "Moving Platforms Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Moving_Platform.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 325, 541, 40, "MM Great Bay Temple SF Green Pipe 3 Barrel", "Great Bay Temple SF Green Pipe 3 Barrel"),
						M("Chest.png", 688, 349, 40, "MM Great Bay Temple Green Pipe 3 Chest", "Great Bay Temple Green Pipe 3 Chest"),
						M("Pot.png", 145, 477, 24, "MM Great Bay Temple Pot Green Pipe 3 Lower", "Great Bay Temple Pot Green Pipe 3 Lower"),
						M("Pot.png", 626, 263, 24, "MM Great Bay Temple Pot Green Pipe 3 Upper 1", "Great Bay Temple Pot Green Pipe 3 Upper 1"),
						M("Pot.png", 656, 263, 24, "MM Great Bay Temple Pot Green Pipe 3 Upper 2", "Great Bay Temple Pot Green Pipe 3 Upper 2"),
						M("Crate.png", 696, 518, 24, "MM Great Bay Temple Crate Green Pipe 1", "Great Bay Temple Crate Green Pipe 1"),
						M("Crate.png", 736, 519, 24, "MM Great Bay Temple Crate Green Pipe 2", "Great Bay Temple Crate Green Pipe 2"),
						M("Crate.png", 685, 494, 24, "MM Great Bay Temple Crate Green Pipe 3", "Great Bay Temple Crate Green Pipe 3"),
						M("Crate.png", 725, 495, 24, "MM Great Bay Temple Crate Green Pipe 4", "Great Bay Temple Crate Green Pipe 4"),
						M("Crate.png", 145, 501, 24, "MM Great Bay Temple Crate Green Pipe 5", "Great Bay Temple Crate Green Pipe 5"),
						M("Crate.png", 173, 502, 24, "MM Great Bay Temple Crate Green Pipe 6", "Great Bay Temple Crate Green Pipe 6"),
						M("Crate.png", 201, 503, 24, "MM Great Bay Temple Crate Green Pipe 7", "Great Bay Temple Crate Green Pipe 7")
                    }
                },
				new MapSubRegion
                {
                    Name = "Pre-Boss Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Pre_Boss.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Stray_Fairy.png", 473, 257, 40, "MM Great Bay Temple SF Pre-Boss Above Water", "Great Bay Temple SF Pre-Boss Above Water"),
						M("Stray_Fairy.png", 196, 488, 40, "MM Great Bay Temple SF Pre-Boss Underwater", "Great Bay Temple SF Pre-Boss Underwater"),
						M("Pot.png", 447, 509, 24, "MM Great Bay Temple Pot Pre-Boss 1", "Great Bay Temple Pot Pre-Boss 1"),
						M("Pot.png", 512, 509, 24, "MM Great Bay Temple Pot Pre-Boss 2", "Great Bay Temple Pot Pre-Boss 2"),
						M("Pot.png", 344, 547, 24, "MM Great Bay Temple Pot Pre-Boss 3", "Great Bay Temple Pot Pre-Boss 3"),
						M("Pot.png", 619, 547, 24, "MM Great Bay Temple Pot Pre-Boss 4", "Great Bay Temple Pot Pre-Boss 4"),
						M("Pot.png", 344, 523, 24, "MM Great Bay Temple Pot Pre-Boss 5", "Great Bay Temple Pot Pre-Boss 5"),
						M("Pot.png", 619, 523, 24, "MM Great Bay Temple Pot Pre-Boss 6", "Great Bay Temple Pot Pre-Boss 6"),
						M("Pot.png", 447, 485, 24, "MM Great Bay Temple Pot Pre-Boss 7", "Great Bay Temple Pot Pre-Boss 7"),
						M("Pot.png", 512, 485, 24, "MM Great Bay Temple Pot Pre-Boss 8", "Great Bay Temple Pot Pre-Boss 8"),
						
						ME("Entrance.png", 473, 357, "Entrance shuffle (Gyorg)", "MM_BOSS_TEMPLE_GREAT_BAY"),
                    }
                },
				new MapSubRegion
                {
                    Name = "Boss Room",
                    BackgroundImage = "region maps/MM/Dungeons/Great_Bay/Boss.png",
                    DestinationEntranceIds = new List<string> { "MM_BOSS_TEMPLE_GREAT_BAY" },
                    Marks = new List<MapMark>
                    {
						M("Collectible.png", 464, 300, 40, "MM Great Bay Temple Boss HC", "Great Bay Temple Boss HC"),
						M("NPC.png", 520, 300, 40, "MM Great Bay Temple Boss", "Great Bay Temple Boss"),
						M("NPC.png", 408, 300, 40, "MM Oath to Order", "Oath to Order"),
						M("Pot.png", 524, 271, 24, "MM Great Bay Temple Boss Pot 1", "Great Bay Temple Boss Pot 1"),
						M("Pot.png", 524, 346, 24, "MM Great Bay Temple Boss Pot 2", "Great Bay Temple Boss Pot 2"),
						M("Pot.png", 425, 346, 24, "MM Great Bay Temple Boss Pot 3", "Great Bay Temple Boss Pot 3"),
						M("Pot.png", 425, 271, 24, "MM Great Bay Temple Boss Pot 4", "Great Bay Temple Boss Pot 4"),
						M("Pot.png", 664, 174, 24, "MM Great Bay Temple Boss Pot Underwater 1", "Great Bay Temple Boss Pot Underwater 1"),
						M("Pot.png", 274, 174, 24, "MM Great Bay Temple Boss Pot Underwater 2", "Great Bay Temple Boss Pot Underwater 2"),
						M("Pot.png", 182, 531, 24, "MM Great Bay Temple Boss Pot Underwater 3", "Great Bay Temple Boss Pot Underwater 3"),
						M("Pot.png", 772, 531, 24, "MM Great Bay Temple Boss Pot Underwater 4", "Great Bay Temple Boss Pot Underwater 4")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion StoneTowerTemple()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Stone Tower Temple";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Entrance",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower/Entrance.png",
                    DestinationEntranceIds = new List<string> { "MM_TEMPLE_STONE_TOWER" },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 453, 339, 40, "MM Stone Tower Temple Entrance Chest", "Stone Tower Temple Entrance Chest"),
						M("Chest.png", 453, 388, 40, "MM Stone Tower Temple Entrance Switch Chest", "Stone Tower Temple Entrance Switch Chest"),
						M("Crate.png", 618, 208, 24, "MM Stone Tower Temple Crate Entrance 1", "Stone Tower Temple Crate Entrance 1"),
						M("Crate.png", 642, 208, 24, "MM Stone Tower Temple Crate Entrance 2", "Stone Tower Temple Crate Entrance 2"),
						M("Grass.png", 595, 224, 24, "MM Stone Tower Temple Grass Entrance 1", "Stone Tower Temple Grass Entrance 1"),
						M("Grass.png", 597, 203, 24, "MM Stone Tower Temple Grass Entrance 2", "Stone Tower Temple Grass Entrance 2"),
						M("Grass.png", 624, 224, 24, "MM Stone Tower Temple Grass Entrance 3", "Stone Tower Temple Grass Entrance 3"),
						M("Pot.png", 480, 277, 24, "MM Stone Tower Temple Pot Entrance 1", "Stone Tower Temple Pot Entrance 1"),
						M("Pot.png", 443, 277, 24, "MM Stone Tower Temple Pot Entrance 2", "Stone Tower Temple Pot Entrance 2"),
						
						ME("Entrance.png", 456, 578, "Entrance shuffle (Stone Tower)", "MM_STONE_TOWER_FROM_TEMPLE")
                    }
                },
				new MapSubRegion
                {
                    Name = "Maze Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower/Maze.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Crate.png", 672, 434, 24, "MM Stone Tower Temple Crate Maze Large 1", "Stone Tower Temple Crate Maze Large 1"),
						M("Crate.png", 704, 434, 24, "MM Stone Tower Temple Crate Maze Large 2", "Stone Tower Temple Crate Maze Large 2"),
						M("Crate.png", 736, 434, 24, "MM Stone Tower Temple Crate Maze Large 3", "Stone Tower Temple Crate Maze Large 3"),
						M("Crate.png", 768, 434, 24, "MM Stone Tower Temple Crate Maze Large 4", "Stone Tower Temple Crate Maze Large 4"),
						M("Crate.png", 800, 434, 24, "MM Stone Tower Temple Crate Maze Large 5", "Stone Tower Temple Crate Maze Large 5"),
						M("Crate.png", 721, 467, 24, "MM Stone Tower Temple Crate Maze Small 1", "Stone Tower Temple Crate Maze Small 1"),
						M("Crate.png", 785, 467, 24, "MM Stone Tower Temple Crate Maze Small 2", "Stone Tower Temple Crate Maze Small 2"),
						M("Grass.png", 268, 220, 24, "MM Stone Tower Temple Grass Garden 1", "Stone Tower Temple Grass Garden 1"),
						M("Grass.png", 253, 202, 24, "MM Stone Tower Temple Grass Garden 2", "Stone Tower Temple Grass Garden 2"),
						M("Grass.png", 263, 246, 24, "MM Stone Tower Temple Grass Garden 3", "Stone Tower Temple Grass Garden 3"),
						M("Grass.png", 244, 229, 24, "MM Stone Tower Temple Grass Garden 4", "Stone Tower Temple Grass Garden 4"),
						M("Grass.png", 293, 229, 24, "MM Stone Tower Temple Grass Garden 5", "Stone Tower Temple Grass Garden 5"),
						M("Grass.png", 281, 196, 24, "MM Stone Tower Temple Grass Garden 6", "Stone Tower Temple Grass Garden 6")
                    }
                },
				new MapSubRegion
                {
                    Name = "Lava Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower/Lava.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 464, 541, 40, "MM Stone Tower Temple Map", "Stone Tower Temple Map"),
						M("Chest.png", 465, 314, 40, "MM Stone Tower Temple Under West Garden Lava Chest", "Stone Tower Temple Under West Garden Lava Chest"),
						M("Chest.png", 333, 113, 40, "MM Stone Tower Temple Under West Garden Ledge Chest", "Stone Tower Temple Under West Garden Ledge Chest"),
						M("Pot.png", 441, 160, 24, "MM Stone Tower Temple Pot Lava Room 1", "Stone Tower Temple Pot Lava Room 1"),
						M("Pot.png", 441, 190, 24, "MM Stone Tower Temple Pot Lava Room 2", "Stone Tower Temple Pot Lava Room 2"),
						M("Pot.png", 504, 160, 24, "MM Stone Tower Temple Pot Lava Room 3", "Stone Tower Temple Pot Lava Room 3"),
						M("Pot.png", 504, 190, 24, "MM Stone Tower Temple Pot Lava Room 4", "Stone Tower Temple Pot Lava Room 4"),
						M("Pot.png", 441, 487, 24, "MM Stone Tower Temple Pot Lava Room After Block 1", "Stone Tower Temple Pot Lava Room After Block 1"),
						M("Pot.png", 504, 487, 24, "MM Stone Tower Temple Pot Lava Room After Block 2", "Stone Tower Temple Pot Lava Room After Block 2"),
						M("Pot.png", 504, 440, 24, "MM Stone Tower Temple Pot Lava Room After Block 3", "Stone Tower Temple Pot Lava Room After Block 3"),
						M("Pot.png", 441, 440, 24, "MM Stone Tower Temple Pot Lava Room After Block 4", "Stone Tower Temple Pot Lava Room After Block 4")
                    }
                },
				new MapSubRegion
                {
                    Name = "Central Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower/Central.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 183, 281, 40, "MM Stone Tower Temple Center Across Water Chest", "Stone Tower Temple Center Across Water Chest"),
						M("Chest.png", 547, 517, 40, "MM Stone Tower Temple Center Sun Block Chest", "Stone Tower Temple Center Sun Block Chest"),
						M("Chest.png", 455, 494, 40, "MM Stone Tower Temple Water Bridge Chest", "Stone Tower Temple Water Bridge Chest"),
						M("Crate.png", 225, 131, 24, "MM Stone Tower Temple Crate Water 1", "Stone Tower Temple Crate Water 1"),
						M("Crate.png", 225, 107, 24, "MM Stone Tower Temple Crate Water 2", "Stone Tower Temple Crate Water 2"),
						M("Crate.png", 225, 83, 24, "MM Stone Tower Temple Crate Water 3", "Stone Tower Temple Crate Water 3"),
						M("Rupee.png", 640, 525, 24, "MM Stone Tower Temple Rupee Center Room Left", "Stone Tower Temple Rupee Center Room Left"),
						M("Rupee.png", 220, 525, 24, "MM Stone Tower Temple Rupee Center Room Right", "Stone Tower Temple Rupee Center Room Right")
                    }
                },
				new MapSubRegion
                {
                    Name = "Water Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower/Water.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 771, 358, 40, "MM Stone Tower Temple Compass", "Stone Tower Temple Compass"),
						M("Chest.png", 552, 354, 40, "MM Stone Tower Temple Water Sun Switch Chest", "Stone Tower Temple Water Sun Switch Chest"),
						M("Pot.png", 207, 269, 24, "MM Stone Tower Temple Pot Water Room Bridge 1", "Stone Tower Temple Pot Water Room Bridge 1"),
						M("Pot.png", 218, 231, 24, "MM Stone Tower Temple Pot Water Room Bridge 2", "Stone Tower Temple Pot Water Room Bridge 2"),
						M("Pot.png", 363, 546, 24, "MM Stone Tower Temple Pot Water Room Underwater Lower 1", "Stone Tower Temple Pot Water Room Underwater Lower 1"),
						M("Pot.png", 341, 522, 24, "MM Stone Tower Temple Pot Water Room Underwater Lower 2", "Stone Tower Temple Pot Water Room Underwater Lower 2"),
						M("Pot.png", 363, 522, 24, "MM Stone Tower Temple Pot Water Room Underwater Lower 3", "Stone Tower Temple Pot Water Room Underwater Lower 3"),
						M("Pot.png", 480, 183, 24, "MM Stone Tower Temple Pot Water Room Underwater Upper 1", "Stone Tower Temple Pot Water Room Underwater Upper 1"),
						M("Pot.png", 502, 183, 24, "MM Stone Tower Temple Pot Water Room Underwater Upper 2", "Stone Tower Temple Pot Water Room Underwater Upper 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Mirrors Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower/Mirrors.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 423, 372, 40, "MM Stone Tower Temple Mirrors Room Center Chest", "Stone Tower Temple Mirrors Room Center Chest"),
						M("Chest.png", 467, 87, 40, "MM Stone Tower Temple Mirrors Room Right Chest", "Stone Tower Temple Mirrors Room Right Chest"),
						M("Crate.png", 239, 305, 24, "MM Stone Tower Temple Crate Mirrors 1", "Stone Tower Temple Crate Mirrors 1"),
						M("Crate.png", 231, 546, 24, "MM Stone Tower Temple Crate Mirrors 2", "Stone Tower Temple Crate Mirrors 2"),
						M("Pot.png", 810, 455, 24, "MM Stone Tower Temple Pot Mirror Room 1", "Stone Tower Temple Pot Mirror Room 1"),
						M("Pot.png", 810, 396, 24, "MM Stone Tower Temple Pot Mirror Room 2", "Stone Tower Temple Pot Mirror Room 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Wind Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower/Wind.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 176, 294, 40, "MM Stone Tower Temple Wind Room Jail Chest", "Stone Tower Temple Wind Room Jail Chest"),
						M("Chest.png", 409, 278, 40, "MM Stone Tower Temple Wind Room Ledge Chest", "Stone Tower Temple Wind Room Ledge Chest"),
						M("Rupee.png", 470, 286, 24, "MM Stone Tower Temple Rupee Wind Room 1", "Stone Tower Temple Rupee Wind Room 1"),
						M("Rupee.png", 510, 262, 24, "MM Stone Tower Temple Rupee Wind Room 2", "Stone Tower Temple Rupee Wind Room 2"),
						M("Rupee.png", 510, 286, 24, "MM Stone Tower Temple Rupee Wind Room 3", "Stone Tower Temple Rupee Wind Room 3"),
						M("Rupee.png", 510, 310, 24, "MM Stone Tower Temple Rupee Wind Room 4", "Stone Tower Temple Rupee Wind Room 4"),
						M("Rupee.png", 490, 274, 24, "MM Stone Tower Temple Rupee Wind Room 5", "Stone Tower Temple Rupee Wind Room 5"),
						M("Rupee.png", 490, 298, 24, "MM Stone Tower Temple Rupee Wind Room 6", "Stone Tower Temple Rupee Wind Room 6"),
						M("Pot.png", 209, 158, 24, "MM Stone Tower Temple Pot Wind Room 1", "Stone Tower Temple Pot Wind Room 1"),
						M("Pot.png", 169, 158, 24, "MM Stone Tower Temple Pot Wind Room 2", "Stone Tower Temple Pot Wind Room 2"),
						M("Pot.png", 209, 134, 24, "MM Stone Tower Temple Pot Wind Room 3", "Stone Tower Temple Pot Wind Room 3"),
						M("Pot.png", 169, 134, 24, "MM Stone Tower Temple Pot Wind Room 4", "Stone Tower Temple Pot Wind Room 4")
                    }
                },
				new MapSubRegion
                {
                    Name = "Garo Master Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower/Garo.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 457, 304, 40, "MM Stone Tower Temple Light Arrow", "Stone Tower Temple Light Arrow")
                    }
                },
				new MapSubRegion
                {
                    Name = "Hiploop Bridge Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower/Hiploop.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 276, 490, 40, "MM Stone Tower Temple Before Water Bridge Chest", "Stone Tower Temple Before Water Bridge Chest"),
						M("Rupee.png", 230, 341, 24, "MM Stone Tower Temple Rupee Before Water Bridge 1", "Stone Tower Temple Rupee Before Water Bridge 1"),
						M("Rupee.png", 245, 289, 24, "MM Stone Tower Temple Rupee Before Water Bridge 2", "Stone Tower Temple Rupee Before Water Bridge 2"),
						M("Rupee.png", 261, 341, 24, "MM Stone Tower Temple Rupee Before Water Bridge 3", "Stone Tower Temple Rupee Before Water Bridge 3"),
						M("Rupee.png", 276, 289, 24, "MM Stone Tower Temple Rupee Before Water Bridge 4", "Stone Tower Temple Rupee Before Water Bridge 4"),
						M("Rupee.png", 394, 345, 24, "MM Stone Tower Temple Rupee Before Water Bridge 5", "Stone Tower Temple Rupee Before Water Bridge 5"),
						M("Rupee.png", 399, 292, 24, "MM Stone Tower Temple Rupee Before Water Bridge 6", "Stone Tower Temple Rupee Before Water Bridge 6"),
						M("Rupee.png", 425, 345, 24, "MM Stone Tower Temple Rupee Before Water Bridge 7", "Stone Tower Temple Rupee Before Water Bridge 7"),
						M("Rupee.png", 430, 292, 24, "MM Stone Tower Temple Rupee Before Water Bridge 8", "Stone Tower Temple Rupee Before Water Bridge 8"),
						M("Pot.png", 254, 487, 24, "MM Stone Tower Temple Pot Before Water Bridge 1", "Stone Tower Temple Pot Before Water Bridge 1"),
						M("Pot.png", 232, 487, 24, "MM Stone Tower Temple Pot Before Water Bridge 2", "Stone Tower Temple Pot Before Water Bridge 2"),
						M("Pot.png", 245, 514, 24, "MM Stone Tower Temple Pot Before Water Bridge 3", "Stone Tower Temple Pot Before Water Bridge 3"),
						M("Pot.png", 223, 514, 24, "MM Stone Tower Temple Pot Before Water Bridge 4", "Stone Tower Temple Pot Before Water Bridge 4"),
						M("Pot.png", 210, 487, 24, "MM Stone Tower Temple Pot Before Water Bridge 5", "Stone Tower Temple Pot Before Water Bridge 5"),
						M("Pot.png", 201, 514, 24, "MM Stone Tower Temple Pot Before Water Bridge 6", "Stone Tower Temple Pot Before Water Bridge 6"),
						M("Pot.png", 179, 514, 24, "MM Stone Tower Temple Pot Before Water Bridge 7", "Stone Tower Temple Pot Before Water Bridge 7"),
						M("Pot.png", 188, 487, 24, "MM Stone Tower Temple Pot Before Water Bridge 8", "Stone Tower Temple Pot Before Water Bridge 8")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion InvertedStoneTowerTemple()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Inverted Stone Tower Temple";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Entrance",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower_Inverted/Entrance.png",
                    DestinationEntranceIds = new List<string> { "MM_TEMPLE_STONE_TOWER_INVERTED" },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 461, 286, 40, "MM Stone Tower Temple Inverted Entrance Chest", "Stone Tower Temple Inverted Entrance Chest"),
						M("Crate.png", 260, 276, 24, "MM Stone Tower Temple Inverted Crate Entrance Left 1", "Stone Tower Temple Inverted Crate Entrance Left 1"),
						M("Crate.png", 284, 276, 24, "MM Stone Tower Temple Inverted Crate Entrance Left 2", "Stone Tower Temple Inverted Crate Entrance Left 2"),
						M("Crate.png", 308, 276, 24, "MM Stone Tower Temple Inverted Crate Entrance Left 3", "Stone Tower Temple Inverted Crate Entrance Left 3"),
						M("Crate.png", 647, 66, 24, "MM Stone Tower Temple Inverted Crate Entrance Right 1", "Stone Tower Temple Inverted Crate Entrance Right 1"),
						M("Crate.png", 647, 90, 24, "MM Stone Tower Temple Inverted Crate Entrance Right 2", "Stone Tower Temple Inverted Crate Entrance Right 2"),
						
						ME("Entrance.png", 461, 540, "Entrance shuffle (Inverted Stone Tower)", "MM_STONE_TOWER_INVERTED_FROM_TEMPLE"),
						ME("Entrance.png", 461, 246, "Entrance shuffle (Twinmold)", "MM_BOSS_TEMPLE_STONE_TOWER")
                    }
                },
				new MapSubRegion
                {
                    Name = "Water Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower_Inverted/Water.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 443, 481, 40, "MM Stone Tower Temple Inverted East Lower Chest", "Stone Tower Temple Inverted East Lower Chest"),
						M("Chest.png", 443, 230, 40, "MM Stone Tower Temple Inverted East Middle Chest", "Stone Tower Temple Inverted East Middle Chest"),
						M("Chest.png", 443, 199, 40, "MM Stone Tower Temple Inverted East Upper Chest", "Stone Tower Temple Inverted East Upper Chest"),
						M("Crate.png", 363, 425, 24, "MM Stone Tower Temple Inverted Crate Before Gomess 1", "Stone Tower Temple Inverted Crate Before Gomess 1"),
						M("Crate.png", 339, 425, 24, "MM Stone Tower Temple Inverted Crate Before Gomess 2", "Stone Tower Temple Inverted Crate Before Gomess 2"),
						M("Crate.png", 315, 425, 24, "MM Stone Tower Temple Inverted Crate Before Gomess 3", "Stone Tower Temple Inverted Crate Before Gomess 3"),
						M("Crate.png", 315, 401, 24, "MM Stone Tower Temple Inverted Crate Before Gomess 4", "Stone Tower Temple Inverted Crate Before Gomess 4"),
						M("Crate.png", 363, 401, 24, "MM Stone Tower Temple Inverted Crate Before Gomess 5", "Stone Tower Temple Inverted Crate Before Gomess 5"),
						M("Crate.png", 339, 401, 24, "MM Stone Tower Temple Inverted Crate Before Gomess 6", "Stone Tower Temple Inverted Crate Before Gomess 6"),
						M("Rupee.png", 537, 142, 24, "MM Stone Tower Temple Inverted Rupee Alcove 1", "Stone Tower Temple Inverted Rupee Alcove 1"),
						M("Rupee.png", 555, 142, 24, "MM Stone Tower Temple Inverted Rupee Alcove 2", "Stone Tower Temple Inverted Rupee Alcove 2"),
						M("Pot.png", 476, 240, 24, "MM Stone Tower Temple Inverted Pot Updrafts Bridge 1", "Stone Tower Temple Inverted Pot Updrafts Bridge 1"),
						M("Pot.png", 427, 240, 24, "MM Stone Tower Temple Inverted Pot Updrafts Bridge 2", "Stone Tower Temple Inverted Pot Updrafts Bridge 2"),
						M("Pot.png", 668, 454, 24, "MM Stone Tower Temple Inverted Pot Updrafts Ledge 1", "Stone Tower Temple Inverted Pot Updrafts Ledge 1"),
						M("Pot.png", 646, 454, 24, "MM Stone Tower Temple Inverted Pot Updrafts Ledge 2", "Stone Tower Temple Inverted Pot Updrafts Ledge 2"),
						M("Pot.png", 646, 413, 24, "MM Stone Tower Temple Inverted Pot Updrafts Ledge 3", "Stone Tower Temple Inverted Pot Updrafts Ledge 3"),
						M("Pot.png", 668, 413, 24, "MM Stone Tower Temple Inverted Pot Updrafts Ledge 4", "Stone Tower Temple Inverted Pot Updrafts Ledge 4")
                    }
                },
				new MapSubRegion
                {
                    Name = "Lava Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower_Inverted/Lava.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 594, 487, 40, "MM Stone Tower Temple Inverted Wizzrobe Chest", "Stone Tower Temple Inverted Wizzrobe Chest"),
						M("Pot.png", 437, 94, 24, "MM Stone Tower Temple Inverted Pot Wizzrobe 1", "Stone Tower Temple Inverted Pot Wizzrobe 1"),
						M("Pot.png", 497, 94, 24, "MM Stone Tower Temple Inverted Pot Wizzrobe 2", "Stone Tower Temple Inverted Pot Wizzrobe 2"),
						M("Pot.png", 497, 62, 24, "MM Stone Tower Temple Inverted Pot Wizzrobe 3", "Stone Tower Temple Inverted Pot Wizzrobe 3"),
						M("Pot.png", 437, 62, 24, "MM Stone Tower Temple Inverted Pot Wizzrobe 4", "Stone Tower Temple Inverted Pot Wizzrobe 4"),
						M("Pot.png", 467, 62, 24, "MM Stone Tower Temple Inverted Pot Wizzrobe 5", "Stone Tower Temple Inverted Pot Wizzrobe 5")
                    }
                },
				new MapSubRegion
                {
                    Name = "Maze Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower_Inverted/Maze.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 656, 421, 40, "MM Stone Tower Temple Inverted Death Armos Chest", "Stone Tower Temple Inverted Death Armos Chest"),
						M("Pot.png", 284, 117, 24, "MM Stone Tower Temple Inverted Pot Poe Maze Side 1", "Stone Tower Temple Inverted Pot Poe Maze Side 1"),
						M("Pot.png", 331, 117, 24, "MM Stone Tower Temple Inverted Pot Poe Maze Side 2", "Stone Tower Temple Inverted Pot Poe Maze Side 2"),
						M("Pot.png", 281, 507, 24, "MM Stone Tower Temple Inverted Pot Poe Wizzrobe Side 1", "Stone Tower Temple Inverted Pot Poe Wizzrobe Side 1"),
						M("Pot.png", 234, 507, 24, "MM Stone Tower Temple Inverted Pot Poe Wizzrobe Side 2", "Stone Tower Temple Inverted Pot Poe Wizzrobe Side 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Central Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower_Inverted/Central.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 460, 187, 40, "MM Stone Tower Temple Inverted Giant Mask", "Stone Tower Temple Inverted Giant Mask"),
						M("Rupee.png", 654, 187, 24, "MM Stone Tower Temple Inverted Rupee Dexihand 1", "Stone Tower Temple Inverted Rupee Dexihand 1"),
						M("Rupee.png", 654, 139, 24, "MM Stone Tower Temple Inverted Rupee Dexihand 2", "Stone Tower Temple Inverted Rupee Dexihand 2"),
						M("Rupee.png", 654, 163, 24, "MM Stone Tower Temple Inverted Rupee Dexihand 3", "Stone Tower Temple Inverted Rupee Dexihand 3"),
						M("Pot.png", 495, 350, 24, "MM Stone Tower Temple Inverted Flying Pot Center 1", "Stone Tower Temple Inverted Flying Pot Center 1"),
						M("Pot.png", 440, 350, 24, "MM Stone Tower Temple Inverted Flying Pot Center 2", "Stone Tower Temple Inverted Flying Pot Center 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Gomess Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower_Inverted/Gomess.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 466, 532, 40, "MM Stone Tower Temple Inverted Boss Key", "Stone Tower Temple Inverted Boss Key"),
						M("Pot.png", 613, 438, 24, "MM Stone Tower Temple Inverted Pot Gomess 1", "Stone Tower Temple Inverted Pot Gomess 1"),
						M("Pot.png", 334, 438, 24, "MM Stone Tower Temple Inverted Pot Gomess 2", "Stone Tower Temple Inverted Pot Gomess 2"),
						M("Pot.png", 334, 155, 24, "MM Stone Tower Temple Inverted Pot Gomess 3", "Stone Tower Temple Inverted Pot Gomess 3"),
						M("Pot.png", 613, 155, 24, "MM Stone Tower Temple Inverted Pot Gomess 4", "Stone Tower Temple Inverted Pot Gomess 4")
                    }
                },
				new MapSubRegion
                {
                    Name = "Hiploop Bridge Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower_Inverted/Hiploop.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Rupee.png", 277, 378, 24, "MM Stone Tower Temple Inverted Rupee Pre-Boss Back 1", "Stone Tower Temple Inverted Rupee Pre-Boss Back 1"),
						M("Rupee.png", 258, 427, 24, "MM Stone Tower Temple Inverted Rupee Pre-Boss Back 2", "Stone Tower Temple Inverted Rupee Pre-Boss Back 2"),
						M("Rupee.png", 289, 427, 24, "MM Stone Tower Temple Inverted Rupee Pre-Boss Back 3", "Stone Tower Temple Inverted Rupee Pre-Boss Back 3"),
						M("Rupee.png", 306, 378, 24, "MM Stone Tower Temple Inverted Rupee Pre-Boss Back 4", "Stone Tower Temple Inverted Rupee Pre-Boss Back 4"),
						M("Rupee.png", 449, 427, 24, "MM Stone Tower Temple Inverted Rupee Pre-Boss Front 1", "Stone Tower Temple Inverted Rupee Pre-Boss Front 1"),
						M("Rupee.png", 416, 427, 24, "MM Stone Tower Temple Inverted Rupee Pre-Boss Front 2", "Stone Tower Temple Inverted Rupee Pre-Boss Front 2"),
						M("Rupee.png", 451, 378, 24, "MM Stone Tower Temple Inverted Rupee Pre-Boss Front 3", "Stone Tower Temple Inverted Rupee Pre-Boss Front 3"),
						M("Rupee.png", 423, 378, 24, "MM Stone Tower Temple Inverted Rupee Pre-Boss Front 4", "Stone Tower Temple Inverted Rupee Pre-Boss Front 4"),
						M("Rupee.png", 613, 427, 24, "MM Stone Tower Temple Inverted Rupee Pre-Boss Top 1", "Stone Tower Temple Inverted Rupee Pre-Boss Top 1"),
						M("Rupee.png", 571, 427, 24, "MM Stone Tower Temple Inverted Rupee Pre-Boss Top 2", "Stone Tower Temple Inverted Rupee Pre-Boss Top 2"),
						M("Rupee.png", 529, 427, 24, "MM Stone Tower Temple Inverted Rupee Pre-Boss Top 3", "Stone Tower Temple Inverted Rupee Pre-Boss Top 3"),
						M("Pot.png", 238, 534, 24, "MM Stone Tower Temple Inverted Pot Pre-Boss 1", "Stone Tower Temple Inverted Pot Pre-Boss 1"),
						M("Pot.png", 224, 563, 24, "MM Stone Tower Temple Inverted Pot Pre-Boss 2", "Stone Tower Temple Inverted Pot Pre-Boss 2"),
						M("Pot.png", 260, 534, 24, "MM Stone Tower Temple Inverted Pot Pre-Boss 3", "Stone Tower Temple Inverted Pot Pre-Boss 3"),
						M("Pot.png", 246, 563, 24, "MM Stone Tower Temple Inverted Pot Pre-Boss 4", "Stone Tower Temple Inverted Pot Pre-Boss 4"),
						M("Pot.png", 216, 534, 24, "MM Stone Tower Temple Inverted Pot Pre-Boss 5", "Stone Tower Temple Inverted Pot Pre-Boss 5"),
						M("Pot.png", 202, 563, 24, "MM Stone Tower Temple Inverted Pot Pre-Boss 6", "Stone Tower Temple Inverted Pot Pre-Boss 6"),
						M("Pot.png", 194, 534, 24, "MM Stone Tower Temple Inverted Pot Pre-Boss 7", "Stone Tower Temple Inverted Pot Pre-Boss 7"),
						M("Pot.png", 180, 563, 24, "MM Stone Tower Temple Inverted Pot Pre-Boss 8", "Stone Tower Temple Inverted Pot Pre-Boss 8"),
						M("Pot.png", 268, 563, 24, "MM Stone Tower Temple Inverted Flying Pot Pre-Boss 1", "Stone Tower Temple Inverted Flying Pot Pre-Boss 1"),
						M("Pot.png", 282, 534, 24, "MM Stone Tower Temple Inverted Flying Pot Pre-Boss 2", "Stone Tower Temple Inverted Flying Pot Pre-Boss 2"),
						M("Pot.png", 290, 563, 24, "MM Stone Tower Temple Inverted Flying Pot Pre-Boss 3", "Stone Tower Temple Inverted Flying Pot Pre-Boss 3"),
						M("Pot.png", 304, 534, 24, "MM Stone Tower Temple Inverted Flying Pot Pre-Boss 4", "Stone Tower Temple Inverted Flying Pot Pre-Boss 4"),
						
						ME("Entrance.png", 793, 506, "Entrance shuffle (Twinmold)", "MM_BOSS_TEMPLE_STONE_TOWER")
                    }
                },
				new MapSubRegion
                {
                    Name = "Boss Room",
                    BackgroundImage = "region maps/MM/Dungeons/Stone_Tower_Inverted/Boss.png",
                    DestinationEntranceIds = new List<string> { "MM_BOSS_TEMPLE_STONE_TOWER" },
                    Marks = new List<MapMark>
                    {
						M("Collectible.png", 496, 307, 40, "MM Stone Tower Temple Inverted Boss HC", "Stone Tower Temple Inverted Boss HC"),
						M("NPC.png", 452, 307, 40, "MM Stone Tower Temple Inverted Boss", "Stone Tower Temple Inverted Boss"),
						M("NPC.png", 408, 307, 40, "MM Oath to Order", "Oath to Order")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion SwampSpiderHouse()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Swamp Spider House";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Lobby",
                    BackgroundImage = "region maps/MM/Southern_Swamp/HS_Lobby.png",
                    DestinationEntranceIds = new List<string> { "MM_SPIDER_HOUSE_SWAMP" },
                    Marks = new List<MapMark>
                    {
						M("NPC.png", 391, 178, 40, "MM Swamp Spider House Mask of Truth", "Swamp Spider House Mask of Truth"),
						M("Rock.png", 364, 223, 24, "MM Swamp Spider House Rock Entrance 1", "Swamp Spider House Rock Entrance 1"),
						M("Rock.png", 551, 255, 24, "MM Swamp Spider House Rock Entrance 2", "Swamp Spider House Rock Entrance 2"),
						
						ME("Entrance.png", 450, 476, "Entrance shuffle (Southern Swamp)", "MM_SWAMP_FROM_SPIDER_HOUSE")
                    }
                },
				new MapSubRegion
                {
                    Name = "Main Room",
                    BackgroundImage = "region maps/MM/Southern_Swamp/HS_1.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 718, 357, 40, "MM Swamp Skulltula Main Room Jar", "Swamp Skulltula Main Room Jar"),
						M("Gold_Skulltula.png", 415, 519, 40, "MM Swamp Skulltula Main Room Lower Left Soft Soil", "Swamp Skulltula Main Room Lower Left Soft Soil"),
						M("Gold_Skulltula.png", 495, 97, 40, "MM Swamp Skulltula Main Room Lower Right Soft Soil", "Swamp Skulltula Main Room Lower Right Soft Soil"),
						M("Gold_Skulltula.png", 90, 287, 40, "MM Swamp Skulltula Main Room Near Ceiling", "Swamp Skulltula Main Room Near Ceiling"),
						M("Gold_Skulltula.png", 367, 192, 40, "MM Swamp Skulltula Main Room Pillar", "Swamp Skulltula Main Room Pillar"),
						M("Gold_Skulltula.png", 162, 439, 40, "MM Swamp Skulltula Main Room Upper Pillar", "Swamp Skulltula Main Room Upper Pillar"),
						M("Gold_Skulltula.png", 870, 75, 40, "MM Swamp Skulltula Main Room Upper Soft Soil", "Swamp Skulltula Main Room Upper Soft Soil"),
						M("Gold_Skulltula.png", 634, 239, 40, "MM Swamp Skulltula Main Room Water", "Swamp Skulltula Main Room Water"),
						M("Pot.png", 690, 365, 24, "MM Swamp Spider House Pot Main Lower 1", "Swamp Spider House Pot Main Lower 1"),
						M("Pot.png", 690, 270, 24, "MM Swamp Spider House Pot Main Lower 2", "Swamp Spider House Pot Main Lower 2"),
						M("Pot.png", 720, 270, 24, "MM Swamp Spider House Pot Main Lower 3", "Swamp Spider House Pot Main Lower 3"),
						M("Pot.png", 802, 537, 24, "MM Swamp Spider House Pot Main Upper Left 1", "Swamp Spider House Pot Main Upper Left 1"),
						M("Pot.png", 802, 496, 24, "MM Swamp Spider House Pot Main Upper Left 2", "Swamp Spider House Pot Main Upper Left 2"),
						M("Pot.png", 788, 118, 24, "MM Swamp Spider House Pot Main Upper Right 1", "Swamp Spider House Pot Main Upper Right 1"),
						M("Pot.png", 788, 77, 24, "MM Swamp Spider House Pot Main Upper Right 2", "Swamp Spider House Pot Main Upper Right 2"),
						M("Wonder.png", 357, 312, 24, "MM Swamp Spider House Wonder Item Pillars 01", "Swamp Spider House Wonder Item Pillars 01"),
						M("Wonder.png", 335, 321, 24, "MM Swamp Spider House Wonder Item Pillars 02", "Swamp Spider House Wonder Item Pillars 02"),
						M("Wonder.png", 326, 343, 24, "MM Swamp Spider House Wonder Item Pillars 03", "Swamp Spider House Wonder Item Pillars 03"),
						M("Wonder.png", 326, 259, 24, "MM Swamp Spider House Wonder Item Pillars 04", "Swamp Spider House Wonder Item Pillars 04"),
						M("Wonder.png", 335, 281, 24, "MM Swamp Spider House Wonder Item Pillars 05", "Swamp Spider House Wonder Item Pillars 05"),
						M("Wonder.png", 357, 290, 24, "MM Swamp Spider House Wonder Item Pillars 06", "Swamp Spider House Wonder Item Pillars 06"),
						M("Wonder.png", 273, 290, 24, "MM Swamp Spider House Wonder Item Pillars 07", "Swamp Spider House Wonder Item Pillars 07"),
						M("Wonder.png", 304, 259, 24, "MM Swamp Spider House Wonder Item Pillars 08", "Swamp Spider House Wonder Item Pillars 08"),
						M("Wonder.png", 295, 281, 24, "MM Swamp Spider House Wonder Item Pillars 09", "Swamp Spider House Wonder Item Pillars 09"),
						M("Wonder.png", 295, 321, 24, "MM Swamp Spider House Wonder Item Pillars 10", "Swamp Spider House Wonder Item Pillars 10"),
						M("Wonder.png", 273, 312, 24, "MM Swamp Spider House Wonder Item Pillars 11", "Swamp Spider House Wonder Item Pillars 11"),
						M("Wonder.png", 304, 343, 24, "MM Swamp Spider House Wonder Item Pillars 12", "Swamp Spider House Wonder Item Pillars 12")
                    }
                },
				new MapSubRegion
                {
                    Name = "Monument Room",
                    BackgroundImage = "region maps/MM/Southern_Swamp/HS_3.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 234, 412, 40, "MM Swamp Skulltula Monument Room Crate 1", "Swamp Skulltula Monument Room Crate 1"),
						M("Gold_Skulltula.png", 634, 377, 40, "MM Swamp Skulltula Monument Room Crate 2", "Swamp Skulltula Monument Room Crate 2"),
						M("Gold_Skulltula.png", 561, 241, 40, "MM Swamp Skulltula Monument Room Lower Wall", "Swamp Skulltula Monument Room Lower Wall"),
						M("Gold_Skulltula.png", 504, 241, 40, "MM Swamp Skulltula Monument Room On Monument", "Swamp Skulltula Monument Room On Monument"),
						M("Gold_Skulltula.png", 813, 199, 40, "MM Swamp Skulltula Monument Room Torch", "Swamp Skulltula Monument Room Torch"),
						M("Pot.png", 586, 192, 24, "MM Swamp Spider House Pot Monument Room 1", "Swamp Spider House Pot Monument Room 1"),
						M("Pot.png", 613, 197, 24, "MM Swamp Spider House Pot Monument Room 2", "Swamp Spider House Pot Monument Room 2"),
						M("Soil.png", 746, 469, 24, "MM Swamp Spider House Soil Monument Item 1", "Swamp Spider House Soil Monument Item 1"),
						M("Soil.png", 706, 469, 24, "MM Swamp Spider House Soil Monument Item 2", "Swamp Spider House Soil Monument Item 2"),
						M("Soil.png", 726, 444, 24, "MM Swamp Spider House Soil Monument Item 3", "Swamp Spider House Soil Monument Item 3"),
						M("Crate.png", 204, 427, 24, "MM Swamp Spider House Crate Monument 1", "Swamp Spider House Crate Monument 1"),
						M("Crate.png", 398, 396, 24, "MM Swamp Spider House Crate Monument 2", "Swamp Spider House Crate Monument 2"),
						M("Crate.png", 594, 363, 24, "MM Swamp Spider House Crate Monument 3", "Swamp Spider House Crate Monument 3"),
						M("Crate.png", 211, 450, 24, "MM Swamp Spider House Crate Monument 4", "Swamp Spider House Crate Monument 4"),
						M("Crate.png", 618, 374, 24, "MM Swamp Spider House Crate Monument 5", "Swamp Spider House Crate Monument 5"),
						M("Crate.png", 666, 396, 24, "MM Swamp Spider House Crate Monument 6", "Swamp Spider House Crate Monument 6")
                    }
                },
				new MapSubRegion
                {
                    Name = "Pots Room",
                    BackgroundImage = "region maps/MM/Southern_Swamp/HS_4.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 253, 458, 40, "MM Swamp Skulltula Pot Room Behind Vines", "Swamp Skulltula Pot Room Behind Vines"),
						M("Gold_Skulltula.png", 574, 78, 40, "MM Swamp Skulltula Pot Room Hive 1", "Swamp Skulltula Pot Room Hive 1"),
						M("Gold_Skulltula.png", 605, 99, 40, "MM Swamp Skulltula Pot Room Hive 2", "Swamp Skulltula Pot Room Hive 2"),
						M("Gold_Skulltula.png", 749, 554, 40, "MM Swamp Skulltula Pot Room Jar", "Swamp Skulltula Pot Room Jar"),
						M("Gold_Skulltula.png", 403, 480, 40, "MM Swamp Skulltula Pot Room Pot 1", "Swamp Skulltula Pot Room Pot 1"),
						M("Gold_Skulltula.png", 613, 480, 40, "MM Swamp Skulltula Pot Room Pot 2", "Swamp Skulltula Pot Room Pot 2"),
						M("Gold_Skulltula.png", 84, 86, 40, "MM Swamp Skulltula Pot Room Wall", "Swamp Skulltula Pot Room Wall"),
						M("Rock.png", 187, 401, 24, "MM Swamp Spider House Rock Pots Room", "Swamp Spider House Rock Pots Room"),
						M("Hive.png", 626, 87, 40, "MM Swamp Spider House Hive Jars Room 1", "Swamp Spider House Hive Jars Room 1"),
						M("Hive.png", 529, 93, 40, "MM Swamp Spider House Hive Jars Room 2", "Swamp Spider House Hive Jars Room 2"),
						M("Hive.png", 558, 108, 40, "MM Swamp Spider House Hive Jars Room 3", "Swamp Spider House Hive Jars Room 3"),
						M("Pot.png", 278, 416, 24, "MM Swamp Spider House Pot Jar Room 1", "Swamp Spider House Pot Jar Room 1"),
						M("Pot.png", 228, 416, 24, "MM Swamp Spider House Pot Jar Room 2", "Swamp Spider House Pot Jar Room 2"),
						M("Pot.png", 845, 565, 24, "MM Swamp Spider House Pot Jar Room 3", "Swamp Spider House Pot Jar Room 3"),
						M("Pot.png", 802, 563, 24, "MM Swamp Spider House Pot Jar Room 4", "Swamp Spider House Pot Jar Room 4"),
						M("Pot.png", 823, 545, 24, "MM Swamp Spider House Pot Jar Room 5", "Swamp Spider House Pot Jar Room 5"),
						M("Pot.png", 780, 543, 24, "MM Swamp Spider House Pot Jar Room 6", "Swamp Spider House Pot Jar Room 6"),
						M("Pot.png", 801, 525, 24, "MM Swamp Spider House Pot Jar Room 7", "Swamp Spider House Pot Jar Room 7"),
						M("Wonder.png", 410, 442, 24, "MM Swamp Spider House Wonder Item Pots 1", "Swamp Spider House Wonder Item Pots 1"),
						M("Wonder.png", 481, 442, 24, "MM Swamp Spider House Wonder Item Pots 2", "Swamp Spider House Wonder Item Pots 2"),
						M("Wonder.png", 548, 442, 24, "MM Swamp Spider House Wonder Item Pots 3", "Swamp Spider House Wonder Item Pots 3"),
						M("Wonder.png", 618, 442, 24, "MM Swamp Spider House Wonder Item Pots 4", "Swamp Spider House Wonder Item Pots 4"),
						M("Wonder.png", 719, 448, 24, "MM Swamp Spider House Wonder Item Pots 5", "Swamp Spider House Wonder Item Pots 5")
                    }
                },
				new MapSubRegion
                {
                    Name = "Gold Room",
                    BackgroundImage = "region maps/MM/Southern_Swamp/HS_2.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 455, 109, 40, "MM Swamp Skulltula Gold Room Hive", "Swamp Skulltula Gold Room Hive"),
						M("Gold_Skulltula.png", 455, 160, 40, "MM Swamp Skulltula Gold Room Near Ceiling", "Swamp Skulltula Gold Room Near Ceiling"),
						M("Gold_Skulltula.png", 572, 191, 40, "MM Swamp Skulltula Gold Room Pillar", "Swamp Skulltula Gold Room Pillar"),
						M("Gold_Skulltula.png", 296, 400, 40, "MM Swamp Skulltula Gold Room Wall", "Swamp Skulltula Gold Room Wall"),
						M("Hive.png", 418, 81, 40, "MM Swamp Spider House Hive Gold Room 1", "Swamp Spider House Hive Gold Room 1"),
						M("Hive.png", 455, 37, 40, "MM Swamp Spider House Hive Gold Room 2", "Swamp Spider House Hive Gold Room 2"),
						M("Hive.png", 492, 81, 40, "MM Swamp Spider House Hive Gold Room 3", "Swamp Spider House Hive Gold Room 3"),
						M("Pot.png", 441, 414, 24, "MM Swamp Spider House Pot Gold Room Lower 1", "Swamp Spider House Pot Gold Room Lower 1"),
						M("Pot.png", 481, 414, 24, "MM Swamp Spider House Pot Gold Room Lower 2", "Swamp Spider House Pot Gold Room Lower 2"),
						M("Pot.png", 691, 304, 24, "MM Swamp Spider House Pot Gold Room Upper 1", "Swamp Spider House Pot Gold Room Upper 1"),
						M("Pot.png", 754, 334, 24, "MM Swamp Spider House Pot Gold Room Upper 2", "Swamp Spider House Pot Gold Room Upper 2"),
						M("Pot.png", 733, 324, 24, "MM Swamp Spider House Pot Gold Room Upper 3", "Swamp Spider House Pot Gold Room Upper 3"),
						M("Pot.png", 712, 314, 24, "MM Swamp Spider House Pot Gold Room Upper 4", "Swamp Spider House Pot Gold Room Upper 4"),
						M("Soil.png", 267, 310, 24, "MM Swamp Spider House Soil Gold", "Swamp Spider House Soil Gold"),
						M("Crate.png", 145, 370, 24, "MM Swamp Spider House Crate Gold 1", "Swamp Spider House Crate Gold 1"),
						M("Crate.png", 208, 370, 24, "MM Swamp Spider House Crate Gold 2", "Swamp Spider House Crate Gold 2")
                    }
                },
				new MapSubRegion
                {
                    Name = "Tree Room",
                    BackgroundImage = "region maps/MM/Southern_Swamp/HS_5.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 767, 536, 40, "MM Swamp Skulltula Tree Room Grass 1", "Swamp Skulltula Tree Room Grass 1"),
						M("Gold_Skulltula.png", 422, 449, 40, "MM Swamp Skulltula Tree Room Grass 2", "Swamp Skulltula Tree Room Grass 2"),
						M("Gold_Skulltula.png", 590, 186, 40, "MM Swamp Skulltula Tree Room Hive", "Swamp Skulltula Tree Room Hive"),
						M("Gold_Skulltula.png", 506, 223, 40, "MM Swamp Skulltula Tree Room Tree 1", "Swamp Skulltula Tree Room Tree 1"),
						M("Gold_Skulltula.png", 464, 235, 40, "MM Swamp Skulltula Tree Room Tree 2", "Swamp Skulltula Tree Room Tree 2"),
						M("Gold_Skulltula.png", 425, 219, 40, "MM Swamp Skulltula Tree Room Tree 3", "Swamp Skulltula Tree Room Tree 3"),
						M("Hive.png", 324, 126, 40, "MM Swamp Spider House Hive Tree Room 1", "Swamp Spider House Hive Tree Room 1"),
						M("Hive.png", 645, 128, 40, "MM Swamp Spider House Hive Tree Room 2", "Swamp Spider House Hive Tree Room 2"),
						M("Hive.png", 359, 190, 40, "MM Swamp Spider House Hive Tree Room 3", "Swamp Spider House Hive Tree Room 3")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion PirateFortress()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Pirate Fortress";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Entrance",
                    BackgroundImage = "region maps/MM/Fortress/Entrance.png",
                    DestinationEntranceIds = new List<string>
					{
						"MM_PIRATE_FORTRESS",
						"MM_EXTERIOR_DOOR_FROM_SEWERS",
						"MM_EXTERIOR_GATE_FROM_SEWERS",
						"MM_PIRATE_FORTRESS_EXTERIOR_FROM_INTERIOR",
						"MM_PIRATE_FORTRESS_EXTERIOR_LOOKOUT"
					},
                    Marks = new List<MapMark>
                    {
						M("Barrel.png", 459, 207, 24, "MM Pirate Fortress Entrance Barrel", "Pirate Fortress Entrance Barrel"),
						M("Chest.png", 477, 432, 40, "MM Pirate Fortress Entrance Chest 1", "Pirate Fortress Entrance Chest 1"),
						M("Chest.png", 862, 369, 40, "MM Pirate Fortress Entrance Chest 2", "Pirate Fortress Entrance Chest 2"),
						M("Chest.png", 463, 336, 40, "MM Pirate Fortress Entrance Chest 3", "Pirate Fortress Entrance Chest 3"),
						
						ME("Entrance.png", 134, 128, "Entrance shuffle (Pirate Fortress Sewers)", "MM_SEWERS_FROM_EXTERIOR_DOOR"),
						ME("Entrance.png", 727, 178, "Entrance shuffle (Great Bay Coast)", "MM_GREAT_BAY_FROM_PIRATE_FORTRESS"),
						ME("Entrance.png", 452, 278, "Entrance shuffle (Pirate Fortress Sewers)", "MM_SEWERS_FROM_EXTERIOR_GATE"),
						ME("Entrance.png", 24, 73, "Entrance shuffle (Pirate Fortress Exterior)", "MM_PIRATE_FORTRESS_INTERIOR"),
						ME("Entrance.png", 417, 184, "Entrance shuffle (Pirate Fortress Exterior)", "MM_PIRATE_FORTRESS_INTERIOR_FROM_LOOKOUT")
                    }
                },
				new MapSubRegion
                {
                    Name = "Interior",
                    BackgroundImage = "region maps/MM/Fortress/Interior.png",
                    DestinationEntranceIds = new List<string>
					{
						"MM_SEWERS_FROM_EXTERIOR_DOOR",
						"MM_SEWERS_FROM_EXTERIOR_GATE"
					},
                    Marks = new List<MapMark>
                    {
						M("Barrel.png", 555, 199, 24, "MM Pirate Fortress Interior Barrel Aquarium", "Pirate Fortress Interior Barrel Aquarium"),
						M("Barrel.png", 495, 519, 24, "MM Pirate Fortress Sewers Barrel Cage Room 01", "Pirate Fortress Sewers Barrel Cage Room 01"),
						M("Barrel.png", 495, 503, 24, "MM Pirate Fortress Sewers Barrel Cage Room 02", "Pirate Fortress Sewers Barrel Cage Room 02"),
						M("Barrel.png", 479, 519, 24, "MM Pirate Fortress Sewers Barrel Cage Room 03", "Pirate Fortress Sewers Barrel Cage Room 03"),
						M("Barrel.png", 488, 447, 24, "MM Pirate Fortress Sewers Barrel Cage Room 04", "Pirate Fortress Sewers Barrel Cage Room 04"),
						M("Barrel.png", 504, 447, 24, "MM Pirate Fortress Sewers Barrel Cage Room 05", "Pirate Fortress Sewers Barrel Cage Room 05"),
						M("Barrel.png", 479, 503, 24, "MM Pirate Fortress Sewers Barrel Cage Room 06", "Pirate Fortress Sewers Barrel Cage Room 06"),
						M("Barrel.png", 520, 447, 24, "MM Pirate Fortress Sewers Barrel Cage Room 07", "Pirate Fortress Sewers Barrel Cage Room 07"),
						M("Barrel.png", 463, 519, 24, "MM Pirate Fortress Sewers Barrel Cage Room 08", "Pirate Fortress Sewers Barrel Cage Room 08"),
						M("Barrel.png", 463, 503, 24, "MM Pirate Fortress Sewers Barrel Cage Room 09", "Pirate Fortress Sewers Barrel Cage Room 09"),
						M("Barrel.png", 472, 447, 24, "MM Pirate Fortress Sewers Barrel Cage Room 10", "Pirate Fortress Sewers Barrel Cage Room 10"),
						M("Barrel.png", 615, 437, 24, "MM Pirate Fortress Sewers Barrel Cage Room 11", "Pirate Fortress Sewers Barrel Cage Room 11"),
						M("Barrel.png", 599, 437, 24, "MM Pirate Fortress Sewers Barrel Cage Room 12", "Pirate Fortress Sewers Barrel Cage Room 12"),
						M("Barrel.png", 583, 437, 24, "MM Pirate Fortress Sewers Barrel Cage Room 13", "Pirate Fortress Sewers Barrel Cage Room 13"),
						M("Barrel.png", 535, 479, 24, "MM Pirate Fortress Sewers Barrel Cage Room 14", "Pirate Fortress Sewers Barrel Cage Room 14"),
						M("Barrel.png", 551, 479, 24, "MM Pirate Fortress Sewers Barrel Cage Room 15", "Pirate Fortress Sewers Barrel Cage Room 15"),
						M("Barrel.png", 567, 479, 24, "MM Pirate Fortress Sewers Barrel Cage Room 16", "Pirate Fortress Sewers Barrel Cage Room 16"),
						M("Barrel.png", 697, 155, 24, "MM Pirate Fortress Interior Barrel Hookshot Room 1", "Pirate Fortress Interior Barrel Hookshot Room 1"),
						M("Barrel.png", 721, 155, 24, "MM Pirate Fortress Interior Barrel Hookshot Room 2", "Pirate Fortress Interior Barrel Hookshot Room 2"),
						M("Barrel.png", 874, 390, 24, "MM Pirate Fortress Sewers Barrel End 1", "Pirate Fortress Sewers Barrel End 1"),
						M("Barrel.png", 874, 374, 24, "MM Pirate Fortress Sewers Barrel End 2", "Pirate Fortress Sewers Barrel End 2"),
						M("Barrel.png", 890, 390, 24, "MM Pirate Fortress Sewers Barrel End 3", "Pirate Fortress Sewers Barrel End 3"),
						M("Barrel.png", 906, 390, 24, "MM Pirate Fortress Sewers Barrel End 4", "Pirate Fortress Sewers Barrel End 4"),
						M("Barrel.png", 890, 374, 24, "MM Pirate Fortress Sewers Barrel End 5", "Pirate Fortress Sewers Barrel End 5"),
						M("Chest.png", 462, 682, 40, "MM Pirate Fortress Interior Aquarium", "Pirate Fortress Interior Aquarium"),
						M("Chest.png", 699, 97, 40, "MM Pirate Fortress Interior Hookshot", "Pirate Fortress Interior Hookshot"),
						M("Chest.png", 455, 80, 40, "MM Pirate Fortress Interior Silver Rupee Chest", "Pirate Fortress Interior Silver Rupee Chest"),
						M("Chest.png", 912, 575, 40, "MM Pirate Fortress Sewers Chest 1", "Pirate Fortress Sewers Chest 1"),
						M("Chest.png", 486, 471, 40, "MM Pirate Fortress Sewers Chest 2", "Pirate Fortress Sewers Chest 2"),
						M("Chest.png", 508, 427, 40, "MM Pirate Fortress Sewers Chest 3", "Pirate Fortress Sewers Chest 3"),
						M("Collectible.png", 599, 461, 40, "MM Pirate Fortress Sewers HP", "Pirate Fortress Sewers HP"),
						M("Pot.png", 992, 256, 24, "MM Pirate Fortress Interior Pot Barrel Maze 1", "Pirate Fortress Interior Pot Barrel Maze 1"),
						M("Pot.png", 1033, 256, 24, "MM Pirate Fortress Interior Pot Barrel Maze 2", "Pirate Fortress Interior Pot Barrel Maze 2"),
						M("Pot.png", 992, 228, 24, "MM Pirate Fortress Interior Pot Barrel Maze 3", "Pirate Fortress Interior Pot Barrel Maze 3"),
						M("Pot.png", 743, 26, 24, "MM Pirate Fortress Interior Pot Beehive 1", "Pirate Fortress Interior Pot Beehive 1"),
						M("Pot.png", 743, 2, 24, "MM Pirate Fortress Interior Pot Beehive 2", "Pirate Fortress Interior Pot Beehive 2"),
						M("Pot.png", 441, 641, 24, "MM Pirate Fortress Interior Pot Chest Aquarium 1", "Pirate Fortress Interior Pot Chest Aquarium 1"),
						M("Pot.png", 486, 641, 24, "MM Pirate Fortress Interior Pot Chest Aquarium 2", "Pirate Fortress Interior Pot Chest Aquarium 2"),
						M("Pot.png", 486, 670, 24, "MM Pirate Fortress Interior Pot Chest Aquarium 3", "Pirate Fortress Interior Pot Chest Aquarium 3"),
						M("Pot.png", 555, 156, 24, "MM Pirate Fortress Interior Pot Guarded 1", "Pirate Fortress Interior Pot Guarded 1"),
						M("Pot.png", 583, 156, 24, "MM Pirate Fortress Interior Pot Guarded 2", "Pirate Fortress Interior Pot Guarded 2"),
						M("Pot.png", 900, 339, 24, "MM Pirate Fortress Sewers Pot End 1", "Pirate Fortress Sewers Pot End 1"),
						M("Pot.png", 856, 390, 24, "MM Pirate Fortress Sewers Pot End 2", "Pirate Fortress Sewers Pot End 2"),
						M("Pot.png", 922, 339, 24, "MM Pirate Fortress Sewers Pot End 3", "Pirate Fortress Sewers Pot End 3"),
						M("Pot.png", 630, 479, 24, "MM Pirate Fortress Sewers Pot Heart Piece Room 1", "Pirate Fortress Sewers Pot Heart Piece Room 1"),
						M("Pot.png", 630, 455, 24, "MM Pirate Fortress Sewers Pot Heart Piece Room 2", "Pirate Fortress Sewers Pot Heart Piece Room 2"),
						M("Pot.png", 728, 478, 24, "MM Pirate Fortress Sewers Pot Waterway 1", "Pirate Fortress Sewers Pot Waterway 1"),
						M("Pot.png", 728, 454, 24, "MM Pirate Fortress Sewers Pot Waterway 2", "Pirate Fortress Sewers Pot Waterway 2"),
						M("Rupee.png", 630, 432, 24, "MM Pirate Fortress Sewers Rupee Cage", "Pirate Fortress Sewers Rupee Cage"),
						M("Rupee.png", 463, 542, 24, "MM Pirate Fortress Sewers Rupee Near Cage 1", "Pirate Fortress Sewers Rupee Near Cage 1"),
						M("Rupee.png", 495, 542, 24, "MM Pirate Fortress Sewers Rupee Near Cage 2", "Pirate Fortress Sewers Rupee Near Cage 2"),
						M("Rupee.png", 504, 463, 24, "MM Pirate Fortress Sewers Rupee Near Cage 3", "Pirate Fortress Sewers Rupee Near Cage 3"),
						M("Rupee.png", 519, 478, 24, "MM Pirate Fortress Sewers Rupee Near Cage 4", "Pirate Fortress Sewers Rupee Near Cage 4"),
						M("Rupee.png", 874, 413, 24, "MM Pirate Fortress Sewers Rupee Water Elevator 1", "Pirate Fortress Sewers Rupee Water Elevator 1"),
						M("Rupee.png", 874, 351, 24, "MM Pirate Fortress Sewers Rupee Water Elevator 2", "Pirate Fortress Sewers Rupee Water Elevator 2"),
						M("Rupee.png", 890, 413, 24, "MM Pirate Fortress Sewers Rupee Water Elevator 3", "Pirate Fortress Sewers Rupee Water Elevator 3"),
						M("Wonder.png", 709, 26, 24, "MM Pirate Fortress Interior Wonder Item Skull Forehead 1", "Pirate Fortress Interior Wonder Item Skull Forehead 1"),
						M("Wonder.png", 725, 42, 24, "MM Pirate Fortress Interior Wonder Item Skull Forehead 2", "Pirate Fortress Interior Wonder Item Skull Forehead 2"),
						M("Wonder.png", 693, 42, 24, "MM Pirate Fortress Interior Wonder Item Skull Forehead 3", "Pirate Fortress Interior Wonder Item Skull Forehead 3"),
						
						ME("Entrance.png", 1064, 750, "Entrance shuffle (Pirate Fortress Entrance)", "MM_EXTERIOR_GATE_FROM_SEWERS"),
						ME("Entrance.png", 923, 285, "Entrance shuffle (Pirate Fortress Entrance)", "MM_EXTERIOR_DOOR_FROM_SEWERS")
                    }
                },
				new MapSubRegion
                {
                    Name = "Exterior",
                    BackgroundImage = "region maps/MM/Fortress/Exterior.png",
                    DestinationEntranceIds = new List<string>
					{
						"MM_PIRATE_FORTRESS_INTERIOR",
						"MM_PIRATE_FORTRESS_INTERIOR_FROM_LOOKOUT"
					},
                    Marks = new List<MapMark>
                    {
						M("Barrel.png", 136, 29, 24, "MM Pirate Fortress Interior Barrel Outside", "Pirate Fortress Interior Barrel Outside"),
						M("Chest.png", 556, 155, 40, "MM Pirate Fortress Interior Lower Chest", "Pirate Fortress Interior Lower Chest"),
						M("Chest.png", 766, 287, 40, "MM Pirate Fortress Interior Upper Chest", "Pirate Fortress Interior Upper Chest"),
						M("Crate.png", 253, 392, 24, "MM Pirate Fortress Interior Crate Entrance", "Pirate Fortress Interior Crate Entrance"),
						M("Crate.png", 456, 327, 24, "MM Pirate Fortress Interior Crate Middle 1", "Pirate Fortress Interior Crate Middle 1"),
						M("Crate.png", 456, 378, 24, "MM Pirate Fortress Interior Crate Middle 2", "Pirate Fortress Interior Crate Middle 2"),
						M("Heart.png", 170, 97, 24, "MM Pirate Fortress Interior Heart 1", "Pirate Fortress Interior Heart 1"),
						M("Heart.png", 182, 77, 24, "MM Pirate Fortress Interior Heart 2", "Pirate Fortress Interior Heart 2"),
						M("Heart.png", 158, 77, 24, "MM Pirate Fortress Interior Heart 3", "Pirate Fortress Interior Heart 3"),
						M("Wonder.png", 371, 167, 24, "MM Pirate Fortress Interior Wonder Item Skull Eyes 1", "Pirate Fortress Interior Wonder Item Skull Eyes 1"),
						M("Wonder.png", 382, 147, 24, "MM Pirate Fortress Interior Wonder Item Skull Eyes 2", "Pirate Fortress Interior Wonder Item Skull Eyes 2"),
						M("Wonder.png", 360, 147, 24, "MM Pirate Fortress Interior Wonder Item Skull Eyes 3", "Pirate Fortress Interior Wonder Item Skull Eyes 3"),
						M("Wonder.png", 414, 147, 24, "MM Pirate Fortress Interior Wonder Item Skull Eyes 4", "Pirate Fortress Interior Wonder Item Skull Eyes 4"),
						M("Wonder.png", 425, 167, 24, "MM Pirate Fortress Interior Wonder Item Skull Eyes 5", "Pirate Fortress Interior Wonder Item Skull Eyes 5"),
						M("Wonder.png", 436, 147, 24, "MM Pirate Fortress Interior Wonder Item Skull Eyes 6", "Pirate Fortress Interior Wonder Item Skull Eyes 6"),
						
						ME("Entrance.png", 210, 425, "Entrance shuffle (Pirate Fortress Entrance)", "MM_PIRATE_FORTRESS_EXTERIOR_FROM_INTERIOR"),
						ME("Entrance.png", 658, 559, "Entrance shuffle (Pirate Fortress Entrance)", "MM_PIRATE_FORTRESS_EXTERIOR_LOOKOUT")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion OceanSpiderHouse()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Ocean Spider House";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Lobby",
                    BackgroundImage = "region maps/MM/Great_Bay_Coast/HS_Lobby.png",
                    DestinationEntranceIds = new List<string> { "MM_SPIDER_HOUSE_OCEAN" },
                    Marks = new List<MapMark>
                    {
						M("NPC.png", 450, 456, 40, "MM Ocean Spider House Wallet", "Ocean Spider House Wallet"),
						M("Gold_Skulltula.png", 431, 245, 40, "MM Ocean Skulltula Entrance Left Wall", "Ocean Skulltula Entrance Left Wall"),
						M("Gold_Skulltula.png", 462, 199, 40, "MM Ocean Skulltula Entrance Right Wall", "Ocean Skulltula Entrance Right Wall"),
						M("Gold_Skulltula.png", 410, 26, 40, "MM Ocean Skulltula Entrance Web", "Ocean Skulltula Entrance Web"),
						M("Pot.png", 395, 177, 24, "MM Ocean Spider House Pot Entrance 1", "Ocean Spider House Pot Entrance 1"),
						M("Pot.png", 377, 160, 24, "MM Ocean Spider House Pot Entrance 2", "Ocean Spider House Pot Entrance 2"),
						M("Pot.png", 516, 177, 24, "MM Ocean Spider House Pot Entrance 3", "Ocean Spider House Pot Entrance 3"),
						M("Pot.png", 534, 160, 24, "MM Ocean Spider House Pot Entrance 4", "Ocean Spider House Pot Entrance 4"),
						
						ME("Entrance.png", 450, 577, "Entrance shuffle (Great Bay Coast)", "MM_GREAT_BAY_FROM_SPIDER_HOUSE")
                    }
                },
				new MapSubRegion
                {
                    Name = "Main Room",
                    BackgroundImage = "region maps/MM/Great_Bay_Coast/HS_1.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 663, 196, 40, "MM Ocean Skulltula 2nd Room Behind Skull 1", "Ocean Skulltula 2nd Room Behind Skull 1"),
						M("Gold_Skulltula.png", 476, 393, 40, "MM Ocean Skulltula 2nd Room Behind Skull 2", "Ocean Skulltula 2nd Room Behind Skull 2"),
						M("Gold_Skulltula.png", 716, 291, 40, "MM Ocean Skulltula 2nd Room Ceiling Edge", "Ocean Skulltula 2nd Room Ceiling Edge"),
						M("Gold_Skulltula.png", 741, 57, 40, "MM Ocean Skulltula 2nd Room Ceiling Plank", "Ocean Skulltula 2nd Room Ceiling Plank"),
						M("Gold_Skulltula.png", 298, 376, 40, "MM Ocean Skulltula 2nd Room Jar", "Ocean Skulltula 2nd Room Jar"),
						M("Gold_Skulltula.png", 517, 406, 40, "MM Ocean Skulltula 2nd Room Lower Pot", "Ocean Skulltula 2nd Room Lower Pot"),
						M("Gold_Skulltula.png", 411, 200, 40, "MM Ocean Skulltula 2nd Room Upper Pot", "Ocean Skulltula 2nd Room Upper Pot"),
						M("Gold_Skulltula.png", 81, 498, 40, "MM Ocean Skulltula 2nd Room Webbed Hole", "Ocean Skulltula 2nd Room Webbed Hole"),
						M("Gold_Skulltula.png", 297, 197, 40, "MM Ocean Skulltula 2nd Room Webbed Pot", "Ocean Skulltula 2nd Room Webbed Pot"),
						M("Pot.png", 359, 460, 24, "MM Ocean Spider House Pot Main Room 1", "Ocean Spider House Pot Main Room 1"),
						M("Pot.png", 697, 357, 24, "MM Ocean Spider House Pot Main Room 2", "Ocean Spider House Pot Main Room 2"),
						M("Pot.png", 739, 434, 24, "MM Ocean Spider House Pot Main Room Boe", "Ocean Spider House Pot Main Room Boe"),
						M("Pot.png", 311, 431, 24, "MM Ocean Spider House Pot Main Room Web", "Ocean Spider House Pot Main Room Web"),
						M("Wonder.png", 530, 193, 24, "MM Ocean Spider House Wonder Item Masks 1", "Ocean Spider House Wonder Item Masks 1"),
						M("Wonder.png", 547, 185, 24, "MM Ocean Spider House Wonder Item Masks 2", "Ocean Spider House Wonder Item Masks 2"),
						M("Wonder.png", 513, 185, 24, "MM Ocean Spider House Wonder Item Masks 3", "Ocean Spider House Wonder Item Masks 3"),
						M("Wonder.png", 581, 140, 24, "MM Ocean Spider House Wonder Item Masks 4", "Ocean Spider House Wonder Item Masks 4"),
						M("Wonder.png", 564, 148, 24, "MM Ocean Spider House Wonder Item Masks 5", "Ocean Spider House Wonder Item Masks 5"),
						M("Wonder.png", 547, 140, 24, "MM Ocean Spider House Wonder Item Masks 6", "Ocean Spider House Wonder Item Masks 6"),
						M("Wonder.png", 450, 377, 24, "MM Ocean Spider House Wonder Item Masks 7", "Ocean Spider House Wonder Item Masks 7"),
						M("Wonder.png", 467, 369, 24, "MM Ocean Spider House Wonder Item Masks 8", "Ocean Spider House Wonder Item Masks 8"),
						M("Wonder.png", 433, 369, 24, "MM Ocean Spider House Wonder Item Masks 9", "Ocean Spider House Wonder Item Masks 9")
                    }
                },
				new MapSubRegion
                {
                    Name = "Library Room",
                    BackgroundImage = "region maps/MM/Great_Bay_Coast/HS_2.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 636, 489, 40, "MM Ocean Skulltula Library Behind Bookcase 1", "Ocean Skulltula Library Behind Bookcase 1"),
						M("Gold_Skulltula.png", 534, 430, 40, "MM Ocean Skulltula Library Behind Bookcase 2", "Ocean Skulltula Library Behind Bookcase 2"),
						M("Gold_Skulltula.png", 221, 349, 40, "MM Ocean Skulltula Library Behind Picture", "Ocean Skulltula Library Behind Picture"),
						M("Gold_Skulltula.png", 252, 145, 40, "MM Ocean Skulltula Library Ceiling Edge", "Ocean Skulltula Library Ceiling Edge"),
						M("Gold_Skulltula.png", 151, 317, 40, "MM Ocean Skulltula Library Hole Behind Cabinet", "Ocean Skulltula Library Hole Behind Cabinet"),
						M("Gold_Skulltula.png", 864, 292, 40, "MM Ocean Skulltula Library Hole Behind Picture", "Ocean Skulltula Library Hole Behind Picture"),
						M("Gold_Skulltula.png", 602, 187, 40, "MM Ocean Skulltula Library On Corner Bookshelf", "Ocean Skulltula Library On Corner Bookshelf")
                    }
                },
				new MapSubRegion
                {
                    Name = "Storage Room",
                    BackgroundImage = "region maps/MM/Great_Bay_Coast/HS_4.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 451, 307, 40, "MM Ocean Skulltula Storage Room Behind Boat", "Ocean Skulltula Storage Room Behind Boat"),
						M("Gold_Skulltula.png", 250, 350, 40, "MM Ocean Skulltula Storage Room Behind Crate", "Ocean Skulltula Storage Room Behind Crate"),
						M("Gold_Skulltula.png", 398, 283, 40, "MM Ocean Skulltula Storage Room Ceiling Web", "Ocean Skulltula Storage Room Ceiling Web"),
						M("Gold_Skulltula.png", 676, 438, 40, "MM Ocean Skulltula Storage Room Crate", "Ocean Skulltula Storage Room Crate"),
						M("Gold_Skulltula.png", 428, 249, 40, "MM Ocean Skulltula Storage Room Jar", "Ocean Skulltula Storage Room Jar"),
						M("Crate.png", 318, 430, 24, "MM Ocean Spider House Crate", "Ocean Spider House Crate"),
						M("Pot.png", 553, 364, 24, "MM Ocean Spider House Pot Storage 1", "Ocean Spider House Pot Storage 1"),
						M("Pot.png", 104, 435, 24, "MM Ocean Spider House Pot Storage 2", "Ocean Spider House Pot Storage 2"),
						M("Pot.png", 132, 424, 24, "MM Ocean Spider House Pot Storage 3", "Ocean Spider House Pot Storage 3"),
						M("Pot.png", 499, 354, 24, "MM Ocean Spider House Pot Storage 4", "Ocean Spider House Pot Storage 4"),
						M("Pot.png", 354, 257, 24, "MM Ocean Spider House Pot Storage Top 1", "Ocean Spider House Pot Storage Top 1"),
						M("Pot.png", 515, 257, 24, "MM Ocean Spider House Pot Storage Top 2", "Ocean Spider House Pot Storage Top 2"),
						M("Pot.png", 593, 257, 24, "MM Ocean Spider House Pot Storage Top 3", "Ocean Spider House Pot Storage Top 3")
                    }
                },
				new MapSubRegion
                {
                    Name = "Masks Room",
                    BackgroundImage = "region maps/MM/Great_Bay_Coast/HS_3.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Gold_Skulltula.png", 707, 394, 40, "MM Ocean Skulltula Colored Skulls Behind Picture", "Ocean Skulltula Colored Skulls Behind Picture"),
						M("Gold_Skulltula.png", 767, 220, 40, "MM Ocean Skulltula Colored Skulls Ceiling Edge", "Ocean Skulltula Colored Skulls Ceiling Edge"),
						M("Gold_Skulltula.png", 707, 303, 40, "MM Ocean Skulltula Colored Skulls Chandelier 1", "Ocean Skulltula Colored Skulls Chandelier 1"),
						M("Gold_Skulltula.png", 716, 273, 40, "MM Ocean Skulltula Colored Skulls Chandelier 2", "Ocean Skulltula Colored Skulls Chandelier 2"),
						M("Gold_Skulltula.png", 679, 268, 40, "MM Ocean Skulltula Colored Skulls Chandelier 3", "Ocean Skulltula Colored Skulls Chandelier 3"),
						M("Gold_Skulltula.png", 641, 406, 40, "MM Ocean Skulltula Colored Skulls Pot", "Ocean Skulltula Colored Skulls Pot"),
						M("Chest.png", 202, 322, 40, "MM Ocean Spider House Chest HP", "Ocean Spider House Chest HP"),
						M("Pot.png", 815, 369, 24, "MM Ocean Spider House Pot Colored Skulls 1", "Ocean Spider House Pot Colored Skulls 1"),
						M("Pot.png", 798, 351, 24, "MM Ocean Spider House Pot Colored Skulls 2", "Ocean Spider House Pot Colored Skulls 2")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion BeneaththeWell()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Beneath the Well";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Beneath the Well",
                    BackgroundImage = "region maps/MM/Ikana_Canyon/Beneath_Well.png",
                    DestinationEntranceIds = new List<string>
					{
						"MM_BENEATH_THE_WELL",
						"MM_BENEATH_THE_WELL_BACK"
					},
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 1069, 405, 40, "MM Beneath The Well Mirror Shield", "Beneath The Well Mirror Shield"),
						M("Chest.png", 817, 430, 40, "MM Beneath The Well Keese Chest", "Beneath The Well Keese Chest"),
						M("Chest.png", 694, 702, 40, "MM Beneath The Well Skulltulla Chest", "Beneath The Well Skulltulla Chest"),
						M("Cow.png", 1297, 894, 40, "MM Beneath The Well Cow", "Beneath The Well Cow"),
						M("Tree.png", 1316, 937, 24, "MM Beneath The Well Tree", "Beneath The Well Tree"),
						M("Bush.png", 1290, 941, 24, "MM Beneath The Well Bush 1", "Beneath The Well Bush 1"),
						M("Bush.png", 1279, 884, 24, "MM Beneath The Well Bush 2", "Beneath The Well Bush 2"),
						M("Grass.png", 1073, 984, 24, "MM Beneath The Well Grass Before Poe 1", "Beneath The Well Grass Before Poe 1"),
						M("Grass.png", 1065, 1008, 24, "MM Beneath The Well Grass Before Poe 2", "Beneath The Well Grass Before Poe 2"),
						M("Grass.png", 1052, 993, 24, "MM Beneath The Well Grass Before Poe 3", "Beneath The Well Grass Before Poe 3"),
						M("Grass.png", 1044, 969, 24, "MM Beneath The Well Grass Before Poe 4", "Beneath The Well Grass Before Poe 4"),
						M("Grass.png", 1309, 922, 24, "MM Beneath The Well Grass Cow 1", "Beneath The Well Grass Cow 1"),
						M("Grass.png", 1287, 917, 24, "MM Beneath The Well Grass Cow 2", "Beneath The Well Grass Cow 2"),
						M("Grass.png", 1278, 899, 24, "MM Beneath The Well Grass Cow 3", "Beneath The Well Grass Cow 3"),
						M("Grass.png", 536, 522, 24, "MM Beneath The Well Grass Left Side 1", "Beneath The Well Grass Left Side 1"),
						M("Grass.png", 524, 547, 24, "MM Beneath The Well Grass Left Side 2", "Beneath The Well Grass Left Side 2"),
						M("Grass.png", 825, 519, 24, "MM Beneath The Well Grass Near End 1", "Beneath The Well Grass Near End 1"),
						M("Grass.png", 824, 538, 24, "MM Beneath The Well Grass Near End 2", "Beneath The Well Grass Near End 2"),
						M("Grass.png", 809, 549, 24, "MM Beneath The Well Grass Near End 3", "Beneath The Well Grass Near End 3"),
						M("Grass.png", 806, 509, 24, "MM Beneath The Well Grass Near End 4", "Beneath The Well Grass Near End 4"),
						M("Grass.png", 793, 525, 24, "MM Beneath The Well Grass Near End 5", "Beneath The Well Grass Near End 5"),
						M("Pot.png", 1155, 727, 24, "MM Beneath The Well Pot Big Poe 1", "Beneath The Well Pot Big Poe 1"),
						M("Pot.png", 1155, 594, 24, "MM Beneath The Well Pot Big Poe 2", "Beneath The Well Pot Big Poe 2"),
						M("Pot.png", 1011, 727, 24, "MM Beneath The Well Pot Big Poe 3", "Beneath The Well Pot Big Poe 3"),
						M("Pot.png", 1011, 594, 24, "MM Beneath The Well Pot Big Poe 4", "Beneath The Well Pot Big Poe 4"),
						M("Pot.png", 697, 512, 24, "MM Beneath The Well Pot Left Side 1", "Beneath The Well Pot Left Side 1"),
						M("Pot.png", 697, 544, 24, "MM Beneath The Well Pot Left Side 2", "Beneath The Well Pot Left Side 2"),
						M("Pot.png", 697, 496, 24, "MM Beneath The Well Pot Left Side 3", "Beneath The Well Pot Left Side 3"),
						M("Pot.png", 697, 560, 24, "MM Beneath The Well Pot Left Side 4", "Beneath The Well Pot Left Side 4"),
						M("Pot.png", 697, 528, 24, "MM Beneath The Well Pot Left Side 5", "Beneath The Well Pot Left Side 5"),
						M("Pot.png", 908, 745, 24, "MM Beneath The Well Pot Middle 01", "Beneath The Well Pot Middle 01"),
						M("Pot.png", 908, 729, 24, "MM Beneath The Well Pot Middle 02", "Beneath The Well Pot Middle 02"),
						M("Pot.png", 908, 713, 24, "MM Beneath The Well Pot Middle 03", "Beneath The Well Pot Middle 03"),
						M("Pot.png", 908, 697, 24, "MM Beneath The Well Pot Middle 04", "Beneath The Well Pot Middle 04"),
						M("Pot.png", 908, 681, 24, "MM Beneath The Well Pot Middle 05", "Beneath The Well Pot Middle 05"),
						M("Pot.png", 908, 665, 24, "MM Beneath The Well Pot Middle 06", "Beneath The Well Pot Middle 06"),
						M("Pot.png", 908, 649, 24, "MM Beneath The Well Pot Middle 07", "Beneath The Well Pot Middle 07"),
						M("Pot.png", 908, 633, 24, "MM Beneath The Well Pot Middle 08", "Beneath The Well Pot Middle 08"),
						M("Pot.png", 908, 617, 24, "MM Beneath The Well Pot Middle 09", "Beneath The Well Pot Middle 09"),
						M("Pot.png", 908, 601, 24, "MM Beneath The Well Pot Middle 10", "Beneath The Well Pot Middle 10"),
						M("Fairy.png", 617, 82, 24, "MM Beneath The Well Fairy Fountain Fairy 1", "Beneath The Well Fairy Fountain Fairy 1"),
						M("Fairy.png", 620, 105, 24, "MM Beneath The Well Fairy Fountain Fairy 2", "Beneath The Well Fairy Fountain Fairy 2"),
						M("Fairy.png", 598, 97, 24, "MM Beneath The Well Fairy Fountain Fairy 3", "Beneath The Well Fairy Fountain Fairy 3"),
						M("Fairy.png", 598, 73, 24, "MM Beneath The Well Fairy Fountain Fairy 4", "Beneath The Well Fairy Fountain Fairy 4"),
						M("Fairy.png", 613, 56, 24, "MM Beneath The Well Fairy Fountain Fairy 5", "Beneath The Well Fairy Fountain Fairy 5"),
						M("Fairy.png", 633, 59, 24, "MM Beneath The Well Fairy Fountain Fairy 6", "Beneath The Well Fairy Fountain Fairy 6"),
						M("Fairy.png", 643, 75, 24, "MM Beneath The Well Fairy Fountain Fairy 7", "Beneath The Well Fairy Fountain Fairy 7"),
						M("Fairy.png", 639, 96, 24, "MM Beneath The Well Fairy Fountain Fairy 8", "Beneath The Well Fairy Fountain Fairy 8"),
						
						ME("Entrance.png", 1070, 227, "Entrance shuffle (Ikana Castle Exterior)", "MM_IKANA_CASTLE_EXTERIOR_FROM_WELL"),
						ME("Entrance.png", 596, 1013, "Entrance shuffle (Ikana Canyon)", "MM_IKANA_CANYON_FROM_WELL")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion AncientCastleofIkana()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Ancient Castle of Ikana";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Exterior",
                    BackgroundImage = "region maps/MM/Ikana_Canyon/Castle_Exterior.png",
					DestinationEntranceIds = new List<string>
					{
						"MM_IKANA_CASTLE_EXTERIOR_FROM_CASTLE",
						"MM_IKANA_CASTLE_EXTERIOR_FROM_WELL"
					},
                    Marks = new List<MapMark>
                    {
						M("Collectible.png", 230, 5, 40, "MM Ancient Castle of Ikana HP", "Ancient Castle of Ikana HP"),
						M("Grass.png", 680, 165, 24, "MM Ancient Castle of Ikana Grass 01", "Ancient Castle of Ikana Grass 01"),
						M("Grass.png", 660, 154, 24, "MM Ancient Castle of Ikana Grass 02", "Ancient Castle of Ikana Grass 02"),
						M("Grass.png", 680, 143, 24, "MM Ancient Castle of Ikana Grass 03", "Ancient Castle of Ikana Grass 03"),
						M("Grass.png", 671, 208, 24, "MM Ancient Castle of Ikana Grass 04", "Ancient Castle of Ikana Grass 04"),
						M("Grass.png", 674, 286, 24, "MM Ancient Castle of Ikana Grass 05", "Ancient Castle of Ikana Grass 05"),
						M("Grass.png", 519, 347, 24, "MM Ancient Castle of Ikana Grass 06", "Ancient Castle of Ikana Grass 06"),
						M("Grass.png", 519, 368, 24, "MM Ancient Castle of Ikana Grass 07", "Ancient Castle of Ikana Grass 07"),
						M("Grass.png", 539, 347, 24, "MM Ancient Castle of Ikana Grass 08", "Ancient Castle of Ikana Grass 08"),
						M("Grass.png", 263, 204, 24, "MM Ancient Castle of Ikana Grass 09", "Ancient Castle of Ikana Grass 09"),
						M("Grass.png", 207, 31, 24, "MM Ancient Castle of Ikana Grass 10", "Ancient Castle of Ikana Grass 10"),
						M("Grass.png", 207, 52, 24, "MM Ancient Castle of Ikana Grass 11", "Ancient Castle of Ikana Grass 11"),
						M("Grass.png", 227, 31, 24, "MM Ancient Castle of Ikana Grass 12", "Ancient Castle of Ikana Grass 12"),
						M("Pot.png", 714, 46, 24, "MM Ancient Castle of Ikana Pot Exterior", "Ancient Castle of Ikana Pot Exterior"),
						
						ME("Entrance.png", 235, 98, "Entrance shuffle (Beneath the Well)", "MM_BENEATH_THE_WELL_BACK"),
						ME("Entrance.png", 456, 406, "Entrance shuffle (Ikana Castle Interior)", "MM_IKANA_CASTLE"),
						ME("Entrance.png", 464, 177, "Entrance shuffle (Ikana Castle Interior)", "MM_IKANA_CASTLE_INTERIOR_KEG"),
						ME("Entrance.png", 585, 146, "Entrance shuffle (Ikana Castle Interior)", "MM_IKANA_CASTLE_INTERIOR_BLOCK")
                    }
                },
				new MapSubRegion
                {
                    Name = "Interior",
                    BackgroundImage = "region maps/MM/Ikana_Canyon/Castle_Inside.png",
                    DestinationEntranceIds = new List<string>
					{
						"MM_IKANA_CASTLE",
						"MM_IKANA_CASTLE_INTERIOR_KEG",
						"MM_IKANA_CASTLE_INTERIOR_BLOCK"
					},
                    Marks = new List<MapMark>
                    {
						M("NPC.png", 462, 59, 40, "MM Ancient Castle of Ikana Song Emptiness", "Ancient Castle of Ikana Song Emptiness"),
						M("Pot.png", 437, 573, 24, "MM Ancient Castle of Ikana Pot Entrance 1", "Ancient Castle of Ikana Pot Entrance 1"),
						M("Pot.png", 491, 573, 24, "MM Ancient Castle of Ikana Pot Entrance 2", "Ancient Castle of Ikana Pot Entrance 2"),
						M("Pot.png", 360, 484, 24, "MM Ancient Castle of Ikana Pot Left First Room 1", "Ancient Castle of Ikana Pot Left First Room 1"),
						M("Pot.png", 360, 521, 24, "MM Ancient Castle of Ikana Pot Left First Room 2", "Ancient Castle of Ikana Pot Left First Room 2"),
						M("Pot.png", 381, 178, 24, "MM Ancient Castle of Ikana Pot Left Second Room 1", "Ancient Castle of Ikana Pot Left Second Room 1"),
						M("Pot.png", 381, 228, 24, "MM Ancient Castle of Ikana Pot Left Second Room 2", "Ancient Castle of Ikana Pot Left Second Room 2"),
						M("Pot.png", 381, 212, 24, "MM Ancient Castle of Ikana Pot Left Second Room 3", "Ancient Castle of Ikana Pot Left Second Room 3"),
						M("Pot.png", 381, 196, 24, "MM Ancient Castle of Ikana Pot Left Second Room 4", "Ancient Castle of Ikana Pot Left Second Room 4"),
						M("Pot.png", 300, 21, 24, "MM Ancient Castle of Ikana Pot Left Third Room 1", "Ancient Castle of Ikana Pot Left Third Room 1"),
						M("Pot.png", 346, 21, 24, "MM Ancient Castle of Ikana Pot Left Third Room 2", "Ancient Castle of Ikana Pot Left Third Room 2"),
						M("Pot.png", 592, 21, 24, "MM Ancient Castle of Ikana Pot Right 1", "Ancient Castle of Ikana Pot Right 1"),
						M("Pot.png", 636, 21, 24, "MM Ancient Castle of Ikana Pot Right 2", "Ancient Castle of Ikana Pot Right 2"),
						M("Pot.png", 444, 315, 24, "MM Ancient Castle of Ikana Boss Pot 1", "Ancient Castle of Ikana Boss Pot 1"),
						M("Pot.png", 444, 291, 24, "MM Ancient Castle of Ikana Boss Pot 2", "Ancient Castle of Ikana Boss Pot 2"),
						M("Pot.png", 494, 315, 24, "MM Ancient Castle of Ikana Boss Pot 3", "Ancient Castle of Ikana Boss Pot 3"),
						M("Pot.png", 494, 291, 24, "MM Ancient Castle of Ikana Boss Pot 4", "Ancient Castle of Ikana Boss Pot 4"),
						M("Pot.png", 530, 250, 24, "MM Ancient Castle of Ikana Boss Pot 5", "Ancient Castle of Ikana Boss Pot 5"),
						M("Pot.png", 410, 250, 24, "MM Ancient Castle of Ikana Boss Pot 6", "Ancient Castle of Ikana Boss Pot 6"),
						M("Pot.png", 530, 112, 24, "MM Ancient Castle of Ikana Boss Pot 7", "Ancient Castle of Ikana Boss Pot 7"),
						M("Pot.png", 410, 112, 24, "MM Ancient Castle of Ikana Boss Pot 8", "Ancient Castle of Ikana Boss Pot 8"),
						
						ME("Entrance.png", 457, 582, "Entrance shuffle (Ikana Castle Exterior)", "MM_IKANA_VALLEY_FROM_SHRINE")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion SecretShrine()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "Secret Shrine";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Secret Shrine",
                    BackgroundImage = "region maps/MM/Ikana_Canyon/Shrine.png",
                    DestinationEntranceIds = new List<string> { "MM_SECRET_SHRINE" },
                    Marks = new List<MapMark>
                    {
						M("Chest.png", 108, 502, 40, "MM Secret Shrine Dinolfos Chest", "Secret Shrine Dinolfos Chest"),
						M("Chest.png", 793, 514, 40, "MM Secret Shrine Garo Master Chest", "Secret Shrine Garo Master Chest"),
						M("Chest.png", 453, 284, 40, "MM Secret Shrine HP Chest", "Secret Shrine HP Chest"),
						M("Chest.png", 772, 152, 40, "MM Secret Shrine Wart Chest", "Secret Shrine Wart Chest"),
						M("Chest.png", 110, 152, 40, "MM Secret Shrine Wizzrobe Chest", "Secret Shrine Wizzrobe Chest"),
						M("Grass.png", 333, 450, 24, "MM Secret Shrine Grass Dinolfos 1", "Secret Shrine Grass Dinolfos 1"),
						M("Grass.png", 299, 463, 24, "MM Secret Shrine Grass Dinolfos 2", "Secret Shrine Grass Dinolfos 2"),
						M("Grass.png", 258, 489, 24, "MM Secret Shrine Grass Dinolfos 3", "Secret Shrine Grass Dinolfos 3"),
						M("Grass.png", 236, 514, 24, "MM Secret Shrine Grass Dinolfos 4", "Secret Shrine Grass Dinolfos 4"),
						M("Grass.png", 425, 398, 24, "MM Secret Shrine Grass Entrance 1", "Secret Shrine Grass Entrance 1"),
						M("Grass.png", 431, 416, 24, "MM Secret Shrine Grass Entrance 2", "Secret Shrine Grass Entrance 2"),
						M("Grass.png", 402, 416, 24, "MM Secret Shrine Grass Entrance 3", "Secret Shrine Grass Entrance 3"),
						M("Grass.png", 495, 398, 24, "MM Secret Shrine Grass Entrance 4", "Secret Shrine Grass Entrance 4"),
						M("Grass.png", 489, 416, 24, "MM Secret Shrine Grass Entrance 5", "Secret Shrine Grass Entrance 5"),
						M("Grass.png", 518, 416, 24, "MM Secret Shrine Grass Entrance 6", "Secret Shrine Grass Entrance 6"),
						M("Grass.png", 854, 429, 24, "MM Secret Shrine Grass Garo Master 1", "Secret Shrine Grass Garo Master 1"),
						M("Grass.png", 729, 581, 24, "MM Secret Shrine Grass Garo Master 2", "Secret Shrine Grass Garo Master 2"),
						M("Grass.png", 584, 449, 24, "MM Secret Shrine Grass Garo Master 3", "Secret Shrine Grass Garo Master 3"),
						M("Grass.png", 699, 310, 24, "MM Secret Shrine Grass Garo Master 4", "Secret Shrine Grass Garo Master 4"),
						M("Grass.png", 729, 310, 24, "MM Secret Shrine Grass Garo Master 5", "Secret Shrine Grass Garo Master 5"),
						M("Grass.png", 584, 423, 24, "MM Secret Shrine Grass Garo Master 6", "Secret Shrine Grass Garo Master 6"),
						M("Grass.png", 615, 71, 24, "MM Secret Shrine Grass Wart 1", "Secret Shrine Grass Wart 1"),
						M("Grass.png", 597, 86, 24, "MM Secret Shrine Grass Wart 2", "Secret Shrine Grass Wart 2"),
						M("Grass.png", 597, 236, 24, "MM Secret Shrine Grass Wart 3", "Secret Shrine Grass Wart 3"),
						M("Grass.png", 615, 251, 24, "MM Secret Shrine Grass Wart 4", "Secret Shrine Grass Wart 4"),
						M("Grass.png", 781, 236, 24, "MM Secret Shrine Grass Wart 5", "Secret Shrine Grass Wart 5"),
						M("Grass.png", 763, 251, 24, "MM Secret Shrine Grass Wart 6", "Secret Shrine Grass Wart 6"),
						M("Grass.png", 763, 71, 24, "MM Secret Shrine Grass Wart 7", "Secret Shrine Grass Wart 7"),
						M("Grass.png", 781, 86, 24, "MM Secret Shrine Grass Wart 8", "Secret Shrine Grass Wart 8"),
						M("Grass.png", 109, 236, 24, "MM Secret Shrine Grass Wizzrobe 1", "Secret Shrine Grass Wizzrobe 1"),
						M("Grass.png", 131, 250, 24, "MM Secret Shrine Grass Wizzrobe 2", "Secret Shrine Grass Wizzrobe 2"),
						M("Grass.png", 139, 57, 24, "MM Secret Shrine Grass Wizzrobe 3", "Secret Shrine Grass Wizzrobe 3"),
						M("Grass.png", 309, 75, 24, "MM Secret Shrine Grass Wizzrobe 4", "Secret Shrine Grass Wizzrobe 4"),
						M("Grass.png", 316, 234, 24, "MM Secret Shrine Grass Wizzrobe 5", "Secret Shrine Grass Wizzrobe 5"),
						M("Pot.png", 540, 444, 24, "MM Secret Shrine Pot 1", "Secret Shrine Pot 1"),
						M("Pot.png", 540, 492, 24, "MM Secret Shrine Pot 2", "Secret Shrine Pot 2"),
						M("Pot.png", 540, 468, 24, "MM Secret Shrine Pot 3", "Secret Shrine Pot 3"),
						M("Pot.png", 397, 266, 24, "MM Secret Shrine Pot 4", "Secret Shrine Pot 4"),
						M("Pot.png", 397, 329, 24, "MM Secret Shrine Pot 5", "Secret Shrine Pot 5"),
						M("Pot.png", 430, 232, 24, "MM Secret Shrine Pot 6", "Secret Shrine Pot 6"),
						M("Pot.png", 491, 232, 24, "MM Secret Shrine Pot 7", "Secret Shrine Pot 7"),
						M("Pot.png", 522, 266, 24, "MM Secret Shrine Pot 8", "Secret Shrine Pot 8"),
						M("Pot.png", 522, 329, 24, "MM Secret Shrine Pot 9", "Secret Shrine Pot 9"),
						M("Soil.png", 460, 458, 24, "MM Secret Shrine Soil Item 1", "Secret Shrine Soil Item 1"),
						M("Soil.png", 472, 482, 24, "MM Secret Shrine Soil Item 2", "Secret Shrine Soil Item 2"),
						M("Soil.png", 448, 482, 24, "MM Secret Shrine Soil Item 3", "Secret Shrine Soil Item 3"),
						M("Rupee.png", 397, 428, 24, "MM Secret Shrine Rupee 01", "Secret Shrine Rupee 01"),
						M("Rupee.png", 397, 451, 24, "MM Secret Shrine Rupee 02", "Secret Shrine Rupee 02"),
						M("Rupee.png", 397, 474, 24, "MM Secret Shrine Rupee 03", "Secret Shrine Rupee 03"),
						M("Rupee.png", 397, 497, 24, "MM Secret Shrine Rupee 04", "Secret Shrine Rupee 04"),
						M("Rupee.png", 397, 520, 24, "MM Secret Shrine Rupee 05", "Secret Shrine Rupee 05"),
						M("Rupee.png", 413, 528, 24, "MM Secret Shrine Rupee 06", "Secret Shrine Rupee 06"),
						M("Rupee.png", 429, 536, 24, "MM Secret Shrine Rupee 07", "Secret Shrine Rupee 07"),
						M("Rupee.png", 445, 544, 24, "MM Secret Shrine Rupee 08", "Secret Shrine Rupee 08"),
						M("Rupee.png", 461, 552, 24, "MM Secret Shrine Rupee 09", "Secret Shrine Rupee 09"),
						M("Rupee.png", 477, 544, 24, "MM Secret Shrine Rupee 10", "Secret Shrine Rupee 10"),
						M("Rupee.png", 493, 536, 24, "MM Secret Shrine Rupee 11", "Secret Shrine Rupee 11"),
						M("Rupee.png", 509, 528, 24, "MM Secret Shrine Rupee 12", "Secret Shrine Rupee 12"),
						M("Rupee.png", 525, 520, 24, "MM Secret Shrine Rupee 13", "Secret Shrine Rupee 13"),
						M("Rupee.png", 525, 497, 24, "MM Secret Shrine Rupee 14", "Secret Shrine Rupee 14"),
						M("Rupee.png", 525, 474, 24, "MM Secret Shrine Rupee 15", "Secret Shrine Rupee 15"),
						M("Rupee.png", 525, 451, 24, "MM Secret Shrine Rupee 16", "Secret Shrine Rupee 16"),
						M("Rupee.png", 525, 428, 24, "MM Secret Shrine Rupee 17", "Secret Shrine Rupee 17"),
						
						ME("Entrance.png", 452, 581, "Entrance shuffle (Ikana Canyon)", "MM_IKANA_VALLEY_FROM_SHRINE")
                    }
                }
			};
			return mapRegion;
		}
		public static MapRegion Moon()
        {
            MapRegion mapRegion = new MapRegion();
            mapRegion.Name = "The Moon";
            mapRegion.Game = "MM";
            mapRegion.SubRegions = new List<MapSubRegion>
            {
				new MapSubRegion
                {
                    Name = "Moon",
                    BackgroundImage = "region maps/MM/Moon/Moon.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("NPC.png", 494, 289, 40, "MM Moon Fierce Deity Mask", "Moon Fierce Deity Mask"),
						M("Butterfly.png", 469, 273, 24, "MM Moon Butterfly 01", "Moon Butterfly 01"),
						M("Butterfly.png", 486, 363, 24, "MM Moon Butterfly 02", "Moon Butterfly 02"),
						M("Butterfly.png", 387, 308, 24, "MM Moon Butterfly 03", "Moon Butterfly 03"),
						M("Butterfly.png", 377, 401, 24, "MM Moon Butterfly 04", "Moon Butterfly 04"),
						M("Butterfly.png", 307, 421, 24, "MM Moon Butterfly 05", "Moon Butterfly 05"),
						M("Butterfly.png", 425, 359, 24, "MM Moon Butterfly 06", "Moon Butterfly 06"),
						M("Butterfly.png", 413, 214, 24, "MM Moon Butterfly 07", "Moon Butterfly 07"),
						M("Butterfly.png", 492, 183, 24, "MM Moon Butterfly 08", "Moon Butterfly 08"),
						M("Butterfly.png", 569, 328, 24, "MM Moon Butterfly 09", "Moon Butterfly 09"),
						M("Butterfly.png", 364, 253, 24, "MM Moon Butterfly 10", "Moon Butterfly 10"),
						M("Butterfly.png", 525, 240, 24, "MM Moon Butterfly 11", "Moon Butterfly 11"),
						M("Butterfly.png", 581, 213, 24, "MM Moon Butterfly 12", "Moon Butterfly 12"),
						M("Butterfly.png", 301, 486, 24, "MM Moon Butterfly 13", "Moon Butterfly 13")
                    }
                },
				new MapSubRegion
                {
                    Name = "Deku Trial",
                    BackgroundImage = "region maps/MM/Moon/Deku.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Collectible.png", 192, 148, 40, "MM Moon Trial Deku HP", "Moon Trial Deku HP")
                    }
                },
				new MapSubRegion
                {
                    Name = "Goron Trial",
                    BackgroundImage = "region maps/MM/Moon/Goron.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Collectible.png", 195, 415, 40, "MM Moon Trial Goron HP", "Moon Trial Goron HP"),
						M("Pot.png", 343, 256, 24, "MM Moon Trial Goron Pot 01", "Moon Trial Goron Pot 01"),
						M("Pot.png", 321, 256, 24, "MM Moon Trial Goron Pot 02", "Moon Trial Goron Pot 02"),
						M("Pot.png", 662, 231, 24, "MM Moon Trial Goron Pot 03", "Moon Trial Goron Pot 03"),
						M("Pot.png", 684, 231, 24, "MM Moon Trial Goron Pot 04", "Moon Trial Goron Pot 04"),
						M("Pot.png", 640, 231, 24, "MM Moon Trial Goron Pot 05", "Moon Trial Goron Pot 05"),
						M("Pot.png", 412, 349, 24, "MM Moon Trial Goron Pot 06", "Moon Trial Goron Pot 06"),
						M("Pot.png", 412, 373, 24, "MM Moon Trial Goron Pot 07", "Moon Trial Goron Pot 07"),
						M("Pot.png", 136, 347, 24, "MM Moon Trial Goron Pot 08", "Moon Trial Goron Pot 08"),
						M("Pot.png", 136, 371, 24, "MM Moon Trial Goron Pot 09", "Moon Trial Goron Pot 09"),
						M("Pot.png", 158, 347, 24, "MM Moon Trial Goron Pot 10", "Moon Trial Goron Pot 10"),
						M("Pot.png", 158, 371, 24, "MM Moon Trial Goron Pot 11", "Moon Trial Goron Pot 11"),
						M("Pot.png", 891, 347, 24, "MM Moon Trial Goron Pot Early 1", "Moon Trial Goron Pot Early 1"),
						M("Pot.png", 913, 347, 24, "MM Moon Trial Goron Pot Early 2", "Moon Trial Goron Pot Early 2"),
						M("Pot.png", 891, 371, 24, "MM Moon Trial Goron Pot Early 3", "Moon Trial Goron Pot Early 3"),
						M("Pot.png", 913, 371, 24, "MM Moon Trial Goron Pot Early 4", "Moon Trial Goron Pot Early 4"),
						M("Icicle.png", 401, 33, 24, "MM Moon Trial Goron Icicle 1", "Moon Trial Goron Icicle 1"),
						M("Icicle.png", 425, 33, 24, "MM Moon Trial Goron Icicle 2", "Moon Trial Goron Icicle 2"),
						M("Icicle.png", 401, 57, 24, "MM Moon Trial Goron Icicle 3", "Moon Trial Goron Icicle 3"),
						M("Icicle.png", 425, 57, 24, "MM Moon Trial Goron Icicle 4", "Moon Trial Goron Icicle 4")
                    }
                },
				new MapSubRegion
                {
                    Name = "Zora Trial",
                    BackgroundImage = "region maps/MM/Moon/Zora.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Collectible.png", 54, 343, 40, "MM Moon Trial Zora HP", "Moon Trial Zora HP")
                    }
                },
				new MapSubRegion
                {
                    Name = "Link Trial",
                    BackgroundImage = "region maps/MM/Moon/Link.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Collectible.png", 808, 276, 40, "MM Moon Trial Link HP", "Moon Trial Link HP"),
						M("Chest.png", 455, 282, 40, "MM Moon Trial Link Garo Master Chest", "Moon Trial Link Garo Master Chest"),
						M("Chest.png", 654, 278, 40, "MM Moon Trial Link Iron Knuckle Chest", "Moon Trial Link Iron Knuckle Chest"),
						M("Pot.png", 126, 233, 24, "MM Moon Trial Link Pot 1", "Moon Trial Link Pot 1"),
						M("Pot.png", 126, 257, 24, "MM Moon Trial Link Pot 2", "Moon Trial Link Pot 2"),
						M("Pot.png", 104, 233, 24, "MM Moon Trial Link Pot 3", "Moon Trial Link Pot 3"),
						M("Pot.png", 104, 257, 24, "MM Moon Trial Link Pot 4", "Moon Trial Link Pot 4"),
						M("Pot.png", 126, 330, 24, "MM Moon Trial Link Pot 5", "Moon Trial Link Pot 5"),
						M("Pot.png", 126, 354, 24, "MM Moon Trial Link Pot 6", "Moon Trial Link Pot 6"),
						M("Pot.png", 104, 330, 24, "MM Moon Trial Link Pot 7", "Moon Trial Link Pot 7"),
						M("Pot.png", 104, 354, 24, "MM Moon Trial Link Pot 8", "Moon Trial Link Pot 8")
                    }
                },
				new MapSubRegion
                {
                    Name = "Majora Lair",
                    BackgroundImage = "region maps/MM/Moon/Majora.png",
                    DestinationEntranceIds = new List<string> { },
                    Marks = new List<MapMark>
                    {
						M("Pot.png", 483, 88, 24, "MM Moon Majora Pot 1", "Moon Majora Pot 1"),
						M("Pot.png", 439, 88, 24, "MM Moon Majora Pot 2", "Moon Majora Pot 2")
                    }
                }
			};
			return mapRegion;
		}
        public static List<MapRegion> GetAll()
        {
            return new List<MapRegion>
            {
                DeathMountainCrater(),
                DeathMountainTrail(),
                DesertColossus(),
                GerudoFortress(),
                GerudoValley(),
                GoronCity(),
                Graveyard(),
                HauntedWasteland(),
                HyruleCastle(),
                HyruleField(),
                Kakariko(),
                KokiriForest(),
                LakeHylia(),
                LonLonRanch(),
                LostWoods(),
                Market(),
                SacredForestMeadow(),
                ZoraDomain(),
                ZoraFountain(),
                ZoraRiver(),
                //MM
                ClockTown(),
                DekuPalace(),
                GoronVillage(),
                GreatBayCoast(),
                IkanaCanyon(),
                IkanaGraveyard(),
                MilkRoad(),
                MountainVillage(),
                PathtoMountainVillage(),
                PathtoSnowhead(),
                RoadtoIkana(),
                RoadtoSouthernSwamp(),
                RomaniRanch(),
                Snowhead(),
                SouthernSwamp(),
                StoneTower(),
                TerminaField(),
                TwinIslands(),
                Woodfall(),
                WoodsofMystery(),
                ZoraCape(),
                ZoraHall(),
                //Dungeons
                DekuTree(),
                DodongoCavern(),
                JabuJabuBelly(),
                ForestTemple(),
                FireTemple(),
                WaterTemple(),
				ShadowTemple(),
				SpiritTemple(),
				IceCavern(),
				BottomoftheWell(),
				GerudoTrainingGround(),
				InsideGanonCastle(),
				GanonTower(),
				WoodfallTemple(),
				SnowheadTemple(),
				GreatBayTemple(),
				StoneTowerTemple(),
				InvertedStoneTowerTemple(),
				SwampSpiderHouse(),
				PirateFortress(),
				OceanSpiderHouse(),
				BeneaththeWell(),
				AncientCastleofIkana(),
				SecretShrine(),
                Moon()
            };
        }
    }
}
