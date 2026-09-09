namespace Plugin.WinUI.MVVMExpress.Forms;

/// <summary>Tracks unsaved edits. Used by forms and navigation guards.</summary>
public interface IDirtyState
{
    /// <summary>Gets a value indicating whether the current values differ from the accepted originals.</summary>
    bool IsDirty { get; }

    /// <summary>Treats the current values as clean (accepted).</summary>
    void MarkClean();

    /// <summary>Restores accepted originals.</summary>
    void Reset();
}
