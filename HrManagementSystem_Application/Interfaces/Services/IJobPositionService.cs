using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Dto.DTOs.Job.Position;

namespace HrMangmentSystem_Application.Interfaces.Services
{
    public interface IJobPositionService
    {
        Task<ApiResponse<JobPositionDto>> CreateJobPositionsAsync(CreateJobPositionDto createJobPositionDto);

        Task<ApiResponse<JobPositionDto?>> GetJobPositionsByIdAsync(int jobPositionId);

        Task<ApiResponse<PagedResult<JobPositionDto>>> GetJobPositionsPagedAsync(PagedRequest request);

        Task<ApiResponse<JobPositionDto>> UpdateJobPositionsAsync(UpdateJobPositionDto updateJobPositionDto);

        Task<ApiResponse<bool>> DeleteJobPositionsAsync(int jobPositionId);


        Task<ApiResponse<JobPositionDto>> ChangeStatusAsync(ChangeJobPositionStatusDto changeJobPositionStatusDto);
    }
}
