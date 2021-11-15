using System;
using System.Globalization;
using UnityEngine;

namespace SpeedUnitAnnex
{
    public static class Logging
    {
        private static readonly string PREFIX = "<color=green>[SpeedUnitAnnex]</color> ";
        private static readonly bool time = false;

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
