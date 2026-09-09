using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Diagnostics;
using Plugin.WinUI.MVVMExpress.Hosting;
using Plugin.WinUI.MVVMExpress.Lifecycle;
using Plugin.WinUI.MVVMExpress.Threading;
using Result = Plugin.WinUI.MVVMExpress.Outcome.Outcome;

namespace Plugin.WinUI.MVVMExpress.Navigation;

/// <summary>
/// <see cref="IPageNavigator"/> that hosts views in a WinUI <see cref="Frame"/>.
/// Core <see cref="NavigationStack"/> is the source of truth. View construction always runs on <see cref="IMainThread"/>.
/// </summary>
public sealed class WinUIFrameNavigator : IPageNavigator, IRouteResolver
{
    private readonly Dictionary<Type, Type> _views = [];
    private readonly NavigationRouteTable _routes = new();
    private readonly NavigationStack _stack = new();
    private readonly Stack<FrameworkElement> _viewStack = [];
    private readonly IServiceProvider? _services;
    private readonly Func<Frame?>? _frame;
    private readonly IMainThread _mainThread;
    private readonly IMvvmExpressDiagnostics? _diagnostics;
    private readonly MvvmExpressOptions? _options;

    public WinUIFrameNavigator(
        IWindowContext? window = null,
        IServiceProvider? services = null,
        Func<Frame?>? frame = null,
        IMainThread? mainThread = null,
        IMvvmExpressDiagnostics? diagnostics = null,
        MvvmExpressOptions? options = null)
    {
        Window = window ?? WindowContext.Default;
        _services = services;
        _frame = frame;
        _mainThread = NavigationThread.Resolve(mainThread);
        _diagnostics = diagnostics;
        _options = options;
    }

    public IWindowContext Window { get; }
    public Type? Current => _stack.Current;
    public IReadOnlyList<Type> Stack => _stack.Stack;
    public IReadOnlyList<Type> ModalStack => _stack.ModalStack;
    public bool CanGoBack => _stack.CanGoBack;
    public IReadOnlyList<NavigationRequest> History => _stack.History;

    public WinUIFrameNavigator Map<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView>(string? route = null)
        where TViewModel : class, IViewModel
        where TView : FrameworkElement
    {
        _views[typeof(TViewModel)] = typeof(TView);
        _routes.Map<TViewModel>(string.IsNullOrWhiteSpace(route) ? typeof(TViewModel).Name : route);
        return this;
    }

