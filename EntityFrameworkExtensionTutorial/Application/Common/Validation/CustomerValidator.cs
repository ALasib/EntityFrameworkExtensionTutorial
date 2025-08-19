using System.Collections.Generic;
using EntityFrameworkExtensionTutorial.Domain.Entities;

namespace EntityFrameworkExtensionTutorial.Application.Common.Validation;

public class CustomerValidator : IValidator<Customer>
{
    public ValidationResult Validate(Customer customer)
    {
        var result = new ValidationResult();

        // Validate Code (KISS principle - simple validation)
        if (string.IsNullOrWhiteSpace(customer.Code))
        {
            result.AddError("Customer code is required");
        }
        else if (customer.Code.Length > 20)
        {
            result.AddError("Customer code cannot exceed 20 characters");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(customer.Name))
        {
            result.AddError("Customer name is required");
        }
        else if (customer.Name.Length > 100)
        {
            result.AddError("Customer name cannot exceed 100 characters");
        }

        // Validate Email
        if (string.IsNullOrWhiteSpace(customer.Email))
        {
            result.AddError("Customer email is required");
        }
        else if (customer.Email.Length > 100)
        {
            result.AddError("Customer email cannot exceed 100 characters");
        }
        else if (!IsValidEmail(customer.Email))
        {
            result.AddError("Invalid email format");
        }

        // Validate Phone
        if (!string.IsNullOrWhiteSpace(customer.Phone) && customer.Phone.Length > 20)
        {
            result.AddError("Customer phone cannot exceed 20 characters");
        }

        return result;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
