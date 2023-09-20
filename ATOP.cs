using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using vatsys;
using vatsys.Plugin;

namespace ATOP
{
    [Export(typeof(IPlugin))]
    public class ATOP : ILabelPlugin, IStripPlugin
    {
        public string Name => "Advanced Techniques and Oceanic Procedures";
        private readonly LabelItems LabelItems = new LabelItems();

        public ATOP()
        {
            Network.PrivateMessagesChanged += AircraftSituationDisplay.OnPrivateMessagesChanged; // AircraftSituationDisplay.cs
        }

        public void OnFDRUpdate(FDP2.FDR updated)
        {
            // GetFDRIndex returns -1 if the callsign is not found (disconnected)
            if (FDP2.GetFDRIndex(updated.Callsign) == -1)
            {
                FlightPlan.Remove(updated.Callsign);
                return;
            }

            FlightPlan.AddOrUpdate(updated.Callsign, updated);

            // Check if we need to automatically assume or drop the aircraft
            AutoAssume(updated);
            AutoDrop(updated);
        }

        public void OnRadarTrackUpdate(RDP.RadarTrack updated)
        {
            AutoAssume(updated);
            AutoDrop(updated);
        }

        #region Track Management
        private void AutoAssume(RDP.RadarTrack rt)
        {
            if (rt.CoupledFDR != null)
            {
                AutoAssume(rt.CoupledFDR);
                RDP.Couple(rt.CoupledFDR, rt);
            }
        }

        private void AutoAssume(FDP2.FDR fdr)
        {
            if (fdr == null) { return; }

            if (!fdr.ESTed && MMI.IsMySectorConcerned(fdr))
            {
                MMI.EstFDR(fdr);
            }

            if (MMI.SectorsControlled.ToList().Exists(s => s.IsInSector(fdr.GetLocation(), fdr.PRL)) &&
                    !fdr.IsTrackedByMe &&
                    MMI.SectorsControlled.Contains(fdr.ControllingSector
                ) || fdr.ControllingSector == null)
            {
                MMI.AcceptJurisdiction(fdr);
            }
        }

        private void AutoDrop(RDP.RadarTrack rt)
        {
            if (rt.CoupledFDR != null && rt != null)
            {
                AutoDrop(rt.CoupledFDR);
            }
        }

        private void AutoDrop(FDP2.FDR fdr)
        {
            if (fdr == null || fdr.CoupledTrack == null) { return; }

            if (
                    MMI.GetSectorEntryTime(fdr) == null &&
                    MMI.SectorsControlled.ToList().TrueForAll(s => !s.IsInSector(fdr.GetLocation(), fdr.PRL))
                )
            {
                MMI.HandoffToNone(fdr);
                Thread.Sleep(50000);
                RDP.DeCouple(fdr.CoupledTrack);
            }
        }
        #endregion

        public CustomLabelItem GetCustomLabelItem(string itemType, Track track, FDP2.FDR flightDataRecord, RDP.RadarTrack radarTrack)
        {
            return LabelItems.GetCustomLabelItem(itemType, track, flightDataRecord, radarTrack); // LabelItems.cs
        }

        CustomColour ILabelPlugin.SelectASDTrackColour(Track track)
        {
            return AircraftSituationDisplay.SelectASDTrackColour(track); // AircraftSituationDisplay.cs
        }

        CustomColour ILabelPlugin.SelectGroundTrackColour(Track track)
        {
            return null;
        }

        CustomStripItem IStripPlugin.GetCustomStripItem(string itemType, Track track, FDP2.FDR flightDataRecord, RDP.RadarTrack radarTrack)
        {
            return Strips.GetCustomStripItem(itemType, track, flightDataRecord, radarTrack); // Strips.cs
        }
    }

    internal class Symbols
    {
        // Symbols by type
        // Commented out symbols are not currently used but may be used in the future, they are commented
        // out solely to silence the warnings of unused variables
        private static string THREE = "3";
        private static string FOUR = "4";
        private static string A = "A";
        private static string D = "D";
        private static string H = "H";
        private static string M = "M";
        private static string O = "O";
        public static string R = "R";
        private static string W = "W";
        //private static string X = "X";
        private static string AMPERSAND = "&";
        private static string ASTERISK_SPECIAL = "✱";
        private static string ARROW_UP = "↑";
        private static string ARROW_DOWN = "↓";
        private static string BOXED_ASTERISK = "⧆";
        private static string CARROT = "^";
        private static string DOT = "◦";
        private static string LOWERCASE_X = "x";
        private static string PLUS = "+";
        private static string MINUS = "-";
        private static string SPACE = " ";
        //private static string SQUARE_FILLED = "⬛";
        private static string SQUARE_HOLLOW = "⬜";
        private static string STAR_FILLED = "★";
        //private static string STAR_HOLLOW = "☆";
        private static string TRIANGLE_DOWN = "▼";
        //private static string TRIANGLE_UP = "▲";

        // Symbols by name, references types above

        public static string ALTITUDE_CLIMBING = ARROW_UP;
        public static string ALTITUDE_DESCENDING = ARROW_DOWN;
        public static string ALTITUDE_DEVIATION_ABOVE = PLUS;
        public static string ALTITUDE_DEVIATION_BELOW = MINUS;
        public static string ALTITUDE_LEVEL = SPACE;

        public static string ANNOTATION = AMPERSAND;
        public static string NO_ANNOTATION = DOT;

        public static string EQUIP_ADSB_CPDLC = ASTERISK_SPECIAL;
        public static string EQUIP_NO_ADSB_NO_CPDLC = BOXED_ASTERISK;
        public static string EQUIP_NO_ADSB_CPDLC = SQUARE_HOLLOW;

        public static string DOWNLINK_FLAG = TRIANGLE_DOWN;
        public static string DOWNLINK_NO_FLAG = SQUARE_HOLLOW;

        public static string HANDOFF = H;
        public static string HANDOFF_COMPLETED = O;

        public static string INHIBIT_FLAG = CARROT;
        public static string INHIBIT_NO_FLAG = SPACE;

        public static string JET = M;

        public static string LATERAL_SEPARATION_RNP4 = FOUR;
        public static string LATERAL_SEPARATION_RNP10 = R;
        public static string LATERAL_SEPARATION_NONE = SPACE;
        public static string LONGITUDINAL_RNP4 = THREE;
        public static string LONGITUDINAL_RNP10 = D;
        public static string LONGITUDINAL_TIME = SPACE;

        public static string LABEL_RADAR_CONTACT_FLAG = STAR_FILLED;
        public static string LABEL_NO_RADAR_CONTACT_FLAG = DOT;
        public static string RESTRICTION = LOWERCASE_X;
        public static string NO_RESTRICTION = SPACE;

        public static string RVSM = W;

        public static string SPACER = SPACE;
        public static string STRIP_RADAR_CONTACT = A;
    }
}
