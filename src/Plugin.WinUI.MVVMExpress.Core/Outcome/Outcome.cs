namespace Plugin.WinUI.MVVMExpress.Outcome;

/// <summary>Success or failure without a payload.</summary>
public readonly struct Outcome
{
    private Outcome(bool isSuccess, ErrorInfo? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Failure details when <see cref="IsSuccess"/> is <see langword="false"/>.</summary>
    public ErrorInfo? Error { get; }

    /// <summary>Creates a success result.</summary>
    public static Outcome Success() => new(true, null);

    /// <summary>Creates a failure result.</summary>
    public static Outcome Failure(ErrorInfo error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Outcome(false, error);
    }

    /// <summary>Creates a failure result.</summary>
    public static Outcome Failure(string code, string message, Exception? exception = null)
        => Failure(new ErrorInfo(code, message, exception));
}

/// <summary>Success or failure with a payload.</summary>
/// <typeparam name="T">Payload type.</typeparam>
public readonly struct Outcome<T>
{
    private Outcome(bool isSuccess, T? value, ErrorInfo? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Payload when successful.</summary>
    public T? Value { get; }

    /// <summary>Failure details when <see cref="IsSuccess"/> is <see langword="false"/>.</summary>
    public ErrorInfo? Error { get; }

    /// <summary>Creates a success result.</summary>
    public static Outcome<T> Success(T value) => new(true, value, null);

    /// <summary>Creates a failure result.</summary>
    public static Outcome<T> Failure(ErrorInfo error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Outcome<T>(false, default, error);
    }

    /// <summary>Creates a failure result.</summary>
    public static Outcome<T> Failure(string code, string message, Exception? exception = null)
        => Failure(new ErrorInfo(code, message, exception));
}
