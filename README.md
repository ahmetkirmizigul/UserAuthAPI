# UserAuthAPI - User Authentication System

This project is a **User Authentication and Authorization API** built with .NET 9.  
It provides **JWT-based authentication** and allows secure user registration and login.

## 🚀 Features
- 🔐 **JWT-Based Authentication** (Secure login and role-based authorization)
- 🔄 **User Registration & Login**
- ⏳ **Rate Limiting** (Prevents excessive login attempts)
- ⛔ **User Ban Mechanism** (Users are permanently banned after 10 failed login attempts)
- 🎛️ **Admin Panel API** (User management and statistics)
- 📊 **User Dashboard** (Displays user-specific details based on their role)
- 📜 **Swagger/OpenAPI Support** (For API documentation and testing)

---

## 📌 Technologies Used
| Technology | Description |
|------------|------------|
| **.NET 9** | Backend framework |
| **Entity Framework Core** | ORM for database operations |
| **SQL Server** | Database |
| **JWT (JSON Web Token)** | Authentication and authorization |
| **BCrypt** | Password hashing for security |
| **Swagger (Swashbuckle)** | API documentation and testing |

---

dotnet restore
dotnet ef database update
dotnet run
