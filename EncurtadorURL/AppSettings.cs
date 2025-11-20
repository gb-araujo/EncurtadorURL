namespace EncurtadorURL;

public class AppSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}
