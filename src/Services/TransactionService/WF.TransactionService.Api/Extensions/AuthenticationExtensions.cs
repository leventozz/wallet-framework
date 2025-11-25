using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace WF.TransactionService.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddWFAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var keycloakSection = configuration.GetSection("Keycloak");
        var baseUrl = keycloakSection["BaseUrl"] ?? "http://localhost:8080";
        var realm = keycloakSection["Realm"] ?? "wallet-realm";
        var authority = $"{baseUrl}/realms/{realm}";

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
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("wf-admin"));

            options.AddPolicy("CustomerOnly", policy =>
                policy.RequireRole("wf-customer"));

            options.AddPolicy("OfficerAccess", policy =>
                policy.RequireRole("wf-admin", "wf-officer"));

            options.AddPolicy("ReadOnly", policy =>
                policy.RequireRole("wf-admin", "wf-officer", "wf-support"));
        });

        return services;
    }
}

