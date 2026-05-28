using BeerZdec.Interfaces;
using BeerZdec.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

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

        public virtual async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public virtual void Update(T entity)
        {
            // Используем метаданные EF Core для поиска первичного ключа
            var key = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties[0];
            var keyValue = key.PropertyInfo.GetValue(entity);

            // Ищем объект с таким же ключом в локальном кэше контекста
            var existing = _dbSet.Find(keyValue);

            if (existing != null)
            {
                // Если нашли, обновляем его значения из переданного объекта
                _context.Entry(existing).CurrentValues.SetValues(entity);
            }
            else
            {
                // Если нет, прикрепляем как измененный
                _dbSet.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            }
        }


        public virtual void Remove(T entity)
        {
            _context.Entry(entity).State = EntityState.Deleted;
            _context.SaveChanges();
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            _context.SaveChanges();
        }

        public virtual IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }
    }
}