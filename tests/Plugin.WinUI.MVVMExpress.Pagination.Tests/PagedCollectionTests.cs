using Plugin.WinUI.MVVMExpress.Pagination;

namespace Plugin.WinUI.MVVMExpress.Pagination.Tests;

public sealed class PagedCollectionTests
{
    [Fact]
    public async Task LoadMore_AppendsUntilExhausted()
    {
        var source = Enumerable.Range(1, 5).ToArray();
        var pages = new DelegatePagedCollection<int>(
            (skip, take, _) => Task.FromResult<IReadOnlyList<int>>(source.Skip(skip).Take(take).ToArray()),
            pageSize: 2);
        await pages.RefreshAsync();
        Assert.Equal([1, 2], pages.Items.ToArray());
        Assert.True(pages.HasMore);
        await pages.LoadMoreAsync();
        await pages.LoadMoreAsync();
        Assert.Equal([1, 2, 3, 4, 5], pages.Items.ToArray());
        Assert.False(pages.HasMore);
    }

    [Fact]
    public async Task Refresh_ResetsFromStart()
    {
        var source = Enumerable.Range(1, 3).ToArray();
        var pages = new DelegatePagedCollection<int>(
            (skip, take, _) => Task.FromResult<IReadOnlyList<int>>(source.Skip(skip).Take(take).ToArray()),
            pageSize: 2);
        await pages.RefreshAsync();
        await pages.LoadMoreAsync();
        await pages.RefreshAsync();
        Assert.Equal([1, 2], pages.Items.ToArray());
        Assert.True(pages.HasMore);
    }
}

public sealed class SnapshotCollectionTests
{
    [Fact]
    public async Task LoadAsync_IsIdempotent_UntilForced()
    {
        var fetches = 0;
        var snapshot = new SnapshotCollection<int>(_ =>
        {
            fetches++;
            return Task.FromResult<IReadOnlyList<int>>([1, 2, 3]);
        });

        await snapshot.LoadAsync();
        await snapshot.LoadAsync();
        Assert.Equal(1, fetches);
        Assert.Equal([1, 2, 3], snapshot.Items.ToArray());
        Assert.True(snapshot.IsLoaded);

        snapshot.AddLocal(4);
        Assert.Equal([1, 2, 3, 4], snapshot.Items.ToArray());

        await snapshot.LoadAsync(force: true);
        Assert.Equal(2, fetches);
        Assert.Equal([1, 2, 3], snapshot.Items.ToArray());
    }
}
