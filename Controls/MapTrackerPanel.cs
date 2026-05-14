using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OoTMMTracker.Models;
using OoTMMTracker.Services;

namespace OoTMMTracker.Controls
{
    /// <summary>
    /// Displays a sub-region map with zoom and pan support.
    /// At 100% zoom the map is uniformly scaled to fit the panel (letterboxed if needed).
    /// </summary>
    public class MapTrackerPanel : Panel
    {
        private MapSubRegion? _subRegion;
        private HashSet<string> _foundLocations = new();
        private HashSet<string> _knownLocations = new();
        private string _ageFilter = "child";
        private string _game = "OOT";
        private bool _colorsMode = false;
        private HashSet<string> _coloredLocations = new();
        private Models.SpoilerLog? _spoilerLog;
        private readonly ToolTip _toolTip = new();

        private Image? _bgImage;

        // Preloaded mark images
        private readonly List<(MapMark mark, Image img)> _markImages = new();

        private static readonly Dictionary<string, Image?> _imageCache = new();

        // ── Zoom and Pan state ────────────────────────────────────────────────
        private float _zoomLevel = 1.0f;
        private PointF _panOffset = PointF.Empty;
        /// <summary>After true, pan is clamped to valid scroll range; false = map stays letterboxed/centered on (re)load and reset zoom.</summary>
        private bool _panUserAdjusted = false;
        private Point _dragStart = Point.Empty;
        private bool _isDragging = false;

        // User zoom multiplier limits (0.1× … 10× relative to fit-to-panel)
        private const float MIN_ZOOM = 0.1f;
        private const float MAX_ZOOM = 10.0f;
        private const float ZOOM_FACTOR = 1.15f;

        /// <summary>Scale that fits the full map inside the client area (uniform, aspect preserved).</summary>
        private float GetFitScale()
        {
            if (_bgImage == null) return 1f;
            int pw = Math.Max(1, ClientSize.Width);
            int ph = Math.Max(1, ClientSize.Height);
            float sx = (float)pw / _bgImage.Width;
            float sy = (float)ph / _bgImage.Height;
            return Math.Min(sx, sy);
        }

        /// <summary>Pixels per source image pixel on screen (fit × user zoom).</summary>
        private float GetEffectiveScale() => GetFitScale() * _zoomLevel;

        // ── Selection state ───────────────────────────────────────────────────
        // List of marks under last clicked position, index into it
        private List<(MapMark mark, Image img)> _clickCandidates = new();
        private int _selectedCandidateIndex = -1;
        private MapMark? _selectedMark = null;
        private Point _lastClickPoint = Point.Empty;

        /// Fired when user right-clicks a selected mark — passes location names to mark as found.
        public event Action<IReadOnlyList<string>, string>? MarkCompleted; // (locationNames, game)

        /// <summary>Fired when user follows an entrance-shuffle mark to another map (region name, sub-map name, game OOT|MM).</summary>
        public event Action<string, string, string>? NavigateMapTo;

        public MapTrackerPanel()
        {
            this.BackColor    = Color.FromArgb(20, 20, 20);
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.MouseMove   += OnMouseMoveHandler;
            this.MouseDown   += OnMouseDownHandler;
            this.MouseUp     += OnMouseUpHandler;
            this.MouseWheel  += OnMouseWheelHandler;
        }

        // ── Public Zoom API ───────────────────────────────────────────────────

        public void ZoomIn() => ZoomAtCenter(ZOOM_FACTOR);
        public void ZoomOut() => ZoomAtCenter(1f / ZOOM_FACTOR);
        public void ResetZoomLevel()
        {
            _zoomLevel = 1.0f;
            _panUserAdjusted = false;
            _panOffset = PointF.Empty;
            ConstrainPan();
            this.Invalidate();
        }

        /// <summary>User zoom multiplier (1 = fit map in panel).</summary>
        public float GetZoomLevel() => _zoomLevel;

        public void CenterMap()
        {
            if (_bgImage == null) return;
            _panUserAdjusted = false;
            _panOffset = PointF.Empty;
            ConstrainPan();
            this.Invalidate();
        }

        private void ZoomAtCenter(float factor)
        {
            if (_bgImage == null) return;

            float eff = GetEffectiveScale();
            float centerX = _panOffset.X + _bgImage.Width * eff / 2f;
            float centerY = _panOffset.Y + _bgImage.Height * eff / 2f;

            ZoomAtPoint(new PointF(centerX, centerY), factor);
        }

