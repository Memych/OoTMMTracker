using System.Collections.Generic;
using OoTMMTracker.Models;

namespace OoTMMTracker.Services
{
    /// <summary>
    /// Soul data — each soul is a separate item (MaxCount=1).
    /// Blocks: Bosses, Enemy (OoT/MM/Shared), NPC (OoT/MM/Shared),
    ///        Animal (OoT/MM/Shared), Misc (OoT/MM/Shared).
    /// </summary>
    public static class SoulsData
    {
        private static TrackerItem S(string id, string name, string icon, string? label = null, int customSize = 0) =>
            new() { Id = id, Name = name, Type = TrackerItemType.Item, MaxCount = 1, IconPath = icon, StaticLabel = label, CustomSize = customSize };

        private static string ToId(string name) =>
            name.ToLower()
                .Replace(" ", "_").Replace("/", "_").Replace("'", "")
                .Replace(".", "").Replace("(", "").Replace(")", "")
                .Replace("&", "and").Replace(",", "").Replace("-", "_")
                .Replace("__", "_").Trim('_');

        // Extracts short label from name: "Soul of Barinade (OoT)" → "Barinade"
        private static string LabelOf(string name)
        {
            var s = name;
            // Remove suffix (OoT)/(MM)
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\s*\([^)]+\)\s*$", "").Trim();
            // Remove "Soul of the " and "Soul of "
            if (s.StartsWith("Soul of the ")) s = s.Substring("Soul of the ".Length);
            else if (s.StartsWith("Soul of "))  s = s.Substring("Soul of ".Length);
            return s;
        }

        // ─── Bosses ───────────────────────────────────────────────────────────────
        // One block: OoT bosses if SoulsBossOot, MM if SoulsBossMm
        // Order: Deku - Dodongo - Jabu - Forest - Fire - Water - Shadow - Spirit
        //          Woodfall - Snowhead - Great Bay - Stone Tower - Ikana

        private static readonly (string name, string label, bool isOot)[] BossEntries =
        {
            // OoT — dungeon order
            ("Soul of Queen Gohma",   "Queen Gohma",   true),
            ("Soul of King Dodongo",  "King Dodongo",  true),
            ("Soul of Barinade",      "Barinade",      true),
            ("Soul of Phantom Ganon", "Phantom Ganon", true),
            ("Soul of Volvagia",      "Volvagia",      true),
            ("Soul of Morpha",        "Morpha",        true),
            ("Soul of Bongo Bongo",   "Bongo Bongo",   true),
            ("Soul of Twinrova",      "Twinrova",      true),
            // MM — dungeon order
            ("Soul of Odolwa",        "Odolwa",        false),
            ("Soul of Goht",          "Goht",          false),
            ("Soul of Gyorg",         "Gyorg",         false),
            ("Soul of Twinmold",      "Twinmold",      false),
            ("Soul of Igos",          "Igos",          false),
        };

        public static List<TrackerItem> GetBossSouls(TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();
            foreach (var (name, label, isOot) in BossEntries)
            {
                if (isOot && !cfg.SoulsBossOot) continue;
                if (!isOot && !cfg.SoulsBossMm) continue;
                items.Add(S(ToId(name), name, "souls/boss.png", label));
            }
            return items;
        }

        // ─── Enemy OoT ────────────────────────────────────────────────────────────

