# 🖥️ Stream Cart - Livestream-based E-commerce Platform 🖥️

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Containerized-blue.svg)](https://www.docker.com/)
[![LiveKit](https://img.shields.io/badge/LiveKit-Streaming-orange.svg)](https://livekit.io/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Messaging-green.svg)](https://www.rabbitmq.com/)

## 🏗️ System Architecture Overview

Stream Cart Backend is a comprehensive microservices-based e-commerce platform designed for live streaming commerce. The system follows Domain-Driven Design (DDD) principles with Clean Architecture patterns, ensuring scalability, maintainability, and testability. Built with modern technologies including LiveKit for real-time streaming, RabbitMQ for messaging, and comprehensive business logic for e-commerce operations.

![Architecture Diagram](/image/StreamCart-System%20Overview.png)

### 🎯 Key Features
- **🎥 Live Streaming Commerce**: Real-time product showcasing with integrated shopping using LiveKit
- **🏗️ Microservices Architecture**: 11 independently deployable and scalable services
- **📨 Event-Driven Communication**: Asynchronous messaging with RabbitMQ and MassTransit
- **🔧 Clean Architecture**: Separation of concerns with DDD patterns across all services
- **🐳 Container-Ready**: Full Docker containerization with production-ready compose
- **🔐 Secure Authentication**: JWT-based authentication with role-based authorization
- **💳 Payment Integration**: Comprehensive payment processing and wallet management
- **📦 Order Management**: Complete order lifecycle with delivery tracking
- **🛍️ Shopping Cart**: Real-time cart management with product synchronization
- **📱 Real-time Notifications**: MongoDB-based notification system
- **🏪 Multi-tenant Shop Management**: Support for multiple shops and sellers

## 📁 Project Structure

```
StreamCartMicroservices/
├── 📂 src/
│   ├── 📂 ApiGateway/               # Ocelot API Gateway
│   ├── 📂 AccountService/           # User Authentication & Management
│   │   ├── 📂 AccountService.Api/
│   │   ├── 📂 AccountService.Application/
│   │   ├── 📂 AccountService.Domain/
│   │   └── 📂 AccountService.Infrastructure/
│   ├── 📂 ProductService/           # Product Catalog & Management
│   │   ├── 📂 ProductService.Api/
│   │   ├── 📂 ProductService.Application/
│   │   ├── 📂 ProductService.Domain/
│   │   └── 📂 ProductService.Infrastructure/
│   ├── 📂 ShopService/              # Shop & Seller Management
│   │   ├── 📂 ShopService.Api/
│   │   ├── 📂 ShopService.Application/
│   │   ├── 📂 ShopService.Domain/
│   │   └── 📂 ShopService.Infrastructure/
│   ├── 📂 CartService/              # Shopping Cart Management
│   │   ├── 📂 CartService.Api/
│   │   ├── 📂 CartService.Application/
│   │   ├── 📂 CartService.Domain/
│   │   └── 📂 CartService.Infrastructure/
│   ├── 📂 OrderService/             # Order Processing & Management
│   │   ├── 📂 OrderService.Api/
│   │   ├── 📂 OrderService.Application/
│   │   ├── 📂 OrderService.Domain/
│   │   └── 📂 OrderService.Infrastructure/
│   ├── 📂 PaymentService/           # Payment Processing & Wallets
│   │   ├── 📂 PaymentService.Api/
│   │   ├── 📂 PaymentService.Application/
│   │   ├── 📂 PaymentService.Domain/
│   │   └── 📂 PaymentService.Infrastructure/
│   ├── 📂 LivestreamService/        # Live Streaming & Real-time Features
│   │   ├── 📂 LivestreamService.Api/
│   │   ├── 📂 LivestreamService.Application/
│   │   ├── 📂 LivestreamService.Domain/
│   │   └── 📂 LivestreamService.Infrastructure/
│   ├── 📂 DeliveryService/          # Shipping & Delivery Management
│   │   ├── 📂 DeliveryService.Api/
│   │   ├── 📂 DeliveryService.Application/
│   │   ├── 📂 DeliveryService.Domain/
│   │   └── 📂 DeliveryService.Infrastructure/
│   ├── 📂 NotificationService/      # Real-time Notifications
│   │   ├── 📂 Notification.Api/
│   │   ├── 📂 Notification.Application/
│   │   ├── 📂 Notification.Domain/
│   │   └── 📂 Notification.Infrastructure/
│   └── 📂 Shared/                   # Shared Libraries
│       ├── 📂 Shared.Common/        # Common utilities & base classes
│       └── 📂 Shared.Messaging/     # Messaging infrastructure
├── 📄 docker-compose.yml           # Container orchestration
├── 📄 Livekit.yaml                # LiveKit streaming configuration
├── 📄 .env                        # Environment variables
└── 📄 StreamCartMicroservices.sln  # Solution file
```

## 🏛️ Architecture Components

### 🌐 API Gateway Layer
- **ApiGateway**: Built with Ocelot (Port: 8000)
  - Unified entry point for all microservices
  - Request routing and aggregation
  - JWT authentication forwarding
  - Swagger documentation aggregation
  - Load balancing and service discovery

### 🔧 Core Microservices

#### 🔐 Account Service (Port: 7022)
**Location**: `src/AccountService/`

**Responsibilities**:
- User authentication and authorization with JWT tokens
- Account management and user profiles
- Role-based access control (Admin, Customer, Seller, ITAdmin)
- Address management with multiple address types
- Image upload with Appwrite integration
- Password reset and email verification

**Key Components**:
- **Domain Layer**: `Account`, `Address` entities with business logic
- **Application Layer**: CQRS with `AccountManagementService`, `AddressManagementService`, `AuthService`
- **Infrastructure Layer**: `AccountRepository`, `AddressRepository` with EF Core
- **API Layer**: RESTful controllers with comprehensive API endpoints

**Database**: PostgreSQL with Entity Framework Core

#### 🛍️ Product Service (Port: 7005)
**Location**: `src/ProductService/`

**Responsibilities**:
- Product catalog management with variants and combinations
- Category and attribute management
- Product images and media handling
- Flash sales and promotional campaigns
- Inventory tracking and stock management
- Search and filtering capabilities

**Key Features**:
- **Product Variants**: Size, color, and custom attributes
- **Product Combinations**: SKU management with pricing
- **Categories**: Hierarchical category structure
- **Flash Sales**: Time-limited promotional campaigns
- **Background Jobs**: Automated flash sale management with Quartz

**Database**: PostgreSQL with comprehensive product schema

#### 🏪 Shop Service (Port: 7077)
**Location**: `src/ShopService/`

**Responsibilities**:
- Multi-tenant shop management
- Seller onboarding and verification
- Shop profiles and branding
- Shop-product associations
- Revenue and analytics tracking
- Integration with Account and Product services

**Key Features**:
- **Shop Profiles**: Complete shop information with branding
- **Seller Management**: KYC and verification workflows
- **Product Integration**: Seamless product-shop relationships
- **Cross-service Communication**: HTTP clients for service integration

#### 🛒 Cart Service (Port: 7228)
**Location**: `src/CartService/`

**Responsibilities**:
- Real-time shopping cart management
- Cart persistence across sessions
- Product synchronization with Product Service
- Shop integration for multi-vendor support
- Cart validation and business rules
- Event-driven updates

**Key Features**:
- **Real-time Updates**: Automatic cart synchronization
- **Multi-vendor Support**: Support for products from different shops
- **Event Consumers**: React to product and shop updates
- **Session Management**: Persistent cart across user sessions

#### 📋 Order Service (Port: 7135)
**Location**: `src/OrderService/`

**Responsibilities**:
- Complete order lifecycle management
- Order processing and status tracking
- Integration with payment, delivery, and inventory
- Automated order completion with background services
- Order validation and business rule enforcement
- Wallet integration for payments

**Key Features**:
- **Order Status Flow**: Pending → Confirmed → Processing → Shipped → Delivered → Completed
- **Background Services**: Automatic order completion with Quartz scheduling
- **Service Integration**: Seamless integration with Payment, Product, and Account services
- **Wallet Support**: Integration with digital wallet functionality

#### 💳 Payment Service (Port: 7021)
**Location**: `src/PaymentService/`

**Responsibilities**:
- Payment processing and gateway integration
- Digital wallet management
- Transaction history and tracking
- Refund and chargeback handling
- Payment method management
- Integration with Order Service

**Key Features**:
- **Multiple Payment Methods**: Support for various payment gateways
- **Digital Wallets**: Built-in wallet functionality
- **Transaction Management**: Comprehensive payment tracking
- **Refund Processing**: Automated and manual refund capabilities

#### 🎥 Livestream Service (Port: 7041)
**Location**: `src/LivestreamService/`

**Responsibilities**:
- Real-time live streaming with LiveKit integration
- Stream room management and participant handling
- Real-time chat and interaction features
- Stream recording and playback
- Integration with Shop and Product services for live commerce
- SignalR for real-time communication

**Key Features**:
- **LiveKit Integration**: Professional live streaming capabilities
- **Real-time Chat**: MongoDB-based chat system with file attachments
- **Stream Management**: Room creation, participant management, and controls
- **Live Commerce**: Product showcasing during streams
- **SignalR Hubs**: Real-time bidirectional communication

**Database**: PostgreSQL for stream metadata, MongoDB for chat messages

#### 🚚 Delivery Service (Port: 7202)
**Location**: `src/DeliveryService/`

**Responsibilities**:
- Shipping and delivery management
- Integration with GHN (Giao Hàng Nhanh) delivery service
- Delivery tracking and status updates
- Shipping cost calculation
- Delivery address validation
- Integration with Order Service

**Key Features**:
- **GHN Integration**: Professional shipping service integration
- **Real-time Tracking**: Delivery status updates and notifications
- **Cost Calculation**: Dynamic shipping cost calculation
- **Address Validation**: Delivery address verification

#### 🔔 Notification Service (Port: 7078)
**Location**: `src/NotificationService/`

**Responsibilities**:
- Real-time notification system
- Multi-channel notification delivery (email, push, in-app)
- Notification templates and personalization
- Event-driven notification triggers
- Notification history and read status
- Integration with all services for comprehensive notifications

**Key Features**:
- **Multi-channel Support**: Email, SMS, push notifications
- **Template System**: Customizable notification templates
- **Event-driven**: Automatic notifications based on system events
- **MongoDB Storage**: Scalable notification storage and retrieval

**Database**: MongoDB for notification storage and templates

### 📚 Shared Libraries

#### 🔄 Shared.Common
**Location**: `src/Shared/Shared.Common/`

**Key Features**:
- **Base Entity**: `BaseEntity` - Comprehensive audit trail and soft delete support
- **Generic Repository**: `IGenericRepository<T>` - Advanced CRUD operations with pagination and search
- **API Response**: `ApiResponse<T>` - Standardized API responses across all services
- **Configuration Extensions**: JWT, CORS, email services, and Appwrite integration
- **Current User Service**: User context management across services
- **Middleware**: Authentication header processing and CORS configuration

#### 📨 Shared.Messaging
**Location**: `src/Shared/Shared.Messaging/`

**Key Features**:
- **MassTransit Integration**: `MessagingExtensions` with RabbitMQ configuration
- **Event Bus**: Centralized event publishing and consumption
- **Retry Policies**: Circuit breaker and retry mechanisms for reliability
- **Base Consumer**: `IBaseConsumer` interface for consistent message handling

### 🗄️ Database Architecture

**Primary Database**: PostgreSQL
- **Account Service**: User accounts, profiles, addresses
- **Product Service**: Products, categories, variants, flash sales
- **Shop Service**: Shop information, seller profiles
- **Cart Service**: Shopping cart items and sessions
- **Order Service**: Orders, order items, transaction history
- **Payment Service**: Payment transactions, wallet data
- **Livestream Service**: Stream metadata, room information
- **Delivery Service**: Shipping information, tracking data

**Secondary Database**: MongoDB
- **Livestream Service**: Real-time chat messages and attachments
- **Notification Service**: Notification history and templates

### 🔄 Inter-Service Communication

#### HTTP Communication
- **Service Clients**: Typed HTTP clients for synchronous communication
- **Circuit Breaker**: Resilience patterns for service failures
- **Service Discovery**: Container-based service resolution

#### Event-Driven Communication
- **MassTransit + RabbitMQ**: Asynchronous event publishing and consumption
- **Domain Events**: Business event propagation across services
- **Event Sourcing**: Comprehensive audit trail through events

## 🔑 Key Design Patterns & Classes

### 🏗️ Domain-Driven Design (DDD)

#### Base Entity Pattern
```csharp
// 📍 BaseEntity provides comprehensive audit trail and soft delete
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

#### Enhanced Repository Pattern
```csharp
// 📍 Generic repository with advanced search and pagination
public interface IGenericRepository<T> where T : class
{
    Task<PagedResult<T>> SearchAsync(
        string searchTerm,
        PaginationParams paginationParams,
        string[]? searchableFields = null,
        Expression<Func<T, bool>>? filter = null,
        bool exactMatch = false);
        
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
```

### 🎯 CQRS Pattern Implementation
All services implement Command Query Responsibility Segregation with MediatR:

**Commands Examples**:
- `CreateAccountCommand`, `UpdateAccountCommand`
- `CreateProductCommand`, `UpdateProductCommand`
- `AddToCartCommand`, `RemoveFromCartCommand`
- `CreateOrderCommand`, `UpdateOrderStatusCommand`

**Query Examples**:
- `GetAccountByIdQuery`, `GetAllAccountsQuery`
- `GetProductsByCategoryQuery`, `SearchProductsQuery`
- `GetCartByUserIdQuery`
- `GetOrderHistoryQuery`, `GetOrderByIdQuery`

**Handlers**:
- `CreateAccountCommandHandler`
- `GetProductsByCategoryQueryHandler`
- `ProcessPaymentCommandHandler`

### 📡 Event-Driven Architecture

#### Domain Events
```csharp
// 📍 Account Service Events
public class AccountRegistered
{
    public Guid AccountId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime RegistrationDate { get; set; }
}

// 📍 Product Service Events
public class ProductCreated
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public Guid ShopId { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}

// 📍 Order Service Events
public class OrderStatusChanged
{
    public Guid OrderId { get; set; }
    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; }
}
```

#### Event Consumers
```csharp
// 📍 Cross-service event handling
public class ProductUpdatedConsumer : IConsumer<ProductUpdated>, IBaseConsumer
{
    private readonly ICartService _cartService;

    public async Task Consume(ConsumeContext<ProductUpdated> context)
    {
        var productEvent = context.Message;
        
        // Update cart items with new product information
        await _cartService.UpdateProductInformationAsync(
            productEvent.ProductId, 
            productEvent.ProductName, 
            productEvent.Price);
    }
}
```

### 🔐 Security Architecture

#### JWT Token Implementation
```csharp
// 📍 Enhanced JWT claims structure
public class JwtToken
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string ShopId { get; set; } // For sellers
    public List<string> Permissions { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

#### Role-Based Authorization
```csharp
// 📍 Comprehensive role system
public enum UserRole
{
    Customer = 1,
    Seller = 2,
    Admin = 3,
    ITAdmin = 4,
    DeliveryPartner = 5
}

// 📍 Permission-based endpoints
[Authorize(Roles = "Admin,ITAdmin")]
[HttpGet("analytics")]
public async Task<ActionResult<AnalyticsDto>> GetSystemAnalytics() { ... }

[Authorize(Roles = "Seller,Admin")]
[HttpPost("products")]
public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto) { ... }
```

## 🛠️ Technology Stack

### Backend Framework & Runtime
- **.NET 8.0**: Latest LTS version with enhanced performance
- **ASP.NET Core**: High-performance web API framework
- **Entity Framework Core**: Advanced ORM with PostgreSQL provider
- **MediatR**: CQRS and mediator pattern implementation
- **FluentValidation**: Comprehensive input validation
- **AutoMapper**: Object-to-object mapping

### Databases & Storage
- **PostgreSQL 15+**: Primary relational database for all core services
- **MongoDB**: Document database for chat messages and notifications
- **Appwrite**: Cloud storage service for file uploads and media management

### Messaging & Communication
- **RabbitMQ**: Message broker for asynchronous communication
- **MassTransit**: .NET service bus implementation with retry policies
- **SignalR**: Real-time bidirectional communication for livestreaming
- **HTTP Clients**: Typed HTTP clients for inter-service communication

### Streaming & Real-time Features
- **LiveKit**: Professional-grade live streaming server
- **WebRTC**: Real-time communication protocol
- **SignalR Hubs**: Real-time chat and notification delivery

### Infrastructure & DevOps
- **Docker**: Containerization platform
- **Docker Compose**: Multi-container orchestration
- **Ocelot**: API Gateway framework with load balancing
- **Quartz.NET**: Background job scheduling and processing

### Security & Authentication
- **JWT Bearer Tokens**: Stateless authentication mechanism
- **Role-based Authorization**: Comprehensive RBAC implementation
- **CORS**: Cross-origin resource sharing configuration
- **HTTPS**: TLS encryption for secure communication

### External Integrations
- **MailJet**: Professional email service provider
- **GHN (Giao Hàng Nhanh)**: Vietnamese shipping and delivery service
- **Appwrite Storage**: Cloud file storage and management
- **Payment Gateways**: Integrated payment processing

### Development Tools
- **Swagger/OpenAPI**: Comprehensive API documentation
- **Health Checks**: Service monitoring and diagnostics
- **Structured Logging**: JSON-formatted logging for observability
- **Environment Configuration**: Flexible configuration management

## 🚀 Getting Started

### Prerequisites
```bash
# Required software
- .NET 8.0 SDK
- Docker & Docker Compose
- PostgreSQL 15+ (or use Docker)
- RabbitMQ (or use Docker)
- MongoDB (for notifications/chat)
- Visual Studio 2022 or VS Code
- Git for version control
```

### 🔧 Environment Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/Dacoband/stream-cart-be.git
   cd stream-cart-be/StreamCartMicroservices/StreamCartMicroservices
   ```

2. **Configure Environment Variables**
   The `.env` file contains all necessary configuration:
   ```env
   # Database Configuration
   POSTGRES_DB=StreamCartDb
   POSTGRES_USER=admin
   POSTGRES_PASSWORD=12345
   POSTGRES_CONNECTION=Host=160.187.241.81;Port=5432;Database=StreamCartDb;Username=admin;Password=12345
   
   # MongoDB for notifications and chat
   MONGO_CONNECTION_STRING=mongodb+srv://admin:password@cluster0.mongodb.net/
   
   # RabbitMQ Configuration
   RABBITMQ_HOST=localhost
   RABBITMQ_USERNAME=guest
   RABBITMQ_PASSWORD=guest
   
   # JWT Configuration
   JWT_SECRET_KEY=your-super-secret-jwt-key-here-256-bits
   JWT_ISSUER=StreamCartApi
   JWT_AUDIENCE=StreamCartClient
   JWT_EXPIRY_MINUTES=60
   
   # Email Configuration (MailJet)
   EMAIL_API_KEY=your-mailjet-api-key
   EMAIL_SECRET_KEY=your-mailjet-secret-key
   EMAIL_FROM_EMAIL=noreply@your-domain.com
   EMAIL_FROM_NAME=Stream Cart
   EMAIL_PROVIDER=MailJet
   
   # Appwrite Configuration
   APPWRITE_PROJECT_ID=your-project-id
   APPWRITE_ENDPOINT=https://cloud.appwrite.io/v1
   APPWRITE_BUCKET_ID=your-bucket-id
   APPWRITE_API_KEY=your-api-key
   
   # LiveKit Configuration
   LIVEKIT_URL=ws://localhost:7880
   LIVEKIT_API_KEY=your-livekit-api-key
   LIVEKIT_API_SECRET=your-livekit-secret
   
   # GHN Delivery Service
   API_TOKEN_GHN=your-ghn-token
   GHN_SHOPID=your-shop-id
   ```

3. **Start Infrastructure Services**
   ```bash
   # Start all infrastructure services
   docker-compose up -d rabbitmq livekit
   
   # Or start individual services
   docker-compose up -d rabbitmq
   docker-compose up -d livekit
   ```

4. **Database Setup**
   Each service handles its own database migrations automatically on startup through `DatabaseInitializer` services.

5. **Start All Services**
   ```bash
   # Start all services with Docker Compose
   docker-compose up --build
   
   # Or start specific services
   docker-compose up --build api-gateway account-service product-service
   ```

6. **Development Mode (Individual Services)**
   ```bash
   # Start API Gateway
   cd src/ApiGateway/ApiGateway
   dotnet run
   
   # Start Account Service (in new terminal)
   cd src/AccountService/AccountService.Api
   dotnet run
   
   # Start Product Service (in new terminal)
   cd src/ProductService/ProductService.Api
   dotnet run
   
   # Continue for other services...
   ```

### 📋 API Documentation & Service Ports

Access Swagger documentation for each service:

| Service | Port | Swagger URL | Description |
|---------|------|-------------|-------------|
| **API Gateway** | 8000 | `http://localhost:8000/swagger` | Unified API entry point |
| **Account Service** | 7022 | `http://localhost:7022/swagger` | User management & auth |
| **Product Service** | 7005 | `http://localhost:7005/swagger` | Product catalog |
| **Shop Service** | 7077 | `http://localhost:7077/swagger` | Shop management |
| **Cart Service** | 7228 | `http://localhost:7228/swagger` | Shopping cart |
| **Order Service** | 7135 | `http://localhost:7135/swagger` | Order processing |
| **Payment Service** | 7021 | `http://localhost:7021/swagger` | Payment processing |
| **Livestream Service** | 7041 | `http://localhost:7041/swagger` | Live streaming |
| **Delivery Service** | 7202 | `http://localhost:7202/swagger` | Shipping management |
| **Notification Service** | 7078 | `http://localhost:7078/swagger` | Notifications |

### 🧪 Testing the Platform

#### 1. Create a User Account
```bash
curl -X POST "http://localhost:8000/api/accounts" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "SecurePassword123!",
    "fullname": "Test User",
    "role": "Customer"
  }'
```

#### 2. Login and Get JWT Token
```bash
curl -X POST "http://localhost:8000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "SecurePassword123!"
  }'
```

#### 3. Create a Shop (Seller Role)
```bash
curl -X POST "http://localhost:8000/api/shops" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "name": "My Test Shop",
    "description": "A test shop for demonstration",
    "address": "123 Test Street"
  }'
```

#### 4. Add Products to Shop
```bash
curl -X POST "http://localhost:8000/api/products" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "name": "Test Product",
    "description": "A test product",
    "price": 29.99,
    "shopId": "YOUR_SHOP_ID",
    "categoryId": "CATEGORY_ID"
  }'
```

#### 5. Add to Cart
```bash
curl -X POST "http://localhost:8000/api/cart/add" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "productId": "PRODUCT_ID",
    "quantity": 2
  }'
```

#### 6. Create Order
```bash
curl -X POST "http://localhost:8000/api/orders" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "shippingAddress": "123 Delivery Street",
    "paymentMethod": "CreditCard"
  }'
```

### 🎥 LiveStreaming Setup

#### Start a Live Stream
```bash
curl -X POST "http://localhost:8000/api/livestreams/start" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "title": "My Live Shopping Stream",
    "description": "Live product demonstration",
    "shopId": "YOUR_SHOP_ID"
  }'
```

#### Join Stream Chat
Connect to SignalR hub at `http://localhost:7041/chatHub` for real-time chat functionality.

## 🏗️ Development Guidelines

### 📁 Adding a New Microservice

1. **Create Service Structure**
   ```bash
   mkdir src/YourService
   cd src/YourService
   
   # Create layered architecture following DDD principles
   dotnet new classlib -n YourService.Domain
   dotnet new classlib -n YourService.Application
   dotnet new classlib -n YourService.Infrastructure
   dotnet new webapi -n YourService.Api
   
   # Create solution file
   dotnet new sln -n YourService
   ```

2. **Add Project References**
   ```bash
   # Infrastructure → Domain + Shared.Common
   cd YourService.Infrastructure
   dotnet add reference ../YourService.Domain/YourService.Domain.csproj
   dotnet add reference ../../Shared/Shared.Common/Shared.Common.csproj
   dotnet add reference ../../Shared/Shared.Messaging/Shared.Messaging.csproj
   
   # Application → Domain + Infrastructure
   cd ../YourService.Application
   dotnet add reference ../YourService.Domain/YourService.Domain.csproj
   dotnet add reference ../YourService.Infrastructure/YourService.Infrastructure.csproj
   
   # API → Application + Shared
   cd ../YourService.Api
   dotnet add reference ../YourService.Application/YourService.Application.csproj
   dotnet add reference ../../Shared/Shared.Common/Shared.Common.csproj
   ```

3. **Add to Main Solution**
   ```bash
   cd ../../..
   dotnet sln StreamCartMicroservices.sln add src/YourService/YourService.Api/YourService.Api.csproj
   dotnet sln StreamCartMicroservices.sln add src/YourService/YourService.Application/YourService.Application.csproj
   dotnet sln StreamCartMicroservices.sln add src/YourService/YourService.Domain/YourService.Domain.csproj
   dotnet sln StreamCartMicroservices.sln add src/YourService/YourService.Infrastructure/YourService.Infrastructure.csproj
   ```

4. **Configure API Gateway**
   Update `src/ApiGateway/ApiGateway/ocelot.json`:
   ```json
   {
     "DownstreamPathTemplate": "/api/yourservice/{everything}",
     "DownstreamScheme": "http",
     "DownstreamHostAndPorts": [
       {
         "Host": "yourservice-service",
         "Port": 80
       }
     ],
     "UpstreamPathTemplate": "/api/yourservice/{everything}",
     "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
     "SwaggerKey": "yourservice"
   }
   ```

5. **Add Docker Configuration**
   Create `src/YourService/YourService.Api/Dockerfile`:
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
   WORKDIR /app
   EXPOSE 80

   FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
   WORKDIR /src

   COPY ["YourService/YourService.Api/YourService.Api.csproj", "YourService/YourService.Api/"]
   COPY ["YourService/YourService.Application/YourService.Application.csproj", "YourService/YourService.Application/"]
   COPY ["YourService/YourService.Domain/YourService.Domain.csproj", "YourService/YourService.Domain/"]
   COPY ["YourService/YourService.Infrastructure/YourService.Infrastructure.csproj", "YourService/YourService.Infrastructure/"]
   COPY ["Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
   COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]

   RUN dotnet restore "YourService/YourService.Api/YourService.Api.csproj"

   COPY . .
   WORKDIR "/src/YourService/YourService.Api"
   RUN dotnet build "YourService.Api.csproj" -c Release -o /app/build

   FROM build AS publish
   RUN dotnet publish "YourService.Api.csproj" -c Release -o /app/publish

   FROM base AS final
   WORKDIR /app
   COPY --from=publish /app/publish .
   ENTRYPOINT ["dotnet", "YourService.Api.dll"]
   ```

6. **Update Docker Compose**
   Add service to `docker-compose.yml`:
   ```yaml
   yourservice-service:
     build:
       context: ./src
       dockerfile: ./YourService/YourService.Api/Dockerfile
     ports:
       - "7XXX:80"
     environment:
       - ASPNETCORE_URLS=http://+:80
       - ASPNETCORE_ENVIRONMENT=Production
       - ConnectionStrings__PostgreSQL=${POSTGRES_CONNECTION}
       - RabbitMQ__Host=rabbitmq
       - RabbitMQ__Username=${RABBITMQ_USERNAME}
       - RabbitMQ__Password=${RABBITMQ_PASSWORD}
       - JwtSettings__SecretKey=${JWT_SECRET_KEY}
       # Add other environment variables
     depends_on:
       rabbitmq:
         condition: service_healthy
     restart: unless-stopped
     networks:
       - app-network
   ```

### 🎨 Code Standards & Architecture

#### 1. Domain Layer Structure
```csharp
// Domain/Entities/YourEntity.cs
public class YourEntity : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    
    // Private constructor for EF Core
    private YourEntity() { }
    
    // Public constructor with validation
    public YourEntity(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        Name = name;
        Description = description;
        SetCreator("system");
    }
    
    // Business methods
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty", nameof(newName));
        
        Name = newName;
        SetModifier("system");
    }
}

// Domain/Enums/YourEnum.cs
public enum YourEntityStatus
{
    Active = 1,
    Inactive = 2,
    Pending = 3
}
```

#### 2. Application Layer Structure
```csharp
// Application/DTOs/YourEntityDto.cs
public class YourEntityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Application/Commands/CreateYourEntityCommand.cs
public class CreateYourEntityCommand : IRequest<YourEntityDto>
{
    public string Name { get; set; }
    public string Description { get; set; }
}

// Application/Handlers/CreateYourEntityCommandHandler.cs
public class CreateYourEntityCommandHandler : IRequestHandler<CreateYourEntityCommand, YourEntityDto>
{
    private readonly IYourEntityRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public async Task<YourEntityDto> Handle(CreateYourEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = new YourEntity(request.Name, request.Description);
        entity.SetCreator(_currentUserService.Username);
        
        await _repository.AddAsync(entity);
        
        return new YourEntityDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt
        };
    }
}
```

#### 3. Infrastructure Layer Structure
```csharp
// Infrastructure/Repositories/YourEntityRepository.cs
public class YourEntityRepository : EfCoreGenericRepository<YourEntity>, IYourEntityRepository
{
    public YourEntityRepository(YourDbContext dbContext) : base(dbContext) { }
    
    public async Task<YourEntity?> GetByNameAsync(string name)
    {
        return await _dbSet
            .Where(e => !e.IsDeleted)
            .FirstOrDefaultAsync(e => e.Name == name);
    }
    
    public async Task<IEnumerable<YourEntity>> GetActiveEntitiesAsync()
    {
        return await _dbSet
            .Where(e => !e.IsDeleted && e.Status == YourEntityStatus.Active)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }
}

// Infrastructure/Data/YourDbContext.cs
public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }
    
    public DbSet<YourEntity> YourEntities { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<YourEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasQueryFilter(e => !e.IsDeleted); // Global soft delete filter
        });
    }
}
```

#### 4. API Layer Structure
```csharp
// Api/Controllers/YourEntityController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class YourEntityController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public YourEntityController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<YourEntityDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<YourEntityDto>>> Create([FromBody] CreateYourEntityCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<YourEntityDto>.Success(result, "Entity created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<YourEntityDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<YourEntityDto>>>> GetAll()
    {
        var query = new GetAllYourEntitiesQuery();
        var result = await _mediator.Send(query);
        return Ok(ApiResponse<IEnumerable<YourEntityDto>>.Success(result));
    }
}
```

### 🔄 Inter-Service Communication Patterns

#### HTTP Client Implementation
```csharp
// Infrastructure/Clients/YourServiceClient.cs
public interface IYourServiceClient
{
    Task<YourEntityDto?> GetEntityAsync(Guid entityId);
    Task<bool> ValidateEntityAsync(Guid entityId);
}

