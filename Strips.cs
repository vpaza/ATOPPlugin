using System;
using vatsys.Plugin;
using vatsys;

namespace ATOP
{
    internal class Strips
    {
        internal static CustomStripItem GetCustomStripItem(string itemType, Track track, FDP2.FDR flightDataRecord, RDP.RadarTrack radarTrack)
        {
            if (flightDataRecord == null) { return null; }

            var atopFPL = FlightPlan.GetFlightPlan(flightDataRecord.Callsign);

            CustomStripItem item;
            string s;

            switch (itemType)
            {
                case "ATOP_CALLSIGN":
                    item = new CustomStripItem()
                    {
                        Text = flightDataRecord.Callsign
                    };

                    if (atopFPL.IsEastbound)
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
                        Text = Shared.FindSectorID(flightDataRecord.ControllingSector?.Name) ?? null
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
                    if (atopFPL.HasCPDLC && atopFPL.HasADSB)
                        s = Symbols.EQUIP_ADSB_CPDLC;
                    else if (!atopFPL.HasADSB && atopFPL.HasCPDLC)
                        s = Symbols.EQUIP_NO_ADSB_CPDLC;
                    else
                        s = Symbols.EQUIP_NO_ADSB_NO_CPDLC;
                    item = new CustomStripItem()
                    {
                        Text = s
                    };

                    if (atopFPL.IsEastbound)
                    {
                        item.BackColourIdentity = Colours.Identities.StripText;
                        item.ForeColourIdentity = Colours.Identities.StripBackground;
                    }

                    return item;
                case "ATOP_SEPARATION_FLAG":
                    item = new CustomStripItem()
                    {
                        Text = Symbols.LONGITUDINAL_TIME,
                        BackColourIdentity = Colours.Identities.Custom,
                        CustomBackColour = Colors.HighlightFieldStrip
                    };
                    if (atopFPL.RNP == FlightPlan.RNPFlag.RNP4)
                    {
                        item.Text = Symbols.LONGITUDINAL_RNP4;
                    }
                    else if (atopFPL.RNP == FlightPlan.RNPFlag.RNP10)
                    {
                        item.Text = Symbols.LONGITUDINAL_RNP4;
                    }
                    else
                    {
                        item.BackColourIdentity = Colours.Identities.StripBackground;
                        item.CustomBackColour = null;
                    }
                    return item;
                case "ATOP_T10_FLAG":
                    if (atopFPL.IsJet)
                        return new CustomStripItem()
                        {
                            Text = Symbols.JET,
                            BackColourIdentity = Colours.Identities.Custom,
                            CustomBackColour = Colors.HighlightFieldStrip
                        };

                    return null;
                case "ATOP_MNT_FLAG":
                    if (atopFPL.IsJet)
                        return new CustomStripItem()
                        {
                            Text = Symbols.R,
                            BackColourIdentity = Colours.Identities.Custom,
                            CustomBackColour = Colors.HighlightFieldStrip
                        };

                    return null;
                case "ATOP_RVSM_FLAG":
                    if (flightDataRecord.RVSM)
                        return new CustomStripItem()
                        {
                            BackColourIdentity = Colours.Identities.Custom,
                            CustomBackColour = Colors.SeparationFlags,
                            Text = Symbols.RVSM
                        };

                    return null;
                case "ATOP_VERTICAL_MOVEMENT_IND":
                    switch (atopFPL.AltitudeFlag)
                    {
                        case FlightPlan.AltitudeFlags.Climbing:
                            s = Symbols.ALTITUDE_CLIMBING;
                            break;
                        case FlightPlan.AltitudeFlags.Descending:
                            s = Symbols.ALTITUDE_DESCENDING;
                            break;
                        default:
                            s = Symbols.ALTITUDE_LEVEL;
                            break;
                    }

                    return new CustomStripItem()
                    {
                        Text = s
                    };
                case "ATOP_ANNOTATION_IND":
                    item = new CustomStripItem(){};
                    if (String.IsNullOrEmpty(flightDataRecord.LabelOpData))
                    {
                        item.Text = Symbols.NO_ANNOTATION;
                    }
                    else
                    {
                        item.Text = Symbols.ANNOTATION;
                    }
                    return item;
                case "ATOP_LATERAL_FLAG":
                    if (atopFPL.RNP == FlightPlan.RNPFlag.RNP4)
                        s = Symbols.LATERAL_SEPARATION_RNP4;
                    else if (atopFPL.RNP == FlightPlan.RNPFlag.RNP10)
                        s = Symbols.LATERAL_SEPARATION_RNP10;
                    else
                        s = Symbols.LATERAL_SEPARATION_NONE;
                    return new CustomStripItem()
                    {
                        Text = s,
                        BackColourIdentity = (atopFPL.RNP != FlightPlan.RNPFlag.None) ? Colours.Identities.Custom : Colours.Identities.StripBackground,
                        CustomBackColour = (atopFPL.RNP != FlightPlan.RNPFlag.None) ? Colors.HighlightFieldStrip : null
                    };
                case "ATOP_ROUTE_OF_FLIGHT_IND":
                    return new CustomStripItem()
                    {
                        Text = "F",
                        OnMouseClick = ToggleRouteOfFlight, // vatSys doesn't call this... :(
                    };
                case "ATOP_RADAR_CONTACT_IND":
                    return new CustomStripItem()
                    {
                        Text = Symbols.STRIP_RADAR_CONTACT,
                        BackColourIdentity = (atopFPL.RadarContactFlag) ? Colours.Identities.Custom : Colours.Identities.StripBackground,
                        CustomBackColour = (atopFPL.RadarContactFlag) ? Colors.HighlightFieldStrip : null,
                        OnMouseClick = AircraftSituationDisplay.HandleRadarFlagClick // vatSys doesn't call this... :(
                    };
                case "ATOP_SPACER":
                    return new CustomStripItem()
                    {
                        Text = Symbols.SPACER
                    };
            }

            return null;
        }

        private static void ToggleRouteOfFlight(CustomLabelItemMouseClickEventArgs args)
        {
/*            if (AircraftSituationDisplay.graphicRouteShown.TryGetValue(args.Track.GetFDR().Callsign, out _))
            {
                AircraftSituationDisplay.graphicRouteShown.TryRemove(args.Track.GetFDR().Callsign, out _);
                MMI.HideGraphicRoute(args.Track);
            }
            else
            {
                AircraftSituationDisplay.graphicRouteShown.TryAdd(args.Track.GetFDR().Callsign, true);
                MMI.ShowGraphicRoute(args.Track);
            } */
        }
    }
}
