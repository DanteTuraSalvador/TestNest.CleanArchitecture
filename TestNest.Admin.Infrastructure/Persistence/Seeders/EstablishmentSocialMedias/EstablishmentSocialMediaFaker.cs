using Bogus;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.Domain.SocialMedias;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders.EstablishmentSocialMedias;

public sealed class EstablishmentSocialMediaFaker : Faker<EstablishmentSocialMedia>
{
    public EstablishmentSocialMediaFaker(
        List<Establishment> establishments,
        List<SocialMediaPlatform> socialMediaPlatforms)
    {
        _ = CustomInstantiator(f =>
        {
            Establishment establishment = f.PickRandom(establishments);
            SocialMediaPlatform platform = f.PickRandom(socialMediaPlatforms);

            string accountName = GenerateValidAccountName(f, platform.SocialMediaName.Name);

            SharedLibrary.Common.Results.Result<EstablishmentSocialMedia> result = EstablishmentSocialMedia.Create(
                establishment.EstablishmentId,
                platform.SocialMediaId,
                SocialMediaAccountName.Create(accountName).Value!
            );

            return result.IsSuccess ? result.Value! : null!;
        });

        _ = RuleFor(esm => esm.SocialMediaAccountName, f =>
        {
            SocialMediaPlatform platform = f.PickRandom(socialMediaPlatforms);
            return SocialMediaAccountName.Create(
                GenerateValidAccountName(f, platform.SocialMediaName.Name)
            ).Value!;
        });
    }

    private static string GenerateValidAccountName(Faker f, string platformName)
        => platformName.ToLowerInvariant() switch
        {
            "facebook" => f.Internet.UserName() + f.Random.Number(100, 999),
            "twitter" => $"@{f.Internet.UserName()}",
            "instagram" => $"{f.Internet.UserName().Replace("-", "_")}",
            "linkedin" => $"{f.Name.FirstName().ToLowerInvariant()}-{f.Name.LastName().ToLowerInvariant()}",
            _ => f.Internet.UserName() + f.Random.Number(1000, 9999)
        };
}
