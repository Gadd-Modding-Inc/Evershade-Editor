using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EvershadeEditor.UI {
    public partial class SettingsWindow : Window {
        public SettingsWindow() {
            InitializeComponent();

            this.Title = $"{App.Name} - Settings";
            WindowName.Text = $"{App.Name} - Settings";

            TitleBarClose.Click += (s, e) => Close();

            DarkModeCheck.IsChecked = App.Settings.DarkMode;
            DarkModeCheck.Click  += ToggleLightMode;

            ShowTraceCheck.IsChecked = App.Settings.ShowStackTrace;
            ShowTraceCheck.Click += ToggleShowTrace;
        }

        private void ToggleLightMode(object sender, RoutedEventArgs e) {
            PopupWindow popupWindow = new PopupWindow(
                "Changing theme requires a restart. Please restart later.", App.Name,
                "OK");

            App.Settings.DarkMode = (bool)DarkModeCheck.IsChecked;

            Console.WriteLine($"[APP] Dark Mode set to \"{App.Settings.DarkMode}\"");
        }

        private void ToggleShowTrace(object sender, RoutedEventArgs e) {
            App.Settings.ShowStackTrace = (bool)DarkModeCheck.IsChecked;

            Console.WriteLine($"[APP] Show Stack Trace set to \"{App.Settings.ShowStackTrace}\"");
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);
        }
    }
}
