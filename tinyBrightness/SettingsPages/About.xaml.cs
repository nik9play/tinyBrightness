using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace tinyBrightness.SettingsPages
{
    /// <summary>
    /// Логика взаимодействия для About.xaml
    /// </summary>
    public partial class About : Page
    {
        public About()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                                    .AddDays(version.Build).AddSeconds(version.Revision * 2);
            string displayableVersion = $"{version} ({buildDate})";
            Version_Text.Text = displayableVersion;
        }

        private void StartBenchmarkButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayConfiguration.PHYSICAL_MONITOR Handle = DisplayConfiguration.GetPhysicalMonitors(DisplayConfiguration.GetCurrentMonitor())[0];
            var GetBrightnessWatch = System.Diagnostics.Stopwatch.StartNew();
            double Brightness = DisplayConfiguration.GetMonitorBrightness(Handle);
            GetBrightnessWatch.Stop();

            var SetBrightnessWatchExtrems = System.Diagnostics.Stopwatch.StartNew();
            DisplayConfiguration.MonitorExtremums MonExtems = DisplayConfiguration.GetMonitorExtremums(Handle);
            uint dwMinimumBrightness = MonExtems.Min;
            uint dwMaximumBrightness = MonExtems.Max;
            DisplayConfiguration.SetMonitorBrightness(Handle, 1, dwMinimumBrightness, dwMaximumBrightness);
            SetBrightnessWatchExtrems.Stop();

            var SetBrightnessWatch = System.Diagnostics.Stopwatch.StartNew();
            DisplayConfiguration.SetMonitorBrightness(Handle, Brightness, dwMinimumBrightness, dwMaximumBrightness);
            SetBrightnessWatch.Stop();

            MessageBox.Show($"GetBrightness: {GetBrightnessWatch.ElapsedMilliseconds} ms.\n" +
                $"SetBrightnessExtrems: {SetBrightnessWatchExtrems.ElapsedMilliseconds} ms.\n" +
                $"SetBrightness: {SetBrightnessWatch.ElapsedMilliseconds} ms.\n\n" +
                $"Total: {GetBrightnessWatch.ElapsedMilliseconds + SetBrightnessWatchExtrems.ElapsedMilliseconds + SetBrightnessWatch.ElapsedMilliseconds} ms.", "Benchmark Results");
        }
    }
}
