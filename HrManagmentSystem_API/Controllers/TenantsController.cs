using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Tenant;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Dto.DTOs.Tenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrManagmentSystem_API.Controllers
{
    [Authorize(Roles = RoleNames.SystemAdmin)]
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ILogger<TenantsController> _logger;

        public TenantsController(
            ITenantService tenantService,
            ILogger<TenantsController> logger)
        {
            _tenantService = tenantService;
            _logger = logger;
        }

        [HttpGet] // GET: api/Tenants
        public async Task<ActionResult<ApiResponse<List<TenantDto>>>> GetAllTenants()
        {
            var result = await _tenantService.GetAllTenantsAsync();
            return Ok(result);
        }
        
        [HttpGet("{id:guid}")] // GET: api/Tenants/{id}
        public async Task<ActionResult<ApiResponse<TenantDto?>>> GetTenantById(Guid id)
        {
            var result = await _tenantService.GetTenantByIdAsync(id);

            if (!result.Success || result.Data is null)
                return NotFound(result);

            return Ok(result);
        }

        // POST: api/Tenants
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TenantDto?>>> CreateTenant(
         [FromBody] CreateTenantDto createTenantDto)
        {
            if (createTenantDto is null)
                return BadRequest(ApiResponse<TenantDto?>.Fail("Invalid request."));

            var result = await _tenantService.CreateTenantAsync(createTenantDto);

            if (!result.Success || result.Data is null)
                return BadRequest(result);
          
            return Ok(result);
        }
        // PUT: api/Tenants/{id} 
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<TenantDto?>>> UpdateTenant(
          Guid id,
          [FromBody] UpdateTenantDto updateTenantDto)
        {
            if (updateTenantDto is null)
                return BadRequest(ApiResponse<TenantDto?>.Fail("Invalid request."));

            var result = await _tenantService.UpdateTenantAsync(id, updateTenantDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        //
        [HttpPut("{id:guid}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleTenantStatus(
           Guid id,
           [FromQuery] bool isActive)
        {
            var result = await _tenantService.ToggleTenantStatusAsync(id, isActive);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

    }
}
