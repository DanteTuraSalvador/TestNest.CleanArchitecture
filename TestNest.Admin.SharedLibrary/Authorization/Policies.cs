namespace TestNest.Admin.SharedLibrary.Authorization;

public static class Policies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireManager = "RequireManager";
    public const string RequireStaff = "RequireStaff";
    public const string RequireManagerOrAdmin = "RequireManagerOrAdmin";
}
