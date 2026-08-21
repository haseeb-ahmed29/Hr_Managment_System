using System.Threading.RateLimiting;

namespace HrManagmentSystem_API.Extension_Method
{
    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(option =>
            {      // Global rate limiting policy
                option.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context => // Identify client by IP address
                {
                    // get IP address
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown"; 
                    return RateLimitPartition.GetSlidingWindowLimiter(ip, _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        SegmentsPerWindow = 2
                    });
                });
                // Custom response for rejected requests
                option.AddPolicy("LoginPolicy", context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                      
                    });
                });
                // Policy for upload requests
                option.AddPolicy("RequestUploadPolicy", context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
                });
                // Custom response for rejected requests 
                option.OnRejected = async (ctx, token) =>
                {
                    ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await ctx.HttpContext.Response.WriteAsync("Too many requests. Try again later.", token);
                };
            });

            return services;
        
        }
    }
}
