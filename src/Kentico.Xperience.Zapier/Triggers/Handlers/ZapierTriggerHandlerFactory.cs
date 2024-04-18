using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers.Extensions;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

using Microsoft.AspNetCore.Http;

namespace Kentico.Xperience.Zapier.Triggers.Handlers;

internal interface IZapierTriggerHandlerFactory
{
    public ZapierTriggerHandler? CreateHandler(ZapierTriggerInfo trigger);
}


internal class ZapierTriggerHandlerFactory : IZapierTriggerHandlerFactory
{
    private readonly HttpClient client;
    private readonly IWorkflowScopeService workflowScopeService;
    private readonly IInfoProvider<ContentLanguageInfo> contentLanguageProvider;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IInfoProvider<ZapierTriggerEventLogTypeInfo> triggerEventLogTypeInfoProvider;
    private readonly IProgressiveCache progressiveCache;
    private readonly IEventLogService logService;


    public ZapierTriggerHandlerFactory(HttpClient client,
        IWorkflowScopeService workflowScopeService,
        IInfoProvider<ContentLanguageInfo> contentLanguageProvider,
        IHttpContextAccessor httpContextAccessor,
        IInfoProvider<ZapierTriggerEventLogTypeInfo> triggerEventLogTypeInfoProvider,
        IProgressiveCache progressiveCache,
        IEventLogService logService)
    {
        this.client = client;
        this.workflowScopeService = workflowScopeService;
        this.contentLanguageProvider = contentLanguageProvider;
        this.httpContextAccessor = httpContextAccessor;
        this.triggerEventLogTypeInfoProvider = triggerEventLogTypeInfoProvider;
        this.progressiveCache = progressiveCache;
        this.logService = logService;
    }


    public ZapierTriggerHandler? CreateHandler(ZapierTriggerInfo trigger)
    {
        if (trigger != null)
        {
            if (!Enum.TryParse(trigger.ZapierTriggerObjectClassType, out ZapierTriggerObjectClassType classType))
            {
                return null;
            }

            if (!Enum.TryParse(trigger.ZapierTriggerEventType, out ZapierTriggerEvents eventType))
            {
                var dataClass = DataClassInfoProvider.ProviderObject.Get(trigger.ZapierTriggerObjectType);
                if (dataClass is null || string.IsNullOrEmpty(trigger.ZapierTriggerEventType))
                {
                    return null;
                }

                bool isWorkflow = workflowScopeService.IsMatchingWorflowEventPerObject(trigger.ZapierTriggerEventType, dataClass.ClassID);

                if (!isWorkflow)
                {
                    return null;
                }

                ZapierWorkflowHandler workflowHandler = classType switch
                {
                    ZapierTriggerObjectClassType.Website => new WorkflowPagesHandler(trigger, logService, client, httpContextAccessor),
                    ZapierTriggerObjectClassType.Reusable => new WorkflowReusableHandler(trigger, logService, client, httpContextAccessor, contentLanguageProvider),
                    ZapierTriggerObjectClassType.Headless => new WorkflowHeadlessHandler(trigger, logService, client, httpContextAccessor),
                    ZapierTriggerObjectClassType.Form => throw new NotImplementedException(),
                    ZapierTriggerObjectClassType.Other => throw new NotImplementedException(),
                    ZapierTriggerObjectClassType.System => throw new NotImplementedException(),
                    ZapierTriggerObjectClassType.Email => throw new NotImplementedException(),
                    _ => throw new NotImplementedException()
                };

                return workflowHandler;
            }
            if (trigger.ZapierTriggerObjectType.Equals(EventLogInfo.OBJECT_TYPE, StringComparison.OrdinalIgnoreCase))
            {
                return new EventLogObjectHandler(trigger, logService, client, progressiveCache, triggerEventLogTypeInfoProvider);
            }

            ZapierTriggerHandler handler = classType switch
            {
                ZapierTriggerObjectClassType.Other => new InfoObjectHandler(trigger, logService, client),
                ZapierTriggerObjectClassType.System => new InfoObjectHandler(trigger, logService, client),
                ZapierTriggerObjectClassType.Form => new BizFormHandler(trigger, logService, client),
                ZapierTriggerObjectClassType.Website => new PageHandler(trigger, logService, client),
                ZapierTriggerObjectClassType.Reusable => new ReusableHandler(trigger, logService, client),
                ZapierTriggerObjectClassType.Headless => new HeadlessHandler(trigger, logService, client),
                ZapierTriggerObjectClassType.Email => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
            return handler;
        }

        logService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), "Zapier trigger handler selection failed", $"Handler selection was not successful");
        return null;
    }
}
