using System;
using System.Text;

namespace EvershadeEditor {
    public static class ConsoleUtils {
        public static void WriteError(string msg, Exception ex) {
            StringBuilder builder = new();
            builder.AppendLine($"{msg}");
            builder.AppendLine($"  {ex.Message}");

            if (App.Settings.ShowStackTrace && ex.StackTrace != null) {
                builder.AppendLine(ex.StackTrace);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(builder.ToString());
            Console.ResetColor();
        }

        public static void WriteWarning(string msg) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static void WriteImportant(string msg) {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}