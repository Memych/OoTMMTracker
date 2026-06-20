# Changelog

## v1.3.0

### New Features
- Added full support for multiworld seeds
- Player/World filter added to the tracker — switch between worlds to see each player's progress
- Item tracker rebuilds automatically when a different world is selected
- Locations list, world flags, entrances, and song events are filtered by the selected world
- Starting items are correctly applied per-player (e.g. "Player 1: Hookshot", "Player 2: Bow")
- Bottles are distributed by owner — OoT and MM bottles are only assigned to the appropriate player
- Map logic evaluator (location/entrance accessibility) is scoped to the current world
- Map region/sub-region counters and completion indicators respect the active world
- Save files store per-world progress; progress keys are namespaced as `World N|<item>`
- Added MM Slingshot to the item tracker (with configurable shared/separate display)

### Improvements
- Reduced redundant work on log load and tracker rebuild
- Tracker is now rebuilt with `resetProgress: true` on log load, fully clearing stale state from the previous log (found locations, item counts, per-panel progress)
- Unloading a log (F6) also rebuilds the tracker to drop state derived from the previous log
- `MapAccessibleLocations` runs against a single world snapshot to avoid recomputing the full known-location set on every change
- Location cache and per-world index are built once on log load instead of on every filter change
- Map region/sub-region draw handlers reuse precomputed totals and found counts

### Bug Fixes
- Fixed item tracker not resetting when loading a new spoiler log — panels kept marks/state from the previous log
- Fixed item tracker not resetting when unloading a log via "Reset Log" (F6)
- Fixed per-world progress bleed-over when switching between worlds in multiworld seeds
- Fixed bottle assignment assuming single-world even when item names were namespaced with `Player N`
- Fixed map accessible-locations list being computed across all worlds instead of the active one
  
## v1.2.7

### New Features
- Added Great Fairy Sword support for OoT (gfsOot setting)
- Added Powder Keg support for OoT (powderKegOot setting)
- Added Rusty Keys support for OoT (rustyKeysOot setting) - 27 different key types
- Added Rusty Keys support for MM (rustyKeysMm setting) - 39 different key types, added preemptively
- Added shared properties for Great Fairy Sword (sharedGfs) and Powder Keg (sharedPowderKeg)
- Rusty Keys now displayed as two separate blocks: "Rusty Keys (OoT)" and "Rusty Keys (MM)"
- All Rusty Keys sorted alphabetically within their respective blocks
- Rusty Keys icons now have text labels for easy identification

### Improvements
- Updated AllItemsShared property to include SharedGfs and SharedPowderKeg
- Great Fairy Sword and Powder Keg logic updated to respect shared properties
- MM Fairy Sword and Powder Keg now respect SharedGfs and SharedPowderKeg properties
- Enhanced GetRustyKeysOot and GetRustyKeysMm methods with alphabetical sorting and StaticLabel support
- Updated TrackerOptionsForm with new checkboxes for all added features
- Improved item tracking logic for shared/exclusive item display

### Bug Fixes
- Fixed shared properties initialization in FromSpoilerLog method
- Corrected checkbox linking in UI for shared properties
- Updated BtnOk_Click method to save all new configuration properties
- Fixed item display logic when shared properties are enabled

## v1.2.6

### New Features
- Added Song Events list display without spoiler log - list now shows even when no log is loaded
- Song Events now includes "Ikana Canyon - Ghost Hut" location for Song of Healing

### Improvements
- Fixed checkbox state preservation between OoT and MM games - separate checkboxes for OoT (Adult/Child) and MM (Cursed/Cleared)
- Checkbox states now persist when navigating between regions via entrance shuffle marks
- Empty sub-regions (0 marks out of 0) now display as completed (green checkmark)
- Updated completion indicators for regions and sub-regions to show as completed when all marks are collected, including empty regions
- Enhanced Song Events UI with better error handling and null checks
- Improved item tracker text labels visibility - added outline effect for better contrast against icons

### Bug Fixes
- Fixed application startup issue caused by Song Events initialization
- Fixed DataGridView cell access errors in Song Events population
- Corrected completion indicator logic for empty regions/sub-regions
- Resolved checkbox state persistence when switching between OoT and MM via entrance navigation
- Fixed progressive clocks display - now starts at 1 of 6 instead of 0 of 6
- 
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
