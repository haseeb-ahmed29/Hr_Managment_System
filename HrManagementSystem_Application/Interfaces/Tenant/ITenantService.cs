using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Dto.DTOs.Tenant;

namespace HrMangmentSystem_Application.Interfaces.Tenant
{
    public interface ITenantService
    {
        Task<ApiResponse<TenantDto?>> CreateTenantAsync(CreateTenantDto createTenantDto);

        Task<ApiResponse<List<TenantDto>>> GetAllTenantsAsync();

        Task<ApiResponse<TenantDto?>> GetTenantByIdAsync(Guid id);

        Task<ApiResponse<TenantDto?>> UpdateTenantAsync(Guid id, UpdateTenantDto updateTenantDto);

        Task<ApiResponse<bool>> ToggleTenantStatusAsync(Guid id, bool isActive);
    }
}
