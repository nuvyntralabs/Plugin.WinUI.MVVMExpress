using Plugin.WinUI.MVVMExpress.Collections;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.State;

namespace Plugin.WinUI.MVVMExpress.Pagination;

/// <summary>Paged list with one collection reset per page fetch.</summary>
/// <typeparam name="T">Item type.</typeparam>
public abstract class PagedCollection<T> : ObservableModel
{
    private int _skip;
    private bool _hasMore = true;

    /// <summary>Creates a paged collection.</summary>
    /// <param name="pageSize">Items per page.</param>
    protected PagedCollection(int pageSize = 20)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        PageSize = pageSize;
    }

    /// <summary>Page size.</summary>
    public int PageSize { get; }

    /// <summary>Last page fetch state.</summary>
    public AsyncState<IReadOnlyList<T>> State { get; } = new();

    /// <summary>Accumulated items.</summary>
    public ObservableRangeCollection<T> Items { get; } = [];

    /// <summary>Gets a value indicating whether another page may exist.</summary>
    public bool HasMore
    {
        get => _hasMore;
        private set => SetProperty(ref _hasMore, value);
    }

    /// <summary>Loads the next page.</summary>
    public async Task LoadMoreAsync(CancellationToken cancellationToken = default)
    {
        var skip = _skip;
        var page = await State.LoadAsync(
            ct => FetchAsync(skip, PageSize, ct),
            cancellationToken).ConfigureAwait(false);
        Items.AddRange(page);
        _skip += page.Count;
        HasMore = page.Count == PageSize;
    }

    /// <summary>
    /// Clears and reloads from the first page.
    /// Do not call from <c>OnAppearingAsync</c> for a live list — use <see cref="SnapshotCollection{T}"/> instead.
    /// Do not pair a sync / instant fetch with <c>CollectionView.RemainingItemsThreshold</c> or <c>RefreshView</c>.
    /// </summary>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _skip = 0;
        Items.Reset();
        HasMore = true;
        await LoadMoreAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Retries the last failed load by fetching the current page again.</summary>
    public Task RetryAsync(CancellationToken cancellationToken = default)
        => LoadMoreAsync(cancellationToken);

    /// <summary>Fetches one page.</summary>
    /// <param name="skip">Items to skip.</param>
    /// <param name="take">Page size.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    protected abstract Task<IReadOnlyList<T>> FetchAsync(int skip, int take, CancellationToken cancellationToken);
}

/// <summary><see cref="PagedCollection{T}"/> backed by a delegate.</summary>
/// <typeparam name="T">Item type.</typeparam>
public sealed class DelegatePagedCollection<T> : PagedCollection<T>
{
    private readonly Func<int, int, CancellationToken, Task<IReadOnlyList<T>>> _fetch;

    /// <summary>Creates a delegate-backed collection.</summary>
    /// <param name="fetch">Page loader.</param>
    /// <param name="pageSize">Items per page.</param>
    public DelegatePagedCollection(
        Func<int, int, CancellationToken, Task<IReadOnlyList<T>>> fetch,
        int pageSize = 20)
        : base(pageSize)
    {
        ArgumentNullException.ThrowIfNull(fetch);
        _fetch = fetch;
    }

    /// <inheritdoc />
    protected override Task<IReadOnlyList<T>> FetchAsync(int skip, int take, CancellationToken cancellationToken)
        => _fetch(skip, take, cancellationToken);
}
