using System;
using Il2CppFusion;

namespace ShiftAtMidnightSuite.Util
{
    /// <summary>
    /// Invoke an Rpc_ method on a NetworkBehaviour so that it actually runs.
    ///
    /// Measured on a real Host session (Dump Network State): scene managers report mask 1 with
    /// state authority, the local player's objects report 3 with state + input. So in this Fusion
    /// build STATE = 1, INPUT = 2, PROXY = 4, ALL = 7.
    ///
    /// Fusion's generated dispatch for a broadcast Rpc_ method:
    ///     042 Compare mask, 1  ; STATE only
    ///     043 JumpIfEqual {91} ; -> NotifyLocalSimulationNotAllowedToSendRpc, RETURN. Refused.
    ///     054..082 SendRpc     ; other masks: message goes out
    ///     083 Compare mask, 7
    ///     084 JumpIfNotEqual {body}   ; and runs here too
    ///     085..090 Return             ; ALL: sent, not run here
    /// A host holds STATE only on the scene managers, so the plain call is refused and nothing
    /// happens at all - that is why the event menu did nothing. The escape is Fusion's own
    /// NetworkBehaviour.InvokeRpc flag, tested at the top of the method, which jumps straight to
    /// the body. On the host the body is the authoritative action: anything it spawns or sets on
    /// networked state replicates by itself.
    ///
    /// For an Rpc_CMD_ command the shape differs: mask 7 is refused, mask 1 takes the send path
    /// (which Fusion routes to the state authority - us - and invokes), anything else runs the
    /// body directly behind a HasStateAuthority check.
    /// </summary>
    internal static class Rpc
    {
        internal const int State = 1;
        internal const int AuthorityAll = 7;

        internal static int MaskOf(NetworkBehaviour nb)
        {
            try { return nb.GetLocalAuthorityMask(); }
            catch { return -1; }
        }

        private static void RunBody(NetworkBehaviour owner, Action call)
        {
            try
            {
                owner.InvokeRpc = true;
                call();
            }
            finally
            {
                try { owner.InvokeRpc = false; } catch { }
            }
        }

        /// <summary>Broadcast-style Rpc_ method. Returns the mask seen, for logging.</summary>
        internal static int Call(NetworkBehaviour owner, Action call, string label)
        {
            int mask = MaskOf(owner);

            if (mask == State)
            {
                // Dispatch refuses a STATE-only caller before doing anything. Run the body here;
                // as the authority, its networked side effects replicate on their own.
                RunBody(owner, call);
                Log.Debug(label + ": run as authority (mask STATE - dispatch would refuse).");
                return mask;
            }

            call();
            if (mask == AuthorityAll)
            {
                // Sent, but the dispatch skipped local execution. Run it here as well.
                RunBody(owner, call);
                Log.Debug(label + ": broadcast, then run locally (mask ALL).");
                return mask;
            }

            Log.Debug(label + ": Fusion dispatch (mask " + mask + ").");
            return mask;
        }

        /// <summary>Command-style Rpc_CMD_ method (client -> state authority).</summary>
        internal static int Command(NetworkBehaviour owner, Action call, string label)
        {
            int mask = MaskOf(owner);
            if (mask == AuthorityAll)
            {
                RunBody(owner, call);
                Log.Debug(label + ": command run directly (mask ALL - dispatch would refuse).");
                return mask;
            }

            call();
            Log.Debug(label + ": command via dispatch (mask " + mask + ").");
            return mask;
        }
    }
}
