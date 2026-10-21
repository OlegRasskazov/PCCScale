using PCCScale.ViewModels;
using PCCScale.Views;
using System.Windows;

namespace PCCScale.Services
{
    public class PanSettingsDialogService
    {
        public void ShowSettings(PanViewModel pan)
        {
            var viewModel = new PanSettingsViewModel(pan);

            var window = new PanSettingsWindow
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };

            window.ShowDialog();
        }
    }
}
