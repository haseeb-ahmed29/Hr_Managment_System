using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HrMangmentSystem_Infrastructure.Implementations.Identity
{
    public class HttpCurrentUser : ICurrentUser
    {   
        //JWT Fill Http.Context.User 
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
   

        public Guid EmployeeId
        {
            get
            {
                var id = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(id, out var guid))
                {
                    return Guid.Empty;
                }

                return guid;
            }
        }

 

        public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

        public Guid? TenantId
        {
            get
            {
                var tenantId = User?.FindFirst("tenantId")?.Value;
                return Guid.TryParse(tenantId, out var guid) ? guid : null;
         
            }
        }


        public IReadOnlyList<string> Roles =>
            User?
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
            ?? new List<string>();
    }
}
