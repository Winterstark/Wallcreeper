using System;
using System.Linq;
using System.Text;

namespace Wallcreeper
{
    class Moon
    {
        const double RAD = Math.PI / 180.0;
        const double SMALL_FLOAT = 1.0 / 1000000000000;


        static void JulianToDate(out DateTime now, double jd)
        {
            long jdi, b;
            long c, d, e, g, g1;

            jd += 0.5;
            jdi = (long)jd;

            if (jdi > 2299160)
            {
                long a = (long)((jdi - 1867216.25) / 36524.25);
                b = jdi + 1 + a - a / 4;
            }
            else b = jdi;

            c = b + 1524;
            d = (long)((c - 122.1) / 365.25);
            e = (long)(365.25 * d);
            g = (long)((c - e) / 30.6001);
            g1 = (long)(30.6001 * g);

            int nowYear, nowMonth, nowDay, nowHour;

            nowDay = (int)(c - e - g1);
            nowHour = (int)((jd - jdi) * 24.0);
            if (g <= 13) nowMonth = (int)(g - 1);
            else nowMonth = (int)(g - 13);
            if (nowMonth > 2) nowYear = (int)(d - 4716);
            else nowYear = (int)(d - 4715);

            now = new DateTime(nowYear, nowMonth, nowDay, nowHour, 0, 0);
        }

        static double Julian(int year, int month, double day)
        {
            /*
              Returns the number of julian days for the specified day.
              */

            int a, b = 0, c, e;
            if (month < 3)
            {
                year--;
                month += 12;
            }
            if (year > 1582 || (year == 1582 && month > 10) ||
            (year == 1582 && month == 10 && day > 15))
            {
                a = year / 100;
                b = 2 - a + a / 4;
            }
            c = (int)(365.25 * year);
            e = (int)(30.6001 * (month + 1));
            return b + c + e + day + 1720994.5;
        }

        static double sun_position(double j)
        {
            double n, x, e, l, dl, v;
            //double m2;
            int i;

            n = 360 / 365.2422 * j;
            i = (int)(n / 360);
            n = n - i * 360.0;
            x = n - 3.762863;
            if (x < 0) x += 360;
            x *= RAD;
            e = x;
            do
            {
                dl = e - .016718 * Math.Sin(e) - x;
                e = e - dl / (1 - .016718 * Math.Cos(e));
            } while (Math.Abs(dl) >= SMALL_FLOAT);
            v = 360 / Math.PI * Math.Atan(1.01686011182 * Math.Tan(e / 2));
            l = v + 282.596403;
            i = (int)(l / 360);
            l = l - i * 360.0;
            return l;
        }

        static double moon_position(double j, double ls)
        {

            double ms, l, mm, n, ev, sms, ae, ec;
            //double z, x, lm, bm;
            //double d;
            //double ds, aS, dm;
            int i;

            /* ls = sun_position(j) */
            ms = 0.985647332099 * j - 3.762863;
            if (ms < 0) ms += 360.0;
            l = 13.176396 * j + 64.975464;
            i = (int)(l / 360);
            l = l - i * 360.0;
            if (l < 0) l += 360.0;
            mm = l - 0.1114041 * j - 349.383063;
            i = (int)(mm / 360);
            mm -= i * 360.0;
            n = 151.950429 - 0.0529539 * j;
            i = (int)(n / 360);
            n -= i * 360.0;
            ev = 1.2739 * Math.Sin((2 * (l - ls) - mm) * RAD);
            sms = Math.Sin(ms * RAD);
            ae = 0.1858 * sms;
            mm += ev - ae - 0.37 * sms;
            ec = 6.2886 * Math.Sin(mm * RAD);
            l += ev + ec - ae + 0.214 * Math.Sin(2 * mm * RAD);
            l = 0.6583 * Math.Sin(2 * (l - ls) * RAD) + l;
            return l;
        }

