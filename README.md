# Entity Framework Extensions Tutorial

This project demonstrates when to use **Entity Framework Core** vs **Entity Framework Extensions** in a .NET Core Web API project, following best practices and layered architecture.

## 🎯 Project Overview

This tutorial showcases the performance differences between EF Core and Entity Framework Extensions, helping developers understand when to use each approach for optimal performance.

### Performance Improvements with Entity Framework Extensions

According to [Entity Framework Extensions](https://entityframework-extensions.net/):

- **Insert:** 14x faster, reducing time by 93%
- **Update:** 4x faster, reducing time by 75%  
- **Delete:** 3x faster, reducing time by 65%

## 🏗️ Architecture

The project follows a **Clean Architecture** pattern with proper separation of concerns:

```
EntityFrameworkExtensionTutorial/
├── Domain/                          # Domain Layer
│   ├── Entities/                    # Domain entities
│   └── Interfaces/                  # Repository interfaces
├── Application/                     # Application Layer
│   ├── DTOs/                        # Data Transfer Objects
│   ├── Interfaces/                  # Service interfaces
│   ├── Services/                    # Business logic services
│   └── Mapping/                     # AutoMapper profiles
├── Infrastructure/                  # Infrastructure Layer
│   ├── Data/                        # DbContext and configurations
│   ├── Repositories/                # Repository implementations
│   └── DependencyInjection/         # DI configuration
└── Controllers/                     # API Controllers
```

## 🚀 When to Use EF Core vs Entity Framework Extensions

### ✅ Use Entity Framework Core For:

1. **Simple CRUD Operations**
   - Single entity operations
   - Small datasets (< 100 entities)
   - Complex queries with navigation properties
   - Business logic requiring loaded entities

2. **Examples in this project:**
   ```csharp
   // Single customer creation
   await _customerRepository.AddAsync(customer);
   
   // Complex query with navigation properties
   var customers = await _context.Customers
       .Where(c => c.IsActive)
       .Include(c => c.Orders)
       .ToListAsync();
   ```

### 🚀 Use Entity Framework Extensions For:

1. **Bulk Operations**
   - Large datasets (> 100 entities)
   - Batch operations
   - Performance-critical operations
   - Data import/export scenarios
   - Complex bulk operations with custom options

2. **All Types of Bulk Methods Implemented:**
   ```csharp
   // Basic bulk operations
   await _context.BulkInsertAsync(entities, options);
   await _context.BulkUpdateAsync(entities, options);
   await _context.BulkDeleteAsync(entities, options);
   await _context.BulkMergeAsync(entities, options);
   await _context.BulkSynchronizeAsync(entities, options);
   
   // Optimized bulk operations
   await _context.BulkInsertOptimizedAsync(entities, options);
   await _context.BulkUpdateOptimizedAsync(entities, options);
   
   // Batch operations (no entity loading)
   await _context.Customers
       .Where(c => c.IsActive && c.LastLogin < threshold)
       .UpdateFromQueryAsync(c => new Customer { IsActive = false });
   
   await _context.Customers
       .Where(c => !c.IsActive)
       .DeleteFromQueryAsync();
   
   await _context.Customers
       .Where(c => c.IsActive)
       .InsertFromQueryAsync("backup_customers", c => new { c.Code, c.Name, c.Email });
   
   // Bulk contains operations
   var result = await _context.Customers
       .WhereBulkContains(customerCodes, x => x.Code)
       .ToListAsync();
   
   var result = await _context.Customers
       .WhereBulkContainsFilterList(customers, x => new { x.Email, x.Name })
       .ToListAsync();
   ```

3. **Advanced Options Available:**
   ```csharp
   var options = new BulkOperationOptions<Customer>
   {
       BatchSize = 1000,
       UseTransaction = true,
       InsertIfNotExists = true,
       UpdateIfExists = true,
       IncludeGraph = true,
       ColumnPrimaryKeyExpression = c => new { c.Code, c.Email }
   };
   ```

## 📊 Performance Comparison

The project includes performance measurement endpoints to demonstrate the difference:

- **`POST /api/customer/performance/insert-comparison`** - Compare insert performance
- **`POST /api/customer/performance/update-comparison`** - Compare update performance

### Expected Results:

| Operation | Entity Count | EF Core Time | Bulk Time | Improvement |
|-----------|--------------|--------------|-----------|-------------|
| Insert    | 1,000        | ~1,200 ms    | ~50 ms    | 93% faster  |
| Update    | 1,000        | ~2,400 ms    | ~600 ms   | 75% faster  |

## 🛠️ Technologies Used

- **.NET 7.0**
- **Entity Framework Core 7.0**
- **Entity Framework Extensions 7.20.0**
- **SQL Server LocalDB**
- **AutoMapper**
- **Scrutor** (for automatic DI registration)
- **Serilog** (for structured logging)
- **Swagger/OpenAPI**

## 🚀 Getting Started

### Prerequisites

- .NET 7.0 SDK
- SQL Server LocalDB (included with Visual Studio)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd EntityFrameworkExtensionTutorial
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Update connection string** (if needed)
   ```json
   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EntityFrameworkExtensionsTutorial;Trusted_Connection=true;MultipleActiveResultSets=true"
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Open Swagger UI**
   Navigate to `https://localhost:7000` (or the URL shown in console)

## 📚 API Endpoints

### Standard CRUD Operations (EF Core)
- `GET /api/customer/{id}` - Get customer by ID
- `GET /api/customer/code/{code}` - Get customer by code
- `GET /api/customer` - Get all customers
- `POST /api/customer` - Create customer
- `PUT /api/customer/{id}` - Update customer
- `DELETE /api/customer/{id}` - Delete customer

### Business Operations (EF Core)
- `GET /api/customer/active` - Get active customers with orders
- `POST /api/customer/{id}/update-stats` - Update customer statistics

### Bulk Operations (Entity Framework Extensions)
- `POST /api/customer/bulk-create` - Bulk create customers with optimized options
- `PUT /api/customer/bulk-update` - Bulk update customers with optimized options
- `POST /api/customer/bulk-deactivate-inactive` - Batch deactivate inactive customers using UpdateFromQuery
- `POST /api/customer/bulk-sync` - Bulk sync customers (upsert) using BulkMerge
- `POST /api/customer/bulk-synchronize` - Complete data synchronization using BulkSynchronize

### Performance Comparison
- `POST /api/customer/performance/insert-comparison` - Compare insert performance
- `POST /api/customer/performance/update-comparison` - Compare update performance

## 🔧 Key Features

### 1. Layered Architecture
- **Domain Layer**: Entities and interfaces
- **Application Layer**: Business logic, DTOs, and validation
- **Infrastructure Layer**: Data access, external services, and middleware
- **API Layer**: Controllers and HTTP endpoints

### 2. SOLID Principles Implementation
- **Single Responsibility Principle (SRP)**: Each class has one reason to change
- **Open/Closed Principle (OCP)**: Open for extension, closed for modification
- **Liskov Substitution Principle (LSP)**: Derived classes can substitute base classes
- **Interface Segregation Principle (ISP)**: Clients depend only on interfaces they use
- **Dependency Inversion Principle (DIP)**: High-level modules don't depend on low-level modules

### 3. DRY Principle (Don't Repeat Yourself)
- Centralized validation logic
- Reusable bulk operation options
- Common error handling patterns
- Shared helper methods

### 4. KISS Principle (Keep It Simple, Stupid)
- Simple validation rules
- Clear method names
- Straightforward error messages
- Easy-to-understand bulk operations

### 5. Dependency Injection with Scrutor
```csharp
// Automatic registration of repositories
services.Scan(scan => scan
    .FromAssemblyOf<Repository<object>>()
    .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());
```

### 6. Repository Pattern
- Generic repository for common operations
- Specialized repositories for specific entities
- Bulk repository for high-performance operations

### 7. AutoMapper Integration
- Clean separation between domain entities and DTOs
- Automatic mapping configuration

### 8. Structured Logging with Serilog
- Console and file logging
- Structured logging for better debugging

### 9. Centralized Error Handling
- Global exception handler middleware
- Result pattern for consistent API responses
- Custom exceptions for different error types
- Proper HTTP status codes

### 10. C# Records for DTOs
- Immutable data transfer objects
- Cleaner syntax and better performance
- Built-in value equality and deconstruction

## 📖 Learning Resources

- [Entity Framework Extensions Documentation](https://entityframework-extensions.net/)
- [EF Core Bulk Insert Tutorial](https://antondevtips.com/blog/ef-core-bulk-insert-boost-your-performance-with-entity-framework-extensions)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Scrutor Documentation](https://github.com/khellang/Scrutor)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- [ZZZ Projects](https://entityframework-extensions.net/) for Entity Framework Extensions
- [Anton Dev Tips](https://antondevtips.com/) for the tutorial inspiration
- Microsoft for Entity Framework Core

---

**Happy Coding! 🚀**
