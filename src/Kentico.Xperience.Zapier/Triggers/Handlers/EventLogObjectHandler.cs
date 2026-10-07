using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

namespace Kentico.Xperience.Zapier.Triggers.Handlers;

internal class EventLogObjectHandler : ZapierObjectHandler
{
    private readonly IProgressiveCache progressiveCache;
    private readonly IInfoProvider<ZapierTriggerEventLogTypeInfo> zapierTriggerEventLogTypeInfoProvider;


    public EventLogObjectHandler(ZapierTriggerInfo zapierTrigger,
        IEventLogService? eventLogService,
        HttpClient client,
        IProgressiveCache progressiveCache,
        IInfoProvider<ZapierTriggerEventLogTypeInfo> zapierTriggerEventLogTypeInfoProvider)
        : base(zapierTrigger, eventLogService, client)
    {
        this.progressiveCache = progressiveCache;
        this.zapierTriggerEventLogTypeInfoProvider = zapierTriggerEventLogTypeInfoProvider;
    }


    protected override string DeliveryFailureEventCode => EventLogDeliveryEventCode;


    public override bool RegistrationProcessor(bool register = true)
    {
        if (register)
        {
            EventLogEvents.LogEvent.After += LogEventHandler;
        }
        else
        {
            EventLogEvents.LogEvent.After -= LogEventHandler;
        }
        EventLogService.LogEvent(EventTypeEnum.Information, nameof(ZapierTriggerHandler), $"{(register ? "REGISTER" : "UNREGISTER")}", $"Action for info object handler '{ZapierTrigger.ZapierTriggerCodeName}' to {ZapierTrigger.ZapierTriggerObjectType} for event {ZapierTrigger.ZapierTriggerEventType} was successful.");
        return true;
    }


    // Internal so unit tests can feed log entries directly: the test base class disables event logging,
    // so EventLogEvents.LogEvent never completes there.
    internal void LogEventHandler(object? sender, LogEventArgs e)
    {
        if (!ZapierTrigger.ZapierTriggerObjectType.Equals(EventLogInfo.OBJECT_TYPE, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Delivery failures of event log triggers are never forwarded: a failing event log trigger would post its own
        // failure to the same Zap URL again, and two failing event log triggers would forward each other's failures,
        // both looping forever. Failures of other triggers are forwarded, so a Zap can alert on them.
        if (IsEventLogDeliveryFailure(e.Event))
        {
            return;
        }

        var eventTypes = progressiveCache.Load((cacheSettings) =>
        {
            // Severities are stored after the trigger row is inserted (and this handler is already registered),
            // so the cached list must also be invalidated when severity rows change, not only the trigger itself.
            cacheSettings.CacheDependency = CacheHelper.GetCacheDependency(
            [
                $"{ZapierTriggerInfo.OBJECT_TYPE}|byid|{ZapierTrigger.ZapierTriggerID}",
                $"{ZapierTriggerEventLogTypeInfo.OBJECT_TYPE}|all",
            ]);
            return zapierTriggerEventLogTypeInfoProvider.Get()
            .WhereEquals(nameof(ZapierTriggerEventLogTypeInfo.ZapierTriggerEventLogTypeZapierTriggerID), ZapierTrigger.ZapierTriggerID)
            .Column(nameof(ZapierTriggerEventLogTypeInfo.ZapierTriggerEventLogTypeType))
            .GetListResult<string>();
        }, new CacheSettings(TimeSpan.FromHours(1).TotalMinutes, $"{ZapierTriggerEventLogTypeInfo.OBJECT_TYPE}{ZapierTrigger.ZapierTriggerID}"));

        if (eventTypes.Contains(e.Event.EventType))
        {
            Handler(e.Event);
        }
    }
}
