using System.Net;
using System.Net.Http.Headers;
using System.Text;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Integration.Zapier;

namespace Kentico.Xperience.Zapier;

public class ZapierHelper
{
    private static readonly HttpClient client = new();

    private static IEventLogService? mLogService;

    public static IEventLogService LogService
    {
        get
        {
            mLogService ??= Service.Resolve<IEventLogService>();

            return mLogService;
        }
    }

    protected static List<ZapierTriggerHandler> ZapierHandlers = [];


    public static void RegisterWebhook(ZapierTriggerInfo webhook)
    {
        var handler = new ZapierTriggerHandler(webhook);
        ZapierHandlers.Add(handler);
        if (webhook.ZapierTriggerEnabled)
        {
            handler.Register();
        }
    }

    public static void UnregisterWebhook(ZapierTriggerInfo webhook)
    {
        var handler = ZapierHandlers.Where(h => h.ZapierTrigger.ZapierTriggerID == webhook.ZapierTriggerID).FirstOrDefault();
        if (handler != null && handler.Unregister())
        {
            ZapierHandlers.Remove(handler);
        }
    }



    public static bool SendPostToWebhook(string url, BaseInfo data) => DoPost(url, data.ToZapierString());

    private static bool DoPost(string url, string content)
    {
        if (DataHelper.IsEmpty(url))
        {
            return false;
        }

        byte[] buffer = Encoding.UTF8.GetBytes(content);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(url),
            Content = byteContent
        };

        var response = client.SendAsync(httpRequestMessage).Result;
        if (response.StatusCode != HttpStatusCode.OK)
        {
            string message = response.Content.ReadAsStringAsync().Result;

            LogService.LogEvent(EventTypeEnum.Information, nameof(ZapierHelper), "POST", $"POST to {url} failed with the following message:<br/> {message}");
            return false;
        }

        return true;
    }
}
