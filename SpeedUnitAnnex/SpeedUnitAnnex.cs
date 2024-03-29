﻿using System;
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
        enum RoverSpeed
        {
            kmph,
            mph
        }

        enum AircraftSpeed
        {
            machNumber,
            knots,
            kmph,
            mph
        }

        enum TargetAngles
        {
            No,
            Roll,
            YawPitchRoll
        }

        enum TargetSpeedSplit
        {
            No,
            RCS,
            Always
        }

        RoverSpeed roverSpeed;
        AircraftSpeed aircraftSpeed;
        TargetAngles targetAngles;
        TargetSpeedSplit targetSpeedSplit;

        SpeedDisplay display;

        readonly float mphTOms = 2.23694f;
        readonly float kmphTOms = 3.6f;
        readonly float knTOms = 1 / 0.514f;

        readonly double Eva_CoM_fix_landed = -0.26;
        readonly double Eva_CoM_fix_splashed = 0.21;

        readonly double BoatSubmarineBorderAlt = -20;

        readonly double epsilon = 0.01;

        readonly string MachNum = Localizer.Format("#SpeedUnitAnnex_machNumber");
        readonly string knots   = Localizer.Format("#SpeedUnitAnnex_knots");
        readonly string kmph    = Localizer.Format("#SpeedUnitAnnex_kmph");
        readonly string mps     = Localizer.Format("#SpeedUnitAnnex_mps");

        readonly string kn_s    = " " + Localizer.Format("#SpeedUnitAnnex_kn");
        readonly string knots_s = " " + Localizer.Format("#SpeedUnitAnnex_knots");
        readonly string kmph_s  = " " + Localizer.Format("#SpeedUnitAnnex_kmph");
        readonly string mps_s   = " " + Localizer.Format("#SpeedUnitAnnex_mps");
        readonly string mph_s   = " " + Localizer.Format("#SpeedUnitAnnex_mph");

        readonly string Orb       = Localizer.Format("#SpeedUnitAnnex_Orb") +" ";
        readonly string Orb_full  = Localizer.Format("#autoLOC_7001217");
        readonly string Srf     = Localizer.Format("#SpeedUnitAnnex_Surf3") + " ";
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

        readonly Color orange = new Color(1f, 0.33f, 0f);
        readonly Color green = Color.green;

        readonly char[] delimiterChars = Localizer.Format("#SpeedUnitAnnex_DelimiterChars").ToCharArray();

        ITargetable Target = null;
        string TargetName;

        string FinalName;
        SUASettingsSurface settingsSurf;
        SUASettingsOrbit settingsOrb;
        SUASettingsTarget settingsTgt;

        FARReflections Reflections;

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

            //if (alt > -0.01 && alt < 0.01)
            //    alt = 0;

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
            => AngleSigned(Vector3.Cross(up, a), Vector3.Cross(up, b), up);
        


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
                name = Localizer.Format("#SpeedUnitAnnex_targetDP", obj.GetVessel().GetDisplayName());
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
                    if (FlightGlobals.ActiveVessel.vesselType == VesselType.EVA 
                        && !settingsOrb.orbit_EVAProp)
                    {
                        var prefix = Srf + (settingsSurf.radar ? RadarAltitudeEVA_str() : "");
                        FinalName = CutKerbalName(prefix, FlightGlobals.ActiveVessel);
                    }
                    else if (FlightGlobals.ActiveVessel.vesselType == VesselType.Flag)
                        FinalName = CutName(Srf, FlightGlobals.ActiveVessel.GetDisplayName());
                    break;

                case FlightGlobals.SpeedDisplayModes.Orbit:
                    if (FlightGlobals.ActiveVessel.vesselType == VesselType.EVA 
                        && settingsOrb.orbit_EVA && !settingsOrb.orbit_EVAProp)
                    {
                        var prefix = settingsOrb.orbit_time ? "" : Orb;
                        FinalName = CutKerbalName(prefix, FlightGlobals.ActiveVessel, false);
                    }
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

            switch (mode)
            {
                case FlightGlobals.SpeedDisplayModes.Orbit:
                    if (settingsSurf.color_vertical || settingsTgt.targetColor)
                        display.textSpeed.color = Color.green;
                    break;
                case FlightGlobals.SpeedDisplayModes.Surface:
                    if (!settingsSurf.color_vertical && settingsTgt.targetColor)
                        display.textSpeed.color = Color.green;
                    break;
                case FlightGlobals.SpeedDisplayModes.Target:
                    if (settingsSurf.color_vertical && !settingsTgt.targetColor)
                        display.textSpeed.color = Color.green;
                    break;
                default:
                    break;
            }
        }


        public void OnGameUnpause()
        {
            //Log("OnGameUnpause");
            SetFinalName(FlightGlobals.speedDisplayMode);
            settingsSurf = HighLogic.CurrentGame.Parameters.CustomParams<SUASettingsSurface>();
            if (settingsSurf.overrideFAR)
            {
                Reflections?.ToggleFARDisplay(!settingsSurf.overrideFAR);
            }
            setSettingsEnums();
        }

        public void setSettingsEnums()
        {
            if (settingsSurf.rover == kmph)
                roverSpeed = RoverSpeed.kmph;
            else // if (settings.rover == mph)
                roverSpeed = RoverSpeed.mph;


            if (settingsSurf.aircraft == kmph)
                aircraftSpeed = AircraftSpeed.kmph;
            else if (settingsSurf.aircraft == knots)
                aircraftSpeed = AircraftSpeed.knots;
            else if (settingsSurf.aircraft == MachNum)
                aircraftSpeed = AircraftSpeed.machNumber;
            else //if (settings.aircraft == mph)
                aircraftSpeed = AircraftSpeed.mph;

            if (settingsTgt.targetDockportAngles == Localizer.Format("#SpeedUnitAnnex_targetAnglesYawPitchRoll"))
                targetAngles = TargetAngles.YawPitchRoll;
            else if (settingsTgt.targetDockportAngles == Localizer.Format("#SpeedUnitAnnex_targetAnglesRoll"))
                targetAngles = TargetAngles.Roll;
            else // if (settingsTgt.targetDockportAngles == Localizer.Format("#SpeedUnitAnnex_targetAnglesNo"))
                targetAngles = TargetAngles.No;


            if (settingsTgt.targetSpeedSplit == Localizer.Format("#SpeedUnitAnnex_targetSpeedSplitAlways"))
                targetSpeedSplit = TargetSpeedSplit.Always; 
            else if (settingsTgt.targetSpeedSplit == Localizer.Format("#SpeedUnitAnnex_targetSpeedSplitRCS"))
                targetSpeedSplit = TargetSpeedSplit.RCS;
            else //if (settingsTgt.targetSpeedSplit == Localizer.Format("#SpeedUnitAnnex_targetSpeedSplitNo"))
                targetSpeedSplit = TargetSpeedSplit.No;

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
            settingsSurf = HighLogic.CurrentGame.Parameters.CustomParams<SUASettingsSurface>();
            settingsOrb = HighLogic.CurrentGame.Parameters.CustomParams<SUASettingsOrbit>();
            settingsTgt = HighLogic.CurrentGame.Parameters.CustomParams<SUASettingsTarget>();

            if (settingsSurf.overrideFAR)
            {
                Reflections = new FARReflections();
                
                Reflections.ToggleFARDisplay(!settingsSurf.overrideFAR);
            }

            display.textSpeed.enableWordWrapping = false;
            display.textTitle.enableWordWrapping = false;
            display.textTitle.fontSize = display.textTitle.fontSize / 1.15f;

            SetFinalName(FlightGlobals.speedDisplayMode);
            setSettingsEnums();

            //Log("Font: "+display.textSpeed.font);
            // NotoSans-Regular SDF
        }



        public void LateUpdate()
        {
            if (display == null) return;
            if (FlightGlobals.ActiveVessel == null) return;
            if (settingsSurf == null || settingsOrb == null || settingsTgt == null) return;

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
            //VesselType.DeployedGroundPart
            //VesselType.DeployedScienceController
            //VesselType.DeployedSciencePart
            //VesselType.DroppedPart
            #endregion

            switch (FlightGlobals.speedDisplayMode)
            {
                case FlightGlobals.SpeedDisplayModes.Surface:
                    {
                        string titleText;
                        VesselType vesselType = FlightGlobals.ActiveVessel.vesselType;
                        Vessel.Situations situation = FlightGlobals.ActiveVessel.situation;

                        double spd = FlightGlobals.ship_srfSpeed;
                        string speedText = spd.ToString("F1") + mps_s;

                        switch (vesselType)
                        {
                            case VesselType.Plane:
                            case VesselType.Rover:
                                // Boat or Submarine
                                if (situation == Vessel.Situations.SPLASHED)
                                {
                                    // Submarine
                                    if (FlightGlobals.ActiveVessel.altitude < BoatSubmarineBorderAlt && settingsSurf.radar)
                                        titleText = Srf + Formatter.Distance_short(AGL())
                                            + "  " + (spd * knTOms).ToString("F1") + kn_s;
                                    // Boat
                                    else
                                        titleText = Srf + (spd * knTOms).ToString("F1") + knots_s;
                                }
                                // Plane (not LANDED) 
                                else if (vesselType == VesselType.Plane &&
                                    situation != Vessel.Situations.LANDED && situation != Vessel.Situations.PRELAUNCH)
                                {

                                    double radar = FlightGlobals.ActiveVessel.radarAltitude;

                                    switch (aircraftSpeed)
                                    {
                                        case AircraftSpeed.machNumber:
                                            bool isATM = FlightGlobals.ActiveVessel.atmDensity > 0.0;
                                            if (isATM)
                                                titleText = Srf + (settingsSurf.radar ? Formatter.Distance_short(radar) + "  " : "")
                                                    + Localizer.Format("#SpeedUnitAnnex_mach", FlightGlobals.ActiveVessel.mach.ToString("F1"));
                                            else
                                                titleText = Srf + (settingsSurf.radar ? Formatter.Distance_long(radar) : "");
            
                                            break;
                                        case AircraftSpeed.knots:
                                            titleText = Srf + (settingsSurf.radar ? Formatter.Distance_short(radar) + "  " : "")
                                                + (spd * knTOms).ToString("F1") + kn_s;
                                            break;
                                        case AircraftSpeed.kmph:
                                            titleText = Srf + (settingsSurf.radar ? Formatter.Distance_short(radar) + "  " : "")
                                                + (spd * kmphTOms).ToString("F1") + kmph_s;
                                            break;
                                        case AircraftSpeed.mph:
                                            titleText = Srf + (settingsSurf.radar ? Formatter.Distance_short(radar) + "  " : "")
                                                + (spd * mphTOms).ToString("F1") + mph_s;
                                            break;
                                        default:
                                            titleText = Srf;
                                            break;
                                    }

                                    if (settingsSurf.ias)
                                    {
                                        double speedIAS = 0;

                                        if (Reflections?.isLoadedFAR ?? false)
                                            speedIAS = Reflections.FAR_ActiveVesselIAS();
                                        else
                                            speedIAS = FlightGlobals.ActiveVessel.indicatedAirSpeed;

                                        if (speedIAS > 0)
                                            speedText += " " + speedIAS.ToString("F1");
                                    }
                                }
                                // Rover (and LANDED Plane)
                                else
                                {
                                    switch (roverSpeed)
                                    {
                                        case RoverSpeed.kmph:
                                            titleText = Srf + (spd * kmphTOms).ToString("F1") + kmph_s;
                                            break;
                                        case RoverSpeed.mph:
                                            titleText = Srf + (spd * mphTOms).ToString("F1") + mph_s;
                                            break;
                                        default:
                                            titleText = Srf;
                                            break;
                                    }

                                }

                                break;

                            case VesselType.EVA:
                                titleText = Srf + (settingsSurf.radar ? RadarAltitudeEVA_str() : "");

                                if (settingsOrb.orbit_EVAProp)
                                {
                                    // FlightGlobals.ActiveVessel.evaController.FuelCapacity != 0
                                    if (FlightGlobals.ActiveVessel.evaController.HasJetpack)
                                        titleText += Localizer.Format("#SpeedUnitAnnex_evaProp",
                                            FlightGlobals.ActiveVessel.evaController.Fuel.ToString("F2")
                                            //,FlightGlobals.ActiveVessel.evaController.FuelCapacity
                                            );
                                }
                                else
                                {
                                    titleText += FinalName;
                                }

                                break;

                            case VesselType.Flag:
                                titleText = Srf + FinalName;
                                break;

                            // Other: Ship, Lander, Probe, Relay, Base, etc 
                            default:
                                titleText = Srf;

                                if (settingsSurf.radar)
                                {
                                    if (settingsSurf.rover_for_all)
                                        titleText += Formatter.Distance_short(RadarAltitude()) + " ";
                                    else
                                        titleText += Formatter.Distance_long(RadarAltitude()) + " ";
                                }
        
                                if (settingsSurf.rover_for_all)
                                {
                                    switch (roverSpeed)
                                    {
                                        case RoverSpeed.kmph:
                                            titleText += (spd * kmphTOms).ToString("F1") + kmph_s;
                                            break;
                                        case RoverSpeed.mph:
                                            titleText += (spd * mphTOms).ToString("F1") + mph_s;
                                            break;
                                    }
                                }
                                break;
                        }

                        if (settingsSurf.color_vertical)
                        {
                            if (FlightGlobals.ship_verticalSpeed < -epsilon)
                                display.textSpeed.color = orange;
                            else
                                display.textSpeed.color = green;
                        }

                        if (settingsSurf.split_vertical)
                        {
                            speedText = String.Format("{0:F1} {1} {2:F1}",
                                FlightGlobals.ActiveVessel.horizontalSrfSpeed,
                                mps,
                                FlightGlobals.ship_verticalSpeed
                            );
                        }

                        display.textTitle.text = titleText;
                        display.textSpeed.text = speedText;

                        break;
                    }

                case FlightGlobals.SpeedDisplayModes.Orbit:
                    {
                        string titleText;
                        if (FlightGlobals.ActiveVessel.vesselType == VesselType.EVA
                            && settingsOrb.orbit_EVA)
                        {
                            titleText = (settingsOrb.orbit_ApPe && settingsOrb.orbit_time ? "" : Orb);

                            if (settingsOrb.orbit_EVAProp)
                            {
                                if (FlightGlobals.ActiveVessel.evaController.HasJetpack)
                                    titleText += Localizer.Format("#SpeedUnitAnnex_evaProp",
                                        FlightGlobals.ActiveVessel.evaController.Fuel.ToString("F2"));
                            }
                            else
                            {
                                titleText += FinalName;
                            }
                        }
                        else if (settingsOrb.orbit_ApPe || settingsOrb.orbit_time)
                        {
                            double SOI_MASL = FlightGlobals.getMainBody().sphereOfInfluence - FlightGlobals.getMainBody().Radius;
                            bool Ap_ok = FlightGlobals.getMainBody().atmosphereDepth < FlightGlobals.ship_orbit.ApA && FlightGlobals.ship_orbit.ApA < SOI_MASL;
                            bool Pe_ok = FlightGlobals.getMainBody().atmosphereDepth < FlightGlobals.ship_orbit.PeA && FlightGlobals.ship_orbit.PeA < SOI_MASL;


                            if (settingsOrb.orbit_ApPe)
                            {
                                string Ap = Formatter.Distance_k(FlightGlobals.ship_orbit.ApA);
                                string Pe = Formatter.Distance_k(FlightGlobals.ship_orbit.PeA);
                                string Apsises = String.Format("<color={0}>{1}</color> <color={2}>{3}</color>",
                                    Ap_ok ? "#00ff00ff" : "#00ff009f", Ap,
                                    Pe_ok ? "#00ff00ff" : "#00ff009f", Pe);

                                string TimeApsis;
                                bool Apsis_ok;

                                if (settingsOrb.orbit_time)
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
                                    titleText = String.Format("{0} <color={1}>{2}</color>",
                                    Apsises, Apsis_ok ? "#ffffffff" : "#ffffff9f", TimeApsis);
                                }
                                else
                                    titleText = Orb + Apsises;


                            }
                            else if (settingsOrb.orbit_time)
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
                                titleText = String.Format("<color={0}>{1}</color>",
                                Apsis_ok ? "#00ff00ff" : "#00ff009f", TimeApsis);
                            }
                            else
                            {
                                titleText = Orb_full;
                            }

                        }
                        else
                        {
                            titleText = Orb_full;
                        }

                        display.textTitle.text = titleText;
                        display.textSpeed.text = FlightGlobals.ship_obtSpeed.ToString("F1") + mps_s;
                        break;
                    }

                case FlightGlobals.SpeedDisplayModes.Target:
                    {
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

                        string titleText;
                        string speedText;

                        ITargetable obj = FlightGlobals.fetch.VesselTarget;

                        if (obj == null)
                        {
                            titleText = NoTrg;
                            return;
                        }

                        bool isMDN = obj is ModuleDockingNode;


                        if (targetAngles == TargetAngles.YawPitchRoll && isMDN)
                        {
                            Vector3 angles = GetOrientationDeviation(obj);

                            if (settingsTgt.targetAngle1Decimal)
                            {
                                titleText =
                                    Formatter.Angle(angles.x, false) +
                                    Formatter.Angle(angles.y, false) +
                                    Formatter.Angle(angles.z, false);
                            }
                            else
                            {
                                titleText = Trg +
                                    Formatter.Angle(angles.x) +
                                    Formatter.Angle(angles.y) +
                                    Formatter.Angle(angles.z);
                            }
                        }
                        else
                        {
                            string TargetAngle = "";
                            string distanceToTarget = "";

                            if (targetAngles == TargetAngles.Roll && isMDN)
                            {
                                Vector3 angles = GetOrientationDeviation(obj);
                                if (settingsTgt.targetAngle1Decimal)
                                    TargetAngle = Formatter.Angle(angles.z, false);
                                else
                                    TargetAngle = Formatter.Angle(angles.z);

                            }

                            if (settingsTgt.targetDistance)
                                distanceToTarget = CalcTargetDistance(obj);

                            bool isAngleAndDistance = settingsTgt.targetDistance && targetAngles == TargetAngles.Roll && isMDN;

                            if (settingsTgt.targetName && !isAngleAndDistance)
                            {
                                if (Target != obj)
                                {
                                    TargetName = CutName(Trg + distanceToTarget + TargetAngle, GetTargetName(obj));
                                    Target = obj;
                                }
                                titleText = Trg + distanceToTarget + TargetName + TargetAngle;
                            }
                            else
                                titleText = Trg + distanceToTarget + " " + TargetAngle; // 2 spaces
                        }


                        bool isRCS = FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.RCS];

                        bool isSpeedSplit = targetSpeedSplit == TargetSpeedSplit.Always
                            || targetSpeedSplit == TargetSpeedSplit.RCS && isRCS;

                        if (settingsTgt.targetColor || isSpeedSplit)
                        {
                            Vector3 v = FlightGlobals.ship_tgtVelocity;
                            Vector3 vessel_pos = FlightGlobals.ActiveVessel.ReferenceTransform.position;
                            Vector3 tgt_pos = FlightGlobals.fetch.VesselTarget.GetTransform().position;
                            Vector3 diff = tgt_pos - vessel_pos;

                            Vector3 v_project = Vector3.Project(v, diff.normalized);
                            float s = v_project.magnitude * Math.Sign(Vector3.Dot(diff, v_project));

                            if (settingsTgt.targetColor)
                            {
                                if (s < 0) display.textSpeed.color = orange;
                                else display.textSpeed.color = green;
                            }

                            if (isSpeedSplit)
                            {
                                Vector3 v_nonproj = v - v_project;
                                speedText = String.Format("{0:F2} {1} {2:F2}", s, mps, v_nonproj.magnitude);
                            }
                            else
                            {
                                if (FlightGlobals.ship_tgtSpeed < 0.195)
                                    speedText = FlightGlobals.ship_tgtSpeed.ToString("F2") + mps_s;
                                else
                                    speedText = FlightGlobals.ship_tgtSpeed.ToString("F1") + mps_s;
                            }
                        }
                        else
                        {
                            if (FlightGlobals.ship_tgtSpeed < 0.195)
                                speedText = FlightGlobals.ship_tgtSpeed.ToString("F2") + mps_s;
                            else
                                speedText = FlightGlobals.ship_tgtSpeed.ToString("F1") + mps_s;
                        }
                        display.textTitle.text = titleText;
                        display.textSpeed.text = speedText;
                        break;
                    }
            }

            // need to be there, for every tick. Doesn't work in the  Start() or onSetSpeedMode()
            display.textTitle.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        }
    }
}
