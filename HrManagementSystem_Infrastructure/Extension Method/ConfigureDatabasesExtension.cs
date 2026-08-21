using HrMangmentSystem_Domain.Entities.Employees;
using HrMangmentSystem_Domain.Entities.Recruitment;
using HrMangmentSystem_Domain.Entities.Requests;
using HrMangmentSystem_Domain.Entities.Roles;
using HrMangmentSystem_Infrastructure.Implementations.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrMangmentSystem_Infrastructure.Extension_Method
{
    public static class ConfigureDatabasesExtension
    {
        // This method configures the database contexts and repositories for dependency injection
        public static IServiceCollection AddConfigureDatabases(this IServiceCollection services, IConfiguration configuration) 
        {

            // Configure database contexts here
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>)); // Register the generic repository


            services.AddScoped<IGenericRepository<Employee, Guid>, SoftDeleteRepository<Employee, Guid>>(); // Register SoftDeleteRepository for Employee entity
            services.AddScoped<IGenericRepository<Department, int>, SoftDeleteRepository<Department, int>>(); // Register SoftDeleteRepository for Department entity
            services.AddScoped<IGenericRepository<JobPosition, int>, SoftDeleteRepository<JobPosition, int>>();  
            services.AddScoped<IGenericRepository<EmployeeRole, int>, GenericRepository<EmployeeRole, int>>(); 
            services.AddScoped<IGenericRepository<JobApplication, int>, SoftDeleteRepository<JobApplication, int>>();
            services.AddScoped<IGenericRepository<DocumentCv , int>, GenericRepository<DocumentCv , int>>();
            services.AddScoped<IGenericRepository<Role, int>, GenericRepository<Role, int>>();
            services.AddScoped<IGenericRepository<EmployeeDataChange, int>, SoftDeleteRepository<EmployeeDataChange, int>>();
            services.AddScoped<IGenericRepository<FinancialRequest,int> , SoftDeleteRepository<FinancialRequest, int>>();
            services.AddScoped<IGenericRepository<GenericRequest ,int>, SoftDeleteRepository<GenericRequest ,int>>();
            services.AddScoped<IGenericRepository<LeaveRequest, int>, SoftDeleteRepository<LeaveRequest, int>>();
            services.AddScoped<IGenericRepository<RequestHistory,int> , GenericRepository<RequestHistory, int>>();
            services.AddScoped<IGenericRepository<DocumentEmployeeInfo, Guid>, SoftDeleteRepository<DocumentEmployeeInfo, Guid>>();
            services.AddScoped<IGenericRepository<EmployeeLeaveBalance, int>, SoftDeleteRepository<EmployeeLeaveBalance, int>>();
            services.AddScoped<ITenantRepository, TenantRepository>();


           
            services.AddScoped<IPasswordHasher<Employee>, PasswordHasher<Employee>>();

       

            return services;
        }

    }
}
