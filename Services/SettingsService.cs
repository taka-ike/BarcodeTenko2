using System;
using System.Diagnostics;
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

        // 現在選択ロケーションを取得/保存する。設定変更時は即時保存する。
        public string Location
        {
            get => _settings.Location;
            set
            {
                _settings.Location = value;
                Save();
            }
        }

        // UI で選択可能なロケーション一覧を返す。
        public List<string> Locations => _locations;

        // settings.json を読み込んで設定を復元する。
        public void Load()
        {
            if (File.Exists(_settingsPath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SettingsService] Load failed: {ex.Message}");
                    _settings = new AppSettings();
                }
            }
        }

        // 現在設定を settings.json へ保存する。
        public void Save()
        {
            string json = JsonSerializer.Serialize(_settings);
            File.WriteAllText(_settingsPath, json);
        }

        // locations.json を読み込み、存在しない場合は既定値を書き出す。
        private void LoadLocations()
        {
            if (File.Exists(_locationsPath))
            {
                try
                {
                    string json = File.ReadAllText(_locationsPath);
                    _locations = JsonSerializer.Deserialize<List<string>>(json) ?? _locations;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SettingsService] LoadLocations failed: {ex.Message}");
                }
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
