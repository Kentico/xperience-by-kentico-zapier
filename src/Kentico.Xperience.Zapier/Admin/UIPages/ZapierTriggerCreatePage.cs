using CMS.Membership;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Zapier.Admin.UIPages;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;


[assembly: UIPage(
    parentType: typeof(ZapierTriggerListing),
    slug: "add",
    uiPageType: typeof(ZapierTriggerCreatePage),
    name: "Create a zapier trigger",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.First)]


namespace Kentico.Xperience.Zapier.Admin.UIPages;

[UIEvaluatePermission(SystemPermissions.CREATE)]
internal class ZapierTriggerCreatePage : ModelEditPage<ZapierTriggerEditModel>
{

    private ZapierTriggerEditModel? model = null;


    protected override ZapierTriggerEditModel Model => model ??= new();

    private readonly IZapierTriggerInfoProvider zapierTriggerInfoProvider;
    private readonly IPageUrlGenerator pageUrlGenerator;

    public ZapierTriggerCreatePage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IZapierTriggerInfoProvider zapierTriggerInfoProvider,
        IPageUrlGenerator pageUrlGenerator)
        : base(formItemCollectionProvider, formDataBinder)
    {
        this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;
        this.pageUrlGenerator = pageUrlGenerator;
    }

    protected override async Task<ICommandResponse> ProcessFormData(ZapierTriggerEditModel model,
        ICollection<IFormItem> formItems)
    {
        CreateZapierTriggerInfo(model);

        var result = await base.ProcessFormData(model, formItems);

        var navigateResponse = NavigateTo(
           pageUrlGenerator.GenerateUrl<ZapierTriggerListing>());

        foreach (var message in result.Messages)
        {
            navigateResponse.Messages.Add(message);
        }

        return navigateResponse;
    }

    private void CreateZapierTriggerInfo(ZapierTriggerEditModel model)
    {
        var infoObject = new ZapierTriggerInfo();

        model.MapToZapierTriggerInfoObject(infoObject);

        zapierTriggerInfoProvider.Set(infoObject);
    }



}
