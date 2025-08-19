using System;
using System.Collections.Generic;
using System.Linq;
using EntityFrameworkExtensionTutorial.Domain.Entities;
using EntityFrameworkExtensionTutorial.Domain.Interfaces;
using EntityFrameworkExtensionTutorial.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EntityFrameworkExtensionTutorial.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByCodeAsync(string code)
    {
        // Use EF Core for simple queries - this is efficient for single entity lookups
        return await _dbSet.FirstOrDefaultAsync(c => c.Code == code);
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        // Use EF Core for simple queries
        return await _dbSet.FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
    {
        // Use EF Core for filtered queries with navigation properties
        // This is efficient when you need related data
        return await _dbSet
            .Where(c => c.IsActive)
            .Include(c => c.Orders)
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetCustomersByLastLoginAsync(DateTime lastLogin)
    {
        // Use EF Core for complex queries with multiple conditions
        return await _dbSet
            .Where(c => c.IsActive && c.LastLogin < lastLogin)
            .OrderByDescending(c => c.LastLogin)
            .ToListAsync();
    }

    public async Task UpdateCustomerStatsAsync(int customerId)
    {
        // Use EF Core for complex business logic that requires loading entities
        var customer = await _dbSet
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer != null)
        {
            customer.OrderCount = customer.Orders.Count;
            customer.TotalSpent = customer.Orders.Sum(o => o.TotalAmount);
            customer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
