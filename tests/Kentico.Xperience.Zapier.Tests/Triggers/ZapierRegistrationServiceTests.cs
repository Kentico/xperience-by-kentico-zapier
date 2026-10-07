using CMS.Core;
using CMS.Tests;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Tests.Support;
using Kentico.Xperience.Zapier.Triggers;
using Kentico.Xperience.Zapier.Triggers.Handlers;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Triggers;

[TestFixture]
public class ZapierRegistrationServiceTests : UnitTests
{
    private IEventLogService eventLogService = null!;
    private IZapierTriggerHandlerFactory handlerFactory = null!;
    private ZapierRegistrationService service = null!;


    [SetUp]
    public void SetUp()
    {
        Fake<ZapierTriggerInfo>();
        eventLogService = Substitute.For<IEventLogService>();
        handlerFactory = Substitute.For<IZapierTriggerHandlerFactory>();
        service = new ZapierRegistrationService(eventLogService, handlerFactory);
    }


    [Test]
    public void RegisterWebhook_RegistersHandlerCreatedByFactory()
    {
        var trigger = new ZapierTriggerInfo { ZapierTriggerID = 1 };
        var handler = new RecordingTriggerHandler(trigger);
        handlerFactory.CreateHandler(trigger).Returns(handler);

        service.RegisterWebhook(trigger);

        Assert.Multiple(() =>
        {
            Assert.That(handler.RegisterCalls, Is.EqualTo(1));
            Assert.That(handler.UnregisterCalls, Is.Zero);
        });
        eventLogService.DidNotReceive().LogEvent(Arg.Any<EventLogData>());
    }


    [Test]
    public void RegisterWebhook_LogsErrorWhenNoHandlerMatches()
    {
        var trigger = new ZapierTriggerInfo { ZapierTriggerID = 2 };
        handlerFactory.CreateHandler(trigger).Returns((ZapierTriggerHandler?)null);

        service.RegisterWebhook(trigger);

        eventLogService.Received(1).LogEvent(Arg.Is<EventLogData>(e =>
            e.EventType == EventTypeEnum.Error && e.EventCode == "REGISTER" && e.EventDescription.Contains("2")));
    }


    [Test]
    public void RegisterWebhook_RejectsDuplicateTriggerIds()
    {
        var trigger = new ZapierTriggerInfo { ZapierTriggerID = 3 };
        var first = new RecordingTriggerHandler(trigger);
        var second = new RecordingTriggerHandler(trigger);
        handlerFactory.CreateHandler(trigger).Returns(first, second);

        service.RegisterWebhook(trigger);
        service.RegisterWebhook(trigger);

        Assert.Multiple(() =>
        {
            Assert.That(first.RegisterCalls, Is.EqualTo(1));
            Assert.That(second.RegisterCalls, Is.Zero);
        });
        eventLogService.Received(1).LogEvent(Arg.Is<EventLogData>(e => e.EventType == EventTypeEnum.Error && e.EventCode == "REGISTER"));
    }


    [Test]
    public void UnregisterWebhook_UnregistersPreviouslyRegisteredHandler()
    {
        var trigger = new ZapierTriggerInfo { ZapierTriggerID = 4 };
        var handler = new RecordingTriggerHandler(trigger);
        handlerFactory.CreateHandler(trigger).Returns(handler);
        service.RegisterWebhook(trigger);

        service.UnregisterWebhook(trigger);
        service.UnregisterWebhook(trigger);

        Assert.That(handler.UnregisterCalls, Is.EqualTo(1));
        eventLogService.Received(1).LogEvent(Arg.Is<EventLogData>(e => e.EventType == EventTypeEnum.Error && e.EventCode == "UNREGISTER"));
    }
}
