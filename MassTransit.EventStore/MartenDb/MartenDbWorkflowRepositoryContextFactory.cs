namespace MassTransit.EventStore.MartenDb;

using Marten;

using Saga;

public sealed class MartenDbWorkflowRepositoryContextFactory<TWorkflow> :
    ISagaRepositoryContextFactory<TWorkflow>
    where TWorkflow : class, ISagaVersion, IEventSourcedWorkflowState
{
    private readonly IDocumentStore _documentStore;

    private readonly ISagaConsumeContextFactory<IDocumentStore, TWorkflow> _factory;

    public MartenDbWorkflowRepositoryContextFactory(IDocumentStore documentStore,
        ISagaConsumeContextFactory<IDocumentStore, TWorkflow> factory)
    {
        _factory = factory;
        _documentStore = documentStore;
    }

    public void Probe(ProbeContext context)
    {
        ProbeContext scope = context.CreateScope("workflowRepository");
        scope.Set(new
        {
            Persistence = "connection",
        });
    }

    public async Task Send<T>(
        ConsumeContext<T> context,
        IPipe<SagaRepositoryContext<TWorkflow, T>> next)
        where T : class
    {
        MartenDbWorkflowRepositoryContext<TWorkflow, T> repositoryContext = new(_documentStore, context, _factory);
        await next.Send(repositoryContext).ConfigureAwait(false);
    }

    public Task SendQuery<T>(
        ConsumeContext<T> context,
        ISagaQuery<TWorkflow> query,
        IPipe<SagaRepositoryQueryContext<TWorkflow, T>> next) where T : class
    {
        throw new NotImplementedByDesignException("MartenDb workflow repository doesn't support queries");
    }

    public async Task<T> Execute<T>(
        Func<SagaRepositoryContext<TWorkflow>,
        Task<T>> asyncMethod,
        CancellationToken cancellationToken = new()) where T : class
    {
        WorkflowRepositoryContext<TWorkflow> repositoryContext = new(_documentStore, cancellationToken);

        return await asyncMethod(repositoryContext).ConfigureAwait(false);
    }
}
