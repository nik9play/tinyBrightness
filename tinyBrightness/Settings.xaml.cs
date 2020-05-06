using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using IniParser.Model;
using Microsoft.Win32;
using ModernWpf.Controls;

namespace tinyBrightness
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ObservableCollection<string> HotkeyPopupPositionList { get; set; } = new ObservableCollection<string>
        {
            "Top Left", "Top Right", "Bottom Left", "Bottom Right"
        };

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            ((TextBox)sender).Text = GetHotkey(key).ToString();
            Keyboard.ClearFocus();
        }

        private StringBuilder GetHotkey(Key key)
        {
            StringBuilder HotkeyText = new StringBuilder();
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                HotkeyText.Append("Ctrl+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                HotkeyText.Append("Shift+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                HotkeyText.Append("Alt+");
            }
            HotkeyText.Append(key.ToString());

            return HotkeyText;
        }

        private void HotkeysSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            IniData data = SettingsController.GetCurrentSettings();

            if (HotkeysSwitch.IsOn)
            {
                data["Hotkeys"]["HotkeysEnable"] = "1";
                ((MainWindow)Owner).SetHotkeysByStrings(BrightnessUpTextbox.Text, BrightnessDownTextbox.Text);
            }
            else
            {
                data["Hotkeys"]["HotkeysEnable"] = "0";
                ((MainWindow)Owner).RemoveAllHotkeys();
            }

            SettingsController.SaveSettings(data);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void AcrylicWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                                    .AddDays(version.Build).AddSeconds(version.Revision * 2);
            string displayableVersion = $"{version} ({buildDate})";
            Version_Text.Text = displayableVersion;

            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rk.GetValue("tinyBrightness", null) != null)
                RunSwitch.IsOn = true;

            IniData data = SettingsController.GetCurrentSettings();

            BrightnessUpTextbox.Text = data["Hotkeys"]["HotkeyUp"];
            BrightnessDownTextbox.Text = data["Hotkeys"]["HotkeyDown"];

            if (data["Hotkeys"]["HotkeysEnable"] == "1")
                HotkeysSwitch.IsOn = true;

            if (data["Misc"]["Blur"] == "1" && System.Environment.OSVersion.Version.Major == 10)
                BlurSwitch.IsOn = true;

            if (Environment.OSVersion.Version.Major != 10)
                BlurSwitch.IsEnabled = false;

            if (data["Misc"]["HotkeyPopupDisable"] != "1")
                HotkeyPopupSwitch.IsOn = true;

            string HotkeyPopupPosition = data["Misc"]["HotkeyPopupPosition"];
            switch (HotkeyPopupPosition)
            {
                case "Bottom Right":
                case "Bottom Left":
                case "Top Right":
                case "Top Left":
                    HotkeyPopupPositionCombobox.SelectedItem = HotkeyPopupPosition;
                    break;
                default:
                    HotkeyPopupPositionCombobox.SelectedItem = "Top Left";
                    break;
            }

            if (data["Updates"]["DisableCheckOnStartup"] != "1")
                UpdatesSwitch.IsOn = true;

            if (data["Updates"]["DisableCheckEveryDay"] != "1")
                EveryDayUpdatesSwitch.IsOn = true;

            LongitudeBox.NumberFormatter = LatitudeBox.NumberFormatter = new CustomNumberFormatter();

            LoadAutoBrightnessSettings();
        }

        private class CustomNumberFormatter : INumberBoxNumberFormatter
        {
            public string FormatDouble(double value)
            {
                return value.ToString("G");
            }
            public double? ParseDouble(string text)
            {
                if (double.TryParse(text, out double result))
                    return result;
                return null;
            }
        }

        private void BrightnessUpTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text == "")
                ((TextBox)sender).Text = "Ctrl+Shift+Add";
            else
            {
                IniData data = SettingsController.GetCurrentSettings();

                data["Hotkeys"]["HotkeyUp"] = ((TextBox)sender).Text;

                SettingsController.SaveSettings(data);

                if (HotkeysSwitch.IsOn)
                    ((MainWindow)Owner).SetHotkeysByStrings(BrightnessUpTextbox.Text, BrightnessDownTextbox.Text);
            }
        }

        private void BrightnessDownTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text == "")
                ((TextBox)sender).Text = "Ctrl+Shift+Subtract";
            else
            {
                IniData data = SettingsController.GetCurrentSettings();

                data["Hotkeys"]["HotkeyDown"] = ((TextBox)sender).Text;

                SettingsController.SaveSettings(data);

                if (HotkeysSwitch.IsOn)
                    ((MainWindow)Owner).SetHotkeysByStrings(BrightnessUpTextbox.Text, BrightnessDownTextbox.Text);
            }
        }

        private void RunSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (RunSwitch.IsOn)
                rk.SetValue("tinyBrightness", Application.ResourceAssembly.Location + " --silent");
            else
                rk.DeleteValue("tinyBrightness", false);
        }

        private void BlurSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            IniData data = SettingsController.GetCurrentSettings();

            if (BlurSwitch.IsOn)
                data["Misc"]["Blur"] = "1";
            else
                data["Misc"]["Blur"] = "0";

            SettingsController.SaveSettings(data);
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

        private void HotkeyPopupPositionCombobox_Selected(object sender, RoutedEventArgs e)
        {
            IniData data = SettingsController.GetCurrentSettings();

            data["Misc"]["HotkeyPopupPosition"] = HotkeyPopupPositionCombobox.SelectedItem.ToString();

            SettingsController.SaveSettings(data);
        }

        private void HotkeyPopupSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            IniData data = SettingsController.GetCurrentSettings();

            if (HotkeyPopupSwitch.IsOn)
                data["Misc"]["HotkeyPopupDisable"] = "0";
            else
                data["Misc"]["HotkeyPopupDisable"] = "1";

            SettingsController.SaveSettings(data);
        }

        private void CrashApp_Click(object sender, RoutedEventArgs e)
        {
            throw new Exception("Expected exception!");
        }

        #region AutoBrightness

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            Flyout f = FlyoutService.GetFlyout(LocationIPButton) as Flyout;
            if (f != null)
                f.Hide();

            Mouse.OverrideCursor = Cursors.Wait;
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", "request");

                client.DownloadStringCompleted += (senderW, eW) =>
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(eW.Result);

                    XmlNodeList LatValue = xmlDoc.GetElementsByTagName("lat");
                    XmlNodeList LongValue = xmlDoc.GetElementsByTagName("lon");

                    double.TryParse(LatValue[0].InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out double LatValueResult);
                    LatitudeBox.Value = LatValueResult;

                    double.TryParse(LongValue[0].InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out double LongValueResult);
                    LongitudeBox.Value = LongValueResult;

                    Mouse.OverrideCursor = null;
                };

                client.DownloadStringAsync(new Uri("http://ip-api.com/xml/"));
            }
        }

        private void LatitudeBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            IniData data = SettingsController.GetCurrentSettings();

            data["AutoBrightness"]["Lat"] = sender.Value.ToString(CultureInfo.InvariantCulture);

            SettingsController.SaveSettings(data);
        }

        private void LongitudeBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            IniData data = SettingsController.GetCurrentSettings();

            data["AutoBrightness"]["Lon"] = sender.Value.ToString(CultureInfo.InvariantCulture);

            SettingsController.SaveSettings(data);
        }

        private void SunriseSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            IniData data = SettingsController.GetCurrentSettings();

            data["AutoBrightness"]["SunriseBrightness"] = (((Slider)sender).Value / 100).ToString(CultureInfo.InvariantCulture);

            SettingsController.SaveSettings(data);
        }

        private void SunsetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            IniData data = SettingsController.GetCurrentSettings();

            data["AutoBrightness"]["SunsetBrightness"] = (((Slider)sender).Value / 100).ToString(CultureInfo.InvariantCulture);

            SettingsController.SaveSettings(data);
        }

        private void AutoBrightnessSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            IniData data = SettingsController.GetCurrentSettings();

            if (AutoBrightnessSwitch.IsOn)
            {
                data["AutoBrightness"]["Enabled"] = "1";
                ((MainWindow)Owner).CheckForSunriset.Start();
            }
            else
            {
                data["AutoBrightness"]["Enabled"] = "0";
                ((MainWindow)Owner).CheckForSunriset.Stop();
            }

            SettingsController.SaveSettings(data);
        }

        private void LoadAutoBrightnessSettings()
        {
            IniData data = SettingsController.GetCurrentSettings();

            if (data["AutoBrightness"]["Enabled"] == "1")
                AutoBrightnessSwitch.IsOn = true;


            if (double.TryParse(data["AutoBrightness"]["Lat"], NumberStyles.Any, CultureInfo.InvariantCulture, out double LatitudeBoxValue))
                LatitudeBox.Value = LatitudeBoxValue;
            else
                LatitudeBox.Value = 0;

            if (double.TryParse(data["AutoBrightness"]["Lon"], NumberStyles.Any, CultureInfo.InvariantCulture, out double LongitudeBoxValue))
                LongitudeBox.Value = LongitudeBoxValue;
            else
                LongitudeBox.Value = 0;

            if (double.TryParse(data["AutoBrightness"]["SunriseBrightness"], NumberStyles.Any, CultureInfo.InvariantCulture, out double SunriseBrightnessValue))
                SunriseSlider.Value = SunriseBrightnessValue * 100;
            else
                SunriseSlider.Value = 90;

            if (double.TryParse(data["AutoBrightness"]["SunsetBrightness"], NumberStyles.Any, CultureInfo.InvariantCulture, out double SunsetBrightnessValue))
                SunsetSlider.Value = SunsetBrightnessValue * 100;
            else
                SunsetSlider.Value = 10;
        }

        #endregion
    }
}
