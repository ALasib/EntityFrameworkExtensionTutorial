namespace EntityFrameworkExtensionTutorial.Application.DTOs;

public record CustomerDto(
    int Id,
    string Code,
    string Name,
    string Email,
    string Phone,
    DateTime? LastLogin,
    decimal TotalSpent,
    int OrderCount,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateCustomerDto(
    string Code,
    string Name,
    string Email,
    string Phone
);

public record UpdateCustomerDto(
    string Name,
    string Phone
);

public record PerformanceComparisonDto(
    int EntityCount,
    TimeSpan EfCoreTime,
    TimeSpan BulkTime,
    double PerformanceImprovement,
    string OperationType
);

public record BulkOperationResult(
    string Message,
    int ProcessedCount,
    TimeSpan Duration,
    string OperationType
);
