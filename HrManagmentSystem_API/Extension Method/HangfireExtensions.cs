using Hangfire;
using Hangfire.SqlServer;
using HrMangmentSystem_Application.Interfaces.Requests;

namespace HrManagmentSystem_API.Extension_Method
{
    public static class HangfireExtensions
    {
       
        public static IServiceCollection AddHangfireWithJobs(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (!configuration.GetValue("Hangfire:Enabled", false))
                return services;

            services.AddHangfire(config =>
            {          // Hangfire configuration
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180) 
                      .UseSimpleAssemblyNameTypeSerializer()
                      .UseRecommendedSerializerSettings()    
                      .UseSqlServerStorage(
                          configuration.GetConnectionString("DefaultConnection"),
                          new SqlServerStorageOptions
                          {
                              CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                              SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                              QueuePollInterval = TimeSpan.FromSeconds(15),
                              UseRecommendedIsolationLevel = true,
                              DisableGlobalLocks = true
                          });
            });

            services.AddHangfireServer();

            return services;
        }

        
        public static WebApplication UseHangfireRecurringJobs(this WebApplication app)
        {
            if (!app.Configuration.GetValue("Hangfire:Enabled", false))
                return app;

            app.UseHangfireDashboard("/hangfire");

            RecurringJob.AddOrUpdate<IPendingRequestsReminderService>(
                recurringJobId: "PendingRequestsReminder",
                methodCall: s => s.SendPendingRequestsSummaryAsync(),
                cronExpression: Cron.HourInterval(3));

            return app;
        }
    }
}
