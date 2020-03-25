using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ScreenTools;

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
        }

        private void Window_Start()
        {
            Show();

            Set_Initial_Brightness();
        }

        private void Slider_Brightness_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
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
    }
}
