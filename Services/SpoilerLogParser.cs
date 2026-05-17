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
        };

        public SpoilerLog Parse(string filePath)
        {
            var log = new SpoilerLog();
            var lines = File.ReadAllLines(filePath);

            string  section      = "";
            string  region       = "";
            string  specialCond  = "";
            string? worldFlagKey = null;

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
                        ParseKeyValue(t, log.StartingItems);
                        break;

                    case "WorldFlags":
                        if (line.StartsWith("    - ") && worldFlagKey != null)
                        {
                            // Multiline list item — append with "|"
                            var item = t.TrimStart('-').Trim();
                            var existing = log.WorldFlags[worldFlagKey];
                            log.WorldFlags[worldFlagKey] = string.IsNullOrEmpty(existing) ? item : existing + "|" + item;
                        }
                        else if (!line.StartsWith("    "))
                        {
                            worldFlagKey = null;
                            if (t.Contains(':'))
                            {
                                var parts = t.Split(':', 2);
                                var key   = parts[0].Trim();
                                var value = parts[1].Trim();
                                log.WorldFlags[key] = value;
                                worldFlagKey = string.IsNullOrEmpty(value) ? key : null;
                            }
                        }
                        break;

                    case "Entrances":
                        if (t.Contains("->"))
                        {
                            var parts = t.Split("->", 2);
                            log.Entrances[parts[0].Trim()] = parts[1].Trim();
                        }
                        break;

                    case "LocationList":
                        if (line.StartsWith("  ") && !line.StartsWith("    ") && t.EndsWith("):"))
                        {
                            // Region header: "  Region Name (123):"
                            var m = Regex.Match(t, @"^(.+?)\s+\(\d+\):$");
                            if (m.Success) region = m.Groups[1].Value.Trim();
                        }
                        else if (line.StartsWith("    "))
                        {
                            ParseLocation(t, region, log);
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

        private static void ParseLocation(string trimmedLine, string currentRegion, SpoilerLog log)
        {
            var m = Regex.Match(trimmedLine, @"^(MM|OOT)\s+(.+?):\s+(.+)$");
            if (!m.Success) return;

            var game     = m.Groups[1].Value;
            var location = m.Groups[2].Value.Trim();
            var item     = NormalizeItemName(m.Groups[3].Value.Trim());
            var key      = $"{game} {location}";

            if (log.Locations.ContainsKey(key)) return;

            var region = (location == "Gerudo Member Card" &&
                          string.IsNullOrEmpty(currentRegion) || currentRegion == "NONE")
                ? "Thieves' Hideout"
                : currentRegion;

            log.Locations[key] = new LocationItem
            {
                Location = location,
                Item     = item,
                Game     = game,
                Region   = RegionMapper.GetMappedRegion(region)
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
