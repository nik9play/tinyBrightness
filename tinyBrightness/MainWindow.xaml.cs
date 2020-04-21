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
using SourceChord.FluentWPF;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Media.Animation;

namespace tinyBrightness
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            AdaptIconToTheme();
            Main_Grid.PreviewMouseWheel += (sender, e)
                                        => Slider_Brightness.Value += Slider_Brightness.SmallChange * e.Delta / 120;
        }

        public void AdaptIconToTheme()
        {
            if (Environment.OSVersion.Version.Major == 10)
            {
                int releaseId = int.Parse(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "").ToString());

                if (releaseId >= 1903)
                {
                    string CurrentTheme = SystemTheme.WindowsTheme.ToString();

                    if (CurrentTheme == "Dark")
                        TrayIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/Icons/lightIcon.ico"));
                    else if (CurrentTheme == "Light")
                        TrayIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/Icons/darkIcon.ico"));
                }
                else
                {
                    TrayIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/Icons/lightIcon.ico"));
                }
            }
            else
            {
                TrayIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/Icons/icon.ico"));
            }
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

                Slider_Brightness.IsEnabled = true;
                Main_Grid.ToolTip = null;
            }
            catch
            {
                Slider_Brightness.IsEnabled = false;
                Main_Grid.ToolTip = "This monitor is not supported. You may need to enable «DDC/CI» option in your monitor settings.";
            }

            Slider_Brightness.Value = Brightness;
            PercentText.Text = Brightness.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMonitorList();
            SetWindowPosition();
            Show();
            Activate();
            Set_Initial_Brightness();
        }

        public double TopAnim { get; set; } = 0;
        public double TopAnimMargin { get; set; } = 0;

        private void SetWindowPosition()
        {
            double factor = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

            var desktopWorkingArea = Screen.GetWorkingArea(System.Windows.Forms.Control.MousePosition);
            Left = desktopWorkingArea.Right / factor - Width;
            /*Top = desktopWorkingArea.Bottom / factor - Height;*/

            int AdditionalPixel = 0;
            if (factor > 1)
                AdditionalPixel = 1;

            TopAnim = desktopWorkingArea.Bottom / factor - Height + AdditionalPixel;
            TopAnimMargin = desktopWorkingArea.Bottom / factor - Height + 30 + AdditionalPixel;

            (FindResource("showMe") as Storyboard).Begin(this);
        }

        private void UpdateMonitorList()
        {
            MonitorList.Clear();

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
            /*Hide();*/
            (FindResource("hideMe") as Storyboard).Begin(this);
        }

        private void Monitor_List_Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentMonitor = MonitorList[Monitor_List_Combobox.SelectedIndex].Handle;
            Set_Initial_Brightness();
        }

        #region Hotkeys

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

        private void SetHotkeysByStrings(string UpString, string DownString)
        {
            //brightness up
            string BrightnessUpString = UpString;
            Keys BrightnessUpKeys = GetKeys(BrightnessUpString);
            HotkeyManager.Current.AddOrReplace("BrightnessUp", BrightnessUpKeys.MainKey, BrightnessUpKeys.Modifiers, OnBrightnessUp);

            //brightness down
            string BrightnessDownString = DownString;
            Keys BrightnessDownKeys = GetKeys(BrightnessDownString);
            HotkeyManager.Current.AddOrReplace("BrightnessDown", BrightnessDownKeys.MainKey, BrightnessDownKeys.Modifiers, OnBrightnessDown);
        }

        private void OnBrightnessUp(object sender, HotkeyEventArgs e)
        {
            DisplayConfiguration.PHYSICAL_MONITOR CurrentMonitor = DisplayConfiguration.GetPhysicalMonitors(DisplayConfiguration.GetCurrentMonitor())[0];

            try
            {
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
            catch
            {

            }
        }

        private void OnBrightnessDown(object sender, HotkeyEventArgs e)
        {
            DisplayConfiguration.PHYSICAL_MONITOR CurrentMonitor = DisplayConfiguration.GetPhysicalMonitors(DisplayConfiguration.GetCurrentMonitor())[0];

            try
            {
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
            catch
            {

            }
        }

        #endregion

        #region Settings
        public void LoadSettings()
        {
            SettingsController.LoadSettings();
            IniData data = SettingsController.GetCurrentSettings();

            if (data["Hotkeys"]["HotkeysEnable"] == "1")
                SetHotkeysByStrings(data["Hotkeys"]["HotkeyUp"], data["Hotkeys"]["HotkeyDown"]);

            if (data["Misc"]["Blur"] == "1" && Environment.OSVersion.Version.Major == 10)
            {
                Background = null;
                AcrylicWindow.SetEnabled(this, true);
            }
        }
        #endregion

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        private void Slider_Brightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PercentText.Text = Convert.ToInt32(((Slider)sender).Value).ToString();

            debounceTimer.Throttle(100, (p) =>
            {
                try
                {
                    DisplayConfiguration.SetMonitorBrightness(CurrentMonitor, ((Slider)sender).Value / 100);
                }
                catch
                {

                }
            });
        }

        #region Tray

        private void TaskbarIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            SetWindowPosition();
            Set_Initial_Brightness();
            Show();
            Activate();
        }

        private void UpdateMonitors_Click(object sender, RoutedEventArgs e)
        {
            UpdateMonitorList();
            Set_Initial_Brightness();
            SetWindowPosition();
            Show();
            Activate();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var Win = new About();
            Win.Owner = this;
            Win.Show();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var Win = new Settings();
            Win.Owner = this;
            Win.Show();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            new Update().Window_Loaded(true);
        }

        #endregion
    }
}
