using HrMangmentSystem_Domain.Tenants;

namespace HrMangmentSystem_Infrastructure.Interfaces.Repositories
{
    public interface ITenantRepository
    {
        Task<TenantEntity?> GetByIdAsync(Guid id);

        Task<TenantEntity?> GetByNameAsync(string name);

        Task<TenantEntity?> FindByCodeAsync(string code);

        Task<List<TenantEntity>> GetAllAsync();

        Task AddAsync(TenantEntity tenant);
        void Update(TenantEntity tenant);
        Task SaveChangesAsync();
    }
}
