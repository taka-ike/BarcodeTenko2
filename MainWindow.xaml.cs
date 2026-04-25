using System.Windows;
using System.Windows.Input;
using Tenko.Native.Services;
using Tenko.Native.ViewModels;

namespace Tenko.Native
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

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
