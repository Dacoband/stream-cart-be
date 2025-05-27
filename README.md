# Stream Cart Backend

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Microservice Architecture

The Stream Cart backend is built with a modern microservice architecture, using .NET framework for service implementation. This README provides an overview of the system architecture, components, and how they interact.

### System Overview

![Architecture Diagram](architecture-diagram.png)

Our stream-cart-be system consists of several interconnected microservices, each responsible for specific business functionality. The architecture follows a domain-driven design approach, allowing for scalable, maintainable, and independently deployable services.

## Architecture Components

### Client Applications
- **Web Application**: Built with Next.js
- **Mobile Application**: Built with Flutter
- **Stream Cart Front-End Application**: Custom streaming e-commerce frontend

### API Gateway & Load Balancing
- **NGINX**: Acts as the web server and load balancer
- **Ocelot API Gateway**: Provides a unified entry point for all microservices
  - Handles routing
  - Request aggregation
  - Authentication forwarding

### Core Microservices
All services are built using .NET Core:

1. **Account Service**
   - User authentication and authorization
   - Profile management
   - User preferences

2. **Product Service**
   - Product catalog management
   - Product metadata
   - Inventory tracking

3. **Livestream Service**
   - Stream management
   - Live video integration
   - Viewer analytics

4. **Order Service**
   - Order processing
   - Order history
   - Status tracking

5. **Notification Service**
   - Push notifications
   - Email notifications
   - In-app alerts

6. **ShoppingCart Service**
   - Cart management
   - Item storage
   - Price calculations

7. **Payment Service**
   - Payment processing
   - Transaction records
   - Integration with payment gateways

### Third-Party Services
- **RTMP Server**: For video streaming
- **WebRTC Server**: For real-time communication
- **Sepay (Webhook)**: For payment processing

### Data Storage
- **PostgreSQL**: Primary relational database
- **MongoDB**: Document database for specific services
- **Redis**: In-memory data structure store for caching and session management

### Messaging
- **RabbitMQ**: Message broker for inter-service communication

### Containerization
- **Docker**: All services are containerized for consistent deployment

## Service Communication

Services communicate through:
1. **Synchronous Communication**: REST APIs for direct requests
2. **Asynchronous Communication**: Message queues via RabbitMQ for event-driven architecture
3. **Event Sourcing**: For maintaining data consistency across services

## Deployment

The system is containerized using Docker, allowing for:
- Easy scaling of individual services
- Consistent environments across development and production
- Simplified deployment process

## Getting Started

### Prerequisites
- .NET Core SDK
- Docker and Docker Compose
- PostgreSQL
- MongoDB
- Redis
- RabbitMQ

### Setup Instructions
1. Clone the repository
   ```bash
   git clone https://github.com/Dacoband/stream-cart-be.git
   ```

2. Navigate to the project directory
   ```bash
   cd stream-cart-be
   ```

3. Run docker-compose to start all services
   ```bash
   docker-compose up -d
   ```

4. Access the API documentation
   ```
   http://localhost:8080/swagger
   ```

## Development Guidelines

### Adding a New Microservice
1. Create a new .NET project in the services directory
2. Configure Docker and Docker Compose files
3. Register the service with the API Gateway
4. Set up necessary database migrations
5. Implement service-to-service communication as needed

### Inter-Service Communication
- Use REST for synchronous requests
- Use RabbitMQ for asynchronous event-based communication
- Implement circuit breakers for fault tolerance

## Monitoring and Logging

The system includes:
- Centralized logging
- Health checks for each service
- Performance monitoring
- Alerting for critical issues

## Security

- JWT-based authentication
- API Gateway security
- HTTPS communication
- Rate limiting
- Input validation

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
