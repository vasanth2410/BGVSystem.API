using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BGVSystem.Application.Interfaces;
using BGVSystem.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BGVSystem.Infrastructure.Services;

public class SupabaseStorageService : IFileStorageService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseSettings _settings;
    private readonly ILogger<SupabaseStorageService> _logger;

    public SupabaseStorageService(
        HttpClient httpClient,
        IOptions<SupabaseSettings> settings,
        ILogger<SupabaseStorageService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<FileUploadResult> UploadAsync(IFormFile file, int candidateId, string documentType)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File cannot be empty.", nameof(file));
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        
        // Clean folder name from document type (e.g. "PAN Card" -> "PAN")
        var cleanDocType = string.IsNullOrWhiteSpace(documentType) 
            ? "General" 
            : documentType.Replace(" ", "").Replace("Card", "").Trim();

        // Path structure: CandidateId/DocumentType/Guid.ext
        var objectPath = $"{candidateId}/{cleanDocType}/{uniqueFileName}";

        try
        {
            var url = BuildStorageUrl($"object/{_settings.BucketName}/{objectPath}");
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            ApplyAuthHeaders(request);
            request.Headers.Add("x-upsert", "true");

            using var fileStream = file.OpenReadStream();
            using var content = new StreamContent(fileStream);
            content.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(extension));
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Supabase Storage Upload Failure for {ObjectPath}. Status: {StatusCode}, Error: {Error}",
                    objectPath, response.StatusCode, errorContent);
                throw new InvalidOperationException($"Supabase Storage upload failed: {response.StatusCode} - {errorContent}");
            }

            _logger.LogInformation("Supabase Storage Upload Success for Candidate {CandidateId}, File: {ObjectPath}",
                candidateId, objectPath);

            return new FileUploadResult
            {
                ObjectPath = objectPath,
                FileName = uniqueFileName,
                OriginalFileName = file.FileName,
                FileType = extension,
                FileSize = file.Length
            };
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Supabase Storage Upload Exception for Candidate {CandidateId}, File {OriginalFileName}",
                candidateId, file.FileName);
            throw;
        }
    }

    public async Task<Stream> DownloadAsync(string objectPath)
    {
        if (string.IsNullOrWhiteSpace(objectPath))
        {
            throw new ArgumentException("Object path cannot be empty.", nameof(objectPath));
        }

        try
        {
            var url = BuildStorageUrl($"object/authenticated/{_settings.BucketName}/{objectPath}");
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            ApplyAuthHeaders(request);

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                // Fallback to unauthenticated or direct object path
                var fallbackUrl = BuildStorageUrl($"object/{_settings.BucketName}/{objectPath}");
                using var fallbackRequest = new HttpRequestMessage(HttpMethod.Get, fallbackUrl);
                ApplyAuthHeaders(fallbackRequest);

                response = await _httpClient.SendAsync(fallbackRequest, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Supabase Storage Download Failure for {ObjectPath}. Status: {StatusCode}",
                        objectPath, response.StatusCode);
                    throw new FileNotFoundException($"File not found in Supabase Storage: {objectPath}");
                }
            }

            var memoryStream = new MemoryStream();
            await (await response.Content.ReadAsStreamAsync()).CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            _logger.LogError(ex, "Supabase Storage Download Exception for {ObjectPath}", objectPath);
            throw;
        }
    }

    public async Task DeleteAsync(string objectPath)
    {
        if (string.IsNullOrWhiteSpace(objectPath)) return;

        try
        {
            var url = BuildStorageUrl($"object/{_settings.BucketName}/{objectPath}");
            using var request = new HttpRequestMessage(HttpMethod.Delete, url);
            ApplyAuthHeaders(request);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Supabase Storage Delete Success for {ObjectPath}", objectPath);
            }
            else
            {
                _logger.LogWarning("Supabase Storage Delete returned status {StatusCode} for {ObjectPath}",
                    response.StatusCode, objectPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Supabase Storage Delete Failure for {ObjectPath}", objectPath);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string objectPath)
    {
        if (string.IsNullOrWhiteSpace(objectPath)) return false;

        try
        {
            var url = BuildStorageUrl($"object/info/authenticated/{_settings.BucketName}/{objectPath}");
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            ApplyAuthHeaders(request);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Supabase Storage Exists check error for {ObjectPath}", objectPath);
            return false;
        }
    }

    public async Task<string> GenerateSignedUrlAsync(string objectPath, int expiryMinutes = 15)
    {
        if (string.IsNullOrWhiteSpace(objectPath)) return string.Empty;

        try
        {
            var expirySeconds = (expiryMinutes > 0 ? expiryMinutes : _settings.SignedUrlExpiryMinutes) * 60;
            var url = BuildStorageUrl($"object/sign/{_settings.BucketName}/{objectPath}");

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            ApplyAuthHeaders(request);

            var jsonBody = JsonSerializer.Serialize(new { expiresIn = expirySeconds });
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Signed URL generation failed for {ObjectPath}. Status: {StatusCode}", objectPath, response.StatusCode);
                throw new InvalidOperationException($"Failed to generate signed URL for {objectPath}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            
            if (doc.RootElement.TryGetProperty("signedURL", out var signedUrlProp))
            {
                var relativeSignedUrl = signedUrlProp.GetString();
                _logger.LogInformation("Generated Signed URL for {ObjectPath} with expiry {Minutes} mins", objectPath, expiryMinutes);
                
                if (relativeSignedUrl != null && relativeSignedUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    return relativeSignedUrl;
                }

                var baseUrl = _settings.ProjectUrl.TrimEnd('/');
                return $"{baseUrl}/storage/v1{relativeSignedUrl}";
            }

            throw new InvalidOperationException("Response JSON did not contain 'signedURL'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GenerateSignedUrl Failure for {ObjectPath}", objectPath);
            throw;
        }
    }

    private string BuildStorageUrl(string path)
    {
        var baseUrl = _settings.ProjectUrl.TrimEnd('/');
        return $"{baseUrl}/storage/v1/{path.TrimStart('/')}";
    }

    private void ApplyAuthHeaders(HttpRequestMessage request)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ServiceRoleKey);
        request.Headers.Add("apikey", _settings.ServiceRoleKey);
    }

    private static string GetContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _ => "application/octet-stream"
    };
}
