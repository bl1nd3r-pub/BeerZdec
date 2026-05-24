using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeerZdec.Models;

namespace BeerZdec.Services
{
    public interface IUserService
    {
        // Получить всех пользователей (для списка в админке)
        Task<IEnumerable<User>> GetAllUsersAsync();

        // Получить список всех доступных ролей (для выпадающего списка)
        Task<IEnumerable<UserRole>> GetAllRolesAsync();

        // Обновить данные пользователя
        Task<bool> UpdateUserAsync(int userId, string newLogin, string? newPassword, int newUserRoleId);

        Task<bool> DeleteUserAsync(int userId);
    }
}
