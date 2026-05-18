using BeerZdec.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BeerZdec.ViewModels
{
    public class RegisterViewModel : ObservableObject
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

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => Set(ref _confirmPassword, value);
        }

        public RegisterViewModel(
            IAuthService authService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            RegisterCommand = new RelayCommandAsync(OnRegisterAsync);
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
        }

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        private async Task OnRegisterAsync()
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Login: '{Login}'");
            System.Diagnostics.Debug.WriteLine($"🔍 Password: '{Password}' (null? {Password == null})");
            System.Diagnostics.Debug.WriteLine($"🔍 ConfirmPassword: '{ConfirmPassword}'");

            // Валидация
            if (!Validation()) return;

            // Попытка регистрации
            bool success = await _authService.RegisterAsync(Login, Password, "User");

            if (success)
            {
                _dialogService.ShowInfo("Регистрация успешна! Теперь войдите.");
                NavigateToLogin();
            }
            else
            {
                _dialogService.ShowError("Пользователь с таким логином уже существует");
            }
        }

        private bool Validation() {
            if (string.IsNullOrWhiteSpace(Login))
            {
                _dialogService.ShowWarning("Введите логин");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                _dialogService.ShowWarning("Введите пароль");
                return false;
            }

            if (Password != ConfirmPassword)
            {
                _dialogService.ShowError("Пароли не совпадают!");
                return false;
            }

            if (Password.Length < 4)
            {
                _dialogService.ShowWarning("Пароль должен быть не менее 4 символов");
                return false;
            }
            return true;
        }

        private void NavigateToLogin()
        {
            _navigationService.NavigateTo<LoginViewModel>();
        }
    }
}
