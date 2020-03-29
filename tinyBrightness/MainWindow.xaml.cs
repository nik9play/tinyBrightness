using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ScreenTools;
using IniParser;
using IniParser.Model;
using System.IO;
using NHotkey.Wpf;
using System.Windows.Input;
using NHotkey;

namespace tinyBrightness
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        class MONITOR
        {
            public string name { get; set; }
            public DisplayConfiguration.PHYSICAL_MONITOR Handle { get; set; }

            public MONITOR(string name, DisplayConfiguration.PHYSICAL_MONITOR Handle)
            {
                this.name = name;
                this.Handle = Handle;
            }
        }

        List<MONITOR> MonitorList { get; set; } = new List<MONITOR>();

        DisplayConfiguration.PHYSICAL_MONITOR CurrentMonitor;

        private void Set_Initial_Brightness()
        {
            Double Brightness = 0;

            try
            {
                Brightness = DisplayConfiguration.GetMonitorBrightness(CurrentMonitor) * 100;
            }
            catch
            {
                Slider_Brightness.IsEnabled = false;
                Main_Grid.ToolTip = "This monitor is not supported. You may need to enable «DDC/CI» option in your monitor settings.";
            }

            Slider_Brightness.Value = Brightness;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MonitorList.Clear();

            var desktopWorkingArea = Screen.GetWorkingArea(System.Windows.Forms.Control.MousePosition);
            Left = desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;

            foreach (Screen screen in Screen.AllScreens)
            {
                DisplayConfiguration.PHYSICAL_MONITOR mon = DisplayConfiguration.GetPhysicalMonitors(DisplayConfiguration.GetMonitorByBounds(screen.Bounds))[0];

                string Name = screen.DeviceFriendlyName();

                if (String.IsNullOrEmpty(Name))
                {
                    Name = "Generic Monitor";
                }

                MonitorList.Add(new MONITOR(Name, mon));
            }

            Monitor_List_Combobox.ItemsSource = MonitorList;
            Monitor_List_Combobox.SelectedItem = MonitorList[0];
            CurrentMonitor = MonitorList[0].Handle;
            Window_Start();
            LoadSettings();
        }

        private void Window_Start()
        {
            Show();
            Activate();
            Set_Initial_Brightness();
        }


        private void Slider_Brightness_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            DisplayConfiguration.SetMonitorBrightness(CurrentMonitor, ((Slider)sender).Value / 100);
        }

        private void Slider_Brightness_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DisplayConfiguration.SetMonitorBrightness(CurrentMonitor, ((Slider)sender).Value / 100);
        }


        private void Window_Deactivated(object sender, EventArgs e)
        {
            Hide();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Monitor_List_Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentMonitor = MonitorList[Monitor_List_Combobox.SelectedIndex].Handle;
            Set_Initial_Brightness();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            new About().Show();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            new Settings().Show();
        }

        private FileIniDataParser parser = new FileIniDataParser();

        private void CreateSettingsFile()
        {
            IniData data = new IniData();

            data["Hotkeys"]["HotkeysEnable"] = "1";
            data["Hotkeys"]["HotkeyUp"] = "Ctrl+Shift+Add";
            data["Hotkeys"]["HotkeyDown"] = "Ctrl+Shift+Subtract";

            parser.WriteFile("tinyBrightness.ini", data);
        }

        class Keys
        {
            public ModifierKeys Modifiers;
            public Key MainKey;
        }

        private Keys GetKeys(string HotkeyString)
        {
            string[] HotkeyArray = HotkeyString.Split('+');

            Keys keys = new Keys();

            foreach(string Element in HotkeyArray)
            {
                switch(Element)
                {
                    case "Ctrl":
                        keys.Modifiers |= ModifierKeys.Control;
                        break;
                    case "Alt":
                        keys.Modifiers |= ModifierKeys.Alt;
                        break;
                    case "Shift":
                        keys.Modifiers |= ModifierKeys.Shift;
                        break;
                    default:
                        Enum.TryParse(Element, out keys.MainKey);
                        break;
                }
            }

            return keys;
        }

        public void LoadSettings()
        {
            if (!File.Exists("tinyBrightness.ini"))
            {
                CreateSettingsFile();
            }
            else
            {
                try
                {
                    IniData data = parser.ReadFile("tinyBrightness.ini");
                    if (data["Hotkeys"]["HotkeysEnable"] == "1")
                    {
                        //brightness up
                        string BrightnessUpString = data["Hotkeys"]["HotkeyUp"];
                        Keys BrightnessUpKeys = GetKeys(BrightnessUpString);
                        HotkeyManager.Current.AddOrReplace("BrightnessUp", BrightnessUpKeys.MainKey, BrightnessUpKeys.Modifiers, OnBrightnessUp);

                        //brightness down
                        string BrightnessDownString = data["Hotkeys"]["HotkeyDown"];
                        Keys BrightnessDownKeys = GetKeys(BrightnessDownString);
                        HotkeyManager.Current.AddOrReplace("BrightnessDown", BrightnessDownKeys.MainKey, BrightnessDownKeys.Modifiers, OnBrightnessDown);
                    }
                }
                catch
                {
                    CreateSettingsFile();
                }
            }
        }

        private void OnBrightnessUp(object sender, HotkeyEventArgs e)
        {
            DisplayConfiguration.PHYSICAL_MONITOR CurrentMonitor = DisplayConfiguration.GetPhysicalMonitors(DisplayConfiguration.GetCurrentMonitor())[0];

            double CurrentBrightness = DisplayConfiguration.GetMonitorBrightness(CurrentMonitor);

            if (CurrentBrightness <= 0.9)
            {
                DisplayConfiguration.SetMonitorBrightness(CurrentMonitor, CurrentBrightness + 0.1);
            }
            else if (CurrentBrightness < 1)
            {
                DisplayConfiguration.SetMonitorBrightness(CurrentMonitor, 1);
            }
        }

        private void OnBrightnessDown(object sender, HotkeyEventArgs e)
        {
            DisplayConfiguration.PHYSICAL_MONITOR CurrentMonitor = DisplayConfiguration.GetPhysicalMonitors(DisplayConfiguration.GetCurrentMonitor())[0];

            double CurrentBrightness = DisplayConfiguration.GetMonitorBrightness(CurrentMonitor);

            if (CurrentBrightness >= 0.1)
            {
                DisplayConfiguration.SetMonitorBrightness(CurrentMonitor, CurrentBrightness - 0.1);
            }
            else if (CurrentBrightness > 0)
            {
                DisplayConfiguration.SetMonitorBrightness(CurrentMonitor, 0);
            }
        }
    }
}
