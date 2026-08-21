using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.FileStorage;
using HrMangmentSystem_Application.Interfaces.Services;
using HrMangmentSystem_Domain.Entities.Recruitment;
using HrMangmentSystem_Dto.DTOs.Job.Appilcation;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

namespace HrManagmentSystem_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentCvController : ControllerBase
    {
        private readonly IGenericRepository<JobPosition, int> _jobPositionRepository;
        private readonly ICurrentTenant _currentTenant;
        private readonly IDocumentCvService _documentCvService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public DocumentCvController
            (IDocumentCvService documentCvService,
            IStringLocalizer<SharedResource> localizer,
            ICurrentTenant currentTenant,
            IGenericRepository<JobPosition, int> jobPositionRepository)
        {
            _documentCvService = documentCvService;
            _localizer = localizer;
            _currentTenant = currentTenant;
            _jobPositionRepository = jobPositionRepository;
        }

        [HttpPost("{jobPositionId:int}/cv/upload")]
        [AllowAnonymous]
        [EnableRateLimiting("RequestUploadPolicy")]
        public async Task<ActionResult<ApiResponse<DocumentCvDto>>> UploadDocumentCv(
            int jobPositionId,
            [FromForm] IFormFile cvFile,
            [FromForm] string candidateName,
            [FromForm] string candidateEmail,
            [FromForm] string? candidatePhone,
            [FromServices] IFileStorageService fileStorageService)

        {
            var jobPosition = await _jobPositionRepository.GetByIdAsync(jobPositionId);
            if (jobPosition is null)
            {
                return ApiResponse<DocumentCvDto>.Fail(_localizer["JobPosition_NotFound", jobPositionId]);
            }
            if (!jobPosition.IsActive)
            {
                return ApiResponse<DocumentCvDto>.Fail(_localizer["JobPosition_NotActive"]);
            }
            _currentTenant.SetTenant(jobPosition.TenantId);


            if (cvFile == null || cvFile.Length <= 0)
            {
                return ApiResponse<DocumentCvDto>.Fail(_localizer["DocumentCv_EmptyFile"]);
            }
            string relativePath;
            
            try
            {
                relativePath = await fileStorageService.SaveCvAsync(cvFile, candidateName, jobPosition.Title);
            }
            catch (InvalidOperationException ex) when (ex.Message == "DocumentCv_OnlyPdfAllowed")
            {
                return ApiResponse<DocumentCvDto>.Fail(_localizer["DocumentCv_OnlyPdfAllowed"]);
            }
            catch(InvalidOperationException ex) when (ex.Message == "DocumentCv_EmptyFile")
            {
                return ApiResponse<DocumentCvDto>.Fail(_localizer["DocumentCv_EmptyFile"]);
            }

            var createDto = new CreateDocumentCvDto
            {
                FileName = cvFile.FileName,
                ContentType = cvFile.ContentType,
                FileSize = cvFile.Length,
                CandidateName = candidateName,
                CandidateEmail = candidateEmail,
                CandidatePhone = candidatePhone
            };
            var result = await _documentCvService.CreateAsync(createDto, relativePath);

            return Ok(result);
        }

    }
}
