namespace MassTransit.EventStore;
public abstract class EventSourcedWorkflowState : IEventSourcedWorkflowState, SagaStateMachineInstance
{
    private readonly EventRecorder _recorder;
    private readonly EventRouter _router;
    private string? _currentState;
    private bool _isFaulted = false;
    private FaultedEvent? _fault;

    public string? CurrentState
    {
        get => _currentState;
        set => Apply(new WorkflowStateTransitioned
        {
            CorrelationId = CorrelationId,
            NewState = value,
        });
    }

    /// <summary>
    /// DO NOT use the SET method directly.  Set the FAULT to set property.
    /// This set is provided so underlying datastore can adequately SERDER value.
    /// </summary>
    public bool IsFaulted
    {
        get => _isFaulted;
        set => _isFaulted = value;
    }

    public FaultedEvent? Fault
    {
        get => _fault;
        set => Apply(new WorkflowStateFaulted
        {
            InstanceId = CorrelationId,
            Fault = value
        });
    }

    public Guid CorrelationId { get; set; }

    /// <inheritdoc/>
    public int PreviousSequenceNumber { get; set; }

    protected EventSourcedWorkflowState()
    {
        _router = new EventRouter();
        _recorder = new EventRecorder();

        Register<WorkflowStateTransitioned>(x =>
        {
            _currentState = x.NewState;

            if (IsFaulted)
            {
                Fault = null;
            }
        });
        Register<WorkflowStateFaulted>(x =>
        {
            if (x.Fault != null)
            {
                _isFaulted = true;
                _fault = x.Fault;
            }
            else
            {
                _isFaulted = false;
                _fault = null;
            }
        });
    }

    /// <inheritdoc />
    public void Initialize(IEnumerable<object> events)
    {
        events.ThrowIfNull();

        if (HasChanges())
        {
            throw new InvalidOperationException("Initialize cannot be called on an instance with changes.");
        }

        foreach (var @event in events)
        {
            Play(@event);
        }
    }

    /// <summary>
    ///     Applies the specified event to this instance and invokes the associated state handler.
    /// </summary>
    /// <param name="event">The event to apply.</param>
    public void Apply(object @event)
    {
        @event.ThrowIfNull();

        Play(@event);
        Record(@event);
    }

    /// <inheritdoc />
    public bool HasChanges()
    {
        return _recorder.Any();
    }

    /// <inheritdoc />
    public IEnumerable<object> GetChanges()
    {
        return _recorder.ToArray();
    }

    /// <inheritdoc />
    public void ClearChanges()
    {
        _recorder.Reset();
    }

    /// <summary>
    ///     Registers the state handler to be invoked when the specified event is applied.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to register the handler for.</typeparam>
    /// <param name="handler">The handler.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="handler" /> is null.</exception>
    protected void Register<TEvent>(Action<TEvent> handler)
    {
        handler.ThrowIfNull();

        _router.ConfigureRoute(handler);
    }

    private void Play(object @event)
    {
        _router.Route(@event);
    }

    private void Record(object @event)
    {
        _recorder.Record(@event);
    }
}
