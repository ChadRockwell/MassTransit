namespace MassTransit.Audit.MartenDb;

using System.Threading.Tasks;

using Marten;

using MassTransit.Audit;

public sealed class MartenDbAuditStore : IMessageAuditStore
{
    private readonly IDocumentStore _store;

    public MartenDbAuditStore(IDocumentStore documentStore)
    {
        _store = documentStore;
    }

    public async Task StoreMessage<T>(T message, MessageAuditMetadata metadata)
        where T : class
    {
        var auditDocument = AuditDocument.Create(message, TypeCache<T>.ShortName, metadata);

        using var session = _store.LightweightSession();

        session.Insert(auditDocument);

        await session.SaveChangesAsync();
    }
}
