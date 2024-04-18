using System.Collections.Concurrent;

using CMS.Core;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers.Handlers;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

namespace Kentico.Xperience.Zapier.Triggers;

internal interface IZapierRegistrationService
{
    void RegisterWebhook(ZapierTriggerInfo webhook);


    void UnregisterWebhook(ZapierTriggerInfo webhook);
}

internal class ZapierRegistrationService : IZapierRegistrationService
{
    private readonly IEventLogService logService;
    private readonly IZapierTriggerHandlerFactory zapierTriggerHandlerFactory;


    protected ConcurrentDictionary<int, ZapierTriggerHandler> ZapierHandlers = [];


    public ZapierRegistrationService(IEventLogService logService, IZapierTriggerHandlerFactory zapierTriggerHandlerFactory)
    {
        this.logService = logService;
        this.zapierTriggerHandlerFactory = zapierTriggerHandlerFactory;
    }


    public void RegisterWebhook(ZapierTriggerInfo webhook)
    {
        var handler = zapierTriggerHandlerFactory.CreateHandler(webhook);
        if (handler == null)
        {
            logService.LogEvent(EventTypeEnum.Error, nameof(ZapierRegistrationService), "REGISTER", $"Could not select handler for trigger with id {webhook.ZapierTriggerID}");
            return;
        }
        if (!ZapierHandlers.TryAdd(webhook.ZapierTriggerID, handler))
        {
            logService.LogEvent(EventTypeEnum.Error, nameof(ZapierRegistrationService), "REGISTER", $"Handler with id {webhook.ZapierTriggerID} could not be added in concurrent collection");
            return;
        }

        handler.Register();
    }


    public void UnregisterWebhook(ZapierTriggerInfo webhook)
    {
        if (!ZapierHandlers.TryRemove(webhook.ZapierTriggerID, out var handler))
        {
            logService.LogEvent(EventTypeEnum.Error, nameof(ZapierRegistrationService), "UNREGISTER", $"Handler with id {webhook.ZapierTriggerID} does not exist in concurrent collection");
            return;
        }

        handler.Unregister();
    }
}
