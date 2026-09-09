using Plugin.WinUI.MVVMExpress.Busy;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Errors;
using Plugin.WinUI.MVVMExpress.Input;
using Plugin.WinUI.MVVMExpress.Navigation;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Composition;

/// <summary>
/// One persistent page that switches sections in-place.
/// Bind tab buttons to <see cref="SelectCommand"/> and content visibility to <see cref="CurrentKey"/>.
/// </summary>
public class SectionHostViewModel : PageViewModel, ISectionHost
{
    private readonly Dictionary<string, IViewModel> _sections = new(StringComparer.Ordinal);
    private readonly List<string> _keys = [];
    private string _currentKey = "";

    /// <summary>Creates a section host.</summary>
    public SectionHostViewModel(
        INavigator? navigator = null,
        IDialogs? dialogs = null,
        IErrorSink? errors = null,
        IBusyGate? busy = null,
        IMainThread? mainThread = null)
        : base(navigator, dialogs, errors, busy, mainThread)
    {
        SelectCommand = new AsyncModelCommand<string>(
            (key, ct) => string.IsNullOrWhiteSpace(key) ? Task.CompletedTask : SelectAsync(key, ct),
            key => key is { Length: > 0 } && _sections.ContainsKey(key));
    }

    /// <inheritdoc />
    public string CurrentKey
    {
        get => _currentKey;
        private set
        {
            if (SetProperty(ref _currentKey, value))
            {
                Notify(nameof(Current));
            }
        }
    }

    /// <inheritdoc />
    public IViewModel? Current => _currentKey.Length > 0 && _sections.TryGetValue(_currentKey, out var section)
        ? section
        : null;

    /// <inheritdoc />
    public IReadOnlyList<string> Keys => _keys;

    /// <summary>Selects a section by key.</summary>
    public AsyncModelCommand<string> SelectCommand { get; }

    /// <summary>Registers <paramref name="section"/> under <paramref name="key"/>. The first section becomes current.</summary>
    public TSection Add<TSection>(string key, TSection section)
        where TSection : class, IViewModel
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(section);
        if (_sections.ContainsKey(key))
        {
            throw new ArgumentException($"Section '{key}' is already registered.", nameof(key));
        }

        _sections[key] = Attach(section);
        _keys.Add(key);
        Notify(nameof(Keys));
        if (_currentKey.Length == 0)
        {
            CurrentKey = key;
        }

        SelectCommand.NotifyCanExecuteChanged();
        return section;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="key"/> is the visible section.</summary>
    public bool IsCurrent(string key)
        => string.Equals(_currentKey, key, StringComparison.Ordinal);

    /// <inheritdoc />
    public async Task SelectAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (!_sections.ContainsKey(key) || string.Equals(_currentKey, key, StringComparison.Ordinal))
        {
            return;
        }

        if (Current is { } leaving)
        {
            await leaving.OnDisappearingAsync(cancellationToken).ConfigureAwait(false);
        }

        CurrentKey = key;
        if (Current is { } entering)
        {
            await entering.OnAppearingAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await InitializeChildrenAsync(cancellationToken).ConfigureAwait(false);
        if (Current is { } current)
        {
            await current.OnAppearingAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public override Task OnAppearingAsync(CancellationToken cancellationToken = default)
        => Current?.OnAppearingAsync(cancellationToken) ?? Task.CompletedTask;

    /// <inheritdoc />
    public override Task OnDisappearingAsync(CancellationToken cancellationToken = default)
        => Current?.OnDisappearingAsync(cancellationToken) ?? Task.CompletedTask;
}
