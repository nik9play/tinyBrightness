using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace tinyBrightness.SettingsPages
{
    /// <summary>
    /// Логика взаимодействия для Appearance.xaml
    /// </summary>
    public partial class Appearance : Page
    {
        public Appearance()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            IniData data = SettingsController.GetCurrentSettings();

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
        }

        public ObservableCollection<string> HotkeyPopupPositionList { get; set; } = new ObservableCollection<string>
        {
            "Top Left", "Top Right", "Bottom Left", "Bottom Right"
        };

        private void BlurSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            IniData data = SettingsController.GetCurrentSettings();

            if (BlurSwitch.IsOn)
                data["Misc"]["Blur"] = "1";
            else
                data["Misc"]["Blur"] = "0";

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Assembly.GetEntryAssembly().Location);
            Application.Current.Shutdown();
        }
    }
}
