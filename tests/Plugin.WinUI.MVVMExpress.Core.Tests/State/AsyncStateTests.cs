using Plugin.WinUI.MVVMExpress.State;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.State;

public sealed class AsyncStateTests
{
    [Fact]
    public async Task LoadAsync_Success_SetsData()
    {
        var state = new AsyncState<IReadOnlyList<int>>();
        var data = await state.LoadAsync(_ => Task.FromResult<IReadOnlyList<int>>([1, 2, 3]));
        Assert.Equal([1, 2, 3], data);
        Assert.True(state.IsSuccess);
        Assert.False(state.IsEmpty);
    }

    [Fact]
    public async Task LoadAsync_Empty_SetsEmpty()
    {
        var state = new AsyncState<IReadOnlyList<int>>();
        await state.LoadAsync(_ => Task.FromResult<IReadOnlyList<int>>([]));
        Assert.True(state.IsEmpty);
    }

    [Fact]
    public async Task LoadAsync_Cancelled_SetsCancelled()
    {
        var state = new AsyncState<int>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => state.LoadAsync(_ => Task.FromResult(1), cts.Token));
        Assert.Equal(ViewModelStatus.Cancelled, state.Status);
    }

    [Fact]
    public async Task LoadAsync_Error_SetsHasError()
    {
        var state = new AsyncState<int>();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => state.LoadAsync(_ => throw new InvalidOperationException("x")));
        Assert.True(state.HasError);
        Assert.Equal("x", state.Error);
        Assert.NotNull(state.Exception);
    }

    [Fact]
    public async Task RefreshAsync_KeepsPreviousData_UntilSuccess()
    {
        var state = new AsyncState<int>();
        await state.LoadAsync(_ => Task.FromResult(1));
        var sawRefreshing = false;
        state.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AsyncState<int>.Status) && state.IsRefreshing)
            {
                sawRefreshing = true;
                Assert.Equal(1, state.Data);
            }
        };

        await state.RefreshAsync(_ => Task.FromResult(2));
        Assert.True(sawRefreshing);
        Assert.Equal(2, state.Data);
        Assert.True(state.IsSuccess);
    }

    [Fact]
    public async Task LoadAsync_NullLoader_Throws()
    {
        var state = new AsyncState<int>();
        await Assert.ThrowsAsync<ArgumentNullException>(() => state.LoadAsync(null!));
    }

    [Fact]
    public async Task LoadAsync_NullPayload_SetsEmpty()
    {
        var state = new AsyncState<string?>();
        await state.LoadAsync(_ => Task.FromResult<string?>(null));
        Assert.True(state.IsEmpty);
        Assert.Null(state.Data);
    }

    [Fact]
    public async Task RefreshAsync_Error_KeepsPreviousData()
    {
        var state = new AsyncState<int>();
        await state.LoadAsync(_ => Task.FromResult(7));
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => state.RefreshAsync(_ => throw new InvalidOperationException("fail")));
        Assert.Equal(7, state.Data);
        Assert.True(state.HasError);
    }
}
