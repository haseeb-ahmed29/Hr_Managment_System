using HrMangmentSystem_Domain.Tenants;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace HrMangmentSystem_Infrastructure.Implementations.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly AppDbContext _appDbContext;

        public TenantRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(TenantEntity tenant)
        {
           
            await _appDbContext.Tenants.AddAsync(tenant);
        }

        public async Task<TenantEntity?> FindByCodeAsync(string code)
        {
            var normalizedCode = code.Trim();
           var tenant =  await _appDbContext.Tenants.FirstOrDefaultAsync(t => t.Code == normalizedCode);
            return tenant;
        }

        public async Task<List<TenantEntity>> GetAllAsync()
        {
            return await _appDbContext.Tenants
            .OrderBy(t => t.Name)
            .ToListAsync();
        }

        public async Task<TenantEntity?> GetByIdAsync(Guid id)
        {
            var tenant = await _appDbContext.Tenants.FindAsync(id);
            return tenant;
        }

        public async Task<TenantEntity?> GetByNameAsync(string name)
        {
            var normalizedName = name.Trim();
            var tenant = await _appDbContext.Tenants.FirstOrDefaultAsync(t => t.Name == normalizedName);
            return tenant;
        }

        public async Task SaveChangesAsync()
        {
            await _appDbContext.SaveChangesAsync();
        }

        public void Update(TenantEntity tenant)
        {
            _appDbContext.Tenants.Update(tenant);
        }
    }
}
