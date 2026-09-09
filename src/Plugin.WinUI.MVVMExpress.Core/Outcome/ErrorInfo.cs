namespace Plugin.WinUI.MVVMExpress.Outcome;

/// <summary>Structured failure information.</summary>
public sealed class ErrorInfo
{
    /// <summary>Creates an error.</summary>
    public ErrorInfo(string code, string message, Exception? exception = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(code);
        ArgumentException.ThrowIfNullOrEmpty(message);
        Code = code;
        Message = message;
        Exception = exception;
    }

    /// <summary>Stable error code (not a secret, not a localized sentence).</summary>
    public string Code { get; }

    /// <summary>User- or log-facing message. Do not put tokens here.</summary>
    public string Message { get; }

    /// <summary>Optional exception.</summary>
    public Exception? Exception { get; }

    /// <summary>Optional property validation messages.</summary>
    public IReadOnlyList<ValidationMessage>? Validation { get; init; }

    /// <summary>Optional extra fields (never passwords or tokens).</summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

/// <summary>One field-level validation message.</summary>
/// <param name="PropertyName">Property name.</param>
/// <param name="Message">Message.</param>
public sealed record ValidationMessage(string PropertyName, string Message);
