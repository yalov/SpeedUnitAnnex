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
        //float line1_fontsize;
        //float line2_fontsize;
        //float lines_fontsize;

        const float mph_ms = 2.23694f;
        const float kmph_ms = 3.6f;
        const float kn_ms = 1 / 0.514f;

        static readonly string kn = " " + Localizer.Format("#SpeedUnitAnnex_kn");
        static readonly string knots = " " + Localizer.Format("#SpeedUnitAnnex_knots");
        static readonly string kmph = " " + Localizer.Format("#SpeedUnitAnnex_kmph");
        static readonly string mps = " " + Localizer.Format("#SpeedUnitAnnex_mps");
        static readonly string mph = " " + Localizer.Format("#SpeedUnitAnnex_mph");
          
        static readonly string Surface = Localizer.Format("#autoLOC_7001218") + " ";
          
        static readonly string Orb = Localizer.Format("#SpeedUnitAnnex_Orb") + "  ";
        static readonly string Surf3 = Localizer.Format("#SpeedUnitAnnex_Surf3") + " ";
        static readonly string Surf5 = Localizer.Format("#SpeedUnitAnnex_Surf5") + " ";
          
        static readonly string Trg = Localizer.Format("#SpeedUnitAnnex_Trg") + " ";
          
        static readonly string Ap_str = Localizer.Format("#autoLOC_6003115") + " ";
        static readonly string Pe_str = Localizer.Format("#autoLOC_6003116") + " ";

        static readonly string Ap_pre = Localizer.Format("#SpeedUnitAnnex_ApoapsisTime_prefix");
        static readonly string Pe_pre = Localizer.Format("#SpeedUnitAnnex_PeriapsisTime_prefix");

        string titleText;

        SpeedUnitAnnexSettings settings;
        //bool setting_orbit_time;


        public SpeedUnitAnnex()
        {
            // Nothing to be done here
        }

        public void Start()
        {
            //Log("OnStart()");

            display = GameObject.FindObjectOfType<SpeedDisplay>();
            settings = HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>();
            //setting_orbit_time = HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>().orbit_time;

            display.textSpeed.enableWordWrapping = false;
            display.textTitle.enableWordWrapping = false;

            //lines_fontsize = display.textTitle.fontSize;

            //line1_fontsize = lines_fontsize / 1.15f;
            //line2_fontsize = display.textSpeed.fontSize;

            display.textTitle.fontSize = display.textTitle.fontSize / 1.15f;

            //GameEvents.onSetSpeedMode.Add(onSetSpeedMode);
        }

    /*
        public void OnGameSettingsWritten()
        {
            Log("OnGameSettingsWritten()");

            settings = HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>();
            setting_orbit_time = HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>().orbit_time;

            onSetSpeedMode(FlightGlobals.speedDisplayMode);

        }

        public void onSetSpeedMode(FlightGlobals.SpeedDisplayModes mode)
        {
            Log("onSetSpeedMode()");



            switch (mode)
            {
                case FlightGlobals.SpeedDisplayModes.Surface:
                case FlightGlobals.SpeedDisplayModes.Target:
                    SetDisplayToDefault();
                    break;
                case FlightGlobals.SpeedDisplayModes.Orbit:
                    if (setting_orbit_time)
                        SetDisplayToOrbitTime();
                    else
                        SetDisplayToDefault();
                    break;
            }
        }

        private void SetDisplayToOrbitTime()
        {

            display.textTitle.fontSize = lines_fontsize;
            display.textSpeed.fontSize = lines_fontsize;
            display.textSpeed.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        }

        private void SetDisplayToDefault()
        {
            display.textTitle.fontSize = line1_fontsize;
            display.textSpeed.fontSize = line2_fontsize;
            display.textSpeed.alignment = TMPro.TextAlignmentOptions.Midline;
            display.textSpeed.color = new Color(0.0f, 1.0f, 0.0f, 1f);
            display.textTitle.color = new Color(0.0f, 1.0f, 0.0f, 1f);

            Log("SetDisplayToDefault()" + display.textTitle.fontSize + " " + display.textSpeed.fontSize);
        }
        */

        public void LateUpdate()
        {
            if (display != null)
            {
                FlightGlobals.SpeedDisplayModes mode = FlightGlobals.speedDisplayMode;

                switch (mode)
                {
                    case FlightGlobals.SpeedDisplayModes.Surface:
                        Vessel.Situations situation = FlightGlobals.ActiveVessel.situation;
                        VesselType vesselType = FlightGlobals.ActiveVessel.vesselType;

                        double spd = FlightGlobals.ship_srfSpeed;                      

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
                                else if (vesselType == VesselType.Plane && situation != Vessel.Situations.LANDED && situation != Vessel.Situations.PRELAUNCH)
                                {
                                    bool isATM = FlightGlobals.ActiveVessel.atmDensity > 0.0;

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

                                    if (situation == Vessel.Situations.SPLASHED) alt += 0.2;
                                    else                                         alt -= 0.2;

                                    titleText = Surf3 + Formatter.Distance_short(alt) + " " + FlightGlobals.ActiveVessel.GetDisplayName();
                                }
                                else titleText = Surf3 + FlightGlobals.ActiveVessel.GetDisplayName();

                                if (titleText.Length > 17) 
                                    titleText = titleText.Substring(0, 16) + "...";

                                break;

                            case VesselType.Flag:

                                titleText = Surf3 + FlightGlobals.ActiveVessel.GetDisplayName();

                                if (titleText.Length > 17)
                                    titleText = titleText.Substring(0, 16) + "...";

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
                        display.textSpeed.text = spd.ToString("F1") + mps;

                        break;

                    case FlightGlobals.SpeedDisplayModes.Orbit:

                        if (FlightGlobals.ActiveVessel.vesselType == VesselType.EVA
                            && settings.orbit_EVA)
                        {
                            titleText = Orb + FlightGlobals.ActiveVessel.GetDisplayName();

                            if (titleText.Length > 17)
                                titleText = titleText.Substring(0, 16) + "...";

                            display.textTitle.text = titleText;

                        }
                        else
                        {
                            double SOI_MASL = FlightGlobals.getMainBody().sphereOfInfluence - FlightGlobals.getMainBody().Radius;
                            bool Ap_ok = FlightGlobals.ship_orbit.ApA > 0 && FlightGlobals.ship_orbit.ApA < SOI_MASL;
                            bool Pe_ok = FlightGlobals.ship_orbit.PeA > 0 && FlightGlobals.ship_orbit.PeA < SOI_MASL;

                            

#if false
                            if (false)
                            {
                                string Ap = Formatter.Distance_short(FlightGlobals.ship_orbit.ApA);
                                string Pe = Formatter.Distance_short(FlightGlobals.ship_orbit.PeA);

                                string TimeAp = Formatter.Time(FlightGlobals.ship_orbit.timeToAp);
                                string TimePe = Formatter.Time(FlightGlobals.ship_orbit.timeToPe);

                                display.textTitle.text = Ap_str + Ap + TimeAp;
                                display.textSpeed.text = "  " + Pe_str + Pe + TimePe;

                                if (Ap_ok)
                                    display.textTitle.color = new Color32(0x00, 0xff, 0x00, 0xff);

                                else
                                    display.textTitle.color = new Color32(0x00, 0xff, 0x00, 0x9f);


                                if (Pe_ok)
                                    display.textSpeed.color = new Color32(0x00, 0xff, 0x00, 0xff);
                                else
                                    display.textSpeed.color = new Color32(0x00, 0xff, 0x00, 0x9f);
                            }
#endif

                            if (settings.orbit_time)
                            {
                                string Ap = Formatter.Distance_k(FlightGlobals.ship_orbit.ApA);
                                string Pe = Formatter.Distance_k(FlightGlobals.ship_orbit.PeA);
                                //double tTr = FlightGlobals.ship_orbit.timeToTransition1;

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

                                display.textTitle.text = (Ap_ok ? "<color=#00ff00ff>" : "<color=#00ff009f>") + Ap +
                                                         (Pe_ok ? " <color=#00ff00ff>" : " <color=#00ff009f>") + Pe +
                                                      (Apsis_ok ? " <color=#ffffffff>" : " <color=#ffffff9f>") + TimeApsis;
                            }
                            else
                            {
                                string Ap = Formatter.Distance_k(FlightGlobals.ship_orbit.ApA);
                                string Pe = Formatter.Distance_k(FlightGlobals.ship_orbit.PeA);

                                display.textTitle.text = Orb + (Ap_ok ? "<color=#00ff00ff>" : "<color=#00ff009f>") + Ap +
                                                               (Pe_ok ? "  <color=#00ff00ff>" : "  <color=#00ff009f>") + Pe;
                                
                            }
                        }

                        display.textSpeed.text = FlightGlobals.ship_obtSpeed.ToString("F1") + mps;
                        break;

                    case FlightGlobals.SpeedDisplayModes.Target:

                        ITargetable obj = FlightGlobals.fetch.VesselTarget;

#region all Target
                        // ITargetable ->  CelestialBody;
                        //                 FlightCoMTracker;
                        //                 ModuleDockingNode;
                        //                 PositionTarget;
                        //                 Vessel;
#endregion

                        string name;

                        if (obj is ModuleDockingNode)
                            name = obj.GetVessel().GetDisplayName();
                        else
                            name = obj.GetDisplayName();

                        if (name.Length > 1 && name.Substring(name.Length - 2, 1) == "^")
                            name = name.Substring(0, name.Length - 2);

                        if (settings.targetDistance)
                        {
                            // from Docking Port Alignment Indicator
                            Transform selfTransform = FlightGlobals.ActiveVessel.ReferenceTransform;
                            Transform targetTransform = FlightGlobals.fetch.VesselTarget.GetTransform();
                            Vector3 targetToOwnship = selfTransform.position - targetTransform.position;
                            float distanceToTarget = targetToOwnship.magnitude;

                            titleText = Trg + Formatter.Distance_short(distanceToTarget) + " " + name;
                        }
                        else
                        {
                            titleText = Trg + name;
                        }

                        if (titleText.Length <= 17)
                            display.textTitle.text = titleText;
                        else
                            display.textTitle.text = titleText.Substring(0, 16) + "...";

                        if (FlightGlobals.ship_tgtSpeed < 0.195)
                            display.textSpeed.text = FlightGlobals.ship_tgtSpeed.ToString("F2") + mps;
                        else
                            display.textSpeed.text = FlightGlobals.ship_tgtSpeed.ToString("F1") + mps;
                        break;
                }

                //Log("LateUpdate()" + display.textTitle.fontSize + " " + display.textSpeed.fontSize);

                // need to be there
                display.textTitle.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            }
        }
    }
}

