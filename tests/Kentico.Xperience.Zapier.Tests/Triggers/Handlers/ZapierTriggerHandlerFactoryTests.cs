using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Tests;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers;
using Kentico.Xperience.Zapier.Triggers.Handlers;

using Microsoft.AspNetCore.Http;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Triggers.Handlers;

[TestFixture]
public class ZapierTriggerHandlerFactoryTests : UnitTests
{
    private const string REUSABLE_CLASS = "DancingGoat.Cafe";
    private const int REUSABLE_CLASS_ID = 20;

    private IWorkflowScopeService workflowScopeService = null!;
    private ZapierTriggerHandlerFactory factory = null!;


    [SetUp]
    public void SetUp()
    {
        Fake<ZapierTriggerInfo>();
        var dataClassFake = Fake<DataClassInfo, DataClassInfoProvider>();
        var cafe = DataClassInfo.New();
        cafe.ClassID = REUSABLE_CLASS_ID;
        cafe.ClassName = REUSABLE_CLASS;
        cafe.ClassDisplayName = "Cafe";
        cafe.ClassType = ClassType.CONTENT_TYPE;
        cafe.ClassContentTypeType = ClassContentTypeType.REUSABLE;
        dataClassFake.WithData(cafe);

        workflowScopeService = Substitute.For<IWorkflowScopeService>();
        factory = new ZapierTriggerHandlerFactory(
            new HttpClient(),
            workflowScopeService,
            Substitute.For<IInfoProvider<ContentLanguageInfo>>(),
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IInfoProvider<ZapierTriggerEventLogTypeInfo>>(),
            Substitute.For<IProgressiveCache>(),
            Substitute.For<IEventLogService>(),
            Substitute.For<IAdminLinkService>(),
            Substitute.For<IInfoProvider<ContentItemInfo>>());
    }


    [TestCase("Form", "BizForm.DancingGoatContactUs", "Create", typeof(BizFormHandler))]
    [TestCase("Form", "BizForm.DancingGoatContactUs", "Delete", typeof(BizFormHandler))]
    [TestCase("Other", "om.contact", "Update", typeof(InfoObjectHandler))]
    [TestCase("System", "cms.user", "Create", typeof(InfoObjectHandler))]
    [TestCase("Website", "DancingGoat.ArticlePage", "Publish", typeof(PageHandler))]
    [TestCase("Reusable", REUSABLE_CLASS, "Update", typeof(ReusableHandler))]
    [TestCase("Headless", "DancingGoat.HeadlessUser", "Create", typeof(HeadlessHandler))]
    public void CreateHandler_SelectsHandlerByObjectClassTypeForStandardEvents(string classType, string objectType, string eventType, Type expected)
    {
        var handler = factory.CreateHandler(NewTrigger(classType, objectType, eventType));

        Assert.That(handler, Is.TypeOf(expected));
    }


    [Test]
    public void CreateHandler_UsesEventLogHandlerForEventLogObjectTypeRegardlessOfClassType()
    {
        var handler = factory.CreateHandler(NewTrigger("Other", EventLogInfo.OBJECT_TYPE, "Create"));

        Assert.That(handler, Is.TypeOf<EventLogObjectHandler>());
    }


    [Test]
    public void CreateHandler_ReturnsNullForUnknownObjectClassType()
    {
        Assert.That(factory.CreateHandler(NewTrigger("NotAClassType", "cms.user", "Create")), Is.Null);
    }


    [Test]
    public void CreateHandler_ReturnsNullForCustomEventThatIsNotAWorkflowStepOfTheContentType()
    {
        workflowScopeService.IsMatchingWorflowEventPerObject("LegalReview", REUSABLE_CLASS_ID).Returns(false);

        Assert.That(factory.CreateHandler(NewTrigger("Reusable", REUSABLE_CLASS, "LegalReview")), Is.Null);
    }


    [Test]
    public void CreateHandler_ReturnsNullForCustomEventOnUnknownClass()
    {
        Assert.That(factory.CreateHandler(NewTrigger("Reusable", "Does.NotExist", "LegalReview")), Is.Null);
        workflowScopeService.DidNotReceiveWithAnyArgs().IsMatchingWorflowEventPerObject(default!, default);
    }


    [TestCase("Reusable", typeof(WorkflowReusableHandler))]
    [TestCase("Website", typeof(WorkflowPagesHandler))]
    [TestCase("Headless", typeof(WorkflowHeadlessHandler))]
    public void CreateHandler_SelectsWorkflowHandlerForMatchingWorkflowStep(string classType, Type expected)
    {
        workflowScopeService.IsMatchingWorflowEventPerObject("LegalReview", REUSABLE_CLASS_ID).Returns(true);

        var handler = factory.CreateHandler(NewTrigger(classType, REUSABLE_CLASS, "LegalReview"));

        Assert.That(handler, Is.TypeOf(expected));
    }


    private static ZapierTriggerInfo NewTrigger(string classType, string objectType, string eventType) => new()
    {
        ZapierTriggerID = 1,
        ZapierTriggerCodeName = "abc12345",
        ZapierTriggerObjectClassType = classType,
        ZapierTriggerObjectType = objectType,
        ZapierTriggerEventType = eventType,
        ZapierTriggerZapierURL = "https://hooks.zapier.com/hooks/standard/1/abc/",
    };
}
