using Bogus;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.ValueObjects;
using TestNest.Admin.SharedLibrary.ValueObjects.Enums;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders.Establishments;

public sealed class EstablishmentFaker : Faker<Establishment>
{
    public EstablishmentFaker() => CustomInstantiator(f =>
    {
        var result = Establishment.Empty();
        Result<EstablishmentName>? nameResult = null;
        Result<EmailAddress>? emailResult = null;
        Result<Establishment>? statusResult = null;

        try
        {
            nameResult = GenerateValidName(f, 10);
            if (!nameResult.IsSuccess)
            {
                result = Establishment.Empty();
                return result;
            }

            emailResult = GenerateValidEmail(f, 10);
            if (!emailResult.IsSuccess)
            {
                result = Establishment.Empty();
                return result;
            }

            Result<Establishment> establishmentResult = Establishment.Create(
                nameResult.Value!,
                emailResult.Value!
            );

            if (!establishmentResult.IsSuccess)
            {
                result = Establishment.Empty();
                return result;
            }

            EstablishmentStatus status = f.PickRandom(GetAllowedStatuses());
            statusResult = establishmentResult.Value!.WithStatus(status);

            result = statusResult.IsSuccess
                ? statusResult.Value!
                : Establishment.Empty();
        }
        finally
        {
            if (result != null && !result.IsEmpty())
            {
                Console.WriteLine($"VALID: {result.EstablishmentName} | " +
                                $"{result.EstablishmentEmail} | " +
                                $"{result.EstablishmentStatus}");
            }
            else
            {
                Console.WriteLine($"INVALID: " +
                                $"Name={nameResult?.IsSuccess.ToString() ?? "N/A"}, " +
                                $"Email={emailResult?.IsSuccess.ToString() ?? "N/A"}, " +
                                $"Status={statusResult?.IsSuccess.ToString() ?? "N/A"}");
            }
        }

        return result;
    });

    private static Result<EstablishmentName> GenerateValidName(Faker f, int maxRetries = 20)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            string name = f.Company.CompanyName()
                .Replace("#", "")
                .Replace("!", "")
                .Trim();

            Result<EstablishmentName> result = EstablishmentName.Create(name);
            if (result.IsSuccess)
            {
                return result;
            }
        }
        return Result<EstablishmentName>.Failure(
            ErrorType.Validation,
            new Error("NameGenFailed", "Could not generate valid name")
        );
    }

    private static Result<EmailAddress> GenerateValidEmail(Faker f, int maxRetries)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            string email = f.Internet.Email(
                firstName: f.Name.FirstName().Replace("'", ""),
                lastName: f.Name.LastName().Replace("'", ""),
                provider: "testnest.com"
            );

            Result<EmailAddress> result = EmailAddress.Create(email);
            if (result.IsSuccess)
            {
                return result;
            }
        }
        return Result<EmailAddress>.Failure(
            ErrorType.Validation,
            new Error("EmailGenFailed", "Could not generate valid email")
        );
    }

    private static List<EstablishmentStatus> GetAllowedStatuses() =>
        [
            EstablishmentStatus.Pending,
            EstablishmentStatus.Approval,
            EstablishmentStatus.Active
        ];
}
