using CMS.DataEngine;

namespace Kentico.Integration.Zapier;

/// <summary>
/// Class providing <see cref="ZapierTriggerEventLogTypeInfo"/> management.
/// </summary>
[ProviderInterface(typeof(IZapierTriggerEventLogTypeInfoProvider))]
public partial class ZapierTriggerEventLogTypeInfoProvider : AbstractInfoProvider<ZapierTriggerEventLogTypeInfo, ZapierTriggerEventLogTypeInfoProvider>, IZapierTriggerEventLogTypeInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZapierTriggerEventLogTypeInfoProvider"/> class.
    /// </summary>
    public ZapierTriggerEventLogTypeInfoProvider()
        : base(ZapierTriggerEventLogTypeInfo.TYPEINFO)
    {
    }
}
