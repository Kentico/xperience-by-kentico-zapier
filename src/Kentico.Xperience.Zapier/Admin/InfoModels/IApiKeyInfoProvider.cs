using CMS.DataEngine;

namespace Kentico.Integration.Zapier;

/// <summary>
/// Declares members for <see cref="ApiKeyInfo"/> management.
/// </summary>
public partial interface IApiKeyInfoProvider : IInfoProvider<ApiKeyInfo>, IInfoByIdProvider<ApiKeyInfo>
{
}
