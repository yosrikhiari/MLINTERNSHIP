# Use the official .NET 9.0 runtime image as the base image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Install system dependencies for ML libraries
RUN apt-get update && apt-get install -y \
    libgomp1 \
    libblas3 \
    liblapack3 \
    && rm -rf /var/lib/apt/lists/*

# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["MLINTERNSHIP.csproj", "./"]
RUN dotnet restore "MLINTERNSHIP.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "MLINTERNSHIP.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MLINTERNSHIP.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Create a directory for data files
RUN mkdir -p /app/data

# Set environment variables
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Expose port if needed (for any web services)
EXPOSE 5000

# Set the entry point
ENTRYPOINT ["dotnet", "MLINTERNSHIP.dll"]