public class YourServiceClient : IYourServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YourServiceClient> _logger;

    public YourServiceClient(HttpClient httpClient, ILogger<YourServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<YourEntityDto?> GetEntityAsync(Guid entityId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/yourentity/{entityId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<YourEntityDto>>(json);
                return apiResponse?.Data;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching entity {EntityId}", entityId);
            return null;
        }
    }
}
```

#### Event Publishing
```csharp
// In your command handler
public class UpdateYourEntityCommandHandler : IRequestHandler<UpdateYourEntityCommand, YourEntityDto>
{
    private readonly IYourEntityRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public async Task<YourEntityDto> Handle(UpdateYourEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        entity.UpdateName(request.Name);
        
        await _repository.UpdateAsync(entity);
        
        // Publish domain event
        await _publishEndpoint.Publish(new YourEntityUpdated
        {
            EntityId = entity.Id,
            Name = entity.Name,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = _currentUserService.Username
        }, cancellationToken);
        
        return MapToDto(entity);
    }
}
```

#### Event Consumption
```csharp
// Infrastructure/Messaging/Consumers/YourEntityUpdatedConsumer.cs
public class YourEntityUpdatedConsumer : IConsumer<YourEntityUpdated>, IBaseConsumer
{
    private readonly IRelatedService _relatedService;
    private readonly ILogger<YourEntityUpdatedConsumer> _logger;

