namespace Plugin.WinUI.MVVMExpress.Messaging;

/// <summary>
/// Publish/subscribe hub. Weak subscriptions must use a handler that receives the recipient
/// so the delegate does not capture and pin the subscriber.
/// </summary>
public interface IMessageHub
{
    /// <summary>Subscribes <paramref name="subscriber"/> to <typeparamref name="TMessage"/>.</summary>
    /// <param name="subscriber">Recipient instance.</param>
    /// <param name="handler">Handler that receives the recipient so a weak subscribe does not capture it.</param>
    /// <param name="weak">When <see langword="true"/>, the subscriber is held with a weak reference.</param>
    IDisposable Subscribe<TRecipient, TMessage>(
        TRecipient subscriber,
        Action<TRecipient, TMessage> handler,
        bool weak = true)
        where TRecipient : class;

    /// <summary>Publishes <paramref name="message"/> to live subscribers.</summary>
    void Publish<TMessage>(TMessage message);

    /// <summary>Publishes <paramref name="message"/> asynchronously (handlers are still invoked synchronously).</summary>
    /// <param name="message">Message payload.</param>
    /// <param name="cancellationToken">Cancels before handlers run.</param>
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default);

    /// <summary>Removes all subscriptions for <paramref name="subscriber"/>.</summary>
    void Unsubscribe(object subscriber);
}
