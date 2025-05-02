using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;
using TestNest.Admin.SharedLibrary.ValueObjects.Enums;

namespace TestNest.Admin.UnitTests.Domain.Employees;

public class EmployeeTests
{
    // Valid test values
    private const string ValidEmployeeNumber = "EMP-123";

    private const string ValidFirstName = "John";
    private const string ValidLastName = "Doe";
    private const string ValidEmail = "john.doe@testnest.com";

    [Fact]
    public void Create_WithValidParameters_Succeeds()
    {
        // Arrange & Act
        Result<Employee> result = Employee.Create(
            CreateValidEmployeeNumber(),
            CreateValidPersonName(),
            CreateValidEmail(),
            EmployeeRoleId.New(),
            EstablishmentId.New()
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(EmployeeStatus.Active, result.Value!.EmployeeStatus);
    }

    [Theory]
    [InlineData("", "EmptyEmployeeNumber")]
    [InlineData("A", "LengthOutOfRangeEmployeeNumber")]
    [InlineData("EMP@123", "InvalidEmployeeNumberFormat")]
    public void Create_InvalidEmployeeNumberInput_ReturnsCorrectError(string invalidInput, string expectedErrorCode)
    {
        // Arrange
        EmployeeNumber invalidNumber = EmployeeNumber.Create(invalidInput).IsSuccess
            ? EmployeeNumber.Create(invalidInput).Value!
            : EmployeeNumber.Empty();

        // Act
        Result<Employee> result = Employee.Create(
            invalidNumber,
            CreateValidPersonName(),
            CreateValidEmail(),
            EmployeeRoleId.New(),
            EstablishmentId.New()
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == expectedErrorCode);
    }

    [Fact]
    public void Create_WithInvalidPersonName_Fails()
    {
        // Arrange & Act
        Result<Employee> result = Employee.Create(
            CreateValidEmployeeNumber(),
            PersonName.Empty(),  // This creates a name with empty strings
            CreateValidEmail(),
            EmployeeRoleId.New(),
            EstablishmentId.New()
        );

        // Assert - Check for specific validation errors
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "EmptyFirstName");
        Assert.Contains(result.Errors, e => e.Code == "EmptyLastName");
    }

    [Fact]
    public void Create_WithEmptyRoleId_Fails()
    {
        // Act
        Result<Employee> result = Employee.Create(
            CreateValidEmployeeNumber(),
            CreateValidPersonName(),
            CreateValidEmail(),
            EmployeeRoleId.Empty(),
            EstablishmentId.New()
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "EmployeeRoleException");
    }

    [Fact]
    public void Create_WithEmptyEstablishmentId_Fails()
    {
        // Act
        Result<Employee> result = Employee.Create(
            CreateValidEmployeeNumber(),
            CreateValidPersonName(),
            CreateValidEmail(),
            EmployeeRoleId.New(),
            EstablishmentId.Empty()
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "NullId");
    }

    [Fact]
    public void WithEmail_ValidEmail_Succeeds()
    {
        // Arrange
        Employee employee = CreateValidEmployee();
        EmailAddress validEmail = EmailAddress.Create("new.email@testnest.com").Value!;

        // Act
        Result<Employee> result = employee.WithEmail(validEmail);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(validEmail.ToString(), result.Value!.EmployeeEmail.ToString());
    }

    [Fact]
    public void WithEmail_InvalidEmail_Fails()
    {
        // Arrange
        Employee employee = CreateValidEmployee();
        var invalidEmail = EmailAddress.Empty();

        // Act
        Result<Employee> result = employee.WithEmail(invalidEmail);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "EmailAddressException");
    }

    [Fact]
    public void WithEmployeeStatus_ValidTransition_Succeeds()
    {
        // Arrange
        Employee employee = CreateValidEmployee();

        // Act
        Result<Employee> result = employee.WithEmployeeStatus(EmployeeStatus.Suspended);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(EmployeeStatus.Suspended, result.Value!.EmployeeStatus);
    }

    [Fact]
    public void WithPersonName_ValidName_Succeeds()
    {
        // Arrange
        Employee employee = CreateValidEmployee();
        Result<PersonName> newNameResult = PersonName.Create("Jane", "Marie", "Doe"); // Use valid middle name

        // First validate the name creation
        Assert.True(newNameResult.IsSuccess);

        // Act
        Result<Employee> result = employee.WithPersonName(newNameResult.Value!);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Jane Marie Doe", result.Value!.EmployeeName.ToString());
    }

    private static EmployeeNumber CreateValidEmployeeNumber() =>
        EmployeeNumber.Create(ValidEmployeeNumber).Value!;

    private static PersonName CreateValidPersonName() =>
        PersonName.Create(ValidFirstName, null, ValidLastName).Value!;

    private static EmailAddress CreateValidEmail() =>
        EmailAddress.Create(ValidEmail).Value!;

    private static Employee CreateValidEmployee() =>
        Employee.Create(
            CreateValidEmployeeNumber(),
            CreateValidPersonName(),
            CreateValidEmail(),
            EmployeeRoleId.New(),
            EstablishmentId.New()
        ).Value!;
}
