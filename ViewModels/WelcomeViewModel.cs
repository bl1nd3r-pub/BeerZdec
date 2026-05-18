using BeerZdec.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BeerZdec.ViewModels
{
    public class WelcomeViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        public bool IsLoggedIn => _authService.IsAuthenticated;

        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand LogoutCommand { get; }

        public string CurrentUserName => _authService.CurrentUser?.UsLogin ?? "Гость";

        public WelcomeViewModel(IAuthService authService, IDialogService dialogService, INavigationService navigationService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
            NavigateToRegisterCommand = new RelayCommand(NavigateToRegister);
            LogoutCommand = new RelayCommand(Logout);
        }

        private void NavigateToLogin()
        {
            _navigationService.NavigateTo<LoginViewModel>();
        }

        private void NavigateToRegister()
        {
            _navigationService.NavigateTo<RegisterViewModel>();
        }

        private void Logout()
        {
            _authService.Logout();
            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(CurrentUserName));
            _navigationService.ClearAndNavigateTo<WelcomeViewModel>();
        }
    }
}
