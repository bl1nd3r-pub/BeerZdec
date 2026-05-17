using BeerZdec.Models;
using BeerZdec.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace BeerZdec.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;
        public INavigationService NavigationService { get; }
        public MainWindowViewModel(INavigationService navigation)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));

            ShowWelcomeCommand = new RelayCommand(
                () => _navigation.NavigateTo<WelcomeViewModel>());


            // Сразу при запуске на нужную стартовую
            _navigation.NavigateTo<WelcomeViewModel>();
        }
        public ICommand ShowWelcomeCommand { get; }
    }
}
