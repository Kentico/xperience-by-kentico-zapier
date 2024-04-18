using CMS.ContentWorkflowEngine;
using CMS.Core;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

using Microsoft.AspNetCore.Http;

namespace Kentico.Xperience.Zapier.Triggers.Handlers;

internal class WorkflowPagesHandler : ZapierWorkflowHandler
{
    public WorkflowPagesHandler(ZapierTriggerInfo zapierTrigger, IEventLogService? eventLogService, HttpClient client, IHttpContextAccessor httpContextAccessor) : base(zapierTrigger, eventLogService, client, httpContextAccessor)
    {
    }


    public override bool RegistrationProcessor(bool register = true)
    {
        if (register)
        {
            WebPageWorkflowEvents.MoveToStep.Execute += Handler;
        }
        else
        {
            WebPageWorkflowEvents.MoveToStep.Execute -= Handler;
        }

        EventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for pages workflow handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {ZapierTrigger.ZapierTriggerEventType} was successful.");
        return true;
    }


    private void Handler(object? sender, WebPageWorkflowMoveToStepArguments e)
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
        var websiteUri = GetHostDomain();
        var adminUrl = new Uri(websiteUri, $"/admin/webpages-{e.WebsiteChannelID}/{e.ContentLanguageName}_{e.ID}");

        data.TryAdd("AdminLink", adminUrl);


        if (ZapierTrigger != null)
        {
            _ = DoPost(ZapierTrigger.ZapierTriggerZapierURL, data);
        }
    }
}
