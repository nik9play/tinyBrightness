using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
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

        public string ChangeLogUrl = "";
        public string DownloadUrl = "";
        public string Description = "";
        public string Version = "";

        public void Window_Loaded()
        {
            UpdateController UpdCtr = new UpdateController();
            UpdCtr.CheckingComplete += (sender, IsAvailabe) =>
            {
                if (IsAvailabe)
                {
                    ChangeLogUrl = UpdCtr.ChangeLogUrl;
                    DownloadUrl = UpdCtr.DownloadUrl;
                    Description = DescLabel.Text = UpdCtr.Description;
                    Version = VersionLabel.Text = UpdCtr.NewVersionString;

                    Show();
                } 
                else
                {
                    Close();
                }

            };
            UpdCtr.CheckForUpdatesAsync();
        }

        private void ChangeLog_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(ChangeLogUrl);
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            DownloadContainer.Visibility = Visibility.Visible;
            DownloadButton.IsEnabled = false;
            DownloadProgressRing.IsActive = true;

            Assembly currentAssembly = Assembly.GetEntryAssembly();
            string OldFileName = Path.GetFileName(currentAssembly.Location);

            File.Delete("tinyBrightness.Old.exe");
            File.Move(OldFileName, "tinyBrightness.Old.exe");
            File.SetAttributes("tinyBrightness.Old.exe", FileAttributes.Hidden);

            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += (senderP, eP) =>
                {
                    DownloadPercent.Text = eP.ProgressPercentage + "%";
                };

                wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new Uri(DownloadUrl), OldFileName);
                Closing += (senderClose, eClose) => wc.CancelAsync();
            }

            void wc_DownloadFileCompleted(object senderC, AsyncCompletedEventArgs eC)
            {
                if (eC.Cancelled)
                {
                    File.Delete(OldFileName);
                    File.Move("tinyBrightness.Old.exe", OldFileName);
                    File.SetAttributes(OldFileName, FileAttributes.Normal);
                    return;
                }

                if (eC.Error != null)
                {
                    MessageBox.Show("An error ocurred while trying to download file");
                    File.Delete(OldFileName);
                    File.Move("tinyBrightness.Old.exe", OldFileName);
                    File.SetAttributes(OldFileName, FileAttributes.Normal);
                    DownloadContainer.Visibility = Visibility.Hidden;
                    DownloadButton.IsEnabled = true;
                    DownloadProgressRing.IsActive = false;
                    return;
                }

                System.Diagnostics.Process.Start(OldFileName);
                Application.Current.Shutdown();
            }
        }
    }
}
