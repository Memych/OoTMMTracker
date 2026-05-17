using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace OoTMMTracker.Forms
{
    public class InfoForm : Form
    {
        public InfoForm()
        {
            this.Text            = "About OoTMM Tracker";
            this.Size            = new Size(560, 640);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.FromArgb(30, 30, 30);

            var rtb = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                ReadOnly    = true,
                BackColor   = Color.FromArgb(30, 30, 30),
                ForeColor   = Color.FromArgb(220, 220, 220),
                BorderStyle = BorderStyle.None,
                Font        = new Font("Segoe UI", 9.5f),
                ScrollBars  = RichTextBoxScrollBars.Vertical,
                DetectUrls  = true,
            };
            this.Controls.Add(rtb);

            H(rtb, "OoTMM Tracker v1.2.2");
            N(rtb, "2026  --  Made in Russia  --  by ");
            B(rtb, "Memych");
            N(rtb, "\n\n");

            H(rtb, "About");
            N(rtb, "A manual tracker for the OoTMM randomizer -- Ocarina of Time + Majora's Mask combined.\nLoad a spoiler log to track locations, items, entrances and map marks.\n\n");

            H(rtb, "Sections");
            E(rtb, "Item Tracker", "Left panel. Shows all items for OoT and MM. Click to toggle collected state.");
            E(rtb, "Locations",    "Full list of all locations from the spoiler log. Check off found items.");
            E(rtb, "Settings",     "Spoiler log settings, tricks, starting items, world flags and special conditions.");
            E(rtb, "Entrances",    "List of all entrance shuffle connections from the spoiler log.");
            E(rtb, "Song Events",  "OoT song event assignments (shuffle mode).");
            E(rtb, "Map",          "Visual map tracker. Select region and sub-map, toggle Child/Adult (OoT) or Cursed/Cleared (MM).");
            N(rtb, "\n");

            H(rtb, "Menu");
            E(rtb, "File",    "Load spoiler log, save and load tracker progress.");
            E(rtb, "Reset",   "Reset All / Tracker / Progress / Log.");
            E(rtb, "Tracker", "Options, Broadcast, zoom controls.");
            E(rtb, "About",   "Credits.");
            N(rtb, "\n");

            H(rtb, "Keyboard Shortcuts");
            E(rtb, "F1",       "Load Spoiler Log");
            E(rtb, "F2",       "Save progress");
            E(rtb, "F3",       "Tracker Options");
            E(rtb, "F4",       "Load progress");
            E(rtb, "F5",       "Reset All");
            E(rtb, "F6",       "Reset Log");
            E(rtb, "F7",       "Reset Tracker");
            E(rtb, "F8",       "Reset Progress");
            E(rtb, "F9",       "Open Broadcast window");
            E(rtb, "Shift + '+'", "Zoom in");
            E(rtb, "Shift + '-'", "Zoom out");
            E(rtb, "Shift + '0'", "Reset zoom");
            N(rtb, "\n");

            H(rtb, "Map Tracker Tips");
            N(rtb, "* Marks appear only when the location exists in the loaded log.\n");
            N(rtb, "* Enable ");
            B(rtb, "Colors");
            N(rtb, " to highlight consumables/traps -- map marks for those locations are hidden.\n");
            N(rtb, "* Child/Adult toggle filters age-specific marks (OoT only).\n");
            N(rtb, "* Cursed/Cleared toggle for MM regions.\n");
            N(rtb, "* Entrance shuffle marks (door icon):\n");
            N(rtb, "  - Right-click on a mark jumps to the destination map\n");
            N(rtb, "  - Quick Jump supports warp songs, owls, and wallmasters as entrances to regions\n\n");

            H(rtb, "OoTMM Randomizer");
            N(rtb, "https://ootmm.com/gen/stable\n");

            rtb.LinkClicked += (s, e) =>
            {
                try { Process.Start(new ProcessStartInfo(e.LinkText) { UseShellExecute = true }); }
                catch { }
            };

            rtb.SelectionStart = 0;
        }

        private static void H(RichTextBox r, string t)
        {
            r.SelectionFont  = new Font("Segoe UI", 11f, FontStyle.Bold);
            r.SelectionColor = Color.FromArgb(255, 200, 80);
            r.AppendText(t + "\n");
            r.SelectionFont  = new Font("Segoe UI", 9.5f);
            r.SelectionColor = Color.FromArgb(220, 220, 220);
        }

        private static void B(RichTextBox r, string t)
        {
            r.SelectionFont  = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            r.SelectionColor = Color.FromArgb(220, 220, 220);
            r.AppendText(t);
            r.SelectionFont  = new Font("Segoe UI", 9.5f);
        }

        private static void N(RichTextBox r, string t)
        {
            r.SelectionFont  = new Font("Segoe UI", 9.5f);
            r.SelectionColor = Color.FromArgb(220, 220, 220);
            r.AppendText(t);
        }

        private static void E(RichTextBox r, string key, string val)
        {
            B(r, key);
            N(r, "  --  " + val + "\n");
        }
    }
}
