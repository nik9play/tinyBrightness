using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ScreenTools;
using IniParser.Model;
using NHotkey.Wpf;
using System.Windows.Input;
using NHotkey;
using SourceChord.FluentWPF;
using Microsoft.Win32;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Media;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

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
                        TrayIcon.Icon = new Icon(Properties.Resources.lightIcon, SystemInformation.SmallIconSize);
                    else if (CurrentTheme == "Light")
                        TrayIcon.Icon = new Icon(Properties.Resources.darkIcon, SystemInformation.SmallIconSize);
                }
                else
                {
                    TrayIcon.Icon = new Icon(Properties.Resources.lightIcon, SystemInformation.SmallIconSize);
                }
            }
            else
            {
                TrayIcon.Icon = new Icon(Properties.Resources.icon, SystemInformation.SmallIconSize);
            }
        }

        class MONITOR
        {
            public string name { get; set; }
            public DisplayConfiguration.PHYSICAL_MONITOR Handle { get; set; }
            public uint Min { get; set; }
            public uint Max { get; set; }

            public MONITOR(string name, DisplayConfiguration.PHYSICAL_MONITOR Handle, uint Min, uint Max)
            {
                this.name = name;
                this.Handle = Handle;
                this.Min = Min;
                this.Max = Max;
            }
        }

        List<MONITOR> MonitorList { get; set; } = new List<MONITOR>();

        MONITOR CurrentMonitor;

        private void Set_Initial_Brightness()
        {
            double Brightness = 0;

            try
            {
                Brightness = DisplayConfiguration.GetMonitorBrightness(CurrentMonitor.Handle) * 100;

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
            AfterUpdateCheck();

            IniData data = SettingsController.GetCurrentSettings();
            DisplayConfiguration.PHYSICAL_MONITOR CurrentMonitor = DisplayConfiguration.GetPhysicalMonitors(DisplayConfiguration.GetCurrentMonitor())[0];

            double CurrentBrightness = 0;

            try
            {
                CurrentBrightness = DisplayConfiguration.GetMonitorBrightness(CurrentMonitor);
            }
            catch { }

            HotkeyPopupWindow.PercentText.Text = (CurrentBrightness * 100).ToString();
            HotkeyPopupWindow.Show();
            HotkeyPopupWindow.ShowMe(data["Misc"]["HotkeyPopupPosition"]);
        }

        public bool IsAnimationsEnabled => SystemParameters.ClientAreaAnimation &&
                                                  RenderCapability.Tier > 0;

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

            if (IsAnimationsEnabled)
                (FindResource("showMe") as Storyboard).Begin(this);
            else
                (FindResource("showMeWOAnim") as Storyboard).Begin(this);
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

                DisplayConfiguration.MonitorExtremums MonExtrs = DisplayConfiguration.GetMonitorExtremums(mon);

                MonitorList.Add(new MONITOR(Name, mon, MonExtrs.Min, MonExtrs.Max));
            }

            Monitor_List_Combobox.ItemsSource = MonitorList;
            Monitor_List_Combobox.SelectedItem = MonitorList[0];
            CurrentMonitor = MonitorList[0];
        }


        private void Window_Deactivated(object sender, EventArgs e)
        {
            /*Hide();*/
            (FindResource("hideMe") as Storyboard).Begin(this);
        }

        private void Monitor_List_Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentMonitor = MonitorList[Monitor_List_Combobox.SelectedIndex];
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

        public void SetHotkeysByStrings(string UpString, string DownString)
        {
            //unbind current bindings
            RemoveAllHotkeys();

            //brightness up
            Keys BrightnessUpKeys = GetKeys(UpString);
            HotkeyManager.Current.AddOrReplace("BrightnessUp", BrightnessUpKeys.MainKey, BrightnessUpKeys.Modifiers, OnBrightnessUp);

            //brightness down
            Keys BrightnessDownKeys = GetKeys(DownString);
            HotkeyManager.Current.AddOrReplace("BrightnessDown", BrightnessDownKeys.MainKey, BrightnessDownKeys.Modifiers, OnBrightnessDown);
        }

        public void RemoveAllHotkeys()
        {
            HotkeyManager.Current.Remove("BrightnessUp");
            HotkeyManager.Current.Remove("BrightnessDown");
        }

        private HotkeyPopup HotkeyPopupWindow = new HotkeyPopup();

        private void OnBrightnessUp(object sender, HotkeyEventArgs e)
        {
            BrightnessHotkeyEvent(true);
        }

        private void OnBrightnessDown(object sender, HotkeyEventArgs e)
        {
            BrightnessHotkeyEvent(false);
        }

        private void BrightnessHotkeyEvent(bool IsUp)
        {
            (FindResource("hideMe") as Storyboard).Begin(this);

            IniData data = SettingsController.GetCurrentSettings();

            try
            {
                DisplayConfiguration.PHYSICAL_MONITOR Handle = DisplayConfiguration.GetPhysicalMonitors(DisplayConfiguration.GetCurrentMonitor())[0];
                Task.Run(() => { try { DisplayConfiguration.SetBrightnessOffset(Handle, IsUp ? 0.05 : -0.05); } catch { } });

                if (HotkeyPopupWindow.IsVisible)
                {
                    int HotkeyPopupBrightness = int.Parse(HotkeyPopupWindow.PercentText.Text);
                    int NewBrightness = HotkeyPopupBrightness + (IsUp ? 5 : -5);

                    if (NewBrightness > 100) NewBrightness = 100;
                    else if (NewBrightness < 0) NewBrightness = 0;

                    HotkeyPopupWindow.PercentText.Text = NewBrightness.ToString();
                    HotkeyPopupWindow.ShowMe(data["Misc"]["HotkeyPopupPosition"]);
                }
                else
                {
                    double CurrentBrightness = DisplayConfiguration.GetMonitorBrightness(Handle);
                    HotkeyPopupWindow.PercentText.Text = (CurrentBrightness * 100).ToString();
                    HotkeyPopupWindow.ShowMe(data["Misc"]["HotkeyPopupPosition"]);
                }
            }
            catch { }
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

            UpdateCheckTimer.Tick += (sender, e) =>
            {
                CheckForUpdates(false);
            };

            if (data["Updates"]["DisableCheckEveryDay"] != "1")
                UpdateCheckTimer.Start();

            if (data["Updates"]["DisableCheckOnStartup"] != "1")
                CheckForUpdates(false);

            //AutoBrightness
            if (data["AutoBrightness"]["Enabled"] == "1")
                CheckForSunriset.Start();

            SetupAutoBrightnessTimer();

            TrayIcon.TrayBalloonTipClicked += (senderB, eB) => new Update().Window_Loaded();
        }

        public void CheckForUpdates(bool IsManual)
        {
            UpdateController UpdContr = new UpdateController();
            UpdContr.CheckForUpdatesAsync();
            UpdContr.CheckingComplete += (sender, IsAvailabe) =>
            {
                double factor = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

                if (IsAvailabe)
                {
                    TrayIcon.ShowBalloonTip("New Version is Available: " + UpdContr.NewVersionString, UpdContr.Description + " Click here to see more.", new Icon(Properties.Resources.updateIcon, new System.Drawing.Size(Convert.ToInt32(40 * factor), Convert.ToInt32(40 * factor))), true);
                } 
                else if (!IsAvailabe && IsManual)
                {
                    TrayIcon.ShowBalloonTip("No Updates Available", "You are using latest version.", new Icon(Properties.Resources.updateIcon, new System.Drawing.Size(Convert.ToInt32(40 * factor), Convert.ToInt32(40 * factor))), true);
                }
            };
        }

        public DispatcherTimer UpdateCheckTimer = new DispatcherTimer()
        {
            Interval = new TimeSpan(1, 0, 0, 0)
        };

        private void AfterUpdateCheck()
        {
            if (File.Exists("tinyBrightness.Old.exe"))
            {
                File.Delete("tinyBrightness.Old.exe");

                double factor = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

                TrayIcon.ShowBalloonTip("Update installed successfully!", "Enjoy new version :3", new Icon(Properties.Resources.updateIcon, new System.Drawing.Size(Convert.ToInt32(40 * factor), Convert.ToInt32(40 * factor))), true);
            }
        }

        #endregion

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        private void Slider_Brightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PercentText.Text = Convert.ToInt32(((Slider)sender).Value).ToString();

            debounceTimer.Throttle(50, (p) =>
            {
                try
                {
                    DisplayConfiguration.PHYSICAL_MONITOR Handle = CurrentMonitor.Handle;
                    uint Min = CurrentMonitor.Min;
                    uint Max = CurrentMonitor.Max;
                    DisplayConfiguration.SetMonitorBrightness(Handle, ((Slider)sender).Value / 100, Min, Max);
                }
                catch { }
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

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SetWindowPosition(); // workaround that prevents the settings window from freezing under certain conditions
            var Win = new Settings();
            Win.Owner = this;
            Win.Show();
            (FindResource("hideMe") as Storyboard).Begin(this);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            CheckForUpdates(true);
        }

        #endregion

        #region AutoBrightness

        private void AutoBrightnessOnPowerChange(object s, PowerModeChangedEventArgs e)
        {
            IniData data = SettingsController.GetCurrentSettings();

            switch (e.Mode)
            {
                case PowerModes.Resume:
                    if (data["AutoBrightness"]["Enabled"] == "1")
                    {
                        CheckForSunriset.Start();
                        TimeSpan CurrentTime = DateTime.UtcNow.TimeOfDay;
                        
                        SunrisetTools RisetTools = new SunrisetTools(AutoBrightnessSettings.GetLat(), AutoBrightnessSettings.GetLon());
                        System.Threading.Thread.Sleep(4000);
                        
/*                        foreach (MONITOR mon in MonitorList)
                        {
                            if (TimeSpan.Compare(CurrentTime, RisetTools.GetTodayDusk()) == 1)
                            {
                                try { DisplayConfiguration.SetMonitorBrightness(mon.Handle, AutoBrightnessSettings.GetAstroSunsetBrightness()); }
                                catch { }
                            }
                            else if (TimeSpan.Compare(CurrentTime, RisetTools.GetTodaySunset()) == 1)
                            {
                                try { DisplayConfiguration.SetMonitorBrightness(mon.Handle, AutoBrightnessSettings.GetSunsetBrightness()); }
                                catch { }
                            }
                            else if (TimeSpan.Compare(CurrentTime, RisetTools.GetTodaySunrise()) == 1)
                            {
                                try { DisplayConfiguration.SetMonitorBrightness(mon.Handle, AutoBrightnessSettings.GetSunriseBrightness()); }
                                catch { }
                            }
                            else if (TimeSpan.Compare(CurrentTime, RisetTools.GetTodayDawn()) == 1)
                            {
                                try { DisplayConfiguration.SetMonitorBrightness(mon.Handle, AutoBrightnessSettings.GetAstroSunriseBrightness()); }
                                catch { }
                            }
                        }
*/                    }
                    break;
                case PowerModes.Suspend:
                    CheckForSunriset.Stop();
                    break;
            }
        }

        public DispatcherTimer CheckForSunriset = new DispatcherTimer()
        {
            Interval = new TimeSpan(0, 1, 0)
        };

        public void SetupAutoBrightnessTimer()
        {
            SystemEvents.PowerModeChanged += AutoBrightnessOnPowerChange;
            CheckForSunriset.Tick += (sender, e) =>
            {
                TimeSpan CurrentTime = DateTime.UtcNow.TimeOfDay;

                SunrisetTools RisetTools = new SunrisetTools(AutoBrightnessSettings.GetLat(), AutoBrightnessSettings.GetLon());

/*                foreach (MONITOR mon in MonitorList)
                {
                    if (TimeSpan.Compare(CurrentTime, RisetTools.GetTodaySunrise()) == 0)
                    {
                        try { DisplayConfiguration.SetMonitorBrightness(mon.Handle, AutoBrightnessSettings.GetSunriseBrightness()); }
                        catch { }
                    }
                    else if (TimeSpan.Compare(CurrentTime, RisetTools.GetTodaySunset()) == 0)
                    {
                        try { DisplayConfiguration.SetMonitorBrightness(mon.Handle, AutoBrightnessSettings.GetSunsetBrightness()); }
                        catch { }
                    }
                    else if (TimeSpan.Compare(CurrentTime, RisetTools.GetTodayDawn()) == 0)
                    {
                        try { DisplayConfiguration.SetMonitorBrightness(mon.Handle, AutoBrightnessSettings.GetAstroSunriseBrightness()); }
                        catch { }
                    }
                    else if (TimeSpan.Compare(CurrentTime, RisetTools.GetTodayDusk()) == 0)
                    {
                        try { DisplayConfiguration.SetMonitorBrightness(mon.Handle, AutoBrightnessSettings.GetAstroSunsetBrightness()); }
                        catch { }
                    }
                }
*/            };
        }

        #endregion
    }
}
