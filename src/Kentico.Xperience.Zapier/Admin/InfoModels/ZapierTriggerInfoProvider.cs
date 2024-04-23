using CMS.DataEngine;

namespace Kentico.Integration.Zapier;

/// <summary>
/// Class providing <see cref="ZapierTriggerInfo"/> management.
/// </summary>
public partial class ZapierTriggerInfoProvider : AbstractInfoProvider<ZapierTriggerInfo, ZapierTriggerInfoProvider>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZapierTriggerInfoProvider"/> class.
    /// </summary>
    public ZapierTriggerInfoProvider()
        : base(ZapierTriggerInfo.TYPEINFO)
    {
    }
}
