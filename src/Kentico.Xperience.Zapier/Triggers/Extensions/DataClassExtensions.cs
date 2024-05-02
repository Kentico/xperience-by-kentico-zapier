using CMS.DataEngine;

namespace Kentico.Xperience.Zapier.Triggers.Extensions
{
    internal static class DataClassExtensions
    {
        internal static bool IsContentType(this DataClassInfo contentType) =>
            (contentType?.ClassType?.Equals("Content", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault();

        internal static bool IsForm(this DataClassInfo contentType) =>
            (contentType?.ClassType?.Equals("Form", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault();
    }
}
