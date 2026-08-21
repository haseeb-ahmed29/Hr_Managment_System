using HrManagmentSystem_Shared.Enum.Employee;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Domain.Entities.Employees;
using HrMangmentSystem_Domain.Entities.Roles;
using HrMangmentSystem_Domain.Tenants;
using HrMangmentSystem_Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace HrMangmentSystem_Infrastructure.SeedingData
{

    public static class DataSeeding
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DataSeeding");
            // Apply migrations
            await db.Database.MigrateAsync();

            logger.LogInformation("Starting database seed...");

            await SeedTenantsAsync(db, logger);
            await SeedDepartmentsAsync(db, logger);
            await SeedRolesAsync(db, logger);
            await SeedEmployeesAsync(db, logger);
            await SeedEmployeeRolesAsync(db, logger);

            logger.LogInformation("Database seed completed.");
        }


        private static async Task SeedTenantsAsync(AppDbContext db, ILogger logger)
        {
            if (await db.Tenants.AnyAsync())
            {
                logger.LogInformation("Tenants already exist. Skipping tenant seed.");
                return;
            }

            var tenant1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var tenant2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var tenant3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

            var tenants = new[]
            {
                new TenantEntity
                {
                    Id = tenant1Id,
                    Name = "AlphaTech Solutions",
                    Code = "ALPHA",
                    Description = "Software & Technology Company",
                    IsActive = true,
                    ContactEmail = "contact@alphatech.com",
                    ContactPhone = "+962790000001",
                    CreatedAt = new DateTime(2024, 1, 1),
                    CreatedBy = Guid.Empty
                },
                new TenantEntity
                {
                    Id = tenant2Id,
                    Name = "GreenField Logistics",
                    Code = "GREEN",
                    Description = "Logistics & Transportation",
                    IsActive = true,
                    ContactEmail = "info@greenfield-logistics.com",
                    ContactPhone = "+962790000002",
                    CreatedAt = new DateTime(2024, 1, 1),
                    CreatedBy = Guid.Empty
                },
                new TenantEntity
                {
                    Id = tenant3Id,
                    Name = "BlueSky Healthcare",
                    Code = "BLUE",
                    Description = "Healthcare & Clinics Group",
                    IsActive = true,
                    ContactEmail = "support@bluesky-health.com",
                    ContactPhone = "+962790000003",
                    CreatedAt = new DateTime(2024, 1, 1),
                    CreatedBy = Guid.Empty
                }
            };

            await db.Tenants.AddRangeAsync(tenants);
            await db.SaveChangesAsync();

            logger.LogInformation("Tenants seeded: {Count}", tenants.Length);
        }




        private static async Task SeedDepartmentsAsync(AppDbContext db, ILogger logger)
        {
            if (await db.Departments.AnyAsync())
            {
                logger.LogInformation("Departments already exist. Skipping department seed.");
                return;
            }

            var tenant1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var tenant2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var tenant3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

            var departments = new[]
            {
                new Department { TenantId = tenant1Id, Code = "IT",   DeptName = "IT Department",        Description = "Software & Infrastructure", Location = "HQ",        DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant1Id, Code = "HR",   DeptName = "HR Department",        Description = "Human Resources",           Location = "HQ",        DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant1Id, Code = "FIN",  DeptName = "Finance",              Description = "Finance & Accounting",      Location = "HQ",        DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant1Id, Code = "OPS",  DeptName = "Operations",           Description = "Operations & Backend",      Location = "Branch 1",  DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant1Id, Code = "MK",   DeptName = "Marketing",            Description = "Marketing & Branding",      Location = "HQ",        DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant1Id, Code = "SL",   DeptName = "Sales",                Description = "Sales Team",                Location = "Branch 2",  DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant1Id, Code = "QA",   DeptName = "Quality Assurance",    Description = "Testing & QA",              Location = "HQ",        DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant1Id, Code = "SUP",  DeptName = "Support",              Description = "Customer Support",          Location = "Branch 2",  DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant1Id, Code = "LG",   DeptName = "Logistics",            Description = "Logistics & Warehouse",     Location = "Warehouse", DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant1Id, Code = "RD",   DeptName = "R&D",                  Description = "Research & Development",    Location = "HQ",        DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },


                new Department { TenantId = tenant2Id, Code = "IT",  DeptName = "IT Department - GREEN",     Description = "Infrastructure & Internal Systems", Location = "HQ - GREEN",   DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant2Id, Code = "HR",  DeptName = "HR Department - GREEN",     Description = "People & Culture",                  Location = "HQ - GREEN",   DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant2Id, Code = "FIN", DeptName = "Finance - GREEN",           Description = "Financial Operations",              Location = "HQ - GREEN",   DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant2Id, Code = "OPS", DeptName = "Operations - GREEN",        Description = "Field & Backoffice Ops",            Location = "Branch 1",     DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant2Id, Code = "MK",  DeptName = "Marketing - GREEN",         Description = "Campaigns & Branding",              Location = "HQ - GREEN",   DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant2Id, Code = "SL",  DeptName = "Sales - GREEN",             Description = "Domestic Sales",                     Location = "Branch 2",     DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant2Id, Code = "QA",  DeptName = "Quality Assurance - GREEN", Description = "Testing & Quality Control",          Location = "HQ - GREEN",   DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant2Id, Code = "SUP", DeptName = "Support - GREEN",           Description = "Customer Care Center",               Location = "Support Hub",  DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant2Id, Code = "LG",  DeptName = "Logistics - GREEN",         Description = "Fleet & Warehousing",               Location = "Warehouse 2",  DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant2Id, Code = "RD",  DeptName = "R&D - GREEN",               Description = "Product Research & Innovation",     Location = "HQ - GREEN",   DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
              
                // Tenant 3 - BLUE
                new Department { TenantId = tenant3Id, Code = "IT",  DeptName = "IT Department - BLUE",      Description = "Networks & Cloud",                  Location = "HQ - BLUE",    DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant3Id, Code = "HR",  DeptName = "HR Department - BLUE",      Description = "Talent & Development",              Location = "HQ - BLUE",    DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant3Id, Code = "FIN", DeptName = "Finance - BLUE",            Description = "Budget & Reporting",                Location = "HQ - BLUE",    DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant3Id, Code = "OPS", DeptName = "Operations - BLUE",         Description = "Service Operations",                Location = "Branch A",     DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant3Id, Code = "MK",  DeptName = "Marketing - BLUE",          Description = "Digital & Offline Marketing",       Location = "HQ - BLUE",    DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant3Id, Code = "SL",  DeptName = "Sales - BLUE",              Description = "Regional Sales",                    Location = "Branch B",     DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant3Id, Code = "QA",  DeptName = "Quality Assurance - BLUE",  Description = "Quality & Compliance",              Location = "HQ - BLUE",    DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant3Id, Code = "SUP", DeptName = "Support - BLUE",            Description = "Technical & Customer Support",      Location = "Support Hub",  DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant3Id, Code = "LG",  DeptName = "Logistics - BLUE",          Description = "Distribution & Transport",          Location = "Warehouse 3",  DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty },
                new Department { TenantId = tenant3Id, Code = "RD",  DeptName = "R&D - BLUE",                Description = "Labs & Innovation Center",          Location = "HQ - BLUE",    DepartmentManagerId = null, CreatedAt = new DateTime(2024,1,1), CreatedBy = Guid.Empty }

                };

            await db.Departments.AddRangeAsync(departments);
            await db.SaveChangesAsync();

            logger.LogInformation("Departments seeded: {Count}", departments.Length);
        }



        private static async Task SeedRolesAsync(AppDbContext db, ILogger logger)
        {
            if (await db.Roles.AnyAsync())
            {
                logger.LogInformation("Roles already exist. Skipping role seed.");
                return;
            }

            var tenant1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var tenant2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var tenant3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

            var roles = new[]
            {
        new Role
        {

            Name = RoleNames.SystemAdmin,
            Description = "System administrator",
            TenantId = tenant1Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },
        new Role
        {

            Name = RoleNames.HrAdmin,
            Description = "HR Manager",
            TenantId = tenant1Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },
        new Role
        {

            Name = RoleNames.Manager,
            Description = "Manager",
            TenantId = tenant1Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },
        new Role
        {

            Name = RoleNames.Employee,
            Description = "Regular employee",
            TenantId = tenant1Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },
        new Role
        {

            Name = RoleNames.Recruiter,
            Description = "Recruitment role",
            TenantId = tenant1Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },
        new Role
        {

            Name = RoleNames.SystemAdmin,
            Description = "System administrator",
            TenantId = tenant2Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },   new Role
        {

            Name = RoleNames.HrAdmin,
            Description = "HR Manager",
            TenantId = tenant2Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },
        new Role
        {

            Name = RoleNames.Manager,
            Description = "Manager",
            TenantId = tenant2Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },
        new Role
        {

            Name = RoleNames.Employee,
            Description = "Regular employee",
            TenantId = tenant2Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },
        new Role
        {

            Name = RoleNames.Recruiter,
            Description = "Recruitment role",
            TenantId = tenant2Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },
        new Role
        {

            Name = RoleNames.SystemAdmin,
            Description = "System administrator",
            TenantId = tenant3Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },   new Role
        {

            Name = RoleNames.HrAdmin,
            Description = "HR Manager",
            TenantId = tenant3Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },
        new Role
        {

            Name = RoleNames.Manager,
            Description = "Manager",
            TenantId = tenant3Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },
        new Role
        {

            Name = RoleNames.Employee,
            Description = "Regular employee",
            TenantId = tenant3Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },
        new Role
        {

            Name = RoleNames.Recruiter,
            Description = "Recruitment role",
            TenantId = tenant3Id,
            CreatedAt = new DateTime(2024, 1, 1),
            CreatedBy = Guid.Empty
        },
    };

            await db.Roles.AddRangeAsync(roles);
            await db.SaveChangesAsync();

            logger.LogInformation("Roles seeded: {Count}", roles.Length);
        }


        private static async Task SeedEmployeesAsync(AppDbContext db, ILogger logger)
        {
            if (await db.Employees.AnyAsync())
            {
                logger.LogInformation("Employees already exist. Skipping employee seed.");
                return;
            }

            var tenant1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var tenant2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var tenant3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

            var emp1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var emp2Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var emp3Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
            var emp4Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
            var emp5Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
            var emp6Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
            var emp7Id = Guid.Parse("11111111-2222-3333-4444-555555555555");
            var emp8Id = Guid.Parse("22222222-3333-4444-5555-666666666666");
            var emp9Id = Guid.Parse("33333333-4444-5555-6666-777777777777");
            var emp10Id = Guid.Parse("44444444-5555-6666-7777-888888888888");

            var greenEmp1Id = Guid.Parse("0a111111-2222-3333-4444-555555555555");
            var greenEmp2Id = Guid.Parse("0a222222-3333-4444-5555-666666666666");
            var greenEmp3Id = Guid.Parse("0a333333-4444-5555-6666-777777777777");
            var greenEmp4Id = Guid.Parse("0a444444-5555-6666-7777-888888888888");
            var greenEmp5Id = Guid.Parse("0a555555-6666-7777-8888-999999999999");
            var greenEmp6Id = Guid.Parse("0a666666-7777-8888-9999-aaaaaaaaaaaa");
            var greenEmp7Id = Guid.Parse("0a777777-8888-9999-aaaa-bbbbbbbbbbbb");
            var greenEmp8Id = Guid.Parse("0a888888-9999-aaaa-bbbb-cccccccccccc");
            var greenEmp9Id = Guid.Parse("0a999999-aaaa-bbbb-cccc-dddddddddddd");
            var greenEmp10Id = Guid.Parse("0aaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

            var blueEmp1Id = Guid.Parse("0b111111-2222-3333-4444-555555555555");
            var blueEmp2Id = Guid.Parse("0b222222-3333-4444-5555-666666666666");
            var blueEmp3Id = Guid.Parse("0b333333-4444-5555-6666-777777777777");
            var blueEmp4Id = Guid.Parse("0b444444-5555-6666-7777-888888888888");
            var blueEmp5Id = Guid.Parse("0b555555-6666-7777-8888-999999999999");
            var blueEmp6Id = Guid.Parse("0b666666-7777-8888-9999-aaaaaaaaaaaa");
            var blueEmp7Id = Guid.Parse("0b777777-8888-9999-aaaa-bbbbbbbbbbbb");
            var blueEmp8Id = Guid.Parse("0b888888-9999-aaaa-bbbb-cccccccccccc");
            var blueEmp9Id = Guid.Parse("0b999999-aaaa-bbbb-cccc-dddddddddddd");
            var blueEmp10Id = Guid.Parse("0bbbbbbb-bbbb-cccc-dddd-eeeeeeeeeeee");

            var employees = new[]
             {
                 new Employee
                 {
                     Id = emp1Id,
                     TenantId = tenant1Id,
                     FirstName = "Basel",
                     LastName = "Abbasi",
                     Email = "Abbasi@alphatech.com",
                     PhoneNumber = "0797000001",
                     Position = "System Administrator",
                     Address = "Amman - Sweifieh",
                     DepartmentId = 1,
                     DateOfBirth = new DateTime(1990, 1, 10),
                     EmploymentStartDate = new DateTime(2020, 1, 1),
                     Gender = Gender.Male,
                     IsDeleted = false,
                     CreatedAt = new DateTime(2024,1,1),
                     CreatedBy = Guid.Empty
                 },
                 new Employee
                 {
                     Id = emp2Id,
                     TenantId = tenant1Id,
                     FirstName = "Sara",
                     LastName = "Hassan",
                     Email = "sara.hassan@alphatech.com",
                     PhoneNumber = "0797000002",
                     Position = "HR Manager",
                     Address = "Amman - Khalda",
                     DepartmentId = 2,
                     DateOfBirth = new DateTime(1992, 5, 5),
                     EmploymentStartDate = new DateTime(2021, 3, 1),
                     Gender = Gender.Female,
                     IsDeleted = false,
                     CreatedAt = new DateTime(2024,1,1),
                     CreatedBy = Guid.Empty
                 },
                 new Employee
                 {
                     Id = emp3Id,
                     TenantId = tenant1Id,
                     FirstName = "Ahmad",
                     LastName = "Naser",
                     Email = "ahmad.naser@alphatech.com",
                     PhoneNumber = "0797000003",
                     Position = "Backend Developer",
                     Address = "Amman - Jubeiha",
                     DepartmentId = 1,
                     DateOfBirth = new DateTime(1995, 2, 20),
                     EmploymentStartDate = new DateTime(2022, 6, 1),
                     Gender = Gender.Male,
                     IsDeleted = false,
                     CreatedAt = new DateTime(2024,1,1),
                     CreatedBy = Guid.Empty
                 },
                 new Employee
                 {
                     Id = emp4Id,
                     TenantId = tenant1Id,
                     FirstName = "Lina",
                     LastName = "Odeh",
                     Email = "lina.odeh@alphatech.com",
                     PhoneNumber = "0797000004",
                     Position = "Frontend Developer",
                     Address = "Amman - Abdoun",
                     DepartmentId = 1,
                     DateOfBirth = new DateTime(1996, 8, 15),
                     EmploymentStartDate = new DateTime(2023, 1, 1),
                     Gender = Gender.Female,
                     IsDeleted = false,
                     CreatedAt = new DateTime(2024,1,1),
                     CreatedBy = Guid.Empty
                 },
                 new Employee
                 {
                     Id = emp5Id,
                     TenantId = tenant1Id,
                     FirstName = "Yousef",
                     LastName = "Salem",
                     Email = "yousef.salem@alphatech.com",
                     PhoneNumber = "0797000005",
                     Position = "Accountant",
                     Address = "Amman - Tlaa Al Ali",
                     DepartmentId = 3,
                     DateOfBirth = new DateTime(1988, 9, 30),
                     EmploymentStartDate = new DateTime(2019, 10, 1),
                     Gender = Gender.Male,
                     IsDeleted = false,
                     CreatedAt = new DateTime(2024,1,1),
                     CreatedBy = Guid.Empty
                 },
                 new Employee
                 {
                     Id = emp6Id,
                     TenantId = tenant1Id,
                     FirstName = "Mona",
                     LastName = "Ali",
                     Email = "mona.ali@alphatech.com",
                     PhoneNumber = "0797000006",
                     Position = "Marketing Specialist",
                     Address = "Amman - Gardens",
                     DepartmentId = 5,
                     DateOfBirth = new DateTime(1993, 11, 11),
                     EmploymentStartDate = new DateTime(2020, 5, 1),
                     Gender = Gender.Female,
                     IsDeleted = false,
                     CreatedAt = new DateTime(2024,1,1),
                     CreatedBy = Guid.Empty
                 },
                 new Employee
                 {
                     Id = emp7Id,
                     TenantId = tenant1Id,
                     FirstName = "Fadi",
                     LastName = "Haddad",
                     Email = "fadi.haddad@alphatech.com",
                     PhoneNumber = "0797000007",
                     Position = "Operations Officer",
                     Address = "Amman - Bayader",
                     DepartmentId = 4,
                     DateOfBirth = new DateTime(1989, 4, 18),
                     EmploymentStartDate = new DateTime(2018, 2, 1),
                     Gender = Gender.Male,
                     IsDeleted = false,
                     CreatedAt = new DateTime(2024,1,1),
                     CreatedBy = Guid.Empty
                 },
                 new Employee
                 {
                     Id = emp8Id,
                     TenantId = tenant1Id,
                     FirstName = "Rana",
                     LastName = "Suleiman",
                     Email = "rana.suleiman@alphatech.com",
                     PhoneNumber = "0797000008",
                     Position = "Support Engineer",
                     Address = "Amman - Marka",
                     DepartmentId = 8,
                     DateOfBirth = new DateTime(1994, 7, 7),
                     EmploymentStartDate = new DateTime(2022, 9, 1),
                     Gender = Gender.Female,
                     IsDeleted = false,
                     CreatedAt = new DateTime(2024,1,1),
                     CreatedBy = Guid.Empty
                 },
                 new Employee
                 {
                     Id = emp9Id,
                     TenantId = tenant1Id,
                     FirstName = "Khaled",
                     LastName = "Awad",
                     Email = "khaled.awad@alphatech.com",
                     PhoneNumber = "0797000009",
                     Position = "Sales Representative",
                     Address = "Amman - Mecca Street",
                     DepartmentId = 6,
                     DateOfBirth = new DateTime(1991, 3, 12),
                     EmploymentStartDate = new DateTime(2021, 7, 1),
                     Gender = Gender.Male,
                     IsDeleted = false,
                     CreatedAt = new DateTime(2024,1,1),
                     CreatedBy = Guid.Empty
                 },
                 new Employee
                 {
                     Id = emp10Id,
                     TenantId = tenant1Id,
                     FirstName = "Diana",
                     LastName = "Farah",
                     Email = "diana.farah@alphatech.com",
                     PhoneNumber = "0797000010",
                     Position = "QA Engineer",
                     Address = "Amman - Jabal Amman",
                     DepartmentId = 7,
                     DateOfBirth = new DateTime(1995, 12, 25),
                     EmploymentStartDate = new DateTime(2023, 4, 1),
                     Gender = Gender.Female,
                     IsDeleted = false,
                     CreatedAt = new DateTime(2024,1,1),
                     CreatedBy = Guid.Empty
                 }
             };
             
            var greenEmployees = new[]
             {
                  new Employee

                  {
                      Id = greenEmp1Id,
                      TenantId = tenant2Id,
                      FirstName = "Omar",
                      LastName = "Khatib",
                      Email = "omar.khatib@green.com",
                      PhoneNumber = "0798100001",
                      Position = "IT Support Engineer",
                      Address = "Amman - Mecca Street",
                      DepartmentId = 21, // IT - GREEN
                      DateOfBirth = new DateTime(1991, 3, 12),

                      EmploymentStartDate = new DateTime(2021, 2, 1),
                      Gender = Gender.Male,

                      IsDeleted = false,
                      CreatedAt = new DateTime(2024, 1, 1),
                      CreatedBy = Guid.Empty
                  },
                  new Employee
                  {
                      Id = greenEmp2Id,
                      TenantId = tenant2Id,
                      FirstName = "Rawan",
                      LastName = "Jaber",
                      Email = "rawan.jaber@green.com",
                      PhoneNumber = "0798100002",
                      Position = "HR Specialist",
                      Address = "Amman - Khalda",
                      DepartmentId = 22, // HR - GREEN
                      DateOfBirth = new DateTime(1994, 7, 25),
                      EmploymentStartDate = new DateTime(2022, 5, 10),
                      Gender = Gender.Female,
                      IsDeleted = false,
                      CreatedAt = new DateTime(2024, 1, 1),
                      CreatedBy = Guid.Empty
                  },
                  new Employee
                  {
                      Id = greenEmp3Id,
                      TenantId = tenant2Id,
                      FirstName = "Ziad",
                      LastName = "Mansour",
                      Email = "ziad.mansour@green.com",
                      PhoneNumber = "0798100003",
                      Position = "Senior Accountant",
                      Address = "Amman - Jubeiha",
                      DepartmentId = 23, // FIN - GREEN
                      DateOfBirth = new DateTime(1989, 11, 3),
                      EmploymentStartDate = new DateTime(2020, 9, 1),
                      Gender = Gender.Male,
                      IsDeleted = false,
                      CreatedAt = new DateTime(2024, 1, 1),
                      CreatedBy = Guid.Empty
                  },
                  new Employee
                  {
                      Id = greenEmp4Id,
                      TenantId = tenant2Id,
                      FirstName = "Dana",
                      LastName = "Qasem",
                      Email = "dana.qasem@green.com",
                      PhoneNumber = "0798100004",
                      Position = "Operations Coordinator",
                      Address = "Amman - Abdali",
                      DepartmentId = 24, // OPS - GREEN
                      DateOfBirth = new DateTime(1993, 9, 18),
                      EmploymentStartDate = new DateTime(2023, 1, 5),
                      Gender = Gender.Female,
                      IsDeleted = false,
                      CreatedAt = new DateTime(2024, 1, 1),
                      CreatedBy = Guid.Empty
                  },
                  new Employee
                  {
                      Id = greenEmp5Id,
                      TenantId = tenant2Id,
                      FirstName = "Kareem",
                      LastName = "Shami",
                      Email = "kareem.shami@green.com",
                      PhoneNumber = "0798100005",
                      Position = "Marketing Executive",
                      Address = "Amman - Gardens",
                      DepartmentId = 25, // MK - GREEN
                      DateOfBirth = new DateTime(1992, 12, 2),
                      EmploymentStartDate = new DateTime(2022, 3, 15),
                      Gender = Gender.Male,
                      IsDeleted = false,
                      CreatedAt = new DateTime(2024, 1, 1),
                      CreatedBy = Guid.Empty
                  },
                  new Employee
                  {
                      Id = greenEmp6Id,
                      TenantId = tenant2Id,
                      FirstName = "Layla",
                      LastName = "Zein",
                      Email = "layla.zein@green.com",
                      PhoneNumber = "0798100006",
                      Position = "Sales Representative",
                      Address = "Amman - Sweileh",
                      DepartmentId = 26, // SL - GREEN
                      DateOfBirth = new DateTime(1996, 4, 9),
                      EmploymentStartDate = new DateTime(2023, 4, 1),
                      Gender = Gender.Female,
                      IsDeleted = false,
                      CreatedAt = new DateTime(2024, 1, 1),
                      CreatedBy = Guid.Empty
                  },
                  new Employee
                  {
                      Id = greenEmp7Id,
                      TenantId = tenant2Id,
                      FirstName = "Tareq",
                      LastName = "Saadeh",
                      Email = "tareq.saadeh@green.com",
                      PhoneNumber = "0798100007",
                      Position = "QA Engineer",
                      Address = "Amman - Dahiyet Al-Rasheed",
                      DepartmentId = 27, // QA - GREEN
                      DateOfBirth = new DateTime(1990, 6, 30),
                      EmploymentStartDate = new DateTime(2021, 11, 20),
                      Gender = Gender.Male,
                      IsDeleted = false,
                      CreatedAt = new DateTime(2024, 1, 1),
                      CreatedBy = Guid.Empty
                  },
                  new Employee
                  {
                      Id = greenEmp8Id,
                      TenantId = tenant2Id,
                      FirstName = "Hiba",
                      LastName = "Barakat",
                      Email = "hiba.barakat@green.com",
                      PhoneNumber = "0798100008",
                      Position = "Customer Support Agent",
                      Address = "Amman - Marka",
                      DepartmentId = 28, // SUP - GREEN
                      DateOfBirth = new DateTime(1997, 1, 14),
                      EmploymentStartDate = new DateTime(2023, 6, 1),
                      Gender = Gender.Female,
                      IsDeleted = false,
                      CreatedAt = new DateTime(2024, 1, 1),
                      CreatedBy = Guid.Empty
                  },
                  new Employee
                  {
                      Id = greenEmp9Id,
                      TenantId = tenant2Id,
                      FirstName = "Samir",
                      LastName = "Darwish",
                      Email = "samir.darwish@green.com",
                      PhoneNumber = "0798100009",
                      Position = "Logistics Officer",
                      Address = "Amman - Sahab",
                      DepartmentId = 29, // LG - GREEN
                      DateOfBirth = new DateTime(1988, 10, 8),
                      EmploymentStartDate = new DateTime(2020, 7, 1),
                      Gender = Gender.Male,
                      IsDeleted = false,
                      CreatedAt = new DateTime(2024, 1, 1),
                      CreatedBy = Guid.Empty
                  },
                  new Employee
                  {
                      Id = greenEmp10Id,
                      TenantId = tenant2Id,
                      FirstName = "Noor",
                      LastName = "Haddad",
                      Email = "noor.haddad@green.com",
                      PhoneNumber = "0798100010",
                      Position = "Research Analyst",
                      Address = "Amman - Jabal Amman",
                      DepartmentId = 30, // RD - GREEN
                      DateOfBirth = new DateTime(1995, 9, 21),
                      EmploymentStartDate = new DateTime(2022, 8, 1),
                      Gender = Gender.Female,
                      IsDeleted = false,
                      CreatedAt = new DateTime(2024, 1, 1),
                      CreatedBy = Guid.Empty
                  }
                };
            
            var blueEmployees = new[]
{
    new Employee
    {
        Id = blueEmp1Id,
        TenantId = tenant3Id,
        FirstName = "Majd",
        LastName = "Salameh",
        Email = "majd.salameh@blue.com",
        PhoneNumber = "0798200001",
        Position = "Network Engineer",
        Address = "Irbid - Wasfi Tal Street",
        DepartmentId = 31, // IT - BLUE
        DateOfBirth = new DateTime(1991, 2, 5),
        EmploymentStartDate = new DateTime(2021, 3, 1),
        Gender = Gender.Male,
        IsDeleted = false,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new Employee
    {
        Id = blueEmp2Id,
        TenantId = tenant3Id,
        FirstName = "Reem",
        LastName = "Khalaf",
        Email = "reem.khalaf@blue.com",
        PhoneNumber = "0798200002",
        Position = "HR Officer",
        Address = "Irbid - University Street",
        DepartmentId = 32, // HR - BLUE
        DateOfBirth = new DateTime(1993, 6, 17),
        EmploymentStartDate = new DateTime(2022, 2, 15),
        Gender = Gender.Female,
        IsDeleted = false,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new Employee
    {
        Id = blueEmp3Id,
        TenantId = tenant3Id,
        FirstName = "Yousef",
        LastName = "Hamdan",
        Email = "yousef.hamdan@blue.com",
        PhoneNumber = "0798200003",
        Position = "Financial Analyst",
        Address = "Irbid - Downtown",
        DepartmentId = 33, // FIN - BLUE
        DateOfBirth = new DateTime(1988, 9, 1),
        EmploymentStartDate = new DateTime(2020, 10, 1),
        Gender = Gender.Male,
        IsDeleted = false,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new Employee
    {
        Id = blueEmp4Id,
        TenantId = tenant3Id,
        FirstName = "Nadine",
        LastName = "Zohdi",
        Email = "nadine.zohdi@blue.com",
        PhoneNumber = "0798200004",
        Position = "Operations Specialist",
        Address = "Irbid - Nuzha",
        DepartmentId = 34, // OPS - BLUE
        DateOfBirth = new DateTime(1994, 3, 22),
        EmploymentStartDate = new DateTime(2023, 1, 20),
        Gender = Gender.Female,
        IsDeleted = false,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new Employee
    {
        Id = blueEmp5Id,
        TenantId = tenant3Id,
        FirstName = "Fares",
        LastName = "AbuLaila",
        Email = "fares.abulaila@blue.com",
        PhoneNumber = "0798200005",
        Position = "Digital Marketer",
        Address = "Irbid - Al Hashmi",
        DepartmentId = 35, // MK - BLUE
        DateOfBirth = new DateTime(1992, 1, 11),
        EmploymentStartDate = new DateTime(2022, 4, 10),
        Gender = Gender.Male,
        IsDeleted = false,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new Employee
    {
        Id = blueEmp6Id,
        TenantId = tenant3Id,
        FirstName = "Jana",
        LastName = "Maher",
        Email = "jana.maher@blue.com",
        PhoneNumber = "0798200006",
        Position = "Sales Coordinator",
        Address = "Irbid - Aidoon",
        DepartmentId = 36, // SL - BLUE
        DateOfBirth = new DateTime(1996, 7, 19),
        EmploymentStartDate = new DateTime(2023, 3, 5),
        Gender = Gender.Female,
        IsDeleted = false,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new Employee
    {
        Id = blueEmp7Id,
        TenantId = tenant3Id,
        FirstName = "Bilal",
        LastName = "Odeh",
        Email = "bilal.odeh@blue.com",
        PhoneNumber = "0798200007",
        Position = "QA Tester",
        Address = "Irbid - Al Husn",
        DepartmentId = 37, // QA - BLUE
        DateOfBirth = new DateTime(1990, 12, 28),
        EmploymentStartDate = new DateTime(2021, 8, 1),
        Gender = Gender.Male,
        IsDeleted = false,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new Employee
    {
        Id = blueEmp8Id,
        TenantId = tenant3Id,
        FirstName = "Maram",
        LastName = "Sabri",
        Email = "maram.sabri@blue.com",
        PhoneNumber = "0798200008",
        Position = "Support Agent",
        Address = "Irbid - Al Sareeh",
        DepartmentId = 38, // SUP - BLUE
        DateOfBirth = new DateTime(1997, 5, 6),
        EmploymentStartDate = new DateTime(2023, 7, 1),
        Gender = Gender.Female,
        IsDeleted = false,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new Employee
    {
        Id = blueEmp9Id,
        TenantId = tenant3Id,
        FirstName = "Saif",
        LastName = "Rayyan",
        Email = "saif.rayyan@blue.com",
        PhoneNumber = "0798200009",
        Position = "Logistics Coordinator",
        Address = "Irbid - Al Masharee",
        DepartmentId = 39, // LG - BLUE
        DateOfBirth = new DateTime(1989, 4, 14),
        EmploymentStartDate = new DateTime(2020, 6, 1),
        Gender = Gender.Male,
        IsDeleted = false,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new Employee
    {
        Id = blueEmp10Id,
        TenantId = tenant3Id,
        FirstName = "Nouran",
        LastName = "Saifi",
        Email = "nouran.saifi@blue.com",
        PhoneNumber = "0798200010",
        Position = "R&D Engineer",
        Address = "Irbid - Kharja",
        DepartmentId = 40, // RD - BLUE
        DateOfBirth = new DateTime(1995, 11, 9),
        EmploymentStartDate = new DateTime(2022, 9, 1),
        Gender = Gender.Female,
        IsDeleted = false,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    }
};

            // Hash password for all employees
            var hasher = new PasswordHasher<Employee>();
            foreach (var emp in blueEmployees)
            {

                emp.PasswordHash = hasher.HashPassword(emp, "Test@123");
            }

            await db.Employees.AddRangeAsync(blueEmployees);
            await db.SaveChangesAsync();

            logger.LogInformation("Employees seeded: {Count}", blueEmployees.Length);
        }



        private static async Task SeedEmployeeRolesAsync(AppDbContext db, ILogger logger)
        {
            if (await db.EmployeeRoles.AnyAsync())
            {
                logger.LogInformation("EmployeeRoles already exist. Skipping employee-role seed.");
               return;
            }

              var tenant1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var tenant2Id = Guid.Parse("22222222-2222-2222-2222-222222222222"); // GREEN
                var tenant3Id = Guid.Parse("33333333-3333-3333-3333-333333333333"); // BLUE
            
                           var roles = await db.Roles.Where(r => r.TenantId == tenant1Id).ToListAsync();

                           int sysAdminRoleId = roles.First(r => r.Name == RoleNames.SystemAdmin).Id;
                           int hrAdminRoleId = roles.First(r => r.Name == RoleNames.HrAdmin).Id;
                           int managerRoleId = roles.First(r => r.Name == RoleNames.Manager).Id;
                           int employeeRoleId = roles.First(r => r.Name == RoleNames.Employee).Id;
                           int recruiterRoleId = roles.First(r => r.Name == RoleNames.Recruiter).Id;

                           var emp1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"); // Basel
                           var emp2Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"); // Sara
                           var emp3Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"); // Ahmad
                           var emp4Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"); // Lina
                           var emp5Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"); // Yousef
                           var emp6Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"); // Mona
                           var emp7Id = Guid.Parse("11111111-2222-3333-4444-555555555555"); // Fadi
                           var emp8Id = Guid.Parse("22222222-3333-4444-5555-666666666666"); // Rana
                           var emp9Id = Guid.Parse("33333333-4444-5555-6666-777777777777"); // Khaled
                           var emp10Id = Guid.Parse("44444444-5555-6666-7777-888888888888"); // Diana

                           var employeeRoles = new[]
                  {
                       // Basel → SystemAdmin 
                       new EmployeeRole
                       {
                           EmployeeId = emp1Id,
                           RoleId = sysAdminRoleId,
                           TenantId = tenant1Id,
                           CreatedAt = new DateTime(2024, 1, 1)
                       },

                       // Sara → HrAdmin
                       new EmployeeRole
                       {
                           EmployeeId = emp2Id,
                           RoleId = hrAdminRoleId,
                           TenantId = tenant1Id,
                           CreatedAt = new DateTime(2024, 1, 1)
                       },

                       // Ahmad → Manager
                       new EmployeeRole
                       {
                           EmployeeId = emp3Id,
                           RoleId = managerRoleId,
                           TenantId = tenant1Id,
                           CreatedAt = new DateTime(2024, 1, 1)
                       },

                       // Lina → Recruiter
                       new EmployeeRole
                       {
                           EmployeeId = emp4Id,
                           RoleId = recruiterRoleId,
                           TenantId = tenant1Id,
                           CreatedAt = new DateTime(2024, 1, 1)
                       },

                       //   All  → Employee
                       new EmployeeRole
                       {
                           EmployeeId = emp5Id,
                           RoleId = employeeRoleId,
                           TenantId = tenant1Id,
                           CreatedAt = new DateTime(2024, 1, 1)
                       },
                       new EmployeeRole
                       {
                           EmployeeId = emp6Id,
                           RoleId = employeeRoleId,
                           TenantId = tenant1Id,
                           CreatedAt = new DateTime(2024, 1, 1)
                       },
                       new EmployeeRole
                       {
                           EmployeeId = emp7Id,
                           RoleId = employeeRoleId,
                           TenantId = tenant1Id,
                           CreatedAt = new DateTime(2024, 1, 1)
                       },
                       new EmployeeRole
                       {
                           EmployeeId = emp8Id,
                           RoleId = employeeRoleId,
                           TenantId = tenant1Id,
                           CreatedAt = new DateTime(2024, 1, 1)
                       },
                       new EmployeeRole
                       {
                           EmployeeId = emp9Id,
                           RoleId = employeeRoleId,
                           TenantId = tenant1Id,
                           CreatedAt = new DateTime(2024, 1, 1)
                       },
                       new EmployeeRole
                       {
                           EmployeeId = emp10Id,
                           RoleId = employeeRoleId,
                           TenantId = tenant1Id,
                           CreatedAt = new DateTime(2024, 1, 1)
                       }
                   };
                        var greenEmp1Id = Guid.Parse("0a111111-2222-3333-4444-555555555555");
                        var greenEmp2Id = Guid.Parse("0a222222-3333-4444-5555-666666666666");
                        var greenEmp3Id = Guid.Parse("0a333333-4444-5555-6666-777777777777");
                        var greenEmp4Id = Guid.Parse("0a444444-5555-6666-7777-888888888888");
                        var greenEmp5Id = Guid.Parse("0a555555-6666-7777-8888-999999999999");
                        var greenEmp6Id = Guid.Parse("0a666666-7777-8888-9999-aaaaaaaaaaaa");
                        var greenEmp7Id = Guid.Parse("0a777777-8888-9999-aaaa-bbbbbbbbbbbb");
                        var greenEmp8Id = Guid.Parse("0a888888-9999-aaaa-bbbb-cccccccccccc");
                        var greenEmp9Id = Guid.Parse("0a999999-aaaa-bbbb-cccc-dddddddddddd");
                        var greenEmp10Id = Guid.Parse("0aaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

                        //  Tenant 2 (GREEN) Roles 
                        var rolesTenant2 = await db.Roles
                            .Where(r => r.TenantId == tenant2Id)
                            .ToListAsync();

                        int t2SysAdminRoleId = rolesTenant2.First(r => r.Name == RoleNames.SystemAdmin).Id;
                        int t2HrAdminRoleId = rolesTenant2.First(r => r.Name == RoleNames.HrAdmin).Id;
                        int t2ManagerRoleId = rolesTenant2.First(r => r.Name == RoleNames.Manager).Id;
                        int t2EmployeeRoleId = rolesTenant2.First(r => r.Name == RoleNames.Employee).Id;
                        int t2RecruiterRoleId = rolesTenant2.First(r => r.Name == RoleNames.Recruiter).Id;

                        var employeeRolesTenant2 = new[]
                        {
                // 1) Omar → SystemAdmin (GREEN)
                new EmployeeRole
                {
                    EmployeeId = greenEmp1Id,
                    RoleId = t2SysAdminRoleId,
                    TenantId = tenant2Id,
                    CreatedAt = new DateTime(2024, 1, 1),
                    CreatedBy = Guid.Empty
                },

                // 2) Rawan → HrAdmin
                new EmployeeRole
                {
                    EmployeeId = greenEmp2Id,
                    RoleId = t2HrAdminRoleId,
                    TenantId = tenant2Id,
                    CreatedAt = new DateTime(2024, 1, 1),
                    CreatedBy = Guid.Empty
                },

                // 3) Ziad → Manager
                new EmployeeRole
                {
                    EmployeeId = greenEmp3Id,
                    RoleId = t2ManagerRoleId,
                    TenantId = tenant2Id,
                    CreatedAt = new DateTime(2024, 1, 1),
                    CreatedBy = Guid.Empty
                },

                // 4) Dana → Recruiter
                new EmployeeRole
                {
                    EmployeeId = greenEmp4Id,
                    RoleId = t2RecruiterRoleId,
                    TenantId = tenant2Id,
                    CreatedAt = new DateTime(2024, 1, 1),
                    CreatedBy = Guid.Empty
                },

                // All  → Employee
                new EmployeeRole
                {
                    EmployeeId = greenEmp5Id,
                    RoleId = t2EmployeeRoleId,
                    TenantId = tenant2Id,
                    CreatedAt = new DateTime(2024, 1, 1),
                    CreatedBy = Guid.Empty
                },
                new EmployeeRole
                {
                    EmployeeId = greenEmp6Id,
                    RoleId = t2EmployeeRoleId,
                    TenantId = tenant2Id,
                    CreatedAt = new DateTime(2024, 1, 1),
                    CreatedBy = Guid.Empty
                },
                new EmployeeRole
                {
                    EmployeeId = greenEmp7Id,
                    RoleId = t2EmployeeRoleId,
                    TenantId = tenant2Id,
                    CreatedAt = new DateTime(2024, 1, 1),
                    CreatedBy = Guid.Empty
                },
                new EmployeeRole
                {
                    EmployeeId = greenEmp8Id,
                    RoleId = t2EmployeeRoleId,
                    TenantId = tenant2Id,
                    CreatedAt = new DateTime(2024, 1, 1),
                    CreatedBy = Guid.Empty
                },
                new EmployeeRole
                {
                    EmployeeId = greenEmp9Id,
                    RoleId = t2EmployeeRoleId,
                    TenantId = tenant2Id,
                    CreatedAt = new DateTime(2024, 1, 1),
                    CreatedBy = Guid.Empty
                },
                new EmployeeRole
                {
                    EmployeeId = greenEmp10Id,
                    RoleId = t2EmployeeRoleId,
                    TenantId = tenant2Id,
                    CreatedAt = new DateTime(2024, 1, 1),
                    CreatedBy = Guid.Empty
                }
            };

            var blueEmp1Id = Guid.Parse("0b111111-2222-3333-4444-555555555555");
            var blueEmp2Id = Guid.Parse("0b222222-3333-4444-5555-666666666666");
            var blueEmp3Id = Guid.Parse("0b333333-4444-5555-6666-777777777777");
            var blueEmp4Id = Guid.Parse("0b444444-5555-6666-7777-888888888888");
            var blueEmp5Id = Guid.Parse("0b555555-6666-7777-8888-999999999999");
            var blueEmp6Id = Guid.Parse("0b666666-7777-8888-9999-aaaaaaaaaaaa");
            var blueEmp7Id = Guid.Parse("0b777777-8888-9999-aaaa-bbbbbbbbbbbb");
            var blueEmp8Id = Guid.Parse("0b888888-9999-aaaa-bbbb-cccccccccccc");
            var blueEmp9Id = Guid.Parse("0b999999-aaaa-bbbb-cccc-dddddddddddd");
            var blueEmp10Id = Guid.Parse("0bbbbbbb-bbbb-cccc-dddd-eeeeeeeeeeee");

            // -------------------- Tenant 3 (BLUE) Roles --------------------
            var rolesTenant3 = await db.Roles
                .Where(r => r.TenantId == tenant3Id)
                .ToListAsync();

            int t3SysAdminRoleId = rolesTenant3.First(r => r.Name == RoleNames.SystemAdmin).Id;
            int t3HrAdminRoleId = rolesTenant3.First(r => r.Name == RoleNames.HrAdmin).Id;
            int t3ManagerRoleId = rolesTenant3.First(r => r.Name == RoleNames.Manager).Id;
            int t3EmployeeRoleId = rolesTenant3.First(r => r.Name == RoleNames.Employee).Id;
            int t3RecruiterRoleId = rolesTenant3.First(r => r.Name == RoleNames.Recruiter).Id;

            var employeeRolesTenant3 = new[]
            {
    // 1) Majd → SystemAdmin (BLUE)
    new EmployeeRole
    {
        EmployeeId = blueEmp1Id,
        RoleId = t3SysAdminRoleId,
        TenantId = tenant3Id,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },

    // 2) Reem → HrAdmin
    new EmployeeRole
    {
        EmployeeId = blueEmp2Id,
        RoleId = t3HrAdminRoleId,
        TenantId = tenant3Id,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },

    // 3) Yousef → Manager
    new EmployeeRole
    {
        EmployeeId = blueEmp3Id,
        RoleId = t3ManagerRoleId,
        TenantId = tenant3Id,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },

    // 4) Nadine → Recruiter
    new EmployeeRole
    {
        EmployeeId = blueEmp4Id,
        RoleId = t3RecruiterRoleId,
        TenantId = tenant3Id,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },

    // All → Employee
    new EmployeeRole
    {
        EmployeeId = blueEmp5Id,
        RoleId = t3EmployeeRoleId,
        TenantId = tenant3Id,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new EmployeeRole
    {
        EmployeeId = blueEmp6Id,
        RoleId = t3EmployeeRoleId,
        TenantId = tenant3Id,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new EmployeeRole
    {
        EmployeeId = blueEmp7Id,
        RoleId = t3EmployeeRoleId,
        TenantId = tenant3Id,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new EmployeeRole
    {
        EmployeeId = blueEmp8Id,
        RoleId = t3EmployeeRoleId,
        TenantId = tenant3Id,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new EmployeeRole
    {
        EmployeeId = blueEmp9Id,
        RoleId = t3EmployeeRoleId,
        TenantId = tenant3Id,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    },
    new EmployeeRole
    {
        EmployeeId = blueEmp10Id,
        RoleId = t3EmployeeRoleId,
        TenantId = tenant3Id,
        CreatedAt = new DateTime(2024, 1, 1),
        CreatedBy = Guid.Empty
    }
};


            await db.EmployeeRoles.AddRangeAsync(employeeRolesTenant3);
            await db.SaveChangesAsync();

            logger.LogInformation("EmployeeRoles seeded: {Count}", employeeRolesTenant3.Length);
        }
    }
}