    public async Task Consume(ConsumeContext<YourEntityUpdated> context)
    {
        var eventData = context.Message;
        
        try
        {
            // Handle the event - update related data
            await _relatedService.UpdateRelatedDataAsync(eventData.EntityId, eventData.Name);
            
            _logger.LogInformation("Successfully processed YourEntityUpdated event for {EntityId}", eventData.EntityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing YourEntityUpdated event for {EntityId}", eventData.EntityId);
            throw; // Re-throw to trigger retry mechanism
        }
    }
}
```

## 🔒 Security Implementation

### JWT Configuration & Claims
The system uses enhanced JWT tokens with comprehensive claims:

```csharp
// JWT Token Structure
{
  "nameid": "user-guid-id",
  "unique_name": "username", 
  "email": "user@example.com",
  "role": "Customer|Seller|Admin|ITAdmin",
  "shopid": "shop-guid-id", // For sellers
  "iat": 1234567890,
  "exp": 1234567890,
  "iss": "StreamCartApi",
  "aud": "StreamCartClient"
}
```

### Authorization Levels & Roles

#### Role Hierarchy
```csharp
public enum UserRole
{
    Customer = 1,      // Can browse, purchase, join streams
    Seller = 2,        // Can manage shop, products, create streams
    Admin = 3,         // Can manage system, users, moderate content
    ITAdmin = 4,       // Full system access, technical operations
    DeliveryPartner = 5 // Can manage deliveries and shipments
}
```

#### Permission-Based Endpoints
```csharp
// Public endpoints (no authentication)
[AllowAnonymous]
[HttpGet("products/featured")]

// Authenticated users only
[Authorize]
[HttpGet("cart")]

// Role-specific access
[Authorize(Roles = "Seller,Admin")]
[HttpPost("products")]

// Admin-only operations
[Authorize(Roles = "Admin,ITAdmin")]
[HttpGet("analytics/system")]

// Shop owner or admin access
[Authorize]
[HttpPut("shops/{shopId}")]
// Additional authorization logic in controller
```

### API Security Headers & Middleware

```csharp
// Security middleware pipeline in each service
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseConfiguredCors();
app.UseAuthHeaderMiddleware(); // Custom auth header processing
app.UseAuthentication();
app.UseAuthorization();
```

### Cross-Origin Resource Sharing (CORS)
```csharp
// Flexible CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("https://streamcart.app", "https://admin.streamcart.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

## 📊 Monitoring & Observability

### Health Checks
Each service implements comprehensive health checks:

```http
GET /health
Response: 200 OK
{
  "status": "Healthy",
  "results": {
    "database": "Healthy",
    "rabbitmq": "Healthy",
    "external_services": "Healthy"
  },
  "totalDuration": "00:00:00.1234567"
}
```

### Logging Strategy

#### Structured Logging
```csharp
// Consistent logging across all services
_logger.LogInformation("User {UserId} created order {OrderId} for shop {ShopId}", 
    userId, orderId, shopId);

_logger.LogWarning("Payment failed for order {OrderId}: {Reason}", 
    orderId, failureReason);

_logger.LogError(exception, "Failed to process livestream event for room {RoomId}", 
    roomId);
```

#### Log Levels & Categories
- **Information**: Business operations, API calls, user actions
- **Warning**: Validation failures, business rule violations, retries
- **Error**: Exceptions, integration failures, data inconsistencies
- **Critical**: System failures, security breaches, data corruption

### Performance Monitoring

#### Background Job Monitoring
```csharp
// Quartz job monitoring in Order Service
[DisallowConcurrentExecution]
public class OrderCompletionJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var completedOrders = await ProcessAutoCompletion();
            _logger.LogInformation("Auto-completed {Count} orders in {Duration}ms", 
                completedOrders, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Order completion job failed after {Duration}ms", 
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

#### Service-to-Service Communication Monitoring
```csharp
// HTTP client monitoring with retry policies
services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://product-service");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());
```

## 🚢 Deployment & DevOps

### Docker Production Configuration

#### Multi-Stage Build Example
```dockerfile
# Production-optimized Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update && apt-get install -y curl

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore dependencies
COPY ["ServiceName/ServiceName.Api/ServiceName.Api.csproj", "ServiceName/ServiceName.Api/"]
COPY ["Shared/", "Shared/"]
RUN dotnet restore "ServiceName/ServiceName.Api/ServiceName.Api.csproj"

# Build and publish
COPY . .
WORKDIR "/src/ServiceName/ServiceName.Api"
RUN dotnet build "ServiceName.Api.csproj" -c Release -o /app/build
RUN dotnet publish "ServiceName.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ServiceName.Api.dll"]
```

#### Production Docker Compose
```yaml
version: '3.8'
services:
  api-gateway:
    build:
      context: ./src
      dockerfile: ./ApiGateway/ApiGateway/Dockerfile
    ports:
      - "8000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:80
    depends_on:
      - account-service
      - product-service
      - shop-service
    restart: unless-stopped
    networks:
      - app-network
    deploy:
      replicas: 2
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
```

### Environment Configuration

#### Production Environment Variables
```bash
# Production .env file
POSTGRES_CONNECTION=Host=prod-db.streamcart.com;Database=streamcart_prod;Username=app_user;Password=${DB_PASSWORD}
RABBITMQ_HOST=prod-rabbitmq.streamcart.com
JWT_SECRET_KEY=${PRODUCTION_JWT_SECRET}
EMAIL_API_KEY=${MAILJET_PROD_API_KEY}
APPWRITE_ENDPOINT=https://cloud.appwrite.io/v1
LIVEKIT_URL=wss://live.streamcart.com
```

#### Container Orchestration
```yaml
# Production considerations
services:
  product-service:
    deploy:
      replicas: 3
      update_config:
        parallelism: 1
        delay: 10s
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

