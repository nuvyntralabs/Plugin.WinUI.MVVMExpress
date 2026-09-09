using Plugin.WinUI.MVVMExpress.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.Forms;

/// <summary>Linear undo / redo stack of paired actions.</summary>
public sealed class UndoStack : ObservableModel
{
    private readonly Stack<(Action Undo, Action Redo)> _undo = new();
    private readonly Stack<(Action Undo, Action Redo)> _redo = new();
    private bool _canUndo;
    private bool _canRedo;

    /// <summary>Gets a value indicating whether <see cref="Undo"/> has work.</summary>
    public bool CanUndo
    {
        get => _canUndo;
        private set => SetProperty(ref _canUndo, value);
    }

    /// <summary>Gets a value indicating whether <see cref="Redo"/> has work.</summary>
    public bool CanRedo
    {
        get => _canRedo;
        private set => SetProperty(ref _canRedo, value);
    }

    /// <summary>Records a change. Clears redo.</summary>
    /// <param name="undo">Restores the previous value.</param>
    /// <param name="redo">Re-applies the new value.</param>
    public void Push(Action undo, Action redo)
    {
        ArgumentNullException.ThrowIfNull(undo);
        ArgumentNullException.ThrowIfNull(redo);
        _undo.Push((undo, redo));
        _redo.Clear();
        RefreshFlags();
    }

    /// <summary>Applies the last undo action.</summary>
    public void Undo()
    {
        if (_undo.Count == 0)
        {
            return;
        }

        var entry = _undo.Pop();
        entry.Undo();
        _redo.Push(entry);
        RefreshFlags();
    }

    /// <summary>Applies the last redo action.</summary>
    public void Redo()
    {
        if (_redo.Count == 0)
        {
            return;
        }

        var entry = _redo.Pop();
        entry.Redo();
        _undo.Push(entry);
        RefreshFlags();
    }

    /// <summary>Drops all history.</summary>
    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
        RefreshFlags();
    }

    private void RefreshFlags()
    {
        CanUndo = _undo.Count > 0;
        CanRedo = _redo.Count > 0;
    }
}
