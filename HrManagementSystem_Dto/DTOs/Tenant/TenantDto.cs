namespace HrMangmentSystem_Dto.DTOs.Tenant
{
    public class TenantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;
        public bool IsActive { get; set; }
        public string ContactEmail { get; set; } = default!;
        public string ContactPhone { get; set; } = default!;
    }
}
