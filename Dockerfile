# Multi-stage build for FBI 2257 Form Application

# Build Stage - Use -bookworm for ARM64 support
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project files
COPY ["SignMy2257.csproj", "./"]

# Restore dependencies
RUN dotnet restore "SignMy2257.csproj"

# Copy application source code
COPY . .

# Build the application
RUN dotnet build "SignMy2257.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "SignMy2257.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage - Use -bookworm for ARM64 support
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create data directory for form storage
RUN mkdir -p /app/data /app/logs

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Storage__BasePath=/app/data

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Expose port
EXPOSE 5000

# Run the application
ENTRYPOINT ["dotnet", "SignMy2257.dll"]
