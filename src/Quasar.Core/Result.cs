namespace Quasar.Core;

/// <summary>
/// Represents an error produced by a domain or application operation.
/// </summary>
public readonly struct Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> struct.
    /// </summary>
    /// <param name="code">Machine friendly code describing the error.</param>
    /// <param name="message">Human readable description of the failure.</param>
    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// Gets the machine friendly identifier for the error.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the human readable description of the error.
    /// </summary>
    public string Message { get; }

    /// <inheritdoc />
    public override string ToString() => $"{Code}: {Message}";
}

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// </summary>
public readonly struct Result
{
    private Result(bool success, Error? error)
    {
        IsSuccess = success;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with a failed result; returns <see langword="null"/> when successful.
    /// </summary>
    public Error? Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed result containing the specified <paramref name="error"/>.
    /// </summary>
    public static Result Failure(Error error) => new(false, error);
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of value produced by a successful operation.</typeparam>
public readonly struct Result<T>
{
    private Result(bool success, T? value, Error? error)
    {
        IsSuccess = success;
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the value produced by a successful operation; returns <see langword="default"/> when it failed.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the error associated with a failed result; returns <see langword="null"/> when successful.
    /// </summary>
    public Error? Error { get; }

    /// <summary>
    /// Creates a successful result containing <paramref name="value"/>.
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Creates a failed result containing the specified <paramref name="error"/>.
    /// </summary>
    public static Result<T> Failure(Error error) => new(false, default, error);
}
