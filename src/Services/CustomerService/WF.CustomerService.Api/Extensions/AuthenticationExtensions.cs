using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WF.CustomerService.Api.Authentication;
using WF.Shared.Contracts.Configuration;
using WF.Shared.Contracts.Enums;

namespace WF.CustomerService.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddWFAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var keycloakOptions = configuration.GetSection("Keycloak").Get<KeycloakOptions>()
            ?? new KeycloakOptions
            {
                BaseUrl = "http://localhost:8080",
                Realm = "wallet-realm"
            };

        var authority = $"{keycloakOptions.BaseUrl}/realms/{keycloakOptions.Realm}";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.RequireHttpsMetadata = environment.IsProduction();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = environment.IsProduction(),
                ValidateIssuer = true,
                ValidIssuer = authority
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy =>
                policy.RequireRole(KeycloakRoles.Admin.GetRoleName()));

            options.AddPolicy("Customer", policy =>
                policy.RequireRole(KeycloakRoles.Customer.GetRoleName()));

            options.AddPolicy("Officer", policy =>
                policy.RequireRole(KeycloakRoles.Admin.GetRoleName(), KeycloakRoles.Officer.GetRoleName()));

            options.AddPolicy("Support", policy =>
                policy.RequireRole(KeycloakRoles.Admin.GetRoleName(), KeycloakRoles.Officer.GetRoleName(), KeycloakRoles.Support.GetRoleName()));
        });

        services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();

        return services;
    }
}

