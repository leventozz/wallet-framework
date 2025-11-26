# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared projects
COPY shared/WF.Shared.Contracts/WF.Shared.Contracts.csproj shared/WF.Shared.Contracts/
COPY shared/WF.Shared.Observability/WF.Shared.Observability.csproj shared/WF.Shared.Observability/

# Copy TransactionService projects
COPY src/Services/TransactionService/WF.TransactionService.Domain/WF.TransactionService.Domain.csproj src/Services/TransactionService/WF.TransactionService.Domain/
COPY src/Services/TransactionService/WF.TransactionService.Application/WF.TransactionService.Application.csproj src/Services/TransactionService/WF.TransactionService.Application/
COPY src/Services/TransactionService/WF.TransactionService.Infrastructure/WF.TransactionService.Infrastructure.csproj src/Services/TransactionService/WF.TransactionService.Infrastructure/
COPY src/Services/TransactionService/WF.TransactionService.Api/WF.TransactionService.Api.csproj src/Services/TransactionService/WF.TransactionService.Api/

# Restore dependencies
RUN dotnet restore src/Services/TransactionService/WF.TransactionService.Api/WF.TransactionService.Api.csproj

# Copy all source files
COPY shared/ shared/
COPY src/Services/TransactionService/ src/Services/TransactionService/

# Build and publish
WORKDIR /src/src/Services/TransactionService/WF.TransactionService.Api
RUN dotnet publish WF.TransactionService.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd --system --gid 1001 dotnetuser && \
    useradd --system --uid 1001 --gid 1001 --no-create-home --shell /bin/false dotnetuser

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
ENTRYPOINT ["dotnet", "WF.TransactionService.Api.dll"]

