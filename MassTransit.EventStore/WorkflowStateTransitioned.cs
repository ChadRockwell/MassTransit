namespace MassTransit.EventStore;

public sealed class WorkflowStateTransitioned
{
    public Guid CorrelationId { get; set; }

    public string? NewState { get; set; }
}
