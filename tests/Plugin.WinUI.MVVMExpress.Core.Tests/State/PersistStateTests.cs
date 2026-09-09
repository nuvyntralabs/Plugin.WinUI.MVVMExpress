using Plugin.WinUI.MVVMExpress.State;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.State;

public sealed class PersistStateTests
{
    [Fact]
    public async Task MemoryStore_RoundTrips()
    {
        var store = new MemoryStateStore();
        await store.SaveAsync("k", "v");
        Assert.Equal("v", await store.LoadAsync("k"));
        Assert.Null(await store.LoadAsync("missing"));
    }

    [Fact]
    public async Task PersistState_NoOp_WhenNotPersistable()
    {
        var store = new MemoryStateStore();
        await PersistState.SaveAsync(new object(), store);
        await PersistState.RestoreAsync(new object(), store);
        Assert.Null(await store.LoadAsync("any"));
    }

    [Fact]
    public async Task Persistable_SavesAndRestores()
    {
        var store = new MemoryStateStore();
        var vm = new Note { Text = "hello" };
        await PersistState.SaveAsync(vm, store);
        var other = new Note();
        await PersistState.RestoreAsync(other, store);
        Assert.Equal("hello", other.Text);
    }

    private sealed class Note : IPersistableViewModel
    {
        public string Text { get; set; } = "";

        public Task SavePersistedStateAsync(IStateStore store, CancellationToken cancellationToken = default)
            => store.SaveAsync("note", Text, cancellationToken);

        public async Task RestorePersistedStateAsync(IStateStore store, CancellationToken cancellationToken = default)
        {
            Text = await store.LoadAsync("note", cancellationToken).ConfigureAwait(false) ?? "";
        }
    }
}
