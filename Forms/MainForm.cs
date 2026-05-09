using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using OoTMMTracker.Models;
using OoTMMTracker.Services;
using OoTMMTracker.Controls;

namespace OoTMMTracker.Forms
{
    public class MainForm : Form
    {
        private SpoilerLog? _spoilerLog;
        private readonly SpoilerLogParser _parser;
        
        private Button _btnLoadFile = null!;
        private TextBox _txtSearch = null!;
        private ComboBox _cmbGameFilter   = null!;
        private ComboBox _cmbRegionFilter = null!;
        private ComboBox _cmbFoundFilter  = null!;
        private CheckBox _chkHideItems    = null!;
        private ComboBox _cmbMapRegion    = null!;
        private ComboBox _cmbMapSub       = null!;
        private ComboBox _cmbMapGame      = null!;
        private Action?  _updateMapCounters;
        private CheckBox _chkColorHighlight = null!;
        private Label    _lblCounter      = null!;
        private TabControl _tabControl = null!;
        private DataGridView _dgvLocations = null!;
        private DataGridView _dgvSettings = null!;
        private ListBox _lstTricks = null!;
        private DataGridView _dgvStartingItems = null!;
        private DataGridView _dgvWorldFlags = null!;
        private DataGridView _dgvSpecialConditions = null!;
        private DataGridView _dgvEntrances = null!;
        private DataGridView _dgvSongEvents = null!;
        private Label _lblInfo = null!;
        private Panel _trackerScrollPanel = null!;
        private Panel _leftPanel = null!;
        private Controls.MapTrackerPanel _mapTrackerPanel = null!;
        private TrackerConfig _currentTrackerConfig = new();
        private BroadcastForm? _broadcastForm;
        // Found locations: key = "Game|Location"
        private readonly HashSet<string> _foundLocations = new HashSet<string>();
        private HashSet<string> _knownLocations   = new HashSet<string>();
        private HashSet<string> _coloredLocations = new HashSet<string>();
        private string? _currentSpoilerLogPath;
        // Song events: location name → song name
        private readonly Dictionary<string, string> _songEvents = new();

        private static readonly string SettingsPath = Path.Combine(
            AppContext.BaseDirectory,
            "tracker_settings.json");

        private void SaveTrackerSettings()
        {
            try
            {
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(_currentTrackerConfig,
                    new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }

        private void LoadTrackerSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return;
                var json = File.ReadAllText(SettingsPath);
                // Protection from old format (Dictionary<string,string>)
                if (json.TrimStart().StartsWith("{\"BombchuBehaviorOot\""))
                {
                    File.Delete(SettingsPath);
                    return;
                }
                var cfg = JsonSerializer.Deserialize<TrackerConfig>(json);
                if (cfg != null) _currentTrackerConfig = cfg;
            }
            catch { }
        }

        // Reads hint from log (only if settings file doesn't exist yet)
        private void ApplyBombchuHintFromLog()
        {
            if (File.Exists(SettingsPath)) return; // already have saved settings
            if (_spoilerLog == null) return;
            if (_spoilerLog.Settings.TryGetValue("bombchuBehaviorOot", out var bcOot))
                _currentTrackerConfig.BombchuBehaviorOot = bcOot == "bagSeparate" ? "bag" : bcOot == "bombBag" ? "bombBag" : "toggle";
            if (_spoilerLog.Settings.TryGetValue("bombchuBehaviorMm", out var bcMm))
                _currentTrackerConfig.BombchuBehaviorMm = bcMm == "bagSeparate" ? "bag" : bcMm == "bombBag" ? "bombBag" : "toggle";
            SaveTrackerSettings(); // save as default
        }
        
        public MainForm()
        {
            _parser = new SpoilerLogParser();
            // Set application icon from embedded resource
            try
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var names = asm.GetManifestResourceNames();
                var icoName = names.FirstOrDefault(n => n.EndsWith("app.ico", StringComparison.OrdinalIgnoreCase));
                if (icoName != null)
                {
                    var stream = asm.GetManifestResourceStream(icoName);
                    if (stream != null)
                        this.Icon = new System.Drawing.Icon(stream);
                }
            }
            catch { }
            InitializeComponent();
            LoadTrackerSettings();
            RebuildTracker();
        }
        
        private void InitializeComponent()
        {
            this.Text = "OoTMM Tracker";
            this.Size = new Size(1600, 900);
            this.MinimumSize = new Size(900, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            // Ensure maximized on all DPI settings
            this.Load += (s, e) => { this.WindowState = FormWindowState.Maximized; };
            
            // ── Menu Bar ─────────────────────────────────────────────────────
            var menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(35, 35, 35),
                ForeColor = Color.FromArgb(220, 220, 220),
                RenderMode = ToolStripRenderMode.Professional,
                Renderer = new ToolStripProfessionalRenderer(new DarkMenuColorTable()),
            };

            // File
            var menuFile = new ToolStripMenuItem("File") { ForeColor = Color.FromArgb(220, 220, 220) };
            var miLoadLog = new ToolStripMenuItem("Load Spoiler Log") { ForeColor = Color.FromArgb(220, 220, 220), ShortcutKeyDisplayString = "F1", ShowShortcutKeys = true };
            var miSave    = new ToolStripMenuItem("Save")             { ForeColor = Color.FromArgb(220, 220, 220), ShortcutKeyDisplayString = "F2", ShowShortcutKeys = true };
            var miLoad    = new ToolStripMenuItem("Load")             { ForeColor = Color.FromArgb(220, 220, 220), ShortcutKeyDisplayString = "F4", ShowShortcutKeys = true };            miLoadLog.Click += BtnLoadFile_Click;
            miSave.Click    += BtnSave_Click;
            miLoad.Click    += BtnLoad_Click;
            menuFile.DropDownItems.AddRange(new ToolStripItem[] { miLoadLog, new ToolStripSeparator(), miSave, miLoad });

            // Reset
            var menuReset = new ToolStripMenuItem("Reset") { ForeColor = Color.FromArgb(220, 220, 220) };
            var miResetAll      = new ToolStripMenuItem("Reset All")      { ForeColor = Color.FromArgb(220, 220, 220), ShortcutKeyDisplayString = "F5", ShowShortcutKeys = true };
            var miResetTracker  = new ToolStripMenuItem("Reset Tracker")  { ForeColor = Color.FromArgb(220, 220, 220), ShortcutKeyDisplayString = "F7", ShowShortcutKeys = true };
            var miResetProgress = new ToolStripMenuItem("Reset Progress") { ForeColor = Color.FromArgb(220, 220, 220), ShortcutKeyDisplayString = "F8", ShowShortcutKeys = true };
            var miResetLog      = new ToolStripMenuItem("Reset Log")      { ForeColor = Color.FromArgb(220, 220, 220), ShortcutKeyDisplayString = "F6", ShowShortcutKeys = true };
            miResetAll.Click      += BtnResetAll_Click;
            miResetTracker.Click  += BtnResetTracker_Click;
            miResetProgress.Click += BtnResetProgress_Click;
            miResetLog.Click      += BtnResetLog_Click;
            menuReset.DropDownItems.AddRange(new ToolStripItem[] { miResetAll, new ToolStripSeparator(), miResetTracker, miResetProgress, new ToolStripSeparator(), miResetLog });

            // Tracker
            var menuTracker = new ToolStripMenuItem("Tracker") { ForeColor = Color.FromArgb(220, 220, 220) };
            var miOptions   = new ToolStripMenuItem("Options")    { ForeColor = Color.FromArgb(220, 220, 220), ShortcutKeyDisplayString = "F3",       ShowShortcutKeys = true };
            var miBroadcast = new ToolStripMenuItem("Broadcast")  { ForeColor = Color.FromArgb(220, 220, 220), ShortcutKeyDisplayString = "F9",       ShowShortcutKeys = true };
            var miZoomIn    = new ToolStripMenuItem("Zoom In")    { ForeColor = Color.FromArgb(220, 220, 220), ShortcutKeyDisplayString = "Shift + '+'", ShowShortcutKeys = true };
            var miZoomOut   = new ToolStripMenuItem("Zoom Out")   { ForeColor = Color.FromArgb(220, 220, 220), ShortcutKeyDisplayString = "Shift + '-'", ShowShortcutKeys = true };
            var miZoomReset = new ToolStripMenuItem("Reset Zoom") { ForeColor = Color.FromArgb(220, 220, 220), ShortcutKeyDisplayString = "Shift + '0'", ShowShortcutKeys = true };
            miOptions.Click   += BtnTrackerOptions_Click;
            miBroadcast.Click += (s, e) =>
            {
                if (_broadcastForm == null || _broadcastForm.IsDisposed)
                {
                    _broadcastForm = new BroadcastForm();
                    _lastBroadcastConfig = null;
                    _lastBroadcastItemSize = -1;
                    _broadcastForm.Show(this);
                    UpdateBroadcast();
                }
                else _broadcastForm.Focus();
            };
            miZoomIn.Click += (s, e) =>
            {
                var steps = ItemTrackerPanel.GetItemSizeSteps();
                int cur = Array.IndexOf(steps, ItemTrackerPanel.GetItemSize());
                if (cur < steps.Length - 1) { ItemTrackerPanel.SetItemSize(steps[cur + 1]); RebuildTracker(); }
            };
            miZoomOut.Click += (s, e) =>
            {
                var steps = ItemTrackerPanel.GetItemSizeSteps();
                int cur = Array.IndexOf(steps, ItemTrackerPanel.GetItemSize());
                if (cur > 0) { ItemTrackerPanel.SetItemSize(steps[cur - 1]); RebuildTracker(); }
            };
            miZoomReset.Click += (s, e) => { ItemTrackerPanel.SetItemSize(48); RebuildTracker(); };
            menuTracker.DropDownItems.AddRange(new ToolStripItem[] { miOptions, new ToolStripSeparator(), miBroadcast, new ToolStripSeparator(), miZoomIn, miZoomOut, miZoomReset });

            // About
            var menuAbout = new ToolStripMenuItem("About") { ForeColor = Color.FromArgb(220, 220, 220) };
            var miCredits = new ToolStripMenuItem("Credits") { ForeColor = Color.FromArgb(220, 220, 220) };
            miCredits.Click += (s, e) => new InfoForm().ShowDialog(this);
            menuAbout.DropDownItems.Add(miCredits);

            menuStrip.Items.AddRange(new ToolStripItem[] { menuFile, menuReset, menuTracker, menuAbout });
            this.MainMenuStrip = menuStrip;

            // Info bar — thin strip showing loaded log info
            var infoBar = new Panel { Dock = DockStyle.Top, Height = 20, BackColor = Color.FromArgb(35, 35, 35) };
            _lblInfo = new Label { Dock = DockStyle.Fill, Text = "No file loaded", ForeColor = Color.FromArgb(160, 160, 160), Font = new Font("Segoe UI", 8f), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0) };
            infoBar.Controls.Add(_lblInfo);

