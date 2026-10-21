using CommunityToolkit.Mvvm.ComponentModel;
using PCCScale.Services.BleService;
using PCCScale.ViewModels.Factories;
using System.Collections.ObjectModel;

namespace PCCScale.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private BleConnectionStateEnum _connectionState = BleConnectionStateEnum.Disconnected;
        public ObservableCollection<OvenViewModel> Ovens { get; }

        public MainViewModel(BLEService bleService, OvenViewModelFactory ovenViewModelFactory)
        {
            bleService.ConnectionStateChanged += ConnectionStateChangedHandler;
            Ovens = new ObservableCollection<OvenViewModel>
            {
                ovenViewModelFactory.Create(1),
                ovenViewModelFactory.Create(2),
                ovenViewModelFactory.Create(3),
            };
            bleService.Connect();
        }
        private void ConnectionStateChangedHandler(BleConnectionStateEnum state) => ConnectionState = state;
    }
}
