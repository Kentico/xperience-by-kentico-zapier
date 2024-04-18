using System.Net;
using System.Net.Http.Json;

using CMS.Core;
using CMS.Helpers;

using Kentico.Integration.Zapier;

namespace Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

internal abstract class ZapierTriggerHandler
{
    protected readonly IEventLogService? EventLogService;
    protected readonly HttpClient Client;
    protected ZapierTriggerInfo ZapierTrigger { get; set; }


    protected ZapierTriggerHandler(ZapierTriggerInfo zapierTrigger, IEventLogService? eventLogService, HttpClient client)
    {
        EventLogService = eventLogService;
        Client = client;
        ZapierTrigger = zapierTrigger;
    }


    public bool Register() => RegistrationProcessor();


    public bool Unregister() => RegistrationProcessor(false);


    public abstract bool RegistrationProcessor(bool register = true);


    protected async Task DoPost(string url, Dictionary<string, object>? content)
    {
        if (DataHelper.IsEmpty(url))
        {
            return;
        }

        var response = await Client.PostAsJsonAsync(new Uri(url), content);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            string message = await response.Content.ReadAsStringAsync();

            EventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), "POST", $"POST to {url} failed with the following message:<br/> {message}");
        }
    }
}
