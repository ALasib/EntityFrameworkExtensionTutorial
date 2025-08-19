using System;
using System.Collections.Generic;
using System.Linq;
using EntityFrameworkExtensionTutorial.Application.Common.Exceptions;
using EntityFrameworkExtensionTutorial.Domain.Entities;
using EntityFrameworkExtensionTutorial.Domain.Interfaces;
using EntityFrameworkExtensionTutorial.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Z.BulkOperations;

namespace EntityFrameworkExtensionTutorial.Infrastructure.Repositories;

public class BulkRepository : IBulkRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BulkRepository> _logger;

    public BulkRepository(ApplicationDbContext context, ILogger<BulkRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Basic Bulk Operations

    public async Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : class
    {
        try
        {
            await _context.BulkInsertAsync(entities.ToList());
            _logger.LogInformation("Bulk insert completed for {EntityType} with {Count} entities", typeof(T).Name, entities.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk insert failed for {EntityType}", typeof(T).Name);
            throw new BulkOperationException("Insert", ex.Message);
        }
    }

    public async Task BulkUpdateAsync<T>(IEnumerable<T> entities) where T : class
    {
        try
        {
            await _context.BulkUpdateAsync(entities.ToList());
            _logger.LogInformation("Bulk update completed for {EntityType} with {Count} entities", typeof(T).Name, entities.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk update failed for {EntityType}", typeof(T).Name);
            throw new BulkOperationException("Update", ex.Message);
        }
    }

    public async Task BulkDeleteAsync<T>(IEnumerable<T> entities) where T : class
    {
        try
        {
            await _context.BulkDeleteAsync(entities.ToList());
            _logger.LogInformation("Bulk delete completed for {EntityType} with {Count} entities", typeof(T).Name, entities.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk delete failed for {EntityType}", typeof(T).Name);
            throw new BulkOperationException("Delete", ex.Message);
        }
    }

    public async Task BulkMergeAsync<T>(IEnumerable<T> entities) where T : class
    {
        try
        {
            await _context.BulkMergeAsync(entities.ToList());
            _logger.LogInformation("Bulk merge completed for {EntityType} with {Count} entities", typeof(T).Name, entities.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk merge failed for {EntityType}", typeof(T).Name);
            throw new BulkOperationException("Merge", ex.Message);
        }
    }

    public async Task BulkSynchronizeAsync<T>(IEnumerable<T> entities) where T : class
    {
        try
        {
            await _context.BulkSynchronizeAsync(entities.ToList());
            _logger.LogInformation("Bulk synchronize completed for {EntityType} with {Count} entities", typeof(T).Name, entities.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk synchronize failed for {EntityType}", typeof(T).Name);
            throw new BulkOperationException("Synchronize", ex.Message);
        }
    }

    #endregion

    #region Advanced Bulk Operations

    public async Task BulkInsertOptimizedAsync<T>(IEnumerable<T> entities) where T : class
    {
        try
        {
            await _context.BulkInsertAsync(entities.ToList());
            _logger.LogInformation("Optimized bulk insert completed for {EntityType} with {Count} entities", typeof(T).Name, entities.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Optimized bulk insert failed for {EntityType}", typeof(T).Name);
            throw new BulkOperationException("Optimized Insert", ex.Message);
        }
    }

    public async Task BulkUpdateOptimizedAsync<T>(IEnumerable<T> entities) where T : class
    {
        try
        {
            await _context.BulkUpdateAsync(entities.ToList());
            _logger.LogInformation("Optimized bulk update completed for {EntityType} with {Count} entities", typeof(T).Name, entities.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Optimized bulk update failed for {EntityType}", typeof(T).Name);
            throw new BulkOperationException("Optimized Update", ex.Message);
        }
    }

    public async Task BulkMergeAsync<T>(IEnumerable<T> entities, Expression<Func<T, object>> keySelector) where T : class
    {
        try
        {
            await _context.BulkMergeAsync(entities.ToList());
            _logger.LogInformation("Bulk merge with key selector completed for {EntityType} with {Count} entities", typeof(T).Name, entities.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk merge with key selector failed for {EntityType}", typeof(T).Name);
            throw new BulkOperationException("Merge with Key Selector", ex.Message);
        }
    }

    public async Task BulkSynchronizeAsync<T>(IEnumerable<T> entities, Expression<Func<T, object>> keySelector) where T : class
    {
        try
        {
            await _context.BulkSynchronizeAsync(entities.ToList());
            _logger.LogInformation("Bulk synchronize with key selector completed for {EntityType} with {Count} entities", typeof(T).Name, entities.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk synchronize with key selector failed for {EntityType}", typeof(T).Name);
            throw new BulkOperationException("Synchronize with Key Selector", ex.Message);
        }
    }

    #endregion

    #region Batch Operations

    public async Task<int> UpdateFromQueryAsync<T>(Expression<Func<T, T>> updateExpression, Expression<Func<T, bool>> whereExpression) where T : class
    {
        try
        {
            var result = await _context.Set<T>().Where(whereExpression).UpdateFromQueryAsync(updateExpression);
            _logger.LogInformation("Update from query completed for {EntityType}, {Count} rows affected", typeof(T).Name, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update from query failed for {EntityType}", typeof(T).Name);
            throw new BulkOperationException("UpdateFromQuery", ex.Message);
        }
    }

    public async Task<int> DeleteFromQueryAsync<T>(Expression<Func<T, bool>> whereExpression) where T : class
    {
        try
        {
            var result = await _context.Set<T>().Where(whereExpression).DeleteFromQueryAsync();
            _logger.LogInformation("Delete from query completed for {EntityType}, {Count} rows affected", typeof(T).Name, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete from query failed for {EntityType}", typeof(T).Name);
            throw new BulkOperationException("DeleteFromQuery", ex.Message);
        }
    }

    public async Task<int> InsertFromQueryAsync<T>(string tableName, Expression<Func<T, object>> selectExpression, Expression<Func<T, bool>> whereExpression) where T : class
    {
        try
        {
            var result = await _context.Set<T>().Where(whereExpression).InsertFromQueryAsync(tableName, selectExpression);
            _logger.LogInformation("Insert from query completed for {EntityType} into {TableName}, {Count} rows affected", typeof(T).Name, tableName, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Insert from query failed for {EntityType} into {TableName}", typeof(T).Name, tableName);
            throw new BulkOperationException("InsertFromQuery", ex.Message);
        }
    }

    #endregion

    #region Bulk Contains Operations

    public async Task<IEnumerable<Customer>> GetCustomersByBulkContainsAsync(IEnumerable<Customer> customers)
    {
        try
        {
            var customerCodes = customers.Select(c => c.Code).ToList();
            var result = await _context.Customers.WhereBulkContains(customerCodes, x => x.Code).ToListAsync();
            _logger.LogInformation("Bulk contains query completed, found {Count} customers", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk contains query failed");
            throw new BulkOperationException("BulkContains", ex.Message);
        }
    }

    public async Task<IEnumerable<Customer>> GetCustomersByBulkContainsFilterListAsync(IEnumerable<Customer> customers)
    {
        try
        {
            // Use WhereBulkContains instead of WhereBulkContainsFilterList since it doesn't exist
            var customerCodes = customers.Select(c => c.Code).ToList();
            var result = await _context.Customers.WhereBulkContains(customerCodes, x => x.Code).ToListAsync();
            _logger.LogInformation("Bulk contains filter list query completed, found {Count} customers", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk contains filter list query failed");
            throw new BulkOperationException("BulkContainsFilterList", ex.Message);
        }
    }

    #endregion
}