        private void ZoomAtPoint(PointF zoomCenter, float factor)
        {
            if (_bgImage == null) return;

            float fit = GetFitScale();
            float oldEff = fit * _zoomLevel;

            float newZoom = _zoomLevel * factor;
            newZoom = Math.Max(MIN_ZOOM, Math.Min(MAX_ZOOM, newZoom));

            if (Math.Abs(newZoom - _zoomLevel) < 0.001f) return;

            float newEff = fit * newZoom;

            // Source-image coords of point under zoom center (before zoom)
            float relX = (zoomCenter.X - _panOffset.X) / oldEff;
            float relY = (zoomCenter.Y - _panOffset.Y) / oldEff;

            _zoomLevel = newZoom;

            _panOffset.X = zoomCenter.X - relX * newEff;
            _panOffset.Y = zoomCenter.Y - relY * newEff;

            _panUserAdjusted = true;
            ConstrainPan();

            this.Invalidate();
        }

        private void ConstrainPan()
        {
            if (_bgImage == null) return;

            float eff = GetEffectiveScale();
            float scaledWidth = _bgImage.Width * eff;
            float scaledHeight = _bgImage.Height * eff;

            float availWidth = this.ClientSize.Width;
            float availHeight = this.ClientSize.Height;

            float maxPanX = Math.Max(0, scaledWidth - availWidth);
            float maxPanY = Math.Max(0, scaledHeight - availHeight);

            if (!_panUserAdjusted)
            {
                _panOffset.X = (availWidth - scaledWidth) / 2f;
                _panOffset.Y = (availHeight - scaledHeight) / 2f;
                return;
            }

            if (scaledWidth < availWidth)
                _panOffset.X = (availWidth - scaledWidth) / 2f;
            else
                _panOffset.X = Math.Max(-maxPanX, Math.Min(0f, _panOffset.X));

            if (scaledHeight < availHeight)
                _panOffset.Y = (availHeight - scaledHeight) / 2f;
            else
                _panOffset.Y = Math.Max(-maxPanY, Math.Min(0f, _panOffset.Y));
        }

        // ── Mouse Wheel Handler ───────────────────────────────────────────────

        private void OnMouseWheelHandler(object? sender, MouseEventArgs e)
        {
            if (_bgImage == null) return;

            float zoomFactor = e.Delta > 0 ? ZOOM_FACTOR : 1f / ZOOM_FACTOR;
            ZoomAtPoint(new PointF(e.X, e.Y), zoomFactor);
        }

        // ── Mouse Up Handler ──────────────────────────────────────────────────

        private void OnMouseUpHandler(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
                this.Cursor = Cursors.Default;
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void SetSubRegion(MapSubRegion? subRegion, HashSet<string> foundLocations, string game = "OOT")
        {
            _subRegion      = subRegion;
            _foundLocations = foundLocations;
            _game           = game;
            _bgImage        = subRegion != null ? LoadImage(subRegion.BackgroundImage) : null;
            // Clear selection on sub-map change
            _selectedMark = null;
            _clickCandidates.Clear();
            _selectedCandidateIndex = -1;
            _lastClickPoint = Point.Empty;
            Rebuild();
        }

        public void UpdateFoundLocations(HashSet<string> foundLocations)
        {
            _foundLocations = foundLocations;
            this.Invalidate();
        }

        public void SetKnownLocations(HashSet<string> knownLocations)
        {
            _knownLocations = knownLocations;
            this.Invalidate();
        }

        public void SetAgeFilter(string age)
        {
            _ageFilter = age;
            this.Invalidate();
        }

        public void SetColorsMode(bool enabled)
        {
            _colorsMode = enabled;
            this.Invalidate();
        }

        public void SetColoredLocations(HashSet<string> coloredLocations)
        {
            _coloredLocations = coloredLocations;
            this.Invalidate();
        }

        public void SetSpoilerLog(Models.SpoilerLog? spoilerLog)
        {
            _spoilerLog = spoilerLog;
        }

        // ── Build ─────────────────────────────────────────────────────────────

        private void Rebuild()
        {
            _markImages.Clear();
            _toolTip.RemoveAll();
            this.Controls.Clear();

            if (_subRegion == null || _bgImage == null)
            {
                if (_subRegion != null && _bgImage == null)
                {
                    var lbl = new Label
                    {
                        Text      = $"Image not found:\n{_subRegion.BackgroundImage}",
                        ForeColor = Color.Gray,
                        AutoSize  = true,
                        Location  = new Point(8, 8)
                    };
                    this.Controls.Add(lbl);
                }
                this.Invalidate();
                return;
            }

            foreach (var mark in _subRegion.Marks
                .OrderByDescending(m => m.Size)
                .ThenBy(m => m.Tooltip))
            {
                var img = LoadImage(mark.IconPath);
                if (img != null)
                    _markImages.Add((mark, img));
            }

            _panUserAdjusted = false;
            ConstrainPan();
            this.Invalidate();
        }

        // ── Layout ────────────────────────────────────────────────────────────

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ConstrainPan();
            this.Invalidate();
        }

