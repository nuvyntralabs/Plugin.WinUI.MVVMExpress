namespace Plugin.WinUI.MVVMExpress.Hosting;

/// <summary>Marks a ViewModel for generated <c>AddTransient</c> registration (no reflection scan).</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RegisterViewModelAttribute : Attribute;

/// <summary>Marks a view type and its ViewModel for generated DI registration.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RegisterViewAttribute : Attribute
{
    /// <summary>Creates the attribute.</summary>
    /// <param name="viewModelType">ViewModel to pair with this view.</param>
    public RegisterViewAttribute(Type viewModelType)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        ViewModelType = viewModelType;
    }

    /// <summary>Paired ViewModel type.</summary>
    public Type ViewModelType { get; }
}

/// <summary>Maps this ViewModel to a URI route for generated <c>ApplyRoutes</c>.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RouteAttribute : Attribute
{
    /// <summary>Creates the attribute.</summary>
    /// <param name="path">Route path (no query).</param>
    public RouteAttribute(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Path = path;
    }

    /// <summary>Route path.</summary>
    public string Path { get; }
}
