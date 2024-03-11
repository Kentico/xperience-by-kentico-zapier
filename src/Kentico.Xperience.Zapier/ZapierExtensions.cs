

using CMS.DataEngine;

namespace Kentico.Xperience.Zapier;

public static class ZapierExtensions
{
    public static Dictionary<string, object>? TozapierDictionary(this BaseInfo baseInfo)
    {
        if (baseInfo != null)
        {
            var obj = new Dictionary<string, object>();
            foreach (string? col in baseInfo.ColumnNames)
            {
                obj[col] = baseInfo.GetValue(col);
            }

            return obj;
        }

        return null;
    }
}
