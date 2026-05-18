using BeerZdec.Services;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BeerZdec.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        private string _login;
        public string Login
        {
            get => _login;
            set => Set(ref _login, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => Set(ref _password, value);
        }

        public LoginViewModel(
            IAuthService authService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            LoginCommand = new RelayCommandAsync(OnLoginAsync);
            NavigateToRegisterCommand = new RelayCommand(NavigateToRegister);
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        private async Task OnLoginAsync()
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(Login))
            {
                _dialogService.ShowWarning("Введите логин");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                _dialogService.ShowWarning("Введите пароль");
                return;
            }

            // Попытка входа
            var result = await _authService.LoginAsync(Login, Password);

            switch (result)
            {
                case LoginResult.Success:
                    _dialogService.ShowInfo($"Добро пожаловать, {Login}!");
                    _navigationService.ClearAndNavigateTo<WelcomeViewModel>();
                    break;

                case LoginResult.UserNotFound:
                    _dialogService.ShowError($"Пользователь \"{Login}\" не найден");
                    break;

                case LoginResult.InvalidPassword:
                    _dialogService.ShowError("Неверный пароль");
                    break;
            }
        }

        private void NavigateToRegister()
        {
            _navigationService.NavigateTo<RegisterViewModel>();
        }
    }
}