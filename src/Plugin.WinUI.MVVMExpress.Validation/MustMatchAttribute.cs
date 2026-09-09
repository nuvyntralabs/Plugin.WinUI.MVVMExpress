using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Plugin.WinUI.MVVMExpress.Validation;

/// <summary>Requires this property to equal another property (password confirm).</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class MustMatchAttribute : ValidationAttribute
{
    /// <summary>Creates the attribute.</summary>
    /// <param name="otherProperty">Other property name.</param>
    public MustMatchAttribute(string otherProperty)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(otherProperty);
        OtherProperty = otherProperty;
    }

    /// <summary>Property to compare against.</summary>
    public string OtherProperty { get; }

    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "OtherProperty is a caller-supplied name; the type is the validated instance.")]
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);
        var other = validationContext.ObjectType.GetProperty(OtherProperty)
            ?? throw new InvalidOperationException($"Unknown property '{OtherProperty}'.");
        var otherValue = other.GetValue(validationContext.ObjectInstance);
        if (Equals(value, otherValue))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            ErrorMessage ?? $"Must match {OtherProperty}.",
            [validationContext.MemberName ?? OtherProperty]);
    }
}
