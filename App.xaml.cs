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
