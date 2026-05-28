using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using OoTMMTracker.Models;

namespace OoTMMTracker.Services
{
    /// <summary>
    /// Resolves spoiler <c>Entrances</c> rows: <b>From</b> (left) ↔ map marks (<see cref="MapMark.EntranceFromId"/>),
    /// <b>To</b> (right) ↔ target sub-map via <see cref="MapSubRegion.DestinationEntranceIds"/>, or by parsing the To text
    /// (<c>OOT Area Name from ...</c>) when no id is registered.
    /// </summary>
    public static class EntranceMapNavigation
    {
        private static Dictionary<string, (string Region, string SubMap, string Game)> _destinationIdToMap =
            new(StringComparer.OrdinalIgnoreCase);

        private static IReadOnlyList<MapRegion> _regionsRegistry = Array.Empty<MapRegion>();

        static EntranceMapNavigation() 
        {
            Debug.WriteLine("[EntranceMapNavigation] Static constructor called");
            RebuildDestinationIndex();
        }

        /// <summary>
        /// Rebuilds the lookup from <see cref="MapSubRegion.DestinationEntranceIds"/> (call after changing map data at runtime).
        /// </summary>
        public static void RebuildDestinationIndex(IReadOnlyList<MapRegion>? regions = null)
        {
            var resolved = (regions ?? MapRegionsData.GetAll()).ToList();
            Debug.WriteLine($"[EntranceMapNavigation] RebuildDestinationIndex: {resolved.Count} regions");
            _regionsRegistry = resolved;
            var map = new Dictionary<string, (string Region, string SubMap, string Game)>(StringComparer.OrdinalIgnoreCase);
            int totalIds = 0;
            foreach (var region in resolved)
            {
                foreach (var sub in region.SubRegions)
                {
                    if (sub.DestinationEntranceIds == null) continue;
                    foreach (var raw in sub.DestinationEntranceIds)
                    {
                        if (string.IsNullOrWhiteSpace(raw)) continue;
                        var id = NormalizeEntranceIdToken(raw.Trim());
                        totalIds++;
                        if (map.TryGetValue(id, out var dup))
                        {
                            // Same To-side id on multiple sub-maps breaks strict uniqueness; keep first registration so the app can load.
                            Trace.WriteLine(
                                $"[EntranceMapNavigation] Duplicate destination id \"{id}\": using \"{dup.Region}/{dup.SubMap}\", skipping \"{region.Name}/{sub.Name}\".");
                            continue;
                        }

                        map[id] = (region.Name, sub.Name, region.Game);
                        Debug.WriteLine($"[EntranceMapNavigation] Registered: {id} -> {region.Name}/{sub.Name} ({region.Game})");
                    }
                }
            }

            _destinationIdToMap = map;
            Debug.WriteLine($"[EntranceMapNavigation] Built dictionary with {map.Count} unique IDs (from {totalIds} total)");
        }

        /// <summary>Spoiler ids sometimes use spaces inside brackets; map data uses underscores.</summary>
        public static string NormalizeEntranceIdToken(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            var s = raw.Trim();
            s = Regex.Replace(s, @"\s+", "_");
            return s;
        }

        /// <summary>
        /// Gets map information for a destination entrance ID directly from MapRegionsData.
        /// Returns true if the ID is found in any sub-map's DestinationEntranceIds.
        /// </summary>
        public static bool TryGetMapForDestinationId(
            string destinationId,
            out string regionName,
            out string subMapName,
            out string game)
        {
            regionName = "";
            subMapName = "";
            game = "";
            
            if (string.IsNullOrWhiteSpace(destinationId))
            {
                Debug.WriteLine($"[EntranceMapNavigation] TryGetMapForDestinationId: empty destinationId");
                return false;
            }
                
            var normId = NormalizeEntranceIdToken(destinationId);
            Debug.WriteLine($"[EntranceMapNavigation] TryGetMapForDestinationId: looking for '{destinationId}' (normalized: '{normId}')");
            
            bool found = _destinationIdToMap.TryGetValue(normId, out var t);
            if (found)
            {
                regionName = t.Region;
                subMapName = t.SubMap;
                game = t.Game;
                Debug.WriteLine($"[EntranceMapNavigation] Found: {destinationId} -> {regionName}/{subMapName} ({game})");
            }
            else
            {
                Debug.WriteLine($"[EntranceMapNavigation] Not found: {destinationId}");
                // Debug: list some keys for comparison
                if (_destinationIdToMap.Count > 0)
                {
                    var sampleKeys = _destinationIdToMap.Keys.Take(5).ToList();
                    Debug.WriteLine($"[EntranceMapNavigation] Sample keys in dictionary: {string.Join(", ", sampleKeys)}");
                }
                else
                {
                    Debug.WriteLine($"[EntranceMapNavigation] Dictionary is empty!");
                }
            }
            
            return found && !string.IsNullOrEmpty(t.Region) && !string.IsNullOrEmpty(t.SubMap) && !string.IsNullOrEmpty(t.Game);
        }

        private static bool TryFindLastBracketSpan(string spoilerSide, out int openIndex, out int closeIndex)
        {
            openIndex = -1;
            closeIndex = -1;
            int closeParen = spoilerSide.LastIndexOf(')');
            int closeBrace = spoilerSide.LastIndexOf('}');
            int useClose = -1;
            char openChar = '(';
            if (closeParen > closeBrace)
                useClose = closeParen;
            else if (closeBrace >= 0)
            {
                useClose = closeBrace;
                openChar = '{';
            }

            if (useClose < 0) return false;
            int open = spoilerSide.LastIndexOf(openChar, useClose);
            if (open < 0 || useClose <= open) return false;
            openIndex = open;
            closeIndex = useClose;
            return true;
        }

