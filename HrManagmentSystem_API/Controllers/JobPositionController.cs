using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Services;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Dto.DTOs.Job.Appilcation;
using HrMangmentSystem_Dto.DTOs.Job.Position;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HrManagmentSystem_API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleNames.HrAdmin + "," + RoleNames.Recruiter + "," + RoleNames.SystemAdmin)]
    public class JobPositionController : ControllerBase
    {
        private readonly IJobPositionService _jobPositionService;
        private readonly IJobApplicationService _jobApplicationService;
        public JobPositionController(IJobPositionService jobPositionService, IJobApplicationService jobApplicationService)
        {
            _jobPositionService = jobPositionService;
            _jobApplicationService = jobApplicationService;
        }


        [HttpGet]
        public async Task<ActionResult<ApiResponse<JobPositionDto>>> GetAllJobPosition([FromQuery] PagedRequest request)
        {
            var result = await _jobPositionService.GetJobPositionsPagedAsync(request);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<JobPositionDto>>> GetJobPositionById(int id)
        {
            var result = await _jobPositionService.GetJobPositionsByIdAsync(id);
            if (result.Data is null || !result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }


        [HttpPost]
        public async Task<ActionResult<ApiResponse<JobPositionDto>>> CreateJobPosition([FromBody] CreateJobPositionDto createJobPositionDto)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.ToDictionary(
                         k => k.Key,
                         v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                         );
                return BadRequest(ApiResponse<JobPositionDto>.Fail("Invalid data", error: error));
            }

            var result = await _jobPositionService.CreateJobPositionsAsync(createJobPositionDto);

            if (!result.Success || result.Data is null)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetJobPositionById), new { id = result.Data!.Id }, result);
        }


        [HttpPut]
        public async Task<ActionResult<ApiResponse<JobPositionDto>>> UpdateJobPosition([FromBody] UpdateJobPositionDto updateJobPositionDto)
        {

            var result = await _jobPositionService.UpdateJobPositionsAsync(updateJobPositionDto);

            if (!result.Success || result.Data is null)
            {
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteJobPosition(int jobPositionId)
        {


            var result = await _jobPositionService.DeleteJobPositionsAsync(jobPositionId);

            if (!result.Success)
            {
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(result);

                return BadRequest(result);
            }
            return Ok(result);
        }


        [HttpPost("{id:int}/status")]
        public async Task<ActionResult<ApiResponse<JobPositionDto>>> ChangeStatus([FromBody] ChangeJobPositionStatusDto changeJobPositionStatusDto)
        {

            var result = await _jobPositionService.ChangeStatusAsync(changeJobPositionStatusDto);
            return Ok(result);
        }


        [HttpPost("{jobPositionId:int}/apply")]
        [AllowAnonymous]
        //[EnableRateLimiting("FixedWindowPolicy")]
        public async Task<ActionResult<ApiResponse<JobApplicationDto>>> Apply(
            int jobPositionId,
            [FromBody] CreateJobApplicationDto createJobApplicationDto,
            [FromQuery] string tenantCode,
            [FromServices] ITenantRepository tenantRepository,
            [FromServices] ICurrentTenant currentTenant
            )
        {
            var tenant = await tenantRepository.FindByCodeAsync(tenantCode);
            if (tenant is null)
                return ApiResponse<JobApplicationDto>.Fail("Tenant_NotFound");

            currentTenant.SetTenant(tenant.Id);

            var result = await _jobApplicationService.ApplyAsync(jobPositionId, createJobApplicationDto);
            return Ok(result);
        }


        [HttpGet("{jobPositionId:int}/applications")]
        public async Task<ActionResult<ApiResponse<PagedResult<JobApplicationDto>>>> GetApplications(
            int jobPositionId , 
            [FromQuery] PagedRequest request)
        {
            var result = await _jobApplicationService.GetByJobApplicationPagedAsync(jobPositionId, request);
            return Ok(result);
        }


        [HttpPut("status")]
        [Authorize(Roles =$"{RoleNames.HrAdmin},{RoleNames.Recruiter },{RoleNames.SystemAdmin}")]
        public async Task<ActionResult<ApiResponse<JobApplicationDto>>> ChangeStatus(
            [FromBody] ChangeJobApplicationStatusDto changeJobApplicationStatusDto)
        {
            var result = await _jobApplicationService.ChangeStatusAsync(changeJobApplicationStatusDto);
            return Ok(result);
        }
    }
}
