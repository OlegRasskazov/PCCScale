using Microsoft.Extensions.DependencyInjection;

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
            OvenViewModel oven)
        {
            return ActivatorUtilities.CreateInstance<PanViewModel>(
                _services,
                number,
                oven);
        }
    }
}
