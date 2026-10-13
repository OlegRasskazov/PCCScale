using Microsoft.Extensions.DependencyInjection;
using PCCScale.Models;
using PCCScale.Services;
using PCCScale.ViewModels.Factories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PCCScale.ViewModels
{
    public class OvenViewModel : INotifyPropertyChanged
    {
        private double _currentTemp;


        public int Number { get; }
        public ObservableCollection<PCM> PCMs { get; }
        public PCM PCM { get; set; }
        public ObservableCollection<PanViewModel> Pans { get; }
        public double CurrentTemp { 
            get => _currentTemp; 
            set {
                if (_currentTemp == value)
                    return;
                _currentTemp = value;
                OnPropertyChanged();

            }
        }

        public OvenViewModel(int number, IServiceProvider services)
        {
            var pcmCatalog = services.GetRequiredService<PCMCatalog>();
            var panViewModelFactory = services.GetRequiredService<PanViewModelFactory>();
            Pans = new ObservableCollection<PanViewModel>()
            {
                panViewModelFactory.Create(1, this),
                panViewModelFactory.Create(2, this),
                panViewModelFactory.Create(3, this),
                panViewModelFactory.Create(4, this),
                panViewModelFactory.Create(5, this),
                panViewModelFactory.Create(6, this)
            };
            Number = number;
            PCMs = new ObservableCollection<PCM>(pcmCatalog.PCMs);
            PCM = PCMs.FirstOrDefault() ?? throw new ArgumentException("No PCM available", nameof(PCMs));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
