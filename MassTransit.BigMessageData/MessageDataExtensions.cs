namespace MassTransit.BigMessageData;

using System.Threading.Tasks;

using MassTransit;

using Newtonsoft.Json;

public static class MessageDataExtensions
{
    public static Task<MessageData<string>> Put<T>(this IMessageDataRepository repository, T value,
            CancellationToken cancellationToken = default)
    {
        var stringValue = JsonConvert.SerializeObject(value);

        return repository.PutString(stringValue, default, cancellationToken);
    }
}
