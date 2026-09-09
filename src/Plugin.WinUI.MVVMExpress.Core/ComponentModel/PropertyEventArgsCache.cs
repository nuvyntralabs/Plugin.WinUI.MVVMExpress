using System.Collections.Concurrent;
using System.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.ComponentModel;

/// <summary>
/// Caches <see cref="PropertyChangedEventArgs"/> so large-app notify loops do not allocate per raise.
/// </summary>
internal static class PropertyEventArgsCache
{
    private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> Changed = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, PropertyChangingEventArgs> Changing = new(StringComparer.Ordinal);

    internal static PropertyChangedEventArgs ForChanged(string propertyName)
        => Changed.GetOrAdd(propertyName, static name => new PropertyChangedEventArgs(name));

    internal static PropertyChangingEventArgs ForChanging(string propertyName)
        => Changing.GetOrAdd(propertyName, static name => new PropertyChangingEventArgs(name));
}
