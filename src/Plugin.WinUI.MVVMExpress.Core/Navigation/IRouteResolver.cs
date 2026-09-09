namespace Plugin.WinUI.MVVMExpress.Navigation;

/// <summary>Resolves a URI path to a ViewModel type.</summary>
public interface IRouteResolver
{
    /// <summary>Resolves <paramref name="route"/> (path or <c>path?query</c>) to a ViewModel type.</summary>
    bool TryResolve(string route, out Type viewModelType);
}
