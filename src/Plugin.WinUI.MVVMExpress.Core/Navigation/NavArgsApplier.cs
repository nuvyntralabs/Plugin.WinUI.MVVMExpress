namespace Plugin.WinUI.MVVMExpress.Navigation;

/// <summary>Applies typed args and URI query dictionaries to a ViewModel.</summary>
public static class NavArgsApplier
{
    /// <summary>Calls <see cref="IAcceptNavArgs{TArgs}.Accept"/> when the ViewModel implements it.</summary>
    public static void ApplyTyped<TArgs>(object? viewModel, TArgs args)
        where TArgs : notnull
    {
        if (viewModel is IAcceptNavArgs<TArgs> typed)
        {
            typed.Accept(args);
        }
    }

    /// <summary>Calls <see cref="IAcceptNavQuery.Accept"/> when the ViewModel implements it.</summary>
    public static void ApplyQuery(object? viewModel, IReadOnlyDictionary<string, object>? query)
    {
        if (query is not null && viewModel is IAcceptNavQuery destination)
        {
            destination.Accept(query);
        }
    }
}
