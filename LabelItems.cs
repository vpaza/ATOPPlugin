using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vatsys;
using vatsys.Plugin;
using static vatsys.FDP2;

namespace ATOP
{
    internal class LabelItems
    {
        public CustomLabelItem GetCustomLabelItem(string itemType, Track track, FDP2.FDR flightDataRecord, RDP.RadarTrack radarTrack)
        {
            if (flightDataRecord == null) return null;

            char c;
            string s = "";
            CustomLabelItem item;

            switch(itemType)
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
                    var b = AircraftSituationDisplay.GetValue<string, bool>(AircraftSituationDisplay.downlink, flightDataRecord.Callsign);
                    if (b)
                        return new CustomLabelItem()
                        {
                            Text = "▼",
                            Border = BorderFlags.All,
                        };
                    else
                        return new CustomLabelItem()
                        {
                            Text = "⬜",
                        };
                case "ATOP_ACID": // field B len 7
                    return new CustomLabelItem()
                    {
                        Text = flightDataRecord.Callsign,
                        ForeColourIdentity = Colours.Identities.Custom,
                        CustomForeColour = Shared.GetDirectionColor(flightDataRecord.Callsign)
                    };
                case "ATOP_ADSB_CPDLC": // field C len 4
                    c = AircraftSituationDisplay.GetValue<string, char>(AircraftSituationDisplay.adsbcpdlcValues, flightDataRecord.Callsign);

                    item = new CustomLabelItem()
                    {
                        Text = AircraftSituationDisplay.GetValue<string, char>(AircraftSituationDisplay.adsbcpdlcValues, flightDataRecord.Callsign).ToString()
                    };

                    if (flightDataRecord.State == FDP2.FDR.FDRStates.STATE_PREACTIVE ||
                        flightDataRecord.State == FDP2.FDR.FDRStates.STATE_COORDINATED)
                    {
                        item.ForeColourIdentity = Colours.Identities.Custom;
                        item.CustomForeColour = Colors.NotCurrentDataAuthority;
                    }

                    return item;
                case "ATOP_ADS_FLAGS": // field C len 4
                    return new CustomLabelItem()
                    {
                        Text = AircraftSituationDisplay.GetValue<string, char>(AircraftSituationDisplay.adsflagValues, flightDataRecord.Callsign).ToString(),
                    };
                case "ATOP_MNT_FLAGS": // field C len 4
                    s = "";
                    if (flightDataRecord.PerformanceData?.IsJet ?? false)
                        s = "M";

                    return new CustomLabelItem()
                    {
                        Text = s,
                    };
                case "ATOP_ANNOTATION_IND": // field E len 1
                    if (String.IsNullOrEmpty(flightDataRecord.LabelOpData))
                    {
                        c = '◦';
                    }
                    else
                    {
                        c = '&';
                    }

                    return new CustomLabelItem()
                    {
                        Text = c.ToString()
                    };
                case "ATOP_RESTRICTION_IND": // field F len 1
                    c = ' ';
                    if (
                            flightDataRecord.LabelOpData.Contains("AT") ||
                            flightDataRecord.LabelOpData.Contains("BY") ||
                            flightDataRecord.LabelOpData.Contains("CLEARED TO")
                        )
                    {
                        c = 'x';
                    }

                    return new CustomLabelItem()
                    {
                        Text = c.ToString()
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
                    if (AircraftSituationDisplay.altValues == null) return null;
                    if (flightDataRecord.Callsign == null) return null;
                    if (AircraftSituationDisplay.altValues.TryGetValue(flightDataRecord.Callsign, out c))
                    {
                        s = c.ToString();
                    } else { s = " "; }
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
                    if (radarTrack != null && radarTrack.ReachedCFL)
                    {
                        return new CustomLabelItem()
                        {
                            Text = " ",
                        };
                    }

                    var cfl = flightDataRecord?.CFLUpper ?? -1;
                    if (cfl == -1)
                        cfl = flightDataRecord?.CFLLower ?? -1;
                    if (cfl == -1)
                        cfl = flightDataRecord?.RFL ?? -1;
                    var prl = flightDataRecord?.PRL ?? -1;

                    if (prl == cfl)
                    {
                        return new CustomLabelItem()
                        {
                            Text = " ",
                        };
                    }

                    if (flightDataRecord?.CFLLower != -1 && flightDataRecord?.CFLUpper != -1)
                    {
                        item = new CustomLabelItem()
                        {
                            Text = (
                                (flightDataRecord.CFLLower / 100).ToString("000") + "B" +
                                (flightDataRecord.CFLUpper / 100).ToString("000")
                            )
                        };
                    }
                    else
                    {
                        item = new CustomLabelItem()
                        {
                            Text = (cfl / 100).ToString(),
                        };
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

                    return item;
                case "ATOP_HANDOFF_IND": // field J len 4
                    if (flightDataRecord.IsHandoff)
                    {
                        s = "H";
                        if (MMI.SectorsControlled.Contains(flightDataRecord.HandoffSector))
                            s += Shared.FindSectorID(flightDataRecord.ControllingSector.Name); // To us, so show where it's coming from
                        else
                            s += Shared.FindSectorID(flightDataRecord.HandoffSector.Name);
                    }
                    else if (flightDataRecord.State == FDP2.FDR.FDRStates.STATE_HANDOVER_FIRST)
                    {
                        s = "O" + Shared.FindSectorID(flightDataRecord.ControllingSector.Name);
                    }
                    
                    return new CustomLabelItem()
                    {
                        Text = s,
                    };
                case "ATOP_RADAR_CONTACT_IND": // Field K len 1
                    s = " ";
                    if (AircraftSituationDisplay.GetValue<string, bool>(AircraftSituationDisplay.radartoggle, flightDataRecord.Callsign))
                    {
                        s = "★";
                    }
                    else if (radarTrack != null && (
                        radarTrack.RadarTypes.HasFlag(RDP.RadarTypes.SSR_ModeC) ||
                        radarTrack.RadarTypes.HasFlag(RDP.RadarTypes.SSR_ModeS)
                    )) {
                        s = "☆";
                    }
                    else
                    {
                        s = "◦";
                    }
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
                            Text = "^",
                        };
                    }
                    else
                    {
                        return new CustomLabelItem()
                        {
                            Text = " ",
                        };
                    }
                case "ATOP_FILED_SPEED": // Field m len 4
                    // @TODO If <FL290, we should display TAS
                    var mach = Conversions.CalculateMach(flightDataRecord.TAS, GRIB.FindTemperature(flightDataRecord.PRL, track.GetLocation(), true));
                    return new CustomLabelItem()
                    {
                        Text = "M" + Convert.ToDecimal(mach).ToString("F2").Replace(".", ""),
                    };
                case "ATOP_GROUND_SPEED": // Field n len 5
                    var gs = (radarTrack == null ? flightDataRecord.PredictedPosition.Groundspeed : radarTrack.GroundSpeed);
                    return new CustomLabelItem()
                    {
                        Text = "N" + gs.ToString("000"), // Zero pad, but can expand to 4 digits where needed
                    };
            }

            return null;
        }
    }
}
