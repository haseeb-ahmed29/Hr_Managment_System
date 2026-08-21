namespace HrMangmentSystem_Dto.DTOs.Tenant
{
    public class UpdateTenantDto
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string ContactEmail { get; set; } = default!;
        public string ContactPhone { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}

