using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vatsys.Plugin;
using vatsys;
using static System.Net.Mime.MediaTypeNames;

namespace ATOP
{
    internal class Strips
    {
        internal static CustomStripItem GetCustomStripItem(string itemType, Track track, FDP2.FDR flightDataRecord, RDP.RadarTrack radarTrack)
        {
            if (flightDataRecord == null) { return null; }

            CustomStripItem item;
            string s;

            switch (itemType)
            {
                case "ATOP_CALLSIGN":
                    item = new CustomStripItem()
                    {
                        Text = flightDataRecord.Callsign
                    };

                    if (AircraftSituationDisplay.GetValue<string, bool>(AircraftSituationDisplay.eastboundCallsigns, flightDataRecord.Callsign))
                    {
                        item.BackColourIdentity = Colours.Identities.StripText;
                        item.ForeColourIdentity = Colours.Identities.StripBackground;
                    }
                    else
                    {
                        item.BackColourIdentity = Colours.Identities.StripBackground;
                        item.ForeColourIdentity = Colours.Identities.StripText;
                    }

                    return item;
                case "ATOP_CURRENT_DATA_AUTHORITY":
                    item = new CustomStripItem()
                    {
                        Text = flightDataRecord.ControllingSector?.Name ?? null
                    };

                    if (
                        flightDataRecord.State == FDP2.FDR.FDRStates.STATE_PREACTIVE ||
                        flightDataRecord.State == FDP2.FDR.FDRStates.STATE_COORDINATED
                        )
                    {
                        item.ForeColourIdentity = Colours.Identities.Custom;
                        item.CustomForeColour = Colors.Pending;
                    }

                    return item;
                case "ATOP_ADSB_CPDLC":
                    item = new CustomStripItem()
                    {
                        Text = Shared.GetCPDLCIndicator(flightDataRecord).ToString()
                    };

                    if (AircraftSituationDisplay.GetValue<string, bool>(AircraftSituationDisplay.eastboundCallsigns, flightDataRecord.Callsign))
                    {
                        item.BackColourIdentity = Colours.Identities.StripText;
                        item.ForeColourIdentity = Colours.Identities.StripBackground;
                    }

                    return item;
                case "ATOP_SEPARATION_FLAG":
                    return new CustomStripItem()
                    {
                        Text = Shared.GetADSCFlag(flightDataRecord).ToString()
                    };
                case "ATOP_T10_FLAG":
                    if (flightDataRecord.PerformanceData?.IsJet ?? false)
                        return new CustomStripItem()
                        {
                            Text = "M"
                        };

                    return null;
                case "ATOP_MNT_FLAG":
                    if (flightDataRecord.PerformanceData?.IsJet ?? false)
                        return new CustomStripItem()
                        {
                            Text = "R"
                        };

                    return null;
                case "ATOP_RVSM_FLAG":
                    if (flightDataRecord.RVSM)
                        return new CustomStripItem()
                        {
                            BackColourIdentity = Colours.Identities.Custom,
                            CustomBackColour = Colors.SeparationFlags,
                            Text = "W"
                        };

                    return null;
                case "ATOP_VERTICAL_MOVEMENT_IND":
                    double vertical_speed = radarTrack?.VerticalSpeed ?? flightDataRecord.PredictedPosition.VerticalSpeed;
                    double level = (radarTrack?.CorrectedAltitude ?? flightDataRecord.PredictedPosition.Altitude) / 100;
                    int clearedFlightLevel;
                    Int32.TryParse(flightDataRecord.CFLString, out clearedFlightLevel);

                    s = " ";
                    if (level == clearedFlightLevel || level == flightDataRecord.RFL)
                        s = " ";
                    else if (clearedFlightLevel > level && track.NewCFL || vertical_speed > 300)
                        s = "↑";
                    else if (clearedFlightLevel > 0 && clearedFlightLevel < level && track.NewCFL || vertical_speed < -300)
                        s = "↓";

                    return new CustomStripItem()
                    {
                        Text = s
                    };
                case "ATOP_ANNOTATION_IND":
                    item = new CustomStripItem(){};
                    if (String.IsNullOrEmpty(flightDataRecord.LabelOpData))
                    {
                        item.Text = "◦";
                    }
                    else
                    {
                        item.Text = "&";
                    }
                    return item;
                case "ATOP_LATERAL_FLAG":
                    return new CustomStripItem()
                    {
                        Text = Shared.GetLateralFlag(flightDataRecord).ToString()
                    };
                case "ATOP_ROUTE_OF_FLIGHT_IND":
                    return new CustomStripItem()
                    {
                        Text = "F",
                        OnMouseClick = ToggleRouteOfFlight,
                    };
                case "ATOP_RADAR_CONTACT_IND":
                    if (!AircraftSituationDisplay.radartoggle.TryGetValue(flightDataRecord.Callsign, out _))
                    {
                        return new CustomStripItem()
                        {
                            Text = "A",
                            BackColourIdentity = Colours.Identities.StripBackground,
                            OnMouseClick = AircraftSituationDisplay.HandleRadarFlagClick
                        };
                    }
                    else
                    {
                        return new CustomStripItem()
                        {
                            Text = "A",
                            BackColourIdentity = Colours.Identities.Custom,
                            CustomBackColour = Colors.LightBlue,
                            OnMouseClick = AircraftSituationDisplay.HandleRadarFlagClick
                        };
                    }
                case "ATOP_SPACER":
                    return new CustomStripItem()
                    {
                        Text = " "
                    };
            }

            return null;
        }

        private static void ToggleRouteOfFlight(CustomLabelItemMouseClickEventArgs args)
        {
            if (AircraftSituationDisplay.graphicRouteShown.TryGetValue(args.Track.GetFDR().Callsign, out _))
            {
                AircraftSituationDisplay.graphicRouteShown.TryRemove(args.Track.GetFDR().Callsign, out _);
                MMI.HideGraphicRoute(args.Track);
            }
            else
            {
                AircraftSituationDisplay.graphicRouteShown.TryAdd(args.Track.GetFDR().Callsign, true);
                MMI.ShowGraphicRoute(args.Track);
            }
        }
    }
}
