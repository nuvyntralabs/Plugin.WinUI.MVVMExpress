namespace Plugin.WinUI.MVVMExpress.Permissions;

/// <summary>Permission check. Production apps should adapt Plugin.Maui.PermissionFlow.</summary>
public interface IPermissionGate
{
    /// <summary>Ensures <paramref name="permission"/> is granted.</summary>
    Task<bool> EnsureAsync(string permission, CancellationToken cancellationToken = default);
}

/// <summary>Always-allow gate for tests and samples.</summary>
public sealed class AllowAllPermissionGate : IPermissionGate
{
    /// <summary>Shared instance.</summary>
    public static AllowAllPermissionGate Instance { get; } = new();

    /// <inheritdoc />
    public Task<bool> EnsureAsync(string permission, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(permission);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }
}

/// <summary>Configurable gate for tests.</summary>
public sealed class MemoryPermissionGate : IPermissionGate
{
    private readonly Dictionary<string, bool> _grants = new(StringComparer.Ordinal);

    /// <summary>Sets whether <paramref name="permission"/> is granted.</summary>
    public MemoryPermissionGate Set(string permission, bool granted)
    {
        ArgumentException.ThrowIfNullOrEmpty(permission);
        _grants[permission] = granted;
        return this;
    }

    /// <inheritdoc />
    public Task<bool> EnsureAsync(string permission, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(permission);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_grants.TryGetValue(permission, out var granted) && granted);
    }
}
