using IniParser.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace tinyBrightness
{
    class UpdateController
    {
        public event EventHandler<bool> CheckingComplete;

        public int NewVersionMinor = 0;
        public string NewVersionString = "";
        public string Description = "";
        public string ChangeLogUrl = "";
        public string DownloadUrl = "";
        public void CheckForUpdatesAsync()
        {
            IniData data = SettingsController.GetCurrentSettings();

            WebClient client = new WebClient();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            client.Headers.Add("user-agent", "request");
            client.DownloadStringAsync(new Uri("https://api.github.com/repos/nik9play/tinyBrightness/releases/latest"));
            client.DownloadStringCompleted += (sender, e) =>
            {
                try
                {
                    JObject json_res = JObject.Parse(e.Result);
                    Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    int CurrentVersionMinor = version.Minor;
                    NewVersionMinor = int.Parse(json_res["tag_name"].ToString().Split('.')[1]);
                    NewVersionString = json_res["tag_name"].ToString();

                    //1 is exe and 0 is zip
                    DownloadUrl = json_res["assets"][1]["browser_download_url"].ToString();
                    Description = json_res["name"].ToString();
                    ChangeLogUrl = json_res["html_url"].ToString();

                    if ((NewVersionMinor > CurrentVersionMinor) && (data["Updates"]["SkipVersion"] != json_res["tag_name"].ToString()))
                        OnCheckingCompleted(true);
                    else
                        OnCheckingCompleted(false);
                }
                catch { OnCheckingCompleted(false); }
            };
        }

        protected virtual void OnCheckingCompleted(bool IsAvailable)
        {
            CheckingComplete?.Invoke(this, IsAvailable);
        }
    }
}
