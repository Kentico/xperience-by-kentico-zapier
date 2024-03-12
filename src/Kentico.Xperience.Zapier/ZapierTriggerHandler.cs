using System.Net;
using System.Net.Http.Json;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.OnlineForms;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Admin;
using Kentico.Xperience.Zapier.Admin.UIPages;

namespace Kentico.Xperience.Zapier;

public class ZapierTriggerHandler
{
    private readonly IEventLogService? eventLogService;

    private readonly HttpClient client;

    public ZapierTriggerInfo ZapierTrigger { get; set; }


    public ZapierTriggerHandler(ZapierTriggerInfo zapInfo, HttpClient httpClient, IEventLogService eventLogService)
    {
        ZapierTrigger = zapInfo;
        client = httpClient;
        this.eventLogService = eventLogService;
    }

    public bool Register() => RegistrationProcessor();

    public bool Unregister() => RegistrationProcessor(false);

    private bool RegistrationProcessor(bool toRegister = true)
    {
        if (ZapierTrigger != null)
        {
            if (!Enum.TryParse(ZapierTrigger.ZapierTriggerEventType, out ZapierTriggerEvents eventType))
            {
                return false;
            }

            if (!Enum.TryParse(ZapierTrigger.ZapierTriggerObjectClassType, out ZapierTriggerObjectClassType classType))
            {
                return false;
            }

            bool result = classType switch
            {
                ZapierTriggerObjectClassType.Other => RegisterInfoObject(eventType, toRegister),
                ZapierTriggerObjectClassType.System => RegisterInfoObject(eventType, toRegister),
                ZapierTriggerObjectClassType.Form => RegisterForm(eventType, toRegister),
                ZapierTriggerObjectClassType.Website => throw new NotImplementedException(),
                ZapierTriggerObjectClassType.Reusable => throw new NotImplementedException(),
                ZapierTriggerObjectClassType.Email => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };

            return result;
        }

        eventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), $"{(toRegister ? "REGISTER" : "UNREGISTER")}", $"Handler (un)registration was not successful");
        return false;
    }


    private bool RegisterInfoObject(ZapierTriggerEvents eventType, bool register)
    {
        var typeInfo = ObjectTypeManager.GetTypeInfo(ZapierTrigger.mZapierTriggerObjectType);

        var objHandler = eventType switch
        {
            ZapierTriggerEvents.Create => typeInfo.Events.Insert,
            ZapierTriggerEvents.Update => typeInfo.Events.Update,
            ZapierTriggerEvents.Delete => typeInfo.Events.Delete,
            _ => null
        };

        if (objHandler is null)
        {
            eventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for info object handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} for event {eventType} was not successful.");
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
        eventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for info object handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} for event {eventType} was successful.");
        return true;
    }


    private bool RegisterForm(ZapierTriggerEvents eventType, bool register)
    {
        var objHandler = eventType switch
        {
            ZapierTriggerEvents.Create => BizFormItemEvents.Insert,
            ZapierTriggerEvents.Update => BizFormItemEvents.Update,
            ZapierTriggerEvents.Delete => BizFormItemEvents.Delete,
            _ => null
        };

        if (objHandler is null)
        {
            eventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for form handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} for event {eventType} was not successful.");
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
        eventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for form handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} for event {eventType} was successful.");
        return true;
    }


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
        if (ZapierTrigger != null && @object != null)
        {
            _ = SendPostToWebhook(ZapierTrigger.ZapierTriggerZapierURL, @object);
        }
    }

    private async Task SendPostToWebhook(string url, BaseInfo data) => await DoPost(url, data.TozapierDictionary());

    #region PostData
    private async Task DoPost(string url, Dictionary<string, object>? content)
    {
        var contentResult = content ?? [];
        if (DataHelper.IsEmpty(url))
        {
            return;
        }

        if (!contentResult.TryAdd("AppId", ZapierConstants.AppId))
        {
            return;
        }

        var response = await client.PostAsJsonAsync(new Uri(url), content);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            string message = await response.Content.ReadAsStringAsync();

            eventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), "POST", $"POST to {url} failed with the following message:<br/> {message}");
        }
    }
    #endregion
}
