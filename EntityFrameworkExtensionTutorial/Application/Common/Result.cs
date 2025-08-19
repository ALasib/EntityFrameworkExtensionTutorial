using System;
using System.Collections.Generic;

namespace EntityFrameworkExtensionTutorial.Application.Common;

public abstract class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public T? Value { get; }

    protected Result(bool isSuccess, T? value = default, string? error = null)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have an error message.");
        
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error message.");

        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
}

public class SuccessResult<T> : Result<T>
{
    public SuccessResult(T value) : base(true, value)
    {
    }
}

public class FailureResult<T> : Result<T>
{
    public FailureResult(string error) : base(false, default, error)
    {
    }
}

// Non-generic result for operations that don't return a value
public abstract class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected Result(bool isSuccess, string? error = null)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have an error message.");
        
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error message.");

        IsSuccess = isSuccess;
        Error = error;
    }
}

public class SuccessResult : Result
{
    public SuccessResult() : base(true)
    {
    }
}

public class FailureResult : Result
{
    public FailureResult(string error) : base(false, error)
    {
    }
}

public static class ResultExtensions
{
    public static Result<T> ToSuccessResult<T>(this T value) => new SuccessResult<T>(value);
    public static Result<T> ToFailureResult<T>(this string error) => new FailureResult<T>(error);
}
