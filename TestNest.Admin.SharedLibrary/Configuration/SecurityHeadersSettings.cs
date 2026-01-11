namespace TestNest.Admin.SharedLibrary.Configuration;

public class SecurityHeadersSettings
{
    public const string SectionName = "SecurityHeaders";

    public bool EnableHsts { get; set; } = true;
    public int HstsMaxAgeInDays { get; set; } = 365;
    public bool HstsIncludeSubDomains { get; set; } = true;
    public bool HstsPreload { get; set; } = false;
    public string ContentSecurityPolicy { get; set; } = "default-src 'self'";
    public string XContentTypeOptions { get; set; } = "nosniff";
    public string XFrameOptions { get; set; } = "DENY";
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";
    public string PermissionsPolicy { get; set; } = "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";
}
