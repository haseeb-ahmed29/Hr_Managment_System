using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Services;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Dto.DTOs.Department;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrManagmentSystem_API.Controllers
{
  
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleNames.HrAdmin+ "," + RoleNames.SystemAdmin)]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        /*
        [HttpGet] // get all : api/departments
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetAllDepartments()
        {
            var result = await _departmentService.GetAllDepartmentsAsync();
            return Ok(result);
        }*/

        [HttpGet] // get all : api/departments
        public async Task<ActionResult<ApiResponse<PagedResult<DepartmentDto>>>> GetAllDepartments([FromQuery] PagedRequest request)
        {
            var result = await _departmentService.GetDepartmentPagedAsync(request);
            return Ok(result);
        }

        [HttpGet("{id:int}")] // get by id : api/departments/{id}
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetDepartmentById(int id)
        {
            var result = await _departmentService.GetDepartmentByIdAsync(id);
            if (result.Data is null || !result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost] // create : api/departments
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> CreateDepartment([FromBody] CreateDepartmentDto createDepartmentDto)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.ToDictionary(
                         k => k.Key,
                         v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                         );

                return BadRequest(ApiResponse<DepartmentDto>.Fail("Invalid data", error: error));
            }
            
            var result = await _departmentService.CreateDepartmentAsync(createDepartmentDto);
            
            if (!result.Success || result.Data == null)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetDepartmentById), new { id = result.Data!.Id }, result);
        }


        [HttpPut]  // update : api/departments
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> UpdateDepatrment ( [FromBody] UpdateDepartmentDto updateDepartmentDto)
        {
            var result = await _departmentService.UpdateDepartmentAsync(updateDepartmentDto);

            if (!result.Success)
            {
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }


        [HttpDelete("{id:int}")] // delete : api/departments/{id}
        public async Task<ActionResult<ApiResponse<bool>>> DeleteDepartment(int departmentId)
        {
            var result = await _departmentService.DeleteDepartmentAsync(departmentId);
            if (!result.Success)
            {
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return NotFound(result);
                }

                return BadRequest(result);
            }
            return Ok(result);
        }
        
        // PUT: api/departments/set-manager
        [HttpPut("set-manager")]
        public async Task<ActionResult<ApiResponse<bool>>> SetDepartmentManager(
            [FromBody] SetDepartmentManagerDto dto)
        {
            var result = await _departmentService.SetDepartmentManagerAsync(
                dto.DepartmentId,
                dto.ManagerEmployeeId);

            return Ok(result);
        }

        // PUT: api/departments/set-parent
        [HttpPut("set-parent")]
        public async Task<ActionResult<ApiResponse<bool>>> SetParentDepartment(
            [FromBody] SetParentDepartmentDto dto)
        {
            var result = await _departmentService.SetParentDepartmentAsync(
                dto.DepartmentId,
                dto.ParentDepartmentId);

            return Ok(result);
        }
    }
}
