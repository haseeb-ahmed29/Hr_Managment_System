using HrMangmentSystem_Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HrManagmentSystem_API.HealthCheck
{
    public class SystemDataHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _context;

        public SystemDataHealthCheck(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

                if (!canConnect)
                { 
                    return HealthCheckResult.Unhealthy("Cannot connect to the database.");
                }
                var tenantsCount = await _context.Tenants.CountAsync(cancellationToken);
                var employeeCount = await _context.Employees.CountAsync(cancellationToken);
                

                if (tenantsCount == 0)
                {
                    return HealthCheckResult.Degraded("Database is reachable but no tenants are configured.");
                }

                if (employeeCount == 0)
                {
                    return HealthCheckResult.Degraded("Database is reachable but no employee are found.");
                }
                var data = new Dictionary<string, object>
                {
                    ["Tenants"] = tenantsCount,
                    ["Employees"] = employeeCount
                };

                return HealthCheckResult.Healthy(
                    $"System data looks good. Tenants={tenantsCount}, Employees={employeeCount}.",
                    data);
               
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("An error occurred while checking system data health.", ex);
            }
           
        }
    }
}