### Scaling Strategy

#### Horizontal Scaling
- **Stateless Services**: All API services are designed to be stateless
- **Load Balancing**: API Gateway handles load distribution
- **Database Per Service**: Each service has its own database schema
- **Event-Driven**: Loose coupling through message queues

#### Performance Optimization
```csharp
// Database optimization
services.AddDbContext<ProductContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
        npgsqlOptions.CommandTimeout(30);
    });
    
    if (environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Connection pooling
services.AddDbContextPool<ProductContext>(options =>
    options.UseNpgsql(connectionString), poolSize: 128);
```

### Backup & Disaster Recovery

#### Database Backup Strategy
```bash
# Automated PostgreSQL backups
docker exec postgres-container pg_dump -U ${POSTGRES_USER} ${POSTGRES_DB} > backup_$(date +%Y%m%d_%H%M%S).sql

# MongoDB backup
docker exec mongo-container mongodump --out /backup/mongo_$(date +%Y%m%d_%H%M%S)
```

#### Service Recovery
- **Health Checks**: Automatic container restart on failure
- **Circuit Breakers**: Service isolation during failures
- **Graceful Degradation**: Fallback mechanisms for external dependencies
- **Event Replay**: Message queue persistence for event recovery

## 🤝 Contributing

We welcome contributions to the Stream Cart platform! Please follow these guidelines to ensure a smooth collaboration process.

