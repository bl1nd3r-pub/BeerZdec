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
        private readonly IPermissionService _permissionService;

        public MainWindowViewModel(
            INavigationService navigation,
            IAuthService authService,
            IPermissionService permissionService)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));

            _authService.AuthStateChanged += OnAuthStateChanged;

            // Загружаем права при старте
            _ = InitializePermissionsAsync();

            NavigateToWelcomeCommand = new RelayCommand(() => _navigation.NavigateTo<WelcomeViewModel>());
            NavigateToAboutCommand = new RelayCommand(() => _navigation.NavigateTo<AboutViewModel>());
            NavigateToAdminCommand = new RelayCommand(() => _navigation.NavigateTo<AdminViewModel>());
            NavigateToAgronomyCommand = new RelayCommand(() => _navigation.NavigateTo<AgronomyViewModel>());
            NavigateToMaltingCommand = new RelayCommand(() => _navigation.NavigateTo<MaltingViewModel>());
            NavigateToCrudeCommand = new RelayCommand(() => _navigation.NavigateTo<CrudeViewModel>());

            _navigation.NavigateTo<WelcomeViewModel>();
        }

        // Публичные свойства для биндинга видимости кнопок
        public bool CanSeeAdminPanel => CheckAccess("AdminPanel");
        public bool CanSeeAgronomy => CheckAccess("AgronomyModule");
        public bool CanSeeBrewing => CheckAccess("BrewingModule");
        public bool CanSeeSales => CheckAccess("SalesModule");
        public bool CanSeeMalting => CheckAccess("MaltingModule");
        public bool CanSeeCrude => CheckAccess("CrudeModule");

        // Метод проверки доступа через сервис
        private bool CheckAccess(string viewCode)
        {
            var roleId = _authService.CurrentUser?.UserRoleId ?? 0;
            var result = _permissionService.HasAccess(viewCode, roleId);
            return result;
        }

        // Инициализация прав
        private async Task InitializePermissionsAsync()
        {
            await _permissionService.LoadPermissionsAsync();
            UpdateAuthProperties();
        }

        // Обработчик изменения авторизации
        private void OnAuthStateChanged(object? sender, EventArgs e)
        {
            UpdateAuthProperties();
        }

        // Обновление всех свойств, зависящих от авторизации
        private void UpdateAuthProperties()
        {
            OnPropertyChanged(nameof(CanSeeAdminPanel));
            OnPropertyChanged(nameof(CanSeeAgronomy));
            OnPropertyChanged(nameof(CanSeeBrewing));
            OnPropertyChanged(nameof(CanSeeSales));
            OnPropertyChanged(nameof(CanSeeMalting));
            OnPropertyChanged(nameof(CanSeeCrude));
        }

        public void Dispose()
        {
            _authService.AuthStateChanged -= OnAuthStateChanged;
        }

        // Команды
        public INavigationService NavigationService => _navigation;
        public ICommand NavigateToWelcomeCommand { get; }
        public ICommand NavigateToAboutCommand { get; }
        public ICommand NavigateToAdminCommand { get; }
        public ICommand NavigateToAgronomyCommand { get; }
        public ICommand NavigateToMaltingCommand { get; }
        public ICommand NavigateToCrudeCommand { get; }
    }
}