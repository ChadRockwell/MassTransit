namespace MassTransit.EventStore;

using System;

using MassTransit;

public sealed class FaultedEvent
{
    public Guid? FaultedMessageId { get; set; }
    public string[] FaultMessageTypes { get; set; }
    public DateTimeOffset TimeStamp { get; set; }

    public FaultedEvent(
        Guid? faultedMessageId,
        string[] faultMessageTypes,
        DateTimeOffset timeStamp)
    {
        FaultedMessageId = faultedMessageId;
        FaultMessageTypes = faultMessageTypes;
        TimeStamp = timeStamp;
    }

    public static FaultedEvent Create<TEvent>(Fault<TEvent> @event)
    {
        return new FaultedEvent(
            @event.FaultedMessageId,
            @event.FaultMessageTypes,
            @event.Timestamp);
    }
}
