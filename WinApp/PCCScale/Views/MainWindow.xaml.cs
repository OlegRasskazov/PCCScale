using PCCScale.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace PCCScale
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : Window
    {
        private bool _stateSaved;
        private bool _saving;
        private readonly ApplicationStateService _stateService;
        public MainWindow()
        {

        }
        public MainWindow(MainViewModel viewModel, ApplicationStateService stateService)
        {
            InitializeComponent();
            DataContext = viewModel;
            _stateService = stateService;
        }
        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_stateSaved)
                return;

            e.Cancel = true;

            if (_saving)
                return;

            _saving = true;

            try
            {
                await _stateService.WriteStateAsync();
                _stateSaved = true;

                Dispatcher.BeginInvoke(new Action(Close), DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _saving = false;
            }
        }
    }
}