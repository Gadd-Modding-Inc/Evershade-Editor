using System.IO;
using System.Text;
using System.Data;

namespace EvershadeEditor.LM2 {
    public class LM2File {
        private Dictionary Dictionary = new();
        public List<ChunkEntry> Chunks = new();

        public List<ChunkEntry> Files => Chunks.Where(x => x is ChunkFileEntry).ToList();
        public List<ChunkEntry> TextureFiles => Files.Where(x => ((ChunkFileEntry)x).DataChunk is TextureChunk).ToList();

        public uint FileSize { get; private set; }
        public string DictPath { get; private set; }
        public string DataPath { get; private set; }

        public LM2File(string filePath) {
            DictPath = Path.ChangeExtension(filePath, ".dict");
            DataPath = Path.ChangeExtension(filePath, ".data");

            if (!File.Exists(DictPath)) { throw new Exception($"The dictionary file {Path.GetFileName(DictPath)} was not found"); }
            if (!File.Exists(DataPath)) { throw new Exception($"The data file {Path.GetFileName(DataPath)} was not found"); }

            Load();
        }

        public void Load() {
            Dictionary.Load(DictPath);
            LoadData();

            Console.WriteLine($"""
                [APP] Loaded Files:
                  Dict: {Path.GetFileName(DictPath)} ({Dictionary.FileSize.FormatBytes()})
                  Data: {Path.GetFileName(DataPath)} ({FileSize.FormatBytes()})
                """);

            foreach (ChunkEntry chunk in Chunks) {
                uint chunkMax = chunk.Offset + chunk.Size;
                uint blockMax = GetChunkBlock(chunk.BlockIndex).DecompressedSize;

                if (chunkMax > blockMax) {
                    Console.WriteLine($"[OUT] INDEX: {chunk.Index} RANGE: {chunkMax} > {blockMax}");
                }
            }
        }

        public void Save(string? filePath = null) {
            if (filePath != null) {
                DictPath = Path.ChangeExtension(filePath, ".dict");
                DataPath = Path.ChangeExtension(filePath, ".data");
            }
            
            SaveData(DataPath);
            Dictionary.Save(DictPath);

            Console.WriteLine($"""
                [APP] Saved Files:
                  Dict: {Path.GetFileName(DictPath)} ({Dictionary.FileSize.FormatBytes()})
                  Data: {Path.GetFileName(DataPath)} ({FileSize.FormatBytes()})
                """);
        }

        private void LoadData() {
            using (FileStream stream = new FileStream(DataPath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(stream)) {
                FileSize = (uint)reader.BaseStream.Length;
                if (FileSize < Helper.ChunkSize) { throw new Exception("Data file size is smaller size than the minimum"); }

                // Load Table
                ChunkFileEntry? file = null;
                for (uint i = 0; i < Dictionary.FileTableCount; i += Dictionary.FileTableRefCount) {
                    for (uint j = 0; j < Dictionary.FileTableInfos[i].FileCount; j++) {
                        ChunkEntry chunk = LoadChunk(reader, j, file);

                        file = null;
                        if (chunk is ChunkFileEntry) {
                            file = (ChunkFileEntry)chunk;
                        }

                        Chunks.Add(chunk);
                    }
                }
            }
        }

        private ChunkEntry LoadChunk(BinaryReader reader, uint fileIndex, ChunkFileEntry? file = null) {
            ushort type = reader.ReadUInt16();
            ChunkEntry chunk;

            switch (type) {
                case (ushort)ChunkType.FileHeader:
                    chunk = new ChunkFileEntry();
                    break;
                case (ushort)ChunkType.Texture:
                    chunk = new TextureChunk();
                    break;
                default:
                    chunk = new ChunkEntry();
                    break;
            }

            chunk.Type = type;
            chunk.Flags = reader.ReadUInt16();
            chunk.Size = reader.ReadUInt32();
            chunk.Offset = reader.ReadUInt32();
            chunk.Index = fileIndex;
            
            if (chunk.HasChildren) {
                chunk.Children = new ChunkEntry[chunk.Size];

                reader.TemporarySeek(chunk.Offset * Helper.ChunkSize, SeekOrigin.Begin, act => {
                    for (uint i = 0; i < chunk.Size; i++) {
                        chunk.Children[i] = LoadChunk(reader, chunk.Offset + i);
                    }
                });
            } else {
                ParseChunkData(reader, chunk);
            }

            if (chunk is ChunkFileEntry) {
                ((ChunkFileEntry)chunk).Read();
            } else if (chunk is TextureChunk) {
                ((TextureChunk)chunk).Read();
            }

            if (file != null) {
                file.DataChunk = chunk;
            }

            return chunk;
        }
        private void ParseChunkData(BinaryReader reader, ChunkEntry chunk) {
            DataBlock targetBlock = GetChunkBlock(chunk.BlockIndex);
            long offset = targetBlock.Offset + chunk.Offset;

            reader.TemporarySeek(offset, SeekOrigin.Begin, act => {
                chunk.Data = reader.ReadBytes((int)chunk.Size);
            });
        }

        private void SaveData(string path) {
            using (MemoryStream stream = new MemoryStream(new byte[FileSize]))
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                foreach (ChunkEntry chunk in Chunks) {
                    WriteChunk(writer, chunk);
                }

                File.WriteAllBytes(path, stream.ToArray());
            }
        }

