using System;
using System.Reflection;
using static SpeedUnitAnnex.Logging;

namespace SpeedUnitAnnex
{
    class ReflectionFAR
    {
        public bool isLoadedFAR = false;

        private delegate bool FAR_ToggleAirspeedDisplayDelegate(bool? enabled = null, Vessel v = null);
        private FAR_ToggleAirspeedDisplayDelegate FAR_ToggleAirspeedDisplay;

        public delegate double FAR_ActiveVesselIASDelegate();
        public FAR_ActiveVesselIASDelegate FAR_ActiveVesselIAS;

        public ReflectionFAR()
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

        public void ToggleFARDisplay(bool enable)
        {
            if (isLoadedFAR)
            {
                bool success = FAR_ToggleAirspeedDisplay(enable);
            }
        }
    }
}
