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

        readonly double Eva_CoM_fix_landed = -0.27;
        readonly double Eva_CoM_fix_splashed = 0.21;

        readonly double BoatSubmarineBorderAlt = -20;

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
        readonly string NoTrg = Localizer.Format("#autoLOC_339139");
        
        readonly string Ap_str = Localizer.Format("#autoLOC_6003115") + " ";
        readonly string Pe_str = Localizer.Format("#autoLOC_6003116") + " ";
        readonly string Ap_pre = Localizer.Format("#SpeedUnitAnnex_ApoapsisTime_prefix");
        readonly string Pe_pre = Localizer.Format("#SpeedUnitAnnex_PeriapsisTime_prefix");

        readonly string Jeb_full  = Localizer.Format("#autoLOC_20803");
        readonly string Val_full = Localizer.Format("#autoLOC_20827");
        readonly string Jeb_short = Localizer.Format("#SpeedUnitAnnex_JebShort");
        readonly string Val_short = Localizer.Format("#SpeedUnitAnnex_ValShort");

        readonly char[] delimiterChars = Localizer.Format("#SpeedUnitAnnex_DelimiterChars").ToCharArray();

        ITargetable Target = null;
        string TargetName;


        string FinalName;
        SpeedUnitAnnexSettings settings;

        public SpeedUnitAnnex()
        {
            // Nothing to be done here
        }


        //                      RA  rA
        // flying above Ground  G   G
        // flying above Water   S   S
        // swimming in Water    G   S
        // landed on Ground     G   G
        // splashed on Water    G   S
        // landed under Water   G   S


        double AGL()
        {
            return FlightGlobals.ship_altitude - FlightGlobals.ActiveVessel.terrainAltitude;
        }

        double RadarAltitude()
        {
            if (FlightGlobals.ActiveVessel.terrainAltitude < 0.0
                && FlightGlobals.ActiveVessel.altitude > BoatSubmarineBorderAlt
                && FlightGlobals.ActiveVessel.situation != Vessel.Situations.LANDED)
                return FlightGlobals.ActiveVessel.radarAltitude;
            else
                return FlightGlobals.ship_altitude - FlightGlobals.ActiveVessel.terrainAltitude;
        }


        string RadarAltitudeEVA_str()
        {
            double alt = RadarAltitude();

            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.SPLASHED)
                alt += Eva_CoM_fix_splashed;
            else
                alt += Eva_CoM_fix_landed;

            return Formatter.Distance_short(alt) + " ";
        }

        string CalcTargetDistance(ITargetable obj)
        {
            // from Docking Port Alignment Indicator
            Transform selfTransform = FlightGlobals.ActiveVessel.ReferenceTransform;
            Transform targetTransform = obj.GetTransform();
            Vector3 targetToOwnship = selfTransform.position - targetTransform.position;
            float distance = targetToOwnship.magnitude;
            return Formatter.Distance_short(distance) + " ";
        }

        string GetTargetName(ITargetable obj)
        {
            string name;
            if (obj is ModuleDockingNode)
                name = obj.GetVessel().GetDisplayName();
            else if (obj is Vessel && obj.GetVessel().vesselType == VesselType.EVA)
                name = obj.GetDisplayName().Split(' ')[0];
            else
                name = obj.GetDisplayName();

            return Localizer.Format("<<1>>", name);
        }


        private void SetFinalName(FlightGlobals.SpeedDisplayModes mode)
        {
            switch (mode)
            {
                case FlightGlobals.SpeedDisplayModes.Surface:
                    if (FlightGlobals.ActiveVessel.vesselType == VesselType.EVA)
                        FinalName = CutKerbalName(Surf3 + (settings.radar ? RadarAltitudeEVA_str() : ""), FlightGlobals.ActiveVessel);

                    else if (FlightGlobals.ActiveVessel.vesselType == VesselType.Flag)
                        FinalName = CutName(Surf3, FlightGlobals.ActiveVessel.GetDisplayName());
                    break;

                case FlightGlobals.SpeedDisplayModes.Orbit:
                    if (FlightGlobals.ActiveVessel.vesselType == VesselType.EVA && settings.orbit_EVA)
                        FinalName = CutKerbalName(settings.orbit_time ? "" : Orb, FlightGlobals.ActiveVessel, false);
                    break;
            }
            //Log("SetFinalName: " + FinalName);

        }

        private string CutKerbalName(string prefix, Vessel kerbal, bool cut_orange_names = true)
        {
            if (FlightGlobals.ActiveVessel.vesselType != VesselType.EVA) return "";

            string trait = kerbal.GetVesselCrew()[0].GetLocalizedTrait();
            string full_name = kerbal.GetDisplayName();

            string first_name;

            if (cut_orange_names)
            {
                if (full_name == Val_full) first_name = Val_short;
                else if (full_name == Jeb_full) first_name = Jeb_short;
                else first_name = full_name.Split(delimiterChars)[0];
            }
            else
                first_name = full_name.Split(delimiterChars)[0];

            string name = trait[0] + ". " + first_name;

            return CutName(prefix, name);
        }

        private string CutName(string prefix, string name)
        {
            //Log("CutName: {0}, {1:F1}", prefix + name, display.textTitle.GetPreferredValues(prefix + name).x);
            //display.textTitle.ForceMeshUpdate(true);

            if (display.textTitle.GetPreferredValues(prefix + name).x > 108)
            { 
                while (display.textTitle.GetPreferredValues(prefix + name).x > 108 && name.Length>0)
                {
                    name = name.Substring(0, name.Length - 1);
                }
                name += ".";
            }
            return name;
        }

        void onVesselChange(Vessel vessel)
        {
            //Log("onVesselChange: " + vessel.GetDisplayName());
            SetFinalName(FlightGlobals.speedDisplayMode);
        }

        void onSetSpeedMode(FlightGlobals.SpeedDisplayModes mode)
        {
            //Log("onSetSpeedMode: " + mode.displayDescription());
            SetFinalName(mode);
        }

        
        public void OnGameSettingsApplied()
        {
            //Log("OnGameSettingsApplied");
            SetFinalName(FlightGlobals.speedDisplayMode);
        }

        public void OnDisable()
        {
            G﻿ameEvents.onVesselChange.Remove(onVesselChange);
            GameEvents.onSetSpeedMode.Remove(onSetSpeedMode);
            G﻿ameEvents.OnGameSettingsWritten.Remove(OnGameSettingsApplied);
        }

        public void Start()
        {
            G﻿ameEvents.onVesselChange.Add(onVesselChange);
            G﻿ameEvents.onSetSpeedMode.Add(onSetSpeedMode);
            G﻿ameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);

            display = GameObject.FindObjectOfType<SpeedDisplay>();
            settings = HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>();

            display.textSpeed.enableWordWrapping = false;
            display.textTitle.enableWordWrapping = false;

            display.textTitle.fontSize = display.textTitle.fontSize / 1.15f;

            SetFinalName(FlightGlobals.speedDisplayMode);

            //Log("Font: "+display.textSpeed.font);
            // NotoSans-Regular SDF
        }

        public void LateUpdate()
        {
            if (display == null) return;
            if (FlightGlobals.ActiveVessel == null) return;

            FlightGlobals.SpeedDisplayModes mode = FlightGlobals.speedDisplayMode;

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

            switch (mode)
            {
                case FlightGlobals.SpeedDisplayModes.Surface:
                    string titleText;
                    Vessel.Situations situation = FlightGlobals.ActiveVessel.situation;
                    VesselType vesselType = FlightGlobals.ActiveVessel.vesselType;

                    double spd = FlightGlobals.ship_srfSpeed;
                    string srfSpeedText = spd.ToString("F1") + mps;

                    switch (vesselType)
                    {
                        case VesselType.Plane:
                        case VesselType.Rover:
                            // Boat or Submarine
                            if (situation == Vessel.Situations.SPLASHED)
                            {
                                // Submarine
                                if (FlightGlobals.ActiveVessel.altitude < BoatSubmarineBorderAlt && settings.radar) 
                                    titleText = Surf3 + Formatter.Distance_short(AGL())
                                        + "  " + (spd * kn_ms).ToString("F1") + kn;
                                // Boat
                                else
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
                                    double radar = FlightGlobals.ActiveVessel.radarAltitude;
                                    if (settings.mach)
                                    {
                                        if (isATM)
                                            titleText = Surf3 + Formatter.Distance_short(radar) + "  "
                                                + Localizer.Format("#SpeedUnitAnnex_mach", FlightGlobals.ActiveVessel.mach.ToString("F1"));

                                        else titleText = Surf5 + Formatter.Distance_long(radar);
                                    }
                                    else
                                        titleText = Surf3 + Formatter.Distance_short(radar) + "  "
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
                            else
                            {
                                if (settings.kmph)
                                    titleText = Surf5 + (spd * kmph_ms).ToString("F1") + kmph;
                                else
                                    titleText = Surf5 + (spd * mph_ms).ToString("F1") + mph;
                            }

                            // All mistake at ksp detecting vessel type can be fixed by 
                            // some additional checking (ex. altitude for rover-carrier)
                            // but it make unclear to user, which values is showed up.
                            break;

                        case VesselType.EVA:
                            titleText = Surf3 + (settings.radar?RadarAltitudeEVA_str():"") + FinalName;
                            break;

                        case VesselType.Flag:
                            titleText = Surf3 + FinalName;
                            break;

                        // Other: Rocket, Lander, Base etc 
                        default:

                            if (settings.radar)
                                titleText = Surf5 + Formatter.Distance_long(RadarAltitude());
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
                        display.textTitle.text = (settings.orbit_time ? "" : Orb) + FinalName;
                    }
                    else
                    {
                        double SOI_MASL = FlightGlobals.getMainBody().sphereOfInfluence - FlightGlobals.getMainBody().Radius;
                        bool Ap_ok = FlightGlobals.getMainBody().atmosphereDepth < FlightGlobals.ship_orbit.ApA && FlightGlobals.ship_orbit.ApA < SOI_MASL;
                        bool Pe_ok = FlightGlobals.getMainBody().atmosphereDepth < FlightGlobals.ship_orbit.PeA && FlightGlobals.ship_orbit.PeA < SOI_MASL;
                        string Ap = Formatter.Distance_k(FlightGlobals.ship_orbit.ApA);
                        string Pe = Formatter.Distance_k(FlightGlobals.ship_orbit.PeA);
                        string Apsises = String.Format("<color={0}>{1}</color> <color={2}>{3}</color>",
                            Ap_ok ? "#00ff00ff" : "#00ff009f", Ap, 
                            Pe_ok ? "#00ff00ff" : "#00ff009f", Pe);

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

                            display.textTitle.text = String.Format("{0} <color={1}>{2}</color>",
                                Apsises, Apsis_ok ? "#ffffffff" : "#ffffff9f", TimeApsis);  
                        }
                        else
                        {
                            display.textTitle.text = Orb + Apsises;
                        }
                    }

                    display.textSpeed.text = FlightGlobals.ship_obtSpeed.ToString("F1") + mps;
                    break;

                case FlightGlobals.SpeedDisplayModes.Target:

                    #region all Target
                    // 1.5.1
                    // ITargetable ->  CelestialBody;
                    //                 FlightCoMTracker;
                    //                 ModuleDockingNode;
                    //                 PositionTarget;
                    //                 Vessel;
                    #endregion

                    ITargetable obj = FlightGlobals.fetch.VesselTarget;
                    if (obj == null)
                    {
                        display.textTitle.text = NoTrg;
                        return;
                    }

                    string distanceToTarget = "";

                    if (settings.targetDistance)
                        distanceToTarget = CalcTargetDistance(obj);


                    if (Target != obj)
                    {
                        TargetName = CutName(Trg + distanceToTarget, GetTargetName(obj));
                        Target = obj;
                    }

                    display.textTitle.text = Trg + distanceToTarget + TargetName;


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

