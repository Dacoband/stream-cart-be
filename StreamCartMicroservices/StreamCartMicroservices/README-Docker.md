# Docker Compose Configuration

This directory contains the optimized Docker Compose configuration for the StreamCart microservices architecture.

## Prerequisites

- Docker Engine 20.10.0 or later
- Docker Compose V2
- At least 4GB of available RAM
- At least 10GB of available disk space

## Quick Start

1. **Configure Environment Variables**
   ```bash
   # Copy the example environment file
   cp .env.example .env
   
   # Edit the .env file with your actual values
   nano .env
   ```

2. **Start Infrastructure Services First**
   ```bash
   # Start database and message broker
   docker compose up -d postgres rabbitmq
   
   # Wait for services to be healthy
   docker compose ps
   ```

3. **Start All Services**
   ```bash
   # Build and start all services
   docker compose up --build -d
   
   # Or start specific services
   docker compose up -d account-service product-service
   ```

4. **Monitor Services**
   ```bash
   # Check service status
   docker compose ps
   
   # View logs
   docker compose logs -f account-service
   
   # View all logs
   docker compose logs
   ```

## Architecture Overview

### Infrastructure Services
- **postgres**: PostgreSQL 15 database with persistent storage
- **rabbitmq**: RabbitMQ message broker with management interface

### Core Microservices
- **account-service**: User authentication and account management
- **product-service**: Product catalog and inventory management
- **shop-service**: Shop and seller management
- **order-service**: Order processing and management
- **payment-service**: Payment processing
- **cart-service**: Shopping cart functionality
- **delivery-service**: Delivery and shipping management
- **api-gateway**: Ocelot API Gateway for request routing

## Service Dependencies

The services have the following dependency hierarchy:

```
Infrastructure Services (postgres, rabbitmq)
├── Core Services (account, product, shop)
├── Business Services (order, payment, cart)
├── Delivery Service
└── API Gateway (depends on all services)
```

## Health Checks

All services include health checks that:
- Test service availability every 30 seconds
- Have a 10-second timeout
- Allow 3 retries before marking as unhealthy
- Wait 40 seconds before starting health checks

## Environment Variables

Key environment variables in `.env`:

### Database Configuration
- `POSTGRES_DB`: Database name
- `POSTGRES_USER`: Database username
- `POSTGRES_PASSWORD`: Database password

### Security
- `JWT_SECRET_KEY`: **IMPORTANT**: Change in production!
- `JWT_ISSUER`: JWT token issuer
- `JWT_AUDIENCE`: JWT token audience

### External Services
- `APPWRITE_*`: Appwrite cloud storage configuration
- `EMAIL_*`: Email service configuration
- `API_TOKEN_GHN`: Giao Hang Nhanh delivery service token

## Port Mappings

| Service | Port | Purpose |
|---------|------|---------|
| postgres | 5432 | Database |
| rabbitmq | 5672 | Message broker |
| rabbitmq-mgmt | 15672 | RabbitMQ management UI |
| account-service | 7022 | Account API |
| product-service | 7005 | Product API |
| shop-service | 7077 | Shop API |
| order-service | 7135 | Order API |
| payment-service | 7021 | Payment API |
| cart-service | 7228 | Cart API |
| delivery-service | 7202 | Delivery API |
| api-gateway | 8000 | Main API Gateway |

## Optimizations Applied

1. **YAML Anchors & Aliases**: Reduced configuration duplication by 80%
2. **Environment File**: Centralized all environment variables in `.env`
3. **Health Checks**: Proper health monitoring for all services
4. **Service Dependencies**: Logical startup order with health check conditions
5. **Alpine Images**: Smaller, more secure base images where possible
6. **Persistent Storage**: Properly configured PostgreSQL data persistence
7. **Network Configuration**: Explicit bridge network configuration

## Troubleshooting

### Check Service Health
```bash
docker compose ps
```

### View Service Logs
```bash
# Specific service
docker compose logs -f account-service

# All services
docker compose logs --tail=50
```

### Reset Database
```bash
docker compose down -v
docker compose up -d postgres
```

### Reset Everything
```bash
docker compose down -v --remove-orphans
docker compose up --build -d
```

### Access Management Interfaces
- RabbitMQ Management: http://localhost:15672 (guest/guest)
- API Gateway: http://localhost:8000

## Production Considerations

1. **Security**
   - Change all default passwords
   - Use strong JWT secret keys
   - Configure proper SSL certificates
   - Use secrets management for sensitive data

2. **Performance**
   - Scale services based on load: `docker compose up -d --scale account-service=3`
   - Monitor resource usage
   - Configure resource limits

3. **Reliability**
   - Set up log aggregation
   - Configure monitoring and alerting
   - Implement backup strategies
   - Use container orchestration (Kubernetes) for production

## Development

### Running Individual Services
```bash
# Start only infrastructure
docker compose up -d postgres rabbitmq

# Start specific services for development
docker compose up -d account-service product-service
```

### Building Images
```bash
# Build all images
docker compose build

# Build specific service
docker compose build account-service
```

### Scaling Services
```bash
# Scale a specific service
docker compose up -d --scale account-service=2
```