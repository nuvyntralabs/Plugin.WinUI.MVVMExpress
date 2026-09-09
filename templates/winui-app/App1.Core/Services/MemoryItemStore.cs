namespace App1;

public sealed class MemoryItemStore : IItemStore
{
    public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<string> items = ["Latte", "Mocha", "Espresso"];
        return Task.FromResult(items);
    }
}
