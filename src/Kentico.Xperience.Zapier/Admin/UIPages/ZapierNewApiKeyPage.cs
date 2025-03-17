using CMS.DataEngine;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Zapier.Admin.UIPages;
using Kentico.Xperience.Zapier.Auth;

[assembly: UIPage(
    parentType: typeof(ZapierApiKeyEditSection),
    slug: "generate",
    uiPageType: typeof(ZapierNewApiKeyPage),
    name: "Save your API key now",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.NoOrder)]

namespace Kentico.Xperience.Zapier.Admin.UIPages;

[UINavigation(false)]
internal class ZapierNewApiKeyPage : InfoEditPage<ApiKeyInfo>
{
    private readonly IInfoProvider<ApiKeyInfo> apiKeyInfoProvider;
    private readonly IPageLinkGenerator pageLinkGenerator;
    private readonly IZapierTokenManager tokenManager;


    private string? Token { get; set; }

    internal const string RawTokenFieldName = "ApiKeyRawToken";


    [PageParameter(typeof(IntPageModelBinder))]
    public override int ObjectId { get; set; }
    protected override bool RefetchAll => true;


    public ZapierNewApiKeyPage(IFormComponentMapper formComponentMapper, IFormDataBinder formDataBinder, IInfoProvider<ApiKeyInfo> apiKeyInfoProvider, IPageLinkGenerator pageLinkGenerator, IZapierTokenManager tokenManager)
    : base(formComponentMapper, formDataBinder)
    {
        this.apiKeyInfoProvider = apiKeyInfoProvider;
        this.pageLinkGenerator = pageLinkGenerator;
        this.tokenManager = tokenManager;
    }


    public override async Task ConfigurePage()
    {
        var apiKeyInfo = apiKeyInfoProvider.Get().FirstOrDefault() ?? throw new InvalidOperationException($"Zapier API Key not found.");
        if (string.IsNullOrEmpty(apiKeyInfo.ApiKeyToken))
        {
            Token = tokenManager.AssignToken(apiKeyInfo);
        }
        PageConfiguration.Headline = LocalizationService.GetString("zapier.apikey.generate");
        PageConfiguration.SubmitConfiguration.Label = LocalizationService.GetString("zapier.apikey.generate.continue");
        if (Token != null)
        {
            PageConfiguration.Callouts.Add(new CalloutConfiguration
            {
                Content = LocalizationService.GetString("zapier.apikey.generate.callout"),
                Type = CalloutType.FriendlyWarning,
                ContentAsHtml = true
            });
        }
        await base.ConfigurePage();
    }


    protected override async Task<ICommandResponse> SubmitInternal(FormSubmissionCommandArguments args, ICollection<IFormItem> items, IFormFieldValueProvider formFieldValueProvider)
    {
        string navigationUrl = pageLinkGenerator.GetPath<ZapierApiKeyListingPage>();
        return await Task.FromResult(NavigateTo(navigationUrl, RefetchAll));
    }


    protected override async Task<ICollection<IFormItem>> GetFormItems()
    {
        var obj = await base.GetFormItems();
        obj.OfType<TextWithLabelComponent>().SingleOrDefault((TextWithLabelComponent component) =>
            string.Equals(component.Name, RawTokenFieldName, StringComparison.OrdinalIgnoreCase))?.SetValue(Token);
        return obj;
    }
}
