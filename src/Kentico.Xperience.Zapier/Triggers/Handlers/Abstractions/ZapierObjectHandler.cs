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


    private async Task SendPostToWebhook(string url, BaseInfo data) => await DoPost(url, data.TozapierDictionary());
}
