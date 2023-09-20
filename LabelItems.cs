using System;
using vatsys;
using vatsys.Plugin;

namespace ATOP
{
    internal class LabelItems
    {
        public CustomLabelItem GetCustomLabelItem(string itemType, Track track, FDP2.FDR flightDataRecord, RDP.RadarTrack radarTrack)
        {
            if (flightDataRecord == null) return null;

            FlightPlan atopFDR = FlightPlan.GetFlightPlan(flightDataRecord.Callsign);

            string s = "";
            CustomLabelItem item;

            switch (itemType)
            {
                case "SELECT_HORI":
                    if (MMI.SelectedTrack?.GetFDR()?.Callsign == flightDataRecord?.Callsign)
                        return new CustomLabelItem()
                        {
                            Border = BorderFlags.Top
                        };
                    else
                        return new CustomLabelItem()
                        {
                            Border = BorderFlags.None
                        };
                case "SELECT_VERT":
                    if (MMI.SelectedTrack?.GetFDR()?.Callsign == flightDataRecord?.Callsign)
                        return new CustomLabelItem()
                        {
                            Border = BorderFlags.Left
                        };
                    else
                        return new CustomLabelItem()
                        {
                            Border = BorderFlags.None
                        };
                case "ATOP_COMM_ICON": // field A len 2
                    if (atopFDR.HasDownlinkFlag)
                        return new CustomLabelItem()
                        {
                            Text = Symbols.DOWNLINK_FLAG,
                            Border = BorderFlags.All,
                            OnMouseClick = AircraftSituationDisplay.HandleDownlinkClick,
                        };
                    else
                        return new CustomLabelItem()
                        {
                            Text = Symbols.DOWNLINK_NO_FLAG,
                        };
                case "ATOP_ACID": // field B len 7
                    return new CustomLabelItem()
                    {
                        Text = flightDataRecord.Callsign,
                    };
                case "ATOP_ADSB_CPDLC": // field C len 4
                    if (atopFDR.HasCPDLC && atopFDR.HasADSB)
                        s = Symbols.EQUIP_ADSB_CPDLC;
                    else if (!atopFDR.HasADSB && atopFDR.HasCPDLC)
                        s = Symbols.EQUIP_NO_ADSB_CPDLC;
                    else
                        s = Symbols.EQUIP_NO_ADSB_NO_CPDLC;

                    item = new CustomLabelItem()
                    {
                        Text = s
                    };

                    if (flightDataRecord.State == FDP2.FDR.FDRStates.STATE_PREACTIVE ||
                        flightDataRecord.State == FDP2.FDR.FDRStates.STATE_COORDINATED)
                    {
                        item.ForeColourIdentity = Colours.Identities.Custom;
                        item.CustomForeColour = Colors.NotCurrentDataAuthority;
                    }

                    return item;
                case "ATOP_ADS_FLAGS": // field C len 4
                    s = Symbols.LONGITUDINAL_TIME;
                    if (atopFDR.RNP == FlightPlan.RNPFlag.RNP4)
                        s = Symbols.LONGITUDINAL_RNP4;
                    else if (atopFDR.RNP == FlightPlan.RNPFlag.RNP10)
                        s = Symbols.LONGITUDINAL_RNP10;
                    return new CustomLabelItem()
                    {
                        Text = s
                    };
                case "ATOP_MNT_FLAGS": // field C len 4
                    s = Symbols.SPACER;
                    if (atopFDR.IsJet)
                        s = Symbols.JET;

                    return new CustomLabelItem()
                    {
                        Text = s,
                    };
                case "ATOP_ANNOTATION_IND": // field E len 1
                    s = Symbols.NO_ANNOTATION;
                    if (String.IsNullOrEmpty(flightDataRecord.LabelOpData))
                        s = Symbols.NO_ANNOTATION;

                    return new CustomLabelItem()
                    {
                        Text = s
                    };
                case "ATOP_RESTRICTION_IND": // field F len 1
                    s = Symbols.NO_RESTRICTION;
                    if (
                            flightDataRecord.LabelOpData.Contains("AT") ||
                            flightDataRecord.LabelOpData.Contains("BY") ||
                            flightDataRecord.LabelOpData.Contains("CLEARED TO")
                        )
                    {
                        s = Symbols.RESTRICTION;
                    }

                    return new CustomLabelItem()
                    {
                        Text = s
                    };
                case "ATOP_REPORTED_LEVEL": // field G len 3
                    int level = (radarTrack == null ? flightDataRecord.PRL : radarTrack.CorrectedAltitude) / 100;

                    item = new CustomLabelItem()
                    {
                        Text = level.ToString("000"),
                    };

                    if (!flightDataRecord.RVSM)
                    {
                        item.ForeColourIdentity = Colours.Identities.Custom;
                        item.CustomForeColour = Colors.NotReducedVerticalSeparationMinima;
                    }

                    return item;
                case "ATOP_VERTICAL_MOVEMENT_IND": // Field H len 1
                    switch (atopFDR.AltitudeFlag)
                    {
                        case FlightPlan.AltitudeFlags.Climbing:
                            s = Symbols.ALTITUDE_CLIMBING;
                            break;
                        case FlightPlan.AltitudeFlags.Descending:
                            s = Symbols.ALTITUDE_DESCENDING;
                            break;
                        case FlightPlan.AltitudeFlags.DeviationAbove:
                            s = Symbols.ALTITUDE_DEVIATION_ABOVE;
                            break;
                        case FlightPlan.AltitudeFlags.DeviationBelow:
                            s = Symbols.ALTITUDE_DEVIATION_BELOW;
                            break;
                        case FlightPlan.AltitudeFlags.Level:
                        default:
                            s = Symbols.ALTITUDE_LEVEL;
                            break;
                    }

                    item = new CustomLabelItem()
                    {
                        Text = s,
                    };
                    if (!flightDataRecord.RVSM)
                    {
                        item.ForeColourIdentity = Colours.Identities.Custom;
                        item.CustomForeColour = Colors.NotReducedVerticalSeparationMinima;
                    }

                    return item;
                case "ATOP_CLEARED_LEVEL": // field I len 7
                    int prl = flightDataRecord.PRL / 100;
                    int alt = (flightDataRecord.CFLUpper == -1) ? flightDataRecord.RFL : flightDataRecord.CFLUpper;
                    item = new CustomLabelItem(){};
                    // If they've reached CFL or PRL minus alt is less than 300 feet
                    if (radarTrack != null && radarTrack.ReachedCFL || prl == alt || Math.Abs(prl - alt) < 3)
                    {
                        item.Text = Symbols.SPACER;
                    }
                    else if (flightDataRecord?.CFLLower != -1 && flightDataRecord?.CFLUpper != -1)
                    {
                        item.Text = (
                                (flightDataRecord.CFLLower / 100).ToString("000") + "B" +
                                (flightDataRecord.CFLUpper / 100).ToString("000")
                            );
                    }
                    else
                    {
                        item.Text = (((flightDataRecord.CFLUpper == -1) ? flightDataRecord.RFL : flightDataRecord.CFLUpper) / 100).ToString();
                    }

                    if (flightDataRecord.State == FDP2.FDR.FDRStates.STATE_PREACTIVE || flightDataRecord.State == FDP2.FDR.FDRStates.STATE_COORDINATED)
                    {
                        item.Border = BorderFlags.All;
                        item.BorderColourIdentity = Colours.Identities.Custom;
                        item.CustomBorderColour = Colors.NotCurrentDataAuthority;
                    }

                    if (!flightDataRecord.RVSM)
                    {
                        item.ForeColourIdentity = Colours.Identities.Custom;
                        item.CustomForeColour = Colors.NotReducedVerticalSeparationMinima;
                    }

                    if (item.Text.Length < 7)
                    {
                        item.Text = item.Text.PadRight(7, ' ');
                    }

                    return item;
                case "ATOP_HANDOFF_IND": // field J len 4
                    if (flightDataRecord.IsHandoff)
                    {
                        s = Symbols.HANDOFF;
                        if (MMI.SectorsControlled.Contains(flightDataRecord.HandoffSector))
                            s += Shared.FindSectorID(flightDataRecord.ControllingSector.Name); // To us, so show where it's coming from
                        else
                            s += Shared.FindSectorID(flightDataRecord.HandoffSector.Name);
                    }
                    else if (flightDataRecord.State == FDP2.FDR.FDRStates.STATE_HANDOVER_FIRST)
                    {
                        s = Symbols.HANDOFF_COMPLETED + Shared.FindSectorID(flightDataRecord.ControllingSector.Name);
                    }

                    return new CustomLabelItem()
                    {
                        Text = s,
                    };
                case "ATOP_RADAR_CONTACT_IND": // Field K len 1
                    s = Symbols.LABEL_NO_RADAR_CONTACT_FLAG;
                    if (atopFDR.RadarContactFlag)
                        s = Symbols.LABEL_RADAR_CONTACT_FLAG;
                    return new CustomLabelItem()
                    {
                        Text = s,
                        OnMouseClick = AircraftSituationDisplay.HandleRadarFlagClick,
                    };
                case "ATOP_INHIBIT_IND": // Field L len 1
                    if (flightDataRecord.State == FDP2.FDR.FDRStates.STATE_INHIBITED)
                    {
                        return new CustomLabelItem()
                        {
                            Text = Symbols.INHIBIT_FLAG,
                        };
                    }
                    else
                    {
                        return new CustomLabelItem()
                        {
                            Text = Symbols.INHIBIT_NO_FLAG,
                        };
                    }
                case "ATOP_FILED_SPEED": // Field m len 4
                    if (flightDataRecord.PRL < 29000)
                    {
                        s = "N" + flightDataRecord.TAS.ToString("000");
                    }
                    else
                    {
                        s = "M" + Conversions.CalculateMach(flightDataRecord.TAS, GRIB.FindTemperature(flightDataRecord.PRL, track.GetLocation(), true)).ToString("F2").Replace(".", "");
                    }
                    return new CustomLabelItem()
                    {
                        Text = s,
                    };
                case "ATOP_GROUND_SPEED": // Field n len 5
                    var gs = (radarTrack == null ? flightDataRecord.PredictedPosition.Groundspeed : radarTrack.GroundSpeed);
                    return new CustomLabelItem()
                    {
                        Text = "N" + gs.ToString("000").PadRight(4, ' '), // Zero pad, but can expand to 4 digits where needed
                    };
            }

            return null;
        }
    }
}
