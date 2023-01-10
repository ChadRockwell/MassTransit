namespace MassTransit.EventStore.MartenDb;

using Marten;

using Saga;

public static class MartenDbWorkflowRepositoryFactory<TWorkflow>
    where TWorkflow : class, ISagaVersion, IEventSourcedWorkflowState
{
    public static ISagaRepository<TWorkflow> Create(
        IDocumentStore documentStore)
    {
        MartenDbWorkflowConsumeContextFactory<IDocumentStore, TWorkflow> sagaConsumeContextFactory = new(documentStore);

        MartenDbWorkflowRepositoryContextFactory<TWorkflow> repositoryContextFactory =
            new(documentStore, sagaConsumeContextFactory);

        return new SagaRepository<TWorkflow>(repositoryContextFactory);
    }
}
