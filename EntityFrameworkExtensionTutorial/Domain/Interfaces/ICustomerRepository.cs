using EntityFrameworkExtensionTutorial.Domain.Entities;

namespace EntityFrameworkExtensionTutorial.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByCodeAsync(string code);
    Task<Customer?> GetByEmailAsync(string email);
    Task<IEnumerable<Customer>> GetActiveCustomersAsync();
    Task<IEnumerable<Customer>> GetCustomersByLastLoginAsync(DateTime lastLogin);
    Task UpdateCustomerStatsAsync(int customerId);
}
