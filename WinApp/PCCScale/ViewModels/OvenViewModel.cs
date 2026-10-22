using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PCCScale.Models;
using PCCScale.Services;
using PCCScale.ViewModels.Factories;
using System.Collections.ObjectModel;

namespace PCCScale.ViewModels
{
    public partial class OvenViewModel : ObservableObject
    {
        [ObservableProperty]
        private double _currentTemp;

        public int Id { get; }
        public ObservableCollection<PCM> PCMs { get; }
        public PCM PCM { get; set; }
        public ObservableCollection<PanViewModel> Pans { get; }

        public OvenViewModel(int id, IServiceProvider services)
        {
            var pcmCatalog = services.GetRequiredService<PCMCatalog>();
            var panViewModelFactory = services.GetRequiredService<PanViewModelFactory>();
            var _applicationState = services.GetRequiredService<ApplicationStateService>().GetState();

            Id = id;
            PCMs = new ObservableCollection<PCM>(pcmCatalog.PCMs);
            PCM = PCMs.FirstOrDefault() ?? throw new ArgumentException("No PCM available", nameof(PCMs));

            Pans = new ObservableCollection<PanViewModel>();
            foreach (var panState in _applicationState.Ovens.FirstOrDefault(o => o.Id == id)?.Pans ?? new List<Services.AppStateService.PanStateData>())
            {
                var panViewModel = panViewModelFactory.Create(panState.Id, panState, this);
                Pans.Add(panViewModel);
            }
        }

    }
}
