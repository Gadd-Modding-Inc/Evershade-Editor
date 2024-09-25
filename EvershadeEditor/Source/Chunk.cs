using System;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EvershadeEditor.LM2 {
    public class ChunkEntry {
        public ushort Type;
        public ushort Flags;
        public uint Offset;
        public uint Size;

        public ChunkEntry[]? Children;
        public byte[]? Data;

        public uint Index;

        public ushort RawAlignment => (ushort)(Flags >> 1 & 10);
        public byte BlockIndex => (byte)(Flags >> 12 & 3);
        public bool HasChildren => (Flags >> 15 & 1) != 0;
        public ushort Alignment {
            get {
                if (RawAlignment == 512) {
                    return 16;
                }

                if ((byte)(Flags >> 8 & 1) != 0) {
                    return 8;
                }

                return 4;
            }
        }
    }

    public class ChunkFileEntry : ChunkEntry {
        public ChunkEntry DataChunk;

        public uint FileType;
        public uint FileHash;

        public void Read() {
            using (MemoryStream stream = new MemoryStream(Data))
            using (BinaryReader reader = new BinaryReader(stream)) {
                FileType = reader.ReadUInt32();
                FileHash = reader.ReadUInt32();
            }   
        }
    }

    public class TextureChunk : ChunkEntry {
        public uint TexSize;
        public uint TexHash;
        public uint TexFlags;
        public ushort TexWidth;
        public ushort TexHeight;
        public ushort TexDepth;
        public byte TexArrayCount;
        private byte RawTexMipmapLevel;
        public uint Compression;

        public byte TexMipmapLevel {
            get {
                return (byte)(RawTexMipmapLevel & 0xF);
            }
            set {
                if (value < 1 || value > 9) {
                    throw new Exception("Mipmap Levels can only be set from 1 to 9");
                }
                RawTexMipmapLevel = (byte)(value * 0x11);
            }
        }

        public void Read() {
            using (MemoryStream stream = new MemoryStream(Children[0].Data))
            using (BinaryReader reader = new BinaryReader(stream)) {
                TexSize = reader.ReadUInt32();
                TexHash = reader.ReadUInt32();

                reader.BaseStream.Seek(4, SeekOrigin.Current); // Skip Padding

                TexFlags = reader.ReadUInt32();
                TexWidth = reader.ReadUInt16();
                TexHeight = reader.ReadUInt16();
                TexDepth = reader.ReadUInt16();
                TexArrayCount = reader.ReadByte();
                RawTexMipmapLevel = reader.ReadByte();

                reader.BaseStream.Seek(4, SeekOrigin.Current); // Skip Padding
            }

            using (MemoryStream stream = new MemoryStream(Children[1].Data))
            using (BinaryReader reader = new BinaryReader(stream)) {
                reader.BaseStream.Seek(0x54, SeekOrigin.Current);
                Compression = reader.ReadUInt32();
            }
        }

        public void Write() {
            using (MemoryStream stream = new MemoryStream(Children[0].Data))
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                writer.Write(TexSize);
                writer.Write(TexHash);

                writer.BaseStream.Seek(4, SeekOrigin.Current); // Skip Padding

                writer.Write(TexFlags);
                writer.Write(TexWidth);
                writer.Write(TexHeight);
                writer.Write(TexDepth);
                writer.Write(TexArrayCount);
                writer.Write(RawTexMipmapLevel);

                writer.BaseStream.Seek(4, SeekOrigin.Current); // Skip Padding
            }
        }

        public string GetCompression() {
            switch (Compression) {
                case Helper.DXT1Identifier:
                    return "DXT1";
                case Helper.DXT5Identifier:
                    return "DXT5";
            }

            return "?";
        }

        public BitmapImage? MakeBitmap() {
            try {
                BitmapImage bitmap = new BitmapImage();

                using (MemoryStream stream = new MemoryStream(Children[1].Data)) {
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }

                return bitmap;
            }
            catch (Exception ex) {
                return null;
            }
        }
    }
}