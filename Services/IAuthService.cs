using BeerZdec.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerZdec.Services
{
    public interface IAuthService
    {
        User CurrentUser { get; }
        bool IsAuthenticated { get; }

        Task<LoginResult> LoginAsync(string login, string password);
        Task<bool> RegisterAsync(string login, string password, string role = "User");
        void Logout();
    }
}
