using System.Text.Json;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Tests;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Tests.Support;
using Kentico.Xperience.Zapier.Triggers.Handlers;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Triggers.Handlers;

/// <summary>
/// Covers severity filtering and cache invalidation of the event log trigger handler,
/// using the real <see cref="IProgressiveCache"/> so cache dependencies are exercised, not mocked.
/// Entries are fed to the handler directly because the Kentico test base disables event logging,
/// so <c>EventLogEvents.LogEvent</c> never reaches its After phase in unit tests.
/// </summary>
[TestFixture]
public class EventLogObjectHandlerTests : UnitTests
{
    private const string WEBHOOK_URL = "https://hooks.zapier.com/hooks/standard/1/eventlog/";
    private const int TRIGGER_ID = 11;

    // Deliveries run fire-and-forget. Positive waits fail after DELIVERY_TIMEOUT; negative checks ("not delivered")
    // can only wait a while, SETTLE_DELAY, for a wrong delivery to show up.
    private static readonly TimeSpan DELIVERY_TIMEOUT = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan SETTLE_DELAY = TimeSpan.FromMilliseconds(300);

    private IInfoProviderFake<ZapierTriggerEventLogTypeInfo, IInfoProvider<ZapierTriggerEventLogTypeInfo>> severityFake = null!;
    private StubHttpMessageHandler http = null!;
    private EventLogObjectHandler handler = null!;


    [SetUp]
    public void SetUp()
    {
        Fake<ZapierTriggerInfo>();
        Fake<EventLogInfo>();
        severityFake = Fake<ZapierTriggerEventLogTypeInfo, IInfoProvider<ZapierTriggerEventLogTypeInfo>>();
        severityFake.WithData();
        http = new StubHttpMessageHandler();

        var trigger = new ZapierTriggerInfo
        {
            ZapierTriggerID = TRIGGER_ID,
            ZapierTriggerCodeName = "evtlog11",
            ZapierTriggerObjectType = EventLogInfo.OBJECT_TYPE,
            ZapierTriggerObjectClassType = "Other",
            ZapierTriggerEventType = "Create",
            ZapierTriggerZapierURL = WEBHOOK_URL,
        };
        handler = new EventLogObjectHandler(
            trigger,
            Substitute.For<IEventLogService>(),
            new HttpClient(http),
            Service.Resolve<IProgressiveCache>(),
            severityFake.Provider());
    }


    [Test]
    public async Task LogEvent_PostsEntriesWhoseSeverityIsSubscribed()
    {
        severityFake.WithData(Severity(1, "I"), Severity(2, "E"));

        RaiseLogEvent("I", "REGISTER");
        RaiseLogEvent("W", "IGNORED");
        RaiseLogEvent("E", "EXCEPTION");
        await WaitForDeliveries(2);

        Assert.Multiple(() =>
        {
            Assert.That(http.Requests.Select(r => r.Request.RequestUri), Is.All.EqualTo(new Uri(WEBHOOK_URL)));
            Assert.That(DeliveredEventCodes(), Is.EquivalentTo(new[] { "REGISTER", "EXCEPTION" }));
        });
    }


    [Test]
    public async Task LogEvent_IgnoresItsOwnDeliveryFailureEntriesToAvoidFeedbackLoops()
    {
        severityFake.WithData(Severity(1, "W"), Severity(2, "I"));

        string handlerSource = nameof(ZapierTriggerHandler);
        string deliveryCode = ZapierTriggerHandler.EventLogDeliveryEventCode;

        RaiseLogEvent("W", deliveryCode, handlerSource, $"POST to {WEBHOOK_URL} failed: timeout"); // own URL: skipped
        RaiseLogEvent("I", deliveryCode, handlerSource, $"POST to {WEBHOOK_URL} failed with the following message:<br/> 410"); // own URL: skipped
        RaiseLogEvent("I", "REGISTER", handlerSource); // other code, own source: delivered
        await WaitForDeliveries(1);
        await Task.Delay(SETTLE_DELAY);

        Assert.That(DeliveredEventCodes(), Is.EqualTo(new[] { "REGISTER" }));
    }


    [Test]
    public async Task LogEvent_IgnoresDeliveryFailuresOfOtherEventLogTriggersToAvoidMutualFeedbackLoops()
    {
        severityFake.WithData(Severity(1, "W"), Severity(2, "I"));

        // Two failing event log triggers would otherwise forward each other's failures forever.
        RaiseLogEvent("W", ZapierTriggerHandler.EventLogDeliveryEventCode, nameof(ZapierTriggerHandler), "POST to https://hooks.example.test/other/ failed: timeout");
        RaiseLogEvent("I", ZapierTriggerHandler.EventLogDeliveryEventCode, nameof(ZapierTriggerHandler), "POST to https://hooks.example.test/other/ failed with the following message:<br/> 410");
        RaiseLogEvent("I", "REGISTER", nameof(ZapierTriggerHandler));
        await WaitForDeliveries(1);
        await Task.Delay(SETTLE_DELAY);

        Assert.That(DeliveredEventCodes(), Is.EqualTo(new[] { "REGISTER" }));
    }


