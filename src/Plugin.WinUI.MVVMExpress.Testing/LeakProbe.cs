namespace Plugin.WinUI.MVVMExpress.Testing;

/// <summary>
/// Forces collections and reports whether a <see cref="WeakReference"/> target was reclaimed.
/// </summary>
public static class LeakProbe
{
    /// <summary>
    /// Runs blocking compacting GCs and returns whether <paramref name="reference"/> is no longer alive.
    /// </summary>
    public static bool IsCollected(WeakReference reference, int rounds = 3)
    {
        ArgumentNullException.ThrowIfNull(reference);
        for (var i = 0; i < rounds; i++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }

        return !reference.IsAlive;
    }

    /// <summary>Creates a <see cref="WeakReference"/> to <paramref name="target"/> without extending its lifetime.</summary>
    public static WeakReference Track(object target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return new WeakReference(target, trackResurrection: false);
    }
}
