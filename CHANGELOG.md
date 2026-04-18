# Changelog

## v1.1.1

### Improvements
- Default icon size changed to 48×48 (was 34×34)
- Left panel width now fixed to match default icon size
- Zoom controls (−/+/↺) added to main window with current size label
- Broadcast window: independent zoom controls (−/+/↺) with own size label
- Broadcast window: layout now updates dynamically when resizing (no need to reopen)
- Broadcast window: zoom changes apply immediately without reopening
- Application icon updated to Triforce

### Bug Fixes
- Fixed soul icon paths (were pointing to non-existent subdirectories)
- Removed Spring Water and Hot Spring Water from OoT bottle options (MM-exclusive)

### Credits
- Added attribution for **OoT Reloaded** and **MM Reloaded** as sources for high-resolution item icons

## v1.1.0

### New Features
- Dungeon Rewards Anywhere — separate "Dungeon Rewards" block when rewards are shuffled outside dungeons
- Song Events — new "Song Events" tab to track which song triggers each location (supports songEventsShuffleOot)
- Bottle System Rework — bottles now show their contents from the spoiler log; right-click to change content; Letter/Gold Dust have 3 states (none/have/used)
- Hotkeys — F1 Load Log, F2 Save, F3 Tracker Options, F4 Load, F5 Reset Progress, F6 Reset Tracker
- Owl Statues — fixed order and added "Pre-activated owls" option
- Tingle Maps — now shown for vanilla and anywhere modes

### Improvements
- Cascading game/region filters in locations list
- Hide Items column toggle
- Location counter (X/Y checked)
- Consumables and traps highlighted in yellow/red in locations list
- Notes mode: hide counter at max, green color at max for other counters
- Starting items now correctly applied on log load
- Filename shown in info bar
- Filters reset when loading new log

### Bug Fixes
- Member Card now correctly placed in Thieves' Hideout region
- Filters reset when loading a new log
- Various starting items mapping fixes

## v1.0.0

### First Release
A tracker application for OoTMM (Ocarina of Time + Majora's Mask) randomizer.

#### Features
- Spoiler log viewer with search and filtering
- Interactive item tracker with visual icons
- Support for all OoT and MM items
- Dungeon tracking (maps, compasses, keys, boss keys)
- Collectibles tracking (Skulltulas, Fairies, Coins, Triforce)
- Save/load progress
- Auto-configuration from spoiler log
