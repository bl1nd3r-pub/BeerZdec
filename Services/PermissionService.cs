using BeerZdec.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeerZdec.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IServiceProvider _serviceProvider;
        private HashSet<(int roleId, string viewCode)> _permissionsCache;

        public PermissionService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _permissionsCache = new HashSet<(int, string)>();
        }

        public async Task LoadPermissionsAsync()
        {

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();


                var permissions = await context.RoleViewAccesses
                    .Include(rva => rva.View)
                    .ToListAsync();


                _permissionsCache.Clear();
                foreach (var perm in permissions)
                {
                    var key = (perm.UserRoleId, perm.View.ViewCode);
                    _permissionsCache.Add(key);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"   ОШИБКА при загрузке прав: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"   Inner: {ex.InnerException.Message}");
                }
            }
        }

        public bool HasAccess(string viewCode, int userRoleId)
        {
            // Проверка из HashSet
            var cacheKey = (userRoleId, viewCode);
            return _permissionsCache.Contains(cacheKey);
        }
    }
}