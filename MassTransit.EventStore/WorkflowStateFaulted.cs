namespace MassTransit.EventStore;
public sealed class WorkflowStateFaulted
{
    public Guid InstanceId { get; set; }

    public FaultedEvent? Fault { get; set; }
}
