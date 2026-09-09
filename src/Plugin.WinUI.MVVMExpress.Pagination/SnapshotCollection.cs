using Plugin.WinUI.MVVMExpress.Collections;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.State;

namespace Plugin.WinUI.MVVMExpress.Pagination;

/// <summary>
/// Load-once list for a live inbox or catalog snapshot.
/// No remaining-items threshold, no refresh-on-appear, no paging loop.
/// Seed or <see cref="LoadAsync"/> before the page binds; after appear, mutate with <see cref="AddLocal"/> instead of <see cref="Replace"/>.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
public sealed class SnapshotCollection<T> : ObservableModel
{
    private readonly Func<CancellationToken, Task<IReadOnlyList<T>>> _fetch;
    private bool _loaded;

    /// <summary>Creates a snapshot collection.</summary>
    /// <param name="fetch">Loader invoked by <see cref="LoadAsync"/>.</param>
    public SnapshotCollection(Func<CancellationToken, Task<IReadOnlyList<T>>> fetch)
    {
        ArgumentNullException.ThrowIfNull(fetch);
        _fetch = fetch;
    }

    /// <summary>Last fetch state.</summary>
    public AsyncState<IReadOnlyList<T>> State { get; } = new();

    /// <summary>Current items.</summary>
    public ObservableRangeCollection<T> Items { get; } = [];

    /// <summary>Gets a value indicating whether <see cref="LoadAsync"/> has completed once.</summary>
    public bool IsLoaded
    {
        get => _loaded;
        private set => SetProperty(ref _loaded, value);
    }

    /// <summary>
    /// Loads once. Later calls are no-ops unless <paramref name="force"/> is <see langword="true"/>.
    /// Do not pair this with <c>CollectionView.RemainingItemsThreshold</c> or <c>RefreshView</c> on a sync fetch.
    /// </summary>
    public async Task LoadAsync(bool force = false, CancellationToken cancellationToken = default)
    {
        if (_loaded && !force)
        {
            return;
        }

        var page = await State.LoadAsync(_fetch, cancellationToken).ConfigureAwait(false);
        Items.ReplaceRange(page);
        IsLoaded = true;
    }

    /// <summary>
    /// Replaces the snapshot. Unsafe on a visible Android <c>BindableLayout</c> — prefer <see cref="AddLocal"/> after appear.
    /// </summary>
    public void Replace(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        Items.ReplaceRange(items);
        IsLoaded = true;
    }

    /// <summary>Appends one item without resetting the bound collection.</summary>
    public void AddLocal(T item)
    {
        Items.Add(item);
        IsLoaded = true;
    }
}
