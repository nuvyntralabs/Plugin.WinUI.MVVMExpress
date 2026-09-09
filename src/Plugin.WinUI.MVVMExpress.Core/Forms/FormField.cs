using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Outcome;

namespace Plugin.WinUI.MVVMExpress.Forms;

/// <summary>One form value with dirty, touched, and field-level errors.</summary>
/// <typeparam name="T">Value type.</typeparam>
public sealed class FormField<T> : ObservableModel, IFormField
{
    private readonly IEqualityComparer<T> _comparer;
    private T? _value;
    private T? _original;
    private bool _isTouched;
    private IReadOnlyList<ValidationMessage> _errors = [];

    /// <summary>Creates a field.</summary>
    /// <param name="name">Property name.</param>
    /// <param name="original">Accepted starting value.</param>
    /// <param name="comparer">Optional equality comparer.</param>
    public FormField(string name, T? original = default, IEqualityComparer<T>? comparer = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Name = name;
        _comparer = comparer ?? EqualityComparer<T>.Default;
        _original = original;
        _value = original;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>Current value.</summary>
    public T? Value
    {
        get => _value;
        set => SetValue(value, touch: true);
    }

    /// <summary>Accepted original.</summary>
    public T? Original => _original;

    /// <inheritdoc />
    public bool IsDirty => !AreEqual(_value, _original);

    /// <inheritdoc />
    public bool IsTouched
    {
        get => _isTouched;
        private set => SetProperty(ref _isTouched, value);
    }

    /// <inheritdoc />
    public IReadOnlyList<ValidationMessage> Errors
    {
        get => _errors;
        private set
        {
            if (SetProperty(ref _errors, value))
            {
                Notify(nameof(Error));
                Notify(nameof(HasError));
            }
        }
    }

    /// <inheritdoc />
    public string? Error => _errors.Count == 0 ? null : _errors[0].Message;

    /// <inheritdoc />
    public bool HasError => _errors.Count > 0;

    /// <inheritdoc />
    public object? BoxedValue => Value;

    /// <inheritdoc />
    public void RestoreBoxed(object? value)
    {
        var typed = value is T typedValue ? typedValue : default;
        SetValue(typed, touch: false);
    }

    /// <summary>Assigns <paramref name="value"/> without recording a touch when <paramref name="touch"/> is false.</summary>
    public bool SetValue(T? value, bool touch)
    {
        if (AreEqual(_value, value))
        {
            return false;
        }

        NotifyChanging(nameof(Value));
        var wasDirty = IsDirty;
        _value = value;
        Notify(nameof(Value));
        if (touch)
        {
            IsTouched = true;
        }

        if (wasDirty != IsDirty)
        {
            Notify(nameof(IsDirty));
        }

        return true;
    }

    /// <inheritdoc />
    public void MarkClean()
    {
        var wasDirty = IsDirty;
        _original = _value;
        if (wasDirty)
        {
            Notify(nameof(IsDirty));
        }
    }

    /// <inheritdoc />
    public void Reset()
    {
        SetValue(_original, touch: false);
        IsTouched = false;
        SetErrors([]);
    }

    /// <inheritdoc />
    public void SetErrors(IReadOnlyList<ValidationMessage> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors;
    }

    private bool AreEqual(T? left, T? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return _comparer.Equals(left, right);
    }
}
