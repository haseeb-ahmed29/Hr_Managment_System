using HrMangmentSystem_Infrastructure.Interfaces.Repository;

namespace HrManagmentSystem_API.Middleware
{
    public class CurrentTenantMiddleware
    {
        private readonly RequestDelegate _next;

        public CurrentTenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context, ICurrentTenant currentTenant)
        {
            // check he is user and authen?
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var tenantClaim = context.User.FindFirst("tenantId");

                if(tenantClaim != null && 
                    Guid.TryParse(tenantClaim.Value, out var tenantId) &&
                    tenantId != Guid.Empty)
                {
                    currentTenant.SetTenant(tenantId);
                }
                
            }
            await _next(context);
        }
    }
}
