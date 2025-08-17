# Separated Authentication Architecture

## Overview
This system now separates user data from authentication data, providing a clean separation of concerns while maintaining security.

## 🏗️ **Architecture Design**

### **Separation of Concerns**
- **User Model**: Contains only business/user data (name, email, gender, status)
- **UserAuth Model**: Contains only authentication data (email, password hash, salt)
- **Clean API**: Authentication and user management are handled separately

## 📊 **Data Models**

### **1. User Model (Business Data)**
```csharp
public class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Gender { get; set; }
    public bool Active { get; set; }
}
```

### **2. UserAuth Model (Authentication Data)**
```csharp
public class UserAuth
{
    public int UserAuthId { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation property to User
    public User User { get; set; }
}
```

### **3. Request/Response Models**
```csharp
// RegisterRequest - Only email and password for auth
public class RegisterRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Gender { get; set; }
}

// LoginRequest - Only email and password
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
```

## 🔄 **Database Relationships**

### **One-to-One Relationship**
- **User** ↔ **UserAuth**
- Each user has exactly one authentication record
- Cascade delete: When user is deleted, auth record is also deleted

### **Database Schema**
```sql
-- Users table (business data)
CREATE TABLE Users (
    UserId INT PRIMARY KEY,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Email NVARCHAR(255),
    Gender NVARCHAR(50),
    Active BIT
);

-- UserAuths table (authentication data)
CREATE TABLE UserAuths (
    UserAuthId INT PRIMARY KEY,
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    Email NVARCHAR(255),
    PasswordHash VARBINARY(MAX),
    PasswordSalt VARBINARY(MAX),
    CreatedAt DATETIME2,
    LastLoginAt DATETIME2
);
```

## 🚀 **API Endpoints**

### **Authentication Controller (`/api/Auth`)**
1. **POST** `/api/Auth/register` - User registration
2. **POST** `/api/Auth/login` - User authentication

### **User Management Controller (`/api/User`)**
1. **POST** `/api/User/add_user` - Add new user (admin)
2. **GET** `/api/User/get_users` - Get all users
3. **GET** `/api/User/get_user/{id}` - Get single user
4. **DELETE** `/api/User/delete_user/{id}` - Delete user

## 🔐 **Security Features**

### **Password Security**
- **PBKDF2 Key Derivation** with HMAC-SHA512
- **Random Salt Generation** using `RandomNumberGenerator.Create()`
- **10,000 Iterations** for brute-force resistance
- **Configuration-based Keys** from `appsettings.json`

### **Data Protection**
- **Separated Storage**: Authentication data stored separately from user data
- **No Password Exposure**: User endpoints never expose password information
- **Secure Hashing**: Industry-standard password hashing algorithms

## 📝 **Usage Examples**

### **User Registration Flow**
1. **Client sends**: RegisterRequest with email, password, and user details
2. **System creates**: User record with business data
3. **System creates**: UserAuth record with authentication data
4. **Response**: Success confirmation with user ID

### **User Login Flow**
1. **Client sends**: LoginRequest with email and password
2. **System finds**: UserAuth record by email
3. **System verifies**: Password using stored hash and salt
4. **System retrieves**: Associated User data for response
5. **Response**: JWT token and user information

### **User Management Flow**
1. **Admin authenticates**: Using JWT token
2. **Admin manages**: User business data only
3. **No access**: To authentication data (passwords, salts)
4. **Clean separation**: Between user management and authentication

## ✅ **Benefits of This Architecture**

### **1. Security**
- **Password isolation**: Authentication data is completely separate
- **Reduced attack surface**: User endpoints don't handle passwords
- **Better access control**: Different permissions for different data types

### **2. Maintainability**
- **Clean separation**: Business logic vs authentication logic
- **Easier testing**: Can test user management without auth concerns
- **Scalability**: Can scale authentication and user services independently

### **3. Compliance**
- **Data privacy**: Sensitive auth data is isolated
- **Audit trails**: Clear separation of authentication and business events
- **Regulatory compliance**: Easier to implement data retention policies

### **4. Development**
- **Team separation**: Different teams can work on different aspects
- **API clarity**: Clear distinction between auth and user operations
- **Code organization**: Better separation of concerns

## 🔧 **Configuration Requirements**

### **appsettings.json**
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

### **Database Migration**
- New `UserAuths` table will be created
- Existing `Users` table structure remains clean
- Foreign key relationship established

## 🎯 **Best Practices Implemented**

1. **Single Responsibility**: Each model has one clear purpose
2. **Data Isolation**: Authentication data is completely separate
3. **Secure Storage**: Passwords are properly hashed and salted
4. **Clean APIs**: Clear separation between auth and user operations
5. **Proper Relationships**: Database relationships are well-defined
6. **Error Handling**: Comprehensive error handling and validation
7. **Configuration Management**: Security keys in configuration files
