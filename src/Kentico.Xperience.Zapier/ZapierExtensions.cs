

using CMS.DataEngine;
using Newtonsoft.Json;

namespace Kentico.Xperience.Zapier;

public static class ZapierExtensions
{
    public static string ToZapierString(this BaseInfo baseInfo)
    {
        if (baseInfo != null)
        {
            var obj = new Dictionary<string, object>();
            foreach (string? col in baseInfo.ColumnNames)
            {
                obj[col] = baseInfo.GetValue(col);
            }

            return JsonConvert.SerializeObject(obj);
        }

        return string.Empty;
    }
}
