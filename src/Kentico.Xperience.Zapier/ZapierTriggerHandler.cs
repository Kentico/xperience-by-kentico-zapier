using System.Net;
using System.Net.Http.Json;
using CMS.ContentEngine;
using CMS.ContentWorkflowEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.OnlineForms;
using CMS.Websites;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Admin.UIPages;
using Kentico.Xperience.Zapier.Extensions;

namespace Kentico.Xperience.Zapier;

public class ZapierTriggerHandler
{
    private readonly IEventLogService? eventLogService;

    private readonly HttpClient client;
    private readonly IWorkflowScopeService workflowScopeService;
    private readonly IContentHelper contentHelper;

    public ZapierTriggerInfo ZapierTrigger { get; set; }


    public ZapierTriggerHandler(ZapierTriggerInfo zapInfo, HttpClient httpClient, IEventLogService eventLogService, IWorkflowScopeService workflowScopeService, IContentHelper contentHelper)
    {
        ZapierTrigger = zapInfo;
        client = httpClient;
        this.eventLogService = eventLogService;
        this.workflowScopeService = workflowScopeService;
        this.contentHelper = contentHelper;
    }

    public bool Register() => RegistrationProcessor();

    public bool Unregister() => RegistrationProcessor(false);

    private bool RegistrationProcessor(bool toRegister = true)
    {
        if (ZapierTrigger != null)
        {
            if (!Enum.TryParse(ZapierTrigger.ZapierTriggerObjectClassType, out ZapierTriggerObjectClassType classType))
            {
                return false;
            }

            if (!Enum.TryParse(ZapierTrigger.ZapierTriggerEventType, out ZapierTriggerEvents eventType))
            {
                var dataClass = DataClassInfoProvider.ProviderObject.Get(ZapierTrigger.mZapierTriggerObjectType);
                if (dataClass is null || string.IsNullOrEmpty(ZapierTrigger.ZapierTriggerEventType))
                {
                    return false;
                }

                bool isWorkflow = workflowScopeService.IsMatchingWorflowEventPerObject(ZapierTrigger.ZapierTriggerEventType, dataClass.ClassID);

                if (!isWorkflow)
                {
                    return false;
                }

                bool workflowResult = classType switch
                {
                    ZapierTriggerObjectClassType.Website => RegisterWorkflowPages(ZapierTrigger.ZapierTriggerEventType, toRegister),
                    ZapierTriggerObjectClassType.Reusable => RegisterWorkflowReusable(ZapierTrigger.ZapierTriggerEventType, toRegister),
                    _ => throw new NotImplementedException()
                };

                return workflowResult;
            }

            bool result = classType switch
            {
                ZapierTriggerObjectClassType.Other => RegisterInfoObject(eventType, toRegister),
                ZapierTriggerObjectClassType.System => RegisterInfoObject(eventType, toRegister),
                ZapierTriggerObjectClassType.Form => RegisterForm(eventType, toRegister),
                ZapierTriggerObjectClassType.Website => RegisterPages(eventType, toRegister),
                ZapierTriggerObjectClassType.Reusable => RegisterReusable(eventType, toRegister),
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

    private bool RegisterReusable(ZapierTriggerEvents eventType, bool register)
    {
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
            default:
                eventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for reusable handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} for event {eventType} was not successful.");
                return false;

        }

        eventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for reusable handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} for event {eventType} was successful.");
        return true;
    }

    private bool RegisterWorkflowPages(string eventType, bool register)
    {
        if (register)
        {
            WebPageWorkflowEvents.MoveToStep.Execute += WorkflowPagesHandler;
        }
        else
        {
            WebPageWorkflowEvents.MoveToStep.Execute -= WorkflowPagesHandler;
        }

        eventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for pages workflow handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} for event {eventType} was successful.");
        return true;
    }


    private bool RegisterWorkflowReusable(string eventType, bool register)
    {
        if (register)
        {
            ContentItemWorkflowEvents.MoveToStep.Execute += WorkflowReusableHandler;
        }
        else
        {
            ContentItemWorkflowEvents.MoveToStep.Execute -= WorkflowReusableHandler;
        }

        eventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for reusable workflow handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} for event {eventType} was successful.");
        return true;
    }

    private bool RegisterPages(ZapierTriggerEvents eventType, bool register)
    {
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
            default:
                eventLogService.LogEvent(EventTypeEnum.Error, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for pages handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} for event {eventType} was not successful.");
                return false;

        }

        eventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for pages handler '{ZapierTrigger.ZapierTriggerDisplayName}' to {ZapierTrigger.mZapierTriggerObjectType} for event {eventType} was successful.");
        return true;
    }

    #region PagesHandlers

    private void PagesCreateHandler(object? sender, CreateWebPageEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);

    private void PagesUpdateHandler(object? sender, UpdateWebPageDraftEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);

    private void PagesDeleteHandler(object? sender, DeleteWebPageEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);

    private void PagesPublishHandler(object? sender, PublishWebPageEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);
    #endregion

    #region ReusableHandlers
    private void ReusableDeleteHandler(object? sender, DeleteContentItemEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);

    private void ReusableUpdateHandler(object? sender, UpdateContentItemDraftEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);

    private void ReusableCreateHandler(object? sender, CreateContentItemEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);

    private void ReusablePublishHandler(object? sender, PublishContentItemEventArgs e) => ContentItemsHandler(e.ContentItemData.GetDataForZapier, e.ContentTypeName);

    #endregion

    #region WorkflowHandlers

    private void WorkflowReusableHandler(object? sender, ContentItemWorkflowMoveToStepArguments e)
    {
        if (!ZapierTrigger.mZapierTriggerObjectType.Equals(e.ContentTypeName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (ZapierTrigger.ZapierTriggerEventType != e.StepName)
        {
            return;
        }

        if (ZapierTrigger != null)
        {
            PostWorkflowReusable(ZapierTrigger.ZapierTriggerZapierURL, e.Guid, e.ContentTypeName);
        }
    }

    private void WorkflowPagesHandler(object? sender, WebPageWorkflowMoveToStepArguments e)
    {
        if (!ZapierTrigger.mZapierTriggerObjectType.Equals(e.ContentTypeName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (ZapierTrigger.ZapierTriggerEventType != e.StepName)
        {
            return;
        }

        if (ZapierTrigger != null)
        {
            PostWorkflowPages(ZapierTrigger.ZapierTriggerZapierURL, e.Guid, e.ContentTypeName, e.WebsiteChannelName);
        }
    }

    #endregion

    private void ContentItemsHandler(Func<string, Dictionary<string, object>?> dataCaller, string contentTypeName)
    {
        if (!ZapierTrigger.mZapierTriggerObjectType.Equals(contentTypeName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var data = dataCaller(contentTypeName);

        if (ZapierTrigger != null && data != null)
        {
            _ = DoPost(ZapierTrigger.ZapierTriggerZapierURL, data);
        }
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

    private void PostWorkflowPages(string url, Guid webItemGuid, string contentTypeName, string websiteChannelName)
    {
        var data = contentHelper.GetWebItemDataByGuid(webItemGuid, contentTypeName, websiteChannelName).GetAwaiter().GetResult();

        _ = DoPost(url, data);
    }

    private void PostWorkflowReusable(string url, Guid contentItemGuid, string contentTypeName)
    {
        var data = contentHelper.GetContentItemDataByGuid(contentItemGuid, contentTypeName).GetAwaiter().GetResult();

        _ = DoPost(url, data);
    }


    private async Task DoPost(string url, Dictionary<string, object>? content)
    {
        if (DataHelper.IsEmpty(url))
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
