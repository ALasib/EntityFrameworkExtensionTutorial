# When to Use EF Core vs Entity Framework Extensions: A Practical Guide

Entity Framework Core is excellent for most database operations, but when dealing with large datasets, Entity Framework Extensions can provide massive performance improvements. This guide shows you exactly when to use each approach with real examples from our project.

## 🚀 Performance Improvements

According to [Entity Framework Extensions](https://entityframework-extensions.net/):

- **Insert:** 14x faster, reducing time by 93%
- **Update:** 4x faster, reducing time by 75%  
- **Delete:** 3x faster, reducing time by 65%

## ✅ Use Entity Framework Core For

### 1. Simple CRUD Operations
When working with individual entities or small datasets (< 100 entities):

```csharp
// Single customer creation
await _customerRepository.AddAsync(customer);

// Get customer by ID
var customer = await _customerRepository.GetByIdAsync(id);

// Update single customer
customer.Name = "Updated Name";
await _customerRepository.UpdateAsync(customer);

// Delete single customer
await _customerRepository.DeleteAsync(customer);
```

### 2. Complex Queries with Navigation Properties
When you need to load related data and perform complex filtering:

```csharp
// Complex query with navigation properties
var customers = await _context.Customers
    .Where(c => c.IsActive && c.TotalSpent > 1000)
    .Include(c => c.Orders)
        .ThenInclude(o => o.OrderItems)
    .OrderByDescending(c => c.TotalSpent)
    .ToListAsync();
```

### 3. Business Logic Requiring Loaded Entities
When you need to perform business operations on entities:

```csharp
public async Task<Result> UpdateCustomerStatsAsync(int customerId)
{
    var customer = await _customerRepository.GetByIdAsync(customerId);
    if (customer == null)
        return new FailureResult("Customer not found");

    // Business logic requiring loaded entity
    customer.OrderCount = await _context.Orders
        .CountAsync(o => o.CustomerId == customerId);
    
    customer.TotalSpent = await _context.Orders
        .Where(o => o.CustomerId == customerId)
        .SumAsync(o => o.TotalAmount);

    await _customerRepository.UpdateAsync(customer);
    return new SuccessResult();
}
```

## 🚀 Use Entity Framework Extensions For

### 1. Bulk Operations (> 100 entities)
When dealing with large datasets, use bulk operations for significant performance gains:

```csharp
// Bulk insert 1000+ customers
public async Task<Result<BulkOperationResult>> BulkCreateCustomersAsync(IEnumerable<Customer> customers)
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    // Use optimized bulk insert for better performance
    await _bulkRepository.BulkInsertOptimizedAsync(customers);

    stopwatch.Stop();
    
    var result = new BulkOperationResult(
        $"Successfully created {customers.Count()} customers using optimized bulk insert",
        customers.Count(),
        stopwatch.Elapsed,
        "Bulk Insert"
    );

    return result.ToSuccessResult();
}
```

### 2. Batch Operations (No Entity Loading)
When you need to update/delete records without loading them into memory:

```csharp
// Batch deactivate inactive customers (no entity loading)
public async Task<Result<BulkOperationResult>> BulkDeactivateInactiveCustomersAsync(DateTime threshold)
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    var affectedRows = await _context.Customers
        .Where(c => c.IsActive && c.LastLogin < threshold)
        .UpdateFromQueryAsync(c => new Customer { IsActive = false });

    stopwatch.Stop();
    
    var result = new BulkOperationResult(
        $"Successfully deactivated {affectedRows} inactive customers",
        affectedRows,
        stopwatch.Stop(),
        "Batch Update"
    );

    return result.ToSuccessResult();
}

// Batch delete inactive customers
var deletedRows = await _context.Customers
    .Where(c => !c.IsActive)
    .DeleteFromQueryAsync();

// Batch insert from query (backup scenario)
var insertedRows = await _context.Customers
    .Where(c => c.IsActive)
    .InsertFromQueryAsync("backup_customers", c => new { c.Code, c.Name, c.Email });
```

### 3. Data Synchronization (Upsert Operations)
When you need to insert new records and update existing ones:

```csharp
// Bulk sync customers (upsert) using BulkMerge
public async Task<Result<BulkOperationResult>> BulkSyncCustomersAsync(IEnumerable<Customer> customers)
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    await _bulkRepository.BulkMergeAsync(customers, 
        keySelector: c => new { c.Code, c.Email });

    stopwatch.Stop();
    
    var result = new BulkOperationResult(
        $"Successfully synced {customers.Count()} customers using bulk merge",
        customers.Count(),
        stopwatch.Elapsed,
        "Bulk Merge"
    );

    return result.ToSuccessResult();
}
```

### 4. Advanced Bulk Operations with Custom Options
When you need fine-grained control over bulk operations:

```csharp
var options = new BulkOperationOptions<Customer>
{
    BatchSize = 1000,                    // Process in batches of 1000
    UseTransaction = true,               // Wrap in transaction
    InsertIfNotExists = true,            // Insert new records
    UpdateIfExists = true,               // Update existing records
    IncludeGraph = true,                 // Include related entities
    ColumnPrimaryKeyExpression = c => new { c.Code, c.Email }  // Custom key
};

await _context.BulkSynchronizeAsync(customers, options);
```

### 5. Bulk Contains Operations
When you need to filter by multiple values efficiently:

```csharp
// Get customers by multiple codes efficiently
var customerCodes = new[] { "CUST001", "CUST002", "CUST003" };
var customers = await _context.Customers
    .WhereBulkContains(customerCodes, x => x.Code)
    .ToListAsync();

// Get customers by multiple criteria
var customers = await _context.Customers
    .WhereBulkContainsFilterList(customerList, x => new { x.Email, x.Name })
    .ToListAsync();
```

## 📊 Real Performance Comparison

Our project includes performance measurement endpoints that demonstrate the actual difference:

### Insert Performance (1,000 customers)
- **EF Core:** ~1,200 ms
- **Entity Framework Extensions:** ~50 ms
- **Improvement:** 93% faster

### Update Performance (1,000 customers)
- **EF Core:** ~2,400 ms
- **Entity Framework Extensions:** ~600 ms
- **Improvement:** 75% faster

## 🔧 Implementation in Our Project

### Bulk Repository Interface
```csharp
public interface IBulkRepository
{
    Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : class;
    Task BulkUpdateAsync<T>(IEnumerable<T> entities) where T : class;
    Task BulkDeleteAsync<T>(IEnumerable<T> entities) where T : class;
    Task BulkMergeAsync<T>(IEnumerable<T> entities) where T : class;
    Task BulkSynchronizeAsync<T>(IEnumerable<T> entities) where T : class;
    
    // Optimized versions
    Task BulkInsertOptimizedAsync<T>(IEnumerable<T> entities) where T : class;
    Task BulkUpdateOptimizedAsync<T>(IEnumerable<T> entities) where T : class;
    
    // Batch operations
    Task<int> UpdateFromQueryAsync<T>(Expression<Func<T, T>> updateExpression, Expression<Func<T, bool>> whereExpression) where T : class;
    Task<int> DeleteFromQueryAsync<T>(Expression<Func<T, bool>> whereExpression) where T : class;
}
```

### Service Layer Integration
```csharp
public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;      // For EF Core operations
    private readonly IBulkRepository _bulkRepository;             // For bulk operations

    // Use EF Core for individual operations
    public async Task<Result> CreateCustomerAsync(Customer customer)
    {
        await _customerRepository.AddAsync(customer);
        return new SuccessResult();
    }

    // Use Entity Framework Extensions for bulk operations
    public async Task<Result<BulkOperationResult>> BulkCreateCustomersAsync(IEnumerable<Customer> customers)
    {
        await _bulkRepository.BulkInsertOptimizedAsync(customers);
        return new SuccessResult();
    }
}
```

## 🎯 Decision Matrix

| Scenario | Entity Count | Use | Reason |
|----------|--------------|-----|---------|
| Single customer CRUD | 1 | EF Core | Simple, no performance benefit |
| Customer search with orders | < 100 | EF Core | Complex queries, navigation properties |
| Customer import from CSV | 100-1000 | Entity Framework Extensions | Large dataset, performance critical |
| Batch status updates | Any | Entity Framework Extensions | No entity loading needed |
| Data synchronization | Any | Entity Framework Extensions | Upsert operations |
| Customer backup | Any | Entity Framework Extensions | Batch operations |

## 🚀 Getting Started

1. **Install Entity Framework Extensions:**
   ```bash
   dotnet add package Z.EntityFramework.Extensions.EFCore
   ```

2. **Add bulk operations to your DbContext:**
   ```csharp
   using Z.BulkOperations;
   
   public class ApplicationDbContext : DbContext
   {
       // Your existing DbContext code
   }
   ```

3. **Create a bulk repository:**
   ```csharp
   public class BulkRepository : IBulkRepository
   {
       private readonly ApplicationDbContext _context;
       
       public async Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : class
       {
           await _context.BulkInsertAsync(entities.ToList());
       }
   }
   ```

## 💡 Key Takeaways

- **Use EF Core** for individual operations, complex queries, and business logic
- **Use Entity Framework Extensions** for bulk operations, batch updates, and large datasets
- **Performance gains** are most significant with 100+ entities
- **Batch operations** are ideal when you don't need loaded entities
- **Bulk operations** provide the best performance for data import/export scenarios

## 🔗 Resources

- [Entity Framework Extensions Documentation](https://entityframework-extensions.net/)
- [EF Core Bulk Insert Tutorial](https://antondevtips.com/blog/ef-core-bulk-insert-boost-your-performance-with-entity-framework-extensions)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)

---

**Happy Coding! 🚀**