### Development Workflow

1. **Fork the Repository**
   ```bash
   git clone https://github.com/Dacoband/stream-cart-be.git
   cd stream-cart-be
   git checkout -b feature/your-amazing-feature
   ```

2. **Follow Architecture Patterns**
   - Maintain the established Clean Architecture and DDD patterns
   - Use CQRS for all business operations
   - Implement proper error handling and validation
   - Follow the existing naming conventions

3. **Code Quality Standards**
   - Write comprehensive unit tests for business logic
   - Add integration tests for API endpoints
   - Ensure proper exception handling
   - Use dependency injection appropriately
   - Follow SOLID principles

4. **API Documentation**
   - Update Swagger documentation for new endpoints
   - Include proper response codes and models
   - Add meaningful descriptions and examples

5. **Database Changes**
   - Create Entity Framework migrations for schema changes
   - Ensure backward compatibility where possible
   - Document any breaking changes

6. **Docker & Deployment**
   - Test Docker container builds locally
   - Verify docker-compose functionality
   - Update environment variable documentation

7. **Commit Your Changes**
   ```bash
   git add .
   git commit -m "feat: add amazing feature with proper tests"
   git push origin feature/your-amazing-feature
   ```

8. **Create Pull Request**
   - Provide a clear description of changes
   - Include testing instructions
   - Reference any related issues
   - Ensure all checks pass

