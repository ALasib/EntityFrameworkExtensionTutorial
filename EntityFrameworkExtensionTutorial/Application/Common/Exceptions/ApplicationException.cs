using System;

namespace EntityFrameworkExtensionTutorial.Application.Common.Exceptions;

public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message) : base(message)
    {
    }

    protected ApplicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class NotFoundException : ApplicationException
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}

public class ValidationException : ApplicationException
{
    public ValidationException(string message) : base(message)
    {
    }
}

public class BulkOperationException : ApplicationException
{
    public BulkOperationException(string operation, string details) 
        : base($"Bulk operation '{operation}' failed: {details}")
    {
    }
}
