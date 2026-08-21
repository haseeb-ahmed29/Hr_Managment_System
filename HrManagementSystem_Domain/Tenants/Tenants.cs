using HrMangmentSystem_Domain.Common;

namespace HrMangmentSystem_Domain.Tenants
{
    public class TenantEntity : BaseEntity<Guid>
    {
        public string Name { get; set; } = null!;

        public string Code { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool IsActive { get; set; }

        public string ContactEmail { get; set; } = null!;
        public string ContactPhone { get; set; } = null!;

    }
}