    public WinUIFrameNavigator Map(Type viewModelType, Type viewType, string? route = null)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        ArgumentNullException.ThrowIfNull(viewType);
        _views[viewModelType] = viewType;
        _routes.Map(viewModelType, string.IsNullOrWhiteSpace(route) ? viewModelType.Name : route);
        return this;
    }

    public bool TryResolve(string route, out Type viewModelType) => _routes.TryResolve(route, out viewModelType);

    public Task<Result> NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => GoAsync(typeof(TViewModel), null, null, null, cancellationToken);

    public Task<Result> NavigateToAsync<TViewModel, TArgs>(TArgs args, CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        where TArgs : notnull
    {
        var query = args as IReadOnlyDictionary<string, object>;
        return GoAsync(typeof(TViewModel), args, null, query, cancellationToken);
    }

    public Task<Result> NavigateToAsync(
        string route,
        IReadOnlyDictionary<string, object>? query = null,
        NavOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        var split = NavigationRouteTable.Split(route);
        var merged = NavigationRouteTable.MergeQuery(split.Query, query);
        if (!_routes.TryResolve(split.Path, out var viewModelType))
        {
            return Task.FromResult(Result.Failure("E_ROUTE", $"No page mapped for '{split.Path}'."));
        }

        _routes.TryGetRoute(viewModelType, out var mapped);
        return GoAsync(viewModelType, merged, mapped ?? split.Path, merged, cancellationToken, options);
    }

    public Task<Result> NavigateToAsync(Type viewModelType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        return GoAsync(viewModelType, null, null, null, cancellationToken);
    }

    public Task<Result> GoBackAsync(CancellationToken cancellationToken = default)
        => PopCore(toRoot: false, cancellationToken);

    public Task<Result> PopToRootAsync(CancellationToken cancellationToken = default)
        => PopCore(toRoot: true, cancellationToken);

    public Task<Result> ReplaceAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => GoAsync(typeof(TViewModel), null, null, null, cancellationToken, new NavOptions { Replace = true });

    public Task<Result> ResetAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => GoAsync(typeof(TViewModel), null, null, null, cancellationToken, reset: true);

    public Task<Result> ReplaceRootAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => ResetAsync<TViewModel>(cancellationToken);

    public Task<Result> PushModalAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => GoAsync(typeof(TViewModel), null, null, null, cancellationToken, new NavOptions { Modal = true });

    public Task<Result> PopModalAsync(CancellationToken cancellationToken = default)
        => ModalStack.Count == 0
            ? Task.FromResult(Result.Failure("E_MODAL", "Modal stack is empty."))
            : GoBackAsync(cancellationToken);

    private async Task<Result> GoAsync(
        Type viewModelType,
        object? args,
        string? route,
        IReadOnlyDictionary<string, object>? query,
        CancellationToken cancellationToken,
        NavOptions? options = null,
        bool reset = false)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_views.TryGetValue(viewModelType, out var viewType))
        {
            return Result.Failure("E_ROUTE", $"No page mapped for {viewModelType.Name}.");
        }

        NavigationThread.TraceOffThread(
            _mainThread,
            _diagnostics,
            "Hopping frame navigation onto IMainThread before constructing the view.");

        Result? outcome = null;
        await _mainThread.InvokeAsync(
            async () => outcome = await GoOnUiAsync(
                viewModelType, viewType, args, route, query, options, reset, cancellationToken).ConfigureAwait(true),
            cancellationToken).ConfigureAwait(false);
        return outcome ?? Result.Failure("E_NAV", "Navigation did not complete.");
    }

    private async Task<Result> GoOnUiAsync(
        Type viewModelType,
        Type viewType,
        object? args,
        string? route,
        IReadOnlyDictionary<string, object>? query,
        NavOptions? options,
        bool reset,
        CancellationToken cancellationToken)
    {
        var currentView = WinUIVisualTree.CurrentView(Window);
        if (currentView?.DataContext is INavigable leaving
            && !await leaving.CanNavigateAwayAsync(cancellationToken).ConfigureAwait(true))
        {
            return Result.Failure("E_GUARD", "Navigation blocked");
        }

        FrameworkElement view;
        try
        {
            NavigationThread.EnsurePageFactoryOnMainThread(_mainThread);
            view = CreateView(viewType, viewModelType, args, query);
        }
        catch (Exception ex)
        {
            return Result.Failure("E_PAGE", ex.Message, ex);
        }

        var frame = ResolveFrame();
        if (frame is null)
        {
            return Result.Failure("E_PAGE", "No Frame host is available for this window.");
        }

        if (reset)
        {
            _viewStack.Clear();
            _viewStack.Push(view);
            frame.Content = view;
        }
        else if (options?.Replace == true && _viewStack.Count > 0)
        {
            _viewStack.Pop();
            _viewStack.Push(view);
            frame.Content = view;
        }
        else
        {
            _viewStack.Push(view);
            frame.Content = view;
        }

        if (currentView?.DataContext is INavigable from)
        {
            await from.OnNavigatedFromAsync(cancellationToken).ConfigureAwait(true);
        }

        if (view.DataContext is INavigable to)
        {
            await to.OnNavigatedToAsync(cancellationToken).ConfigureAwait(true);
        }

        var request = new NavigationRequest(viewModelType, args, route, query, options?.Modal == true);
        if (reset)
        {
            _stack.Reset(request);
        }
        else if (options?.Replace == true)
        {
            _stack.Replace(request);
        }
        else
        {
            _stack.Push(request, options);
        }

        return Result.Success();
    }

    private async Task<Result> PopCore(bool toRoot, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Result? outcome = null;
        await _mainThread.InvokeAsync(
            async () => outcome = await PopOnUiAsync(toRoot, cancellationToken).ConfigureAwait(true),
            cancellationToken).ConfigureAwait(false);
        return outcome ?? Result.Failure("E_NAV", "Navigation did not complete.");
    }

    private async Task<Result> PopOnUiAsync(bool toRoot, CancellationToken cancellationToken)
    {
        var currentView = WinUIVisualTree.CurrentView(Window);
        if (currentView?.DataContext is INavigable leaving
            && !await leaving.CanNavigateAwayAsync(cancellationToken).ConfigureAwait(true))
        {
            return Result.Failure("E_GUARD", "Navigation blocked");
        }

        var frame = ResolveFrame();
        if (frame is null)
        {
            return Result.Failure("E_PAGE", "No Frame host is available for this window.");
        }

        if (_viewStack.Count <= 1)
        {
            return Result.Failure("E_NAV", "Navigation stack is empty.");
        }

        if (toRoot)
        {
            var root = _viewStack.First();
            _viewStack.Clear();
            _viewStack.Push(root);
            frame.Content = root;
        }
        else
        {
            _viewStack.Pop();
            frame.Content = _viewStack.Peek();
        }

        if (currentView?.DataContext is INavigable from)
        {
            await from.OnNavigatedFromAsync(cancellationToken).ConfigureAwait(true);
        }

        if (toRoot)
        {
            _stack.PopToRoot();
        }
        else
        {
            _stack.Pop();
        }

        return Result.Success();
    }

    private Frame? ResolveFrame() => _frame is not null ? _frame() : WinUIVisualTree.CurrentFrame(Window);

    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Map<TViewModel,TView> captures constructors; DI is the AOT path.")]
    private FrameworkElement CreateView(
        Type viewType,
        Type viewModelType,
        object? args,
        IReadOnlyDictionary<string, object>? query)
    {
        var view = _services?.GetService(viewType) as FrameworkElement
            ?? (FrameworkElement)Activator.CreateInstance(viewType)!;

        if (view.DataContext is null && _services?.GetService(viewModelType) is { } viewModel)
        {
            view.DataContext = viewModel;
        }

        NavArgsApplier.ApplyQuery(view.DataContext, query);
        if (args is not null && args is not IReadOnlyDictionary<string, object>)
        {
            ApplyTypedArgs(view.DataContext, args);
        }

        if (_options?.AutoAttachLifecycle != false)
        {
            ViewModelLifecycle.Attach(view, _options);
        }

        return view;
    }

    private static void ApplyTypedArgs(object? viewModel, object args)
    {
        if (viewModel is null)
        {
            return;
        }

        foreach (var iface in viewModel.GetType().GetInterfaces())
        {
            if (!iface.IsGenericType || iface.GetGenericTypeDefinition() != typeof(IAcceptNavArgs<>))
            {
                continue;
            }

            if (!iface.GenericTypeArguments[0].IsInstanceOfType(args))
            {
                continue;
            }

            iface.GetMethod(nameof(IAcceptNavArgs<object>.Accept))?.Invoke(viewModel, [args]);
            return;
        }
    }
}