        private static readonly string[] EnemyOot =
        {
            "Soul of Anubis (OoT)",
            "Soul of Armos (OoT)",
            "Soul of Baby\nDodongos (OoT)",
            "Soul of Beamos (OoT)",
            "Soul of Biris/Baris (OoT)",
            "Soul of Bubbles (OoT)",
            "Soul of Dark\nLink (OoT)",
            "Soul of Dead\nHands (OoT)",
            "Soul of Deku\nBabas (OoT)",
            "Soul of Deku\nScrubs (OoT)",
            "Soul of Dodongos (OoT)",
            "Soul of Flare\nDancers (OoT)",
            "Soul of Floormasters (OoT)",
            "Soul of Flying\nPots (OoT)",
            "Soul of Freezards (OoT)",
            "Soul of the Fighting\nGerudos (OoT)",
            "Soul of Gohma\nLarvaes (OoT)",
            "Soul of Guays (OoT)",
            "Soul of Iron\nKnuckles (OoT)",
            "Soul of Jabu-Jabu's\nParasites (OoT)",
            "Soul of Keese (OoT)",
            "Soul of Leevers (OoT)",
            "Soul of Like\nLikes (OoT)",
            "Soul of Lizalfos/Dinolfos (OoT)",
            "Soul of Moblins (OoT)",
            "Soul of Octoroks (OoT)",
            "Soul of Peahats (OoT)",
            "Soul of Poes (OoT)",
            "Soul of ReDeads/Gibdos (OoT)",
            "Soul of Shaboms (OoT)",
            "Soul of Shell\nBlades (OoT)",
            "Soul of Skull\nKids (OoT)",
            "Soul of Skulltulas (OoT)",
            "Soul of Skullwalltulas (OoT)",
            "Soul of Spikes (OoT)",
            "Soul of Stalchildren (OoT)",
            "Soul of Stalfos (OoT)",
            "Soul of Stingers (OoT)",
            "Soul of Tailpasarans (OoT)",
            "Soul of Tektites (OoT)",
            "Soul of Torch\nSlugs (OoT)",
            "Soul of Wallmasters (OoT)",
            "Soul of Wolfos (OoT)",
        };

        // ─── Enemy MM ─────────────────────────────────────────────────────────────
        // 49 enemies, alphabetical, suffix (MM)

        private static readonly string[] EnemyMm =
        {
            "Soul of Armos (MM)",
            "Soul of Bad\nBats (MM)",
            "Soul of Beamos (MM)",
            "Soul of Bio\nBabas (MM)",
            "Soul of Boes (MM)",
            "Soul of Bubbles (MM)",
            "Soul of Captain\nKeeta (MM)",
            "Soul of Chuchus (MM)",
            "Soul of Deep\nPythons (MM)",
            "Soul of Deku\nBabas (MM)",
            "Soul of Deku\nScrubs (MM)",
            "Soul of Dexihands (MM)",
            "Soul of Dodongos (MM)",
            "Soul of Dragonflies (MM)",
            "Soul of Eenoes (MM)",
            "Soul of Eyegores (MM)",
            "Soul of the Fighting\nPirates (MM)",
            "Soul of Floormasters (MM)",
            "Soul of Flying\nPots (MM)",
            "Soul of Freezards (MM)",
            "Soul of Garo (MM)",
            "Soul of Gekkos (MM)",
            "Soul of Gomess (MM)",
            "Soul of Guays (MM)",
            "Soul of Hiploops (MM)",
            "Soul of Iron\nKnuckles (MM)",
            "Soul of Keese (MM)",
            "Soul of Leevers (MM)",
            "Soul of Like\nLikes (MM)",
            "Soul of Lizalfos/Dinolfos (MM)",
            "Soul of Nejirons (MM)",
            "Soul of Octoroks (MM)",
            "Soul of Peahats (MM)",
            "Soul of Poes (MM)",
            "Soul of Real\nBombchu (MM)",
            "Soul of ReDeads/Gibdos (MM)",
            "Soul of Shell\nBlades (MM)",
            "Soul of Skullfish (MM)",
            "Soul of Skulltulas (MM)",
            "Soul of Skullwalltulas (MM)",
            "Soul of Snappers (MM)",
            "Soul of Stalchildren (MM)",
            "Soul of Takkuri (MM)",
            "Soul of Tektites (MM)",
            "Soul of Wallmasters (MM)",
            "Soul of Warts (MM)",
            "Soul of Wizzrobes (MM)",
            "Soul of Wolfos (MM)",
        };

        // ─── Enemy Shared ─────────────────────────────────────────────────────────

