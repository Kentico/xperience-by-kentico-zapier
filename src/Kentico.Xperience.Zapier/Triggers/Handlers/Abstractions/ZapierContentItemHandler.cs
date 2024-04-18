using CMS.Core;

using Kentico.Integration.Zapier;

namespace Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

internal abstract class ZapierContentItemHandler : ZapierTriggerHandler
{
    protected ZapierContentItemHandler(ZapierTriggerInfo zapierTrigger, IEventLogService? eventLogService, HttpClient client) : base(zapierTrigger, eventLogService, client)
    {
    }


    protected void ContentItemsHandler(Func<string, Dictionary<string, object>?> dataCaller, string contentTypeName)
    {
        if (!ZapierTrigger.ZapierTriggerObjectType.Equals(contentTypeName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var data = dataCaller(contentTypeName);

        if (ZapierTrigger != null && data != null)
        {
            _ = DoPost(ZapierTrigger.ZapierTriggerZapierURL, data);
        }
    }
}
