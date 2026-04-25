using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tenko.Native.Services
{
    public class ScanFileService
    {
        private string GetFilePath(string location)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "scans", $"ids_{location}.bin");
        }

        public bool Exists(string location)
        {
            if (string.IsNullOrEmpty(location)) return false;
            string path = GetFilePath(location);
            return File.Exists(path) && new FileInfo(path).Length > 0;
        }

        public void AppendLast5(string location, ushort last5)
        {
            if (string.IsNullOrEmpty(location)) return;
            string path = GetFilePath(location);
            using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(last5);
            }
        }

        public void RemoveLast5(string location, ushort last5)
        {
            if (string.IsNullOrEmpty(location)) return;
            string path = GetFilePath(location);
            if (!File.Exists(path)) return;

            byte[] allBytes = File.ReadAllBytes(path);
            if (allBytes.Length % 2 != 0)
            {
                // Handle abnormal binary length
                // For now, we'll just log or ignore
                return;
            }

            List<ushort> values = new List<ushort>();
            for (int i = 0; i < allBytes.Length; i += 2)
            {
                values.Add(BitConverter.ToUInt16(allBytes, i));
            }

            // Find first occurrence from end (as per "last match")
            int index = values.LastIndexOf(last5);
            if (index >= 0)
            {
                values.RemoveAt(index);
                File.WriteAllBytes(path, values.SelectMany(v => BitConverter.GetBytes(v)).ToArray());
            }
        }

        public void DeleteBin(string location)
        {
            if (string.IsNullOrEmpty(location)) return;
            string path = GetFilePath(location);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public void DeleteAllBins()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string scansDir = Path.Combine(baseDir, "scans");
            if (Directory.Exists(scansDir))
            {
                foreach (var file in Directory.GetFiles(scansDir, "ids_*.bin"))
                {
                    File.Delete(file);
                }
            }
        }

        public void RenameBin(string oldLocation, string newFileName)
        {
            // Note: newFileName is expected to be just the location-like part or the full ids_...bin?
            // gemini.md says: "input dialog accepts new name (ids_ and .bin are auto-added)"
            string oldPath = GetFilePath(oldLocation);
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string newPath = Path.Combine(baseDir, "scans", $"ids_{newFileName}.bin");

            if (File.Exists(oldPath))
            {
                if (File.Exists(newPath))
                {
                    throw new IOException("Target file already exists.");
                }
                File.Move(oldPath, newPath);
            }
        }
        
        public byte[] GetBinContent(string location)
        {
            string path = GetFilePath(location);
            return File.Exists(path) ? File.ReadAllBytes(path) : Array.Empty<byte>();
        }
    }
}
