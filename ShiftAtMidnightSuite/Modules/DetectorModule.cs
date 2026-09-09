using System;
using System.Collections.Generic;
using Il2Cpp;
using ShiftAtMidnightSuite.Util;
using UnityEngine;

namespace ShiftAtMidnightSuite.Modules
{
    /// <summary>
    /// Doppelganger detector. The game marks them itself with StoreBrowseBehaviour.isDoppelganger,
    /// so there is no prefab-name guessing needed to know *that* someone is one.
    ///
    /// The *why* is authored too: the end-of-day report prints it from the localisation table under
    /// "EOD Report Descs", keyed by the npc id. The overlay pulls that same sentence, so what you
    /// read in the world is exactly what the report will tell you afterwards. ID name and the other
    /// flags are shown underneath as supporting detail.
    /// </summary>
    internal sealed class DetectorModule
    {
        internal bool Enabled = true;
        internal bool Radar = true;
        internal bool ShowEvidence = true;

        /// <summary>Small corner tag while armed, so "is it even on?" has a visible answer.</summary>
        internal bool ShowBadge = true;
        internal float RadarRange = 40f;

        /// <summary>Size multiplier for the in-world warning. Independent of the menu scale.</summary>
        internal float OverlayScale = 1f;

        private const float ScanInterval = 0.15f;
        private const float RadarInterval = 0.5f;   // cheap now: registry, not a scene scan
        private float _nextRadar;
        private float _nextSeed;

        private float _nextScan;
        private bool _lookingAtDoppel;
        private string _subject = "";
        private readonly List<string> _evidence = new List<string>();
        private int _nearbyCount;

        internal bool LookingAtDoppelganger { get { return _lookingAtDoppel; } }
        internal int NearbyCount { get { return _nearbyCount; } }

        internal void OnSceneChanged()
        {
            _reportCache.Clear();
            _verdicts.Clear();
            _lookingAtDoppel = false;
            _subject = "";
            _evidence.Clear();
            _nearbyCount = 0;
        }

        internal void Tick()
        {
            if (!Enabled)
            {
                if (_lookingAtDoppel || _nearbyCount != 0) OnSceneChanged();
                return;
            }

            float now = Time.unscaledTime;
            if (now < _nextScan) return;
            _nextScan = now + ScanInterval;

            ScanCrosshair();
            if (Radar)
            {
                if (now >= _nextRadar) { _nextRadar = now + RadarInterval; ScanNearby(); }
            }
            else _nearbyCount = 0;
        }

        // Inspect() walks up to 24 parents doing three GetComponent calls each - fine once, but
        // it was running 6-7 times a second against the same collider while you look at someone.
        // Remember the verdict per collider for a moment and only re-walk when the target changes.
        private sealed class Verdict
        {
            internal bool Doppel;
            internal string Subject;
            internal List<string> Evidence;
            internal float Until;
        }
        private readonly Dictionary<int, Verdict> _verdicts = new Dictionary<int, Verdict>();
        private const float VerdictTtl = 2f;

        private void ScanCrosshair()
        {
            _lookingAtDoppel = false;
            _subject = "";
            _evidence.Clear();

            Camera cam = Camera.main;
            if (!Net.Alive(cam)) return;

            try
            {
                Transform eye = cam.transform;
                Ray ray = new Ray(eye.position + eye.forward * 0.25f, eye.forward);
                RaycastHit hit;
                if (!Physics.Raycast(ray, out hit, 40f, ~0, QueryTriggerInteraction.Collide)) return;

                Collider col = hit.collider;
                if (col == null) return;

                int key = 0;
                try { key = col.GetInstanceID(); } catch { }

                Verdict v;
                float now = Time.unscaledTime;
                if (key != 0 && _verdicts.TryGetValue(key, out v) && now < v.Until)
                {
                    _lookingAtDoppel = v.Doppel;
                    _subject = v.Subject;
                    _evidence.AddRange(v.Evidence);
                    return;
                }

                Inspect(col.transform);

                if (key != 0)
                {
                    _verdicts[key] = new Verdict
                    {
                        Doppel = _lookingAtDoppel,
                        Subject = _subject,
                        Evidence = new List<string>(_evidence),
                        Until = now + VerdictTtl
                    };
                    if (_verdicts.Count > 256) _verdicts.Clear();
                }
            }
            catch (Exception ex) { Log.Debug("detector raycast: " + ex.Message); }
        }

