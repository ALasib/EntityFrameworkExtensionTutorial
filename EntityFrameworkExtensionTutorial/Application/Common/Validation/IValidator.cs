using System.Collections.Generic;

namespace EntityFrameworkExtensionTutorial.Application.Common.Validation;

public interface IValidator<T>
{
    ValidationResult Validate(T entity);
}

public class ValidationResult
{
    public bool IsValid => !Errors.Any();
    public List<string> Errors { get; } = new();
    
    public void AddError(string error)
    {
        Errors.Add(error);
    }
    
    public void AddErrors(IEnumerable<string> errors)
    {
        Errors.AddRange(errors);
    }
}
