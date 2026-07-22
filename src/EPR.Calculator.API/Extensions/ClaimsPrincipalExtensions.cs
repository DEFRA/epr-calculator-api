using System.Security.Claims;
using Microsoft.Identity.Web;

namespace EPR.Calculator.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetName(this ClaimsPrincipal principal)
    {
        var name = principal.FindFirstValue("name");

        if (string.IsNullOrWhiteSpace(name))
            name = principal.GetDisplayName();

        if (string.IsNullOrWhiteSpace(name))
            name = "Unknown User";

        return name;
    }
}
