namespace Plugin.WinUI.MVVMExpress.Navigation;

/// <summary>URI / page stack bookkeeping shared by in-memory and MAUI hosts.</summary>
public sealed class NavigationStack
{
    private readonly List<NavigationRequest> _stack = [];
    private readonly List<NavigationRequest> _modal = [];
    private readonly List<NavigationRequest> _history = [];
    private Type? _currentOverride;

    /// <summary>Last navigated ViewModel type, if any.</summary>
    public Type? Current
    {
        get
        {
            if (_modal.Count > 0)
            {
                return _modal[^1].ViewModelType;
            }

            if (_stack.Count > 0)
            {
                return _stack[^1].ViewModelType;
            }

            return _currentOverride;
        }
        set => _currentOverride = value;
    }

    /// <summary>Non-modal stack (root first).</summary>
    public IReadOnlyList<Type> Stack => [.. _stack.Select(frame => frame.ViewModelType)];

    /// <summary>Modal stack (first modal first).</summary>
    public IReadOnlyList<Type> ModalStack => [.. _modal.Select(frame => frame.ViewModelType)];

    /// <summary>Recorded navigation requests (including back / pop-to-root).</summary>
    public IReadOnlyList<NavigationRequest> History => _history;

    /// <summary>True when a back navigation would pop a frame.</summary>
    public bool CanGoBack => _modal.Count > 0 || _stack.Count > 1;

    /// <summary>Applies <paramref name="frame"/> according to <paramref name="options"/>.</summary>
    public void Push(NavigationRequest frame, NavOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(frame);
        _currentOverride = null;
        if (options?.Replace == true)
        {
            Replace(frame);
            return;
        }

        if (options?.Modal == true)
        {
            _modal.Add(frame);
        }
        else
        {
            _stack.Add(frame);
        }

        _history.Add(frame);
    }

    /// <summary>Replaces the current top frame, or pushes when the stack is empty.</summary>
    public void Replace(NavigationRequest frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        _currentOverride = null;
        if (_modal.Count > 0)
        {
            _modal[^1] = frame;
        }
        else if (_stack.Count > 0)
        {
            _stack[^1] = frame;
        }
        else
        {
            _stack.Add(frame);
        }

        _history.Add(frame);
    }

    /// <summary>Clears both stacks and pushes <paramref name="root"/>.</summary>
    public void Reset(NavigationRequest root)
    {
        ArgumentNullException.ThrowIfNull(root);
        _currentOverride = null;
        _modal.Clear();
        _stack.Clear();
        _stack.Add(root);
        _history.Add(root);
    }

    /// <summary>Pops a modal first, then a non-root stack frame.</summary>
    /// <returns>The popped frame, or <see langword="null"/> when nothing can be popped.</returns>
    public NavigationRequest? Pop()
    {
        NavigationRequest? popped = null;
        if (_modal.Count > 0)
        {
            popped = _modal[^1];
            _modal.RemoveAt(_modal.Count - 1);
        }
        else if (_stack.Count > 1)
        {
            popped = _stack[^1];
            _stack.RemoveAt(_stack.Count - 1);
        }

        _history.Add(new NavigationRequest(typeof(object), "back"));
        return popped;
    }

    /// <summary>Clears the modal stack and pops to the first stack frame.</summary>
    public void PopToRoot()
    {
        _modal.Clear();
        if (_stack.Count > 1)
        {
            _stack.RemoveRange(1, _stack.Count - 1);
        }

        _history.Add(new NavigationRequest(typeof(object), "root"));
    }
}
