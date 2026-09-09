using System.ComponentModel;
using Plugin.WinUI.MVVMExpress.Busy;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Errors;
using Plugin.WinUI.MVVMExpress.Input;
using Plugin.WinUI.MVVMExpress.Navigation;
using Plugin.WinUI.MVVMExpress.Outcome;
using Plugin.WinUI.MVVMExpress.Threading;
using Result = Plugin.WinUI.MVVMExpress.Outcome.Outcome;

namespace Plugin.WinUI.MVVMExpress.Forms;

/// <summary>How a dirty form treats navigation away.</summary>
public enum DirtyNavigationMode
{
    /// <summary>Return <see langword="false"/> with no UI (tests / silent block).</summary>
    SilentBlock = 0,

    /// <summary>Confirm via <c>IDialogs</c> when dialogs are available.</summary>
    Confirm = 1,

    /// <summary>Allow leave even when dirty.</summary>
    Allow = 2
}

/// <summary>
/// Page ViewModel with tracked <see cref="FormField{T}"/> values, dirty navigation guard, and undo / redo.
/// </summary>
public abstract class FormViewModel : PageViewModel, IDirtyState
{
    private readonly List<IFormField> _fields = [];
    private readonly UndoStack _history = new();
    private readonly Dictionary<IFormField, object?> _changing = [];
    private readonly List<(INotifyPropertyChanged Source, PropertyChangedEventHandler Handler)> _binds = [];
    private bool _suppressHistory;
    private bool _isDirty;

    /// <summary>Creates a form ViewModel.</summary>
    protected FormViewModel(
        INavigator? navigator = null,
        IDialogs? dialogs = null,
        IErrorSink? errors = null,
        IBusyGate? busy = null,
        IMainThread? mainThread = null)
        : base(navigator, dialogs, errors, busy, mainThread)
    {
        ResetCommand = new ModelCommand(Reset, () => IsDirty);
        UndoCommand = new ModelCommand(Undo, () => CanUndo);
        RedoCommand = new ModelCommand(Redo, () => CanRedo);
        _history.PropertyChanged += OnHistoryChanged;
    }

    /// <summary>Dirty leave policy. Default confirms when <see cref="PageViewModel.Dialogs"/> is set.</summary>
    public DirtyNavigationMode DirtyNavigation { get; set; } = DirtyNavigationMode.Confirm;

    /// <summary>Tracked fields.</summary>
    public IReadOnlyList<IFormField> Fields => _fields;

