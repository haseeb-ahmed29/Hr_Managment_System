using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Dto.DTOs.Job.Appilcation;

namespace HrMangmentSystem_Application.Interfaces.Services
{
    public interface IDocumentCvService
    {
        Task<ApiResponse<DocumentCvDto>> CreateAsync(CreateDocumentCvDto createDocumentCvDto , string filePath);
    }
}
