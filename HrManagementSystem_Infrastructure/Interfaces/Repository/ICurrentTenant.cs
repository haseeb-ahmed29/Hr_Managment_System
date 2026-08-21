namespace HrMangmentSystem_Infrastructure.Interfaces.Repository
{
    public interface ICurrentTenant
    {
        Guid TenantId { get; } 
        bool IsSet { get; }

        void SetTenant(Guid tenantId);
    }
}
