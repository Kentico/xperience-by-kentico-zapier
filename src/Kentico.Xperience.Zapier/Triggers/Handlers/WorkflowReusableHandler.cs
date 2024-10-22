using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.ContentWorkflowEngine;
using CMS.Core;
using CMS.DataEngine;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.Zapier.Helpers;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

using Microsoft.AspNetCore.Http;

namespace Kentico.Xperience.Zapier.Triggers.Handlers;

internal class WorkflowReusableHandler : ZapierWorkflowHandler
{
    private readonly IInfoProvider<ContentLanguageInfo> contentLanguageProvider;
    private readonly IInfoProvider<ContentItemInfo> contentInfoProvider;

    public WorkflowReusableHandler(ZapierTriggerInfo zapierTrigger,
        IEventLogService? eventLogService,
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IInfoProvider<ContentLanguageInfo> contentLanguageProvider,
        IInfoProvider<ContentItemInfo> contentInfoProvider,
        IAdminLinkService adminLinkService)
        : base(zapierTrigger, eventLogService, client, httpContextAccessor, adminLinkService)
    {
        this.contentLanguageProvider = contentLanguageProvider;
        this.contentInfoProvider = contentInfoProvider;
    }


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

        var contentItemInfo = contentInfoProvider.Get(e.ID);
        var info = contentLanguageProvider.Get(e.ContentLanguageID);

        var pageParams = AdminUrlHelper.GetReusableParams(e.ID, contentItemInfo.ContentItemContentFolderID,
            info.ContentLanguageName);

        string adminLink = AdminLinkService.GenerateAdminLink<ContentItemEdit>(pageParams, GetHostDomain());

        data.TryAdd("AdminLink", adminLink);


        if (ZapierTrigger != null)
        {
            _ = DoPost(ZapierTrigger.ZapierTriggerZapierURL, data);
        }
    }
}
