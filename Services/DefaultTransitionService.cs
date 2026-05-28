using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OoTMMTracker.Services
{
    /// <summary>
    /// Provides default (vanilla) transition destinations for entrance IDs when they are not shuffled.
    /// </summary>
    public static class DefaultTransitionService
    {
        // Dictionary: EntranceId -> Default destination area name (from entrances.yml areas[1])
        private static readonly Dictionary<string, string> _defaultTransitions = new(StringComparer.OrdinalIgnoreCase)
        {
            // OoT Spawns
            ["OOT_SPAWN_CHILD"] = "OOT Link's House",
            ["OOT_SPAWN_ADULT"] = "OOT Temple of Time",

            // OoT Warp Songs
            ["OOT_WARP_SONG_MEADOW"] = "OOT Sacred Meadow",
            ["OOT_WARP_SONG_CRATER"] = "OOT Death Mountain Crater Warp",
            ["OOT_WARP_SONG_LAKE"] = "OOT Lake Hylia",
            ["OOT_WARP_SONG_DESERT"] = "OOT Desert Colossus",
            ["OOT_WARP_SONG_GRAVE"] = "OOT Graveyard Upper",
            ["OOT_WARP_SONG_TEMPLE"] = "OOT Temple of Time",

            // OoT Owls
            ["OOT_VILLAGE_OWL"] = "OOT Kakariko Rooftop",
            ["OOT_FIELD_OWL"] = "OOT Hyrule Field Drawbridge",

            // MM Owls
            ["MM_WARP_OWL_CLOCK_TOWN"] = "MM Owl Clock Town",
            ["MM_WARP_OWL_MILK_ROAD"] = "MM Owl Milk Road",
            ["MM_WARP_OWL_SOUTHERN_SWAMP"] = "MM Owl Swamp",
            ["MM_WARP_OWL_WOODFALL"] = "MM Owl Woodfall",
            ["MM_WARP_OWL_MOUNTAIN_VILLAGE"] = "MM Owl Mountain",
            ["MM_WARP_OWL_SNOWHEAD"] = "MM Owl Snowhead",
            ["MM_WARP_OWL_GREAT_BAY"] = "MM Owl Great Bay",
            ["MM_WARP_OWL_ZORA_CAPE"] = "MM Owl Zora Cape",
            ["MM_WARP_OWL_IKANA_CANYON"] = "MM Owl Ikana",
            ["MM_WARP_OWL_STONE_TOWER"] = "MM Owl Stone Tower",
        };

        // Dictionary: Area name -> Region name (for navigation)
        private static readonly Dictionary<string, string?> _areaToRegion = new(StringComparer.OrdinalIgnoreCase)
        {
            // OoT Areas - based on actual map/submap destinations
            ["OOT Link's House"] = "Kokiri Forest",           // Map: Kokiri Forest, Submap: Link House
            ["OOT Temple of Time"] = "Market",                // Map: Market, Submap: Temple of Time
            ["OOT Sacred Meadow"] = "Sacred Forest Meadow",   // Map: Sacred Forest Meadow, Submap: Sacred Forest Meadow
            ["OOT Death Mountain Crater Warp"] = "Death Mountain Crater", // Map: Death Mountain Crater, Submap: Death Mountain Crater
            ["OOT Lake Hylia"] = "Lake Hylia",                // Map: Lake Hylia, Submap: Lake Hylia
            ["OOT Desert Colossus"] = "Desert Colossus",      // Map: Desert Colossus, Submap: Desert Colossus
            ["OOT Graveyard Upper"] = "Graveyard",            // Map: Graveyard, Submap: Graveyard
            ["OOT Kakariko Rooftop"] = "Kakariko",            // Map: Kakariko, Submap: Kakariko Village
            ["OOT Hyrule Field Drawbridge"] = "Hyrule Field", // Map: Hyrule Field, Submap: Hyrule Field

            // MM Areas - based on actual map/submap destinations
            ["MM Owl Clock Town"] = "Clock Town",             // Map: Clock Town, Submap: South Clock Town
            ["MM Owl Milk Road"] = "Milk Road",               // Map: Milk Road, Submap: Milk Road
            ["MM Owl Swamp"] = "Southern Swamp",              // Map: Southern Swamp, Submap: Southern Swamp
            ["MM Owl Woodfall"] = "Woodfall",                 // Map: Woodfall, Submap: Woodfall
            ["MM Owl Mountain"] = "Mountain Village",         // Map: Mountain Village, Submap: Mountain Village
            ["MM Owl Snowhead"] = "Snowhead",                 // Map: Snowhead, Submap: Snowhead
            ["MM Owl Great Bay"] = "Great Bay Coast",         // Map: Great Bay Coast, Submap: Great Bay Coast
            ["MM Owl Zora Cape"] = "Zora Cape",               // Map: Zora Cape, Submap: Zora Cape
            ["MM Owl Ikana"] = "Ikana Canyon",                // Map: Ikana Canyon, Submap: Ikana Canyon
            ["MM Owl Stone Tower"] = "Stone Tower",           // Map: Stone Tower, Submap: Stone Tower

            // Special cases
            ["MM SOARING"] = null,
            ["MM VOID"] = null,
            ["OOT VOID"] = null,
        };

        /// <summary>
        /// Gets the default destination region for an entrance ID.
        /// Returns null if no default transition exists or for wallmasters.
        /// </summary>
        public static string? GetDefaultDestinationRegion(string entranceId)
        {
            // Wallmasters don't have default region transitions
            if (IsWallmaster(entranceId))
                return null;

            if (!_defaultTransitions.TryGetValue(entranceId, out var area))
                return null;

            return _areaToRegion.TryGetValue(area, out var region) ? region : null;
        }

        /// <summary>
        /// Gets the default destination area name for an entrance ID.
        /// Returns null if no default transition exists.
        /// </summary>
        public static string? GetDefaultDestinationArea(string entranceId)
        {
            if (IsWallmaster(entranceId))
                return null;

            return _defaultTransitions.TryGetValue(entranceId, out var area) ? area : null;
        }

        /// <summary>
        /// Checks if an entrance ID has a default transition.
        /// </summary>
        public static bool HasDefaultTransition(string entranceId)
        {
            if (IsWallmaster(entranceId))
                return false;

            return _defaultTransitions.ContainsKey(entranceId);
        }

        /// <summary>
        /// Checks if an entrance ID is a wallmaster (doesn't change region).
        /// </summary>
        public static bool IsWallmaster(string entranceId)
        {
            return entranceId.Contains("WALLMASTER", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the display text for a default transition.
        /// </summary>
        public static string? GetDefaultTransitionDisplay(string entranceId)
        {
            if (IsWallmaster(entranceId))
                return "Wallmaster - teleports within same area";

            if (!HasDefaultTransition(entranceId))
                return null;

            var region = GetDefaultDestinationRegion(entranceId);
            return region != null ? $"Vanilla to: {region}" : "Vanilla connection";
        }

        /// <summary>
        /// Creates a synthetic destination line for default transitions.
        /// Format: "Vanilla connection (ENTRANCE_ID)"
        /// </summary>
        public static string? CreateSyntheticDestinationLine(string entranceId)
        {
            if (!HasDefaultTransition(entranceId))
                return null;

            var area = GetDefaultDestinationArea(entranceId);
            if (area == null)
                return null;

            // Format: "Area Name (ENTRANCE_ID)"
            return $"{area} ({entranceId})";
        }

        /// <summary>
        /// Gets map information for a default transition directly from DestinationEntranceIds.
        /// Returns true if the entrance ID is found in any sub-map's DestinationEntranceIds.
        /// </summary>
        public static bool TryGetMapForDefaultTransition(
            string entranceId,
            out string regionName,
            out string subMapName,
            out string game)
        {
            regionName = "";
            subMapName = "";
            game = "";
            
            if (!HasDefaultTransition(entranceId))
            {
                Debug.WriteLine($"[DefaultTransitionService] No default transition for: {entranceId}");
                return false;
            }
                
            bool result = EntranceMapNavigation.TryGetMapForDestinationId(entranceId, out regionName, out subMapName, out game);
            Debug.WriteLine($"[DefaultTransitionService] TryGetMapForDestinationId({entranceId}) = {result}, region={regionName}, sub={subMapName}, game={game}");
            return result;
        }

        /// <summary>
        /// Debug method to check if a default transition is properly configured in MapRegionsData.
        /// </summary>
        public static bool IsDefaultTransitionConfigured(string entranceId)
        {
            return HasDefaultTransition(entranceId) && 
                   TryGetMapForDefaultTransition(entranceId, out _, out _, out _);
        }

        /// <summary>
        /// Test method to verify all default transitions are properly configured.
        /// </summary>
        public static void TestAllDefaultTransitions()
        {
            Debug.WriteLine("[DefaultTransitionService] Testing all default transitions:");
            foreach (var kvp in _defaultTransitions)
            {
                var entranceId = kvp.Key;
                var area = kvp.Value;
                bool hasTransition = HasDefaultTransition(entranceId);
                bool hasMap = TryGetMapForDefaultTransition(entranceId, out var region, out var sub, out var game);
                
                Debug.WriteLine($"[DefaultTransitionService] {entranceId}: hasTransition={hasTransition}, hasMap={hasMap}, area={area}, region={region}, sub={sub}, game={game}");
            }
            
            // Also test a few specific IDs
            Debug.WriteLine("[DefaultTransitionService] Testing specific IDs:");
            string[] testIds = new[] 
            {
                "OOT_SPAWN_CHILD",
                "OOT_SPAWN_ADULT", 
                "OOT_WARP_SONG_MEADOW",
                "OOT_VILLAGE_OWL",
                "OOT_FIELD_OWL",
                "MM_WARP_OWL_CLOCK_TOWN"
            };
            
            foreach (var id in testIds)
            {
                bool hasTransition = HasDefaultTransition(id);
                bool hasMap = TryGetMapForDefaultTransition(id, out var region, out var sub, out var game);
                Debug.WriteLine($"[DefaultTransitionService] {id}: hasTransition={hasTransition}, hasMap={hasMap}, region={region}, sub={sub}, game={game}");
            }
        }
    }
}