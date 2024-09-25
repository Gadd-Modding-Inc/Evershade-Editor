using System.Windows;
using System.Windows.Controls;

namespace EvershadeEditor.UI {
    public partial class PopupWindow : Window {
        public sbyte Result { get; private set; } = -1;

        public PopupWindow(string message, string title, string button1Text, string? button2Text = null, string? button3Text = null) {
            InitializeComponent();

            this.Title = title;
            PopupTitle.Text = title;
            PopupMessage.Text = message;

            Button1.Content = button1Text;
            Button1.Click += PopupButtonClick;
            Button1.Tag = (sbyte)0;

            if (button2Text != null) {
                Button2.Content = button2Text;
                Button2.Click += PopupButtonClick;
                Button2.Tag = (sbyte)1;
            } else {
                Button2.Visibility = Visibility.Hidden;
                Button2.IsEnabled = false;
            }

            if (button3Text != null) {
                Button3.Content = button3Text;
                Button3.Click += PopupButtonClick;
                Button3.Tag = (sbyte)2;
            } else {
                Button3.Visibility = Visibility.Hidden;
                Button3.IsEnabled = false;
            }

            ShowDialog();
        }

        private void PopupButtonClick(object sender, RoutedEventArgs e) {
            Button button = ((Button)sender);
            Result = (sbyte)button.Tag;

            Close();
        }
    }
}
