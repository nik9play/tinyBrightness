using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Threading.Tasks;
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
/*            if (!startMinimized)
            {
                mainWindow.Visibility = Visibility.Visible;
            }*/
        }
    }
}
