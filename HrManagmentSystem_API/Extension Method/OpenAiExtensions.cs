using HrMangmentSystem_Application.Config;

namespace HrManagmentSystem_API.Extension_Method
{
    public static class OpenAiExtensions
    {
        public static IServiceCollection AddOpenAi(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<OpenAiOptions>(config.GetSection("OpenAI"));
            services.AddHttpClient(); 
            return services;
        }
    }
}