        private Rectangle ComputeMapRect(float eff)
        {
            if (_bgImage == null) return Rectangle.Empty;

            float scaledWidth = _bgImage.Width * eff;
            float scaledHeight = _bgImage.Height * eff;
            return new Rectangle((int)_panOffset.X, (int)_panOffset.Y, (int)scaledWidth, (int)scaledHeight);
        }

        private static Rectangle MarkDestRect(MapMark mark, in Rectangle mapRect, float scale)
        {
            int size = Math.Max(8, (int)(mark.Size * scale));
            return new Rectangle(
                mapRect.X + (int)(mark.X * scale),
                mapRect.Y + (int)(mark.Y * scale),
                size,
                size);
        }

        // ── Paint ─────────────────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_bgImage == null) return;

            float scale = GetEffectiveScale();
            var mapRect = ComputeMapRect(scale);
            var g = e.Graphics;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(_bgImage, mapRect);

            foreach (var (mark, img) in _markImages)
            {
                if (!IsMarkVisible(mark)) continue;

                var destRect = MarkDestRect(mark, in mapRect, scale);
                g.DrawImage(img, destRect);

                if (_selectedMark == mark)
                {
                    // Redraw icon with yellow tint — only affects non-transparent pixels
                    var cm = new System.Drawing.Imaging.ColorMatrix(new float[][]{
                        new float[] { 1.0f,  0.0f,  0.0f, 0, 0 },
                        new float[] { 0.0f,  1.0f,  0.0f, 0, 0 },
                        new float[] { 0.0f,  0.0f,  0.0f, 0, 0 },
                        new float[] { 0,     0,     0,    1, 0 },
                        new float[] { 0.4f,  0.4f,  0.0f, 0, 1 },
                    });
                    using var ia = new System.Drawing.Imaging.ImageAttributes();
                    ia.SetColorMatrix(cm, System.Drawing.Imaging.ColorMatrixFlag.Default,
                                         System.Drawing.Imaging.ColorAdjustType.Bitmap);
                    g.DrawImage(img, destRect, 0, 0, img.Width, img.Height,
                                GraphicsUnit.Pixel, ia);
                }
            }
        }

        // ── Mouse interaction ─────────────────────────────────────────────────

