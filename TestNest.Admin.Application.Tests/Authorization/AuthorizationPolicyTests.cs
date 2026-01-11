using TestNest.Admin.SharedLibrary.Authorization;

namespace TestNest.Admin.Application.Tests.Authorization;

public class AuthorizationPolicyTests
{
    [Fact]
    public void Roles_Admin_ShouldBeCorrectValue()
    {
        // Assert
        Assert.Equal("Admin", Roles.Admin);
    }

    [Fact]
    public void Roles_Manager_ShouldBeCorrectValue()
    {
        // Assert
        Assert.Equal("Manager", Roles.Manager);
    }

    [Fact]
    public void Roles_Staff_ShouldBeCorrectValue()
    {
        // Assert
        Assert.Equal("Staff", Roles.Staff);
    }

    [Fact]
    public void Roles_All_ShouldContainAllRoles()
    {
        // Assert
        Assert.Contains(Roles.Admin, Roles.All);
        Assert.Contains(Roles.Manager, Roles.All);
        Assert.Contains(Roles.Staff, Roles.All);
        Assert.Equal(3, Roles.All.Count);
    }

    [Fact]
    public void Policies_RequireAdmin_ShouldBeCorrectValue()
    {
        // Assert
        Assert.Equal("RequireAdmin", Policies.RequireAdmin);
    }

    [Fact]
    public void Policies_RequireManager_ShouldBeCorrectValue()
    {
        // Assert
        Assert.Equal("RequireManager", Policies.RequireManager);
    }

    [Fact]
    public void Policies_RequireStaff_ShouldBeCorrectValue()
    {
        // Assert
        Assert.Equal("RequireStaff", Policies.RequireStaff);
    }

    [Fact]
    public void Policies_RequireManagerOrAdmin_ShouldBeCorrectValue()
    {
        // Assert
        Assert.Equal("RequireManagerOrAdmin", Policies.RequireManagerOrAdmin);
    }
}
