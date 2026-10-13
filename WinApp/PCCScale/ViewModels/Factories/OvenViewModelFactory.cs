using Microsoft.Extensions.DependencyInjection;

namespace PCCScale.ViewModels.Factories
{
    public class OvenViewModelFactory
    {
        private readonly IServiceProvider _services;

        public OvenViewModelFactory(IServiceProvider services)
        {
            _services = services;
        }

        public OvenViewModel Create(
            int number)
        {
            return ActivatorUtilities.CreateInstance<OvenViewModel>(
                _services,
                number);
        }
    }
}
