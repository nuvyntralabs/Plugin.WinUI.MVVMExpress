using Plugin.WinUI.MVVMExpress.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.Composition;

/// <summary>
/// Switches child ViewModels by key without <c>INavigator</c> or replacing <c>window.Page</c>.
/// WhatsApp-style tabs are visibility on one host page, not four Shell routes.
/// </summary>
public interface ISectionHost
{
    /// <summary>Key of the visible section.</summary>
    string CurrentKey { get; }

    /// <summary>Visible section ViewModel, if any.</summary>
    IViewModel? Current { get; }

    /// <summary>Registered section keys (registration order).</summary>
    IReadOnlyList<string> Keys { get; }

    /// <summary>Shows <paramref name="key"/> and forwards appear/disappear to the children.</summary>
    Task SelectAsync(string key, CancellationToken cancellationToken = default);
}
