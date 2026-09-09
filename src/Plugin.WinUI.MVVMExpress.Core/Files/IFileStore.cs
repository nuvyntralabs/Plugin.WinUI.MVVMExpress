namespace Plugin.WinUI.MVVMExpress.Files;

/// <summary>Named blob store. Production apps should adapt platform file APIs or FileVault.</summary>
public interface IFileStore
{
    /// <summary>Opens <paramref name="path"/> for read, or <see langword="null"/> when missing.</summary>
    Task<Stream?> OpenReadAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Writes <paramref name="content"/> to <paramref name="path"/>.</summary>
    Task WriteAsync(string path, Stream content, CancellationToken cancellationToken = default);
}

/// <summary>In-memory <see cref="IFileStore"/>.</summary>
public sealed class MemoryFileStore : IFileStore
{
    private readonly Dictionary<string, byte[]> _files = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task<Stream?> OpenReadAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        cancellationToken.ThrowIfCancellationRequested();
        if (!_files.TryGetValue(path, out var bytes))
        {
            return Task.FromResult<Stream?>(null);
        }

        return Task.FromResult<Stream?>(new MemoryStream(bytes, writable: false));
    }

    /// <inheritdoc />
    public async Task WriteAsync(string path, Stream content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(content);
        cancellationToken.ThrowIfCancellationRequested();
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        _files[path] = buffer.ToArray();
    }
}
