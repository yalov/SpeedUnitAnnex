using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using KSP.Localization;

namespace SpeedUnitAnnex
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class SUASettingsSurface : GameParameters.CustomParameterNode
    {
        public override string Title { get { return Localizer.Format("#SpeedUnitAnnex_surfaceMode"); } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Speed Unit Annex"; } }
        public override string DisplaySection { get { return "Speed Unit Annex"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_rover_speedometer", toolTip = "#SpeedUnitAnnex_rover_speedometer_toolTip")]
        public string rover = Localizer.Format("#SpeedUnitAnnex_kmph");

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_aircraft_speedometer", toolTip = "#SpeedUnitAnnex_aircraft_speedometer_toolTip")]
        public string aircraft = Localizer.Format("#SpeedUnitAnnex_machNumber");

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_altimeter", toolTip = "#SpeedUnitAnnex_altimeter_toolTip")]
        public bool radar = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_aircraft_ias", toolTip = "#SpeedUnitAnnex_aircraft_ias_toolTip")]
        public bool ias = false;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_override_FAR", toolTip = "#SpeedUnitAnnex_override_FAR_toolTip")]
        public bool overrideFAR = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_color_vertical", toolTip = "#SpeedUnitAnnex_color_vertical_toolTip")]
        public bool color_vertical = false;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_split_vertical", toolTip = "#SpeedUnitAnnex_split_vertical_toolTip")]
        public bool split_vertical = false;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_rover_for_all", toolTip = "#SpeedUnitAnnex_rover_for_all_toolTip")]
        public bool rover_for_all = false;

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            return true;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == nameof(ias))
                return !split_vertical;
            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            if (member.Name == "rover")
            {
                List<string> myList = new List<string>
                {
                    Localizer.Format("#SpeedUnitAnnex_kmph"),
                    Localizer.Format("#SpeedUnitAnnex_mph")
                };

                return myList;
            }
            else if (member.Name == "aircraft")
            {
                List<string> myList = new List<string>
                {
                    Localizer.Format("#SpeedUnitAnnex_machNumber"),
                    Localizer.Format("#SpeedUnitAnnex_knots"),
                    Localizer.Format("#SpeedUnitAnnex_kmph"),
                    Localizer.Format("#SpeedUnitAnnex_mph")
                };

                return myList;
            }
            else
            {
                return null;
            }
        }
    }

    public class SUASettingsOrbit : GameParameters.CustomParameterNode
    {
        public override string Title { get { return Localizer.Format("#SpeedUnitAnnex_orbitMode"); } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Speed Unit Annex"; } }
        public override string DisplaySection { get { return "Speed Unit Annex"; } }
        public override int SectionOrder { get { return 2; } }
        public override bool HasPresets { get { return false; } }


        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_orbitEVA", toolTip = "#SpeedUnitAnnex_orbitEVA_toolTip")]
        public bool orbit_EVA = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_orbitEVAProp", toolTip = "#SpeedUnitAnnex_orbitEVAProp_toolTip")]
        public bool orbit_EVAProp = false;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_orbitApPe", toolTip = "#SpeedUnitAnnex_orbitApPe_toolTip")]
        public bool orbit_ApPe = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_orbitTime", toolTip = "#SpeedUnitAnnex_orbitTime_toolTip")]
        public bool orbit_time = false;

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == nameof(orbit_EVAProp))
                return orbit_EVA;
            return true;
        }

    }


    public class SUASettingsTarget : GameParameters.CustomParameterNode
    {
        public override string Title { get { return Localizer.Format("#SpeedUnitAnnex_targetMode"); } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Speed Unit Annex"; } }
        public override string DisplaySection { get { return "Speed Unit Annex"; } }
        public override int SectionOrder { get { return 3; } }
        public override bool HasPresets { get { return false; } }


        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetDistance", toolTip = "#SpeedUnitAnnex_targetDistance_toolTip")]
        public bool targetDistance = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetName", toolTip = "#SpeedUnitAnnex_targetName_toolTip")]
        public bool targetName = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetDockportAngles", toolTip = "#SpeedUnitAnnex_targetDockportAngles_toolTip")]
        public string targetDockportAngles = Localizer.Format("#SpeedUnitAnnex_targetAnglesRoll");

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetAngles1Decimal", toolTip = "#SpeedUnitAnnex_targetAngles1Decimal_toolTip")]
        public bool targetAngle1Decimal = false;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetColor", toolTip = "#SpeedUnitAnnex_targetColor_toolTip")]
        public bool targetColor = false;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetSpeedSplit", toolTip = "#SpeedUnitAnnex_targetSpeedSplit_toolTip")]
        public string targetSpeedSplit = Localizer.Format("#SpeedUnitAnnex_targetSpeedSplitNo");

        //[GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetSpeedSplit", toolTip = "#SpeedUnitAnnex_targetSpeedSplit_toolTip")]
        //public bool targetSpeedSplit = false;


        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == nameof(targetAngle1Decimal))
                return targetDockportAngles != Localizer.Format("#SpeedUnitAnnex_targetNo");

            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            if (member.Name == "targetDockportAngles")
            {
                List<string> myList = new List<string>
                {
                    Localizer.Format("#SpeedUnitAnnex_targetAnglesNo"),
                    Localizer.Format("#SpeedUnitAnnex_targetAnglesRoll"),
                    Localizer.Format("#SpeedUnitAnnex_targetAnglesYawPitchRoll")
                };

                return myList;
            }
            else if (member.Name == "targetSpeedSplit")
            {
                List<string> myList = new List<string>
                {
                    Localizer.Format("#SpeedUnitAnnex_targetSpeedSplitNo"),
                    Localizer.Format("#SpeedUnitAnnex_targetSpeedSplitRCS"),
                    Localizer.Format("#SpeedUnitAnnex_targetSpeedSplitAlways")
                };
                return myList;
            }
            else
            {
                return null;
            }
        }
    }
}
