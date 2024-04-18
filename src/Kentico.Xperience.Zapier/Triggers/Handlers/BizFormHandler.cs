using CMS.Core;
using CMS.OnlineForms;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers.Extensions;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

namespace Kentico.Xperience.Zapier.Triggers.Handlers;

internal class BizFormHandler : ZapierObjectHandler
{
    public BizFormHandler(ZapierTriggerInfo zapierTrigger, IEventLogService? eventLogService, HttpClient client) : base(zapierTrigger, eventLogService, client)
    {
    }


    public override bool RegistrationProcessor(bool register = true)
    {
        if (!Enum.TryParse(ZapierTrigger.ZapierTriggerEventType, out ZapierTriggerEvents eventType))
        {
            return false;
        }
        var objHandler = eventType switch
        {
            ZapierTriggerEvents.Create => BizFormItemEvents.Insert,
            ZapierTriggerEvents.Update => BizFormItemEvents.Update,
            ZapierTriggerEvents.Delete => BizFormItemEvents.Delete,
            ZapierTriggerEvents.None => null,
            ZapierTriggerEvents.Publish => null,
            _ => null
        };

        if (objHandler is null)
        {
            EventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for form handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {eventType} was not successful.");
            return false;
        }

        if (eventType is ZapierTriggerEvents.Delete)
        {
            if (register)
            {
                objHandler.Before += FormHandler;
            }
            else
            {
                objHandler.Before -= FormHandler;
            }
        }
        else
        {
            if (register)
            {
                objHandler.After += FormHandler;
            }
            else
            {
                objHandler.After -= FormHandler;
            }
        }
        EventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for form handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {eventType} was successful.");
        return true;
    }


    private void FormHandler(object? sender, BizFormItemEventArgs e)
    {
        if (!ZapierTrigger.ZapierTriggerObjectType.Equals(e.Item.ClassName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        Handler(e.Item);
    }
}
