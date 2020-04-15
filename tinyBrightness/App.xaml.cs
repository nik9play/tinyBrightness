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

            SourceChord.FluentWPF.SystemTheme.ThemeChanged += (senderIcon, eIcon) => mainWindow.AdaptIconToTheme();

            new Update().Window_Loaded(false);
        }
    }
}
