using CMS.Membership;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Zapier.Admin.UIPages;

[assembly: UIPage(
    parentType: typeof(ZapierApplicationPage),
    slug: "zaps",
    uiPageType: typeof(ZapierTriggerListing),
    name: "List of Zapier triggers",
    templateName: TemplateNames.LISTING,
    order: UIPageOrder.First)]

namespace Kentico.Xperience.Zapier.Admin.UIPages;

[UIEvaluatePermission(SystemPermissions.VIEW)]
internal class ZapierTriggerListing : ListingPage
{
    protected override string ObjectType => ZapierTriggerInfo.OBJECT_TYPE;


    public override async Task ConfigurePage()
    {
        PageConfiguration.AddEditRowAction<ZapierTriggerEditPage>();

        PageConfiguration.ColumnConfigurations
        .AddColumn(nameof(ZapierTriggerInfo.ZapierTriggerID), "ID", defaultSortDirection: SortTypeEnum.Asc, maxWidth: 20, sortable: true)
        .AddColumn(nameof(ZapierTriggerInfo.ZapierTriggerEventType), "Event type", searchable: true, sortable: true)
        .AddColumn(nameof(ZapierTriggerInfo.ZapierTriggerObjectType), "Object type", searchable: true, sortable: true)
        .AddColumn(nameof(ZapierTriggerInfo.ZapierTriggerZapierURL), "Url", searchable: true, sortable: true);

        await base.ConfigurePage();
    }
}
