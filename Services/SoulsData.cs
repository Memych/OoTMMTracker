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
                .Trim('_');

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
        // 44 enemies, alphabetical, suffix (OoT)

        private static readonly string[] EnemyOot =
        {
            "Soul of Anubis (OoT)",
            "Soul of Armos (OoT)",
            "Soul of Baby Dodongos (OoT)",
            "Soul of Beamos (OoT)",
            "Soul of Biris/Baris (OoT)",
            "Soul of Bubbles (OoT)",
            "Soul of Dark Link (OoT)",
            "Soul of Dead Hands (OoT)",
            "Soul of Deku Babas (OoT)",
            "Soul of Deku Scrubs (OoT)",
            "Soul of Dodongos (OoT)",
            "Soul of Flare Dancers (OoT)",
            "Soul of Floormasters (OoT)",
            "Soul of Flying Pots (OoT)",
            "Soul of Freezards (OoT)",
            "Soul of Gerudo Fighters (OoT)",
            "Soul of Gohma Larvae (OoT)",
            "Soul of Guays (OoT)",
            "Soul of Iron Knuckles (OoT)",
            "Soul of Jabu-Jabu's Parasites (OoT)",
            "Soul of Keese (OoT)",
            "Soul of Leevers (OoT)",
            "Soul of Like Likes (OoT)",
            "Soul of Lizalfos/Dinolfos (OoT)",
            "Soul of Moblins (OoT)",
            "Soul of Octoroks (OoT)",
            "Soul of Peahats (OoT)",
            "Soul of Poes (OoT)",
            "Soul of ReDeads/Gibdos (OoT)",
            "Soul of Shaboms (OoT)",
            "Soul of Shell Blades (OoT)",
            "Soul of Skull Kids (OoT)",
            "Soul of Skulltulas (OoT)",
            "Soul of Skullwalltulas (OoT)",
            "Soul of Spikes (OoT)",
            "Soul of Stalchildren (OoT)",
            "Soul of Stalfos (OoT)",
            "Soul of Stingers (OoT)",
            "Soul of Tailpasarans (OoT)",
            "Soul of Tektites (OoT)",
            "Soul of Torch Slugs (OoT)",
            "Soul of Wallmasters (OoT)",
            "Soul of Wolfos (OoT)",
        };

        // ─── Enemy MM ─────────────────────────────────────────────────────────────
        // 49 enemies, alphabetical, suffix (MM)

        private static readonly string[] EnemyMm =
        {
            "Soul of Armos (MM)",
            "Soul of Bad Bats (MM)",
            "Soul of Beamos (MM)",
            "Soul of Bio Babas (MM)",
            "Soul of Boes (MM)",
            "Soul of Bubbles (MM)",
            "Soul of Captain Keeta (MM)",
            "Soul of Chuchus (MM)",
            "Soul of Deep Pythons (MM)",
            "Soul of Deku Babas (MM)",
            "Soul of Deku Scrubs (MM)",
            "Soul of Dexihands (MM)",
            "Soul of Dodongos (MM)",
            "Soul of Dragonflies (MM)",
            "Soul of Eenoes (MM)",
            "Soul of Eyegores (MM)",
            "Soul of Floormasters (MM)",
            "Soul of Flying Pots (MM)",
            "Soul of Freezards (MM)",
            "Soul of Garo (MM)",
            "Soul of Gekkos (MM)",
            "Soul of Gerudo Pirate Fighters (MM)",
            "Soul of Gomess (MM)",
            "Soul of Guays (MM)",
            "Soul of Hiploops (MM)",
            "Soul of Iron Knuckles (MM)",
            "Soul of Keese (MM)",
            "Soul of Leevers (MM)",
            "Soul of Like Likes (MM)",
            "Soul of Lizalfos/Dinolfos (MM)",
            "Soul of Nejirons (MM)",
            "Soul of Octoroks (MM)",
            "Soul of Peahats (MM)",
            "Soul of Poes (MM)",
            "Soul of Real Bombchu (MM)",
            "Soul of ReDeads/Gibdos (MM)",
            "Soul of Shell Blades (MM)",
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
        // Combined (without suffixes) — from shared log

        private static readonly string[] EnemyShared =
        {
            "Soul of Anubis",
            "Soul of Armos",
            "Soul of Baby Dodongos",
            "Soul of Bad Bats",
            "Soul of Beamos",
            "Soul of Bio Babas",
            "Soul of Biris/Baris",
            "Soul of Boes",
            "Soul of Bubbles",
            "Soul of Captain Keeta",
            "Soul of Chuchus",
            "Soul of Dark Link",
            "Soul of Dead Hands",
            "Soul of Deep Pythons",
            "Soul of Deku Babas",
            "Soul of Deku Scrubs",
            "Soul of Dexihands",
            "Soul of Dodongos",
            "Soul of Dragonflies",
            "Soul of Eenoes",
            "Soul of Eyegores",
            "Soul of Flare Dancers",
            "Soul of Floormasters",
            "Soul of Flying Pots",
            "Soul of Freezards",
            "Soul of Garo",
            "Soul of Gekkos",
            "Soul of Fighting Thieves",
            "Soul of Gohma Larvae",
            "Soul of Gomess",
            "Soul of Guays",
            "Soul of Hiploops",
            "Soul of Iron Knuckles",
            "Soul of Jabu-Jabu's Parasites",
            "Soul of Keese",
            "Soul of Leevers",
            "Soul of Like Likes",
            "Soul of Lizalfos/Dinolfos",
            "Soul of Moblins",
            "Soul of Nejirons",
            "Soul of Octoroks",
            "Soul of Peahats",
            "Soul of Poes",
            "Soul of Real Bombchu",
            "Soul of ReDeads/Gibdos",
            "Soul of Shaboms",
            "Soul of Shell Blades",
            "Soul of Skullfish",
            "Soul of Skull Kids",
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
            "Soul of Torch Slugs",
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
        // 47 NPCs, alphabetical, suffix (OoT)

        private static readonly string[] NpcOot =
        {
            "Soul of Astronomer (OoT)",
            "Soul of Bazaar Owner (OoT)",
            "Soul of Bean Salesman (OoT)",
            "Soul of Beggar (OoT)",
            "Soul of Biggoron (OoT)",
            "Soul of Bombchu Bowling Lady (OoT)",
            "Soul of Bombchu Shopkeeper (OoT)",
            "Soul of Carpenters (OoT)",
            "Soul of Carpet Man (OoT)",
            "Soul of Chest Game Owner (OoT)",
            "Soul of Child Goron (OoT)",
            "Soul of Composer Bros. (OoT)",
            "Soul of Cucco Lady (OoT)",
            "Soul of Dampe (OoT)",
            "Soul of Darunia (OoT)",
            "Soul of Dog Lady (OoT)",
            "Soul of Fishing Pond Owner (OoT)",
            "Soul of Gerudo Guards (OoT)",
            "Soul of Gorons (OoT)",
            "Soul of Goron Shopkeeper (OoT)",
            "Soul of Graveyard Kids (OoT)",
            "Soul of Guru-Guru (OoT)",
            "Soul of Honey & Darling (OoT)",
            "Soul of Hylian Citizens (OoT)",
            "Soul of Hylian Guards (OoT)",
            "Soul of Ingo (OoT)",
            "Soul of King Zora (OoT)",
            "Soul of Kokiri (OoT)",
            "Soul of Kokiri Shopkeeper (OoT)",
            "Soul of Lakeside Scientist (OoT)",
            "Soul of Malon (OoT)",
            "Soul of Medigoron (OoT)",
            "Soul of Mido (OoT)",
            "Soul of Old Hag (OoT)",
            "Soul of Poe Collector (OoT)",
            "Soul of Potion Shopkeeper (OoT)",
            "Soul of Punk Boy (OoT)",
            "Soul of Roof Man (OoT)",
            "Soul of Ruto (OoT)",
            "Soul of Saria (OoT)",
            "Soul of Sheik (OoT)",
            "Soul of Shooting Gallery Owner (OoT)",
            "Soul of Talon (OoT)",
            "Soul of the Astronomer (OoT)",
            "Soul of the Beggar (OoT)",
            "Soul of Zelda (OoT)",
            "Soul of Zoras (OoT)",
            "Soul of Zora Shopkeeper (OoT)",
        };

        // ─── NPC MM ───────────────────────────────────────────────────────────────
        // 54 NPCs, alphabetical, suffix (MM)

        private static readonly string[] NpcMm =
        {
            "Soul of Anju (MM)",
            "Soul of Anju's Grandma (MM)",
            "Soul of Astronomer (MM)",
            "Soul of Baby Goron (MM)",
            "Soul of Banker (MM)",
            "Soul of Bean Salesman (MM)",
            "Soul of Biggoron (MM)",
            "Soul of Blacksmiths (MM)",
            "Soul of Boat Cruise Man (MM)",
            "Soul of Bombs Shopkeeper (MM)",
            "Soul of Bombers Kids (MM)",
            "Soul of Carpenters (MM)",
            "Soul of Chest Game Lady (MM)",
            "Soul of Clock Town Citizens (MM)",
            "Soul of Composer Bros. (MM)",
            "Soul of Dampe (MM)",
            "Soul of Deku Butler (MM)",
            "Soul of Deku King (MM)",
            "Soul of Deku Princess (MM)",
            "Soul of Dog Lady (MM)",
            "Soul of Fisherman (MM)",
            "Soul of Ghost Hut Owner (MM)",
            "Soul of Goron Elder (MM)",
            "Soul of Gorons (MM)",
            "Soul of Goron Shopkeeper (MM)",
            "Soul of Gorman Bros. (MM)",
            "Soul of Grog (MM)",
            "Soul of Guru-Guru (MM)",
            "Soul of Honey & Darling (MM)",
            "Soul of Kafei (MM)",
            "Soul of Keaton (MM)",
            "Soul of Koume and Kotake (MM)",
            "Soul of Lulu (MM)",
            "Soul of Madame Aroma (MM)",
            "Soul of Marine Scientist (MM)",
            "Soul of Mayor Dotour (MM)",
            "Soul of Medigoron (MM)",
            "Soul of Moon Children (MM)",
            "Soul of Mr. Barten (MM)",
            "Soul of Part-Time Shopkeeper (MM)",
            "Soul of Pirate Guards (MM)",
            "Soul of Playground Scrubs",
            "Soul of Romani/Cremia (MM)",
            "Soul of Swamp Archery Owner (MM)",
            "Soul of Swordsman (MM)",
            "Soul of Tingle (MM)",
            "Soul of Toilet Hand (MM)",
            "Soul of Toto (MM)",
            "Soul of Town Archery Owner (MM)",
            "Soul of Trading Post Owner (MM)",
            "Soul of the Zora Musicians",
            "Soul of Zoras (MM)",
            "Soul of Zora Shopkeeper (MM)",
        };

        // ─── NPC Shared ───────────────────────────────────────────────────────────
        // Combined (without suffixes)

        private static readonly string[] NpcShared =
        {
            "Soul of Anju/Cucco Lady",
            "Soul of Anju's Grandma/Old Hag",
            "Soul of Astronomer",
            "Soul of Baby Goron/Child Goron",
            "Soul of Banker/Beggar",
            "Soul of Bean Salesman",
            "Soul of Biggoron",
            "Soul of Blacksmiths",
            "Soul of Boat Cruise Man",
            "Soul of Bombs Shopkeeper/Bombchu Shopkeeper",
            "Soul of Bombers Kids/Graveyard Kids",
            "Soul of Bombchu Bowling/Chest Game Lady",
            "Soul of Carpenters",
            "Soul of Carpet Man/Swordsman",
            "Soul of Clock Town Citizens/Hylian Citizens",
            "Soul of Composer Bros.",
            "Soul of Dampe",
            "Soul of Darunia",
            "Soul of Deku Butler",
            "Soul of Deku King",
            "Soul of Deku Princess",
            "Soul of Dog Lady",
            "Soul of Fisherman/Fishpond Owner",
            "Soul of Gerudo Guards/Pirate Guards",
            "Soul of Goron Elder",
            "Soul of Gorons",
            "Soul of Goron Shopkeeper",
            "Soul of Gorman Bros./Ingo",
            "Soul of Grog/Punk Boy",
            "Soul of Guru-Guru",
            "Soul of Honey & Darling/Lovers",
            "Soul of Hylian Guards",
            "Soul of Kafei",
            "Soul of Keaton",
            "Soul of King Zora",
            "Soul of Kokiri",
            "Soul of Kokiri Shopkeeper",
            "Soul of Koume and Kotake",
            "Soul of Lakeside Scientist/Marine Scientist",
            "Soul of Lulu/Ruto",
            "Soul of Madame Aroma",
            "Soul of Malon/Romani/Cremia",
            "Soul of Mayor Dotour",
            "Soul of Medigoron",
            "Soul of Mido",
            "Soul of Moon Children",
            "Soul of Mr. Barten/Talon",
            "Soul of Part-Time Shopkeeper/Roof Man",
            "Soul of Playground Scrubs",
            "Soul of Poe Collector/Ghost Hut Owner",
            "Soul of Potion Shopkeeper",
            "Soul of Saria",
            "Soul of Sheik",
            "Soul of Shooting Gallery/Town Archery Owner",
            "Soul of Swamp Archery Owner/Bazaar Owner",
            "Soul of Tingle",
            "Soul of Toilet Hand",
            "Soul of Toto",
            "Soul of Trading Post Owner",
            "Soul of Zelda",
            "Soul of the Zora Musicians",
            "Soul of Zoras",
            "Soul of Zora Shopkeeper",
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
            { "Soul of Butterflies (MM)", "Soul of Cows (MM)", "Soul of Cuccos (MM)" };
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
            { "Soul of Business Scrubs (OoT)", "Soul of Gold Skulltulas (OoT)" };
        private static readonly string[] MiscMm =
            { "Soul of Business Scrubs (MM)", "Soul of Gold Skulltulas (MM)" };
        private static readonly string[] MiscShared =
            { "Soul of Business Scrubs", "Soul of Gold Skulltulas" };

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
