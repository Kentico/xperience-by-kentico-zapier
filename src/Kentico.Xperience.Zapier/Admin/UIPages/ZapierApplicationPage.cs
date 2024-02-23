
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Zapier.Admin.UIPages;
using CMS.Membership;

[assembly: UIApplication(
    identifier: ZapierApplicationPage.IDENTIFIER,
    type: typeof(ZapierApplicationPage),
    slug: "zapier",
    name: "Zapier",
    category: BaseApplicationCategories.CONFIGURATION,
    icon: Icons.ArrowRightRect,
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace Kentico.Xperience.Zapier.Admin.UIPages;

/// <summary>
/// The root application page for the Zapier integration.
/// </summary>
[UIPermission(SystemPermissions.VIEW)]
[UIPermission(SystemPermissions.CREATE)]
[UIPermission(SystemPermissions.UPDATE)]
[UIPermission(SystemPermissions.DELETE)]
internal class ZapierApplicationPage : ApplicationPage
{
    public const string IDENTIFIER = "zapier";
}
