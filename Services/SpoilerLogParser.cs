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
        public SpoilerLog Parse(string filePath)
        {
            var spoilerLog = new SpoilerLog();
            var lines = File.ReadAllLines(filePath);
            
            string currentSection = "";
            string currentRegion = "";
            string currentSpecialCondition = "";
            string? currentWorldFlagKey = null;
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmedLine = line.Trim();
                
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;
                
                // Parse main sections
                if (trimmedLine.StartsWith("Seed:"))
                {
                    spoilerLog.Seed = trimmedLine.Substring(5).Trim();
                }
                else if (trimmedLine.StartsWith("Version:"))
                {
                    spoilerLog.Version = trimmedLine.Substring(8).Trim();
                }
                else if (trimmedLine == "Settings")
                {
                    currentSection = "Settings";
                }
                else if (trimmedLine == "Special Conditions")
                {
                    currentSection = "SpecialConditions";
                }
                else if (trimmedLine == "Tricks")
                {
                    currentSection = "Tricks";
                }
                else if (trimmedLine == "Starting Items")
                {
                    currentSection = "StartingItems";
                }
                else if (trimmedLine == "World Flags")
                {
                    currentSection = "WorldFlags";
                }
                else if (trimmedLine == "Entrances")
                {
                    currentSection = "Entrances";
                }
                else if (trimmedLine.StartsWith("Location List"))
                {
                    currentSection = "LocationList";
                }
                else if (currentSection == "Settings" && trimmedLine.Contains(":"))
                {
                    var parts = trimmedLine.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();
                        
                        spoilerLog.Settings[key] = value;
                    }
                }
                else if (currentSection == "SpecialConditions" && line.StartsWith("  ") && !line.StartsWith("    ") && trimmedLine.EndsWith(":"))
                {
                    // Parse condition name (BRIDGE, MOON, LACS, GANON_BK, MAJORA)
                    currentSpecialCondition = trimmedLine.TrimEnd(':');
                    if (!spoilerLog.SpecialConditions.ContainsKey(currentSpecialCondition))
                    {
                        spoilerLog.SpecialConditions[currentSpecialCondition] = new SpecialCondition
                        {
                            Name = currentSpecialCondition
                        };
                    }
                }
                else if (currentSection == "SpecialConditions" && line.StartsWith("    ") && trimmedLine.Contains(":"))
                {
                    // Parse condition requirements
                    var parts = trimmedLine.Split(':', 2);
                    if (parts.Length == 2 && !string.IsNullOrEmpty(currentSpecialCondition))
                    {
                        spoilerLog.SpecialConditions[currentSpecialCondition].Requirements[parts[0].Trim()] = parts[1].Trim();
                    }
                }
                else if (currentSection == "Tricks" && line.StartsWith("  ") && !string.IsNullOrWhiteSpace(trimmedLine))
                {
                    spoilerLog.Tricks.Add(trimmedLine);
                }
                else if (currentSection == "StartingItems" && trimmedLine.Contains(":"))
                {
                    var parts = trimmedLine.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        spoilerLog.StartingItems[parts[0].Trim()] = parts[1].Trim();
                    }
                }
                else if (currentSection == "WorldFlags" && trimmedLine.Contains(":"))
                {
                    var parts = trimmedLine.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();
                        spoilerLog.WorldFlags[key] = value;
                        // If value is empty — this is the start of a multiline list
                        currentWorldFlagKey = string.IsNullOrEmpty(value) ? key : null;
                    }
                }
                else if (currentSection == "WorldFlags" && line.StartsWith("    - ") && currentWorldFlagKey != null)
                {
                    // List line — add to current key via |
                    var item = trimmedLine.TrimStart('-').Trim();
                    var existing = spoilerLog.WorldFlags[currentWorldFlagKey];
                    spoilerLog.WorldFlags[currentWorldFlagKey] = string.IsNullOrEmpty(existing)
                        ? item
                        : existing + "|" + item;
                }
                else if (currentSection == "WorldFlags" && !line.StartsWith("    "))
                {
                    // Line not from list — reset current key
                    currentWorldFlagKey = null;
                }
                else if (currentSection == "Entrances" && trimmedLine.Contains("->"))
                {
                    var parts = trimmedLine.Split("->", 2);
                    if (parts.Length == 2)
                    {
                        var from = parts[0].Trim();
                        var to = parts[1].Trim();
                        spoilerLog.Entrances[from] = to;
                    }
                }
                else if (currentSection == "LocationList" && line.StartsWith("  ") && !line.StartsWith("    ") && trimmedLine.Contains("(") && trimmedLine.EndsWith("):"))
                {
                    // Parse region name
                    // Format: "  Region Name (123):"
                    var regionMatch = Regex.Match(trimmedLine, @"^(.+?)\s+\(\d+\):$");
                    if (regionMatch.Success)
                    {
                        currentRegion = regionMatch.Groups[1].Value.Trim();
                    }
                }
                else if (currentSection == "LocationList" && line.StartsWith("    ") && trimmedLine.Contains(":"))
                {
                    // Parse locations with items from Location List
                    // Format: "    OOT Location Name: Item Name" or "    MM Location Name: Item Name"
                    var match = Regex.Match(trimmedLine, @"^(MM|OOT)\s+(.+?):\s+(.+)$");
                    if (match.Success)
                    {
                        var game = match.Groups[1].Value;
                        var location = match.Groups[2].Value.Trim();
                        var item = match.Groups[3].Value.Trim();
                        
                        var key = $"{game} {location}";
                        if (!spoilerLog.Locations.ContainsKey(key))
                        {
                            // Special case: Gerudo Member Card should be in Thieves' Hideout
                            var region = currentRegion;
                            if (location == "Gerudo Member Card" && (string.IsNullOrEmpty(region) || region == "NONE"))
                            {
                                region = "Thieves' Hideout";
                            }
                            
                            spoilerLog.Locations[key] = new LocationItem
                            {
                                Location = location,
                                Item = item,
                                Game = game,
                                Region = RegionMapper.GetMappedRegion(region)
                            };
                        }
                    }
                }
            }
            
            return spoilerLog;
        }
    }
}
