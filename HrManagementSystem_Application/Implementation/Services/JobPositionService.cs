using AutoMapper;
using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Services;
using HrMangmentSystem_Domain.Entities.Employees;
using HrMangmentSystem_Domain.Entities.Recruitment;
using HrMangmentSystem_Dto.DTOs.Job.Position;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace HrMangmentSystem_Application.Implementation.Services
{
    public class JobPositionService : IJobPositionService
    {
        private readonly IGenericRepository<JobPosition, int> _jobPositionRepository;
        private readonly IGenericRepository<Department, int> _departmentRepository;
        private readonly IGenericRepository<JobApplication, int> _applicationRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<JobPositionService> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ICurrentUser _currentUser;

        public JobPositionService(IGenericRepository<JobPosition, int> jobPositionRepository, IGenericRepository<Department, int> departmentRepository, IMapper mapper, ILogger<JobPositionService> logger, IStringLocalizer<SharedResource> Localizer, IGenericRepository<JobApplication, int> applicationRepository, ICurrentUser currentUser)
        {
            _jobPositionRepository = jobPositionRepository;
            _departmentRepository = departmentRepository;
            _mapper = mapper;
            _logger = logger;
            _localizer = Localizer;
            _applicationRepository = applicationRepository;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<JobPositionDto>> ChangeStatusAsync(ChangeJobPositionStatusDto changeJobPositionStatusDto)
        {
            try
            {
                var jobPosition = await _jobPositionRepository.GetByIdAsync(changeJobPositionStatusDto.Id);
                if (jobPosition is null)
                {
                    _logger.LogWarning($"Delete Job Position: Job position with Id {changeJobPositionStatusDto.Id} not found");
                    return ApiResponse<JobPositionDto>.Fail(_localizer["JobPosition_NotFound", changeJobPositionStatusDto.Id]);
                }

                if (jobPosition.IsActive == changeJobPositionStatusDto.IsActive)
                {
                    var noChangeDto = _mapper.Map<JobPositionDto>(jobPosition);

                    var messageKey = changeJobPositionStatusDto.IsActive
                        ? "JobPosition_AlreadyOpen"
                        : "JobPosition_AlreadyClosed";

                    _logger.LogInformation(
                        $"ChangeStatus JobPosition: Id {changeJobPositionStatusDto.Id} already in requested state IsActive = {changeJobPositionStatusDto.IsActive}");
                    return ApiResponse<JobPositionDto>.Ok(noChangeDto, _localizer[messageKey]);
                }

                jobPosition.IsActive = changeJobPositionStatusDto.IsActive;

                if (!changeJobPositionStatusDto.IsActive)
                {

                    if (!jobPosition.ClosingDate.HasValue)
                    {
                        jobPosition.ClosingDate = DateTime.Now;
                    }
                }
                else
                {
                    jobPosition.ClosingDate = null;
                }

                _jobPositionRepository.Update(jobPosition);

                await _jobPositionRepository.SaveChangesAsync();

                var resultDto = _mapper.Map<JobPositionDto>(jobPosition);
                var successKey = changeJobPositionStatusDto.IsActive
                  ? "JobPosition_Reopened"
                  : "JobPosition_Closed";


                _logger.LogInformation(
               $"JobPosition Id {changeJobPositionStatusDto.Id} status changed successfully to IsActive={changeJobPositionStatusDto.IsActive}");

                return ApiResponse<JobPositionDto>.Ok(resultDto, _localizer[successKey]);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
              $"Error occurred while changing job position status Id {changeJobPositionStatusDto.Id} to IsActive ={changeJobPositionStatusDto.Id}");

                return ApiResponse<JobPositionDto>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<JobPositionDto>> CreateJobPositionsAsync(CreateJobPositionDto createJobPositionDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createJobPositionDto.Title))
                {
                    _logger.LogWarning("Create Job position : Title is required");
                    return ApiResponse<JobPositionDto>.Fail(_localizer["JobPosition_TitleRequired"]);
                }

                if (string.IsNullOrWhiteSpace(createJobPositionDto.Requirements))
                {
                    _logger.LogWarning("Create Job position : Requirement is required");
                    return ApiResponse<JobPositionDto>.Fail(_localizer["JobPosition_RequirementRequired"]);
                }
                if (createJobPositionDto.ClosingDate.HasValue && createJobPositionDto.ClosingDate.Value.Date < DateTime.Now.Date)
                {
                    _logger.LogWarning("Create JobPosition : Closing date is in the past");
                    return ApiResponse<JobPositionDto>.Fail(_localizer["JobPosition_ClosingDateInvalid"]);
                }
                var department = await _departmentRepository.GetByIdAsync(createJobPositionDto.DepartmentId);
                if (department is null)
                {
                    _logger.LogWarning($"Create JobPosition : Department {createJobPositionDto.DepartmentId} not found");
                    return ApiResponse<JobPositionDto>.Fail(_localizer["JobPosition_DepartmentNotFound", createJobPositionDto.DepartmentId]);
                }
                var jobPosition = _mapper.Map<JobPosition>(createJobPositionDto);

                await _jobPositionRepository.AddAsync(jobPosition);
                await _jobPositionRepository.SaveChangesAsync();

                var jobPositionDto = _mapper.Map<JobPositionDto>(jobPosition);

                _logger.LogInformation($"JobPosition created successfully with Id {jobPosition.Id} in Department {department.Id}");
                return ApiResponse<JobPositionDto>.Ok(jobPositionDto, _localizer["JobPosition_Created"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating job position");

                return ApiResponse<JobPositionDto>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }


        public async Task<ApiResponse<bool>> DeleteJobPositionsAsync(int jobPositionId)
        {
            try
            {
                var jobPosition = await _jobPositionRepository.GetByIdAsync(jobPositionId);
                if (jobPosition is null)
                {
                    _logger.LogWarning($"Delete Job Position: Job position with Id {jobPositionId} not found");
                    return ApiResponse<bool>.Fail(_localizer["JobPosition_NotFound", jobPositionId]);
                }

                var applications = await _applicationRepository.FindAsync(ja => ja.JobPositionId == jobPositionId);
                if (applications.Any())
                {
                    _logger.LogWarning($"Delete Job Position: Job position {jobPositionId} has applications and can not be deleted");
                    return ApiResponse<bool>.Fail(_localizer["JobPosition_HasApplications", jobPositionId]);
                }

                await _jobPositionRepository.DeleteAsync(jobPositionId);
                await _jobPositionRepository.SaveChangesAsync();

                var deletedByEmployee = _currentUser.EmployeeId;
                _logger.LogInformation($"JobPosition with Id {jobPositionId} deleted successfully by  {deletedByEmployee}");

                return ApiResponse<bool>.Ok(true, _localizer["JobPosition_Deleted", jobPositionId]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting job position");
                return ApiResponse<bool>.Fail(_localizer["Generic_UnexpectedError"]);

            }
        }

        public async Task<ApiResponse<JobPositionDto?>> GetJobPositionsByIdAsync(int jobPositionId)
        {
            try
            {
                var jobPosition = await _jobPositionRepository.GetByIdAsync(jobPositionId);
                if (jobPosition is null)
                {
                    _logger.LogWarning($"Get JobPosition By Id: Id {jobPositionId} not found");
                    return ApiResponse<JobPositionDto?>.Fail(_localizer["JobPosition_NotFound", jobPositionId]);
                }
                var jobPositionDto = _mapper.Map<JobPositionDto>(jobPosition);

                _logger.LogInformation($"JobPosition with Id {jobPositionId} get successfully ");
                return ApiResponse<JobPositionDto?>.Ok(jobPositionDto, _localizer["JobPosition_DetailsLoaded"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving job position by Id {jobPositionId}");
                return ApiResponse<JobPositionDto?>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<PagedResult<JobPositionDto>>> GetJobPositionsPagedAsync(PagedRequest request)
        {
            try
            {
                if (request.PageNumber <= 0)
                    request.PageNumber = 1;

                if (request.PageSize <= 0)
                    request.PageSize = 10;

                var query = _jobPositionRepository.Query();

                if (!string.IsNullOrWhiteSpace(request.Term)) //Search (Term) by Req || deptName || Title 
                {
                    var term = request.Term.Trim().ToLower();

                    query = query.Where(j =>
                        !string.IsNullOrEmpty(j.Requirements) && j.Requirements.ToLower().Contains(term) ||
                        !string.IsNullOrEmpty(j.Title) && j.Title.ToLower().Contains(term) ||
                        !string.IsNullOrEmpty(j.Department.DeptName) && j.Department.DeptName.ToLower().Contains(term));
                }

                if (!string.IsNullOrWhiteSpace(request.SortBy))
                {
                    var sort = request.SortBy.Trim().ToLower();

                    query = sort switch
                    {
                        "title" => request.Desc
                            ? query.OrderByDescending(jp => jp.Title)
                            : query.OrderBy(jp => jp.Title),

                        "posteddate" => request.Desc
                            ? query.OrderByDescending(jp => jp.PostedDate)
                            : query.OrderBy(jp => jp.PostedDate),

                        "isactive" => request.Desc
                            ? query.OrderByDescending(jp => jp.IsActive)
                            : query.OrderBy(jp => jp.IsActive),

                        _ => request.Desc
                            ? query.OrderByDescending(jp => jp.PostedDate)
                            : query.OrderBy(jp => jp.PostedDate)
                    };
                }
                else
                {
                    // default sort
                    query = query.OrderByDescending(jp => jp.PostedDate);
                }
                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var dtoItems = _mapper.Map<List<JobPositionDto>>(items);

                var result = new PagedResult<JobPositionDto>
                {
                    Items = dtoItems,
                    TotalCount = totalCount,
                    Page = request.PageNumber,
                    PageSize = request.PageSize
                };

                _logger.LogInformation(
                    $"Retrieved job positions page {request.PageNumber} with page size {request.PageSize}");

                return ApiResponse<PagedResult<JobPositionDto>>.Ok(result, _localizer["JobPosition_ListLoaded"]);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving job positions with pagination");
                return ApiResponse<PagedResult<JobPositionDto>>.Fail(_localizer["Generic_UnexpectedError"]);
            }

        }

        public async Task<ApiResponse<JobPositionDto>> UpdateJobPositionsAsync(UpdateJobPositionDto updateJobPositionDto)
        {
            try
            {
                var jobPosition = await _jobPositionRepository.GetByIdAsync(updateJobPositionDto.Id);
                if (jobPosition is null)
                {
                    _logger.LogWarning($"Delete Job Position: Job position with Id {updateJobPositionDto.Id} not found");
                    return ApiResponse<JobPositionDto>.Fail(_localizer["JobPosition_NotFound", updateJobPositionDto.Id]);
                }
                if (updateJobPositionDto.ClosingDate.HasValue && updateJobPositionDto.ClosingDate.Value.Date < DateTime.Now.Date)
                {
                    _logger.LogWarning("Update JobPosition: Closing date is in the past");
                    return ApiResponse<JobPositionDto>.Fail(_localizer["JobPosition_ClosingDateInvalid"]);
                }
                _mapper.Map(updateJobPositionDto, jobPosition);

                ///
                if (updateJobPositionDto.IsActive.HasValue && updateJobPositionDto.IsActive == false)
                {
                    if (!jobPosition.IsActive)
                    {
                        jobPosition.ClosingDate = DateTime.Now;
                    }
                }

                _jobPositionRepository.Update(jobPosition);
                await _jobPositionRepository.SaveChangesAsync();

                var jobPositionDto = _mapper.Map<JobPositionDto>(jobPosition);



                _logger.LogInformation($"JobPosition Id {updateJobPositionDto.Id} updated successfully");

                return ApiResponse<JobPositionDto>.Ok(jobPositionDto, _localizer["JobPosition_Updated"]);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating job position");
                return ApiResponse<JobPositionDto>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }
    }
}
