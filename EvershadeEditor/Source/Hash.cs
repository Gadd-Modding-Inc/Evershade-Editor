using System;
using System.IO;

namespace EvershadeEditor.LM2 {
    public class HashBin {
        public Dictionary<uint, string> Hashes { get; private set; } = new();
        public uint HashCount;

        public HashBin() {
            Load();
        }

        private void Load() {
            if (!Path.Exists(App.HashPath)) { throw new Exception("Hash file does not exist (Hashes.bin)"); }

            using (FileStream stream = new FileStream(App.HashPath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(stream)) {
                HashCount = reader.ReadUInt32();

                for (int i = 0; i < HashCount; i++) {
                    uint hash = reader.ReadUInt32();
                    uint offset = reader.ReadUInt32();

                    reader.TemporarySeek(4 + HashCount * 8 + offset, SeekOrigin.Begin, act => {
                        Hashes.Add(hash, reader.ReadZeroTerminatedString());
                    });
                }
            }
        }

        public string GetHashValue(uint hash, bool nameOnly = false) {
            if (Hashes.TryGetValue(hash, out string value)) {
                return (nameOnly) ? Path.GetFileName(value) : value;
            }

            return $"Unknown Hash ({hash:X8})";
        }

        public string GetPathHash(string path) {
            var hashEntry = Hashes.FirstOrDefault(x => x.Value == path);

            if (hashEntry.Equals(default(KeyValuePair<uint, string>))) {
                return $"No hash for \"{path}\" was found.";
            }

            return hashEntry.Key.ToString("X8");
        }
    }
}