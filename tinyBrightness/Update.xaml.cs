using System;
using System.Net;
using System.Windows;
using IniParser.Model;
using Newtonsoft.Json.Linq;

namespace tinyBrightness
{
    /// <summary>
    /// Логика взаимодействия для Update.xaml
    /// </summary>
    public partial class Update : Window
    {
        public Update()
        {
            InitializeComponent();
        }

        private string ChangeLogUrl = "";
        private string DownloadUrl = "";
        private string NewVersionString = "";

        public void Window_Loaded(bool IsManualCheck)
        {
            IniData data = SettingsController.GetCurrentSettings();

            if ((data["Updates"]["DisableCheckOnStartup"] != "1" && !IsManualCheck) || IsManualCheck)
                using (WebClient client = new WebClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    client.Headers.Add("user-agent", "request");
                    client.DownloadStringCompleted += (sender, e) =>
                    {
                        string result = null;
                        try
                        {
                            result = e.Result;
                            JObject json_res = JObject.Parse(result);
                            NewVersionString = json_res["tag_name"].ToString();
                            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                            float CurrentVersion = float.Parse(version.Major + "," + version.Minor);

                            float NewVersion = float.Parse(json_res["tag_name"].ToString().Replace('.', ','));

                            VersionLabel.Text = "Version: " + json_res["tag_name"];
                            ChangeLogUrl = json_res["html_url"].ToString();

                            if ((NewVersion > CurrentVersion) && (data["Updates"]["SkipVersion"] != json_res["tag_name"].ToString()))
                            {
                                DescLabel.Text = json_res["name"].ToString();
                                DownloadUrl = json_res["assets"][0]["browser_download_url"].ToString();
                                Show();
                            }
                            else
                            {
                                if (IsManualCheck)
                                {
                                    HeadingText.Text = "You are using latest version.";
                                    SkipButton.IsEnabled = false;
                                    DownloadButton.IsEnabled = false;
                                    Show();
                                }
                            }
                        }
                        catch (Exception ex) {
                            if (IsManualCheck)
                            {
                                Show();
                                MessageBox.Show(ex.InnerException.Message, "tinyBrightness Update check fail.", MessageBoxButton.OK, MessageBoxImage.Error);
                                Close();
                            }
                        }

                    };
                    client.DownloadStringAsync(new Uri("https://api.github.com/repos/nik9play/tinyBrightness/releases/latest"));
                }
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            IniData data = SettingsController.GetCurrentSettings();
            data["Updates"]["SkipVersion"] = NewVersionString;
            SettingsController.SaveSettings(data);
            Close();
        }

        private void ChangeLog_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(ChangeLogUrl);
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(DownloadUrl);
            Close();
        }
    }
}
