using BeerZdec.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerZdec.Services
{
    public class NavigationService : ObservableObject, INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private object? _currentViewModel;
        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public object? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                OnPropertyChanged();

            }
        }
        public void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : class //можно наложить более точное ограничение
        {
            // 1. Уведомляем текущую ViewModel, что уходим с неё (опционально, но полезно)
            if (_currentViewModel is INavigationAware aware)
            {
                aware.OnNavigatedFrom();
            }

            // 2. Получаем новую ViewModel из контейнера DI
            var vm = _serviceProvider.GetRequiredService<TViewModel>();

            // 3. Если новая ViewModel поддерживает приём параметров
            if (vm is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(parameter);
            }

            // 4. Обновляем CurrentViewModel. ContentControl подхватит изменение.
            CurrentViewModel = vm;
        }
        public void ClearAndNavigateTo<TViewModel>(object? parameter = null) where TViewModel : class
        {
            // Сейчас это аналог NavigateTo, но с семантикой "сбросить историю"
            // В будущем здесь можно очистить стек навигации, если добавишь back/forward

            // Для надёжности тоже вызываем OnNavigatedFrom, если нужно
            if (_currentViewModel is INavigationAware aware)
            {
                aware.OnNavigatedFrom();
            }

            var vm = _serviceProvider.GetRequiredService<TViewModel>();
            if (vm is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(parameter);
            }
            CurrentViewModel = vm;
        }
    }
}
