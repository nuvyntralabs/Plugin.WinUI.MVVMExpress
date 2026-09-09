using Plugin.WinUI.MVVMExpress.Messaging;

namespace Plugin.WinUI.MVVMExpress.Testing;

/// <summary>Records publishes and forwards to an in-process <see cref="MessageHub"/>.</summary>
public sealed class FakeMessageHub : IMessageHub
{
    private readonly MessageHub _inner = new();

    /// <summary>Payloads passed to <see cref="Publish{TMessage}"/> / <see cref="PublishAsync{TMessage}"/>.</summary>
    public List<object?> Published { get; } = [];

    /// <inheritdoc />
    public IDisposable Subscribe<TRecipient, TMessage>(
        TRecipient subscriber,
        Action<TRecipient, TMessage> handler,
        bool weak = true)
        where TRecipient : class
        => _inner.Subscribe(subscriber, handler, weak);

    /// <inheritdoc />
    public void Publish<TMessage>(TMessage message)
    {
        Published.Add(message);
        _inner.Publish(message);
    }

    /// <inheritdoc />
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Published.Add(message);
        return _inner.PublishAsync(message, cancellationToken);
    }

    /// <inheritdoc />
    public void Unsubscribe(object subscriber) => _inner.Unsubscribe(subscriber);
}
