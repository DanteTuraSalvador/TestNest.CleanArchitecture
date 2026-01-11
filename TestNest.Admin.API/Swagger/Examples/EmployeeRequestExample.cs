using Swashbuckle.AspNetCore.Filters;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Employee;

namespace TestNest.Admin.API.Swagger.Examples;

public class EmployeeForCreationRequestExample : IExamplesProvider<EmployeeForCreationRequest>
{
    public EmployeeForCreationRequest GetExamples()
    {
        return new EmployeeForCreationRequest
        {
            EmployeeNumber = "EMP-2024-001",
            FirstName = "Juan",
            MiddleName = "Dela",
            LastName = "Cruz",
            EmailAddress = "juan.delacruz@testnest.com",
            EmployeeRoleId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            EstablishmentId = "3fa85f64-5717-4562-b3fc-2c963f66afa7"
        };
    }
}

public class EmployeeForUpdateRequestExample : IExamplesProvider<EmployeeForUpdateRequest>
{
    public EmployeeForUpdateRequest GetExamples()
    {
        return new EmployeeForUpdateRequest
        {
            EmployeeNumber = "EMP-2024-001-UPDATED",
            FirstName = "Juan",
            MiddleName = "Dela",
            LastName = "Cruz",
            EmailAddress = "juan.delacruz.updated@testnest.com",
            EmployeeRoleId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            EstablishmentId = "3fa85f64-5717-4562-b3fc-2c963f66afa7"
        };
    }
}