        private static readonly string[] EnemyShared =
        {
            "Soul of Anubis",
            "Soul of Armos",
            "Soul of Baby\nDodongos",
            "Soul of Bad\nBats",
            "Soul of Beamos",
            "Soul of Bio\nBabas",
            "Soul of Biris/Baris",
            "Soul of Boes",
            "Soul of Bubbles",
            "Soul of Captain\nKeeta",
            "Soul of Chuchus",
            "Soul of Dark\nLink",
            "Soul of Dead\nHands",
            "Soul of Deep\nPythons",
            "Soul of Deku\nBabas",
            "Soul of Deku\nScrubs",
            "Soul of Dexihands",
            "Soul of Dodongos",
            "Soul of Dragonflies",
            "Soul of Eenoes",
            "Soul of Eyegores",
            "Soul of Fighting\nThieves",
            "Soul of Flare\nDancers",
            "Soul of Floormasters",
            "Soul of Flying\nPots",
            "Soul of Freezards",
            "Soul of Garo",
            "Soul of Gekkos",
            "Soul of Gohma\nLarvaes",
            "Soul of Gomess",
            "Soul of Guays",
            "Soul of Hiploops",
            "Soul of Iron\nKnuckles",
            "Soul of Jabu-Jabu's\nParasites",
            "Soul of Keese",
            "Soul of Leevers",
            "Soul of Like\nLikes",
            "Soul of Lizalfos/Dinolfos",
            "Soul of Moblins",
            "Soul of Nejirons",
            "Soul of Octoroks",
            "Soul of Peahats",
            "Soul of Poes",
            "Soul of Real\nBombchu",
            "Soul of ReDeads/Gibdos",
            "Soul of Shaboms",
            "Soul of Shell\nBlades",
            "Soul of Skull\nKids",
            "Soul of Skullfish",
            "Soul of Skulltulas",
            "Soul of Skullwalltulas",
            "Soul of Snappers",
            "Soul of Spikes",
            "Soul of Stalchildren",
            "Soul of Stalfos",
            "Soul of Stingers",
            "Soul of Tailpasarans",
            "Soul of Takkuri",
            "Soul of Tektites",
            "Soul of Torch\nSlugs",
            "Soul of Wallmasters",
            "Soul of Warts",
            "Soul of Wizzrobes",
            "Soul of Wolfos",
        };

        public static List<TrackerItem> GetEnemySoulsOot(TrackerConfig cfg)
        {
            if (!cfg.SoulsEnemyOot || cfg.SharedSoulsEnemy) return new();
            var items = new List<TrackerItem>();
            foreach (var name in EnemyOot)
                items.Add(S(ToId(name), name, "souls/enemy.png", LabelOf(name)));
            return items;
        }

        public static List<TrackerItem> GetEnemySoulsMm(TrackerConfig cfg)
        {
            if (!cfg.SoulsEnemyMm || cfg.SharedSoulsEnemy) return new();
            var items = new List<TrackerItem>();
            foreach (var name in EnemyMm)
                items.Add(S(ToId(name), name, "souls/enemy.png", LabelOf(name)));
            return items;
        }

        public static List<TrackerItem> GetEnemySoulsShared(TrackerConfig cfg)
        {
            if (!cfg.SharedSoulsEnemy) return new();
            var items = new List<TrackerItem>();
            foreach (var name in EnemyShared)
                items.Add(S(ToId(name), name, "souls/enemy.png", LabelOf(name)));
            return items;
        }

        // ─── NPC OoT ──────────────────────────────────────────────────────────────

