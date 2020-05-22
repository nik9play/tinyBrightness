using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tinyBrightness
{
    class AutoBrightnessSettings
    {
        public static double GetLat()
        {
            IniData data = SettingsController.GetCurrentSettings();
            if (double.TryParse(data["AutoBrightness"]["Lat"], NumberStyles.Any, CultureInfo.InvariantCulture, out double Lat))
                return Lat; 
            else 
                return 0;
        }

        public static double GetLon()
        {
            IniData data = SettingsController.GetCurrentSettings();
            if (double.TryParse(data["AutoBrightness"]["Lon"], NumberStyles.Any, CultureInfo.InvariantCulture, out double Lon)) 
                return Lon;
            else 
                return 0;
        }

        public static double GetSunriseBrightness()
        {
            IniData data = SettingsController.GetCurrentSettings();
            if (double.TryParse(data["AutoBrightness"]["SunriseBrightness"], NumberStyles.Any, CultureInfo.InvariantCulture, out double SunriseBrightness)) 
                return SunriseBrightness;
            else 
                return 0.9;
        }

        public static double GetSunsetBrightness()
        {
            IniData data = SettingsController.GetCurrentSettings();
            if (double.TryParse(data["AutoBrightness"]["SunsetBrightness"], NumberStyles.Any, CultureInfo.InvariantCulture, out double SunsetBrightness))
                return SunsetBrightness;
            else
                return 0.3;
        }

        public static double GetAstroSunriseBrightness()
        {
            IniData data = SettingsController.GetCurrentSettings();
            if (double.TryParse(data["AutoBrightness"]["AstroSunriseBrightness"], NumberStyles.Any, CultureInfo.InvariantCulture, out double AstroSunriseBrightness))
                return AstroSunriseBrightness;
            else
                return 0.2;
        }

        public static double GetAstroSunsetBrightness()
        {
            IniData data = SettingsController.GetCurrentSettings();
            if (double.TryParse(data["AutoBrightness"]["AstroSunsetBrightness"], NumberStyles.Any, CultureInfo.InvariantCulture, out double AstroSunsetBrightness))
                return AstroSunsetBrightness;
            else
                return 0.1;
        }
    }
}
