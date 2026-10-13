using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PCCScale.Services;
using System.Windows.Input;

namespace PCCScale.ViewModels
{
    internal class PanSettingsViewModel
    {
        public PanViewModel Pan { get; }

        //public ICommand SavePCCslabCommand => new AsyncRelayCommand(SavePCCslabAsync);

        public ICommand TareCommand => new AsyncRelayCommand(TareAsync);
        public ICommand CalibrateCommand => new AsyncRelayCommand(CalibrateAsync);

        public PanSettingsViewModel(PanViewModel pan)
        {
            Pan = pan;

        }

        //private async Task SavePCCslabAsync()
        //{

        //}

        private async Task TareAsync() => Pan.TareWeight = Pan.AbsWeight;
        private async Task CalibrateAsync() { }
    }
}
