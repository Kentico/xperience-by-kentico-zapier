using CMS.Helpers;
using Kentico.Integration.Zapier;

namespace Kentico.Xperience.Zapier;


public interface IApiKeyCachedService
{
    string? GetApiKeyToken();
}

public class ApiKeyCachedService : IApiKeyCachedService
{
    private readonly IProgressiveCache progressiveCache;
    private readonly IApiKeyInfoProvider apiKeyInfoProvider;

    public ApiKeyCachedService(IProgressiveCache progressiveCache, IApiKeyInfoProvider apiKeyInfoProvider)
    {
        this.progressiveCache = progressiveCache;
        this.apiKeyInfoProvider = apiKeyInfoProvider;
    }

    public string? GetApiKeyToken()
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

