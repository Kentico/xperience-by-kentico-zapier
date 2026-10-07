using CMS.DataEngine;
using CMS.Helpers;
using CMS.Tests;

using Kentico.Xperience.Zapier.Tests.Support;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Auth;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Auth;

[TestFixture]
public class ApiKeyCachedServiceTests : UnitTests
{
    private IInfoProviderFake<ApiKeyInfo, IInfoProvider<ApiKeyInfo>> apiKeyFake = null!;
    private IProgressiveCache progressiveCache = null!;


    [SetUp]
    public void SetUp()
    {
        apiKeyFake = Fake<ApiKeyInfo, IInfoProvider<ApiKeyInfo>>();

        // Pass-through cache: always executes the loader so the test observes the provider call.
        progressiveCache = Substitute.For<IProgressiveCache>();
        progressiveCache
            .Load(Arg.Any<Func<CacheSettings, ApiKeyInfo?>>(), Arg.Any<CacheSettings>())
            .Returns(call => call.ArgAt<Func<CacheSettings, ApiKeyInfo?>>(0)(call.ArgAt<CacheSettings>(1)));
    }


    [Test]
    public void GetApiKeyTokenHash_ReturnsStoredHash()
    {
        apiKeyFake.WithData(new ApiKeyInfo { ApiKeyID = 1, ApiKeyToken = "hash-value" });
        var service = new ApiKeyCachedService(progressiveCache, apiKeyFake.Provider());

        Assert.That(service.GetApiKeyTokenHash(), Is.EqualTo("hash-value"));
    }


    [Test]
    public void GetApiKeyTokenHash_ReturnsNullWhenNoKeyExists()
    {
        apiKeyFake.WithData();
        var service = new ApiKeyCachedService(progressiveCache, apiKeyFake.Provider());

        Assert.That(service.GetApiKeyTokenHash(), Is.Null);
    }


    [Test]
    public void GetApiKeyTokenHash_InvalidatesOnAnyApiKeyChange()
    {
        apiKeyFake.WithData(new ApiKeyInfo { ApiKeyID = 1, ApiKeyToken = "hash-value" });
        CacheSettings? capturedSettings = null;
        progressiveCache
            .Load(Arg.Any<Func<CacheSettings, ApiKeyInfo?>>(), Arg.Any<CacheSettings>())
            .Returns(call =>
            {
                capturedSettings = call.ArgAt<CacheSettings>(1);
                return call.ArgAt<Func<CacheSettings, ApiKeyInfo?>>(0)(capturedSettings);
            });
        var service = new ApiKeyCachedService(progressiveCache, apiKeyFake.Provider());

        service.GetApiKeyTokenHash();

        Assert.Multiple(() =>
        {
            Assert.That(capturedSettings, Is.Not.Null);
            Assert.That(capturedSettings!.CacheDependency?.CacheKeys, Does.Contain($"{ApiKeyInfo.OBJECT_TYPE}|all"));
        });
    }
}
