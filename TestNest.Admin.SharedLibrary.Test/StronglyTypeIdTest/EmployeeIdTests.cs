using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.SharedLibrary.Test.StronglyTypeIdTest;

public class EmployeeIdTests
{
    [Fact]
    public void EmployeeId_Constructor_SetsNewGuid()
    {
        var employeeId = new EmployeeId();

        Assert.NotEqual(Guid.Empty, employeeId.Value);
    }

    [Fact]
    public void EmployeeId_Create_WithValidGuid_ReturnsSuccessResult()
    {
        var validGuid = Guid.NewGuid();

        Result<EmployeeId> result = EmployeeId.Create(validGuid);

        Assert.True(result.IsSuccess);
        Assert.Equal(validGuid, result.Value!.Value);
    }

    [Fact]
    public void EmployeeId_Create_WithEmptyGuid_ReturnsFailureResult()
    {
        Guid emptyGuid = Guid.Empty;

        Result<EmployeeId> result = EmployeeId.Create(emptyGuid);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        _ = Assert.Single(result.Errors);
        Assert.Equal(StronglyTypedIdException.ErrorCode.NullId.ToString(), result.Errors[0].Code);
        Assert.Equal(StronglyTypedIdException.NullId().Message, result.Errors[0].Message);
    }

    [Fact]
    public void EmployeeId_ToString_ReturnsGuidAsString()
    {
        var validGuid = Guid.NewGuid();
        EmployeeId? employeeId = EmployeeId.Create(validGuid).Value;

        string stringRepresentation = employeeId.ToString();

        Assert.Equal(validGuid.ToString(), stringRepresentation);
    }

    [Fact]
    public void EmployeeId_Equality_SameValue_AreEqual()
    {
        var guid = Guid.NewGuid();
        EmployeeId? employeeId1 = EmployeeId.Create(guid).Value;
        EmployeeId? employeeId2 = EmployeeId.Create(guid).Value;

        Assert.Equal(employeeId1, employeeId2);
        Assert.True(employeeId1 == employeeId2);
    }

    [Fact]
    public void EmployeeId_Equality_DifferentValue_AreNotEqual()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        EmployeeId? employeeId1 = EmployeeId.Create(guid1).Value;
        EmployeeId? employeeId2 = EmployeeId.Create(guid2).Value;

        Assert.NotEqual(employeeId1, employeeId2);
        Assert.True(employeeId1 != employeeId2);
    }
}
