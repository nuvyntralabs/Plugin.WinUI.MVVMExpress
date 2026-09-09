using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Plugin.WinUI.MVVMExpress.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.Navigation;

/// <summary>Maps route strings to ViewModel types and formats URI query strings.</summary>
public sealed class NavigationRouteTable
{
    private readonly Dictionary<string, Type> _byRoute = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Type, string> _byType = [];

    /// <summary>Maps <typeparamref name="TViewModel"/> to <paramref name="route"/>.</summary>
    public NavigationRouteTable Map<TViewModel>(string route)
        where TViewModel : class, IViewModel
        => Map(typeof(TViewModel), route);

    /// <summary>Maps <paramref name="viewModelType"/> to <paramref name="route"/>.</summary>
    public NavigationRouteTable Map(Type viewModelType, string route)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        var key = Normalize(route);
        _byRoute[key] = viewModelType;
        _byType[viewModelType] = route.Trim();
        return this;
    }

    /// <summary>Resolves a route path (without query) to a ViewModel type.</summary>
    public bool TryResolve(string route, out Type viewModelType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        var path = Split(route).Path;
        return _byRoute.TryGetValue(Normalize(path), out viewModelType!);
    }

    /// <summary>Returns the mapped route for <paramref name="viewModelType"/>.</summary>
    public bool TryGetRoute(Type viewModelType, out string route)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        return _byType.TryGetValue(viewModelType, out route!);
    }

    /// <summary>Splits <c>path?a=1&amp;b=2</c> into path and query.</summary>
    public static (string Path, IReadOnlyDictionary<string, object> Query) Split(string route)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        var trimmed = route.Trim();
        var q = trimmed.IndexOf('?', StringComparison.Ordinal);
        if (q < 0)
        {
            return (trimmed, new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
        }

        var path = trimmed[..q];
        var query = ParseQuery(trimmed[(q + 1)..]);
        return (path, query);
    }

    /// <summary>Parses a raw query string into a dictionary.</summary>
    public static IReadOnlyDictionary<string, object> ParseQuery(string query)
    {
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(query))
        {
            return result;
        }

        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = part.IndexOf('=', StringComparison.Ordinal);
            if (eq < 0)
            {
                result[Uri.UnescapeDataString(part)] = "";
                continue;
            }

            var key = Uri.UnescapeDataString(part[..eq]);
            var value = Uri.UnescapeDataString(part[(eq + 1)..]);
            result[key] = value;
        }

        return result;
    }

    /// <summary>Builds a query string from public instance properties on <paramref name="args"/>.</summary>
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Typed records used as nav args expose public properties; dictionary overload is the AOT path.")]
    public static string FormatQuery(object args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args is IReadOnlyDictionary<string, object> dictionary)
        {
            return FormatQuery(dictionary);
        }

        var pairs = args.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetIndexParameters().Length == 0)
            .Select(property =>
                $"{Uri.EscapeDataString(property.Name)}={Uri.EscapeDataString(Convert.ToString(property.GetValue(args), CultureInfo.InvariantCulture) ?? "")}");
        return string.Join("&", pairs);
    }

    /// <summary>Builds a query string from <paramref name="query"/>.</summary>
    public static string FormatQuery(IReadOnlyDictionary<string, object> query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return string.Join("&", query.Select(pair =>
            $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(Convert.ToString(pair.Value, CultureInfo.InvariantCulture) ?? "")}"));
    }

    /// <summary>Merges <paramref name="left"/> and <paramref name="right"/>; right wins on key conflicts.</summary>
    public static IReadOnlyDictionary<string, object> MergeQuery(
        IReadOnlyDictionary<string, object>? left,
        IReadOnlyDictionary<string, object>? right)
    {
        if (left is null && right is null)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        var merged = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        if (left is not null)
        {
            foreach (var pair in left)
            {
                merged[pair.Key] = pair.Value;
            }
        }

        if (right is not null)
        {
            foreach (var pair in right)
            {
                merged[pair.Key] = pair.Value;
            }
        }

        return merged;
    }

    private static string Normalize(string route)
    {
        var path = Split(route).Path.Trim();
        while (path.StartsWith("//", StringComparison.Ordinal))
        {
            path = path[2..];
        }

        return path.TrimStart('/');
    }
}
