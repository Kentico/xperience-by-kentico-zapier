using CMS.DataEngine;

using Kentico.Integration.Zapier;

namespace Kentico.Xperience.Zapier.Triggers.Extensions;

internal static class ZapierTriggerExtensions
{
    public const string FormComponentOptions = $"{nameof(ZapierTriggerEvents.Create)}\r\n" +
                                               $"{nameof(ZapierTriggerEvents.Update)}\r\n" +
                                               $"{nameof(ZapierTriggerEvents.Delete)}\r\n";


    public static string GetType(string objectType)
    {
        var dataInfo = DataClassInfoProvider.ProviderObject.Get(objectType);

        string? type = dataInfo?.ClassType ?? string.Empty;
        string? contentType = dataInfo?.ClassContentTypeType;

        return !string.IsNullOrEmpty(contentType) ? contentType : type;
    }
    public static string GetUniqueCodename()
    {
        string guid = Guid.NewGuid().ToString();
        string? randString = guid.Split('-').FirstOrDefault();

        return randString ?? string.Empty;
    }
}


internal enum ZapierTriggerObjectClassType
{
    Form,
    Other,
    System,
    Website,
    Reusable,
    Headless,
    Email,
}


internal enum ZapierTriggerEvents
{
    None = -1,
    Create,
    Update,
    Delete,
    Publish
}
