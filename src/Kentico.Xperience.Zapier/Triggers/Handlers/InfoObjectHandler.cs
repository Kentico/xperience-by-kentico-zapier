using CMS.Core;
using CMS.DataEngine;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers.Extensions;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

namespace Kentico.Xperience.Zapier.Triggers.Handlers;

internal class InfoObjectHandler : ZapierObjectHandler
{
    public InfoObjectHandler(ZapierTriggerInfo zapierTrigger, IEventLogService? eventLogService, HttpClient client) : base(zapierTrigger, eventLogService, client)
    {
    }

    public override bool RegistrationProcessor(bool register = true)
    {
        var typeInfo = ObjectTypeManager.GetTypeInfo(ZapierTrigger.ZapierTriggerObjectType);
        if (!Enum.TryParse(ZapierTrigger.ZapierTriggerEventType, out ZapierTriggerEvents eventType))
        {
            return false;
        }
        var objHandler = eventType switch
        {
            ZapierTriggerEvents.Create => typeInfo.Events.Insert,
            ZapierTriggerEvents.Update => typeInfo.Events.Update,
            ZapierTriggerEvents.Delete => typeInfo.Events.Delete,
            ZapierTriggerEvents.None => null,
            ZapierTriggerEvents.Publish => null,
            _ => null
        };
        if (objHandler is null)
        {
            EventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for info object handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {eventType} was not successful.");
            return false;
        }

        if (eventType is ZapierTriggerEvents.Delete)
        {
            if (register)
            {
                objHandler.Before += ObjectInfoHandler;
            }
            else
            {
                objHandler.Before -= ObjectInfoHandler;
            }
        }
        else
        {
            if (register)
            {
                objHandler.After += ObjectInfoHandler;
            }
            else
            {
                objHandler.After -= ObjectInfoHandler;
            }
        }
        EventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for info object handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {eventType} was successful.");
        return true;
    }
    private void ObjectInfoHandler(object? sender, ObjectEventArgs e) => Handler(e.Object);
}