        private static readonly string[] NpcOot =
        {
            "Soul of the Astronomer (OoT)",
            "Soul of Bazaar\nShopkeeper (OoT)",
            "Soul of the Bean\nSalesman (OoT)",
            "Soul of the Beggar (OoT)",
            "Soul of Biggoron (OoT)",
            "Soul of Bombchu\nShopkeeper (OoT)",
            "Soul of Bombchu\nBowling Lady (OoT)",
            "Soul of Carpenters (OoT)",
            "Soul of the Carpet\nMan (OoT)",
            "Soul of Chest Game\nOwner (OoT)",
            "Soul of Goron\nChild (OoT)",
            "Soul of Composer Bros. (OoT)",
            "Soul of Cucco\nLady (OoT)",
            "Soul of Dampe (OoT)",
            "Soul of Darunia (OoT)",
            "Soul of the Dog\nLady (OoT)",
            "Soul of Fishing Pond\nOwner (OoT)",
            "Soul of the Patrolling\nGerudos (OoT)",
            "Soul of Goron (OoT)",
            "Soul of Goron\nShopkeeper (OoT)",
            "Soul of Graveyard\nKid (OoT)",
            "Soul of Guru-Guru (OoT)",
            "Soul of the Citizens (OoT)",
            "Soul of Hylian\nGuard (OoT)",
            "Soul of Ingo (OoT)",
            "Soul of King\nZora (OoT)",
            "Soul of Kokiri (OoT)",
            "Soul of Kokiri\nShopkeeper (OoT)",
            "Soul of the Scientist (OoT)",
            "Soul of Honey & Darling (OoT)",
            "Soul of Malon (OoT)",
            "Soul of Medigoron (OoT)",
            "Soul of Mido (OoT)",
            "Soul of the Old\nHag (OoT)",
            "Soul of Poe\nCollector (OoT)",
            "Soul of Potion\nShopkeeper (OoT)",
            "Soul of the Punk\nKid (OoT)",
            "Soul of the Rooftop\nMan (OoT)",
            "Soul of Ruto (OoT)",
            "Soul of Saria (OoT)",
            "Soul of Sheik (OoT)",
            "Soul of Shooting Gallery\nOwner (OoT)",
            "Soul of Talon (OoT)",
            "Soul of Zelda (OoT)",
            "Soul of Zora (OoT)",
            "Soul of Zora\nShopkeeper (OoT)",
        };

        // ─── NPC MM ───────────────────────────────────────────────────────────────

        private static readonly string[] NpcMm =
        {
            "Soul of Anju (MM)",
            "Soul of Anju's\nGrandmother (MM)",
            "Soul of Astronomer (MM)",
            "Soul of the Banker (MM)",
            "Soul of the Beans\nSalesman (MM)",
            "Soul of Mr. Barten (MM)",
            "Soul of Biggoron (MM)",
            "Soul of Blacksmiths (MM)",
            "Soul of the Bomb Shop\nOwner (MM)",
            "Soul of Bombers (MM)",
            "Soul of Carpenters (MM)",
            "Soul of Chest Game\nLady (MM)",
            "Soul of Citizens (MM)",
            "Soul of Composer Bros. (MM)",
            "Soul of Dampe (MM)",
            "Soul of the Deku\nButler (MM)",
            "Soul of the Deku\nKing (MM)",
            "Soul of the Deku\nPrincess (MM)",
            "Soul of Dog\nLady (MM)",
            "Soul of the Fisherman (MM)",
            "Soul of Ghost Hut\nOwner (MM)",
            "Soul of Gorman & Bros. (MM)",
            "Soul of Gorons (MM)",
            "Soul of the Goron\nBaby (MM)",
            "Soul of the Goron\nElder (MM)",
            "Soul of the Goron\nShopkeeper (MM)",
            "Soul of Grog (MM)",
            "Soul of Guru-Guru (MM)",
            "Soul of Honey & Darling (MM)",
            "Soul of Kafei (MM)",
            "Soul of Keaton (MM)",
            "Soul of Keg Trial\nGoron (MM)",
            "Soul of Koume & Kotake (MM)",
            "Soul of Lulu (MM)",
            "Soul of Madame\nAroma (MM)",
            "Soul of Mayor\nDotour (MM)",
            "Soul of Moon\nChildren (MM)",
            "Soul of Part-Timer (MM)",
            "Soul of the Patrolling Pirates\nand their Chief (MM)",
            "Soul of Playground\nScrubs (MM)",
            "Soul of Romani/Cremia (MM)",
            "Soul of the Scientist (MM)",
            "Soul of Swamp Archery\nOwner (MM)",
            "Soul of Swordsman (MM)",
            "Soul of Tingle (MM)",
            "Soul of Toilet\nHand (MM)",
            "Soul of Tourist Center\nOwner (MM)",
            "Soul of Toto (MM)",
            "Soul of Town Archery\nOwner (MM)",
            "Soul of Trading Post\nOwner (MM)",
            "Soul of Zoras (MM)",
            "Soul of the Zora\nMusicians (MM)",
            "Soul of the Zora\nShopkeeper (MM)",
        };

