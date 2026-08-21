using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Services;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Domain.Entities.Employees;
using HrMangmentSystem_Domain.Entities.Roles;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace HrMangmentSystem_Application.Implementation.Services
{
    public class EmployeeRoleService : IEmployeeRoleService
    {
        private readonly IGenericRepository<EmployeeRole, int> _employeeRoleRepository;
        private readonly IGenericRepository<Employee, Guid> _employeeRepository;
        private readonly IGenericRepository<Role, int> _roleRepository;
        private readonly IStringLocalizer<SharedResource> _localization;
        private readonly ILogger<EmployeeRoleService> _logger;

        public EmployeeRoleService(
            IGenericRepository<EmployeeRole, int> employeeRoleRepository,
            IGenericRepository<Employee, Guid> employeeRepository,
            IGenericRepository<Role, int> roleRepository,
            IStringLocalizer<SharedResource> localization,
            ILogger<EmployeeRoleService> logger)
        {
            _employeeRepository = employeeRepository;
            _employeeRoleRepository = employeeRoleRepository;
            _roleRepository = roleRepository;
            _localization = localization;
            _logger = logger;
        }


        public async Task<ApiResponse<bool>> AssignDefaultRoleToEmployeeAsync(Guid employeId) =>
            await AssignRoleToEmployeeAsync(employeId, RoleNames.Employee);

        public async Task<ApiResponse<bool>> AssignRoleToEmployeeAsync(Guid employeeId, string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    _logger.LogWarning("AssignRole: Role name is required");
                    return ApiResponse<bool>.Fail(_localization["Role_NameRequired"]);
                }

                var employee = await _employeeRepository.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    _logger.LogWarning($"AssignRole: Employee {employeeId} not found");
                    return ApiResponse<bool>.Fail(_localization["Employee_NotFound"]);
                }

                var RoleList = await _roleRepository.FindAsync(r => r.Name == roleName);   // Why not use FirstOrDefault First ? becuse work by
                var Role = RoleList.FirstOrDefault();                                               // Repo (don't have method FirstOrDefault) Not Like DBSet
                if (Role == null)
                {
                    _logger.LogError($"AssignRole: Role {roleName} not found");
                    return ApiResponse<bool>.Fail(_localization["Role_NotFound", roleName]);
                }

                var existing = await _employeeRoleRepository.FindAsync(
                    er => er.EmployeeId == employeeId && er.RoleId == Role.Id);

                if (existing.Any())
                {
                    _logger.LogInformation($"AssignRole: Employee {employeeId} already has role {RoleNames.Employee}");
                    return ApiResponse<bool>.Ok(true, _localization["EmployeeRole_AlreadyAssigned"],roleName);
                }

                var employeeRole = new EmployeeRole
                {
                    EmployeeId = employeeId,
                    RoleId = Role.Id
                };

                await _employeeRoleRepository.AddAsync(employeeRole);
                await _employeeRoleRepository.SaveChangesAsync();

                _logger.LogInformation($"AssignRole: Role {RoleNames.Employee} assigned to Employee {employeeId}");
                return ApiResponse<bool>.Ok(true, _localization["EmployeeRole_Assigned"],roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while assigning role {roleName} to Employee {employeeId} ");
                return ApiResponse<bool>.Fail(_localization["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<bool>> UpdateRoleToEmployeeAsync(Guid employeeId, string newRoleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newRoleName))
                {
                    _logger.LogWarning("UpdateRole: Role name is required");
                    return ApiResponse<bool>.Fail(_localization["Role_NameRequired"]);
                }

                var employee = await _employeeRepository.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    _logger.LogWarning("UpdateRole: Employee {EmployeeId} not found", employeeId);
                    return ApiResponse<bool>.Fail(_localization["Employee_NotFound"]);
                }

                
                var roles = await _roleRepository.FindAsync(r => r.Name == newRoleName);
                var newRole = roles.FirstOrDefault();
                if (newRole == null)
                {
                    _logger.LogWarning("UpdateRole: Role {RoleName} not found", newRoleName);
                    return ApiResponse<bool>.Fail(_localization["Role_NotFound", newRoleName]);
                }

                // Prevent cross-tenant role assignments
                if (employee.TenantId != newRole.TenantId)
                {
                    _logger.LogWarning(
                        "UpdateRole: Cross-tenant role update blocked. EmployeeTenant={EmployeeTenant}, RoleTenant={RoleTenant}",
                        employee.TenantId, newRole.TenantId);

                    return ApiResponse<bool>.Fail(_localization["Tenant_CrossTenantNotAllowed"]);
                }

                var employeeRoles = await _employeeRoleRepository.FindAsync(er => er.EmployeeId == employeeId);
                var employeeRolesList = employeeRoles.ToList();

                    
                if (!employeeRolesList.Any())
                {
                    var employeeRole = new EmployeeRole
                    {
                        EmployeeId = employeeId,
                        RoleId = newRole.Id,
                        TenantId = employee.TenantId
                    };

                    await _employeeRoleRepository.AddAsync(employeeRole);
                    await _employeeRoleRepository.SaveChangesAsync();

                    _logger.LogInformation(
                        "UpdateRole: Employee {EmployeeId} had no roles. Assigned role {RoleName}.",
                        employeeId, newRoleName);

                    return ApiResponse<bool>.Ok(true, _localization["EmployeeRole_Assigned", newRoleName]);
                }

               
                if (employeeRolesList.Any(er => er.RoleId == newRole.Id))
                {
                    _logger.LogInformation(
                        "UpdateRole: Employee {EmployeeId} already has role {RoleName}.",
                        employeeId, newRoleName);

                    return ApiResponse<bool>.Ok(true, _localization["EmployeeRole_AlreadyAssigned", newRoleName]);
                }

                // For simplicity, update the first role found
                var employeeRoleToUpdate = employeeRolesList.First();

                employeeRoleToUpdate.RoleId = newRole.Id;

                _employeeRoleRepository.Update(employeeRoleToUpdate);
                await _employeeRoleRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "UpdateRole: Employee {EmployeeId} role updated to {RoleName}.",
                    employeeId, newRoleName);

                return ApiResponse<bool>.Ok(true, _localization["EmployeeRole_Updated", newRoleName]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "UpdateRole: Error while updating role for Employee {EmployeeId} to {RoleName}",
                    employeeId, newRoleName);

                return ApiResponse<bool>.Fail(_localization["Generic_UnexpectedError"]);
            }
        }


    }
}
