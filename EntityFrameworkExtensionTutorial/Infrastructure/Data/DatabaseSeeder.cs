using System;
using System.Collections.Generic;
using System.Linq;
using EntityFrameworkExtensionTutorial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkExtensionTutorial.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Customers.Any())
            return; // Database already seeded

        var customers = GenerateSampleCustomers();
        
        // First, save customers to get their IDs
        await context.Customers.AddRangeAsync(customers);
        await context.SaveChangesAsync();
        
        // Then create orders using the saved customer IDs
        var orders = GenerateSampleOrders(customers);
        await context.Orders.AddRangeAsync(orders);
        await context.SaveChangesAsync();
    }

    private static List<Customer> GenerateSampleCustomers()
    {
        var customers = new List<Customer>();
        var random = new Random();

        for (int i = 1; i <= 50; i++)
        {
            customers.Add(new Customer
            {
                Code = $"CUST{i:D6}",
                Name = $"Customer {i}",
                Email = $"customer{i}@example.com",
                Phone = $"+1-555-{random.Next(100, 999):D3}-{random.Next(1000, 9999):D4}",
                LastLogin = DateTime.UtcNow.AddDays(-random.Next(1, 365)),
                TotalSpent = random.Next(100, 10000),
                OrderCount = random.Next(0, 10),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
            });
        }

        return customers;
    }

    private static List<Order> GenerateSampleOrders(List<Customer> customers)
    {
        var orders = new List<Order>();
        var random = new Random();

        foreach (var customer in customers.Take(20)) // Only first 20 customers have orders
        {
            var orderCount = random.Next(1, 5);
            
            for (int i = 1; i <= orderCount; i++)
            {
                var order = new Order
                {
                    OrderNumber = $"ORD-{customer.Code}-{i:D3}",
                    OrderDate = DateTime.UtcNow.AddDays(-random.Next(1, 90)),
                    TotalAmount = random.Next(50, 500),
                    Status = GetRandomOrderStatus(random),
                    CustomerId = customer.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                };

                // Generate order items
                var itemCount = random.Next(1, 4);
                for (int j = 1; j <= itemCount; j++)
                {
                    var unitPrice = random.Next(10, 100);
                    var quantity = random.Next(1, 5);
                    
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductName = $"Product {j}",
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = unitPrice * quantity,
                        IsActive = true,
                        CreatedAt = order.CreatedAt
                    });
                }

                orders.Add(order);
            }
        }

        return orders;
    }

    private static string GetRandomOrderStatus(Random random)
    {
        var statuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
        return statuses[random.Next(statuses.Length)];
    }
}
