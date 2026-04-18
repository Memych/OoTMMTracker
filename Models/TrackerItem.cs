using System;

namespace OoTMMTracker.Models
{
    public class TrackerItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TrackerItemType Type { get; set; }
        public int MaxCount { get; set; } = 1;
        public int CurrentCount { get; set; } = 0;
        // Minimum value — cannot click below (for wallet without Child = 1)
        public int MinCount { get; set; } = 0;
        public string[]? StepNames { get; set; }
        public bool AlwaysCollected { get; set; } = false;
        
        // Path to icon relative to Resources/Images/ folder
        // Example: "equipment/kokiri_sword_oot.png"
        public string? IconPath { get; set; }
        // Icons for each progression step
        public string[]? StepIconPaths { get; set; }
        // Text on icon for each step (e.g. "8", "99", "200")
        public string[]? StepLabels { get; set; }
        // If true — item is considered collected only when CurrentCount == MaxCount
        public bool CollectedWhenFull { get; set; } = false;
        // Minimum count for "partial" lighting (for Goron Lullaby = 6)
        public int PartialCollectedAt { get; set; } = 0;
        // Permanent label on icon (always visible, regardless of state)
        public string? StaticLabel { get; set; } = null;
        // Override icon size (0 = use standard ItemSize)
        public int CustomSize { get; set; } = 0;
        // If true — item is not clickable, lights up when CurrentCount >= MaxCount
        public bool IsAutoKey { get; set; } = false;
        // Threshold for AutoKey: lights up when CurrentCount >= AutoKeyThreshold (0 = use MaxCount)
        public int AutoKeyThreshold { get; set; } = 0;

        // ── Bottle ───────────────────────────────────────────────────────────────
        // If true — item is a bottle (special click behavior)
        public bool IsBottle { get; set; } = false;
        // Content key for bottle (e.g. "milk", "letter", "empty")
        public string BottleContent { get; set; } = "empty";
        // Available content icons/names for this bottle type
        public string[]? BottleContentIcons { get; set; }
        public string[]? BottleContentNames { get; set; }
        // List of possible rewards (icons), index 0 = "?" (unknown)
        public string[]? RewardIcons { get; set; } = null;
        public string[]? RewardNames { get; set; } = null;
        // Currently selected reward index (0 = ?)
        public int RewardIndex { get; set; } = 0;
        // Is dungeon collected (left click)
        public bool DungeonCleared { get; set; } = false;

        public bool IsDungeonReward => RewardIcons != null;

        public bool IsCollected => AlwaysCollected || (IsDungeonReward ? DungeonCleared : (CollectedWhenFull ? CurrentCount >= MaxCount : CurrentCount > 0));
        // For AutoKey: lights up when threshold is reached
        public bool AutoKeyLit => IsAutoKey && CurrentCount >= (AutoKeyThreshold > 0 ? AutoKeyThreshold : MaxCount);        public string CurrentStepName => StepNames != null && CurrentCount > 0 && CurrentCount <= StepNames.Length
            ? StepNames[CurrentCount - 1]
            : Name;

        public void Increment()
        {
            if (!AlwaysCollected && CurrentCount < MaxCount)
                CurrentCount++;
        }

        public void Decrement()
        {
            if (!AlwaysCollected && CurrentCount > MinCount)
                CurrentCount--;
        }

        public void Reset()
        {
            if (!AlwaysCollected)
            {
                CurrentCount = MinCount;
                if (IsDungeonReward) { DungeonCleared = false; RewardIndex = 0; }
            }
        }
    }
    
    public enum TrackerItemType
    {
        Equipment,
        Mask,
        Song,
        Item,
        Dungeon,
        TradeQuest,
        Soul
    }
}
