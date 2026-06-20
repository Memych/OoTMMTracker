using System.Collections.Generic;

namespace OoTMMTracker.Models
{
    /// <summary>
    /// Top-level region (e.g. "Death Mountain Crater").
    /// Contains one or more sub-maps (e.g. "Crater", "Grottos", "Fairy Fountain").
    /// </summary>
    public class MapRegion
    {
        public string Name { get; set; } = "";
        public string Game { get; set; } = "";   // "OOT" or "MM"
        public List<MapSubRegion> SubRegions { get; set; } = new();

        public override string ToString() => Name;
    }

    /// <summary>
    /// A single sub-map within a region (one background image + its marks).
    /// </summary>
    public class MapSubRegion
    {
        public string Name { get; set; } = "";

        /// <summary>
        /// Path to background image relative to Resources/Images/.
        /// E.g. "region maps/OoT/Death_Mountain_Crater/Crater.png"
        /// </summary>
        public string BackgroundImage { get; set; } = "";

        public List<MapMark> Marks { get; set; } = new();

        /// <summary>
        /// OoTMM spoiler entrance ids from the <b>To</b> (right) side of <c>Entrances</c> — the token inside the last
        /// <c>(...)</c> on that side. When the randomized exit points to one of these ids, navigation and the
        /// entrance-mark tooltip resolve to this sub-map. Configure alongside <see cref="MapMark.EntranceFromId"/>
        /// on marks, which matches the <b>From</b> (left) side.
        /// </summary>
        public List<string>? DestinationEntranceIds { get; set; }

        /// <summary>
        /// Optional: spoiler log setting key that must match a value for this sub-map to appear.
        /// E.g. RequiredSettingKey = "Majora's Mask JP Layouts", RequiredSettingValue = "all"
        /// If null — always shown.
        /// RequiredSettingContains — if set, checks if the value contains this string (for pipe-separated lists).
        /// </summary>
        public string? RequiredSettingKey { get; set; } = null;
        public string? RequiredSettingValue { get; set; } = null;
        public string? RequiredSettingContains { get; set; } = null;

        public override string ToString() => Name;
    }

    /// <summary>
    /// A single mark icon positioned on a sub-map.
    /// Coordinates are in original (unscaled) image pixels.
    /// Mark is hidden when ALL linked locations are checked off.
    ///
    /// HOW TO POSITION MARKS:
    ///   Open the background PNG in any image editor, hover over the spot
    ///   where you want the mark center, note the X,Y pixel coordinates,
    ///   and enter them below. The mark will scale automatically with the map.
    /// </summary>
    public class MapMark
    {
        /// <summary>
        /// Path to mark icon relative to Resources/Images/.
        /// Use icons from marks/ folder, e.g. "marks/Chest.png"
        /// </summary>
        public string IconPath { get; set; } = "";

        /// <summary>X center position in original image pixels.</summary>
        public int X { get; set; }

        /// <summary>Y center position in original image pixels.</summary>
        public int Y { get; set; }

        /// <summary>Icon size in original image pixels (square). Default 24.</summary>
        public int Size { get; set; } = 24;

        /// <summary>
        /// Spoiler log location names linked to this mark (without "OOT"/"MM" prefix).
        /// Mark hides when ALL listed locations are checked off.
        /// </summary>
        public List<string> LocationNames { get; set; } = new();

        public string Tooltip { get; set; } = "";

        /// <summary>
        /// Age/world-state filter for marks shown on the map: "both" (default), or a single state that must match the toolbar toggle.
        /// OoT: "child" | "adult". Majora's Mask: "cursed" | "cleared".
        /// Applies to regular marks and to entrance shuffle marks (<see cref="EntranceFromId"/>).
        /// </summary>
        public string AgeFilter { get; set; } = "both";

        /// <summary>
        /// Optional: spoiler log setting key/value required for this mark to appear.
        /// RequiredSettingValue — exact match.
        /// RequiredSettingContains — match within pipe-separated list.
        /// RequiredSettingInvert — if true, mark shown when condition does NOT match.
        /// </summary>
        public string? RequiredSettingKey { get; set; } = null;
        public string? RequiredSettingValue { get; set; } = null;
        public string? RequiredSettingContains { get; set; } = null;
        public bool RequiredSettingInvert { get; set; } = false;

        /// <summary>
        /// Spoiler internal id for the <b>From</b> (left) side of this entrance — the token inside the matching ( ) in the Entrances key.
        /// Mark is visible only if the loaded spoiler has that row. The <b>To</b> (right) side is resolved via
        /// <see cref="MapSubRegion.DestinationEntranceIds"/> on the destination sub-map; right-click jumps there.
        /// </summary>
        public string? EntranceFromId { get; set; }

        public bool IsEntranceShuffleMark => !string.IsNullOrEmpty(EntranceFromId);

        public bool IsAccessible { get; set; } = true;
    }
}
