using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Tests;
using CMS.Websites.Routing;

using Kentico.Xperience.Zapier.Tests.Support;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Triggers;

[TestFixture]
public class ZapierTriggerServiceTests : UnitTests
{
    private const string FORM_CLASS = "BizForm.DancingGoatContactUs";
    private const string REUSABLE_CLASS = "DancingGoat.Cafe";
    private const string EMAIL_CLASS = "DancingGoat.Email";

    private IInfoProviderFake<ZapierTriggerInfo, IInfoProvider<ZapierTriggerInfo>> triggerFake = null!;
    private IInfoProviderFake<ZapierTriggerEventLogTypeInfo, IInfoProvider<ZapierTriggerEventLogTypeInfo>> eventLogTypeFake = null!;
    private IWorkflowScopeService workflowScopeService = null!;


    [SetUp]
    public void SetUp()
    {
        Fake<DataClassInfo, DataClassInfoProvider>().WithData(
            NewDataClass(1, FORM_CLASS, ClassType.FORM, string.Empty),
            NewDataClass(2, REUSABLE_CLASS, ClassType.CONTENT_TYPE, ClassContentTypeType.REUSABLE),
            NewDataClass(3, EMAIL_CLASS, ClassType.CONTENT_TYPE, ClassContentTypeType.EMAIL));

        triggerFake = Fake<ZapierTriggerInfo, IInfoProvider<ZapierTriggerInfo>>();
        triggerFake.WithData();
        eventLogTypeFake = Fake<ZapierTriggerEventLogTypeInfo, IInfoProvider<ZapierTriggerEventLogTypeInfo>>();
        eventLogTypeFake.WithData();
        workflowScopeService = Substitute.For<IWorkflowScopeService>();
    }


    [Test]
    public void CreateTrigger_StoresFormTriggerWithResolvedClassType()
    {
        var service = CreateService();

        int id = service.CreateTrigger(FORM_CLASS, "Create", "https://hooks.zapier.com/1");

        var stored = triggerFake.Provider().Get(id);
        Assert.Multiple(() =>
        {
            Assert.That(stored, Is.Not.Null);
            Assert.That(stored!.ZapierTriggerObjectType, Is.EqualTo(FORM_CLASS));
            Assert.That(stored.ZapierTriggerObjectClassType, Is.EqualTo("Form"));
            Assert.That(stored.ZapierTriggerEventType, Is.EqualTo("Create"));
            Assert.That(stored.ZapierTriggerZapierURL, Is.EqualTo("https://hooks.zapier.com/1"));
            Assert.That(stored.ZapierTriggerCodeName, Has.Length.EqualTo(8));
        });
    }


    [Test]
    public void CreateTrigger_UsesContentTypeTypeForContentClasses()
    {
        var service = CreateService();

        int id = service.CreateTrigger(REUSABLE_CLASS, "LegalReview", "https://hooks.zapier.com/2");

        Assert.That(triggerFake.Provider().Get(id)?.ZapierTriggerObjectClassType, Is.EqualTo(ClassContentTypeType.REUSABLE));
    }


    [Test]
    public void CreateTrigger_GeneratesUniqueCodeNames()
    {
        var service = CreateService();

        int first = service.CreateTrigger(FORM_CLASS, "Create", "https://hooks.zapier.com/1");
        int second = service.CreateTrigger(FORM_CLASS, "Create", "https://hooks.zapier.com/1");

        Assert.That(triggerFake.Provider().Get(first)?.ZapierTriggerCodeName,
            Is.Not.EqualTo(triggerFake.Provider().Get(second)?.ZapierTriggerCodeName));
    }


    [Test]
    public void AssignEventSeverities_StoresOneRowPerSeverity()
    {
        var service = CreateService();

        service.AssignEventSeverities(42, ["I", "W", "E"]);

        var rows = eventLogTypeFake.Provider().Get().ToList();
        Assert.Multiple(() =>
        {
            Assert.That(rows, Has.Count.EqualTo(3));
            Assert.That(rows.Select(r => r.ZapierTriggerEventLogTypeZapierTriggerID), Is.All.EqualTo(42));
            Assert.That(rows.Select(r => r.ZapierTriggerEventLogTypeType), Is.EquivalentTo(new[] { "I", "W", "E" }));
        });
    }


    [Test]
    public void DeleteTrigger_RemovesTriggerAndItsSeverities()
    {
        var triggerProvider = Substitute.For<IInfoProvider<ZapierTriggerInfo>>();
        // BulkDelete is an extension that requires the provider to implement IBulkInfoProvider<T>.
        var eventLogTypeProvider = Substitute.For<IInfoProvider<ZapierTriggerEventLogTypeInfo>, IBulkInfoProvider<ZapierTriggerEventLogTypeInfo>>();
        var bulkProvider = (IBulkInfoProvider<ZapierTriggerEventLogTypeInfo>)eventLogTypeProvider;
        var service = CreateService(triggerProvider, eventLogTypeProvider);
        var info = new ZapierTriggerInfo { ZapierTriggerID = 7 };

        service.DeleteTrigger(info);

        bulkProvider.Received(1).BulkDelete(Arg.Is<IWhereCondition>(w => w.Parameters.Any(p => Equals(p.Value, 7))), Arg.Any<BulkDeleteSettings>());
        triggerProvider.Received(1).Delete(info);
    }


    [Test]
    public async Task GetFallbackDataAsync_ReturnsNullForEmailContentTypes()
    {
        var service = CreateService();

        var result = await service.GetFallbackDataAsync(EMAIL_CLASS, "Create");

        Assert.That(result, Is.Null);
    }


    [Test]
    public async Task GetFallbackDataAsync_ReturnsNullForUnknownWorkflowStep()
    {
        workflowScopeService.IsMatchingWorflowEventPerObject("NotAStep", 2).Returns(false);
        var service = CreateService();

        var result = await service.GetFallbackDataAsync(REUSABLE_CLASS, "NotAStep");

        Assert.That(result, Is.Null);
    }


    [Test]
    public async Task GetFallbackDataAsync_ReturnsWorkflowSampleForMatchingStep()
    {
        workflowScopeService.IsMatchingWorflowEventPerObject("LegalReview", 2).Returns(true);
        var service = CreateService();

        var result = await service.GetFallbackDataAsync(REUSABLE_CLASS, "LegalReview");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Keys, Is.SupersetOf(new[] { "DisplayName", "ContentTypeName", "StepName", "OriginalStepName", "UserID", "UserName", "AdminLink", "DateTime" }));
    }


    private ZapierTriggerService CreateService(
        IInfoProvider<ZapierTriggerInfo>? triggerProvider = null,
        IInfoProvider<ZapierTriggerEventLogTypeInfo>? eventLogTypeProvider = null) =>
        new(
            Substitute.For<IWebsiteChannelContext>(),
            workflowScopeService,
            Substitute.For<IContentQueryExecutor>(),
            Substitute.For<IContentQueryResultMapper>(),
            Substitute.For<IContentQueryModelTypeMapper>(),
            eventLogTypeProvider ?? eventLogTypeFake.Provider(),
            triggerProvider ?? triggerFake.Provider());


    private static DataClassInfo NewDataClass(int id, string className, string classType, string contentTypeType)
    {
        var dataClass = DataClassInfo.New();
        dataClass.ClassID = id;
        dataClass.ClassName = className;
        dataClass.ClassDisplayName = className;
        dataClass.ClassType = classType;
        dataClass.ClassContentTypeType = contentTypeType;
        return dataClass;
    }
}
