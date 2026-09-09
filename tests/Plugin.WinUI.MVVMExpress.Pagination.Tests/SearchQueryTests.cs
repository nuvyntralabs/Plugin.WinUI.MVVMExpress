using Plugin.WinUI.MVVMExpress.Pagination;

namespace Plugin.WinUI.MVVMExpress.Pagination.Tests;

public sealed class SearchQueryTests
{
    [Fact]
    public async Task WhenReady_ReturnsFalse_WhenSuperseded()
    {
        var query = new SearchQuery(TimeSpan.FromMilliseconds(40));
        query.Text = "a";
        var first = query.WhenReadyAsync();
        query.Text = "ab";
        Assert.False(await first);
        Assert.True(await query.WhenReadyAsync());
        Assert.Equal("ab", query.Text);
    }

    [Fact]
    public async Task ZeroDebounce_IsImmediatelyReady()
    {
        var query = new SearchQuery(TimeSpan.Zero);
        query.Text = "x";
        Assert.True(await query.WhenReadyAsync());
    }

    [Fact]
    public async Task CommittedText_Updates_AfterDebounce()
    {
        var query = new SearchQuery(TimeSpan.FromMilliseconds(30));
        query.Text = "ab";
        Assert.Equal("", query.CommittedText);
        await query.WhenReadyAsync();
        await Task.Delay(40);
        Assert.Equal("ab", query.CommittedText);
    }
}