            // Search bar — initialized later inside rightContainer
            
            // Right panel (Fill) — add first
            var rightContainer = new Panel { Dock = DockStyle.Fill, BackColor = SystemColors.Control };
            this.Controls.Add(rightContainer);

            // Left panel (Left) — add second
            // Width fixed for default 48px icon size: PadX*2 + 10cols*48 + 9gaps*2 + scrollbar + margin = ~540
            _leftPanel = new Panel { Dock = DockStyle.Left, Width = 540, BackColor = Color.FromArgb(30, 30, 30) };
            this.Controls.Add(_leftPanel);

            // MenuStrip and infoBar — add last so they dock to top above everything
            this.Controls.Add(infoBar);
            this.Controls.Add(menuStrip);

            // Tracker inside left panel
            _trackerScrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(30, 30, 30) };
            _leftPanel.Controls.Add(_trackerScrollPanel);
            RebuildTracker();
            // Right panel: search + tabs
            var rightPanel = rightContainer;
            // Search panel: outer container with fixed height, inner panel scrolls horizontally
            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(45, 45, 45) };
            var searchInner = new Panel
            {
                Location  = new Point(0, 0),
                Height    = 36,
                Width     = 1210,
                BackColor = Color.FromArgb(45, 45, 45)
            };
            var searchHScroll = new HScrollBar
            {
                Dock    = DockStyle.Bottom,
                Height  = 16,
                Minimum = 0,
                Maximum = 1210,
                SmallChange = 20,
                LargeChange = 200,
            };
            searchHScroll.Scroll += (s, e) =>
            {
                searchInner.Location = new Point(-searchHScroll.Value, 0);
            };
            searchPanel.Controls.Add(searchInner);
            searchPanel.Controls.Add(searchHScroll);
            // Hide scrollbar when panel is wide enough, update Maximum dynamically
            searchPanel.Resize += (s, e) =>
            {
                int overflow = searchInner.Width - searchPanel.Width;
                searchHScroll.Visible = overflow > 0;
                searchPanel.Height = searchHScroll.Visible ? 52 : 36;
                if (overflow > 0)
                {
                    searchHScroll.Maximum = overflow + searchHScroll.LargeChange;
                    searchHScroll.Value = Math.Min(searchHScroll.Value, overflow);
                    searchInner.Location = new Point(-searchHScroll.Value, 0);
                }
                else
                {
                    searchInner.Location = new Point(0, 0);
                }
            };            // searchPanel added to rightContainer later (after tabControl)

            // Fill searchPanel
            searchInner.Controls.Add(new Label { Text = "Search:", Location = new Point(6, 10), Size = new Size(50, 20), ForeColor = Color.White });
            _txtSearch = new TextBox { Location = new Point(58, 7), Size = new Size(220, 22) };
            _txtSearch.TextChanged += TxtSearch_TextChanged;
            searchInner.Controls.Add(_txtSearch);
            
            searchInner.Controls.Add(new Label { Text = "Game:", Location = new Point(286, 10), Size = new Size(45, 20), ForeColor = Color.White });
            _cmbGameFilter = new ComboBox { Location = new Point(334, 7), Size = new Size(100, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbGameFilter.Items.AddRange(new object[] { "All", "OoT", "MM" });
            _cmbGameFilter.SelectedIndex = 0;
            _cmbGameFilter.SelectedIndexChanged += (s, e) => 
            {
                PopulateRegionFilter(); // Update region filter based on selected game
                UpdateLocationsList(_txtSearch.Text);
            };
            _cmbGameFilter.Enabled = false; // Disabled until log is loaded with both games
            searchInner.Controls.Add(_cmbGameFilter);
            
            searchInner.Controls.Add(new Label { Text = "Region:", Location = new Point(442, 10), Size = new Size(55, 20), ForeColor = Color.White });
            _cmbRegionFilter = new ComboBox { Location = new Point(500, 7), Size = new Size(220, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbRegionFilter.Items.Add("All Regions");
            _cmbRegionFilter.SelectedIndex = 0;
            _cmbRegionFilter.SelectedIndexChanged += CmbRegionFilter_SelectedIndexChanged;
            searchInner.Controls.Add(_cmbRegionFilter);
            
            searchInner.Controls.Add(new Label { Text = "Status:", Location = new Point(728, 10), Size = new Size(50, 20), ForeColor = Color.White });
            _cmbFoundFilter = new ComboBox { Location = new Point(780, 7), Size = new Size(120, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbFoundFilter.Items.AddRange(new object[] { "All", "Not Found", "Found" });
            _cmbFoundFilter.SelectedIndex = 0;
            _cmbFoundFilter.SelectedIndexChanged += (s, e) => UpdateLocationsList(_txtSearch.Text);
            searchInner.Controls.Add(_cmbFoundFilter);

            _chkHideItems = new CheckBox { Text = "Hide Items", Location = new Point(910, 9), Size = new Size(90, 18), ForeColor = Color.White };
            _chkHideItems.CheckedChanged += (s, e) =>
            {
                _dgvLocations.Columns["Item"]!.Visible = !_chkHideItems.Checked;
                UpdateLocationsList(_txtSearch.Text);
            };
            searchInner.Controls.Add(_chkHideItems);

            _chkColorHighlight = new CheckBox { Text = "Colors", Location = new Point(1008, 9), Size = new Size(70, 18), ForeColor = Color.White, Checked = false };
            _chkColorHighlight.CheckedChanged += (s, e) =>
            {
                UpdateLocationsList(_txtSearch.Text);
                _mapTrackerPanel.SetColorsMode(_chkColorHighlight.Checked);
                UpdateMapColoredLocations();
                _updateMapCounters?.Invoke();
            };
            searchInner.Controls.Add(_chkColorHighlight);

            _lblCounter = new Label { Location = new Point(1086, 10), Size = new Size(120, 18), ForeColor = Color.FromArgb(180, 180, 180), TextAlign = ContentAlignment.MiddleLeft };
            searchInner.Controls.Add(_lblCounter);

            _tabControl = new TabControl { Dock = DockStyle.Fill };            // Tab: Locations
            var tabLocations = new TabPage("Locations");
            _dgvLocations = MakeGrid();
            _dgvLocations.ReadOnly = false; // allow editing for checkboxes
            // Disable blue selection highlight
            _dgvLocations.DefaultCellStyle.SelectionBackColor = _dgvLocations.DefaultCellStyle.BackColor;
            _dgvLocations.DefaultCellStyle.SelectionForeColor = _dgvLocations.DefaultCellStyle.ForeColor;
            // Column with checkbox for marking found
            var chkCol = new DataGridViewCheckBoxColumn
            {
                Name = "Found", HeaderText = "✓",
                Width = 28, Resizable = DataGridViewTriState.False,
                FillWeight = 5, AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ReadOnly = false
            };
            _dgvLocations.Columns.Add(chkCol);
            _dgvLocations.Columns.Add("Game", "Game");
            _dgvLocations.Columns.Add("Region", "Region");
            _dgvLocations.Columns.Add("Location", "Location");
            _dgvLocations.Columns.Add("Item", "Item");
            // Text columns — read only
            _dgvLocations.Columns[1].ReadOnly = true;
            _dgvLocations.Columns[2].ReadOnly = true;
            _dgvLocations.Columns[3].ReadOnly = true;
            _dgvLocations.Columns[4].ReadOnly = true;
            _dgvLocations.Columns[1].FillWeight = 8;
            _dgvLocations.Columns[2].FillWeight = 22;
            _dgvLocations.Columns[3].FillWeight = 35;
            _dgvLocations.Columns[4].FillWeight = 35;
            // Click on checkbox — mark location
            _dgvLocations.CellValueChanged    += DgvLocations_CellValueChanged;
            _dgvLocations.CurrentCellDirtyStateChanged += DgvLocations_DirtyStateChanged;
            _dgvLocations.CellFormatting      += DgvLocations_CellFormatting;
            tabLocations.Controls.Add(_dgvLocations);
            _tabControl.TabPages.Add(tabLocations);
            
            // Tab: Settings
            var tabSettings = new TabPage("Settings");
            var innerTabs = new TabControl { Dock = DockStyle.Fill };
            
            var tabMain = new TabPage("Main");
            _dgvSettings = MakeGrid();
            _dgvSettings.Columns.Add("Setting", "Setting"); _dgvSettings.Columns[0].FillWeight = 40;
            _dgvSettings.Columns.Add("Value", "Value");    _dgvSettings.Columns[1].FillWeight = 60;
            tabMain.Controls.Add(_dgvSettings); innerTabs.TabPages.Add(tabMain);
            
            var tabTricks = new TabPage("Tricks");
            _lstTricks = new ListBox { Dock = DockStyle.Fill };
            tabTricks.Controls.Add(_lstTricks); innerTabs.TabPages.Add(tabTricks);
            
            var tabStart = new TabPage("Starting Items");
            _dgvStartingItems = MakeGrid();
            _dgvStartingItems.Columns.Add("Item", "Item");    _dgvStartingItems.Columns[0].FillWeight = 70;
            _dgvStartingItems.Columns.Add("Qty", "Quantity");  _dgvStartingItems.Columns[1].FillWeight = 30;
            tabStart.Controls.Add(_dgvStartingItems); innerTabs.TabPages.Add(tabStart);
            
            var tabFlags = new TabPage("World Flags");
            _dgvWorldFlags = MakeGrid();
            _dgvWorldFlags.Columns.Add("Flag", "Flag");   _dgvWorldFlags.Columns[0].FillWeight = 60;
            _dgvWorldFlags.Columns.Add("Value", "Value"); _dgvWorldFlags.Columns[1].FillWeight = 40;
            tabFlags.Controls.Add(_dgvWorldFlags); innerTabs.TabPages.Add(tabFlags);
            
            var tabSC = new TabPage("Special Conditions");
            _dgvSpecialConditions = MakeGrid();
            _dgvSpecialConditions.Columns.Add("Condition", "Condition");     _dgvSpecialConditions.Columns[0].FillWeight = 25;
            _dgvSpecialConditions.Columns.Add("Requirement", "Requirement");_dgvSpecialConditions.Columns[1].FillWeight = 40;
            _dgvSpecialConditions.Columns.Add("Value", "Value");        _dgvSpecialConditions.Columns[2].FillWeight = 35;
            tabSC.Controls.Add(_dgvSpecialConditions); innerTabs.TabPages.Add(tabSC);
            
            tabSettings.Controls.Add(innerTabs);
            _tabControl.TabPages.Add(tabSettings);
            
            // Tab: Entrances
            var tabEntrances = new TabPage("Entrances");
            _dgvEntrances = MakeGrid();
            _dgvEntrances.Columns.Add("From", "From"); _dgvEntrances.Columns[0].FillWeight = 50;
            _dgvEntrances.Columns.Add("To", "To");     _dgvEntrances.Columns[1].FillWeight = 50;
            tabEntrances.Controls.Add(_dgvEntrances);
            _tabControl.TabPages.Add(tabEntrances);

            // Tab: Song Events
            var tabSongEvents = new TabPage("Song Events");
            _dgvSongEvents = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                DefaultCellStyle = { SelectionBackColor = Color.FromArgb(45, 45, 45), SelectionForeColor = Color.Empty }
            };
            _dgvSongEvents.CellFormatting += (s, e) =>
            {
                e.CellStyle.SelectionBackColor = e.CellStyle.BackColor == Color.Empty
                    ? Color.FromArgb(45, 45, 45)
                    : e.CellStyle.BackColor;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            };
            _dgvSongEvents.Columns.Add("Location", "Location");
            _dgvSongEvents.Columns[0].FillWeight = 60;
            _dgvSongEvents.Columns[0].ReadOnly = true;
            // Song column — ComboBox
            var songCol = new DataGridViewComboBoxColumn
            {
                Name = "Song", HeaderText = "Song",
                FillWeight = 40,
                FlatStyle = FlatStyle.Flat,
            };
            songCol.Items.AddRange("?", "Zelda's Lullaby", "Epona's Song", "Saria's Song",
                                   "Sun's Song", "Song of Time", "Song of Storms");
            _dgvSongEvents.Columns.Add(songCol);
            _dgvSongEvents.CellValueChanged += DgvSongEvents_CellValueChanged;
            _dgvSongEvents.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (_dgvSongEvents.IsCurrentCellDirty)
                    _dgvSongEvents.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            tabSongEvents.Controls.Add(_dgvSongEvents);
            _tabControl.TabPages.Add(tabSongEvents);

            // Tab: Map
            var tabMap = new TabPage("Map");
            _mapTrackerPanel = new Controls.MapTrackerPanel { Dock = DockStyle.Fill };
            _mapTrackerPanel.MarkCompleted += (locationNames, game) =>
            {
                foreach (var loc in locationNames)
                {
                    var key = $"{game}|{loc}";
                    _foundLocations.Add(key);
                    // Check the corresponding row in the locations grid
                    foreach (DataGridViewRow row in _dgvLocations.Rows)
                    {
                        if (row.Cells["Game"]?.Value?.ToString() == game &&
                            row.Cells["Location"]?.Value?.ToString() == loc)
                        {
                            row.Cells[0].Value = true;
                            ApplyRowColor(row, true);
                        }
                    }
                }
                _mapTrackerPanel.UpdateFoundLocations(_foundLocations);
                _updateMapCounters?.Invoke();
                // Update counter
                int total    = _dgvLocations.Rows.Count;
                int checked_ = _dgvLocations.Rows.Cast<DataGridViewRow>().Count(r => r.Cells[0].Value is true);
                if (_lblCounter != null)
                    _lblCounter.Text = $"{checked_}/{total} checked";
            };

            var mapFilterPanel = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(45, 45, 45) };
            var mapFilterInner = new Panel
            {
                Location  = new Point(0, 0),
                Height    = 32,
                Width     = 970,
                BackColor = Color.FromArgb(45, 45, 45)
            };
            var mapFilterHScroll = new HScrollBar
            {
                Dock        = DockStyle.Bottom,
                Height      = 16,
                Minimum     = 0,
                Maximum     = 760,
                SmallChange = 20,
                LargeChange = 200,
            };
            mapFilterHScroll.Scroll += (s, e) =>
            {
                mapFilterInner.Location = new Point(-mapFilterHScroll.Value, 0);
            };
            mapFilterPanel.Controls.Add(mapFilterInner);
            mapFilterPanel.Controls.Add(mapFilterHScroll);
            mapFilterPanel.Resize += (s, e) =>
            {
                int overflow = mapFilterInner.Width - mapFilterPanel.Width;
                mapFilterHScroll.Visible = overflow > 0;
                mapFilterPanel.Height = mapFilterHScroll.Visible ? 48 : 32;
                if (overflow > 0)
                {
                    mapFilterHScroll.Maximum = overflow + mapFilterHScroll.LargeChange;
                    mapFilterHScroll.Value = Math.Min(mapFilterHScroll.Value, overflow);
                    mapFilterInner.Location = new Point(-mapFilterHScroll.Value, 0);
                }
                else
                {
                    mapFilterInner.Location = new Point(0, 0);
                }
            };
            mapFilterInner.Controls.Add(new Label { Text = "Game:", Location = new Point(6, 8), Size = new Size(42, 18), ForeColor = Color.White });

            _cmbMapGame = new ComboBox
            {
                Location = new Point(50, 5), Size = new Size(80, 22),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbMapGame.Items.AddRange(new object[] { "All", "OoT", "MM" });
            _cmbMapGame.SelectedIndex = 0;
            mapFilterInner.Controls.Add(_cmbMapGame);
            var cmbMapGame = _cmbMapGame;

            mapFilterInner.Controls.Add(new Label { Text = "Region:", Location = new Point(138, 8), Size = new Size(52, 18), ForeColor = Color.White });

            _cmbMapRegion = new ComboBox
            {
                Location = new Point(192, 5), Size = new Size(200, 22),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbMapRegion.Items.Add("— Select —");
            foreach (var r in Services.MapRegionsData.GetAll())
                _cmbMapRegion.Items.Add(r);
            _cmbMapRegion.SelectedIndex = 0;

            // Rebuild region list filtered by game
            void RebuildMapRegionList()
            {
                var selected = _cmbMapRegion.SelectedItem as Models.MapRegion;
                _cmbMapRegion.Items.Clear();
                _cmbMapRegion.Items.Add("— Select —");
                var gameFilter = cmbMapGame.SelectedItem?.ToString() ?? "All";
                foreach (var r in Services.MapRegionsData.GetAll())
                {
                    if (gameFilter == "All" || r.Game == gameFilter.ToUpper())
                        _cmbMapRegion.Items.Add(r);
                }
                if (selected != null && _cmbMapRegion.Items.Contains(selected))
                    _cmbMapRegion.SelectedItem = selected;
                else
                    _cmbMapRegion.SelectedIndex = 0;
            }

            cmbMapGame.SelectedIndexChanged += (s, e) => RebuildMapRegionList();

            mapFilterInner.Controls.Add(_cmbMapRegion);
            mapFilterInner.Controls.Add(new Label { Text = "Sub-map:", Location = new Point(400, 8), Size = new Size(58, 18), ForeColor = Color.White });
            _cmbMapSub = new ComboBox
            {
                Location = new Point(460, 5), Size = new Size(200, 22),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            var cmbMapRegion = _cmbMapRegion;
            var cmbMapSub    = _cmbMapSub;

            // Counters: region total and sub-region
            var lblMapRegionCounter = new Label
            {
                Location = new Point(760, 8), Size = new Size(100, 18),
                ForeColor = Color.FromArgb(180, 180, 180), Text = ""
            };
            var lblMapSubCounter = new Label
            {
                Location = new Point(864, 8), Size = new Size(100, 18),
                ForeColor = Color.FromArgb(180, 180, 180), Text = ""
            };

            // Helper: count found/total for a set of location names (excluding consumables/traps when Colors is on)
            int CountFound(System.Collections.Generic.IEnumerable<string> locationNames, string game)
            {
                int found = 0;
                foreach (var loc in locationNames)
                {
                    if (_chkColorHighlight.Checked && _coloredLocations.Contains($"{game}|{loc}"))
                        continue;
                    if (_foundLocations.Contains($"{game}|{loc}"))
                        found++;
                }
                return found;
            }

            int CountTotal(System.Collections.Generic.IEnumerable<string> locationNames, string game)
            {
                int total = 0;
                foreach (var loc in locationNames)
                {
                    if (_chkColorHighlight.Checked && _coloredLocations.Contains($"{game}|{loc}"))
                        continue;
                    if (_knownLocations.Contains($"{game}|{loc}"))
                        total++;
                }
                return total;
            }

            void UpdateMapCounters()
            {
                if (_spoilerLog == null)
                {
                    lblMapRegionCounter.Text = "";
                    lblMapSubCounter.Text    = "";
                    return;
                }
                if (cmbMapRegion.SelectedItem is Models.MapRegion region)
                {
                    var game = region.Game;
                    var allLocs = region.SubRegions
                        .SelectMany(s => s.Marks)
                        .SelectMany(m => m.LocationNames)
                        .Distinct()
                        .ToList();
                    int total = CountTotal(allLocs, game);
                    int found = CountFound(allLocs, game);
                    lblMapRegionCounter.Text = $"Region: {found}/{total}";

                    if (cmbMapSub.SelectedItem is Models.MapSubRegion sub)
                    {
                        var subLocs = sub.Marks.SelectMany(m => m.LocationNames).Distinct().ToList();
                        int subTotal = CountTotal(subLocs, game);
                        int subFound = CountFound(subLocs, game);
                        lblMapSubCounter.Text = $"Sub: {subFound}/{subTotal}";
                    }
                    else
                    {
                        lblMapSubCounter.Text = "";
                    }
                }
                else
                {
                    lblMapRegionCounter.Text = "";
                    lblMapSubCounter.Text = "";
                }
            }

            cmbMapRegion.SelectedIndexChanged += (s, e) =>
            {
                cmbMapSub.Items.Clear();
                cmbMapSub.Enabled = false;
                _mapTrackerPanel.SetSubRegion(null, _foundLocations);

                if (cmbMapRegion.SelectedItem is Models.MapRegion region)
                {
                    foreach (var sub in region.SubRegions)
                    {
                        // Filter sub-maps by required setting
                        if (sub.RequiredSettingKey != null)
                        {
                            if (_spoilerLog == null) continue;
                            // Check both Settings and WorldFlags
                            string? val = null;
                            if (!_spoilerLog.Settings.TryGetValue(sub.RequiredSettingKey, out val))
                                _spoilerLog.WorldFlags.TryGetValue(sub.RequiredSettingKey, out val);
                            if (val == null) continue;
                            // Check: exact match OR contains in pipe-separated list
                            bool matches = string.Equals(val, sub.RequiredSettingValue, StringComparison.OrdinalIgnoreCase);
                            if (!matches && sub.RequiredSettingContains != null)
                            {
                                var parts = val.Split('|');
                                matches = parts.Any(p => string.Equals(p.Trim(), sub.RequiredSettingContains, StringComparison.OrdinalIgnoreCase));
                            }
                            if (!matches) continue;
                        }
                        cmbMapSub.Items.Add(sub);
                    }
                    if (cmbMapSub.Items.Count > 0)
                    {
                        cmbMapSub.Enabled = true;
                        cmbMapSub.SelectedIndex = 0;
                    }
                }
                UpdateMapCounters();
            };

            cmbMapSub.SelectedIndexChanged += (s, e) =>
            {
                var sub = cmbMapSub.SelectedItem as Models.MapSubRegion;
                var game = (cmbMapRegion.SelectedItem as Models.MapRegion)?.Game ?? "OOT";
                _mapTrackerPanel.SetSubRegion(sub, _foundLocations, game);
                UpdateMapCounters();
            };

            // Hook into location changes to refresh counters
            _dgvLocations.CellValueChanged += (s, e) => UpdateMapCounters();
            _updateMapCounters = UpdateMapCounters;

            mapFilterInner.Controls.Add(cmbMapSub);
            mapFilterInner.Controls.Add(lblMapRegionCounter);
            mapFilterInner.Controls.Add(lblMapSubCounter);

            // Age/state filter — OoT: child/adult, MM: cursed/cleared
            var chkAdult = new CheckBox
            {
                Text      = "Adult",
                Location  = new Point(668, 7),
                Size      = new Size(85, 18),
                ForeColor = Color.White,
                Checked   = false,
                Visible   = false
            };
            chkAdult.CheckedChanged += (s, e) =>
            {
                bool isMm = cmbMapRegion.SelectedItem is Models.MapRegion rm && rm.Game == "MM";
                _mapTrackerPanel.SetAgeFilter(chkAdult.Checked ? (isMm ? "cleared" : "adult") : (isMm ? "cursed" : "child"));
            };
            // Show filter only for OoT or MM regions, update label accordingly
            cmbMapRegion.SelectedIndexChanged += (s2, e2) =>
            {
                if (cmbMapRegion.SelectedItem is Models.MapRegion r)
                {
                    chkAdult.Visible = true;
                    if (r.Game == "MM")
                    {
                        chkAdult.Text = "Cleared";
                        _mapTrackerPanel.SetAgeFilter(chkAdult.Checked ? "cleared" : "cursed");
                    }
                    else
                    {
                        chkAdult.Text = "Adult";
                        _mapTrackerPanel.SetAgeFilter(chkAdult.Checked ? "adult" : "child");
                    }
                }
                else
                {
                    chkAdult.Visible = false;
                    chkAdult.Checked = false;
                    _mapTrackerPanel.SetAgeFilter("both");
                }
            };
            mapFilterInner.Controls.Add(chkAdult);
            tabMap.Controls.Add(_mapTrackerPanel);
            tabMap.Controls.Add(mapFilterPanel);
            _tabControl.TabPages.Add(tabMap);

            // Add tabControl to rightContainer (Fill — first!)
            rightContainer.Controls.Add(_tabControl);
            // searchPanel (Top)
            rightContainer.Controls.Add(searchPanel);
            // Drag & Drop
            this.AllowDrop = true;
            this.DragEnter += MainForm_DragEnter;
            this.DragDrop += MainForm_DragDrop;
            // Hotkeys
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
            // Update broadcast when any item changes
            ItemTrackerPanel.ItemChanged += () => UpdateBroadcast();
            // Unsubscribe on form close to prevent memory leaks
            this.FormClosed += (s, e) =>
            {
                ItemTrackerPanel.ItemChanged -= () => UpdateBroadcast();
                foreach (var panel in _trackerPanels)
                    panel.Dispose();
                _trackerToolTip.Dispose();
            };
        }
        
        private static DataGridView MakeGrid()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
            };
            // Remove selection highlight — match selection color to row background
            dgv.CellFormatting += (s, e) =>
            {
                e.CellStyle.SelectionBackColor = e.CellStyle.BackColor == Color.Empty
                    ? Color.FromArgb(45, 45, 45)
                    : e.CellStyle.BackColor;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            };
            return dgv;
        }
        // ── Tracker ──────────────────────────────────────────────────────────────
        
        private readonly List<ItemTrackerPanel> _trackerPanels = new();
        private readonly ToolTip _trackerToolTip = new();
        
        private void RebuildTracker(bool resetProgress = false)
        {
            var progress = new Dictionary<string, int>();
            // When loading a new log — don't transfer old progress
            if (!resetProgress)
            {
                foreach (var panel in _trackerPanels)
                    panel.SaveProgress(progress);
            }
            
            // Dispose old panels to release event handlers and resources
            foreach (var panel in _trackerPanels)
                panel.Dispose();
            _trackerPanels.Clear();
            _trackerScrollPanel.Controls.Clear();
            ItemTrackerPanel.ClearGlobal();
            // Reset tooltips — otherwise after recreating panels tooltips disappear
            _trackerToolTip.RemoveAll();
            ItemTrackerPanel.SetGlobalConfig(_currentTrackerConfig);
            
            var mainFlow = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(4),
                BackColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 0)
            };
            
            var cfg = _currentTrackerConfig;
            bool hasShared = TrackerItemsData.GetSharedItems(cfg).Count > 0
                          || TrackerItemsData.GetSharedItems_Items(cfg).Count > 0
                          || TrackerItemsData.GetSharedBottles(cfg).Count > 0;

            void Add(string title, List<TrackerItem> items, int cols = 6)
            {
                if (items.Count == 0) return;
                int actualCols = Math.Min(cols, items.Count);
                var p = new ItemTrackerPanel(title, items, actualCols, _trackerToolTip);
                _trackerPanels.Add(p);
                mainFlow.Controls.Add(p);
            }

            // ── 1. Equipment ─────────────────────────────────────────────────
            if (hasShared)
                Add("Equipment (Shared)", TrackerItemsData.GetSharedItems(cfg), 6);
            if (cfg.HasOot) Add("Equipment (OoT)", TrackerItemsData.GetOotEquipmentItems(cfg), 4);
            if (cfg.HasMm)  Add("Equipment (MM)",  TrackerItemsData.GetMmEquipmentItems(cfg), 5);

            // ── 2. Masks ──────────────────────────────────────────────────────
            Add("Masks (Shared)", TrackerItemsData.GetSharedMasks(cfg), 6);
            if (cfg.HasMm)  Add("Masks (MM)",  TrackerItemsData.GetMmMaskItems(cfg), 6);
            if (cfg.HasOot) Add("Masks (OoT)", TrackerItemsData.GetOotMaskItems(cfg), 4);

            // ── 3. Items ───────────────────────────────────────────────────
            if (hasShared)
                Add("Items (Shared)", TrackerItemsData.GetSharedItems_Items(cfg), 7);
            if (cfg.HasOot) Add("Items (OoT)", TrackerItemsData.GetOotItems(cfg), 6);
            if (cfg.HasMm)  Add("Items (MM)",  TrackerItemsData.GetMmItems(cfg), 5);

            // ── 4. Bottles ────────────────────────────────────────────────────
            if (hasShared)
                Add("Bottles (Shared)", TrackerItemsData.GetSharedBottles(cfg), 6);
            if (cfg.HasOot) Add("Bottles (OoT)", TrackerItemsData.GetOotBottles(cfg), 4);
            if (cfg.HasMm)  Add("Bottles (MM)",  TrackerItemsData.GetMmBottles(cfg), 6);

            // ── 5. Trade Quests ───────────────────────────────────────────────
            if (cfg.HasOot) Add("Trade Quests (OoT)", TrackerItemsData.GetOotTradeItems(cfg), 5);
            if (cfg.HasMm)  Add("Trade Quests (MM)",  TrackerItemsData.GetMmTradeItems(), 9);

            // ── 6. Collectibles ──────────────────────────────────────────────
            Add("Skulltula Tokens", TrackerItemsData.GetSkulltulas(cfg), 10);
            if (cfg.HasMm) Add("Stray Fairies", TrackerItemsData.GetStrayFairies(cfg), 6);
            Add("Misc",             TrackerItemsData.GetMiscItems(cfg), 4);

            // ── 7. Songs ──────────────────────────────────────────────────────
            Add("Songs (Shared)", TrackerItemsData.GetSharedSongItems(cfg), 6);
            if (cfg.HasOot) Add("Songs (OoT)", TrackerItemsData.GetOotSongItems(cfg), 6);
            if (cfg.HasMm)  Add("Songs (MM)",  TrackerItemsData.GetMmSongItems(cfg), 5);

            // ── 8. Owls ───────────────────────────────────────────────────────
            if (cfg.HasMm) Add("Owl Statues", TrackerItemsData.GetOwlStatues(cfg), 5);

            // ── 9. Days/Nights ───────────────────────────────────────────────────
            if (cfg.HasMm) Add("Clocks", TrackerItemsData.GetClocks(cfg, _spoilerLog), 6);

            // ── 10. Tingle Maps ───────────────────────────────────────────────
            if (cfg.HasMm) Add("Tingle Maps", TrackerItemsData.GetTingleMaps(cfg), 6);

            // ── If rewards are shuffled anywhere — show separate rewards block first
            Add("Dungeon Rewards", TrackerItemsData.GetDungeonRewards(cfg), 9);
            foreach (var (id, name, label) in TrackerItemsData.DungeonList)
            {
                bool isMm = id is "woodfall" or "snowhead" or "great_bay" or "stone_tower";
                if (isMm && !cfg.HasMm) continue;
                if (!isMm && !cfg.HasOot) continue;
                var blockName = (!isMm && cfg.IsMq(id)) ? $"{name} (MQ)" : name;
                Add(blockName, TrackerItemsData.GetDungeonSingle(id, name, label, cfg), int.MaxValue);
            }
            foreach (var (id, name, label) in TrackerItemsData.SubDungeonList)
            {
                if (!cfg.HasOot) continue;
                var blockName = cfg.IsMq(id) ? $"{name} (MQ)" : name;
                Add(blockName, TrackerItemsData.GetDungeonSingle(id, name, label, cfg), int.MaxValue);
            }
            if (cfg.HasOot && cfg.SmallKeysHideout)
                Add("Thieves' Hideout",    TrackerItemsData.GetDungeonSingle("thieves_hideout", "Thieves' Hideout",    "dungeons/labels/thieves_hideout.png",    cfg), int.MaxValue);
            if (cfg.HasOot && cfg.SmallKeysTcg)
                Add("Treasure Chest Game", TrackerItemsData.GetDungeonSingle("tcg",              "Treasure Chest Game", "dungeons/labels/treasure_chest_game.png", cfg), int.MaxValue);

            // ── 12. Skeleton Key + Magical Rupee ──────────────────────────────
            Add("Dungeon Items", TrackerItemsData.GetDungeonSpecialItems(cfg), 4);

            // ── 13. Souls ─────────────────────────────────────────────────────
            Add("Boss Souls",          SoulsData.GetBossSouls(cfg),          8);
            Add("Enemy Souls",         SoulsData.GetEnemySoulsShared(cfg),   8);
            Add("Enemy Souls (OoT)",   SoulsData.GetEnemySoulsOot(cfg),      8);
            Add("Enemy Souls (MM)",    SoulsData.GetEnemySoulsMm(cfg),       8);
            Add("NPC Souls",           SoulsData.GetNpcSoulsShared(cfg),     8);
            Add("NPC Souls (OoT)",     SoulsData.GetNpcSoulsOot(cfg),        8);
            Add("NPC Souls (MM)",      SoulsData.GetNpcSoulsMm(cfg),         8);
            Add("Animal Souls",        SoulsData.GetAnimalSoulsShared(cfg),  4);
            Add("Animal Souls (OoT)",  SoulsData.GetAnimalSoulsOot(cfg),     4);
            Add("Animal Souls (MM)",   SoulsData.GetAnimalSoulsMm(cfg),      4);
            Add("Misc Souls",          SoulsData.GetMiscSoulsShared(cfg),    4);
            Add("Misc Souls (OoT)",    SoulsData.GetMiscSoulsOot(cfg),       4);
            Add("Misc Souls (MM)",     SoulsData.GetMiscSoulsMm(cfg),        4);
            
            foreach (var panel in _trackerPanels)
                panel.LoadProgress(progress);
            
            _trackerScrollPanel.Controls.Add(mainFlow);
            UpdateBroadcast();
        }
        
        private static void MigrateBottleIds(Dictionary<string, int> progress)
        {
            // Old IDs → new IDs mapping
            var migrations = new Dictionary<string, string>
            {
                ["oot_bottle_letter"] = "oot_bottle_1",
                ["oot_bottle_2"]      = "oot_bottle_2",
                ["oot_bottle_3"]      = "oot_bottle_3",
                ["oot_bottle_4"]      = "oot_bottle_4",
                ["mm_bottle_gold_dust"] = "mm_bottle_1",
                ["mm_bottle_2"]       = "mm_bottle_2",
                ["mm_bottle_3"]       = "mm_bottle_3",
                ["mm_bottle_4"]       = "mm_bottle_4",
                ["mm_bottle_5"]       = "mm_bottle_5",
                ["mm_bottle_6"]       = "mm_bottle_6",
                ["sh_bottle_letter"]    = "sh_bottle_1",
                ["sh_bottle_gold_dust"] = "sh_bottle_2",
                ["sh_bottle_3"]       = "sh_bottle_3",
                ["sh_bottle_4"]       = "sh_bottle_4",
                ["sh_bottle_5"]       = "sh_bottle_5",
                ["sh_bottle_6"]       = "sh_bottle_6",
            };

            foreach (var kv in migrations)
            {
                if (progress.TryGetValue(kv.Key, out var val) && !progress.ContainsKey(kv.Value))
                {
                    progress[kv.Value] = val;
                    progress.Remove(kv.Key);
                }
            }
        }

        private TrackerConfig? _lastBroadcastConfig;
        private int _lastBroadcastItemSize = -1;

        private void UpdateBroadcast()
        {
            if (_broadcastForm == null || _broadcastForm.IsDisposed) return;
            var progress = new Dictionary<string, int>();
            foreach (var panel in _trackerPanels)
                panel.SaveProgress(progress);

            int currentItemSize = ItemTrackerPanel.GetItemSize();

            // Full rebuild when config or icon size changes
            if (_lastBroadcastConfig != _currentTrackerConfig || _lastBroadcastItemSize != currentItemSize)
            {
                _lastBroadcastConfig = _currentTrackerConfig;
                _lastBroadcastItemSize = currentItemSize;
                _broadcastForm.Rebuild(_currentTrackerConfig, progress, _spoilerLog);
            }
            else
            {
                _broadcastForm.UpdateProgress(progress);
            }
        }

        private void ApplyBottlesFromLog()
        {
            if (_spoilerLog == null) return;

            var progress = new Dictionary<string, int>();
            foreach (var panel in _trackerPanels)
                panel.SaveProgress(progress);

            // Collect bottles from locations, grouped by game
            var ootBottles = new List<string>();
            var mmBottles  = new List<string>();
            var shBottles  = new List<string>();

            foreach (var loc in _spoilerLog.Locations.Values)
            {
                var content = StartingItemsMapper.GetBottleContentPublic(loc.Item);
                if (content == null) continue;

                var itemLower = loc.Item.ToLower();
                // Determine game by item suffix
                if (itemLower.Contains("(mm)"))
                    mmBottles.Add(content);
                else if (itemLower.Contains("(oot)"))
                    ootBottles.Add(content);
                // OoT-exclusive items (no suffix)
                else if (content is "Ruto's Letter" or "Big Poe" or "Blue Fire")
                    ootBottles.Add(content);
                // MM-exclusive items (no suffix)
                else if (content is "Gold Dust" or "Chateau Romani" or
                         "Spring Water" or "Hot Spring Water" or
                         "Zora Egg" or "Seahorse" or "Deku Princess" or "Magic Mushroom")
                    mmBottles.Add(content);
                else
                {
                    // Shared items (Milk, Potions, Poe, Fairy, Fish, Bugs) — by location game
                    if (loc.Game == "OOT")
                        ootBottles.Add(content);
                    else
                        mmBottles.Add(content);
                }
            }

            // Assign to tracker slots — sort by priority
            bool shared = _currentTrackerConfig.SharedBottles;
            if (shared)
            {
                var all = ootBottles.Concat(mmBottles)
                    .OrderBy(c => GetBottlePriority(c, StartingItemsMapper.ShBottleContentNames))
                    .ToList();
                for (int i = 0; i < Math.Min(all.Count, 6); i++)
                    SetBottleProgress(progress, $"sh_bottle_{i + 1}", all[i], StartingItemsMapper.ShBottleContentNames);
            }
            else
            {
                var sortedOot = ootBottles
                    .OrderBy(c => GetBottlePriority(c, StartingItemsMapper.OotBottleContentNames))
                    .ToList();
                var sortedMm = mmBottles
                    .OrderBy(c => GetBottlePriority(c, StartingItemsMapper.MmBottleContentNames))
                    .ToList();
                for (int i = 0; i < Math.Min(sortedOot.Count, 4); i++)
                    SetBottleProgress(progress, $"oot_bottle_{i + 1}", sortedOot[i], StartingItemsMapper.OotBottleContentNames);
                for (int i = 0; i < Math.Min(sortedMm.Count, 6); i++)
                    SetBottleProgress(progress, $"mm_bottle_{i + 1}", sortedMm[i], StartingItemsMapper.MmBottleContentNames);
            }

            foreach (var panel in _trackerPanels)
                panel.LoadProgress(progress);
        }

        private static int GetBottlePriority(string content, string[] names)
        {
            for (int i = 0; i < names.Length; i++)
                if (string.Equals(names[i], content, StringComparison.OrdinalIgnoreCase))
                    return i;
            return names.Length;
        }

        private static void SetBottleProgress(Dictionary<string, int> progress, string id, string content, string[] names)
        {
            // Case-insensitive search
            int idx = -1;
            for (int i = 0; i < names.Length; i++)
                if (string.Equals(names[i], content, StringComparison.OrdinalIgnoreCase))
                { idx = i; break; }
            if (idx < 0) idx = names.Length - 1; // empty
            progress[id] = idx * 10; // not collected yet (count = 0)
        }

        private void ApplyStartingItems()        {
            if (_spoilerLog == null || _spoilerLog.StartingItems.Count == 0) return;

            var progress = new Dictionary<string, int>();
            foreach (var panel in _trackerPanels)
                panel.SaveProgress(progress);

            // Apply starting items (only set value, don't lock)
            StartingItemsMapper.Apply(_spoilerLog, progress);

            foreach (var panel in _trackerPanels)
                panel.LoadProgress(progress);
        }

        private void BtnTrackerOptions_Click(object? sender, EventArgs e)        {
            using var dlg = new TrackerOptionsForm(_currentTrackerConfig);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _currentTrackerConfig = dlg.Config;
                SaveTrackerSettings();
                RebuildTracker();
            }
        }
        
        private void BtnResetAll_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Reset everything?\n(Log, tracker progress, settings and locations will be cleared)", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            BtnResetLog_Click(sender, e);
            if (_spoilerLog == null) // log was cleared
            {
                foreach (var p in _trackerPanels) p.ResetAll();
                _currentTrackerConfig = new TrackerConfig();
                SaveTrackerSettings();
                RebuildTracker();
                _foundLocations.Clear();
                _mapTrackerPanel.UpdateFoundLocations(_foundLocations);
                _updateMapCounters?.Invoke();
                if (_cmbFoundFilter != null) _cmbFoundFilter.SelectedIndex = 0;
                UpdateLocationsList(_txtSearch.Text);
            }
        }

        private void BtnResetLog_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Unload the spoiler log?\n(All progress and locations will be cleared)", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _spoilerLog = null;
            _currentSpoilerLogPath = null;
            _lblInfo.Text = "No file loaded";
            _foundLocations.Clear();
            _songEvents.Clear();
            _mapTrackerPanel.SetKnownLocations(new HashSet<string>());
            _mapTrackerPanel.UpdateFoundLocations(_foundLocations);
            // Reset map region selection — clears map and counters
            if (_cmbMapRegion != null) _cmbMapRegion.SelectedIndex = 0;
            if (_cmbMapSub    != null) { _cmbMapSub.Items.Clear(); _cmbMapSub.Enabled = false; }
            if (_cmbMapGame   != null) { _cmbMapGame.Items.Clear(); _cmbMapGame.Items.AddRange(new object[] { "All", "OoT", "MM" }); _cmbMapGame.SelectedIndex = 0; }
            _dgvLocations.Rows.Clear();
            _dgvSettings.Rows.Clear();
            _lstTricks.Items.Clear();
            _dgvStartingItems.Rows.Clear();
            _dgvWorldFlags.Rows.Clear();
            _dgvSpecialConditions.Rows.Clear();
            _dgvEntrances.Rows.Clear();
            _dgvSongEvents.Rows.Clear();
            _cmbGameFilter.Enabled = false;
            _cmbGameFilter.SelectedIndex = 0;
            _cmbRegionFilter.Items.Clear();
            _cmbRegionFilter.Items.Add("All Regions");
            _cmbRegionFilter.SelectedIndex = 0;
            if (_cmbFoundFilter != null) _cmbFoundFilter.SelectedIndex = 0;
            _lblCounter.Text = "";
        }

        private void BtnResetTracker_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Reset all tracker progress and settings?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            // Reset item progress
            foreach (var p in _trackerPanels) p.ResetAll();
            // Reset config to default
            _currentTrackerConfig = new TrackerConfig();
            SaveTrackerSettings();
            RebuildTracker();
            // Reset found locations
            _foundLocations.Clear();
            _mapTrackerPanel.UpdateFoundLocations(_foundLocations);
            _updateMapCounters?.Invoke();
            if (_cmbFoundFilter != null) _cmbFoundFilter.SelectedIndex = 0;
            UpdateLocationsList(_txtSearch.Text);
        }

        private void BtnResetProgress_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Reset tracker progress?\n(Settings and starting items will be preserved)", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            // Rebuild tracker with clean progress (don't touch settings)
            RebuildTracker(resetProgress: true);
            // Apply starting items from current log
            if (_spoilerLog != null)
                ApplyStartingItems();
            // Reset found locations
            _foundLocations.Clear();
            _mapTrackerPanel.UpdateFoundLocations(_foundLocations);
            _updateMapCounters?.Invoke();
            if (_cmbFoundFilter != null) _cmbFoundFilter.SelectedIndex = 0;
            UpdateLocationsList(_txtSearch.Text);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "OoTMM Tracker Save (*.ootmm-save)|*.ootmm-save|All files (*.*)|*.*",
                Title = "Save Tracker Progress",
                DefaultExt = "ootmm-save"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                var progress = new Dictionary<string, int>();
                foreach (var panel in _trackerPanels)
                    panel.SaveProgress(progress);

                var save = new TrackerSaveData
                {
                    Progress = progress,
                    SpoilerLogPath = _currentSpoilerLogPath ?? "",
                    BombchuBehaviorOot = _currentTrackerConfig.BombchuBehaviorOot,
                    BombchuBehaviorMm  = _currentTrackerConfig.BombchuBehaviorMm,
                    FoundLocations = new List<string>(_foundLocations),
                    SongEvents = new Dictionary<string, string>(_songEvents),
                };

                File.WriteAllText(dlg.FileName, JsonSerializer.Serialize(save, new JsonSerializerOptions { WriteIndented = true }));
                MessageBox.Show("Progress saved!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLoad_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "OoTMM Tracker Save (*.ootmm-save)|*.ootmm-save|All files (*.*)|*.*",
                Title = "Load Tracker Progress"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                var json = File.ReadAllText(dlg.FileName);
                var save = JsonSerializer.Deserialize<TrackerSaveData>(json);
                if (save == null) return;

                // Restore Bombchu
                _currentTrackerConfig.BombchuBehaviorOot = save.BombchuBehaviorOot;
                _currentTrackerConfig.BombchuBehaviorMm  = save.BombchuBehaviorMm;
                SaveTrackerSettings();

                // If save has a log path and it differs — offer to load
                if (!string.IsNullOrEmpty(save.SpoilerLogPath) && File.Exists(save.SpoilerLogPath)
                    && save.SpoilerLogPath != _currentSpoilerLogPath)
                {
                    var res = MessageBox.Show(
                        $"Save file references a Spoiler Log:\n{save.SpoilerLogPath}\nLoad it?",
                        "Load Spoiler Log?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (res == DialogResult.Yes)
                        LoadSpoilerLog(save.SpoilerLogPath);
                }

                // Restore progress
                MigrateBottleIds(save.Progress);
                foreach (var panel in _trackerPanels)
                    panel.LoadProgress(save.Progress);

                // Restore found locations
                _foundLocations.Clear();
                if (save.FoundLocations != null)
                    foreach (var loc in save.FoundLocations)
                        _foundLocations.Add(loc);
                // Restore song events
                _songEvents.Clear();
                if (save.SongEvents != null)
                    foreach (var kv in save.SongEvents)
                        _songEvents[kv.Key] = kv.Value;
                PopulateSongEvents();
                if (_cmbFoundFilter != null) _cmbFoundFilter.SelectedIndex = 0;
                UpdateLocationsList(_txtSearch.Text);

                // Update map marks and counters
                _mapTrackerPanel.UpdateFoundLocations(_foundLocations);
                _updateMapCounters?.Invoke();

                // Re-apply starting items (they should be locked)
                if (_spoilerLog != null)
                    ApplyStartingItems();

                MessageBox.Show("Progress loaded!", "Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Load error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnLoadFile_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Select Spoiler Log"
            };
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadSpoilerLog(openFileDialog.FileName);
            }
        }
        
        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.Oemplus:
                    case Keys.Add:
                    {
                        var steps = ItemTrackerPanel.GetItemSizeSteps();
                        int cur = Array.IndexOf(steps, ItemTrackerPanel.GetItemSize());
                        if (cur < steps.Length - 1) { ItemTrackerPanel.SetItemSize(steps[cur + 1]); RebuildTracker(); }
                        e.Handled = true; break;
                    }
                    case Keys.OemMinus:
                    case Keys.Subtract:
                    {
                        var steps = ItemTrackerPanel.GetItemSizeSteps();
                        int cur = Array.IndexOf(steps, ItemTrackerPanel.GetItemSize());
                        if (cur > 0) { ItemTrackerPanel.SetItemSize(steps[cur - 1]); RebuildTracker(); }
                        e.Handled = true; break;
                    }
                    case Keys.D0:
                    case Keys.NumPad0:
                        ItemTrackerPanel.SetItemSize(48); RebuildTracker();
                        e.Handled = true; break;
                }
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.F1: BtnLoadFile_Click(this, EventArgs.Empty); break;
                case Keys.F2: BtnSave_Click(this, EventArgs.Empty); break;
                case Keys.F3: BtnTrackerOptions_Click(this, EventArgs.Empty); break;
                case Keys.F4: BtnLoad_Click(this, EventArgs.Empty); break;
                case Keys.F5: BtnResetAll_Click(this, EventArgs.Empty); break;
                case Keys.F6: BtnResetLog_Click(this, EventArgs.Empty); break;
                case Keys.F7: BtnResetTracker_Click(this, EventArgs.Empty); break;
                case Keys.F8: BtnResetProgress_Click(this, EventArgs.Empty); break;
                case Keys.F9:
                    if (_broadcastForm == null || _broadcastForm.IsDisposed)
                    {
                        _broadcastForm = new BroadcastForm();
                        _lastBroadcastConfig = null;
                        _lastBroadcastItemSize = -1;
                        _broadcastForm.Show(this);
                        UpdateBroadcast();
                    }
                    else _broadcastForm.Focus();
                    break;
            }
        }

        private void MainForm_DragEnter(object? sender, DragEventArgs e)        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        
        private void MainForm_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                LoadSpoilerLog(files[0]);
            }
        }
        
        private void LoadSpoilerLog(string filePath)
        {
            try
            {
                _currentSpoilerLogPath = filePath;
                SpoilerLogParser.Validate(filePath);
                _spoilerLog = _parser.Parse(filePath);
                var seedPreview = _spoilerLog.Seed.Length > 16 ? _spoilerLog.Seed.Substring(0, 16) + "..." : _spoilerLog.Seed;
                var fileName = Path.GetFileName(filePath);
                _lblInfo.Text = $"{fileName} | {_spoilerLog.Version} | Seed: {seedPreview} | Locations: {_spoilerLog.Locations.Count} | Entrances: {_spoilerLog.Entrances.Count}";
                
                // Reset filters to "All"
                _cmbGameFilter.SelectedIndex = 0;
                
                // Populate region list
                PopulateRegionFilter();
                
                // Enable game filter only if both games are present
                bool hasOot = _spoilerLog.Locations.Values.Any(l => l.Game == "OOT");
                bool hasMm  = _spoilerLog.Locations.Values.Any(l => l.Game == "MM");
                _cmbGameFilter.Enabled = hasOot && hasMm;
                if (!_cmbGameFilter.Enabled) _cmbGameFilter.SelectedIndex = 0; // Reset to "All" if disabled
                
                UpdateLocationsList();
                UpdateSettingsList();
                UpdateTricksList();
                UpdateStartingItemsList();
                UpdateWorldFlagsList();
                UpdateSpecialConditionsList();
                UpdateEntrancesList();

                // Update tracker to match spoiler log settings
                _currentTrackerConfig = TrackerConfig.FromSpoilerLog(_spoilerLog);
                // Save Bombchu from log to file (to remember it)
                SaveTrackerSettings();

                // Reset and populate song events (after config update)
                _songEvents.Clear();
                PopulateSongEvents();

                // Reset progress — new log, new game
                _foundLocations.Clear();
                RebuildTracker(resetProgress: true);
                // Apply bottles from log locations
                ApplyBottlesFromLog();
                // Apply starting items
                ApplyStartingItems();

                // Update map with known locations from log
                var knownLocs = new HashSet<string>(
                    _spoilerLog.Locations.Values.Select(l => $"{l.Game}|{l.Location}"));
                _knownLocations = knownLocs;
                _mapTrackerPanel.SetKnownLocations(knownLocs);
                _mapTrackerPanel.SetSpoilerLog(_spoilerLog);
                _mapTrackerPanel.UpdateFoundLocations(_foundLocations);
                UpdateMapColoredLocations();
                _updateMapCounters?.Invoke();

                // Restrict map game filter based on loaded game mode
                _cmbMapGame.Items.Clear();
                if (_currentTrackerConfig.HasOot && _currentTrackerConfig.HasMm)
                    _cmbMapGame.Items.AddRange(new object[] { "All", "OoT", "MM" });
                else if (_currentTrackerConfig.HasOot)
                    _cmbMapGame.Items.AddRange(new object[] { "OoT" });
                else
                    _cmbMapGame.Items.AddRange(new object[] { "MM" });
                _cmbMapGame.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void UpdateLocationsList(string? searchText = null)
        {
            _dgvLocations.SuspendLayout();
            _dgvLocations.Rows.Clear();
            
            if (_spoilerLog == null)
            {
                _dgvLocations.ResumeLayout();
                return;
            }

            // Pre-compute filter values once
            string? search = string.IsNullOrWhiteSpace(searchText) ? null : searchText.ToLower();
            string? gameFilter = _cmbGameFilter.SelectedIndex > 0
                ? (_cmbGameFilter.SelectedItem?.ToString() == "OoT" ? "OOT" : _cmbGameFilter.SelectedItem?.ToString())
                : null;
            string? regionFilter = _cmbRegionFilter.SelectedIndex > 0
                ? _cmbRegionFilter.SelectedItem?.ToString()
                : null;
            int statusFilter = _cmbFoundFilter?.SelectedIndex ?? 0;

            // Single-pass filter + sort
            var filtered = _spoilerLog.Locations.Values
                .Where(l =>
                    (search == null || l.Location.ToLower().Contains(search) || l.Item.ToLower().Contains(search)) &&
                    (gameFilter == null || l.Game == gameFilter) &&
                    (regionFilter == null || l.Region == regionFilter))
                .OrderBy(l => l.Region)
                .ThenBy(l => l.Location);

            // Batch add rows
            _dgvLocations.SuspendLayout();
            foreach (var location in filtered)
            {
                var key = $"{location.Game}|{location.Location}";
                bool found = _foundLocations.Contains(key);
                if (statusFilter == 1 && found)  continue;
                if (statusFilter == 2 && !found) continue;
                _dgvLocations.Rows.Add(found, location.Game, location.Region, location.Location, location.Item);
            }
            _dgvLocations.ResumeLayout();

            // Update counter
            int total    = _dgvLocations.Rows.Count;
            int checked_ = _dgvLocations.Rows.Cast<DataGridViewRow>().Count(r => r.Cells[0].Value is true);
            if (_lblCounter != null)
                _lblCounter.Text = $"{checked_}/{total} checked";

            _dgvLocations.ResumeLayout();
        }

        private static string LocationKey(DataGridViewRow row)
            => $"{row.Cells[1].Value}|{row.Cells[3].Value}";

        private void DgvLocations_DirtyStateChanged(object? sender, EventArgs e)
        {
            // Commit checkbox change immediately on click
            if (_dgvLocations.IsCurrentCellDirty)
                _dgvLocations.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void DgvLocations_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 0) return;
            var row = _dgvLocations.Rows[e.RowIndex];
            var key = LocationKey(row);
            bool isChecked = row.Cells[0].Value is true;
            if (isChecked)
                _foundLocations.Add(key);
            else
                _foundLocations.Remove(key);
            // Recolor the row
            ApplyRowColor(row, isChecked);
            // Update map marks
            _mapTrackerPanel?.UpdateFoundLocations(_foundLocations);
            // Update counter
            int total    = _dgvLocations.Rows.Count;
            int checked_ = _dgvLocations.Rows.Cast<DataGridViewRow>().Count(r => r.Cells[0].Value is true);
            if (_lblCounter != null)
                _lblCounter.Text = $"{checked_}/{total} checked";
            // If filter is active — update list
            if (_cmbFoundFilter?.SelectedIndex > 0)
                UpdateLocationsList(_txtSearch.Text);
        }

        private void DgvLocations_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex == 0) return; // don't color checkbox
            var row = _dgvLocations.Rows[e.RowIndex];
            var key = LocationKey(row);
            bool found = _foundLocations.Contains(key);
            var item = row.Cells[4].Value?.ToString() ?? "";

            if (found)
            {
                e.CellStyle.BackColor = Color.FromArgb(40, 120, 40);
                e.CellStyle.ForeColor = Color.White;
            }
            else if (IsTrap(item))
            {
                if (_chkColorHighlight.Checked)
                {
                    e.CellStyle.BackColor = Color.FromArgb(100, 30, 30);
                    e.CellStyle.ForeColor = Color.FromArgb(255, 120, 120);
                }
            }
            else if (IsConsumable(item))
            {
                if (_chkColorHighlight.Checked)
                {
                    e.CellStyle.BackColor = Color.FromArgb(80, 70, 20);
                    e.CellStyle.ForeColor = Color.FromArgb(220, 200, 80);
                }
            }
            else
            {
                e.CellStyle.BackColor = _dgvLocations.DefaultCellStyle.BackColor;
                e.CellStyle.ForeColor = _dgvLocations.DefaultCellStyle.ForeColor;
            }
            // Keep selection color same as row color (no blue highlight)
            e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
        }

        private static bool IsTrap(string item) =>
            item.Contains("Trap", StringComparison.OrdinalIgnoreCase);

        private static bool IsConsumable(string item)
        {
            if (string.IsNullOrEmpty(item)) return false;
            var lower = item.ToLower();

            // Starts with a number (e.g. "10 Arrows", "5 Bombs", "10 Deku Nuts") — but NOT upgrades
            if (System.Text.RegularExpressions.Regex.IsMatch(item, @"^\d+\s") &&
                !lower.Contains("upgrade") && !lower.Contains("bag"))
                return true;

            // Rupees — but NOT Silver Rupees with dungeon names (they are dungeon items)
            // "Silver Rupee (MM)" and "Silver Rupee (OoT)" are regular rupees
            if (System.Text.RegularExpressions.Regex.IsMatch(lower, @"^(green|blue|red|purple|huge|gold)?\s*rupee"))
                return true;
            if (lower.StartsWith("silver rupee") &&
                (lower.Contains("(mm)") || lower.Contains("(oot)") || !lower.Contains("(")))
                return true;

            // Hearts / Magic
            if (lower.StartsWith("recovery heart") ||
                lower.StartsWith("small magic jar") ||
                lower.StartsWith("large magic jar"))
                return true;

            // Fairy (but not "Fairy Bow", "Fairy Slingshot", "Fairy Ocarina", "Fairy Sword", "Bottled Fairy")
            if (lower.StartsWith("fairy") &&
                !lower.Contains("bow") && !lower.Contains("slingshot") &&
                !lower.Contains("ocarina") && !lower.Contains("sword") && !lower.Contains("mask"))
                return true;
            if (lower.StartsWith("big fairy"))
                return true;

            // Bombs/Bombchu — only consumable packs, not Bomb Bag or Bombchu Bag
            if ((lower.StartsWith("bomb") || lower.StartsWith("bombchu")) &&
                !lower.Contains("bag") && !lower.Contains("upgrade"))
                return true;

            // Deku Sticks — only consumable, not upgrade
            if (lower.StartsWith("deku stick") && !lower.Contains("upgrade"))
                return true;

            // Bottle contents — only loose items, NOT "Bottle of..." or "Bottled..." (those are the bottles themselves)
            if (lower.StartsWith("blue fire") || lower.StartsWith("blue potion") ||
                lower.StartsWith("red potion") || lower.StartsWith("green potion") ||
                lower.StartsWith("chateau romani refill") ||
                lower.StartsWith("bugs") || lower.StartsWith("poe") ||
                lower.StartsWith("child fish") || lower.StartsWith("adult fish") ||
                lower.StartsWith("spring water") || lower.StartsWith("hot spring water") ||
                lower.StartsWith("milk refill") || lower.StartsWith("lon lon milk"))
                return true;

            // Milk (not bottled)
            if (lower.StartsWith("lon lon milk") || lower == "romani milk")
                return true;

            // Nothing
            if (lower == "nothing") return true;

            return false;
        }

        private void UpdateMapColoredLocations()
        {
            if (_spoilerLog == null) return;
            var colored = new HashSet<string>();
            foreach (var loc in _spoilerLog.Locations.Values)
                if (IsTrap(loc.Item) || IsConsumable(loc.Item))
                    colored.Add($"{loc.Game}|{loc.Location}");
            _coloredLocations = colored;
            _mapTrackerPanel.SetColoredLocations(colored);
        }

        private static void ApplyRowColor(DataGridViewRow row, bool found)
        {
            if (found)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(40, 120, 40);
                row.DefaultCellStyle.ForeColor = Color.White;
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.Empty;
                row.DefaultCellStyle.ForeColor = Color.Empty;
            }
        }
        
        private void PopulateRegionFilter()
        {
            if (_spoilerLog == null)
                return;
            
            _cmbRegionFilter.Items.Clear();
            _cmbRegionFilter.Items.Add("All Regions");
            
            var locations = _spoilerLog.Locations.Values.AsEnumerable();
            
            // Filter regions by selected game
            if (_cmbGameFilter.SelectedIndex > 0)
            {
                var selectedGame = _cmbGameFilter.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedGame))
                {
                    // Game in log is "OOT" or "MM", but combo shows "OoT" or "MM"
                    var gameToMatch = selectedGame == "OoT" ? "OOT" : selectedGame;
                    locations = locations.Where(l => l.Game == gameToMatch);
                }
            }
            
            var regions = locations
                .Select(l => l.Region)
                .Distinct()
                .OrderBy(r => r);
            
            foreach (var region in regions)
            {
                _cmbRegionFilter.Items.Add(region);
            }
            
            _cmbRegionFilter.SelectedIndex = 0;
        }
        
        private void CmbRegionFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateLocationsList(_txtSearch.Text);
        }
        
        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            UpdateLocationsList(_txtSearch.Text);
        }
        
        private void UpdateSettingsList()
        {
            _dgvSettings.Rows.Clear();
            if (_spoilerLog == null) return;
            var scKeys = new HashSet<string> { "rainbowBridge", "lacs", "ganonBossKey", "majoraChild" };
            foreach (var setting in _spoilerLog.Settings.OrderBy(s => s.Key))
            {
                var displayValue = setting.Value;
                if (scKeys.Contains(setting.Key) && (string.IsNullOrWhiteSpace(setting.Value) || setting.Value == "custom"))
                    displayValue = string.IsNullOrWhiteSpace(setting.Value) ? "(see Special Conditions)" : "custom (see Special Conditions)";
                _dgvSettings.Rows.Add(setting.Key, displayValue);
            }
        }
        
        private void UpdateTricksList()
        {
            _lstTricks.Items.Clear();
            
            if (_spoilerLog == null)
                return;
            
            if (_spoilerLog.Tricks.Count == 0)
            {
                _lstTricks.Items.Add("(No tricks enabled)");
            }
            else
            {
                foreach (var trick in _spoilerLog.Tricks)
                {
                    _lstTricks.Items.Add(trick);
                }
            }
        }
        
        private void UpdateStartingItemsList()
        {
            _dgvStartingItems.Rows.Clear();
            
            if (_spoilerLog == null)
                return;
            
            foreach (var item in _spoilerLog.StartingItems.OrderBy(i => i.Key))
            {
                _dgvStartingItems.Rows.Add(item.Key, item.Value);
            }
        }
        
        private void UpdateWorldFlagsList()
        {
            _dgvWorldFlags.Rows.Clear();
            
            if (_spoilerLog == null)
                return;
            
            foreach (var flag in _spoilerLog.WorldFlags.OrderBy(f => f.Key))
            {
                _dgvWorldFlags.Rows.Add(flag.Key, flag.Value);
            }
        }
        
        private void UpdateSpecialConditionsList()
        {
            _dgvSpecialConditions.Rows.Clear();
            
            if (_spoilerLog == null)
                return;
            
            // Display order for conditions
            var conditionOrder = new[] { "BRIDGE", "MOON", "LACS", "GANON_BK", "MAJORA" };
            
            foreach (var conditionName in conditionOrder)
            {
                if (_spoilerLog.SpecialConditions.TryGetValue(conditionName, out var condition))
                {
                    bool firstRow = true;
                    foreach (var req in condition.Requirements.OrderBy(r => r.Key))
                    {
                        _dgvSpecialConditions.Rows.Add(
                            firstRow ? conditionName : "",
                            req.Key,
                            req.Value
                        );
                        firstRow = false;
                    }
                }
            }
        }
        
        // All OoT song event locations with vanilla songs
        private static readonly Dictionary<string, string> SongEventDefaults = new()
        {
            ["Temple of Time - Door of Time"]       = "Song of Time",
            ["Hyrule Castle - Great Fairy"]         = "Zelda's Lullaby",
            ["Death Mountain Trail - Great Fairy"]  = "Zelda's Lullaby",
            ["Death Mountain Crater - Great Fairy"] = "Zelda's Lullaby",
            ["Zora's Fountain - Great Fairy"]       = "Zelda's Lullaby",
            ["Desert Colossus - Great Fairy"]       = "Zelda's Lullaby",
            ["Ganon's Castle - Great Fairy"]        = "Zelda's Lullaby",
            ["Goron City - Darunia's Room"]         = "Zelda's Lullaby",
            ["Zora's River - Waterfall"]            = "Zelda's Lullaby",
            ["Kakariko - Windmill"]                 = "Song of Storms",
            ["Royal Family's Tomb"]                 = "Zelda's Lullaby",
            ["Water Temple - Water Levels"]         = "Zelda's Lullaby",
            ["Bottom of the Well - Water Levels"]   = "Zelda's Lullaby",
            ["Shadow Temple - Boat"]                = "Zelda's Lullaby",
            ["Spirit Temple - Statue"]              = "Zelda's Lullaby",
            ["Spirit Temple - Compass Chest"]       = "Zelda's Lullaby",
            ["Spirit Temple - Boss Key Room"]       = "Zelda's Lullaby",
            ["Ganon's Castle - Light Trial"]        = "Zelda's Lullaby",
        };

        private void PopulateSongEvents()
        {
            _dgvSongEvents.Rows.Clear();

            // Song Events are OoT-only — skip if MM-only game
            if (!_currentTrackerConfig.HasOot) return;

            foreach (var kv in SongEventDefaults)
            {
                // If shuffle enabled — use saved value or "?", otherwise use vanilla
                var song = _currentTrackerConfig.SongEventsShuffleOot
                    ? (_songEvents.TryGetValue(kv.Key, out var s) ? s : "?")
                    : kv.Value;
                _dgvSongEvents.Rows.Add(kv.Key, song);
            }

            // Make read-only if shuffle is disabled
            _dgvSongEvents.Columns[1].ReadOnly = !_currentTrackerConfig.SongEventsShuffleOot;
        }

        private void DgvSongEvents_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
            var row = _dgvSongEvents.Rows[e.RowIndex];
            var loc  = row.Cells[0].Value?.ToString() ?? "";
            var song = row.Cells[1].Value?.ToString() ?? "?";
            if (!string.IsNullOrEmpty(loc))
                _songEvents[loc] = song;
        }

        private void UpdateEntrancesList()        {
            _dgvEntrances.Rows.Clear();
            
            if (_spoilerLog == null)
                return;
            
            foreach (var entrance in _spoilerLog.Entrances.OrderBy(e => e.Key))
            {
                _dgvEntrances.Rows.Add(entrance.Key, entrance.Value);
            }
        }
    }

    // Tracker progress save structure
    public class TrackerSaveData
    {
        public Dictionary<string, int> Progress { get; set; } = new();
        public string SpoilerLogPath { get; set; } = "";
        public string BombchuBehaviorOot { get; set; } = "toggle";
        public string BombchuBehaviorMm  { get; set; } = "toggle";
        public List<string> FoundLocations { get; set; } = new();
        public Dictionary<string, string> SongEvents { get; set; } = new();
    }


    /// <summary>Dark color table for MenuStrip.</summary>
    internal class DarkMenuColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected          => Color.FromArgb(60, 60, 80);
        public override Color MenuItemBorder            => Color.FromArgb(80, 80, 100);
        public override Color MenuBorder                => Color.FromArgb(60, 60, 60);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(60, 60, 80);
        public override Color MenuItemSelectedGradientEnd   => Color.FromArgb(60, 60, 80);
        public override Color MenuItemPressedGradientBegin  => Color.FromArgb(50, 50, 70);
        public override Color MenuItemPressedGradientEnd    => Color.FromArgb(50, 50, 70);
        public override Color ToolStripDropDownBackground   => Color.FromArgb(40, 40, 40);
        public override Color ImageMarginGradientBegin      => Color.FromArgb(40, 40, 40);
        public override Color ImageMarginGradientMiddle     => Color.FromArgb(40, 40, 40);
        public override Color ImageMarginGradientEnd        => Color.FromArgb(40, 40, 40);
        public override Color SeparatorDark                 => Color.FromArgb(70, 70, 70);
        public override Color SeparatorLight                => Color.FromArgb(70, 70, 70);
    }
}
