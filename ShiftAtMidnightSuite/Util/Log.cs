using System;
using MelonLoader;

namespace ShiftAtMidnightSuite.Util
{
    internal static class Log
    {
        private const string P = "[SAM Suite] ";

        internal static bool Verbose;

        internal static void Msg(string s) { MelonLogger.Msg(P + s); }
        internal static void Warn(string s) { MelonLogger.Warning(P + s); }
        internal static void Err(string s) { MelonLogger.Error(P + s); }

        internal static void Debug(string s)
        {
            if (Verbose) MelonLogger.Msg(P + s);
        }

        internal static void Ex(string what, Exception ex)
        {
            MelonLogger.Warning(P + what + ": " + (ex == null ? "?" : ex.Message));
        }
    }
}
