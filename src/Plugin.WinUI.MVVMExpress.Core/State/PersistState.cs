namespace Plugin.WinUI.MVVMExpress.State;

/// <summary>Marks a field for generated save/restore. Sensitive members are excluded.</summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class PersistStateAttribute : Attribute
{
    /// <summary>When <see langword="true"/>, the generator skips this field.</summary>
    public bool Sensitive { get; set; }
}

/// <summary>String bag used by generated <see cref="IPersistableViewModel"/> implementations.</summary>
public interface IStateStore
{
    /// <summary>Writes <paramref name="value"/> under <paramref name="key"/>.</summary>
    Task SaveAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>Reads a previously saved value, or <see langword="null"/>.</summary>
    Task<string?> LoadAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>In-memory <see cref="IStateStore"/> for tests and samples.</summary>
public sealed class MemoryStateStore : IStateStore
{
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task SaveAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        cancellationToken.ThrowIfCancellationRequested();
        _values[key] = value;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string?> LoadAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_values.TryGetValue(key, out var value) ? value : null);
    }
}

/// <summary>Implemented by generated partials that have <see cref="PersistStateAttribute"/> fields.</summary>
public interface IPersistableViewModel
{
    /// <summary>Writes marked fields to <paramref name="store"/>.</summary>
    Task SavePersistedStateAsync(IStateStore store, CancellationToken cancellationToken = default);

    /// <summary>Restores marked fields from <paramref name="store"/>.</summary>
    Task RestorePersistedStateAsync(IStateStore store, CancellationToken cancellationToken = default);
}

/// <summary>Calls <see cref="IPersistableViewModel"/> when the instance implements it.</summary>
public static class PersistState
{
    /// <summary>Saves when <paramref name="viewModel"/> is persistable.</summary>
    public static Task SaveAsync(object viewModel, IStateStore store, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(store);
        return viewModel is IPersistableViewModel persistable
            ? persistable.SavePersistedStateAsync(store, cancellationToken)
            : Task.CompletedTask;
    }

    /// <summary>Restores when <paramref name="viewModel"/> is persistable.</summary>
    public static Task RestoreAsync(object viewModel, IStateStore store, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(store);
        return viewModel is IPersistableViewModel persistable
            ? persistable.RestorePersistedStateAsync(store, cancellationToken)
            : Task.CompletedTask;
    }
}
