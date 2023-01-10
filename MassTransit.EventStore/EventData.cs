namespace MassTransit.EventStore;
public sealed class EventData
{
    public EventData(
        Guid correlationId,
        string workflowName,
        string? currentState,
        object data,
        object workflowState,
        string type,
        int sequenceNumber,
        int version,
        bool isFaulted,
        FaultedEvent? faultedEvent,
        DateTimeOffset createdAt)
    {
        WorkflowName = workflowName;
        CurrentState = currentState;
        Data = data;
        WorkflowState = workflowState;
        Type = type;
        CorrelationId = correlationId;
        SequenceNumber = sequenceNumber;
        Version = version;
        IsFaulted = isFaulted;
        FaultedEvent = faultedEvent;
        CreatedAt = createdAt;
    }

    public Guid Id { get; internal set; }
    public string WorkflowName { get; internal set; }
    public string? CurrentState { get; internal set; }
    public object Data { get; internal set; }
    public object WorkflowState { get; internal set; }
    public string Type { get; internal set; }
    public Guid CorrelationId { get; internal set; }
    public int SequenceNumber { get; internal set; }
    public int Version { get; internal set; }
    public bool IsFaulted { get; internal set; }
    public FaultedEvent? FaultedEvent { get; internal set; }
    public DateTimeOffset CreatedAt { get; internal set; }
}
