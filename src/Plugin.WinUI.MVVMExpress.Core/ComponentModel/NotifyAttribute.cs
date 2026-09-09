namespace Plugin.WinUI.MVVMExpress.ComponentModel;

/// <summary>Marks a field for a generated bindable property on a <c>partial</c> <see cref="ObservableModel"/>.</summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class NotifyAttribute : Attribute;

/// <summary>Also raises <see cref="ObservableModel.Notify"/> for <see cref="PropertyName"/> when the annotated field changes.</summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
public sealed class NotifyAlsoAttribute : Attribute
{
    /// <summary>Creates the attribute.</summary>
    /// <param name="propertyName">Dependent property name.</param>
    public NotifyAlsoAttribute(string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        PropertyName = propertyName;
    }

    /// <summary>Dependent property to notify.</summary>
    public string PropertyName { get; }
}

/// <summary>
/// Raises <see cref="ObservableModel.Notify"/> for this computed property when any listed source property changes.
/// The 80% case (<c>FullName</c> from <c>First</c> + <c>Last</c>) does not require the Reactive package.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public sealed class NotifyDependsOnAttribute : Attribute
{
    /// <summary>Creates the attribute.</summary>
    /// <param name="sourceProperties">Source property names that should refresh this property.</param>
    public NotifyDependsOnAttribute(params string[] sourceProperties)
    {
        ArgumentNullException.ThrowIfNull(sourceProperties);
        if (sourceProperties.Length == 0 || sourceProperties.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("At least one source property name is required.", nameof(sourceProperties));
        }

        SourceProperties = sourceProperties;
    }

    /// <summary>Source properties that notify this computed property.</summary>
    public string[] SourceProperties { get; }
}
