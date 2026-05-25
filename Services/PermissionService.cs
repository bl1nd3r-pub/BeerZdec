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
            System.Diagnostics.Debug.WriteLine("🔧 PermissionService создан");
        }

        public async Task LoadPermissionsAsync()
        {
            System.Diagnostics.Debug.WriteLine("📥 Загружаем права из БД...");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                System.Diagnostics.Debug.WriteLine("   🔗 Подключение к БД...");

                var permissions = await context.RoleViewAccesses
                    .Include(rva => rva.View)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"   📊 Найдено записей: {permissions.Count}");

                _permissionsCache.Clear();
                foreach (var perm in permissions)
                {
                    var key = (perm.UserRoleId, perm.View.ViewCode);
                    _permissionsCache.Add(key);
                    System.Diagnostics.Debug.WriteLine($"   ✅ RoleId={perm.UserRoleId}, ViewCode={perm.View.ViewCode}");
                }

                System.Diagnostics.Debug.WriteLine($"📦 Всего прав в кэше: {_permissionsCache.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ОШИБКА при загрузке прав: {ex.Message}");
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