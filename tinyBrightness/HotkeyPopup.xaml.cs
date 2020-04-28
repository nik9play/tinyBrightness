using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace tinyBrightness
{
    /// <summary>
    /// Логика взаимодействия для HotkeyPopup.xaml
    /// </summary>
    public partial class HotkeyPopup : Window
    {
        public HotkeyPopup()
        {
            InitializeComponent();
            DataContext = this;
        }

        private DispatcherTimer HideTimer = new DispatcherTimer()
        {
            Interval = new TimeSpan(0, 0, 3)
        };

        public void ShowMe(string Position)
        {
            double factor = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

            var desktopWorkingArea = Screen.GetWorkingArea(System.Windows.Forms.Control.MousePosition);

            switch (Position)
            {
                case "Bottom Right":
                    Top = desktopWorkingArea.Bottom / factor - Height - 50;
                    Left = desktopWorkingArea.Right / factor - Width - 50;
                    break;
                case "Top Right":
                    Top = 50;
                    Left = desktopWorkingArea.Right / factor - Width - 50;
                    break;
                case "Bottom Left":
                    Top = desktopWorkingArea.Bottom / factor - Height - 50;
                    Left = 50;
                    break;
                case "Top Left":
                default:
                    Top = 50;
                    Left = 50;
                    break;

            }

            Storyboard StoryboardShow = FindResource("showMe") as Storyboard;

            if (Opacity < 1 || Visibility == Visibility.Hidden)
            {
                StoryboardShow.Begin(this);
            }

            HideTimer.Stop();
            HideTimer.Start();
        }

        public void HideMe()
        {
            Storyboard StoryboardHide = FindResource("hideMe") as Storyboard;

            StoryboardHide.Begin(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HideTimer.Tick += (senderT, eT) => {
                HideMe();
                HideTimer.Stop();
            };
        }
    }
}
