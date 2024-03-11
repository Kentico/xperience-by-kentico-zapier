using CMS.Base;
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

        PageConfiguration.TableActions.AddDeleteAction(nameof(Delete), "Delete");


        PageConfiguration.ColumnConfigurations
        .AddColumn(nameof(ZapierTriggerInfo.ZapierTriggerID), "ID", defaultSortDirection: SortTypeEnum.Asc, maxWidth: 20, sortable: true)
        .AddColumn(nameof(ZapierTriggerInfo.ZapierTriggerDisplayName), "Name", sortable: true, searchable: true)
        .AddColumn(nameof(ZapierTriggerInfo.ZapierTriggerEventType), "Event type", searchable: true, sortable: true)
        .AddColumn(nameof(ZapierTriggerInfo.ZapierTriggerObjectType), "Object type", searchable: true, sortable: true)
        .AddColumn(nameof(ZapierTriggerInfo.ZapierTriggerZapierURL), "Url", searchable: true, sortable: true)
        .AddComponentColumn(nameof(ZapierTriggerInfo.ZapierTriggerEnabled), NamedComponentCellComponentNames.SIMPLE_STATUS_COMPONENT, "Status", maxWidth: 30, searchable: true, modelRetriever: StatusColumnModelRetriever);

        await base.ConfigurePage();
    }

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public override Task<ICommandResponse<RowActionResult>> Delete(int id) => base.Delete(id);


    private object StatusColumnModelRetriever(object formattedValue, IDataContainer allValues)
    {
        (Color, string, string) valueTuple;
        switch ((bool)formattedValue)
        {
            case true:
                valueTuple = (Color.SuccessText, "Active", Icons.CheckCircle);
                break;
            case false:
                valueTuple = (Color.AlertText, "Not active", Icons.BanSign);
                break;
        }
        (var color, string str1, string str2) = valueTuple;
        var componentCellProps = new SimpleStatusNamedComponentCellProps
        {
            IconName = str2,
            Label = str1,
            IconColor = color,
            LabelColor = color
        };
        return componentCellProps;
    }

}
