namespace Plugin.WinUI.MVVMExpress.Connectivity;

/// <summary>Online check. Production apps should adapt Plugin.Maui.NetworkMonitor.</summary>
public interface IConnectivityProbe
{
    /// <summary>Gets a value indicating whether the app considers the network usable.</summary>
    bool IsOnline { get; }
}

/// <summary>Mutable probe for tests and samples.</summary>
public sealed class InMemoryConnectivityProbe : IConnectivityProbe
{
    /// <inheritdoc />
    public bool IsOnline { get; set; } = true;
}
