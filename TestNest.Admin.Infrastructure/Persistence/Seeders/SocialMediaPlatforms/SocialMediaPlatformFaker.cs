using Bogus;
using TestNest.Admin.Domain.SocialMedias;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders.SocialMediaPlatforms;

public sealed class SocialMediaPlatformFaker : Faker<SocialMediaPlatform>
{
    private readonly List<string> _popularPlatforms =
    [
        "Facebook", "Twitter", "Instagram", "LinkedIn",
        "YouTube", "TikTok", "Pinterest", "Snapchat"
    ];

    public SocialMediaPlatformFaker()
        => CustomInstantiator(f =>
            {
                for (int i = 0; i < 5; i++)
                {
                    (string name, string url) = GenerateValidPlatform(f);
                    Result<SocialMediaPlatform> result = SocialMediaPlatform.Create(
                        SocialMediaName.Create(name, url).Value!
                    );

                    if (result.IsSuccess)
                    {
                        return result.Value!;
                    }
                }
                return SocialMediaPlatform.Empty();
            });

    private (string Name, string Url) GenerateValidPlatform(Faker f)
    {
        bool usePredefined = f.Random.Bool(0.7f);

        if (usePredefined && _popularPlatforms.Count != 0)
        {
            string platform = f.PickRandom(_popularPlatforms);
            return (
                platform,
                GeneratePlatformUrl(platform, f)
            );
        }

        string name = f.Internet.DomainWord()
            .Replace("-", "_")
            .Replace(" ", "_")
            .TrimToMax(45);

        return (
            name,
            GeneratePlatformUrl(name, f)
        );
    }

    private static string GeneratePlatformUrl(string platformName, Faker f)
    {
        _ = f.Internet.DomainName();
        return f.Random.Bool(0.8f)
            ? $"https://www.{platformName.ToLowerInvariant()}.com"
            : f.Internet.UrlWithPath();
    }
}

public static class StringExtensions
{
    public static string TrimToMax(this string input, int maxLength) =>
        input.Length > maxLength ? input[..maxLength] : input;
}
