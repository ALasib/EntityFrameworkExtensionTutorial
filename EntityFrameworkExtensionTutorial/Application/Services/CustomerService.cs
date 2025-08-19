using System;
using System.Collections.Generic;
using System.Linq;
using EntityFrameworkExtensionTutorial.Application.Common;
using EntityFrameworkExtensionTutorial.Application.Common.Exceptions;
using EntityFrameworkExtensionTutorial.Application.Common.Validation;
using EntityFrameworkExtensionTutorial.Application.DTOs;
using EntityFrameworkExtensionTutorial.Application.Interfaces;
using EntityFrameworkExtensionTutorial.Domain.Entities;
using EntityFrameworkExtensionTutorial.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkExtensionTutorial.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IBulkRepository _bulkRepository;
    private readonly IRepository<Customer> _genericRepository;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customerRepository,
        IBulkRepository bulkRepository,
        IRepository<Customer> genericRepository,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _bulkRepository = bulkRepository;
        _genericRepository = genericRepository;
        _logger = logger;
    }

    #region Standard CRUD Operations - Use EF Core

    public async Task<Result<Customer>> GetCustomerByIdAsync(int id)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            return customer?.ToSuccessResult() ?? $"Customer with ID {id} not found".ToFailureResult<Customer>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer with ID {CustomerId}", id);
            return ex.Message.ToFailureResult<Customer>();
        }
    }

    public async Task<Result<Customer>> GetCustomerByCodeAsync(string code)
    {
        try
        {
            var customer = await _customerRepository.GetByCodeAsync(code);
            return customer?.ToSuccessResult() ?? $"Customer with code {code} not found".ToFailureResult<Customer>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer with code {CustomerCode}", code);
            return ex.Message.ToFailureResult<Customer>();
        }
    }

    public async Task<Result<IEnumerable<Customer>>> GetAllCustomersAsync()
    {
        try
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.ToSuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all customers");
            return ex.Message.ToFailureResult<IEnumerable<Customer>>();
        }
    }

    public async Task<Result<Customer>> CreateCustomerAsync(Customer customer)
    {
        try
        {
                    // Validate customer data using validator (SRP principle)
        var validator = new CustomerValidator();
        var validationResult = validator.Validate(customer);
        
        if (!validationResult.IsValid)
        {
            return string.Join("; ", validationResult.Errors).ToFailureResult<Customer>();
        }

            var createdCustomer = await _customerRepository.AddAsync(customer);
            _logger.LogInformation("Customer created successfully with ID {CustomerId}", createdCustomer.Id);
            return createdCustomer.ToSuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return ex.Message.ToFailureResult<Customer>();
        }
    }

    public async Task<Result<Customer>> UpdateCustomerAsync(Customer customer)
    {
        try
        {
            var updatedCustomer = await _customerRepository.UpdateAsync(customer);
            _logger.LogInformation("Customer updated successfully with ID {CustomerId}", updatedCustomer.Id);
            return updatedCustomer.ToSuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer with ID {CustomerId}", customer.Id);
            return ex.Message.ToFailureResult<Customer>();
        }
    }

    public async Task<Result> DeleteCustomerAsync(int id)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return new FailureResult($"Customer with ID {id} not found");
            }

            await _customerRepository.DeleteAsync(customer);
            _logger.LogInformation("Customer deleted successfully with ID {CustomerId}", id);
            return new SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer with ID {CustomerId}", id);
            return new FailureResult(ex.Message);
        }
    }

    #endregion

    #region Business Operations - Use EF Core

    public async Task<Result<IEnumerable<Customer>>> GetActiveCustomersAsync()
    {
        try
        {
            var customers = await _customerRepository.GetActiveCustomersAsync();
            return customers.ToSuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active customers");
            return ex.Message.ToFailureResult<IEnumerable<Customer>>();
        }
    }

    public async Task<Result> UpdateCustomerStatsAsync(int customerId)
    {
        try
        {
            await _customerRepository.UpdateCustomerStatsAsync(customerId);
            _logger.LogInformation("Customer statistics updated successfully for ID {CustomerId}", customerId);
            return new SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer statistics for ID {CustomerId}", customerId);
            return new FailureResult(ex.Message);
        }
    }

    #endregion

    #region Bulk Operations - Use Entity Framework Extensions

    public async Task<Result<BulkOperationResult>> BulkCreateCustomersAsync(IEnumerable<Customer> customers)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk customer creation");
            return ex.Message.ToFailureResult<BulkOperationResult>();
        }
    }

    public async Task<Result<BulkOperationResult>> BulkUpdateCustomersAsync(IEnumerable<Customer> customers)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Use optimized bulk update
            await _bulkRepository.BulkUpdateOptimizedAsync(customers);

            stopwatch.Stop();
            
            var result = new BulkOperationResult(
                $"Successfully updated {customers.Count()} customers using optimized bulk update",
                customers.Count(),
                stopwatch.Elapsed,
                "Bulk Update"
            );

            return result.ToSuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk customer update");
            return ex.Message.ToFailureResult<BulkOperationResult>();
        }
    }

    public async Task<Result<int>> BulkDeactivateInactiveCustomersAsync(DateTime lastLoginThreshold)
    {
        try
        {
            // Use batch operation for better performance
            var affectedRows = await _bulkRepository.UpdateFromQueryAsync<Customer>(
                updateExpression: c => new Customer { IsActive = false, UpdatedAt = DateTime.UtcNow },
                whereExpression: c => c.IsActive && c.LastLogin < lastLoginThreshold
            );

            _logger.LogInformation("Bulk deactivation completed, {Count} customers affected", affectedRows);
            return affectedRows.ToSuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk customer deactivation");
            return ex.Message.ToFailureResult<int>();
        }
    }

    public async Task<Result<BulkOperationResult>> BulkSyncCustomersAsync(IEnumerable<Customer> customers)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Use bulk merge with key selector for better performance
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk customer sync");
            return ex.Message.ToFailureResult<BulkOperationResult>();
        }
    }

    public async Task<Result<BulkOperationResult>> BulkSynchronizeCustomersAsync(IEnumerable<Customer> customers)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Use bulk synchronize for complete data sync
            await _bulkRepository.BulkSynchronizeAsync(customers, 
                keySelector: c => new { c.Code, c.Email });

            stopwatch.Stop();
            
            var result = new BulkOperationResult(
                $"Successfully synchronized {customers.Count()} customers using bulk synchronize",
                customers.Count(),
                stopwatch.Elapsed,
                "Bulk Synchronize"
            );

            return result.ToSuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk customer synchronization");
            return ex.Message.ToFailureResult<BulkOperationResult>();
        }
    }

    #endregion

    #region Performance Demonstration Methods

    public async Task<Result<TimeSpan>> MeasureEfCoreInsertAsync(int count)
    {
        try
        {
            var customers = GenerateTestCustomers(count);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Use EF Core for individual inserts
            foreach (var customer in customers)
            {
                await _genericRepository.AddAsync(customer);
            }

            stopwatch.Stop();
            return stopwatch.Elapsed.ToSuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error measuring EF Core insert performance");
            return ex.Message.ToFailureResult<TimeSpan>();
        }
    }

    public async Task<Result<TimeSpan>> MeasureBulkInsertAsync(int count)
    {
        try
        {
            var customers = GenerateTestCustomers(count);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Use Entity Framework Extensions for bulk insert
            await _bulkRepository.BulkInsertAsync(customers);

            stopwatch.Stop();
            return stopwatch.Elapsed.ToSuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error measuring bulk insert performance");
            return ex.Message.ToFailureResult<TimeSpan>();
        }
    }

    public async Task<Result<TimeSpan>> MeasureEfCoreUpdateAsync(int count)
    {
        try
        {
            var customers = await _genericRepository.GetAllAsync();
            var customersToUpdate = customers.Take(count).ToList();
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Use EF Core for individual updates
            foreach (var customer in customersToUpdate)
            {
                customer.UpdatedAt = DateTime.UtcNow;
                await _genericRepository.UpdateAsync(customer);
            }

            stopwatch.Stop();
            return stopwatch.Elapsed.ToSuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error measuring EF Core update performance");
            return ex.Message.ToFailureResult<TimeSpan>();
        }
    }

    public async Task<Result<TimeSpan>> MeasureBulkUpdateAsync(int count)
    {
        try
        {
            var customers = await _genericRepository.GetAllAsync();
            var customersToUpdate = customers.Take(count).ToList();
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Use Entity Framework Extensions for bulk update
            await _bulkRepository.BulkUpdateAsync(customersToUpdate);

            stopwatch.Stop();
            return stopwatch.Elapsed.ToSuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error measuring bulk update performance");
            return ex.Message.ToFailureResult<TimeSpan>();
        }
    }

    #endregion

    #region Helper Methods (DRY Principle)

    private static IEnumerable<Customer> GenerateTestCustomers(int count)
    {
        var customers = new List<Customer>();
        var random = new Random();

        for (int i = 0; i < count; i++)
        {
            customers.Add(new Customer
            {
                Code = $"CUST{i:D6}",
                Name = $"Customer {i}",
                Email = $"customer{i}@example.com",
                Phone = $"+1-555-{random.Next(100, 999):D3}-{random.Next(1000, 9999):D4}",
                LastLogin = DateTime.UtcNow.AddDays(-random.Next(1, 365)),
                TotalSpent = random.Next(100, 10000),
                OrderCount = random.Next(0, 50),
                IsActive = true
            });
        }

        return customers;
    }

    #endregion
}
