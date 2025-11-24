# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects
COPY shared/WF.Shared.Contracts/WF.Shared.Contracts.csproj shared/WF.Shared.Contracts/
COPY shared/WF.Shared.Observability/WF.Shared.Observability.csproj shared/WF.Shared.Observability/

# Copy ApiGateway project
COPY src/Infrastructure/WF.ApiGateway/WF.ApiGateway.csproj src/Infrastructure/WF.ApiGateway/

# Restore dependencies
RUN dotnet restore src/Infrastructure/WF.ApiGateway/WF.ApiGateway.csproj

# Copy all source files
COPY shared/ shared/
COPY src/Infrastructure/WF.ApiGateway/ src/Infrastructure/WF.ApiGateway/

# Build and publish
WORKDIR /src/src/Infrastructure/WF.ApiGateway
RUN dotnet publish WF.ApiGateway.csproj -c Release -o /app/publish

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
ENTRYPOINT ["dotnet", "WF.ApiGateway.dll"]

