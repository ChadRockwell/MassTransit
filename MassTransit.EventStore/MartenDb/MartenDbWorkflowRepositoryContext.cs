namespace MassTransit.EventStore.MartenDb;

using System.Linq.Expressions;
using System.Reflection;

using Context;

using Marten;

using MassTransit.EventStore;

using Middleware;

using Saga;

public sealed class MartenDbWorkflowRepositoryContext<TWorkflow, TMessage> :
    ConsumeContextScope<TMessage>,
    SagaRepositoryContext<TWorkflow, TMessage>
    where TWorkflow : class, ISagaVersion, IEventSourcedWorkflowState
    where TMessage : class
{
    private readonly IDocumentStore _documentStore;
    private readonly ConsumeContext<TMessage> _consumeContext;
    private readonly ISagaConsumeContextFactory<IDocumentStore, TWorkflow> _factory;

    public MartenDbWorkflowRepositoryContext(
        IDocumentStore documentStore,
        ConsumeContext<TMessage> consumeContext,
        ISagaConsumeContextFactory<IDocumentStore, TWorkflow> factory)
        : base(consumeContext)
    {
        _documentStore = documentStore;
        _consumeContext = consumeContext;
        _factory = factory;
    }

    public Task<SagaConsumeContext<TWorkflow, T>> CreateSagaConsumeContext<T>(
        ConsumeContext<T> consumeContext,
        TWorkflow instance,
        SagaConsumeContextMode mode)
        where T : class
    {
        return _factory.CreateSagaConsumeContext(_documentStore, consumeContext, instance, mode);
    }

    public Task<SagaConsumeContext<TWorkflow, TMessage>> Add(TWorkflow instance)
    {
        return _factory.CreateSagaConsumeContext(
            _documentStore,
            _consumeContext,
            instance,
            SagaConsumeContextMode.Add);
    }

    public Task<SagaConsumeContext<TWorkflow, TMessage>> Insert(TWorkflow instance)
    {
        throw new NotSupportedException("Inserting saga events is currently not supported");
    }

    public async Task<SagaConsumeContext<TWorkflow, TMessage>?> Load(Guid correlationId)
    {
        using var session = _documentStore.QuerySession();

        var results = await session.Query<EventData>().Where(w => w.CorrelationId == correlationId).ToListAsync();
        var currentWorkflowState = await session.Query<CurrentWorkflowState>().Where(w => w.CorrelationId == correlationId).SingleOrDefaultAsync();

        if (results == null || results.Count == 0 || currentWorkflowState == null)
        {
            return default;
        }

        List<object> events = new(results.OrderBy(o => o.SequenceNumber).Select(x => x.Data));
        int maxSequenceNumber = results.Max(x => x.SequenceNumber);

        TWorkflow instance = SagaFactory();
        instance.Initialize(events);
        instance.CorrelationId = correlationId;
        instance.PreviousSequenceNumber = maxSequenceNumber;
        instance.Version = currentWorkflowState.Version + 1;

        SagaConsumeContext<TWorkflow, TMessage>? context = await _factory
            .CreateSagaConsumeContext(_documentStore, _consumeContext, instance, SagaConsumeContextMode.Load)
            .ConfigureAwait(false);

        return context;
    }

    public async Task Save(SagaConsumeContext<TWorkflow> context)
    {
        TWorkflow saga = context.Saga;
        int currentEventSequenceNumber = saga.PreviousSequenceNumber;

        IEnumerable<EventData> events =
            saga.GetChanges()
                .Select(x =>
                {
                    return new EventData(
                        saga.CorrelationId,
                        context.Saga.ToString()!,
                        x is WorkflowStateTransitioned data ? data.NewState! : default,
                        x,
                        saga,
                        TypeMapping.GetTypeName(x.GetType()),
                        ++currentEventSequenceNumber,
                        saga.Version,
                        x is WorkflowStateFaulted fault && fault.Fault != null,
                        x is WorkflowStateFaulted fault1 ? fault1.Fault : null,
                        DateTimeOffset.UtcNow);
                }).ToList();

        using var session = _documentStore.LightweightSession();

        session.Store(events);

        // get most recent event
        var latestEvent = events.OrderByDescending(o => o.SequenceNumber).First();

        CurrentWorkflowState current = new(
                        saga.CorrelationId,
                        context.Saga.ToString()!,
                        latestEvent.CurrentState!,
                        context.Saga,
                        latestEvent.SequenceNumber,
                        context.Saga.Version,
                        latestEvent.IsFaulted,
                        latestEvent.FaultedEvent,
                        DateTimeOffset.UtcNow);

        session.Store(current);

        await session.SaveChangesAsync();
    }

    public async Task Update(SagaConsumeContext<TWorkflow> context)
    {
        await Save(context);
    }

    public Task Delete(SagaConsumeContext<TWorkflow> context)
    {
        throw new NotSupportedException("Deleting saga events is currently not supported");
    }

    public Task Discard(SagaConsumeContext<TWorkflow> context)
    {
        return Task.CompletedTask;
    }

    public Task Undo(SagaConsumeContext<TWorkflow> context)
    {
        return Task.CompletedTask;
    }

    private static TWorkflow SagaFactory()
    {
        ConstructorInfo? ctor = typeof(TWorkflow).GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, Array.Empty<Type>(), null);

        ctor.ThrowIfNull();

        return (TWorkflow)ctor.Invoke(Array.Empty<object>());
    }
}

public class WorkflowRepositoryContext<TWorkflow> :
    BasePipeContext,
    SagaRepositoryContext<TWorkflow>
    where TWorkflow : class, ISagaVersion, IEventSourcedWorkflowState
{
    private readonly IDocumentStore _documentStore;

    public WorkflowRepositoryContext(IDocumentStore documentStore, CancellationToken cancellationToken)
        : base(cancellationToken)
    {
        _documentStore = documentStore;
    }

    public async Task<SagaRepositoryQueryContext<TWorkflow>> Query(
        ISagaQuery<TWorkflow> query,
        CancellationToken cancellationToken = new())
    {
        using var session = _documentStore.QuerySession();

        try
        {
            Expression<Func<TWorkflow, bool>> expression = query.FilterExpression;

            ParameterExpression param = Expression.Parameter(typeof(EventData));

            Expression body = new QueryExpressionVisitor<EventData>(param).Visit(expression.Body);

            Expression<Func<EventData, bool>> lambda = Expression.Lambda<Func<EventData, bool>>(body, param);

            var instances = await session
                .Query<EventData>()
                .Where(lambda)
                .Select(s => s.CorrelationId)
                .ToListAsync(cancellationToken);

            return new DefaultSagaRepositoryQueryContext<TWorkflow>(this, instances.ToList());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public Task<TWorkflow> Load(Guid correlationId)
    {
        throw new NotImplementedException();
    }
}
