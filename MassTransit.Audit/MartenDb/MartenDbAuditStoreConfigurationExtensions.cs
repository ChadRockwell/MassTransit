namespace MassTransit.Audit.MartenDb;

using System;

using Marten;

public static class MartenDbAuditStoreConfiguratorExtensions
{
    public static void UseMartenDbAuditStore(
        this IBusFactoryConfigurator configurator,
        IDocumentStore documentStore,
        Action<IMessageFilterConfigurator>? configureFilter = default)
    {
        var auditStore = new MartenDbAuditStore(documentStore);

        ConfigureAuditStore(configurator, auditStore, configureFilter);
    }

    static void ConfigureAuditStore(
        IBusFactoryConfigurator configurator,
        MartenDbAuditStore auditStore,
        Action<IMessageFilterConfigurator>? configureFilter = default)
    {
        configurator.ConnectSendAuditObservers(auditStore, configureFilter);
        configurator.ConnectConsumeAuditObserver(auditStore, configureFilter);
    }
}
