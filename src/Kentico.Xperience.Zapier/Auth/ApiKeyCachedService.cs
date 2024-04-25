using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Integration.Zapier;

namespace Kentico.Xperience.Zapier.Auth;

internal interface IApiKeyCachedService
{
    string? GetApiKeyTokenHash();
}


internal class ApiKeyCachedService : IApiKeyCachedService
{
    private readonly IProgressiveCache progressiveCache;
    private readonly IInfoProvider<ApiKeyInfo> apiKeyInfoProvider;


    public ApiKeyCachedService(IProgressiveCache progressiveCache, IInfoProvider<ApiKeyInfo> apiKeyInfoProvider)
    {
        this.progressiveCache = progressiveCache;
        this.apiKeyInfoProvider = apiKeyInfoProvider;
    }


    public string? GetApiKeyTokenHash()
    {
        var data = progressiveCache.Load(cacheSettings =>
        {
            var result = apiKeyInfoProvider.Get().FirstOrDefault();

            cacheSettings.CacheDependency = CacheHelper.GetCacheDependency($"{ApiKeyInfo.OBJECT_TYPE}|all");

            return result;
        }, new CacheSettings(TimeSpan.FromHours(1).TotalMinutes, $"{ApiKeyInfo.OBJECT_TYPE}-resultstring"));

        return data?.ApiKeyToken;
    }
}
