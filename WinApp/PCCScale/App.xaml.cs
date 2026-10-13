using Microsoft.Extensions.DependencyInjection;
using PCCScale.Services;
using PCCScale.ViewModels;
using PCCScale.ViewModels.Factories;
using System.Configuration;
using System.Data;
using System.Windows;

namespace PCCScale
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider Services { get; }

        public App()
        {
            var services = new ServiceCollection();

            services.AddSingleton<PanViewModelFactory>();
            services.AddSingleton<OvenViewModelFactory>();

            services.AddSingleton<PanSettingsDialogService>();

            services.AddSingleton<BLEService, BLEService>();
            services.AddSingleton<PCMCatalog, PCMCatalog>();
            services.AddSingleton<PCCCatalog, PCCCatalog>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();

            Services = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = Services.GetRequiredService<MainWindow>();
            window.Show();
        }
    }

}
