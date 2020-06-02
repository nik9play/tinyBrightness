using IniParser.Model;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace tinyBrightness.SettingsPages
{
    /// <summary>
    /// Логика взаимодействия для General.xaml
    /// </summary>
    public partial class General : Page
    {
        public General()
        {
            InitializeComponent();
        }

        private Window Owner;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Owner = Window.GetWindow(this).Owner;

            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rk.GetValue("tinyBrightness", null) != null)
                RunSwitch.IsOn = true;

            IniData data = SettingsController.GetCurrentSettings();

            if (data["Updates"]["DisableCheckOnStartup"] != "1")
                UpdatesSwitch.IsOn = true;

            if (data["Updates"]["DisableCheckEveryDay"] != "1")
                EveryDayUpdatesSwitch.IsOn = true;
        }

        private void RunSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (RunSwitch.IsOn)
                rk.SetValue("tinyBrightness", Application.ResourceAssembly.Location + " --silent");
            else
                rk.DeleteValue("tinyBrightness", false);
        }

        private void UpdatesSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            IniData data = SettingsController.GetCurrentSettings();

            if (UpdatesSwitch.IsOn)
                data["Updates"]["DisableCheckOnStartup"] = "0";
            else
                data["Updates"]["DisableCheckOnStartup"] = "1";

            SettingsController.SaveSettings(data);
        }

        private void EveryDayUpdatesSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            IniData data = SettingsController.GetCurrentSettings();

            if (EveryDayUpdatesSwitch.IsOn)
            {
                data["Updates"]["DisableCheckEveryDay"] = "0";
                if (!((MainWindow)Owner).UpdateCheckTimer.IsEnabled)
                    ((MainWindow)Owner).UpdateCheckTimer.Start();
            }
            else
            {
                data["Updates"]["DisableCheckEveryDay"] = "1";
                if (((MainWindow)Owner).UpdateCheckTimer.IsEnabled)
                    ((MainWindow)Owner).UpdateCheckTimer.Stop();
            }

            SettingsController.SaveSettings(data);
        }
    }
}
