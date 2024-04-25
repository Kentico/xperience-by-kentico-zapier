using CMS.Core;
using CMS.Headless;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers.Extensions;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

namespace Kentico.Xperience.Zapier.Triggers.Handlers;

internal class HeadlessHandler : ZapierContentItemHandler
{
    public HeadlessHandler(ZapierTriggerInfo zapierTrigger, IEventLogService? eventLogService, HttpClient client) : base(zapierTrigger, eventLogService, client)
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
                    HeadlessItemEvents.Create.After += HeadlessCreateHandler;
                }
                else
                {
                    HeadlessItemEvents.Create.After -= HeadlessCreateHandler;
                }

                break;

            case ZapierTriggerEvents.Update:

                if (register)
                {
                    HeadlessItemEvents.UpdateDraft.After += HeadlessUpdateHandler;
                }
                else
                {
                    HeadlessItemEvents.UpdateDraft.After -= HeadlessUpdateHandler;
                }

                break;
            case ZapierTriggerEvents.Delete:

                if (register)
                {
                    HeadlessItemEvents.Delete.Execute += HeadlessDeleteHandler;
                }
                else
                {
                    HeadlessItemEvents.Delete.Execute -= HeadlessDeleteHandler;
                }
                break;
            case ZapierTriggerEvents.Publish:

                if (register)
                {
                    HeadlessItemEvents.Publish.Execute += HeadlessPublishHandler;
                }
                else
                {
                    HeadlessItemEvents.Publish.Execute -= HeadlessPublishHandler;
                }
                break;
            case ZapierTriggerEvents.None:
                EventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for headless handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {eventType} was not successful.");
                break;
            default:
                EventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for headless handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {eventType} was not successful.");
                return false;
        }

        EventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for headless handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {eventType} was successful.");
        return true;
    }


    private void HeadlessDeleteHandler(object? sender, DeleteHeadlessItemEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);


    private void HeadlessUpdateHandler(object? sender, UpdateHeadlessItemDraftEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);


    private void HeadlessCreateHandler(object? sender, CreateHeadlessItemEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);


    private void HeadlessPublishHandler(object? sender, PublishHeadlessItemEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);
}
