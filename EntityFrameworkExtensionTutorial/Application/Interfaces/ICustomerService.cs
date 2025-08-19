using EntityFrameworkExtensionTutorial.Application.Common;
using EntityFrameworkExtensionTutorial.Application.DTOs;
using EntityFrameworkExtensionTutorial.Domain.Entities;

namespace EntityFrameworkExtensionTutorial.Application.Interfaces;

public interface ICustomerService
{
    // Standard CRUD operations (use EF Core)
    Task<Result<Customer>> GetCustomerByIdAsync(int id);
    Task<Result<Customer>> GetCustomerByCodeAsync(string code);
    Task<Result<IEnumerable<Customer>>> GetAllCustomersAsync();
    Task<Result<Customer>> CreateCustomerAsync(Customer customer);
    Task<Result<Customer>> UpdateCustomerAsync(Customer customer);
    Task<Result> DeleteCustomerAsync(int id);
    
    // Business operations (use EF Core for complex logic)
    Task<Result<IEnumerable<Customer>>> GetActiveCustomersAsync();
    Task<Result> UpdateCustomerStatsAsync(int customerId);
    
    // Bulk operations (use Entity Framework Extensions)
    Task<Result<BulkOperationResult>> BulkCreateCustomersAsync(IEnumerable<Customer> customers);
    Task<Result<BulkOperationResult>> BulkUpdateCustomersAsync(IEnumerable<Customer> customers);
    Task<Result<int>> BulkDeactivateInactiveCustomersAsync(DateTime lastLoginThreshold);
    Task<Result<BulkOperationResult>> BulkSyncCustomersAsync(IEnumerable<Customer> customers);
    Task<Result<BulkOperationResult>> BulkSynchronizeCustomersAsync(IEnumerable<Customer> customers);
    
    // Performance demonstration methods
    Task<Result<TimeSpan>> MeasureEfCoreInsertAsync(int count);
    Task<Result<TimeSpan>> MeasureBulkInsertAsync(int count);
    Task<Result<TimeSpan>> MeasureEfCoreUpdateAsync(int count);
    Task<Result<TimeSpan>> MeasureBulkUpdateAsync(int count);
}
