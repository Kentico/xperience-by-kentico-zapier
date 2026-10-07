using CMS.Core;
using CMS.DataEngine;

using Kentico.Integration.Zapier;

namespace Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

internal abstract class ZapierObjectHandler : ZapierTriggerHandler
{
    protected ZapierObjectHandler(ZapierTriggerInfo zapierTrigger, IEventLogService? eventLogService, HttpClient client) : base(zapierTrigger, eventLogService, client)
    {
    }


    protected void Handler(BaseInfo @object)
    {
        if (ZapierTrigger != null && @object != null)
        {
            _ = SendPostToWebhook(ZapierTrigger.ZapierTriggerZapierURL, @object);
        }
    }


    private async Task SendPostToWebhook(string url, BaseInfo data)
    {
        Dictionary<string, object>? content;
        try
        {
            content = data.TozapierDictionary();
        }
        catch (Exception ex)
        {
            // Runs inside the fire-and-forget task started by Handler, so an unlogged exception here would be lost.
            LogDeliveryFailure(url, ex);
            return;
        }

        await DoPost(url, content);
    }
}
