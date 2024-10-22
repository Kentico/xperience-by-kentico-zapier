using CMS.ContentWorkflowEngine;
using CMS.Core;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Websites.UIPages;
using Kentico.Xperience.Zapier.Helpers;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

using Microsoft.AspNetCore.Http;

namespace Kentico.Xperience.Zapier.Triggers.Handlers;

internal class WorkflowPagesHandler : ZapierWorkflowHandler
{
    public WorkflowPagesHandler(
        ZapierTriggerInfo zapierTrigger,
        IEventLogService? eventLogService,
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IAdminLinkService adminLinkService)
        : base(zapierTrigger, eventLogService, client, httpContextAccessor, adminLinkService)
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

        var pageParams = AdminUrlHelper.GetWebPageParams(e.ID, e.WebsiteChannelID, e.ContentLanguageName);
        string adminLink = AdminLinkService.GenerateAdminLink<ContentTab>(pageParams, GetHostDomain());

        data.TryAdd("AdminLink", adminLink);


        if (ZapierTrigger != null)
        {
            _ = DoPost(ZapierTrigger.ZapierTriggerZapierURL, data);
        }
    }
}
