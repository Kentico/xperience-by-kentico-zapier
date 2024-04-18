using CMS.DataEngine;

namespace Kentico.Integration.Zapier;

/// <summary>
/// Declares members for <see cref="ZapierTriggerInfo"/> management.
/// </summary>
public partial interface IZapierTriggerInfoProvider : IInfoProvider<ZapierTriggerInfo>, IInfoByIdProvider<ZapierTriggerInfo>, IInfoByNameProvider<ZapierTriggerInfo>
{
}
