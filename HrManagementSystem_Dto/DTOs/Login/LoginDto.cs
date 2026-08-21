namespace HrMangmentSystem_Dto.DTOs.Login
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        public string TenantCode { get; set; } = null!;


    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }


        public Guid EmployeeId { get; set; }

        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public Guid TenantId { get; set; }

        public List<string> Roles { get; set; } = new();
        
        public bool MustChangePassword { get; set; }

    }
}
