using AutoMapper;
using HrManagmentSystem_Shared.Enum.Recruitment;
using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Services;
using HrMangmentSystem_Domain.Entities.Recruitment;
using HrMangmentSystem_Dto.DTOs.Job.Appilcation;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace HrMangmentSystem_Application.Implementation.Services
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IGenericRepository<JobApplication, int> _jobApplicationRepository;
        private readonly IGenericRepository<JobPosition, int> _jobPositionRepository;
        private readonly IGenericRepository<DocumentCv, int> _documentCvRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<JobApplicationService> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ICurrentUser _currentUser;
        private readonly ICvRankingService _cvRankingService;
        public JobApplicationService(
            IGenericRepository<JobApplication, int> jobApplicationRepository,
            IGenericRepository<JobPosition, int> jobPositionRepository,
            IGenericRepository<DocumentCv, int> documentCvRepository,
            IMapper mapper,
            ILogger<JobApplicationService> logger,
            IStringLocalizer<SharedResource> localizer,
            ICurrentUser currentUser,
            ICvRankingService cvRankingService
            )
        {
            _jobApplicationRepository = jobApplicationRepository;
            _jobPositionRepository = jobPositionRepository;
            _documentCvRepository = documentCvRepository;
            _mapper = mapper;
            _logger = logger;
            _localizer = localizer;
            _currentUser = currentUser;
            _cvRankingService = cvRankingService;
        }

        public async Task<ApiResponse<JobApplicationDto>> ApplyAsync(int jobPositionId, CreateJobApplicationDto createJobApplicationDto)
        {
            try
            {
                var jobPosition = await _jobPositionRepository.GetByIdAsync(jobPositionId);
                if (jobPosition is null)
                {
                    _logger.LogWarning($"Apply JobApplication: Job Position {jobPositionId} not found");
                    return ApiResponse<JobApplicationDto>.Fail(_localizer["JobPosition_NotFound", jobPositionId]);
                }
                if (!jobPosition.IsActive)
                {
                    _logger.LogWarning($"Apply JobApplication: JobPosition {jobPositionId} is not active");
                    return ApiResponse<JobApplicationDto>.Fail(_localizer["JobApplication_PositionClosed"]);
                }
                if (createJobApplicationDto.DocumentCvId <= 0)
                {
                    _logger.LogWarning("Apply JobApplication: DocumentCvId is required");
                    return ApiResponse<JobApplicationDto>.Fail(_localizer["JobApplication_CvRequired"]);
                }
                var documentCv = await _documentCvRepository.GetByIdAsync(createJobApplicationDto.DocumentCvId);
                if (documentCv is null)
                {
                    _logger.LogWarning($"Apply JobApplication: DocumentCv {createJobApplicationDto.DocumentCvId} not found");
                    return ApiResponse<JobApplicationDto>.Fail(_localizer["JobApplication_CvNotFound", createJobApplicationDto.DocumentCvId]);
                }
                var existing = await _jobApplicationRepository.FindAsync(a =>
                    a.JobPositionId == jobPositionId &&
                    a.DocumentCvId == createJobApplicationDto.DocumentCvId);

                if (existing.Any())
                {
                    _logger.LogInformation($"Apply JobApplication: CV {createJobApplicationDto.DocumentCvId} already applied for JobPosition {jobPositionId}");

                    return ApiResponse<JobApplicationDto>.Fail(
                        _localizer["JobApplication_AlreadyApplied"]);
                }
                var application = new JobApplication
                {
                    JobPositionId = jobPositionId,
                    DocumentCvId = createJobApplicationDto.DocumentCvId,
                    Notes = createJobApplicationDto.Notes,
                    Status = JobApplicationStatus.New,
                    AppliedAt = DateTime.Now,
                    MatchScore = null
                };
                await _jobApplicationRepository.AddAsync(application);
                await _jobApplicationRepository.SaveChangesAsync();

                var rankResult = await _cvRankingService.CalculateMatchScoreAsync(application.Id);
                application.MatchScore = rankResult.Data;
                if (!rankResult.Success)
                { 
                    _logger.LogWarning("JobApplication created but ranking failed. JobAppId={Id}, Msg={Msg}",
                        application.Id, rankResult.Message);
                }
                else
                {
                    _logger.LogInformation("JobApplication ranked successfully. JobAppId={Id}, Score={Score}",
                        application.Id, rankResult.Data);
                }

                var resultDto = _mapper.Map<JobApplicationDto>(application);

                _logger.LogInformation($"Apply JobApplication: Application {application.Id} created for JobPosition {jobPositionId}");

                return ApiResponse<JobApplicationDto>.Ok(resultDto, _localizer["JobApplication_Created"]);

                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Apply JobApplication: Unexpected error for JobPosition {jobPositionId}");

                return ApiResponse<JobApplicationDto>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }


        public async Task<ApiResponse<PagedResult<JobApplicationDto>>> GetByJobApplicationPagedAsync(int jobPositionId, PagedRequest request)
        {
            try
            {
                if (request.PageNumber <= 0)
                    request.PageNumber = 1;
                if (request.PageSize <= 0)
                    request.PageSize = 10;

                var jobPosition = await _jobPositionRepository.GetByIdAsync(jobPositionId);
                if (jobPosition is null)
                {
                    _logger.LogWarning($"Get JobApplication: Job Position {jobPositionId} not found");
                    return ApiResponse<PagedResult<JobApplicationDto>>.Fail(_localizer["JobPosition_NotFound", jobPositionId]);
                }

                var query = _jobApplicationRepository.Query()
                    .Include(j => j.JobPosition)
                        .ThenInclude(jp => jp.Department)
                    .Include(j => j.ReviewedByEmployee)
                    .Where(j => j.JobPositionId == jobPositionId);

                if (!string.IsNullOrWhiteSpace(request.Term))
                {
                    var term = request.Term.Trim().ToLower();
                    query = query.Where(j =>
                                      !string.IsNullOrEmpty(j.JobPosition.Department.DeptName) &&
                                      j.JobPosition.Department.DeptName.ToLower().Contains(term));
                }

                if (!string.IsNullOrWhiteSpace(request.SortBy))
                {
                    var sort = request.SortBy.Trim().ToLower();

                    query = sort switch
                    {
                        "appliedat" => request.Desc
                             ? query.OrderByDescending(j => j.AppliedAt)
                             : query.OrderBy(j => j.AppliedAt),

                        "status" => request.Desc
                             ? query.OrderByDescending(j => j.Status)
                             : query.OrderBy(j => j.Status),

                        "matchscore" => request.Desc
                              ? query.OrderByDescending(j => j.MatchScore)
                             : query.OrderBy(j => j.MatchScore),

                        _ => request.Desc
                             ? query.OrderByDescending(j => j.AppliedAt)
                             : query.OrderBy(j => j.AppliedAt),

                    };
                }
                else
                {
                    query = query.OrderByDescending(j => j.AppliedAt);
                }
                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var dtoItems = _mapper.Map<List<JobApplicationDto>>(items);

                var paged = new PagedResult<JobApplicationDto>
                {
                    Items = dtoItems,
                    TotalCount = totalCount,
                    Page = request.PageNumber,
                    PageSize = request.PageSize

                };

                _logger.LogInformation($"Get JobApplications: Loaded page {request.PageNumber} for JobPosition {jobPositionId}");

                return ApiResponse<PagedResult<JobApplicationDto>>.Ok(paged, _localizer["JobApplication_ListLoaded"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get JobApplications: Unexpected error for JobPosition {jobPositionId}");

                return ApiResponse<PagedResult<JobApplicationDto>>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }


        public async Task<ApiResponse<JobApplicationDto>> ChangeStatusAsync(ChangeJobApplicationStatusDto changeJobApplicationStatusDto)
        {
            try
            {
                var application = await _jobApplicationRepository
                    .Query()
                    .Include(j => j.JobPosition)
                    .Include(j => j.DocumentCvId)
                    .FirstOrDefaultAsync(j => j.Id == changeJobApplicationStatusDto.JobApplicationId);

                if (application is null)
                {
                    _logger.LogWarning($"Change Status JobApplication : Application {changeJobApplicationStatusDto.JobApplicationId} not found");
                   return ApiResponse<JobApplicationDto>.Fail(_localizer["JobApplication_NotFound",changeJobApplicationStatusDto.JobApplicationId]);
                }

                var currentStatus = application.Status;
                var newStatus = changeJobApplicationStatusDto.Newstatus;

                if (currentStatus == newStatus)
                {
                    _logger.LogWarning($"ChangeStatus JobApplication: Application {changeJobApplicationStatusDto.JobApplicationId}" +
                        $" already in status {currentStatus}");

                    return ApiResponse<JobApplicationDto>.Fail(_localizer["JobApplication_StatusSame"]);
                }
                if(!IsValidStatusTransition(currentStatus, newStatus))
                {
                    _logger.LogWarning($"ChangeStatus JobApplaication : Invalid status transition {currentStatus} -> {newStatus} " +
                        $"for application {changeJobApplicationStatusDto.JobApplicationId}");
                    return ApiResponse<JobApplicationDto>.Fail(_localizer["JobApplication_InvalidTransition",
                        currentStatus.ToString(), newStatus.ToString()]);
                }
                application.Status = newStatus;
                application.AppliedAt = DateTime.Now;

                application.ReviewedByEmployeeId = _currentUser.EmployeeId;
                application.UpdatedBy = _currentUser.EmployeeId;
                application.UpdatedAt = DateTime.Now;
                
                if(!string.IsNullOrWhiteSpace(changeJobApplicationStatusDto.Notes))
                {
                    if (!string.IsNullOrWhiteSpace(changeJobApplicationStatusDto.Notes))
                    {
                        application.Notes = changeJobApplicationStatusDto.Notes.Trim();
                    }
                    else
                    {
                        application.Notes = application.Notes + Environment.NewLine +
                            $"[{DateTime.Now:yyyy-mm-dd HH:mm}] {changeJobApplicationStatusDto.Notes.Trim()}";
                    }

                }

                _jobApplicationRepository.Update(application);
                await _jobApplicationRepository.SaveChangesAsync();

                var resultDto = _mapper.Map<JobApplicationDto>(application);

                _logger.LogInformation($"ChangeStatus JobApplication : Application {changeJobApplicationStatusDto.JobApplicationId} " +
                    $"changed from {currentStatus} to {newStatus}");

                return ApiResponse<JobApplicationDto>.Ok(resultDto, _localizer["JobApplication_StatusChanged",
                    changeJobApplicationStatusDto.JobApplicationId, newStatus.ToString()]);
            }


            catch (Exception ex)
            {
                _logger.LogError(ex, $"ChangeStatus JobApplication: Unexpected error for Application {changeJobApplicationStatusDto.JobApplicationId}");

                return ApiResponse<JobApplicationDto>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }
      
        private bool IsValidStatusTransition(JobApplicationStatus current , JobApplicationStatus target)
        {
            if(current == target)
                return false;

            return current switch
            {
                JobApplicationStatus.New => target is JobApplicationStatus.UnderReview or JobApplicationStatus.Rejected,

                JobApplicationStatus.UnderReview => target is JobApplicationStatus.Shortlisted or JobApplicationStatus.Rejected,

                JobApplicationStatus.Shortlisted => target is JobApplicationStatus.Hired or JobApplicationStatus.Rejected,

                JobApplicationStatus.Rejected => false,

                JobApplicationStatus.Hired => false,

                _ => false
            };
        }
    }
}
