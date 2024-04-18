using CMS.ContentEngine;
using CMS.Core;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers.Extensions;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

namespace Kentico.Xperience.Zapier.Triggers.Handlers;

internal class ReusableHandler : ZapierContentItemHandler
{
    public ReusableHandler(ZapierTriggerInfo zapierTrigger, IEventLogService? eventLogService, HttpClient client) : base(zapierTrigger, eventLogService, client)
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
                    ContentItemEvents.Create.After += ReusableCreateHandler;
                }
                else
                {
                    ContentItemEvents.Create.After -= ReusableCreateHandler;
                }

                break;

            case ZapierTriggerEvents.Update:

                if (register)
                {
                    ContentItemEvents.UpdateDraft.After += ReusableUpdateHandler;
                }
                else
                {
                    ContentItemEvents.UpdateDraft.After -= ReusableUpdateHandler;
                }

                break;
            case ZapierTriggerEvents.Delete:

                if (register)
                {
                    ContentItemEvents.Delete.Execute += ReusableDeleteHandler;
                }
                else
                {
                    ContentItemEvents.Delete.Execute -= ReusableDeleteHandler;
                }
                break;
            case ZapierTriggerEvents.Publish:

                if (register)
                {
                    ContentItemEvents.Publish.Execute += ReusablePublishHandler;
                }
                else
                {
                    ContentItemEvents.Publish.Execute -= ReusablePublishHandler;
                }
                break;
            case ZapierTriggerEvents.None:
                EventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for reusable handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {eventType} was not successful.");
                break;
            default:
                EventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for reusable handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {eventType} was not successful.");
                return false;

        }

        EventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for reusable handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {eventType} was successful.");
        return true;
    }


    private void ReusableDeleteHandler(object? sender, DeleteContentItemEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);


    private void ReusableUpdateHandler(object? sender, UpdateContentItemDraftEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);


    private void ReusableCreateHandler(object? sender, CreateContentItemEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);


    private void ReusablePublishHandler(object? sender, PublishContentItemEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);
}
