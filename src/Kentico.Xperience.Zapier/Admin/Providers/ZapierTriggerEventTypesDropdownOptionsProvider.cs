using CMS.DataEngine;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace Kentico.Xperience.Zapier.Admin.Providers;

internal class ZapierTriggerEventTypesDropdownOptionsProvider : IDropDownOptionsProvider
{
    private readonly IWorkflowScopeService workflowScopeService;

    public ZapierTriggerEventTypesDropdownOptionsProvider(IWorkflowScopeService workflowScopeService) => this.workflowScopeService = workflowScopeService;

    public Task<IEnumerable<DropDownOptionItem>> GetOptionItems()
    {
        var dropDownOptions = new List<DropDownOptionItem>()
        {
            new () { Value = nameof(ZapierTriggerEvents.Create), Text = "Create" },
            new () { Value = nameof(ZapierTriggerEvents.Update), Text = "Update" },
            new () { Value = nameof(ZapierTriggerEvents.Delete), Text = "Delete" }
        };

        var steps = workflowScopeService.GetAllSteps();

        var items = steps.Select(s => new DropDownOptionItem
        {
            Value = s.ContentWorkflowStepName,
            Text = $"{s.ContentWorkflowStepDisplayName} - workflow step"
        });

        dropDownOptions.AddRange(items);

        return Task.FromResult<IEnumerable<DropDownOptionItem>>(dropDownOptions);
    }
}



internal class ZapierTriggerObjectTypesDropdownOptionsProvider : IDropDownOptionsProvider
{
    public Task<IEnumerable<DropDownOptionItem>> GetOptionItems() => Task.FromResult(ObjectTypeManager.MainObjectTypes.Select(x => new DropDownOptionItem() { Value = x, Text = x }));
}


internal class ZapierTriggerObjectTypesWhereConditionProvider : IObjectSelectorWhereConditionProvider
{
    public WhereCondition Get() => new WhereCondition().Where(w =>
               w.WhereNull(nameof(DataClassInfo.ClassContentTypeType))
                   .Or()
                   .WhereEquals(nameof(DataClassInfo.ClassContentTypeType), ClassContentTypeType.REUSABLE)
                   .Or()
                   .WhereEquals(nameof(DataClassInfo.ClassContentTypeType), ClassContentTypeType.WEBSITE));
}