    /// <inheritdoc />
    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (SetProperty(ref _isDirty, value))
            {
                ResetCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>Gets a value indicating whether <see cref="Undo"/> has work.</summary>
    public bool CanUndo => _history.CanUndo;

    /// <summary>Gets a value indicating whether <see cref="Redo"/> has work.</summary>
    public bool CanRedo => _history.CanRedo;

    /// <summary>Restores accepted originals.</summary>
    public ModelCommand ResetCommand { get; }

    /// <summary>Undoes the last field edit.</summary>
    public ModelCommand UndoCommand { get; }

    /// <summary>Redoes the last undone edit.</summary>
    public ModelCommand RedoCommand { get; }

    /// <inheritdoc />
    public override async Task<bool> CanNavigateAwayAsync(CancellationToken cancellationToken = default)
    {
        if (!IsDirty || DirtyNavigation == DirtyNavigationMode.Allow)
        {
            return true;
        }

        if (DirtyNavigation == DirtyNavigationMode.Confirm && Dialogs is { } dialogs)
        {
            return await dialogs.ConfirmAsync(
                "Discard changes?",
                "You have unsaved changes.",
                "Discard",
                "Stay",
                cancellationToken).ConfigureAwait(false);
        }

        return false;
    }

    /// <summary>
    /// Runs <paramref name="work"/> and calls <see cref="MarkClean"/> when it succeeds.
    /// Apply field errors first when <paramref name="validation"/> is invalid.
    /// </summary>
    protected async Task<Result> SubmitAsync(
        Func<CancellationToken, Task<Result>> work,
        IReadOnlyList<ValidationMessage>? validation = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        if (validation is { Count: > 0 })
        {
            ApplyFieldErrors(validation);
            return Result.Failure("E_VAL", string.Join("; ", validation.Select(static item => item.Message)));
        }

        var result = await work(cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            MarkClean();
        }

        return result;
    }

    /// <summary>Pushes validation messages onto matching <see cref="FormField{T}"/> instances.</summary>
    protected void ApplyFieldErrors(IEnumerable<ValidationMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        var grouped = messages.GroupBy(static item => item.PropertyName, StringComparer.Ordinal)
            .ToDictionary(static g => g.Key, static g => (IReadOnlyList<ValidationMessage>)g.ToArray(), StringComparer.Ordinal);
        foreach (var field in _fields)
        {
            field.SetErrors(grouped.TryGetValue(field.Name, out var list) ? list : []);
        }
    }

    /// <summary>Adds a compare error when <paramref name="left"/> and <paramref name="right"/> differ.</summary>
    protected static ValidationMessage? MustMatch<T>(FormField<T> left, FormField<T> right, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        if (EqualityComparer<T>.Default.Equals(left.Value, right.Value))
        {
            return null;
        }

        return new ValidationMessage(right.Name, message ?? "Values do not match.");
    }

    /// <inheritdoc />
    public void MarkClean()
    {
        foreach (var field in _fields)
        {
            field.MarkClean();
        }

        _history.Clear();
        RefreshDirty();
    }

    /// <inheritdoc />
    public void Reset()
    {
        _suppressHistory = true;
        try
        {
            foreach (var field in _fields)
            {
                field.Reset();
            }

            _history.Clear();
            RefreshDirty();
        }
        finally
        {
            _suppressHistory = false;
        }
    }

    /// <summary>Undoes the last recorded field change.</summary>
    public void Undo()
    {
        _suppressHistory = true;
        try
        {
            _history.Undo();
            RefreshDirty();
        }
        finally
        {
            _suppressHistory = false;
        }
    }

    /// <summary>Redoes the last undone field change.</summary>
    public void Redo()
    {
        _suppressHistory = true;
        try
        {
            _history.Redo();
            RefreshDirty();
        }
        finally
        {
            _suppressHistory = false;
        }
    }

    /// <summary>Tracks <paramref name="field"/> and records undo entries when its value changes.</summary>
    protected TField Track<TField>(TField field)
        where TField : class, IFormField
    {
        ArgumentNullException.ThrowIfNull(field);
        _fields.Add(field);
        field.PropertyChanging += OnFieldChanging;
        field.PropertyChanged += OnFieldChanged;
        RefreshDirty();
        return field;
    }

    /// <summary>Creates and tracks a <see cref="FormField{T}"/>.</summary>
    protected FormField<T> Field<T>(string name, T? original = default)
        => Track(new FormField<T>(name, original));

    /// <summary>
    /// Forwards <see cref="FormField{T}.Value"/> changes to a public property and optional <c>CanExecute</c> refresh.
    /// Use this instead of a manual <c>PropertyChanged</c> wrapper around one text box.
    /// </summary>
    /// <param name="field">Tracked field.</param>
    /// <param name="propertyName">Public property to notify (for example <c>nameof(Draft)</c>).</param>
    /// <param name="notifyCanExecute">Optional command refresh (<c>() => SendCommand.NotifyCanExecuteChanged()</c>).</param>
    public void Bind<T>(FormField<T> field, string propertyName, Action? notifyCanExecute = null)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        void OnChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is not nameof(FormField<T>.Value))
            {
                return;
            }

            Notify(propertyName);
            notifyCanExecute?.Invoke();
        }

        field.PropertyChanged += OnChanged;
        _binds.Add((field, OnChanged));
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _history.PropertyChanged -= OnHistoryChanged;
            foreach (var field in _fields)
            {
                field.PropertyChanging -= OnFieldChanging;
                field.PropertyChanged -= OnFieldChanged;
            }

            foreach (var (source, handler) in _binds)
            {
                source.PropertyChanged -= handler;
            }

            _binds.Clear();
        }

        base.Dispose(disposing);
    }

    private void OnFieldChanging(object? sender, PropertyChangingEventArgs e)
    {
        if (_suppressHistory || sender is not IFormField field || e.PropertyName != nameof(FormField<object>.Value))
        {
            return;
        }

        _changing[field] = field.BoxedValue;
    }

    private void OnFieldChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not IFormField field)
        {
            return;
        }

        if (e.PropertyName is nameof(IFormField.IsDirty) or nameof(FormField<object>.Value))
        {
            RefreshDirty();
        }

        if (_suppressHistory
            || e.PropertyName != nameof(FormField<object>.Value)
            || !_changing.Remove(field, out var previous))
        {
            return;
        }

        var next = field.BoxedValue;
        _history.Push(
            () => field.RestoreBoxed(previous),
            () => field.RestoreBoxed(next));
    }

    private void RefreshDirty()
    {
        IsDirty = _fields.Exists(static item => item.IsDirty);
    }

    private void OnHistoryChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(UndoStack.CanUndo))
        {
            Notify(nameof(CanUndo));
            UndoCommand.NotifyCanExecuteChanged();
        }
        else if (e.PropertyName is nameof(UndoStack.CanRedo))
        {
            Notify(nameof(CanRedo));
            RedoCommand.NotifyCanExecuteChanged();
        }
    }
}
