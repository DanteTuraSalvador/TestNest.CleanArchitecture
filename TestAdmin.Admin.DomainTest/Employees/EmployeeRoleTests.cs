using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestAdmin.Admin.DomainTest.Employees;

public class EmployeeRoleTests
{
    [Fact]
    public void Create_ValidRoleName_ReturnsSuccess()
    {
        RoleName validRoleName = RoleName.Create("Manager").Value!;

        Result<EmployeeRole> result = EmployeeRole.Create(validRoleName);

        Assert.True(result.IsSuccess);
        Assert.Equal(validRoleName, result.Value!.RoleName);
        Assert.NotEqual(EmployeeRoleId.Empty(), result.Value.EmployeeRoleId);
    }

    [Fact]
    public void Create_EmptyRoleName_ReturnsFailure()
    {
        var invalidRoleName = RoleName.Empty();
        Result<EmployeeRole> result = EmployeeRole.Create(invalidRoleName);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "EmptyRoleName");
    }

    [Fact]
    public void WithRoleName_ValidNewName_UpdatesSuccessfully()
    {
        EmployeeRole original = EmployeeRole.Create(RoleName.Create("Cashier").Value!).Value!;
        RoleName newRoleName = RoleName.Create("Supervisor").Value!;

        Result<EmployeeRole> result = original.WithRoleName(newRoleName);

        Assert.True(result.IsSuccess);
        Assert.Equal(newRoleName, result.Value!.RoleName);
        Assert.Equal(original.EmployeeRoleId, result.Value.EmployeeRoleId);
    }

    [Fact]
    public void WithRoleName_InvalidNewName_ReturnsFailure()
    {
        EmployeeRole original = EmployeeRole.Create(RoleName.Create("Chef").Value!).Value!;
        var invalidRoleName = RoleName.Empty();

        Result<EmployeeRole> result = original.WithRoleName(invalidRoleName);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "EmptyRoleName");
    }

    [Fact]
    public void Empty_Instance_HasCorrectEmptyState()
    {
        var emptyRole = EmployeeRole.Empty();

        Assert.True(emptyRole.IsEmpty());
        Assert.Equal(RoleName.Empty(), emptyRole.RoleName);
        Assert.Equal(EmployeeRoleId.Empty(), emptyRole.EmployeeRoleId);
    }

    [Fact]
    public void ToString_ReturnsRoleNameString()
    {
        EmployeeRole role = EmployeeRole.Create(RoleName.Create("Analyst").Value!).Value!;

        Assert.Equal("Analyst", role.ToString());
    }

    [Fact]
    public void Create_TooLongRoleName_ReturnsValidationError()
    {
        var invalidRoleName = RoleName.Empty();
        Result<EmployeeRole> result = EmployeeRole.Create(invalidRoleName);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "EmptyRoleName");
    }
}
