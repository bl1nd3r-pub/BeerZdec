using BeerZdec.Interfaces;
using BeerZdec.Models;
using BeerZdec.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BeerZdec.ViewModels
{
    public class AdminViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        public bool HasSelectedUser => SelectedUser != null;

        public AdminViewModel(IUserService userService)
        {
            _userService = userService;

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
                OnPropertyChanged(nameof(HasSelectedUser)); // ← Важно!

                if (value != null)
                {
                    EditLogin = value.UsLogin;
                    EditPassword = string.Empty;
                    EditRoleId = value.UserRoleId;
                }
            }
        }

        // Поля формы редактирования
        private string _editLogin = string.Empty;
        public string EditLogin
        {
            get => _editLogin;
            set => Set(ref _editLogin, value);
        }

        private string _editPassword = string.Empty;
        public string EditPassword
        {
            get => _editPassword;
            set => Set(ref _editPassword, value);
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
        }

        // Проверка возможности сохранения
        private bool CanSaveUser()
        {
            return SelectedUser != null &&
                   !string.IsNullOrWhiteSpace(EditLogin) &&
                   EditRoleId.HasValue;
        }

        // Сохранение пользователя
        private async Task SaveUser()
        {
            if (SelectedUser == null || !EditRoleId.HasValue) return;

            bool success = await _userService.UpdateUserAsync(
                SelectedUser.Id,
                EditLogin,
                string.IsNullOrWhiteSpace(EditPassword) ? null : EditPassword,
                EditRoleId.Value
            );

            if (success)
            {
                // Перезагружаем данные
                await LoadData();
                SelectedUser = null; // Снимаем выделение
            }
        }

        // Отмена редактирования
        private void CancelEdit()
        {
            SelectedUser = null;
            EditLogin = string.Empty;
            EditPassword = string.Empty;
            EditRoleId = null;
        }
    }
}