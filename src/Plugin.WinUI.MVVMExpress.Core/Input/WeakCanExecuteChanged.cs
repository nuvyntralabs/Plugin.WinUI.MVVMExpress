using System.Reflection;

namespace Plugin.WinUI.MVVMExpress.Input;

/// <summary>
/// Stores <c>ICommand.CanExecuteChanged</c> handlers by weak target so a long-lived
/// command does not pin a Button, page, or other subscriber.
/// </summary>
internal sealed class WeakCanExecuteChanged
{
    private readonly List<Slot> _handlers = [];
    private readonly object _gate = new();

    public void Add(EventHandler? handler)
    {
        if (handler is null)
        {
            return;
        }

        lock (_gate)
        {
            foreach (var item in handler.GetInvocationList())
            {
                var typed = (EventHandler)item;
                _handlers.Add(typed.Target is null
                    ? new Slot(null, typed.Method, IsStatic: true)
                    : new Slot(new WeakReference(typed.Target), typed.Method, IsStatic: false));
            }
        }
    }

    public void Remove(EventHandler? handler)
    {
        if (handler is null)
        {
            return;
        }

        lock (_gate)
        {
            foreach (var item in handler.GetInvocationList())
            {
                var typed = (EventHandler)item;
                for (var index = _handlers.Count - 1; index >= 0; index--)
                {
                    if (!Matches(_handlers[index], typed))
                    {
                        continue;
                    }

                    _handlers.RemoveAt(index);
                    break;
                }
            }
        }
    }

    public void Raise(object sender, EventArgs e)
    {
        EventHandler[] live;
        lock (_gate)
        {
            var snapshot = new List<EventHandler>(_handlers.Count);
            var remaining = new List<Slot>(_handlers.Count);
            foreach (var slot in _handlers)
            {
                if (!TryCreate(slot, out var liveHandler))
                {
                    continue;
                }

                remaining.Add(slot);
                snapshot.Add(liveHandler);
            }

            _handlers.Clear();
            _handlers.AddRange(remaining);
            live = [.. snapshot];
        }

        foreach (var handler in live)
        {
            handler(sender, e);
        }
    }

    private static bool Matches(Slot slot, EventHandler handler)
    {
        if (slot.Method != handler.Method)
        {
            return false;
        }

        if (handler.Target is null)
        {
            return slot.IsStatic;
        }

        return !slot.IsStatic && ReferenceEquals(slot.Target?.Target, handler.Target);
    }

    private static bool TryCreate(Slot slot, out EventHandler handler)
    {
        if (slot.IsStatic)
        {
            handler = (EventHandler)Delegate.CreateDelegate(typeof(EventHandler), slot.Method);
            return true;
        }

        var target = slot.Target?.Target;
        if (target is null)
        {
            handler = null!;
            return false;
        }

        handler = (EventHandler)Delegate.CreateDelegate(typeof(EventHandler), target, slot.Method);
        return true;
    }

    private readonly record struct Slot(WeakReference? Target, MethodInfo Method, bool IsStatic);
}
