namespace App1;

public interface IFeatureService
{
    Task<string> LoadAsync(CancellationToken cancellationToken = default);
}
