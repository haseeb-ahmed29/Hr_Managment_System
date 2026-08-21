using HrMangmentSystem_Application.Config;
using HrMangmentSystem_Application.Interfaces.FileStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HrMangmentSystem_Application.Implementation.FileStorage
{
    public class FileStorageService : IFileStorageService
    {
       
        private readonly ILogger<FileStorageService> _logger;
        private readonly string _cvRootPathOption;

        public FileStorageService(
            IOptions<FileStorageOptions> options,
            ILogger<FileStorageService> logger)
        {
            _cvRootPathOption = options.Value.CvRootPath;
            _logger = logger;
        }

        public async Task<string> SaveCvAsync(IFormFile formFile, string candidateName, string positionTitle)
        {
            if (formFile == null || formFile.Length <= 0)
            {
                _logger.LogWarning("Document CV: Empty file uploaded");
                throw new InvalidOperationException("DocumentCv_EmptyFile");
            }

            var uploadsRoot = Path.IsPathRooted(_cvRootPathOption)
                ? _cvRootPathOption
                : Path.Combine(Directory.GetCurrentDirectory(), _cvRootPathOption);

            Directory.CreateDirectory(uploadsRoot);

            var extension = Path.GetExtension(formFile.FileName);

            var isPdfExtension = string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase);

            //  var isPdfContentType = string.Equals(formFile.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase);

            if (!isPdfExtension)
            {
                _logger.LogWarning("Document CV : A document was uploaded that is not PDF ");
                throw new InvalidOperationException("DocumentCv_OnlyPdfAllowed");
            }

            var candidatePart = CleanFilePart(candidateName);
            var positionPart = CleanFilePart(positionTitle);
            var shortGuid = Guid.NewGuid().ToString("N").Substring(0, 8);

            var fileNameOnDisk = $"{candidatePart}_{positionPart}_{shortGuid}{extension}";
            var physicalPath = Path.Combine(uploadsRoot, fileNameOnDisk);

            using (var stream = File.Create(physicalPath))
            {
                await formFile.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("Uploads", "Cvs", fileNameOnDisk)
                .Replace("\\", "/");

            return relativePath;

        }

        private static string CleanFilePart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var invalid = Path.GetInvalidFileNameChars();
            var cleanedChars = value
                .Where(c => !invalid.Contains(c)) // remove invalid chars
                .ToArray();

            var cleaned = new string(cleanedChars).Replace(" ", "");

            return cleaned;
        }
    }
}