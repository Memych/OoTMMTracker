using OoTMMTracker.Controls;
using OoTMMTracker.Models;
using OoTMMTracker.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace OoTMMTracker.Forms
{
    public class MainForm : Form
    {
        private sealed class MapJumpTarget
        {
            public string Label { get; }
            public string FromId { get; }
            public MapJumpTarget(string label, string fromId)
            {
                Label = label;
                FromId = fromId;
            }
            public override string ToString() => Label;
        }

        /// <summary>Vanilla <b>source</b> spot (where the Wallmaster grabs you), shown after &quot;Wallmaster - &quot;.</summary>
        private static readonly Dictionary<string, string> WallmasterSourceLabels =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["MM_WALLMASTER_BTW_ENTRANCE"] = "Beneath the Well Entrance",
                ["MM_WALLMASTER_BTW_EXIT"] = "Beneath the Well Exit",
                ["MM_WALLMASTER_BTW_FOUNTAIN"] = "Beneath the Well Fountain",
                ["MM_WALLMASTER_DAMPE"] = "Ikana Graveyard Dampe",
                ["OOT_WALLMASTER_BOTW_BASEMENT"] = "Bottom of the Well Basement",
                ["OOT_WALLMASTER_BOTW_MAIN"] = "Bottom of the Well Main",
                ["OOT_WALLMASTER_BOTW_PIT"] = "Bottom of the Well Pit",
                ["OOT_WALLMASTER_FOREST_CORRIDOR_EAST"] = "Forest Temple East Corridor",
                ["OOT_WALLMASTER_FOREST_CORRIDOR_WEST"] = "Forest Temple West Corridor",
                ["OOT_WALLMASTER_GANON_LIGHT"] = "Ganon's Castle Light Trial",
                ["OOT_WALLMASTER_GANON_SPIRIT"] = "Ganon's Castle Spirit Trial",
                ["OOT_WALLMASTER_GTG"] = "Gerudo Training Ground",
                ["OOT_WALLMASTER_SHADOW"] = "Shadow Temple",
                ["OOT_WALLMASTER_SPIRIT_ADULT_CLIMB"] = "Spirit Temple Adult Climb",
                ["OOT_WALLMASTER_SPIRIT_CHILD_RUPEES"] = "Spirit Temple Child Rupees",
                ["OOT_WALLMASTER_SPIRIT_CHILD_SUN"] = "Spirit Temple Child Sun Block",
                ["OOT_WALLMASTER_SPIRIT_STATUE"] = "Spirit Temple Statue Hands",
            };

        private static string HumanizeWallmasterIdTail(string fromId)
        {
            var key = EntranceMapNavigation.NormalizeEntranceIdToken(fromId);
            foreach (var p in new[] { "OOT_WALLMASTER_", "MM_WALLMASTER_" })
            {
                if (key.Length > p.Length && key.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                {
                    var tail = key.Substring(p.Length);
                    if (tail.Length == 0) break;
                    return string.Join(" ",
                        tail.Split('_', StringSplitOptions.RemoveEmptyEntries).Select(static part =>
                            part.Length == 0 ? part : char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant()));
                }
            }

            return key;
        }

        private static string ResolveWallmasterSourceLabel(string fromId)
        {
            var key = EntranceMapNavigation.NormalizeEntranceIdToken(fromId);
            return WallmasterSourceLabels.TryGetValue(key, out var place)
                ? place
                : HumanizeWallmasterIdTail(fromId);
        }

        /// <summary>Wallmasters: &quot;Wallmaster - &lt;where it grabs you&gt;&quot;; others use the fixed template label.</summary>
        private static string BuildQuickJumpDisplayLabel(MapJumpTarget template)
        {
            if (!template.FromId.Contains("WALLMASTER", StringComparison.OrdinalIgnoreCase))
                return template.Label;

            return $"Wallmaster - {ResolveWallmasterSourceLabel(template.FromId)}";
        }

        private SpoilerLog? _spoilerLog;
        private readonly SpoilerLogParser _parser;
        
        private Button _btnLoadFile = null!;
        private TextBox _txtSearch = null!;
        private ComboBox _cmbTrackerWorldFilter = null!;
        private ComboBox _cmbWorldFilter = null!;
        private ComboBox _cmbGameFilter   = null!;
        private ComboBox _cmbRegionFilter = null!;
        private ComboBox _cmbFoundFilter  = null!;
        private CheckBox _chkHideItems    = null!;
        private ComboBox? _cmbMapWorld;
        private ComboBox _cmbMapRegion    = null!;
        private ComboBox _cmbMapSub       = null!;
        private ComboBox _cmbMapGame      = null!;
        private CheckBox _chkOoTAdult     = null!;
        private CheckBox _chkMMCleared    = null!;
        private bool _ootAdultState = false;    // Saved state for OoT adult checkbox
        private bool _mmClearedState = false;   // Saved state for MM cleared checkbox
        private Action?  _updateMapCounters;
        private Action?  _updateQuickJump;
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
        private Panel _trackerWorldFilterPanel = null!;
        private Controls.MapTrackerPanel _mapTrackerPanel = null!;
        private MapLogicEvaluator _mapLogicEvaluator;
        private TrackerConfig _currentTrackerConfig = new();
        private BroadcastForm? _broadcastForm;
		private Label _lblMapZoomLevel = null!;
        private Label _lblMapRegionCounter = null!;
        private Label _lblMapSubCounter = null!;

        // Found locations: key = "Game|Location"
        private readonly HashSet<string> _foundLocations = new HashSet<string>();
        private readonly Dictionary<string, int> _sessionProgress = new();
        private Dictionary<string, string> _locationCache = new();
        private Dictionary<string, HashSet<string>> _locationsByWorld = new();
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
            UpdateTrackerWorldFilterVisibility();
            // Test default transitions on startup
            DefaultTransitionService.TestAllDefaultTransitions();
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

            _trackerWorldFilterPanel = new Panel
            {
                Height = 40,
                Width = 523,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            var lblTWorld = new Label { Text = "Player/World:", Location = new Point(10, 10), Size = new Size(60, 20), ForeColor = Color.White, AutoSize = true };
            _cmbTrackerWorldFilter = new ComboBox { Location = new Point(90, 7), Size = new Size(120, 22), DropDownStyle = ComboBoxStyle.DropDownList };

            _cmbTrackerWorldFilter.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbMapWorld != null && _cmbTrackerWorldFilter.SelectedItem != null)
                {
                    string selectedWorld = _cmbTrackerWorldFilter.SelectedItem.ToString()!;
                    if (_cmbMapWorld.Items.Contains(selectedWorld))
                    {
                        if (!string.Equals(_cmbMapWorld.SelectedItem?.ToString(), selectedWorld, StringComparison.OrdinalIgnoreCase))
                            _cmbMapWorld.SelectedItem = selectedWorld;
                    }
                }
                UpdateTrackerConfigForCurrentWorld();

                RebuildTracker();
                UpdateStartingItemsList();
                _updateMapCounters?.Invoke();
                RefreshComboBoxDisplay();
            };
            _trackerWorldFilterPanel.Controls.Add(lblTWorld);
            _trackerWorldFilterPanel.Controls.Add(_cmbTrackerWorldFilter);
            _leftPanel.Controls.Add(_trackerWorldFilterPanel);

            // Tracker inside left panel
            _trackerScrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(30, 30, 30) };
            _leftPanel.Controls.Add(_trackerScrollPanel);
            RebuildTracker();
            UpdateTrackerWorldFilterVisibility();
            // Right panel: search + tabs
            var rightPanel = rightContainer;
            // Search panel: outer container with fixed height, inner panel scrolls horizontally
            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(45, 45, 45) };
            var searchInner = new Panel
            {
                Location  = new Point(0, 0),
                Height    = 36,
                Width     = 1320,
                BackColor = Color.FromArgb(45, 45, 45)
            };
            var searchHScroll = new HScrollBar
            {
                Dock    = DockStyle.Bottom,
                Height  = 16,
                Minimum = 0,
                Maximum = 1320,
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

            searchInner.Controls.Add(new Label { Text = "World:", Location = new Point(286, 10), Size = new Size(45, 20), ForeColor = Color.White });
            _cmbWorldFilter = new ComboBox { Location = new Point(334, 7), Size = new Size(100, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbWorldFilter.Items.Add("All Worlds");
            _cmbWorldFilter.SelectedIndex = 0;
            _cmbWorldFilter.Visible = false;
            searchInner.Controls.Add(_cmbWorldFilter);
            _cmbWorldFilter.SelectedIndexChanged += (s, e) =>
            {
                UpdateLocationsList(_txtSearch.Text);
                UpdateWorldFlagsList();
                UpdateEntrancesList();
                PopulateSongEvents();
                UpdateStartingItemsList();
            };

            searchInner.Controls.Add(new Label { Text = "Game:", Location = new Point(442, 10), Size = new Size(45, 20), ForeColor = Color.White });
            _cmbGameFilter = new ComboBox { Location = new Point(490, 7), Size = new Size(100, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbGameFilter.Items.AddRange(new object[] { "All", "OoT", "MM" });
            _cmbGameFilter.SelectedIndex = 0;
            _cmbGameFilter.SelectedIndexChanged += (s, e) =>
            {
                UpdateWorldFlagsList();
                PopulateRegionFilter();
                UpdateLocationsList(_txtSearch.Text);
            };
            _cmbGameFilter.Enabled = false;
            searchInner.Controls.Add(_cmbGameFilter);

            searchInner.Controls.Add(new Label { Text = "Region:", Location = new Point(598, 10), Size = new Size(55, 20), ForeColor = Color.White });
            _cmbRegionFilter = new ComboBox { Location = new Point(656, 7), Size = new Size(220, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbRegionFilter.Items.Add("All Regions");
            _cmbRegionFilter.SelectedIndex = 0;
            _cmbRegionFilter.SelectedIndexChanged += CmbRegionFilter_SelectedIndexChanged;
            searchInner.Controls.Add(_cmbRegionFilter);

            searchInner.Controls.Add(new Label { Text = "Status:", Location = new Point(884, 10), Size = new Size(50, 20), ForeColor = Color.White });
            _cmbFoundFilter = new ComboBox { Location = new Point(936, 7), Size = new Size(120, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbFoundFilter.Items.AddRange(new object[] { "All", "Not Found", "Found" });
            _cmbFoundFilter.SelectedIndex = 0;
            _cmbFoundFilter.SelectedIndexChanged += (s, e) => UpdateLocationsList(_txtSearch.Text);
            searchInner.Controls.Add(_cmbFoundFilter);

            _chkHideItems = new CheckBox { Text = "Hide Items", Location = new Point(1066, 9), Size = new Size(90, 18), ForeColor = Color.White };
            _chkHideItems.CheckedChanged += (s, e) =>
            {
                _dgvLocations.Columns["Item"]!.Visible = !_chkHideItems.Checked;
                UpdateLocationsList(_txtSearch.Text);
            };
            searchInner.Controls.Add(_chkHideItems);

            _chkColorHighlight = new CheckBox { Text = "Colors", Location = new Point(1164, 9), Size = new Size(70, 18), ForeColor = Color.White, Checked = false };
            _chkColorHighlight.CheckedChanged += (s, e) =>
            {
                UpdateLocationsList(_txtSearch.Text);
                _mapTrackerPanel.SetColorsMode(_chkColorHighlight.Checked);
                UpdateMapColoredLocations();
                _updateMapCounters?.Invoke();
                RefreshComboBoxDisplay();
            };
            searchInner.Controls.Add(_chkColorHighlight);

            _lblCounter = new Label { Location = new Point(1242, 10), Size = new Size(120, 18), ForeColor = Color.FromArgb(180, 180, 180), TextAlign = ContentAlignment.MiddleLeft };
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
            _dgvLocations.Columns.Add("World", "World");
            _dgvLocations.Columns.Add("Game", "Game");
            _dgvLocations.Columns.Add("Region", "Region");
            _dgvLocations.Columns.Add("Location", "Location");
            _dgvLocations.Columns.Add("Item", "Item");
            // Text columns — read only
            _dgvLocations.Columns[1].ReadOnly = true;
            _dgvLocations.Columns[2].ReadOnly = true;
            _dgvLocations.Columns[3].ReadOnly = true;
            _dgvLocations.Columns[4].ReadOnly = true;
            _dgvLocations.Columns[5].ReadOnly = true;
            _dgvLocations.Columns[1].FillWeight = 10;
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
            _dgvStartingItems.Columns.Add("Player", "Player");
            _dgvStartingItems.Columns[0].FillWeight = 25;
            _dgvStartingItems.Columns.Add("Item", "Item");
            _dgvStartingItems.Columns[1].FillWeight = 55;
            _dgvStartingItems.Columns.Add("Qty", "Quantity");
            _dgvStartingItems.Columns[2].FillWeight = 20;
            tabStart.Controls.Add(_dgvStartingItems);
            innerTabs.TabPages.Add(tabStart);

            var tabFlags = new TabPage("World Flags");
            _dgvWorldFlags = MakeGrid();
            _dgvWorldFlags.Columns.Add("World", "World"); _dgvWorldFlags.Columns[0].FillWeight = 15;
            _dgvWorldFlags.Columns.Add("Flag", "Flag"); _dgvWorldFlags.Columns[1].FillWeight = 50;
            _dgvWorldFlags.Columns.Add("Value", "Value"); _dgvWorldFlags.Columns[2].FillWeight = 35;
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
            _dgvEntrances.Columns.Add("World", "World"); _dgvEntrances.Columns[0].FillWeight = 15;
            _dgvEntrances.Columns.Add("From", "From"); _dgvEntrances.Columns[1].FillWeight = 45;
            _dgvEntrances.Columns.Add("To", "To"); _dgvEntrances.Columns[2].FillWeight = 40;
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
            _dgvSongEvents.Columns.Add("World", "World");
            _dgvSongEvents.Columns[0].FillWeight = 15;
            _dgvSongEvents.Columns[0].ReadOnly = true;
            _dgvSongEvents.Columns["World"].SortMode = DataGridViewColumnSortMode.NotSortable;
            _dgvSongEvents.Columns.Add("Location", "Location");
            _dgvSongEvents.Columns[1].FillWeight = 50;
            _dgvSongEvents.Columns[1].ReadOnly = true;
            _dgvSongEvents.Columns["Location"].SortMode = DataGridViewColumnSortMode.NotSortable;
            // Song column — ComboBox
            var songCol = new DataGridViewComboBoxColumn
            {
                Name = "Song", HeaderText = "Song",
                FillWeight = 40,
                FlatStyle = FlatStyle.Flat,
            };
            songCol.Items.AddRange("?", "Zelda's Lullaby", "Epona's Song", "Saria's Song",
                                   "Sun's Song", "Song of Time", "Song of Storms",
								   "Minuet of Forest", "Bolero of Fire", "Serenade of Water",
								   "Requiem of Spirit", "Nocturne of Shadow", "Prelude of Light",
								   "Song of Healing", "Song of Soaring", "Sonata of Awakening", "Goron Lullaby (Intro)",
								   "Goron Lullaby", "New Wave Bossa Nova", "Elegy of Emptiness", "Oath to Order");
            _dgvSongEvents.Columns.Add(songCol);
            _dgvSongEvents.Columns["Song"].ReadOnly = false;
            _dgvSongEvents.Columns["Song"].SortMode = DataGridViewColumnSortMode.NotSortable;
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
			_mapTrackerPanel.ZoomChanged += () => 
			{ 
				_lblMapZoomLevel.Text = $"{(int)(_mapTrackerPanel.GetZoomLevel() * 100)}%"; 
			};
            _mapTrackerPanel.MarkCompleted += (locationNames, game) =>
            {
                string currentWorld = _cmbMapWorld?.SelectedItem?.ToString() ?? "World 1";

                foreach (var loc in locationNames)
                {
                    var key = $"{currentWorld}|{game}|{loc}";
                    _foundLocations.Add(key);
                    foreach (DataGridViewRow row in _dgvLocations.Rows)
                    {
                        if (row.Tag?.ToString() == key)
                        {
                            row.Cells[0].Value = true;
                            ApplyRowColor(row, true);
                            break;
                        }
                    }
                }
                _mapTrackerPanel.UpdateFoundLocations(_foundLocations);
                UpdateMapAccessibleLocations();
                _updateMapCounters?.Invoke();
                RefreshComboBoxDisplay();

                int total = _dgvLocations.Rows.Count;
                int checked_ = _dgvLocations.Rows.Cast<DataGridViewRow>().Count(r => r.Cells[0].Value is true);
                if (_lblCounter != null)
                    _lblCounter.Text = $"{checked_}/{total} checked";
            };

            var mapFilterPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(45, 45, 45) };
            var mapFilterInner = new Panel
            {
                Location  = new Point(0, 0),
                Height    = 64,
                Width     = 1150,
                BackColor = Color.FromArgb(45, 45, 45)
            };
            var mapFilterRow1 = new Panel
            {
                Location = new Point(0, 0),
                Height = 32,
                Width = mapFilterInner.Width,
                BackColor = Color.FromArgb(45, 45, 45)
            };
            var mapFilterRow2 = new Panel
            {
                Location = new Point(0, 32),
                Height = 32,
                Width = mapFilterInner.Width,
                BackColor = Color.FromArgb(45, 45, 45)
            };
            mapFilterInner.Controls.Add(mapFilterRow1);
            mapFilterInner.Controls.Add(mapFilterRow2);
            var mapFilterHScroll = new HScrollBar
            {
                Dock        = DockStyle.Bottom,
                Height      = 16,
                Minimum     = 0,
                Maximum     = 940,
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
                mapFilterPanel.Height = mapFilterHScroll.Visible ? 80 : 64;
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
            mapFilterRow1.Controls.Add(new Label { Text = "World:", Location = new Point(6, 8), Size = new Size(50, 18), ForeColor = Color.White });
            _cmbMapWorld = new ComboBox
            {
                Location = new Point(56, 5),
                Size = new Size(100, 22),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbMapWorld.Items.Add("World 1");
            _cmbMapWorld.SelectedIndex = 0;
            _cmbMapWorld.SelectedIndexChanged += (s, e) =>
            {
                UpdateMapWorldFilter();
            };
            mapFilterRow1.Controls.Add(_cmbMapWorld);
            _cmbMapWorld.Visible = false;

            mapFilterRow1.Controls.Add(new Label { Text = "Game:", Location = new Point(164, 8), Size = new Size(45, 18), ForeColor = Color.White });
            _cmbMapGame = new ComboBox
            {
                Location = new Point(209, 5),
                Size = new Size(80, 22),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbMapGame.Items.AddRange(new object[] { "All", "OoT", "MM" });
            _cmbMapGame.SelectedIndex = 0;
            mapFilterRow1.Controls.Add(_cmbMapGame);
            var cmbMapGame = _cmbMapGame;
            cmbMapGame.SelectedIndexChanged += (s, e) => RebuildMapRegionList();

            mapFilterRow1.Controls.Add(new Label { Text = "Region:", Location = new Point(297, 8), Size = new Size(55, 18), ForeColor = Color.White });
            _cmbMapRegion = new ComboBox
            {
                Location = new Point(352, 5),
                Size = new Size(180, 22),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode = DrawMode.OwnerDrawFixed
            };

            // Handle custom drawing for region completion indicator
            _cmbMapRegion.DrawItem += (sender, e) =>
            {
                // Always use window background, no selection highlight
                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
                
                if (e.Index < 0) return;
                
                var item = _cmbMapRegion.Items[e.Index];
                
                // First item is "— Select —"
                if (e.Index == 0)
                {
                    e.Graphics.DrawString("— Select —", e.Font, Brushes.Black, e.Bounds);
                    return;
                }
                
                if (item is Models.MapRegion region)
                {
                    // Calculate completion for this region
                    var allLocs = region.SubRegions
                        .SelectMany(s => s.Marks)
                        .Where(m => !m.IsEntranceShuffleMark)
                        .SelectMany(m => m.LocationNames)
                        .Distinct()
                        .ToList();

                    string currentWorld = _cmbMapWorld?.SelectedItem?.ToString() ?? "World 1";
                    int total = GetRegionTotalCount(allLocs, region.Game, currentWorld);
                    int found = GetRegionFoundCount(allLocs, region.Game, currentWorld);

                    // Draw region name with completion indicator
                    string displayText = region.Name;
                    Brush textBrush = Brushes.Black;
                    
                    // If region is fully completed, show with green checkmark (including 0/0)
                    if (found == total)
                    {
                        displayText = "✓ " + displayText;
                        textBrush = Brushes.LimeGreen;
                    }
                    // If region has some progress but not complete
                    else if (found > 0)
                    {
                        displayText = $"{displayText} ({found}/{total})";
                        textBrush = Brushes.Black;
                    }
                    
                    e.Graphics.DrawString(displayText, e.Font, textBrush, e.Bounds);
                }
                else
                {
                    e.Graphics.DrawString(item.ToString(), e.Font, Brushes.Black, e.Bounds);
                }
                
                // Don't draw focus rectangle to avoid any visual selection indicator
                // e.DrawFocusRectangle();
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

            mapFilterRow1.Controls.Add(_cmbMapRegion);
            mapFilterRow1.Controls.Add(new Label { Text = "Sub-map:", Location = new Point(540, 8), Size = new Size(60, 18), ForeColor = Color.White });
            _cmbMapSub = new ComboBox
            {
                Location = new Point(600, 5),
                Size = new Size(180, 22),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode = DrawMode.OwnerDrawFixed,
                Enabled = false
            };

            // Handle custom drawing for sub-region completion indicator
            _cmbMapSub.DrawItem += (sender, e) =>
            {
                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
                if (e.Index < 0) return;

                var item = _cmbMapSub.Items[e.Index];
                if (item is Models.MapSubRegion sub)
                {
                    var region = _cmbMapRegion.SelectedItem as Models.MapRegion;
                    string game = region?.Game ?? "OOT";
                    string currentWorld = _cmbMapWorld?.SelectedItem?.ToString() ?? "World 1";

                    var subLocs = sub.Marks
                        .Where(m => !m.IsEntranceShuffleMark)
                        .SelectMany(m => m.LocationNames)
                        .Distinct()
                        .ToList();
                    int total = GetRegionTotalCount(subLocs, game, currentWorld);
                    int found = GetRegionFoundCount(subLocs, game, currentWorld);

                    string displayText = sub.Name;
                    Brush textBrush = Brushes.Black;

                    if (found == total)
                    {
                        displayText = "✓ " + displayText;
                        textBrush = Brushes.LimeGreen;
                    }
                    else if (found > 0)
                    {
                        displayText = $"{displayText} ({found}/{total})";
                        textBrush = Brushes.Black;
                    }
                    e.Graphics.DrawString(displayText, e.Font, textBrush, e.Bounds);
                }
                else
                {
                    e.Graphics.DrawString(item?.ToString() ?? "", e.Font, Brushes.Black, e.Bounds);
                }
            };
            var cmbMapRegion = _cmbMapRegion;
            var cmbMapSub    = _cmbMapSub;

            // Counters: region total and sub-region
            var lblMapRegionCounter = new Label
            {
                Location = new Point(790, 8), Size = new Size(100, 18),
                ForeColor = Color.FromArgb(180, 180, 180), Text = ""
            };
            var lblMapSubCounter = new Label
            {
                Location = new Point(900, 8), Size = new Size(100, 18),
                ForeColor = Color.FromArgb(180, 180, 180), Text = ""
            };

            // Zoom controls for map
            var btnMapZoomIn = new Button
            {
                Text = "+",
                Size = new Size(24, 24),
                Location = new Point(970, 4),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = "map"
            };
            btnMapZoomIn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            btnMapZoomIn.FlatAppearance.BorderSize = 1;

            var btnMapZoomOut = new Button
            {
                Text = "−",
                Size = new Size(24, 24),
                Location = new Point(1000, 4),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = "map"
            };
            btnMapZoomOut.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            btnMapZoomOut.FlatAppearance.BorderSize = 1;

            var btnMapZoomReset = new Button
            {
                Text = "↺",
                Size = new Size(24, 24),
                Location = new Point(1030, 4),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = "map"
            };
            btnMapZoomReset.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            btnMapZoomReset.FlatAppearance.BorderSize = 1;

          _lblMapZoomLevel = new Label
            {
                Text = "100%",
                Size = new Size(45, 24),
                Location = new Point(1060, 7),
                ForeColor = Color.FromArgb(180, 180, 180),
                Font = new Font("Segoe UI", 9)
            };

            // Add zoom controls to filter panel (row 1)
            mapFilterRow1.Controls.Add(btnMapZoomIn);
            mapFilterRow1.Controls.Add(btnMapZoomOut);
            mapFilterRow1.Controls.Add(btnMapZoomReset);
            mapFilterRow1.Controls.Add(_lblMapZoomLevel);

            // Zoom button handlers
            btnMapZoomIn.Click += (s, e) =>
            {
                _mapTrackerPanel.ZoomIn();
                _lblMapZoomLevel.Text = $"{(int)(_mapTrackerPanel.GetZoomLevel() * 100)}%";
            };
            btnMapZoomOut.Click += (s, e) =>
            {
                _mapTrackerPanel.ZoomOut();
                _lblMapZoomLevel.Text = $"{(int)(_mapTrackerPanel.GetZoomLevel() * 100)}%";
            };
            btnMapZoomReset.Click += (s, e) =>
            {
                _mapTrackerPanel.ResetZoomLevel();
                _mapTrackerPanel.CenterMap();
                _lblMapZoomLevel.Text = "100%";
            };

            // Quick jump (spawn / songs / owls) using Entrances From-id
            var quickJumpsAll = new List<MapJumpTarget>
            {
                new("OOT Spawn Child", "OOT_SPAWN_CHILD"),
                new("OOT Spawn Adult", "OOT_SPAWN_ADULT"),
                new("OOT Warp: Meadow", "OOT_WARP_SONG_MEADOW"),
                new("OOT Warp: Crater", "OOT_WARP_SONG_CRATER"),
                new("OOT Warp: Lake", "OOT_WARP_SONG_LAKE"),
                new("OOT Warp: Desert", "OOT_WARP_SONG_DESERT"),
                new("OOT Warp: Grave", "OOT_WARP_SONG_GRAVE"),
                new("OOT Warp: Temple", "OOT_WARP_SONG_TEMPLE"),
                new("OOT Owl: Village", "OOT_VILLAGE_OWL"),
                new("OOT Owl: Field", "OOT_FIELD_OWL"),
                new("MM Owl: Clock Town", "MM_WARP_OWL_CLOCK_TOWN"),
                new("MM Owl: Milk Road", "MM_WARP_OWL_MILK_ROAD"),
                new("MM Owl: Southern Swamp", "MM_WARP_OWL_SOUTHERN_SWAMP"),
                new("MM Owl: Woodfall", "MM_WARP_OWL_WOODFALL"),
                new("MM Owl: Mountain Village", "MM_WARP_OWL_MOUNTAIN_VILLAGE"),
                new("MM Owl: Snowhead", "MM_WARP_OWL_SNOWHEAD"),
                new("MM Owl: Great Bay", "MM_WARP_OWL_GREAT_BAY"),
                new("MM Owl: Zora Cape", "MM_WARP_OWL_ZORA_CAPE"),
                new("MM Owl: Ikana Canyon", "MM_WARP_OWL_IKANA_CANYON"),
                new("MM Owl: Stone Tower", "MM_WARP_OWL_STONE_TOWER"),
            };
            foreach (var wmId in SpoilerEntranceIdDatabase.GetKnownIds()
                .Where(id => id.Contains("WALLMASTER", StringComparison.OrdinalIgnoreCase))
                .OrderBy(id => id))
            {
                quickJumpsAll.Add(new MapJumpTarget(wmId, wmId));
            }
            mapFilterRow2.Controls.Add(new Label
            {
                Text = "Quick jump:",
                Location = new Point(6, 8),
                Size = new Size(72, 18),
                ForeColor = Color.White
            });
            var cmbQuickJump = new ComboBox
            {
                Location = new Point(80, 5),
                Size = new Size(360, 22),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            var btnQuickJump = new Button
            {
                Text = "Go",
                Size = new Size(44, 24),
                Location = new Point(446, 4),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = "map"
            };
            btnQuickJump.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            btnQuickJump.FlatAppearance.BorderSize = 1;
            mapFilterRow2.Controls.Add(cmbQuickJump);
            mapFilterRow2.Controls.Add(btnQuickJump);

            void UpdateQuickJumpOptions()
            {
                cmbQuickJump.BeginUpdate();
                try
                {
                    var prev = cmbQuickJump.SelectedItem as MapJumpTarget;
                    cmbQuickJump.Items.Clear();
                    string currentWorld = _cmbMapWorld?.SelectedItem?.ToString() ?? "World 1";
                    foreach (var q in quickJumpsAll)
                    {
                        bool exists = false;
                        if (_spoilerLog != null)
                        {
                            exists = _spoilerLog.Entrances.Keys.Any(k =>
                                k.StartsWith(currentWorld, StringComparison.OrdinalIgnoreCase) &&
                                k.Contains(q.FromId, StringComparison.OrdinalIgnoreCase));
                        }
                        if (!exists && DefaultTransitionService.HasDefaultTransition(q.FromId))
                        {
                            exists = true;
                        }
                        if (exists)
                        {
                            var label = BuildQuickJumpDisplayLabel(q);
                            cmbQuickJump.Items.Add(new MapJumpTarget(label, q.FromId));
                        }
                    }
                    cmbQuickJump.Enabled = cmbQuickJump.Items.Count > 0;
                    btnQuickJump.Enabled = cmbQuickJump.Items.Count > 0;

                    if (cmbQuickJump.Items.Count == 0) return;
                    if (prev != null)
                    {
                        for (int i = 0; i < cmbQuickJump.Items.Count; i++)
                        {
                            if (cmbQuickJump.Items[i] is MapJumpTarget t &&
                                string.Equals(t.FromId, prev.FromId, StringComparison.OrdinalIgnoreCase))
                            {
                                cmbQuickJump.SelectedIndex = i;
                                return;
                            }
                        }
                    }
                    cmbQuickJump.SelectedIndex = 0;
                }
                finally
                {
                    cmbQuickJump.EndUpdate();
                }
            }
            _updateQuickJump = UpdateQuickJumpOptions;
            UpdateQuickJumpOptions();

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
                    lblMapSubCounter.Text = "";
                    return;
                }

                if (cmbMapRegion.SelectedItem is Models.MapRegion region)
                {
                    var game = region.Game;
                    string currentWorld = _cmbMapWorld?.SelectedItem?.ToString() ?? "World 1";

                    var allLocs = region.SubRegions
                        .SelectMany(s => s.Marks)
                        .Where(m => !m.IsEntranceShuffleMark)
                        .SelectMany(m => m.LocationNames)
                        .Distinct()
                        .ToList();
                    int total = GetRegionTotalCount(allLocs, game, currentWorld);
                    int found = GetRegionFoundCount(allLocs, game, currentWorld);

                    lblMapRegionCounter.Text = $"Region: {found}/{total}";
                    lblMapRegionCounter.ForeColor = (found == total && total > 0) ? Color.LimeGreen : Color.White;

                    if (cmbMapSub.SelectedItem is Models.MapSubRegion sub)
                    {
                        var subLocs = sub.Marks
                            .Where(m => !m.IsEntranceShuffleMark)
                            .SelectMany(m => m.LocationNames)
                            .Distinct()
                            .ToList();
                        int subTotal = GetRegionTotalCount(subLocs, game, currentWorld);
                        int subFound = GetRegionFoundCount(subLocs, game, currentWorld);

                        lblMapSubCounter.Text = $"Sub: {subFound}/{subTotal}";
                        lblMapSubCounter.ForeColor = (subFound == subTotal && subTotal > 0) ? Color.LimeGreen : Color.White;
                    }
                    else
                    {
                        lblMapSubCounter.Text = "";
                        lblMapSubCounter.ForeColor = Color.White;
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
                UpdateSubRegionsList();
                UpdateMapCounters();
                UpdateMapAccessibleLocations();
            };

            cmbMapSub.SelectedIndexChanged += (s, e) =>
            {
                var sub = cmbMapSub.SelectedItem as Models.MapSubRegion;
                var game = (cmbMapRegion.SelectedItem as Models.MapRegion)?.Game ?? "OOT";
                string currentWorld = _cmbMapWorld?.SelectedItem?.ToString() ?? "World 1";

                _mapTrackerPanel.SetSubRegion(sub, _foundLocations, game, currentWorld);
                _mapTrackerPanel.CenterMap();
                _mapTrackerPanel.ResetZoomLevel();
                _lblMapZoomLevel.Text = "100%";
                UpdateMapCounters();
                UpdateMapAccessibleLocations();
            };

            // Hook into location changes to refresh counters
            _dgvLocations.CellValueChanged += (s, e) => UpdateMapCounters();
            _updateMapCounters = UpdateMapCounters;

            bool TryNavigateToMapSelection(string regionName, string subMapName, string game, string? world, bool showErrors)
            {
                _tabControl.SelectedTab = tabMap;
                if (!string.IsNullOrEmpty(world) && _cmbMapWorld != null)
                {
                    if (_cmbMapWorld.Items.Contains(world))
                    {
                        _cmbMapWorld.SelectedItem = world;
                        UpdateMapWorldFilter();
                    }
                }
                // Save current region and age filter state before changing region
                Models.MapRegion? previousRegion = cmbMapRegion.SelectedItem as Models.MapRegion;
                string currentAgeFilter = "child"; // default
                if (previousRegion != null)
                {
                    if (previousRegion.Game == "MM")
                        currentAgeFilter = _mmClearedState ? "cleared" : "cursed";
                    else
                        currentAgeFilter = _ootAdultState ? "adult" : "child";
                }
                
                cmbMapGame.SelectedItem = game == "MM" ? "MM" : "OoT";
                RebuildMapRegionList();

                Models.MapRegion? targetRegion = null;
                foreach (var it in _cmbMapRegion.Items)
                {
                    if (it is Models.MapRegion mr && mr.Name == regionName && mr.Game == game)
                    {
                        targetRegion = mr;
                        break;
                    }
                }

                if (targetRegion == null)
                {
                    if (showErrors)
                        MessageBox.Show(this, $"Map region not found: {regionName} ({game})", "Entrance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                _cmbMapRegion.SelectedItem = targetRegion;

                // Set age filter for the new region
                // With separate checkboxes, we preserve each game's filter state independently
                if (targetRegion.Game == "MM")
                {
                    // Show MM checkbox, hide OoT checkbox
                    _chkOoTAdult.Visible = false;
                    _chkMMCleared.Visible = true;
                    
                    // Check if we're coming from another MM region
                    if (previousRegion != null && previousRegion.Game == "MM")
                    {
                        // Keep the same MM filter if staying within MM
                        // currentAgeFilter already contains correct MM filter
                        _mmClearedState = currentAgeFilter == "cleared";
                        _chkMMCleared.Checked = _mmClearedState;
                        _mapTrackerPanel.SetAgeFilter(currentAgeFilter);
                    }
                    else
                    {
                        // Coming from OoT or no previous region - use saved MM filter state
                        // Restore saved MM state
                        _chkMMCleared.Checked = _mmClearedState;
                        _mapTrackerPanel.SetAgeFilter(_mmClearedState ? "cleared" : "cursed");
                    }
                }
                else
                {
                    // Show OoT checkbox, hide MM checkbox
                    _chkOoTAdult.Visible = true;
                    _chkMMCleared.Visible = false;
                    
                    // Check if we're coming from another OoT region
                    if (previousRegion != null && previousRegion.Game == "OOT")
                    {
                        // Keep the same OoT filter if staying within OoT
                        // currentAgeFilter already contains correct OoT filter
                        _ootAdultState = currentAgeFilter == "adult";
                        _chkOoTAdult.Checked = _ootAdultState;
                        _mapTrackerPanel.SetAgeFilter(currentAgeFilter);
                    }
                    else
                    {
                        // Coming from MM or no previous region - use saved OoT filter state
                        // Restore saved OoT state
                        _chkOoTAdult.Checked = _ootAdultState;
                        _mapTrackerPanel.SetAgeFilter(_ootAdultState ? "adult" : "child");
                    }
                }

                bool foundSub = false;
                for (int i = 0; i < cmbMapSub.Items.Count; i++)
                {
                    if (cmbMapSub.Items[i] is Models.MapSubRegion su && su.Name == subMapName)
                    {
                        cmbMapSub.SelectedIndex = i;
                        foundSub = true;
                        break;
                    }
                }

                if (!foundSub)
                {
                    if (showErrors)
                        MessageBox.Show(this, $"Sub-map not found: {subMapName}", "Entrance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return true;
            }

            _mapTrackerPanel.NavigateMapTo += (regionName, subMapName, game) =>
            {
                string currentWorld = _cmbMapWorld?.SelectedItem?.ToString();
                _ = TryNavigateToMapSelection(regionName, subMapName, game, currentWorld, showErrors: true);
            };

            btnQuickJump.Click += (s, e) =>
            {
                if (cmbQuickJump.SelectedItem is not MapJumpTarget target)
                    return;

                string region = null;
                string sub = null;
                string game = null;
                string world = null;
                bool success = false;
                string currentWorld = _cmbMapWorld?.SelectedItem?.ToString() ?? "World 1";
                if (_spoilerLog != null)
                {
                    var entranceKey = _spoilerLog.Entrances.Keys
                        .FirstOrDefault(k => k.StartsWith(currentWorld, StringComparison.OrdinalIgnoreCase)
                                           && k.Contains(target.FromId, StringComparison.OrdinalIgnoreCase));

                    if (entranceKey != null)
                    {
                        string destLine = _spoilerLog.Entrances[entranceKey];
                        if (EntranceMapNavigation.TryGetMapForDestinationLine(destLine, out region, out sub, out game))
                        {
                            if (destLine.StartsWith("World "))
                            {
                                var parts = destLine.Split(' ', 3);
                                if (parts.Length >= 2) world = $"{parts[0]} {parts[1]}";
                            }
                            else
                            {
                                world = currentWorld;
                            }
                            success = true;
                        }
                    }
                }
                if (!success && DefaultTransitionService.HasDefaultTransition(target.FromId))
                {
                    if (DefaultTransitionService.TryGetMapForDefaultTransition(target.FromId, out region, out sub, out game))
                    {
                        world = currentWorld;
                        success = true;
                    }
                }
                else if (!success && DefaultTransitionService.IsWallmaster(target.FromId))
                {
                    MessageBox.Show(this, "Wallmaster teleports within the same area.", "Quick jump", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (!success && EntranceMapNavigation.TryGetMapForDestinationId(target.FromId, out region, out sub, out game))
                {
                    world = currentWorld;
                    success = true;
                }

                if (success && region != null)
                {
                    _ = TryNavigateToMapSelection(region, sub, game, world, showErrors: true);
                }
                else
                {
                    MessageBox.Show(this, $"No transition found for {target.FromId} in {currentWorld}.", "Quick jump", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            mapFilterRow1.Controls.Add(cmbMapSub);
            mapFilterRow1.Controls.Add(lblMapRegionCounter);
            mapFilterRow1.Controls.Add(lblMapSubCounter);

            // Age/state filter — OoT: child/adult, MM: cursed/cleared
            // Separate checkboxes for OoT and MM to preserve state when switching between games
            _chkOoTAdult = new CheckBox
            {
                Text      = "Adult",
                Location  = new Point(668, 7),
                Size      = new Size(85, 18),
                ForeColor = Color.White,
                Checked   = _ootAdultState,
                Visible   = false
            };
            _chkOoTAdult.CheckedChanged += (s, e) =>
            {
                _ootAdultState = _chkOoTAdult.Checked;
                if (cmbMapRegion.SelectedItem is Models.MapRegion r && r.Game == "OOT")
                {
                    _mapTrackerPanel.SetAgeFilter(_chkOoTAdult.Checked ? "adult" : "child");
                }
            };

            _chkMMCleared = new CheckBox
            {
                Text      = "Cleared",
                Location  = new Point(668, 7),  // Same position as OoT checkbox
                Size      = new Size(85, 18),
                ForeColor = Color.White,
                Checked   = _mmClearedState,
                Visible   = false
            };
            _chkMMCleared.CheckedChanged += (s, e) =>
            {
                _mmClearedState = _chkMMCleared.Checked;
                if (cmbMapRegion.SelectedItem is Models.MapRegion r && r.Game == "MM")
                {
                    _mapTrackerPanel.SetAgeFilter(_chkMMCleared.Checked ? "cleared" : "cursed");
                }
            };

            // Show appropriate filter only for OoT or MM regions
            cmbMapRegion.SelectedIndexChanged += (s2, e2) =>
            {
                if (cmbMapRegion.SelectedItem is Models.MapRegion r)
                {
                    if (r.Game == "MM")
                    {
                        _chkOoTAdult.Visible = false;
                        _chkMMCleared.Visible = true;
                        _chkMMCleared.Text = "Cleared";
                        // Restore saved MM state
                        _chkMMCleared.Checked = _mmClearedState;
                        _mapTrackerPanel.SetAgeFilter(_chkMMCleared.Checked ? "cleared" : "cursed");
                    }
                    else
                    {
                        _chkOoTAdult.Visible = true;
                        _chkOoTAdult.Text = "Adult";
                        _chkMMCleared.Visible = false;
                        // Restore saved OoT state
                        _chkOoTAdult.Checked = _ootAdultState;
                        _mapTrackerPanel.SetAgeFilter(_chkOoTAdult.Checked ? "adult" : "child");
                    }
                }
                else
                {
                    _chkOoTAdult.Visible = false;
                    _chkMMCleared.Visible = false;
                    _mapTrackerPanel.SetAgeFilter("both");
                }
            };
            mapFilterRow1.Controls.Add(_chkOoTAdult);
            mapFilterRow1.Controls.Add(_chkMMCleared);
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
            ItemTrackerPanel.ItemChanged += () =>
            {
                UpdateBroadcast();
                UpdateMapAccessibleLocations();
            };
            // Unsubscribe on form close to prevent memory leaks
            this.FormClosed += (s, e) =>
            {
                ItemTrackerPanel.ItemChanged -= () =>
                {
                    UpdateBroadcast();
                    UpdateMapAccessibleLocations();
                };
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
            if (resetProgress)
            {
                _sessionProgress.Clear();
            }
            else
            {
                foreach (var panel in _trackerPanels)
                {
                    panel.SaveProgress(_sessionProgress);
                }
            }
            foreach (var panel in _trackerPanels) panel.Dispose();
            _trackerPanels.Clear();
            _trackerScrollPanel.Controls.Clear();
            ItemTrackerPanel.ClearGlobal();
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
                Location = new Point(0, 0),
            };

            var topSpacer = new Panel
            {
                AutoSize = false,
                Width = 100,
                Height = 32,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = Color.FromArgb(30, 30, 30),
            };
            mainFlow.Controls.Add(topSpacer);

            var cfg = _currentTrackerConfig;
            string currentWorld = _cmbTrackerWorldFilter?.SelectedItem?.ToString() ?? "World 1";
            bool hasShared = TrackerItemsData.GetSharedItems(cfg).Count > 0
                          || TrackerItemsData.GetSharedItems_Items(cfg).Count > 0
                          || TrackerItemsData.GetSharedBottles(cfg).Count > 0;

            void Add(string title, List<TrackerItem> items, int cols = 6)
            {
                if (items.Count == 0) return;
                int actualCols = Math.Min(cols, items.Count);
                var p = new ItemTrackerPanel(title, items, actualCols, _trackerToolTip);
                p.World = currentWorld;
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

                bool isMq = cfg.IsMq(id);
                var blockName = isMq ? $"{name} (MQ)" : name;
                Add(blockName, TrackerItemsData.GetDungeonSingle(id, name, label, cfg), int.MaxValue);
            }

            foreach (var (id, name, label) in TrackerItemsData.SubDungeonList)
            {
                if (!cfg.HasOot) continue;
                bool isMq = cfg.IsMq(id);
                var blockName = isMq ? $"{name} (MQ)" : name;
                Add(blockName, TrackerItemsData.GetDungeonSingle(id, name, label, cfg), int.MaxValue);
            }
            if (cfg.HasOot && cfg.SmallKeysHideout)
                Add("Thieves' Hideout", TrackerItemsData.GetDungeonSingle("thieves_hideout", "Thieves' Hideout", "dungeons/labels/thieves_hideout.png", cfg), int.MaxValue);
            if (cfg.HasOot && cfg.SmallKeysTcg)
                Add("Treasure Chest Game", TrackerItemsData.GetDungeonSingle("tcg", "Treasure Chest Game", "dungeons/labels/treasure_chest_game.png", cfg), int.MaxValue);

            // ── 12. Skeleton Key + Magical Rupee ──────────────────────────────
            Add("Dungeon Items", TrackerItemsData.GetDungeonSpecialItems(cfg), 4);

            // ── 13. Rusty Keys ────────────────────────────────────────────────
            // Rusty Keys OoT
            var rustyKeysOot = TrackerItemsData.GetRustyKeysOot(cfg);
            if (rustyKeysOot.Count > 0)
                Add("Rusty Keys (OoT)", rustyKeysOot, 7);

            // Rusty Keys MM
            var rustyKeysMm = TrackerItemsData.GetRustyKeysMm(cfg);
            if (rustyKeysMm.Count > 0)
                Add("Rusty Keys (MM)", rustyKeysMm, 8);

            // ── 14. Souls ─────────────────────────────────────────────────────
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
                panel.LoadProgress(_sessionProgress);

            if (_spoilerLog != null)
                ApplyBottlesFromLog();

            if (_spoilerLog != null)
                ApplyStartingItems();

            foreach (var panel in _trackerPanels)
                panel.LoadProgress(_sessionProgress);
            _trackerScrollPanel.Controls.Add(mainFlow);
            UpdateBroadcast();
            PopulateSongEvents();
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
            string currentWorld = _cmbTrackerWorldFilter?.SelectedItem?.ToString() ?? "World 1";
            var cfg = _currentTrackerConfig;

            var ootBottles = new List<string>();
            var mmBottles = new List<string>();
            bool isMultiworld = false;
            int currentPlayerNum = 0;
            if (currentWorld.StartsWith("World "))
                int.TryParse(currentWorld.Substring(6), out currentPlayerNum);

            foreach (var loc in _spoilerLog.Locations.Values)
            {
                if (!isMultiworld && loc.Item.StartsWith("Player ", StringComparison.OrdinalIgnoreCase))
                    isMultiworld = true;

                bool isBottle = loc.Item.Contains("Bottle", StringComparison.OrdinalIgnoreCase)
                               || loc.Item.Contains("Ruto's Letter", StringComparison.OrdinalIgnoreCase)
                               || (loc.Item.StartsWith("Player ", StringComparison.OrdinalIgnoreCase) && (
                                   loc.Item.Contains("Letter") || loc.Item.Contains("Milk") ||
                                   loc.Item.Contains("Potion") || loc.Item.Contains("Poe") ||
                                   loc.Item.Contains("Fire") || loc.Item.Contains("Fairy") ||
                                   loc.Item.Contains("Fish") || loc.Item.Contains("Bugs") ||
                                   loc.Item.Contains("Dust") || loc.Item.Contains("Egg") ||
                                   loc.Item.Contains("Horse") || loc.Item.Contains("Princess") ||
                                   loc.Item.Contains("Mushroom") || loc.Item.Contains("Spring") ||
                                   loc.Item.Contains("Water")
                               ));

                if (!isBottle) continue;

                if (isMultiworld)
                {
                    if (!loc.Item.StartsWith("Player ", StringComparison.OrdinalIgnoreCase)) continue;
                    var parts = loc.Item.Split(new[] { ' ' }, 3);
                    if (parts.Length < 2) continue;
                    int itemPlayerNum = 0;
                    int.TryParse(parts[1], out itemPlayerNum);
                    if (itemPlayerNum != currentPlayerNum) continue;
                }
                else
                {
                    if (loc.World != currentWorld) continue;
                }

                string cleanItem = GetCleanPlayerItemName(loc.Item);
                var content = StartingItemsMapper.GetBottleContentPublic(cleanItem);
                if (content == null || content == "Empty") continue;

                var itemLower = cleanItem.ToLower();
                if (itemLower.Contains("(mm)")) mmBottles.Add(content);
                else if (itemLower.Contains("(oot)")) ootBottles.Add(content);
                else if (content is "Ruto's Letter" or "Big Poe" or "Blue Fire") ootBottles.Add(content);
                else if (content is "Gold Dust" or "Chateau Romani" or "Spring Water" or "Hot Spring Water"
                       or "Zora Egg" or "Seahorse" or "Deku Princess" or "Magic Mushroom") mmBottles.Add(content);
                else ootBottles.Add(content);
            }

            var sortedOot = ootBottles.OrderBy(c => GetBottlePriority(c, StartingItemsMapper.OotBottleContentNames)).ToList();
            var sortedMm = mmBottles.OrderBy(c => GetBottlePriority(c, StartingItemsMapper.MmBottleContentNames)).ToList();

            var assignments = new Dictionary<string, string>();
            if (cfg.SharedBottles)
            {
                var all = ootBottles.Concat(mmBottles)
                                    .OrderBy(c => GetBottlePriority(c, StartingItemsMapper.ShBottleContentNames))
                                    .ToList();
                for (int i = 0; i < Math.Min(all.Count, 6); i++)
                    assignments[$"sh_bottle_{i + 1}"] = all[i];
            }
            else
            {
                for (int i = 0; i < Math.Min(sortedOot.Count, 4); i++)
                    assignments[$"oot_bottle_{i + 1}"] = sortedOot[i];
                for (int i = 0; i < Math.Min(sortedMm.Count, 6); i++)
                    assignments[$"mm_bottle_{i + 1}"] = sortedMm[i];
            }

            foreach (var panel in _trackerPanels)
            {
                if (panel.World != currentWorld) continue;

                var bottles = panel.Items.Where(i => i.IsBottle).ToList();
                int ootIdx = 0;
                int mmIdx = 0;

                foreach (var bottle in bottles)
                {
                    bool isMm = bottle.Id.Contains("_mm_") || bottle.Id.StartsWith("mm_");

                    string? assignedContent = null;
                    if (isMm && mmIdx < sortedMm.Count)
                        assignedContent = sortedMm[mmIdx];
                    else if (!isMm && ootIdx < sortedOot.Count)
                        assignedContent = sortedOot[ootIdx];

                    if (assignedContent != null)
                    {
                        bottle.BottleContent = assignedContent;
                        if (bottle.BottleContentIcons != null && bottle.BottleContentNames != null)
                        {
                            int idx = Array.IndexOf(bottle.BottleContentNames, assignedContent);
                            if (idx >= 0) bottle.IconPath = bottle.BottleContentIcons[idx];
                        }
                        bottle.MaxCount = (bottle.BottleContent is "Ruto's Letter" or "Gold Dust") ? 2 : 1;
                        bottle.CurrentCount = 0;

                        if (isMm) mmIdx++;
                        else ootIdx++;
                    }
                    else
                    {
                        bottle.BottleContent = "Empty";
                        if (bottle.BottleContentIcons != null && bottle.BottleContentNames != null)
                        {
                            int idx = Array.IndexOf(bottle.BottleContentNames, "Empty");
                            if (idx >= 0) bottle.IconPath = bottle.BottleContentIcons[idx];
                        }
                        bottle.MaxCount = 0;
                        bottle.CurrentCount = 0;
                    }
                }

                foreach (Control c in panel.Controls)
                {
                    if (c is PictureBox pb && pb.Tag is TrackerItem item && item.IsBottle)
                    {
                        ItemTrackerPanel.RefreshIconPublic(pb, item);
                        pb.Invalidate();
                    }
                }
                panel.Invalidate();
            }
        }




        private static int GetBottlePriority(string content, string[] names)
        {
            for (int i = 0; i < names.Length; i++)
                if (string.Equals(names[i], content, StringComparison.OrdinalIgnoreCase))
                    return i;
            return names.Length;
        }

        private void ApplyStartingItems()
        {
            if (_spoilerLog == null || _spoilerLog.StartingItems.Count == 0) return;

            var itemsByWorld = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in _spoilerLog.StartingItems)
            {
                string rawKey = kv.Key;
                string playerPart = rawKey;
                string cleanKey = rawKey;

                if (rawKey.Contains(':'))
                {
                    var colonParts = rawKey.Split(new[] { ':' }, 2);
                    playerPart = colonParts[0].Trim();
                    cleanKey = colonParts[1].Trim();
                }

                string? itemWorld = null;
                if (playerPart.StartsWith("Player ", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = playerPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 &&
                        string.Equals(parts[0], "Player", StringComparison.OrdinalIgnoreCase) &&
                        int.TryParse(parts[1], out int playerNum))
                    {
                        itemWorld = $"World {playerNum}";
                    }
                }

                if (itemWorld == null) continue;

                if (!itemsByWorld.TryGetValue(itemWorld, out var dict))
                {
                    dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    itemsByWorld[itemWorld] = dict;
                }
                dict[cleanKey] = kv.Value;
            }

            if (itemsByWorld.Count == 0) return;
            foreach (var (world, playerItems) in itemsByWorld)
            {
                var tempLog = new SpoilerLog
                {
                    Settings = _spoilerLog.Settings,
                    WorldFlags = _spoilerLog.WorldFlags,
                    Locations = _spoilerLog.Locations,
                    StartingItems = playerItems
                };

                var tempProgress = new Dictionary<string, int>();
                StartingItemsMapper.Apply(tempLog, tempProgress);
                foreach (var kv in tempProgress)
                {
                    string key = $"{world}|{kv.Key}";
                    if (!_sessionProgress.TryGetValue(key, out int existing) || existing < kv.Value)
                    {
                        _sessionProgress[key] = kv.Value;
                    }
                }
                foreach (var panel in _trackerPanels)
                {
                    if (!string.Equals(panel.World, world, StringComparison.OrdinalIgnoreCase))
                        continue;
                    panel.LockStartingItems(tempProgress);
                }
            }
        }


        private void BtnTrackerOptions_Click(object? sender, EventArgs e)        {
            using var dlg = new TrackerOptionsForm(_currentTrackerConfig);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _currentTrackerConfig = dlg.Config;
                SaveTrackerSettings();
                RebuildTracker();
                _mapLogicEvaluator = new MapLogicEvaluator(
                    (id) =>
                    {
                        foreach (var panel in _trackerPanels)
                        {
                            var item = panel.GetItemById(id);
                            if (item != null) return item;
                        }
                        return null;
                    },
                    (locationNames) => _foundLocations.Contains(locationNames),
                    _currentTrackerConfig
                );
                UpdateMapAccessibleLocations();
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
                UpdateTrackerWorldFilterVisibility();
                RefreshComboBoxDisplay();
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
            if (_dgvSongEvents.Columns.Contains("Song"))
            {
                _dgvSongEvents.Columns["Song"].ReadOnly = false;
            }
            _mapTrackerPanel.SetKnownLocations(new HashSet<string>());
            _mapTrackerPanel.UpdateFoundLocations(_foundLocations);
            // Reset map region selection — clears map and counters
            if (_cmbMapRegion != null) _cmbMapRegion.SelectedIndex = 0;
            if (_cmbMapSub    != null) { _cmbMapSub.Items.Clear(); _cmbMapSub.Enabled = false; }
            // Refresh combo box display to update completion indicators
            _coloredLocations.Clear();
            UpdateMapColoredLocations();
            RefreshComboBoxDisplay();
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
            UpdateTrackerWorldFilterVisibility();
            _updateQuickJump?.Invoke();
            RebuildTracker(resetProgress: true);
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
            // Refresh combo box display to update completion indicators
            UpdateMapColoredLocations();
            UpdateTrackerWorldFilterVisibility();
            RefreshComboBoxDisplay();
        }

        private void BtnResetProgress_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Reset tracker progress?\n(Settings and starting items will be preserved)", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            RebuildTracker(resetProgress: true);
            _foundLocations.Clear();
            _mapTrackerPanel.UpdateFoundLocations(_foundLocations);
            _updateMapCounters?.Invoke();
            if (_cmbFoundFilter != null) _cmbFoundFilter.SelectedIndex = 0;
            UpdateLocationsList(_txtSearch.Text);
            RefreshComboBoxDisplay();
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
                foreach (var panel in _trackerPanels)
                    panel.SaveProgress(_sessionProgress);
                var progress = _sessionProgress;
                foreach (var kv in progress)
                {
                    _sessionProgress[kv.Key] = kv.Value;
                }
                var save = new TrackerSaveData
                {
                    Progress = new Dictionary<string, int>(_sessionProgress),
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
                _sessionProgress.Clear();
                MigrateBottleIds(save.Progress);
                foreach (var kv in save.Progress)
                {
                    string key = kv.Key;
                    if (key.StartsWith("Player ", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = key.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && int.TryParse(parts[1], out int playerNum))
                        {
                            key = $"World {playerNum}|{parts[2]}";
                        }
                    }
                    _sessionProgress[key] = kv.Value;
                }
                foreach (var panel in _trackerPanels)
                    panel.LoadProgress(_sessionProgress);

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
                // Refresh combo box display to update completion indicators
                RefreshComboBoxDisplay();

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
                BuildLocationCache();
                _locationsByWorld.Clear();
                foreach (var loc in _spoilerLog.Locations.Values)
                {
                    string world = loc.World;
                    if (!_locationsByWorld.ContainsKey(world))
                        _locationsByWorld[world] = new HashSet<string>();
                    string locWithoutWorld = loc.Location;
                    if (loc.Location.StartsWith("World "))
                    {
                        var parts = loc.Location.Split(new[] { ' ' }, 3);
                        if (parts.Length >= 3)
                            locWithoutWorld = parts[2];
                    }
                    _locationsByWorld[world].Add(locWithoutWorld);
                }
                var seedPreview = _spoilerLog.Seed.Length > 16 ? _spoilerLog.Seed.Substring(0, 16) + "..." : _spoilerLog.Seed;
                var fileName = Path.GetFileName(filePath);
                _lblInfo.Text = $"{fileName} | {_spoilerLog.Version} | Seed: {seedPreview} | Locations: {_spoilerLog.Locations.Count} | Entrances: {_spoilerLog.Entrances.Count}";
                _cmbGameFilter.SelectedIndex = 0;
                bool hasOot = _spoilerLog.Locations.Values.Any(l => l.Game == "OOT");
                bool hasMm = _spoilerLog.Locations.Values.Any(l => l.Game == "MM");
                _cmbGameFilter.Enabled = hasOot && hasMm;
                if (!_cmbGameFilter.Enabled) _cmbGameFilter.SelectedIndex = 0;
                UpdateWorldFilterVisibility();
                _currentTrackerConfig = TrackerConfig.FromSpoilerLog(_spoilerLog);
                UpdateTrackerConfigForCurrentWorld();
                _mapLogicEvaluator = new MapLogicEvaluator(
                    (id) =>
                    {
                        foreach (var panel in _trackerPanels)
                        {
                            var item = panel.GetItemById(id);
                            if (item != null) return item;
                        }
                        return null;
                    },
                    (locationName) =>
                    {
                        return _foundLocations.Any(fl => fl.EndsWith($"|{locationName}", StringComparison.OrdinalIgnoreCase));
                    },
                    _currentTrackerConfig);
                _foundLocations.Clear();
                _sessionProgress.Clear();
                PopulateRegionFilter();
                UpdateLocationsList();
                UpdateWorldFlagsList();
                UpdateEntrancesList();
                UpdateSettingsList();
                UpdateTricksList();
                UpdateStartingItemsList();
                UpdateSpecialConditionsList();
                _songEvents.Clear();
                var knownLocs = new HashSet<string>(
                    _spoilerLog.Locations.Values.Select(l => $"{l.World}|{l.Game}|{l.Location}")
                );
                LoadSongEventsFromLog();
                PopulateSongEvents();
                _knownLocations = knownLocs;
                _mapTrackerPanel.SetKnownLocations(knownLocs);
                _mapTrackerPanel.SetSpoilerLog(_spoilerLog);
                _mapTrackerPanel.UpdateFoundLocations(_foundLocations);
                _cmbMapGame.Items.Clear();
                if (_currentTrackerConfig.HasOot && _currentTrackerConfig.HasMm)
                    _cmbMapGame.Items.AddRange(new object[] { "All", "OoT", "MM" });
                else if (_currentTrackerConfig.HasOot)
                    _cmbMapGame.Items.AddRange(new object[] { "OoT" });
                else
                    _cmbMapGame.Items.AddRange(new object[] { "MM" });
                _cmbMapGame.SelectedIndex = 0;
                UpdateMapWorldFilterVisibility();
                if (_cmbMapWorld.Items.Count <= 1)
                {
                    _cmbMapWorld.SelectedIndex = 0;
                }
                _cmbTrackerWorldFilter.Items.Clear();
                var worldsForTracker = _spoilerLog.Locations.Values
                    .Select(l => l.World)
                    .Where(w => !string.IsNullOrEmpty(w))
                    .Distinct()
                    .OrderBy(w => w, new NaturalStringComparer())
                    .ToList();

                foreach (var w in worldsForTracker)
                    _cmbTrackerWorldFilter.Items.Add(w);

                if (_cmbTrackerWorldFilter.Items.Count > 0)
                    _cmbTrackerWorldFilter.SelectedIndex = 0;
                UpdateMapWorldFilter();
                UpdateMapColoredLocations();
                _updateMapCounters?.Invoke();
                UpdateTrackerWorldFilterVisibility();
                RefreshComboBoxDisplay();
                SaveTrackerSettings();
                _updateQuickJump?.Invoke();
                if (_cmbTrackerWorldFilter.Items.Count > 0)
                    _cmbTrackerWorldFilter.SelectedIndex = 0;
                RebuildTracker(resetProgress: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateMapAccessibleLocations()
        {
            if (_mapLogicEvaluator == null || _mapTrackerPanel == null) return;
            string currentWorld = _cmbMapWorld?.SelectedItem?.ToString() ?? "World 1";

            var accessibleLocations = new HashSet<string>();
            var accessibleEntrances = new HashSet<string>();
            var currentRegion = _cmbMapRegion.SelectedItem as Models.MapRegion;
            if (currentRegion == null) return;

            foreach (var subRegion in currentRegion.SubRegions)
            {
                foreach (var mark in subRegion.Marks)
                {
                    foreach (var loc in mark.LocationNames)
                    {
                        if (_mapLogicEvaluator.CanAccessLocation(loc))
                        {
                            accessibleLocations.Add($"{currentWorld}|{currentRegion.Game}|{loc}");
                        }
                    }

                    if (mark.IsEntranceShuffleMark && !string.IsNullOrEmpty(mark.EntranceFromId))
                    {
                        if (_mapLogicEvaluator.CanAccessEntrance(mark.EntranceFromId))
                        {
                            accessibleEntrances.Add(mark.EntranceFromId);
                        }
                    }
                }
            }
            _mapTrackerPanel.SetAccessibleLocations(accessibleLocations, currentWorld);
            _mapTrackerPanel.SetAccessibleEntrances(accessibleEntrances, currentWorld);
        }

        private void RefreshComboBoxDisplay()
        {
            _cmbMapRegion?.Invalidate();
            _cmbMapSub?.Invalidate();
        }
        private void UpdateMapWorldFilter()
        {
            if (_cmbMapWorld == null || _spoilerLog == null) return;
            string? selectedWorld = _cmbMapWorld.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedWorld)) return;
            UpdateMapMarkersForWorld(selectedWorld);
            _mapTrackerPanel.SetCurrentWorld(selectedWorld);
            UpdateSubRegionsList();
            UpdateMapAccessibleLocations();
            _updateMapCounters?.Invoke();
            _mapTrackerPanel.Invalidate();
        }

        private void UpdateMapMarkersForWorld(string world)
        {
            if (_mapTrackerPanel == null || _spoilerLog == null) return;

            var worldLocations = new HashSet<string>();
            foreach (var loc in _spoilerLog.Locations.Values)
            {
                if (world != null && loc.World != world)
                    continue;
                worldLocations.Add($"{loc.World}|{loc.Game}|{loc.Location}");
            }
            _mapTrackerPanel.SetKnownLocations(worldLocations);
        }
        private void UpdateMapWorldFilterVisibility()
        {
            if (_cmbMapWorld == null) return;

            if (_spoilerLog == null)
            {
                _cmbMapWorld.Visible = false;
                return;
            }

            var worlds = _spoilerLog.Locations.Values
                .Select(l => l.World)
                .Where(w => !string.IsNullOrEmpty(w))
                .Distinct()
                .OrderBy(w => w, new NaturalStringComparer())
                .ToList();

            if (worlds.Count > 1)
            {
                _cmbMapWorld.Items.Clear();
                foreach (var world in worlds)
                    _cmbMapWorld.Items.Add(world);
                _cmbMapWorld.SelectedIndex = 0;
                _cmbMapWorld.Visible = true;
            }
            else
            {
                _cmbMapWorld.Items.Clear();
                _cmbMapWorld.Items.Add(worlds.FirstOrDefault() ?? "World 1");
                _cmbMapWorld.SelectedIndex = 0;
                _cmbMapWorld.Visible = false;
            }
        }
        private void UpdateLocationsList(string? searchText = null)
        {
            if (_spoilerLog == null)
            {
                _dgvLocations.SuspendLayout();
                _dgvLocations.Rows.Clear();
                _dgvLocations.ResumeLayout();
                return;
            }

            string? search = string.IsNullOrWhiteSpace(searchText) ? null : searchText.ToLower();
            string? worldFilter = (_cmbWorldFilter.Visible && _cmbWorldFilter.SelectedIndex > 0)
                ? _cmbWorldFilter.SelectedItem?.ToString()
                : null;
            string? gameFilter = _cmbGameFilter.SelectedIndex > 0
                ? (_cmbGameFilter.SelectedItem?.ToString() == "OoT" ? "OOT" : _cmbGameFilter.SelectedItem?.ToString())
                : null;
            string? regionFilter = _cmbRegionFilter.SelectedIndex > 0
                ? _cmbRegionFilter.SelectedItem?.ToString()
                : null;
            int statusFilter = _cmbFoundFilter?.SelectedIndex ?? 0;

            var newRows = new List<(bool found, string world, string game, string region, string location, string item, string key)>();

            foreach (var l in _spoilerLog.Locations.Values)
            {
                if (search != null &&
                    !l.Location.ToLower().Contains(search) &&
                    !l.Item.ToLower().Contains(search))
                    continue;
                if (worldFilter != null && l.World != worldFilter) continue;
                if (gameFilter != null && l.Game != gameFilter) continue;
                if (regionFilter != null && l.Region != regionFilter) continue;

                var key = $"{l.World}|{l.Game}|{l.Location}";
                bool found = _foundLocations.Contains(key);
                if (statusFilter == 1 && found) continue;
                if (statusFilter == 2 && !found) continue;

                newRows.Add((found, l.World, l.Game, l.Region, l.Location, l.Item, key));
            }

            newRows.Sort((a, b) =>
            {
                int cmp = string.Compare(a.world, b.world, StringComparison.OrdinalIgnoreCase);
                if (cmp != 0) return cmp;
                cmp = string.Compare(a.region, b.region, StringComparison.OrdinalIgnoreCase);
                if (cmp != 0) return cmp;
                return string.Compare(a.location, b.location, StringComparison.OrdinalIgnoreCase);
            });

            _dgvLocations.SuspendLayout();
            _dgvLocations.Rows.Clear();
            foreach (var r in newRows)
            {
                var rowIndex = _dgvLocations.Rows.Add(r.found, r.world, r.game, r.region, r.location, r.item);
                _dgvLocations.Rows[rowIndex].Tag = r.key;
            }
            _dgvLocations.ResumeLayout();

            int total = _dgvLocations.Rows.Count;
            int checked_ = 0;
            for (int i = 0; i < _dgvLocations.Rows.Count; i++)
                if (_dgvLocations.Rows[i].Cells[0].Value is true) checked_++;

            if (_lblCounter != null)
                _lblCounter.Text = $"{checked_}/{total} checked";
        }


        private static string LocationKey(DataGridViewRow row)
        => row.Tag?.ToString() ?? "";

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
            UpdateMapAccessibleLocations();
            // Update counter
            int total    = _dgvLocations.Rows.Count;
            int checked_ = _dgvLocations.Rows.Cast<DataGridViewRow>().Count(r => r.Cells[0].Value is true);
            if (_lblCounter != null)
                _lblCounter.Text = $"{checked_}/{total} checked";
            // Refresh combo box display to update completion indicators
            RefreshComboBoxDisplay();
            // If filter is active — update list
            if (_cmbFoundFilter?.SelectedIndex > 0)
                UpdateLocationsList(_txtSearch.Text);
        }

        private void DgvLocations_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex == 0) return;
            var row = _dgvLocations.Rows[e.RowIndex];
            var key = LocationKey(row);
            bool found = _foundLocations.Contains(key);
            var item = row.Cells[5].Value?.ToString() ?? "";

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

            e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
        }

        private static bool IsTrap(string item)
        {
            string cleanItem = GetCleanItemName(item);
            return cleanItem.Contains("Trap", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsConsumable(string item)
        {
            if (string.IsNullOrEmpty(item)) return false;
            string cleanItem = GetCleanItemName(item);
            string lower = cleanItem.ToLower();
            if (System.Text.RegularExpressions.Regex.IsMatch(cleanItem, @"^\d+\s") &&
                !lower.Contains("upgrade") && !lower.Contains("bag"))
                return true;
            if (System.Text.RegularExpressions.Regex.IsMatch(lower, @"^(green|blue|red|purple|huge|gold)?\s*rupee"))
                return true;
            if (lower.StartsWith("silver rupee") &&
                (lower.Contains("(mm)") || lower.Contains("(oot)") || !lower.Contains("(")))
                return true;
            if (lower.StartsWith("recovery heart") ||
                lower.StartsWith("small magic jar") ||
                lower.StartsWith("large magic jar"))
                return true;
            if (lower.StartsWith("fairy") &&
                !lower.Contains("bow") && !lower.Contains("slingshot") &&
                !lower.Contains("ocarina") && !lower.Contains("sword") && !lower.Contains("mask"))
                return true;
            if (lower.StartsWith("big fairy"))
                return true;
            if ((lower.StartsWith("bomb") || lower.StartsWith("bombchu")) &&
                !lower.Contains("bag") && !lower.Contains("upgrade"))
                return true;
            if (lower.StartsWith("deku stick") && !lower.Contains("upgrade"))
                return true;
            if (lower.StartsWith("blue fire") || lower.StartsWith("blue potion") ||
                lower.StartsWith("red potion") || lower.StartsWith("green potion") ||
                lower.StartsWith("chateau romani refill") ||
                lower.StartsWith("bugs") || lower.StartsWith("poe") ||
                lower.StartsWith("child fish") || lower.StartsWith("adult fish") ||
                lower.StartsWith("spring water") || lower.StartsWith("hot spring water") ||
                lower.StartsWith("milk refill") || lower.StartsWith("lon lon milk"))
                return true;
            if (lower.StartsWith("lon lon milk") || lower == "romani milk")
                return true;
            if (lower == "nothing") return true;

            return false;
        }
        private static string GetCleanItemName(string item)
        {
            if (string.IsNullOrEmpty(item)) return "";
            return System.Text.RegularExpressions.Regex.Replace(item, @"^Player\s+\d+\s+", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        private string GetCleanPlayerItemName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return "";
            return System.Text.RegularExpressions.Regex.Replace(
                rawName,
                @"^Player\s+\d+\s+",
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
        }
        private void UpdateMapColoredLocations()
        {
            if (_mapTrackerPanel == null) return;

            var colored = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (_spoilerLog != null)
            {
                foreach (var loc in _spoilerLog.Locations.Values)
                {
                    if (IsTrap(loc.Item) || IsConsumable(loc.Item))
                    {
                        colored.Add($"{loc.World}|{loc.Game}|{loc.Location}");
                    }
                }
            }

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
        private void UpdateWorldFilterVisibility()
        {
            if (_spoilerLog == null) return;

            var worlds = _spoilerLog.Locations.Values
                .Select(l => l.World)
                .Where(w => !string.IsNullOrEmpty(w))
                .Distinct()
                .OrderBy(w => w)
                .ToList();

            bool isMultiworld = worlds.Count > 1;

            if (isMultiworld)
            {
                _cmbWorldFilter.Items.Clear();
                _cmbWorldFilter.Items.Add("All Worlds");
                foreach (var world in worlds)
                    _cmbWorldFilter.Items.Add(world);
                _cmbWorldFilter.SelectedIndex = 0;
                _cmbWorldFilter.Visible = true;
            }
            else
            {
                _cmbWorldFilter.Visible = false;
            }
        }
        private static (string World, string CleanLocation, string CleanRegion) ParseWorldInfo(string location, string region)
        {
            string world = "World 1";
            string cleanLoc = location;
            string cleanReg = region;

            if (location.StartsWith("World "))
            {
                var parts = location.Split(new[] { ' ' }, 3);
                if (parts.Length >= 2 && int.TryParse(parts[1], out int worldNum))
                {
                    world = $"World {worldNum}";
                    cleanLoc = parts.Length == 3 ? parts[2] : location;
                }
            }

            if (region.StartsWith("World "))
            {
                var parts = region.Split(new[] { ' ' }, 3);
                if (parts.Length >= 2 && int.TryParse(parts[1], out _))
                {
                    cleanReg = parts.Length == 3 ? parts[2] : region;
                }
            }

            return (world, cleanLoc, cleanReg);
        }

        private string? GetSettingValue(string key)
        {
            if (_spoilerLog == null) return null;

            string currentWorld = _cmbTrackerWorldFilter?.SelectedItem?.ToString() ?? "World 1";
            if (string.IsNullOrEmpty(currentWorld)) currentWorld = "World 1";

            string worldPrefix = $"{currentWorld} ";

            if (_spoilerLog.WorldFlags.TryGetValue(worldPrefix + key, out var worldVal))
                return worldVal;

            if (_spoilerLog.Settings.TryGetValue(key, out var settingVal))
                return settingVal;

            if (_spoilerLog.WorldFlags.TryGetValue(key, out var generalVal))
                return generalVal;

            return null;
        }
        private class NaturalStringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
            }
        }
        private void PopulateRegionFilter()
        {
            if (_spoilerLog == null) return;

            _cmbRegionFilter.Items.Clear();
            _cmbRegionFilter.Items.Add("All Regions");

            var locations = _spoilerLog.Locations.Values.AsEnumerable();

            if (_cmbGameFilter.SelectedIndex > 0)
            {
                var selectedGame = _cmbGameFilter.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedGame))
                {
                    var gameToMatch = selectedGame == "OoT" ? "OOT" : selectedGame;
                    locations = locations.Where(l => l.Game == gameToMatch);
                }
            }

            var regions = locations
                .Select(l =>
                {
                    if (l.Region.StartsWith("World "))
                    {
                        var parts = l.Region.Split(new[] { ' ' }, 3);
                        if (parts.Length >= 2 && int.TryParse(parts[1], out _))
                            return parts.Length == 3 ? parts[2] : l.Region;
                    }
                    return l.Region;
                })
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
        private void UpdateTrackerConfigForCurrentWorld()
        {
            if (_spoilerLog == null) return;

            string currentWorld = _cmbTrackerWorldFilter?.SelectedItem?.ToString() ?? "World 1";

            // Очищаем старые списки
            _currentTrackerConfig.MqDungeons.Clear();
            _currentTrackerConfig.SrPouchPacks.Clear();

            // Ищем WorldFlags для текущего мира
            string mqKey = $"{currentWorld} Master Quest Dungeons";
            if (_spoilerLog.WorldFlags.TryGetValue(mqKey, out var mqValue)
                && !string.IsNullOrEmpty(mqValue)
                && mqValue != "none"
                && mqValue != "all")
            {
                // Парсим список MQ-данжей
                var mqDungeons = mqValue.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var name in mqDungeons)
                {
                    string id = MapDungeonNameToId(name.Trim());
                    if (id != null) _currentTrackerConfig.MqDungeons.Add(id);
                }
            }
            else if (mqValue == "all")
            {
                // Все данжи - MQ
                foreach (var id in new[] { "deku_tree", "dodongo", "jabu", "forest_temple", "fire_temple",
                                  "water_temple", "shadow_temple", "spirit_temple", "botw", "ice_cavern", "gtg", "ganons_castle" })
                    _currentTrackerConfig.MqDungeons.Add(id);
            }

            // Silver Rupee Pouches (по аналогии)
            string srKey = $"{currentWorld} Silver Rupee Pouches";
            if (_spoilerLog.WorldFlags.TryGetValue(srKey, out var srValue)
                && !string.IsNullOrEmpty(srValue)
                && srValue != "none")
            {
                // Простая реализация - "all" = все пакеты
                if (srValue == "all")
                {
                    // Добавляем пакеты для немек MQ
                    foreach (var dId in new[] { "shadow_temple", "spirit_temple", "ice_cavern", "botw", "gtg", "ganons_castle" })
                    {
                        if (!_currentTrackerConfig.MqDungeons.Contains(dId))
                        {
                            foreach (var pId in GetVanillaPacks(dId))
                                _currentTrackerConfig.SrPouchPacks.Add($"{dId}_{pId}");
                        }
                    }
                }
                // Иначе - кастомный список
            }
        }

        private string? MapDungeonNameToId(string name)
        {
            return name.ToLower().Replace("'s", "").Replace("'", "").Trim() switch
            {
                "deku tree" => "deku_tree",
                "dodongo cavern" => "dodongo",
                "inside jabu-jabu" => "jabu",
                "forest temple" => "forest_temple",
                "fire temple" => "fire_temple",
                "water temple" => "water_temple",
                "shadow temple" => "shadow_temple",
                "spirit temple" => "spirit_temple",
                "bottom of the well" => "botw",
                "ice cavern" => "ice_cavern",
                "gerudo training ground" => "gtg",
                "gerudo training grounds" => "gtg",
                "ganon castle" => "ganons_castle",
                "ganons castle" => "ganons_castle",
                _ => null
            };
        }

        private string[] GetVanillaPacks(string dungeonId) => dungeonId switch
        {
            "shadow_temple" => new[] { "scythe", "pit", "spikes" },
            "spirit_temple" => new[] { "child", "sun", "boulders" },
            "ice_cavern" => new[] { "scythe", "block" },
            "botw" => new[] { "basement" },
            "gtg" => new[] { "slopes", "lava", "water" },
            "ganons_castle" => new[] { "spirit", "light", "fire", "forest" },
            _ => new string[0]
        };

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
            if (_spoilerLog == null) return;

            string currentWorld = _cmbTrackerWorldFilter?.SelectedItem?.ToString() ?? "World 1";
            string currentPlayer = currentWorld.Replace("World", "Player");

            string? worldFilter = (_cmbWorldFilter.Visible && _cmbWorldFilter.SelectedIndex > 0)
                ? _cmbWorldFilter.SelectedItem?.ToString()
                : null;

            foreach (var kv in _spoilerLog.StartingItems.OrderBy(k => k.Key))
            {
                string key = kv.Key;
                string value = kv.Value;
                string playerName = "";
                string itemName = key;
                string itemWorld = "";

                string playerPart = key;
                if (key.Contains(':'))
                {
                    var colonParts = key.Split(new[] { ':' }, 2);
                    playerPart = colonParts[0].Trim();
                    itemName = colonParts[1].Trim();
                }

                if (playerPart.StartsWith("Player ", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = playerPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 &&
                        string.Equals(parts[0], "Player", StringComparison.OrdinalIgnoreCase) &&
                        int.TryParse(parts[1], out int playerNum))
                    {
                        playerName = $"Player {playerNum}";
                        itemWorld = $"World {playerNum}";
                    }
                }

                if (worldFilter != null && itemWorld != worldFilter)
                    continue;

                if (string.IsNullOrEmpty(playerName)) playerName = "?";

                _dgvStartingItems.Rows.Add(playerName, itemName, value);
            }
        }




        private void UpdateWorldFlagsList()
        {
            _dgvWorldFlags.Rows.Clear();
            if (_spoilerLog == null) return;

            string? worldFilter = (_cmbWorldFilter.Visible && _cmbWorldFilter.SelectedIndex > 0)
                ? _cmbWorldFilter.SelectedItem?.ToString()
                : null;

            var flags = new List<(string World, string Key, string Value)>();

            foreach (var kvp in _spoilerLog.WorldFlags)
            {
                string world = "World 1";
                string flagName = kvp.Key;
                if (kvp.Key.StartsWith("World "))
                {
                    int firstSpace = kvp.Key.IndexOf(' ');
                    int secondSpace = kvp.Key.IndexOf(' ', firstSpace + 1);
                    if (secondSpace > 0)
                    {
                        world = kvp.Key.Substring(0, secondSpace);
                        flagName = kvp.Key.Substring(secondSpace + 1).Trim();
                    }
                }

                flags.Add((world, flagName, kvp.Value));
            }

            var filtered = flags
                .Where(f => worldFilter == null || f.World == worldFilter)
                .OrderBy(f => f.World)
                .ThenBy(f => f.Key);

            foreach (var flag in filtered)
            {
                string displayValue = string.IsNullOrWhiteSpace(flag.Value) ? "None" : flag.Value;
                _dgvWorldFlags.Rows.Add(flag.World, flag.Key, displayValue);
            }
        }
        private void UpdateSubRegionsList()
        {
            if (_cmbMapRegion.SelectedItem is not Models.MapRegion region) return;
            string selectedSubName = (_cmbMapSub.SelectedItem as Models.MapSubRegion)?.Name;

            _cmbMapSub.Items.Clear();
            _cmbMapSub.Enabled = false;

            string currentWorld = _cmbMapWorld?.SelectedItem?.ToString() ?? "World 1";

            foreach (var sub in region.SubRegions)
            {
                if (sub.RequiredSettingKey != null)
                {
                    string? val = GetSettingValue(sub.RequiredSettingKey);
                    if (val == null) continue;

                    bool matches = string.Equals(val, sub.RequiredSettingValue, StringComparison.OrdinalIgnoreCase);
                    if (!matches && sub.RequiredSettingContains != null)
                    {
                        var parts = val.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
                        matches = parts.Any(p => string.Equals(p.Trim(), sub.RequiredSettingContains, StringComparison.OrdinalIgnoreCase));
                    }
                    if (!matches) continue;
                }
                _cmbMapSub.Items.Add(sub);
            }

            if (_cmbMapSub.Items.Count > 0)
            {
                _cmbMapSub.Enabled = true;
                int restoredIndex = -1;
                for (int i = 0; i < _cmbMapSub.Items.Count; i++)
                {
                    if (_cmbMapSub.Items[i] is Models.MapSubRegion s && s.Name == selectedSubName)
                    {
                        restoredIndex = i;
                        break;
                    }
                }
                _cmbMapSub.SelectedIndex = restoredIndex != -1 ? restoredIndex : 0;
            }
            else
            {
                _cmbMapSub.SelectedIndex = -1;
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

        // Mapping from spoiler log event names to tracker location names
        private static readonly Dictionary<string, string> SongEventNameMapping = new()
        {
            // OoT events
            ["SONG_EVENT_TEMPLE_OF_TIME"] = "Temple of Time - Door of Time",
            ["SONG_EVENT_WINDMILL"] = "Kakariko - Windmill",
            ["SONG_EVENT_GRAVEYARD"] = "Royal Family's Tomb",
            ["SONG_EVENT_ZORA_RIVER"] = "Zora's River - Waterfall",
            ["SONG_EVENT_GORON_CITY"] = "Goron City - Darunia's Room",
            ["SONG_EVENT_GREAT_FAIRY_SPELL_WIND"] = "Zora's Fountain - Great Fairy",
            ["SONG_EVENT_GREAT_FAIRY_SPELL_FIRE"] = "Hyrule Castle - Great Fairy",
            ["SONG_EVENT_GREAT_FAIRY_SPELL_LOVE"] = "Desert Colossus - Great Fairy",
            ["SONG_EVENT_GREAT_FAIRY_UPGRADE_MAGIC"] = "Death Mountain Trail - Great Fairy",
            ["SONG_EVENT_GREAT_FAIRY_UPGRADE_MAGIC2"] = "Death Mountain Crater - Great Fairy",
            ["SONG_EVENT_GREAT_FAIRY_UPGRADE_DEFENSE"] = "Ganon's Castle - Great Fairy",
            ["SONG_EVENT_TEMPLE_WATER"] = "Water Temple - Water Levels",
            ["SONG_EVENT_TEMPLE_SHADOW"] = "Shadow Temple - Boat",
            ["SONG_EVENT_TEMPLE_SPIRIT_STATUE"] = "Spirit Temple - Statue",
            ["SONG_EVENT_TEMPLE_SPIRIT_LOWER"] = "Spirit Temple - Compass Chest",
            ["SONG_EVENT_TEMPLE_SPIRIT_HIGHER"] = "Spirit Temple - Boss Key Room",
            ["SONG_EVENT_TEMPLE_BOTW"] = "Bottom of the Well - Water Levels",
            ["SONG_EVENT_TEMPLE_GANON"] = "Ganon's Castle - Light Trial",
            
            // MM events
            ["SONG_EVENT_TEMPLE_WOODFALL"] = "Woodfall - Temple Entrance",
            ["SONG_EVENT_TEMPLE_SNOWHEAD"] = "Snowhead - Temple Entrance",
            ["SONG_EVENT_TEMPLE_GREATBAY"] = "Zora Cape - Temple Entrance",
            ["SONG_EVENT_HEALING_POEHUT"] = "Ikana Canyon - Ghost Hut",
            ["SONG_EVENT_HEALING_DARMANI"] = "Goron Graveyard - Heal Darmani",
            ["SONG_EVENT_HEALING_PAMELA_FATHER"] = "Ikana Canyon - Heal Pamela's Father",
            ["SONG_EVENT_HEALING_KAMARO"] = "Termina Field - Heal Kamaro",
            ["SONG_EVENT_HEALING_MIKAU"] = "Great Bay Coast - Heal Mikau",
            ["SONG_EVENT_AWAKENING_KEETA"] = "Ikana Graveyard - Captain Keeta",
            ["SONG_EVENT_AWAKENING_SCRUB"] = "Swamp Skulltula House - Deku Scrub",
            ["SONG_EVENT_LULLABY_KID"] = "Goron Shrine - Goron Baby",
            ["SONG_EVENT_STORMS_COMPOSER"] = "Ikana Canyon - Spring Water Cave",
            ["SONG_EVENT_CLOCK_TOWER_ROOF"] = "Clock Town Rooftop - Moon"
        };

        // Mapping from spoiler log song names to tracker song names
        private static readonly Dictionary<string, string> SongNameMapping = new(StringComparer.OrdinalIgnoreCase)
        {
            // OoT songs
            ["Zelda's Lullaby"] = "Zelda's Lullaby",
            ["Epona's Song"] = "Epona's Song",
            ["Saria's Song"] = "Saria's Song",
            ["Sun's Song"] = "Sun's Song",
            ["Song of Time"] = "Song of Time",
            ["Song of Storms"] = "Song of Storms",
            ["Minuet of Forest"] = "Minuet of Forest",
            ["Bolero of Fire"] = "Bolero of Fire",
            ["Serenade of Water"] = "Serenade of Water",
            ["Requiem of Spirit"] = "Requiem of Spirit",
            ["Nocturne of Shadow"] = "Nocturne of Shadow",
            ["Prelude of Light"] = "Prelude of Light",
            
            // MM songs
            ["Song of Healing"] = "Song of Healing",
            ["Song of Soaring"] = "Song of Soaring",
            ["Sonata of Awakening"] = "Sonata of Awakening",
            ["Goron Lullaby Intro"] = "Goron Lullaby (Intro)",
            ["Goron Lullaby"] = "Goron Lullaby",
            ["New Wave Bossa Nova"] = "New Wave Bossa Nova",
            ["Elegy of Emptiness"] = "Elegy of Emptiness",
            ["Oath to Order"] = "Oath to Order"
        };
        
        // All OoT song event locations with vanilla songs
        private static readonly Dictionary<string, string> OotSongEventDefaults = new()
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
            ["Ganon's Castle - Light Trial"]        = "Zelda's Lullaby"
        };
		private static readonly Dictionary<string, string> MmSongEventDefaults = new()
        {
            ["Clock Town Rooftop - Moon"]           = "Oath to Order",
			["Termina Field - Heal Kamaro"]         = "Song of Healing",
			["Woodfall - Temple Entrance"]          = "Sonata of Awakening",
			["Swamp Skulltula House - Deku Scrub"]  = "Sonata of Awakening",
			["Goron Shrine - Goron Baby"]           = "Goron Lullaby (Intro)",
			["Goron Graveyard - Heal Darmani"]      = "Song of Healing",
			["Snowhead - Temple Entrance"]          = "Goron Lullaby",
			["Great Bay Coast - Heal Mikau"]        = "Song of Healing",
			["Zora Cape - Temple Entrance"]         = "New Wave Bossa Nova",
			["Ikana Graveyard - Captain Keeta"]     = "Sonata of Awakening",
			["Ikana Canyon - Spring Water Cave"]    = "Song of Storms",
			["Ikana Canyon - Heal Pamela's Father"] = "Song of Healing",
			["Ikana Canyon - Ghost Hut"]            = "Song of Healing"
        };

        private void PopulateSongEvents()
        {
            if (_dgvSongEvents == null) return;
            _dgvSongEvents.Rows.Clear();

            bool ootShuffleEnabled = _currentTrackerConfig.HasOot && _currentTrackerConfig.SongEventsShuffleOot;
            bool mmShuffleEnabled = _currentTrackerConfig.HasMm && _currentTrackerConfig.SongEventsShuffleMm;

            if (_dgvSongEvents.Columns.Contains("Song"))
                _dgvSongEvents.Columns["Song"].ReadOnly = false;

            string? worldFilter = (_cmbWorldFilter.Visible && _cmbWorldFilter.SelectedIndex > 0)
                ? _cmbWorldFilter.SelectedItem?.ToString()
                : null;

            if (_spoilerLog == null || _songEvents.Count == 0)
            {
                AddSongEventBlock("", worldFilter, ootShuffleEnabled, mmShuffleEnabled);
                return;
            }

            var worlds = _songEvents.Keys
                .Where(k => k.StartsWith("World ", StringComparison.OrdinalIgnoreCase))
                .Select(k =>
                {
                    int firstSpace = k.IndexOf(' ');
                    int secondSpace = k.IndexOf(' ', firstSpace + 1);
                    return secondSpace > 0 ? k.Substring(0, secondSpace) : "World 1";
                })
                .Distinct()
                .OrderBy(w => w, new NaturalStringComparer())
                .ToList();

            if (worlds.Count == 0)
            {
                AddSongEventBlock("", worldFilter, ootShuffleEnabled, mmShuffleEnabled);
                return;
            }

            foreach (var world in worlds)
            {
                if (worldFilter != null && !string.Equals(worldFilter, world, StringComparison.OrdinalIgnoreCase))
                    continue;

                AddSongEventBlock(world, worldFilter, ootShuffleEnabled, mmShuffleEnabled);
            }
        }

        private void AddSongEventBlock(string world, string? worldFilter, bool ootShuffleEnabled, bool mmShuffleEnabled)
        {
            string prefix = string.IsNullOrEmpty(world) ? "" : $"{world} ";

            int headerIdx = _dgvSongEvents.Rows.Add(world, $"{world} - Ocarina of Time", "");
            if (headerIdx >= 0 && headerIdx < _dgvSongEvents.Rows.Count)
            {
                var headerRow = _dgvSongEvents.Rows[headerIdx];
                headerRow.ReadOnly = true;
                headerRow.DefaultCellStyle.BackColor = Color.FromArgb(60, 60, 80);
                headerRow.DefaultCellStyle.ForeColor = Color.White;
                headerRow.DefaultCellStyle.Font = new Font(_dgvSongEvents.Font, FontStyle.Bold);
                headerRow.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 60, 80);
                headerRow.DefaultCellStyle.SelectionForeColor = Color.White;
            }

            foreach (var kv in OotSongEventDefaults)
            {
                string fullKey = prefix + kv.Key;
                string song;
                if (ootShuffleEnabled)
                    song = _songEvents.TryGetValue(fullKey, out var s) ? s : "?";
                else
                    song = kv.Value;

                int rowIdx = _dgvSongEvents.Rows.Add(world, kv.Key, song);
                SetSongCellReadOnly(rowIdx, ootShuffleEnabled);
            }

            int mmHeaderIdx = _dgvSongEvents.Rows.Add(world, $"{world} - Majora's Mask", "");
            if (mmHeaderIdx >= 0 && mmHeaderIdx < _dgvSongEvents.Rows.Count)
            {
                var mmHeaderRow = _dgvSongEvents.Rows[mmHeaderIdx];
                mmHeaderRow.ReadOnly = true;
                mmHeaderRow.DefaultCellStyle.BackColor = Color.FromArgb(60, 60, 80);
                mmHeaderRow.DefaultCellStyle.ForeColor = Color.White;
                mmHeaderRow.DefaultCellStyle.Font = new Font(_dgvSongEvents.Font, FontStyle.Bold);
                mmHeaderRow.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 60, 80);
                mmHeaderRow.DefaultCellStyle.SelectionForeColor = Color.White;
            }

            foreach (var kv in MmSongEventDefaults)
            {
                string fullKey = prefix + kv.Key;
                string song;
                if (mmShuffleEnabled)
                    song = _songEvents.TryGetValue(fullKey, out var s) ? s : "?";
                else
                    song = kv.Value;

                int rowIdx = _dgvSongEvents.Rows.Add(world, kv.Key, song);
                SetSongCellReadOnly(rowIdx, mmShuffleEnabled);
            }
        }

        private void SetSongCellReadOnly(int rowIdx, bool enabled)
        {
            if (rowIdx < 0 || rowIdx >= _dgvSongEvents.Rows.Count) return;
            var row = _dgvSongEvents.Rows[rowIdx];
            int songColIndex = _dgvSongEvents.Columns.IndexOf(_dgvSongEvents.Columns["Song"]);
            if (songColIndex >= 0 && songColIndex < row.Cells.Count)
                row.Cells[songColIndex].ReadOnly = !enabled;
        }


        private void LoadSongEventsFromLog()
        {
            if (_spoilerLog == null) return;

            foreach (var kv in _spoilerLog.SongEvents)
            {
                string worldPrefix = "";
                string eventKey = kv.Key.Trim();

                if (eventKey.StartsWith("World ", StringComparison.OrdinalIgnoreCase))
                {
                    int firstSpace = eventKey.IndexOf(' ');
                    int secondSpace = eventKey.IndexOf(' ', firstSpace + 1);
                    if (secondSpace > 0)
                    {
                        worldPrefix = eventKey.Substring(0, secondSpace);
                        eventKey = eventKey.Substring(secondSpace + 1).Trim();
                    }
                    else
                    {
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(eventKey)) continue;

                if (SongEventNameMapping.TryGetValue(eventKey, out var locationName))
                {
                    string songName = kv.Value;
                    if (SongNameMapping.TryGetValue(songName, out var mappedSongName))
                        songName = mappedSongName;

                    string fullKey = string.IsNullOrEmpty(worldPrefix)
                        ? locationName
                        : $"{worldPrefix} {locationName}";
                    _songEvents[fullKey] = songName;
                }
            }
        }

        private void DgvSongEvents_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
            var row = _dgvSongEvents.Rows[e.RowIndex];
            var world = row.Cells[0].Value?.ToString() ?? "World 1";
            var loc = row.Cells[1].Value?.ToString() ?? "";
            var song = row.Cells[2].Value?.ToString() ?? "?";

            if (loc.Contains("Ocarina of Time") || loc.Contains("Majora's Mask") || string.IsNullOrWhiteSpace(loc))
                return;

            string fullKey = string.IsNullOrEmpty(world) ? loc : $"{world} {loc}";
            if (!string.IsNullOrEmpty(loc))
                _songEvents[fullKey] = song;
        }

        private void UpdateEntrancesList()
        {
            _dgvEntrances.Rows.Clear();
            if (_spoilerLog == null) return;

            string? worldFilter = (_cmbWorldFilter.Visible && _cmbWorldFilter.SelectedIndex > 0)
                ? _cmbWorldFilter.SelectedItem?.ToString()
                : null;

            var entrances = _spoilerLog.Entrances
                .Select(kvp =>
                {
                    if (kvp.Key.StartsWith("World "))
                    {
                        var parts = kvp.Key.Split(new[] { ' ' }, 3);
                        if (parts.Length >= 2)
                        {
                            string world = $"{parts[0]} {parts[1]}";
                            string entranceName = parts.Length == 3 ? parts[2] : kvp.Key;
                            return (World: world, From: entranceName, To: kvp.Value);
                        }
                    }
                    return (World: "World 1", From: kvp.Key, To: kvp.Value);
                })
                .Where(e => worldFilter == null || e.World == worldFilter)
                .OrderBy(e => e.World)
                .ThenBy(e => e.From);

            foreach (var entrance in entrances)
            {
                _dgvEntrances.Rows.Add(
                    entrance.World,
                    _cmbWorldFilter.Visible ? entrance.From : $"{entrance.From} ({entrance.World})",
                    entrance.To
                );
            }
        }
        private void UpdateTrackerWorldFilterVisibility()
        {
            if (_trackerWorldFilterPanel == null || _cmbTrackerWorldFilter == null) return;

            if (_spoilerLog == null)
            {
                _cmbTrackerWorldFilter.Visible = false;
                return;
            }

            var worlds = _spoilerLog.Locations.Values
                .Select(l => l.World)
                .Where(w => !string.IsNullOrEmpty(w))
                .Distinct()
                .ToList();

            if (worlds.Count <= 1)
            {
                _cmbTrackerWorldFilter.Visible = false;

                if (worlds.Count == 1 && _cmbTrackerWorldFilter.Items.Count == 0)
                {
                    _cmbTrackerWorldFilter.Items.Add(worlds[0]);
                    _cmbTrackerWorldFilter.SelectedIndex = 0;
                }
            }
            else
            {
                _cmbTrackerWorldFilter.Visible = true;
            }
        }
        private void BuildLocationCache()
        {
            _locationCache.Clear();

            foreach (var loc in _spoilerLog.Locations.Values)
            {
                string locWithoutPrefix = loc.Location;
                if (locWithoutPrefix.StartsWith("World "))
                {
                    var parts = locWithoutPrefix.Split(new[] { ' ' }, 3);
                    if (parts.Length >= 3)
                        locWithoutPrefix = parts[2];
                }
                _locationCache[locWithoutPrefix] = loc.Location;
            }
        }
        // Helper methods for counting found/total locations in regions
        private int GetRegionFoundCount(IEnumerable<string> locationNames, string game, string world)
        {
            int found = 0;
            foreach (var loc in locationNames)
            {
                if (_chkColorHighlight?.Checked == true && _coloredLocations.Contains($"{world}|{game}|{loc}"))
                    continue;
                if (_foundLocations.Contains($"{world}|{game}|{loc}"))
                    found++;
            }
            return found;
        }

        private int GetRegionTotalCount(IEnumerable<string> locationNames, string game, string world)
        {
            int total = 0;
            foreach (var loc in locationNames)
            {
                if (_chkColorHighlight?.Checked == true && _coloredLocations.Contains($"{world}|{game}|{loc}"))
                    continue;
                if (_knownLocations.Contains($"{world}|{game}|{loc}"))
                    total++;
            }
            return total;
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
