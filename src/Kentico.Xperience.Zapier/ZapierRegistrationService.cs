using System.Collections.Concurrent;
using CMS.Core;
using Kentico.Integration.Zapier;

namespace Kentico.Xperience.Zapier;

public interface IZapierRegistrationService
{
    void RegisterWebhook(ZapierTriggerInfo webhook);

    void UnregisterWebhook(ZapierTriggerInfo webhook);
}

public class ZapierRegistrationService : IZapierRegistrationService
{
    private readonly IEventLogService logService;
    private readonly HttpClient client;

    protected ConcurrentDictionary<int, ZapierTriggerHandler> ZapierHandlers = [];

    public ZapierRegistrationService(IEventLogService logService, HttpClient client)
    {
        this.logService = logService;
        this.client = client;
    }

    public void RegisterWebhook(ZapierTriggerInfo webhook)
    {
        var handler = new ZapierTriggerHandler(webhook, client, logService);

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
