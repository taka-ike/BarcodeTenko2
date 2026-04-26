using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Tenko.Native.Models;
using Tenko.Native.Services;

namespace Tenko.Native.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;
        private readonly HistoryService _historyService;
        private readonly ScanFileService _scanFileService;
        private readonly NotificationService _notificationService;

        private string _manualInput = string.Empty;
        private string _currentLocation = string.Empty;
        private bool _showBinWarning = false;
        private string _notificationMessage = string.Empty;
        private NotificationType _notificationType = NotificationType.Success;
        private bool _isNotificationVisible = false;

        public ObservableCollection<ScanRecord> History { get; } = new();
        public ObservableCollection<string> Locations { get; } = new();

        public MainViewModel(
            SettingsService settingsService,
            HistoryService historyService,
            ScanFileService scanFileService,
            NotificationService notificationService)
        {
            _settingsService = settingsService;
            _historyService = historyService;
            _scanFileService = scanFileService;
            _notificationService = notificationService;

            foreach (var loc in _settingsService.Locations)
            {
                Locations.Add(loc);
            }

            _currentLocation = _settingsService.Location;
            
            _notificationService.OnNotification += (s, e) => ShowNotification(e.Message, e.Type);

            LoadHistory();
            CheckBinFile();

            SubmitCommand = new RelayCommand(_ => SubmitManualInput());
            DeleteRecordCommand = new RelayCommand<ScanRecord>(record => DeleteRecord(record));
            DeleteAllCommand = new RelayCommand(_ => DeleteAll());
            ExportCsvCommand = new RelayCommand(_ => ExportCsv());
            ExportBinCommand = new RelayCommand(_ => ExportBin());
            RenameBinCommand = new RelayCommand<string>(newName => RenameBin(newName));
        }

        public string ManualInput
        {
            get => _manualInput;
            set => SetProperty(ref _manualInput, value);
        }

        public string CurrentLocation
        {
            get => _currentLocation;
            set
            {
                if (SetProperty(ref _currentLocation, value))
                {
                    _settingsService.Location = value;
                    OnPropertyChanged(nameof(IsLocationSet));
                    CheckBinFile();
                }
            }
        }

        public bool IsLocationSet => !string.IsNullOrEmpty(CurrentLocation);

        public bool ShowBinWarning
        {
            get => _showBinWarning;
            set => SetProperty(ref _showBinWarning, value);
        }

        public string NotificationMessage
        {
            get => _notificationMessage;
            set => SetProperty(ref _notificationMessage, value);
        }

        public NotificationType NotificationType
        {
            get => _notificationType;
            set => SetProperty(ref _notificationType, value);
        }

        public bool IsNotificationVisible
        {
            get => _isNotificationVisible;
            set => SetProperty(ref _isNotificationVisible, value);
        }

        public ICommand SubmitCommand { get; }
        public ICommand DeleteRecordCommand { get; }
        public ICommand DeleteAllCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand ExportBinCommand { get; }
        public ICommand RenameBinCommand { get; }

        private void LoadHistory()
        {
            var items = _historyService.LoadHistory();
            History.Clear();
            foreach (var item in items) History.Add(item);
        }

        private void CheckBinFile()
        {
            ShowBinWarning = _scanFileService.Exists(CurrentLocation);
        }

        private void SubmitManualInput()
        {
            if (string.IsNullOrWhiteSpace(ManualInput)) return;
            
            // Manual input must be numeric
            if (!ManualInput.All(char.IsDigit))
            {
                _notificationService.Error("数字のみ入力可能です。");
                return;
            }

            // Valid length 5 or 10
            if (ManualInput.Length != 5 && ManualInput.Length != 10)
            {
                _notificationService.Error("5桁または10桁の数字を入力してください。");
                return;
            }

            ProcessScan(ManualInput);
            ManualInput = string.Empty;
        }

        private void ProcessScan(string barcode)
        {
            if (string.IsNullOrEmpty(CurrentLocation))
            {
                _notificationService.Warning("スキャン場所を選択してください。");
                return;
            }

            try
            {
                ushort last5 = ushort.Parse(barcode.Length >= 5 ? barcode.Substring(barcode.Length - 5) : barcode);
                var record = new ScanRecord
                {
                    Id = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() + last5.ToString("D5"),
                    Timestamp = DateTime.Now,
                    Barcode = barcode,
                    Last5 = last5,
                    Location = CurrentLocation
                };

                History.Insert(0, record);
                _historyService.SaveHistory(History.ToList());
                _scanFileService.AppendLast5(CurrentLocation, last5);
            }
            catch (Exception ex)
            {
                _notificationService.Error($"スキャン処理失敗: {ex.Message}");
            }
        }

        private void DeleteRecord(ScanRecord? record)
        {
            if (record == null) return;

            var result = MessageBox.Show(
                $"このレコードを削除しますか？\n時刻: {record.FormattedTimestamp}\nバーコード: {record.Barcode}",
                "レコード削除の確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            History.Remove(record);
            _historyService.SaveHistory(History.ToList());
            _scanFileService.RemoveLast5(record.Location, record.Last5);
            _notificationService.Success("レコードを削除しました。");
        }

        private void DeleteAll()
        {
            var result = MessageBox.Show(
                $"現在の「{CurrentLocation}」の履歴とバイナリデータを削除しますか？\n他のデータは削除されません。",
                "履歴削除の確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            History.Clear();
            _historyService.SaveHistory(new());
            _scanFileService.DeleteBin(CurrentLocation);
            CheckBinFile();
            _notificationService.Success($"現在の「{CurrentLocation}」の履歴を削除しました。");
        }

        private void RenameBin(string? newName)
        {
            if (string.IsNullOrEmpty(newName)) return;
            
            // Sanitize
            string sanitized = new string(newName.Select(c => Path.GetInvalidFileNameChars().Contains(c) || char.IsControl(c) ? '_' : c).ToArray());

            try
            {
                _scanFileService.RenameBin(CurrentLocation, sanitized);
                
                // Clear scan history to start fresh for the current location
                History.Clear();
                _historyService.SaveHistory(new());
                
                ShowBinWarning = false;
                _notificationService.Success($"既存ファイルを ids_{CurrentLocation}_{sanitized}.bin に退避しました。");
            }
            catch (Exception ex)
            {
                _notificationService.Error($"名前変更失敗: {ex.Message}");
            }
        }

        private void ExportCsv()
        {
            if (History.Count == 0) return;
            try
            {
                string filename = $"scan_{CurrentLocation}_{DateTime.Now:yyyyMMddHHmm}.csv";
                using (var writer = new StreamWriter(filename))
                {
                    writer.WriteLine("Timestamp,ID");
                    foreach (var r in History)
                    {
                        writer.WriteLine($"{r.FormattedTimestamp},{r.Last5:D5}");
                    }
                }
                _notificationService.Success($"{filename} を出力しました。");
            }
            catch (Exception ex)
            {
                _notificationService.Error($"CSV出力失敗: {ex.Message}");
            }
        }

        private void ExportBin()
        {
            if (History.Count == 0) return;
            try
            {
                string filename = $"ids_{CurrentLocation}_{DateTime.Now:yyyyMMddHHmm}.bin";
                var data = History.SelectMany(r => BitConverter.GetBytes(r.Last5)).ToArray();
                File.WriteAllBytes(filename, data);
                _notificationService.Success($"{filename} を出力しました。");
            }
            catch (Exception ex)
            {
                _notificationService.Error($"BIN出力失敗: {ex.Message}");
            }
        }

        private System.Timers.Timer? _notificationTimer;
        private void ShowNotification(string message, NotificationType type)
        {
            NotificationMessage = message;
            NotificationType = type;
            IsNotificationVisible = true;

            _notificationTimer?.Stop();
            _notificationTimer = new System.Timers.Timer(1200);
            _notificationTimer.Elapsed += (s, e) =>
            {
                IsNotificationVisible = false;
                _notificationTimer.Stop();
            };
            _notificationTimer.Start();
        }
    }
}
