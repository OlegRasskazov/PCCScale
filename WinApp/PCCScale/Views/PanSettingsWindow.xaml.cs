using PCCScale.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace PCCScale.Views
{
    /// <summary>
    /// Interaction logic for PanSettingsWindow.xaml
    /// </summary>
    public partial class PanSettingsWindow : Window
    {
        private bool _closeAfterSave;

        public PanSettingsWindow()
        {
            InitializeComponent();
        }

        private async void Window_Closing(
            object sender,
            CancelEventArgs e)
        {
            if (_closeAfterSave)
                return;

            // Cancel closing until saving is complete
            e.Cancel = true;

            if (DataContext is not PanSettingsViewModel vm)
                return;

            try
            {
                vm.UpdateState();

                _closeAfterSave = true;
                Dispatcher.BeginInvoke(new Action(Close), DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save settings:\n{ex.Message}",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
