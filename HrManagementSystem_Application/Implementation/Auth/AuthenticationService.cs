using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Auth;
using HrMangmentSystem_Domain.Entities.Employees;
using HrMangmentSystem_Domain.Entities.Roles;
using HrMangmentSystem_Dto.DTOs.Login;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace HrMangmentSystem_Application.Implementation.Auth
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IGenericRepository<Employee, Guid> _employeeRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IGenericRepository<EmployeeRole, int> _employeeRoleRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IPasswordHasher<Employee> _passwordHasher;


        public AuthenticationService(IGenericRepository<Employee, Guid> employeeRepository,
                   ITenantRepository tenantRepository,
                   IGenericRepository<EmployeeRole, int> employeeRoleRepository,
                 IJwtTokenGenerator jwtTokenGenerator,
                   ILogger<AuthenticationService> logger,
                   IStringLocalizer<SharedResource> localizer,
                   IPasswordHasher<Employee> passwordHasher)
        {
            _employeeRepository = employeeRepository;
            _employeeRoleRepository = employeeRoleRepository;
            _tenantRepository = tenantRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _logger = logger;
            _localizer = localizer;
            _passwordHasher = passwordHasher;
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid employeeId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId);
                if (employee is null)
                {
                    _logger.LogWarning("ChangePassword: Employee {employeeId} not found", employeeId);
                    return ApiResponse<bool>.Fail(_localizer["Employee_NotFound"]);
                }

                var verifyResult = _passwordHasher.VerifyHashedPassword(employee, employee.PasswordHash, changePasswordDto.CurrentPassword);

                var isValidCurrent = verifyResult == PasswordVerificationResult.Success || verifyResult == PasswordVerificationResult.SuccessRehashNeeded;

                if (!isValidCurrent)
                {
                    _logger.LogWarning("ChangePassword: Invalid current password for employee {employeeId}", employeeId);
                    return ApiResponse<bool>.Fail(_localizer["Auth_InvalidCurrentPassword"]);
                }

                var passwordErrors = ValidatePassword(changePasswordDto.NewPassword, employee.Email);
                if (passwordErrors.Any())
                {
                    var firstErrorKey = passwordErrors.First();
                    _logger.LogWarning("ChangePassword: Invalid new password for employee {employeeId} becuse {error key}" , employeeId , _localizer[firstErrorKey]);
                    return ApiResponse<bool>.Fail(_localizer[firstErrorKey]);
                }
                if (changePasswordDto.NewPassword == changePasswordDto.CurrentPassword)
                {
                    return ApiResponse<bool>.Fail(_localizer["Auth_NewPasswordMustBeDifferent"]);
                }

                employee.PasswordHash = _passwordHasher.HashPassword(employee, changePasswordDto.NewPassword);
                employee.MustChangePassword = false;
                employee.LastPasswordChangeAt = DateTime.Now;

                _employeeRepository.Update(employee);
                await _employeeRepository.SaveChangesAsync();

                _logger.LogInformation("ChangePassword: Password changed successfully for employee {employeeId}" , employeeId);

                return ApiResponse<bool>.Ok(true, _localizer["Auth_PasswordChanged"]);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChangePasswordAsync");
                return ApiResponse<bool>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequestDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginRequestDto.Email) ||
                    string.IsNullOrWhiteSpace(loginRequestDto.Password) ||
                    string.IsNullOrWhiteSpace(loginRequestDto.TenantCode))
                {
                    _logger.LogWarning("Login failed : missing Tenant Code or Email or Password");
                    return ApiResponse<LoginResponseDto>.Fail(_localizer["Auth_InvalidCredentials"]);
                }

                var tenantCode = loginRequestDto.TenantCode.Trim();

                var tenant = await _tenantRepository.FindByCodeAsync(tenantCode);
                if (tenant is null || !tenant.IsActive)
                {
                    _logger.LogWarning("Login failed: Tenant with code {tenantCode} not found or inactive" , tenantCode);
                    return ApiResponse<LoginResponseDto>.Fail(_localizer["Auth_InvalidCredentials"]);
                }
                var employees = await _employeeRepository.FindAsync(e =>
                e.Email == loginRequestDto.Email &&
                e.TenantId == tenant.Id);
                // Why not use FirstOrDefault First ? becuse work by
                // Repo (don't have method FirstOrDefault) Not Like DBSet
                var employee = employees.FirstOrDefault();
                if (employee is null)
                {
                    _logger.LogWarning("Login failed: Employee with email {loginRequestDto.Email} not found in Tenant {tenant.Id}" 
                        , loginRequestDto.Email , tenant.Id);
                    return ApiResponse<LoginResponseDto>.Fail(_localizer["Auth_InvalidCredentials"]);
                }

                var query = _employeeRoleRepository.Query()
                    .Where(er => er.EmployeeId == employee.Id)
                    .Include(er => er.Role);

                var roleNames = await query
                    .Select(er => er.Role.Name)
                    .ToListAsync();

                var verifyResult = _passwordHasher.VerifyHashedPassword(employee, employee.PasswordHash, loginRequestDto.Password);

                var IsValidPassword = verifyResult == PasswordVerificationResult.Success || verifyResult == PasswordVerificationResult.SuccessRehashNeeded;

                if (!IsValidPassword)
                {
                    _logger.LogWarning("Login failed: Invalid password for email {loginRequestDto.Email} in tenant {tenant.Id}"
                       ,loginRequestDto.Email , tenant.Id);

                    return ApiResponse<LoginResponseDto>.Fail(_localizer["Auth_InvalidCredentials"]);
                }
                                                                           
                //tuple value (deconstruction)
                var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(employee, tenant, roleNames);

                var responseDto = new LoginResponseDto
                {
                    Token = token,
                    Expiration = expiresAt,
                    EmployeeId = employee.Id,
                    FullName = $"{employee.FirstName} {employee.LastName}",
                    Email = employee.Email,
                    TenantId = tenant.Id,
                    Roles = roleNames,
                };


                _logger.LogInformation("Login succeeded for {employee.Email} in tenant {tenant.Id}" , employee.Email, tenant.Id);

                return ApiResponse<LoginResponseDto>.Ok(responseDto, _localizer["Auth_LoginSuccess"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during login for email {loginRequestDto.Email}" , loginRequestDto.Email);
                return ApiResponse<LoginResponseDto>.Fail(_localizer["Generic_UnexpectedError"]);

            }
        }

        private static readonly char[] _passwordSpecialChars = "!@#$%^&*()_+-=[]{}|;':\",.<>/?`~\\".ToCharArray();

        private static readonly HashSet<string> _weakPasswords = new(StringComparer.OrdinalIgnoreCase)
        {
                         "password", "passw0rd", "admin", "user",
                       "123456", "12345678", "abc123", "87654321"
        };



        private static List<string> ValidatePassword(string? password, string? email = null, int minLength = 12)
        {

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Auth_Password_Required");
                return errors;
            }

            if (password.Length < minLength)
                errors.Add("Auth_Password_TooShort");

            if (!password.Any(char.IsUpper))
                errors.Add("Auth_Password_MissingUpper");

            if (!password.Any(char.IsLower))
                errors.Add("Auth_Password_MissingLower");

            if (!password.Any(char.IsDigit))
                errors.Add("Auth_Password_MissingDigit");

            if (!password.Any(c => _passwordSpecialChars.Contains(c)))
                errors.Add("Auth_Password_MissingSpecial");

            if (password.Any(char.IsWhiteSpace))
                errors.Add("Auth_Password_HasWhitespace");


            if (!string.IsNullOrWhiteSpace(email))
            {
                var local = email.Split('@')[0];
                if (!string.IsNullOrWhiteSpace(local) &&
                    password.IndexOf(local, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    errors.Add("Auth_Password_ContainsEmailPart");
                }
            }

            if (_weakPasswords.Contains(password))
                errors.Add("Auth_Password_IsCommon");

            return errors;
        }



    }
}

