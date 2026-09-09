namespace App1;

public interface IItemStore
{
    Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken = default);
}
