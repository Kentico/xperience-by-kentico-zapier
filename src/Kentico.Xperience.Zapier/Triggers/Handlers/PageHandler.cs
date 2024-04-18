using CMS.Core;
using CMS.Websites;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers.Extensions;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

namespace Kentico.Xperience.Zapier.Triggers.Handlers;

internal class PageHandler : ZapierContentItemHandler
{
    public PageHandler(ZapierTriggerInfo zapierTrigger, IEventLogService? eventLogService, HttpClient client) : base(zapierTrigger, eventLogService, client)
    {
    }


    public override bool RegistrationProcessor(bool register = true)
    {
        if (!Enum.TryParse(ZapierTrigger.ZapierTriggerEventType, out ZapierTriggerEvents eventType))
        {
            return false;
        }
        switch (eventType)
        {
            case ZapierTriggerEvents.Create:

                if (register)
                {
                    WebPageEvents.Create.After += PagesCreateHandler;
                }
                else
                {
                    WebPageEvents.Create.After -= PagesCreateHandler;
                }

                break;

            case ZapierTriggerEvents.Update:

                if (register)
                {
                    WebPageEvents.UpdateDraft.After += PagesUpdateHandler;
                }
                else
                {
                    WebPageEvents.UpdateDraft.After -= PagesUpdateHandler;
                }

                break;
            case ZapierTriggerEvents.Delete:

                if (register)
                {
                    WebPageEvents.Delete.Execute += PagesDeleteHandler;
                }
                else
                {
                    WebPageEvents.Delete.Execute -= PagesDeleteHandler;
                }
                break;
            case ZapierTriggerEvents.Publish:

                if (register)
                {
                    WebPageEvents.Publish.Execute += PagesPublishHandler;
                }
                else
                {
                    WebPageEvents.Publish.Execute -= PagesPublishHandler;
                }
                break;
            case ZapierTriggerEvents.None:
                EventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for pages handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {eventType} was not successful.");
                break;
            default:
                EventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for pages handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {eventType} was not successful.");
                return false;
        }
        EventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for pages handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {eventType} was successful.");
        return true;
    }


    private void PagesCreateHandler(object? sender, CreateWebPageEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);


    private void PagesUpdateHandler(object? sender, UpdateWebPageDraftEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);


    private void PagesDeleteHandler(object? sender, DeleteWebPageEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);


    private void PagesPublishHandler(object? sender, PublishWebPageEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);
}
