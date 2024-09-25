using EvershadeEditor.LM2;
using EvershadeEditor.UI;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace EvershadeEditor {
    public partial class FileWindow : Window {
        private LM2File? LM2File;
        public uint SelectedTextureIndex;
        public bool IsSelected;

        private HashBin Hashes;

        public FileWindow() {
            InitializeComponent();
            
            this.Title = App.FullTitle;
            WindowName.Text = App.FullTitle;

            TexturePanelHint.Text = "Textures will show up here\nPlease open a file";

            this.StateChanged += WindowStateChanged;
            TitleBarClose.Click += (s, e) => Close();
            TitleBarMaximize.Click += (s, e) => WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
            TitleBarMinimize.Click += (s, e) => WindowState = WindowState.Minimized;
            TitleBarSettings.Click += (s, e) => new SettingsWindow().ShowDialog();

            MenuOpenButton.Click += OpenFileClick;
            MenuSaveButton.Click += SaveFileClick;
            MenuSaveAsButton.Click += SaveFileAsClick;

            ImportButton.Click += ImportTexture;
            ExportButton.Click += ExportTexture;
            CopyInfoButton.Click += CopyTextureInfo;
            MenuExportAllButton.Click += ExportAllTextures;

            ToggleFileOptions();
            Console.WriteLine("[APP] Loaded GUI");

            try {
                Hashes = new();
                Console.WriteLine("[APP] Loaded Hashes");
            }
            catch (Exception ex) {
                ConsoleUtils.WriteError($"[APP] Error loading hashes:", ex);
            }
        }

        // For the Window not to clip out of your monitor, a margin has to be added to WindowBorder
        private void WindowStateChanged(object? sender, EventArgs e) {
            WindowBorder.Margin = (WindowState == WindowState.Maximized) ? new Thickness(5) : new Thickness(0);
        }

        protected override void OnClosing(CancelEventArgs e) {
            if (LM2File == null) {
                base.OnClosing(e);
                return;
            }

            PopupWindow popupWindow = new PopupWindow(
                "Do you want to save changes before exiting?", App.Name,
                "Cancel", "No", "Yes");

            if (popupWindow.Result == 1) {
                base.OnClosing(e);
                return;
            }

            if (popupWindow.Result == 2) {
                try {
                    LM2File.Save();
                    base.OnClosing(e);
                    return;
                }
                catch (Exception ex) {
                    ConsoleUtils.WriteError($"[APP] Unexpected error while saving:\n", ex);
                }
            }

            e.Cancel = true;
        }

        private void OpenFileClick(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new() {
                Title = "Open a file",
                Multiselect = false,
                Filter = "Dictionary/Data Files|*.dict;*.data|Data Files|*.data|Dict Files|*.dict"
            };

            bool? result = dialog.ShowDialog();

            if (result == true) {
                IsSelected = false;

                PreviewImage.Source = null;
                TextureNameText.Text = String.Empty;
                TexturePropertyText.Text = String.Empty;

                try {
                    LM2File = new(dialog.FileName);
                    SelectedTextureIndex = 0;
                    UpdateTextureDisplay();
                    ToggleFileOptions();
                }
                catch (Exception ex) {
                    ConsoleUtils.WriteError($"[APP] Error when opening file:", ex);
                    TexturePanelHint.Text = "Textures will show up here\nPlease open a file";
                    LM2File = null;

                    TexturePanel.Children.Clear();
                }
            }
        }

        private void SaveFileClick(object sender, RoutedEventArgs e) {
            if (LM2File == null) { return; }

            if (!File.Exists(LM2File.DictPath) || !File.Exists(LM2File.DataPath)) {
                SaveFileAsClick(sender, e);
            }

            try {
                LM2File.Save();
            }
            catch (Exception ex) {
                ConsoleUtils.WriteError($"[APP] Error when saving file:", ex);
            }
        }

        private void SaveFileAsClick(object sender, RoutedEventArgs e) {
            if (LM2File == null) { return; }

            SaveFileDialog dialog = new() {
                Title = "Open a file",
                Filter = "Dictionary/Data Files|*.dict;*.data|Data Files|*.data|Dict Files|*.dict"
            };

            bool? result = dialog.ShowDialog();

            if (result == true) {
                try {
                    LM2File.Save(dialog.FileName);
                }
                catch (Exception ex) {
                    ConsoleUtils.WriteError($"[APP] Error when saving file:", ex);
                }
            }
        }

        private void ImportTexture(object sender, RoutedEventArgs e) {
            if (LM2File == null || !IsSelected) { return; }

            OpenFileDialog dialog = new() {
                Title = "Open a file to import",
                Multiselect = false,
                Filter = "DDS Texture|*.dds"
            };

            bool? result = dialog.ShowDialog();

            if (result == true) {
                try {
                    TextureChunk chunk = (TextureChunk)LM2File.GetDataChunk((int)SelectedTextureIndex);

                    LM2File.SetTexture(chunk, File.ReadAllBytes(dialog.FileName));
                    UpdateTexturePreview((int)SelectedTextureIndex, chunk.MakeBitmap());

                    Console.WriteLine($"[APP] Imported texture ({dialog.FileName})");
                }
                catch (Exception ex) {
                    ConsoleUtils.WriteError($"[APP] Error when importing texture:", ex);
                }
            }
        }

        private void ExportTexture(object sender, RoutedEventArgs e) {
            if (LM2File == null || !IsSelected) { return; }

            SaveFileDialog dialog = new() {
                Title = "Open a file",
                Filter = "DDS Image|*.dds"
            };

            bool? result = dialog.ShowDialog();

            if (result == true) {
                try {
                    File.WriteAllBytes(dialog.FileName, LM2File.GetDataChunk((int)SelectedTextureIndex).Children[1].Data);
                    Console.WriteLine($"[APP] Exported texture ({dialog.FileName})");
                }
                catch (Exception ex) {
                    ConsoleUtils.WriteError($"[APP] Error when export image:", ex);
                }
            }
        }

        private void CopyTextureInfo(object sender, RoutedEventArgs e) {
            if (LM2File == null || !IsSelected) { return; }
            Clipboard.SetText($"""
                Texture Name: {TextureNameText.Text}
                {TexturePropertyText.Text}
                """);
        }

        private void ExportAllTextures(object sender, RoutedEventArgs e) {
            if (LM2File == null || !IsSelected) { return; }

            OpenFolderDialog folderDialog = new() {
                Title = "Select a folder to export all textures",
                Multiselect = false
            };

            bool? result = folderDialog.ShowDialog();

            if (result == true) {
                try {
                    foreach (ChunkFileEntry file in LM2File.TextureFiles) {
                        TextureChunk chunk = (TextureChunk)file.DataChunk;

                        string filePath = Path.Combine(folderDialog.FolderName,
                            $"{Path.GetFileNameWithoutExtension(LM2File.DataPath)}_" +
                            Path.ChangeExtension(Hashes.GetHashValue(file.FileHash, true), ".dds"));

                        File.WriteAllBytes(filePath, chunk.Children[1].Data);
                    }

                    Console.WriteLine("[APP] Exported all textures successfully");
                }
                catch (Exception ex) {
                    ConsoleUtils.WriteError($"[APP] Error when exporting all textures:", ex);
                }
            }
        }

        private void UpdateTextureDisplay() {
            TexturePanel.Children.Clear();

            if (LM2File.TextureFiles.Count <= 0) {
                TexturePanelHint.Text = "File has no textures";
                ConsoleUtils.WriteWarning($"[APP] The file {Path.GetFileName(LM2File.DataPath)} has no textures");
                return;
            }

            int i = 0;
            foreach (ChunkFileEntry file in LM2File.TextureFiles) {
                TextureChunk texChunk = (TextureChunk)LM2File.GetDataChunk(i);

                TextureDisplay display = new TextureDisplay() {
                    ChunkIndex = texChunk.Index,
                    PreviewTexture = texChunk.MakeBitmap(),
                    TextureName = Path.ChangeExtension(Hashes.GetHashValue(file.FileHash, true), ".dds"),
                    Index = (uint)i,
                };

                display.MouseDown += DisplayTextureClick;
                TexturePanel.Children.Add(display);

                i++;
            }

            TexturePanelHint.Text = String.Empty;
        }

        private void DisplayTextureClick(object sender, MouseButtonEventArgs e) {
            SelectTexture(((TextureDisplay)sender).Index, false);
        }

        private void SelectTexture(uint newIndex, bool scrollToDisplay) {
            TextureDisplay oldDisplay = (TextureDisplay)TexturePanel.Children[(int)SelectedTextureIndex];
            TextureDisplay display = (TextureDisplay)TexturePanel.Children[(int)newIndex];

            oldDisplay.DisplayState = TexDisplayState.None;
            display.DisplayState = TexDisplayState.Selected;

            SelectedTextureIndex = display.Index;
            PreviewImage.Source = display.PreviewTexture;
            TextureNameText.Text = display.TextureName;
            TexturePropertyText.Text = GetTextureInfo((int)display.Index);

            if (scrollToDisplay) {
                TexturePanelScroll.ScrollToVerticalOffset(newIndex * 60);
            }

            IsSelected = true;
            ToggleSelectOptions();
        }

        private void UpdateTexturePreview(int index, BitmapImage bitmap) {
            ((TextureDisplay)TexturePanel.Children[index]).PreviewTexture = bitmap;
            PreviewImage.Source = ((TextureDisplay)TexturePanel.Children[index]).PreviewTexture;
            TexturePropertyText.Text = GetTextureInfo(index);
        }

        private void ToggleFileOptions() {
            bool fileState = (LM2File != null);
            MenuSaveButton.IsEnabled = fileState;
            MenuSaveAsButton.IsEnabled = fileState;

            ToggleSelectOptions();
        }

        private void ToggleSelectOptions() {
            ImportButton.IsEnabled = IsSelected;
            ExportButton.IsEnabled = IsSelected;
            CopyInfoButton.IsEnabled = IsSelected;
        }

        private string GetTextureInfo(int index) {
            ChunkFileEntry file = (ChunkFileEntry)LM2File.TextureFiles[index];
            TextureChunk chunk = (TextureChunk)LM2File.GetDataChunk(index);

            return $"""
                Texture Hash: {file.FileHash:X8}
                Texture Size: {chunk.Children[1].Size.FormatBytes()} ({chunk.Children[1].Size} B)
                Texture Dimension: {chunk.TexWidth}x{chunk.TexHeight}
                Texture Depth: {chunk.TexDepth}
                Texture Array Count: {chunk.TexArrayCount}
                Texture Mipmap Level: {chunk.TexMipmapLevel}
                Texture Compression: {chunk.GetCompression()}
                """;
        }
    }
}

/*
OpenFolderDialog folderDialog = new() {
    Title = "Test",
    Multiselect = false
};

folderDialog.ShowDialog();
*/