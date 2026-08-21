using HrManagmentSystem_API.HealthCheck;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace HrManagmentSystem_API.Extension_Method
{
    public static class HealthChecksExtensions
    {
        public static IServiceCollection AddCustomHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddCheck<SystemDataHealthCheck>("system_data", failureStatus: HealthStatus.Unhealthy);


            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.AddHealthChecks().AddSqlServer(
                    connectionString: connectionString,
                    name: "sql_server",
                    healthQuery: "SELECT 1;",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "db", "sql", "sqlserver" });
            }

            return services;
        }

        public static IEndpointRouteBuilder MapCustomHealthChecks(
            this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            data = e.Value.Data
                        })
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                }
            });

            return endpoints;
        }

    }
}