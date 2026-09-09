namespace Plugin.WinUI.MVVMExpress.Flags;

/// <summary>Feature-flag lookup. Production apps should adapt Plugin.Maui.FeatureFlags.</summary>
public interface IFeatureSwitch
{
    /// <summary>Gets a value indicating whether <paramref name="key"/> is on.</summary>
    bool IsEnabled(string key);
}

/// <summary>In-memory flags for tests and samples.</summary>
public sealed class MemoryFeatureSwitch : IFeatureSwitch
{
    private readonly Dictionary<string, bool> _flags = new(StringComparer.Ordinal);

    /// <summary>Sets <paramref name="key"/>.</summary>
    public MemoryFeatureSwitch Set(string key, bool enabled)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _flags[key] = enabled;
        return this;
    }

    /// <inheritdoc />
    public bool IsEnabled(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return _flags.TryGetValue(key, out var enabled) && enabled;
    }
}
