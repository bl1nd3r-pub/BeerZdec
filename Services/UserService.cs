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
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IHashingService _hashingService;

        public UserService(
            IRepository<User> userRepository,
            IRepository<UserRole> userRoleRepository,
            IHashingService hashingService)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _hashingService = hashingService;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.Query()
                .Include(u => u.RoleNavigation)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserRole>> GetAllRolesAsync()
        {
            return await _userRoleRepository.GetAllAsync();
        }

        public async Task<bool> UpdateUserAsync(int userId, string newLogin, int newUserRoleId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.UsLogin = newLogin;
            user.UserRoleId = newUserRoleId;

            _userRepository.Update(user);
            return true;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            _userRepository.Remove(user);
            return true;
        }
    }
}
