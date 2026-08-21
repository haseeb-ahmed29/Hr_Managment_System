using AutoMapper;
using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Services;
using HrMangmentSystem_Domain.Entities.Employees;
using HrMangmentSystem_Dto.DTOs.Department;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace HrMangmentSystem_Application.Implementation.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ILogger<DepartmentService> _logger;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Department, int> _departmentRepository;
        private readonly IGenericRepository<Employee, Guid> _employeeRepository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ICurrentUser _currentUser;

        public DepartmentService(
            ILogger<DepartmentService> logger,
            IMapper mapper,
            IGenericRepository<Department, int> departmentRepository,
            IGenericRepository<Employee, Guid> employeeRepository,
            IStringLocalizer<SharedResource> localizer,
            ICurrentUser currentUser)
        {

            _localizer = localizer;
            _logger = logger;
            _mapper = mapper;
            _departmentRepository = departmentRepository;
            _employeeRepository = employeeRepository;
            _currentUser = currentUser;
        }
        public async Task<ApiResponse<DepartmentDto?>> CreateDepartmentAsync(CreateDepartmentDto createDepartmentDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createDepartmentDto.Code) || string.IsNullOrWhiteSpace(createDepartmentDto.DeptName))
                {
                    _logger.LogWarning("Create Department : DeptName and Code are required.");
                    return ApiResponse<DepartmentDto?>.Fail(_localizer["Department_CodeRequired"]);
                }

                // Check for unique of Department Code
                var existingDept = await _departmentRepository.FindAsync(d => d.Code == createDepartmentDto.Code);
                if (existingDept.Any())
                {
                    _logger.LogWarning($"Create Department : Department with Code {createDepartmentDto.Code} already exists.");
                    return ApiResponse<DepartmentDto?>.Fail(_localizer["Department_CodeExists", createDepartmentDto.Code]);
                }

                if (createDepartmentDto.ParentDepartmentId.HasValue)
                {
                    var parentDept = await _departmentRepository.GetByIdAsync(createDepartmentDto.ParentDepartmentId.Value);
                    if (parentDept is null)
                    {
                        _logger.LogWarning($"Create Department : Parent Department with Id {createDepartmentDto.ParentDepartmentId.Value} not found.");
                        return ApiResponse<DepartmentDto?>.Fail(_localizer["Department_ParentNotFound", createDepartmentDto.ParentDepartmentId.Value]);
                    }
                }

                var department = _mapper.Map<Department>(createDepartmentDto);

                await _departmentRepository.AddAsync(department);
                await _departmentRepository.SaveChangesAsync();

                var deptDto = _mapper.Map<DepartmentDto>(department);

                _logger.LogInformation($"Department with Id {deptDto.Id} created successfully.");
                return ApiResponse<DepartmentDto?>.Ok(deptDto, _localizer["Department_Created"]);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating depatment");
                return ApiResponse<DepartmentDto?>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<bool>> DeleteDepartmentAsync(int departmentId)
        {
            try
            {
                var department = await _departmentRepository.GetByIdAsync(departmentId);
                if (department is null)
                {
                    _logger.LogWarning($"Delete Department: Department with Id {departmentId} not found");
                    return ApiResponse<bool>.Fail(_localizer["Department_NotFound", departmentId]);
                }
 

                var hasEmployees = await _employeeRepository.FindAsync(e => e.DepartmentId == departmentId);
                if (hasEmployees.Any())
                {
                    _logger.LogWarning($"Delete Department : Department {departmentId} is has employee ");
                    return ApiResponse<bool>.Fail(_localizer["Department_HasEmployees", departmentId]);
                }

                var hasDepartment = await _departmentRepository.FindAsync(d => d.ParentDepartmentId == departmentId);
                if (hasDepartment.Any())
                {
                    _logger.LogWarning($"Delete Department : Department {departmentId} is has department children ");
                    return ApiResponse<bool>.Fail(_localizer["Department_HasChildren", departmentId]);
                }
                await _departmentRepository.DeleteAsync(departmentId );
                await _departmentRepository.SaveChangesAsync();

                var deletedEmployeeBy = _currentUser.EmployeeId;
                _logger.LogInformation($"Department with Id {departmentId} deleted successfully by {deletedEmployeeBy}");
                return ApiResponse<bool>.Ok(true, _localizer["Department_Deleted",departmentId]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting department");
                return ApiResponse<bool>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

      

        public async Task<ApiResponse<DepartmentDto?>> GetDepartmentByIdAsync(int departmentId)
        {
            try
            {
                var department = await _departmentRepository.GetByIdAsync(departmentId);
                if (department is null)
                {
                    _logger.LogWarning($"Get Department By Id: Department Id {departmentId} not found");
                    return ApiResponse<DepartmentDto?>.Fail(_localizer["Department_NotFound", departmentId]);
                }
                var departmentDto = _mapper.Map<DepartmentDto>(department);

                return ApiResponse<DepartmentDto?>.Ok(departmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving {departmentId} departments");
                return ApiResponse<DepartmentDto?>.Fail(_localizer["Generic_UnexpectedError"]);
            }
         }

        public async Task<ApiResponse<PagedResult<DepartmentDto>>> GetDepartmentPagedAsync(PagedRequest request)
        {
            try
            {
                if (request.PageNumber <= 0)
                    request.PageNumber = 1;

                if (request.PageSize <= 0)
                    request.PageSize = 10;

                var query = _departmentRepository.Query();

                if (!string.IsNullOrWhiteSpace(request.Term))
                {
                    var term = request.Term.Trim().ToLower();

                    query = query.Where(d =>
                        !string.IsNullOrEmpty(d.DeptName) && d.DeptName.ToLower().Contains(term) ||
                        !string.IsNullOrEmpty(d.Code) && d.Code.ToLower().Contains(term) ||
                        !string.IsNullOrEmpty(d.Location) && d.Location.ToLower().Contains(term));
                }
                if (!string.IsNullOrWhiteSpace(request.SortBy))
                {
                    var sort = request.SortBy.Trim().ToLower();

                    query = sort switch
                    {
                        "deptname" => request.Desc
                        ? query.OrderByDescending(d => d.DeptName)
                        : query.OrderBy(d => d.DeptName),

                        "code" => request.Desc
                        ? query.OrderByDescending(d => d.Code)
                        : query.OrderBy(d => d.Code),

                        "location" => request.Desc
                        ? query.OrderByDescending(d => d.Location)
                        : query.OrderBy(d => d.Location),

                        _ => request.Desc
                        ? query.OrderByDescending(d => d.DeptName)
                        : query.OrderBy(d => d.DeptName)
                    };

                }
                else
                {
                    query = query.OrderBy(d => d.DeptName);
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var dtoItems = _mapper.Map<List<DepartmentDto>>(items);

                var pagedResult = new PagedResult<DepartmentDto>
                {
                    Items = dtoItems,
                    TotalCount = totalCount,
                    Page = request.PageNumber,
                    PageSize = request.PageSize

                };

                _logger.LogInformation($"Retrieved department page {request.PageNumber} with page size {request.PageSize}");


                return ApiResponse<PagedResult<DepartmentDto>>.Ok(pagedResult, _localizer["Department_ListLoaded"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving department with pagination ");
                return ApiResponse<PagedResult<DepartmentDto>>.Fail(_localizer["Generic_UnexpectedError"]);
            }

         }

     
        public async Task<ApiResponse<DepartmentDto>> UpdateDepartmentAsync(UpdateDepartmentDto updateDepartmentDto)
        {
            try
            {
                var department = await _departmentRepository.GetByIdAsync(updateDepartmentDto.Id);
                if (department is null)
                {
                    _logger.LogWarning($"Update Department: Department Id {updateDepartmentDto.Id} not found");
                    return ApiResponse<DepartmentDto>.Fail(_localizer["Department_NotFound", updateDepartmentDto.Id]);
                }
                // Check for unique of Department Code
                if (!string.IsNullOrWhiteSpace(updateDepartmentDto.Code) && updateDepartmentDto.Code != department.Code)
                {
                    var existingDept = await _departmentRepository.FindAsync(d => d.Code == updateDepartmentDto.Code && d.Id != updateDepartmentDto.Id);
                    if (existingDept.Any())
                    {
                        _logger.LogWarning($"Update Department : Department with Code {updateDepartmentDto.Code} already exists.");
                        return ApiResponse<DepartmentDto>.Fail(_localizer["Department_CodeExists", updateDepartmentDto.Code]);
                    }
                }
                if (updateDepartmentDto.ParentDepartmentId.HasValue)
                {
                    // A department cannot be its own parent
                    if (updateDepartmentDto.ParentDepartmentId.Value == updateDepartmentDto.Id)
                    {
                        _logger.LogWarning("Update Department: A department cannot be its own parent");
                        return ApiResponse<DepartmentDto>.Fail(_localizer["Department_CannotBeOwnParent"]);
                    }

                    var parentDept = await _departmentRepository.GetByIdAsync(updateDepartmentDto.ParentDepartmentId.Value);
                    if (parentDept is null)
                    {
                        _logger.LogWarning($"Update Department: Parent Department Id {updateDepartmentDto.ParentDepartmentId} not found");
                        return ApiResponse<DepartmentDto>.Fail(_localizer["Department_ParentNotFound"]);
                    }
                }
                if(!string.IsNullOrWhiteSpace(updateDepartmentDto.Code))
                    department.Code = updateDepartmentDto.Code;

                if (!string.IsNullOrWhiteSpace(updateDepartmentDto.DeptName))
                    department.DeptName = updateDepartmentDto.DeptName;

                if (!string.IsNullOrWhiteSpace(updateDepartmentDto.Description))
                    department.Description = updateDepartmentDto.Description;

                if (!string.IsNullOrWhiteSpace(updateDepartmentDto.Location))
                    department.Location = updateDepartmentDto.Location;

                department.ParentDepartmentId = updateDepartmentDto.ParentDepartmentId;

                _departmentRepository.Update(department);
                await _departmentRepository.SaveChangesAsync();

                var deptDto = _mapper.Map<DepartmentDto>(department);
                _logger.LogInformation($"Department with Id {deptDto.Id} updated successfully.");
                return ApiResponse<DepartmentDto>.Ok(deptDto, _localizer["Department_Updated"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating department");
                return ApiResponse<DepartmentDto>.Fail(_localizer["Generic_UnexpectedError"]);

            }
        }


        public async Task<ApiResponse<bool>> SetDepartmentManagerAsync(int departmentId, Guid managerEmployeeId)
        {
            try
            {
                var department = await _departmentRepository.GetByIdAsync(departmentId);
                if (department is null)
                {
                    _logger.LogWarning("Set Department Manager : Department {DepartmentId} not found.", departmentId);
                    return ApiResponse<bool>.Fail(_localizer["Department_NotFound", departmentId]);
                }

                var manager = await _employeeRepository.GetByIdAsync(managerEmployeeId);
                if (manager is null)
                {
                    _logger.LogWarning("Set Department Manager : Manager {ManagerId} not found.", managerEmployeeId);
                    return ApiResponse<bool>.Fail(_localizer["Employee_ManagerNotFound", managerEmployeeId]);
                }

                var currentTenantId = _currentUser.TenantId;

              
                if (department.TenantId != currentTenantId || manager.TenantId != currentTenantId)
                {
                    _logger.LogWarning(
                        "Set Department Manager : Cross-tenant operation blocked. DepartmentTenant={DepartmentTenant}, ManagerTenant={ManagerTenant}, CurrentTenant={CurrentTenant}",
                        department.TenantId, manager.TenantId, currentTenantId);

                    return ApiResponse<bool>.Fail(_localizer["Tenant_CrossTenantNotAllowed"]);
                }

                
                if (manager.DepartmentId != department.Id)
                {
                    _logger.LogWarning(
                        "Set Department Manager : Manager {ManagerId} does not belong to Department {DepartmentId}.",
                        managerEmployeeId, departmentId);

                    return ApiResponse<bool>.Fail(_localizer["Department_ManagerDepartmentMismatch"]);
                }

           
                if (department.DepartmentManagerId == managerEmployeeId)
                {
                    _logger.LogInformation(
                        "Set Department Manager : Department {DepartmentId} already has manager {ManagerId}.",
                        departmentId, managerEmployeeId);

                    return ApiResponse<bool>.Ok(true, _localizer["Department_ManagerAlreadyAssigned"]);
                }

                department.DepartmentManagerId = managerEmployeeId;

                _departmentRepository.Update(department);
                await _departmentRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Set Department Manager : Department {DepartmentId} manager set to employee {ManagerId} in Tenant {TenantId}.",
                    departmentId, managerEmployeeId, currentTenantId);

                return ApiResponse<bool>.Ok(true, _localizer["Department_ManagerAssigned"]);
            
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                          "Set Department Manager : Error occurred while setting manager {ManagerId} for Department {DepartmentId}.",
                          managerEmployeeId, departmentId);
                return ApiResponse<bool>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<bool>> SetParentDepartmentAsync(int departmentId, int? parentDepartmentId)
        {
            try
            {
                var department = await _departmentRepository.GetByIdAsync(departmentId);
                if (department is null)
                {
                    _logger.LogWarning("Set Parent Department : Department {DepartmentId} not found.", departmentId);
                    return ApiResponse<bool>.Fail(_localizer["Department_NotFound", departmentId]);
                }

                var currentTenantId = _currentUser.TenantId;

                Department? parentDept = null;

                if (parentDepartmentId.HasValue)
                {
                    if (parentDepartmentId.Value == departmentId)
                    {
                        _logger.LogWarning("Set Parent Department : Department cannot be its own parent.");
                        return ApiResponse<bool>.Fail(_localizer["Department_InvalidParent"]);
                    }

                    parentDept = await _departmentRepository.GetByIdAsync(parentDepartmentId.Value);
                    if (parentDept is null)
                    {
                        _logger.LogWarning(
                            "Set Parent Department : Parent Department {ParentId} not found.",
                            parentDepartmentId.Value);

                        return ApiResponse<bool>.Fail(_localizer["Department_ParentNotFound", parentDepartmentId.Value]);
                    }

                
                    if (department.TenantId != currentTenantId || parentDept.TenantId != currentTenantId)
                    {
                        _logger.LogWarning(
                            "Set Parent Department : Cross-tenant hierarchy blocked. DepartmentTenant={DepartmentTenant}, ParentTenant={ParentTenant}, CurrentTenant={CurrentTenant}",
                            department.TenantId, parentDept.TenantId, currentTenantId);

                        return ApiResponse<bool>.Fail(_localizer["Tenant_CrossTenantNotAllowed"]);
                    }

                   
                    var visited = new HashSet<int> { departmentId };
                    var current = parentDept;

                    while (current != null && current.ParentDepartmentId != null)
                    // Traverse up the hierarchy
                    {
                        int currentParentId = current.ParentDepartmentId.Value; // Get the parent ID

                        if (currentParentId == departmentId)  // Cycle detected
                        {
                            _logger.LogWarning(
                                "Set Parent Department : Cycle detected when setting parent {ParentId} for department {DepartmentId}.",
                                parentDepartmentId.Value, departmentId);

                            return ApiResponse<bool>.Fail(_localizer["Department_CyclicHierarchyNotAllowed"]);
                        }

                        if (!visited.Add(currentParentId)) // If already visited, cycle detected
                        {
                            _logger.LogWarning(
                                "Set Parent Department : Cycle detected due to repeated parent id {ParentId} for department {DepartmentId}.",
                                currentParentId, departmentId);

                            return ApiResponse<bool>.Fail(_localizer["Department_CyclicHierarchyNotAllowed"]);
                        }

                        current = await _departmentRepository.GetByIdAsync(currentParentId);
                    }
                }
                else
                {
                   
                    if (department.TenantId != currentTenantId)
                    {
                        _logger.LogWarning(
                            "Set Parent Department : Cross-tenant operation blocked for department {DepartmentId}. Tenant={TenantId}, CurrentTenant={CurrentTenant}",
                            departmentId, department.TenantId, currentTenantId);

                        return ApiResponse<bool>.Fail(_localizer["Tenant_CrossTenantNotAllowed"]);
                    }
                }

                department.ParentDepartmentId = parentDepartmentId;

                _departmentRepository.Update(department);
                await _departmentRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Set Parent Department : Department {DepartmentId} parent set to {ParentId} in Tenant {TenantId}.",
                    departmentId,
                    parentDepartmentId?.ToString() ?? "NULL",
                    currentTenantId);

                return ApiResponse<bool>.Ok(true, _localizer["Department_ParentAssigned"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Set Parent Department : Error occurred while setting parent {ParentId} for Department {DepartmentId}.",
                    parentDepartmentId, departmentId);

                return ApiResponse<bool>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

    }
}
