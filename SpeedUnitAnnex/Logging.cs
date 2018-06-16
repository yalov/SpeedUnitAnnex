using System;
using UnityEngine;

namespace SpeedUnitAnnex
{
    public static class Logging
    {
        private static string PREFIX = "<color=green>SpeedUnitAnnex:</color> " ;

        public static void Log<T>(T msg)
        {
            Debug.Log(PREFIX + DateTime.Now.ToString("hh:mm:ss.f ") + msg.ToString());
        }
    }
}
