using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Integration.Zapier;
using CMS.Websites;
using CMS.Core;
using CMS.DataEngine.Internal;
using CMS.OnlineForms;
using Kentico.Xperience.Zapier.Admin.UIPages;

namespace Kentico.Xperience.Zapier.Admin;

public interface IZapierEventRegistrationService
{
    Task<bool> SetListenersAsync();
}


internal class ZapierEventRegistrationService : IZapierEventRegistrationService
{
    private readonly IZapierTriggerInfoProvider zapierTriggerInfoProvider;
    private readonly IEventLogService eventLogService;

    public ZapierEventRegistrationService(IZapierTriggerInfoProvider zapierTriggerInfoProvider, IEventLogService eventLogService)
    {
        this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;
        this.eventLogService = eventLogService;
    }

    public async Task<bool> SetListenersAsync()
    {
        var triggers = zapierTriggerInfoProvider.Get().GetEnumerableTypedResult();
        var dataClassProvider = DataClassInfoProvider.ProviderObject;

        //ClassHelper.
        //ClassType.CONTENT_TYPE

        //var x = ObjectTypeManager.GetTypeInfo("DancingGoat.ArticlesSection");
        //ContentItemInfo? joij = null;


        //ContentItemEventHandler

        //WebPageEvents.Create.After += HandleWebPageEvents;
        //ContentItemEvents.Create.After += HandleContentItems;


        //BizFormInfo.TYPEINFO.Events

        //bool type = ContentItemDataTypeUtils.TryGetContentTypeName("DancingGoat.Banner", out string str);


        foreach (var t in triggers)
        {
            //ObjectEvents.
            var typeInfo = ObjectTypeManager.GetTypeInfo(t.mZapierTriggerObjectType);

            //var data = dataClasses.Where(dc => dc.ClassName == t.mZapierTriggerObjectType).FirstOrDefault();

            //DataClassInfoExtensions.
            if (!Enum.TryParse(t.ZapierTriggerEventType, out ZapierTriggerEvents tEvent))
            {
                continue;
            }

            switch (tEvent)
            {
                case ZapierTriggerEvents.Create:

                    if (t.IsForm())
                    {
                        BizFormItemEvents.Insert.After += (sender, e) => FormHandler(t, sender, e);
                    }
                    else
                    {
                        typeInfo.Events.Insert.After += (sender, e) => ObjectInfoHandler(e, t);
                    }


                    eventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierEventRegistrationService), "REGISTER", $"Registered handler '{t.ZapierTriggerDisplayName}' to {t.mZapierTriggerObjectType} create event.");
                    break;
                case ZapierTriggerEvents.Update:
                    if (t.IsForm())
                    {
                        BizFormItemEvents.Update.After += (sender, e) => FormHandler(t, sender, e);
                    }
                    else
                    {
                        typeInfo.Events.Update.After += (sender, e) => ObjectInfoHandler(e, t);
                    }

                    eventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierEventRegistrationService), "REGISTER", $"Registered handler '{t.ZapierTriggerDisplayName}' to {t.mZapierTriggerObjectType} update event.");
                    break;
                case ZapierTriggerEvents.Delete:
                    if (t.IsForm())
                    {
                        BizFormItemEvents.Delete.After += (sender, e) => FormHandler(t, sender, e);
                    }
                    else
                    {
                        typeInfo.Events.Delete.After += (sender, e) => ObjectInfoHandler(e, t);
                    }
                    eventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierEventRegistrationService), "REGISTER", $"Registered handler '{t.ZapierTriggerDisplayName}' to {t.mZapierTriggerObjectType} delete event.");
                    break;
                case ZapierTriggerEvents.None:
                    eventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierEventRegistrationService), "REGISTER", $"Handler '{t.ZapierTriggerDisplayName}' could not be registered.");
                    break;
                default:
                    break;
            }
        }

        return true;
    }

    private void FormHandler(ZapierTriggerInfo trigger, object? sender, BizFormItemEventArgs e)
    {
        if (!trigger.ZapierTriggerObjectType.Contains(e.Item.ClassName, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        Handler(trigger, e.Item);
    }

    private void ObjectInfoHandler(ObjectEventArgs e, ZapierTriggerInfo trigger) => Handler(trigger, e.Object);

    private void Handler(ZapierTriggerInfo trigger, BaseInfo @object)
    {
        if (trigger != null && trigger.ZapierTriggerEnabled && @object != null)
        {
            bool status = ZapierHelper.SendPostToWebhook(trigger.ZapierTriggerZapierURL, @object);
        }
    }



    private void HandleWebPageEvents(object? sender, CreateWebPageEventArgs e)
    {
        //TODO
        return;
    }


    private void HandleContentItems(object? sender, CreateContentItemEventArgs e)
    {
        //TODO
        return;
    }




}
