namespace MassTransit.EventStore.MartenDb;

using System.Threading.Tasks;

using Context;

using Marten;

public sealed class MartenDbWorkflowConsumeContext<TWorkflow, TMessage> :
    ConsumeContextScope<TMessage>,
    SagaConsumeContext<TWorkflow, TMessage>
    where TMessage : class
    where TWorkflow : class, ISaga
{
    private readonly IDocumentStore _documentStore;

    public MartenDbWorkflowConsumeContext(
        IDocumentStore documentStore,
        ConsumeContext<TMessage> context,
        TWorkflow instance)
        : base(context)
    {
        _documentStore = documentStore;
        Saga = instance;
    }

    public override Guid? CorrelationId => Saga.CorrelationId;

    public TWorkflow Saga { get; }

    public bool IsCompleted { get; private set; }

    public async Task SetCompleted()
    {
        using var session = _documentStore.LightweightSession();

        session.HardDeleteWhere<EventData>(ed => ed.CorrelationId == CorrelationId);

        IsCompleted = true;

        await session.SaveChangesAsync();
    }
}
