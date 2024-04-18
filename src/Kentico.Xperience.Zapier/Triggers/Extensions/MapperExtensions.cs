using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;

namespace Kentico.Xperience.Zapier.Triggers.Extensions;


internal static class MapperExtensions
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
