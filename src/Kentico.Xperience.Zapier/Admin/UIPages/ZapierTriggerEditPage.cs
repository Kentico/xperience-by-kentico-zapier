
using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Zapier.Admin.UIPages;

[assembly: UIPage(
    parentType: typeof(ZapierTriggerSectionPage),
    slug: "edit",
    uiPageType: typeof(ZapierTriggerEditPage),
    name: "View zapier trigger",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.First)]

namespace Kentico.Xperience.Zapier.Admin.UIPages;

internal class ZapierTriggerEditPage : InfoEditPage<ZapierTriggerInfo>
{
    [PageParameter(typeof(IntPageModelBinder))]
    public override int ObjectId { get; set; }


    public ZapierTriggerEditPage(IFormComponentMapper formComponentMapper, IFormDataBinder formDataBinder) : base(formComponentMapper, formDataBinder)
    {
    }

    public override async Task ConfigurePage()
    {
        PageConfiguration.SubmitConfiguration.Visible = false;
        await base.ConfigurePage();
    }
}
