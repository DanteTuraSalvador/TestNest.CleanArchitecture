using System.IO.Compression;

namespace TestNest.Admin.SharedLibrary.Configuration;

public class CompressionSettings
{
    public const string SectionName = "Compression";

    public bool Enabled { get; set; } = true;
    public bool EnableForHttps { get; set; } = true;
    public CompressionLevel Level { get; set; } = CompressionLevel.Fastest;
    public string[] MimeTypes { get; set; } =
    [
        "application/json",
        "application/xml",
        "text/plain",
        "text/json",
        "text/xml",
        "text/html",
        "application/javascript",
        "text/css"
    ];
}
