using CMS.DataEngine;
using CMS.Membership;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Zapier.Admin.UIPages;
using Kentico.Xperience.Admin.Base.Authentication;
using CMS.Base;

[assembly: UIPage(
    parentType: typeof(ZapierApplicationPage),
    slug: "apikey",
    uiPageType: typeof(ZapierApiKeyListingPage),
    name: "API Key",
    templateName: TemplateNames.LISTING,
    order: UIPageOrder.NoOrder)]


namespace Kentico.Xperience.Zapier.Admin.UIPages;

internal class ZapierApiKeyListingPage : ListingPage
{
    private readonly IApiKeyInfoProvider apiKeyInfoProvider;
    private readonly IAuthenticatedUserAccessor userAccessor;
    private readonly IUserInfoProvider userProvider;


    public ZapierApiKeyListingPage(IApiKeyInfoProvider apiKeyInfoProvider, IAuthenticatedUserAccessor userAccessor, IUserInfoProvider userProvider)
    {
        this.apiKeyInfoProvider = apiKeyInfoProvider;
        this.userAccessor = userAccessor;
        this.userProvider = userProvider;
    }

    protected override string ObjectType => ApiKeyInfo.OBJECT_TYPE;

    public override Task ConfigurePage()
    {
        PageConfiguration.Callouts = [new()
        {
            Content = LocalizationService.GetString("apikey.callout"),
            Type = CalloutType.FriendlyWarning,
            ContentAsHtml = true
        }];

        PageConfiguration.HeaderActions.AddCommand("Generate", nameof(Generate));
        PageConfiguration.TableActions.AddDeleteAction(nameof(Delete), "Delete");

        PageConfiguration.ColumnConfigurations
        .AddColumn(nameof(ApiKeyInfo.ApiKeyToken), "Key", formatter: (o, all) => "API Key")
        .AddColumn(nameof(ApiKeyInfo.ApiKeyCreated), "Created")
        .AddColumn(nameof(ApiKeyInfo.ApiKeyCreatedBy), "Created by", formatter: GetUserName);

        return base.ConfigurePage();
    }

    private string GetUserName(object formattedValue, IDataContainer _)
    {
        int userId = (int)formattedValue;

        var user = userProvider.Get(userId);

        return user.UserName ?? user.Email ?? string.Empty;
    }

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public override Task<ICommandResponse<RowActionResult>> Delete(int id) => base.Delete(id);

    [PageCommand(Permission = ZapierConstants.Permissions.GENERATE)]
    public async Task<ICommandResponse<RowActionResult>> Generate(CancellationToken _)
    {
        var rowAction = new RowActionResult(true, true);
        var user = await userAccessor.Get();
        string apiKey = ApiKeyHelper.GenerateApiKey();

        using var transaction = new CMSTransactionScope();

        ApiKeyInfo.Provider.BulkDelete(new WhereCondition());

        var apiKeyInfo = new ApiKeyInfo()
        {
            ApiKeyCreatedBy = user.UserID,
            ApiKeyToken = ApiKeyHelper.GetToken(apiKey),
        };

        apiKeyInfoProvider.Set(apiKeyInfo);

        transaction.Commit();

        return ResponseFrom(rowAction).UseCommand("LoadData").AddWarningMessage(string.Format(LocalizationService.GetString("apikey.savemessage"), apiKey)); //Nice to have - Can be redirected to a separate page to display ApiKey
    }


}
