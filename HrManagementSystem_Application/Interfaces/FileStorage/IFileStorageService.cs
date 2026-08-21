using Microsoft.AspNetCore.Http;

namespace HrMangmentSystem_Application.Interfaces.FileStorage
{
    public interface IFileStorageService

    {
        Task<string> SaveCvAsync(IFormFile formFile , string candidateName , string positionTitle);
    }
}
