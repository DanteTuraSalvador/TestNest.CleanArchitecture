namespace TestNest.Admin.SharedLibrary.Authorization;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Staff = "Staff";

    public static readonly IReadOnlyList<string> All = new[] { Admin, Manager, Staff };
}
