using CMS.ContentWorkflowEngine;
using CMS.DataEngine;
using CMS.Tests;

using Kentico.Xperience.Zapier.Tests.Support;
using Kentico.Xperience.Zapier.Triggers;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Triggers;

[TestFixture]
public class WorkflowScopeServiceTests : UnitTests
{
    private const int CONTENT_TYPE_WITH_WORKFLOW = 10;
    private const int CONTENT_TYPE_WITHOUT_WORKFLOW = 11;

    private WorkflowScopeService service = null!;


    [SetUp]
    public void SetUp()
    {
        var contentTypeFake = Fake<ContentWorkflowContentTypeInfo, IInfoProvider<ContentWorkflowContentTypeInfo>>().WithData(
            new ContentWorkflowContentTypeInfo
            {
                ContentWorkflowContentTypeID = 1,
                ContentWorkflowContentTypeContentTypeID = CONTENT_TYPE_WITH_WORKFLOW,
                ContentWorkflowContentTypeContentWorkflowID = 100
            });

        var stepFake = Fake<ContentWorkflowStepInfo, IInfoProvider<ContentWorkflowStepInfo>>().WithData(
            new ContentWorkflowStepInfo
            {
                ContentWorkflowStepID = 1,
                ContentWorkflowStepName = "LegalReview",
                ContentWorkflowStepDisplayName = "Legal review",
                ContentWorkflowStepWorkflowID = 100
            },
            new ContentWorkflowStepInfo
            {
                ContentWorkflowStepID = 2,
                ContentWorkflowStepName = "Approved",
                ContentWorkflowStepDisplayName = "Approved",
                ContentWorkflowStepWorkflowID = 100
            },
            new ContentWorkflowStepInfo
            {
                ContentWorkflowStepID = 3,
                ContentWorkflowStepName = "OtherWorkflowStep",
                ContentWorkflowStepDisplayName = "Other",
                ContentWorkflowStepWorkflowID = 200
            });

        service = new WorkflowScopeService(contentTypeFake.Provider(), stepFake.Provider());
    }


    [Test]
    public void GetStepsByContentTypeID_ReturnsStepsOfWorkflowsAssignedToTheContentType()
    {
        var steps = service.GetStepsByContentTypeID(CONTENT_TYPE_WITH_WORKFLOW).Select(s => s.ContentWorkflowStepName);

        Assert.That(steps, Is.EquivalentTo(new[] { "LegalReview", "Approved" }));
    }


    [Test]
    public void GetStepsByContentTypeID_ReturnsEmptyForContentTypeWithoutWorkflow()
    {
        Assert.That(service.GetStepsByContentTypeID(CONTENT_TYPE_WITHOUT_WORKFLOW), Is.Empty);
    }


    [TestCase("LegalReview", CONTENT_TYPE_WITH_WORKFLOW, true)]
    [TestCase("legalreview", CONTENT_TYPE_WITH_WORKFLOW, true)]
    [TestCase("OtherWorkflowStep", CONTENT_TYPE_WITH_WORKFLOW, false)]
    [TestCase("LegalReview", CONTENT_TYPE_WITHOUT_WORKFLOW, false)]
    public void IsMatchingWorflowEventPerObject_MatchesStepNamesCaseInsensitivelyWithinScope(string step, int contentTypeId, bool expected)
    {
        Assert.That(service.IsMatchingWorflowEventPerObject(step, contentTypeId), Is.EqualTo(expected));
    }
}
