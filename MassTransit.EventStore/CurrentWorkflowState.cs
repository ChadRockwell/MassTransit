namespace MassTransit.EventStore;

public sealed class CurrentWorkflowState
{
    public CurrentWorkflowState(
        Guid correlationId,
        string workflowName,
        string currentState,
        object workflowState,
        int sequenceNumber,
        int version,
        bool isFaulted,
        FaultedEvent? faultedEvent,
        DateTimeOffset createdAt)
    {
        WorkflowName = workflowName;
        CurrentState = currentState;
        WorkflowState = workflowState;
        CorrelationId = correlationId;
        SequenceNumber = sequenceNumber;
        Version = version;
        IsFaulted = isFaulted;
        FaultedEvent = faultedEvent;
        CreatedAt = createdAt;
    }

    public Guid CorrelationId { get; internal set; }
    public string WorkflowName { get; internal set; }
    public string CurrentState { get; internal set; }
    public object WorkflowState { get; internal set; }
    public int SequenceNumber { get; internal set; }
    public int Version { get; internal set; }
    public bool IsFaulted { get; internal set; }
    public FaultedEvent? FaultedEvent { get; internal set; }
    public DateTimeOffset CreatedAt { get; internal set; }
}
