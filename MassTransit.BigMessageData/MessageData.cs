namespace MassTransit.BigMessageData;

using System;

public sealed class MessageData
{
    public Guid Id { get; private set; }

    public string Data { get; private set; }

    public MessageData(Guid id, string data)
    {
        Id = id;
        Data = data;
    }
}
