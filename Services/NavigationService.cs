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
            // 1. Получаем ViewModel из контейнера DI
            var vm = _serviceProvider.GetRequiredService<TViewModel>();
            // 2. Если ViewModel поддерживает прием параметров (опционально)
            if (vm is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(parameter);
            }
            // 3. Обновляем CurrentViewModel. ContentControl подхватит изменение.
            CurrentViewModel = vm;
        }
    }
}
