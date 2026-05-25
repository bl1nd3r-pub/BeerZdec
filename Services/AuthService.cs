using BeerZdec.Interfaces;
using BeerZdec.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerZdec.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IHashingService _hashingService;

        public User CurrentUser { get; private set; }
        public string? CurrentUserRole => CurrentUser?.RoleNavigation?.RoleName;
        public bool IsAuthenticated => CurrentUser != null;

        public event EventHandler? AuthStateChanged;

        public AuthService(
            IRepository<User> userRepository,
            IRepository<UserRole> userRoleRepository,
            IHashingService hashingService
            )
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _hashingService = hashingService;
        }

        public async Task<LoginResult> LoginAsync(string login, string password)
        {
            var user = await _userRepository.Query()
                .Include(u => u.RoleNavigation)
                .FirstOrDefaultAsync(u => u.UsLogin == login);

            if (user == null) return LoginResult.UserNotFound;

            bool isValid = _hashingService.VerifyPassword(password, user.UsPassword);

            if (isValid)
            {
                CurrentUser = user;
                AuthStateChanged?.Invoke(this, EventArgs.Empty);
                return LoginResult.Success;
            }

            return LoginResult.InvalidPassword;
        }

        public async Task<bool> RegisterAsync(string login, string password, string roleName = "User")
        {
            var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.UsLogin == login);
            if (existingUser != null) return false;

            // Находим роль по имени через репозиторий
            var role = await _userRoleRepository.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role == null) return false; // Роль не найдена

            var newUser = new User
            {
                UsLogin = login,
                UsPassword = _hashingService.HashPassword(password),
                UserRoleId = role.UserRoleId
            };

            await _userRepository.AddAsync(newUser);
            return true;
        }

        public void Logout()
        {
            CurrentUser = null;
            AuthStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
