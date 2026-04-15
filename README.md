# OoTMM Tracker

A tracker application for OoTMM (Ocarina of Time + Majora's Mask) randomizer.

## Features

### Spoiler Log Viewer
- Load Spoiler Log files (.txt)
- View all locations, items, settings, and entrances
- Search locations and items
- Filter by game (OoT/MM) and regions
- Cascading filters (selecting a game updates the region list)
- Mark found locations
- Drag & Drop file loading

### Interactive Tracker
- Visual item tracker with icons
- Support for all OoT and MM items
- Counters for collectibles (Skulltulas, Fairies, Coins, Triforce)
- Dungeon tracking (maps, compasses, keys, boss keys)
- Automatic configuration based on Spoiler Log
- Save and load progress
- Apply Starting Items from log

### Tracker Settings
- Full customization of displayed items
- Support for shared items between games
- Dungeon settings (MQ, Key Rings, Silver Rupees)
- Triforce Hunt/Quest support
- Special Conditions for Ganon Boss Key
- And much more

## Installation

### Option 1: Portable (Recommended)
1. Download `OoTMMTracker.exe` from [Releases](../../releases)
2. Run the file - no .NET installation required

### Option 2: Build from Source
1. Install [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or higher
2. Clone the repository:
   ```bash
   git clone https://github.com/YOUR_USERNAME/OoTMMTracker.git
   cd OoTMMTracker
   ```
3. Build the project:
   ```bash
   dotnet build -c Release
   ```
4. Run:
   ```bash
   dotnet run -c Release
   ```

## Usage

1. Launch the application
2. Click "Load Spoiler Log" or drag & drop a .txt file into the window
3. Use tabs to view locations, settings, and entrances
4. Use the tracker to track collected items
5. Save progress using the "Save" button

## Building Single-File Executable

To create a portable version:
```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

Output: `bin/Release/net6.0-windows/win-x64/publish/OoTMMTracker.exe`

## Requirements

- Windows 10/11 (x64)
- .NET 6.0 Runtime (for regular build) or portable version (no .NET required)

## Technologies

- C# / .NET 6.0
- Windows Forms
- JSON for settings storage

## Resources & Credits

### Item Icons
All item icons are sourced from another OoTMM tracker project:
- **Source**: [OoTMM Randomizer Item Tracker by ariahks](https://github.com/ariahks/ootmmrando_ariahks)
- **Original Tracker**: EmoTracker package for OoTMM Randomizer
- **Original Assets**: The Legend of Zelda: Ocarina of Time and Majora's Mask (Nintendo Co., Ltd.)
- **Usage**: Icons are used under fair use for non-commercial fan project purposes
- **Location**: `Resources/Images/` directory contains icons for items, dungeons, songs, masks, and other game elements

### Special Thanks
- **ariahks** - For the EmoTracker OoTMM package and icon resources
- **OoTMM Development Team** - For creating the amazing randomizer
- **Nintendo** - For the original games and assets
- **OoT/MM Speedrunning Community** - For inspiration and feedback

## License

This project is licensed under the MIT License - see below for details.

**Note**: Game assets (icons, images) are property of Nintendo and are used under fair use for non-commercial fan project purposes. This tracker is not affiliated with or endorsed by Nintendo.

```
MIT License

Copyright (c) 2026 Memych

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## Development

Project uses Visual Studio 2022 or VS Code with C# extension.

### Project Structure
- `Forms/` - UI forms (MainForm, TrackerOptionsForm)
- `Models/` - Data models (SpoilerLog, TrackerConfig, TrackerItem)
- `Services/` - Services (log parser, region mapping, item generation)
- `Resources/Images/` - Item icons and visual assets

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Contact

*(your contact info or Discord/Reddit link)*
