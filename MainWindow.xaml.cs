using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Tenko.Native.Services;
using Tenko.Native.ViewModels;

namespace Tenko.Native
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private DispatcherTimer? _clockTimer;

        public MainWindow()
        {
            InitializeComponent();

            var settingsService = new SettingsService();
            var historyService = new HistoryService();
            var scanFileService = new ScanFileService();
            var notificationService = new NotificationService();

            _viewModel = new MainViewModel(
                settingsService,
                historyService,
                scanFileService,
                notificationService
            );

            this.DataContext = _viewModel;

            // 初期フォーカス
            this.Loaded += (s, e) => ManualInputBox.Focus();

            // Set up live clock for bottom-left
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += ClockTimer_Tick;
            _clockTimer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_clockTimer != null)
            {
                _clockTimer.Tick -= ClockTimer_Tick;
                _clockTimer.Stop();
                _clockTimer = null;
            }

            base.OnClosed(e);
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            CurrentTimeText.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        private void ManualInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.SubmitCommand.Execute(null);
                ManualInputBox.Focus();
                ManualInputBox.SelectAll();
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsModal.Visibility = Visibility.Visible;
        }

        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsModal.Visibility = Visibility.Collapsed;
            ManualInputBox.Focus();
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            string newName = RenameTextBox.Text;
            if (!string.IsNullOrWhiteSpace(newName))
            {
                _viewModel.RenameBinCommand.Execute(newName);
                ManualInputBox.Focus();
            }
        }

        private void DismissWarning_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShowBinWarning = false;
            ManualInputBox.Focus();
        }
    }
}
