using HrMangmentSystem_Domain.Tenants;
using System.ComponentModel.DataAnnotations;

namespace HrMangmentSystem_Domain.Common
{
    public class TenantEntity<T> : BaseEntity<T> , ITenantEntity
    {
        public Guid TenantId { get; set; } 
        public TenantEntity Tenant { get; set; } = null!;

    }
}
