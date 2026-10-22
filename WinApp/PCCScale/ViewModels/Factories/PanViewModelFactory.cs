using Microsoft.Extensions.DependencyInjection;
using PCCScale.Services.AppStateService;

namespace PCCScale.ViewModels.Factories
{
    public class PanViewModelFactory
    {
        private readonly IServiceProvider _services;

        public PanViewModelFactory(IServiceProvider services)
        {
            _services = services;
        }

        public PanViewModel Create(
            int number,
            PanStateData panStateData,
            OvenViewModel oven)
        {
            return ActivatorUtilities.CreateInstance<PanViewModel>(
                _services,
                number,
                oven,
                panStateData);
        }
    }
}
