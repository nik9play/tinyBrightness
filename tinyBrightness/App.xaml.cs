using Microsoft.Win32;
using System;
using System.Windows;
using System.IO;

namespace tinyBrightness
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool startMinimized = false;
            for (int i = 0; i != e.Args.Length; ++i)
                if (e.Args[i] == "--silent")
                    startMinimized = true;

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

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string path = Directory.GetCurrentDirectory() + "\\tb_ErrorLog_" + DateTime.Now.ToString("dd.MM.yyyy_HH.mm.ss") + ".log";

            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(e.Exception.StackTrace);
            }

            CrashReport win = new CrashReport
            {
                ExceptionMessage = e.Exception.Message,
                StackTrace = e.Exception.StackTrace,
                StackTracePath = path
            };
            win.Show();

            e.Handled = true;
        }
    }
}