### Code Review Guidelines

#### What We Look For
- **Architecture Compliance**: Follows established patterns
- **Security**: Proper authorization and input validation
- **Performance**: Efficient database queries and caching
- **Maintainability**: Clean, readable, and well-documented code
- **Testing**: Adequate test coverage for new features

#### Pull Request Template
```markdown
## Description
Brief description of the changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Comments added for complex logic
- [ ] Documentation updated
- [ ] No new warnings or errors
```

### Issue Reporting

#### Bug Reports
Please include:
- **Environment**: Development/Production, OS, .NET version
- **Steps to Reproduce**: Detailed steps to reproduce the issue
- **Expected Behavior**: What should happen
- **Actual Behavior**: What actually happens
- **Logs**: Relevant error messages or logs
- **Screenshots**: If applicable

#### Feature Requests
Please include:
- **Use Case**: Business need for the feature
- **Proposed Solution**: How you envision the feature working
- **Alternatives**: Any alternative solutions considered
- **Impact**: How this affects existing functionality

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### MIT License Summary
- ✅ **Commercial Use**: You can use this software for commercial purposes
- ✅ **Modification**: You can modify the source code
- ✅ **Distribution**: You can distribute the software
- ✅ **Private Use**: You can use the software privately
- ⚠️ **Liability**: The software is provided "as is" without warranty
- ⚠️ **Attribution**: You must include the original license and copyright notice

