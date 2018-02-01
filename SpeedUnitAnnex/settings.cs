using System.Collections;
using System.Reflection;
using KSP.Localization;

namespace SpeedUnitAnnex
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class SpeedUnitAnnexSettings : GameParameters.CustomParameterNode
    {

        public override string Title { get { return Localizer.Format("#SpeedUnitAnnex_navball_info") ; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Speed Unit Annex"; } }
        public override string DisplaySection { get { return "Speed Unit Annex"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }

        [GameParameters.CustomStringParameterUI("#SpeedUnitAnnex_surfaceMode", lines = 2, title = "#SpeedUnitAnnex_surfaceMode")]
        public string UIstring1 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_rover_speedometer", toolTip = "#SpeedUnitAnnex_rover_speedometer_toolTip")]
        public bool setting_kmph = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_aircraft_speedometer", toolTip = "#SpeedUnitAnnex_aircraft_speedometer_toolTip")]
        public bool setting_mach = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_altimeter", toolTip = "#SpeedUnitAnnex_altimeter_toolTip")]
        public bool setting_radar = true;

        [GameParameters.CustomStringParameterUI("#SpeedUnitAnnex_orbitMode", lines = 2, title = "#SpeedUnitAnnex_orbitMode")]
        public string UIstring2 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_Ap", toolTip = "#SpeedUnitAnnex_Ap_toolTip")]
        public bool setting_orbit = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_orbitEVA", toolTip = "#SpeedUnitAnnex_orbitEVA_toolTip")]
        public bool setting_orbit_EVA = true;

        [GameParameters.CustomStringParameterUI("#SpeedUnitAnnex_targetMode", lines = 2, title = "#SpeedUnitAnnex_targetMode")]
        public string UIstring3 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetDistance", toolTip = "#SpeedUnitAnnex_targetDistance_toolTip")]
        public bool setting_targetDistance = true;

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            setting_kmph = true;
            setting_mach = true;
            setting_radar = true;
            setting_orbit = true;
            setting_orbit_EVA = true;
            setting_targetDistance = true;
        }

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
            return null;
        }
    }
}
