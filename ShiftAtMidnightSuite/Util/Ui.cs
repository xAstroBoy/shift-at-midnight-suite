using System;
using UnityEngine;

namespace ShiftAtMidnightSuite.Util
{
    /// <summary>
    /// Tiny IMGUI helpers shared by the menu and the detector overlay.
    ///
    /// Deliberately limited to calls that survive IL2CPP stripping in this build. Texture2D and
    /// GUI.DrawTexture throw "Method unstripping failed" here, so solid fills are drawn by stacking
    /// tinted GUI.Box calls instead - the same technique the original mod menu used.
    /// </summary>
    internal static class Ui
    {
        internal static readonly Color Accent = new Color(1f, 0.30f, 0f, 1f);
        internal static readonly Color AccentDim = new Color(1f, 0.25f, 0f, 1f);
        internal static readonly Color Text = new Color(0.98f, 0.98f, 0.96f, 1f);
        internal static readonly Color TextDim = new Color(0.72f, 0.70f, 0.68f, 1f);
        internal static readonly Color Panel = new Color(0f, 0f, 0f, 0.97f);
        internal static readonly Color RowSel = new Color(0.95f, 0.24f, 0.005f, 0.45f);

        /// <summary>
        /// Global size multiplier for the UniverseLib panel. Changing it rebuilds the panel, which
        /// is the one kind of UI change that can be applied without restarting the game.
        /// </summary>
        internal static float Scale = 1f;

        /// <summary>Scale an integer size, never below 1.</summary>
        internal static int S(int v)
        {
            int r = (int)System.Math.Round(v * Scale);
            return r < 1 ? 1 : r;
        }

        internal static GUIStyle Title, Section, Row, RowSelected, Small, Center, Tab, TabActive;

        /// <summary>Big styles for the in-world overlay, which is read at a glance mid-game.</summary>
        internal static GUIStyle OverlayTitle, OverlayLine;

        private static bool _ready;
        private static float _builtScale = -1f;

        internal static void EnsureStyles()
        {
            // Rebuild when the scale changes, otherwise the IMGUI styles keep their old font sizes.
            if (_ready && Row != null && Math.Abs(_builtScale - Scale) < 0.001f) return;
            _builtScale = Scale;

            GUIStyle baseLabel;
            try { baseLabel = new GUIStyle(GUI.skin.label); }
            catch { baseLabel = new GUIStyle(); }

            Title = Make(baseLabel, S(25), true, TextAnchor.MiddleLeft);
            Section = Make(baseLabel, S(17), true, TextAnchor.MiddleLeft);
            Row = Make(baseLabel, S(19), false, TextAnchor.MiddleLeft);
            RowSelected = Make(baseLabel, S(20), true, TextAnchor.MiddleLeft);
            Small = Make(baseLabel, S(15), false, TextAnchor.MiddleLeft);
            Center = Make(baseLabel, S(22), true, TextAnchor.MiddleCenter);
            Tab = Make(baseLabel, S(16), false, TextAnchor.MiddleCenter);
            TabActive = Make(baseLabel, S(16), true, TextAnchor.MiddleCenter);

            OverlayTitle = Make(baseLabel, S(34), true, TextAnchor.MiddleCenter);
            OverlayLine = Make(baseLabel, S(22), false, TextAnchor.MiddleLeft);
            _ready = true;
        }

        /// <summary>
        /// Only fontSize is reliably present. The rest is nice-to-have, so each setter is attempted
        /// separately - a stripped one degrades the look instead of killing the whole GUI.
        /// </summary>
        private static GUIStyle Make(GUIStyle from, int size, bool bold, TextAnchor anchor)
        {
            GUIStyle s;
            try { s = new GUIStyle(from); }
            catch { s = new GUIStyle(); }

            try { s.fontSize = size; } catch { }
            try { s.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; } catch { }
            try { s.alignment = anchor; } catch { }
            try { s.wordWrap = false; } catch { }
            return s;
        }

        /// <summary>Solid-ish fill: stack tinted boxes until the colour reads as opaque.</summary>
        internal static void Box(Rect r, Color c)
        {
            Color old = GUI.color;
            GUI.color = c;
            int passes = c.a >= 0.95f ? 6 : (c.a >= 0.5f ? 3 : 2);
            for (int i = 0; i < passes; i++) GUI.Box(r, string.Empty);
            GUI.color = old;
        }

        internal static void Label(Rect r, string text, GUIStyle style, Color c)
        {
            Color old = GUI.color;
            GUI.color = c;
            try { GUI.Label(r, text ?? string.Empty, style ?? GUI.skin.label); }
            catch { }
            GUI.color = old;
        }
    }
}
