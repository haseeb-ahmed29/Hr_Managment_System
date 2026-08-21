using HrMangmentSystem_Application.Config;
using HrMangmentSystem_Application.Job;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;



namespace HrMangmentSystem_Application.Extension_Method
{
    public static class QuartzExtensions
    {
        public static IServiceCollection AddLeaveAccrualQuartz(this IServiceCollection services
            , IConfiguration configuration)
        {
            var cron = configuration["Quartz:LeaveAccrualJobCron"];

            if (string.IsNullOrWhiteSpace(cron))
            { 
                cron = "0 0 2 * * ?";
            }

            services.AddQuartz(q =>
            {
                var jobKey = new JobKey("LeaveAccrualJob");

                q.AddJob<LeaveAccrualJob>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("LeaveAccrualJob-trigger")
                    .WithCronSchedule(cron));
              
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });
         
            return services;
        }
    }
}
