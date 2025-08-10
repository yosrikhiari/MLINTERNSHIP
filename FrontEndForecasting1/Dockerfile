# FrontEndForecasting1/Dockerfile - Fixed version
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install wget for health checks
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project files with correct paths
COPY ["FrontEndForecasting1/FrontEndForecasting.csproj", "FrontEndForecasting1/"]
COPY ["MLINTERNSHIP/MLINTERNSHIP.csproj", "MLINTERNSHIP/"]

# Restore dependencies
RUN dotnet restore "FrontEndForecasting1/FrontEndForecasting.csproj"

# Copy the rest of the source code
COPY FrontEndForecasting1/ FrontEndForecasting1/
COPY MLINTERNSHIP/ MLINTERNSHIP/

WORKDIR "/src/FrontEndForecasting1"

# Build the application
RUN dotnet build "FrontEndForecasting.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "FrontEndForecasting.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "FrontEndForecasting.dll"]