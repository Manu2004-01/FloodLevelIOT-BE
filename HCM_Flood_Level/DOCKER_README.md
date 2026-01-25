# Docker Setup Guide

## Prerequisites
- Docker Desktop installed
- Docker Compose installed

## Quick Start

### Development Environment

1. Build and run the container:
```bash
docker-compose up -d --build
```

2. View logs:
```bash
docker-compose logs -f webapi
```

3. Stop the container:
```bash
docker-compose down
```

### Production Environment

1. Create `.env.prod` file with your production configuration
2. Run:
```bash
docker-compose -f docker-compose.prod.yml up -d --build
```

## Building Docker Image

```bash
# Build the image
docker build -t floodlevel-api:latest -f Dockerfile .

# Run the container
docker run -d -p 8080:8080 --name floodlevel-api floodlevel-api:latest
```

## Environment Variables

You can override environment variables in `docker-compose.yml` or use `.env` file:

- `ConnectionStrings__CoreDb`: Core database connection string
- `ConnectionStrings__EventsDb`: Events database connection string
- `Jwt__Key`: JWT secret key
- `Jwt__Issuer`: JWT issuer
- `Jwt__Audience`: JWT audience
- `Jwt__ExpiresMinutes`: JWT expiration time in minutes
- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Production)

## Accessing the API

After starting the container, the API will be available at:
- HTTP: `http://localhost:8080`
- Swagger UI: `http://localhost:8080/swagger`

## Troubleshooting

1. **Check container status:**
```bash
docker ps
```

2. **View container logs:**
```bash
docker logs floodlevel-api
```

3. **Execute commands inside container:**
```bash
docker exec -it floodlevel-api /bin/bash
```

4. **Remove container and image:**
```bash
docker-compose down
docker rmi floodlevel-api:latest
```

