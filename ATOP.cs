using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Deployment.Internal;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vatsys;
using vatsys.Plugin;

namespace ATOP
{
    [Export(typeof(vatsys.Plugin.IPlugin))]
    public class ATOP : vatsys.Plugin.ILabelPlugin, vatsys.Plugin.IStripPlugin
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
                AircraftSituationDisplay.eastboundCallsigns.TryRemove(updated.Callsign, out _);
                AircraftSituationDisplay.adsbcpdlcValues.TryRemove(updated.Callsign, out _);
                AircraftSituationDisplay.adsflagValues.TryRemove(updated.Callsign, out _);
                AircraftSituationDisplay.mntflagValues.TryRemove(updated.Callsign, out _);
                AircraftSituationDisplay.altValues.TryRemove(updated.Callsign, out _);
                AircraftSituationDisplay.radartoggle.TryRemove(updated.Callsign, out _);
                AircraftSituationDisplay.mntflagtoggle.TryRemove(updated.Callsign, out _);
                AircraftSituationDisplay.downlink.TryRemove(updated.Callsign, out _);
                AircraftSituationDisplay.graphicRouteShown.TryRemove(updated.Callsign, out _);
                return;
            }

            AircraftSituationDisplay.adsbcpdlcValues.AddOrUpdate(
                updated.Callsign,
                Shared.GetCPDLCIndicator(updated),
                (k, v) => Shared.GetCPDLCIndicator(updated)
            );

            AircraftSituationDisplay.adsflagValues.AddOrUpdate(
                updated.Callsign,
                Shared.GetADSCFlag(updated),
                (k, v) => Shared.GetADSCFlag(updated)
            );

            AircraftSituationDisplay.mntflagValues.AddOrUpdate(
                updated.Callsign,
                Shared.GetMNTFlag(updated),
                (k, v) => Shared.GetMNTFlag(updated)
            );

            AircraftSituationDisplay.altValues.AddOrUpdate(
                updated.Callsign,
                Shared.GetAltFlag(updated),
                (k, v) => Shared.GetAltFlag(updated)
            );

            AircraftSituationDisplay.graphicRouteShown.AddOrUpdate(
                updated.Callsign,
                false,
                (k, v) => false
            );

            if (updated.ParsedRoute.Count > 1)
            {
                double trk = Conversions.CalculateTrack(
                    updated.ParsedRoute.First().Intersection.LatLong,
                    updated.ParsedRoute.Last().Intersection.LatLong
                );
                bool east = trk >= 0 && trk <= 180;
                AircraftSituationDisplay.eastboundCallsigns.AddOrUpdate(
                    updated.Callsign,
                    east,
                    (k, v) => east
                );
            }

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
            if (fdr == null && fdr.CoupledTrack != null) { return; }

            if (
                    MMI.GetSectorEntryTime(fdr) == null &&
                    MMI.SectorsControlled.ToList().TrueForAll(s => !s.IsInSector(fdr.GetLocation(), fdr.PRL))
                )
            {
                MMI.HandoffToNone(fdr);
                Thread.Sleep(300000);
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
}
