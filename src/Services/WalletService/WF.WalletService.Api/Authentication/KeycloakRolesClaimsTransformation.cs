using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace WF.WalletService.Api.Authentication;

public class KeycloakRolesClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var claimsIdentity = principal.Identity as ClaimsIdentity;
        if (claimsIdentity == null || !claimsIdentity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        if (claimsIdentity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value.StartsWith("wf-")))
        {
            return Task.FromResult(principal);
        }

        var realmAccessClaim = claimsIdentity.FindFirst("realm_access");
        if (realmAccessClaim == null || string.IsNullOrWhiteSpace(realmAccessClaim.Value))
        {
            return Task.FromResult(principal);
        }

        try
        {
            using var document = JsonDocument.Parse(realmAccessClaim.Value);
            var root = document.RootElement;

            if (root.TryGetProperty("roles", out var rolesElement) && rolesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var roleElement in rolesElement.EnumerateArray())
                {
                    if (roleElement.ValueKind == JsonValueKind.String)
                    {
                        var role = roleElement.GetString();
                        if (!string.IsNullOrWhiteSpace(role))
                        {
                            if (role.StartsWith("wf-", StringComparison.OrdinalIgnoreCase))
                            {
                                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                            }
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
        }

        return Task.FromResult(principal);
    }
}
