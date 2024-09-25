using System;
using System.Text;

public static class ByteExtensions {
    public static string ToFormatedString(this byte[] bytes) {
        if (bytes == null) {
            return "Empty Array";
        }

        StringBuilder strBuilder = new StringBuilder();

        for (int i = 0; i < bytes.Length; i++) {
            strBuilder.Append(bytes[i].ToString("X2")).Append(' ');

            if ((i + 1) % 16 == 0) {
                strBuilder.AppendLine();
            }
        }

        return strBuilder.ToString();
    }
}