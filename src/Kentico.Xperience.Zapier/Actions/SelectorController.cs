using System.Data;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Headless;
using CMS.Headless.Internal;
using CMS.Helpers;
using CMS.Websites;
using Kentico.Xperience.Admin.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kentico.Xperience.Zapier.Actions;

//[Authorize(AuthenticationSchemes = ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme)]
[ApiController]
public class SelectorController : ControllerBase
{
    private readonly ILogger<ActionPublishController> logger;
    private readonly IContentQueryExecutor contentQueryExecutor;
    private readonly ZapierConfiguration zapierConfiguration;
    public SelectorController(
        ILogger<ActionPublishController> logger,
        IContentQueryExecutor contentQueryExecutor,
        IOptionsMonitor<ZapierConfiguration> zapierConfiguration)
    {
        this.logger = logger;
        this.contentQueryExecutor = contentQueryExecutor;
        this.zapierConfiguration = zapierConfiguration.CurrentValue;
    }

    [HttpGet]
    [Route($"zapier/actions/{ClassContentTypeType.HEADLESS}/{{className}}/{{headlessChannelId}}/{{languageName}}")]
    public async Task<ActionResult<IEnumerable<object>>> GetHeadlessItemsAsync(string className, int headlessChannelId, string languageName)
    {
        if (!zapierConfiguration.AllowedObjects.Contains(className))
        {
            return BadRequest();
        }
        int channelId = HeadlessChannelInfo.Provider.Get(headlessChannelId).HeadlessChannelChannelID;
        var itemsData = ContentItemLanguageMetadataInfo.Provider.Get()
            .Source(x => x.Join<ContentItemInfo>(
                nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID), nameof(ContentItemInfo.ContentItemID)))
            .Source(x => x.Join<ContentLanguageInfo>(
                $"[CMS_ContentItemLanguageMetadata].{nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID)}", $"[CMS_ContentLanguage].{nameof(ContentLanguageInfo.ContentLanguageID)}"))
            .Source(x => x.Join<DataClassInfo>(
                $"[CMS_ContentItem].{nameof(ContentItemInfo.ContentItemContentTypeID)}", $"[CMS_Class].{nameof(DataClassInfo.ClassID)}"))
            .Source(x => x.Join<HeadlessItemInfo>(
                $"[CMS_ContentItem].{nameof(ContentItemInfo.ContentItemID)}", $"[CMS_HeadlessItem].{nameof(HeadlessItemInfo.HeadlessItemContentItemID)}"))
            .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageName), languageName)
            .WhereEquals(nameof(DataClassInfo.ClassName), className)
            .WhereEquals(nameof(DataClassInfo.ClassContentTypeType), ClassContentTypeType.HEADLESS)
            .WhereEquals(nameof(ContentItemInfo.ContentItemChannelID), channelId)
            .Columns(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataDisplayName), nameof(HeadlessItemInfo.HeadlessItemID))
            .Result;
        if (DataHelper.DataSourceIsEmpty(itemsData))
        {
            return Array.Empty<string>();
        }
        return Ok(itemsData.Tables[0]
            .AsEnumerable()
            .Select(row => new
            {
                Id = row[nameof(HeadlessItemInfo.HeadlessItemID)],
                Name = row[nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataDisplayName)]
            }));
    }

    [HttpGet]
    [Route($"zapier/actions/{ClassContentTypeType.WEBSITE}/{{className}}/{{websiteChannelId}}/{{languageName}}")]
    public async Task<ActionResult<IEnumerable<object>>> GetWebPagesAsync(string className, int websiteChannelId, string languageName)
    {
        if (!zapierConfiguration.AllowedObjects.Contains(className))
        {
            return BadRequest();
        }
        string channelName = ChannelInfoProvider.ProviderObject.Get()
            .Source(x => x.Join<WebsiteChannelInfo>(nameof(ChannelInfo.ChannelID), nameof(WebsiteChannelInfo.WebsiteChannelChannelID)))
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelID), websiteChannelId)
            .Column(nameof(ChannelInfo.ChannelName))
            .TopN(1)
            .GetScalarResult<string>();
        if (channelName == null)
        {
            return BadRequest();
        }
        int languageId = await ContentLanguageInfoProvider.ProviderObject.Get()
            .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageName), languageName)
            .Column(nameof(ContentLanguageInfo.ContentLanguageID))
            .GetScalarResultAsync<int>();
        var builder = new ContentItemQueryBuilder()
            .ForContentType(className,
                c => c.ForWebsite(channelName)
                .Where(x => x.WhereEquals(nameof(WebPageFields.ContentItemCommonDataContentLanguageID), languageId))
                    .Columns(nameof(WebPageFields.WebPageUrlPath),
                    nameof(WebPageFields.WebPageItemName),
                    nameof(WebPageFields.WebPageItemID)));
        var res = await contentQueryExecutor.GetWebPageResult(builder, resultSelector: rowData =>
        {
            string Name = $"{rowData.WebPageItemName} ({rowData.WebPageUrlPath})";
            return new { Id = rowData.WebPageItemID, Name };
        });
        return Ok(res);
    }

    [HttpGet]
    [Route($"zapier/actions/{ClassContentTypeType.REUSABLE}/{{className}}/{{languageName}}")]
    public ActionResult<IEnumerable<object>> GetReusableItems(string className, string languageName)
    {
        if (!zapierConfiguration.AllowedObjects.Contains(className))
        {
            return BadRequest();
        }
        var itemsData = ContentItemLanguageMetadataInfo.Provider.Get()
            .Source(x => x.Join<ContentItemInfo>(
                nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID), nameof(ContentItemInfo.ContentItemID)))
            .Source(x => x.Join<ContentLanguageInfo>(
                $"[CMS_ContentItemLanguageMetadata].{nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID)}", $"[CMS_ContentLanguage].{nameof(ContentLanguageInfo.ContentLanguageID)}"))
            .Source(x => x.Join<DataClassInfo>(
                $"[CMS_ContentItem].{nameof(ContentItemInfo.ContentItemContentTypeID)}", $"[CMS_Class].{nameof(DataClassInfo.ClassID)}"))
            .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageName), languageName)
            .WhereEquals(nameof(DataClassInfo.ClassName), className)
            .WhereEquals(nameof(DataClassInfo.ClassContentTypeType), ClassContentTypeType.REUSABLE)
            .Columns(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataDisplayName), nameof(ContentItemInfo.ContentItemID))
            .Result;
        if (DataHelper.DataSourceIsEmpty(itemsData))
        {
            return Array.Empty<string>();
        }
        return Ok(itemsData.Tables[0]
            .AsEnumerable()
            .Select(row => new
            {
                Id = row[nameof(ContentItemInfo.ContentItemID)],
                Name = row[nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataDisplayName)]
            }));
    }

    [HttpGet]
    [Route($"zapier/actions/types/{ClassContentTypeType.REUSABLE}")]
    public ActionResult<IEnumerable<object>> GetReusableTypes() =>
        GetTypes(ClassContentTypeType.REUSABLE);

    [HttpGet]
    [Route($"zapier/actions/types/{ClassContentTypeType.WEBSITE}/{{websiteChannelId}}")]
    public ActionResult<IEnumerable<object>> GetWebsiteTypes(int websiteChannelId)
    {
        int channelId = WebsiteChannelInfoProvider.ProviderObject.Get(websiteChannelId).WebsiteChannelChannelID;
        return GetTypes(ClassContentTypeType.WEBSITE, channelId);
    }

    [HttpGet]
    [Route($"zapier/actions/types/{ClassContentTypeType.HEADLESS}/{{headlessChannelId}}")]
    public ActionResult<IEnumerable<object>> GetHeadlessTypes(int headlessChannelId)
    {
        int channelId = HeadlessChannelInfo.Provider.Get(headlessChannelId).HeadlessChannelChannelID;
        return GetTypes(ClassContentTypeType.HEADLESS, channelId);
    }

    private ActionResult<IEnumerable<object>> GetTypes(string classContentTypeType, int? channelId = null)
    {
        var allowedObjects = zapierConfiguration.AllowedObjects.ToList();
        var query = DataClassInfoProvider.ProviderObject.Get();
        if (channelId != null)
        {
            query = query.Source(x =>
                x.Join<ContentTypeChannelInfo>(
                    nameof(DataClassInfo.ClassID),
                    nameof(ContentTypeChannelInfo.ContentTypeChannelContentTypeID))
                )
            .WhereEquals(nameof(ContentTypeChannelInfo.ContentTypeChannelChannelID), channelId);
        }
        var res = query
            .WhereEquals(nameof(DataClassInfo.ClassContentTypeType), classContentTypeType)
            .WhereIn(nameof(DataClassInfo.ClassName), allowedObjects)
            .Columns(nameof(DataClassInfo.ClassDisplayName), nameof(DataClassInfo.ClassName))
            .OrderBy(nameof(DataClassInfo.ClassName))
            .Select(x => new { Id = x.ClassName, Name = x.ClassDisplayName })
            .ToList();
        return Ok(res);
    }

    [HttpGet]
    [Route("zapier/actions/languages")]
    public ActionResult<IEnumerable<object>> GetLanguages() =>
        Ok(ContentLanguageInfoProvider.ProviderObject.Get()
            .OrderBy(nameof(ContentLanguageInfo.ContentLanguageIsDefault))
            .Columns(nameof(ContentLanguageInfo.ContentLanguageName),
                nameof(ContentLanguageInfo.ContentLanguageDisplayName))
            .Select(x => new { Id = x.ContentLanguageName, Name = x.ContentLanguageDisplayName })
            .ToList());

    [HttpGet]
    [Route("zapier/actions/types")]
    public ActionResult<IEnumerable<object>> GetClassContentTypesTypes()
    {
        var res = new string[] { ClassContentTypeType.WEBSITE, ClassContentTypeType.REUSABLE, ClassContentTypeType.HEADLESS }
            .Select(x => new { Id = x, Name = x })
            .ToList();
        return Ok(res);
    }

    [HttpGet]
    [Route($"zapier/actions/{ClassContentTypeType.WEBSITE}/channels")]
    public ActionResult<IEnumerable<object>> GetWebsiteChannels()
    {
        var channelsData = WebsiteChannelInfoProvider.ProviderObject.Get()
            .Source(x =>
                x.Join<ChannelInfo>(nameof(WebsiteChannelInfo.WebsiteChannelChannelID), nameof(ChannelInfo.ChannelID)))
            .Columns(nameof(WebsiteChannelInfo.WebsiteChannelID), nameof(ChannelInfo.ChannelDisplayName))
            .Result;
        if (DataHelper.DataSourceIsEmpty(channelsData))
        {
            return Array.Empty<string>();
        }
        return Ok(channelsData.Tables[0]
            .AsEnumerable()
            .Select(row => new
            {
                Id = row[nameof(WebsiteChannelInfo.WebsiteChannelID)],
                Name = row[nameof(ChannelInfo.ChannelDisplayName)]
            }));
    }

    [HttpGet]
    [Route($"zapier/actions/{ClassContentTypeType.HEADLESS}/channels")]
    public ActionResult<IEnumerable<object>> GetHeadlessChannels()
    {
        var channelsData = HeadlessChannelInfo.Provider.Get()
            .Source(x =>
                x.Join<ChannelInfo>(nameof(HeadlessChannelInfo.HeadlessChannelChannelID), nameof(ChannelInfo.ChannelID)))
            .Columns(nameof(HeadlessChannelInfo.HeadlessChannelID), nameof(ChannelInfo.ChannelDisplayName))
            .Result;
        if (DataHelper.DataSourceIsEmpty(channelsData))
        {
            return Array.Empty<string>();
        }
        return Ok(channelsData.Tables[0]
            .AsEnumerable()
            .Select(row => new
            {
                Id = row[nameof(HeadlessChannelInfo.HeadlessChannelID)],
                Name = row[nameof(ChannelInfo.ChannelDisplayName)]
            }));
    }
}