        static double moon_phase(int year, int month, int day, double hour, out int ip)
        {
            /*
              Calculates more accurately than Moon_phase , the phase of the moon at
              the given epoch.
              returns the moon phase as a real number (0-1)
              */

            double j = Julian(year, month, (double)day + hour / 24.0) - 2444238.5;
            double ls = sun_position(j);
            double lm = moon_position(j, ls);

            double t = lm - ls;
            if (t < 0) t += 360;
            ip = (int)((t + 22.5) / 45) & 0x7;
            return (1.0 - Math.Cos((lm - ls) * RAD)) / 2;
        }

        static void nextDay(ref int y, ref int m, ref int d, double dd)
        {
            DateTime tp;
            double jd = Julian(y, m, (double)d);

            jd += dd;
            JulianToDate(out tp, jd);

            y = tp.Year;
            m = tp.Month;
            d = tp.Day;
        }

        static DateTime calcNext(int y, int m, int d)
        {
            int m0;
            int h;
            //int i;
            double step = 1;
            int begun = 0;

            double pmax = 0;
            double pmin = 1;
            int ymax = 0, mmax = 0, dmax = 0, hmax = 0;
            int ymin, mmin, dmin, hmin;
            double plast = 0;

            //MessageBox.Show("tabulation of the phase of the moon for one month\n\n");

            //MessageBox.Show("year: ");// fflush(stdout);
            //scanf("%d", &y);
            //y = 2012;

            //MessageBox.Show("month: ");// fflush(stdout);
            //scanf("%d", &m);    
            //m = 5;

            //d = 1;
            m0 = m;

            //MessageBox.Show("\nDate       Time   Phase Segment\n");
            for (; ; )
            {
                double p;
                int ip;

                for (h = 0; h < 24; h += (int)step)
                {

                    p = moon_phase(y, m, d, h, out ip);

                    if (begun != 0)
                    {
                        if (p > plast && p > pmax)
                        {
                            pmax = p;
                            ymax = y;
                            mmax = m;
                            dmax = d;
                            hmax = h;
                        }
                        else if (pmax != 0)
                        {
                            //MessageBox.Show("%04d/%02d/%02d %02d:00          (fullest)\n,      ymax, mmax, dmax, hmax");
                            //MessageBox.Show("Full Moon :: " + ymax.ToString() + " " + mmax.ToString() + " " + dmax.ToString() + " " + hmax.ToString() + " ");
                            return new DateTime(ymax, mmax, dmax, hmax, 0, 0);
                            //pmax = 0;
                        }

                        if (p < plast && p < pmin)
                        {
                            pmin = p;
                            ymin = y;
                            mmin = m;
                            dmin = d;
                            hmin = h;
                        }
                        else if (pmin < 1)
                        {
                            //MessageBox.Show("%04d/%02d/%02d %02d:00          (newest)\n    ymin, mmin, dmin, hmin");
                            pmin = 1.0;
                        }

                        if (h == 16)
                        {
                            //MessageBox.Show("%04d/%02d/%02d %02d:00 %5.1f%%   (%d)\n,     y, m, d, h, Math.Floor(p*1000+0.5)/10, ip");
                        }
                    }
                    else begun = 1;

                    plast = p;

                }
                nextDay(ref y, ref m, ref d, 1.0);
                if (m != m0) break;
            }

            return new DateTime();
        }

        public static DateTime NextFullMoon(DateTime fromDate)
        {
            if (calcNext(fromDate.Year, fromDate.Month, fromDate.Day) == new DateTime())
                return calcNext(fromDate.AddMonths(1).Year, fromDate.AddMonths(1).Month, 1);
            else
                return calcNext(fromDate.Year, fromDate.Month, fromDate.Day);
        }

        public static DateTime PrevFullMoon(DateTime fromDate)
        {
            DateTime nextFM = NextFullMoon(fromDate), prevFM, iDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);

            for (; ; iDate = iDate.AddDays(-1))
            {
                prevFM = NextFullMoon(iDate);

                if (prevFM.Year != nextFM.Year || prevFM.Month != nextFM.Month || prevFM.Day != nextFM.Day)
                    return prevFM;
            }

            //return naturalDate(prevFM);
        }

        static DateTime nightOfTheFullMoon(DateTime date)
        {
            if (date.Hour < 12)
                return date.AddDays(-1);
            else
                return date;
        }
    }
}
