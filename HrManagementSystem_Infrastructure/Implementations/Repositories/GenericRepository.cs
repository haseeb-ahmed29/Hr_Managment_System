using HrMangmentSystem_Domain.Common;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HrMangmentSystem_Infrastructure.Implementations.Repositories
{
    public class GenericRepository<TEntity, TId> : IGenericRepository<TEntity, TId> where TEntity : TenantEntity<TId>
    {
        private readonly AppDbContext _appDbContext;
        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntity>();
        }

        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            
        }
        public virtual IQueryable<TEntity> Query(bool asNoTracking = true)
        {
            var query = _dbSet.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();

            return query;
        }

        public virtual async Task DeleteAsync(TId id )
        {
            var db = await _dbSet
                 .FirstOrDefaultAsync(e => e.Id!.Equals(id)); // ! (bang Operator) to tell compiler that Id is not null here.

            if (db == null)
                return;


            _dbSet.Remove(db);

            
        }

        public virtual Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
           var db = _dbSet.Where(predicate).ToListAsync();
            return db;
        }

        public virtual async Task<List<TEntity>> GetAllAsync()
        {
           var db = await _dbSet.ToListAsync();
            return db;
        }

        public virtual async Task<TEntity?> GetByIdAsync(TId id)
        {
            var db = await _dbSet
                .FirstOrDefaultAsync(e => e.Id!.Equals(id));
            return db;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _appDbContext.SaveChangesAsync();
        }

       

        public void Update(TEntity entity)
        {
            var db =  _dbSet.Update(entity);
        }
    }
}
