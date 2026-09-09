using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace ShiftAtMidnightSuite.Util
{
    /// <summary>
    /// Per-section timing so a lag spike can be attributed rather than guessed at.
    ///
    /// Every module tick is wrapped in a Section. Two outputs: any single call over
    /// <see cref="SpikeMs"/> is logged immediately with its duration, and every
    /// <see cref="SummaryEvery"/> seconds the top sections by total time are logged. Cost when
    /// disabled is one bool check per section.
    /// </summary>
    internal static class Profiler
    {
        internal static bool Enabled;
        internal static float SpikeMs = 6f;
        internal const float SummaryEvery = 10f;

        private sealed class Stat
        {
            internal int Calls;
            internal double TotalMs;
            internal double MaxMs;
        }

        private static readonly Dictionary<string, Stat> _stats = new Dictionary<string, Stat>();
        private static float _nextSummary;
        private static readonly Stopwatch _clock = Stopwatch.StartNew();

        internal struct Section : IDisposable
        {
            private readonly string _name;
            private readonly long _start;
            private readonly bool _active;

            internal Section(string name)
            {
                _active = Enabled;
                _name = name;
                _start = _active ? _clock.ElapsedTicks : 0;
            }

            public void Dispose()
            {
                if (!_active) return;
                double ms = (_clock.ElapsedTicks - _start) * 1000.0 / Stopwatch.Frequency;
                Record(_name, ms);
            }
        }

        internal static Section Begin(string name) { return new Section(name); }

        private static void Record(string name, double ms)
        {
            Stat s;
            if (!_stats.TryGetValue(name, out s)) { s = new Stat(); _stats[name] = s; }
            s.Calls++;
            s.TotalMs += ms;
            if (ms > s.MaxMs) s.MaxMs = ms;

            if (ms >= SpikeMs) Log.Msg("SPIKE " + name + " took " + ms.ToString("0.0") + " ms");
        }

        /// <summary>Call once per frame from the mod's update.</summary>
        internal static void Tick()
        {
            if (!Enabled) return;
            float now = Time.unscaledTime;
            if (now < _nextSummary) return;
            _nextSummary = now + SummaryEvery;
            if (_stats.Count == 0) return;

            var rows = new List<KeyValuePair<string, Stat>>(_stats);
            rows.Sort(delegate (KeyValuePair<string, Stat> a, KeyValuePair<string, Stat> b)
            {
                return b.Value.TotalMs.CompareTo(a.Value.TotalMs);
            });

            Log.Msg("--- profile, last " + SummaryEvery + "s (total ms / calls / worst) ---");
            int shown = 0;
            for (int i = 0; i < rows.Count && shown < 12; i++)
            {
                Stat s = rows[i].Value;
                if (s.TotalMs < 0.5) continue;
                Log.Msg("  " + rows[i].Key.PadRight(22) + s.TotalMs.ToString("0.0").PadLeft(8) + " ms" +
                        s.Calls.ToString().PadLeft(7) + " calls   worst " + s.MaxMs.ToString("0.0") + " ms");
                shown++;
            }
            _stats.Clear();
        }
    }
}