        /// <summary>Walk up the hierarchy to the NPC that owns the collider, gathering evidence.</summary>
        private void Inspect(Transform start)
        {
            Transform t = start;
            for (int depth = 0; depth < 24 && t != null; depth++)
            {
                GameObject go;
                try { go = t.gameObject; } catch { break; }
                if (go == null) { try { t = t.parent; } catch { break; } continue; }

                try
                {
                    StoreBrowseBehaviour b = go.GetComponent<StoreBrowseBehaviour>();
                    if (b != null)
                    {
                        ReadBrowser(go, b);
                        return;
                    }
                }
                catch { }

                try
                {
                    Npc npc = go.GetComponent<Npc>();
                    if (npc != null)
                    {
                        _lookingAtDoppel = npc.isDoppelganger;
                        _subject = SafeName(go);
                        if (_lookingAtDoppel)
                        {
                            Add("flagged as a doppelganger in the night roster (Npc.isDoppelganger)");
                            AddIfText("roster id: " + Str(npc.id), Str(npc.id));
                        }
                        return;
                    }
                }
                catch { }

                // Event-spawned player copies are plain objects with a telling name.
                try
                {
                    string name = go.name;
                    if (name != null && name.IndexOf("Doppel", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _lookingAtDoppel = true;
                        _subject = SafeName(go);
                        Add("object name contains \"Doppel\" - an event-spawned copy");
                        return;
                    }
                }
                catch { }

                try { t = t.parent; } catch { break; }
            }
        }

        /// <summary>
        /// The end-of-day report explains why each customer was a doppelganger, and that text is not
        /// generated - it is authored, and looked up as
        /// <c>JSONAccess.GetMiscText("EOD Report Descs", npcId)</c>. The npc id is
        /// <c>StoreBrowseBehaviour.dialogueInteractable.dialogueId</c>, which is what the report
        /// parses when it records a customer. So we can show the same sentence live, in the world,
        /// instead of making one up.
        /// </summary>
        // JSONAccess lookups twice per 0.15s scan while you stare at someone is needless; the
        // answer for a given NPC never changes, so remember it by instance id.
        private readonly Dictionary<int, string[]> _reportCache = new Dictionary<int, string[]>();

        private string LookupReportDesc(StoreBrowseBehaviour b, out string reportName)
        {
            reportName = null;
            int key = 0;
            try { key = b.GetInstanceID(); } catch { }
            string[] hit;
            if (key != 0 && _reportCache.TryGetValue(key, out hit)) { reportName = hit[0]; return hit[1]; }

            string desc = LookupReportDescUncached(b, out reportName);
            if (key != 0) _reportCache[key] = new string[] { reportName, desc };
            return desc;
        }

        private static string LookupReportDescUncached(StoreBrowseBehaviour b, out string reportName)
        {
            reportName = null;
            try
            {
                DialogueInteractable di = b.dialogueInteractable;
                if (di == null) return null;

                string key = di.dialogueId;
                if (string.IsNullOrEmpty(key)) return null;

                JSONAccess json = JSONAccess.Instance;
                if (!Net.Alive(json)) return null;

                reportName = json.GetMiscText("EOD Report Names", key);
                string desc = json.GetMiscText("EOD Report Descs", key);

                // The loader returns a not-found marker rather than null for a missing key.
                if (!string.IsNullOrEmpty(desc) &&
                    desc.IndexOf("not found", StringComparison.OrdinalIgnoreCase) < 0 &&
                    desc.IndexOf("TNF", StringComparison.Ordinal) < 0)
                    return desc;
            }
            catch (Exception ex) { Log.Debug("EOD desc lookup: " + ex.Message); }
            return null;
        }

        private void ReadBrowser(GameObject go, StoreBrowseBehaviour b)
        {
            _subject = SafeName(go);
            try { _lookingAtDoppel = b.isDoppelganger; }
            catch { return; }

            if (!_lookingAtDoppel) return;

            // The authored explanation first - it is the actual answer to "why".
            string reportName;
            string why = LookupReportDesc(b, out reportName);
            if (!string.IsNullOrEmpty(reportName)) _subject = reportName;
            if (!string.IsNullOrEmpty(why))
            {
                Add(StripAuthoredPrefix(why));
            }
            else
            {
                Add("flagged by the game (StoreBrowseBehaviour.isDoppelganger)");
            }

            // The id/database name and killed-record id are the same name as the title, and the
            // stolen-items flag is noise next to the authored tell - none of them are shown.
            try { if (b.isThief) Add("also flagged as a thief"); } catch { }
            try { if (!b.countsAsCustomer) Add("does not count as a real customer"); } catch { }
            try { if (b.canNeverInteract) Add("cannot be interacted with"); } catch { }
        }

        /// <summary>
        /// The authored report text is prefixed with its category ("Doppelganger - His piercing
        /// is..."). The title already says what they are, so drop that lead-in.
        /// </summary>
        private static string StripAuthoredPrefix(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string t = text.TrimStart();
            if (t.StartsWith("Doppelganger", StringComparison.OrdinalIgnoreCase))
            {
                int i = "Doppelganger".Length;
                while (i < t.Length && (t[i] == ' ' || t[i] == '-' || t[i] == ':' || t[i] == '–' || t[i] == '—')) i++;
                if (i < t.Length) t = t.Substring(i);
            }
            return t;
        }

        private const int WrapAt = 72;
        private const int MaxEvidenceLines = 8;

        /// <summary>The authored report text is a full sentence, so wrap it instead of clipping.</summary>
        private void Add(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            while (line.Length > WrapAt && _evidence.Count < MaxEvidenceLines)
            {
                int cut = line.LastIndexOf(' ', Math.Min(WrapAt, line.Length - 1));
                if (cut <= 0) cut = WrapAt;
                _evidence.Add(line.Substring(0, cut).TrimEnd());
                line = line.Substring(cut).TrimStart();
            }
            if (_evidence.Count < MaxEvidenceLines) _evidence.Add(line);
        }

        private void AddIfText(string line, string value)
        {
            if (!string.IsNullOrEmpty(value)) Add(line);
        }

        private static string Str(string s) { return string.IsNullOrEmpty(s) ? "" : s; }

        private static string SafeName(GameObject go)
        {
            try
            {
                string n = go.name;
                if (string.IsNullOrEmpty(n)) return "";
                // Trim Unity's clone suffix so the label reads like a person, not a prefab.
                int clone = n.IndexOf("(Clone)", StringComparison.Ordinal);
                return clone > 0 ? n.Substring(0, clone) : n;
            }
            catch { return ""; }
        }

        private void ScanNearby()
        {
            _nearbyCount = 0;
            Transform me = Net.LocalTransform;
            if (me == null) return;

            Vector3 origin = me.position;
            float rangeSq = RadarRange * RadarRange;

            // Registry first (filled by the Spawned postfix). It is empty until the first customer
            // spawns after the plugin loaded, so seed it once from the scene if it has nothing.
            List<StoreBrowseBehaviour> npcs = NpcRegistry.Live();
            if (npcs.Count == 0 && Time.unscaledTime >= _nextSeed)
            {
                _nextSeed = Time.unscaledTime + 10f;
                List<StoreBrowseBehaviour> seed = Net.FindActive<StoreBrowseBehaviour>();
                for (int i = 0; i < seed.Count; i++) NpcRegistry.Add(seed[i]);
                npcs = NpcRegistry.Live();
            }
            for (int i = 0; i < npcs.Count; i++)
            {
                StoreBrowseBehaviour b = npcs[i];
                if (!Net.Alive(b)) continue;
                try
                {
                    if (!b.isDoppelganger) continue;
                    GameObject go = b.gameObject;
                    if (go == null || !go.activeInHierarchy || !go.scene.IsValid()) continue;
                    if ((go.transform.position - origin).sqrMagnitude > rangeSq) continue;
                    _nearbyCount++;
                }
                catch { }
            }
        }

        // ------------------------------------------------------------------ overlay styles

        // Owned here, sized off OverlayScale, and rebuilt when that changes. They used to come from
        // the shared menu styles, which are sized off the *menu* scale - so raising the warning size
        // grew the boxes and left the text at the same size inside them.
        private GUIStyle _titleStyle, _lineStyle, _badgeStyle;
        private float _stylesBuiltFor = -1f;

        private const int TitleBase = 24;
        private const int LineBase = 17;
        private const int BadgeBase = 15;

        private void EnsureOverlayStyles()
        {
            float k = Mathf.Clamp(OverlayScale, 0.5f, 3f);
            if (_lineStyle != null && Math.Abs(_stylesBuiltFor - k) < 0.001f) return;
            _stylesBuiltFor = k;

            GUIStyle baseLabel;
            try { baseLabel = new GUIStyle(GUI.skin.label); }
            catch { baseLabel = new GUIStyle(); }

            _titleStyle = Make(baseLabel, Mathf.RoundToInt(TitleBase * k), true, TextAnchor.MiddleCenter);
            _lineStyle = Make(baseLabel, Mathf.RoundToInt(LineBase * k), false, TextAnchor.MiddleLeft);
            _badgeStyle = Make(baseLabel, Mathf.RoundToInt(BadgeBase * k), true, TextAnchor.MiddleCenter);
        }

        private static GUIStyle Make(GUIStyle from, int size, bool bold, TextAnchor anchor)
        {
            GUIStyle st;
            try { st = new GUIStyle(from); } catch { st = new GUIStyle(); }
            try { st.fontSize = size; } catch { }
            try { st.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; } catch { }
            try { st.alignment = anchor; } catch { }
            try { st.wordWrap = false; } catch { }
            try { st.clipping = TextClipping.Overflow; } catch { }
            return st;
        }

        /// <summary>
        /// Drawn from the mod's OnGUI so the warning shows without opening the menu. Every size here
        /// derives from the font size, so text and boxes scale together.
        /// </summary>
        internal void DrawOverlay()
        {
            if (!Enabled) return;
            EnsureOverlayStyles();

            float k = Mathf.Clamp(OverlayScale, 0.5f, 3f);
            float titleH = TitleBase * k * 1.4f;
            float lineH = LineBase * k * 1.3f;
            float pad = 8f * k;
            float w = Mathf.Min(Screen.width * 0.9f, 720f * k);
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.08f;

            if (ShowBadge)
            {
                float bh = BadgeBase * k * 1.6f;
                float bw = 220f * k;
                Rect br = new Rect(Screen.width - bw - 12f, 12f, bw, bh);
                Color bc = _lookingAtDoppel ? new Color(0.62f, 0.02f, 0.02f, 0.92f) : new Color(0.08f, 0.08f, 0.08f, 0.78f);
                Ui.Box(br, bc);
                string badge = _lookingAtDoppel ? "DETECTOR: DOPPELGANGER" :
                               (_nearbyCount > 0 ? "DETECTOR: " + _nearbyCount + " NEARBY" : "DETECTOR ARMED");
                Ui.Label(br, badge, _badgeStyle,
                         _lookingAtDoppel ? new Color(1f, 0.92f, 0.9f, 1f) : new Color(0.85f, 0.85f, 0.82f, 1f));
            }

            if (!_lookingAtDoppel && _nearbyCount == 0) return;

            if (_lookingAtDoppel)
            {
                Ui.Box(new Rect(x, y, w, titleH), new Color(0.62f, 0.02f, 0.02f, 0.95f));
                string title = string.IsNullOrEmpty(_subject)
                    ? "!!  DOPPELGANGER  !!"
                    : "!!  DOPPELGANGER  -  " + _subject + "  !!";
                Ui.Label(new Rect(x, y, w, titleH), title, _titleStyle, new Color(1f, 0.95f, 0.93f, 1f));
                y += titleH + 4f * k;

                if (ShowEvidence && _evidence.Count > 0)
                {
                    float h = pad * 2f + _evidence.Count * lineH;
                    Ui.Box(new Rect(x, y, w, h), new Color(0.14f, 0.02f, 0.02f, 0.94f));
                    float ly = y + pad;
                    for (int i = 0; i < _evidence.Count; i++)
                    {
                        Ui.Label(new Rect(x + pad * 1.5f, ly, w - pad * 3f, lineH), "- " + _evidence[i],
                                 _lineStyle, new Color(1f, 0.86f, 0.76f, 1f));
                        ly += lineH;
                    }
                    y += h + 4f * k;
                }
            }

            if (_nearbyCount > 0)
            {
                float h = lineH + pad;
                Ui.Box(new Rect(x, y, w, h), new Color(0.26f, 0.05f, 0.02f, 0.92f));
                Ui.Label(new Rect(x, y, w, h),
                         _nearbyCount + " doppelganger(s) within " + (int)RadarRange + "m",
                         _titleStyle, new Color(1f, 0.66f, 0.42f, 1f));
            }
        }
    }
}
