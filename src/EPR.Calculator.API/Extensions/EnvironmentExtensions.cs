namespace EPR.Calculator.API.Extensions;

public static class EnvironmentExtensions
{
    public static bool IsLocal(this IHostEnvironment hostEnvironment) => hostEnvironment.IsEnvironment(CommonResources.Local);
}
