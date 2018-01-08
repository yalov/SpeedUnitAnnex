using System;
using UnityEngine;
using KSP.UI.Screens.Flight;
using KSP.Localization;

namespace SpeedUnitDisplay
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SpeedUnitDash : MonoBehaviour
    {

        private SpeedDisplay display;
        private float fontSize;
        private float titleFontSize;
        private float mph_ms = 2.23694f;
        private float kmph_ms = 3.6f;
        private float knToms = 0.514f;

        private string kn = " " + Localizer.Format("#SpeedUnitDisplay_knot");
        private string kmph = " " + Localizer.Format("#SpeedUnitDisplay_kmph");
        private string mph = " " + Localizer.Format("#SpeedUnitDisplay_mph");

        private string Surface = Localizer.Format("#autoLOC_7001218") + " ";
        private string Orbit = Localizer.Format("#autoLOC_7001217") + " ";

        private string Surf3 = Localizer.Format("#SpeedUnitDisplay_Surf3") + " ";
        private string Surf5 = Localizer.Format("#SpeedUnitDisplay_Surf5") + " "; 

        private string m = Localizer.Format("#SpeedUnitDisplay_meter");
        private string k = Localizer.Format("#SpeedUnitDisplay_kilo");
        private string M = Localizer.Format("#SpeedUnitDisplay_mega");
        private string G = Localizer.Format("#SpeedUnitDisplay_giga");
        private string T = Localizer.Format("#SpeedUnitDisplay_tera");

        private string Mm = Localizer.Format("#SpeedUnitDisplay_mega") + Localizer.Format("#SpeedUnitDisplay_meter");
        private string Gm = Localizer.Format("#SpeedUnitDisplay_giga") + Localizer.Format("#SpeedUnitDisplay_meter");




        //private System.Globalization.CultureInfo culture =
        //    System.Globalization.CultureInfo.CreateSpecificCulture(KSP.Localization.Localizer.CurrentLanguage);

        public SpeedUnitDash()
        {
            // Nothing to be done here
        }

        public void OnGUI()
        {
            if (display == null)
            {
                display = GameObject.FindObjectOfType<SpeedDisplay>();
                if (display != null)
                {
                    fontSize = display.textSpeed.fontSize;
                    titleFontSize = display.textTitle.fontSize;
                    display.textTitle.fontSize = titleFontSize / 1.15f;
                    
                }
            }
        }


        private string Truncate(double value, string mode, int digits )
        {
            double multiplier = Math.Pow(10, digits);
            return (Math.Truncate(multiplier * value) / multiplier).ToString(mode + digits);
        }

        private string Unitize_short(double value)
        {
            string str;
            double v = Math.Abs(value);

            if (v < 1000)
            {
                if (v < 100) str = Truncate(value,"F",1) + m;
                else str = Truncate(value, "F", 1) + m;
            }
            else if (v < 1000E3)
            {
                if (v < 100E3) str = Truncate(value / 1E3, "F", 2) + k;
                else str = Truncate(value / 1E3, "F", 1) + k;
            }
            else if (v < 1000E6)
            {
                if (v < 100E6) str = Truncate(value / 1E6, "F", 2) + M;
                else str = Truncate(value / 1E6, "F", 1) + M;
            }
            else if (v < 1000E9)
            {
                if (v < 100E9) str = Truncate(value / 1E9, "F", 2) + G;
                else str = Truncate(value / 1E9, "F", 1) + G;
            }
            else
            {
                if (v < 100E12) str = Truncate(value / 1E12, "F", 2) + T;
                else str = Truncate(value / 1E12, "F", 1) + T;
            }
            return str;
        }

        private string Unitize_long(double value)
        {
            string str;
            double v = Math.Abs(value);
            //  KerbATM 70kм   Minmus 47M              Moho 4G  Eeloo 113G  Plock 700G      
            //                             Moon 400M   Mercury 60G                       Pluto 7300G


            if (v < 1E8)             // 1 m - 99,999,999 m
            {
                str = Truncate(value, "N", 0) + " " + m;
            }
            else if (v < 1E12)       // 100.0 Mm - 999,999.9 Mm
            {
                str = Truncate(value / 1E6, "N", 1) + " " + Mm;
            }
            else                     // 1,000.0 Gm - 999,999.9 Gm and beyond
            {
                str = Truncate(value / 1E6, "N", 1) + " " + Gm;
            }
            return str;
        }

        public void LateUpdate()
        {
            if (display != null)
            {
                FlightGlobals.SpeedDisplayModes mode = FlightGlobals.speedDisplayMode;
                Vessel.Situations situation = FlightGlobals.ActiveVessel.situation;
                VesselType vesselType = FlightGlobals.ActiveVessel.vesselType;
                
                switch (mode)
                {
                    case FlightGlobals.SpeedDisplayModes.Surface:
                    {
                        double spd = FlightGlobals.ActiveVessel.srfSpeed;
                        string titleText;

                        // Boat (or SPLASHED)
                        if (situation == Vessel.Situations.SPLASHED)
                        {
                            titleText = Surf5 + (spd * knToms).ToString("F1") + kn;
                        }
                        // Plane (not LANDED)
                        else if (vesselType == VesselType.Plane
                            && situation != Vessel.Situations.LANDED && situation != Vessel.Situations.PRELAUNCH)
                        {
                            bool isradar = HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitDashSettings>().radar;
                            bool isATM = FlightGlobals.ActiveVessel.atmDensity > 0.0;

                            if (isradar)
                            {
                                if (isATM)
                                    titleText = Surf3 + Unitize_short(FlightGlobals.ActiveVessel.radarAltitude) + "  " 
                                        + Localizer.Format("#SpeedUnitDisplay_mach", FlightGlobals.ActiveVessel.mach.ToString("F1"));

                                else  titleText = Surf5 + Unitize_long(FlightGlobals.ActiveVessel.radarAltitude);
                            }
                            else
                            {
                                if (isATM)
                                    titleText = Surf5 + Localizer.Format("#SpeedUnitDisplay_mach", FlightGlobals.ActiveVessel.mach.ToString("F1"));
                                else  titleText = Surface;
                            }
                        }
                        // Rover or LANDED Plane
                        else if ((vesselType == VesselType.Rover || vesselType == VesselType.Plane) 
                                && FlightGlobals.ActiveVessel.radarAltitude < 1000)
                        {
                            if (HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitDashSettings>().kph)
                                titleText = Surf5 + (spd * kmph_ms).ToString("F1") + kmph;
                            else
                                titleText = Surf5 + (spd * mph_ms).ToString("F1") + mph;
                        }
                        // Other: Rocket, Lander, etc 
                        else
                        {
                            if (HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitDashSettings>().radar)
                            {
                                titleText = Surf5 + Unitize_long(FlightGlobals.ActiveVessel.radarAltitude);
                            }
                            else
                            {
                                titleText = Surface;
                            }
                        }

                        // TODO test repulsors

                        display.textTitle.text = titleText;
                        break;
                    }

                    case FlightGlobals.SpeedDisplayModes.Orbit:
                    {
                        if (HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitDashSettings>().orbit)
                        {
                            string ApStr = Unitize_short(FlightGlobals.ship_orbit.ApA);
                            string PeStr = Unitize_short(FlightGlobals.ship_orbit.PeA);

                            string titleText = Localizer.Format("#SpeedUnitDisplay_Apses", ApStr, PeStr);

                            display.textTitle.alignment = TMPro.TextAlignmentOptions.MidlineLeft;

                            display.textTitle.text = titleText;
                        }
                        break;
                    }
                }
            }
        }
    }
}

