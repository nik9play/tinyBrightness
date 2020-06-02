using IniParser.Model;
using ModernWpf.Controls;
using System;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace tinyBrightness.SettingsPages
{
    /// <summary>
    /// Логика взаимодействия для AutoBrightness.xaml
    /// </summary>
    public partial class AutoBrightness : System.Windows.Controls.Page
    {
        public AutoBrightness()
        {
            InitializeComponent();
        }

        private Window Owner;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Owner = Window.GetWindow(this).Owner;

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
                    try
                    {
                        xmlDoc.LoadXml(eW.Result);
                        XmlNodeList LatValue = xmlDoc.GetElementsByTagName("lat");
                        XmlNodeList LongValue = xmlDoc.GetElementsByTagName("lon");

                        double.TryParse(LatValue[0].InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out double LatValueResult);
                        LatitudeBox.Value = LatValueResult;

                        double.TryParse(LongValue[0].InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out double LongValueResult);
                        LongitudeBox.Value = LongValueResult;
                    }
                    catch { MessageBox.Show("Error while getting location.", "tinyBrightness", MessageBoxButton.OK, MessageBoxImage.Error); }
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

        private void AstroSunriseSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            IniData data = SettingsController.GetCurrentSettings();
            data["AutoBrightness"]["AstroSunriseBrightness"] = (((Slider)sender).Value / 100).ToString(CultureInfo.InvariantCulture);
            SettingsController.SaveSettings(data);
        }

        private void AstroSunsetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            IniData data = SettingsController.GetCurrentSettings();
            data["AutoBrightness"]["AstroSunsetBrightness"] = (((Slider)sender).Value / 100).ToString(CultureInfo.InvariantCulture);
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
                SunsetSlider.Value = 30;

            if (double.TryParse(data["AutoBrightness"]["AstroSunriseBrightness"], NumberStyles.Any, CultureInfo.InvariantCulture, out double AstroSunriseBrightnessValue))
                AstroSunriseSlider.Value = AstroSunriseBrightnessValue * 100;
            else
                AstroSunriseSlider.Value = 20;

            if (double.TryParse(data["AutoBrightness"]["AstroSunsetBrightness"], NumberStyles.Any, CultureInfo.InvariantCulture, out double AstroSunsetBrightnessValue))
                AstroSunsetSlider.Value = AstroSunsetBrightnessValue * 100;
            else
                AstroSunsetSlider.Value = 10;
        }
    }
}
