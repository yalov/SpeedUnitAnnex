using System;
using UnityEngine;
using UnityEngine.UIElements;
using KSP.UI.Screens.Flight;
using KSP.Localization;
using static SpeedUnitAnnex.Logging;
using System.Reflection;
using System.Collections.Generic;

namespace SpeedUnitAnnex
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SpeedUnitAnnex : MonoBehaviour
    {
        SpeedDisplay display;

        readonly float mphTOms = 2.23694f;
        readonly float kmphTOms = 3.6f;
        readonly float knTOms = 1 / 0.514f;

        readonly double Eva_CoM_fix_landed = -0.27;
        readonly double Eva_CoM_fix_splashed = 0.21;

        readonly double BoatSubmarineBorderAlt = -20;

        readonly double epsilon = 0.01;

        readonly string MachNum = Localizer.Format("#SpeedUnitAnnex_machNumber");
        readonly string knots   = Localizer.Format("#SpeedUnitAnnex_knots");
        readonly string kmph    = Localizer.Format("#SpeedUnitAnnex_kmph");
        readonly string mph     = Localizer.Format("#SpeedUnitAnnex_mph");

        readonly string kn_s    = " " + Localizer.Format("#SpeedUnitAnnex_kn");
        readonly string knots_s = " " + Localizer.Format("#SpeedUnitAnnex_knots");
        readonly string kmph_s  = " " + Localizer.Format("#SpeedUnitAnnex_kmph");
        readonly string mps_s   = " " + Localizer.Format("#SpeedUnitAnnex_mps");
        readonly string mph_s   = " " + Localizer.Format("#SpeedUnitAnnex_mph");

        // readonly string Orb     = String.Format("<color={0}>{1}</color> ", "#ffffffff", Localizer.Format("#SpeedUnitAnnex_Orb"));
        // readonly string Surf3   = String.Format("<color={0}>{1}</color> ", "#ffffffff", Localizer.Format("#SpeedUnitAnnex_Surf3"));
        // readonly string Surf5   = String.Format("<color={0}>{1}</color> ", "#ffffffff", Localizer.Format("#SpeedUnitAnnex_Surf5"));
        // readonly string Surface = String.Format("<color={0}>{1}</color> ", "#ffffffff", Localizer.Format("#autoLOC_7001218"));
        // readonly string Trg     = String.Format("<color={0}>{1}</color> ", "#ffffffff", Localizer.Format("#SpeedUnitAnnex_Trg"));
        // readonly string NoTrg   = String.Format("<color={0}>{1}</color>", "#ffffffff", Localizer.Format("#autoLOC_339139")); // No Target

        readonly string Orb       = Localizer.Format("#SpeedUnitAnnex_Orb") +" ";
        readonly string Orb_full  = Localizer.Format("#autoLOC_7001217");
        readonly string Surf3     = Localizer.Format("#SpeedUnitAnnex_Surf3") + " ";
        readonly string Surf5     = Localizer.Format("#SpeedUnitAnnex_Surf5") + " ";
        readonly string Surf_full = Localizer.Format("#autoLOC_7001218");
        readonly string Trg       = Localizer.Format("#SpeedUnitAnnex_Trg") + " ";
        readonly string NoTrg     = Localizer.Format("#autoLOC_339139"); // No Target

        
        readonly string A_prefix = Localizer.Format("#SpeedUnitAnnex_ApoapsisTime_prefix");
        readonly string P_prefix = Localizer.Format("#SpeedUnitAnnex_PeriapsisTime_prefix");

        readonly string ApT_prefix = Localizer.Format("#autoLOC_6003115") + Localizer.Format("#SpeedUnitAnnex_Time_prefix");
        readonly string PeT_prefix = Localizer.Format("#autoLOC_6003116") + Localizer.Format("#SpeedUnitAnnex_Time_prefix");

        readonly string Jeb_full  = Localizer.Format("#autoLOC_20803");
        readonly string Val_full = Localizer.Format("#autoLOC_20827");
        readonly string Jeb_short = Localizer.Format("#SpeedUnitAnnex_JebShort");
        readonly string Val_short = Localizer.Format("#SpeedUnitAnnex_ValShort");

        readonly char[] delimiterChars = Localizer.Format("#SpeedUnitAnnex_DelimiterChars").ToCharArray();

        ITargetable Target = null;
        string TargetName;

        string FinalName;
        SpeedUnitAnnexSettings settings;
        SpeedUnitAnnexSettings2 settings2;

        bool isLoadedFAR = false;
        //bool isDisabledFARDisplay = false;

        private delegate bool FAR_ToggleAirspeedDisplayDelegate(bool? enabled=null, Vessel v=null);
        private static FAR_ToggleAirspeedDisplayDelegate FAR_ToggleAirspeedDisplay;

        private delegate double FAR_ActiveVesselIASDelegate();
        private static FAR_ActiveVesselIASDelegate FAR_ActiveVesselIAS;
        // FAR_EAS also available

        private void CreateFARReflections()
        {
            isLoadedFAR = ReflectionUtils.IsAssemblyLoaded("FerramAerospaceResearch");
            if (isLoadedFAR)
            {
                var FAR_ToggleAirspeedDisplayMethodInfo = ReflectionUtils.GetMethodByReflection(
                    "FerramAerospaceResearch", "FerramAerospaceResearch.FARAPI",
                    "ToggleAirspeedDisplay",
                    BindingFlags.Public | BindingFlags.Static,
                    new Type[] { typeof(bool?), typeof(Vessel) }
                );

                var FAR_ActiveVesselIASMethodInfo = ReflectionUtils.GetMethodByReflection(
                    "FerramAerospaceResearch", "FerramAerospaceResearch.FARAPI",
                    "ActiveVesselIAS",
                    BindingFlags.Public | BindingFlags.Static,
                    new Type[] { }
                );

                if (FAR_ToggleAirspeedDisplayMethodInfo == null)
                {
                    Log("FAR loaded, but FAR API has no ToggleAirspeedDisplay method. " +
                        "Do you have FAR at least v0.15.10.0? Disabling FAR-support.");
                    isLoadedFAR = false;
                }
                else
                {
                    FAR_ToggleAirspeedDisplay = (FAR_ToggleAirspeedDisplayDelegate)Delegate.CreateDelegate(
                        typeof(FAR_ToggleAirspeedDisplayDelegate), FAR_ToggleAirspeedDisplayMethodInfo);
                }
                

                if (FAR_ActiveVesselIASMethodInfo == null)
                {
                    Log("FAR loaded, but FAR API has no ActiveVesselIAS method. " +
                        "Do you have FAR at least v0.15.10.0? Disabling FAR-support.");
                    isLoadedFAR = false;
                }
                else
                {
                    FAR_ActiveVesselIAS = (FAR_ActiveVesselIASDelegate)Delegate.CreateDelegate(
                        typeof(FAR_ActiveVesselIASDelegate), FAR_ActiveVesselIASMethodInfo);
                }
            }
        }

        private void ToggleFARDisplay()
        {
            if (isLoadedFAR)
            {
                bool success = FAR_ToggleAirspeedDisplay(!settings.overrideFAR);
                //Log("{0}FARDisplay : {1}", settings.overrideFAR ? "Disable" : "Enable", success);
            }
        }

        public SpeedUnitAnnex()
        {
            
        }

        double AGL()
        {
            return FlightGlobals.ship_altitude - FlightGlobals.ActiveVessel.terrainAltitude;
        }

        double RadarAltitude()
        {
            // FlightGlobals.ActiveVessel.pqsAltitude;
            

            if (FlightGlobals.ActiveVessel.terrainAltitude < 0.0) // on/over/under/water
            {
                if (FlightGlobals.ActiveVessel.altitude > BoatSubmarineBorderAlt)
                    return FlightGlobals.ship_altitude;
                else
                    return FlightGlobals.ship_altitude - FlightGlobals.ActiveVessel.terrainAltitude;
            }
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

        // return signed angle in relation to normal's 2d plane
        // from Docking Port Alignment Indicator
        private static float AngleAroundNormal(Vector3 a, Vector3 b, Vector3 up)
        {
            return AngleSigned(Vector3.Cross(up, a), Vector3.Cross(up, b), up);
        }


        // -180 to 180 angle
        // from Docking Port Alignment Indicator
        private static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 up)
        {
            if (Vector3.Dot(Vector3.Cross(v1, v2), up) < 0) //greater than 90 i.e v1 left of v2
                return -Vector3.Angle(v1, v2);
            return Vector3.Angle(v1, v2);
        }


        Vector3 GetOrientationDeviation(ITargetable obj)
        {
            Transform selfTransform = FlightGlobals.ActiveVessel.ReferenceTransform;
            Transform targetTransform = obj.GetTransform();

            Vector3 targetPortOutVector = targetTransform.forward.normalized;
            Vector3 targetPortRollReferenceVector = -targetTransform.up;

            Vector3 orientationDeviation = new Vector3
            {
                x = AngleAroundNormal(-targetPortOutVector, selfTransform.up, selfTransform.forward),
                y = AngleAroundNormal(-targetPortOutVector, selfTransform.up, -selfTransform.right),
                z = AngleAroundNormal(targetPortRollReferenceVector, selfTransform.forward, selfTransform.up)
            };

            orientationDeviation.x = (360 + orientationDeviation.x) % 360;  // -90..270
            orientationDeviation.y = (360 + orientationDeviation.y) % 360;
            orientationDeviation.z = (360 - orientationDeviation.z) % 360;
            return orientationDeviation;
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
                    if (FlightGlobals.ActiveVessel.vesselType == VesselType.EVA && settings2.orbit_EVA)
                        FinalName = CutKerbalName(settings2.orbit_time ? "" : Orb, FlightGlobals.ActiveVessel, false);
                    break;
                case FlightGlobals.SpeedDisplayModes.Target:
                    Target = null;
                    break;
            }
        }


        private string CutKerbalName(string prefix, Vessel kerbal, bool cut_orange_names = true)
        {
            if (kerbal == null || kerbal.vesselType != VesselType.EVA) return "";

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
            if (display.textTitle.GetPreferredValues(prefix + name).x > 108)
            { 
                while (display.textTitle.GetPreferredValues(prefix + name).x > 108 && name.Length>0)
                {
                    name = name.Substring(0, name.Length - 1);
                }
                name = name.Trim() + ".";
            }
            return name + " ";
        }
        

        void OnVesselChange(Vessel vessel)
        {
            //Log("onVesselChange: " + vessel.GetDisplayName());
            SetFinalName(FlightGlobals.speedDisplayMode);
        }


        void OnSetSpeedMode(FlightGlobals.SpeedDisplayModes mode)
        {
            //Log("onSetSpeedMode: " + mode.displayDescription());
            SetFinalName(mode);
        }


        public void OnGameUnpause()
        {
            //Log("OnGameUnpause");
            SetFinalName(FlightGlobals.speedDisplayMode);
            settings = HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>();
            ToggleFARDisplay();
        }


        public void OnDisable()
        {
            G﻿ameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onSetSpeedMode.Remove(OnSetSpeedMode);
            G﻿ameEvents.onGameUnpause.Remove(OnGameUnpause);
        }


        public void Start()
        {
            G﻿ameEvents.onVesselChange.Add(OnVesselChange);
            G﻿ameEvents.onSetSpeedMode.Add(OnSetSpeedMode);
            G﻿ameEvents.onGameUnpause.Add(OnGameUnpause);

            display = GameObject.FindObjectOfType<SpeedDisplay>();
            settings = HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>();
            settings2 = HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings2>();

            if (settings.overrideFAR)
            {
                CreateFARReflections();
                ToggleFARDisplay();
            }

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
            if (settings == null) settings = HighLogic.CurrentGame.Parameters.CustomParams<SpeedUnitAnnexSettings>();

            //Log("{0:F1}, {1:F1}={2:F1}, {3:F1}, {4:F1}={5:F1}",
            //    FlightGlobals.ActiveVessel.heightFromTerrain == FlightGlobals.ship_altitude - FlightGlobals.ActiveVessel.terrainAltitude, 
            //    FlightGlobals.ActiveVessel.heightFromTerrain, FlightGlobals.ship_altitude - FlightGlobals.ActiveVessel.terrainAltitude,
            //    FlightGlobals.ship_altitude == FlightGlobals.ActiveVessel.altitude, FlightGlobals.ship_altitude, FlightGlobals.ActiveVessel.altitude);

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
                    string srfSpeedText = spd.ToString("F1") + mps_s;

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
                                        + "  " + (spd * knTOms).ToString("F1") + kn_s;
                                // Boat
                                else
                                    titleText = Surf5 + (spd * knTOms).ToString("F1") + knots_s;
                            }
                            // Plane (not LANDED) 
                            else if (vesselType == VesselType.Plane &&
                                situation != Vessel.Situations.LANDED && situation != Vessel.Situations.PRELAUNCH)
                            {
                                bool isATM = FlightGlobals.ActiveVessel.atmDensity > 0.0;

                                if (settings.radar)
                                {
                                    double radar = FlightGlobals.ActiveVessel.radarAltitude;

                                    if (settings.aircraft == MachNum)
                                    {
                                        if (isATM)
                                            titleText = Surf3 + Formatter.Distance_short(radar) + "  "
                                                + Localizer.Format("#SpeedUnitAnnex_mach", FlightGlobals.ActiveVessel.mach.ToString("F1"));

                                        else titleText = Surf5 + Formatter.Distance_long(radar);
                                    }
                                    else if (settings.aircraft == knots)
                                        titleText = Surf3 + Formatter.Distance_short(radar) + "  "
                                            + (spd * knTOms).ToString("F1") + kn_s;
                                    else if (settings.aircraft == kmph)
                                        titleText = Surf3 + Formatter.Distance_short(radar) + " "
                                            + (spd * kmphTOms).ToString("F1") + kmph_s;
                                    else // settings.aircraft == mph
                                        titleText = Surf3 + Formatter.Distance_short(radar) + " "
                                            + (spd * mphTOms).ToString("F1") + mph_s;
                                }
                                else
                                {
                                    if (settings.aircraft == MachNum)
                                    {
                                        if (isATM)
                                            titleText = Surf5 + Localizer.Format("#SpeedUnitAnnex_mach", FlightGlobals.ActiveVessel.mach.ToString("F1"));
                                        else titleText = Surf_full;
                                    }
                                    else if (settings.aircraft == knots)
                                        titleText = Surf5 + (spd * knTOms).ToString("F1") + knots_s;
                                    else if (settings.aircraft == kmph)
                                        titleText = Surf5 + (spd * kmphTOms).ToString("F1") + kmph_s;
                                    else // settings.aircraft == mph
                                        titleText = Surf5 + (spd * mphTOms).ToString("F1") + mph_s;
                                }

                                if (settings.ias)
                                {
                                    double speedIAS = 0;

                                    if (isLoadedFAR) speedIAS = FAR_ActiveVesselIAS();
                                    else             speedIAS = FlightGlobals.ActiveVessel.indicatedAirSpeed;

                                    if (speedIAS > 0)
                                        srfSpeedText += " " + speedIAS.ToString("F1");
                                }
                            }
                            // Rover (and LANDED Plane)
                            else
                            {
                                if (settings.rover == kmph)
                                    titleText = Surf5 + (spd * kmphTOms).ToString("F1") + kmph_s;
                                else // settings.rover == mph
                                    titleText = Surf5 + (spd * mphTOms).ToString("F1") + mph_s;
                            }

                            break;

                        case VesselType.EVA:
                            titleText = Surf3 + (settings.radar ? RadarAltitudeEVA_str() : "") + FinalName;
                            break;

                        case VesselType.Flag:
                            titleText = Surf3 + FinalName;
                            break;

                        // Other: Rocket, Lander, Base etc 
                        default:

                            if (settings.radar)
                                titleText = Surf5 + Formatter.Distance_long(RadarAltitude());
                            else
                                titleText = Surf_full;
                            break;
                    }

                    display.textTitle.text = titleText;
                    display.textSpeed.text = srfSpeedText;
                    if (settings.vertical_indicator)
                    {
                        if (FlightGlobals.ship_verticalSpeed < -epsilon)
                            display.textSpeed.color = Color.red;
                        else
                            display.textSpeed.color = Color.green;
                    }
                    Log(FlightGlobals.ship_verticalSpeed);
                    break;

                case FlightGlobals.SpeedDisplayModes.Orbit:

                    if (FlightGlobals.ActiveVessel.vesselType == VesselType.EVA
                        && settings2.orbit_EVA)
                    {
                        display.textTitle.text = (settings2.orbit_ApPe && settings2.orbit_time ? "" : Orb) + FinalName;
                    }
                    else if (settings2.orbit_ApPe || settings2.orbit_time)
                    {
                        double SOI_MASL = FlightGlobals.getMainBody().sphereOfInfluence - FlightGlobals.getMainBody().Radius;
                        bool Ap_ok = FlightGlobals.getMainBody().atmosphereDepth < FlightGlobals.ship_orbit.ApA && FlightGlobals.ship_orbit.ApA < SOI_MASL;
                        bool Pe_ok = FlightGlobals.getMainBody().atmosphereDepth < FlightGlobals.ship_orbit.PeA && FlightGlobals.ship_orbit.PeA < SOI_MASL;
                        

                        if (settings2.orbit_ApPe)
                        {
                            string Ap = Formatter.Distance_k(FlightGlobals.ship_orbit.ApA);
                            string Pe = Formatter.Distance_k(FlightGlobals.ship_orbit.PeA);
                            string Apsises = String.Format("<color={0}>{1}</color> <color={2}>{3}</color>",
                                Ap_ok ? "#00ff00ff" : "#00ff009f", Ap,
                                Pe_ok ? "#00ff00ff" : "#00ff009f", Pe);

                            string TimeApsis;
                            bool Apsis_ok;

                            if (settings2.orbit_time)
                            {
                                if (FlightGlobals.ship_orbit.timeToAp < FlightGlobals.ship_orbit.timeToPe)
                                {
                                    Apsis_ok = Ap_ok;
                                    TimeApsis = Formatter.Time(FlightGlobals.ship_orbit.timeToAp, A_prefix);
                                }
                                else
                                {
                                    Apsis_ok = Pe_ok;
                                    TimeApsis = Formatter.Time(FlightGlobals.ship_orbit.timeToPe, P_prefix);
                                }
                                display.textTitle.text = String.Format("{0} <color={1}>{2}</color>",
                                Apsises, Apsis_ok ? "#ffffffff" : "#ffffff9f", TimeApsis);
                            }
                            else
                                display.textTitle.text = Orb + Apsises;


                        }
                        else if (settings2.orbit_time)
                        {
                            string TimeApsis;
                            bool Apsis_ok;

                            if (FlightGlobals.ship_orbit.timeToAp < FlightGlobals.ship_orbit.timeToPe)
                            {
                                Apsis_ok = Ap_ok;
                                TimeApsis = Formatter.TimeLong(FlightGlobals.ship_orbit.timeToAp, ApT_prefix);
                            }
                            else
                            {
                                Apsis_ok = Pe_ok;
                                TimeApsis = Formatter.TimeLong(FlightGlobals.ship_orbit.timeToPe, PeT_prefix);
                            }
                            display.textTitle.text = String.Format("<color={0}>{1}</color>",
                            Apsis_ok ? "#00ff00ff" : "#00ff009f", TimeApsis);
                        }
                    }
                    
                    else
                    {
                        display.textTitle.text = Orb_full;
                    }

                    display.textSpeed.text = FlightGlobals.ship_obtSpeed.ToString("F1") + mps_s;
                    break;

                case FlightGlobals.SpeedDisplayModes.Target:
                    
                    #region all Target
                    // 1.6.0
                    // ITargetable:
                    //  * CelestialBody
                    //  * FlightCoMTracker
                    //  * ModuleDockingNode
                    //  * PositionTarget:
                    //    * DirectionTarget
                    //  * Vessel
                    #endregion

                    ITargetable obj = FlightGlobals.fetch.VesselTarget;

                    if (obj == null)
                    {
                        display.textTitle.text = NoTrg;
                        return;
                    }

                    bool isMDN = obj is ModuleDockingNode;
                    

                    if (settings2.targetAngles && isMDN)
                    {
                        Vector3 angles = GetOrientationDeviation(obj);

                        if (settings2.targetInteger)
                            display.textTitle.text = Trg +
                                Formatter.Angle(angles.x, true, 5) +
                                Formatter.Angle(angles.y, true, 5) +
                                Formatter.Angle(angles.z, true, 5);
                        else
                            display.textTitle.text = Formatter.Angle(angles.x) + Formatter.Angle(angles.y) + Formatter.Angle(angles.z);

                    }
                    else
                    {
                        string TargetAngle = "";
                        string distanceToTarget = "";

                        if (settings2.targetAngle && isMDN)
                        {
                            Vector3 angles = GetOrientationDeviation(obj);
                            if (settings2.targetInteger)
                                TargetAngle = Formatter.Angle(angles.z, true, 5);
                            else
                                TargetAngle = Formatter.Angle(angles.z);
                        }

                        if (settings2.targetDistance)
                            distanceToTarget = CalcTargetDistance(obj);

                        bool isAngleAndDistance = settings2.targetDistance && settings2.targetAngle && isMDN;

                        if (settings2.targetName && !isAngleAndDistance)
                        {
                            if (Target != obj)
                            {
                                TargetName = CutName(Trg + distanceToTarget + TargetAngle, GetTargetName(obj));
                                Target = obj;
                            }
                            display.textTitle.text = Trg + distanceToTarget + TargetName + TargetAngle;
                        }
                        else
                            display.textTitle.text = Trg + distanceToTarget +" " + TargetAngle; // 2 spaces
                    }

                    if (FlightGlobals.ship_tgtSpeed < 0.195)
                        display.textSpeed.text = FlightGlobals.ship_tgtSpeed.ToString("F2") + mps_s;
                    else
                        display.textSpeed.text = FlightGlobals.ship_tgtSpeed.ToString("F1") + mps_s;
                    break;
            }

            // need to be there, for every tick. Doesn't work in the  Start() or onSetSpeedMode()
            display.textTitle.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        }
    }
}