        // ─── NPC Shared ───────────────────────────────────────────────────────────

        private static readonly string[] NpcShared =
        {
            "Soul of Astronomer",
            "Soul of the Bazaar/Swamp\nArchery Owner",
            "Soul of Bean\nSalesman",
            "Soul of the Beggar/Banker",
            "Soul of Biggoron",
            "Soul of Blacksmiths",
            "Soul of Tourist Center\nOwner",
            "Soul of the Bombchu/Bomb\nShop Owner",
            "Soul of Bombchu Bowling/\nChest Game Lady",
            "Soul of Carpenters",
            "Soul of the Carpet\nMan/Swordsman",
            "Soul of the Chest Game\nOwner/Fisherman",
            "Soul of the Goron\nChild/Baby",
            "Soul of Composer Bros.",
            "Soul of Cucco\nLady/Anju",
            "Soul of Dampe",
            "Soul of Darunia",
            "Soul of Deku\nButler",
            "Soul of Deku\nKing",
            "Soul of Deku\nPrincess",
            "Soul of Dog\nLady",
            "Soul of Fishing Pond/\nTrading Post Owner",
            "Soul of the Patrolling Thieves\nand their Chief",
            "Soul of Goron\nElder",
            "Soul of Gorons",
            "Soul of Goron\nShopkeeper",
            "Soul of the Graveyard\nKid/Bombers",
            "Soul of Guru-Guru",
            "Soul of Citizens",
            "Soul of Hylian\nGuard",
            "Soul of Ingo/Gorman & Bros.",
            "Soul of Kafei",
            "Soul of Keaton",
            "Soul of King\nZora",
            "Soul of Kokiri",
            "Soul of Kokiri\nShopkeeper",
            "Soul of Koume & Kotake",
            "Soul of the Scientist",
            "Soul of Honey & Darling",
            "Soul of Madame\nAroma",
            "Soul of Malon/Romani/Cremia",
            "Soul of Mayor\nDotour",
            "Soul of Medigoron/Keg Trial\nGoron",
            "Soul of Mido",
            "Soul of Moon\nChildren",
            "Soul of the Old Hag/Anju's\nGrandmother",
            "Soul of Playground\nScrubs",
            "Soul of Poe Collector/\nGhost Hut Owner",
            "Soul of Potion\nShopkeeper",
            "Soul of the Punk Kid/\nGrog",
            "Soul of Rooftop Man/\nPart-Timer",
            "Soul of Ruto/Lulu",
            "Soul of Saria",
            "Soul of Sheik",
            "Soul of the Shooting Gallery/\nTown Archery Owner",
            "Soul of Talon/\nMr. Barten",
            "Soul of Tingle",
            "Soul of Toilet\nHand",
            "Soul of Toto",
            "Soul of Zelda",
            "Soul of the Zora\nMusicians",
            "Soul of Zoras",
            "Soul of the Zora\nShopkeeper",
        };

        public static List<TrackerItem> GetNpcSoulsOot(TrackerConfig cfg)
        {
            if (!cfg.SoulsNpcOot || cfg.SharedSoulsNpc) return new();
            var items = new List<TrackerItem>();
            foreach (var name in NpcOot)
                items.Add(S(ToId(name), name, "souls/npc.png", LabelOf(name)));
            return items;
        }

