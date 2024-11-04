using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.Admin.Headless.UIPages;
using Kentico.Xperience.Admin.Websites;
using Kentico.Xperience.Admin.Websites.UIPages;

namespace Kentico.Xperience.Zapier.Helpers;

internal static class AdminUrlHelper
{
    internal static PageParameterValues GetWebPageParams(int webPageItemId, int websiteChannelId, string lang) => new()
        {
            { typeof(WebPageLayout), new WebPageUrlIdentifier(lang, webPageItemId).ToString() },
            { typeof(WebPagesApplication), $"webpages-{websiteChannelId}" },
        };

    internal static PageParameterValues GetHeadlessParams(int headlessItemId, int headlessChannelId, string lang) => new()
    {
        { typeof(HeadlessEditLayout), headlessItemId },
        { typeof(HeadlessChannelContentLanguage), lang },
        { typeof(HeadlessChannelApplication), $"headless-{headlessChannelId}"},
    };

    internal static PageParameterValues GetReusableParams(int contentItemId, int contentItemContentFolderId, string lang) => new()
    {
        { typeof(ContentItemEditSection), contentItemId},
        { typeof(ContentHubFolder), contentItemContentFolderId },
        { typeof(ContentHubContentLanguage), lang },
    };
}
