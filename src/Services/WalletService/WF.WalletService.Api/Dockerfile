# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared projects
COPY shared/WF.Shared.Contracts/WF.Shared.Contracts.csproj shared/WF.Shared.Contracts/
COPY shared/WF.Shared.Observability/WF.Shared.Observability.csproj shared/WF.Shared.Observability/

# Copy WalletService projects
COPY src/Services/WalletService/WF.WalletService.Domain/WF.WalletService.Domain.csproj src/Services/WalletService/WF.WalletService.Domain/
COPY src/Services/WalletService/WF.WalletService.Application/WF.WalletService.Application.csproj src/Services/WalletService/WF.WalletService.Application/
COPY src/Services/WalletService/WF.WalletService.Infrastructure/WF.WalletService.Infrastructure.csproj src/Services/WalletService/WF.WalletService.Infrastructure/
COPY src/Services/WalletService/WF.WalletService.Api/WF.WalletService.Api.csproj src/Services/WalletService/WF.WalletService.Api/

# Restore dependencies
RUN dotnet restore src/Services/WalletService/WF.WalletService.Api/WF.WalletService.Api.csproj

# Copy all source files
COPY shared/ shared/
COPY src/Services/WalletService/ src/Services/WalletService/

# Build and publish
WORKDIR /src/src/Services/WalletService/WF.WalletService.Api
RUN dotnet publish WF.WalletService.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
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
ENTRYPOINT ["dotnet", "WF.WalletService.Api.dll"]

