using System;
using System.IO;
using System.Text.Json;

namespace Tenko.Native.Services
{
    public class AppSettings
    {
        public string Location { get; set; } = string.Empty;
    }

    public class SettingsService
    {
        private readonly string _settingsPath;
        private readonly string _locationsPath;
        private AppSettings _settings = new();
        private List<string> _locations = new() { "2棟2階", "第一体育館前", "本部横" };

        public SettingsService()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dataDir = Path.Combine(baseDir, "data");
            string scansDir = Path.Combine(baseDir, "scans");

            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            if (!Directory.Exists(scansDir)) Directory.CreateDirectory(scansDir);

            _settingsPath = Path.Combine(dataDir, "settings.json");
            _locationsPath = Path.Combine(dataDir, "locations.json");
            Load();
            LoadLocations();
        }

        public string Location
        {
            get => _settings.Location;
            set
            {
                _settings.Location = value;
                Save();
            }
        }

        public List<string> Locations => _locations;

        public void Load()
        {
            if (File.Exists(_settingsPath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch
                {
                    _settings = new AppSettings();
                }
            }
        }

        public void Save()
        {
            string json = JsonSerializer.Serialize(_settings);
            File.WriteAllText(_settingsPath, json);
        }

        private void LoadLocations()
        {
            if (File.Exists(_locationsPath))
            {
                try
                {
                    string json = File.ReadAllText(_locationsPath);
                    _locations = JsonSerializer.Deserialize<List<string>>(json) ?? _locations;
                }
                catch { }
            }
            else
            {
                // Save defaults if not exists
                string json = JsonSerializer.Serialize(_locations, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_locationsPath, json);
            }
        }
    }
}
