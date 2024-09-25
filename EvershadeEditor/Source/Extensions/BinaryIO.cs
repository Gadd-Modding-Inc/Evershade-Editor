using System;
using System.IO;
using System.Text;

public static class BinaryReaderExtensions {
    public static void TemporarySeek(this BinaryReader reader, long offset, SeekOrigin origin, Action<BinaryReader> action) {
        long originalPosition = reader.BaseStream.Position;

        reader.BaseStream.Seek(offset, origin);
        action(reader);
        reader.BaseStream.Seek(originalPosition, SeekOrigin.Begin);
    }

    public static string ReadZeroTerminatedString(this BinaryReader reader) {
        List<byte> byteList = new();
        byte currentByte;

        while ((currentByte = reader.ReadByte()) != 0x00) {
            byteList.Add(currentByte);
        }

        return Encoding.UTF8.GetString(byteList.ToArray());
    }
}

public static class BinaryWriterExtensions {
    public static void TemporarySeek(this BinaryWriter writer, long offset, SeekOrigin origin, Action<BinaryWriter> action) {
        long originalPosition = writer.BaseStream.Position;

        writer.BaseStream.Seek(offset, origin);
        action(writer);
        writer.BaseStream.Seek(originalPosition, SeekOrigin.Begin);
    }

    public static void AlignTo(this BinaryWriter writer, uint value) {
        long paddingRequired = value - (writer.BaseStream.Position % value);

        if (paddingRequired != 0) {
            writer.BaseStream.Seek(paddingRequired, SeekOrigin.Current);
        }
    }
}