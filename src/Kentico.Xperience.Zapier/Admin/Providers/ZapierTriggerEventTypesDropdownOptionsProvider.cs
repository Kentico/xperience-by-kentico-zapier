using CMS.DataEngine;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace Kentico.Xperience.Zapier.Admin.Providers;

internal class ZapierTriggerEventTypesDropdownOptionsProvider : IDropDownOptionsProvider
{
    public Task<IEnumerable<DropDownOptionItem>> GetOptionItems() => Task.FromResult<IEnumerable<DropDownOptionItem>>(
[
    new DropDownOptionItem
    {
        Value = nameof(ZapierTriggerEvents.Create),
        Text = "Create"
    },
    new DropDownOptionItem
    {
        Value = nameof(ZapierTriggerEvents.Update),
        Text = "Update"
    },
    new DropDownOptionItem
    {
        Value = nameof(ZapierTriggerEvents.Delete),
        Text = "Delete"
    }
]);
}



internal class ZapierTriggerObjectTypesDropdownOptionsProvider : IDropDownOptionsProvider
{
    public Task<IEnumerable<DropDownOptionItem>> GetOptionItems() => Task.FromResult(ObjectTypeManager.MainObjectTypes.Select(x => new DropDownOptionItem() { Value = x, Text = x }));
}


internal class ZapierTriggerObjectTypesWhereConditionProvider : IObjectSelectorWhereConditionProvider
{
    public WhereCondition Get() => new WhereCondition().WhereNull(nameof(DataClassInfo.ClassContentTypeType));
}
