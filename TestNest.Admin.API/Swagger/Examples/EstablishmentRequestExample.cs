using Swashbuckle.AspNetCore.Filters;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;

namespace TestNest.Admin.API.Swagger.Examples;

public class EstablishmentForCreationRequestExample : IExamplesProvider<EstablishmentForCreationRequest>
{
    public EstablishmentForCreationRequest GetExamples()
    {
        return new EstablishmentForCreationRequest
        {
            EstablishmentName = "TestNest Main Branch",
            EmailAddress = "mainbranch@testnest.com"
        };
    }
}

public class EstablishmentForUpdateRequestExample : IExamplesProvider<EstablishmentForUpdateRequest>
{
    public EstablishmentForUpdateRequest GetExamples()
    {
        return new EstablishmentForUpdateRequest
        {
            EstablishmentName = "TestNest Main Branch - Updated",
            EmailAddress = "mainbranch.updated@testnest.com"
        };
    }
}
