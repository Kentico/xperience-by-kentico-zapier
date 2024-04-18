using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Zapier.Admin.UIPages;

[assembly: UIPage(
    parentType: typeof(ZapierApiKeyListingPage),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(ZapierApiKeyEditSection),
    name: "Edit",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 0)]

namespace Kentico.Xperience.Zapier.Admin.UIPages;

[UINavigation(false)]
[UIBreadcrumbs(false)]
internal class ZapierApiKeyEditSection : EditSectionPage<ApiKeyInfo>
{
}
