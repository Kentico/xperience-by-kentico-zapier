
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineForms;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Admin;
using Kentico.Xperience.Zapier.Admin.UIPages;

namespace Kentico.Xperience.Zapier;

public class ZapierTriggerHandler
{
    private IEventLogService? eventLogService;

    public ZapierTriggerInfo ZapierTrigger { get; set; }

    public IEventLogService LogService
    {
        get
        {
            eventLogService ??= Service.Resolve<IEventLogService>();

            return eventLogService;
        }
    }

    public ZapierTriggerHandler(ZapierTriggerInfo zapInfo) => ZapierTrigger = zapInfo;

    public bool Register()
    {
        if (ZapierTrigger != null)
        {
            var typeInfo = ObjectTypeManager.GetTypeInfo(ZapierTrigger.mZapierTriggerObjectType);

            if (!Enum.TryParse(ZapierTrigger.ZapierTriggerEventType, out ZapierTriggerEvents eventType))
            {
                return false;
            }

            switch (eventType)
            {
                case ZapierTriggerEvents.Create:

                    if (ZapierTrigger.IsForm())
                    {
                        BizFormItemEvents.Insert.After += FormHandler;
                    }
                    else
                    {
                        typeInfo.Events.Insert.After += ObjectInfoHandler;
                    }

                    LogService.LogEvent(EventTypeEnum.Information, nameof(ZapierEventRegistrationService), "REGISTER", $"Registered handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} create event.");
                    break;

                case ZapierTriggerEvents.Update:
                    if (ZapierTrigger.IsForm())
                    {
                        BizFormItemEvents.Update.After += FormHandler;
                    }
                    else
                    {
                        typeInfo.Events.Update.After += ObjectInfoHandler;
                    }

                    LogService.LogEvent(EventTypeEnum.Information, nameof(ZapierEventRegistrationService), "REGISTER", $"Registered handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} update event.");
                    break;
                case ZapierTriggerEvents.Delete:
                    if (ZapierTrigger.IsForm())
                    {
                        BizFormItemEvents.Delete.Before += FormHandler;
                    }
                    else
                    {
                        typeInfo.Events.Delete.Before += ObjectInfoHandler;
                    }
                    LogService.LogEvent(EventTypeEnum.Information, nameof(ZapierEventRegistrationService), "REGISTER", $"Registered handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} delete event.");
                    break;
                case ZapierTriggerEvents.None:
                    LogService.LogEvent(EventTypeEnum.Error, nameof(ZapierEventRegistrationService), "REGISTER", $"Handler '{ZapierTrigger.ZapierTriggerDisplayName}' could not be registered.");
                    break;
                default:
                    return false;
            }

            return true;
        }

        LogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), "REGISTER", $"Unable to register handler: not found.");
        return false;
    }


    public bool Unregister()
    {
        if (ZapierTrigger != null)
        {
            var typeInfo = ObjectTypeManager.GetTypeInfo(ZapierTrigger.mZapierTriggerObjectType);

            if (!Enum.TryParse(ZapierTrigger.ZapierTriggerEventType, out ZapierTriggerEvents eventType))
            {
                return false;
            }

            switch (eventType)
            {
                case ZapierTriggerEvents.Create:

                    if (ZapierTrigger.IsForm())
                    {
                        BizFormItemEvents.Insert.After -= FormHandler;
                    }
                    else
                    {
                        typeInfo.Events.Insert.After -= ObjectInfoHandler;
                    }

                    LogService.LogEvent(EventTypeEnum.Information, nameof(ZapierEventRegistrationService), "REGISTER", $"Registered handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} create event.");
                    break;

                case ZapierTriggerEvents.Update:
                    if (ZapierTrigger.IsForm())
                    {
                        BizFormItemEvents.Update.After -= FormHandler;
                    }
                    else
                    {
                        typeInfo.Events.Update.After -= ObjectInfoHandler;
                    }

                    LogService.LogEvent(EventTypeEnum.Information, nameof(ZapierEventRegistrationService), "REGISTER", $"Registered handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} update event.");
                    break;
                case ZapierTriggerEvents.Delete:
                    if (ZapierTrigger.IsForm())
                    {
                        BizFormItemEvents.Delete.Before -= FormHandler;
                    }
                    else
                    {
                        typeInfo.Events.Delete.Before -= ObjectInfoHandler;
                    }
                    LogService.LogEvent(EventTypeEnum.Information, nameof(ZapierEventRegistrationService), "REGISTER", $"Registered handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} delete event.");
                    break;
                case ZapierTriggerEvents.None:
                    LogService.LogEvent(EventTypeEnum.Error, nameof(ZapierEventRegistrationService), "REGISTER", $"Handler '{ZapierTrigger.ZapierTriggerDisplayName}' could not be registered.");
                    break;
                default:
                    return false;
            }

            return true;

        }

        LogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), "REGISTER", $"Handler could not be unregistered.");
        return false;
    }


    //private bool HandlerRegistrationConnector(bool register)
    //{
    //    if (ZapierTrigger != null)
    //    {
    //        var typeInfo = ObjectTypeManager.GetTypeInfo(ZapierTrigger.mZapierTriggerObjectType);

    //        if (!Enum.TryParse(ZapierTrigger.ZapierTriggerEventType, out ZapierTriggerEvents eventType))
    //        {
    //            return false;
    //        }

    //        if (typeInfo != null)
    //        {
    //            switch (eventType)
    //            {
    //                case ZapierTriggerEvents.Create:

    //                    if (ZapierTrigger.IsForm())
    //                    {
    //                        BizFormItemEvents.Insert.After += FormHandler;
    //                    }
    //                    else
    //                    {
    //                        typeInfo.Events.Insert.After += ObjectInfoHandler;
    //                    }

    //                    eventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierEventRegistrationService), "REGISTER", $"Registered handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} create event.");
    //                    break;

    //                case ZapierTriggerEvents.Update:
    //                    if (ZapierTrigger.IsForm())
    //                    {
    //                        BizFormItemEvents.Update.After += FormHandler;
    //                    }
    //                    else
    //                    {
    //                        typeInfo.Events.Update.After += ObjectInfoHandler;
    //                    }

    //                    eventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierEventRegistrationService), "REGISTER", $"Registered handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} update event.");
    //                    break;

    //                case ZapierTriggerEvents.Delete:
    //                    if (ZapierTrigger.IsForm())
    //                    {
    //                        BizFormItemEvents.Delete.After += FormHandler;
    //                    }
    //                    else
    //                    {
    //                        typeInfo.Events.Delete.After += ObjectInfoHandler;
    //                    }
    //                    eventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierEventRegistrationService), "REGISTER", $"Registered handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} delete event.");
    //                    break;

    //                case ZapierTriggerEvents.None:
    //                    eventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierEventRegistrationService), "REGISTER", $"Handler '{ZapierTrigger.ZapierTriggerDisplayName}' could not be registered.");
    //                    break;

    //                default:
    //                    return false;
    //            }

    //            return true;
    //        }
    //        else
    //        {
    //            LogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), "REGISTER", $"Unable to register handler '{ZapierTrigger.ZapierTriggerDisplayName}': TypeInfo for {ZapierTrigger.mZapierTriggerObjectType} not found.");
    //            return false;
    //        }

    //    }

    //    LogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), "REGISTER", $"Unable to register handler: not found.");
    //    return false;

    //}

    private void ObjectInfoHandler(object? sender, ObjectEventArgs e) => Handler(e.Object);
    private void FormHandler(object? sender, BizFormItemEventArgs e)
    {
        if (!ZapierTrigger.mZapierTriggerObjectType.Equals(e.Item.ClassName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Handler(e.Item);

    }




    private void Handler(BaseInfo @object)
    {
        //CMS thread?
        if (ZapierTrigger != null && ZapierTrigger.ZapierTriggerEnabled && @object != null)
        {
            ZapierHelper.SendPostToWebhook(ZapierTrigger.ZapierTriggerZapierURL, @object);
        }
    }
}
