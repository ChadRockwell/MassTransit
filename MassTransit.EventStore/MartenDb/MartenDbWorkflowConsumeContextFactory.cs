namespace MassTransit.EventStore.MartenDb;

using Marten;

using Saga;

public class MartenDbWorkflowConsumeContextFactory<TContext, TWorkflow> :
    ISagaConsumeContextFactory<TContext, TWorkflow>
    where TWorkflow : class, ISaga
    where TContext : class
{
    private readonly IDocumentStore _documentStore;

    public MartenDbWorkflowConsumeContextFactory(IDocumentStore documentStore)
    {
        _documentStore = documentStore;
    }

    public Task<SagaConsumeContext<TWorkflow, T>> CreateSagaConsumeContext<T>(
        TContext context,
        ConsumeContext<T> consumeContext,
        TWorkflow instance,
        SagaConsumeContextMode mode) where T : class
    {
        return Task.FromResult<SagaConsumeContext<TWorkflow, T>>(
            new MartenDbWorkflowConsumeContext<TWorkflow, T>(
                _documentStore,
                consumeContext,
                instance)
            );
    }
}
