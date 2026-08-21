using HrMangmentSystem_Infrastructure.Interfaces.Repository;

namespace HrMangmentSystem_Infrastructure.Implementation.Tenant
{
    public class CurrentTenant : ICurrentTenant
    {
        public Guid TenantId { get; private set; }

        public bool IsSet => TenantId  != Guid.Empty;

        public void SetTenant(Guid tenantId)
        {
            TenantId = tenantId;
        }
    }
}
