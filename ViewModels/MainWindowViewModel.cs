using BeerZdec.Models;
using BeerZdec.Services;
using System;
using System.Windows.Input;

namespace BeerZdec.ViewModels
{
    public class MainWindowViewModel : ObservableObject, IDisposable
    {
        private readonly INavigationService _navigation;
        private readonly IAuthService _authService;

        public MainWindowViewModel(
            INavigationService navigation,
            IAuthService authService)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            // Подписываемся на изменения авторизации
            _authService.AuthStateChanged += OnAuthStateChanged;

            // Инициализируем команды (inline, как ты просил)
            NavigateToWelcomeCommand = new RelayCommand(
                () => _navigation.NavigateTo<WelcomeViewModel>());
            NavigateToAboutCommand = new RelayCommand(
                () => _navigation.NavigateTo<AboutViewModel>());
            NavigateToAdminCommand = new RelayCommand(
                () => _navigation.NavigateTo<AdminViewModel>());

            // Сразу при запуске на нужную стартовую
            _navigation.NavigateTo<WelcomeViewModel>();
        }

        // Публичные свойства для биндинга
        public INavigationService NavigationService => _navigation;
        public ICommand NavigateToWelcomeCommand { get; }
        public ICommand NavigateToAboutCommand { get; }
        public ICommand NavigateToAdminCommand { get; }

        // === Данные пользователя ===
        public string CurrentUserName => _authService.CurrentUser?.UsLogin ?? "Гость";
        public string CurrentUserRole => _authService.CurrentUser?.RoleNavigation?.RoleName ?? "User";

        // === Проверка прав доступа (простая строковая логика) ===
        // Админ видит всё, остальные — только свои модули
        public bool CanSeeAdminPanel => CurrentUserRole == "Admin";
        public bool CanSeeAgronomy => CurrentUserRole is "Admin" or "Agronomist";
        public bool CanSeeBrewing => CurrentUserRole is "Admin" or "Brewer";
        public bool CanSeeSales => CurrentUserRole is "Admin" or "SalesManager";

        // === Обработчик изменения авторизации ===
        private void OnAuthStateChanged(object? sender, EventArgs e)
        {
            // Уведомляем обо всех свойствах, зависящих от авторизации
            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(CurrentUserRole));
            OnPropertyChanged(nameof(CanSeeAdminPanel));
            OnPropertyChanged(nameof(CanSeeAgronomy));
            OnPropertyChanged(nameof(CanSeeBrewing));
            OnPropertyChanged(nameof(CanSeeSales));
        }

        // === Очистка ресурсов ===
        public void Dispose()
        {
            _authService.AuthStateChanged -= OnAuthStateChanged;
        }
    }
}