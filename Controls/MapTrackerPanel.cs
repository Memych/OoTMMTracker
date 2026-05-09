using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OoTMMTracker.Models;

namespace OoTMMTracker.Controls
{
    /// <summary>
    /// Displays a sub-region map scaled to fill the panel (preserving aspect ratio),
    /// with location mark icons drawn directly in OnPaint for correct transparency.
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

        private Rectangle _mapRect;
        private Image? _bgImage;

        // Preloaded mark images
        private readonly List<(MapMark mark, Image img)> _markImages = new();

        private static readonly Dictionary<string, Image?> _imageCache = new();

        // ── Selection state ───────────────────────────────────────────────────
        // List of marks under last clicked position, index into it
        private List<(MapMark mark, Image img)> _clickCandidates = new();
        private int _selectedCandidateIndex = -1;
        private MapMark? _selectedMark = null;
        private Point _lastClickPoint = Point.Empty;

        /// Fired when user right-clicks a selected mark — passes location names to mark as found.
        public event Action<IReadOnlyList<string>, string>? MarkCompleted; // (locationNames, game)

        public MapTrackerPanel()
        {
            this.BackColor    = Color.FromArgb(20, 20, 20);
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.MouseMove   += OnMouseMoveHandler;
            this.MouseDown   += OnMouseDownHandler;
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

            this.Invalidate();
        }

        // ── Layout ────────────────────────────────────────────────────────────

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        private Rectangle ComputeMapRect()
        {
            if (_bgImage == null) return Rectangle.Empty;
            int pw = this.ClientSize.Width;
            int ph = this.ClientSize.Height;
            float scale = Math.Min((float)pw / _bgImage.Width, (float)ph / _bgImage.Height);
            int w = (int)(_bgImage.Width  * scale);
            int h = (int)(_bgImage.Height * scale);
            return new Rectangle((pw - w) / 2, (ph - h) / 2, w, h);
        }

        // ── Paint ─────────────────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_bgImage == null) return;

            _mapRect = ComputeMapRect();
            var g = e.Graphics;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(_bgImage, _mapRect);

            float scaleX = (float)_mapRect.Width  / _bgImage.Width;
            float scaleY = (float)_mapRect.Height / _bgImage.Height;
            float scale  = Math.Min(scaleX, scaleY);

            foreach (var (mark, img) in _markImages)
            {
                if (!IsMarkVisible(mark)) continue;

                int size = Math.Max(8, (int)(mark.Size * scale));
                int px   = _mapRect.X + (int)(mark.X * scaleX);
                int py   = _mapRect.Y + (int)(mark.Y * scaleY);

                g.DrawImage(img, new Rectangle(px, py, size, size));

                // Draw selection overlay if this mark is selected
                if (_selectedMark == mark)
                {
                    // Redraw icon with yellow tint — only affects non-transparent pixels
                    var destRect = new Rectangle(px, py, size, size);
                    var cm = new System.Drawing.Imaging.ColorMatrix(new float[][]
                    {
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

            float scaleX = (float)_mapRect.Width  / _bgImage.Width;
            float scaleY = (float)_mapRect.Height / _bgImage.Height;
            float scale  = Math.Min(scaleX, scaleY);

            // Find all marks under click position (in reverse order — top to bottom)
            var candidates = new List<(MapMark mark, Image img)>();
            foreach (var (mark, img) in _markImages)
            {
                if (!IsMarkVisible(mark)) continue;
                int size = Math.Max(8, (int)(mark.Size * scale));
                int px   = _mapRect.X + (int)(mark.X * scaleX);
                int py   = _mapRect.Y + (int)(mark.Y * scaleY);
                if (new Rectangle(px, py, size, size).Contains(e.Location))
                    candidates.Add((mark, img));
            }

            if (e.Button == MouseButtons.Left)
            {
                if (candidates.Count == 0)
                {
                    // Clicked on empty space — clear selection
                    _selectedMark = null;
                    _clickCandidates.Clear();
                    _selectedCandidateIndex = -1;
                    _lastClickPoint = Point.Empty;
                    this.Invalidate();
                }
                else
                {
                    // Check if clicking near the same position as before
                    const int samePosTolerance = 4;
                    bool samePosition = _clickCandidates.Count > 0 &&
                                       Math.Abs(e.X - _lastClickPoint.X) <= samePosTolerance &&
                                       Math.Abs(e.Y - _lastClickPoint.Y) <= samePosTolerance;

                    if (samePosition)
                    {
                        // Remove any candidates that are no longer visible (e.g. were just completed)
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
                            // Cycle to next candidate
                            _selectedCandidateIndex = (_selectedCandidateIndex + 1) % _clickCandidates.Count;
                            _selectedMark = _clickCandidates[_selectedCandidateIndex].mark;
                        }
                    }
                    else
                    {
                        // New position — build fresh candidate list, select first
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
                if (_selectedMark != null && _selectedMark.LocationNames.Count > 0)
                {
                    // Mark all locations as found
                    MarkCompleted?.Invoke(_selectedMark.LocationNames, _game);
                    // Clear selection
                    _selectedMark = null;
                    _clickCandidates.Clear();
                    _selectedCandidateIndex = -1;
                    this.Invalidate();
                }
            }
        }

        // ── Tooltip on hover ──────────────────────────────────────────────────

        private string _lastTooltip = "";

        private void OnMouseMoveHandler(object? sender, MouseEventArgs e)
        {
            if (_bgImage == null) return;

            float scaleX = (float)_mapRect.Width  / _bgImage.Width;
            float scaleY = (float)_mapRect.Height / _bgImage.Height;
            float scale  = Math.Min(scaleX, scaleY);

            string tip = "";
            foreach (var (mark, _) in _markImages)
            {
                if (!IsMarkVisible(mark)) continue;
                int size = Math.Max(8, (int)(mark.Size * scale));
                int px   = _mapRect.X + (int)(mark.X * scaleX);
                int py   = _mapRect.Y + (int)(mark.Y * scaleY);
                if (new Rectangle(px, py, size, size).Contains(e.Location))
                {
                    tip = mark.Tooltip;
                    break;
                }
            }

            if (tip != _lastTooltip)
            {
                _lastTooltip = tip;
                _toolTip.SetToolTip(this, tip);
            }
        }

        // ── Visibility helpers ────────────────────────────────────────────────

        private bool IsMarkVisible(MapMark mark)
        {
            bool ageMatch = mark.AgeFilter == "both" || mark.AgeFilter == _ageFilter;
            if (!ageMatch || !IsMarkKnown(mark) || IsMarkComplete(mark)) return false;
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
