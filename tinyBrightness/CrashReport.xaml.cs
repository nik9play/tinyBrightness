using System.Web;
using System.Windows;

namespace tinyBrightness
{
    /// <summary>
    /// Логика взаимодействия для CrashReport.xaml
    /// </summary>
    public partial class CrashReport : Window
    {
        public CrashReport()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string ExceptionMessage { get; set; } = "";

        public string StackTrace { get; set; } = "";

        public string StackTracePath { get; set; } = "";

        private void Create_Issue(object sender, RoutedEventArgs e)
        {
            string Url = "https://github.com/nik9play/tinyBrightness/issues/new?assignees=&labels=bug&template=bug_report.md&body=";
            string Body = $@"**Describe the bug**
A clear and concise description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior.

**Expected behavior**
A clear and concise description of what you expected to happen.

**Screenshots**
If applicable, add screenshots to help explain your problem.

**Error Log (autofilled)**
<details>
<summary>Open stack trace</summary>

```
{StackTrace}
```
</details>

**Software info**
 - OS [If you use Windows 10, please specify update version (e.g. 1903, 1809...)]
 - Device [Laptop, PC, Tablet, etc.]
 - Monitor count
 - Windows scaling [100 %, 125 %, etc.]
 - Version [e.g. 1.5]
";

            System.Diagnostics.Process.Start(Url + HttpUtility.UrlEncode(Body));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void Run_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select, {StackTracePath}");
            }
            catch { }
        }
    }
}
