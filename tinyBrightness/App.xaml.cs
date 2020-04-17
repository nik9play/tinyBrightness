using Microsoft.Win32;
using System;
using System.Windows;

namespace tinyBrightness
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool startMinimized = false;
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "--silent")
                {
                    startMinimized = true;
                }
            }

            MainWindow mainWindow = new MainWindow();

            if (startMinimized)
                mainWindow.Visibility = Visibility.Hidden;

            mainWindow.LoadSettings();


            if (Environment.OSVersion.Version.Major == 10)
            {
                int releaseId = int.Parse(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "").ToString());

                if (releaseId >= 1903)
                    SourceChord.FluentWPF.SystemTheme.ThemeChanged += (senderIcon, eIcon) => mainWindow.AdaptIconToTheme();
            }

            new Update().Window_Loaded(false);
        }
    }
}
