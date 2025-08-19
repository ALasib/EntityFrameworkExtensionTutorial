using EntityFrameworkExtensionTutorial.Domain.Entities;
using System.Linq.Expressions;

namespace EntityFrameworkExtensionTutorial.Domain.Interfaces;

public interface IBulkRepository
{
    // Basic bulk operations
    Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : class;
    Task BulkUpdateAsync<T>(IEnumerable<T> entities) where T : class;
    Task BulkDeleteAsync<T>(IEnumerable<T> entities) where T : class;
    Task BulkMergeAsync<T>(IEnumerable<T> entities) where T : class;
    Task BulkSynchronizeAsync<T>(IEnumerable<T> entities) where T : class;
    
    // Batch operations (UpdateFromQuery, DeleteFromQuery, InsertFromQuery)
    Task<int> UpdateFromQueryAsync<T>(Expression<Func<T, T>> updateExpression, Expression<Func<T, bool>> whereExpression) where T : class;
    Task<int> DeleteFromQueryAsync<T>(Expression<Func<T, bool>> whereExpression) where T : class;
    Task<int> InsertFromQueryAsync<T>(string tableName, Expression<Func<T, object>> selectExpression, Expression<Func<T, bool>> whereExpression) where T : class;
    
    // Bulk contains operations
    Task<IEnumerable<Customer>> GetCustomersByBulkContainsAsync(IEnumerable<Customer> customers);
    Task<IEnumerable<Customer>> GetCustomersByBulkContainsFilterListAsync(IEnumerable<Customer> customers);
    
    // Advanced bulk operations
    Task BulkInsertOptimizedAsync<T>(IEnumerable<T> entities) where T : class;
    Task BulkUpdateOptimizedAsync<T>(IEnumerable<T> entities) where T : class;
    
    // Bulk operations with specific keys
    Task BulkMergeAsync<T>(IEnumerable<T> entities, Expression<Func<T, object>> keySelector) where T : class;
    Task BulkSynchronizeAsync<T>(IEnumerable<T> entities, Expression<Func<T, object>> keySelector) where T : class;
}
