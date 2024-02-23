
using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Zapier.Admin.UIPages;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

[assembly: UIPage(
    parentType: typeof(ZapierTriggerSectionPage),
    slug: "edit",
    uiPageType: typeof(ZapierTriggerEditPage),
    name: "Edit zapier trigger",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.First)]

namespace Kentico.Xperience.Zapier.Admin.UIPages;

internal class ZapierTriggerEditPage : ModelEditPage<ZapierTriggerEditModel>
{
    private ZapierTriggerEditModel? model;
    private readonly IZapierTriggerInfoProvider zapierTriggerInfoProvider;
    protected override ZapierTriggerEditModel Model
    {
        get
        {

            if (model != null)
            {
                return model;
            }

            var info = zapierTriggerInfoProvider.Get(ObjectID);
            if (info == null)
            {
                return new ZapierTriggerEditModel();
            }

            model = new ZapierTriggerEditModel()
            {
                Name = info.ZapierTriggerDisplayName,
                EventType = info.ZapierTriggerEventType,
                ObjectType = new List<ObjectRelatedItem> { new() { ObjectCodeName = info.mZapierTriggerObjectType } },
                Enabled = info.ZapierTriggerEnabled,
                ZapierURL = info.ZapierTriggerZapierURL
            };

            return model;
        }
    }

    [PageParameter(typeof(IntPageModelBinder))]
    public int ObjectID { get; set; }

    public ZapierTriggerEditPage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IZapierTriggerInfoProvider zapierTriggerInfoProvider
        ) : base(formItemCollectionProvider, formDataBinder) => this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;


    protected override async Task<ICommandResponse> ProcessFormData(ZapierTriggerEditModel model,
        ICollection<IFormItem> formItems)
    {
        var infoObject = zapierTriggerInfoProvider.Get(ObjectID);

        model.MapToZapierTriggerInfoObject(infoObject);
        zapierTriggerInfoProvider.Set(infoObject);

        return await base.ProcessFormData(model, formItems);
    }

}
