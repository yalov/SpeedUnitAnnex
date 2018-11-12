using System;
using UnityEngine;
using KSP.UI.Screens.Flight;
using KSP.Localization;
using static SpeedUnitAnnex.Logging;


namespace SpeedUnitAnnex
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SpeedUnitAnnex : MonoBehaviour
    {
        SpeedDisplay display;

        readonly float mph_ms = 2.23694f;
        readonly float kmph_ms = 3.6f;
        readonly float kn_ms = 1 / 0.514f;

        readonly string kn = " " + Localizer.Format("#SpeedUnitAnnex_kn");
        readonly string knots = " " + Localizer.Format("#SpeedUnitAnnex_knots");
        readonly string kmph = " " + Localizer.Format("#SpeedUnitAnnex_kmph");
        readonly string mps = " " + Localizer.Format("#SpeedUnitAnnex_mps");
        readonly string mph = " " + Localizer.Format("#SpeedUnitAnnex_mph");
        
        readonly string Surface = Localizer.Format("#autoLOC_7001218") + " ";
        
        readonly string Orb = Localizer.Format("#SpeedUnitAnnex_Orb") + " ";
        readonly string Surf3 = Localizer.Format("#SpeedUnitAnnex_Surf3") + " ";
        readonly string Surf5 = Localizer.Format("#SpeedUnitAnnex_Surf5") + " ";
        
        readonly string Trg = Localizer.Format("#SpeedUnitAnnex_Trg") + " ";
        
        readonly string Ap_str = Localizer.Format("#autoLOC_6003115") + " ";
        readonly string Pe_str = Localizer.Format("#autoLOC_6003116") + " ";
        readonly string Ap_pre = Localizer.Format("#SpeedUnitAnnex_ApoapsisTime_prefix");
        readonly string Pe_pre = Localizer.Format("#SpeedUnitAnnex_PeriapsisTime_prefix");

        readonly string Jeb_full  = Localizer.Format("#autoLOC_20803");
        readonly string Val_full = Localizer.Format("#autoLOC_20827");
        readonly string Jeb_short = Localizer.Format("#SpeedUnitAnnex_JebShort");
        readonly string Val_short = Localizer.Format("#SpeedUnitAnnex_ValShort");

        readonly char[] delimiterChars = Localizer.Format("#SpeedUnitAnnex_DelimiterChars").ToCharArray();
        readonly char[] wideChars = Localizer.Format("#SpeedUnitAnnex_WideChars").ToCharArray();
        readonly char[] thinChars = Localizer.Format("#SpeedUnitAnnex_ThinChars").ToCharArray();

        int MaxCharsInLine = 17;

        string titleText;
        SpeedUnitAnnexSettings settings;

        public SpeedUnitAnnex()
        {
            // Nothing to be done here
        }

        void onVesselChange(Vessel vessel)
        {
            Log("onVesselChange: " + vessel.GetDisplayName());
        }

        void onSetSpeedMode(FlightGlobals.SpeedDisplayModes mode)
        {
            Log("onSetSpeedMode: " + mode.displayDescription());
            Log("renderedWidth: " + display.textTitle.renderedWidth);
            Log("bounds.size: " + display.textTitle.bounds.size);

            Log("textBounds.size: " + display.textTitle.textBounds.size);
        }



        public void OnDisable()
        {
            G﻿ameEvents.onVesselChange.Remove(onVesselChange);
            GameEvents.onSetSpeedMode.Remove(onSetSpeedMode);
        }

        public void Start()
        {
            G﻿ameEvents.onVesselChange.Add(onVesselChange);
            G﻿ameEvents.onSetSpeedMode.Add(onSetSpeedMode);

            display = GameObject.FindObjectOfType<SpeedDisplay>();
            settings = HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>();

            display.textSpeed.enableWordWrapping = false;
            display.textTitle.enableWordWrapping = false;

            display.textTitle.fontSize = display.textTitle.fontSize / 1.15f;

            Int32.TryParse(Localizer.Format("#SpeedUnitAnnex_MaxCharsInLine"), out MaxCharsInLine);


            //Log("MaxCharsInLine " + MaxCharsInLine);

            //Log("Font: "+display.textSpeed.font);
            // NotoSans-Regular SDF
        }

        private string CutKerbalName(string prefix, Vessel kerbal, bool full_trait = true)
        {
            if (FlightGlobals.ActiveVessel.vesselType != VesselType.EVA) return "";

            string trait = kerbal.GetVesselCrew()[0].GetLocalizedTrait();
            string name = kerbal.GetDisplayName();

            string first_name;

            if      (name == Val_full) first_name = Val_short;
            else if (name == Jeb_full) first_name = Jeb_short;
            else                       first_name = name.Split(delimiterChars)[0];
	        
            string str = prefix + trait + " "+ first_name ;
            // Orb Pilot Jebediah      18 - 5/2 - 2/2 = 14.5
            // Srf 0.00m P. Jebediah   21 - 6/2 - 2/2 = 17
            // Srf 0.00m P. Valentina  22 - 6/2 - 3/2 = 17.5

            // Srf 0.00m P. Shelbrett    22 - 6/2 - 3/2 = 17.5
            // Srf 0.00m P. Wehrming     21 - 6/2 - 1/2 = 17.5
            // Srf 0.00m P. Billy-Bobsy  24 - 6/2 - 3/2 = 19.5
            // Srf 0.00m E. Bill         17 - 6/2 - 3/2 = 12.5
            // Srf 0.00m Engineer Bill   23 - 6/2 - 3/2 = 18.5
            // Pilot Tomsen     // virf.red  // shel.brett  // jebe.diah

            // str.Length ? MaxCharsInLine + diff = MaxCharsInLine +  str.Length - (int)len - 1
            // 0 ? MaxCharsInLine - (int)len - 1
            // (int)len ? MaxCharsInLine - 1
            // len > MaxCharsInLine
            // 108

            if (full_trait && SmartLength(str) <= MaxCharsInLine)
                return str;
            else
            {
                str = prefix  + trait[0] + ". "+ first_name;
                float len = SmartLength(str);

                if (len <= MaxCharsInLine)
                    return str;
                else
                {
                    int diff = Math.Max(0, str.Length - (int)len - 1);
                    return str.Substring(0, MaxCharsInLine + diff) + ".";
                }
            }
        }

        private string CutName(string prefix, string name)
        {
            string str = prefix + name;

            if (SmartLength(str) < MaxCharsInLine)
                return str;
            else
                return prefix + name.Substring(0, Math.Max(0, MaxCharsInLine - prefix.Length - 1)) + "...";
        }

        float SmartLength(string str)
        {
            int thin_char_count = 0;
            int wide_char_count = 0;

            foreach (char c in str)
            {
                foreach (char t in thinChars) if (c == t) thin_char_count++;
                foreach (char w in wideChars) if (c == w) wide_char_count++;
            }
            return str.Length - 1/2.0f * thin_char_count + 1/2.0f * wide_char_count;
        }

        

        public void LateUpdate()
        {
            if (display == null) return;
            if (FlightGlobals.ActiveVessel == null) return;

            FlightGlobals.SpeedDisplayModes mode = FlightGlobals.speedDisplayMode;

            switch (mode)
            {
                case FlightGlobals.SpeedDisplayModes.Surface:
                    Vessel.Situations situation = FlightGlobals.ActiveVessel.situation;
                    VesselType vesselType = FlightGlobals.ActiveVessel.vesselType;

                    double spd = FlightGlobals.ship_srfSpeed;
                    string srfSpeedText = spd.ToString("F1") + mps;

                    #region all VesselTypes and Situations
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
                    #endregion

                    switch (vesselType)
                    {
                        case VesselType.Plane:
                        case VesselType.Rover:
                            // Boat or Submarine
                            if (situation == Vessel.Situations.SPLASHED)
                            {
                                if (FlightGlobals.ActiveVessel.altitude < -20 && settings.radar) // Submarine
                                    titleText = Surf3 + Formatter.Distance_short(FlightGlobals.ActiveVessel.altitude - FlightGlobals.ActiveVessel.terrainAltitude)
                                        + "  " + (spd * kn_ms).ToString("F1") + kn;
                                else                                                       // Boat
                                    titleText = Surf5 + (spd * kn_ms).ToString("F1") + knots;
                            }
                            // Plane (not LANDED) 
                            else if (vesselType == VesselType.Plane &&
                                situation != Vessel.Situations.LANDED && situation != Vessel.Situations.PRELAUNCH)
                            {
                                bool isATM = FlightGlobals.ActiveVessel.atmDensity > 0.0;
                                double speedIAS = FlightGlobals.ActiveVessel.indicatedAirSpeed;

                                if (settings.radar)
                                {
                                    if (settings.mach)
                                    {
                                        if (isATM)
                                            titleText = Surf3 + Formatter.Distance_short(FlightGlobals.ActiveVessel.radarAltitude) + "  "
                                                + Localizer.Format("#SpeedUnitAnnex_mach", FlightGlobals.ActiveVessel.mach.ToString("F1"));

                                        else titleText = Surf5 + Formatter.Distance_long(FlightGlobals.ActiveVessel.radarAltitude);
                                    }
                                    else
                                        titleText = Surf3 + Formatter.Distance_short(FlightGlobals.ActiveVessel.radarAltitude) + "  "
                                            + (spd * kn_ms).ToString("F1") + kn;
                                }
                                else
                                {
                                    if (settings.mach)
                                    {
                                        if (isATM)
                                            titleText = Surf5 + Localizer.Format("#SpeedUnitAnnex_mach", FlightGlobals.ActiveVessel.mach.ToString("F1"));
                                        else titleText = Surface;
                                    }
                                    else
                                        titleText = Surf5 + (spd * kn_ms).ToString("F1") + knots;
                                }

                                if (settings.ias && speedIAS > 0)
                                    srfSpeedText += " " + speedIAS.ToString("F1");
                            }
                            // Rover (and LANDED Plane)  // and rover-carrier if ksp detect them as rover
                            // All mistake at ksp detecting vessel type can be fixed by some additional checking (ex. altitude for rover-carrier)
                            // but it make unclear to user, which values is showed up.
                            else //if FlightGlobals.ActiveVessel.radarAltitude < 100)
                            {
                                if (settings.kmph)
                                    titleText = Surf5 + (spd * kmph_ms).ToString("F1") + kmph;
                                else
                                    titleText = Surf5 + (spd * mph_ms).ToString("F1") + mph;
                            }
                            break;

                        case VesselType.EVA:

                            if (settings.radar)
                            {
                                double alt = FlightGlobals.ActiveVessel.radarAltitude;

                                if (situation == Vessel.Situations.SPLASHED) alt += 0.21;
                                else alt -= 0.27;

                                titleText = CutKerbalName(Surf3 + Formatter.Distance_short(alt) + " ", FlightGlobals.ActiveVessel, false);
                            }
                            else
                                titleText = CutKerbalName(Surf3, FlightGlobals.ActiveVessel);

                            break;

                        case VesselType.Flag:
                            titleText = CutName(Surf3, FlightGlobals.ActiveVessel.GetDisplayName());
                            break;

                        // Other: Rocket, Lander, Base etc 
                        default:

                            if (settings.radar)
                                titleText = Surf5 + Formatter.Distance_long(FlightGlobals.ActiveVessel.radarAltitude);
                            else
                                titleText = Surface;
                            break;
                    }

                    display.textTitle.text = titleText;
                    display.textSpeed.text = srfSpeedText;

                    break;

                case FlightGlobals.SpeedDisplayModes.Orbit:

                    if (FlightGlobals.ActiveVessel.vesselType == VesselType.EVA
                        && settings.orbit_EVA)
                    {
                        display.textTitle.text =
                            CutKerbalName(settings.orbit_time ? "" : Orb, FlightGlobals.ActiveVessel);
                    }
                    else
                    {
                        double SOI_MASL = FlightGlobals.getMainBody().sphereOfInfluence - FlightGlobals.getMainBody().Radius;
                        bool Ap_ok = FlightGlobals.getMainBody().atmosphereDepth < FlightGlobals.ship_orbit.ApA && FlightGlobals.ship_orbit.ApA < SOI_MASL;
                        bool Pe_ok = FlightGlobals.getMainBody().atmosphereDepth < FlightGlobals.ship_orbit.PeA && FlightGlobals.ship_orbit.PeA < SOI_MASL;
                        string Ap = Formatter.Distance_k(FlightGlobals.ship_orbit.ApA);
                        string Pe = Formatter.Distance_k(FlightGlobals.ship_orbit.PeA);
                        string Apsises = (Ap_ok ? "<color=#00ff00ff>" : "<color=#00ff009f>") + Ap +
                                         (Pe_ok ? " <color=#00ff00ff>" : " <color=#00ff009f>") + Pe;

                        if (settings.orbit_time)
                        {
                            string TimeApsis;
                            bool Apsis_ok;

                            if (FlightGlobals.ship_orbit.timeToAp < FlightGlobals.ship_orbit.timeToPe)
                            {
                                Apsis_ok = Ap_ok;
                                TimeApsis = Formatter.Time(FlightGlobals.ship_orbit.timeToAp, Ap_pre);
                            }
                            else
                            {
                                Apsis_ok = Pe_ok;
                                TimeApsis = Formatter.Time(FlightGlobals.ship_orbit.timeToPe, Pe_pre);
                            }

                            display.textTitle.text = Apsises +
                                                  (Apsis_ok ? " <color=#ffffffff>" : " <color=#ffffff9f>") + TimeApsis;
                        }
                        else
                        {
                            display.textTitle.text = Orb + Apsises;
                        }
                    }

                    display.textSpeed.text = FlightGlobals.ship_obtSpeed.ToString("F1") + mps;
                    break;

                case FlightGlobals.SpeedDisplayModes.Target:

                    ITargetable obj = FlightGlobals.fetch.VesselTarget;
                    if (obj == null) return;

                    #region all Target
                    // 1.5.1
                    // ITargetable ->  CelestialBody;
                    //                 FlightCoMTracker;
                    //                 ModuleDockingNode;
                    //                 PositionTarget;
                    //                 Vessel;
                    #endregion

                    string distanceToTarget = "";
                    string name = "";

                    if (settings.targetDistance)
                    {
                        // from Docking Port Alignment Indicator
                        Transform selfTransform = FlightGlobals.ActiveVessel.ReferenceTransform;
                        Transform targetTransform = FlightGlobals.fetch.VesselTarget.GetTransform();
                        Vector3 targetToOwnship = selfTransform.position - targetTransform.position;
                        float distance = targetToOwnship.magnitude;
                        distanceToTarget = Formatter.Distance_short(distance) + " ";
                    }
             
                    if (obj is ModuleDockingNode)
                        name = obj.GetVessel().GetDisplayName();
                    else if (obj is Vessel && obj.GetVessel().vesselType == VesselType.EVA)
                        name = obj.GetDisplayName().Split(' ')[0];
                    else
                        name = obj.GetDisplayName();

                    if (name.Length > 1 && name.Substring(name.Length - 2, 1) == "^")
                        name = name.Substring(0, name.Length - 2);

                    display.textTitle.text = CutName(Trg + distanceToTarget, name);

                    if (FlightGlobals.ship_tgtSpeed < 0.195)
                        display.textSpeed.text = FlightGlobals.ship_tgtSpeed.ToString("F2") + mps;
                    else
                        display.textSpeed.text = FlightGlobals.ship_tgtSpeed.ToString("F1") + mps;
                    break;
            }


            // need to be there, for every tick. Doesn't work in the  Start() or onSetSpeedMode()
            display.textTitle.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        }
    }
}

