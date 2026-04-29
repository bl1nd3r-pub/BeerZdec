using BeerZdec.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BeerZdec.ViewModels
{
    public class MainWindowViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigation;
        public INavigationService NavigationService { get; }
        public MainWindowViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            ShowWelcomeCommand = new RelayCommand(
            () => _navigation.NavigateTo<WelcomeViewModel>());
            ShowStorageCommand = new RelayCommand(
            () => _navigation.NavigateTo<StorageViewModel>());
            _navigation.NavigateTo<WelcomeViewModel>();
        }
        public ICommand ShowWelcomeCommand { get; }
        public ICommand ShowStorageCommand { get; }
    }
}
