using System.ComponentModel;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Zapier.Admin.Providers;

namespace Kentico.Xperience.Zapier.Admin.UIPages;

internal class ZapierTriggerEditModel
{
    [RequiredValidationRule]
    [TextInputComponent(Label = "Name", Order = 0, Disabled = true)]
    public string? Name { get; set; }

    [CheckBoxComponent(Label = "Enabled", Order = 1, Disabled = true)]
    public bool Enabled { get; set; }

    [RequiredValidationRule]
    [ObjectSelectorComponent("cms.class", Label = "Object type", Disabled = true, MaximumItems = 1, Placeholder = "Choose an option", Order = 2, WhereConditionProviderType = typeof(ZapierTriggerObjectTypesWhereConditionProvider))]
    public IEnumerable<ObjectRelatedItem>? ObjectType { get; set; }

    [RequiredValidationRule]
    [DropDownComponent(Label = "Event", Order = 3, Disabled = true, DataProviderType = typeof(ZapierTriggerEventTypesDropdownOptionsProvider))]
    public string? EventType { get; set; }


    [TextInputComponent(Label = "Zapier Url", Order = 4, Disabled = true)]
    public string? ZapierURL { get; set; }



    public void MapToZapierTriggerInfoObject(ZapierTriggerInfo infoObject)
    {
        string objectType = ObjectType?.FirstOrDefault()?.ObjectCodeName ?? string.Empty;

        infoObject.ZapierTriggerObjectClassType = ZapierTriggerExtensions.GetType(objectType);
        infoObject.ZapierTriggerDisplayName = Name;
        infoObject.ZapierTriggerCodeName = ValidationHelper.GetCodeName(Name);
        infoObject.ZapierTriggerEnabled = Enabled;
        infoObject.ZapierTriggerEventType = EventType;
        infoObject.mZapierTriggerObjectType = objectType;
        infoObject.ZapierTriggerZapierURL = ZapierURL;
    }

}


internal static class ZapierTriggerExtensions
{
    public const string FormComponentOptions = $"{nameof(ZapierTriggerEvents.Create)}\r\n" +
                                               $"{nameof(ZapierTriggerEvents.Update)}\r\n" +
                                               $"{nameof(ZapierTriggerEvents.Delete)}\r\n";


    public static string GetType(string objectType)
    {
        var dataInfo = DataClassInfoProvider.ProviderObject.Get(objectType);

        string? type = dataInfo?.ClassType ?? string.Empty;
        string? contentType = dataInfo?.ClassContentTypeType;

        return !string.IsNullOrEmpty(contentType) ? contentType : type;
    }



    public static bool IsForm(this ZapierTriggerInfo info) => Enum.TryParse(info.ZapierTriggerObjectClassType, out ZapierTriggerObjectClassType type) && type == ZapierTriggerObjectClassType.Form;

}

public enum ZapierTriggerObjectClassType
{
    Form,
    Other,
    System,
    Website,
    Reusable,
    Email,
}
