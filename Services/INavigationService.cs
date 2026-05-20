using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerZdec.Services
{
    public interface INavigationService
    {
        object? CurrentViewModel { get; }

        // Базовая навигация
        void NavigateTo<TViewModel>(object? parameter = null)
        where TViewModel : class;

        // Навигация с очисткой истории (для входа/выхода)
        void ClearAndNavigateTo<TViewModel>(object? parameter = null) where TViewModel : class;
    }
    public interface INavigationAware
    {
        void OnNavigatedTo(object? parameter);
        void OnNavigatedFrom();
    }
}
