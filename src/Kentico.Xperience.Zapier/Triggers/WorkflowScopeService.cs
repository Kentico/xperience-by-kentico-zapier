using CMS.ContentWorkflowEngine;
using CMS.DataEngine;

namespace Kentico.Xperience.Zapier.Triggers;

internal interface IWorkflowScopeService
{
    bool IsMatchingWorflowEventPerObject(string workflowEvent, int contentTypeID);
    IEnumerable<ContentWorkflowStepInfo> GetStepsByContentTypeID(int contentTypeID);
}


internal class WorkflowScopeService : IWorkflowScopeService
{
    private readonly IInfoProvider<ContentWorkflowContentTypeInfo> contentWorkflowContentTypeProvider;
    private readonly IInfoProvider<ContentWorkflowStepInfo> contentWorkflowStepProvider;


    public WorkflowScopeService(
        IInfoProvider<ContentWorkflowContentTypeInfo> contentWorkflowContentTypeProvider,
        IInfoProvider<ContentWorkflowStepInfo> contentWorkflowStepProvider)
    {
        this.contentWorkflowContentTypeProvider = contentWorkflowContentTypeProvider;
        this.contentWorkflowStepProvider = contentWorkflowStepProvider;
    }


    public bool IsMatchingWorflowEventPerObject(string workflowEvent, int contentTypeID)
    {
        var stepInfos = GetStepsByContentTypeID(contentTypeID);
        var matches = stepInfos.Where(s => s.ContentWorkflowStepName.Equals(workflowEvent, StringComparison.OrdinalIgnoreCase));
        return matches.Any();
    }


    public IEnumerable<ContentWorkflowStepInfo> GetStepsByContentTypeID(int contentTypeID)
    {
        var contentTypeWorkflows = contentWorkflowContentTypeProvider.Get()
                  .WhereEquals("ContentWorkflowContentTypeContentTypeID", contentTypeID)
                  .GetEnumerableTypedResult();

        return StepsByWorkflowIDs(contentTypeWorkflows.Select(x => x.ContentWorkflowContentTypeContentWorkflowID).ToList()) ?? [];
    }


    private IEnumerable<ContentWorkflowStepInfo> StepsByWorkflowIDs(List<int> workflowIds) =>
        contentWorkflowStepProvider.Get()
        .WhereIn("ContentWorkflowStepWorkflowID", workflowIds)
        .GetEnumerableTypedResult();
}

