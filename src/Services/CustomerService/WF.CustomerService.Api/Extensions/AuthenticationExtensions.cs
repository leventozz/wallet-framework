using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace WF.CustomerService.Api.Extensions;

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

        return services;
    }
}

