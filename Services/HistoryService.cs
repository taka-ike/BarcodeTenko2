using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Tenko.Native.Models;

namespace Tenko.Native.Services
{
    public class HistoryService
    {
        private readonly string _historyPath;

        public HistoryService()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _historyPath = Path.Combine(baseDir, "data", "history.json");
        }

        public List<ScanRecord> LoadHistory()
        {
            if (File.Exists(_historyPath))
            {
                try
                {
                    string json = File.ReadAllText(_historyPath);
                    return JsonSerializer.Deserialize<List<ScanRecord>>(json) ?? new List<ScanRecord>();
                }
                catch
                {
                    return new List<ScanRecord>();
                }
            }
            return new List<ScanRecord>();
        }

        public void SaveHistory(List<ScanRecord> history)
        {
            string json = JsonSerializer.Serialize(history);
            File.WriteAllText(_historyPath, json);
        }
    }
}
