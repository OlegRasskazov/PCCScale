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

        public MainViewModel(ApplicationStateService applicationStateService, BLEService bleService, OvenViewModelFactory ovenViewModelFactory)
        {
            bleService.ConnectionStateChanged += ConnectionStateChangedHandler;
            var _applicationState = applicationStateService.GetState();
            
            Ovens = new ObservableCollection<OvenViewModel>();

            foreach (var ovenState in _applicationState.Ovens)
            {
                var ovenViewModel = ovenViewModelFactory.Create(ovenState.Id);
                Ovens.Add(ovenViewModel);
            }
            bleService.Connect();
        }
        private void ConnectionStateChangedHandler(BleConnectionStateEnum state) => ConnectionState = state;
    }
}
