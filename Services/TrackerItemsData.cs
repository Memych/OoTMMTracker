using System.Collections.Generic;
using System.Linq;
using OoTMMTracker.Models;

namespace OoTMMTracker.Services
{
    public static class TrackerItemsData
    {
        // ─── Progression Icons ────────────────────────────────────────────────
        private static readonly string[] WalletIcons3 = { "equipment/wallet1.png", "equipment/wallet1.png", "equipment/wallet2.png" };
        private static readonly string[] WalletIcons4 = { "equipment/wallet1.png", "equipment/wallet1.png", "equipment/wallet2.png", "equipment/wallet2.png" };
        private static readonly string[] WalletIcons5 = { "equipment/wallet1.png", "equipment/wallet1.png", "equipment/wallet1.png", "equipment/wallet2.png", "equipment/wallet2.png" };
        private static readonly string[] MagicIcons   = { "equipment/magic.png", "equipment/double_magic.png" };
        private static readonly string[] BiggoronIcons = { "equipment/giants_knife.png", "equipment/biggoron_sword.png" };
        private static readonly string[] ScaleIcons2  = { "equipment/silver_scale.png", "equipment/golden_scale.png" };
        private static readonly string[] ScaleIcons3  = { "equipment/bronze_scale.png", "equipment/silver_scale.png", "equipment/golden_scale.png" };
        private static readonly string[] StrengthIcons = { "equipment/goron_bracelet.png", "equipment/silver_gauntlets.png", "equipment/golden_gauntlets.png" };

        // Wallet labels: 99, 200, 500, 999, 9999 — always full set
        private static readonly string[] WalletLabels3 = { "99", "200", "500" };
        private static readonly string[] WalletLabels4 = { "99", "200", "500", "999" };
        private static readonly string[] WalletLabels5 = { "99", "200", "500", "999", "9999" };

        private static string[] GetWalletIcons(int max) => max switch
        {
            3 => WalletIcons3, 4 => WalletIcons4, _ => WalletIcons5
        };

        private static string[] GetWalletLabels(int max) => max switch
        {
            3 => WalletLabels3, 4 => WalletLabels4, _ => WalletLabels5
        };