        private void WriteChunkHeaders(BinaryWriter writer, ChunkEntry chunk) {
            writer.Write(chunk.Type);
            writer.Write(chunk.Flags);
            writer.Write(chunk.Size);
            writer.Write(chunk.Offset);

            if (chunk.HasChildren) {
                writer.TemporarySeek(chunk.Offset * Helper.ChunkSize, SeekOrigin.Begin, act => {
                    foreach (ChunkEntry childChunk in chunk.Children) {
                        WriteChunkHeaders(writer, childChunk);
                    }
                });
            }
        }

        private void WriteChunk(BinaryWriter writer, ChunkEntry chunk) {
            writer.Write(chunk.Type);
            writer.Write(chunk.Flags);
            writer.Write(chunk.Size);
            writer.Write(chunk.Offset);
            
            if (chunk.HasChildren) {
                writer.TemporarySeek(chunk.Offset * Helper.ChunkSize, SeekOrigin.Begin, act => {
                    foreach (ChunkEntry childChunk in chunk.Children) {
                        WriteChunk(writer, childChunk);
                    }
                });
            } else {
                DataBlock targetBlock = GetChunkBlock(chunk.BlockIndex);
                long offset = targetBlock.Offset + chunk.Offset;

                writer.TemporarySeek(offset, SeekOrigin.Begin, act => {
                    writer.Write(chunk.Data);
                    writer.AlignTo(chunk.Alignment);
                });
            }
        }

        private void SetChunkData(ChunkEntry chunk, byte[] data, int parentIndex, bool aligned = false) {
            if (chunk.HasChildren == true) { throw new Exception("Cannot set data of file chunk (Has Children)"); }
            if ((data.Length % 16 != 0) && aligned) { throw new Exception("Data is not 16-byte aligned"); }
            
            int sizeOffset = data.Length - chunk.Data.Length;
            chunk.Data = data;

            if (sizeOffset == 0) {
                return;
            }

            chunk.Size = (uint)data.Length;
            FileSize = (uint)(FileSize + sizeOffset);

            byte blockIndex = GetChunkBlockIndex(chunk.BlockIndex);
            uint endBlockPos = Dictionary.Blocks[blockIndex].Offset + Dictionary.Blocks[blockIndex].DecompressedSize;

            Dictionary.Blocks[blockIndex].DecompressedSize = (uint)(Dictionary.Blocks[blockIndex].DecompressedSize + sizeOffset);

            for (int i = parentIndex + 1; i < Chunks.Count; i++) {
                OffsetChunk(Chunks[i], sizeOffset, blockIndex);
            }

            foreach (DataBlock block in Dictionary.Blocks) {
                if (block.Offset >= endBlockPos) {
                    block.Offset = (uint)((block.Offset + sizeOffset + 511) & ~511);
                }
                
                block.Offset = Math.Clamp(block.Offset, 0, FileSize);
                block.DecompressedSize = Math.Clamp(block.DecompressedSize, 0, FileSize - block.Offset);
            }
        }

