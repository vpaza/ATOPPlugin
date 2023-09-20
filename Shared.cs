using vatsys.Plugin;

namespace ATOP
{
    internal class Shared
    {
        internal static CustomColour GetDirectionColor(string callsign)
        {
            if (FlightPlan.GetFlightPlan(callsign) != null)
            {
                if (FlightPlan.GetFlightPlan(callsign).IsEastbound)
                    return Colors.EastboundTracks;
                else
                    return Colors.WestboundTracks;
            }
            else
            {
                return Colors.WestboundTracks;
            }
        }

        internal static string FindSectorID(string callsign)
        {
            if (callsign == null)
                return null;

            // 3 characters or less are generally defined by us, so we can just return the callsign
            if (callsign.Length < 4)
                return callsign;

            // Attempt to be smart... we'll have AAA_BB_CCC, we want to return BB
            // If we don't have that, just return the callsign
            // This is because external facilities aren't really known to us by sector, so we
            // attempt to figure out the sector id from the callsign
            string[] split = callsign.Split('_');
            if (split.Length == 3)
            {
                return split[1];
            }

            return callsign;
        }
    }
}
