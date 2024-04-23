using CMS.DataEngine;

namespace Kentico.Integration.Zapier;

/// <summary>
/// Class providing <see cref="ZapierTriggerEventLogTypeInfo"/> management.
/// </summary>
public partial class ZapierTriggerEventLogTypeInfoProvider : AbstractInfoProvider<ZapierTriggerEventLogTypeInfo, ZapierTriggerEventLogTypeInfoProvider>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZapierTriggerEventLogTypeInfoProvider"/> class.
    /// </summary>
    public ZapierTriggerEventLogTypeInfoProvider()
        : base(ZapierTriggerEventLogTypeInfo.TYPEINFO)
    {
    }
}
