using Microsoft.AspNetCore.Http;

namespace BGVSystem.Application.Interfaces;

public class FileUploadResult
{
    public string ObjectPath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(IFormFile file, int candidateId, string documentType);
    Task<Stream> DownloadAsync(string objectPath);
    Task DeleteAsync(string objectPath);
    Task<bool> ExistsAsync(string objectPath);
    Task<string> GenerateSignedUrlAsync(string objectPath, int expiryMinutes = 15);
}
