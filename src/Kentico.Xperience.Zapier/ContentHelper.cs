using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;

namespace Kentico.Xperience.Zapier;


public interface IContentHelper
{
    Task<Dictionary<string, object>?> GetContentItemDataByGuid(Guid contentItemGuid, string contentTypeName);

    Task<Dictionary<string, object>?> GetWebItemDataByGuid(Guid webItemGuid, string contentTypeName, string websiteChannelName);
}


public class ContentHelper : IContentHelper
{
    private readonly IContentQueryExecutor contentQueryExecutor;

    private readonly IContentQueryResultMapper contentQueryResultMapper;
    private readonly IWebPageQueryResultMapper webPageQueryResultMapper;


    public ContentHelper(IContentQueryExecutor contentQueryExecutor, IContentQueryResultMapper contentQueryResultMapper, IWebPageQueryResultMapper webPageQueryResultMapper)
    {
        this.contentQueryExecutor = contentQueryExecutor;
        this.contentQueryResultMapper = contentQueryResultMapper;
        this.webPageQueryResultMapper = webPageQueryResultMapper;
    }

    public async Task<Dictionary<string, object>?> GetContentItemDataByGuid(Guid contentItemGuid, string contentTypeName)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName,
                c => c.Where(w => w.WhereEquals(nameof(IContentQueryDataContainer.ContentItemGUID), contentItemGuid)).TopN(1));

        var data = await contentQueryExecutor.GetResult(builder, contentQueryResultMapper.MapReusable, new ContentQueryExecutionOptions { ForPreview = true });

        return data.FirstOrDefault();
    }

    public async Task<Dictionary<string, object>?> GetWebItemDataByGuid(Guid webItemGuid, string contentTypeName, string websiteChannelName)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName,
                c => c.ForWebsite(websiteChannelName)
                .Where(w => w.WhereEquals(nameof(IWebPageContentQueryDataContainer.WebPageItemGUID), webItemGuid))
                .TopN(1));

        var data = await contentQueryExecutor.GetWebPageResult(builder, webPageQueryResultMapper.MapPages, new ContentQueryExecutionOptions { ForPreview = true });


        return data.FirstOrDefault();
    }

}



public static class MapperExtensions
{
    public static Dictionary<string, object> MapReusable(this IContentQueryResultMapper _, IContentQueryDataContainer container) => GetProperties(container);

    public static Dictionary<string, object> MapPages(this IWebPageQueryResultMapper _, IWebPageContentQueryDataContainer container) => GetProperties(container);

    private static Dictionary<string, object> GetProperties(IContentQueryDataContainer container)
    {
        var obj = new Dictionary<string, object>() { };

        var classStructure = ClassStructureInfo.GetClassInfo(container.ContentTypeName);

        var forbiddenCols = new List<string> { "ContentItemDataID", "ContentItemDataCommonDataID", "ContentItemDataGUID" };

        foreach (string? col in classStructure.ColumnNames.Where(x => !forbiddenCols.Contains(x)))
        {
            obj[col] = container.GetValue<object>(col);
        }
        obj.TryAdd("ContentTypeName", container.ContentTypeName);
        return obj;
    }
}
