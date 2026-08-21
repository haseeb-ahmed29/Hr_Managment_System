using HrMangmentSystem_Domain.Entities.Employees;
using HrMangmentSystem_Domain.Tenants;

namespace HrMangmentSystem_Application.Interfaces.Auth
{
    public interface IJwtTokenGenerator
    {
        (string Token, DateTime Expiration) GenerateToken(
            Employee employee,
            TenantEntity tenant,
            IReadOnlyList<string> roles);
    }
}
