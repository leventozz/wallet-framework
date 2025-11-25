namespace WF.Shared.Contracts.Enums;

public enum KeycloakRoles
{
    Customer,
    Admin,
    Officer,
    Support
}

public static class KeycloakRolesExtensions
{
    public static string GetRoleName(this KeycloakRoles role)
    {
        return role switch
        {
            KeycloakRoles.Customer => "wf-customer",
            KeycloakRoles.Admin => "wf-admin",
            KeycloakRoles.Officer => "wf-officer",
            KeycloakRoles.Support => "wf-support",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown role")
        };
    }
}

