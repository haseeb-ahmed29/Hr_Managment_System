using HrMangmentSystem_Application.Implementation.Auth;
using HrMangmentSystem_Application.Implementation.FileStorage;
using HrMangmentSystem_Application.Implementation.Notifications;
using HrMangmentSystem_Application.Implementation.OpenAi;
using HrMangmentSystem_Application.Implementation.Requests;
using HrMangmentSystem_Application.Implementation.Services;
using HrMangmentSystem_Application.Implementation.Tenant;
using HrMangmentSystem_Application.Interfaces.Auth;
using HrMangmentSystem_Application.Interfaces.FileStorage;
using HrMangmentSystem_Application.Interfaces.Notifications;
using HrMangmentSystem_Application.Interfaces.OpenAi;
using HrMangmentSystem_Application.Interfaces.Requests;
using HrMangmentSystem_Application.Interfaces.Services;
using HrMangmentSystem_Application.Interfaces.Tenant;
using HrMangmentSystem_Infrastructure.Implementation.Requests;
using HrMangmentSystem_Infrastructure.Implementation.Tenant;
using HrMangmentSystem_Infrastructure.Implementations.Identity;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace HrMangmentSystem_Application.Extension_Method
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services )
        {
            // Add application services here
            services.AddScoped<IEmployeeService, EmployeeService>();

            services.AddScoped<IDepartmentService, DepartmentService>();

            services.AddScoped<IEmployeeRoleService, EmployeeRoleService>();
            
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IJobPositionService , JobPositionService>();

            services.AddScoped<ICurrentTenant, CurrentTenant>();


           
            services.AddScoped<ICurrentUser, HttpCurrentUser>();

            services.AddScoped<IJobApplicationService, JobApplicationService>();
            services.AddScoped<IDocumentCvService, DocumentCvService>();

            services.AddScoped<IFileStorageService, FileStorageService>();
           
            services.AddScoped<ILeaveRequestService, LeaveRequestService>();
            services.AddScoped<IEmployeeDataChangeRequestService, EmployeeDataChangeRequestService>();
            services.AddScoped<IResignationRequestService, ResignationRequestService>();
            services.AddScoped<IFinancialRequestService, FinancialRequestService>();
            services.AddScoped<IRequestService, RequestService>();
            services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
            services.AddScoped<ILeaveAccrualService, LeaveAccrualService>();

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();

            services.AddScoped<IPendingRequestsReminderService, PendingRequestsReminderService>();
            services.AddScoped<IOpenAiCvScoringClient, OpenAiCvScoringClient>();
            services.AddScoped<ICvRankingService, CvRankingService>();
            services.AddScoped<ITenantService, TenantService>();
            return services;
        }
    }
}
