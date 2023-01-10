namespace MassTransit.BigMessageData.MartenDb;

using System;

public sealed class MessageDataResolver
{
    private const string Scheme = "guid";
    private const string System = "martendb";
    private const string Specification = "collection";

    private readonly string _format = string.Join(":", Scheme, System, Specification);

    public Guid GetMessageDataId(Uri uri)
    {
        if (uri.Scheme != Scheme)
            throw new UriFormatException($"The scheme did not match the expected value: {Scheme}");

        string[] tokens = uri.AbsolutePath.Split(':');

        if (tokens.Length != 3 || !uri.AbsoluteUri.StartsWith($"{_format}:"))
            throw new UriFormatException($"Urn is not in the correct format. Use '{_format}:{{resourceId}}'");

        return Guid.Parse(tokens[2]);
    }

    public Uri GetAddress(Guid id)
    {
        return new Uri($"{_format}:{id}");
    }
}
