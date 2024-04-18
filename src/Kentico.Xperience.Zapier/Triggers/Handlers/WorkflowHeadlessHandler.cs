using CMS.ContentWorkflowEngine;
using CMS.Core;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

using Microsoft.AspNetCore.Http;

namespace Kentico.Xperience.Zapier.Triggers.Handlers;

internal class WorkflowHeadlessHandler : ZapierWorkflowHandler
{
    public WorkflowHeadlessHandler(ZapierTriggerInfo zapierTrigger, IEventLogService? eventLogService, HttpClient client, IHttpContextAccessor httpContextAccessor) : base(zapierTrigger, eventLogService, client, httpContextAccessor)
    {
    }


    public override bool RegistrationProcessor(bool register = true)
    {
        if (register)
        {
            HeadlessItemWorkflowEvents.MoveToStep.Execute += Handler;
        }
        else
        {
            HeadlessItemWorkflowEvents.MoveToStep.Execute -= Handler;
        }

        EventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for headless workflow handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {ZapierTrigger.ZapierTriggerEventType} was successful.");
        return true;
    }


    private void Handler(object? sender, HeadlessItemWorkflowMoveToStepArguments e)
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
        var adminUrl = new Uri(websiteUri, $"/admin/headless-{e.HeadlessChannelID}/{e.ContentLanguageName}/list/{e.ID}");
        data.TryAdd("AdminLink", adminUrl);

        if (ZapierTrigger != null)
        {
            _ = DoPost(ZapierTrigger.ZapierTriggerZapierURL, data);
        }
    }
}
