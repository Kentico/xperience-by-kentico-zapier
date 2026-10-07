using AspNetCore.Authentication.ApiKey;

using Kentico.Xperience.Zapier.Admin;
using Kentico.Xperience.Zapier.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Auth;

/// <summary>
/// Exercises the API key validation callback that <see cref="ZapierServiceCollectionExtensions.AddKenticoZapier"/>
/// wires into the <c>XbyKZapierApiKey</c> authentication scheme.
/// </summary>
[TestFixture]
public class ApiKeyAuthenticationTests
{
    private const string SCHEME = ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme;

    private ApiKeyOptions options = null!;
    private IApiKeyCachedService apiKeyCachedService = null!;


    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.AddKenticoZapier();
        var provider = services.BuildServiceProvider();

        options = provider.GetRequiredService<IOptionsMonitor<ApiKeyOptions>>().Get(SCHEME);
        apiKeyCachedService = Substitute.For<IApiKeyCachedService>();
    }


    [Test]
    public void AddKenticoZapier_RegistersSchemeTheZapierAppSendsInTheAuthorizationHeader()
    {
        // src/XbKcli/authentication.js sends "Authorization: XbyKZapierApiKey <key>"; both sides must agree.
        Assert.Multiple(() =>
        {
            Assert.That(SCHEME, Is.EqualTo("XbyKZapierApiKey"));
            Assert.That(options.KeyName, Is.EqualTo("Authorization"));
        });
    }


    [Test]
    public async Task ValidateKey_SucceedsForTheKeyWhoseHashIsStored()
    {
        string key = ApiKeyHelper.GenerateApiKey();
        apiKeyCachedService.GetApiKeyTokenHash().Returns(ApiKeyHelper.GetToken(key));

        var context = await ValidateAsync(key);

        Assert.That(context.Result?.Succeeded, Is.True);
    }


    [Test]
    public async Task ValidateKey_FailsForWrongKey()
    {
        apiKeyCachedService.GetApiKeyTokenHash().Returns(ApiKeyHelper.GetToken("the-real-key"));

        var context = await ValidateAsync("some-other-key");

        Assert.Multiple(() =>
        {
            Assert.That(context.Result, Is.Not.Null);
            Assert.That(context.Result!.Succeeded, Is.False);
        });
    }


    [Test]
    public async Task ValidateKey_FailsWhenNoKeyHasBeenGenerated()
    {
        apiKeyCachedService.GetApiKeyTokenHash().Returns((string?)null);

        var context = await ValidateAsync("anything");

        Assert.Multiple(() =>
        {
            Assert.That(context.Result, Is.Not.Null);
            Assert.That(context.Result!.Succeeded, Is.False);
        });
    }


    private async Task<ApiKeyValidateKeyContext> ValidateAsync(string providedKey)
    {
        var requestServices = new ServiceCollection()
            .AddSingleton(apiKeyCachedService)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = requestServices };
        var scheme = new AuthenticationScheme(SCHEME, SCHEME, typeof(ApiKeyInAuthorizationHeaderHandler));
        var context = new ApiKeyValidateKeyContext(httpContext, scheme, options, providedKey);

        await options.Events.ValidateKeyAsync(context);

        return context;
    }
}
