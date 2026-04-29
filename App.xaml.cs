using BeerZdec.Services;
using BeerZdec.ViewModels;
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

            // 1. Созданиек коллекции для регистрации сервисов
            var services = new ServiceCollection(); // Великий Майковский сервис колекшн

            // 2. Регистрация сервисов сервисы
            services.AddSingleton<IDialogService, DialogService>(); // Singleton: один экземпляр на всё приложение
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddTransient<WelcomeViewModel>(); // Transient: новый экземпляр при каждом запросе
            services.AddSingleton<MainWindow>(sp => // Singleton + явная настройка DataContext
            {
                var window = new MainWindow();
                window.DataContext = sp.GetRequiredService<WelcomeViewModel>();
                return window;
            });

            // 3. Контейнер (ServiceProvider)
            var serviceProvider = services.BuildServiceProvider();

            // 4. Получение и отображение главного окна
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
