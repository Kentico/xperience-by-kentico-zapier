using CMS.ContentEngine;
using CMS.DataEngine;

namespace Kentico.Xperience.Zapier.Extensions;


public static class ContentItemDataExtension
{
    public delegate bool MyFunc<in T1, T2>(T1 a, out T2 b);

    public static readonly List<string> ForbiddenCols = ["ContentItemDataID", "ContentItemDataCommonDataID", "ContentItemDataGUID"];

    public static Dictionary<string, object> GetDataForZapier(this ContentItemData contentItemData, string contentTypeName) => GetData(contentItemData.TryGetValue<object>, contentTypeName);


    public static Dictionary<string, object> GetDataForZapier(this ContentItemDataEventContainer contentItemDataContainer, string contentTypeName) => GetData(contentItemDataContainer.TryGetValue<object>, contentTypeName);

    public static Dictionary<string, object> GetData(MyFunc<string, object> tryGetMethod, string contentTypeName)
    {
        var obj = new Dictionary<string, object>();

        var classStructure = ClassStructureInfo.GetClassInfo(contentTypeName);

        foreach (string col in classStructure.ColumnNames.Where(x => !ForbiddenCols.Contains(x)))
        {
            if (tryGetMethod(col, out object value))
            {
                obj[col] = value;
            }
        }

        obj.TryAdd("ContentTypeName", contentTypeName);

        return obj;
    }

}
