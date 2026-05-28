// DbRepository.cs
using BeerZdec.Interfaces;
using BeerZdec.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BeerZdec.Repositories
{
    public class DbRepository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public DbRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public virtual async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _dbSet.Where(predicate).ToListAsync();
        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) => await _dbSet.FirstOrDefaultAsync(predicate);

        public virtual IQueryable<T> Query() => _dbSet.AsQueryable();

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        // --- АСИНХРОННОЕ ОБНОВЛЕНИЕ ---
        public virtual async Task<bool> UpdateAsync(T entity)
        {
            try
            {
                // Используем метаданные EF Core для поиска первичного ключа
                var key = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties[0];
                var keyValue = key.PropertyInfo.GetValue(entity);

                // Ищем объект в контексте
                var existing = await _dbSet.FindAsync(keyValue);

                if (existing != null)
                {
                    // Если найден, обновляем свойства
                    _context.Entry(existing).CurrentValues.SetValues(entity);
                }
                else
                {
                    // Если нет, прикрепляем как Modified
                    _dbSet.Attach(entity);
                    _context.Entry(entity).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! [DbRepository] UpdateAsync error: {ex.Message}");
                return false;
            }
        }

        // --- АСИНХРОННОЕ УДАЛЕНИЕ ---
        public virtual async Task<bool> RemoveAsync(T entity)
        {
            try
            {
                var keyProperty = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties[0];
                var keyValue = keyProperty.PropertyInfo.GetValue(entity);

                var hasDependencies = await HasForeignDependenciesAsync(entity, keyValue);

                if (hasDependencies)
                {
                    return false; // Есть реальные FK-зависимости
                }

                var trackedEntity = await _dbSet.FindAsync(keyValue);

                if (trackedEntity != null)
                {
                    _dbSet.Remove(trackedEntity);
                }
                else
                {
                    _dbSet.Attach(entity);
                    _dbSet.Remove(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // SQL Server error 547 = constraint violation
                if (ex.InnerException?.Message.Contains("547") == true ||
                    ex.Message.Contains("547") == true)
                {
                    return false;
                }

                Debug.WriteLine($"!!! [DbRepository] DbUpdateException in RemoveAsync: {ex.Message}");
                return false; // На всякий случай считаем всё, что DbUpdateException — FK-ошибкой
            }
            catch (InvalidOperationException ex)
            {
                // Проверяем, это нарушение связи (FK) или ошибка трекинга
                var msg = ex.Message;

                // Это нарушение обязательной связи (FK violation)
                if (msg.Contains("association between entity types") &&
                    msg.Contains("has been severed") &&
                    msg.Contains("foreign key is not nullable"))
                {
                    Debug.WriteLine($"!!! [DbRepository] FK violation in RemoveAsync: {ex.Message}");
                    return false; // Показываем диалог пользователю
                }

                // Это ошибка трекинга — пробрасываем для отладки
                Debug.WriteLine($"!!! [DbRepository] Tracking error in RemoveAsync: {ex.Message}");
                throw;
            }
        }

        private async Task<bool> HasForeignDependenciesAsync(T entity, object keyValue)
        {
            var entityType = _context.Model.FindEntityType(typeof(T));
            var pkName = entityType.FindPrimaryKey().Properties[0].Name;

            // Проходим по всем типам сущностей в модели
            foreach (var otherEntityType in _context.Model.GetEntityTypes())
            {
                // Ищем FK, которые ссылаются на наш тип
                var foreignKeys = otherEntityType.GetForeignKeys()
                    .Where(fk => fk.PrincipalEntityType == entityType);

                foreach (var fk in foreignKeys)
                {
                    // Получаем CLR тип сущности, которая может ссылаться на нас
                    var dependentClrType = otherEntityType.ClrType;

                    // Получаем имя FK-свойства
                    var fkPropertyName = fk.Properties[0].Name;

                    // Создаём запрос: await _context.OtherEntities.AnyAsync(e => e.FKProperty == keyValue)
                    var depends = await HasDependentRecordsAsync(dependentClrType, fkPropertyName, keyValue);

                    if (depends)
                        return true;
                }
            }

            return false;
        }

        private async Task<bool> HasDependentRecordsAsync(Type entityType, string fkPropertyName, object keyValue)
        {
            // Получаем DbSet для типа
            var dbSetMethod = _context.GetType().GetMethod("Set", Type.EmptyTypes);
            var dbSet = dbSetMethod?.MakeGenericMethod(entityType).Invoke(_context, null) as IQueryable;

            if (dbSet == null) return false;

            // Создаём выражение: e => e.fkProperty == keyValue
            var parameter = Expression.Parameter(entityType, "e");
            var property = Expression.Property(parameter, fkPropertyName);
            var constant = Expression.Constant(keyValue);
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda(equality, parameter);

            // Ищем метод AnyAsync с 3 параметрами: (IQueryable, Expression<Func<...>>, CancellationToken)
            var anyMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .First(m => m.Name == "AnyAsync" && m.GetParameters().Length == 3)
                .MakeGenericMethod(entityType);

            // Вызываем с CancellationToken.None
            var task = anyMethod.Invoke(null, new object[] { dbSet, lambda, CancellationToken.None }) as Task<bool>;

            if (task != null)
            {
                await task;
                return task.Result;
            }

            return false;
        }

        public virtual async Task RemoveRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        // --- АСИНХРОННОЕ СОХРАНЕНИЕ (если нужно вызвать отдельно) ---
        public virtual async Task<bool> SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) when (ex is DbUpdateException || ex is InvalidOperationException)
            {
                Debug.WriteLine($"!!! [DbRepository] SaveChangesAsync error: {ex.Message}");
                return false;
            }
        }
    }
}