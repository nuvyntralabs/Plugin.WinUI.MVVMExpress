using Plugin.WinUI.MVVMExpress.Outcome;

namespace Plugin.WinUI.MVVMExpress.Validation;

/// <summary>Validates an instance or a single property.</summary>
public interface IValidator
{
    /// <summary>Validates <paramref name="instance"/>.</summary>
    /// <param name="instance">Object to validate.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task<ValidationSummary> ValidateAsync(object instance, CancellationToken cancellationToken = default);

    /// <summary>Validates one property.</summary>
    /// <param name="instance">Object to validate.</param>
    /// <param name="propertyName">Property name.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task<ValidationSummary> ValidatePropertyAsync(object instance, string propertyName, CancellationToken cancellationToken = default);

    /// <summary>Synchronous validate for completed validators such as DataAnnotations.</summary>
    /// <param name="instance">Object to validate.</param>
    ValidationSummary Validate(object instance);
}

/// <summary>Validation result.</summary>
public sealed class ValidationSummary
{
    /// <summary>Creates a summary.</summary>
    /// <param name="messages">Validation messages; empty means valid.</param>
    public ValidationSummary(IReadOnlyList<ValidationMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        Messages = messages;
        IsValid = messages.Count == 0;
    }

    /// <summary>Gets a value indicating whether there are no messages.</summary>
    public bool IsValid { get; }

    /// <summary>Messages.</summary>
    public IReadOnlyList<ValidationMessage> Messages { get; }

    /// <summary>Empty valid summary.</summary>
    public static ValidationSummary Valid { get; } = new([]);

    /// <inheritdoc />
    public override string ToString()
        => IsValid
            ? string.Empty
            : string.Join("; ", Messages.Select(item => string.IsNullOrEmpty(item.PropertyName)
                ? item.Message
                : $"{item.PropertyName}: {item.Message}"));
}
