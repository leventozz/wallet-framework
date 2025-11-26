using System.Reflection;
using VaultSharp.Extensions.Configuration;

namespace WF.FraudService.Api.Extensions;

public static class ConfigurationExtensions
{
    public static void AddVaultConfiguration(this IConfigurationBuilder configurationBuilder, IConfiguration configuration)
    {
        var vaultOptions = configuration.GetSection("Vault");
        var enabled = vaultOptions.GetValue<bool>("Enabled");

        if (!enabled)
        {
            return;
        }

        var address = vaultOptions.GetValue<string>("Address");
        var token = vaultOptions.GetValue<string>("Token");

        if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(token))
        {
            return;
        }

        configurationBuilder.AddVaultConfiguration(
            () => new VaultOptions(address, token),
            "wallet/shared",
            "secret"
        );

        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        string? serviceName = null;
        if (assemblyName != null)
        {
            var parts = assemblyName.Split('.');
            if (parts.Length > 1)
            {
                serviceName = parts[1].ToLower();
            }
        }

        if (!string.IsNullOrEmpty(serviceName))
        {
             configurationBuilder.AddVaultConfiguration(
                () => new VaultOptions(address, token),
                $"wallet/{serviceName}",
                "secret"
            );
        }
    }
}
