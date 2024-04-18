using CMS.DataEngine;

namespace Kentico.Xperience.Zapier.Common;


internal static class DataClassInfoProviderExtension
{
    public static ObjectQuery<DataClassInfo> GetZapierTypesQuery(this DataClassInfoProvider provider, string classType, IList<string> allowedObjects)
    {
        var query = provider.Get()
            .WhereIn(nameof(DataClassInfo.ClassName), allowedObjects)
            .WhereEquals(nameof(DataClassInfo.ClassType), classType)
            .Columns(nameof(DataClassInfo.ClassDisplayName), nameof(DataClassInfo.ClassName), nameof(DataClassInfo.ClassContentTypeType))
            .OrderBy(nameof(DataClassInfo.ClassName));
        return query;
    }
}
