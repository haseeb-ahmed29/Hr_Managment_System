namespace HrMangmentSystem_Dto.DTOs.Tenant
{
    public class CreateTenantDto
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string ContactEmail { get; set; } = default!;
        public string ContactPhone { get; set; } = default!;
    }

}
