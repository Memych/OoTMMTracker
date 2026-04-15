using System;
using System.Collections.Generic;
using OoTMMTracker.Models;

namespace OoTMMTracker.Services
{
    /// <summary>
    /// Mapping of starting item names from spoiler log → tracker item IDs.
    /// Automatically marks items as collected when loading the log.
    /// </summary>
    public static class StartingItemsMapper
    {
        // Mapping: name from log (lowercase) → tracker ID
        private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
        {
            // Clocks
            ["clock (day 1)"]   = "clock_day1",
            ["clock (night 1)"] = "clock_night1",
            ["clock (day 2)"]   = "clock_day2",
            ["clock (night 2)"] = "clock_night2",
            ["clock (day 3)"]   = "clock_day3",
            ["clock (night 3)"] = "clock_night3",

            // Swords OoT
            ["kokiri sword"]    = "oot_sword",
            ["razor sword"]     = "oot_sword",
            ["gilded sword"]    = "oot_sword",
            ["master sword"]    = "oot_master_sword",
            ["giant's knife"]   = "oot_biggoron_sword",
            ["biggoron's sword"]= "oot_biggoron_sword",
            ["great spin attack"] = "oot_great_spin",

            // Swords MM
            ["kokiri sword (mm)"]    = "mm_sword",
            ["razor sword (mm)"]     = "mm_sword",
            ["gilded sword (mm)"]    = "mm_sword",

            // Shields OoT
            ["deku shield"]     = "oot_deku_shield",
            ["hylian shield"]   = "oot_hylian_shield",
            ["mirror shield"]   = "oot_mirror_shield",

            // Shields MM
            ["hero's shield"]   = "mm_hero_shield",
            ["mirror shield (mm)"] = "mm_mirror_shield",

            // Tunics
            ["goron tunic"]     = "oot_goron_tunic",
            ["zora tunic"]      = "oot_zora_tunic",
            ["goron tunic (mm)"]= "mm_goron_tunic",
            ["zora tunic (mm)"] = "mm_zora_tunic",

            // Boots
            ["iron boots"]      = "oot_iron_boots",
            ["hover boots"]     = "oot_hover_boots",
            ["iron boots (mm)"] = "mm_iron_boots",
            ["hover boots (mm)"]= "mm_hover_boots",

            // Strength
            ["goron's bracelet"]    = "oot_strength",
            ["silver gauntlets"]    = "oot_strength",
            ["golden gauntlets"]    = "oot_strength",

            // Scales
            ["bronze scale"]    = "oot_scale",
            ["silver scale"]    = "oot_scale",
            ["golden scale"]    = "oot_scale",

            // Magic
            ["magic upgrade"]       = "oot_magic",
            ["large magic upgrade"] = "oot_magic",

            // Wallet
            ["child's wallet"]  = "oot_wallet",
            ["adult's wallet"]  = "oot_wallet",
            ["giant's wallet"]  = "oot_wallet",
            ["colossal wallet"] = "oot_wallet",
            ["bottomless wallet"] = "oot_wallet",

            // Ocarina
            ["fairy ocarina"]   = "oot_ocarina",
            ["ocarina of time"] = "oot_ocarina",

            // Items OoT
            ["fairy bow"]       = "oot_bow",
            ["fire arrows"]     = "oot_fire_arrows",
            ["ice arrows"]      = "oot_ice_arrows",
            ["light arrows"]    = "oot_light_arrows",
            ["fairy slingshot"] = "oot_slingshot",
            ["boomerang"]       = "oot_boomerang",
            ["hookshot"]        = "oot_hookshot",
            ["longshot"]        = "oot_hookshot",
            ["megaton hammer"]  = "oot_hammer",
            ["lens of truth"]   = "oot_lens",
            ["magic beans"]     = "oot_beans",
            ["magic beans (oot)"] = "oot_beans",
            ["bomb bag"]        = "oot_bomb_bag",
            ["deku stick"]      = "oot_deku_stick",
            ["deku nut"]        = "oot_deku_nut",
            ["din's fire"]      = "oot_dins_fire",
            ["farore's wind"]   = "oot_farores_wind",
            ["nayru's love"]    = "oot_nayrus_love",
            ["stone of agony"]  = "oot_stone_agony",
            ["gerudo membership card"] = "oot_gerudo_card",
            ["double defence"]  = "oot_double_defence",

            // Items MM
            ["hero's bow"]      = "mm_bow",
            ["fire arrows (mm)"]= "mm_fire_arrows",
            ["ice arrows (mm)"] = "mm_ice_arrows",
            ["light arrows (mm)"] = "mm_light_arrows",
            ["great fairy's sword"] = "mm_fairy_sword",
            ["hookshot (mm)"]   = "mm_hookshot",
            ["short hookshot"]  = "mm_hookshot",
            ["megaton hammer (mm)"] = "mm_hammer",
            ["lens of truth (mm)"] = "mm_lens",
            ["magic beans (mm)"]= "mm_beans",
            ["bomb bag (mm)"]   = "mm_bomb_bag",
            ["powder keg"]      = "mm_powder_keg",
            ["pictograph box"]  = "mm_pictograph",
            ["deku stick (mm)"] = "mm_deku_stick",
            ["deku nut (mm)"]   = "mm_deku_nut",
            ["din's fire (mm)"] = "mm_dins_fire",
            ["farore's wind (mm)"] = "mm_farores_wind",
            ["nayru's love (mm)"] = "mm_nayrus_love",
            ["stone of agony (mm)"] = "mm_stone_agony",
            ["bombers' notebook"] = "mm_notebook",
            ["great spin attack (mm)"] = "mm_great_spin",
            ["double defence (mm)"] = "mm_double_defence",

            // Tingle Maps
            ["tingle map: clock town"]   = "tingle_clock_town",
            ["tingle map: woodfall"]     = "tingle_woodfall",
            ["tingle map: snowhead"]     = "tingle_snowhead",
            ["tingle map: great bay"]    = "tingle_great_bay",
            ["tingle map: stone tower"]  = "tingle_stone_tower",
            ["tingle map: romani ranch"] = "tingle_romani_ranch",
            // Short variants
            ["tingle map clock town"]    = "tingle_clock_town",
            ["tingle map woodfall"]      = "tingle_woodfall",
            ["tingle map snowhead"]      = "tingle_snowhead",
            ["tingle map great bay"]     = "tingle_great_bay",
            ["tingle map stone tower"]   = "tingle_stone_tower",
            ["tingle map romani ranch"]  = "tingle_romani_ranch",
            ["deku mask"]       = "deku_mask",
            ["goron mask"]      = "goron_mask",
            ["zora mask"]       = "zora_mask",
            ["fierce deity's mask"] = "fierce_deity",
            ["postman's hat"]   = "postman_hat",
            ["all-night mask"]  = "all_night_mask",
            ["blast mask"]      = "blast_mask",
            ["stone mask"]      = "stone_mask",
            ["great fairy's mask"] = "great_fairy_mask",
            ["keaton mask"]     = "keaton_mask",
            ["bremen mask"]     = "bremen_mask",
            ["bunny hood"]      = "bunny_hood",
            ["don gero's mask"] = "don_gero_mask",
            ["mask of scents"]  = "mask_of_scents",
            ["romani's mask"]   = "romani_mask",
            ["circus leader's mask"] = "circus_leader_mask",
            ["kafei's mask"]    = "kafei_mask",
            ["couple's mask"]   = "couples_mask",
            ["mask of truth"]   = "mask_of_truth",
            ["kamaro's mask"]   = "kamaro_mask",
            ["gibdo mask"]      = "gibdo_mask",
            ["garo's mask"]     = "garo_mask",
            ["captain's hat"]   = "captain_hat",
            ["giant's mask"]    = "giants_mask",

            // Masks OoT
            ["keaton mask (oot)"] = "oot_keaton_mask",
            ["skull mask"]      = "oot_skull_mask",
            ["spooky mask"]     = "oot_spooky_mask",
            ["bunny hood (oot)"]= "oot_bunny_hood",
            ["goron mask (oot)"]= "oot_goron_mask",
            ["zora mask (oot)"] = "oot_zora_mask",
            ["gerudo mask"]     = "oot_gerudo_mask",
            ["mask of truth (oot)"] = "oot_truth_mask",

            // Songs OoT
            ["zelda's lullaby"] = "oot_zelda_lullaby",
            ["epona's song"]    = "oot_epona_song",
            ["saria's song"]    = "oot_saria_song",
            ["sun's song"]      = "oot_sun_song",
            ["song of time"]    = "oot_song_of_time",
            ["song of storms"]  = "oot_song_of_storms",
            ["minuet of forest"]= "oot_minuet",
            ["bolero of fire"]  = "oot_bolero",
            ["serenade of water"] = "oot_serenade",
            ["requiem of spirit"] = "oot_requiem",
            ["nocturne of shadow"] = "oot_nocturne",
            ["prelude of light"]= "oot_prelude",
            ["elegy of emptiness"] = "oot_elegy",

            // Songs MM
            ["song of time (mm)"]   = "mm_song_of_time",
            ["song of healing"]     = "mm_song_of_healing",
            ["epona's song (mm)"]   = "mm_epona_song",
            ["song of soaring"]     = "mm_song_of_soaring",
            ["song of storms (mm)"] = "mm_song_of_storms",
            ["sonata of awakening"] = "mm_sonata",
            ["goron lullaby"]       = "mm_goron_lullaby",
            ["goron lullaby intro"] = "mm_goron_lullaby",
            ["new wave bossa nova"] = "mm_new_wave",
            ["elegy of emptiness (mm)"] = "mm_elegy",
            ["oath to order"]       = "mm_oath",
            ["sun's song (mm)"]     = "mm_sun_song",

            // Souls — bosses (explicit mapping for reliability)
            ["soul of queen gohma"]   = "soul_of_queen_gohma",
            ["soul of king dodongo"]  = "soul_of_king_dodongo",
            ["soul of barinade"]      = "soul_of_barinade",
            ["soul of phantom ganon"] = "soul_of_phantom_ganon",
            ["soul of volvagia"]      = "soul_of_volvagia",
            ["soul of morpha"]        = "soul_of_morpha",
            ["soul of bongo bongo"]   = "soul_of_bongo_bongo",
            ["soul of twinrova"]      = "soul_of_twinrova",
            ["soul of odolwa"]        = "soul_of_odolwa",
            ["soul of goht"]          = "soul_of_goht",
            ["soul of gyorg"]         = "soul_of_gyorg",
            ["soul of twinmold"]      = "soul_of_twinmold",
            ["soul of igos"]          = "soul_of_igos",

            // Boss Keys
            ["boss key (forest temple)"]     = "forest_temple_bk",
            ["boss key (fire temple)"]       = "fire_temple_bk",
            ["boss key (water temple)"]      = "water_temple_bk",
            ["boss key (shadow temple)"]     = "shadow_temple_bk",
            ["boss key (spirit temple)"]     = "spirit_temple_bk",
            ["boss key (ganon's castle)"]    = "ganons_castle_bk",
            ["boss key (woodfall temple)"]   = "woodfall_bk",
            ["boss key (snowhead temple)"]   = "snowhead_bk",
            ["boss key (great bay temple)"]  = "great_bay_bk",
            ["boss key (stone tower temple)"]= "stone_tower_bk",

            // Small Keys — add 1 to counter
            ["small key (forest temple)"]     = "forest_temple_sk",
            ["small key (fire temple)"]       = "fire_temple_sk",
            ["small key (water temple)"]      = "water_temple_sk",
            ["small key (shadow temple)"]     = "shadow_temple_sk",
            ["small key (spirit temple)"]     = "spirit_temple_sk",
            ["small key (bottom of the well)"]= "botw_sk",
            ["small key (gerudo training grounds)"] = "gtg_sk",
            ["small key (ganon's castle)"]    = "ganons_castle_sk",
            ["small key (thieves' hideout)"]  = "thieves_hideout_sk",
            ["small key (chest game)"]        = "tcg_sk",
            ["small key (woodfall temple)"]   = "woodfall_sk",
            ["small key (snowhead temple)"]   = "snowhead_sk",
            ["small key (great bay temple)"]  = "great_bay_sk",
            ["small key (stone tower temple)"]= "stone_tower_sk",

            // Key Rings
            ["key ring (forest)"]    = "forest_temple_sk",
            ["key ring (fire)"]      = "fire_temple_sk",
            ["key ring (water)"]     = "water_temple_sk",
            ["key ring (shadow)"]    = "shadow_temple_sk",
            ["key ring (spirit)"]    = "spirit_temple_sk",
            ["key ring (well)"]      = "botw_sk",
            ["key ring (gtg)"]       = "gtg_sk",
            ["key ring (ganon)"]     = "ganons_castle_sk",
            ["key ring (hideout)"]   = "thieves_hideout_sk",
            ["key ring (chest game)"]= "tcg_sk",
            ["key ring (woodfall)"]  = "woodfall_sk",
            ["key ring (snowhead)"]  = "snowhead_sk",
            ["key ring (great bay)"] = "great_bay_sk",
            ["key ring (stone tower)"]= "stone_tower_sk",

            // Silver Rupee Pouches
            ["silver rupee pouch (shadow temple - scythe)"]         = "shadow_temple_sr_scythe",
            ["silver rupee pouch (shadow temple - large pits)"]     = "shadow_temple_sr_pit",
            ["silver rupee pouch (shadow temple - spikes)"]         = "shadow_temple_sr_spikes",
            ["silver rupee pouch (spirit temple - child)"]          = "spirit_temple_sr_child",
            ["silver rupee pouch (spirit temple - sun block)"]      = "spirit_temple_sr_sun",
            ["silver rupee pouch (spirit temple - boulders)"]       = "spirit_temple_sr_boulders",
            ["silver rupee pouch (ice cavern - scythe)"]            = "ice_cavern_sr_scythe",
            ["silver rupee pouch (ice cavern - block)"]             = "ice_cavern_sr_block",
            ["silver rupee pouch (bottom of the well)"]             = "botw_sr_basement",
            ["silver rupee pouch (gerudo training ground - slopes)"]= "gtg_sr_slopes",
            ["silver rupee pouch (gerudo training ground - lava)"]  = "gtg_sr_lava",
            ["silver rupee pouch (gerudo training ground - water)"] = "gtg_sr_water",
            ["silver rupee pouch (ganon's castle - spirit trial)"]  = "ganons_castle_sr_spirit",
            ["silver rupee pouch (ganon's castle - light trial)"]   = "ganons_castle_sr_light",
            ["silver rupee pouch (ganon's castle - fire trial)"]    = "ganons_castle_sr_fire",
            ["silver rupee pouch (ganon's castle - forest trial)"]  = "ganons_castle_sr_forest",

            // Tokens
            ["gold skulltula token"]  = "gold_skulltula_tokens",
            ["swamp skulltula token"] = "swamp_skulltula_tokens",
            ["ocean skulltula token"] = "ocean_skulltula_tokens",

            // Stray Fairies
            ["stray fairy (woodfall temple)"]   = "sf_woodfall",
            ["stray fairy (snowhead temple)"]   = "sf_snowhead",
            ["stray fairy (great bay temple)"]  = "sf_great_bay",
            ["stray fairy (stone tower temple)"]= "sf_stone_tower",
            ["stray fairy (clock town)"]        = "sf_clock_town",

            // Notes for OoT songs
            ["note from zelda's lullaby (oot)"] = "oot_zelda_lullaby",
            ["note from epona's song"]          = "oot_epona_song",
            ["note from saria's song (oot)"]    = "oot_saria_song",
            ["note from sun's song"]            = "oot_sun_song",
            ["note from song of time"]          = "oot_song_of_time",
            ["note from song of storms"]        = "oot_song_of_storms",
            ["note from minuet of forest (oot)"]= "oot_minuet",
            ["note from bolero of fire (oot)"]  = "oot_bolero",
            ["note from serenade of water (oot)"]= "oot_serenade",
            ["note from requiem of spirit (oot)"]= "oot_requiem",
            ["note from nocturne of shadow (oot)"]= "oot_nocturne",
            ["note from prelude of light (oot)"]= "oot_prelude",
            ["note from elegy of emptiness"]    = "oot_elegy",

            // Notes for MM songs
            ["note from song of time (mm)"]     = "mm_song_of_time",
            ["note from song of healing (mm)"]  = "mm_song_of_healing",
            ["note from epona's song (mm)"]     = "mm_epona_song",
            ["note from song of soaring (mm)"]  = "mm_song_of_soaring",
            ["note from song of storms (mm)"]   = "mm_song_of_storms",
            ["note from sonata of awakening (mm)"]= "mm_sonata",
            ["note from goron lullaby (mm)"]    = "mm_goron_lullaby",
            ["note from new wave bossa nova (mm)"]= "mm_new_wave",
            ["note from elegy of emptiness (mm)"]= "mm_elegy",
            ["note from oath to order (mm)"]    = "mm_oath",
            ["note from sun's song (mm)"]       = "mm_sun_song",

            // Shared items
            ["shared bow"]           = "sh_bow",
            ["shared fire arrows"]   = "sh_fire_arrows",
            ["shared ice arrows"]    = "sh_ice_arrows",
            ["shared light arrows"]  = "sh_light_arrows",
            ["shared bomb bag"]      = "sh_bomb_bag",
            ["shared magic upgrade"] = "sh_magic",
            ["shared hookshot"]      = "sh_hookshot",

            // Coins
            ["coin (red)"]    = "coin_red",
            ["coin (green)"]  = "coin_green",
            ["coin (blue)"]   = "coin_blue",
            ["coin (yellow)"] = "coin_yellow",

            // Triforce
            ["triforce piece"] = "triforce",
            ["triforce"]       = "triforce",

            // Hearts
            ["piece of heart"] = "oot_heart_piece",
            ["heart container"]= "oot_heart_container",
        };

