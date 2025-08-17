# AuthApiDemo - Clean Architecture Implementation

## Overview
AuthApiDemo is a secure authentication API built with ASP.NET Core using Clean Architecture principles and Dependency Injection. The application provides user registration, authentication, and user management functionality with enterprise-level security features.

## 🏗️ Architecture

This application follows **Clean Architecture** principles with clear separation of concerns across four distinct layers:

### 1. **Domain Layer** (`Domain/`)
- **Entities**: Core business entities (`User`, `UserAuth`)
- **Interfaces**: Repository and service contracts
- **Business Rules**: Entity validation and business logic methods

### 2. **Application Layer** (`Application/`)
- **Services**: Business logic implementation (`AuthService`, `UserService`)
- **DTOs**: Data Transfer Objects for external communication
- **Use Cases**: Application-specific business rules

### 3. **Infrastructure Layer** (`Infrastructure/`)
- **Data**: Entity Framework DbContext and database configuration
- **Repositories**: Data access implementations
- **Services**: External service implementations (JWT, Password Hashing)

### 4. **Presentation Layer** (`Controllers/`, `Models/`)
- **Controllers**: API endpoints and HTTP handling
- **Request/Response Models**: API contract models
- **Validation**: Input validation and model binding

## 🔐 Security Features

### Enhanced Password Security
- **PBKDF2 Key Derivation** with HMAC-SHA512
- **Cryptographically Secure Random Salt** generation
- **10,000 Iterations** for brute-force resistance
- **Configuration-based Security Keys**

### Account Security
- **Account Lockout**: Automatic lockout after 5 failed attempts
- **Lockout Duration**: 15-minute temporary lockout
- **Password Validation**: Minimum 6 characters with confirmation
- **Secure Error Handling**: No information leakage

### JWT Token Security
- **Configurable Issuer Validation**
- **Secure Token Signing** with HMAC-SHA256
- **Token Expiration**: 1-day default expiration
- **Refresh Token Support**: Secure token renewal

## 📊 Database Design

### Separated Authentication Architecture
- **Users Table**: Business data (name, email, status)
- **UserAuths Table**: Authentication data (password hash, salt, login tracking)
- **One-to-One Relationship**: Clean separation with cascade delete

```sql
-- Users table (business data)
CREATE TABLE Users (
    UserId INT PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Gender NVARCHAR(50),
    Active BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL
);

-- UserAuths table (authentication data)
CREATE TABLE UserAuths (
    UserAuthId INT PRIMARY KEY,
    UserId INT NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARBINARY(32) NOT NULL,
    PasswordSalt VARBINARY(32) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    LastLoginAt DATETIME2 NULL,
    IsLocked BIT NOT NULL DEFAULT 0,
    FailedLoginAttempts INT NOT NULL DEFAULT 0,
    LockedUntil DATETIME2 NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
```

## 🚀 API Endpoints

### Authentication Controller (`/api/Auth`)

#### **POST** `/api/Auth/register`
Register a new user account.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "securepassword123",
  "confirmPassword": "securepassword123",
  "firstName": "John",
  "lastName": "Doe",
  "gender": "Male"
}
```

**Response:**
```json
{
  "success": true,
  "message": "User registered successfully",
  "user": {
    "userId": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "user@example.com",
    "gender": "Male",
    "active": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "fullName": "John Doe"
  }
}
```

#### **POST** `/api/Auth/login`
Authenticate user and receive JWT tokens.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "securepassword123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "expiration": "2024-01-16T10:30:00Z",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "isAuthenticated": true,
  "message": "Login successful"
}
```

#### **POST** `/api/Auth/refresh-token` 🔒
Refresh JWT token using refresh token.

#### **POST** `/api/Auth/change-password` 🔒
Change user password (requires authentication).

### User Management Controller (`/api/User`) 🔒

All user management endpoints require JWT authentication.

#### **GET** `/api/User/get_users`
Get all users in the system.

#### **GET** `/api/User/get_user/{id}`
Get specific user by ID.

#### **POST** `/api/User/add_user`
Create a new user (admin function).

