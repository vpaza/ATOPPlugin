using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using vatsys;

namespace ATOP
{
    internal class FlightPlan
    {
        public bool IsEastbound { get; set; }
        public bool HasCPDLC { get; set; }
        public bool HasADSB { get; set; }
        public RNPFlag RNP { get; set; }
        public bool IsJet { get; set; }
        public bool HasDownlinkFlag { get; set; }
        public bool RadarContactFlag { get; set; }
        public CFLAltitude ClearedFlightLevel { get; set; }
        public int ReportedFlightLevel { get; set; }
        public AltitudeFlags AltitudeFlag { get; set; }

        public static readonly ConcurrentDictionary<string, FlightPlan> flightPlans = new ConcurrentDictionary<string, FlightPlan>();

        public enum RNPFlag
        {
            RNP4 = 0,
            RNP10,
            None
        }

        public enum AltitudeFlags
        {
            DeviationAbove = 0,
            DeviationBelow,
            Climbing,
            Descending,
            Level,
        }

        public static FlightPlan GetFlightPlan(string callsign)
        {
            FlightPlan flightPlan;
            if (flightPlans.TryGetValue(callsign, out flightPlan))
            {
                return flightPlan;
            }

            return null;
        }

        public static FlightPlan AddOrUpdate(string callsign, FDP2.FDR fdr)
        {
            if (fdr == null) { return null; }

            FlightPlan flightPlan = GetFlightPlan(callsign);

            if (flightPlan == null)
            {
                flightPlan = new FlightPlan();
            }

            flightPlan.SetEquipmentFlags(callsign, fdr);
            flightPlan.SetJetFlags(callsign, fdr);
            flightPlan.SetAltitudes(callsign, fdr);
            flightPlan.SetDirection(callsign, fdr);

            flightPlans.AddOrUpdate(callsign, flightPlan, (k, v) => flightPlan);

            return flightPlan;

        }

        public static void AddOrUpdate(string callsign, FlightPlan fdr)
        {
            flightPlans.AddOrUpdate(callsign, fdr, (k, v) => fdr);
        }

        public static void Remove(string callsign)
        {
            flightPlans.TryRemove(callsign, out _);
        }

        private void SetEquipmentFlags(string callsign, FDP2.FDR fdr)
        {
            Match pbn = Regex.Match(fdr.Remarks, @"PBN\/\w+\s");
            bool rnp10 = Regex.IsMatch(pbn.Value, @"A1");
            bool rnp4 = Regex.IsMatch(pbn.Value, @"L1");
            // CPDLC and ADSC are not used due to inconsitences in the data of pilot client vs prefiled
            // FPL data. Given CPDLC is not implemented on VATSIM and we use DMs to simulate, all aircraft
            // are CPDLC capable. Given that ADS-C is also not implemented, we can assume all aircraft
            // have it.
            bool cpdlc = Regex.IsMatch(fdr.AircraftEquip, @"J5") || Regex.IsMatch(fdr.AircraftEquip, @"J7");
            bool adsc = Regex.IsMatch(fdr.AircraftSurvEquip, @"D1");

            if (rnp4) RNP = RNPFlag.RNP4;
            else if (rnp10) RNP = RNPFlag.RNP10;
            else RNP = RNPFlag.None;

            HasCPDLC = true; // See VATSIM-ism comment note above
            HasADSB = true; // See VATSIM-ism comment note above
            /*
            if (cpdlc) HasCPDLC = true;
            else HasCPDLC = false;

            if (adsc) HasADSB = true;
            else HasADSB = false;
            */
        }

        private void SetJetFlags(string callsign, FDP2.FDR fdr)
        {
            if (fdr.PerformanceData?.IsJet ?? false)
                IsJet = true;
            else
                IsJet = false;
        }

        private void SetAltitudes(string callsign, FDP2.FDR fdr)
        {
            int alt = (fdr.CFLUpper == -1) ? fdr.RFL : fdr.CFLUpper;

            CFLAltitude cfl = new CFLAltitude()
            {
                Upper = alt,
                Lower = (fdr.CFLLower == -1) ? 0 : fdr.CFLLower
            };
            ClearedFlightLevel = cfl;
            ReportedFlightLevel = (fdr.PRL / 100);

            // Issued or trending climbing
            if ((alt / 100) > ReportedFlightLevel || fdr.PredictedPosition.VerticalSpeed > 300)
            {
                AltitudeFlag = AltitudeFlags.Climbing;
            }
            else if ((alt / 100) < ReportedFlightLevel || fdr.PredictedPosition.VerticalSpeed < -300)
            {
                AltitudeFlag = AltitudeFlags.Descending;
            }
            else if (ReportedFlightLevel - alt / 100 >= 3)
            {
                AltitudeFlag = AltitudeFlags.DeviationAbove;
            }
            else if (ReportedFlightLevel - alt / 100 <= -3)
            {
                AltitudeFlag = AltitudeFlags.DeviationBelow;
            }
            else
            {
                AltitudeFlag = AltitudeFlags.Level;
            }
        }

        private void SetDirection(string callsign, FDP2.FDR fdr)
        {
            if (fdr == null) { return; }

            double trk;

            // No FPL aircraft should be treated by their track heading if coupled, otherwise
            // go with default white
            if (fdr.ParsedRoute.Count < 2)
            {
                if (fdr.CoupledTrack == null)
                {
                    IsEastbound = false;
                    return;
                }
                trk = fdr.CoupledTrack.Heading;
            }
            else
            {
                // This feels dirty... but we do have aircraft that go the long way around so incorrectly
                // get tagged as westbound when they are actually eastbound.
                double sumtrks = 0;
                foreach (var route in fdr.ParsedRoute)
                {
                    sumtrks += route.Track;
                };
                trk = sumtrks / fdr.ParsedRoute.Count;
            }

            // 360-179 = Eastbound
            // 180-359 = Westbound
            if (trk >= 0 && trk < 180)
                IsEastbound = true;
            else
                IsEastbound = false;
        }
    }

    internal class CFLAltitude
    {
        public int Upper { get; set; }
        public int Lower { get; set; }
    }
}
