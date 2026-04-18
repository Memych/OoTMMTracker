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
        private Label _lblZoomMain = null!;
        private Panel _trackerScrollPanel = null!;
        private Panel _leftPanel = null!;
        private TrackerConfig _currentTrackerConfig = new();
        private BroadcastForm? _broadcastForm;
        private System.Windows.Forms.Timer? _broadcastTimer;
        // Found locations: key = "Game|Location"
        private readonly HashSet<string> _foundLocations = new HashSet<string>();
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
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            
            // Header — one row with buttons
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(45, 45, 45) };
            
            // Row 1: buttons
            _btnLoadFile = new Button { Text = "Load Spoiler Log", Location = new Point(8, 8), Size = new Size(180, 28),
                BackColor = Color.FromArgb(60, 80, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnLoadFile.Click += BtnLoadFile_Click;
            topPanel.Controls.Add(_btnLoadFile);
            
            var btnTrackerOptions = new Button
            {
                Text = "Tracker Options",
                Location = new Point(196, 8),
                Size = new Size(140, 28),
                BackColor = Color.FromArgb(50, 60, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTrackerOptions.Click += BtnTrackerOptions_Click;
            topPanel.Controls.Add(btnTrackerOptions);
            
            var btnResetTracker = new Button
            {
                Text = "Reset Tracker",
                Location = new Point(344, 8),
                Size = new Size(120, 28),
                BackColor = Color.FromArgb(80, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnResetTracker.Click += BtnResetTracker_Click;
            topPanel.Controls.Add(btnResetTracker);

            var btnResetProgress = new Button
            {
                Text = "Reset Progress",
                Location = new Point(472, 8),
                Size = new Size(130, 28),
                BackColor = Color.FromArgb(80, 55, 20),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnResetProgress.Click += BtnResetProgress_Click;
            topPanel.Controls.Add(btnResetProgress);

            var btnSave = new Button
            {
                Text = "Save",
                Location = new Point(610, 8),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(40, 80, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;
            topPanel.Controls.Add(btnSave);

            var btnLoad = new Button
            {
                Text = "Load",
                Location = new Point(708, 8),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(40, 60, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLoad.Click += BtnLoad_Click;
            topPanel.Controls.Add(btnLoad);

            var btnBroadcast = new Button
            {
                Text = "Broadcast",
                Location = new Point(806, 8),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(60, 40, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBroadcast.Click += (s, e) =>
            {
                if (_broadcastForm == null || _broadcastForm.IsDisposed)
                {
                    _broadcastForm = new BroadcastForm();
                    _lastBroadcastConfig = null; // force rebuild
                    _broadcastForm.Show(this);
                    UpdateBroadcast();
                }
                else
                {
                    _broadcastForm.Focus();
                }
            };
            topPanel.Controls.Add(btnBroadcast);

            var btnZoomOut = new Button
            {
                Text = "−",
                Location = new Point(904, 8),
                Size = new Size(28, 28),
                BackColor = Color.FromArgb(50, 50, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            btnZoomOut.Click += (s, e) =>
            {
                var steps = ItemTrackerPanel.GetItemSizeSteps();
                int cur = Array.IndexOf(steps, ItemTrackerPanel.GetItemSize());
                if (cur > 0)
                {
                    ItemTrackerPanel.SetItemSize(steps[cur - 1]);
                    _lblZoomMain.Text = $"{ItemTrackerPanel.GetItemSize()}px";
                    RebuildTracker();
                }
            };
            topPanel.Controls.Add(btnZoomOut);

            _lblZoomMain = new Label
            {
                Location = new Point(934, 11),
                Size = new Size(44, 18),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8f),
                Text = $"{ItemTrackerPanel.GetItemSize()}px"
            };
            topPanel.Controls.Add(_lblZoomMain);

            var btnZoomIn = new Button
            {
                Text = "+",
                Location = new Point(980, 8),
                Size = new Size(28, 28),
                BackColor = Color.FromArgb(50, 50, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            btnZoomIn.Click += (s, e) =>
            {
                var steps = ItemTrackerPanel.GetItemSizeSteps();
                int cur = Array.IndexOf(steps, ItemTrackerPanel.GetItemSize());
                if (cur < steps.Length - 1)
                {
                    ItemTrackerPanel.SetItemSize(steps[cur + 1]);
                    _lblZoomMain.Text = $"{ItemTrackerPanel.GetItemSize()}px";
                    RebuildTracker();
                }
            };
            topPanel.Controls.Add(btnZoomIn);

            var btnZoomReset = new Button
            {
                Text = "↺",
                Location = new Point(1010, 8),
                Size = new Size(28, 28),
                BackColor = Color.FromArgb(50, 50, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 11, FontStyle.Bold)
            };
            btnZoomReset.Click += (s, e) =>
            {
                ItemTrackerPanel.SetItemSize(48);
                _lblZoomMain.Text = "48px";
                RebuildTracker();
            };
            topPanel.Controls.Add(btnZoomReset);
            
            _lblInfo = new Label { Location = new Point(808, 12), Size = new Size(700, 20), Text = "No file loaded", ForeColor = Color.White };
            topPanel.Controls.Add(_lblInfo);
            
            // Row 2: search and region — initialized below in searchPanel            
            this.Controls.Add(topPanel);

            // Search bar — initialized later inside rightContainer
            
            // Right panel (Fill) — add first
            var rightContainer = new Panel { Dock = DockStyle.Fill, BackColor = SystemColors.Control };
            this.Controls.Add(rightContainer);

            // Left panel (Left) — add second
            // Width fixed for default 48px icon size: PadX*2 + 10cols*48 + 9gaps*2 + scrollbar + margin = ~540
            _leftPanel = new Panel { Dock = DockStyle.Left, Width = 540, BackColor = Color.FromArgb(30, 30, 30) };
            this.Controls.Add(_leftPanel);

            // Tracker inside left panel
            _trackerScrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(30, 30, 30) };
            _leftPanel.Controls.Add(_trackerScrollPanel);
            RebuildTracker();
            // Right panel: search + tabs
            var rightPanel = rightContainer;
            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(45, 45, 45) };
            // searchPanel added to rightContainer later (after tabControl)

            // Fill searchPanel
            searchPanel.Controls.Add(new Label { Text = "Search:", Location = new Point(6, 10), Size = new Size(50, 20), ForeColor = Color.White });
            _txtSearch = new TextBox { Location = new Point(58, 7), Size = new Size(220, 22) };
            _txtSearch.TextChanged += TxtSearch_TextChanged;
            searchPanel.Controls.Add(_txtSearch);
            
            searchPanel.Controls.Add(new Label { Text = "Game:", Location = new Point(286, 10), Size = new Size(45, 20), ForeColor = Color.White });
            _cmbGameFilter = new ComboBox { Location = new Point(334, 7), Size = new Size(100, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbGameFilter.Items.AddRange(new object[] { "All", "OoT", "MM" });
            _cmbGameFilter.SelectedIndex = 0;
            _cmbGameFilter.SelectedIndexChanged += (s, e) => 
            {
                PopulateRegionFilter(); // Update region filter based on selected game
                UpdateLocationsList(_txtSearch.Text);
            };
            _cmbGameFilter.Enabled = false; // Disabled until log is loaded with both games
            searchPanel.Controls.Add(_cmbGameFilter);
            
            searchPanel.Controls.Add(new Label { Text = "Region:", Location = new Point(442, 10), Size = new Size(55, 20), ForeColor = Color.White });
            _cmbRegionFilter = new ComboBox { Location = new Point(500, 7), Size = new Size(220, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbRegionFilter.Items.Add("All Regions");
            _cmbRegionFilter.SelectedIndex = 0;
            _cmbRegionFilter.SelectedIndexChanged += CmbRegionFilter_SelectedIndexChanged;
            searchPanel.Controls.Add(_cmbRegionFilter);
            
            searchPanel.Controls.Add(new Label { Text = "Status:", Location = new Point(728, 10), Size = new Size(50, 20), ForeColor = Color.White });
            _cmbFoundFilter = new ComboBox { Location = new Point(780, 7), Size = new Size(120, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbFoundFilter.Items.AddRange(new object[] { "All", "Not Found", "Found" });
            _cmbFoundFilter.SelectedIndex = 0;
            _cmbFoundFilter.SelectedIndexChanged += (s, e) => UpdateLocationsList(_txtSearch.Text);
            searchPanel.Controls.Add(_cmbFoundFilter);

            _chkHideItems = new CheckBox { Text = "Hide Items", Location = new Point(910, 9), Size = new Size(90, 18), ForeColor = Color.White };
            _chkHideItems.CheckedChanged += (s, e) =>
            {
                _dgvLocations.Columns["Item"]!.Visible = !_chkHideItems.Checked;
                UpdateLocationsList(_txtSearch.Text);
            };
            searchPanel.Controls.Add(_chkHideItems);

            _chkColorHighlight = new CheckBox { Text = "Colors", Location = new Point(1008, 9), Size = new Size(70, 18), ForeColor = Color.White, Checked = false };
            _chkColorHighlight.CheckedChanged += (s, e) => UpdateLocationsList(_txtSearch.Text);
            searchPanel.Controls.Add(_chkColorHighlight);

            _lblCounter = new Label { Location = new Point(1086, 10), Size = new Size(160, 18), ForeColor = Color.FromArgb(180, 180, 180), TextAlign = ContentAlignment.MiddleLeft };
            searchPanel.Controls.Add(_lblCounter);

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
                ReadOnly = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                DefaultCellStyle = { SelectionBackColor = Color.FromArgb(60, 80, 100), SelectionForeColor = Color.White }
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
            
            // Add tabControl to rightContainer (Fill — first!)
            rightContainer.Controls.Add(_tabControl);
            // searchPanel (Top)
            rightContainer.Controls.Add(searchPanel);
            // Placeholder — added last, will be topmost
            rightContainer.Controls.Add(new Label { Dock = DockStyle.Top, Height = 44, Text = "", BackColor = Color.FromArgb(45, 45, 45) });            
            // Drag & Drop
            this.AllowDrop = true;
            this.DragEnter += MainForm_DragEnter;
            this.DragDrop += MainForm_DragDrop;
            // Hotkeys
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
            // Update broadcast when any item changes
            ItemTrackerPanel.ItemChanged += () => UpdateBroadcast();
        }
        
        private static DataGridView MakeGrid() => new()
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
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
            switch (e.KeyCode)
            {
                case Keys.F1: BtnLoadFile_Click(this, EventArgs.Empty); break;
                case Keys.F2: BtnSave_Click(this, EventArgs.Empty); break;
                case Keys.F3: BtnTrackerOptions_Click(this, EventArgs.Empty); break;
                case Keys.F4: BtnLoad_Click(this, EventArgs.Empty); break;
                case Keys.F5: BtnResetProgress_Click(this, EventArgs.Empty); break;
                case Keys.F6: BtnResetTracker_Click(this, EventArgs.Empty); break;
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
                RebuildTracker(resetProgress: true);
                // Apply bottles from log locations
                ApplyBottlesFromLog();
                // Apply starting items
                ApplyStartingItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void UpdateLocationsList(string? searchText = null)
        {
            _dgvLocations.Rows.Clear();
            
            if (_spoilerLog == null)
                return;
            
            var locations = _spoilerLog.Locations.Values.AsEnumerable();
            
            // Filter by search
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();
                locations = locations.Where(l => 
                    l.Location.ToLower().Contains(searchText) || 
                    l.Item.ToLower().Contains(searchText));
            }
            
            // Filter by game
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
            
            // Filter by regions
            if (_cmbRegionFilter.SelectedIndex > 0)
            {
                var selectedRegion = _cmbRegionFilter.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedRegion))
                {
                    locations = locations.Where(l => l.Region == selectedRegion);
                }
            }
            
            foreach (var location in locations.OrderBy(l => l.Region).ThenBy(l => l.Location))
            {
                var key = $"{location.Game}|{location.Location}";
                bool found = _foundLocations.Contains(key);
                // Filter by status
                int statusFilter = _cmbFoundFilter?.SelectedIndex ?? 0;
                if (statusFilter == 1 && found)  continue; // only not found
                if (statusFilter == 2 && !found) continue; // only found
                _dgvLocations.Rows.Add(found, location.Game, location.Region, location.Location, location.Item);
            }

            // Update counter
            int total   = _dgvLocations.Rows.Count;
            int checked_ = _dgvLocations.Rows.Cast<DataGridViewRow>().Count(r => r.Cells[0].Value is true);
            if (_lblCounter != null)
                _lblCounter.Text = $"{checked_}/{total} checked";
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

            // Nothing
            if (lower == "nothing") return true;

            return false;
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
}