    [Test]
    public async Task LogEvent_ForwardsDeliveryFailuresOfNonEventLogTriggers()
    {
        severityFake.WithData(Severity(1, "W"));

        // A form or content trigger failing cannot loop through the event log, so a Zap may alert on it.
        RaiseLogEvent("W", ZapierTriggerHandler.DeliveryEventCode, nameof(ZapierTriggerHandler), "POST to https://hooks.example.test/form/ failed: timeout");
        await WaitForDeliveries(1);

        Assert.That(DeliveredEventCodes(), Is.EqualTo(new[] { ZapierTriggerHandler.DeliveryEventCode }));
    }


    [Test]
    public async Task DeliveryFailure_IsLoggedWithTheEventLogDeliveryCode()
    {
        severityFake.WithData(Severity(1, "I"));
        var eventLogService = Substitute.For<IEventLogService>();
        var failingHttp = new StubHttpMessageHandler { ThrowOnSend = new HttpRequestException("unreachable") };
        var failingHandler = new EventLogObjectHandler(
            new ZapierTriggerInfo
            {
                ZapierTriggerID = TRIGGER_ID,
                ZapierTriggerCodeName = "evtlog11",
                ZapierTriggerObjectType = EventLogInfo.OBJECT_TYPE,
                ZapierTriggerZapierURL = WEBHOOK_URL,
            },
            eventLogService,
            new HttpClient(failingHttp),
            Service.Resolve<IProgressiveCache>(),
            severityFake.Provider());

        failingHandler.LogEventHandler(this, new LogEventArgs { Event = new EventLogInfo { EventType = "I", EventCode = "ANY", Source = "Test" } });
        var deadline = DateTime.UtcNow + DELIVERY_TIMEOUT;
        while (DateTime.UtcNow < deadline && eventLogService.ReceivedCalls().All(c => c.GetMethodInfo().Name != nameof(IEventLogService.LogEvent)))
        {
            await Task.Delay(20);
        }

        eventLogService.Received(1).LogEvent(Arg.Is<EventLogData>(d =>
            d.EventCode == ZapierTriggerHandler.EventLogDeliveryEventCode && d.Source == nameof(ZapierTriggerHandler)));
    }


    [Test]
    public async Task LogEvent_ForwardsPostEventCodeFromOtherSources()
    {
        severityFake.WithData(Severity(1, "W"));

        // Only entries logged by the integration itself are suppressed, not any "POST" entry.
        RaiseLogEvent("W", ZapierTriggerHandler.DeliveryEventCode, "SomeOtherModule", "POST failed");
        await WaitForDeliveries(1);

        Assert.That(DeliveredEventCodes(), Is.EqualTo(new[] { ZapierTriggerHandler.DeliveryEventCode }));
    }


    [Test]
    public async Task LogEvent_PicksUpSeveritiesAssignedAfterTheHandlerAlreadyRan()
    {
        // Production order: trigger row inserted -> handler registered -> "REGISTER" entry logged (caches an empty
        // severity list) -> severities inserted. The cache must be invalidated by the severity insert.
        RaiseLogEvent("I", "REGISTER");
        await Task.Delay(SETTLE_DELAY);
        Assert.That(http.Requests, Is.Empty, "no severities assigned yet");

        severityFake.IncludeData(Severity(1, "I"));
        CacheHelper.TouchKey($"{ZapierTriggerEventLogTypeInfo.OBJECT_TYPE}|all"); // what the real provider does on insert

        RaiseLogEvent("I", "LATER");
        await WaitForDeliveries(1);

        Assert.That(DeliveredEventCodes(), Is.EqualTo(new[] { "LATER" }));
    }


    private IEnumerable<string?> DeliveredEventCodes() =>
        http.Requests.Select(r => JsonDocument.Parse(r.Body).RootElement.GetProperty("EventCode").GetString());


    private static ZapierTriggerEventLogTypeInfo Severity(int id, string type) => new()
    {
        ZapierTriggerEventLogTypeID = id,
        ZapierTriggerEventLogTypeZapierTriggerID = TRIGGER_ID,
        ZapierTriggerEventLogTypeType = type,
    };


    private void RaiseLogEvent(string eventType, string eventCode, string source = nameof(EventLogObjectHandlerTests), string description = "")
    {
        var entry = new EventLogInfo
        {
            EventType = eventType,
            EventCode = eventCode,
            Source = source,
            EventDescription = description,
            EventTime = DateTime.Now,
        };
        handler.LogEventHandler(this, new LogEventArgs { Event = entry });
    }


    private async Task WaitForDeliveries(int expected)
    {
        var deadline = DateTime.UtcNow + DELIVERY_TIMEOUT;
        while (http.Requests.Count < expected)
        {
            if (DateTime.UtcNow >= deadline)
            {
                Assert.Fail($"Expected {expected} deliveries within {DELIVERY_TIMEOUT.TotalSeconds} s, got {http.Requests.Count}.");
            }
            await Task.Delay(20);
        }
    }
}
