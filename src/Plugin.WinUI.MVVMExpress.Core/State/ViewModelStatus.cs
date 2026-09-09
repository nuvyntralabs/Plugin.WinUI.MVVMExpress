namespace Plugin.WinUI.MVVMExpress.State;

/// <summary>Unified UI status for ViewModels and <see cref="AsyncState{T}"/>.</summary>
public enum ViewModelStatus
{
    /// <summary>No work in flight; no result yet.</summary>
    Idle = 0,

    /// <summary>Initial load.</summary>
    Loading = 1,

    /// <summary>Reload of existing data.</summary>
    Refreshing = 2,

    /// <summary>Write / submit in flight.</summary>
    Saving = 3,

    /// <summary>Succeeded with data.</summary>
    Success = 4,

    /// <summary>Succeeded with no items.</summary>
    Empty = 5,

    /// <summary>Failed.</summary>
    Error = 6,

    /// <summary>Offline.</summary>
    Offline = 7,

    /// <summary>Not authorized.</summary>
    Unauthorized = 8,

    /// <summary>Cancelled.</summary>
    Cancelled = 9
}
