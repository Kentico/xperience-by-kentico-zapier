using CMS.ContentEngine;
using CMS.ContentWorkflowEngine;
using CMS.Core;
using CMS.DataEngine;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

using Microsoft.AspNetCore.Http;

namespace Kentico.Xperience.Zapier.Triggers.Handlers;

internal class WorkflowReusableHandler : ZapierWorkflowHandler
{
    private readonly IInfoProvider<ContentLanguageInfo> contentLanguageProvider;
    public WorkflowReusableHandler(ZapierTriggerInfo zapierTrigger,
        IEventLogService? eventLogService,
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IInfoProvider<ContentLanguageInfo> contentLanguageProvider)
        : base(zapierTrigger, eventLogService, client, httpContextAccessor) =>
        this.contentLanguageProvider = contentLanguageProvider;


    public override bool RegistrationProcessor(bool register = true)
    {
        if (register)
        {
            ContentItemWorkflowEvents.MoveToStep.Execute += Handler;
        }
        else
        {
            ContentItemWorkflowEvents.MoveToStep.Execute -= Handler;
        }

        EventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for reusable workflow handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {ZapierTrigger.ZapierTriggerEventType} was successful.");
        return true;
    }


    private void Handler(object? sender, ContentItemWorkflowMoveToStepArguments e)
    {
        if (!ZapierTrigger.ZapierTriggerObjectType.Equals(e.ContentTypeName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (ZapierTrigger.ZapierTriggerEventType != e.StepName)
        {
            return;
        }

        var data = e.GetZapierWorkflowPostObject();

        var info = contentLanguageProvider.Get(e.ContentLanguageID);

        var websiteUri = GetHostDomain();
        var adminUrl = new Uri(websiteUri, $"/admin/content-hub/{info.ContentLanguageName}/list/{e.ID}");

        data.TryAdd("AdminLink", adminUrl);


        if (ZapierTrigger != null)
        {
            _ = DoPost(ZapierTrigger.ZapierTriggerZapierURL, data);
        }
    }
}
