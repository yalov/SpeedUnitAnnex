using System;
using UnityEngine;
using KSP.UI.Screens.Flight;
using KSP.Localization;


namespace SpeedUnitAnnex
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SpeedUnitAnnex : MonoBehaviour
    {
        private SpeedDisplay display;
        //private float fontSize;
        //private float titleFontSize;
        private float mph_ms = 2.23694f;
        private float kmph_ms = 3.6f;
        private float kn_ms = 0.514f;

        private string kn =   " " + Localizer.Format("#SpeedUnitAnnex_knot");
        private string kmph = " " + Localizer.Format("#SpeedUnitAnnex_kmph");
        private string mph =  " " + Localizer.Format("#SpeedUnitAnnex_mph");

        //private string Orbit   = Localizer.Format("#autoLOC_7001217") + " ";
        private string Surface = Localizer.Format("#autoLOC_7001218") + " ";
        //private string Target  = Localizer.Format("#autoLOC_7001219") + " ";

        private string Surf3 = Localizer.Format("#SpeedUnitAnnex_Surf3") + " ";
        private string Surf5 = Localizer.Format("#SpeedUnitAnnex_Surf5") + " ";

        private string Trg = Localizer.Format("#SpeedUnitAnnex_Trg") + " ";
        //private string TrgDP = Localizer.Format("#SpeedUnitAnnex_TrgDockingPort") + " ";

        private string m = Localizer.Format("#SpeedUnitAnnex_meter");
        private string k = Localizer.Format("#SpeedUnitAnnex_kilo");
        private string M = Localizer.Format("#SpeedUnitAnnex_mega");
        private string G = Localizer.Format("#SpeedUnitAnnex_giga");
        private string T = Localizer.Format("#SpeedUnitAnnex_tera");

        private string Mm = Localizer.Format("#SpeedUnitAnnex_mega") + Localizer.Format("#SpeedUnitAnnex_meter");
        private string Gm = Localizer.Format("#SpeedUnitAnnex_giga") + Localizer.Format("#SpeedUnitAnnex_meter");


        //private System.Globalization.CultureInfo culture =
        //    System.Globalization.CultureInfo.CreateSpecificCulture(KSP.Localization.Localizer.CurrentLanguage);

        public SpeedUnitAnnex()
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
                    //fontSize = display.textSpeed.fontSize;
                    //titleFontSize = display.textTitle.fontSize;
                    //display.textTitle.fontSize = titleFontSize / 1.15f;

                    display.textTitle.fontSize /= 1.15f;
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
            //  KerbATM 70kм   Minmus 47M Moho 4G  Eeloo 113G  Plock 700G      
            //  Moon 400M      Mercury 60G Pluto 7300G
            if (v < 1E3)          // 0.0 m - 999.9 m
                str = Truncate(value, "N", 1) + " " + m;

            else if (v < 1E8)          // 1,000 m - 99,999,999 m
                str = Truncate(value, "N", 0) + " " + m;

            else if (v < 1E12)    // 100.0 Mm - 999,999.9 Mm
                str = Truncate(value / 1E6, "N", 1) + " " + Mm;

            else                  // 1,000.0 Gm - 999,999.9 Gm and beyond
                str = Truncate(value / 1E6, "N", 1) + " " + Gm;
    
            return str;
        }

        public void LateUpdate()
        {
            if (display != null)
            {
                FlightGlobals.SpeedDisplayModes mode = FlightGlobals.speedDisplayMode;


                switch (mode)
                {
                    case FlightGlobals.SpeedDisplayModes.Surface:
                        {
                            Vessel.Situations situation = FlightGlobals.ActiveVessel.situation;
                            VesselType vesselType = FlightGlobals.ActiveVessel.vesselType;

                            double spd = FlightGlobals.ActiveVessel.srfSpeed;
                            string titleText;

                            //VesselType.Base;          //Situations.LANDED   
                            //VesselType.Debris         //Situations.SPLASHED
                            //VesselType.EVA            //Situations.PRELAUNCH
                            //VesselType.Flag           //Situations.FLYING
                            //VesselType.Lander         //Situations.SUB_ORBITAL
                            //VesselType.Plane;         //Situations.ORBITING
                            //VesselType.Probe          //Situations.ESCAPING
                            //VesselType.Relay          //Situations.DOCKED
                            //VesselType.Rover
                            //VesselType.Ship
                            //VesselType.SpaceObject
                            //VesselType.Station
                            //VesselType.Unknown

                            switch (vesselType)
                            {
                                case VesselType.Plane:
                                case VesselType.Rover:
                                    {
                                        // Boat or Submarine
                                        if (situation == Vessel.Situations.SPLASHED)
                                        {
                                            bool isradar = HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>().radar;

                                            if (FlightGlobals.ActiveVessel.altitude < -20 && isradar) // Submarine
                                                titleText = Surf3 + Unitize_short(FlightGlobals.ActiveVessel.altitude - FlightGlobals.ActiveVessel.terrainAltitude) + "  " + (spd * kn_ms).ToString("F1") + kn;
                                            else                                                       // Boat
                                                titleText = Surf5 + (spd * kn_ms).ToString("F1") + kn;
                                        }
                                        // Plane (not LANDED) 
                                        else if (vesselType == VesselType.Plane
                                            && situation != Vessel.Situations.LANDED && situation != Vessel.Situations.PRELAUNCH)
                                        {
                                            bool isradar = HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>().radar;
                                            bool isATM = FlightGlobals.ActiveVessel.atmDensity > 0.0;

                                            if (isradar)
                                            {
                                                if (isATM)
                                                    titleText = Surf3 + Unitize_short(FlightGlobals.ActiveVessel.radarAltitude) + "  "
                                                        + Localizer.Format("#SpeedUnitAnnex_mach", FlightGlobals.ActiveVessel.mach.ToString("F1"));

                                                else titleText = Surf5 + Unitize_long(FlightGlobals.ActiveVessel.radarAltitude);
                                            }
                                            else
                                            {
                                                if (isATM)
                                                    titleText = Surf5 + Localizer.Format("#SpeedUnitAnnex_mach", FlightGlobals.ActiveVessel.mach.ToString("F1"));
                                                else titleText = Surface;
                                            }
                                        }
                                        // Rover (and LANDED Plane)  // and rover-carrier if ksp detect them as rover
                                        // All mistake at ksp detecting vessel type can be fixed by some additional checking (ex. altitude for rover-carrier)
                                        // but it make unclear to user, which values shows up.
                                        else //if FlightGlobals.ActiveVessel.radarAltitude < 100)
                                        {
                                            if (HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>().kph)
                                                titleText = Surf5 + (spd * kmph_ms).ToString("F1") + kmph;
                                            else
                                                titleText = Surf5 + (spd * mph_ms).ToString("F1") + mph;
                                        }
                                    }
                                    break;

                                case VesselType.EVA:
                                    {
                                        if (HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>().radar)
                                        {
                                            double alt;

                                            if (situation == Vessel.Situations.SPLASHED)
                                                alt = FlightGlobals.ActiveVessel.radarAltitude + 0.2;
                                            else
                                                alt = FlightGlobals.ActiveVessel.radarAltitude - 0.2;

                                            titleText = Surf3 + Unitize_short(alt) + " " + FlightGlobals.ActiveVessel.GetDisplayName();
                                        }
                                        else
                                            titleText = Surf3 + FlightGlobals.ActiveVessel.GetDisplayName();

                                        if (titleText.Length > 17)
                                            titleText = titleText.Substring(0, 16) + "...";

                                    }
                                    break;

                                case VesselType.Flag:
                                    {
                                        titleText = Surf3 + FlightGlobals.ActiveVessel.GetDisplayName();

                                        if (titleText.Length > 17)
                                            titleText = titleText.Substring(0, 16) + "...";
                                    }
                                    break;

                                // Other: Rocket, Lander, Base etc 
                                default:
                                    {
                                        if (HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>().radar)
                                            titleText = Surf5 + Unitize_long(FlightGlobals.ActiveVessel.radarAltitude);
                                        else
                                            titleText = Surface;

                                    }
                                    break;

                            }
                            
                            display.textTitle.text = titleText;
                            break;
                        }
                    case FlightGlobals.SpeedDisplayModes.Orbit:
                        {
                            if (HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>().orbit)
                            {
                                string ApStr = Unitize_short(FlightGlobals.ship_orbit.ApA);
                                string PeStr = Unitize_short(FlightGlobals.ship_orbit.PeA);

                                string titleText = Localizer.Format("#SpeedUnitAnnex_Apses", ApStr, PeStr);

                                display.textTitle.text = titleText;
                            }
                            break;
                        }
                    case FlightGlobals.SpeedDisplayModes.Target:
                        {
                            
                 
                            ITargetable obj = FlightGlobals.fetch.VesselTarget;

                            // ITargetable ->  CelestialBody;
                            //                 FlightCoMTracker;
                            //                 ModuleDockingNode;
                            //                 PositionTarget;
                            //                 Vessel;

                            string text;
                            string name;

                            if (obj is ModuleDockingNode)
                                name = obj.GetVessel().GetDisplayName();
                            else
                                name = obj.GetDisplayName();

                            if (name.Length > 1 && name.Substring(name.Length - 2, 1) == "^")
                                name = name.Substring(0, name.Length - 2);

                            if (HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>().targetDistance)
                            {
                                // from Docking Port Alignment Indicator
                                Transform selfTransform = FlightGlobals.ActiveVessel.ReferenceTransform;
                                Transform targetTransform = FlightGlobals.fetch.VesselTarget.GetTransform();
                                Vector3 targetToOwnship = selfTransform.position - targetTransform.position;
                                float distanceToTarget = targetToOwnship.magnitude;

                                text = Trg + Unitize_short(distanceToTarget) + " " +name;
                            }
                            else
                            {
                                text = Trg + name;
                            }


                            if (text.Length <= 17)
                                display.textTitle.text = text;
                            else
                                display.textTitle.text = text.Substring(0, 16) + "...";

                            break;
                        }
                }

                display.textTitle.alignment = TMPro.TextAlignmentOptions.MidlineLeft;

            }
        }
    }
}