        private void OffsetChunk(ChunkEntry chunk, int offset, int blockIndex) {
            if (chunk.HasChildren) {
                foreach (ChunkEntry childChunk in chunk.Children) {
                    OffsetChunk(childChunk, offset, blockIndex);
                }
            } else {
                if (GetChunkBlockIndex(chunk.BlockIndex) == blockIndex) {
                    chunk.Offset = (uint)(chunk.Offset + offset);
                }
            }
        }

        public void SetTexture(TextureChunk chunk, byte[] data) {
            ushort width;
            ushort height;
            int mipCount;

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream)) {
                if (chunk.Children[1].HasChildren == true) { throw new Exception("Cannot set data of file chunk (Has Children)"); }
                if (data.Length % 16 != 0) { throw new Exception("Data is not 16-byte aligned"); }

                if (reader.ReadUInt32() != Helper.DDSIdentifier) {
                    throw new Exception("This is not a DDS image (Invalid DDS identifier)");
                }

                reader.BaseStream.Seek(12, SeekOrigin.Begin);
                height = (ushort)reader.ReadUInt32();
                width = (ushort)reader.ReadUInt32();

                if (!width.IsPowerOfTwo()) {
                    throw new Exception("Width is not a power of 2");
                }

                if (!height.IsPowerOfTwo()) {
                    throw new Exception("Height is not a power of 2");
                }

                if (width < Helper.MinDimension ||
                    width > Helper.MaxDimension) {
                    throw new Exception($"Image is out of width limits ({Helper.MinDimension} - {Helper.MaxDimension})");
                }

                if (height < Helper.MinDimension ||
                    height > Helper.MaxDimension) {
                    throw new Exception($"Image is out of height limits ({Helper.MinDimension} - {Helper.MaxDimension})");
                }

                reader.BaseStream.Seek(0x1C, SeekOrigin.Begin);
                mipCount = reader.ReadInt32();

                reader.BaseStream.Seek(0x44, SeekOrigin.Begin);
                uint identifier = reader.ReadUInt32();
                if (identifier != Helper.NVT3Identifier && identifier != Helper.NVTTIdentifier) {
                    throw new Exception("Image does not use NVTT or NVT3");
                }

                reader.BaseStream.Seek(0x54, SeekOrigin.Begin);
                uint compression = reader.ReadUInt32();

                if (mipCount < 1 || mipCount > 9) {
                    throw new Exception("Mipmap count out of range (1 to 9)");
                }

                switch (compression) {
                    case Helper.DXT1Identifier:
                        if (reader.BaseStream.Length < Helper.DXT1MinSize ||
                            reader.BaseStream.Length > Helper.DXT1MaxSize) {
                            throw new Exception("Image is out of DXT1 size limits");
                        }
                        break;
                    case Helper.DXT5Identifier:
                        if (reader.BaseStream.Length < Helper.DXT5MinSize ||
                            reader.BaseStream.Length > Helper.DXT5MaxSize) {
                            throw new Exception("Image is out of DXT5 size limits");
                        }
                        break;
                    default:
                        throw new Exception("Unsupported pixel format (Use either DXT1 or DXT5)");
                }

                chunk.Compression = compression;
            }

            data[0x47] = 0x54;

            SetChunkData(chunk.Children[1], data, (int)chunk.Index, true);

            chunk.TexSize = (uint)data.Length;
            chunk.TexWidth = width;
            chunk.TexHeight = height;
            chunk.TexMipmapLevel = (byte)mipCount;

