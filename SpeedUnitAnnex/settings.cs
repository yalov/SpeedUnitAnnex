using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using KSP.Localization;

namespace SpeedUnitAnnex
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class SpeedUnitAnnexSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return Localizer.Format("#SpeedUnitAnnex_navball_info"); } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Speed Unit Annex"; } }
        public override string DisplaySection { get { return "Speed Unit Annex"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }

        [GameParameters.CustomStringParameterUI("#SpeedUnitAnnex_surfaceMode", lines = 2)]
        public string UIstring1 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_rover_speedometer", toolTip = "#SpeedUnitAnnex_rover_speedometer_toolTip")]
        public string rover = Localizer.Format("#SpeedUnitAnnex_kmph");

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_aircraft_speedometer", toolTip = "#SpeedUnitAnnex_aircraft_speedometer_toolTip")]
        public string aircraft = Localizer.Format("#SpeedUnitAnnex_machNumber");

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_altimeter", toolTip = "#SpeedUnitAnnex_altimeter_toolTip")]
        public bool radar = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_vertical_indicator", toolTip = "#SpeedUnitAnnex_vertical_indicator_toolTip")]
        public bool vertical_indicator = false;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_aircraft_ias", toolTip = "#SpeedUnitAnnex_aircraft_ias_toolTip")]
        public bool ias = false;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_override_FAR", toolTip = "#SpeedUnitAnnex_override_FAR_toolTip")]
        public bool overrideFAR = true;

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            return true;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
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

    public class SpeedUnitAnnexSettings2 : GameParameters.CustomParameterNode
    {
        public override string Title { get { return Localizer.Format("#SpeedUnitAnnex_navball_info"); } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Speed Unit Annex"; } }
        public override string DisplaySection { get { return "Speed Unit Annex"; } }
        public override int SectionOrder { get { return 2; } }
        public override bool HasPresets { get { return false; } }
        

        [GameParameters.CustomStringParameterUI("#SpeedUnitAnnex_orbitMode", lines = 2)]
        public string UIstring2 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_orbitEVA", toolTip = "#SpeedUnitAnnex_orbitEVA_toolTip")]
        public bool orbit_EVA = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_orbitApPe", toolTip = "#SpeedUnitAnnex_orbitApPe_toolTip")]
        public bool orbit_ApPe = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_orbitTime", toolTip = "#SpeedUnitAnnex_orbitTime_toolTip")]
        public bool orbit_time = false;


        [GameParameters.CustomStringParameterUI("#SpeedUnitAnnex_targetMode", lines = 2)]
        public string UIstring3 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetDistance", toolTip = "#SpeedUnitAnnex_targetDistance_toolTip")]
        public bool targetDistance = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetName", toolTip = "#SpeedUnitAnnex_targetName_toolTip")]
        public bool targetName = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetAngle", toolTip = "#SpeedUnitAnnex_targetAngle_toolTip")]
        public bool targetAngle = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetAngles", toolTip = "#SpeedUnitAnnex_targetAngles_toolTip")]
        public bool targetAngles = false;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetAnglesInteger", toolTip = "#SpeedUnitAnnex_targetAnglesInteger_toolTip")]
        public bool targetInteger = true;

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            return true;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "targetAngle")
                return !targetAngles;

            if (member.Name == "targetInteger")
                return targetAngles || targetAngle;

            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }
}
