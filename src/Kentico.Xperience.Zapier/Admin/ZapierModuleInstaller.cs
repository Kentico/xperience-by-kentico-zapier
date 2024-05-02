
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Zapier.Admin.UIPages;

using static Kentico.Xperience.Zapier.Admin.ZapierConstants;

namespace Kentico.Xperience.Zapier.Admin;

internal interface IZapierModuleInstaller
{
    void Install();
}


internal class ZapierModuleInstaller : IZapierModuleInstaller
{
    private readonly IInfoProvider<ResourceInfo> resourceInfoProvider;


    public ZapierModuleInstaller(IInfoProvider<ResourceInfo> resourceInfoProvider) => this.resourceInfoProvider = resourceInfoProvider;


    public void Install()
    {
        var resourceInfo = InstallModule();
        InstallApiKeyInfo(resourceInfo);
        InstallZapierTriggerInfo(resourceInfo);
        InstallZapierTriggerEventLogSeverity(resourceInfo);
    }


    private ResourceInfo InstallModule()
    {
        var resourceInfo = resourceInfoProvider.Get(ZapierResourceConstants.ResourceName)
            // Handle v1.0.0 resource name
            ?? resourceInfoProvider.Get("Kentico.Xperience.Zapier")
            ?? new ResourceInfo();

        resourceInfo.ResourceDisplayName = ZapierResourceConstants.ResourceDisplayName;
        resourceInfo.ResourceName = ZapierResourceConstants.ResourceName;
        resourceInfo.ResourceDescription = ZapierResourceConstants.ResourceDescription;
        resourceInfo.ResourceIsInDevelopment = ZapierResourceConstants.ResourceIsInDevelopment;
        if (resourceInfo.HasChanged)
        {
            resourceInfoProvider.Set(resourceInfo);
        }
        return resourceInfo;
    }


    private static void InstallZapierTriggerEventLogSeverity(ResourceInfo resourceInfo)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(ZapierTriggerEventLogTypeInfo.TYPEINFO.ObjectClassName) ??
                                      DataClassInfo.New(ZapierTriggerEventLogTypeInfo.OBJECT_TYPE);

        info.ClassName = ZapierTriggerEventLogTypeInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = ZapierTriggerEventLogTypeInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Zapier trigger event log severity";
        info.ClassResourceID = resourceInfo.ResourceID;
        info.ClassType = ClassType.OTHER;
        var formInfo = FormHelper.GetBasicFormDefinition(nameof(ZapierTriggerEventLogTypeInfo.ZapierTriggerEventLogTypeID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(ZapierTriggerEventLogTypeInfo.ZapierTriggerEventLogTypeZapierTriggerID),
            Visible = false,
            DataType = FieldDataType.Integer,
            Enabled = true,
            AllowEmpty = false,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ZapierTriggerEventLogTypeInfo.ZapierTriggerEventLogTypeType),
            Visible = false,
            DataType = FieldDataType.Text,
            Enabled = true,
            AllowEmpty = false,
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }


    private static void InstallZapierTriggerInfo(ResourceInfo resourceInfo)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(ZapierTriggerInfo.TYPEINFO.ObjectClassName) ??
                                      DataClassInfo.New(ZapierTriggerInfo.OBJECT_TYPE);

        info.ClassName = ZapierTriggerInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = ZapierTriggerInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Zapier trigger";
        info.ClassResourceID = resourceInfo.ResourceID;
        info.ClassType = ClassType.OTHER;
        var formInfo = FormHelper.GetBasicFormDefinition(nameof(ZapierTriggerInfo.ZapierTriggerID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(ZapierTriggerInfo.ZapierTriggerCodeName),
            Visible = false,
            DataType = FieldDataType.Text,
            Enabled = true,
            AllowEmpty = false,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ZapierTriggerInfo.ZapierTriggerObjectType),
            Visible = true,
            DataType = FieldDataType.Text,
            Caption = "Object type",
            Enabled = true,
            AllowEmpty = false,
        };
        formItem.SetComponentName(TextWithLabelComponent.IDENTIFIER);
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ZapierTriggerInfo.ZapierTriggerObjectClassType),
            Visible = true,
            DataType = FieldDataType.Text,
            Caption = "Object class type",
            Enabled = true,
            AllowEmpty = false,
        };
        formItem.SetComponentName(TextWithLabelComponent.IDENTIFIER);
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ZapierTriggerInfo.ZapierTriggerEventType),
            Visible = true,
            DataType = FieldDataType.Text,
            Caption = "Event type",
            Enabled = true,
            AllowEmpty = false,
        };
        formItem.SetComponentName(TextWithLabelComponent.IDENTIFIER);
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ZapierTriggerInfo.ZapierTriggerZapierURL),
            Visible = true,
            DataType = FieldDataType.Text,
            Caption = "Zapier Url",
            Enabled = true,
            AllowEmpty = true,
        };
        formItem.SetComponentName(TextWithLabelComponent.IDENTIFIER);
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }


    private static void InstallApiKeyInfo(ResourceInfo resourceInfo)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(ApiKeyInfo.TYPEINFO.ObjectClassName) ??
                                      DataClassInfo.New(ApiKeyInfo.OBJECT_TYPE);

        info.ClassName = ApiKeyInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = ApiKeyInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Api Key info";
        info.ClassResourceID = resourceInfo.ResourceID;
        info.ClassType = ClassType.OTHER;
        var formInfo = FormHelper.GetBasicFormDefinition(nameof(ApiKeyInfo.ApiKeyID));
        var formItem = new FormFieldInfo
        {
            Name = nameof(ApiKeyInfo.ApiKeyToken),
            Visible = false,
            DataType = FieldDataType.Text,
            Enabled = true,
            AllowEmpty = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = ZapierNewApiKeyPage.RawTokenFieldName,
            Caption = "API key",
            IsDummyField = true,
            Visible = true,
            DataType = FieldDataType.Text,
            Size = 100,
            Enabled = true,
            AllowEmpty = true,
        };
        formItem.SetComponentName(TextWithLabelComponent.IDENTIFIER);
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ApiKeyInfo.ApiKeyCreated),
            DataType = FieldDataType.DateTime,
            Enabled = true,
            AllowEmpty = true,
            Visible = false,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ApiKeyInfo.ApiKeyCreatedBy),
            DataType = FieldDataType.Integer,
            Enabled = true,
            AllowEmpty = true,
            Visible = false,
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }


    /// <summary>
    /// Ensure that the form is not upserted with any existing form
    /// </summary>
    /// <param name="info"></param>
    /// <param name="form"></param>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}
