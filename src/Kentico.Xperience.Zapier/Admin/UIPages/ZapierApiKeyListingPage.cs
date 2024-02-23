﻿
using CMS.DataEngine.Internal;
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
            Content = "Generate button will create a new API Key.If API Key already exists, this will override the existing one. Generating new key, can broke other connected zapier webhooks. Keep that in mind!",
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


    [PageCommand(Permission = "Generate")]
    public async Task<ICommandResponse<RowActionResult>> Generate(CancellationToken _)
    {
        var rowAction = new RowActionResult(true, true);

        ApiKeyInfo.Provider.BulkDelete(new WhereCondition());

        string apiKey = ApiKeyHelper.GenerateApiKey();

        var user = await userAccessor.Get();

        var apiKeyInfo = new ApiKeyInfo()
        {
            ApiKeyCreatedBy = user.UserID,
            ApiKeyToken = ApiKeyHelper.GetHash(apiKey),
        };

        apiKeyInfoProvider.Set(apiKeyInfo);

        //TODO nice
        //var command =  NavigateTo(pageUrlGenerator.GenerateUrl<ZapierTriggerListing>());

        return ResponseFrom(rowAction).UseCommand("LoadData").AddWarningMessage($"Save your API Key (It will not be visible again): {apiKey}");
    }


}
