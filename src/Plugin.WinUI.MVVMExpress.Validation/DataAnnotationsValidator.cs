using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Plugin.WinUI.MVVMExpress.Outcome;

namespace Plugin.WinUI.MVVMExpress.Validation;

/// <summary><see cref="IValidator"/> using <see cref="Validator"/>.</summary>
[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "ILLink.Descriptors.xml and DynamicDependency root the DataAnnotations used by Validator.TryValidateObject.")]
[UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Property lookup uses the caller-supplied name; supported attributes are rooted.")]
public sealed class DataAnnotationsValidator : IValidator
{
    /// <summary>Shared instance.</summary>
    public static DataAnnotationsValidator Instance { get; } = new();

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RequiredAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(StringLengthAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MinLengthAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MaxLengthAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RangeAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RegularExpressionAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(EmailAddressAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CompareAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MustMatchAttribute))]
    static DataAnnotationsValidator()
    {
    }

    /// <inheritdoc />
    public ValidationSummary Validate(object instance)
        => ValidateAsync(instance).GetAwaiter().GetResult();

    /// <inheritdoc />
    public Task<ValidationSummary> ValidateAsync(object instance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instance);
        cancellationToken.ThrowIfCancellationRequested();
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);
        return Task.FromResult(ToSummary(results));
    }

    /// <inheritdoc />
    public Task<ValidationSummary> ValidatePropertyAsync(object instance, string propertyName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        cancellationToken.ThrowIfCancellationRequested();
        var property = instance.GetType().GetProperty(propertyName)
            ?? throw new ArgumentException($"Unknown property '{propertyName}'.", nameof(propertyName));
        var results = new List<ValidationResult>();
        Validator.TryValidateProperty(
            property.GetValue(instance),
            new ValidationContext(instance) { MemberName = propertyName },
            results);
        return Task.FromResult(ToSummary(results));
    }

    private static ValidationSummary ToSummary(List<ValidationResult> results)
    {
        var messages = results
            .Select(item => new ValidationMessage(item.MemberNames.FirstOrDefault() ?? "", item.ErrorMessage ?? ""))
            .ToArray();
        return new ValidationSummary(messages);
    }
}
