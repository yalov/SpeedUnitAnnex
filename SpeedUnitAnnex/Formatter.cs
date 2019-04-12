using System;
using System.Collections.Generic;
using KSP.Localization;
using UnityEngine;

namespace SpeedUnitAnnex
{
    static class Formatter
    {
        private static readonly string m = Localizer.Format("#SpeedUnitAnnex_meter");
        private static readonly string k = Localizer.Format("#SpeedUnitAnnex_kilo");
        private static readonly string M = Localizer.Format("#SpeedUnitAnnex_mega");
        private static readonly string G = Localizer.Format("#SpeedUnitAnnex_giga");
        private static readonly string T = Localizer.Format("#SpeedUnitAnnex_tera");
        private static readonly string P = Localizer.Format("#SpeedUnitAnnex_peta");
        private static readonly string E = Localizer.Format("#SpeedUnitAnnex_exa");
        private static readonly string Z = Localizer.Format("#SpeedUnitAnnex_zetta");
        private static readonly string Y = Localizer.Format("#SpeedUnitAnnex_yotta");

        private static readonly string[] SI = { m, k, M, G, T, P, E, Z, Y };

        private static readonly string Mm = Localizer.Format("#SpeedUnitAnnex_mega") + Localizer.Format("#SpeedUnitAnnex_meter");
        private static readonly string Gm = Localizer.Format("#SpeedUnitAnnex_giga") + Localizer.Format("#SpeedUnitAnnex_meter");

        private static readonly string sec_str = Localizer.Format("#SpeedUnitAnnex_sec");
        private static readonly string min_str = Localizer.Format("#SpeedUnitAnnex_min");
        private static readonly string hour_str = Localizer.Format("#SpeedUnitAnnex_hour");
        private static readonly string day_str = Localizer.Format("#SpeedUnitAnnex_day");
        private static readonly string year_str = Localizer.Format("#SpeedUnitAnnex_year");


        /// <summary>
        /// rounds value toward zero, 
        /// mode = F|N|C 
        /// F - FixedPoint
        /// N - Numeric (Group Separator)
        /// C - Currency
        /// decimal_digits >= 0,
        /// </summary>
        private static string Truncate(double value, string mode = "F", int decimal_digits = 1)
        {
            double multiplier = Math.Pow(10, decimal_digits);
            return (Math.Truncate(multiplier * value) / multiplier).ToString(mode + decimal_digits);
        }


        /// <summary>
        /// rounds value up, 
        /// mode = F|N|C 
        /// F - FixedPoint
        /// N - Numeric (Group Separator)
        /// C - Currency
        /// decimal_digits >= 0,
        /// </summary>
        private static string Ceiling(double value, string mode = "F", int decimal_digits = 1)
        {
            double multiplier = Math.Pow(10, decimal_digits);
            return (Math.Ceiling(multiplier * value) / multiplier).ToString(mode + decimal_digits);
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

        public static string TimeLong(double seconds, string prefix = "T-")
        {
            string str;

            if (Double.IsInfinity(seconds) || seconds < 0) return "";

            str = prefix;

            if (seconds < 60)
                str += Truncate(seconds) + sec_str;

            else if (seconds < 3600)
            {
                int s = (int)seconds;
                int min = s / 60;
                int sec = s % 60;
                str += $"{min}{min_str}, {sec}{sec_str}";
            }
            else if (seconds < KSPUtil.dateTimeFormatter.Day)
            {
                int s = (int)seconds;
                int hour = s / 3600;
                int min = s % 3600 / 60;
                int sec = s % 60;
                str += $"{hour}{hour_str}, {min}{min_str}, {sec}{sec_str}";
            }
            else if (seconds < 10* KSPUtil.dateTimeFormatter.Year)
            {
                int s = (int)seconds;
                int year = s / KSPUtil.dateTimeFormatter.Year;
                int days = s % KSPUtil.dateTimeFormatter.Year / KSPUtil.dateTimeFormatter.Day;
                int hour = s % KSPUtil.dateTimeFormatter.Day / 3600;
                int min = s % 3600 / 60;

                if (year == 0)
                    str += days + day_str + ", " +
                        (GameSettings.KERBIN_TIME ? hour.ToString() : hour.ToString("00")) + hour_str + ", " +
                        min.ToString("00") + min_str;
                else
                    str += year + year_str + ", " + days.ToString("000") + day_str + ", " +
                        (GameSettings.KERBIN_TIME ? hour.ToString() : hour.ToString("00")) + hour_str;

                
            }
            else
            {
                if (seconds < 10000.0 * KSPUtil.dateTimeFormatter.Year)
                {
                    double years = Math.Truncate(seconds / KSPUtil.dateTimeFormatter.Year);
                    double days = Math.Truncate(seconds % (years * KSPUtil.dateTimeFormatter.Year) /
                        KSPUtil.dateTimeFormatter.Day);
                    str += $"{years}{year_str}, {days:000}{day_str}";
                }
                else if (seconds < 100000000.0 * KSPUtil.dateTimeFormatter.Year)
                {
                    str += Math.Truncate(seconds / KSPUtil.dateTimeFormatter.Year) + year_str;
                }

                else
                    str += (seconds / KSPUtil.dateTimeFormatter.Year).ToString("0.000e00") + year_str;
            }

            return str;
        }

        /// <summary>
        /// convert value to Distance string with unit (lower unit is m — 99.9m 999m 9.99k 99.9k 999k )
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
        /// convert value to Distance string with unit (lower unit is km — 0.09k 0.99k 9.99k 99.9k 999k )
        /// </summary>
        public static string Distance_k(double value)
        {
            double v = Math.Abs(value);

            if (v < 1E4) return Truncate(value / 1E3, "F", 2) + k;

            return Distance_short(value);
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

            else if (v < 1E8)     // 1,000 m - 99,999,999 m
                str = Truncate(value, "N", 0) + " " + m;

            else if (v < 1E12)    // 100.0 Mm - 999,999.9 Mm
                str = Truncate(value / 1E6, "N", 1) + " " + Mm;

            else                  // 1,000.0 Gm - 999,999.9 Gm and beyond
                str = Truncate(value / 1E9, "N", 1) + " " + Gm;

            return str;
        }

        public static string Angle(double value, bool Integer = false, int totalWidth = 7)
        {
            if (Integer)
            {
                if (value > 359.5) value = 0.0;
                return String.Format("{0:F0}\u00B0 ", value).PadLeft(totalWidth, '\u2007');
            }
            else
            {
                if (value > 359.95) value = 0.0;
                return String.Format("{0:F1}\u00B0 ", value).PadLeft(totalWidth, '\u2007');
            }
        }
    }
}
