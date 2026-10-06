# 🦟 Mosquito API

A comprehensive .NET 9 Web API for managing mosquito data with Clean Architecture, JWT authentication, and advanced user management capabilities.

## 📋 Table of Contents

- [Features](#-features)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Authentication & Authorization](#-authentication--authorization)
- [Database](#-database)
- [Configuration](#-configuration)
- [Development](#-development)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Contributing](#-contributing)

## ✨ Features

### Core Functionality
- **Mosquito Data Management**: Complete CRUD operations for mosquito records
- **Advanced Search & Filtering**: Filter by species, size, location, and dates
- **User Management**: Comprehensive user CRUD with role-based access control
- **JWT Authentication**: Secure authentication with refresh token rotation
- **Audit Logging**: Complete audit trail for all data changes
- **Health Monitoring**: Built-in health checks and monitoring

### Technical Features
- **Clean Architecture**: Domain-driven design with clear separation of concerns
- **PostgreSQL**: Dual database setup for main data and audit logs
- **Modern API Documentation**: Scalar UI for interactive API exploration
- **Comprehensive Validation**: FluentValidation with detailed error messages
- **Rate Limiting**: Configurable rate limiting for API protection
- **CORS Support**: Cross-origin resource sharing configuration
- **Security Headers**: Comprehensive security headers implementation

## 🏗️ Architecture

The project follows Clean Architecture principles with four main layers:

```
src/
├── MosquitoApi.Domain/          # Core business logic and entities
├── MosquitoApi.Application/     # Use cases and business rules
├── MosquitoApi.Infrastructure/  # Data access and external services
└── MosquitoApi.Api/            # Web API controllers and configuration
```

### Domain Layer
- **Entities**: User, Mosquito, Token, AuditLog
- **Enums**: RoleType, GenderType, MosquitoSpecies, MosquitoSize
- **Interfaces**: Repository contracts and domain services
- **Value Objects**: Location and other domain concepts

### Application Layer
- **Services**: Business logic implementation
- **DTOs**: Data transfer objects for API communication
- **Validators**: Input validation using FluentValidation
- **Mappings**: AutoMapper profiles for object mapping

### Infrastructure Layer
- **Repositories**: Data access implementations
- **Database Contexts**: Entity Framework Core contexts
- **Configurations**: Entity Framework configurations
- **Services**: External service implementations

### API Layer
- **Endpoints**: Minimal API endpoints
- **Middleware**: Custom middleware for logging, CORS, etc.
- **Extensions**: Service registration and pipeline configuration

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 13+](https://www.postgresql.org/download/)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
```bash
git clone <repository-url>
cd MosquitoApi
```

2. **Install dependencies**
```bash
dotnet restore
```

3. **Configure PostgreSQL**
```bash
# Create databases
createdb mosquito_main
createdb mosquito_audit
```

4. **Update connection strings**
   
   Edit `src/MosquitoApi.Api/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mosquito_main;Username=your_username;Password=your_password;Port=5432;",
    "AuditConnection": "Host=localhost;Database=mosquito_audit;Username=your_username;Password=your_password;Port=5432;"
  }
}
```

5. **Run database migrations**
```bash
cd src/MosquitoApi.Api
dotnet ef migrations add InitialCreate --context MosquitoDbContext --project ../MosquitoApi.Infrastructure
dotnet ef migrations add InitialCreate --context AuditDbContext --project ../MosquitoApi.Infrastructure
```

6. **Build and run the application**
```bash
dotnet build
dotnet run --project src/MosquitoApi.Api
```

The API will be available at `http://localhost:5275`

## 📚 API Documentation

### Interactive Documentation
- **Scalar UI**: `http://localhost:5275/scalar/v1` (Recommended)
- **OpenAPI Spec**: `http://localhost:5275/openapi/v1.json`

### Main Endpoints

#### Authentication (`/api/auth`)
```http
POST   /api/auth/login                    # User login
POST   /api/auth/refresh                  # Refresh tokens
POST   /api/auth/logout                   # User logout
POST   /api/auth/forgot-password          # Request password reset
POST   /api/auth/reset-password           # Reset password
POST   /api/auth/change-password          # Change password
```

#### User Management (`/api/users`)
```http
POST   /api/users                         # Create user (Admin)
GET    /api/users/{id}                    # Get user by ID
GET    /api/users/me                      # Get current user
PUT    /api/users/{id}                    # Update user
DELETE /api/users/{id}                    # Delete user (Admin)
POST   /api/users/search                  # Search users
GET    /api/users/statistics              # User statistics (Admin)
```

#### Mosquito Data (`/api/mosquitos`)
```http
GET    /api/mosquitos                     # Get all mosquitos
GET    /api/mosquitos/{id}                # Get mosquito by ID
POST   /api/mosquitos                     # Create mosquito
PUT    /api/mosquitos/{id}                # Update mosquito
DELETE /api/mosquitos/{id}                # Delete mosquito
POST   /api/mosquitos/search              # Search mosquitos
```

#### System (`/health`)
```http
GET    /health                            # Health check
GET    /health/ready                      # Readiness probe
GET    /health/live                       # Liveness probe
```

## 🔐 Authentication & Authorization

### JWT Authentication
The API uses JWT Bearer tokens with refresh token rotation:

```http
Authorization: Bearer <access_token>
```

### User Roles
- **User**: Basic access to own data
- **Admin**: User management and system administration
- **SuperAdmin**: Full system access including permanent deletions

### Example Login Flow
```bash
# 1. Login
curl -X POST http://localhost:5275/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!"
  }'

# Response includes access_token and refresh_token

# 2. Use access token for authenticated requests
curl -X GET http://localhost:5275/api/users/me \
  -H "Authorization: Bearer <access_token>"

# 3. Refresh token when expired
curl -X POST http://localhost:5275/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "<refresh_token>"
  }'
```

## 🗄️ Database

### Database Schema

The application uses two PostgreSQL databases:

#### Main Database (`mosquito_main`)
- **users**: User accounts and profiles
- **tokens**: Refresh tokens for authentication
- **mosquitos**: Mosquito observation data
- **locations**: Geographic location data

#### Audit Database (`mosquito_audit`)
- **audit_logs**: Complete audit trail of all changes
- **system_logs**: Application logging and metrics

### Database Features
- **Code-First Approach**: Entity Framework Core with migrations
- **Dual Database Architecture**: Separate databases for data and audit
- **PostgreSQL Optimizations**: JSONB columns, proper indexing, UUID keys
- **Soft Delete**: Recoverable deletion with audit trails
- **Connection Pooling**: Optimized database connections

### Sample Data
```sql
-- Create a sample user
INSERT INTO mosquito.users (id, first_name, last_name, username, email, password, role_type, is_active, created_at)
VALUES (
  gen_random_uuid(),
  'John',
  'Doe',
  'johndoe',
  'john@example.com',
  '$2a$12$encrypted_password_hash',
  'User',
  true,
  CURRENT_TIMESTAMP
);
```

## ⚙️ Configuration

### Application Settings

Key configuration options in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mosquito_main;Username=postgres;Password=password;",
    "AuditConnection": "Host=localhost;Database=mosquito_audit;Username=postgres;Password=password;"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "RateLimiting": {
    "GlobalLimitPerMinute": 100,
    "ApiLimitPerMinute": 50
  },
  "Scalar": {
    "Title": "Mosquito API Documentation",
    "Theme": "purple",
    "DarkMode": true
  }
}
```

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Set to `Development`, `Staging`, or `Production`
- `ConnectionStrings__DefaultConnection`: Override default connection string
- `ConnectionStrings__AuditConnection`: Override audit connection string
- `JwtSettings__SecretKey`: JWT signing key (required in production)

## 🛠️ Development

### Project Structure
```
MosquitoApi/
├── src/
│   ├── MosquitoApi.Api/              # Web API layer
│   ├── MosquitoApi.Application/      # Business logic layer
│   ├── MosquitoApi.Domain/           # Domain entities layer
│   └── MosquitoApi.Infrastructure/   # Data access layer
├── tests/
│   ├── MosquitoApi.Api.Tests/
│   ├── MosquitoApi.Application.Tests/
│   ├── MosquitoApi.Domain.Tests/
│   ├── MosquitoApi.Infrastructure.Tests/
│   └── MosquitoApi.IntegrationTests/
└── README.md
```

### Development Commands
```bash
# Build solution
dotnet build

# Run with hot reload
dotnet watch --project src/MosquitoApi.Api

# Run specific project
dotnet run --project src/MosquitoApi.Api

# Add migration
dotnet ef migrations add MigrationName --context MosquitoDbContext --project src/MosquitoApi.Infrastructure

# Update database
dotnet ef database update --context MosquitoDbContext --project src/MosquitoApi.Api

# Format code
dotnet format
```

### Code Quality
- **EditorConfig**: Consistent code formatting
- **Nullable Reference Types**: Enhanced null safety
- **Code Analysis**: Static analysis rules
- **FluentValidation**: Input validation
- **AutoMapper**: Object mapping
- **Serilog**: Structured logging

## 🧪 Testing

### Test Structure
```bash
tests/
├── Unit Tests/
│   ├── Domain.Tests/        # Domain logic tests
│   ├── Application.Tests/   # Service layer tests
│   └── Infrastructure.Tests/ # Repository tests
├── Integration Tests/
│   └── IntegrationTests/    # Full API tests
└── Load Tests/              # Performance tests
```

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/MosquitoApi.Domain.Tests/

# Run integration tests with TestContainers
dotnet test tests/MosquitoApi.IntegrationTests/
```

### Test Features
- **xUnit**: Testing framework
- **FluentAssertions**: Expressive assertions
- **NSubstitute**: Mocking framework
- **TestContainers**: Integration testing with real databases
- **AutoFixture**: Test data generation

## 🚢 Deployment

### Docker Support
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/build .
EXPOSE 80
ENTRYPOINT ["dotnet", "MosquitoApi.Api.dll"]
```

### Production Configuration
- Use environment-specific `appsettings.Production.json`
- Set secure JWT secret keys
- Configure production database connections
- Enable HTTPS with proper certificates
- Set up monitoring and logging
- Configure reverse proxy (nginx/Apache)

### Health Checks
The API includes comprehensive health checks:
- Database connectivity
- External service dependencies
- Memory and CPU usage
- Custom business logic health



**Built with ❤️ using .NET 9, PostgreSQL, and Clean Architecture principles**
