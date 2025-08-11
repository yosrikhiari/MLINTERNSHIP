# FrontEndForecasting1/Dockerfile - Fixed version
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install dependencies: wget (health checks), Python3 + pip (Prophet), and native libs
RUN apt-get update && apt-get install -y \
    wget \
    python3 \
    python3-pip \
    python3-venv \
    libgomp1 \
    libstdc++6 \
    unzip \
    ca-certificates \
    libblas3 \
    liblapack3 \
    && rm -rf /var/lib/apt/lists/*

# Install Prophet and scientific stack at runtime (small set)
RUN python3 -m venv /opt/venv && \
    . /opt/venv/bin/activate && \
    python3 -m pip install --no-cache-dir --upgrade pip && \
    python3 -m pip install --no-cache-dir prophet pandas numpy

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install unzip for the build stage (needed for libxgboost package)
RUN apt-get update && apt-get install -y \
    unzip \
    && rm -rf /var/lib/apt/lists/*

# Copy the project files with correct paths
COPY ["FrontEndForecasting1/FrontEndForecasting.csproj", "FrontEndForecasting1/"]
COPY ["MLINTERNSHIP/MLINTERNSHIP.csproj", "MLINTERNSHIP/"]

# Restore dependencies
RUN dotnet restore "FrontEndForecasting1/FrontEndForecasting.csproj"

# Copy the rest of the source code
COPY FrontEndForecasting1/ FrontEndForecasting1/
COPY MLINTERNSHIP/ MLINTERNSHIP/

WORKDIR "/src/FrontEndForecasting1"

# Set environment variables for the build
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Build the application with additional flags to handle native dependencies
RUN dotnet build "FrontEndForecasting.csproj" -c Release -o /app/build --verbosity normal

# Publish the application
FROM build AS publish
RUN dotnet publish "FrontEndForecasting.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Combine and extract libxgboost parts into native runtime directory if present
RUN set -eux; \
    mkdir -p /app/runtimes/linux-x64/native; \
    if [ -f /app/runtimes/linux-x64/native/libxgboost.so.part1.zip ] && [ -f /app/runtimes/linux-x64/native/libxgboost.so.part2.zip ]; then \
      cat /app/runtimes/linux-x64/native/libxgboost.so.part1.zip /app/runtimes/linux-x64/native/libxgboost.so.part2.zip > /app/runtimes/linux-x64/native/libxgboost.so.zip; \
      unzip -o /app/runtimes/linux-x64/native/libxgboost.so.zip -d /app/runtimes/linux-x64/native/; \
    elif [ -f /app/libxgboost.so.part1.zip ] && [ -f /app/libxgboost.so.part2.zip ]; then \
      cat /app/libxgboost.so.part1.zip /app/libxgboost.so.part2.zip > /app/libxgboost.so.zip; \
      unzip -o /app/libxgboost.so.zip -d /app/runtimes/linux-x64/native/; \
    fi; \
    # Ensure permissions are readable
    find /app/runtimes/linux-x64/native -type f -name "*.so*" -exec chmod a+r {} + || true; \
    # Copy to common probe locations
    if [ -f /app/runtimes/linux-x64/native/libxgboost.so ]; then \
      cp -f /app/runtimes/linux-x64/native/libxgboost.so /app/libxgboost.so; \
      cp -f /app/runtimes/linux-x64/native/libxgboost.so /app/xgboost.so; \
    fi

# Ensure Python can run Prophet script inside container
ENV PYTHONUNBUFFERED=1
ENV PATH="/opt/venv/bin:$PATH"
ENV LD_LIBRARY_PATH="/app/runtimes/linux-x64/native:$LD_LIBRARY_PATH"

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "FrontEndForecasting.dll"]