using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PCCScale.Services;
using PCCScale.Services.BleService;
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

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            services.Configure<BleOptions>(
                configuration.GetSection(BleOptions.SectionName));

            services.AddSingleton<IConfiguration>(configuration);


            services.AddSingleton<PanViewModelFactory>();
            services.AddSingleton<OvenViewModelFactory>();

            services.AddSingleton<PanSettingsDialogService>();

            services.AddSingleton<ApplicationStateService>();
            services.AddSingleton<BLEService>();
            services.AddSingleton<PCMCatalog>();
            services.AddSingleton<PCCCatalog>();

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
