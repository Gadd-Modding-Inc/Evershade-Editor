using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EvershadeEditor.UI {
    public class TextureDisplay : Border {
        public uint Index;
        public uint ChunkIndex;

        public BitmapImage PreviewTexture {
            get { return (BitmapImage)GetValue(PreviewTextureProperty); }
            set { SetValue(PreviewTextureProperty, value); }
        }

        public string TextureName {
            get { return (string)GetValue(TextureNameProperty); }
            set { SetValue(TextureNameProperty, value); }
        }

        public TexDisplayState DisplayState {
            get { return (TexDisplayState)GetValue(DisplayStateProperty); }
            set { SetValue(DisplayStateProperty, value); }
        }

        public static readonly DependencyProperty PreviewTextureProperty = DependencyProperty.Register(
            "PreviewTexture",
            typeof(BitmapImage),
            typeof(TextureDisplay),
            new PropertyMetadata(null, OnPreviewTextureChanged));

        public static readonly DependencyProperty TextureNameProperty = DependencyProperty.Register(
            "TextureName",
            typeof(string),
            typeof(TextureDisplay),
            new PropertyMetadata(string.Empty, OnTextureNameChanged));


        public static readonly DependencyProperty DisplayStateProperty = DependencyProperty.Register(
            "DisplayState",
            typeof(TexDisplayState),
            typeof(TextureDisplay),
            new PropertyMetadata(TexDisplayState.None, OnDisplayStateChanged));

        private readonly Image this_PreviewTexture;
        private readonly TextBlock this_TextureName;

        public TextureDisplay() {
            Background = (SolidColorBrush)Application.Current.Resources["ElementBrush"];
            BorderBrush = (SolidColorBrush)Application.Current.Resources["FocusBrush"];
            CornerRadius = new CornerRadius(5);
            BorderThickness = new Thickness(1);
            Margin = new Thickness(0, 10, 0, 0);
            Height = 50;

            Grid grid = new Grid();

            this_PreviewTexture = new Image {
                Width = 40,
                Height = 40,
                Margin = new Thickness(5, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            this_TextureName = new TextBlock {
                Foreground = (SolidColorBrush)Application.Current.Resources["TextBrush"],
                FontSize = 13,
                Margin = new Thickness(60, 0, 5, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            grid.Children.Add(this_PreviewTexture);
            grid.Children.Add(this_TextureName);
            Child = grid;

            MouseEnter += (s, e) => { if (DisplayState != TexDisplayState.Selected) { DisplayState = TexDisplayState.Hover; } };
            MouseLeave += (s, e) => { if (DisplayState == TexDisplayState.Hover) { DisplayState = TexDisplayState.None; } };
        }

        private static void OnPreviewTextureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            TextureDisplay? controlDisplay = (TextureDisplay)d;
            controlDisplay.this_PreviewTexture.Source = (BitmapImage)e.NewValue;
        }

        private static void OnTextureNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            TextureDisplay? control = (TextureDisplay)d;
            control.this_TextureName.Text = (string)e.NewValue;

            control.ToolTip = (string)e.NewValue;
        }

        private static void OnDisplayStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            TextureDisplay? controlDisplay = (TextureDisplay)d;
            string backgroundKey = "ElementBrush";
            string borderBrushKey = "FocusBrush";

            switch ((TexDisplayState)e.NewValue) {
                case TexDisplayState.Hover:
                    backgroundKey = "FocusBrush";
                    break;
                case TexDisplayState.Selected:
                    backgroundKey = "AccentBrush";
                    borderBrushKey = "AccentBrush";
                    break;
            }

            controlDisplay.Background = (SolidColorBrush)Application.Current.Resources[backgroundKey];
            controlDisplay.BorderBrush = (SolidColorBrush)Application.Current.Resources[borderBrushKey];
        }
    }

    public enum TexDisplayState {
        None,
        Hover,
        Selected
    }
}