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

            // - Таблица - справочник элеваторов - ячеек +
            // - Таблица по посеву семян на участки +
            // - Таблица сбора урожая с участков
            // - Таблица партий зерна
            // - Таблица перемещений партий меж ячеек-элеваторов

            // И просто в бд, без отдельных вкладок надо добавить данные для:

            // - Статусы партий зерна(считай, что enum, но только в БД.Они как и юзер-роли будут оч редко меняться)
            // - Качества партий зерна(то же самое)

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString),
                ServiceLifetime.Scoped);

            services.AddScoped(typeof(IRepository<>), typeof(DbRepository<>));
            services.AddScoped<IRepository<UserRole>, DbRepository<UserRole>>();

            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IHashingService, HashingService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IAppInfoService, AppInfoService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IPermissionService, PermissionService>();

            services.AddTransient<AboutViewModel>();
            services.AddTransient<AdminViewModel>();
            services.AddTransient<WelcomeViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<AgronomyViewModel>();
            services.AddTransient<SoilViewModel>();
            services.AddTransient<SoilTextureViewModel>();
            services.AddTransient<VarietyViewModel>();
            services.AddTransient<GrainViewModel>();
            services.AddTransient<SowingPlotViewModel>();
            services.AddTransient<SowingProcessViewModel>();
            services.AddTransient<StorageCellViewModel>();
            services.AddTransient<HarvestEventViewModel>();
            services.AddTransient<GrainBatchViewModel>();
            services.AddTransient<StorageMoveViewModel>();
            services.AddTransient<MaltingViewModel>();

            services.AddTransient<MainWindowViewModel>();
            services.AddSingleton<MainWindow>(sp =>
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
