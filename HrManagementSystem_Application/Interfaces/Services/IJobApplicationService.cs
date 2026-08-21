using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Dto.DTOs.Job.Appilcation;

namespace HrMangmentSystem_Application.Interfaces.Services
{
    public interface IJobApplicationService
    {
        Task<ApiResponse<JobApplicationDto>> ApplyAsync(int jobPositionId, CreateJobApplicationDto createJobApplicationDto);
        Task<ApiResponse<PagedResult<JobApplicationDto>>> GetByJobApplicationPagedAsync(int jobPositionId , PagedRequest request);

        Task<ApiResponse<JobApplicationDto>> ChangeStatusAsync (ChangeJobApplicationStatusDto changeJobApplicationStatusDto);
  
    }
}
