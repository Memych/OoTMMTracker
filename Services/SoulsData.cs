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

        public static string ToId(string name) =>
            name.ToLower()
                .Replace(" ", "_").Replace("/", "_").Replace("'", "")
                .Replace(".", "").Replace("(", "").Replace(")", "")
                .Replace("&", "and").Replace(",", "").Replace("-", "_")
                .Replace("__", "_").Trim('_');

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
        // Using fixed labels instead of LabelOf extraction

        private static readonly (string name, string label)[] EnemyOot =
        {
            ("Soul of Anubis (OoT)", "Anubis"),
            ("Soul of Armos (OoT)", "Armos"),
            ("Soul of Baby Dodongos (OoT)", "Baby\nDodongos"),
            ("Soul of Beamos (OoT)", "Beamos"),
            ("Soul of Biris/Baris (OoT)", "Biris/Baris"),
            ("Soul of Bubbles (OoT)", "Bubbles"),
            ("Soul of Dark Link (OoT)", "Dark\nLink"),
            ("Soul of Dead Hands (OoT)", "Dead\nHands"),
            ("Soul of Deku Babas (OoT)", "Deku\nBabas"),
            ("Soul of Deku Scrubs (OoT)", "Deku\nScrubs"),
            ("Soul of Dodongos (OoT)", "Dodongos"),
            ("Soul of Flare Dancers (OoT)", "Flare\nDancers"),
            ("Soul of Floormasters (OoT)", "Floormasters"),
            ("Soul of Flying Pots (OoT)", "Flying\nPots"),
            ("Soul of Freezards (OoT)", "Freezards"),
            ("Soul of the Fighting Gerudos (OoT)", "Fighting\nGerudos"),
            ("Soul of Gohma Larvaes (OoT)", "Gohma\nLarvaes"),
            ("Soul of Guays (OoT)", "Guays"),
            ("Soul of Iron Knuckles (OoT)", "Iron\nKnuckles"),
            ("Soul of Jabu-Jabu's Parasites (OoT)", "Jabu-Jabu's\nParasites"),
            ("Soul of Keese (OoT)", "Keese"),
            ("Soul of Leevers (OoT)", "Leevers"),
            ("Soul of Like-Likes (OoT)", "Like-Likes"),
            ("Soul of Lizalfos/Dinolfos (OoT)", "Lizalfos/\nDinolfos"),
            ("Soul of Moblins (OoT)", "Moblins"),
            ("Soul of Octoroks (OoT)", "Octoroks"),
            ("Soul of Peahats (OoT)", "Peahats"),
            ("Soul of Poes (OoT)", "Poes"),
            ("Soul of ReDeads/Gibdos (OoT)", "ReDeads/\nGibdos"),
            ("Soul of Shaboms (OoT)", "Shaboms"),
            ("Soul of Shell Blades (OoT)", "Shell\nBlades"),
            ("Soul of Skull Kids (OoT)", "Skull\nKids"),
            ("Soul of Skulltulas (OoT)", "Skulltulas"),
            ("Soul of Skullwalltulas (OoT)", "Skullwalltulas"),
            ("Soul of Spikes (OoT)", "Spikes"),
            ("Soul of Stalchildren (OoT)", "Stalchildren"),
            ("Soul of Stalfos (OoT)", "Stalfos"),
            ("Soul of Stingers (OoT)", "Stingers"),
            ("Soul of Tailpasarans (OoT)", "Tailpasarans"),
            ("Soul of Tektites (OoT)", "Tektites"),
            ("Soul of Torch Slugs (OoT)", "Torch\nSlugs"),
            ("Soul of Wallmasters (OoT)", "Wallmasters"),
            ("Soul of Wolfos (OoT)", "Wolfos"),
        };

        // ─── Enemy MM ─────────────────────────────────────────────────────────────
        // 49 enemies, alphabetical, suffix (MM)

        private static readonly (string name, string label)[] EnemyMm =
        {
            ("Soul of Armos (MM)", "Armos"),
            ("Soul of Bad Bats (MM)", "Bad\nBats"),
            ("Soul of Beamos (MM)", "Beamos"),
            ("Soul of Bio Babas (MM)", "Bio\nBabas"),
            ("Soul of Boes (MM)", "Boes"),
            ("Soul of Bubbles (MM)", "Bubbles"),
            ("Soul of Captain Keeta (MM)", "Captain\nKeeta"),
            ("Soul of Chuchus (MM)", "Chuchus"),
            ("Soul of Deep Pythons (MM)", "Deep\nPythons"),
            ("Soul of Deku Babas (MM)", "Deku\nBabas"),
            ("Soul of Deku Scrubs (MM)", "Deku\nScrubs"),
            ("Soul of Dexihands (MM)", "Dexihands"),
            ("Soul of Dodongos (MM)", "Dodongos"),
            ("Soul of Dragonflies (MM)", "Dragonflies"),
            ("Soul of Eenoes (MM)", "Eenoes"),
            ("Soul of Eyegores (MM)", "Eyegores"),
            ("Soul of the Fighting Pirates (MM)", "Fighting\nPirates"),
            ("Soul of Floormasters (MM)", "Floormasters"),
            ("Soul of Flying Pots (MM)", "Flying\nPots"),
            ("Soul of Freezards (MM)", "Freezards"),
            ("Soul of Garo (MM)", "Garo"),
            ("Soul of Gekkos (MM)", "Gekkos"),
            ("Soul of Gomess (MM)", "Gomess"),
            ("Soul of Guays (MM)", "Guays"),
            ("Soul of Hiploops (MM)", "Hiploops"),
            ("Soul of Iron Knuckles (MM)", "Iron\nKnuckles"),
            ("Soul of Keese (MM)", "Keese"),
            ("Soul of Leevers (MM)", "Leevers"),
            ("Soul of Like-Likes (MM)", "Like-Likes"),
            ("Soul of Lizalfos/Dinolfos (MM)", "Lizalfos/\nDinolfos"),
            ("Soul of Nejirons (MM)", "Nejirons"),
            ("Soul of Octoroks (MM)", "Octoroks"),
            ("Soul of Peahats (MM)", "Peahats"),
            ("Soul of Poes (MM)", "Poes"),
            ("Soul of Real Bombchu (MM)", "Real\nBombchu"),
            ("Soul of ReDeads/Gibdos (MM)", "ReDeads/\nGibdos"),
            ("Soul of Shell Blades (MM)", "Shell\nBlades"),
            ("Soul of Skullfish (MM)", "Skullfish"),
            ("Soul of Skulltulas (MM)", "Skulltulas"),
            ("Soul of Skullwalltulas (MM)", "Skullwalltulas"),
            ("Soul of Snappers (MM)", "Snappers"),
            ("Soul of Stalchildren (MM)", "Stalchildren"),
            ("Soul of Takkuri (MM)", "Takkuri"),
            ("Soul of Tektites (MM)", "Tektites"),
            ("Soul of Wallmasters (MM)", "Wallmasters"),
            ("Soul of Warts (MM)", "Warts"),
            ("Soul of Wizzrobes (MM)", "Wizzrobes"),
            ("Soul of Wolfos (MM)", "Wolfos"),
        };

        // ─── Enemy Shared ─────────────────────────────────────────────────────────

        private static readonly (string name, string label)[] EnemyShared =
        {
            ("Soul of Anubis", "Anubis"),
            ("Soul of Armos", "Armos"),
            ("Soul of Baby Dodongos", "Baby\nDodongos"),
            ("Soul of Bad Bats", "Bad\nBats"),
            ("Soul of Beamos", "Beamos"),
            ("Soul of Bio Babas", "Bio\nBabas"),
            ("Soul of Biris/Baris", "Biris/Baris"),
            ("Soul of Boes", "Boes"),
            ("Soul of Bubbles", "Bubbles"),
            ("Soul of Captain Keeta", "Captain\nKeeta"),
            ("Soul of Chuchus", "Chuchus"),
            ("Soul of Dark Link", "Dark\nLink"),
            ("Soul of Dead Hands", "Dead\nHands"),
            ("Soul of Deep Pythons", "Deep\nPythons"),
            ("Soul of Deku Babas", "Deku\nBabas"),
            ("Soul of Deku Scrubs", "Deku\nScrubs"),
            ("Soul of Dexihands", "Dexihands"),
            ("Soul of Dodongos", "Dodongos"),
            ("Soul of Dragonflies", "Dragonflies"),
            ("Soul of Eenoes", "Eenoes"),
            ("Soul of Eyegores", "Eyegores"),
            ("Soul of Fighting Thieves", "Fighting\nThieves"),
            ("Soul of Flare Dancers", "Flare\nDancers"),
            ("Soul of Floormasters", "Floormasters"),
            ("Soul of Flying Pots", "Flying\nPots"),
            ("Soul of Freezards", "Freezards"),
            ("Soul of Garo", "Garo"),
            ("Soul of Gekkos", "Gekkos"),
            ("Soul of Gohma Larvaes", "Gohma\nLarvaes"),
            ("Soul of Gomess", "Gomess"),
            ("Soul of Guays", "Guays"),
            ("Soul of Hiploops", "Hiploops"),
            ("Soul of Iron Knuckles", "Iron\nKnuckles"),
            ("Soul of Jabu-Jabu's Parasites", "Jabu-Jabu's\nParasites"),
            ("Soul of Keese", "Keese"),
            ("Soul of Leevers", "Leevers"),
            ("Soul of Like-Likes", "Like-Likes"),
            ("Soul of Lizalfos/Dinolfos", "Lizalfos/\nDinolfos"),
            ("Soul of Moblins", "Moblins"),
            ("Soul of Nejirons", "Nejirons"),
            ("Soul of Octoroks", "Octoroks"),
            ("Soul of Peahats", "Peahats"),
            ("Soul of Poes", "Poes"),
            ("Soul of Real Bombchu", "Real\nBombchu"),
            ("Soul of ReDeads/Gibdos", "ReDeads/\nGibdos"),
            ("Soul of Shaboms", "Shaboms"),
            ("Soul of Shell Blades", "Shell\nBlades"),
            ("Soul of Skull Kids", "Skull\nKids"),
            ("Soul of Skullfish", "Skullfish"),
            ("Soul of Skulltulas", "Skulltulas"),
            ("Soul of Skullwalltulas", "Skullwalltulas"),
            ("Soul of Snappers", "Snappers"),
            ("Soul of Spikes", "Spikes"),
            ("Soul of Stalchildren", "Stalchildren"),
            ("Soul of Stalfos", "Stalfos"),
            ("Soul of Stingers", "Stingers"),
            ("Soul of Tailpasarans", "Tailpasarans"),
            ("Soul of Takkuri", "Takkuri"),
            ("Soul of Tektites", "Tektites"),
            ("Soul of Torch Slugs", "Torch\nSlugs"),
            ("Soul of Wallmasters", "Wallmasters"),
            ("Soul of Warts", "Warts"),
            ("Soul of Wizzrobes", "Wizzrobes"),
            ("Soul of Wolfos", "Wolfos"),
        };

        public static List<TrackerItem> GetEnemySoulsOot(TrackerConfig cfg)
        {
            if (!cfg.SoulsEnemyOot || cfg.SharedSoulsEnemy) return new();
            var items = new List<TrackerItem>();
            foreach (var (name, label) in EnemyOot)
                items.Add(S(ToId(name), name, "souls/enemy.png", label));
            return items;
        }

        public static List<TrackerItem> GetEnemySoulsMm(TrackerConfig cfg)
        {
            if (!cfg.SoulsEnemyMm || cfg.SharedSoulsEnemy) return new();
            var items = new List<TrackerItem>();
            foreach (var (name, label) in EnemyMm)
                items.Add(S(ToId(name), name, "souls/enemy.png", label));
            return items;
        }

        public static List<TrackerItem> GetEnemySoulsShared(TrackerConfig cfg)
        {
            if (!cfg.SharedSoulsEnemy) return new();
            var items = new List<TrackerItem>();
            foreach (var (name, label) in EnemyShared)
                items.Add(S(ToId(name), name, "souls/enemy.png", label));
            return items;
        }

        // ─── NPC OoT ──────────────────────────────────────────────────────────────

        private static readonly (string name, string label)[] NpcOot =
        {
            ("Soul of the Astronomer (OoT)", "Astronomer"),
            ("Soul of Bazaar Shopkeeper (OoT)", "Bazaar\nShopkeeper"),
            ("Soul of the Bean Salesman (OoT)", "Bean\nSalesman"),
            ("Soul of the Beggar (OoT)", "Beggar"),
            ("Soul of Biggoron (OoT)", "Biggoron"),
            ("Soul of Bombchu Shopkeeper (OoT)", "Bombchu\nShopkeeper"),
            ("Soul of Bombchu Bowling Lady (OoT)", "Bombchu\nBowling Lady"),
            ("Soul of Carpenters (OoT)", "Carpenters"),
            ("Soul of the Carpet Man (OoT)", "Carpet\nMan"),
            ("Soul of Chest Game Owner (OoT)", "Chest Game\nOwner"),
            ("Soul of Goron Child (OoT)", "Child\nGoron"),
            ("Soul of the Citizens (OoT)", "Citizens"),
            ("Soul of Composer Bros. (OoT)", "Composer\nBros."),
            ("Soul of Cucco Lady (OoT)", "Cucco\nLady"),
            ("Soul of Dampe (OoT)", "Dampe"),
            ("Soul of Darunia (OoT)", "Darunia"),
            ("Soul of the Dog Lady (OoT)", "Dog\nLady"),
            ("Soul of Fishing Pond Owner (OoT)", "Fishing Pond\nOwner"),
            ("Soul of Goron (OoT)", "Gorons"),
            ("Soul of Goron Shopkeeper (OoT)", "Goron\nShopkeeper"),
            ("Soul of Graveyard Kid (OoT)", "Graveyard\nKid"),
            ("Soul of Guru-Guru (OoT)", "Guru-Guru"),
            ("Soul of Honey & Darling (OoT)", "Honey &\nDarling"),
            ("Soul of Hylian Guard (OoT)", "Hylian\nGuard"),
            ("Soul of Ingo (OoT)", "Ingo"),
            ("Soul of King Zora (OoT)", "King\nZora"),
            ("Soul of Kokiri (OoT)", "Kokiri"),
            ("Soul of Kokiri Shopkeeper (OoT)", "Kokiri\nShopkeeper"),
            ("Soul of Malon (OoT)", "Malon"),
            ("Soul of Medigoron (OoT)", "Medigoron"),
            ("Soul of Mido (OoT)", "Mido"),
            ("Soul of the Old Hag (OoT)", "Old\nHag"),
            ("Soul of the Patrolling Gerudos (OoT)", "Patrolling\nGerudos"),
            ("Soul of Poe Collector (OoT)", "Poe\nCollector"),
            ("Soul of Potion Shopkeeper (OoT)", "Potion\nShopkeeper"),
            ("Soul of the Punk Kid (OoT)", "Punk\nKid"),
            ("Soul of the Rooftop Man (OoT)", "Rooftop\nMan"),
            ("Soul of Ruto (OoT)", "Ruto"),
            ("Soul of Saria (OoT)", "Saria"),
            ("Soul of the Scientist (OoT)", "Scientist"),
            ("Soul of Sheik (OoT)", "Sheik"),
            ("Soul of Shooting Gallery Owner (OoT)", "Shooting\nGallery Owner"),
            ("Soul of Talon (OoT)", "Talon"),
            ("Soul of Zelda (OoT)", "Zelda"),
            ("Soul of Zora (OoT)", "Zoras"),
            ("Soul of Zora Shopkeeper (OoT)", "Zora\nShopkeeper"),
        };

        // ─── NPC MM ───────────────────────────────────────────────────────────────

        private static readonly (string name, string label)[] NpcMm =
        {
            ("Soul of Anju (MM)", "Anju"),
            ("Soul of Anju's Grandmother (MM)", "Anju's\nGrandmother"),
            ("Soul of Astronomer (MM)", "Astronomer"),
            ("Soul of the Goron Baby (MM)", "Baby\nGoron"),
            ("Soul of the Banker (MM)", "Banker"),
            ("Soul of the Beans Salesman (MM)", "Bean\nSalesman"),
            ("Soul of Mr. Barten (MM)", "Mr. Barten"),
            ("Soul of Biggoron (MM)", "Biggoron"),
            ("Soul of Blacksmiths (MM)", "Blacksmiths"),
            ("Soul of Tourist Center Owner (MM)", "Boat Cruise\nMan"),
            ("Soul of the Bomb Shop Owner (MM)", "Bomb Shop\nOwner"),
            ("Soul of Bombers (MM)", "Bomber\nKids"),
            ("Soul of Carpenters (MM)", "Carpenters"),
            ("Soul of Chest Game Lady (MM)", "Chest Game\nLady"),
            ("Soul of Citizens (MM)", "Citizens"),
            ("Soul of Composer Bros. (MM)", "Composer\nBros."),
            ("Soul of Dampe (MM)", "Dampe"),
            ("Soul of the Deku Butler (MM)", "Deku\nButler"),
            ("Soul of the Deku King (MM)", "Deku\nKing"),
            ("Soul of the Deku Princess (MM)", "Deku\nPrincess"),
            ("Soul of Dog Lady (MM)", "Dog\nLady"),
            ("Soul of the Fisherman (MM)", "Fisherman"),
            ("Soul of Ghost Hut Owner (MM)", "Ghost Hut\nOwner"),
            ("Soul of Gorman & Bros. (MM)", "Gorman &\nBros."),
            ("Soul of Gorons (MM)", "Gorons"),
            ("Soul of the Goron Elder (MM)", "Goron\nElder"),
            ("Soul of the Goron Shopkeeper (MM)", "Goron\nShopkeeper"),
            ("Soul of Grog (MM)", "Grog"),
            ("Soul of Guru-Guru (MM)", "Guru-Guru"),
            ("Soul of Honey & Darling (MM)", "Honey &\nDarling"),
            ("Soul of Kafei (MM)", "Kafei"),
            ("Soul of Keaton (MM)", "Keaton"),
            ("Soul of Keg Trial Goron (MM)", "Keg Trial\nGoron"),
            ("Soul of Koume and Kotake (MM)", "Koume &\nKotake"),
            ("Soul of Lulu (MM)", "Lulu"),
            ("Soul of Madame Aroma (MM)", "Madame\nAroma"),
            ("Soul of Mayor Dotour (MM)", "Mayor\nDotour"),
            ("Soul of Moon Children (MM)", "Moon\nChildren"),
            ("Soul of Part-Timer (MM)", "Part-Timer"),
            ("Soul of the Patrolling Pirates and their Chief (MM)", "Patrolling\nPirates"),
            ("Soul of Playground Scrubs (MM)", "Playground\nScrubs"),
            ("Soul of Romani/Cremia (MM)", "Romani/\nCremia"),
            ("Soul of the Scientist (MM)", "Scientist"),
            ("Soul of Swamp Archery Owner (MM)", "Swamp Archery\nOwner"),
            ("Soul of Swordsman (MM)", "Swordsman"),
            ("Soul of Tingle (MM)", "Tingle"),
            ("Soul of Toilet Hand (MM)", "Toilet\nHand"),
            ("Soul of Toto (MM)", "Toto"),
            ("Soul of Town Archery Owner (MM)", "Town Archery\nOwner"),
            ("Soul of Trading Post Owner (MM)", "Trading Post\nOwner"),
            ("Soul of Zoras (MM)", "Zoras"),
            ("Soul of the Zora Musicians (MM)", "Zora\nMusicians"),
            ("Soul of the Zora Shopkeeper (MM)", "Zora\nShopkeeper"),
        };

        // ─── NPC Shared ───────────────────────────────────────────────────────────

        private static readonly (string name, string label)[] NpcShared =
        {
            ("Soul of Astronomer", "Astronomer"),
            ("Soul of the Bazaar/Swamp Archery Owner", "Bazaar/Swamp\nArchery Owner"),
            ("Soul of Bean Salesman", "Bean\nSalesman"),
            ("Soul of the Beggar/Banker", "Beggar/\nBanker"),
            ("Soul of Biggoron", "Biggoron"),
            ("Soul of Blacksmiths", "Blacksmiths"),
            ("Soul of Tourist Center Owner", "Boat Cruise\nMan"),
            ("Soul of the Bombchu/Bomb Shop Owner", "Bombchu/Bomb\nShop Owner"),
            ("Soul of Bombchu Bowling/Chest Game Lady", "Bombchu Bowling/\nChest Game Lady"),
            ("Soul of Carpenters", "Carpenters"),
            ("Soul of the Carpet Man/Swordsman", "Carpet Man/\nSwordsman"),
            ("Soul of the Chest Game Owner/Fisherman", "Chest Game Owner/\nFisherman"),
            ("Soul of the Goron Child/Baby", "Child/Baby\nGoron"),
            ("Soul of Citizens", "Citizens"),
            ("Soul of Composer Bros.", "Composer\nBros."),
            ("Soul of Cucco Lady/Anju", "Cucco Lady/\nAnju"),
            ("Soul of Dampe", "Dampe"),
            ("Soul of Darunia", "Darunia"),
            ("Soul of Deku Butler", "Deku\nButler"),
            ("Soul of Deku King", "Deku\nKing"),
            ("Soul of Deku Princess", "Deku\nPrincess"),
            ("Soul of Dog Lady", "Dog\nLady"),
            ("Soul of Fishing Pond/Trading Post Owner", "Fishing Pond/\nTrading Post Owner"),
            ("Soul of Goron Elder", "Goron\nElder"),
            ("Soul of Gorons", "Gorons"),
            ("Soul of Goron Shopkeeper", "Goron\nShopkeeper"),
            ("Soul of Graveyard Kid/Bombers", "Graveyard Kid/\nBombers"),
            ("Soul of Guru-Guru", "Guru-Guru"),
            ("Soul of Honey & Darling", "Honey &\nDarling"),
            ("Soul of Hylian Guard", "Hylian\nGuard"),
            ("Soul of Ingo/Gorman & Bros.", "Ingo/Gorman &\nBros."),
            ("Soul of Kafei", "Kafei"),
            ("Soul of Keaton", "Keaton"),
            ("Soul of King Zora", "King\nZora"),
            ("Soul of Kokiri", "Kokiri"),
            ("Soul of Kokiri Shopkeeper", "Kokiri\nShopkeeper"),
            ("Soul of Koume and Kotake", "Koume &\nKotake"),
            ("Soul of Madame Aroma", "Madame\nAroma"),
            ("Soul of Malon/Romani/Cremia", "Malon/Romani/\nCremia"),
            ("Soul of Mayor Dotour", "Mayor\nDotour"),
            ("Soul of Medigoron/Keg Trial Goron", "Medigoron/Keg\nTrial Goron"),
            ("Soul of Mido", "Mido"),
            ("Soul of Moon Children", "Moon\nChildren"),
            ("Soul of Old Hag/Anju's Grandmother", "Old Hag/Anju's\nGrandmother"),
            ("Soul of the Patrolling Thieves and their Chief", "Patrolling\nThieves"),
            ("Soul of Playground Scrubs", "Playground\nScrubs"),
            ("Soul of Poe Collector/Ghost Hut Owner", "Poe Collector/\nGhost Hut Owner"),
            ("Soul of Potion Shopkeeper", "Potion\nShopkeeper"),
            ("Soul of Punk Kid/Grog", "Punk Kid/\nGrog"),
            ("Soul of Rooftop Man/Part-Timer", "Rooftop Man/\nPart-Timer"),
            ("Soul of Ruto/Lulu", "Ruto/Lulu"),
            ("Soul of Saria", "Saria"),
            ("Soul of the Scientist", "Scientist"),
            ("Soul of Sheik", "Sheik"),
            ("Soul of Shooting Gallery/Town Archery Owner", "Shooting Gallery/\nTown Archery Owner"),
            ("Soul of Talon/Mr. Barten", "Talon/\nMr. Barten"),
            ("Soul of Tingle", "Tingle"),
            ("Soul of Toilet Hand", "Toilet\nHand"),
            ("Soul of Toto", "Toto"),
            ("Soul of Zelda", "Zelda"),
            ("Soul of Zora Musicians", "Zora\nMusicians"),
            ("Soul of Zoras", "Zoras"),
            ("Soul of Zora Shopkeeper", "Zora\nShopkeeper"),
        };

        public static List<TrackerItem> GetNpcSoulsOot(TrackerConfig cfg)
        {
            if (!cfg.SoulsNpcOot || cfg.SharedSoulsNpc) return new();
            var items = new List<TrackerItem>();
            foreach (var (name, label) in NpcOot)
                items.Add(S(ToId(name), name, "souls/npc.png", label));
            return items;
        }

        public static List<TrackerItem> GetNpcSoulsMm(TrackerConfig cfg)
        {
            if (!cfg.SoulsNpcMm || cfg.SharedSoulsNpc) return new();
            var items = new List<TrackerItem>();
            foreach (var (name, label) in NpcMm)
                items.Add(S(ToId(name), name, "souls/npc.png", label));
            return items;
        }

        public static List<TrackerItem> GetNpcSoulsShared(TrackerConfig cfg)
        {
            if (!cfg.SharedSoulsNpc) return new();
            var items = new List<TrackerItem>();
            foreach (var (name, label) in NpcShared)
                items.Add(S(ToId(name), name, "souls/npc.png", label));
            return items;
        }

        // ─── Animal ───────────────────────────────────────────────────────────────

        private static readonly (string name, string label)[] AnimalOot =
            { ("Soul of Butterflies (OoT)", "Butterflies"), ("Soul of Cows (OoT)", "Cows"), ("Soul of Cuccos (OoT)", "Cuccos"), ("Soul of Dogs (OoT)", "Dogs") };
        
        private static readonly (string name, string label)[] AnimalMm =
            { ("Soul of Butterflies (MM)", "Butterflies"), ("Soul of Cows (MM)", "Cows"), ("Soul of Cuccos (MM)", "Cuccos"), ("Soul of Dogs (MM)", "Dogs") };
        
        private static readonly (string name, string label)[] AnimalShared =
            { ("Soul of Butterflies", "Butterflies"), ("Soul of Cows", "Cows"), ("Soul of Cuccos", "Cuccos"), ("Soul of Dogs", "Dogs") };

        public static List<TrackerItem> GetAnimalSoulsOot(TrackerConfig cfg)
        {
            if (!cfg.SoulsAnimalOot || cfg.SharedSoulsAnimal) return new();
            var items = new List<TrackerItem>();
            foreach (var (name, label) in AnimalOot)
                items.Add(S(ToId(name), name, "souls/animal.png", label));
            return items;
        }

        public static List<TrackerItem> GetAnimalSoulsMm(TrackerConfig cfg)
        {
            if (!cfg.SoulsAnimalMm || cfg.SharedSoulsAnimal) return new();
            var items = new List<TrackerItem>();
            foreach (var (name, label) in AnimalMm)
                items.Add(S(ToId(name), name, "souls/animal.png", label));
            return items;
        }

        public static List<TrackerItem> GetAnimalSoulsShared(TrackerConfig cfg)
        {
            if (!cfg.SharedSoulsAnimal) return new();
            var items = new List<TrackerItem>();
            foreach (var (name, label) in AnimalShared)
                items.Add(S(ToId(name), name, "souls/animal.png", label));
            return items;
        }

        // ─── Misc ─────────────────────────────────────────────────────────────────

        private static readonly (string name, string label)[] MiscOot =
            { ("Soul of Business Scrubs (OoT)", "Business\nScrubs"), ("Soul of Gold Skulltulas (OoT)", "Gold\nSkulltulas") };
        
        private static readonly (string name, string label)[] MiscMm =
            { ("Soul of Business Scrubs (MM)", "Business\nScrubs"), ("Soul of Gold Skulltulas (MM)", "Gold\nSkulltulas") };
        
        private static readonly (string name, string label)[] MiscShared =
            { ("Soul of Business Scrubs", "Business\nScrubs"), ("Soul of Gold Skulltulas", "Gold\nSkulltulas") };

        public static List<TrackerItem> GetMiscSoulsOot(TrackerConfig cfg)
        {
            if (!cfg.SoulsMiscOot || cfg.SharedSoulsMisc) return new();
            var items = new List<TrackerItem>();
            foreach (var (name, label) in MiscOot)
                items.Add(S(ToId(name), name, "souls/misc.png", label));
            return items;
        }

        public static List<TrackerItem> GetMiscSoulsMm(TrackerConfig cfg)
        {
            if (!cfg.SoulsMiscMm || cfg.SharedSoulsMisc) return new();
            var items = new List<TrackerItem>();
            foreach (var (name, label) in MiscMm)
                items.Add(S(ToId(name), name, "souls/misc.png", label));
            return items;
        }

        public static List<TrackerItem> GetMiscSoulsShared(TrackerConfig cfg)
        {
            if (!cfg.SharedSoulsMisc) return new();
            var items = new List<TrackerItem>();
            foreach (var (name, label) in MiscShared)
                items.Add(S(ToId(name), name, "souls/misc.png", label));
            return items;
        }
    }
}