        public static List<TrackerItem> GetOotEquipmentItems(TrackerConfig cfg)
        {
            int swordMax = cfg.OotExtraChildSwords ? 3 : 1;
            string[] swordSteps = cfg.OotExtraChildSwords
                ? new[] { "Kokiri Sword", "Razor Sword", "Gilded Sword" }
                : new[] { "Kokiri Sword" };
            string? swordIcon = cfg.OotExtraChildSwords ? "equipment/kokiri_sword_oot.png" : "equipment/kokiri_sword_oot.png";
            int scaleMax = cfg.OotBronzeScale ? 3 : 2;
            string[] scaleSteps = cfg.OotBronzeScale
                ? new[] { "Bronze Scale", "Silver Scale", "Golden Scale" }
                : new[] { "Silver Scale", "Golden Scale" };
            string? scaleIcon = cfg.OotBronzeScale ? "equipment/bronze_scale.png" : "equipment/silver_scale.png";
            var (walletMax, walletSteps, walletMin) = BuildWalletProgression(cfg);
            var items = new List<TrackerItem>();

            // Row 1: Stone - Kokiri Sword - Master Sword - Biggoron Sword
            if (!cfg.SharedStoneAgony)
                items.Add(new() { Id="oot_stone_agony",    Name="Stone of Agony",   Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/stone_of_agony.png" });
            if (!cfg.SharedSwords)
            {
                items.Add(new() { Id="oot_sword", Name="Sword", Type=TrackerItemType.Equipment, MaxCount=swordMax, StepNames=swordSteps, IconPath=swordIcon,
                    StepIconPaths=cfg.OotExtraChildSwords ? new[]{"equipment/kokiri_sword_oot.png","equipment/razor_sword.png","equipment/gilded_sword.png"} : new[]{"equipment/kokiri_sword_oot.png"} });
                items.Add(new() { Id="oot_master_sword",   Name="Master Sword",     Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/master_sword.png" });
                items.Add(new() { Id="oot_biggoron_sword", Name="Biggoron's Sword", Type=TrackerItemType.Equipment, MaxCount=2,
                    StepNames=new[]{"Giant's Knife","Biggoron's Sword"}, IconPath="equipment/giants_knife.png", StepIconPaths=BiggoronIcons });
            }

            // Row 2: Gerudo Card - Deku Shield - Hylian Shield - Mirror Shield
            if (!cfg.SharedSwords)
                items.Add(new() { Id="oot_gerudo_card",    Name="Gerudo Membership Card", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/gerudo_membership_card.png" });
            if (!cfg.SharedShields)
            {
                items.Add(new() { Id="oot_deku_shield",    Name="Deku Shield",   Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/deku_shield.png" });
                items.Add(new() { Id="oot_hylian_shield",  Name="Hylian Shield", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/hylian_shield.png" });
                items.Add(new() { Id="oot_mirror_shield",  Name="Mirror Shield", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/mirror_shield_oot.png" });
            }

            // Row 3: Goron Tunic - Zora Tunic - Iron Boots - Hover Boots
            if (!cfg.SharedTunicGoron)
                items.Add(new() { Id="oot_goron_tunic",    Name="Goron Tunic",   Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/goron_tunic.png" });
            if (!cfg.SharedTunicZora)
                items.Add(new() { Id="oot_zora_tunic",     Name="Zora Tunic",    Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/zora_tunic.png" });
            if (!cfg.SharedBootsIron)
                items.Add(new() { Id="oot_iron_boots",     Name="Iron Boots",    Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/iron_boots.png" });
            if (!cfg.SharedBootsHover)
                items.Add(new() { Id="oot_hover_boots",    Name="Hover Boots",   Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/hover_boots.png" });

            // Row 4: Strength - Scale - Magic - Wallet
            if (!cfg.SharedStrength)
                items.Add(new() { Id="oot_strength", Name="Strength", Type=TrackerItemType.Equipment, MaxCount=3,
                    StepNames=new[]{"Goron's Bracelet","Silver Gauntlets","Golden Gauntlets"}, IconPath="equipment/goron_bracelet.png", StepIconPaths=StrengthIcons });
            if (!cfg.SharedScales)
                items.Add(new() { Id="oot_scale", Name="Scale", Type=TrackerItemType.Equipment, MaxCount=scaleMax, StepNames=scaleSteps, IconPath=scaleIcon,
                    StepIconPaths=cfg.OotBronzeScale ? ScaleIcons3 : ScaleIcons2 });
            if (!cfg.SharedMagic)
                items.Add(new() { Id="oot_magic", Name="Magic", Type=TrackerItemType.Equipment, MaxCount=2,
                    StepNames=new[]{"Magic Upgrade","Large Magic Upgrade"}, IconPath="equipment/magic.png", StepIconPaths=MagicIcons });
            if (!cfg.SharedWallets)
                items.Add(new() { Id="oot_wallet", Name="Wallet", Type=TrackerItemType.Equipment, MaxCount=walletMax, StepNames=walletSteps, IconPath="equipment/wallet1.png",
                    StepIconPaths=GetWalletIcons(walletMax), StepLabels=GetWalletLabels(walletMax), MinCount=walletMin, CurrentCount=walletMin });

            // Row 5: Defence - Container - Pieces
            if (!cfg.SharedHealth)
            {
                items.Add(new() { Id="oot_double_defence",  Name="Double Defence",  Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/double_defense.png" });
                items.Add(new() { Id="oot_heart_container", Name="Heart Container", Type=TrackerItemType.Equipment, MaxCount=8, IconPath="equipment/heart_container.png",
                    StepLabels=new[]{"1","2","3","4","5","6","7","8"} });
                items.Add(new() { Id="oot_heart_piece",     Name="Piece of Heart",  Type=TrackerItemType.Equipment, MaxCount=36, IconPath="equipment/heart_piece.png",
                    StepLabels=Enumerable.Range(1,36).Select(i=>i.ToString()).ToArray() });
            }
            // Great Spin — OoT exclusive, add if not shared
            if (cfg.OotSpinUpgrade && !cfg.SharedSpinUpgrade)
                items.Add(new() { Id="oot_great_spin", Name="Great Spin Attack", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/great_spin_attack.png" });

            return items;
        }

        // ─── MM Equipment ─────────────────────────────────────────────────────────

        public static List<TrackerItem> GetMmEquipmentItems(TrackerConfig cfg)
        {
            int scaleMax = 0; string[]? scaleSteps = null; string? scaleIcon = null;
            if (cfg.MmScales)
            {
                if (cfg.OotBronzeScale) { scaleMax=3; scaleSteps=new[]{"Bronze Scale","Silver Scale","Golden Scale"}; scaleIcon="equipment/bronze_scale.png"; }
                else                    { scaleMax=2; scaleSteps=new[]{"Silver Scale","Golden Scale"};                scaleIcon="equipment/silver_scale.png"; }
            }
            var (walletMax, walletSteps, walletMin) = BuildWalletProgression(cfg);
            var items = new List<TrackerItem>();

            // Row 1: Notebook - Sword - Spin - Hero Shield - Mirror Shield
            if (!cfg.SharedSwords)
                items.Add(new() { Id="mm_notebook", Name="Bombers' Notebook", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/bombers_notebook.png" });
            if (!cfg.SharedSwords)
                items.Add(new() { Id="mm_sword", Name="Kokiri Sword (MM)", Type=TrackerItemType.Equipment,
                    MaxCount=3, StepNames=new[]{"Kokiri Sword","Razor Sword","Gilded Sword"},
                    IconPath="equipment/kokiri_sword_mm.png",
                    StepIconPaths=new[]{"equipment/kokiri_sword_mm.png","equipment/razor_sword.png","equipment/gilded_sword.png"} });
            if (!cfg.SharedSpinUpgrade)
                items.Add(new() { Id="mm_great_spin", Name="Great Spin Attack", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/great_spin_attack.png" });
            if (!cfg.SharedShields)
            {
                if (cfg.MmDekuShield)
                    items.Add(new() { Id="mm_deku_shield",   Name="Deku Shield (MM)", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/deku_shield.png" });
                items.Add(new() { Id="mm_hero_shield",   Name="Hero's Shield",    Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/heros_shield.png" });
                items.Add(new() { Id="mm_mirror_shield", Name="Mirror Shield",    Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/mirror_shield_mm.png" });
            }

            // Row 2: Defence - Container - Pieces - Magic - Wallet
            if (!cfg.SharedHealth)
            {
                items.Add(new() { Id="mm_double_defence",  Name="Double Defence",  Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/double_defense.png" });
                items.Add(new() { Id="mm_heart_container", Name="Heart Container", Type=TrackerItemType.Equipment, MaxCount=4, IconPath="equipment/heart_container.png",
                    StepLabels=new[]{"1","2","3","4"} });
                items.Add(new() { Id="mm_heart_piece",     Name="Piece of Heart",  Type=TrackerItemType.Equipment, MaxCount=52, IconPath="equipment/heart_piece.png",
                    StepLabels=Enumerable.Range(1,52).Select(i=>i.ToString()).ToArray() });
            }
            if (!cfg.SharedMagic)
                items.Add(new() { Id="mm_magic", Name="Magic", Type=TrackerItemType.Equipment, MaxCount=2,
                    StepNames=new[]{"Magic Upgrade","Large Magic Upgrade"}, IconPath="equipment/magic.png", StepIconPaths=MagicIcons });
            if (!cfg.SharedWallets)
                items.Add(new() { Id="mm_wallet", Name="Wallet", Type=TrackerItemType.Equipment, MaxCount=walletMax, StepNames=walletSteps, IconPath="equipment/wallet1.png",
                    StepIconPaths=GetWalletIcons(walletMax), StepLabels=GetWalletLabels(walletMax), MinCount=walletMin, CurrentCount=walletMin });

            // Optional MM items
            if (cfg.MmStoneAgony && !cfg.SharedStoneAgony)
                items.Add(new() { Id="mm_stone_agony", Name="Stone of Agony (MM)", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/stone_of_agony.png" });
            if (!cfg.SharedTunicGoron && cfg.MmGoronTunic)
                items.Add(new() { Id="mm_goron_tunic", Name="Goron Tunic (MM)", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/goron_tunic.png" });
            if (!cfg.SharedTunicZora && cfg.MmZoraTunic)
                items.Add(new() { Id="mm_zora_tunic", Name="Zora Tunic (MM)", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/zora_tunic.png" });
            if (!cfg.SharedBootsIron && cfg.MmIronBoots)
                items.Add(new() { Id="mm_iron_boots",  Name="Iron Boots (MM)",  Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/iron_boots.png" });
            if (!cfg.SharedBootsHover && cfg.MmHoverBoots)
                items.Add(new() { Id="mm_hover_boots", Name="Hover Boots (MM)", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/hover_boots.png" });
            if (cfg.MmStrength && !cfg.SharedStrength)
                items.Add(new() { Id="mm_strength", Name="Strength (MM)", Type=TrackerItemType.Equipment, MaxCount=3,
                    StepNames=new[]{"Goron's Bracelet","Silver Gauntlets","Golden Gauntlets"}, IconPath="equipment/goron_bracelet.png", StepIconPaths=StrengthIcons });
            if (scaleMax > 0 && !cfg.SharedScales)
                items.Add(new() { Id="mm_scale", Name="Scale (MM)", Type=TrackerItemType.Equipment, MaxCount=scaleMax, StepNames=scaleSteps, IconPath=scaleIcon,
                    StepIconPaths=cfg.OotBronzeScale ? ScaleIcons3 : ScaleIcons2 });

            return items;
        }

        // ─── Shared (combined) items ───────────────────────────────────────

        public static List<TrackerItem> GetSharedItems(TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();
            var (walletMax, walletSteps, walletMin) = BuildWalletProgression(cfg);
            int scaleMax = cfg.OotBronzeScale ? 3 : 2;
            string[] scaleSteps = cfg.OotBronzeScale
                ? new[] { "Bronze Scale", "Silver Scale", "Golden Scale" }
                : new[] { "Silver Scale", "Golden Scale" };

            // Row 1: Stone - Notebook - Kokiri Sword - Spin - Master Sword - Biggoron Sword
            if (cfg.SharedStoneAgony)
                items.Add(new() { Id="sh_stone_agony",    Name="Stone of Agony",   Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/stone_of_agony.png" });
            if (cfg.SharedSwords)
                items.Add(new() { Id="sh_notebook",       Name="Bombers' Notebook", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/bombers_notebook.png" });
            if (cfg.SharedSwords)
            {
                int sMax = cfg.OotExtraChildSwords ? 3 : 1;
                string[] sSteps = cfg.OotExtraChildSwords ? new[]{"Kokiri Sword","Razor Sword","Gilded Sword"} : new[]{"Kokiri Sword"};
                items.Add(new() { Id="sh_sword", Name="Sword", Type=TrackerItemType.Equipment, MaxCount=sMax, StepNames=sSteps, IconPath="equipment/kokiri_sword_oot.png",
                    StepIconPaths=cfg.OotExtraChildSwords ? new[]{"equipment/kokiri_sword_oot.png","equipment/razor_sword.png","equipment/gilded_sword.png"} : null });
            }
            if (cfg.SharedSpinUpgrade && cfg.OotSpinUpgrade)
                items.Add(new() { Id="sh_great_spin",     Name="Great Spin Attack", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/great_spin_attack.png" });
            if (cfg.SharedSwords)
            {
                items.Add(new() { Id="sh_master_sword",   Name="Master Sword",     Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/master_sword.png" });
                items.Add(new() { Id="sh_biggoron_sword", Name="Biggoron's Sword", Type=TrackerItemType.Equipment, MaxCount=2,
                    StepNames=new[]{"Giant's Knife","Biggoron's Sword"}, IconPath="equipment/giants_knife.png", StepIconPaths=BiggoronIcons });
            }

            // Row 2: Gerudo Card - Strength - Scale - Deku Shield - Hylian/Hero Shield - Mirror Shield
            if (cfg.SharedSwords)
                items.Add(new() { Id="sh_gerudo_card",    Name="Gerudo Membership Card", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/gerudo_membership_card.png" });
            if (cfg.SharedStrength)
                items.Add(new() { Id="sh_strength", Name="Strength", Type=TrackerItemType.Equipment, MaxCount=3,
                    StepNames=new[]{"Goron's Bracelet","Silver Gauntlets","Golden Gauntlets"}, IconPath="equipment/goron_bracelet.png", StepIconPaths=StrengthIcons });
            if (cfg.SharedScales)
                items.Add(new() { Id="sh_scale", Name="Scale", Type=TrackerItemType.Equipment, MaxCount=scaleMax, StepNames=scaleSteps,
                    IconPath=cfg.OotBronzeScale ? "equipment/bronze_scale.png" : "equipment/silver_scale.png",
                    StepIconPaths=cfg.OotBronzeScale ? ScaleIcons3 : ScaleIcons2 });
            if (cfg.SharedShields)
            {
                items.Add(new() { Id="sh_deku_shield",   Name="Deku Shield",   Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/deku_shield.png" });
                items.Add(new() { Id="sh_hylian_shield", Name="Hylian Shield", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/hylian_shield.png" });
                items.Add(new() { Id="sh_mirror_shield", Name="Mirror Shield", Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/mirror_shield_oot.png" });
            }

            // Row 3: Goron Tunic - Zora Tunic - Iron Boots - Hover Boots - Magic - Wallet
            if (cfg.SharedTunicGoron)
                items.Add(new() { Id="sh_goron_tunic",   Name="Goron Tunic",   Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/goron_tunic.png" });
            if (cfg.SharedTunicZora)
                items.Add(new() { Id="sh_zora_tunic",    Name="Zora Tunic",    Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/zora_tunic.png" });
            if (cfg.SharedBootsIron)
                items.Add(new() { Id="sh_iron_boots",    Name="Iron Boots",    Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/iron_boots.png" });
            if (cfg.SharedBootsHover)
                items.Add(new() { Id="sh_hover_boots",   Name="Hover Boots",   Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/hover_boots.png" });
            if (cfg.SharedMagic)
                items.Add(new() { Id="sh_magic", Name="Magic", Type=TrackerItemType.Equipment, MaxCount=2,
                    StepNames=new[]{"Magic Upgrade","Large Magic Upgrade"}, IconPath="equipment/magic.png", StepIconPaths=MagicIcons });
            if (cfg.SharedWallets)
                items.Add(new() { Id="sh_wallet", Name="Wallet", Type=TrackerItemType.Equipment, MaxCount=walletMax, StepNames=walletSteps, IconPath="equipment/wallet1.png",
                    StepIconPaths=GetWalletIcons(walletMax), StepLabels=GetWalletLabels(walletMax), MinCount=walletMin, CurrentCount=walletMin });

            // Row 4-5: Defence - Containers - Pieces
            if (cfg.SharedHealth)
            {
                items.Add(new() { Id="sh_double_defence",  Name="Double Defence",  Type=TrackerItemType.Equipment, MaxCount=1, IconPath="equipment/double_defense.png" });
                items.Add(new() { Id="sh_heart_container", Name="Heart Container", Type=TrackerItemType.Equipment, MaxCount=6, IconPath="equipment/heart_container.png",
                    StepLabels=new[]{"1","2","3","4","5","6"} });
                items.Add(new() { Id="sh_heart_piece",     Name="Piece of Heart",  Type=TrackerItemType.Equipment, MaxCount=44, IconPath="equipment/heart_piece.png",
                    StepLabels=Enumerable.Range(1,44).Select(i=>i.ToString()).ToArray() });
            }

            return items;
        }

        // ─── Masks MM (24, order 6×4) ──────────────────────────────────────────

        public static List<TrackerItem> GetMmMaskItems(TrackerConfig cfg)
        {
            bool allShared = cfg.SharedMaskGoron && cfg.SharedMaskZora && cfg.SharedMaskBunny
                          && cfg.SharedMaskKeaton && cfg.SharedMaskTruth && cfg.SharedMaskBlast
                          && cfg.SharedMaskStone;
            if (allShared) return new List<TrackerItem>();

            var items = new List<TrackerItem>();

            // Row 1 — Blast and Stone can be shared
            items.Add(new() { Id="postman_hat",      Name="Postman's Hat",      Type=TrackerItemType.Mask, IconPath="masks/postmans_hat.png" });
            items.Add(new() { Id="all_night_mask",   Name="All-Night Mask",     Type=TrackerItemType.Mask, IconPath="masks/allnight_mask.png" });
            if (!cfg.SharedMaskBlast)
                items.Add(new() { Id="blast_mask",   Name="Blast Mask",         Type=TrackerItemType.Mask, IconPath="masks/blast_mask.png" });
            if (!cfg.SharedMaskStone)
                items.Add(new() { Id="stone_mask",   Name="Stone Mask",         Type=TrackerItemType.Mask, IconPath="masks/stone_mask.png" });
            items.Add(new() { Id="great_fairy_mask", Name="Great Fairy's Mask", Type=TrackerItemType.Mask, IconPath="masks/great_fairys_mask.png" });
            items.Add(new() { Id="deku_mask",        Name="Deku Mask",          Type=TrackerItemType.Mask, IconPath="masks/deku_mask.png" });

            // Row 2 — Keaton, Bunny, Goron can be shared
            if (!cfg.SharedMaskKeaton)
                items.Add(new() { Id="keaton_mask",  Name="Keaton Mask",        Type=TrackerItemType.Mask, IconPath="masks/keaton_mask.png" });
            items.Add(new() { Id="bremen_mask",      Name="Bremen Mask",        Type=TrackerItemType.Mask, IconPath="masks/bremen_mask.png" });
            if (!cfg.SharedMaskBunny)
                items.Add(new() { Id="bunny_hood",   Name="Bunny Hood",         Type=TrackerItemType.Mask, IconPath="masks/bunny_hood.png" });
            items.Add(new() { Id="don_gero_mask",    Name="Don Gero's Mask",    Type=TrackerItemType.Mask, IconPath="masks/don_geros_mask.png" });
            items.Add(new() { Id="mask_of_scents",   Name="Mask of Scents",     Type=TrackerItemType.Mask, IconPath="masks/mask_of_scents.png" });
            if (!cfg.SharedMaskGoron)
                items.Add(new() { Id="goron_mask",   Name="Goron Mask",         Type=TrackerItemType.Mask, IconPath="masks/goron_mask.png" });

            // Row 3 — Zora, Truth can be shared
            items.Add(new() { Id="romani_mask",        Name="Romani's Mask",        Type=TrackerItemType.Mask, IconPath="masks/romanis_mask.png" });
            items.Add(new() { Id="circus_leader_mask", Name="Circus Leader's Mask", Type=TrackerItemType.Mask, IconPath="masks/circus_leaders_mask.png" });
            items.Add(new() { Id="kafei_mask",         Name="Kafei's Mask",         Type=TrackerItemType.Mask, IconPath="masks/kafeis_mask.png" });
            items.Add(new() { Id="couples_mask",       Name="Couple's Mask",        Type=TrackerItemType.Mask, IconPath="masks/couples_mask.png" });
            if (!cfg.SharedMaskTruth)
                items.Add(new() { Id="mask_of_truth",  Name="Mask of Truth",        Type=TrackerItemType.Mask, IconPath="masks/mask_of_truth.png" });
            if (!cfg.SharedMaskZora)
                items.Add(new() { Id="zora_mask",      Name="Zora Mask",            Type=TrackerItemType.Mask, IconPath="masks/zora_mask.png" });

            // Row 4
            items.Add(new() { Id="kamaro_mask",  Name="Kamaro's Mask",       Type=TrackerItemType.Mask, IconPath="masks/kamaros_mask.png" });
            items.Add(new() { Id="gibdo_mask",   Name="Gibdo Mask",          Type=TrackerItemType.Mask, IconPath="masks/gibdo_mask.png" });
            items.Add(new() { Id="garo_mask",    Name="Garo's Mask",         Type=TrackerItemType.Mask, IconPath="masks/garos_mask.png" });
            items.Add(new() { Id="captain_hat",  Name="Captain's Hat",       Type=TrackerItemType.Mask, IconPath="masks/captains_hat.png" });
            items.Add(new() { Id="giants_mask",  Name="Giant's Mask",        Type=TrackerItemType.Mask, IconPath="masks/giants_mask.png" });
            items.Add(new() { Id="fierce_deity", Name="Fierce Deity's Mask", Type=TrackerItemType.Mask, IconPath="masks/fierce_deitys_mask.png" });

            return items;
        }

        // ─── Masks OoT (8 basic + Blast/Stone optional) ─────────────────────

        public static List<TrackerItem> GetOotMaskItems(TrackerConfig cfg)
        {
            bool allShared = cfg.SharedMaskGoron && cfg.SharedMaskZora && cfg.SharedMaskBunny
                          && cfg.SharedMaskKeaton && cfg.SharedMaskTruth && cfg.SharedMaskBlast
                          && cfg.SharedMaskStone;
            if (allShared) return new List<TrackerItem>();

            var items = new List<TrackerItem>();

            // Keaton — shared or OoT
            if (!cfg.SharedMaskKeaton)
                items.Add(new() { Id="oot_keaton_mask", Name="Keaton Mask",   Type=TrackerItemType.Mask, IconPath="masks/keaton_mask.png" });
            items.Add(new() { Id="oot_skull_mask",  Name="Skull Mask",        Type=TrackerItemType.Mask, IconPath="masks/skull_mask.png" });
            items.Add(new() { Id="oot_spooky_mask", Name="Spooky Mask",       Type=TrackerItemType.Mask, IconPath="masks/spooky_mask.png" });
            if (!cfg.SharedMaskBunny)
                items.Add(new() { Id="oot_bunny_hood",  Name="Bunny Hood",    Type=TrackerItemType.Mask, IconPath="masks/bunny_hood.png" });
            if (!cfg.SharedMaskGoron)
                items.Add(new() { Id="oot_goron_mask",  Name="Goron Mask",    Type=TrackerItemType.Mask, IconPath="masks/goron_mask.png" });
            if (!cfg.SharedMaskZora)
                items.Add(new() { Id="oot_zora_mask",   Name="Zora Mask",     Type=TrackerItemType.Mask, IconPath="masks/zora_mask.png" });
            items.Add(new() { Id="oot_gerudo_mask", Name="Gerudo Mask",       Type=TrackerItemType.Mask, IconPath="masks/gerudo_mask.png" });
            if (!cfg.SharedMaskTruth)
                items.Add(new() { Id="oot_truth_mask",  Name="Mask of Truth", Type=TrackerItemType.Mask, IconPath="masks/mask_of_truth.png" });
            // Optional OoT masks
            if (cfg.OotBlastMask && !cfg.SharedMaskBlast)
                items.Add(new() { Id="oot_blast_mask",  Name="Blast Mask",    Type=TrackerItemType.Mask, IconPath="masks/blast_mask.png" });
            if (cfg.OotStoneMask && !cfg.SharedMaskStone)
                items.Add(new() { Id="oot_stone_mask",  Name="Stone Mask",    Type=TrackerItemType.Mask, IconPath="masks/stone_mask.png" });

            return items;
        }

        // ─── Masks Shared ─────────────────────────────────────────────────────────

        public static List<TrackerItem> GetSharedMasks(TrackerConfig cfg)
        {
            bool allShared = cfg.SharedMaskGoron && cfg.SharedMaskZora && cfg.SharedMaskBunny
                          && cfg.SharedMaskKeaton && cfg.SharedMaskTruth && cfg.SharedMaskBlast
                          && cfg.SharedMaskStone;

            var items = new List<TrackerItem>();

            if (allShared)
            {
                // All masks in one block: first MM order, then OoT exclusives
                items.AddRange(new[]
                {
                    new TrackerItem { Id="postman_hat",        Name="Postman's Hat",        Type=TrackerItemType.Mask, IconPath="masks/postmans_hat.png" },
                    new TrackerItem { Id="all_night_mask",     Name="All-Night Mask",       Type=TrackerItemType.Mask, IconPath="masks/allnight_mask.png" },
                    new TrackerItem { Id="blast_mask",         Name="Blast Mask",           Type=TrackerItemType.Mask, IconPath="masks/blast_mask.png" },
                    new TrackerItem { Id="stone_mask",         Name="Stone Mask",           Type=TrackerItemType.Mask, IconPath="masks/stone_mask.png" },
                    new TrackerItem { Id="great_fairy_mask",   Name="Great Fairy's Mask",   Type=TrackerItemType.Mask, IconPath="masks/great_fairys_mask.png" },
                    new TrackerItem { Id="deku_mask",          Name="Deku Mask",            Type=TrackerItemType.Mask, IconPath="masks/deku_mask.png" },
                    new TrackerItem { Id="keaton_mask",        Name="Keaton Mask",          Type=TrackerItemType.Mask, IconPath="masks/keaton_mask.png" },
                    new TrackerItem { Id="bremen_mask",        Name="Bremen Mask",          Type=TrackerItemType.Mask, IconPath="masks/bremen_mask.png" },
                    new TrackerItem { Id="bunny_hood",         Name="Bunny Hood",           Type=TrackerItemType.Mask, IconPath="masks/bunny_hood.png" },
                    new TrackerItem { Id="don_gero_mask",      Name="Don Gero's Mask",      Type=TrackerItemType.Mask, IconPath="masks/don_geros_mask.png" },
                    new TrackerItem { Id="mask_of_scents",     Name="Mask of Scents",       Type=TrackerItemType.Mask, IconPath="masks/mask_of_scents.png" },
                    new TrackerItem { Id="goron_mask",         Name="Goron Mask",           Type=TrackerItemType.Mask, IconPath="masks/goron_mask.png" },
                    new TrackerItem { Id="romani_mask",        Name="Romani's Mask",        Type=TrackerItemType.Mask, IconPath="masks/romanis_mask.png" },
                    new TrackerItem { Id="circus_leader_mask", Name="Circus Leader's Mask", Type=TrackerItemType.Mask, IconPath="masks/circus_leaders_mask.png" },
                    new TrackerItem { Id="kafei_mask",         Name="Kafei's Mask",         Type=TrackerItemType.Mask, IconPath="masks/kafeis_mask.png" },
                    new TrackerItem { Id="couples_mask",       Name="Couple's Mask",        Type=TrackerItemType.Mask, IconPath="masks/couples_mask.png" },
                    new TrackerItem { Id="mask_of_truth",      Name="Mask of Truth",        Type=TrackerItemType.Mask, IconPath="masks/mask_of_truth.png" },
                    new TrackerItem { Id="zora_mask",          Name="Zora Mask",            Type=TrackerItemType.Mask, IconPath="masks/zora_mask.png" },
                    new TrackerItem { Id="kamaro_mask",        Name="Kamaro's Mask",        Type=TrackerItemType.Mask, IconPath="masks/kamaros_mask.png" },
                    new TrackerItem { Id="gibdo_mask",         Name="Gibdo Mask",           Type=TrackerItemType.Mask, IconPath="masks/gibdo_mask.png" },
                    new TrackerItem { Id="garo_mask",          Name="Garo's Mask",          Type=TrackerItemType.Mask, IconPath="masks/garos_mask.png" },
                    new TrackerItem { Id="captain_hat",        Name="Captain's Hat",        Type=TrackerItemType.Mask, IconPath="masks/captains_hat.png" },
                    new TrackerItem { Id="giants_mask",        Name="Giant's Mask",         Type=TrackerItemType.Mask, IconPath="masks/giants_mask.png" },
                    new TrackerItem { Id="fierce_deity",       Name="Fierce Deity's Mask",  Type=TrackerItemType.Mask, IconPath="masks/fierce_deitys_mask.png" },
                    // OoT exclusives
                    new TrackerItem { Id="oot_skull_mask",     Name="Skull Mask",           Type=TrackerItemType.Mask, IconPath="masks/skull_mask.png" },
                    new TrackerItem { Id="oot_spooky_mask",    Name="Spooky Mask",          Type=TrackerItemType.Mask, IconPath="masks/spooky_mask.png" },
                    new TrackerItem { Id="oot_gerudo_mask",    Name="Gerudo Mask",          Type=TrackerItemType.Mask, IconPath="masks/gerudo_mask.png" },
                });
                return items;
            }

            // Partial shared — only specific masks
            if (cfg.SharedMaskKeaton)
                items.Add(new() { Id="keaton_mask",   Name="Keaton Mask",   Type=TrackerItemType.Mask, IconPath="masks/keaton_mask.png" });
            if (cfg.SharedMaskBunny)
                items.Add(new() { Id="bunny_hood",    Name="Bunny Hood",    Type=TrackerItemType.Mask, IconPath="masks/bunny_hood.png" });
            if (cfg.SharedMaskGoron)
                items.Add(new() { Id="goron_mask",    Name="Goron Mask",    Type=TrackerItemType.Mask, IconPath="masks/goron_mask.png" });
            if (cfg.SharedMaskZora)
                items.Add(new() { Id="zora_mask",     Name="Zora Mask",     Type=TrackerItemType.Mask, IconPath="masks/zora_mask.png" });
            if (cfg.SharedMaskTruth)
                items.Add(new() { Id="mask_of_truth", Name="Mask of Truth", Type=TrackerItemType.Mask, IconPath="masks/mask_of_truth.png" });
            if (cfg.SharedMaskBlast)
                items.Add(new() { Id="blast_mask",    Name="Blast Mask",    Type=TrackerItemType.Mask, IconPath="masks/blast_mask.png" });
            if (cfg.SharedMaskStone)
                items.Add(new() { Id="stone_mask",    Name="Stone Mask",    Type=TrackerItemType.Mask, IconPath="masks/stone_mask.png" });

            return items;
        }

        // ─── Songs ────────────────────────────────────────────────────────────────

        public static List<TrackerItem> GetSongItems(TrackerConfig cfg)
        {
            // This method is no longer used — ocarinas are built into OoT/MM/Shared blocks
            return new List<TrackerItem>();
        }

        public static List<TrackerItem> GetOotSongItems(TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();
            bool n = cfg.SongsAsNotes;
            TrackerItem S(string id, string name, string icon, int notes=6) =>
                n ? MakeSongWithNotes(id,name,icon,notes) : new(){Id=id,Name=name,Type=TrackerItemType.Song,IconPath=icon};

            items.Add(S("oot_zelda_lullaby","Zelda's Lullaby",  "songs/zeldas_lullaby.png"));
            if (!cfg.SharedSongEpona)
                items.Add(S("oot_epona_song",   "Epona's Song",    "songs/eponas_song.png"));
            items.Add(S("oot_saria_song",   "Saria's Song",    "songs/sarias_song.png"));
            if (!cfg.SharedSongSun)
                items.Add(S("oot_sun_song",     "Sun's Song",      "songs/suns_song.png"));
            if (!cfg.SharedSongTime)
                items.Add(S("oot_song_of_time", "Song of Time",    "songs/song_of_time.png"));
            if (!cfg.SharedSongStorms)
                items.Add(S("oot_song_of_storms","Song of Storms", "songs/song_of_storms.png"));

            items.Add(S("oot_minuet",  "Minuet of Forest",  "songs/minuet_of_forest.png"));
            items.Add(S("oot_bolero",  "Bolero of Fire",    "songs/bolero_of_fire.png",   8));
            items.Add(S("oot_serenade","Serenade of Water", "songs/serenade_of_water.png",5));
            items.Add(S("oot_requiem", "Requiem of Spirit", "songs/requiem_of_spirit.png"));
            items.Add(S("oot_nocturne","Nocturne of Shadow","songs/nocturne_of_shadow.png",7));
            items.Add(S("oot_prelude", "Prelude of Light",  "songs/prelude_of_light.png"));

            if (!cfg.SharedOcarina)
                items.Add(new(){Id="oot_ocarina",Name="Ocarina (OoT)",Type=TrackerItemType.Song,MaxCount=2,
                    StepNames=new[]{"Fairy Ocarina","Ocarina of Time"},
                    StepIconPaths=new[]{"items/fairy_ocarina.png","items/ocarina_of_time.png"},IconPath="items/fairy_ocarina.png"});
            if (cfg.OotElegy && !cfg.SharedSongElegy)
                items.Add(S("oot_elegy",    "Elegy of Emptiness","songs/elegy_of_emptiness.png",7));
            if (!cfg.FreeScarecrowOot && !cfg.SharedScarecrow)
                items.Add(new(){Id="oot_scarecrow",Name="Scarecrow (OoT)",Type=TrackerItemType.Song,IconPath="songs/scarecrow_song_oot.png"});

            items.AddRange(GetOotOcarinaButtons(cfg));
            return items;
        }

        public static List<TrackerItem> GetMmSongItems(TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();
            bool n = cfg.SongsAsNotes;
            TrackerItem S(string id, string name, string icon, int notes=6) =>
                n ? MakeSongWithNotes(id,name,icon,notes) : new(){Id=id,Name=name,Type=TrackerItemType.Song,IconPath=icon};

            if (!cfg.SharedSongTime)
                items.Add(S("mm_song_of_time",  "Song of Time",    "songs/song_of_time.png"));
            items.Add(S("mm_song_of_healing","Song of Healing",  "songs/song_of_healing.png"));
            if (!cfg.SharedSongEpona)
                items.Add(S("mm_epona_song",    "Epona's Song",    "songs/eponas_song.png"));
            items.Add(S("mm_song_of_soaring","Song of Soaring",  "songs/song_of_soaring.png"));
            if (!cfg.SharedSongStorms)
                items.Add(S("mm_song_of_storms","Song of Storms",  "songs/song_of_storms.png"));

            items.Add(S("mm_sonata","Sonata of Awakening","songs/sonata_of_awakening.png",7));
            // Goron Lullaby
            if (n)
                items.Add(MakeSongWithNotes("mm_goron_lullaby","Goron Lullaby","songs/goron_lullaby.png",8,
                    cfg.ProgressiveGoronLullaby,"songs/lullaby_intro.png"));
            else if (cfg.ProgressiveGoronLullaby)
                items.Add(new(){Id="mm_goron_lullaby",Name="Goron Lullaby",Type=TrackerItemType.Song,MaxCount=2,
                    StepNames=new[]{"Goron Lullaby Intro","Goron Lullaby"},
                    StepIconPaths=new[]{"songs/lullaby_intro.png","songs/goron_lullaby.png"},IconPath="songs/lullaby_intro.png"});
            else
                items.Add(new(){Id="mm_goron_lullaby",Name="Goron Lullaby",Type=TrackerItemType.Song,MaxCount=1,IconPath="songs/goron_lullaby.png"});
            items.Add(S("mm_new_wave","New Wave Bossa Nova","songs/new_wave_bossa_nova.png",7));
            if (!cfg.SharedSongElegy)
                items.Add(S("mm_elegy","Elegy of Emptiness","songs/elegy_of_emptiness.png",7));
            items.Add(S("mm_oath","Oath to Order","songs/oath_to_order.png"));

            if (!cfg.SharedOcarina)
            {
                if (cfg.MmFairyOcarina)
                    items.Add(new(){Id="mm_ocarina",Name="Ocarina (MM)",Type=TrackerItemType.Song,MaxCount=2,
                        StepNames=new[]{"Fairy Ocarina","Ocarina of Time"},
                        StepIconPaths=new[]{"items/fairy_ocarina.png","items/ocarina_of_time.png"},IconPath="items/fairy_ocarina.png"});
                else
                    items.Add(new(){Id="mm_ocarina",Name="Ocarina (MM)",Type=TrackerItemType.Song,MaxCount=1,IconPath="items/ocarina_of_time.png"});
            }
            if (cfg.MmSunSong && !cfg.SharedSongSun)
                items.Add(S("mm_sun_song","Sun's Song (MM)","songs/suns_song.png"));
            if (!cfg.FreeScarecrowMm && !cfg.SharedScarecrow)
                items.Add(new(){Id="mm_scarecrow",Name="Scarecrow (MM)",Type=TrackerItemType.Song,IconPath="songs/scarecrow_song_mm.png"});

            items.AddRange(GetMmOcarinaButtons(cfg));
            return items;
        }

        public static List<TrackerItem> GetSharedSongItems(TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();
            bool n = cfg.SongsAsNotes;
            TrackerItem S(string id, string name, string icon, int notes=6) =>
                n ? MakeSongWithNotes(id,name,icon,notes) : new(){Id=id,Name=name,Type=TrackerItemType.Song,IconPath=icon};

            // Ocarina shared
            if (cfg.SharedOcarina)
                items.Add(new(){Id="sh_ocarina",Name="Ocarina",Type=TrackerItemType.Song,MaxCount=2,
                    StepNames=new[]{"Fairy Ocarina","Ocarina of Time"},
                    StepIconPaths=new[]{"items/fairy_ocarina.png","items/ocarina_of_time.png"},IconPath="items/fairy_ocarina.png"});

            if (cfg.SharedSongEpona)
                items.Add(S("sh_epona_song",    "Epona's Song",       "songs/eponas_song.png"));
            if (cfg.SharedSongSun)
                items.Add(S("sh_sun_song",      "Sun's Song",         "songs/suns_song.png"));
            if (cfg.SharedSongTime)
                items.Add(S("sh_song_of_time",  "Song of Time",       "songs/song_of_time.png"));
            if (cfg.SharedSongStorms)
                items.Add(S("sh_song_of_storms","Song of Storms",     "songs/song_of_storms.png"));
            if (cfg.SharedSongElegy)
                items.Add(S("sh_elegy",         "Elegy of Emptiness", "songs/elegy_of_emptiness.png",7));
            if (cfg.SharedScarecrow && (!cfg.FreeScarecrowOot || !cfg.FreeScarecrowMm))
                items.Add(new(){Id="sh_scarecrow",Name="Scarecrow's Song",Type=TrackerItemType.Song,IconPath="songs/scarecrow_song.png"});

            items.AddRange(GetSharedOcarinaButtons(cfg));
            return items;
        }

        // ─── Ocarina Buttons ──────────────────────────────────────────────────────

        public static List<TrackerItem> GetOotOcarinaButtons(TrackerConfig cfg)
        {
            if (!cfg.OotOcarinaButtons && !cfg.SharedOcarinaButtons) return new();
            if (cfg.SharedOcarinaButtons) return new(); // will be in Shared
            return MakeOcarinaButtons("oot");
        }

        public static List<TrackerItem> GetMmOcarinaButtons(TrackerConfig cfg)
        {
            if (!cfg.MmOcarinaButtons && !cfg.SharedOcarinaButtons) return new();
            if (cfg.SharedOcarinaButtons) return new(); // will be in Shared
            return MakeOcarinaButtons("mm");
        }

        public static List<TrackerItem> GetSharedOcarinaButtons(TrackerConfig cfg)
        {
            if (!cfg.SharedOcarinaButtons) return new();
            return MakeOcarinaButtons("sh");
        }

        private static List<TrackerItem> MakeOcarinaButtons(string prefix) => new()
        {
            new() { Id=$"{prefix}_btn_a",      Name="A Button",       Type=TrackerItemType.Item, MaxCount=1, IconPath="ocarina_buttons/a.png" },
            new() { Id=$"{prefix}_btn_cup",    Name="C-Up Button",    Type=TrackerItemType.Item, MaxCount=1, IconPath="ocarina_buttons/c-up.png" },
            new() { Id=$"{prefix}_btn_cdown",  Name="C-Down Button",  Type=TrackerItemType.Item, MaxCount=1, IconPath="ocarina_buttons/c-down.png" },
            new() { Id=$"{prefix}_btn_cleft",  Name="C-Left Button",  Type=TrackerItemType.Item, MaxCount=1, IconPath="ocarina_buttons/c-left.png" },
            new() { Id=$"{prefix}_btn_cright", Name="C-Right Button", Type=TrackerItemType.Item, MaxCount=1, IconPath="ocarina_buttons/c-right.png" },
        };

        // ─── Song Notes ───────────────────────────────────────────────────────────
        // Creates a song item with notes. When all notes are collected — lights up.
        // For Goron Lullaby with progression: 6/8 = Intro, 8/8 = full.

        private static TrackerItem MakeSongWithNotes(string id, string name, string iconPath, int noteCount,
            bool progressiveGoron = false, string? introIconPath = null)
        {
            if (progressiveGoron && introIconPath != null)
            {
                return new()
                {
                    Id = id, Name = name, Type = TrackerItemType.Song,
                    MaxCount = noteCount,
                    IconPath = introIconPath,
                    StepIconPaths = BuildGoronStepIcons(noteCount, iconPath, introIconPath),
                    StepLabels = Enumerable.Range(1, noteCount).Select(i => $"{i}/{noteCount}").ToArray(),
                    CollectedWhenFull = false,
                    PartialCollectedAt = 6  // lights up Intro at 6/8
                };
            }
            // Regular song: icon always visible, lights up when all notes collected
            return new()
            {
                Id = id, Name = name, Type = TrackerItemType.Song,
                MaxCount = noteCount,
                IconPath = iconPath,
                StepLabels = Enumerable.Range(1, noteCount).Select(i => $"{i}/{noteCount}").ToArray(),
                CollectedWhenFull = true // lights up only at MaxCount
            };
        }

        private static string[] BuildGoronStepIcons(int count, string finalIcon, string introIcon)
        {
            // count = 8: steps 1-5 = intro (dimmed), 6-7 = intro (lit), 8 = full
            var icons = new string[count];
            for (int i = 0; i < count; i++)
            {
                if (i < 7) icons[i] = introIcon;
                else       icons[i] = finalIcon;
            }
            return icons;
        }

        // ─── Clocks ───────────────────────────────────────────────────────────────

        public static List<TrackerItem> GetClocks(TrackerConfig cfg, SpoilerLog? log = null)
        {
            if (!cfg.ClocksEnabled) return new();

            // Determine starting Clock from Starting Items
            int startClock = 0; // 0 = none, 1 = Day1, 2 = Night1, 3 = Day2, 4 = Night2, 5 = Day3, 6 = Night3
            if (log != null)
            {
                foreach (var kv in log.StartingItems)
                {
                    if (kv.Key.StartsWith("Clock"))
                    {
                        startClock = kv.Key switch
                        {
                            "Clock (Day 1)"   => 1, "Clock (Night 1)" => 2,
                            "Clock (Day 2)"   => 3, "Clock (Night 2)" => 4,
                            "Clock (Day 3)"   => 5, "Clock (Night 3)" => 6,
                            _ => 0
                        };
                        break;
                    }
                }
            }

            var clocks = new[]
            {
                ("clock_day1",   "Day 1",   "clocks/1st_day.png"),
                ("clock_night1", "Night 1", "clocks/1st_night.png"),
                ("clock_day2",   "Day 2",   "clocks/2nd_day.png"),
                ("clock_night2", "Night 2", "clocks/2nd_night.png"),
                ("clock_day3",   "Day 3",   "clocks/3rd_day.png"),
                ("clock_night3", "Night 3", "clocks/3rd_night.png"),
            };

            if (cfg.ProgressiveClocks == "separate")
            {
                // Each Clock separate, starting one marked as AlwaysCollected
                var items = new List<TrackerItem>();
                for (int i = 0; i < clocks.Length; i++)
                {
                    var (id, name, icon) = clocks[i];
                    bool isStart = (i + 1) == startClock;
                    items.Add(new() { Id=id, Name=name, Type=TrackerItemType.Item, MaxCount=1,
                        IconPath=icon, AlwaysCollected=isStart });
                }
                return items;
            }
            else
            {
                // Progression: one item with 6 steps
                var stepIcons = clocks.Select(c => c.Item3).ToArray();
                var stepNames = clocks.Select(c => c.Item2).ToArray();
                int initCount = startClock > 0 ? startClock : 0;
                return new()
                {
                    new() { Id="clock_progressive", Name="Clock", Type=TrackerItemType.Item,
                        MaxCount=6, StepNames=stepNames, StepIconPaths=stepIcons,
                        IconPath=stepIcons[0], CurrentCount=initCount, MinCount=initCount }
                };
            }
        }

        // ─── Owl Statues ──────────────────────────────────────────────────────────

        public static List<TrackerItem> GetOwlStatues(TrackerConfig cfg)
        {
            if (!cfg.OwlShuffle) return new();
            return new()
            {
                new() { Id="owl_clock_town",  Name="Clock Town",     Type=TrackerItemType.Item, MaxCount=1, IconPath="owls/clock_town.png" },
                new() { Id="owl_milk_road",   Name="Milk Road",      Type=TrackerItemType.Item, MaxCount=1, IconPath="owls/milk_road.png" },
                new() { Id="owl_woodfall",    Name="Woodfall",       Type=TrackerItemType.Item, MaxCount=1, IconPath="owls/woodfall.png" },
                new() { Id="owl_swamp",       Name="Southern Swamp", Type=TrackerItemType.Item, MaxCount=1, IconPath="owls/southern_swamp.png" },
                new() { Id="owl_mountain",    Name="Mountain Village",Type=TrackerItemType.Item, MaxCount=1, IconPath="owls/mountain_village.png" },
                new() { Id="owl_snowhead",    Name="Snowhead",       Type=TrackerItemType.Item, MaxCount=1, IconPath="owls/snowhead.png" },
                new() { Id="owl_great_bay",   Name="Great Bay Coast",Type=TrackerItemType.Item, MaxCount=1, IconPath="owls/great_bay_coast.png" },
                new() { Id="owl_zora_cape",   Name="Zora Cape",      Type=TrackerItemType.Item, MaxCount=1, IconPath="owls/zora_cape.png" },
                new() { Id="owl_ikana",       Name="Ikana Canyon",   Type=TrackerItemType.Item, MaxCount=1, IconPath="owls/ikana_canyon.png" },
                new() { Id="owl_stone_tower", Name="Stone Tower",    Type=TrackerItemType.Item, MaxCount=1, IconPath="owls/stone_tower.png" },
            };
        }

        // ─── Tingle Maps ──────────────────────────────────────────────────────────

        public static List<TrackerItem> GetTingleMaps(TrackerConfig cfg)
        {
            if (!cfg.TingleMaps) return new();
            return new()
            {
                new() { Id="tingle_clock_town",  Name="Tingle Map: Clock Town",   Type=TrackerItemType.Item, MaxCount=1, IconPath="equipment/maps/clock_town.png" },
                new() { Id="tingle_woodfall",    Name="Tingle Map: Woodfall",     Type=TrackerItemType.Item, MaxCount=1, IconPath="equipment/maps/woodfall.png" },
                new() { Id="tingle_snowhead",    Name="Tingle Map: Snowhead",     Type=TrackerItemType.Item, MaxCount=1, IconPath="equipment/maps/snowhead.png" },
                new() { Id="tingle_great_bay",   Name="Tingle Map: Great Bay",    Type=TrackerItemType.Item, MaxCount=1, IconPath="equipment/maps/great_bay.png" },
                new() { Id="tingle_stone_tower", Name="Tingle Map: Stone Tower",  Type=TrackerItemType.Item, MaxCount=1, IconPath="equipment/maps/stone_tower.png" },
                new() { Id="tingle_romani_ranch",Name="Tingle Map: Romani Ranch", Type=TrackerItemType.Item, MaxCount=1, IconPath="equipment/maps/romani_ranch.png" },
            };
        }

        // ─── Bottles ──────────────────────────────────────────────────────────────

        // Bottle with progression: Letter → Empty (OoT) or Gold Dust → Empty (MM)
        private static TrackerItem MakeBottle(string id, string name, string firstIcon, string emptyIcon) =>
            new() { Id=id, Name=name, Type=TrackerItemType.Item, MaxCount=2,
                StepNames=new[]{name,"Empty Bottle"},
                StepIconPaths=new[]{firstIcon, emptyIcon}, IconPath=firstIcon };

        private static TrackerItem MakeEmptyBottle(string id, string name) =>
            new() { Id=id, Name=name, Type=TrackerItemType.Item, MaxCount=1, IconPath="items/bottles/empty.png" };

        public static List<TrackerItem> GetOotBottles(TrackerConfig cfg)
        {
            if (cfg.SharedBottles) return new();
            return new()
            {
                MakeBottle("oot_bottle_letter", "Bottle (Letter)", "items/bottles/rutos_letter.png", "items/bottles/empty.png"),
                MakeEmptyBottle("oot_bottle_2", "Bottle 2"),
                MakeEmptyBottle("oot_bottle_3", "Bottle 3"),
                MakeEmptyBottle("oot_bottle_4", "Bottle 4"),
            };
        }

        public static List<TrackerItem> GetMmBottles(TrackerConfig cfg)
        {
            if (cfg.SharedBottles) return new();
            return new()
            {
                MakeBottle("mm_bottle_gold_dust", "Bottle (Gold Dust)", "items/bottles/gold_dust.png", "items/bottles/empty.png"),
                MakeEmptyBottle("mm_bottle_2", "Bottle 2"),
                MakeEmptyBottle("mm_bottle_3", "Bottle 3"),
                MakeEmptyBottle("mm_bottle_4", "Bottle 4"),
                MakeEmptyBottle("mm_bottle_5", "Bottle 5"),
                MakeEmptyBottle("mm_bottle_6", "Bottle 6"),
            };
        }

        public static List<TrackerItem> GetSharedBottles(TrackerConfig cfg)
        {
            if (!cfg.SharedBottles) return new();
            return new()
            {
                MakeBottle("sh_bottle_letter",    "Bottle (Letter)",    "items/bottles/rutos_letter.png", "items/bottles/empty.png"),
                MakeBottle("sh_bottle_gold_dust", "Bottle (Gold Dust)", "items/bottles/gold_dust.png",   "items/bottles/empty.png"),
                MakeEmptyBottle("sh_bottle_3", "Bottle 3"),
                MakeEmptyBottle("sh_bottle_4", "Bottle 4"),
                MakeEmptyBottle("sh_bottle_5", "Bottle 5"),
                MakeEmptyBottle("sh_bottle_6", "Bottle 6"),
            };
        }

        // ─── Dungeons ─────────────────────────────────────────────────────────────

        // All possible rewards (index 0 = unknown)
        private static readonly string[] AllRewardIcons = {
            "dungeons/rewards/unknown.png",
            "dungeons/rewards/kokiris_emerald.png",
            "dungeons/rewards/gorons_ruby.png",
            "dungeons/rewards/zoras_sapphire.png",
            "dungeons/rewards/light_medallion.png",
            "dungeons/rewards/forest_medallion.png",
            "dungeons/rewards/fire_medallion.png",
            "dungeons/rewards/water_medallion.png",
            "dungeons/rewards/shadow_medallion.png",
            "dungeons/rewards/spirit_medallion.png",
            "dungeons/rewards/odolwas_remains.png",
            "dungeons/rewards/gohts_remains.png",
            "dungeons/rewards/gyorgs_remains.png",
            "dungeons/rewards/twinmolds_remains.png",
        };
        private static readonly string[] AllRewardNames = {
            "?",
            "Kokiri's Emerald", "Goron's Ruby", "Zora's Sapphire",
            "Light Medallion",
            "Forest Medallion", "Fire Medallion", "Water Medallion",
            "Shadow Medallion", "Spirit Medallion",
            "Odolwa's Remains", "Goht's Remains", "Gyorg's Remains", "Twinmold's Remains",
        };

        private static TrackerItem MakeDungeon(string id, string name, string labelIcon)
        {
            // Only sub-dungeons without rewards
            bool hasReward = id is not ("ice_cavern" or "botw" or "gtg" or "ganons_castle"
                                     or "thieves_hideout" or "tcg");
            return new()
            {
                Id = id, Name = name, Type = TrackerItemType.Dungeon,
                IconPath = labelIcon,
                RewardIcons = hasReward ? AllRewardIcons : null,
                RewardNames = hasReward ? AllRewardNames : null,
            };
        }

        public static List<TrackerItem> GetDungeonItems() => new()
        {
            MakeDungeon("woodfall",      "Woodfall Temple",    "dungeons/labels/woodfall_temple.png"),
            MakeDungeon("snowhead",      "Snowhead Temple",    "dungeons/labels/snowhead_temple.png"),
            MakeDungeon("great_bay",     "Great Bay Temple",   "dungeons/labels/great_bay_temple.png"),
            MakeDungeon("stone_tower",   "Stone Tower Temple", "dungeons/labels/stone_tower_temple.png"),
            MakeDungeon("deku_tree",     "Deku Tree",          "dungeons/labels/deku_tree.png"),
            MakeDungeon("dodongo",       "Dodongo's Cavern",   "dungeons/labels/dodongos_cavern.png"),
            MakeDungeon("jabu",          "Inside Jabu-Jabu",   "dungeons/labels/jabu-jabu.png"),
            MakeDungeon("forest_temple", "Forest Temple",      "dungeons/labels/forest_temple.png"),
            MakeDungeon("fire_temple",   "Fire Temple",        "dungeons/labels/fire_temple.png"),
            MakeDungeon("water_temple",  "Water Temple",       "dungeons/labels/water_temple.png"),
            MakeDungeon("shadow_temple", "Shadow Temple",      "dungeons/labels/shadow_temple.png"),
            MakeDungeon("spirit_temple", "Spirit Temple",      "dungeons/labels/spirit_temple.png"),
            MakeDungeon("free_reward",   "Free Reward",        "dungeons/labels/free.png"),
        };

        // Individual dungeons for separate blocks
        public static readonly (string id, string name, string label)[] DungeonList =
        {
            ("woodfall",      "Woodfall Temple",     "dungeons/labels/woodfall_temple.png"),
            ("snowhead",      "Snowhead Temple",     "dungeons/labels/snowhead_temple.png"),
            ("great_bay",     "Great Bay Temple",    "dungeons/labels/great_bay_temple.png"),
            ("stone_tower",   "Stone Tower Temple",  "dungeons/labels/stone_tower_temple.png"),
            ("deku_tree",     "Deku Tree",           "dungeons/labels/deku_tree.png"),
            ("dodongo",       "Dodongo's Cavern",    "dungeons/labels/dodongos_cavern.png"),
            ("jabu",          "Inside Jabu-Jabu",    "dungeons/labels/jabu-jabu.png"),
            ("forest_temple", "Forest Temple",       "dungeons/labels/forest_temple.png"),
            ("fire_temple",   "Fire Temple",         "dungeons/labels/fire_temple.png"),
            ("water_temple",  "Water Temple",        "dungeons/labels/water_temple.png"),
            ("shadow_temple", "Shadow Temple",       "dungeons/labels/shadow_temple.png"),
            ("spirit_temple", "Spirit Temple",       "dungeons/labels/spirit_temple.png"),
            ("free_reward",   "Free Reward",         "dungeons/labels/free.png"),
        };

        // Sub-dungeons (always visible)
        public static readonly (string id, string name, string label)[] SubDungeonList =
        {
            ("ice_cavern",  "Ice Cavern",              "dungeons/labels/ice_cavern.png"),
            ("botw",        "Bottom of the Well",      "dungeons/labels/bottom_of_the_well.png"),
            ("gtg",         "Gerudo Training Ground",  "dungeons/labels/gerudo_training_ground.png"),
            ("ganons_castle","Ganon's Castle",          "dungeons/labels/ganons_castle.png"),
        };

        private static int SmallKeyCount(string id, bool keysanity) => id switch
        {
            "woodfall"        => 1,
            "snowhead"        => 3,
            "great_bay"       => 1,
            "stone_tower"     => 4,
            "forest_temple"   => 5,
            "fire_temple"     => keysanity ? 8 : 7,
            "water_temple"    => 5,
            "shadow_temple"   => 5,
            "spirit_temple"   => 5,
            "botw"            => 3,
            "gtg"             => 9,
            "ganons_castle"   => 2,
            "thieves_hideout" => 4,
            "tcg"             => 6,
            _ => 0
        };

        private static int SmallKeyCountMq(string id) => id switch
        {
            "forest_temple"   => 6,
            "fire_temple"     => 5,  // MQ Fire has no keysanity
            "water_temple"    => 2,
            "shadow_temple"   => 6,
            "spirit_temple"   => 7,
            "botw"            => 2,
            "gtg"             => 3,
            "ganons_castle"   => 3,
            _ => 0  // Deku, Dodongo, Jabu, Ice — no keys in MQ
        };

        private static bool HasBossKey(string id) =>
            id is "woodfall" or "snowhead" or "great_bay" or "stone_tower"
               or "forest_temple" or "fire_temple" or "water_temple"
               or "shadow_temple" or "spirit_temple" or "ganons_castle";

        // Silver rupee packs by dungeon (pack id, name)
        private static (string packId, string packName)[] GetSilverRupeePacks(string id) => id switch
        {
            "shadow_temple"  => new[] { ("scythe","Scythe"), ("pit","Pit"), ("spikes","Spikes") },
            "spirit_temple"  => new[] { ("child","Child"), ("sun","Sun"), ("boulders","Boulders") },
            "ice_cavern"     => new[] { ("scythe","Scythe"), ("block","Block") },
            "botw"           => new[] { ("basement","Basement") },
            "gtg"            => new[] { ("slopes","Slopes"), ("lava","Lava"), ("water","Water") },
            "ganons_castle"  => new[] { ("spirit","Spirit"), ("light","Light"), ("fire","Fire"), ("forest","Forest") },
            _ => new (string, string)[0]
        };

        // Silver rupee pack size (Vanilla = 5, MQ may differ)
        private static int GetSrPackSize(string dungId, string packId, bool isMq)
        {
            if (!isMq) return 5; // Vanilla always 5
            // MQ packs use mq_ prefix
            return (dungId, packId) switch
            {
                ("dodongo",       "mq_staircase") => 5,
                ("shadow_temple", "mq_scythe")    => 5,
                ("shadow_temple", "mq_blades")    => 10,
                ("shadow_temple", "mq_pit")       => 5,
                ("shadow_temple", "mq_spikes")    => 10,
                ("spirit_temple", "mq_lobby")     => 5,
                ("spirit_temple", "mq_adult")     => 5,
                ("gtg",           "mq_slopes")    => 5,
                ("gtg",           "mq_lava")      => 6,
                ("gtg",           "mq_water")     => 3,
                ("ganons_castle", "mq_fire")      => 5,
                ("ganons_castle", "mq_shadow")    => 5,
                ("ganons_castle", "mq_water")     => 5,
                _ => 5
            };
        }

        private static (string packId, string packName)[] GetSilverRupeePacks(string id, bool isMq) => isMq
            ? id switch
            {
                "dodongo"        => new[] { ("mq_staircase","Staircase") },
                "shadow_temple"  => new[] { ("mq_scythe","Scythe"), ("mq_blades","Blades"), ("mq_pit","Pit"), ("mq_spikes","Spikes") },
                "spirit_temple"  => new[] { ("mq_lobby","Lobby"), ("mq_adult","Adult") },
                "gtg"            => new[] { ("mq_slopes","Slopes"), ("mq_lava","Lava"), ("mq_water","Water") },
                "ganons_castle"  => new[] { ("mq_fire","Fire"), ("mq_shadow","Shadow"), ("mq_water","Water") },
                _ => new (string, string)[0]
            }
            : GetSilverRupeePacks(id);

        public static List<TrackerItem> GetDungeonSingle(string id, string name, string label, TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();
            bool isMm  = id is "woodfall" or "snowhead" or "great_bay" or "stone_tower";
            bool isMq  = !isMm && cfg.IsMq(id);
            // Only dungeons with rewards get reward icon first
            bool hasReward = id is not ("ice_cavern" or "botw" or "gtg" or "ganons_castle"
                                     or "thieves_hideout" or "tcg");
            if (hasReward)
                items.Add(MakeDungeon(id, name, label));
            if (id != "free_reward")
            {
                if (cfg.MapsCompasses && id is not ("thieves_hideout" or "tcg" or "gtg" or "ganons_castle"))
                {
                    items.Add(new() { Id=$"{id}_map",     Name="Map",     Type=TrackerItemType.Dungeon, MaxCount=1, IconPath="dungeons/map.png" });
                    items.Add(new() { Id=$"{id}_compass", Name="Compass", Type=TrackerItemType.Dungeon, MaxCount=1, IconPath="dungeons/compass.png" });
                }
            }
            bool showSK = id == "thieves_hideout" ? cfg.SmallKeysHideout
                        : id == "tcg"             ? cfg.SmallKeysTcg
                        : isMm                    ? cfg.SmallKeysMm
                        :                           cfg.SmallKeysOot;
            int skCount = isMq ? SmallKeyCountMq(id) : SmallKeyCount(id, cfg.Keysanity);
            if (showSK && skCount > 0)
            {
                bool hasRing = cfg.HasKeyRing(id);
                if (hasRing)
                    items.Add(new() { Id=$"{id}_sk", Name=$"Key Ring (max {skCount})", Type=TrackerItemType.Dungeon,
                        MaxCount=1, IconPath="dungeons/small_key.png" });
                else
                    items.Add(new() { Id=$"{id}_sk", Name="Small Key", Type=TrackerItemType.Dungeon,
                        MaxCount=skCount, IconPath="dungeons/small_key.png",
                        StepLabels=Enumerable.Range(1,skCount).Select(i=>i.ToString()).ToArray() });
            }
            // Boss Key
            bool showBK = id == "ganons_castle"
                ? cfg.GanonBossKey != "vanilla"
                : isMm ? cfg.BossKeysMm : cfg.BossKeysOot;
            if (showBK && HasBossKey(id))
            {
                if (id == "ganons_castle" && cfg.GanonBossKey == "custom")
                    items.Add(new() { Id="ganons_castle_bk", Name="Ganon's Boss Key", Type=TrackerItemType.Dungeon,
                        MaxCount=cfg.GanonBkRequired > 0 ? cfg.GanonBkRequired : 9,
                        IconPath="dungeons/boss_key.png",
                        CollectedWhenFull=false, IsAutoKey=true,
                        AutoKeyThreshold=cfg.GanonBkRequired > 0 ? cfg.GanonBkRequired : 9 });
                else
                    items.Add(new() { Id=$"{id}_bk", Name="Boss Key", Type=TrackerItemType.Dungeon,
                        MaxCount=1, IconPath="dungeons/boss_key.png" });
            }
            // Silver Rupees
            if (cfg.SilverRupees)
            {
                var srPacks = GetSilverRupeePacks(id, isMq);
                foreach (var (packId, packName) in srPacks)
                {
                    bool hasPouch = cfg.HasSrPouch(id, packId);
                    // MQ has different pack sizes
                    int srMax = GetSrPackSize(id, packId, isMq);
                    if (hasPouch)
                        items.Add(new() { Id=$"{id}_sr_{packId}", Name=$"SR Pouch: {packName}", Type=TrackerItemType.Dungeon,
                            MaxCount=1, IconPath="dungeons/silver_rupee.png" });
                    else
                        items.Add(new() { Id=$"{id}_sr_{packId}", Name=$"SR: {packName}", Type=TrackerItemType.Dungeon,
                            MaxCount=srMax, IconPath="dungeons/silver_rupee.png",
                            StepLabels=Enumerable.Range(1,srMax).Select(i=>i.ToString()).ToArray() });
                }
            }
            return items;
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private static (int max, string[] steps, int minCount) BuildWalletProgression(TrackerConfig cfg)
        {
            var steps = new System.Collections.Generic.List<string>();
            // Always add Child to steps (for correct labels), but MinCount=1 prevents removing it
            steps.Add("Child's Wallet");
            steps.Add("Adult's Wallet");
            steps.Add("Giant's Wallet");
            if (cfg.ColossalWallet)   steps.Add("Colossal Wallet");
            if (cfg.BottomlessWallet) steps.Add("Bottomless Wallet");
            // If Child Wallet disabled — minimum 1 (can't remove Adult Wallet)
            int minCount = cfg.ChildWallet ? 0 : 1;
            return (steps.Count, steps.ToArray(), minCount);
        }

        // ─── Items OoT (6×3) ─────────────────────────────────────────────────────

        public static List<TrackerItem> GetOotItems(TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();

            // Row 1: Bow - Fire - Ice - Light - Slingshot - Boomerang
            if (!cfg.SharedBows)
                items.Add(new() { Id="oot_bow", Name="Bow", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Fairy Bow (30)","Bow (40)","Bow (50)"},
                    StepLabels=new[]{"30","40","50"}, IconPath="items/fairy_bow.png" });
            if (!cfg.SharedFireArrows)
                items.Add(new() { Id="oot_fire_arrows",  Name="Fire Arrows",  Type=TrackerItemType.Item, MaxCount=1, IconPath="items/fire_arrows.png" });
            if (!cfg.SharedIceArrows)
                items.Add(new() { Id="oot_ice_arrows",   Name="Ice Arrows",   Type=TrackerItemType.Item, MaxCount=1, IconPath="items/ice_arrows.png" });
            if (!cfg.SharedLightArrows)
                items.Add(new() { Id="oot_light_arrows", Name="Light Arrows", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/light_arrows.png" });
            // Slingshot and Boomerang — OoT exclusives, go to Shared if all items shared
            if (!cfg.AllItemsShared)
            {
                items.Add(new() { Id="oot_slingshot", Name="Slingshot", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Slingshot (30)","Slingshot (40)","Slingshot (50)"},
                    StepLabels=new[]{"30","40","50"}, IconPath="items/fairy_slingshot.png" });
                items.Add(new() { Id="oot_boomerang", Name="Boomerang", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/boomerang.png" });
            }

            // Row 2: Din - Farore - Nayru - Bomb - Bombchu - Hammer
            if (!cfg.SharedSpellFire)
                items.Add(new() { Id="oot_dins_fire",    Name="Din's Fire",    Type=TrackerItemType.Item, MaxCount=1, IconPath="items/dins_fire.png" });
            if (!cfg.SharedSpellWind)
                items.Add(new() { Id="oot_farores_wind", Name="Farore's Wind", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/farores_wind.png" });
            if (!cfg.SharedSpellLove)
                items.Add(new() { Id="oot_nayrus_love",  Name="Nayru's Love",  Type=TrackerItemType.Item, MaxCount=1, IconPath="items/nayrus_love.png" });
            if (!cfg.SharedBombBags)
                items.Add(new() { Id="oot_bomb_bag", Name="Bomb Bag", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Bomb Bag (20)","Bomb Bag (30)","Bomb Bag (40)"},
                    StepLabels=new[]{"20","30","40"}, IconPath="items/bomb.png" });
            if (!cfg.SharedBombchu)
                items.Add(MakeBombchu("oot_bombchu", cfg.BombchuBehaviorOot, cfg.SharedBombBags ? "sh_bomb_bag" : "oot_bomb_bag"));
            if (!cfg.SharedHammer)
                items.Add(new() { Id="oot_hammer", Name="Megaton Hammer", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/megaton_hammer.png" });

            // Row 3: Stick - Nut - Bean(OoT) - Lens - Hookshot
            if (!cfg.SharedNutsSticks)
            {
                items.Add(new() { Id="oot_deku_stick", Name="Deku Stick", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Deku Stick (10)","Deku Stick (20)","Deku Stick (30)"},
                    StepLabels=new[]{"10","20","30"}, IconPath="items/deku_stick.png" });
                items.Add(new() { Id="oot_deku_nut", Name="Deku Nut", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Deku Nut (20)","Deku Nut (30)","Deku Nut (40)"},
                    StepLabels=new[]{"20","30","40"}, IconPath="items/deku_nut.png" });
            }
            // Beans(OoT) — remove if preplanted
            if (!cfg.AllItemsShared && !cfg.OotPreplantedBeans)
                items.Add(new() { Id="oot_beans", Name="Magic Beans (OoT)", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/magic_beans_oot.png" });
            if (!cfg.SharedLens)
                items.Add(new() { Id="oot_lens", Name="Lens of Truth", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/lens_of_truth.png" });
            if (!cfg.SharedHookshot)
                items.Add(new() { Id="oot_hookshot", Name="Hookshot", Type=TrackerItemType.Item, MaxCount=2,
                    StepNames=new[]{"Hookshot","Longshot"}, IconPath="items/short_hookshot.png",
                    StepIconPaths=new[]{"items/short_hookshot.png","items/longshot.png"} });

            return items;
        }

        // ─── Items MM (5×3) ──────────────────────────────────────────────────────

        public static List<TrackerItem> GetMmItems(TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();

            // Row 1: Bow - Fire - Ice - Light - Fairy Sword
            if (!cfg.SharedBows)
                items.Add(new() { Id="mm_bow", Name="Bow (MM)", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Hero's Bow (30)","Bow (40)","Bow (50)"},
                    StepLabels=new[]{"30","40","50"}, IconPath="items/heros_bow.png" });
            if (!cfg.SharedFireArrows)
                items.Add(new() { Id="mm_fire_arrows",  Name="Fire Arrows",  Type=TrackerItemType.Item, MaxCount=1, IconPath="items/fire_arrows.png" });
            if (!cfg.SharedIceArrows)
                items.Add(new() { Id="mm_ice_arrows",   Name="Ice Arrows",   Type=TrackerItemType.Item, MaxCount=1, IconPath="items/ice_arrows.png" });
            if (!cfg.SharedLightArrows)
                items.Add(new() { Id="mm_light_arrows", Name="Light Arrows", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/light_arrows.png" });
            // Fairy Sword — MM exclusive, goes to Shared if all items shared
            if (!cfg.AllItemsShared)
                items.Add(new() { Id="mm_fairy_sword", Name="Fairy Sword", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/great_fairys_sword.png" });

            // Row 2: Stick - Nut - Bomb - Bombchu - Hookshot
            if (!cfg.SharedNutsSticks && cfg.MmSticksNuts)
            {
                items.Add(new() { Id="mm_deku_stick", Name="Deku Stick (MM)", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Deku Stick (10)","Deku Stick (20)","Deku Stick (30)"},
                    StepLabels=new[]{"10","20","30"}, IconPath="items/deku_stick.png" });
                items.Add(new() { Id="mm_deku_nut", Name="Deku Nut (MM)", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Deku Nut (20)","Deku Nut (30)","Deku Nut (40)"},
                    StepLabels=new[]{"20","30","40"}, IconPath="items/deku_nut.png" });
            }
            if (!cfg.SharedBombBags)
                items.Add(new() { Id="mm_bomb_bag", Name="Bomb Bag (MM)", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Bomb Bag (20)","Bomb Bag (30)","Bomb Bag (40)"},
                    StepLabels=new[]{"20","30","40"}, IconPath="items/bomb.png" });
            if (!cfg.SharedBombchu)
                items.Add(MakeBombchu("mm_bombchu", cfg.BombchuBehaviorMm, cfg.SharedBombBags ? "sh_bomb_bag" : "mm_bomb_bag"));
            if (!cfg.SharedHookshot)
            {
                if (cfg.MmShortHookshot)
                    items.Add(new() { Id="mm_hookshot", Name="Hookshot (MM)", Type=TrackerItemType.Item, MaxCount=2,
                        StepNames=new[]{"Short Hookshot","Hookshot"},
                        StepIconPaths=new[]{"items/short_hookshot.png","items/mm_hookshot.png"}, IconPath="items/short_hookshot.png" });
                else
                    items.Add(new() { Id="mm_hookshot", Name="Hookshot (MM)", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/mm_hookshot.png" });
            }

            // Row 3: Keg - Pictograph - Lens - Beans(MM) — MM exclusives, go to Shared if all items shared
            if (!cfg.AllItemsShared)
            {
                items.Add(new() { Id="mm_powder_keg",  Name="Powder Keg",       Type=TrackerItemType.Item, MaxCount=1, IconPath="items/powder_keg.png" });
                items.Add(new() { Id="mm_pictograph",  Name="Pictograph Box",   Type=TrackerItemType.Item, MaxCount=1, IconPath="items/pictograph_box.png" });
            }
            if (!cfg.SharedLens)
                items.Add(new() { Id="mm_lens",    Name="Lens of Truth",    Type=TrackerItemType.Item, MaxCount=1, IconPath="items/lens_of_truth.png" });
            if (!cfg.AllItemsShared)
            {
                if (cfg.OotPreplantedBeans)
                    // If OoT beans already planted — show MM beans as regular item (without labels)
                    items.Add(new() { Id="mm_beans", Name="Magic Beans", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/magic_beans.png" });
                else
                    items.Add(new() { Id="mm_beans", Name="Magic Beans (MM)", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/magic_beans_mm.png" });
            }
            // MM spells — if enabled and not shared
            if (cfg.MmSpellFire && !cfg.SharedSpellFire)
                items.Add(new() { Id="mm_dins_fire",    Name="Din's Fire (MM)",    Type=TrackerItemType.Item, MaxCount=1, IconPath="items/dins_fire.png" });
            if (cfg.MmSpellWind && !cfg.SharedSpellWind)
                items.Add(new() { Id="mm_farores_wind", Name="Farore's Wind (MM)", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/farores_wind.png" });
            if (cfg.MmSpellLove && !cfg.SharedSpellLove)
                items.Add(new() { Id="mm_nayrus_love",  Name="Nayru's Love (MM)",  Type=TrackerItemType.Item, MaxCount=1, IconPath="items/nayrus_love.png" });
            if (cfg.MmHammer && !cfg.SharedHammer)
                items.Add(new() { Id="mm_hammer",  Name="Megaton Hammer (MM)", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/megaton_hammer.png" });

            return items;
        }

        // ─── Items Shared (7×3) ───────────────────────────────────────────────────

        public static List<TrackerItem> GetSharedItems_Items(TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();

            // Only items with shared option enabled
            if (cfg.SharedBows)
                items.Add(new() { Id="sh_bow", Name="Bow", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Bow (30)","Bow (40)","Bow (50)"},
                    StepLabels=new[]{"30","40","50"}, IconPath="items/fairy_bow.png" });
            if (cfg.SharedFireArrows)
                items.Add(new() { Id="sh_fire_arrows",  Name="Fire Arrows",  Type=TrackerItemType.Item, MaxCount=1, IconPath="items/fire_arrows.png" });
            if (cfg.SharedIceArrows)
                items.Add(new() { Id="sh_ice_arrows",   Name="Ice Arrows",   Type=TrackerItemType.Item, MaxCount=1, IconPath="items/ice_arrows.png" });
            if (cfg.SharedLightArrows)
                items.Add(new() { Id="sh_light_arrows", Name="Light Arrows", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/light_arrows.png" });
            if (cfg.SharedSpellFire)
                items.Add(new() { Id="sh_dins_fire",    Name="Din's Fire",    Type=TrackerItemType.Item, MaxCount=1, IconPath="items/dins_fire.png" });
            if (cfg.SharedSpellWind)
                items.Add(new() { Id="sh_farores_wind", Name="Farore's Wind", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/farores_wind.png" });
            if (cfg.SharedSpellLove)
                items.Add(new() { Id="sh_nayrus_love",  Name="Nayru's Love",  Type=TrackerItemType.Item, MaxCount=1, IconPath="items/nayrus_love.png" });
            if (cfg.SharedHookshot)
                items.Add(new() { Id="sh_hookshot", Name="Hookshot", Type=TrackerItemType.Item, MaxCount=2,
                    StepNames=new[]{"Hookshot","Longshot"},
                    StepIconPaths=new[]{"items/short_hookshot.png","items/longshot.png"}, IconPath="items/short_hookshot.png" });
            if (cfg.SharedHammer)
                items.Add(new() { Id="sh_hammer", Name="Megaton Hammer", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/megaton_hammer.png" });
            if (cfg.SharedLens)
                items.Add(new() { Id="sh_lens", Name="Lens of Truth", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/lens_of_truth.png" });
            if (cfg.SharedNutsSticks)
            {
                items.Add(new() { Id="sh_deku_stick", Name="Deku Stick", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Deku Stick (10)","Deku Stick (20)","Deku Stick (30)"},
                    StepLabels=new[]{"10","20","30"}, IconPath="items/deku_stick.png" });
                items.Add(new() { Id="sh_deku_nut", Name="Deku Nut", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Deku Nut (20)","Deku Nut (30)","Deku Nut (40)"},
                    StepLabels=new[]{"20","30","40"}, IconPath="items/deku_nut.png" });
            }
            if (cfg.SharedBombBags)
                items.Add(new() { Id="sh_bomb_bag", Name="Bomb Bag", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Bomb Bag (20)","Bomb Bag (30)","Bomb Bag (40)"},
                    StepLabels=new[]{"20","30","40"}, IconPath="items/bomb.png" });
            if (cfg.SharedBombchu)
                items.Add(MakeBombchu("sh_bombchu", cfg.BombchuBehaviorOot, "sh_bomb_bag"));

            // If all items shared — add OoT and MM exclusives
            if (cfg.AllItemsShared)
            {
                items.Add(new() { Id="sh_slingshot", Name="Slingshot", Type=TrackerItemType.Item, MaxCount=3,
                    StepNames=new[]{"Slingshot (30)","Slingshot (40)","Slingshot (50)"},
                    StepLabels=new[]{"30","40","50"}, IconPath="items/fairy_slingshot.png" });
                items.Add(new() { Id="sh_boomerang",   Name="Boomerang",        Type=TrackerItemType.Item, MaxCount=1, IconPath="items/boomerang.png" });
                if (!cfg.OotPreplantedBeans)
                    items.Add(new() { Id="sh_beans_oot", Name="Magic Beans (OoT)", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/magic_beans_oot.png" });
                items.Add(new() { Id="sh_fairy_sword", Name="Fairy Sword",      Type=TrackerItemType.Item, MaxCount=1, IconPath="items/great_fairys_sword.png" });
                items.Add(new() { Id="sh_powder_keg",  Name="Powder Keg",       Type=TrackerItemType.Item, MaxCount=1, IconPath="items/powder_keg.png" });
                items.Add(new() { Id="sh_pictograph",  Name="Pictograph Box",   Type=TrackerItemType.Item, MaxCount=1, IconPath="items/pictograph_box.png" });
                if (cfg.OotPreplantedBeans)
                    items.Add(new() { Id="sh_beans_mm", Name="Magic Beans",      Type=TrackerItemType.Item, MaxCount=1, IconPath="items/magic_beans.png" });
                else
                    items.Add(new() { Id="sh_beans_mm", Name="Magic Beans (MM)", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/magic_beans_mm.png" });
            }

            return items;
        }

        // Creates Bombchu item depending on mode
        // bombBagId — Id of bomb item to which bombBag mode is linked
        private static TrackerItem MakeBombchu(string id, string behavior, string bombBagId = "") => behavior switch
        {
            "bag" => new() { Id=id, Name="Bombchu", Type=TrackerItemType.Item, MaxCount=3,
                StepNames=new[]{"Bombchu (20)","Bombchu (30)","Bombchu (40)"},
                StepLabels=new[]{"20","30","40"}, IconPath="items/bombchu.png" },
            "bombBag" => new() { Id=id, Name="Bombchu", Type=TrackerItemType.Item, MaxCount=1,
                IconPath="items/bombchu.png",
                // Store bomb Id in StepNames[0] for auto-highlight logic
                StepNames=new[]{ bombBagId } },
            _ => new() { Id=id, Name="Bombchu", Type=TrackerItemType.Item, MaxCount=1, IconPath="items/bombchu.png" }
        };

        // ─── Trade Quests ─────────────────────────────────────────────────────────

        public static List<TrackerItem> GetOotTradeItems(TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();

            // Adult trade chain
            items.Add(new() { Id="tq_cucco_adult",  Name="Cucco (Adult)",    Type=TrackerItemType.TradeQuest, IconPath="sidequests/cucco.png",           StaticLabel="Adult" });
            items.Add(new() { Id="tq_cojiro",       Name="Cojiro",           Type=TrackerItemType.TradeQuest, IconPath="sidequests/cojiro.png" });
            items.Add(new() { Id="tq_mushroom",     Name="Odd Mushroom",     Type=TrackerItemType.TradeQuest, IconPath="sidequests/odd_mushroom.png" });
            items.Add(new() { Id="tq_odd_potion",   Name="Odd Potion",       Type=TrackerItemType.TradeQuest, IconPath="sidequests/odd_potion.png" });
            items.Add(new() { Id="tq_poacher_saw",  Name="Poacher's Saw",    Type=TrackerItemType.TradeQuest, IconPath="sidequests/poachers_saw.png" });
            items.Add(new() { Id="tq_broken_sword", Name="Broken Sword",     Type=TrackerItemType.TradeQuest, IconPath="sidequests/broken_goron_sword.png" });
            items.Add(new() { Id="tq_prescription", Name="Prescription",     Type=TrackerItemType.TradeQuest, IconPath="sidequests/prescription.png" });
            items.Add(new() { Id="tq_frog",         Name="Eyeball Frog",     Type=TrackerItemType.TradeQuest, IconPath="sidequests/eyeball_frog.png" });
            items.Add(new() { Id="tq_eye_drops",    Name="Eye Drops",        Type=TrackerItemType.TradeQuest, IconPath="sidequests/eye_drops.png" });
            items.Add(new() { Id="tq_claim_check",  Name="Claim Check",      Type=TrackerItemType.TradeQuest, IconPath="sidequests/claim_check.png" });

            // Child trade chain (optional)
            if (!cfg.OotSkipZelda)
                items.Add(new() { Id="tq_cucco_child",  Name="Cucco (Child)",   Type=TrackerItemType.TradeQuest, IconPath="sidequests/cucco.png", StaticLabel="Child" });
            if (!cfg.OotOpenKakariko)
                items.Add(new() { Id="tq_zelda_letter", Name="Zelda's Letter",  Type=TrackerItemType.TradeQuest, IconPath="sidequests/zeldas_letter.png" });
            if (cfg.OotEggShuffle)
                items.Add(new() { Id="tq_egg", Name="Egg", Type=TrackerItemType.TradeQuest,
                    MaxCount=2, IconPath="sidequests/egg.png",
                    StepNames=new[]{"Weird Egg","Pocket Egg"},
                    StepLabels=new[]{"1","2"} });

            return items;
        }

        public static List<TrackerItem> GetMmTradeItems()
        {
            var items = new List<TrackerItem>();

            // Moon's Tear / deed chain
            items.Add(new() { Id="tq_moon_tear",     Name="Moon's Tear",          Type=TrackerItemType.TradeQuest, IconPath="sidequests/moons_tear.png" });
            items.Add(new() { Id="tq_land_deed",     Name="Land Title Deed",      Type=TrackerItemType.TradeQuest, IconPath="sidequests/land_title_deed.png" });
            items.Add(new() { Id="tq_swamp_deed",    Name="Swamp Title Deed",     Type=TrackerItemType.TradeQuest, IconPath="sidequests/swamp_title_deed.png" });
            items.Add(new() { Id="tq_mountain_deed", Name="Mountain Title Deed",  Type=TrackerItemType.TradeQuest, IconPath="sidequests/mountain_title_deed.png" });
            items.Add(new() { Id="tq_ocean_deed",    Name="Ocean Title Deed",     Type=TrackerItemType.TradeQuest, IconPath="sidequests/ocean_title_deed.png" });

            // Kafei chain
            items.Add(new() { Id="tq_room_key",      Name="Room Key",             Type=TrackerItemType.TradeQuest, IconPath="sidequests/room_key.png" });
            items.Add(new() { Id="tq_kafei_letter",  Name="Letter to Kafei",      Type=TrackerItemType.TradeQuest, IconPath="sidequests/letter_to_kafei.png" });
            items.Add(new() { Id="tq_pendant",       Name="Pendant of Memories",  Type=TrackerItemType.TradeQuest, IconPath="sidequests/pendant_of_memories.png" });
            items.Add(new() { Id="tq_mama_letter",   Name="Letter to Mama",       Type=TrackerItemType.TradeQuest, IconPath="sidequests/special_delivery_to_mama.png" });

            return items;
        }

        // ─── Collectibles: Stray Fairies ─────────────────────────────────────────

        public static List<TrackerItem> GetStrayFairies(TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();
            if (cfg.StrayFairiesDungeons)
            {
                items.Add(new() { Id="sf_woodfall",    Name="Woodfall",    Type=TrackerItemType.Item,
                    MaxCount=15, IconPath="dungeons/woodfall_fairy.png",
                    StepLabels=Enumerable.Range(1,15).Select(i=>i.ToString()).ToArray() });
                items.Add(new() { Id="sf_snowhead",    Name="Snowhead",    Type=TrackerItemType.Item,
                    MaxCount=15, IconPath="dungeons/snowhead_fairy.png",
                    StepLabels=Enumerable.Range(1,15).Select(i=>i.ToString()).ToArray() });
                items.Add(new() { Id="sf_great_bay",   Name="Great Bay",   Type=TrackerItemType.Item,
                    MaxCount=15, IconPath="dungeons/greatbay_fairy.png",
                    StepLabels=Enumerable.Range(1,15).Select(i=>i.ToString()).ToArray() });
                items.Add(new() { Id="sf_stone_tower", Name="Stone Tower", Type=TrackerItemType.Item,
                    MaxCount=15, IconPath="dungeons/stonetower_fairy.png",
                    StepLabels=Enumerable.Range(1,15).Select(i=>i.ToString()).ToArray() });
            }
            if (cfg.StrayFairyTown)
                items.Add(new() { Id="sf_clock_town", Name="Clock Town", Type=TrackerItemType.Item,
                    MaxCount=1, IconPath="dungeons/clocktown_fairy.png" });
            if (cfg.TranscendentFairy)
                items.Add(new() { Id="transcendent_fairy", Name="Transcendent Fairy", Type=TrackerItemType.Item,
                    MaxCount=1, IconPath="dungeons/transcendent_fairy.png" });
            return items;
        }

        // ─── Collectibles: Misc (Coins + Triforce) ───────────────────────────────

        public static List<TrackerItem> GetMiscItems(TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();
            // Coins
            if (cfg.CoinsRed)
                items.Add(new() { Id="coin_red",    Name="Red Coins",    Type=TrackerItemType.Item,
                    MaxCount=cfg.CoinsRedMax,    IconPath="coins/red_coin.png",
                    StepLabels=Enumerable.Range(1,cfg.CoinsRedMax).Select(i=>i.ToString()).ToArray() });
            if (cfg.CoinsGreen)
                items.Add(new() { Id="coin_green",  Name="Green Coins",  Type=TrackerItemType.Item,
                    MaxCount=cfg.CoinsGreenMax,  IconPath="coins/green_coin.png",
                    StepLabels=Enumerable.Range(1,cfg.CoinsGreenMax).Select(i=>i.ToString()).ToArray() });
            if (cfg.CoinsBlue)
                items.Add(new() { Id="coin_blue",   Name="Blue Coins",   Type=TrackerItemType.Item,
                    MaxCount=cfg.CoinsBlueMax,   IconPath="coins/blue_coin.png",
                    StepLabels=Enumerable.Range(1,cfg.CoinsBlueMax).Select(i=>i.ToString()).ToArray() });
            if (cfg.CoinsYellow)
                items.Add(new() { Id="coin_yellow", Name="Yellow Coins", Type=TrackerItemType.Item,
                    MaxCount=cfg.CoinsYellowMax, IconPath="coins/yellow_coin.png",
                    StepLabels=Enumerable.Range(1,cfg.CoinsYellowMax).Select(i=>i.ToString()).ToArray() });
            // Triforce
            if (cfg.TriforceMode == "quest")
                items.Add(new() { Id="triforce", Name="Triforce Pieces", Type=TrackerItemType.Item,
                    MaxCount=3, IconPath="equipment/triforce_piece.png",
                    StepLabels=new[]{"1/3","2/3","3/3"} });
            else if (cfg.TriforceMode == "hunt")
                items.Add(new() { Id="triforce", Name="Triforce Pieces", Type=TrackerItemType.Item,
                    MaxCount=cfg.TriforceHuntGoal, IconPath="equipment/triforce_piece.png",
                    StepLabels=Enumerable.Range(1,cfg.TriforceHuntGoal).Select(i=>$"{i}/{cfg.TriforceHuntGoal}").ToArray() });
            return items;
        }

        // ─── Collectibles: Skulltula Tokens ──────────────────────────────────────
        // One block with all enabled tokens: Gold (OoT) + Swamp/Ocean (MM) + Platinum

        public static List<TrackerItem> GetSkulltulas(TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();
            if (cfg.GoldSkulltulas)
                items.Add(new() { Id="gold_skulltula_tokens", Name="Gold Skulltulas", Type=TrackerItemType.Item,
                    MaxCount=100, IconPath="sidequests/gold_skulltula_token.png",
                    StepLabels=Enumerable.Range(1,100).Select(i=>i.ToString()).ToArray() });
            if (cfg.MmSkulltulas)
            {
                items.Add(new() { Id="swamp_skulltula_tokens", Name="Swamp Skulltulas", Type=TrackerItemType.Item,
                    MaxCount=30, IconPath="sidequests/swamp_skulltula_token.png",
                    StepLabels=Enumerable.Range(1,30).Select(i=>i.ToString()).ToArray() });
                items.Add(new() { Id="ocean_skulltula_tokens", Name="Ocean Skulltulas", Type=TrackerItemType.Item,
                    MaxCount=30, IconPath="sidequests/ocean_skulltula_token.png",
                    StepLabels=Enumerable.Range(1,30).Select(i=>i.ToString()).ToArray() });
            }
            // Platinum tokens
            if (cfg.SharedPlatinumToken)
            {
                items.Add(new() { Id="platinum_token", Name="Platinum Token", Type=TrackerItemType.Item,
                    MaxCount=1, IconPath="sidequests/platinum_skulltula_token.png" });
            }
            else
            {
                if (cfg.PlatinumTokenOot)
                    items.Add(new() { Id="platinum_token_oot", Name="Platinum Token (OoT)", Type=TrackerItemType.Item,
                        MaxCount=1, IconPath="sidequests/platinum_skulltula_token.png",
                        StaticLabel="OoT" });
                if (cfg.PlatinumTokenMm)
                    items.Add(new() { Id="platinum_token_mm", Name="Platinum Token (MM)", Type=TrackerItemType.Item,
                        MaxCount=1, IconPath="sidequests/platinum_skulltula_token.png",
                        StaticLabel="MM" });
            }
            return items;
        }

        // ─── Dungeon Special Items: Skeleton Key + Magical Rupee ─────────────────

        public static List<TrackerItem> GetDungeonSpecialItems(TrackerConfig cfg)
        {
            var items = new List<TrackerItem>();

            // Skeleton Key — replaces all small keys with one item
            // Similar to Platinum Token: OoT, MM, Shared
            if (cfg.SharedSkeletonKey)
            {
                items.Add(new() { Id="skeleton_key", Name="Skeleton Key", Type=TrackerItemType.Item,
                    MaxCount=1, IconPath="dungeons/skeleton_key.png" });
            }
            else
            {
                if (cfg.SkeletonKeyOot)
                    items.Add(new() { Id="skeleton_key_oot", Name="Skeleton Key (OoT)", Type=TrackerItemType.Item,
                        MaxCount=1, IconPath="dungeons/skeleton_key.png", StaticLabel="OoT" });
                if (cfg.SkeletonKeyMm)
                    items.Add(new() { Id="skeleton_key_mm", Name="Skeleton Key (MM)", Type=TrackerItemType.Item,
                        MaxCount=1, IconPath="dungeons/skeleton_key.png", StaticLabel="MM" });
            }

            // Magical Rupee — replaces all Silver Rupees with one item
            if (cfg.MagicalRupee)
                items.Add(new() { Id="magical_rupee", Name="Magical Rupee", Type=TrackerItemType.Item,
                    MaxCount=1, IconPath="dungeons/magical_rupee.png" });

            return items;
        }

        // Souls moved to SoulsData.cs
    }
}
