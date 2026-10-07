using System.Net;
using System.Text.Json;

using CMS.Core;
using CMS.Tests;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Tests.Support;
using Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Triggers.Handlers;

/// <summary>
/// Covers the webhook delivery shared by all trigger handlers (<c>ZapierTriggerHandler.DoPost</c>).
/// </summary>
[TestFixture]
public class ZapierTriggerHandlerTests : UnitTests
{
    private const string WEBHOOK_URL = "https://hooks.zapier.com/hooks/standard/1/abc/";

    private IEventLogService eventLogService = null!;
    private ZapierTriggerInfo trigger = null!;


    [SetUp]
    public void SetUp()
    {
        Fake<ZapierTriggerInfo>();
        eventLogService = Substitute.For<IEventLogService>();
        trigger = new ZapierTriggerInfo { ZapierTriggerID = 1, ZapierTriggerZapierURL = WEBHOOK_URL };
    }


    [Test]
    public async Task DoPost_PostsPayloadAsJsonToTheWebhookUrl()
    {
        var http = new StubHttpMessageHandler();
        var handler = new RecordingTriggerHandler(trigger, eventLogService, new HttpClient(http));
        var payload = new Dictionary<string, object>
        {
            ["UserFirstName"] = "Ada",
            ["FormInserted"] = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            ["Subscribe"] = true,
        };

        await handler.PostAsync(WEBHOOK_URL, payload);

        Assert.That(http.Requests, Has.Count.EqualTo(1));
        var (request, body) = http.Requests[0];
        var json = JsonDocument.Parse(body).RootElement;
        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(request.RequestUri, Is.EqualTo(new Uri(WEBHOOK_URL)));
            Assert.That(request.Content?.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
            Assert.That(json.GetProperty("UserFirstName").GetString(), Is.EqualTo("Ada"));
            Assert.That(json.GetProperty("Subscribe").GetBoolean(), Is.True);
            Assert.That(json.GetProperty("FormInserted").GetDateTime(), Is.EqualTo(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc)));
        });
        eventLogService.DidNotReceive().LogEvent(Arg.Any<EventLogData>());
    }


    [Test]
    public async Task DoPost_SkipsRequestWhenUrlIsEmpty()
    {
        var http = new StubHttpMessageHandler();
        var handler = new RecordingTriggerHandler(trigger, eventLogService, new HttpClient(http));

        await handler.PostAsync(string.Empty, new Dictionary<string, object> { ["a"] = 1 });

        Assert.That(http.Requests, Is.Empty);
    }


    [TestCase(typeof(HttpRequestException))]
    [TestCase(typeof(TaskCanceledException))]
    [TestCase(typeof(InvalidOperationException))]
    public async Task DoPost_LogsWarningInsteadOfThrowingWhenTheWebhookIsUnreachable(Type exceptionType)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "delivery failed")!;
        var http = new StubHttpMessageHandler { ThrowOnSend = exception };
        var handler = new RecordingTriggerHandler(trigger, eventLogService, new HttpClient(http));

        await handler.PostAsync(WEBHOOK_URL, new Dictionary<string, object> { ["a"] = 1 });

        eventLogService.Received(1).LogEvent(Arg.Is<EventLogData>(e =>
            e.EventType == EventTypeEnum.Warning && e.EventCode == ZapierTriggerHandler.DeliveryEventCode && e.EventDescription.Contains("delivery failed")));
    }


    [Test]
    public async Task DoPost_LogsInnerExceptionCause()
    {
        var exception = new HttpRequestException("An error occurred while sending the request.", new IOException("No such host is known."));
        var http = new StubHttpMessageHandler { ThrowOnSend = exception };
        var handler = new RecordingTriggerHandler(trigger, eventLogService, new HttpClient(http));

        await handler.PostAsync(WEBHOOK_URL, new Dictionary<string, object> { ["a"] = 1 });

        eventLogService.Received(1).LogEvent(Arg.Is<EventLogData>(e =>
            e.EventDescription.Contains("HttpRequestException") && e.EventDescription.Contains("IOException: No such host is known.")));
    }


    [Test]
    public void DoPost_LogsWarningForInvalidUrl()
    {
        var http = new StubHttpMessageHandler();
        var handler = new RecordingTriggerHandler(trigger, eventLogService, new HttpClient(http));

        Assert.That(() => handler.PostAsync("not a url", new Dictionary<string, object> { ["a"] = 1 }), Throws.Nothing);
        Assert.That(http.Requests, Is.Empty);
        eventLogService.Received(1).LogEvent(Arg.Is<EventLogData>(e => e.EventType == EventTypeEnum.Warning && e.EventCode == ZapierTriggerHandler.DeliveryEventCode));
    }


    [Test]
    public async Task DoPost_LogsWarningWhenThePayloadCannotBeSerialized()
    {
        var http = new StubHttpMessageHandler();
        var handler = new RecordingTriggerHandler(trigger, eventLogService, new HttpClient(http));

        // System.Text.Json refuses System.Type values (NotSupportedException) while the request content is written.
        await handler.PostAsync(WEBHOOK_URL, new Dictionary<string, object> { ["type"] = typeof(string) });

        eventLogService.Received(1).LogEvent(Arg.Is<EventLogData>(e =>
            e.EventType == EventTypeEnum.Warning && e.EventCode == ZapierTriggerHandler.DeliveryEventCode));
    }


    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.Gone)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task DoPost_LogsNonSuccessResponsesToEventLog(HttpStatusCode status)
    {
        var http = new StubHttpMessageHandler(status, "zap is paused");
        var handler = new RecordingTriggerHandler(trigger, eventLogService, new HttpClient(http));

        await handler.PostAsync(WEBHOOK_URL, new Dictionary<string, object> { ["a"] = 1 });

        eventLogService.Received(1).LogEvent(Arg.Is<EventLogData>(e =>
            e.EventType == EventTypeEnum.Warning && e.EventCode == "POST" && e.EventDescription.Contains(WEBHOOK_URL) && e.EventDescription.Contains("zap is paused")));
    }


    [TestCase(HttpStatusCode.Accepted)]
    [TestCase(HttpStatusCode.NoContent)]
    public async Task DoPost_TreatsAnySuccessStatusAsDelivered(HttpStatusCode status)
    {
        var http = new StubHttpMessageHandler(status);
        var handler = new RecordingTriggerHandler(trigger, eventLogService, new HttpClient(http));

        await handler.PostAsync(WEBHOOK_URL, new Dictionary<string, object> { ["a"] = 1 });

        eventLogService.DidNotReceive().LogEvent(Arg.Any<EventLogData>());
    }


    [Test]
    public void DoPost_DoesNotThrowWithoutAnEventLogService()
    {
        var http = new StubHttpMessageHandler { ThrowOnSend = new HttpRequestException("unreachable") };
        var handler = new RecordingTriggerHandler(trigger, eventLogService: null, new HttpClient(http));

        Assert.That(() => handler.PostAsync(WEBHOOK_URL, new Dictionary<string, object> { ["a"] = 1 }), Throws.Nothing);
    }
}
