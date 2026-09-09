using System.ComponentModel;
using System.Runtime.CompilerServices;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.ComponentModel;

/// <summary>
/// Bindable model with property-change notification. Event args are cached by property name.
/// When an <see cref="IMainThread"/> is present, changing/changed events hop to the UI thread.
/// </summary>
public abstract class ObservableModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    private IMainThread? _notificationThread;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Optional per-instance dispatcher. Falls back to <see cref="NotificationMarshaller.Current"/>.</summary>
    protected IMainThread? NotificationThread
    {
        get => _notificationThread;
        set => _notificationThread = value;
    }

    /// <summary>
    /// Assigns <paramref name="value"/> and raises changing/changed when the value actually differs.
    /// </summary>
    /// <returns><see langword="true"/> if the value changed.</returns>
    protected bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null,
        IEqualityComparer<T>? comparer = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        comparer ??= EqualityComparer<T>.Default;
        if (comparer.Equals(field, value))
        {
            return false;
        }

        NotifyChanging(propertyName);
        field = value;
        Notify(propertyName);
        return true;
    }

    /// <summary>
    /// Assigns <paramref name="value"/> and invokes the changing/changed callbacks when the value differs.
    /// </summary>
    protected bool SetProperty<T>(
        ref T field,
        T value,
        Action<T> onChanging,
        Action<T> onChanged,
        [CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(onChanging);
        ArgumentNullException.ThrowIfNull(onChanged);
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        var comparer = EqualityComparer<T>.Default;
        if (comparer.Equals(field, value))
        {
            return false;
        }

        NotifyChanging(propertyName);
        onChanging(value);
        field = value;
        onChanged(value);
        Notify(propertyName);
        return true;
    }

    /// <summary>Raises <see cref="PropertyChanged"/> for <paramref name="propertyName"/>.</summary>
    protected void Notify([CallerMemberName] string? propertyName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        var args = PropertyEventArgsCache.ForChanged(propertyName);
        NotificationMarshaller.Raise(() => PropertyChanged?.Invoke(this, args), _notificationThread);
    }

    /// <summary>Raises <see cref="PropertyChanging"/> for <paramref name="propertyName"/>.</summary>
    protected void NotifyChanging([CallerMemberName] string? propertyName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        var args = PropertyEventArgsCache.ForChanging(propertyName);
        NotificationMarshaller.Raise(() => PropertyChanging?.Invoke(this, args), _notificationThread);
    }

    /// <summary>Raises <see cref="PropertyChanged"/> for each name in <paramref name="dependents"/>.</summary>
    protected void NotifyDependsOn(string sourceProperty, params string[] dependents)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceProperty);
        ArgumentNullException.ThrowIfNull(dependents);
        foreach (var name in dependents)
        {
            Notify(name);
        }
    }
}
