namespace MassTransit.EventStore;

public interface IEventSourcedWorkflowState : ISaga
{
    /// <summary>
    /// This is necessary for tracking the logical sequence of events
    /// It is for internal purposes only and shouldn't be set or used.
    /// </summary>
    int PreviousSequenceNumber { get; set; }

    /// <summary>
    ///     Initializes this instance using the specified events.
    /// </summary>
    /// <param name="events">The events to initialize with.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="events" /> are null.</exception>
    void Initialize(IEnumerable<object> events);

    /// <summary>
    ///     Determines whether this instance has state changes.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this instance has state changes; otherwise, <c>false</c>.
    /// </returns>
    bool HasChanges();

    /// <summary>
    ///     Gets the state changes applied to this instance.
    /// </summary>
    /// <returns>A list of recorded state changes.</returns>
    IEnumerable<object> GetChanges();

    /// <summary>
    ///     Clears the state changes.
    /// </summary>
    void ClearChanges();
}
