using System;
using System.Collections.Generic;
using System.Linq;
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
            ["deku shield"]        = "oot_deku_shield",
            ["deku shield (oot)"]  = "oot_deku_shield",
            ["hylian shield"]      = "oot_hylian_shield",
            ["mirror shield"]      = "oot_mirror_shield",

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
            ["deku sticks"]     = "oot_deku_stick",
            ["deku stick upgrade"] = "oot_deku_stick",
            ["deku stick upgrade (oot)"] = "oot_deku_stick",
            ["deku nut"]        = "oot_deku_nut",
            ["deku nuts"]       = "oot_deku_nut",
            ["deku nut upgrade"] = "oot_deku_nut",
            ["deku nut upgrade (oot)"] = "oot_deku_nut",
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
            ["deku sticks (mm)"] = "mm_deku_stick",
            ["deku stick upgrade (mm)"] = "mm_deku_stick",
            ["deku nut (mm)"]   = "mm_deku_nut",
            ["deku nuts (mm)"]  = "mm_deku_nut",
            ["deku nut upgrade (mm)"] = "mm_deku_nut",
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
            ["deku mask"]          = "deku_mask",
            ["goron mask"]         = "goron_mask",
            ["zora mask"]          = "zora_mask",
            ["fierce deity's mask"]= "fierce_deity",
            ["postman's hat"]      = "postman_hat",
            ["all-night mask"]     = "all_night_mask",
            ["blast mask"]         = "blast_mask",
            ["stone mask"]         = "stone_mask",
            ["great fairy's mask"] = "great_fairy_mask",
            ["keaton mask"]        = "keaton_mask",
            ["bremen mask"]        = "bremen_mask",
            ["bunny hood"]         = "bunny_hood",
            ["bunny hood (mm)"]    = "bunny_hood",
            ["don gero's mask"]    = "don_gero_mask",
            ["mask of scents"]     = "mask_of_scents",
            ["romani's mask"]      = "romani_mask",
            ["circus leader's mask"]= "circus_leader_mask",
            ["kafei's mask"]       = "kafei_mask",
            ["couple's mask"]      = "couples_mask",
            ["mask of truth"]      = "mask_of_truth",
            ["kamaro's mask"]      = "kamaro_mask",
            ["gibdo mask"]         = "gibdo_mask",
            ["garo's mask"]        = "garo_mask",
            ["captain's hat"]      = "captain_hat",
            ["giant's mask"]       = "giants_mask",

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
            ["song of storms"]  = "oot_song_of_storms",            ["minuet of forest"]= "oot_minuet",
            ["bolero of fire"]  = "oot_bolero",
            ["serenade of water"] = "oot_serenade",
            ["requiem of spirit"] = "oot_requiem",
            ["nocturne of shadow"] = "oot_nocturne",
            ["prelude of light"]= "oot_prelude",
            ["elegy of emptiness"] = "oot_elegy",

            // Ocarina variants
            ["ocarina (mm)"]             = "mm_ocarina",
            ["fairy ocarina (mm)"]       = "mm_ocarina",
            ["progressive ocarina (oot)"]= "oot_ocarina",
            ["progressive ocarina (mm)"] = "mm_ocarina",

            // Sword variants
            ["progressive sword (mm)"]   = "mm_sword",
            ["progressive sword (oot)"]  = "oot_sword",

            // Shield variants
            ["deku shield (oot)"]        = "oot_deku_shield",
            ["hylian/hero shield"]       = "oot_hylian_shield",

            // Song variants
            ["sun's song"]               = "oot_sun_song",
            ["sun\u2019s song"]          = "oot_sun_song",  // curly apostrophe variant
            ["saria's song"]             = "oot_saria_song",
            ["zelda's lullaby"]          = "oot_zelda_lullaby",
            ["epona's song"]             = "oot_epona_song",
            ["song of soaring"]          = "mm_song_of_soaring",

            // Souls — NPC/Animal/Misc with game suffix
            ["soul of bombchu bowling lady (oot)"]  = "soul_of_bombchu_bowling_lady_oot",
            ["soul of carpenters (mm)"]             = "soul_of_carpenters_mm",
            ["soul of carpenters (oot)"]            = "soul_of_carpenters_oot",
            ["soul of chest game lady (mm)"]        = "soul_of_chest_game_lady_mm",
            ["soul of composer bros. (oot)"]        = "soul_of_composer_bros_oot",
            ["soul of dampe (mm)"]                  = "soul_of_dampe_mm",
            ["soul of dog lady (mm)"]               = "soul_of_dog_lady_mm",
            ["soul of fishing pond owner (oot)"]    = "soul_of_fishing_pond_owner_oot",
            ["soul of flying pots (mm)"]            = "soul_of_flying_pots_mm",
            ["soul of flying pots (oot)"]           = "soul_of_flying_pots_oot",
            ["soul of ghost hut owner (mm)"]        = "soul_of_ghost_hut_owner_mm",
            ["soul of honey & darling (mm)"]        = "soul_of_honey_and_darling_mm",
            ["soul of honey & darling (oot)"]       = "soul_of_honey_and_darling_oot",
            ["soul of ingo (oot)"]                  = "soul_of_ingo_oot",
            ["soul of lulu (mm)"]                   = "soul_of_lulu_mm",
            ["soul of playground scrubs"]           = "soul_of_playground_scrubs",
            ["soul of poe collector (oot)"]         = "soul_of_poe_collector_oot",
            ["soul of swamp archery owner (mm)"]    = "soul_of_swamp_archery_owner_mm",
            ["soul of the astronomer (oot)"]        = "soul_of_the_astronomer_oot",
            ["soul of the beggar (oot)"]            = "soul_of_the_beggar_oot",
            ["soul of the zora musicians"]          = "soul_of_the_zora_musicians",
            ["soul of town archery owner (mm)"]     = "soul_of_town_archery_owner_mm",
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

            // Owl Statues
            ["owl statue (clock town)"]     = "owl_clock_town",
            ["owl statue (woodfall)"]       = "owl_woodfall",
            ["owl statue (mountain village)"]= "owl_mountain",
            ["owl statue (great bay)"]      = "owl_great_bay",
            ["owl statue (ikana canyon)"]   = "owl_ikana",
            ["owl statue (milk road)"]      = "owl_milk_road",
            ["owl statue (southern swamp)"] = "owl_swamp",
            ["owl statue (snowhead)"]       = "owl_snowhead",
            ["owl statue (zora cape)"]      = "owl_zora_cape",
            ["owl statue (stone tower)"]    = "owl_stone_tower",

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

            // Shared items — explicit OoT/MM/Shared variants
            ["shared bow"]               = "sh_bow",
            ["shared fire arrows"]       = "sh_fire_arrows",
            ["shared ice arrows"]        = "sh_ice_arrows",
            ["shared light arrows"]      = "sh_light_arrows",
            ["shared bomb bag"]          = "sh_bomb_bag",
            ["shared magic upgrade"]     = "sh_magic",
            ["shared large magic upgrade"] = "sh_magic",
            ["shared hookshot"]          = "sh_hookshot",
            ["shared hammer"]            = "sh_hammer",
            ["shared bottles"]           = "sh_bottle",
            ["shared bombchu"]           = "sh_bombchu",
            ["shared lens of truth"]     = "sh_lens",
            ["shared din's fire"]        = "sh_dins_fire",
            ["shared farore's wind"]     = "sh_farores_wind",
            ["shared nayru's love"]      = "sh_nayrus_love",
            ["shared stone of agony"]    = "sh_stone_agony",
            ["shared sticks & nuts"]     = "sh_nuts_sticks",
            ["shared great spin attack"] = "sh_great_spin",
            ["shared sword"]             = "sh_sword",
            ["shared shields"]           = "sh_shield",
            ["shared health"]            = "sh_health",
            ["shared wallets"]           = "sh_wallet",
            ["shared magic"]             = "sh_magic",
            ["shared goron tunic"]       = "sh_goron_tunic",
            ["shared zora tunic"]        = "sh_zora_tunic",
            ["shared iron boots"]        = "sh_iron_boots",
            ["shared hover boots"]       = "sh_hover_boots",
            ["shared strength"]          = "sh_strength",
            ["shared scales"]            = "sh_scale",
            ["shared ocarina"]           = "sh_ocarina",
            ["shared keaton mask"]       = "keaton_mask",
            ["shared bunny hood"]        = "bunny_hood",
            ["shared goron mask"]        = "goron_mask",
            ["shared zora mask"]         = "zora_mask",
            ["shared blast mask"]        = "blast_mask",
            ["shared stone mask"]        = "stone_mask",
            ["shared mask of truth"]     = "mask_of_truth",
            ["shared gold skulltula token"] = "gold_skulltula_tokens",
            ["shared platinum token"]    = "sh_platinum_token",
            ["shared skeleton key"]      = "sh_skeleton_key",

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

        // Bottle content → step index for OoT bottles
        private static readonly Dictionary<string, int> OotBottleContentStep = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ruto's letter"] = 1, ["letter"] = 1,
            ["milk"] = 2, ["lon lon milk"] = 2, ["half milk"] = 2,
            ["red potion"] = 3,
            ["green potion"] = 4,
            ["blue potion"] = 5,
            ["poe"] = 6,
            ["big poe"] = 7,
            ["blue fire"] = 8,
            ["fairy"] = 9,
            ["fish"] = 10,
            ["bugs"] = 11,
        };

        // Bottle content → step index for MM bottles
        private static readonly Dictionary<string, int> MmBottleContentStep = new(StringComparer.OrdinalIgnoreCase)
        {
            ["gold dust"] = 1,
            ["milk"] = 2, ["lon lon milk"] = 2, ["half milk"] = 2,
            ["chateau romani"] = 3,
            ["red potion"] = 4,
            ["green potion"] = 5,
            ["blue potion"] = 6,
            ["poe"] = 7,
            ["big poe"] = 8,
            ["fairy"] = 9,
            ["fish"] = 10,
            ["zora egg"] = 11,
            ["seahorse"] = 12,
            ["deku princess"] = 13,
            ["magic mushroom"] = 14,
            ["hot spring water"] = 15,
            ["spring water"] = 16,
        };

        // Bottle content → step index for Shared bottles
        private static readonly Dictionary<string, int> ShBottleContentStep = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ruto's letter"] = 1, ["letter"] = 1,
            ["gold dust"] = 2,
            ["milk"] = 3, ["lon lon milk"] = 3, ["half milk"] = 3,
            ["chateau romani"] = 4,
            ["red potion"] = 5,
            ["green potion"] = 6,
            ["blue potion"] = 7,
            ["poe"] = 8,
            ["big poe"] = 9,
            ["blue fire"] = 10,
            ["fairy"] = 11,
            ["fish"] = 12,
            ["bugs"] = 13,
            ["zora egg"] = 14,
            ["seahorse"] = 15,
            ["deku princess"] = 16,
            ["magic mushroom"] = 17,
            ["spring water"] = 18,
            ["hot spring water"] = 19,
        };

        /// <summary>
        /// Applies starting items from log to tracker progress.
        /// </summary>
        public static void Apply(SpoilerLog log, Dictionary<string, int> progress)
        {
            // Track next available bottle slot per game
            var ootBottleSlot = 1;
            var mmBottleSlot  = 1;
            var shBottleSlot  = 1;

            foreach (var kv in log.StartingItems)
            {
                // Normalize: replace curly apostrophes with straight ones
                var name = kv.Key.Trim()
                    .Replace('\u2019', '\'')
                    .Replace('\u2018', '\'');
                int qty = int.TryParse(kv.Value, out var q) ? q : 1;

                // Handle "10 Deku Nuts" style - strip numeric prefix, treat as consumable pack
                var numPrefixMatch = System.Text.RegularExpressions.Regex.Match(name, @"^(\d+)\s+(.+)$");
                int packSize = 1;
                var cleanName = name;
                if (numPrefixMatch.Success)
                {
                    packSize = int.Parse(numPrefixMatch.Groups[1].Value);
                    cleanName = numPrefixMatch.Groups[2].Value.Trim();
                }

                // Handle bottle contents from Starting Items
                var bottleContent = GetBottleContent(name);
                if (bottleContent != null)
                {
                    for (int i = 0; i < qty; i++)
                    {
                        bool isOot = name.EndsWith("(OoT)", StringComparison.OrdinalIgnoreCase) || bottleContent == "oot";
                        bool isMm  = name.EndsWith("(MM)",  StringComparison.OrdinalIgnoreCase) || bottleContent == "mm";

                        if (isMm && mmBottleSlot <= 6)
                        {
                            if (MmBottleContentStep.TryGetValue(bottleContent, out var mmStep))
                                progress[$"mm_bottle_{mmBottleSlot}"] = (mmStep - 1) * 10 + 1;
                            mmBottleSlot++;
                        }
                        else if (isOot && ootBottleSlot <= 4)
                        {
                            if (OotBottleContentStep.TryGetValue(bottleContent, out var ootStep))
                                progress[$"oot_bottle_{ootBottleSlot}"] = (ootStep - 1) * 10 + 1;
                            ootBottleSlot++;
                        }
                        else if (!isOot && !isMm && shBottleSlot <= 6)
                        {
                            if (ShBottleContentStep.TryGetValue(bottleContent, out var shStep))
                                progress[$"sh_bottle_{shBottleSlot}"] = (shStep - 1) * 10 + 1;
                            shBottleSlot++;
                        }
                    }
                    continue;
                }

                if (Map.TryGetValue(name, out var trackerId) || Map.TryGetValue(cleanName, out trackerId))
                {                    bool isProgressive = name.Contains("Progressive", StringComparison.OrdinalIgnoreCase)
                                      || name.Contains("Upgrade", StringComparison.OrdinalIgnoreCase);

                    if (trackerId is "oot_deku_stick" or "mm_deku_stick" or "sh_deku_stick")
                    {
                        foreach (var id in new[]{ "sh_deku_stick", "oot_deku_stick", "mm_deku_stick" })
                        {
                            int step = isProgressive ? (qty >= 3 ? 3 : qty >= 2 ? 2 : 1) : 1;
                            progress[id] = Math.Max(progress.GetValueOrDefault(id), step);
                        }
                    }
                    else if (trackerId is "oot_deku_nut" or "mm_deku_nut" or "sh_deku_nut")
                    {
                        foreach (var id in new[]{ "sh_deku_nut", "oot_deku_nut", "mm_deku_nut" })
                        {
                            int step = isProgressive ? (qty >= 3 ? 3 : qty >= 2 ? 2 : 1) : 1;
                            progress[id] = Math.Max(progress.GetValueOrDefault(id), step);
                        }
                    }
                    else
                    {
                        SetProgress(progress, trackerId, qty);
                    }
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

        // Sets progress trying primary ID, then shared/oot/mm fallbacks
        private static void SetProgress(Dictionary<string, int> progress, string trackerId, int qty)
        {
            // Candidates: primary, then prefix variants
            var candidates = new List<string> { trackerId };
            if (trackerId.StartsWith("oot_"))
                candidates.Add("sh_" + trackerId.Substring(4));
            else if (trackerId.StartsWith("mm_"))
                candidates.Add("sh_" + trackerId.Substring(3));
            else if (trackerId.StartsWith("sh_"))
            {
                candidates.Add("oot_" + trackerId.Substring(3));
                candidates.Add("mm_" + trackerId.Substring(3));
            }

            foreach (var id in candidates)
                progress[id] = Math.Max(progress.GetValueOrDefault(id), qty);
        }

        // Converts soul name from log to tracker Id (similar to SoulsData.ToId)
        private static string SoulNameToId(string name) =>
            name.ToLower()
                .Replace(" ", "_").Replace("/", "_").Replace("'", "")
                .Replace(".", "").Replace("(", "").Replace(")", "")
                .Replace("&", "and").Replace(",", "").Replace("-", "_")
                .Trim('_');

        // Public content name arrays for bottle assignment
        public static readonly string[] OotBottleContentNames = {
            "Ruto's Letter", "Milk", "Red Potion", "Green Potion", "Blue Potion",
            "Poe", "Big Poe", "Blue Fire", "Fairy", "Fish", "Bugs",
            "Empty"
        };
        public static readonly string[] MmBottleContentNames = {
            "Gold Dust", "Milk", "Chateau Romani", "Red Potion", "Green Potion", "Blue Potion",
            "Poe", "Big Poe", "Fairy", "Fish", "Zora Egg", "Seahorse", "Deku Princess",
            "Magic Mushroom", "Hot Spring Water", "Spring Water", "Empty"
        };
        public static readonly string[] ShBottleContentNames = {
            "Ruto's Letter", "Gold Dust", "Milk", "Chateau Romani",
            "Red Potion", "Green Potion", "Blue Potion",
            "Poe", "Big Poe", "Blue Fire", "Fairy", "Fish", "Bugs",
            "Zora Egg", "Seahorse", "Deku Princess", "Magic Mushroom",
            "Spring Water", "Hot Spring Water", "Empty"
        };

        public static string? GetBottleContentPublic(string itemName) => GetBottleContent(itemName);

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

        // Returns the content name if item is a bottle, null otherwise
        private static string? GetBottleContent(string name)
        {
            var lower = name.ToLower();
            // Strip game suffix
            var stripped = System.Text.RegularExpressions.Regex.Replace(lower, @"\s*\((oot|mm)\)\s*$", "").Trim();

            // "Bottle of X"
            if (stripped.StartsWith("bottle of "))
                return NormalizeBottleContent(stripped.Substring("bottle of ".Length).Trim());

            // "Bottled X"
            if (stripped.StartsWith("bottled "))
                return NormalizeBottleContent(stripped.Substring("bottled ".Length).Trim());

            // Ruto's Letter
            if (stripped is "ruto's letter")
                return "Ruto's Letter";

            // Empty Bottle
            if (stripped is "empty bottle")
                return "Empty";

            return null;
        }

        private static string? NormalizeBottleContent(string content) => content switch
        {
            "ruto's letter"    => "Ruto's Letter",
            "gold dust"        => "Gold Dust",
            "milk" or "lon lon milk" or "half milk" => "Milk",
            "chateau romani"   => "Chateau Romani",
            "red potion"       => "Red Potion",
            "green potion"     => "Green Potion",
            "blue potion"      => "Blue Potion",
            "big poe"          => "Big Poe",
            "poe"              => "Poe",
            "blue fire"        => "Blue Fire",
            "fairy"            => "Fairy",
            "fish"             => "Fish",
            "bugs"             => "Bugs",
            "zora egg"         => "Zora Egg",
            "seahorse"         => "Seahorse",
            "deku princess"    => "Deku Princess",
            "magic mushroom"   => "Magic Mushroom",
            "spring water"     => "Spring Water",
            "hot spring water" => "Hot Spring Water",
            "empty" or "empty bottle" => "Empty",
            _                  => null
        };
    }
}
