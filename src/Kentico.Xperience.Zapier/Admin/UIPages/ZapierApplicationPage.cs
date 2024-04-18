using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.Zapier.Admin.UIPages;

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
[UIPermission(ZapierConstants.Permissions.GENERATE)]
internal class ZapierApplicationPage : ApplicationPage
{
    /// <summary>
    /// Unique identifier of Zapier application
    /// </summary>
    public const string IDENTIFIER = "zapier";
}
