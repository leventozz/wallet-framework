# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects
COPY shared/WF.Shared.Contracts/WF.Shared.Contracts.csproj shared/WF.Shared.Contracts/
COPY shared/WF.Shared.Observability/WF.Shared.Observability.csproj shared/WF.Shared.Observability/

# Copy FraudService projects
COPY src/Services/FraudService/WF.FraudService.Domain/WF.FraudService.Domain.csproj src/Services/FraudService/WF.FraudService.Domain/
COPY src/Services/FraudService/WF.FraudService.Application/WF.FraudService.Application.csproj src/Services/FraudService/WF.FraudService.Application/
COPY src/Services/FraudService/WF.FraudService.Infrastructure/WF.FraudService.Infrastructure.csproj src/Services/FraudService/WF.FraudService.Infrastructure/
COPY src/Services/FraudService/WF.FraudService.Api/WF.FraudService.Api.csproj src/Services/FraudService/WF.FraudService.Api/

# Restore dependencies
RUN dotnet restore src/Services/FraudService/WF.FraudService.Api/WF.FraudService.Api.csproj

# Copy all source files
COPY shared/ shared/
COPY src/Services/FraudService/ src/Services/FraudService/

# Build and publish
WORKDIR /src/src/Services/FraudService/WF.FraudService.Api
RUN dotnet publish WF.FraudService.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN addgroup --system --gid 1001 dotnetuser && \
    adduser --system --uid 1001 --ingroup dotnetuser dotnetuser

# Copy published files
COPY --from=build /app/publish .

# Change ownership to non-root user
RUN chown -R dotnetuser:dotnetuser /app

# Switch to non-root user
USER dotnetuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Health check can be configured in docker-compose.yml
# HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
#     CMD dotnet --version || exit 1

# Entry point
ENTRYPOINT ["dotnet", "WF.FraudService.Api.dll"]

