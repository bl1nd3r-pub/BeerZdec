using BeerZdec.Models;
using BeerZdec.Repositories;
using BeerZdec.Interfaces;
using BeerZdec.Services;
using BeerZdec.ViewModels;
using BeerZdec.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace BeerZdec
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("HomeConnection");

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped(typeof(IRepository<>), typeof(DbRepository<>));

            services.AddSingleton<IDialogService, DialogService>(); // Singleton: один экземпляр на всё приложение
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddTransient<WelcomeViewModel>(); // Transient: новый экземпляр при каждом запросе

            services.AddTransient<MainWindowViewModel>();
            services.AddSingleton<MainWindow>(sp => // Singleton + явная настройка DataContext
            {
                var window = new MainWindow();
                window.DataContext = sp.GetRequiredService<MainWindowViewModel>();
                return window;
            });

            var serviceProvider = services.BuildServiceProvider();

            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
