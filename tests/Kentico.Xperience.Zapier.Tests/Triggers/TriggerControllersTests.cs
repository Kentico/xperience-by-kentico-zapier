using CMS.DataEngine;
using CMS.EventLog;
using CMS.Tests;

using Kentico.Xperience.Zapier.Tests.Support;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Common;
using Kentico.Xperience.Zapier.Triggers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Triggers;

/// <summary>
/// Contract of the endpoints the Zapier CLI app calls for subscribe / unsubscribe / performList.
/// </summary>
[TestFixture]
public class TriggerControllersTests : UnitTests
{
    private const string ZAP_URL = "https://hooks.zapier.com/hooks/standard/1/abc/";

    private IZapierTriggerService triggerService = null!;


    [SetUp]
    public void SetUp()
    {
        triggerService = Substitute.For<IZapierTriggerService>();
        triggerService.CreateTrigger(default!, default!, default!).ReturnsForAnyArgs(42);
    }


    [Test]
    public void FormSubmission_CreateTrigger_RegistersCreateEventForTheForm()
    {
        var controller = new FormSubmissionTriggerController(triggerService);

        var result = controller.CreateTrigger(new FormSubmissionDto("BizForm.DancingGoatContactUs", ZAP_URL));

        triggerService.Received(1).CreateTrigger("BizForm.DancingGoatContactUs", "Create", ZAP_URL);
        Assert.That(OkValue<CreateTriggerResponse>(result), Is.EqualTo(new CreateTriggerResponse(42)));
    }


    [Test]
    public async Task FormSubmission_GetFallbackData_ReturnsOkWithDataOrBadRequest()
    {
        var controller = new FormSubmissionTriggerController(triggerService);
        triggerService.GetFallbackDataAsync("BizForm.A", "Create").Returns(new Dictionary<string, object> { ["UserEmail"] = "a@b.c" });
        triggerService.GetFallbackDataAsync("BizForm.B", "Create").Returns((IDictionary<string, object>?)null);

        var ok = await controller.GetFallbackDataAsync("BizForm.A");
        var bad = await controller.GetFallbackDataAsync("BizForm.B");

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.TypeOf<OkObjectResult>());
            Assert.That(((OkObjectResult)ok).Value, Is.EquivalentTo(new Dictionary<string, object> { ["UserEmail"] = "a@b.c" }));
            Assert.That(bad, Is.TypeOf<BadRequestResult>());
        });
    }


    [Test]
    public void EventLog_CreateTrigger_RegistersEventLogCreateAndDistinctSeverities()
    {
        var controller = new EventLogTriggerController(triggerService);

        var result = controller.CreateTrigger(new EventLogTriggerDto(ZAP_URL, ["I", "W", "I"]));

        Received.InOrder(() =>
        {
            triggerService.CreateTrigger(EventLogInfo.OBJECT_TYPE, "Create", ZAP_URL);
            triggerService.AssignEventSeverities(42, Arg.Is<IEnumerable<string>>(s => s.SequenceEqual(new[] { "I", "W" })));
        });
        Assert.That(OkValue<CreateTriggerResponse>(result), Is.EqualTo(new CreateTriggerResponse(42)));
    }


    [Test]
    public async Task EventLog_GetFallbackData_UsesEventLogObjectType()
    {
        var controller = new EventLogTriggerController(triggerService);
        triggerService.GetFallbackDataAsync(EventLogInfo.OBJECT_TYPE, "Create").Returns(new Dictionary<string, object> { ["EventType"] = "I" });

        var result = await controller.GetFallbackDataAsync();

        await triggerService.Received(1).GetFallbackDataAsync(EventLogInfo.OBJECT_TYPE, "Create");
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }


    [Test]
    public void EventLog_GetEventLogSeverities_ReturnsSingleLetterIdsUsedBySubscribe()
    {
        var controller = new EventLogTriggerController(triggerService);

        var options = OkValue<IEnumerable<SelectOptionItem>>(controller.GetEventLogSeverities()).ToList();

        Assert.That(options, Is.EquivalentTo(new[]
        {
            new SelectOptionItem("I", "Information"),
            new SelectOptionItem("W", "Warning"),
            new SelectOptionItem("E", "Error"),
        }));
    }


    [Test]
    public void MoveToStep_CreateTrigger_PassesWorkflowStepAsEventType()
    {
        var controller = new MoveToStepTriggerController(triggerService, Options(["DancingGoat.Cafe"]));

        var result = controller.CreateTrigger(new MoveToStepTriggerDto("DancingGoat.Cafe", "LegalReview", ZAP_URL));

        triggerService.Received(1).CreateTrigger("DancingGoat.Cafe", "LegalReview", ZAP_URL);
        Assert.That(OkValue<CreateTriggerResponse>(result), Is.EqualTo(new CreateTriggerResponse(42)));
    }


    [Test]
    public async Task MoveToStep_GetFallbackData_ReturnsBadRequestForUnknownStep()
    {
        var controller = new MoveToStepTriggerController(triggerService, Options(["DancingGoat.Cafe"]));
        triggerService.GetFallbackDataAsync("DancingGoat.Cafe", "Nope").Returns((IDictionary<string, object>?)null);

        var result = await controller.GetFallbackDataAsync("DancingGoat.Cafe", "Nope");

        Assert.That(result, Is.TypeOf<BadRequestResult>());
    }


    [Test]
    public void MoveToStep_GetWorkflowSteps_ReturnsEmptyForClassOutsideAllowedObjects()
    {
        var controller = new MoveToStepTriggerController(triggerService, Options(["DancingGoat.Cafe"]));

        var result = controller.GetWorkflowSteps("DancingGoat.Coffee");

        Assert.That(result.Value, Is.Empty);
    }


    [Test]
    public void ZapierTrigger_DeleteTrigger_DeletesExistingTriggerThroughService()
    {
        var fake = Fake<ZapierTriggerInfo, IInfoProvider<ZapierTriggerInfo>>().WithData(
            new ZapierTriggerInfo { ZapierTriggerID = 5, ZapierTriggerCodeName = "aaaaaaaa", ZapierTriggerObjectType = "BizForm.A", ZapierTriggerEventType = "Create" });
        var controller = new ZapierTriggerController(fake.Provider(), triggerService);

        var result = controller.DeleteTrigger(5);

        triggerService.Received(1).DeleteTrigger(Arg.Is<ZapierTriggerInfo>(i => i.ZapierTriggerID == 5));
        Assert.That(OkValue<DeleteTriggerResponse>(result).Status, Is.EqualTo("Success"));
    }


    [Test]
    public void ZapierTrigger_DeleteTrigger_IsIdempotentForUnknownId()
    {
        var fake = Fake<ZapierTriggerInfo, IInfoProvider<ZapierTriggerInfo>>().WithData();
        var controller = new ZapierTriggerController(fake.Provider(), triggerService);

        var result = controller.DeleteTrigger(999);

        triggerService.DidNotReceiveWithAnyArgs().DeleteTrigger(default!);
        Assert.That(OkValue<DeleteTriggerResponse>(result).Status, Does.Contain("999"));
    }


    private static IOptionsMonitor<ZapierConfiguration> Options(IEnumerable<string> allowedObjects)
    {
        var monitor = Substitute.For<IOptionsMonitor<ZapierConfiguration>>();
        monitor.CurrentValue.Returns(new ZapierConfiguration { AllowedObjects = [.. allowedObjects] });
        return monitor;
    }


    private static T OkValue<T>(ActionResult<T> result)
    {
        if (result.Result is OkObjectResult ok)
        {
            return (T)ok.Value!;
        }

        return result.Value!;
    }
}
