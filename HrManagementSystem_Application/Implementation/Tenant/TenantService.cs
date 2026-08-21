using AutoMapper;
using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Tenant;
using HrMangmentSystem_Domain.Tenants;
using HrMangmentSystem_Dto.DTOs.Tenant;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace HrMangmentSystem_Application.Implementation.Tenant
{
    public class TenantService : ITenantService
    {
        private readonly ILogger<TenantService> _logger;
        private readonly IMapper _mapper;
        private readonly ITenantRepository _tenantRepository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ICurrentUser _currentUser;

        public TenantService(
            ILogger<TenantService> logger,
            IMapper mapper,
            ITenantRepository tenantRepository,
            IStringLocalizer<SharedResource> localizer,
            ICurrentUser currentUser)
        {
            _logger = logger;
            _mapper = mapper;
            _tenantRepository = tenantRepository;
            _localizer = localizer;
            _currentUser = currentUser;
        }

      

        public async Task<ApiResponse<TenantDto?>> CreateTenantAsync(CreateTenantDto createTenantDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createTenantDto.Code) ||
                    string.IsNullOrWhiteSpace(createTenantDto.Name))
                {
                    _logger.LogWarning("Create Tenant : Name and Code are required.");
                    return ApiResponse<TenantDto?>.Fail(_localizer["Tenant_NameCodeRequired"]);
                }

                // Unique Code check
                var existingByCode = await _tenantRepository.FindByCodeAsync(createTenantDto.Code);
                if (existingByCode is not null)
                {
                    _logger.LogWarning("Create Tenant : Tenant with Code {Code} already exists.", createTenantDto.Code);
                    return ApiResponse<TenantDto?>.Fail(_localizer["Tenant_CodeExists", createTenantDto.Code]);
                }

                // Optional: unique Name
                var existingByName = await _tenantRepository.GetByNameAsync(createTenantDto.Name);
                if (existingByName is not null)
                {
                    _logger.LogWarning("Create Tenant : Tenant with Name {Name} already exists.", createTenantDto.Name);
                    return ApiResponse<TenantDto?>.Fail(_localizer["Tenant_NameExists", createTenantDto.Name]);
                }

                var tenant = _mapper.Map<TenantEntity>(createTenantDto);

                
                tenant.CreatedBy = _currentUser.EmployeeId;
                tenant.CreatedAt = DateTime.Now;

                await _tenantRepository.AddAsync(tenant);
                await _tenantRepository.SaveChangesAsync();

                var dto = _mapper.Map<TenantDto>(tenant);

                _logger.LogInformation("Tenant with Id {TenantId} created successfully.", dto.Id);
                return ApiResponse<TenantDto?>.Ok(dto, _localizer["Tenant_Created"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating tenant");
                return ApiResponse<TenantDto?>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

      

      

        public async Task<ApiResponse<List<TenantDto>>> GetAllTenantsAsync()
        {
            try
            {
                var tenants = await _tenantRepository.GetAllAsync();
                var dtos = _mapper.Map<List<TenantDto>>(tenants);

                _logger.LogInformation("Retrieved {Count} tenants.", dtos.Count);
                return ApiResponse<List<TenantDto>>.Ok(dtos, _localizer["Tenant_ListRetrieved"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting tenants");
                return ApiResponse<List<TenantDto>>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

       

       

        public async Task<ApiResponse<TenantDto?>> GetTenantByIdAsync(Guid id)
        {
            try
            {
                var tenant = await _tenantRepository.GetByIdAsync(id);
                if (tenant is null)
                {
                    _logger.LogWarning("Get Tenant : Tenant with Id {TenantId} not found.", id);
                    return ApiResponse<TenantDto?>.Fail(_localizer["Tenant_NotFound", id]);
                }

                var dto = _mapper.Map<TenantDto>(tenant);
                return ApiResponse<TenantDto?>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting tenant by id");
                return ApiResponse<TenantDto?>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<TenantDto?>> UpdateTenantAsync(Guid id, UpdateTenantDto updateTenantDto)
        {
            try
            {
                var tenant = await _tenantRepository.GetByIdAsync(id);
                if (tenant is null)
                {
                    _logger.LogWarning("Update Tenant : Tenant with Id {TenantId} not found.", id);
                    return ApiResponse<TenantDto?>.Fail(_localizer["Tenant_NotFound", id]);
                }

                if (string.IsNullOrWhiteSpace(updateTenantDto.Name))
                {
                    _logger.LogWarning("Update Tenant : Name is required.");
                    return ApiResponse<TenantDto?>.Fail(_localizer["Tenant_NameRequired"]);
                }

                tenant.Name = updateTenantDto.Name;
                tenant.IsActive = updateTenantDto.IsActive;
                tenant.UpdatedBy = _currentUser.EmployeeId;
                tenant.UpdatedAt = DateTime.Now;

                _tenantRepository.Update(tenant);
                await _tenantRepository.SaveChangesAsync();

                var dto = _mapper.Map<TenantDto>(tenant);

                _logger.LogInformation("Tenant with Id {TenantId} updated successfully.", dto.Id);
                return ApiResponse<TenantDto?>.Ok(dto, _localizer["Tenant_Updated"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating tenant");
                return ApiResponse<TenantDto?>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<bool>> ToggleTenantStatusAsync(Guid id, bool isActive)
        {
            try
            {
                var tenant = await _tenantRepository.GetByIdAsync(id);
                if (tenant is null)
                {
                    _logger.LogWarning("Toggle Tenant Status : Tenant with Id {TenantId} not found.", id);
                    return ApiResponse<bool>.Fail(_localizer["Tenant_NotFound", id]);
                }

                tenant.IsActive = isActive;
                tenant.UpdatedBy = _currentUser.EmployeeId;
                tenant.UpdatedAt = DateTime.Now;

                _tenantRepository.Update(tenant);
                await _tenantRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Tenant with Id {TenantId} status changed to {IsActive}.",
                    id,
                    isActive);

                return ApiResponse<bool>.Ok(true, _localizer["Tenant_StatusUpdated"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling tenant status");
                return ApiResponse<bool>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        
    }



}
