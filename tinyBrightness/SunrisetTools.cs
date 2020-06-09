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
            TimeSpan Time = TimeSpan.FromHours(tsunrise);
            return new TimeSpan(Time.Hours, Time.Minutes, 0);
        }

        public TimeSpan GetTodaySunset()
        {
            Sunriset.SunriseSunset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Lat, Lon, out _, out double tsunset);
            TimeSpan Time = TimeSpan.FromHours(tsunset);
            return new TimeSpan(Time.Hours, Time.Minutes, 0);
        }

        public TimeSpan GetTodayDawn()
        {
            Sunriset.AstronomicalTwilight(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Lat, Lon, out double tastrosunrise, out _);
            TimeSpan Time = TimeSpan.FromHours(tastrosunrise);
            return new TimeSpan(Time.Hours, Time.Minutes, 0);
        }

        public TimeSpan GetTodayDusk()
        {
            Sunriset.AstronomicalTwilight(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Lat, Lon, out _, out double tastrosunset);
            TimeSpan Time = TimeSpan.FromHours(tastrosunset);
            return new TimeSpan(Time.Hours, Time.Minutes, 0);
        }
    }
}
