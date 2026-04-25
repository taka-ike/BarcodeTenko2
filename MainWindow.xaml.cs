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

            // Dependency Injection (Manual)
            var settingsService = new SettingsService();
            var historyService = new HistoryService();
            var scanFileService = new ScanFileService();
            var notificationService = new NotificationService();
            var keyboardCaptureService = new KeyboardCaptureService();

            _viewModel = new MainViewModel(
                settingsService,
                historyService,
                scanFileService,
                notificationService,
                keyboardCaptureService
            );

            this.DataContext = _viewModel;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // If the manual input box has focus, don't double-process 
            // unless it's Enter which we might want to handle specifically.
            if (ManualInputBox.IsFocused) return;

            // Pass key to ViewModel for background scanning
            _viewModel.OnPreviewKeyDown(e.Key.ToString());
            
            // Note: Enter key might be "Return" in e.Key.ToString()
        }

        private void ManualInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.SubmitCommand.Execute(null);
                // After submit, ensure focus stays or returns
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
            }
        }

        private void DismissWarning_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShowBinWarning = false;
            ManualInputBox.Focus();
        }
    }
}
