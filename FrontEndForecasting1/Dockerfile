# Use the official .NET 9.0 runtime image as base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project files
COPY ["FrontEndForecasting1.csproj", "FrontEndForecasting1/"]
COPY ["../MLINTERNSHIP/MLINTERNSHIP.csproj", "../MLINTERNSHIP/"]

# Restore dependencies
RUN dotnet restore "FrontEndForecasting1/FrontEndForecasting1.csproj"
RUN dotnet restore "../MLINTERNSHIP/MLINTERNSHIP.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/FrontEndForecasting1"

# Build the application
RUN dotnet build "FrontEndForecasting1.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "FrontEndForecasting1.csproj" -c Release -o /app/publish

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

ENTRYPOINT ["dotnet", "FrontEndForecasting1.dll"]
