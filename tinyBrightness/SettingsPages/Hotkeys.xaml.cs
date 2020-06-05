using IniParser.Model;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace tinyBrightness.SettingsPages
{
    /// <summary>
    /// Логика взаимодействия для Hotkeys.xaml
    /// </summary>
    public partial class Hotkeys : Page
    {
        public Hotkeys()
        {
            InitializeComponent();
        }

        Window Owner;
        private bool Ready = false;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Owner = Window.GetWindow(this).Owner;

            IniData data = SettingsController.GetCurrentSettings();

            BrightnessUpTextbox.Text = data["Hotkeys"]["HotkeyUp"];
            BrightnessDownTextbox.Text = data["Hotkeys"]["HotkeyDown"];

            if (data["Hotkeys"]["HotkeysEnable"] == "1")
                HotkeysSwitch.IsOn = true;

            int StepSize = 5;
            if (int.TryParse(data["Hotkeys"]["StepSize"], out int StepSizeValue)) StepSize = StepSizeValue;
            
            StepSlider.Value = StepSize;

            Ready = true;
        }

        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            ((TextBox)sender).Text = GetHotkey(key).ToString();
            Keyboard.ClearFocus();
        }

        private StringBuilder GetHotkey(Key key)
        {
            StringBuilder HotkeyText = new StringBuilder();
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                HotkeyText.Append("Ctrl+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                HotkeyText.Append("Shift+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                HotkeyText.Append("Alt+");
            }
            HotkeyText.Append(key.ToString());

            return HotkeyText;
        }

        private void HotkeysSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            IniData data = SettingsController.GetCurrentSettings();

            if (HotkeysSwitch.IsOn)
            {
                data["Hotkeys"]["HotkeysEnable"] = "1";
                ((MainWindow)Owner).SetHotkeysByStrings(BrightnessUpTextbox.Text, BrightnessDownTextbox.Text);
            }
            else
            {
                data["Hotkeys"]["HotkeysEnable"] = "0";
                ((MainWindow)Owner).RemoveAllHotkeys();
            }

            SettingsController.SaveSettings(data);
        }

        private void BrightnessUpTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text == "")
                ((TextBox)sender).Text = "Ctrl+Shift+Add";
            else
            {
                IniData data = SettingsController.GetCurrentSettings();

                data["Hotkeys"]["HotkeyUp"] = ((TextBox)sender).Text;

                SettingsController.SaveSettings(data);

                if (HotkeysSwitch.IsOn)
                    ((MainWindow)Owner).SetHotkeysByStrings(BrightnessUpTextbox.Text, BrightnessDownTextbox.Text);
            }
        }

        private void BrightnessDownTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text == "")
                ((TextBox)sender).Text = "Ctrl+Shift+Subtract";
            else
            {
                IniData data = SettingsController.GetCurrentSettings();

                data["Hotkeys"]["HotkeyDown"] = ((TextBox)sender).Text;

                SettingsController.SaveSettings(data);

                if (HotkeysSwitch.IsOn)
                    ((MainWindow)Owner).SetHotkeysByStrings(BrightnessUpTextbox.Text, BrightnessDownTextbox.Text);
            }
        }

        private void StepSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Ready)
            {
                IniData data = SettingsController.GetCurrentSettings();

                data["Hotkeys"]["StepSize"] = StepSlider.Value.ToString();

                SettingsController.SaveSettings(data);
            }
        }
    }
}
