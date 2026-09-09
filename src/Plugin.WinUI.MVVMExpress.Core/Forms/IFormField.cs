using System.ComponentModel;
using Plugin.WinUI.MVVMExpress.Outcome;

namespace Plugin.WinUI.MVVMExpress.Forms;

/// <summary>Non-generic field surface for <see cref="FormViewModel"/>.</summary>
public interface IFormField : INotifyPropertyChanged, INotifyPropertyChanging, IDirtyState
{
    /// <summary>Field name (usually a property name).</summary>
    string Name { get; }

    /// <summary>Gets a value indicating whether the user has edited the field.</summary>
    bool IsTouched { get; }

    /// <summary>Boxed current value.</summary>
    object? BoxedValue { get; }

    /// <summary>Current validation messages.</summary>
    IReadOnlyList<ValidationMessage> Errors { get; }

    /// <summary>First validation message, if any.</summary>
    string? Error { get; }

    /// <summary>Gets a value indicating whether <see cref="Errors"/> is non-empty.</summary>
    bool HasError { get; }

    /// <summary>Replaces <see cref="Errors"/>.</summary>
    void SetErrors(IReadOnlyList<ValidationMessage> errors);

    /// <summary>Assigns a boxed value without recording a user touch (undo / reset).</summary>
    void RestoreBoxed(object? value);
}
