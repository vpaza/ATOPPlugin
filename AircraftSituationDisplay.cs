using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vatsys;
using vatsys.Plugin;

namespace ATOP
{
    internal class AircraftSituationDisplay
    {
        public string Name { get => "ATOP ASD"; }

        #region Data
        public static readonly ConcurrentDictionary<string, bool> eastboundCallsigns = new ConcurrentDictionary<string, bool>();
        public static readonly ConcurrentDictionary<string, char> adsbcpdlcValues = new ConcurrentDictionary<string, char>();
        public static readonly ConcurrentDictionary<string, char> adsflagValues = new ConcurrentDictionary<string, char>();
        public static readonly ConcurrentDictionary<string, char> mntflagValues = new ConcurrentDictionary<string, char>();
        public static readonly ConcurrentDictionary<string, char> altValues = new ConcurrentDictionary<string, char>();
        public static readonly ConcurrentDictionary<string, bool> radartoggle = new ConcurrentDictionary<string, bool>();
        public static readonly ConcurrentDictionary<string, bool> mntflagtoggle = new ConcurrentDictionary<string, bool>();
        public static readonly ConcurrentDictionary<string, bool> downlink = new ConcurrentDictionary<string, bool>();
        public static readonly ConcurrentDictionary<string, bool> graphicRouteShown = new ConcurrentDictionary<string, bool>();

        public static V GetValue<T, V>(ConcurrentDictionary<T, V> dict, T key)
        {
            V value;
            if (dict.TryGetValue(key, out value))
            {
                return value;
            }

            return default;
        }
        #endregion

        #region Events
        public static void OnPrivateMessagesChanged(object sender, Network.GenericMessageEventArgs e)
        {
            switch (e.Message.Sent)
            {
                case true:
                    downlink.TryRemove(e.Message.Address, out _);
                    break;
                case false:
                    downlink.TryAdd(e.Message.Address, true);
                    break;
            }
        }

        internal static CustomColour SelectASDTrackColour(Track track)
        {
            if (track.GetFDR() == null) { return null; }

            if (track.State == MMI.HMIStates.Jurisdiction)
            {
                return Shared.GetDirectionColor(track.GetFDR().Callsign);
            }

            return default;
        }
        #endregion

        internal static void HandleRadarFlagClick(CustomLabelItemMouseClickEventArgs e)
        {
            HandleRadarFlagClick(e.Track.GetFDR().Callsign);

            e.Handled = true;
        }

        private static void HandleRadarFlagClick(string callsign)
        {
            if (radartoggle.TryGetValue(callsign, out _))
            {
                radartoggle.TryRemove(callsign, out _);
            }
            else
            {
                radartoggle.TryAdd(callsign, true);
            }
        }

        internal static void HandleDownlinkClick(CustomLabelItemMouseClickEventArgs args)
        {
            AircraftSituationDisplay.downlink.TryRemove(args.Track.GetFDR().Callsign, out _);
        }
    }
}