## 🆘 Troubleshooting

### Common Issues & Solutions

#### 🔧 Database Connection Issues
```bash
# Check PostgreSQL container status
docker-compose logs postgres

# Verify connection string in .env file
echo $POSTGRES_CONNECTION

# Reset database containers
docker-compose down -v
docker-compose up postgres -d

# Wait for database to be ready
docker-compose logs -f postgres | grep "ready to accept connections"
```

#### 📨 RabbitMQ Connection Issues
```bash
# Check RabbitMQ container
docker-compose logs rabbitmq

# Access RabbitMQ Management UI
# URL: http://localhost:15672
# Default credentials: guest/guest

# Restart RabbitMQ service
docker-compose restart rabbitmq
```

#### 🔐 JWT Token Issues
```bash
# Common JWT problems and solutions:

# 1. Token expired
# Solution: Refresh token or login again

# 2. Invalid secret key
# Check JWT_SECRET_KEY in .env file (must be 256+ bits)

# 3. Claims not found
# Verify token structure and claims mapping
```

#### 🎥 LiveKit Streaming Issues
```bash
# Check LiveKit container
docker-compose logs livekit

# Verify LiveKit configuration
cat Livekit.yaml

# Test LiveKit connection
curl -f http://localhost:7880/health

# Common port conflicts
netstat -an | grep :7880
```

