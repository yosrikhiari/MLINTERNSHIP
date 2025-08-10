# Docker Setup for Forecasting Application

This document describes how to run the Forecasting Application using Docker with external monitoring.

## Architecture Overview

The application uses a **clean separation of concerns**:
- **Frontend Application**: .NET 9.0 web application (no embedded Prometheus)
- **Prometheus**: External monitoring service (Docker container)
- **Grafana**: External visualization service (Docker container)

## Why This Approach?

✅ **No Overcompensation**: Removed embedded Prometheus client libraries  
✅ **Clean Separation**: Application focuses on business logic, monitoring is external  
✅ **Scalability**: Can monitor multiple instances without code changes  
✅ **Maintenance**: Update monitoring independently of application  
✅ **Resource Efficiency**: No memory overhead from embedded metrics  

## Prerequisites

- Docker Desktop installed and running
- Docker Compose v2.0+
- At least 4GB RAM available

## Quick Start

1. **Build and Start Services**:
   ```bash
   docker-compose up -d --build
   ```

2. **Access Services**:
   - **Application**: http://localhost:8080
   - **Prometheus**: http://localhost:9090
   - **Grafana**: http://localhost:3000 (admin/admin123)

3. **Check Status**:
   ```bash
   docker-compose ps
   ```

## Service Details

### Frontend Application
- **Port**: 8080 (HTTP), 8443 (HTTPS)
- **Health Check**: `/health` endpoint
- **Monitoring**: Exposes health metrics for Prometheus scraping

### Prometheus
- **Port**: 9090
- **Scrapes**: Application health metrics every 30 seconds
- **Storage**: Persistent volume for metrics retention

### Grafana
- **Port**: 3000
- **Default Credentials**: admin/admin123
- **Pre-configured**: Prometheus datasource and basic dashboard

## Monitoring Configuration

### Prometheus Scraping
The application exposes a `/health` endpoint that Prometheus scrapes for:
- Application availability
- Response times
- Basic health metrics

### Custom Metrics
Since we removed embedded Prometheus, the application now:
- Logs performance data to structured logs
- Exposes health status via `/health` endpoint
- Relies on external Prometheus for metrics collection

## Development vs Production

### Development
```bash
# Run with hot reload
dotnet run --project FrontEndForecasting1
```

### Production (Docker)
```bash
# Build and run containers
docker-compose up -d --build

# View logs
docker-compose logs -f frontend-forecasting

# Scale if needed
docker-compose up -d --scale frontend-forecasting=3
```

## Troubleshooting

### Common Issues

1. **Port Conflicts**:
   ```bash
   # Check what's using the ports
   netstat -an | findstr :8080
   netstat -an | findstr :9090
   netstat -an | findstr :3000
   ```

2. **Container Won't Start**:
   ```bash
   # Check logs
   docker-compose logs frontend-forecasting
   
   # Check container status
   docker-compose ps
   ```

3. **Health Check Failures**:
   ```bash
   # Test health endpoint directly
   curl http://localhost:8080/health
   ```

### Logs
```bash
# View all logs
docker-compose logs

# Follow specific service logs
docker-compose logs -f frontend-forecasting

# View last 100 lines
docker-compose logs --tail=100 frontend-forecasting
```

## Performance Benefits

### Before (Embedded Prometheus)
- ❌ Memory overhead from metrics collection
- ❌ CPU overhead from metrics processing
- ❌ Complex dependency management
- ❌ Hard to scale monitoring independently

### After (External Prometheus)
- ✅ Clean application code
- ✅ No memory overhead
- ✅ No CPU overhead from metrics
- ✅ Independent monitoring scaling
- ✅ Professional monitoring setup

## Security Considerations

- **Network Isolation**: Services communicate via internal Docker network
- **Non-root User**: Application runs as non-root user in container
- **Read-only Volumes**: Configuration files mounted as read-only
- **Health Checks**: Built-in health monitoring for all services

## Scaling

### Horizontal Scaling
```bash
# Scale the frontend application
docker-compose up -d --scale frontend-forecasting=3

# Load balancer will distribute traffic
```

### Monitoring Scaling
```bash
# Add more Prometheus instances
docker-compose up -d --scale prometheus=2

# Add more Grafana instances
docker-compose up -d --scale grafana=2
```

## Maintenance

### Updates
```bash
# Pull latest images
docker-compose pull

# Rebuild and restart
docker-compose up -d --build
```

### Backup
```bash
# Backup Prometheus data
docker run --rm -v prometheus_data:/data -v $(pwd):/backup alpine tar czf /backup/prometheus-backup.tar.gz -C /data .

# Backup Grafana data
docker run --rm -v grafana_data:/data -v $(pwd):/backup alpine tar czf /backup/grafana-backup.tar.gz -C /data .
```

## Next Steps

1. **Custom Dashboards**: Create Grafana dashboards for your specific metrics
2. **Alerting**: Configure Prometheus alerting rules
3. **Log Aggregation**: Add ELK stack or similar for log management
4. **Metrics**: Add custom business metrics via structured logging