        /// <summary>
        /// Extracts the entrance id from the end of a spoiler side: last <c>(...)</c> or <c>{...}</c>,
        /// then <see cref="NormalizeEntranceIdToken"/>.
        /// </summary>
        public static string? TryExtractEntranceId(string spoilerSide)
        {
            if (string.IsNullOrWhiteSpace(spoilerSide)) return null;
            if (!TryFindLastBracketSpan(spoilerSide, out int open, out int close)) return null;
            var inner = spoilerSide.Substring(open + 1, close - open - 1).Trim();
            var norm = NormalizeEntranceIdToken(inner);
            return string.IsNullOrEmpty(norm) ? null : norm;
        }

        /// <summary>Finds the spoiler Entrances row whose <b>From</b> (key) id matches <paramref name="fromId"/>.</summary>
        public static bool TryGetDestinationForFromId(
            IReadOnlyDictionary<string, string> entrances,
            string fromId,
            out string destinationLine)
        {
            destinationLine = "";
            if (string.IsNullOrEmpty(fromId)) return false;
            var normFrom = NormalizeEntranceIdToken(fromId);
            foreach (var kv in entrances)
            {
                var keyId = TryExtractEntranceId(kv.Key);
                if (keyId != null &&
                    string.Equals(keyId, normFrom, StringComparison.OrdinalIgnoreCase))
                {
                    destinationLine = kv.Value;
                    return true;
                }
            }

            // Legacy: substring match for keys that put the id mid-string without being the last bracket
            string legacyNeedle = $"({fromId})";
            foreach (var kv in entrances)
            {
                if (kv.Key.Contains(legacyNeedle, StringComparison.OrdinalIgnoreCase))
                {
                    destinationLine = kv.Value;
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetMapForDestinationLine(
            string destinationSpoilerLine,
            out string regionName,
            out string subMapName,
            out string game)
        {
            regionName = "";
            subMapName = "";
            game = "";
            var destId = TryExtractEntranceId(destinationSpoilerLine);
            if (destId != null && _destinationIdToMap.TryGetValue(destId, out var t))
            {
                regionName = t.Region;
                subMapName = t.SubMap;
                game = t.Game;
                return true;
            }

            return TryResolveMapFromSpoilerDestinationText(destinationSpoilerLine, out regionName, out subMapName, out game);
        }

        /// <summary>
        /// When no explicit <see cref="MapSubRegion.DestinationEntranceIds"/> entry exists, infer region from the
        /// OoTMM-style <c>To</c> text: <c>OOT Zora River Upper from ... (ID)</c> → area before the first <c> from </c>.
        /// </summary>
        private static bool TryResolveMapFromSpoilerDestinationText(
            string destinationSpoilerLine,
            out string regionName,
            out string subMapName,
            out string game)
        {
            regionName = "";
            subMapName = "";
            game = "";
            if (string.IsNullOrWhiteSpace(destinationSpoilerLine)) return false;

            var line = destinationSpoilerLine.Trim();
            if (TryFindLastBracketSpan(line, out int openIdx, out _))
                line = line[..openIdx].TrimEnd();

            var fromIdx = line.IndexOf(" from ", StringComparison.OrdinalIgnoreCase);
            var beforeFrom = fromIdx >= 0 ? line[..fromIdx].Trim() : line;
            if (beforeFrom.Length < 4) return false;

            var firstSpace = beforeFrom.IndexOf(' ');
            if (firstSpace < 0) return false;
            var g = beforeFrom[..firstSpace].Trim().ToUpperInvariant();
            if (g != "OOT" && g != "MM") return false;
            game = g;
            var locationPart = beforeFrom[(firstSpace + 1)..].Trim();
            if (string.IsNullOrEmpty(locationPart)) return false;

            MapRegion? bestRegion = null;
            foreach (var reg in _regionsRegistry)
            {
                if (!string.Equals(reg.Game, game, StringComparison.OrdinalIgnoreCase)) continue;
                if (locationPart.IndexOf(reg.Name, StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (bestRegion == null || reg.Name.Length > bestRegion.Name.Length)
                    bestRegion = reg;
            }

            if (bestRegion == null) return false;

            MapSubRegion? bestSub = null;
            foreach (var sub in bestRegion.SubRegions)
            {
                if (locationPart.StartsWith(sub.Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (bestSub == null || sub.Name.Length > bestSub.Name.Length)
                        bestSub = sub;
                }
            }

            bestSub ??= bestRegion.SubRegions.FirstOrDefault(s => s.Name == bestRegion.Name)
                       ?? bestRegion.SubRegions.FirstOrDefault();
            if (bestSub == null) return false;

            regionName = bestRegion.Name;
            subMapName = bestSub.Name;
            return true;
        }

        /// <summary>
        /// Sub-map name as in the Map tab dropdown, for short tooltips.
        /// </summary>
        public static bool TryGetDestinationSubMapDisplayName(
            IReadOnlyDictionary<string, string>? entrances,
            string? fromEntranceId,
            out string subMapName)
        {
            subMapName = "";
            if (entrances == null || string.IsNullOrEmpty(fromEntranceId)) return false;
            if (!TryGetDestinationForFromId(entrances, fromEntranceId, out var destLine)) return false;
            return TryGetMapForDestinationLine(destLine, out _, out subMapName, out _) &&
                   !string.IsNullOrEmpty(subMapName);
        }
    }
}
