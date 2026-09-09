namespace Plugin.WinUI.MVVMExpress.Testing;

/// <summary>Application size used by leak, allocation, and benchmark jobs.</summary>
public enum ApplicationScale
{
    /// <summary>Settings / counter / ≤ 15 screens. Lists ≤ 200.</summary>
    Small = 0,

    /// <summary>CRUD / field app. Lists ≤ 5_000.</summary>
    Mid = 1,

    /// <summary>Enterprise. Lists 50_000 items in memory; UI must virtualize.</summary>
    Large = 2
}

/// <summary>Numeric sizes for <see cref="ApplicationScale"/>.</summary>
public static class ScaleProfile
{
    /// <summary>Bound-list item count used in tests and benchmarks.</summary>
    public static int ListSize(ApplicationScale scale) => scale switch
    {
        ApplicationScale.Small => 200,
        ApplicationScale.Mid => 5_000,
        ApplicationScale.Large => 50_000,
        _ => 200
    };

    /// <summary>How many ViewModels a session-scale leak/create test constructs.</summary>
    public static int ViewModelBatch(ApplicationScale scale) => scale switch
    {
        ApplicationScale.Small => 32,
        ApplicationScale.Mid => 256,
        ApplicationScale.Large => 1_024,
        _ => 32
    };
}