#### **DELETE** `/api/User/delete_user/{id}`
Delete user and associated authentication data.

#### **POST** `/api/User/activate_user/{id}`
Activate a deactivated user account.

#### **POST** `/api/User/deactivate_user/{id}`
Deactivate a user account.

#### **GET** `/api/User/active_users`
Get all active users only.

## 🔧 Configuration

### Required Configuration (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=users.db"
  },
  "AppSettings": {
    "passwordKey": "your-secure-password-key-here",
    "TokenKey": "your-secure-jwt-key-here"
  },
  "Jwt": {
    "Issuer": "AuthApiDemo"
  }
}
```

### Environment Variables (Production)
```bash
AppSettings__passwordKey="your-production-password-key"
AppSettings__TokenKey="your-production-jwt-key"
ConnectionStrings__DefaultConnection="your-production-db-connection"
```

## 🏃‍♂️ Running the Application

### Prerequisites
- .NET 9.0 SDK
- SQLite (for default configuration)

### Development Setup
```bash
# Clone the repository
git clone <repository-url>
cd AuthApiDemo

# Restore dependencies
dotnet restore

# Run the application
dotnet run
```

### Database Setup
The application uses **Entity Framework Core** with **Code First** approach. The database will be automatically created on first run.

### API Documentation
When running in development mode, Swagger UI is available at:
- `https://localhost:5001/swagger`
- `http://localhost:5000/swagger`

## 🛠️ Dependency Injection Configuration

The application uses ASP.NET Core's built-in DI container with the following service registrations:

```csharp
// Repositories
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IUserAuthRepository, UserAuthRepository>();

// Business Services
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IUserService, UserService>();

// Utility Services
services.AddScoped<IPasswordHasher, PasswordHasher>();
services.AddScoped<IJwtTokenService, JwtTokenService>();
```

## 🧪 Testing

### Manual Testing with HTTP Client
Use the included `AuthApiDemo.http` file for manual API testing:

```http
@AuthApiDemo_HostAddress = http://localhost:5063

### Register User
POST {{AuthApiDemo_HostAddress}}/api/Auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "password123",
  "confirmPassword": "password123",
  "firstName": "Test",
  "lastName": "User",
  "gender": "Other"
}
```

## 📦 Dependencies

### Core Dependencies
- **Microsoft.AspNetCore.Authentication.JwtBearer** (9.0.8)
- **Microsoft.AspNetCore.Cryptography.KeyDerivation** (9.0.5)
- **Microsoft.EntityFrameworkCore.Sqlite** (9.0.8)
- **System.IdentityModel.Tokens.Jwt** (8.13.0)

### Development Dependencies
- **Microsoft.EntityFrameworkCore.Design** (9.0.8)
- **Swashbuckle.AspNetCore** (9.0.3)

## 🎯 Clean Architecture Benefits

### 1. **Separation of Concerns**
- Each layer has a single responsibility
- Business logic is isolated from infrastructure
- Easy to test and maintain

### 2. **Dependency Inversion**
- High-level modules don't depend on low-level modules
- Both depend on abstractions (interfaces)
- Easy to swap implementations

### 3. **Testability**
- Business logic can be tested without database
- Mock implementations for testing
- Clear boundaries between layers

### 4. **Maintainability**
- Changes in one layer don't affect others
- Easy to add new features
- Clear code organization

### 5. **Scalability**
- Can scale different layers independently
- Easy to add caching, logging, etc.
- Microservices-ready architecture

## 🔒 Security Best Practices Implemented

1. **Password Security**: PBKDF2 with high iteration count
2. **Account Lockout**: Protection against brute-force attacks
3. **Input Validation**: Comprehensive request validation
4. **Secure Error Handling**: No sensitive information in errors
5. **JWT Security**: Proper token validation and expiration
6. **Data Separation**: Authentication data isolated from business data
7. **Configuration Security**: Sensitive keys in configuration files
8. **CORS Configuration**: Proper cross-origin request handling

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Follow Clean Architecture principles
4. Add tests for new features
5. Submit a pull request

## 📞 Support

For questions or issues, please create an issue in the repository or contact the development team.