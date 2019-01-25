using System;
using System.Collections.Generic;
using KSP.Localization;
using UnityEngine;

namespace SpeedUnitAnnex
{
    static class Formatter
    {
        private static string m = Localizer.Format("#SpeedUnitAnnex_meter");
        private static string k = Localizer.Format("#SpeedUnitAnnex_kilo");
        private static string M = Localizer.Format("#SpeedUnitAnnex_mega");
        private static string G = Localizer.Format("#SpeedUnitAnnex_giga");
        private static string T = Localizer.Format("#SpeedUnitAnnex_tera");

        private static string[] SI = { m, k, M, G, T };

        private static string Mm = Localizer.Format("#SpeedUnitAnnex_mega") + Localizer.Format("#SpeedUnitAnnex_meter");
        private static string Gm = Localizer.Format("#SpeedUnitAnnex_giga") + Localizer.Format("#SpeedUnitAnnex_meter");

        private static string sec_str = Localizer.Format("#SpeedUnitAnnex_sec");
        private static string min_str = Localizer.Format("#SpeedUnitAnnex_min");
        private static string hour_str = Localizer.Format("#SpeedUnitAnnex_hour");
        private static string day_str = Localizer.Format("#SpeedUnitAnnex_day");
        private static string year_str = Localizer.Format("#SpeedUnitAnnex_year");

        //private System.Globalization.CultureInfo culture =
        //    System.Globalization.CultureInfo.CreateSpecificCulture(KSP.Localization.Localizer.CurrentLanguage);


        /// <summary>
        /// decimal_digits >= 0, mode = C|F|N
        /// </summary>
        private static string Truncate(double value, string mode = "F", int decimal_digits = 1)
        {
            double multiplier = Math.Pow(10, decimal_digits);
            return (Math.Truncate(multiplier * value) / multiplier).ToString(mode + decimal_digits);
        }

        public static string Time(double seconds, string prefix = "T-")
        {
            string str;

            if (Double.IsInfinity(seconds) || seconds < 0) return "";
            else str = prefix;

            if (seconds < 100)
                str += Truncate(seconds) + sec_str;

            else if (seconds < 3600)
            {
                if (seconds < 600)
                    str += Truncate(seconds / 60.0) + min_str;
                else
                    str += Truncate(seconds / 60.0, "F", 0) + min_str;
            }
            else if (seconds < KSPUtil.dateTimeFormatter.Day)
                str += Truncate(seconds / 3600.0) + hour_str;

            else if (seconds < KSPUtil.dateTimeFormatter.Year)
            {
                if (seconds < 10 * KSPUtil.dateTimeFormatter.Day)
                    str += Truncate(seconds / KSPUtil.dateTimeFormatter.Day) + day_str;
                else
                    str += Truncate(seconds / KSPUtil.dateTimeFormatter.Day, "F", 0) + day_str;
            }
            else
            {
                if (seconds < 100.0 * KSPUtil.dateTimeFormatter.Year)
                    str += Truncate(seconds / KSPUtil.dateTimeFormatter.Year) + year_str;
                else if (seconds < 1000.0 * KSPUtil.dateTimeFormatter.Year)
                    str += Truncate(seconds / KSPUtil.dateTimeFormatter.Year, "F", 0) + year_str;
                else
                    str += (seconds / KSPUtil.dateTimeFormatter.Year).ToString("0e0") + year_str;
            }

            return str;
        }

        /// <summary>
        /// convert value to Distance string with unit (lower unit is m — 999m 9.99k 99.9k 999k )
        /// </summary>
        public static string Distance_short(double value)
        {
            double v = Math.Abs(value);

            int i;
            for (i = 0; v >= 1000 && i < SI.Length - 1; i++)
                v /= 1000;

            if (v < 10) return Truncate(Math.Sign(value) * v, "F", 2) + SI[i];
            else if (v < 100) return Truncate(Math.Sign(value) * v, "F", 1) + SI[i];
            else if (v < 1000) return Truncate(Math.Sign(value) * v, "F", 0) + SI[i];
            else return value.ToString("0e0") + m;
        }

        /// <summary>
        /// convert value to Distance string with unit (lower unit is km — 0.99k 9.99k 99.9k 999k )
        /// </summary>
        public static string Distance_k(double value)
        {
            double v = Math.Abs(value);

            if (v < 1E4) return Truncate(value / 1E3, "F", 2) + k;

            int i;
            for (i = 0; v >= 1000 && i < SI.Length - 1; i++)
                v /= 1000;

            if (v < 10) return Truncate(Math.Sign(value) * v, "F", 2) + SI[i];
            else if (v < 100) return Truncate(Math.Sign(value) * v, "F", 1) + SI[i];
            else if (v < 1000) return Truncate(Math.Sign(value) * v, "F", 0) + SI[i];
            else return value.ToString("0e0") + m;
        }

        /// <summary>
        /// convert value to Distance string with unit (999.9m, 99,999,999m  999,999.9Mm  999,999.9Gm)
        /// </summary>
        public static string Distance_long(double value)
        {
            string str;
            double v = Math.Abs(value);
            //  KerbATM 70kм   Minmus 47M Moho 4G  Eeloo 113G  Plock 700G      
            //  Moon 400M      Mercury 60G Pluto 7300G
            if (v < 1E3)          // 0.0 m - 999.9 m
                str = Truncate(value, "N", 1) + " " + m;

            else if (v < 1E8)          // 1,000 m - 99,999,999 m
                str = Truncate(value, "N", 0) + " " + m;

            else if (v < 1E12)    // 100.0 Mm - 999,999.9 Mm
                str = Truncate(value / 1E6, "N", 1) + " " + Mm;

            else                  // 1,000.0 Gm - 999,999.9 Gm and beyond
                str = Truncate(value / 1E9, "N", 1) + " " + Gm;

            return str;
        }

        public static string Angle(double value)
        {
            return String.Format("{0:F1}\u00B0 ", value).PadLeft(7, '\u2007');

        }
        public static string Angle_Short(double value)
        {
            return String.Format("{0:F1}\u00B0 ", value);
            
        }
    }

}
