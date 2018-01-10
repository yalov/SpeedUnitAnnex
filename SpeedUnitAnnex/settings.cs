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
        public override string Section { get { return "SpeedUnitAnnex"; } }
        public override string DisplaySection { get { return "SpeedUnitAnnex"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }

        [GameParameters.CustomStringParameterUI("#SpeedUnitAnnex_surfaceMode", lines = 2, title = "#SpeedUnitAnnex_surfaceMode")]
        public string UIstring1 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_speedometer", toolTip = "#SpeedUnitAnnex_speedometer_toolTip")]
        public bool kph = true;

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_altimeter", toolTip = "#SpeedUnitAnnex_altimeter_toolTip")]
        public bool radar = true;

        [GameParameters.CustomStringParameterUI("#SpeedUnitAnnex_orbitMode", lines = 2, title = "#SpeedUnitAnnex_orbitMode")]
        public string UIstring2 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_Ap", toolTip = "#SpeedUnitAnnex_Ap_toolTip")]
        public bool orbit = true;

        [GameParameters.CustomStringParameterUI("#SpeedUnitAnnex_targetMode", lines = 2, title = "#SpeedUnitAnnex_targetMode")]
        public string UIstring3 = "";

        [GameParameters.CustomParameterUI("#SpeedUnitAnnex_targetName", toolTip = "#SpeedUnitAnnex_targetName_toolTip")]
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
