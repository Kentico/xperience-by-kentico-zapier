using System.Collections.Concurrent;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using Kentico.Integration.Zapier;
using Microsoft.Extensions.Options;

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
    private readonly IWorkflowScopeService workflowScopeService;
    private readonly IContentHelper contentHelper;
    private readonly IOptionsMonitor<ZapierConfiguration> options;
    private readonly IInfoProvider<ContentLanguageInfo> contentLanguageProvider;


    protected ConcurrentDictionary<int, ZapierTriggerHandler> ZapierHandlers = [];

    public ZapierRegistrationService(IEventLogService logService, HttpClient client, IWorkflowScopeService workflowScopeService, IContentHelper contentHelper, IOptionsMonitor<ZapierConfiguration> options, IInfoProvider<ContentLanguageInfo> contentLanguageProvider)
    {
        this.logService = logService;
        this.client = client;
        this.workflowScopeService = workflowScopeService;
        this.contentHelper = contentHelper;
        this.options = options;
        this.contentLanguageProvider = contentLanguageProvider;
    }

    public void RegisterWebhook(ZapierTriggerInfo webhook)
    {
        var handler = new ZapierTriggerHandler(webhook, client, logService, workflowScopeService, contentHelper, options.CurrentValue, contentLanguageProvider);

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
