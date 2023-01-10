namespace MassTransit.BigMessageData.MartenDb;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Marten;

using MassTransit.BigMessageData;

public sealed class MartenDbMessageDataRepository : IMessageDataRepository
{
    private readonly MessageDataResolver _resolver = new();

    private readonly IDocumentStore _documentStore;

    public MartenDbMessageDataRepository(IDocumentStore documentStore)
    {
        _documentStore = documentStore;
    }

    public async Task<Stream> Get(Uri address, CancellationToken cancellationToken)
    {
        var id = _resolver.GetMessageDataId(address);

        using var session = _documentStore.QuerySession();

        var messageData = await session.LoadAsync<MessageData>(id, cancellationToken);

        return GenerateStreamFromString(messageData!.Data);
    }

    public async Task<Uri> Put(Stream stream, TimeSpan? timeToLive, CancellationToken cancellationToken)
    {
        if (timeToLive.HasValue)
        {
            throw new NotSupportedException("Time-To-Live not supported.");
        }

        var id = NewId.NextSequentialGuid();
        var data = GenerateStringFromStream(stream);

        MessageData messageData = new(id, data);

        using var session = _documentStore.LightweightSession();

        session.Insert(messageData);

        await session.SaveChangesAsync(cancellationToken);

        return _resolver.GetAddress(id);
    }

    private static Stream GenerateStreamFromString(string s)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private static string GenerateStringFromStream(Stream stream)
    {
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
