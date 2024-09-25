using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Configuration;
using System.Data;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace EvershadeEditor {
    public partial class App : Application {
        public const string Name = "Evershade Editor";
        public const string Version = "1.0.0";
        public const string FullTitle = $"{Name} [{Version} - Experimental]";

        public static string HashPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hashes.bin");

        public static Settings Settings = new();

        public App() {
            ConsoleUtils.WriteImportant($"[APP] Started {FullTitle}");
            Console.Title = FullTitle;
        }

        protected override void OnStartup(StartupEventArgs e) {
            try {
                Settings.LoadSettings();
            }
            catch (Exception ex) {
                ConsoleUtils.WriteError($"Could not load settings:", ex);
            }
            
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e) {
            ConsoleUtils.WriteImportant($"[APP] Closing {App.Name}\n");
            try {
                Settings.SaveSettings();
            }
            catch (Exception ex) {
                ConsoleUtils.WriteError($"Could not save settings:", ex);
            }

            base.OnExit(e);
        }
    }

    public class Settings {
        [JsonInclude] public bool DarkMode = true;
        [JsonInclude] public bool ShowStackTrace = false;
        [JsonInclude] public string LastPath = string.Empty;
        [JsonInclude] public string NVTTPath = string.Empty;

        public readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{App.Name.Replace(" ", "")}.settings.json");
        public void LoadSettings() {
            if (!File.Exists(SettingsPath)) {
                return;
            }

            Settings? settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsPath));

            if (settings == null) {
                throw new JsonException("Could not load settings. Using default.");
            }

            DarkMode = settings.DarkMode;
            ShowStackTrace = settings.ShowStackTrace;
            LastPath = settings.LastPath;
            NVTTPath = settings.NVTTPath;

            App.Current.Resources.MergedDictionaries[0] = new() {
                Source = new(DarkMode ? "/Themes/ColorsDark.xaml" : "/Themes/ColorsLight.xaml", UriKind.Relative)
            };
        }

        public void SaveSettings() {
            string json = JsonSerializer.Serialize(this);
            File.WriteAllText(SettingsPath, json);
        }
    }

    public static class AppUtils {
        public static void Restart() {
            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{App.Name.Replace(" ", "")}.exe"),
            };

            Process.Start(startInfo);
            App.Current.Shutdown();
        }
    }
}