#### 🐳 Docker & Container Issues
```bash
# Clean up Docker resources
docker system prune -a

# Rebuild specific service
docker-compose build --no-cache account-service

# Check container logs
docker-compose logs -f service-name

# Monitor resource usage
docker stats

# Fix permission issues (Linux/Mac)
sudo chown -R $USER:$USER .
```

#### 🌐 API Gateway & Routing Issues
```bash
# Check Ocelot configuration
cat src/ApiGateway/ApiGateway/ocelot.json

# Verify service discovery
docker-compose ps

# Test direct service access
curl http://localhost:7022/health

# Check API Gateway logs
docker-compose logs api-gateway
```

#### 📦 Package & Dependency Issues
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Check for version conflicts
dotnet list package --outdated

# Update packages
dotnet add package PackageName --version x.x.x
```

### Performance Troubleshooting

#### Database Performance
```sql
-- Check slow queries (PostgreSQL)
SELECT query, mean_exec_time, calls 
FROM pg_stat_statements 
ORDER BY mean_exec_time DESC 
LIMIT 10;

-- Check database connections
SELECT count(*) FROM pg_stat_activity;
```

#### Memory & CPU Issues
```bash
# Monitor container resources
docker stats --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}"

# Check system resources
top -p $(pgrep -f dotnet)

# Analyze garbage collection
dotnet-counters monitor --process-id <pid> --counters System.Runtime
```

### Getting Help

#### Community Support
- **GitHub Issues**: [Report bugs and request features](https://github.com/Dacoband/stream-cart-be/issues)
- **Discussions**: [Community discussions and Q&A](https://github.com/Dacoband/stream-cart-be/discussions)

#### Documentation
- **API Documentation**: Available via Swagger UI on each service
- **Architecture Guide**: See the Architecture Components section above
- **Development Setup**: Follow the Getting Started guide

#### Quick Support Checklist
Before asking for help, please:
1. ✅ Check this troubleshooting section
2. ✅ Search existing GitHub issues
3. ✅ Verify your environment setup
4. ✅ Check service logs for error messages
5. ✅ Test with a minimal reproduction case

---

**Project Maintainer**: [Dacoband](https://github.com/Dacoband)  
**Contact**: For urgent issues or commercial support, please open a GitHub issue with the `urgent` label.

**⭐ If this project helped you, please consider giving it a star on GitHub!**
