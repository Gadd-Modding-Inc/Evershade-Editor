using System;

public static class IntegerExtensions {
    public static string FormatBytes(this uint bytes) {
        double num = bytes;
        string[] extensions = { "B", "KB", "MB", "GB", "TB" };

        byte extIndex = 0;
        while (num >= 1024 && extIndex < extensions.Length - 1) {
            num /= 1024;
            extIndex++;
        }

        return $"{num:0.00} {extensions[extIndex]}";
    }

    public static bool IsPowerOfTwo(this ushort value) {
        return (value & (value - 1)) == 0;
    }
}