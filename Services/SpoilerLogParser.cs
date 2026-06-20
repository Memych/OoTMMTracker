using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OoTMMTracker.Models;

namespace OoTMMTracker.Services
{
    public class SpoilerLogParser
    {
        public static void Validate(string filePath)
        {
            if (!File.Exists(filePath))
                throw new InvalidOperationException("File not found.");

            if (!filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid file format. Expected a .txt spoiler log file.");

            var lines = File.ReadAllLines(filePath);

            bool hasSeed      = lines.Any(l => l.TrimStart().StartsWith("Seed:"));
            bool hasVersion   = lines.Any(l => l.TrimStart().StartsWith("Version:"));
            bool hasSettings  = lines.Any(l => l.Trim() == "Settings");
            bool hasLocations = lines.Any(l => l.TrimStart().StartsWith("Location List"));

            if (!hasSeed || !hasVersion || !hasSettings || !hasLocations)
                throw new InvalidOperationException(
                    "The file does not appear to be a valid OoTMM spoiler log.\n\n" +
                    $"Missing sections:{(!hasSeed      ? "\n  - Seed"          : "")}" +
                    $"{(!hasVersion   ? "\n  - Version"      : "")}" +
                    $"{(!hasSettings  ? "\n  - Settings"     : "")}" +
                    $"{(!hasLocations ? "\n  - Location List" : "")}");
        }

        // ── Section header → section name ─────────────────────────────────────
        private static readonly Dictionary<string, string> SectionHeaders = new()
        {
            ["Settings"]           = "Settings",
            ["Special Conditions"] = "SpecialConditions",
            ["Tricks"]             = "Tricks",
            ["Starting Items"]     = "StartingItems",
            ["World Flags"]        = "WorldFlags",
            ["Entrances"]          = "Entrances",
            ["Song Events"]        = "SongEvents",
        };

        public SpoilerLog Parse(string filePath)
        {
            var log = new SpoilerLog();
            var lines = File.ReadAllLines(filePath);

            string  section      = "";
            string  region       = "";
            string  specialCond  = "";
            string? worldFlagKey = null;
            string currentWorld = "World 1";
            string currentPlayer = "Player 1";
            string? multilineKey = null;
            string currentGame = "";

            foreach (var line in lines)
            {
                var t = line.Trim();
                if (string.IsNullOrWhiteSpace(t)) continue;

                // ── Top-level header fields ────────────────────────────────
                if (t.StartsWith("Seed:"))    { log.Seed    = t[5..].Trim(); continue; }
                if (t.StartsWith("Version:")) { log.Version = t[8..].Trim(); continue; }

                // ── Section header detection ───────────────────────────────
                if (SectionHeaders.TryGetValue(t, out var newSection))
                    { section = newSection; continue; }
                if (t.StartsWith("Location List"))
                    { section = "LocationList"; continue; }

                // ── Dispatch to section handler ────────────────────────────
                switch (section)
                {
                    case "Settings":
                        ParseKeyValue(t, log.Settings);
                        break;

                    case "SpecialConditions":
                        if (line.StartsWith("  ") && !line.StartsWith("    ") && t.EndsWith(":"))
                        {
                            specialCond = t.TrimEnd(':');
                            log.SpecialConditions.TryAdd(specialCond, new SpecialCondition { Name = specialCond });
                        }
                        else if (line.StartsWith("    "))
                        {
                            if (!string.IsNullOrEmpty(specialCond))
                                ParseKeyValue(t, log.SpecialConditions[specialCond].Requirements);
                        }
                        break;

                    case "Tricks":
                        if (line.StartsWith("  "))
                            log.Tricks.Add(t);
                        break;

                    case "StartingItems":

                        if (t.StartsWith("Player ", StringComparison.OrdinalIgnoreCase))
                        {
                            currentPlayer = t;
                            continue;
                        }

                        if (t.Contains(':'))
                        {
                            var parts = t.Split(':', 2);
                            var key = parts[0].Trim();
                            var value = parts[1].Trim();
                            log.StartingItems[$"{currentPlayer}: {key}"] = value;
                        }
                        break;

                    case "WorldFlags":
                        if (Regex.IsMatch(t, @"^World\s+\d+$"))
                        {
                            currentWorld = t;
                            multilineKey = null;
                            break;
                        }

                        if (!string.IsNullOrEmpty(line) && line.Length > 0 && char.IsWhiteSpace(line[0]) && t.StartsWith("-") && multilineKey != null)
                        {
                            var item = t.TrimStart('-').Trim();
                            var keyWithWorld = $"{currentWorld} {multilineKey}";

                            if (log.WorldFlags.TryGetValue(keyWithWorld, out var existing) && !string.IsNullOrEmpty(existing))
                            {
                                log.WorldFlags[keyWithWorld] = existing + ", " + item;
                            }
                            else
                            {
                                log.WorldFlags[keyWithWorld] = item;
                            }
                            break;
                        }

                        if (!string.IsNullOrEmpty(line) && !char.IsWhiteSpace(line[0]) && !t.StartsWith("-"))
                        {
                            multilineKey = null;
                        }

                        if (t.Contains(':'))
                        {
                            var parts = t.Split(':', 2);
                            var key = parts[0].Trim();
                            var value = parts[1].Trim();

                            if (key == "Gonon Trials") key = "Ganon Trials";

                            var keyWithWorld = $"{currentWorld} {key}";
                            log.WorldFlags[keyWithWorld] = value;

                            multilineKey = string.IsNullOrEmpty(value) ? key : null;
                        }
                        break;

                    case "Entrances":
                        if (Regex.IsMatch(t, @"^World\s+\d+$"))
                        {
                            currentWorld = t;
                            break;
                        }
                        if (t.Contains("->"))
                        {
                            var parts = t.Split(new[] { "->" }, 2, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2)
                            {
                                var from = parts[0].Trim();
                                var to = parts[1].Trim();
                                string fullKey = string.IsNullOrEmpty(currentWorld) ? from : $"{currentWorld} {from}";
                                log.Entrances[fullKey] = to;
                            }
                        }
                        break;

                    case "SongEvents":
                        if (Regex.IsMatch(t, @"^World\s+\d+$"))
                        {
                            currentWorld = t;
                            currentGame = "";
                            break;
                        }
                        if (t == "Ocarina of Time" || t == "Majora's Mask")
                        {
                            currentGame = t;
                            break;
                        }
                        if (t.Contains(':'))
                        {
                            var parts = t.Split(':', 2);
                            if (parts.Length == 2)
                            {
                                var key = parts[0].Trim().Trim('"');
                                var value = parts[1].Trim().Trim('"');
                                string fullKey = string.IsNullOrEmpty(currentWorld) ? key : $"{currentWorld} {key}";
                                log.SongEvents[fullKey] = value;
                            }
                        }
                        break;

                    case "LocationList":
                        if (Regex.IsMatch(t, @"^World\s+\d+\s+\(\d+\)$"))
                        {
                            var m = Regex.Match(t, @"^World\s+(\d+)");
                            if (m.Success) currentWorld = $"World {m.Groups[1].Value}";
                            break;
                        }

                        if (t.EndsWith("):") && Regex.IsMatch(t, @"\(\d+\):$"))
                        {
                            var m = Regex.Match(t, @"^(.+?)\s+\(\d+\):$");
                            if (m.Success) region = m.Groups[1].Value.Trim();
                            break;
                        }

                        if (t.Contains(":"))
                        {
                            ParseLocation(t, region, currentWorld, log);
                        }
                        break;
                }
            }

            return log;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void ParseKeyValue(string trimmedLine, Dictionary<string, string> target)
        {
            if (!trimmedLine.Contains(':')) return;
            var parts = trimmedLine.Split(':', 2);
            target[parts[0].Trim()] = parts[1].Trim();
        }

        private static void ParseLocation(string trimmedLine, string currentRegion, string currentWorld, SpoilerLog log)
        {
            var m = Regex.Match(trimmedLine, @"^(MM|OOT)\s+(.+?):\s+(.+)$");
            if (!m.Success) return;

            var game = m.Groups[1].Value;
            var locationName = m.Groups[2].Value.Trim();
            var item = NormalizeItemName(m.Groups[3].Value.Trim());
            var fullKey = $"{currentWorld} {game} {locationName}";

            if (log.Locations.ContainsKey(fullKey)) return;

            var region = (locationName == "Gerudo Member Card" &&
                          (string.IsNullOrEmpty(currentRegion) || currentRegion == "NONE"))
                        ? "Thieves' Hideout"
                        : (string.IsNullOrEmpty(currentRegion) ? "Other" : currentRegion);

            log.Locations[fullKey] = new LocationItem
            {
                World = currentWorld,
                Location = locationName,
                Item = item,
                Game = game,
                Region = RegionMapper.GetMappedRegion(region)
            };
        }

        private static string NormalizeItemName(string item)
        {
            if (item.StartsWith("Sould of ", StringComparison.OrdinalIgnoreCase))
                item = "Soul of " + item["Sould of ".Length..];

            item = item.Replace("Wolfoses", "Wolfos");
            item = Regex.Replace(item, @"(\w)\(", "$1 (");
            return item;
        }
    }
}
