﻿using System.Windows;
using ModernWpf.Controls;

namespace tinyBrightness
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void AcrylicWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void SettingsNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            string Tag = (string)selectedItem.Tag;
        
            switch (Tag)
            {
                case "General":
                    SettingsFrame.Navigate(typeof(SettingsPages.General));
                    break;
                case "AutoBrightness":
                    SettingsFrame.Navigate(typeof(SettingsPages.AutoBrightness));
                    break;
                case "Appearance":
                    SettingsFrame.Navigate(typeof(SettingsPages.Appearance));
                    break;
                case "Hotkeys":
                    SettingsFrame.Navigate(typeof(SettingsPages.Hotkeys));
                    break;
                case "About":
                    SettingsFrame.Navigate(typeof(SettingsPages.About));
                    break;
            }
        }
    }
}
