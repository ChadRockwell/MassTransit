namespace MassTransit.Audit;

using System.Collections.Generic;

public sealed class AuditHeaders : Dictionary<string, string>
{
    AuditHeaders(IDictionary<string, string> dictionary)
        : base(dictionary)
    {
    }

    public AuditHeaders()
    {
        // Serialization constructor
    }

    internal static AuditHeaders FromDictionary(IDictionary<string, string> dictionary)
    {
        return dictionary == null
            ? new AuditHeaders()
            : new AuditHeaders(dictionary);
    }
}
