namespace Plugin.WinUI.MVVMExpress.Auth;

/// <summary>ORs <see cref="INavigationAuthPolicy"/> results from generated modules and an optional DI policy.</summary>
internal sealed class CompositeNavigationAuthPolicy : INavigationAuthPolicy
{
    private readonly IReadOnlyList<INavigationAuthPolicy> _policies;

    public CompositeNavigationAuthPolicy(IReadOnlyList<INavigationAuthPolicy> policies)
    {
        ArgumentNullException.ThrowIfNull(policies);
        _policies = policies;
    }

    /// <inheritdoc />
    public bool RequiresAuthentication(Type viewModelType)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        foreach (var policy in _policies)
        {
            if (policy.RequiresAuthentication(viewModelType))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public bool RequiresRole(Type viewModelType, out string? role)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        foreach (var policy in _policies)
        {
            if (policy.RequiresRole(viewModelType, out role))
            {
                return true;
            }
        }

        role = null;
        return false;
    }
}
