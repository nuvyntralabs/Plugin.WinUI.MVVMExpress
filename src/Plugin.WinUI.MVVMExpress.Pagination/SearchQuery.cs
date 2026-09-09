using Plugin.WinUI.MVVMExpress.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.Pagination;

/// <summary>Debounced search text. Assign <see cref="Text"/>; wait for <see cref="WhenReadyAsync"/>.</summary>
public sealed class SearchQuery : ObservableModel
{
    private string _text = "";
    private string _committed = "";
    private CancellationTokenSource? _debounce;

    /// <summary>Creates a query.</summary>
    /// <param name="debounce">Delay after the last keystroke.</param>
    /// <param name="minimumLength">Ignore shorter queries.</param>
    public SearchQuery(TimeSpan? debounce = null, int minimumLength = 0)
    {
        Debounce = debounce ?? TimeSpan.FromMilliseconds(300);
        MinimumLength = minimumLength;
    }

    /// <summary>Debounce window.</summary>
    public TimeSpan Debounce { get; }

    /// <summary>Minimum text length to search.</summary>
    public int MinimumLength { get; }

    /// <summary>
    /// Live input. Bind an <c>Entry</c> to this (not Android <c>SearchBar</c> — <c>TextChanged</c> during layout loops).
    /// Filter collections from <see cref="CommittedText"/> after debounce.
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            if (SetProperty(ref _text, value ?? ""))
            {
                RestartDebounce();
            }
        }
    }

    /// <summary>Debounced <see cref="Text"/>. Watch this for filtering; do not two-way bind it.</summary>
    public string CommittedText
    {
        get => _committed;
        private set => SetProperty(ref _committed, value);
    }

    /// <summary>Gets a value indicating whether <see cref="Text"/> meets <see cref="MinimumLength"/>.</summary>
    public bool IsReady => Text.Length >= MinimumLength;

    /// <summary>Waits until debounce elapses. Returns <see langword="false"/> when a newer query replaced this wait.</summary>
    /// <param name="cancellationToken">Caller token.</param>
    public async Task<bool> WhenReadyAsync(CancellationToken cancellationToken = default)
    {
        var source = _debounce;
        if (source is null)
        {
            return IsReady;
        }

        try
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, source.Token);
            await Task.Delay(Debounce, linked.Token).ConfigureAwait(false);
            return IsReady;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    /// <summary>Cancels an in-flight debounce.</summary>
    public void Cancel()
    {
        _debounce?.Cancel();
        _debounce?.Dispose();
        _debounce = null;
    }

    private void RestartDebounce()
    {
        Cancel();
        var source = new CancellationTokenSource();
        _debounce = source;
        _ = CommitAfterDebounceAsync(source);
    }

    private async Task CommitAfterDebounceAsync(CancellationTokenSource source)
    {
        try
        {
            await Task.Delay(Debounce, source.Token).ConfigureAwait(false);
            if (!source.IsCancellationRequested)
            {
                CommittedText = Text;
            }
        }
        catch (OperationCanceledException)
        {
            // A newer keystroke replaced this wait.
        }
    }
}
