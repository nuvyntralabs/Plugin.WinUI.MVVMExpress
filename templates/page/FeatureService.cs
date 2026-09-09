namespace App1;

public sealed class FeatureService : IFeatureService
{
    public Task<string> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult("Feature");
    }
}
