namespace Plugin.WinUI.MVVMExpress.Messaging;

/// <summary>Default in-process <see cref="IMessageHub"/>.</summary>
public sealed class MessageHub : IMessageHub
{
    private readonly object _gate = new();
    private readonly Dictionary<Type, List<Subscription>> _map = [];

    /// <inheritdoc />
    public IDisposable Subscribe<TRecipient, TMessage>(
        TRecipient subscriber,
        Action<TRecipient, TMessage> handler,
        bool weak = true)
        where TRecipient : class
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(handler);

        var messageType = typeof(TMessage);
        var subscription = new Subscription(
            typeof(TRecipient),
            subscriber,
            (recipient, message) => handler((TRecipient)recipient, (TMessage)message!),
            weak);

        subscription.OnDispose = () => Remove(messageType, subscription);

        lock (_gate)
        {
            if (!_map.TryGetValue(messageType, out var list))
            {
                list = [];
                _map[messageType] = list;
            }

            list.Add(subscription);
        }

        return subscription;
    }

    /// <inheritdoc />
    public void Publish<TMessage>(TMessage message)
    {
        Subscription[] snapshot;
        lock (_gate)
        {
            if (!_map.TryGetValue(typeof(TMessage), out var list) || list.Count == 0)
            {
                return;
            }

            snapshot = list.ToArray();
        }

        foreach (var subscription in snapshot)
        {
            if (!subscription.TryGetRecipient(out var recipient) || recipient is null)
            {
                Remove(typeof(TMessage), subscription);
                continue;
            }

            subscription.Invoke(recipient, message);
        }
    }

    /// <inheritdoc />
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Publish(message);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Unsubscribe(object subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        lock (_gate)
        {
            foreach (var list in _map.Values)
            {
                list.RemoveAll(item => item.Matches(subscriber));
            }
        }
    }

    private void Remove(Type messageType, Subscription subscription)
    {
        lock (_gate)
        {
            if (_map.TryGetValue(messageType, out var list))
            {
                list.Remove(subscription);
            }
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly Type _recipientType;
        private readonly WeakReference<object>? _weak;
        private object? _strong;
        private Action<object, object?>? _handler;

        internal Action? OnDispose { get; set; }

        internal Subscription(Type recipientType, object subscriber, Action<object, object?> handler, bool weak)
        {
            _recipientType = recipientType;
            _handler = handler;
            if (weak)
            {
                _weak = new WeakReference<object>(subscriber);
            }
            else
            {
                _strong = subscriber;
            }
        }

        internal bool TryGetRecipient(out object? recipient)
        {
            if (_strong is not null)
            {
                recipient = _strong;
                return true;
            }

            if (_weak is not null && _weak.TryGetTarget(out var target))
            {
                recipient = target;
                return true;
            }

            recipient = null;
            return false;
        }

        internal bool Matches(object subscriber)
        {
            if (_strong is not null)
            {
                return ReferenceEquals(_strong, subscriber);
            }

            return _weak is not null && _weak.TryGetTarget(out var target) && ReferenceEquals(target, subscriber);
        }

        internal void Invoke(object recipient, object? message)
        {
            if (!_recipientType.IsInstanceOfType(recipient))
            {
                return;
            }

            _handler?.Invoke(recipient, message);
        }

        public void Dispose()
        {
            OnDispose?.Invoke();
            OnDispose = null;
            _handler = null;
            _strong = null;
        }
    }
}
