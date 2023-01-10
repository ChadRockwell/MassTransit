namespace MassTransit.EventStore;

public sealed class EventRouter
{
    private readonly Dictionary<Type, Action<object>> _handlers;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstanceEventRouter" /> class.
    /// </summary>
    public EventRouter()
    {
        _handlers = new Dictionary<Type, Action<object>>();
    }

    /// <summary>
    ///     Adds a route for the specified event type to the specified state handler.
    /// </summary>
    /// <param name="event">The event type the route is for.</param>
    /// <param name="handler">The state handler that should be invoked when an event of the specified type is routed.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="event" /> or <paramref name="handler" /> is
    ///     <c>null</c>.
    /// </exception>
    public void ConfigureRoute(Type @event, Action<object> handler)
    {
        @event.ThrowIfNull();
        handler.ThrowIfNull();

        _handlers.Add(@event, handler);
    }

    /// <summary>
    ///     Adds a route for the specified event type to the specified state handler.
    /// </summary>
    /// <typeparam name="TEvent">The event type the route is for.</typeparam>
    /// <param name="handler">The state handler that should be invoked when an event of the specified type is routed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler" /> is <c>null</c>.</exception>
    public void ConfigureRoute<TEvent>(Action<TEvent> handler)
    {
        handler.ThrowIfNull();

        _handlers.Add(typeof(TEvent), @event => handler((TEvent)@event));
    }

    /// <summary>
    ///     Routes the specified <paramref name="event" /> to a configured state handler, if any.
    /// </summary>
    /// <param name="event">The event to route.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="event" /> is null.</exception>
    public void Route(object @event)
    {
        @event.ThrowIfNull();

        if (_handlers.TryGetValue(@event.GetType(), out Action<object>? handler))
        {
            handler.ThrowIfNull();
            handler(@event);
        }
    }
}