        /// <summary>
        /// Applies starting items from log to tracker progress.
        /// </summary>
        public static void Apply(SpoilerLog log, Dictionary<string, int> progress)
        {
            foreach (var kv in log.StartingItems)
            {
                var name = kv.Key.Trim();
                int qty = int.TryParse(kv.Value, out var q) ? q : 1;

                if (Map.TryGetValue(name, out var trackerId))
                {
                    // Set quantity (minimum 1 for binary items)
                    progress[trackerId] = Math.Max(progress.GetValueOrDefault(trackerId), qty);
                }
                // Clocks with progression
                else if (name.StartsWith("Clock", StringComparison.OrdinalIgnoreCase))
                {
                    progress["clock_progressive"] = GetClockStep(name);
                }
                // Souls — search by Id via ToId (name → id)
                else if (name.StartsWith("Soul of", StringComparison.OrdinalIgnoreCase))
                {
                    var soulId = SoulNameToId(name);
                    if (!string.IsNullOrEmpty(soulId))
                        progress[soulId] = Math.Max(progress.GetValueOrDefault(soulId), 1);
                }
            }
        }

        // Converts soul name from log to tracker Id (similar to SoulsData.ToId)
        private static string SoulNameToId(string name) =>
            name.ToLower()
                .Replace(" ", "_").Replace("/", "_").Replace("'", "")
                .Replace(".", "").Replace("(", "").Replace(")", "")
                .Replace("&", "and").Replace(",", "").Replace("-", "_")
                .Trim('_');

        private static int GetClockStep(string name) => name.ToLower() switch
        {
            "clock (day 1)"   => 1,
            "clock (night 1)" => 2,
            "clock (day 2)"   => 3,
            "clock (night 2)" => 4,
            "clock (day 3)"   => 5,
            "clock (night 3)" => 6,
            _ => 0
        };
    }
}
