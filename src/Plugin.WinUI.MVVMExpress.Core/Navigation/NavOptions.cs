namespace Plugin.WinUI.MVVMExpress.Navigation;

/// <summary>Options for a single navigation request.</summary>
public sealed class NavOptions
{
    /// <summary>Pushes onto the modal stack instead of the page stack.</summary>
    public bool Modal { get; init; }

    /// <summary>Whether the host should animate. Default is <see langword="true"/>.</summary>
    public bool Animated { get; init; } = true;

    /// <summary>Replaces the current stack entry instead of pushing.</summary>
    public bool Replace { get; init; }
}
