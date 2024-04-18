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


    private void LogEventHandler(object? sender, LogEventArgs e)
    {
        if (!ZapierTrigger.ZapierTriggerObjectType.Equals(EventLogInfo.OBJECT_TYPE, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var eventTypes = progressiveCache.Load((cacheSettings) =>
        {
            cacheSettings.CacheDependency = CacheHelper.GetCacheDependency($"{ZapierTriggerInfo.OBJECT_TYPE}|byid|{ZapierTrigger.ZapierTriggerID}");
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
