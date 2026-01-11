using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using TestNest.Admin.SharedLibrary.Authorization;

namespace TestNest.Admin.Application.Tests.Authorization;

public class ControllerAuthorizationTests
{
    private readonly Assembly _apiAssembly;

    public ControllerAuthorizationTests()
    {
        // Load the API assembly to inspect controller attributes
        _apiAssembly = Assembly.Load("TestNest.Admin.API");
    }

    #region EmployeesController Tests

    [Fact]
    public void EmployeesController_CreateEmployee_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EmployeesController", "CreateEmployee");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EmployeesController_UpdateEmployee_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EmployeesController", "UpdateEmployee");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EmployeesController_PatchEmployee_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EmployeesController", "PatchEmployee");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EmployeesController_DeleteEmployee_RequiresAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EmployeesController", "DeleteEmployee");

        // Assert
        Assert.Equal(Policies.RequireAdmin, policy);
    }

    #endregion

    #region EmployeeRolesController Tests

    [Fact]
    public void EmployeeRolesController_CreateEmployeeRole_RequiresAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EmployeeRolesController", "CreateEmployeeRole");

        // Assert
        Assert.Equal(Policies.RequireAdmin, policy);
    }

    [Fact]
    public void EmployeeRolesController_UpdateEmployeeRole_RequiresAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EmployeeRolesController", "UpdateEmployeeRole");

        // Assert
        Assert.Equal(Policies.RequireAdmin, policy);
    }

    [Fact]
    public void EmployeeRolesController_DeleteEmployeeRole_RequiresAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EmployeeRolesController", "DeleteEmployeeRole");

        // Assert
        Assert.Equal(Policies.RequireAdmin, policy);
    }

    #endregion

    #region EstablishmentsController Tests

    [Fact]
    public void EstablishmentsController_CreateEstablishment_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentsController", "CreateEstablishment");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentsController_UpdateEstablishment_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentsController", "UpdateEstablishment");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentsController_PatchEstablishment_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentsController", "PatchEstablishment");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentsController_DeleteEstablishment_RequiresAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentsController", "DeleteEstablishment");

        // Assert
        Assert.Equal(Policies.RequireAdmin, policy);
    }

    #endregion

    #region SocialMediaPlatformsController Tests

    [Fact]
    public void SocialMediaPlatformsController_CreateSocialMediaPlatform_RequiresAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("SocialMediaPlatformsController", "CreateSocialMediaPlatform");

        // Assert
        Assert.Equal(Policies.RequireAdmin, policy);
    }

    [Fact]
    public void SocialMediaPlatformsController_UpdateSocialMediaPlatform_RequiresAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("SocialMediaPlatformsController", "UpdateSocialMediaPlatform");

        // Assert
        Assert.Equal(Policies.RequireAdmin, policy);
    }

    [Fact]
    public void SocialMediaPlatformsController_DeleteSocialMediaPlatform_RequiresAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("SocialMediaPlatformsController", "DeleteSocialMediaPlatform");

        // Assert
        Assert.Equal(Policies.RequireAdmin, policy);
    }

    #endregion

    #region EstablishmentAddressesController Tests

    [Fact]
    public void EstablishmentAddressesController_CreateEstablishmentAddress_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentAddressesController", "CreateEstablishmentAddress");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentAddressesController_UpdateEstablishmentAddress_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentAddressesController", "UpdateEstablishmentAddress");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentAddressesController_PatchEstablishmentAddress_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentAddressesController", "PatchEstablishmentAddress");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentAddressesController_DeleteEstablishmentAddress_RequiresAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentAddressesController", "DeleteEstablishmentAddress");

        // Assert
        Assert.Equal(Policies.RequireAdmin, policy);
    }

    #endregion

    #region EstablishmentContactsController Tests

    [Fact]
    public void EstablishmentContactsController_CreateEstablishmentContact_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentContactsController", "CreateEstablishmentContact");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentContactsController_UpdateEstablishmentContact_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentContactsController", "UpdateEstablishmentContact");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentContactsController_PatchEstablishmentContact_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentContactsController", "PatchEstablishmentContact");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentContactsController_DeleteEstablishmentContact_RequiresAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentContactsController", "DeleteEstablishmentContact");

        // Assert
        Assert.Equal(Policies.RequireAdmin, policy);
    }

    #endregion

    #region EstablishmentMembersController Tests

    [Fact]
    public void EstablishmentMembersController_CreateEstablishmentMember_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentMembersController", "CreateEstablishmentMember");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentMembersController_UpdateEstablishmentMember_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentMembersController", "UpdateEstablishmentMember");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentMembersController_PatchEstablishmentMember_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentMembersController", "PatchEstablishmentMember");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentMembersController_DeleteEstablishmentMember_RequiresAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentMembersController", "DeleteEstablishmentMember");

        // Assert
        Assert.Equal(Policies.RequireAdmin, policy);
    }

    #endregion

    #region EstablishmentPhonesController Tests

    [Fact]
    public void EstablishmentPhonesController_CreateEstablishmentPhone_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentPhonesController", "CreateEstablishmentPhone");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentPhonesController_UpdateEstablishmentPhone_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentPhonesController", "UpdateEstablishmentPhone");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentPhonesController_PatchEstablishmentPhone_RequiresManagerOrAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentPhonesController", "PatchEstablishmentPhone");

        // Assert
        Assert.Equal(Policies.RequireManagerOrAdmin, policy);
    }

    [Fact]
    public void EstablishmentPhonesController_DeleteEstablishmentPhone_RequiresAdminPolicy()
    {
        // Arrange & Act
        var policy = GetMethodPolicy("EstablishmentPhonesController", "DeleteEstablishmentPhone");

        // Assert
        Assert.Equal(Policies.RequireAdmin, policy);
    }

    #endregion

    #region Controller-Level Authorization Tests

    [Theory]
    [InlineData("EmployeesController")]
    [InlineData("EmployeeRolesController")]
    [InlineData("EstablishmentsController")]
    [InlineData("SocialMediaPlatformsController")]
    [InlineData("EstablishmentAddressesController")]
    [InlineData("EstablishmentContactsController")]
    [InlineData("EstablishmentMembersController")]
    [InlineData("EstablishmentPhonesController")]
    public void Controller_ShouldHaveAuthorizeAttribute(string controllerName)
    {
        // Arrange
        var controllerType = _apiAssembly.GetTypes()
            .FirstOrDefault(t => t.Name == controllerName);

        // Assert
        Assert.NotNull(controllerType);
        var authorizeAttribute = controllerType.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authorizeAttribute);
    }

    #endregion

    #region Helper Methods

    private string? GetMethodPolicy(string controllerName, string methodName)
    {
        var controllerType = _apiAssembly.GetTypes()
            .FirstOrDefault(t => t.Name == controllerName);

        if (controllerType == null)
            return null;

        var method = controllerType.GetMethods()
            .FirstOrDefault(m => m.Name == methodName);

        if (method == null)
            return null;

        var authorizeAttribute = method.GetCustomAttribute<AuthorizeAttribute>();
        return authorizeAttribute?.Policy;
    }

    #endregion
}
