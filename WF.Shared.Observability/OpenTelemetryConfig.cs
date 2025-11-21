namespace WF.Shared.Observability
{
    public static class OpenTelemetryConfig
    {
        public static string[] CommonActivitySources => new[]
        {
            "MassTransit",
            "WF.CustomerService",
            "WF.FraudService",
            "WF.TransactionService",
            "WF.WalletService"
        };

        public static Dictionary<string, object> GetResourceAttributes(string serviceName, string version)
        {
            return new Dictionary<string, object>
            {
                ["service.name"] = serviceName,
                ["service.version"] = version,
                ["deployment.environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            };
        }

        public static string OtlpEndpoint =>
            Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
            ?? "http://localhost:4317";
    }
}
