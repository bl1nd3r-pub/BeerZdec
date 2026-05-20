using BeerZdec.Models;
using BeerZdec.Interfaces;
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
        private readonly IHashingService _hashingService;

        public User CurrentUser { get; private set; }
        public bool IsAuthenticated => CurrentUser != null;

        public AuthService(IRepository<User> userRepository, IHashingService hashingService)
        {
            _userRepository = userRepository;
            _hashingService = hashingService;
        }

        public async Task<LoginResult> LoginAsync(string login, string password)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.UsLogin == login);

            if (user == null) return LoginResult.UserNotFound;

            bool isValid = _hashingService.VerifyPassword(password, user.UsPassword);

            if (isValid)
            {
                CurrentUser = user;
                return LoginResult.Success;
            }

            return LoginResult.InvalidPassword;
        }

        public async Task<bool> RegisterAsync(string login, string password, string role = "User")
        {
            var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.UsLogin == login);
            if (existingUser != null) return false;

            var newUser = new User
            {
                UsLogin = login,
                UsPassword = _hashingService.HashPassword(password),
                Role = role
            };

            await _userRepository.AddAsync(newUser);
            return true;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
