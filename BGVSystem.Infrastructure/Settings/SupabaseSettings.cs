namespace BGVSystem.Infrastructure.Settings;

public class SupabaseSettings
{
    public string ProjectUrl { get; set; } = string.Empty;
    public string ServiceRoleKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = "candidate-documents";
    public int SignedUrlExpiryMinutes { get; set; } = 15;
}
