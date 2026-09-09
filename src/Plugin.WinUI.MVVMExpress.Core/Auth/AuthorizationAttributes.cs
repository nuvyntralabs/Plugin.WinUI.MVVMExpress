namespace Plugin.WinUI.MVVMExpress.Auth;

/// <summary>Generated auth policy requires a signed-in user before navigating to this ViewModel.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RequiresAuthAttribute : Attribute;

/// <summary>Generated auth policy requires <see cref="Role"/> in addition to sign-in.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RequiresRoleAttribute : Attribute
{
    /// <summary>Creates the attribute.</summary>
    /// <param name="role">Required role name.</param>
    public RequiresRoleAttribute(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        Role = role;
    }

    /// <summary>Required role.</summary>
    public string Role { get; }
}

/// <summary>Optional role checks used by <see cref="Navigation.GuardedNavigator"/>.</summary>
public interface IRoleState
{
    /// <summary>Returns whether the current principal has <paramref name="role"/>.</summary>
    bool HasRole(string role);
}

/// <summary>AOT-friendly map of ViewModels that require auth or a role. Prefer generated registrations over reflection.</summary>
public interface INavigationAuthPolicy
{
    /// <summary>Whether <paramref name="viewModelType"/> requires a signed-in user.</summary>
    bool RequiresAuthentication(Type viewModelType);

    /// <summary>Whether <paramref name="viewModelType"/> requires a role.</summary>
    bool RequiresRole(Type viewModelType, out string? role);
}

/// <summary>Dictionary-backed <see cref="INavigationAuthPolicy"/>.</summary>
public sealed class NavigationAuthPolicy : INavigationAuthPolicy
{
    private readonly HashSet<Type> _auth;
    private readonly Dictionary<Type, string> _roles;

    /// <summary>Creates a policy.</summary>
    /// <param name="authRequired">Types that require sign-in.</param>
    /// <param name="roles">Type → required role.</param>
    public NavigationAuthPolicy(IEnumerable<Type>? authRequired = null, IReadOnlyDictionary<Type, string>? roles = null)
    {
        _auth = authRequired is null ? [] : [.. authRequired];
        _roles = roles is null ? [] : new Dictionary<Type, string>(roles);
        foreach (var type in _roles.Keys)
        {
            _auth.Add(type);
        }
    }

    /// <inheritdoc />
    public bool RequiresAuthentication(Type viewModelType)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        return _auth.Contains(viewModelType);
    }

    /// <inheritdoc />
    public bool RequiresRole(Type viewModelType, out string? role)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        return _roles.TryGetValue(viewModelType, out role);
    }
}
