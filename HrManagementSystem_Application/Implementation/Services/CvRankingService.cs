using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Config;
using HrMangmentSystem_Application.Interfaces.OpenAi;
using HrMangmentSystem_Application.Interfaces.Services;
using HrMangmentSystem_Domain.Entities.Recruitment;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace HrMangmentSystem_Application.Implementation.Services
{
    public class CvRankingService : ICvRankingService
    {

        private readonly IGenericRepository<JobApplication, int> _jobApplicationRepository;
        private readonly IGenericRepository<JobPosition, int> _jobPositionRepository;
        private readonly IGenericRepository<DocumentCv, int> _documentCvRepository;
        private readonly ILogger<CvRankingService> _logger;
        private readonly IStringLocalizer<CvRankingService> _localizer;
        private readonly IOpenAiCvScoringClient _openAiCvScoringClient;

        public CvRankingService(
            IGenericRepository<JobApplication, int> jobApplicationRepository,
            IGenericRepository<JobPosition, int> jobPositionRepository,
            IGenericRepository<DocumentCv, int> documentCvRepository,
            ILogger<CvRankingService> logger,
            IStringLocalizer<CvRankingService> localizer,
            IConfiguration configuration,
             IOptions<FileStorageOptions> options,
            IHttpClientFactory httpClientFactory,
            IOpenAiCvScoringClient openAiCvScoringClient)
        {
            _jobApplicationRepository = jobApplicationRepository;
            _jobPositionRepository = jobPositionRepository;
            _documentCvRepository = documentCvRepository;
            _logger = logger;
            _localizer = localizer;
            _openAiCvScoringClient = openAiCvScoringClient;
        }
        public async Task<ApiResponse<double>> CalculateMatchScoreAsync(int jobApplicationId)
        {
            try
            {
                var jobApplication = _jobApplicationRepository.Query()
                    .FirstOrDefault(ja => ja.Id == jobApplicationId);

                if (jobApplication is null)
                {
                    _logger.LogWarning("CV Ranking: JobApplication {Id} not found", jobApplicationId);
                    return ApiResponse<double>.Fail(_localizer["JobApplication_NotFound"]);
                }
                var jobPosition = _jobPositionRepository.Query()
                    .FirstOrDefault(jp => jp.Id == jobApplication.JobPositionId);
                if (jobPosition is null)
                {
                    _logger.LogWarning("CV Ranking: JobPosition {Id} not found for JobApplication {JobApplicationId}", jobApplication.JobPositionId, jobApplicationId);
                    return ApiResponse<double>.Fail(_localizer["JobPosition_NotFound"]);
                }

                var cv = _documentCvRepository.Query()
                    .FirstOrDefault(cv => cv.Id == jobApplication.DocumentCvId);
                if (cv is null)
                {
                    _logger.LogWarning("CV Ranking: DocumentCv {Id} not found for JobApplication {JobApplicationId}", jobApplication.DocumentCvId, jobApplicationId);
                    return ApiResponse<double>.Fail(_localizer["DocumentCv_NotFound"]);
                }

                var filePath = cv.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString());
                var physicalPath = Path.IsPathRooted(filePath)
                    ? filePath
                    : Path.Combine(Directory.GetCurrentDirectory(), filePath);

                _logger.LogInformation("CV Ranking paths: FilePath={FilePath}, CurrentDir={CurrentDir}, Physical={Physical}",  
                    cv.FilePath, Directory.GetCurrentDirectory(), physicalPath);

                if (!File.Exists(physicalPath))
                {
                    _logger.LogWarning("CV Ranking: CV file not found at path {PhysicalPath} for DocumentCvId {DocumentCvId}", physicalPath, cv.Id);
                    return ApiResponse<double>.Fail(_localizer["DocumentCv_FileNotFound"]);
                }
                // Simulate CV parsing and matching logic
                var jobText = $"{jobPosition.Title}\n\n{jobPosition.Requirements}"; 

                var score = await  _openAiCvScoringClient.GetScoreAsync(jobText, physicalPath);

                jobApplication.MatchScore = score;
                await _jobApplicationRepository.SaveChangesAsync();

                return ApiResponse<double>.Ok(score, _localizer["CvRanking_Scored"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating match score for JobApplicationId: {JobApplicationId}", jobApplicationId);
                return ApiResponse<double>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }
    }
}