        private void OnMouseDownHandler(object? sender, MouseEventArgs e)
        {
            if (_bgImage == null) return;

            float scale = GetEffectiveScale();
            var mapRect = ComputeMapRect(scale);
            var candidates = new List<(MapMark mark, Image img)>();

            foreach (var (mark, img) in _markImages)
            {
                if (!IsMarkVisible(mark)) continue;
                if (MarkDestRect(mark, in mapRect, scale).Contains(e.Location))
                    candidates.Add((mark, img));
            }

            if (e.Button == MouseButtons.Left)
            {
                if (candidates.Count == 0)
                {
                    // Click away from marks — clear selection, then pan from this point
                    if (_selectedMark != null || _clickCandidates.Count > 0 || _selectedCandidateIndex >= 0)
                    {
                        _selectedMark = null;
                        _clickCandidates.Clear();
                        _selectedCandidateIndex = -1;
                        _lastClickPoint = Point.Empty;
                        this.Invalidate();
                    }
                    _isDragging = true;
                    _dragStart = e.Location;
                    this.Cursor = Cursors.Hand;
                }
                else
                {
                    // Mark clicked — handle mark selection
                    const int samePosTolerance = 4;
                    bool samePosition = _clickCandidates.Count > 0 &&
                                       Math.Abs(e.X - _lastClickPoint.X) <= samePosTolerance &&
                                       Math.Abs(e.Y - _lastClickPoint.Y) <= samePosTolerance;

                    if (samePosition)
                    {
                        _clickCandidates = _clickCandidates
                            .Where(c => _markImages.Any(m => m.mark == c.mark) && IsMarkVisible(c.mark))
                            .ToList();

                        if (_clickCandidates.Count == 0)
                        {
                            _selectedMark = null;
                            _selectedCandidateIndex = -1;
                        }
                        else
                        {
                            _selectedCandidateIndex = (_selectedCandidateIndex + 1) % _clickCandidates.Count;
                            _selectedMark = _clickCandidates[_selectedCandidateIndex].mark;
                        }
                    }
                    else
                    {
                        _clickCandidates = candidates;
                        _selectedCandidateIndex = 0;
                        _selectedMark = _clickCandidates[0].mark;
                        _lastClickPoint = e.Location;
                    }
                    this.Invalidate();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                MapMark? entranceMark = candidates.Select(c => c.mark).FirstOrDefault(m => m.IsEntranceShuffleMark);
                if (entranceMark == null && _selectedMark?.IsEntranceShuffleMark == true)
                    entranceMark = _selectedMark;

                if (entranceMark != null)
                {
                    if (TryNavigateEntrance(entranceMark, out string? err))
                    {
                        _selectedMark = null;
                        _clickCandidates.Clear();
                        _selectedCandidateIndex = -1;
                        this.Invalidate();
                    }
                    else if (!string.IsNullOrEmpty(err))
                        MessageBox.Show(this, err, "Entrance", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (_selectedMark != null && _selectedMark.LocationNames.Count > 0)
                {
                    MarkCompleted?.Invoke(_selectedMark.LocationNames, _game);
                    _selectedMark = null;
                    _clickCandidates.Clear();
                    _selectedCandidateIndex = -1;
                    this.Invalidate();
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                if (_selectedMark != null && _selectedMark.LocationNames.Count > 0)
                {
                    MarkCompleted?.Invoke(_selectedMark.LocationNames, _game);
                    _selectedMark = null;
                    _clickCandidates.Clear();
                    _selectedCandidateIndex = -1;
                    this.Invalidate();
                }
            }
        }

        // ── Mouse Move Handler (Pan + Tooltip) ───────────────────────────────

        private string _lastTooltip = "";

        private void OnMouseMoveHandler(object? sender, MouseEventArgs e)
        {
            if (_bgImage == null) return;

            // Handle panning
            if (_isDragging)
            {
                int dx = e.X - _dragStart.X;
                int dy = e.Y - _dragStart.Y;
                
                _panOffset.X += dx;
                _panOffset.Y += dy;
                _panUserAdjusted = true;
                ConstrainPan();
                
                _dragStart = e.Location;
                this.Invalidate();
                return;
            }

            float scale = GetEffectiveScale();
            var mapRect = ComputeMapRect(scale);
            string tip = "";
            foreach (var (mark, _) in _markImages)
            {
                if (!IsMarkVisible(mark)) continue;
                if (MarkDestRect(mark, in mapRect, scale).Contains(e.Location))
                {
                    tip = mark.Tooltip;
                    if (mark.IsEntranceShuffleMark && _spoilerLog != null &&
                        EntranceMapNavigation.TryGetDestinationSubMapDisplayName(
                            _spoilerLog.Entrances, mark.EntranceFromId, out var toSubMap))
                    {
                        tip = $"To {toSubMap}";
                    }
                    break;
                }
            }

            if (tip != _lastTooltip)
            {
                _lastTooltip = tip;
                _toolTip.SetToolTip(this, tip);
            }

            bool overMark = tip.Length > 0;
            int cw = this.ClientSize.Width;
            int ch = this.ClientSize.Height;
            bool canPan = mapRect.Width > cw || mapRect.Height > ch;
            this.Cursor = (overMark || canPan) ? Cursors.Hand : Cursors.Default;
        }

        // ── Visibility helpers ────────────────────────────────────────────────

        private bool TryNavigateEntrance(MapMark mark, out string? error)
        {
            error = null;
            if (_spoilerLog == null)
            {
                error = "Load a spoiler log with an Entrances section.";
                return false;
            }
            if (!EntranceMapNavigation.TryGetDestinationForFromId(_spoilerLog.Entrances, mark.EntranceFromId!, out var destLine))
            {
                error = $"This entrance ({mark.EntranceFromId}) is not listed in the loaded Entrances.";
                return false;
            }
            if (!EntranceMapNavigation.TryGetMapForDestinationLine(destLine, out var region, out var sub, out var game))
            {
                var id = EntranceMapNavigation.TryExtractEntranceId(destLine);
                error = "No sub-map lists this destination id in MapRegionsData.\n" +
                        "On the target map's sub-region, set DestinationEntranceIds to include the id inside " +
                        "the last (... ) or {...} id on the To side of the Entrances row.\n" +
                        $"Missing id: {id ?? "(none)"}\nTo: {destLine}";
                return false;
            }

            NavigateMapTo?.Invoke(region, sub, game);
            return true;
        }

        private bool IsMarkVisible(MapMark mark)
        {
            bool ageMatch = mark.AgeFilter == "both" || mark.AgeFilter == _ageFilter;
            if (!ageMatch) return false;

            if (mark.IsEntranceShuffleMark)
            {
                if (_spoilerLog == null || _spoilerLog.Entrances.Count == 0)
                    return false;
                if (!EntranceMapNavigation.TryGetDestinationForFromId(_spoilerLog.Entrances, mark.EntranceFromId!, out _))
                    return false;
            }

            if (!IsMarkKnown(mark) || IsMarkComplete(mark)) return false;
            // Check required setting condition
            if (mark.RequiredSettingKey != null)
            {
                string? val = null;
                if (_spoilerLog != null)
                {
                    if (!_spoilerLog.Settings.TryGetValue(mark.RequiredSettingKey, out val))
                        _spoilerLog.WorldFlags.TryGetValue(mark.RequiredSettingKey, out val);
                }
                bool matches;
                if (val == null)
                    matches = false;
                else
                {
                    matches = string.Equals(val, mark.RequiredSettingValue, StringComparison.OrdinalIgnoreCase);
                    if (!matches && mark.RequiredSettingContains != null)
                        matches = val.Split('|').Any(p => string.Equals(p.Trim(), mark.RequiredSettingContains, StringComparison.OrdinalIgnoreCase));
                }
                if (mark.RequiredSettingInvert) matches = !matches;
                if (!matches) return false;
            }
            // If colors mode: hide mark if ALL its locations are consumable/trap
            if (_colorsMode && mark.LocationNames.Count > 0 &&
                mark.LocationNames.All(loc =>
                    _coloredLocations.Contains($"{_game}|{loc}")))
                return false;
            return true;
        }

        private bool IsMarkKnown(MapMark mark)
        {
            if (mark.LocationNames.Count == 0) return true;
            return mark.LocationNames.Any(loc =>
                _knownLocations.Contains($"{_game}|{loc}"));
        }

        private bool IsMarkComplete(MapMark mark)
        {
            if (mark.LocationNames.Count == 0) return false;
            return mark.LocationNames.All(loc =>
                _foundLocations.Contains($"{_game}|{loc}"));
        }

        // ── Image loading ─────────────────────────────────────────────────────

        private static Image? LoadImage(string? path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (_imageCache.TryGetValue(path, out var cached)) return cached;

            Image? img = null;
            try
            {
                var resKey = path.Replace('/', '\\');
                var asm    = System.Reflection.Assembly.GetExecutingAssembly();
                var names  = asm.GetManifestResourceNames();
                var match  = names.FirstOrDefault(n =>
                    string.Equals(n, resKey, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    using var stream = asm.GetManifestResourceStream(match);
                    if (stream != null)
                    {
                        using var ms = new MemoryStream();
                        stream.CopyTo(ms);
                        img = Image.FromStream(new MemoryStream(ms.ToArray()));
                    }
                }
            }
            catch { }

            if (img == null)
            {
                try
                {
                    var exeDir = Path.GetDirectoryName(Application.ExecutablePath) ?? ".";
                    var full   = Path.Combine(exeDir, "Resources", "Images", path);
                    if (File.Exists(full))
                        img = Image.FromFile(full);
                    // Fallback: try .jpg if .png not found (and vice versa)
                    if (img == null && path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        var jpgPath = Path.Combine(exeDir, "Resources", "Images",
                            path.Substring(0, path.Length - 4) + ".jpg");
                        if (File.Exists(jpgPath)) img = Image.FromFile(jpgPath);
                    }
                    else if (img == null && path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                    {
                        var pngPath = Path.Combine(exeDir, "Resources", "Images",
                            path.Substring(0, path.Length - 4) + ".png");
                        if (File.Exists(pngPath)) img = Image.FromFile(pngPath);
                    }
                }
                catch { }
            }

            _imageCache[path] = img;
            return img;
        }

        public static void ClearImageCache() => _imageCache.Clear();
    }
}