        public static List<TrackerItem> GetNpcSoulsMm(TrackerConfig cfg)
        {
            if (!cfg.SoulsNpcMm || cfg.SharedSoulsNpc) return new();
            var items = new List<TrackerItem>();
            foreach (var name in NpcMm)
                items.Add(S(ToId(name), name, "souls/npc.png", LabelOf(name)));
            return items;
        }

        public static List<TrackerItem> GetNpcSoulsShared(TrackerConfig cfg)
        {
            if (!cfg.SharedSoulsNpc) return new();
            var items = new List<TrackerItem>();
            foreach (var name in NpcShared)
                items.Add(S(ToId(name), name, "souls/npc.png", LabelOf(name)));
            return items;
        }

        // ─── Animal ───────────────────────────────────────────────────────────────

        private static readonly string[] AnimalOot =
            { "Soul of Butterflies (OoT)", "Soul of Cows (OoT)", "Soul of Cuccos (OoT)", "Soul of Dogs (OoT)" };
        private static readonly string[] AnimalMm =
            { "Soul of Butterflies (MM)", "Soul of Cows (MM)", "Soul of Cuccos (MM)", "Soul of Dogs (MM)" };
        private static readonly string[] AnimalShared =
            { "Soul of Butterflies", "Soul of Cows", "Soul of Cuccos", "Soul of Dogs" };

        public static List<TrackerItem> GetAnimalSoulsOot(TrackerConfig cfg)
        {
            if (!cfg.SoulsAnimalOot || cfg.SharedSoulsAnimal) return new();
            var items = new List<TrackerItem>();
            foreach (var name in AnimalOot)
                items.Add(S(ToId(name), name, "souls/animal.png", LabelOf(name)));
            return items;
        }

        public static List<TrackerItem> GetAnimalSoulsMm(TrackerConfig cfg)
        {
            if (!cfg.SoulsAnimalMm || cfg.SharedSoulsAnimal) return new();
            var items = new List<TrackerItem>();
            foreach (var name in AnimalMm)
                items.Add(S(ToId(name), name, "souls/animal.png", LabelOf(name)));
            return items;
        }

        public static List<TrackerItem> GetAnimalSoulsShared(TrackerConfig cfg)
        {
            if (!cfg.SharedSoulsAnimal) return new();
            var items = new List<TrackerItem>();
            foreach (var name in AnimalShared)
                items.Add(S(ToId(name), name, "souls/animal.png", LabelOf(name)));
            return items;
        }

        // ─── Misc ─────────────────────────────────────────────────────────────────

        private static readonly string[] MiscOot =
            { "Soul of Business\nScrubs (OoT)", "Soul of Gold\nSkulltulas (OoT)" };
        private static readonly string[] MiscMm =
            { "Soul of Business\nScrubs (MM)", "Soul of Gold\nSkulltulas (MM)" };
        private static readonly string[] MiscShared =
            { "Soul of Business\nScrubs", "Soul of Gold\nSkulltulas" };

        public static List<TrackerItem> GetMiscSoulsOot(TrackerConfig cfg)
        {
            if (!cfg.SoulsMiscOot || cfg.SharedSoulsMisc) return new();
            var items = new List<TrackerItem>();
            foreach (var name in MiscOot)
                items.Add(S(ToId(name), name, "souls/misc.png", LabelOf(name)));
            return items;
        }

        public static List<TrackerItem> GetMiscSoulsMm(TrackerConfig cfg)
        {
            if (!cfg.SoulsMiscMm || cfg.SharedSoulsMisc) return new();
            var items = new List<TrackerItem>();
            foreach (var name in MiscMm)
                items.Add(S(ToId(name), name, "souls/misc.png", LabelOf(name)));
            return items;
        }

        public static List<TrackerItem> GetMiscSoulsShared(TrackerConfig cfg)
        {
            if (!cfg.SharedSoulsMisc) return new();
            var items = new List<TrackerItem>();
            foreach (var name in MiscShared)
                items.Add(S(ToId(name), name, "souls/misc.png", LabelOf(name)));
            return items;
        }
    }
}
