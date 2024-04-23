using CMS.DataEngine;

namespace Kentico.Integration.Zapier;

/// <summary>
/// Class providing <see cref="ApiKeyInfo"/> management.
/// </summary>
public partial class ApiKeyInfoProvider : AbstractInfoProvider<ApiKeyInfo, ApiKeyInfoProvider>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyInfoProvider"/> class.
    /// </summary>
    public ApiKeyInfoProvider()
        : base(ApiKeyInfo.TYPEINFO)
    {
    }
}
