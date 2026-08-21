using AutoMapper;
using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Common.Security;
using HrMangmentSystem_Application.Interfaces.Notifications;
using HrMangmentSystem_Application.Interfaces.Requests;
using HrMangmentSystem_Application.Interfaces.Services;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Domain.Entities.Employees;
using HrMangmentSystem_Dto.DTOs.Employee;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace HrMangmentSystem_Application.Implementation.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IGenericRepository<Employee, Guid> _employeeRepository;
        private readonly IGenericRepository<Department, int> _departmentRepository;
        private readonly IEmployeeRoleService _employeeRoleService;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IPasswordHasher<Employee> _passwordHasher;
        private readonly ICurrentUser _currentUser;
        private readonly ILeaveBalanceService _leaveBalanceService;
        private readonly IEmailSender _emailSender;

        public EmployeeService(IGenericRepository<Employee, Guid> employeeRepository,
            IGenericRepository<Department, int> deparmentRepository,
            ILogger<EmployeeService> logger,
               IMapper mapper,
           IStringLocalizer<SharedResource> localizer,
          IEmployeeRoleService employeeRoleService,
          IPasswordHasher<Employee> passwordHasher,
          ICurrentUser currentUser,
          ILeaveBalanceService leaveBalanceService,
          IEmailSender emailSender)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = deparmentRepository;
            _logger = logger;
            _mapper = mapper;
            _localizer = localizer;
            _employeeRoleService = employeeRoleService;
            _passwordHasher = passwordHasher;
            _currentUser = currentUser;
            _leaveBalanceService = leaveBalanceService;
            _emailSender = emailSender;
        }

      

        public async Task<ApiResponse<EmployeeDto?>> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto)
        {
            try
            {
               
               
                if (string.IsNullOrWhiteSpace(createEmployeeDto.FirstName) || string.IsNullOrWhiteSpace(createEmployeeDto.LastName) || string.IsNullOrWhiteSpace(createEmployeeDto.Email))
                {
                    _logger.LogWarning("Create Employee: Missing basic fields (FirstName || LastName || Email) ");
                    return  ApiResponse<EmployeeDto?>.Fail(_localizer["Employee_BasicFieldsRequired"]);
                }

                if (createEmployeeDto.DateOfBirth >= DateTime.Now.Date)
                {
                    _logger.LogWarning("Create Employee: Invalid Date of Birth");

                    return ApiResponse<EmployeeDto?>.Fail(_localizer["Employee_InvalidDateOfBirth"]);
                }
                if (createEmployeeDto.EmploymentStartDate > DateTime.Now.AddDays(30))
                {
                    _logger.LogWarning("Create Employee: Invalid Employment Start Date");
                    return ApiResponse<EmployeeDto?>.Fail(_localizer["Employee_InvalidEmploymentDate"] );
                }
                var ageAtStart = createEmployeeDto.EmploymentStartDate.Date.Year - createEmployeeDto.DateOfBirth.Date.Year;
                if ( ageAtStart < 18)
                {
                    _logger.LogWarning("Create Employee: Employee must be at least 18 years old");
                    return ApiResponse<EmployeeDto?>.Fail(_localizer["Employee_MustBeAdult"]);
                }
                if (string.IsNullOrWhiteSpace(createEmployeeDto.Position))
                {
                    _logger.LogWarning("Create Employee: Position is required");
                    return ApiResponse<EmployeeDto?>.Fail(_localizer["Employee_PositionRequired"]);
                }



                var DepartmentId = await _departmentRepository.GetByIdAsync(createEmployeeDto.DepartmentId);
                if (DepartmentId is null)
                {
                    _logger.LogWarning($"Create Employee: Department Id {createEmployeeDto.DepartmentId} not found");
                    return ApiResponse<EmployeeDto?>.Fail(_localizer["Employee_DepartmentNotFound", createEmployeeDto.DepartmentId]);
                }

                var existingEmployee = await _employeeRepository.FindAsync(e => e.Email == createEmployeeDto.Email);
                if (existingEmployee.Any())
                {
                    _logger.LogWarning($"Create Employee: Employee with email {createEmployeeDto.Email} already exists");
                    return ApiResponse<EmployeeDto?>.Fail(_localizer["Employee_EmailExists", createEmployeeDto.Email]);
                }


                var employee = _mapper.Map<Employee>(createEmployeeDto);

                

                  var tempPassword = PasswordGenerator.Generate(12);

                employee.PasswordHash = _passwordHasher.HashPassword(employee, tempPassword);
                employee.LastPasswordChangeAt = null;

                await _employeeRepository.AddAsync(employee);

                await _employeeRepository.SaveChangesAsync();

                await _employeeRoleService.AssignDefaultRoleToEmployeeAsync(employee.Id);

                await _leaveBalanceService.InitializeAnnualLeaveForEmployeeAsync(
                    employee.Id, employee.EmploymentStartDate);
                  

                var employeeDto = _mapper.Map<EmployeeDto>(employee);
                try
                {
                    var subject = "Welcome to HR Management System";

                    var body = $@"
                        <html>
                          <body style=""font-family: Arial, sans-serif; font-size: 14px; color: #333;"">
                            <p>Hello {employee.FirstName},</p>
                        
                            <p>
                              Welcome to the company!<br />
                              Your account has been created in the <strong>HR Management System</strong>.
                            </p>
                        
                            <p>
                              You can log in using the following credentials:
                            </p>
                        
                            <p>
                              <strong>Email:</strong> <a href=""mailto:{employee.Email}"">{employee.Email}</a><br />
                              <strong>Temporary Password:</strong> <code>{tempPassword}</code>
                            </p>
                        
                            <p>
                              Please make sure to log in and change your password as soon as possible.
                            </p>
                        
                            <p>
                              Best regards,<br />
                              <strong>HR Team</strong>
                            </p>
                          </body>
                        </html>";
                        
                    await _emailSender.SendEmailAsync(employee.Email, subject, body);

                    _logger.LogInformation(
                        "Create Employee: Welcome email sent successfully to {Email}",
                        employee.Email);
                }
                catch (Exception ex)
                {
                   
                    _logger.LogError(
                        ex,
                        "Create Employee: Failed to send welcome email to {Email}",
                        employee.Email);
                }

                _logger.LogInformation($"Employee created successfully with Id {employee.Id}");
                return ApiResponse<EmployeeDto?>.Ok(employeeDto, _localizer["Employee_Created"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating employee");
                 return ApiResponse<EmployeeDto?>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

     
        public async Task<ApiResponse<bool>> DeleteEmployeeAsync(Guid employeeId)
        {

            try
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId);
                if (employee is null)
                {
                    _logger.LogWarning($"Delete Employee: Employee Id {employeeId} not found");
                    return ApiResponse<bool>.Fail(_localizer["Employee_NotFound"]);
                }

              
                

                var subordinates = await _employeeRepository.FindAsync(e => e.ManagerId == employeeId);
                if (subordinates.Any())
                {
                    _logger.LogWarning($"Delete Employee: Employee {employeeId} has subordinates");
                    return ApiResponse<bool>.Fail(_localizer["Employee_HasSubordinates"]);
                }

                var managedDepartments = await _departmentRepository.FindAsync(d => d.DepartmentManagerId == employeeId);
                if (managedDepartments.Any())
                {
                    _logger.LogWarning($"Delete Employee: Employee {employeeId} is assigned as department manager");
                    return ApiResponse<bool>.Fail(_localizer["Employee_IsDepartmentManager"]);
                }

                await _employeeRepository.DeleteAsync(employeeId);
                await _employeeRepository.SaveChangesAsync();

                var deletedByEmployeeId = _currentUser.EmployeeId;
                _logger.LogInformation($"Employee with Id {employeeId} deleted successfully by {deletedByEmployeeId} ");
                return ApiResponse<bool>.Ok(true, _localizer["Employee_Deleted"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting employee");
                return ApiResponse<bool>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<List<EmployeeDto>>> GetAllEmployeesAsync()
        {
            try
            { 
                var employees = await _employeeRepository.GetAllAsync();
                 var employeeDtos = _mapper.Map<List<EmployeeDto>>(employees);

                _logger.LogInformation("Retrieved all employees successfully");
                return ApiResponse<List<EmployeeDto>>.Ok(employeeDtos, _localizer["Employee_ListLoaded"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all employees");
                return ApiResponse<List<EmployeeDto>>.Fail(_localizer["Generic_UnexpectedError"]);

            }
        }

        public async Task<ApiResponse<EmployeeDto?>> GetEmployeeByIdAsync(Guid employeeId)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId);
                if (employee is null)
                {
                    _logger.LogWarning($"Get Employee By Id: Employee Id {employeeId} not found");
                    return ApiResponse<EmployeeDto?>.Fail(_localizer["Employee_NotFound"]);
                }

                var employeeDto = _mapper.Map<EmployeeDto>(employee);

                return ApiResponse<EmployeeDto?>.Ok(employeeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving employee by Id : {employeeId}");
                return ApiResponse<EmployeeDto?>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<PagedResult<EmployeeDto>>> GetEmployeesPagedAsync(PagedRequest request) 
        {
            try
            {
                if(request.PageNumber <= 0)
                    request.PageNumber = 1;

                if (request.PageSize <= 0)
                    request.PageSize = 10;


                var query = _employeeRepository.Query();

                //search (Term) by Name/ Email
                if(!string.IsNullOrWhiteSpace(request.Term))
                {
                    var term = request.Term.Trim().ToLower();

                    query = query.Where(e =>
                    !string.IsNullOrEmpty(e.FirstName) &&  e.FirstName.ToLower().Contains(term) ||
                    !string.IsNullOrWhiteSpace(e.LastName) && e.LastName.ToLower().Contains(term) ||
                    !string.IsNullOrWhiteSpace(e.Email) && e.Email.ToLower().Contains(term));
                }

                if(!string.IsNullOrWhiteSpace(request.SortBy))
                {
                    var sort = request.SortBy.Trim().ToLower();

                    query = sort switch
                    {
                        "firstname" => request.Desc
                        ? query.OrderByDescending(e => e.FirstName)
                        : query.OrderBy(e => e.FirstName),

                        "lastname" => request.Desc
                        ? query.OrderByDescending(e => e.LastName)
                        : query.OrderBy(e => e.LastName),

                        "email" => request.Desc
                        ? query.OrderByDescending(e => e.Email)
                        : query.OrderBy(e => e.Email),

                        "employmentstartdate" => request.Desc
                        ? query.OrderByDescending(e => e.EmploymentStartDate)
                        : query.OrderBy(e => e.EmploymentStartDate),

                        _ => request.Desc
                        ? query.OrderByDescending(e => e.FirstName)
                        :  query.OrderBy(e => e.FirstName)
                    };
                }
                else
                {
                    //defult sort
                    query = query.OrderBy(e => e.FirstName);
                }

                var totalCount = await query.CountAsync(); //select count (*)

                var items = await query
                   .Skip((request.PageNumber  -1 ) * request.PageSize)
                   .Take(request.PageSize)
                   .ToListAsync(); // implemanation select

                var dtoItems = _mapper.Map<List<EmployeeDto>>(items);

                var pagedResult = new PagedResult<EmployeeDto>
                {
                    Items = dtoItems,
                    TotalCount = totalCount,
                    Page = request.PageNumber,
                    PageSize = request.PageSize

                };

                _logger.LogInformation($"Retrieved employees page {request.PageNumber} with page size {request.PageSize}");

                return ApiResponse<PagedResult<EmployeeDto>>.Ok(pagedResult, _localizer["Employee_ListLoaded"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving employees with pagination ");
                return ApiResponse<PagedResult<EmployeeDto>>.Fail( _localizer["Generic_UnexpectedError"]);
            }
            
        }

        public async Task<ApiResponse<EmployeeDto>> UpdateEmployeeAsync(UpdateEmployeeDto updateEmployeeDto)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(updateEmployeeDto.Id);
                if (employee is null)
                {
                    _logger.LogWarning($"Update Employee: Employee Id {updateEmployeeDto.Id} not found");
                    return ApiResponse<EmployeeDto>.Fail(_localizer["Employee_NotFound"]);
                }
                if (updateEmployeeDto.DepartmentId.HasValue)
                { 
                    var department = await _departmentRepository.GetByIdAsync(updateEmployeeDto.DepartmentId.Value);
                    if (department is null)
                    {
                        _logger.LogWarning($"Update Employee: Department Id {updateEmployeeDto.DepartmentId} not found");
                        return ApiResponse<EmployeeDto>.Fail(_localizer["Employee_DepartmentNotFound", updateEmployeeDto.DepartmentId]);
                    }

                   employee.DepartmentId = updateEmployeeDto.DepartmentId.Value;
                }
               

                if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Position))
                    employee.Position = updateEmployeeDto.Position;

                if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Address))
                    employee.Address = updateEmployeeDto.Address;


                _employeeRepository.Update(employee);
                await _employeeRepository.SaveChangesAsync();

                var employeeDto = _mapper.Map<EmployeeDto>(employee);
                _logger.LogInformation($"Employee Id {updateEmployeeDto.Id} updated successfully");
                return ApiResponse<EmployeeDto>.Ok(employeeDto, _localizer["Employee_Updated"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating employee");
                return ApiResponse<EmployeeDto>.Fail(_localizer["Generic_UnexpectedError"]);
            }

        }

        public async Task<ApiResponse<bool>> AssignManagerAsync(Guid employeeId, Guid managerId)
        {
            try
            {
                if (employeeId == managerId)
                {
                    _logger.LogWarning("Assign Manager : Employee cannot be their own manager.");
                    return ApiResponse<bool>.Fail(_localizer["Employee_ManagerCannotBeSelf"]);
                }

                var employee = await _employeeRepository.GetByIdAsync(employeeId);
                if (employee is null)
                {
                    _logger.LogWarning("Assign Manager : Employee {EmployeeId} not found.", employeeId);
                    return ApiResponse<bool>.Fail(_localizer["Employee_NotFound", employeeId]);
                }

                var manager = await _employeeRepository.GetByIdAsync(managerId);
                if (manager is null)
                {
                    _logger.LogWarning("Assign Manager : Manager {ManagerId} not found.", managerId);
                    return ApiResponse<bool>.Fail(_localizer["Employee_ManagerNotFound", managerId]);
                }

               
                var currentTenantId = _currentUser.TenantId;

                if (employee.TenantId != currentTenantId || manager.TenantId != currentTenantId)
                {
                    _logger.LogWarning(
                        "Assign Manager : Cross-tenant assignment blocked. EmployeeTenant={EmployeeTenant}, ManagerTenant={ManagerTenant}, CurrentTenant={CurrentTenant}",
                        employee.TenantId, manager.TenantId, currentTenantId);

                    return ApiResponse<bool>.Fail(_localizer["Tenant_CrossTenantNotAllowed"]);
                }
                if(employee.DepartmentId != manager.DepartmentId)
                {
                    _logger.LogWarning(
                        "Assign Manager : Employee {EmployeeId} and Manager {ManagerId} belong to different departments.",
                        employeeId, managerId);
                    return ApiResponse<bool>.Fail(_localizer["Employee_ManagerDepartmentMismatch"]);
                }
                if (manager.ManagerId == employeeId)
                {
                    _logger.LogWarning(
                        "Assign Manager : Circular management relationship detected between Employee {EmployeeId} and Manager {ManagerId}.",
                        employeeId, managerId);
                    return ApiResponse<bool>.Fail(_localizer["Employee_CircularManagementDetected"]);
                }
                if (employee.ManagerId == managerId)
                {
                    _logger.LogInformation(
                        "Assign Manager : Employee {EmployeeId} is already assigned to Manager {ManagerId}.",
                        employeeId, managerId);
                    return ApiResponse<bool>.Ok(true, _localizer["Employee_ManagerAlreadyAssigned"]);
                }
                if (employee.Subordinates.Any(s => s.Id == managerId))
                {
                    _logger.LogWarning(
                        "Assign Manager : Circular management relationship detected. Employee {EmployeeId} manages Manager {ManagerId}.",
                        employeeId, managerId);
                    return ApiResponse<bool>.Fail(_localizer["Employee_CircularManagementDetected"]);
                }

                employee.ManagerId = managerId;

                _employeeRepository.Update(employee);
                await _employeeRepository.SaveChangesAsync();

                var roleResult =  await _employeeRoleService.AssignRoleToEmployeeAsync(managerId, RoleNames.Manager);
                if (!roleResult.Success)
                {
                    _logger.LogWarning(
                        "Assign Manager : Manager {ManagerId} assigned but failed to ensure Manager role. Reason: {Message}",
                        managerId, roleResult.Message);
                    
                }
                _logger.LogInformation(
                    "Manager {ManagerId} assigned to Employee {EmployeeId} in Tenant {TenantId}.",
                    managerId, employeeId, currentTenantId);

                return ApiResponse<bool>.Ok(true, _localizer["Employee_ManagerAssigned"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Assign Manager : Error occurred while assigning manager {ManagerId} to employee {EmployeeId}.",managerId , employeeId);
                return ApiResponse<bool>.Fail(_localizer["Generic_UnexpectedError"]);
            }

        }

    
    }

 }

