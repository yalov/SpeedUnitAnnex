using System.Collections;
using System.Reflection;
using KSP.Localization;

namespace SpeedUnitDisplay
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class SpeedUnitDashSettings : GameParameters.CustomParameterNode
    {

        public override string Title { get { return Localizer.Format("#SpeedUnitDisplay_navball_info") ; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "SpeedUnitDisplay"; } }
        public override string DisplaySection { get { return "SpeedUnitDisplay"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }

        [GameParameters.CustomStringParameterUI("#SpeedUnitDisplay_surf", lines = 2, title = "#SpeedUnitDisplay_surf")]
        public string UIstring1 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitDisplay_speedometer", toolTip = "#SpeedUnitDisplay_speedometer_toolTip")]
        public bool kph = true;

        [GameParameters.CustomParameterUI("#SpeedUnitDisplay_altimeter", toolTip = "#SpeedUnitDisplay_altimeter_toolTip")]
        public bool radar = true;

        [GameParameters.CustomStringParameterUI("#SpeedUnitDisplay_orb", lines = 2, title = "#SpeedUnitDisplay_orb")]
        public string UIstring2 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitDisplay_Ap", toolTip = "#SpeedUnitDisplay_Ap_toolTip")]
        public bool orbit = true;

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
