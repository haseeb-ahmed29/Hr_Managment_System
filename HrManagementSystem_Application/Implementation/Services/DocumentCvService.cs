using AutoMapper;
using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Services;
using HrMangmentSystem_Domain.Entities.Recruitment;
using HrMangmentSystem_Dto.DTOs.Job.Appilcation;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace HrMangmentSystem_Application.Implementation.Services
{
    public class DocumentCvService : IDocumentCvService
    {
        private readonly IGenericRepository<DocumentCv , int> _documentCvRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentCvService> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;
      

        public DocumentCvService(
            IGenericRepository<DocumentCv, int> documentCvRepository,
            IMapper mapper,
            ILogger<DocumentCvService> logger,
            IStringLocalizer<SharedResource> localizer
            )
        {
            _documentCvRepository = documentCvRepository;
            _mapper = mapper;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<ApiResponse<DocumentCvDto>> CreateAsync(CreateDocumentCvDto createDocumentCvDto , string filePath ) 
        {
            try
            {
                if ( createDocumentCvDto.FileSize <= 0 ) 
                {
                    _logger.LogWarning("Create DocumentCv : Empty file");
                    return ApiResponse<DocumentCvDto>.Fail(_localizer["DocumentCv_EmptyFile"]);
                }

                if (string.IsNullOrWhiteSpace(createDocumentCvDto.CandidateName) ||
                        string.IsNullOrWhiteSpace(createDocumentCvDto.CandidateEmail))
                {
                    _logger.LogWarning("Create DocumentCv: CandidateName or CandidateEmail is missing");
                    return ApiResponse<DocumentCvDto>.Fail( _localizer["DocumentCv_CandidateInfoRequired"]);
                }

                var document = _mapper.Map<DocumentCv>(createDocumentCvDto);
                
                document.FilePath = filePath;

                await _documentCvRepository.AddAsync(document);
                await _documentCvRepository.SaveChangesAsync();

                var result = _mapper.Map<DocumentCvDto>(document);

                _logger.LogInformation("Create DocumentCv : Uploaded document cv it Success ");
                return ApiResponse<DocumentCvDto>.Ok(result , _localizer["DocumentCv_Uploaded"]);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create DocumentCv: Unexpected error");
                return ApiResponse<DocumentCvDto>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }
    }
}
