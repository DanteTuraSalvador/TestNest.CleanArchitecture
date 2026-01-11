using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace TestNest.Admin.API.Telemetry;

public class CloudRoleNameTelemetryInitializer : ITelemetryInitializer
{
    private readonly string _roleName;

    public CloudRoleNameTelemetryInitializer(string roleName = "TestNest.Admin.API")
    {
        _roleName = roleName;
    }

    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = _roleName;
        telemetry.Context.Cloud.RoleInstance = Environment.MachineName;
    }
}
