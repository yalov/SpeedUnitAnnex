using System.Collections;
using System.Reflection;
using KSP.Localization;

namespace SpeedUnitDash
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class SpeedUnitDashSettings : GameParameters.CustomParameterNode
    {

        public override string Title { get { return Localizer.Format("#SpeedUnitDash_navball_info") ; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "SpeedUnitDash"; } }
        public override string DisplaySection { get { return "SpeedUnitDash"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }

        [GameParameters.CustomStringParameterUI("#SpeedUnitDash_surfaceMode", lines = 2, title = "#SpeedUnitDash_surfaceMode")]
        public string UIstring1 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitDash_speedometer", toolTip = "#SpeedUnitDash_speedometer_toolTip")]
        public bool kph = true;

        [GameParameters.CustomParameterUI("#SpeedUnitDash_altimeter", toolTip = "#SpeedUnitDash_altimeter_toolTip")]
        public bool radar = true;

        [GameParameters.CustomStringParameterUI("#SpeedUnitDash_orbitMode", lines = 2, title = "#SpeedUnitDash_orbitMode")]
        public string UIstring2 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitDash_Ap", toolTip = "#SpeedUnitDash_Ap_toolTip")]
        public bool orbit = true;

        [GameParameters.CustomStringParameterUI("#SpeedUnitDash_targetMode", lines = 2, title = "#SpeedUnitDash_targetMode")]
        public string UIstring3 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitDash_targetName", toolTip = "#SpeedUnitDash_targetName_toolTip")]
        public bool targetName = true;

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            kph = true;
            radar = true;
            orbit = true;
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
