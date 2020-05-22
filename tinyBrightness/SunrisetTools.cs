using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tinyBrightness
{
    class SunrisetTools
    {
        public double Lat = 0;
        public double Lon = 0;

        public SunrisetTools(double LatValue, double LonValue)
        {
            Lat = LatValue;
            Lon = LonValue;
        }

        public TimeSpan GetTodaySunrise()
        {
            Sunriset.SunriseSunset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Lat, Lon, out double tsunrise, out _);
            return TimeSpan.FromHours(tsunrise);
        }

        public TimeSpan GetTodaySunset()
        {
            Sunriset.SunriseSunset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Lat, Lon, out _, out double tsunset);
            return TimeSpan.FromHours(tsunset);
        }

        public TimeSpan GetTodayDawn()
        {
            Sunriset.AstronomicalTwilight(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Lat, Lon, out double tastrosunrise, out _);
            return TimeSpan.FromHours(tastrosunrise);
        }

        public TimeSpan GetTodayDusk()
        {
            Sunriset.AstronomicalTwilight(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Lat, Lon, out _, out double tastrosunset);
            return TimeSpan.FromHours(tastrosunset);
        }
    }
}
