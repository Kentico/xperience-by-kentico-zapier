using System.Net.Http.Json;

using CMS.Core;
using CMS.EventLog;
using CMS.Helpers;

using Kentico.Integration.Zapier;

namespace Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

internal abstract class ZapierTriggerHandler
{
    /// <summary>Event code used for webhook delivery problems logged by <see cref="DoPost"/>.</summary>
    internal const string DeliveryEventCode = "POST";

    /// <summary>Event code used for webhook delivery problems of event log triggers.</summary>
    internal const string EventLogDeliveryEventCode = "POST_EVENTLOG";

    /// <summary>
    /// True when <paramref name="entry"/> is a webhook delivery failure of an event log trigger. Failures of other
    /// triggers use <see cref="DeliveryEventCode"/> and can still be forwarded by event log triggers.
    /// </summary>
    internal static bool IsEventLogDeliveryFailure(EventLogInfo entry) =>
        entry.Source == nameof(ZapierTriggerHandler)
        && entry.EventCode == EventLogDeliveryEventCode;


    private static string DeliveryFailurePrefix(string url) => $"POST to {url} failed";


    /// <summary>Event code of the delivery failures this handler logs.</summary>
    protected virtual string DeliveryFailureEventCode => DeliveryEventCode;

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

        try
        {
            using var response = await Client.PostAsJsonAsync(new Uri(url), content);

            if (!response.IsSuccessStatusCode)
            {
                string message = await response.Content.ReadAsStringAsync();

                // Same severity as the exception path below: a 410 from a disabled Zap is as actionable as an unreachable host.
                EventLogService?.LogEvent(EventTypeEnum.Warning, nameof(ZapierTriggerHandler), DeliveryFailureEventCode, $"{DeliveryFailurePrefix(url)} with status {(int)response.StatusCode} and the following message:<br/> {message}");
            }
        }
        catch (Exception ex)
        {
            // Delivery runs fire-and-forget from object event handlers, so nothing observes the returned task: any failure
            // (unreachable Zap URL, timeout, invalid URL, JSON serialization of the payload) must be logged here or it is lost.
            LogDeliveryFailure(url, ex);
        }
    }


    // Null-conditional: this runs inside catch blocks of fire-and-forget tasks, where a throw would be unobserved.
    protected void LogDeliveryFailure(string url, Exception ex) =>
        EventLogService?.LogEvent(EventTypeEnum.Warning, nameof(ZapierTriggerHandler), DeliveryFailureEventCode, $"{DeliveryFailurePrefix(url)}: {DescribeException(ex)}");


    // The actionable cause of an HttpRequestException (DNS, TLS, connection refused) is in its inner exceptions.
    private static string DescribeException(Exception ex)
    {
        var parts = new List<string>();
        for (var current = ex; current != null; current = current.InnerException)
        {
            parts.Add($"{current.GetType().Name}: {current.Message}");
        }
        return string.Join(" ---> ", parts);
    }
}
