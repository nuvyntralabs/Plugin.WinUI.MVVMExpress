namespace Plugin.WinUI.MVVMExpress.Media;

/// <summary>Photo / file picker. Production apps should adapt MAUI <c>MediaPicker</c>.</summary>
public interface IMediaPicker
{
    /// <summary>Picks a photo path, or <see langword="null"/> when cancelled.</summary>
    Task<string?> PickPhotoAsync(CancellationToken cancellationToken = default);
}

/// <summary>No-op picker for tests and samples without a camera.</summary>
public sealed class NullMediaPicker : IMediaPicker
{
    /// <summary>Shared instance.</summary>
    public static NullMediaPicker Instance { get; } = new();

    /// <inheritdoc />
    public Task<string?> PickPhotoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(null);
    }
}

/// <summary>Returns a configured path for tests.</summary>
public sealed class MemoryMediaPicker : IMediaPicker
{
    /// <summary>Path returned from <see cref="PickPhotoAsync"/>.</summary>
    public string? NextPath { get; set; }

    /// <inheritdoc />
    public Task<string?> PickPhotoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(NextPath);
    }
}