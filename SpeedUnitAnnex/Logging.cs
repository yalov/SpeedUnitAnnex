using System;
using System.Globalization;
using UnityEngine;

namespace SpeedUnitAnnex
{
    public static class Logging
    {
        private static readonly string PREFIX = "<color=green>[SpeedUnitAnnex]</color> ";
        internal static String _AssemblyName { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; } }
        private static readonly bool time = false;

        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(params object[] args)
        {
            Log(args);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogFormatDebug(string msg, params object[] args)
        {
            LogFormat(msg, args);
        }

        public static void Log(params object[] args)
        {
            Debug.Log(PREFIX + (time ? DateTime.Now.ToString("HH:mm:ss.f ") : "") +
                String.Join(", ", args)
                );
        }

        public static void LogFormat(string msg, params object[] args)
        {
            Debug.LogFormat(PREFIX + (time ? DateTime.Now.ToString("HH:mm:ss.f ") : "") +
                msg, args);
        }
    }
}
