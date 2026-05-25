using BeerZdec.Interfaces;
using BeerZdec.Models;
using BeerZdec.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BeerZdec.ViewModels
{
    public class AdminViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public AdminViewModel(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;

            Users = new ObservableCollection<User>();
            Roles = new ObservableCollection<UserRole>();

            LoadDataCommand = new RelayCommandAsync(LoadData);
            SaveUserCommand = new RelayCommandAsync(SaveUser, CanSaveUser);
            CancelEditCommand = new RelayCommand(CancelEdit);

            _ = LoadData();
        }

        // Коллекции данных
        public ObservableCollection<User> Users { get; set; }
        public ObservableCollection<UserRole> Roles { get; set; }

        // Выбранный пользователь
        private User? _selectedUser;
        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                Set(ref _selectedUser, value);
                OnPropertyChanged(nameof(HasSelectedUser));
                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(HintMessage));

                if (value != null)
                {
                    EditLogin = value.UsLogin;
                    EditRoleId = value.UserRoleId;
                }
            }
        }

        // ID текущего залогиненного пользователя
        private int CurrentUserId => _authService.CurrentUser?.Id ?? 0;

        // Свойства для UI
        public bool HasSelectedUser => SelectedUser != null;
        public bool CanSave => SelectedUser != null && SelectedUser.Id != CurrentUserId;
        public string HintMessage => CanSave ? "" : "Нельзя изменять данные текущего пользователя";

        // Поля формы редактирования (ТОЛЬКО логин и роль)
        private string _editLogin = string.Empty;
        public string EditLogin
        {
            get => _editLogin;
            set => Set(ref _editLogin, value);
        }

        private int? _editRoleId;
        public int? EditRoleId
        {
            get => _editRoleId;
            set => Set(ref _editRoleId, value);
        }

        // Команды
        public ICommand LoadDataCommand { get; }
        public ICommand SaveUserCommand { get; }
        public ICommand CancelEditCommand { get; }

        // Загрузка данных
        private async Task LoadData()
        {
            // Загружаем пользователей
            var users = await _userService.GetAllUsersAsync();
            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }

            // Загружаем роли
            var roles = await _userService.GetAllRolesAsync();
            Roles.Clear();
            foreach (var role in roles)
            {
                Roles.Add(role);
            }

            // Обновляем свойства для UI
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(HintMessage));
        }

        // Проверка возможности сохранения
        private bool CanSaveUser()
        {
            return SelectedUser != null &&
                   SelectedUser.Id != CurrentUserId &&
                   !string.IsNullOrWhiteSpace(EditLogin) &&
                   EditRoleId.HasValue;
        }

        // Сохранение пользователя (БЕЗ ПАРОЛЯ!)
        private async Task SaveUser()
        {
            if (SelectedUser == null || !EditRoleId.HasValue || !CanSaveUser())
                return;

            bool success = await _userService.UpdateUserAsync(
                SelectedUser.Id,
                EditLogin,
                EditRoleId.Value
            );

            if (success)
            {
                await LoadData();
                CancelEdit();
            }
        }

        // Отмена редактирования
        private void CancelEdit()
        {
            SelectedUser = null;
            EditLogin = string.Empty;
            EditRoleId = null;
        }
    }
}