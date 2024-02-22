
using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Zapier.Admin.UIPages;

[assembly: UIPage(
    parentType: typeof(ZapierTriggerListing),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(ZapierTriggerSectionPage),
    name: "Edit",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 0)]

namespace Kentico.Xperience.Zapier.Admin.UIPages;

internal class ZapierTriggerSectionPage : EditSectionPage<ZapierTriggerInfo>
{
}
