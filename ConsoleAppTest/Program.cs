using System;
using System.Diagnostics;

namespace ConsoleAppTest
{
    class Program
    {
        private static string m = "m";
        private static string k = "k";
        private static string M = "M";
        private static string G = "G";
        private static string T = "T";

        private static string[] SI = { m, k, M, G, T };

        static string TruncateF1(double value)
        {
            double multiplier = Math.Pow(10, 1);
            return (Math.Truncate(multiplier * value) / multiplier).ToString("F1");
        }

        static string Truncate(double value, string mode = "F", int digits = 1)
        {
            double multiplier = Math.Pow(10, digits);
            return (Math.Truncate(multiplier * value) / multiplier).ToString(mode + digits);
        }

        public static string Distance_short(double value, int significant_digits = 3)
        {
            string str;
            double v = Math.Abs(value);
            int i;
            for (i = 0; v >= 1000 && i < SI.Length - 1; i++)
                v /= 1000;

            if (v < 10) str = Truncate(Math.Sign(value) * v, "F", significant_digits - 1) + SI[i];
            else if (v < 100) str = Truncate(Math.Sign(value) * v, "F", significant_digits - 2) + SI[i];
            else if (v < 1000) str = Truncate(Math.Sign(value) * v, "F", significant_digits - 3) + SI[i];
            else str = value.ToString("0e0") + m;

            return str;
        }


        public static string Distance_short_k(double value)
        {
            double v = Math.Abs(value);

            int i;
            for (i = 0; v >= 1000 && i < SI.Length - 1; i++)
                v /= 1000;

            if (v < 100) return Truncate(Math.Sign(value) * v, "F", 1) + SI[i];
            else if (v < 1000) return Truncate(Math.Sign(value) * v, "F", 0) + SI[i];
            else return value.ToString("0e0") + m;

        }


        public static string Distance_k(double value)
        {
            string str;
            double v = Math.Abs(value);

            if (v < 1000E3)
            {
                if (v < 100E3) str = Truncate(value / 1E3, "F", 2) + k;
                else str = Truncate(value / 1E3, "F", 1) + k;
            }
            else if (v < 1000E6)
            {
                if (v < 100E6) str = Truncate(value / 1E6, "F", 2) + M;
                else str = Truncate(value / 1E6, "F", 1) + M;
            }
            else if (v < 1000E9)
            {
                if (v < 100E9) str = Truncate(value / 1E9, "F", 2) + G;
                else str = Truncate(value / 1E9, "F", 1) + G;
            }
            else if (v < 1000E12)
            {
                if (v < 100E12) str = Truncate(value / 1E12, "F", 2) + T;
                else str = Truncate(value / 1E12, "F", 1) + T;
            }
            else
            {
                str = value.ToString("0e0") + m;
            }
            return str;
        }


        public static string Distance_k_new(double value)
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



        static void Main(string[] args)
        {
            for (double j = 100; j <= 1E11; j *= 1000)
            {
                double max = 1E6 + j;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                double i;
                for (i = j; i < max; i += 1)
                    Distance_short_k(i / 1.005);

                sw.Stop();
                Console.WriteLine("Distance_short_k={0}, {1}-{2}", sw.Elapsed, (j / 1.005).ToString("F1"), (i / 1.005).ToString("F1"));

                sw.Reset();
                sw.Start();

                for (i = j; i < max; i += 1)
                    Distance_k(i / 1.006);

                sw.Stop();
                Console.WriteLine("Distance_k=      {0}, {1}-{2}", sw.Elapsed, (j / 1.006).ToString("F1"), (i / 1.006).ToString("F1"));

                sw.Reset();
                sw.Start();

                for (i = j; i < max; i += 1)
                    Distance_k_new(i / 1.007);

                sw.Stop();
                Console.WriteLine("Distance_k_new=  {0}, {1}-{2}\n", sw.Elapsed, (j / 1.007).ToString("F1"), (i / 1.007).ToString("F1"));

            }
        }
    }
}
