# Changelog

## v1.2.5

### New Features
- Added full support for MM Song Events Shuffle (Majora's Mask)
- Added cross-game song support (e.g. Song of Healing in OoT, Zelda's Lullaby in MM)
- Added configurable shared song options for cross-game songs
- Expanded Song Events tab with full MM Song Events Shuffle support
- Updated song list in Song Events tab to include all OoT and MM songs

### Improvements
- Improved UI responsiveness in Song Events tab when toggling shuffle settings
- Refactored song configuration parsing for better maintainability
- Fixed Song Events dropdowns remaining editable when shuffle is disabled

### Bug Fixes
- Fixed compilation errors in custom ComboBox drawing code

## v1.2.4

### New Features
- Added MM Boomerang item support
- Added OoT Kamaro Mask item support

### Improvements
- Fixed zoom percentage counter display during map scrolling
- Removed blue selection highlight from map filter dropdowns
- Map filter dropdowns now maintain consistent appearance without visual selection indicators

### Bug Fixes
- Fixed map filter dropdowns showing blue highlight when selecting regions/sub-regions
- Improved visual consistency of filter controls

## v1.2.3

### New Features
- Added region completion indicators in map filters - shows which regions are fully cleared

### Improvements
- Region and sub-region filters now display completion status with visual indicators
- Fully completed regions show with green checkmark (✓) and green text
- Partially completed regions show progress counter (found/total)
- Completion indicators update dynamically when marking/unmarking locations
- Indicators respect Colors mode - exclude consumables/traps from counts when enabled
- Text color changed to black for better visibility on light backgrounds

### Bug Fixes
- Fixed adult/child filter persistence during map transitions via entrance markers
- Fixed compilation errors in custom ComboBox drawing code
- Improved refresh logic for completion indicators across all progress operations

## v1.2.2

### New Features
- Added vanilla entrance markers display - entrance icons now show even without spoiler log
- Enhanced entrance navigation to work without loaded spoiler log
- Improved tooltips for entrance markers showing destination in "Region (Sub-region)" format

### Improvements
- Entrance markers are now always visible regardless of spoiler log presence
- Navigation to vanilla destinations works without requiring spoiler log
- Tooltips for shuffled entrances show destination region and sub-region
- Tooltips for vanilla entrances show "Vanilla: Region (Sub-region)" format
- Better error messages when entrance destination is not configured

### Bug Fixes
- Fixed MM NPC order in soul tracking
- Fixed item marker display issues
- Fixed entrance navigation requiring spoiler log even for vanilla connections
- Corrected tooltip display for entrance markers with missing destination configuration

## v1.2.1

### New Features
- Added comprehensive entrance shuffle support for all regions
- Entrance markers (ME) now show exits FROM current regions
- DestinationEntranceIds list entrances TO current regions
- Right-click on entrance markers jumps to destination maps
- Quick Jump supports warp songs, owls, and wallmasters as entrances

### Improvements
- Updated map scales for better visibility and navigation
- Enhanced InfoForm documentation with detailed entrance shuffle information
- Added "Entrances" section to documentation
- Improved map marker placement accuracy

### Bug Fixes
- Fixed entrance marker connections for all OoT and MM non-dungeon regions
- Corrected DestinationEntranceIds for proper entrance tracking
- Resolved map navigation issues between regions

## v1.2.0

### New Features
- Added comprehensive map support for all regions
- Enhanced map visualization with detailed region boundaries
- New map tracking panel for better navigation

### Improvements
- Application window size and layout optimizations
- Improved window resizing behavior across all forms
- Better handling of different screen resolutions
- Enhanced UI responsiveness during window operations
- Updated default window positions and sizes for better user experience

### Bug Fixes
- Fixed window size persistence issues
- Resolved layout problems when resizing application windows
- Fixed map display issues in various regions
- Corrected window positioning on multi-monitor setups
- Addressed minor UI glitches in tracker panels

### Credits
- Added attribution for **OoTMM Combo Tracker by Loupimo** as source for map images and region stamps

## v1.1.1

### Improvements
- Default icon size changed to 48×48 (was 34×34)
- Left panel width fixed to match default icon size
- Zoom controls (−/+/↺) added to main window with current size label
- Broadcast window: independent zoom controls (−/+/↺) with own size label
- Broadcast window: layout now updates dynamically when resizing (no need to reopen)
- Broadcast window: zoom changes apply immediately without reopening
- Application icon updated to Triforce
- F7 hotkey added to open/focus Broadcast window
- Info bar moved to second row to avoid overlapping with buttons

### Bug Fixes
- Fixed soul icon paths (were pointing to non-existent subdirectories)
- Removed Spring Water and Hot Spring Water from OoT bottle options (MM-exclusive)
- Fixed Ganon's Boss Key tooltip not updating when Special Conditions change

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