            chunk.Write();
        }

        public DataBlock GetChunkBlock(byte blockIndex) {
            return Dictionary.Blocks[GetChunkBlockIndex(blockIndex)];
        }

        public byte GetChunkBlockIndex(byte blockIndex) {
            return Dictionary.FileTableReferences[0].BlockIndices[blockIndex];
        }

        public ChunkEntry GetDataChunk(int index) {
            return ((ChunkFileEntry)TextureFiles[index]).DataChunk;
        }
    }

    public class Dictionary {
        public ushort Flags;
        public byte IsCompressed;
        public uint BlockCount;
        public uint LargestBlockSize;
        public byte FileTableCount;
        public byte FileTableRefCount;
        public byte FileExtensionCount;
        public FileTableReference[] FileTableReferences;
        public FileTableInfo[] FileTableInfos;
        public DataBlock[] Blocks;

        public uint FileSize { get; private set; }

        public void Load(string filePath) {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(stream)) {
                FileSize = (uint)reader.BaseStream.Length;
                if (FileSize <= Helper.MinDictionarySize) { throw new Exception("Dictionary file size is smaller size than the minimum"); }

                if (reader.ReadUInt32() != Helper.Identifier) { throw new Exception("File is not a Dictionary file"); }

                // Load Properties
                Flags = reader.ReadUInt16();
                IsCompressed = reader.ReadByte();
                reader.BaseStream.Seek(1, SeekOrigin.Current); // Skip Padding
                BlockCount = reader.ReadUInt32();
                LargestBlockSize = reader.ReadUInt32();
                FileTableCount = reader.ReadByte();
                reader.BaseStream.Seek(1, SeekOrigin.Current); // Skip Padding
                FileTableRefCount = reader.ReadByte();
                FileExtensionCount = reader.ReadByte();

                FileTableReferences = new FileTableReference[FileTableRefCount];
                FileTableInfos = new FileTableInfo[FileTableRefCount * FileTableCount];
                Blocks = new DataBlock[BlockCount];

                // Load File Table References
                for (int i = 0; i < FileTableRefCount; i++) {
                    FileTableReferences[i] = new FileTableReference()
                    {
                        Hash = reader.ReadUInt32(),
                        BlockIndices = reader.ReadBytes(8),
                    };
                }

                // Load File Table Info
                for (int i = 0; i < FileTableRefCount * FileTableCount; i++) {
                    FileTableInfos[i] = new FileTableInfo()
                    {
                        FileCount = reader.ReadUInt16(),
                        BlockIndex = reader.ReadUInt16(),
                    };
                }

                // Load Blocks
                for (int i = 0; i < BlockCount; i++) {
                    Blocks[i] = new DataBlock()
                    {
                        Offset = reader.ReadUInt32(),
                        DecompressedSize = reader.ReadUInt32(),
                        CompressedSize = reader.ReadUInt32(),
                        UsageType = reader.ReadByte(),
                        Unknown = reader.ReadByte(),
                        FileExtension = reader.ReadByte(),
                        UnknownFlag = reader.ReadByte(),
                    };
                }
            }
        }

        public void Save(string filePath) {
            byte[] data = new byte[FileSize];

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                writer.Write(Helper.Identifier);
                writer.Write(Flags);
                writer.Write(IsCompressed);
                writer.Write((byte)0);
                writer.Write(BlockCount);
                writer.Write(Blocks.Max(block => block.CompressedSize));
                writer.Write(FileTableCount);
                writer.Write((byte)0);
                writer.Write(FileTableRefCount);
                writer.Write(FileExtensionCount);

                foreach (FileTableReference reference in FileTableReferences) {
                    writer.Write(reference.Hash);
                    writer.Write(reference.BlockIndices);
                }

                foreach (FileTableInfo infoPart in FileTableInfos) {
                    writer.Write(infoPart.FileCount);
                    writer.Write(infoPart.BlockIndex);
                }

                foreach (DataBlock block in Blocks) {
                    writer.Write(block.Offset);
                    writer.Write(block.DecompressedSize);
                    writer.Write(block.CompressedSize);
                    writer.Write(block.UsageType);
                    writer.Write(block.Unknown);
                    writer.Write(block.FileExtension);
                    writer.Write(block.UnknownFlag);
                }

                writer.Write(Encoding.Default.GetBytes(Helper.Terminator));
            }

            File.WriteAllBytes(filePath, data);
        }
    }

    public class FileTableReference {
        public uint Hash;
        public byte[] BlockIndices = new byte[8];
    }
    
    public class FileTableInfo {
        public ushort FileCount;
        public ushort BlockIndex;
    }
    
    public class DataBlock {
        public uint Offset;
        public uint DecompressedSize;
        public uint CompressedSize;
        public byte UsageType;
        public byte Unknown;
        public byte FileExtension;
        public byte UnknownFlag;

        public bool IsOffset = false;
    }
}