using System;
using System.Collections.Generic;

namespace OoTMMTracker.Models
{
    public class SpoilerLog
    {
        public string Seed { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public Dictionary<string, string> Settings { get; set; } = new();
        public Dictionary<string, SpecialCondition> SpecialConditions { get; set; } = new();
        public List<string> Tricks { get; set; } = new();
        public Dictionary<string, string> StartingItems { get; set; } = new();
        public Dictionary<string, string> WorldFlags { get; set; } = new();
        public Dictionary<string, LocationItem> Locations { get; set; } = new();
        public Dictionary<string, string> Entrances { get; set; } = new();
    }

    public class SpecialCondition
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string> Requirements { get; set; } = new();
    }

    public class LocationItem
    {
        public string Location { get; set; } = string.Empty;
        public string Item { get; set; } = string.Empty;
        public string Game { get; set; } = string.Empty; // OOT or MM
        public string Region { get; set; } = string.Empty;
    }
}
