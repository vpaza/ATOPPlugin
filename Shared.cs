using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vatsys;
using vatsys.Plugin;

namespace ATOP
{
    internal class Shared
    {
        internal static char GetCPDLCIndicator(FDP2.FDR fdr)
        {
            bool cpdlc = Regex.IsMatch(fdr.AircraftEquip, @"J5") || Regex.IsMatch(fdr.AircraftEquip, @"J7");

            if (!fdr.ADSB && !cpdlc)
                return '⧆';

            if (!fdr.ADSB)
                return '⎕';

            return '✱';
        }

        internal static char GetADSCFlag(FDP2.FDR fdr)
        {
            Match pbn = Regex.Match(fdr.Remarks, @"PBN\/\w+\s");
            bool rnp10 = Regex.IsMatch(pbn.Value, @"A1");
            bool rnp4 = Regex.IsMatch(pbn.Value, @"L1");
            bool cpdlc = Regex.IsMatch(fdr.AircraftEquip, @"J5") || Regex.IsMatch(fdr.AircraftEquip, @"J7");
            bool adsc = Regex.IsMatch(fdr.AircraftSurvEquip, @"D1");

            // ATCTrainer and filing through vPilot don't allow filing Equipment suffixes, so we have to assume
            // that they're CPDLC and ADS-C capable if they're filed with RNP4 or RNP10
            if (rnp4)
                return '3';

            if (rnp10)
                return 'D';

            return ' ';
        }

        internal static char GetLateralFlag(FDP2.FDR fdr)
        {
            Match pbn = Regex.Match(fdr.Remarks, @"PBN\/\w+\s");
            bool rnp10 = Regex.IsMatch(pbn.Value, @"A1");
            bool rnp4 = Regex.IsMatch(pbn.Value, @"L1");
            bool cpdlc = Regex.IsMatch(fdr.AircraftEquip, @"J5") || Regex.IsMatch(fdr.AircraftEquip, @"J7");
            bool adsc = Regex.IsMatch(fdr.AircraftSurvEquip, @"D1");

            if (rnp4 || rnp10)
                return rnp4 ? '4' : 'R';

            return default;
        }

        internal static char GetMNTFlag(FDP2.FDR fdr)
        {
            if (fdr.PerformanceData?.IsJet ?? false)
                return 'M';

            return default;
        }

        internal static char GetAltFlag(FDP2.FDR fdr)
        {
            // If cleared flight level is -1, use the requested flight level
            int alt = (fdr.CFLUpper == -1) ? fdr.RFL : fdr.CFLUpper;

            // Pilot Reported Level is requested level or top of cleared flight level
            // then they're level
            if (fdr.PRL == alt)
                return ' ';

            // Issued or is trending climbing
            if (alt / 100 > fdr.PRL / 100 || fdr.PredictedPosition.VerticalSpeed > 300)
                return '↑';

            // Issued or is trending descending
            if (alt / 100 < fdr.PRL / 100 || fdr.PredictedPosition.VerticalSpeed < -300)
                return '↓';

            // Deviated above
            if (fdr.PRL - alt / 100 >= 3) 
                return '+';

            // Deviated below
            if (fdr.PRL - alt / 100 <= -3)
                return '-';

            // Assume level
            return ' ';
        }


        internal static CustomColour GetDirectionColor(string callsign)
        {
            bool east;
            if (AircraftSituationDisplay.eastboundCallsigns.TryGetValue(callsign, out east))
            {
                if (east)
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
