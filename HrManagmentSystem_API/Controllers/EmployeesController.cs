using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Services;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Dto.DTOs.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrManagmentSystem_API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleNames.HrAdmin +","+ RoleNames.SystemAdmin)]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeRoleService _employeeRoleService;

        public EmployeesController(IEmployeeService employeeService, IEmployeeRoleService employeeRoleService)
        {
            _employeeService = employeeService;
            _employeeRoleService = employeeRoleService;
        }

        // GET: api/Employees
        //[HttpGet]
        //public async Task<ActionResult<ApiResponse<List<EmployeeDto>>>> GetAllEmployees()
        //{
        //    var result = await _employeeService.GetAllEmployeesAsync();

        //    return  Ok(result);
        //}

        // GET: api/employees
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<EmployeeDto>>>> GetAllEmployees([FromQuery] PagedRequest request)
        {
            var result = await _employeeService.GetEmployeesPagedAsync(request);

            return Ok(result);
        }

        // GET: api/Employees/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetEmployeeById(Guid id)
        {
            var result = await _employeeService.GetEmployeeByIdAsync(id);
            if (result.Data is null || !result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        // POST: api/Employees
        [HttpPost]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> CreateEmployee([FromBody] CreateEmployeeDto createEmployeeDto) 
        {
            if (!ModelState.IsValid)
            {  // Validation errors
                var error = ModelState.ToDictionary(
                         k => k.Key,
                         v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                         );
                return BadRequest(ApiResponse<EmployeeDto>.Fail("Invalid data", error: error));
            }

            var result = await _employeeService.CreateEmployeeAsync(createEmployeeDto);

            if (!result.Success || result.Data is null)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetEmployeeById), new { id = result.Data!.Id }, result);
        }


        [HttpPut("{id:guid}")] // update : api/employees/{id}
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> UpdateEmployee( [FromBody] UpdateEmployeeDto updateEmployeeDto)
        {
            var result = await _employeeService.UpdateEmployeeAsync(updateEmployeeDto);

            if (!result.Success || result.Data is null)
            {
                if(result.Message?.Contains("not found" , StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }
     
        // DELETE: api/Employees/{id}
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteEmployee(Guid Id)
        {
           

            var result = await _employeeService.DeleteEmployeeAsync(Id);
            
            if (!result.Success)
            {
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(result);

                return BadRequest(result);
            }
            return Ok(result);
        }
        // PUT: api/employees/assign-manager
        [HttpPut("assign-manager")]
        public async Task<ActionResult<ApiResponse<bool>>> AssignManager( [FromBody] AssignManagerDto dto)
        {
            if (dto is null )
            {
                return BadRequest(ApiResponse<bool>.Fail("Invalid request."));
            }

            var result = await _employeeService.AssignManagerAsync(dto.EmployeeId, dto.ManagerId);
            return Ok(result);
        }

        // PUT: api/employees/update-role
        [HttpPut("update-role")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateEmployeeRole(
            [FromBody] UpdateEmployeeRoleDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.RoleName))
            {
                return BadRequest(ApiResponse<bool>.Fail("Invalid request."));
            }

            var result = await _employeeRoleService.UpdateRoleToEmployeeAsync(dto.EmployeeId, dto.RoleName);
            return Ok(result);
        }
    }
}
