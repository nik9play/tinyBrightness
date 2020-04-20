using IniParser;
using IniParser.Model;
using System.IO;
using System.Windows;

namespace tinyBrightness
{
    class SettingsController
    {
        public static IniData GetDefaultSettings()
        {
            IniData data = new IniData();

            data["Hotkeys"]["HotkeysEnable"] = "1";
            data["Hotkeys"]["HotkeyUp"] = "Ctrl+Shift+Add";
            data["Hotkeys"]["HotkeyDown"] = "Ctrl+Shift+Subtract";
            data["Misc"]["Blur"] = "1";

            return data;
        }

        private static FileIniDataParser parser = new FileIniDataParser();

        public static void LoadSettings()
        {
            if (!File.Exists("tinyBrightness.ini"))
                try
                {
                    parser.WriteFile("tinyBrightness.ini", GetDefaultSettings());
                }
                catch { }

            IniData data;

            try
            {
                data = parser.ReadFile("tinyBrightness.ini");
            }
            catch
            {
                data = GetDefaultSettings();
            }

            Application.Current.Properties["IniSettings"] = data;
        }

        public static IniData GetCurrentSettings()
        {
            return (IniData)Application.Current.Properties["IniSettings"];
        }

        public static void SaveSettings(IniData data)
        {
            try
            {
                parser.WriteFile("tinyBrightness.ini", data);
            }
            catch
            {
                /*throw new System.Exception("Settings file is not accessible");*/
            }
            finally
            {
                Application.Current.Properties["IniSettings"] = data;
            }
        }
    }
}
