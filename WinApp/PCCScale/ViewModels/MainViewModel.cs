using CommunityToolkit.Mvvm.Input;
using PCCScale.Services;
using PCCScale.ViewModels.Factories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace PCCScale.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private readonly BLEService _bleService;

        public ObservableCollection<OvenViewModel> Ovens { get; }

        public ICommand ConnectCommand { get; }
        public ICommand SetThresholdCommand { get; }


        public MainViewModel(BLEService bleService, OvenViewModelFactory ovenViewModelFactory)
        {
            _bleService = bleService;
            Ovens = new ObservableCollection<OvenViewModel>
            {
                ovenViewModelFactory.Create(1),
                ovenViewModelFactory.Create(2),
                ovenViewModelFactory.Create(3),
            };
            ConnectCommand = new AsyncRelayCommand(ConnectAsync);
            SetThresholdCommand = new AsyncRelayCommand(SetThresholdAsync);
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        private async Task ConnectAsync() => await _bleService.ConnectAsync();

        private async Task SetThresholdAsync()
        {
            int thresholdValue = 1000;
            await _bleService.SetThresholdAsync(1, 1, thresholdValue);
        }
    }
}
