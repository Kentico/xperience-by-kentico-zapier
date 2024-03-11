using CMS.ContentWorkflowEngine;
using CMS.DataEngine;

namespace Kentico.Xperience.Zapier;

public interface IWorkflowScopeService
{
    List<EventItemDto> GetEventItemsByContentType(int contentTypeID);
    bool IsMatchingWorflowEventPerObject(string workflowEvent, int contentTypeID);
}


public class WorkflowScopeService : IWorkflowScopeService
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

    public List<EventItemDto> GetEventItemsByContentType(int contentTypeID)
    {
        var stepInfos = GetStepsByContentTypeID(contentTypeID);

        var customSteps = stepInfos.Where(step => step.ContentWorkflowStepType == ContentWorkflowStepType.Custom).ToList();

        return customSteps.Select(s => new EventItemDto(s.ContentWorkflowStepName, $"{s.ContentWorkflowStepDisplayName} - (Workflow step)")).ToList();
    }



    public bool IsMatchingWorflowEventPerObject(string workflowEvent, int contentTypeID)
    {
        var stepInfos = GetStepsByContentTypeID(contentTypeID);

        var matches = stepInfos.Where(s => s.ContentWorkflowStepName == workflowEvent).ToList();

        return matches.Any();

    }


    private IEnumerable<ContentWorkflowStepInfo> GetStepsByContentTypeID(int contentTypeID)
    {
        var contentTypeWorkflows = contentWorkflowContentTypeProvider.Get()
                  .WhereEquals("ContentWorkflowContentTypeContentTypeID", contentTypeID)
                  .GetEnumerableTypedResult();

        var steps = contentWorkflowStepProvider.Get()
            .WhereIn("ContentWorkflowStepWorkflowID", contentTypeWorkflows.Select(x => x.ContentWorkflowContentTypeContentWorkflowID).ToList())
            .GetEnumerableTypedResult();

        return steps ?? [];
    }
}

