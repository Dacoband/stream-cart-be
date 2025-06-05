# 🖥️ Stream Cart - Livestream-base Ecom AI

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Containerized-blue.svg)](https://www.docker.com/)

## 🏗️ System Architecture Overview

Stream Cart Backend is a comprehensive microservices-based e-commerce platform designed for live streaming commerce. The system follows Domain-Driven Design (DDD) principles with Clean Architecture patterns, ensuring scalability, maintainability, and testability.

![Architecture Diagram](/image/architecture-diagram.png)

### 🎯 Key Features
- **Live Streaming Commerce**: Real-time product showcasing with integrated shopping
- **Microservices Architecture**: Independently deployable and scalable services
- **Event-Driven Communication**: Asynchronous messaging with RabbitMQ
- **Clean Architecture**: Separation of concerns with DDD patterns
- **Container-Ready**: Full Docker containerization support

## 📁 Project Structure

```
StreamCartMicroservices/
├── 📂 src/
│   ├── 📂 ApiGateway/           # Ocelot API Gateway
│   ├── 📂 AccountService/       # User Authentication & Management
│   │   ├── 📂 AccountService.Api/
│   │   ├── 📂 AccountService.Application/
│   │   ├── 📂 AccountService.Domain/
│   │   └── 📂 AccountService.Infrastructure/
│   └── 📂 Shared/              # Shared Libraries
│       ├── 📂 Shared.Common/    # Common utilities & base classes
│       └── 📂 Shared.Messaging/ # Messaging infrastructure
├── 📄 docker-compose.yml       # Container orchestration
└── 📄 README.md
```

## 🏛️ Architecture Components

### 🌐 API Gateway Layer
- **ApiGateway**: Built with Ocelot
  - Unified entry point for all microservices
  - Request routing and aggregation
  - JWT authentication forwarding
  - Swagger documentation aggregation

### 🔧 Core Microservices

#### 🔐 Account Service
**Location**: src/AccountService/

**Responsibilities**:
- User authentication and authorization
- Account management and profiles
- Role-based access control
- JWT token generation

**Key Components**:
- **Domain Layer**: `Account` entity with business logic
- **Application Layer**: `AccountManagementService` with CQRS pattern
- **Infrastructure Layer**: `AccountRepository` with EF Core
- **API Layer**: RESTful controllers for account operations

### 📚 Shared Libraries

#### 🔄 Shared.Common
**Location**: src/Shared/Shared.Common/

**Key Features**:
- **Base Entity**: `BaseEntity` - Audit trail and soft delete support
- **Generic Repository**: `IGenericRepository<T>` - CRUD operations with pagination
- **API Response**: `ApiResponse<T>` - Standardized API responses
- **Configuration Extensions**: JWT, CORS, and settings management

#### 📨 Shared.Messaging
**Location**: src/Shared/Shared.Messaging/

**Key Features**:
- **MassTransit Integration**: `MessagingExtensions`
- **RabbitMQ Configuration**: Retry policies and circuit breaker
- **Base Consumer**: `IBaseConsumer` for message handling

## 🔑 Key Design Patterns & Classes

### 🏗️ Domain-Driven Design (DDD)

#### Base Entity Pattern
```csharp
// 📍 BaseEntity provides audit trail and soft delete
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public string CreatedBy { get; protected set; }
    public DateTime? LastModifiedAt { get; protected set; }
    public string? LastModifiedBy { get; protected set; }
    public bool IsDeleted { get; protected set; }
    
    // Business methods for audit trail
    public void SetCreator(string creator) { ... }
    public void SetModifier(string modifier) { ... }
    public void Delete(string? modifier = null) { ... }
}
```

#### Repository Pattern
```csharp
// 📍 Generic repository with advanced features
public interface IGenericRepository<T> where T : class
{
    Task<PagedResult<T>> SearchAsync(
        string searchTerm,
        PaginationParams paginationParams,
        string[]? searchableFields = null,
        Expression<Func<T, bool>>? filter = null,
        bool exactMatch = false);
}
```

### 🎯 CQRS Pattern
The Account Service implements Command Query Responsibility Segregation:

- **Commands**: `CreateAccountCommand`, `UpdateAccountCommand`
- **Handlers**: `CreateAccountHandler`
- **DTOs**: `AccountDto`, `CreateAccountDto`

### 📡 Event-Driven Architecture
```csharp
// 📍 Domain events for inter-service communication
public class AccountRegistered
{
    public Guid AccountId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime RegistrationDate { get; set; }
}
```

## 🛠️ Technology Stack

### Backend Framework
- **.NET 8.0**: Primary development framework
- **Entity Framework Core**: ORM with PostgreSQL provider
- **MediatR**: CQRS and mediator pattern implementation
- **FluentValidation**: Input validation

### Infrastructure
- **PostgreSQL**: Primary relational database
- **RabbitMQ**: Message broker for asynchronous communication
- **Docker**: Containerization platform
- **Ocelot**: API Gateway framework

### Security & Authentication
- **JWT Bearer Tokens**: Authentication mechanism
- **Role-based Authorization**: RBAC implementation
- **CORS**: Cross-origin resource sharing configuration

## 🚀 Getting Started

### Prerequisites
```bash
# Required software
- .NET 8.0 SDK
- Docker & Docker Compose
- PostgreSQL 15+
- RabbitMQ
- Visual Studio 2022 or VS Code
```

### 🔧 Environment Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/yourusername/stream-cart-be.git
   cd stream-cart-be
   ```

2. **Configure Environment Variables**
   Create `.env` file in the AccountService.Api directory:
   ```env
   # Database Configuration
   POSTGRES_CONNECTION=Host=localhost;Database=streamcart_accounts;Username=postgres;Password=yourpassword
   
   # JWT Configuration
   JWT_SECRET_KEY=your-super-secret-jwt-key-here
   JWT_ISSUER=StreamCartAPI
   JWT_AUDIENCE=StreamCartClients
   JWT_EXPIRY_MINUTES=60
   
   # RabbitMQ Configuration
   RABBITMQ_HOST=localhost
   RABBITMQ_USERNAME=guest
   RABBITMQ_PASSWORD=guest
   ```

3. **Start Infrastructure Services**
   ```bash
   # Start PostgreSQL and RabbitMQ
   docker-compose up postgres rabbitmq -d
   ```

4. **Run Database Migrations**
   ```bash
   cd StreamCartMicroservices/StreamCartMicroservices/src/AccountService/AccountService.Api
   dotnet ef database update
   ```

5. **Start the Services**
   ```bash
   # Start API Gateway
   cd src/ApiGateway/ApiGateway
   dotnet run
   
   # Start Account Service (in new terminal)
   cd src/AccountService/AccountService.Api
   dotnet run
   ```

### 📋 API Documentation

Access Swagger documentation:
- **API Gateway**: `https://localhost:7195/swagger`
- **Account Service**: `https://localhost:7022/swagger`

### 🧪 Testing the API

#### Create a New Account
```bash
curl -X POST "https://localhost:7195/api/account" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "SecurePassword123",
    "fullname": "Test User",
    "role": "Customer"
  }'
```

#### Login
```bash
curl -X POST "https://localhost:7195/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "SecurePassword123"
  }'
```

## 🏗️ Development Guidelines

### 📁 Adding a New Microservice

1. **Create Service Structure**
   ```bash
   mkdir src/YourService
   cd src/YourService
   
   # Create layered architecture
   dotnet new classlib -n YourService.Domain
   dotnet new classlib -n YourService.Application
   dotnet new classlib -n YourService.Infrastructure
   dotnet new webapi -n YourService.Api
   ```

2. **Add Project References**
   ```bash
   # Infrastructure → Domain
   cd YourService.Infrastructure
   dotnet add reference ../YourService.Domain/YourService.Domain.csproj
   dotnet add reference ../../Shared/Shared.Common/Shared.Common.csproj
   
   # Application → Domain + Infrastructure
   cd ../YourService.Application
   dotnet add reference ../YourService.Domain/YourService.Domain.csproj
   dotnet add reference ../YourService.Infrastructure/YourService.Infrastructure.csproj
   
   # API → Application
   cd ../YourService.Api
   dotnet add reference ../YourService.Application/YourService.Application.csproj
   ```

3. **Configure API Gateway**
   Update `ocelot.json`:
   ```json
   {
     "DownstreamPathTemplate": "/api/yourservice/{everything}",
     "DownstreamScheme": "https",
     "DownstreamHostAndPorts": [
       {
         "Host": "localhost",
         "Port": 7023
       }
     ],
     "UpstreamPathTemplate": "/api/yourservice/{everything}",
     "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
     "SwaggerKey": "yourservice"
   }
   ```

### 🎨 Code Standards

#### Entity Design
```csharp
public class YourEntity : BaseEntity
{
    // Private setters for encapsulation
    public string Name { get; private set; }
    
    // Constructor with validation
    public YourEntity(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        Name = name;
    }
    
    // Business methods
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty", nameof(newName));
        
        Name = newName;
        SetModifier("system"); // Audit trail
    }
}
```

#### Repository Implementation
```csharp
public class YourRepository : EfCoreGenericRepository<YourEntity>, IYourRepository
{
    public YourRepository(YourDbContext dbContext) : base(dbContext) { }
    
    public async Task<YourEntity?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Name == name);
    }
}
```

### 🔄 Inter-Service Communication

#### Publishing Events
```csharp
// In your handler
await _publishEndpoint.Publish(new YourEventHappened
{
    EntityId = entity.Id,
    Timestamp = DateTime.UtcNow,
    Data = "Additional data"
}, cancellationToken);
```

#### Consuming Events
```csharp
public class YourEventConsumer : IConsumer<YourEventHappened>, IBaseConsumer
{
    public async Task Consume(ConsumeContext<YourEventHappened> context)
    {
        // Handle the event
        var eventData = context.Message;
        // Process business logic
    }
}
```

## 🔒 Security Implementation

### JWT Configuration
The system uses JWT tokens with the following claims:
- `NameIdentifier`: User ID
- `Name`: Username
- `Email`: User email
- `Role`: User role for authorization

### Authorization Levels
- **Public**: No authentication required
- **Authenticated**: Valid JWT token required
- **Role-based**: Specific roles (Admin, Customer, Seller, etc.)

### API Security Headers
```csharp
[Authorize(Roles = "Admin,ITAdmin")]
[ProducesResponseType(typeof(ApiResponse<IEnumerable<AccountDto>>), 200)]
public async Task<IActionResult> GetAll() { ... }
```

## 📊 Monitoring & Logging

### Health Checks
Each service includes health check endpoints:
```
GET /health
```

### Logging Strategy
- **Structured Logging**: JSON format for easy parsing
- **Log Levels**: Information, Warning, Error, Critical
- **Correlation IDs**: Track requests across services

## 🚢 Deployment

### Docker Deployment
```bash
# Build and start all services
docker-compose up --build

# Scale specific services
docker-compose up --scale account-service=3
```

### Production Considerations
- Use proper SSL certificates
- Configure production database connections
- Set up log aggregation (ELK Stack)
- Implement monitoring (Prometheus + Grafana)
- Configure backup strategies

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Workflow
1. Follow the established architecture patterns
2. Add unit tests for business logic
3. Update API documentation
4. Ensure Docker compatibility
5. Test inter-service communication

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Troubleshooting

### Common Issues

#### Database Connection Issues
```bash
# Check PostgreSQL container
docker-compose logs postgres

# Reset database
docker-compose down -v
docker-compose up postgres -d
```

#### RabbitMQ Connection Issues
```bash
# Access RabbitMQ Management UI
http://localhost:15672
# Default credentials: guest/guest
```

#### JWT Token Issues
- Ensure JWT settings are correctly configured in appsettings.json
- Verify token expiration time
- Check that the secret key is properly set in environment variables

---

**Contact**: For questions or support, please open an issue in the GitHub repository.
