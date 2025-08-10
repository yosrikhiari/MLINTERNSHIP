# MLINTERNSHIP/Dockerfile - Fixed version with wget
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

# Install system dependencies for ML libraries and wget for health checks
RUN apt-get update && apt-get install -y \
    libgomp1 \
    libblas3 \
    liblapack3 \
    wget \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["MLINTERNSHIP.csproj", "./"]
RUN dotnet restore "MLINTERNSHIP.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "MLINTERNSHIP.csproj" -c Release -o /app/build

# Publish the application as framework-dependent (not self-contained)
FROM build AS publish
RUN dotnet publish "MLINTERNSHIP.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Create necessary directories
RUN mkdir -p /app/data /app/logs

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Set environment variables
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

# Set the entry point
ENTRYPOINT ["dotnet", "MLINTERNSHIP.dll"]