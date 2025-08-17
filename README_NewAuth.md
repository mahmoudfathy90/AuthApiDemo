# Enhanced Authentication System

## Overview
This is a new, enhanced authentication system that provides secure user registration and login functionality with advanced security features.

## 🔐 Security Features

### 1. **Enhanced Password Security**
- **PBKDF2 Key Derivation**: Uses PBKDF2 (Password-Based Key Derivation Function 2) with HMAC-SHA512
- **Random Salt Generation**: Uses `RandomNumberGenerator.Create()` for cryptographically secure random salts
- **Configurable Iterations**: 10,000 iterations for brute-force resistance
- **App Settings Integration**: Password key retrieved from `appsettings.json`

### 2. **Password Hashing Process**
```csharp
// Generate 32-byte random salt
var salt = new byte[32];
using (var rng = RandomNumberGenerator.Create())
{
    rng.GetBytes(salt);
}

// Create hash using PBKDF2
var hash = KeyDerivation.Pbkdf2(
    password: password,
    salt: salt,
    prf: KeyDerivationPrf.HMACSHA512,
    iterationCount: 10000,
    numBytesRequested: 32
);
```

## 📱 API Endpoints

### **1. User Registration**
- **Route**: `POST /api/Auth/register`
- **Authentication**: Not required
- **Request Body**:
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

**Features**:
- Email validation
- Password confirmation
- Duplicate email check
- Secure password hashing
- User activation by default

### **2. User Login**
- **Route**: `POST /api/Auth/login`
- **Authentication**: Not required
- **Request Body**:
```json
{
  "email": "user@example.com",
  "password": "securepassword123"
}
```

**Features**:
- Email-based authentication
- Secure password verification
- Account status validation
- JWT token generation
- Last login tracking

## 🏗️ Models

### **RegisterRequest**
```csharp
public class RegisterRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Gender { get; set; }
}
```

### **LoginRequest**
```csharp
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
```

### **LoginResponse**
```csharp
public class LoginResponse
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsAuthenticated { get; set; }
    public string Message { get; set; }
}
```

## ⚙️ Configuration

### **appsettings.json Requirements**
```json
{
  "AppSettings": {
    "passwordKey": "your_secure_password_key_here",
    "TokenKey": "your_secure_token_key_here"
  },
  "Jwt": {
    "Issuer": "AuthApiDemo"
  }
}
```

## 🔒 Security Benefits

1. **PBKDF2**: Industry-standard password hashing algorithm
2. **Random Salt**: Unique salt per password prevents rainbow table attacks
3. **High Iterations**: 10,000 iterations slow down brute-force attacks
4. **Configurable Keys**: Security keys stored in configuration, not hardcoded
5. **Input Validation**: Comprehensive request validation
6. **Error Handling**: Secure error responses without information leakage
7. **Account Status**: Active/inactive user management
8. **Login Tracking**: Audit trail for user access

## 🚀 Usage Examples

### **Registration Flow**
1. Send registration request with user details
2. System validates input and checks for duplicates
3. Password is securely hashed with random salt
4. User is created and activated
5. Success response with user ID

### **Login Flow**
1. Send login request with email and password
2. System finds user by email
3. Password is verified using stored hash and salt
4. JWT token is generated with user claims
5. Last login time is updated
6. Success response with tokens and user info

## 📊 Database Schema Updates

The User model has been enhanced with:
- `Email` field for authentication
- `PasswordHash` and `PasswordSalt` for secure storage
- `CreatedAt` timestamp for audit
- `LastLoginAt` timestamp for tracking

## 🔧 Dependencies

- `Microsoft.AspNetCore.Cryptography.KeyDerivation` - For PBKDF2 password hashing
- `System.Security.Cryptography` - For random salt generation
- `Microsoft.IdentityModel.Tokens.Jwt` - For JWT token handling

## 🎯 Best Practices Implemented

1. **Never store plain text passwords**
2. **Use cryptographically secure random generators**
3. **Implement proper input validation**
4. **Provide secure error handling**
5. **Use configuration-based security keys**
6. **Implement comprehensive logging and tracking**
7. **Follow OWASP security guidelines**
