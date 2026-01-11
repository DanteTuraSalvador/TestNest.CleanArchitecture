using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace TestNext.Admin.ArchitecturalTest;

public class DependencyTests
{
    private static readonly Architecture Architecture =
        new ArchLoader()
            .LoadAssemblies(
                typeof(TestNest.Admin.API.Controllers.EstablishmentAddressesController).Assembly,
                typeof(TestNest.Admin.Application.Services.EstablishmentAddressService).Assembly,
                typeof(TestNest.Admin.Domain.Establishments.Establishment).Assembly,
                typeof(TestNest.Admin.Infrastructure.Persistence.Repositories.EstablishmentRepository).Assembly,
                typeof(TestNest.Admin.SharedLibrary.StronglyTypeIds.EstablishmentId).Assembly
            ).Build();

    // Layer Definitions
    private readonly IObjectProvider<IType> DomainLayer =
        Types().That().ResideInNamespace("TestNest.Admin.Domain.*", true)
            .As("Domain Layer");

    private readonly IObjectProvider<IType> ApplicationLayer =
        Types().That().ResideInNamespace("TestNest.Admin.Application.*", true)
            .As("Application Layer");

    private readonly IObjectProvider<IType> InfrastructureLayer =
        Types().That().ResideInNamespace("TestNest.Admin.Infrastructure.*", true)
            .As("Infrastructure Layer");

    private readonly IObjectProvider<IType> ApiLayer =
        Types().That().ResideInNamespace("TestNest.Admin.API.*", true)
            .As("API Layer");

    [Fact]
    public void DomainLayer_ShouldNotDependOnOtherProjects()
        => Types().That().Are(DomainLayer)
            .Should().NotDependOnAny(ApplicationLayer)
            .AndShould().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(ApiLayer)
            .Check(Architecture);

    [Fact]
    public void ApplicationLayer_ShouldNotDependOnInfrastructureOrApi()
        => Types().That().Are(ApplicationLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(ApiLayer)
            .Check(Architecture);

    [Fact]
    public void Controllers_ShouldResideInApiLayer() 
        => Classes().That().HaveNameEndingWith("Controller")
            .Should().ResideInNamespace("TestNest.Admin.API.Controllers")
            .Check(Architecture);

    [Fact]
    public void Controllers_ShouldNotReferenceRepositoriesDirectly()
        => Classes().That().ResideInNamespace("TestNest.Admin.API.Controllers")
            .Should().NotDependOnAnyTypesThat().ResideInNamespace("TestNest.Admin.Infrastructure.Persistence")
            .Check(Architecture);

    [Fact]
    public void DomainLayer_ShouldNotReferenceNewtonsoftJson() 
        => Types().That().Are(DomainLayer)
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("Newtonsoft.Json")
            .Check(Architecture);

    [Fact]
    public void Infrastructure_ShouldNotContainBusinessLogic() 
        => Classes().That().ResideInNamespace("TestNest.Admin.Infrastructure")
            .Should().NotHaveNameEndingWith("Service")
            .Check(Architecture);

    [Fact]
    public void DatabaseContext_ShouldOnlyBeReferencedByInfrastructure() 
        => Types().That().AreAssignableTo("Microsoft.EntityFrameworkCore.DbContext")
            .Should().ResideInNamespace("TestNest.Admin.Infrastructure.Persistence")
            .Check(Architecture);

}
