using CMS.Core;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

namespace Kentico.Xperience.Zapier.Tests.Support;

/// <summary>
/// Minimal <see cref="ZapierTriggerHandler"/> that records registration calls and exposes <see cref="DoPost"/>.
/// </summary>
internal sealed class RecordingTriggerHandler : ZapierTriggerHandler
{
    /// <summary>Shared client for handlers that never send (HttpClient is meant to be long-lived).</summary>
    private static readonly HttpClient noopClient = new();


    public int RegisterCalls { get; private set; }

    public int UnregisterCalls { get; private set; }


    public RecordingTriggerHandler(ZapierTriggerInfo trigger, IEventLogService? eventLogService = null, HttpClient? client = null)
        : base(trigger, eventLogService, client ?? noopClient)
    {
    }


    public override bool RegistrationProcessor(bool register = true)
    {
        if (register)
        {
            RegisterCalls++;
        }
        else
        {
            UnregisterCalls++;
        }

        return true;
    }


    public Task PostAsync(string url, Dictionary<string, object>? content) => DoPost(url, content);
}
