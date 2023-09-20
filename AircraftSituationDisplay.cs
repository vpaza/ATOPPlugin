using vatsys;
using vatsys.Plugin;

namespace ATOP
{
    internal class AircraftSituationDisplay
    {
        #region Events
        public static void OnPrivateMessagesChanged(object sender, Network.GenericMessageEventArgs e)
        {
            FlightPlan fp = FlightPlan.GetFlightPlan(e.Message.Address);
            if (fp == null) { return; }
            fp.HasDownlinkFlag = e.Message.Sent;
            FlightPlan.AddOrUpdate(e.Message.Address, fp);
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
            var fpl = FlightPlan.GetFlightPlan(callsign);
            if (fpl == null) { return; }
            fpl.RadarContactFlag = !fpl.RadarContactFlag;
            FlightPlan.AddOrUpdate(callsign, fpl);
        }

        internal static void HandleDownlinkClick(CustomLabelItemMouseClickEventArgs args)
        {
            var fpl = FlightPlan.GetFlightPlan(args.Track.GetFDR().Callsign);
            if (fpl == null) { return; }
            fpl.HasDownlinkFlag = !fpl.HasDownlinkFlag;
            FlightPlan.AddOrUpdate(args.Track.GetFDR().Callsign, fpl);
        }
    }
}
