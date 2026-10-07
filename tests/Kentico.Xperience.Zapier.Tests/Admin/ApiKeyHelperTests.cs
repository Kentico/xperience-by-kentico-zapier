using Kentico.Xperience.Zapier.Admin;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Admin;

[TestFixture]
public class ApiKeyHelperTests
{
    [Test]
    public void GenerateApiKey_Returns32RandomBytesAsBase64()
    {
        string first = ApiKeyHelper.GenerateApiKey();
        string second = ApiKeyHelper.GenerateApiKey();

        Assert.Multiple(() =>
        {
            Assert.That(Convert.FromBase64String(first), Has.Length.EqualTo(32));
            Assert.That(first, Is.Not.EqualTo(second));
        });
    }


    [Test]
    public void GetToken_IsDeterministicSha256OfUtf8Input()
    {
        // SHA-256("secret") in Base64
        const string expected = "K7gNU3sdo+OL0wNhqoVWhr3g6s1xYv72ol/pe/Unols=";

        Assert.That(ApiKeyHelper.GetToken("secret"), Is.EqualTo(expected));
    }


    [Test]
    public void VerifyToken_MatchesOnlyTheOriginalKey()
    {
        const string key = "AbC123+/xyzQWErty0987654321ABCDEFGHIJKLMNOP=";
        string hash = ApiKeyHelper.GetToken(key);
        Assume.That(hash, Is.Not.EqualTo(hash.ToLowerInvariant()), "fixture hash must contain upper-case characters");

        Assert.Multiple(() =>
        {
            Assert.That(ApiKeyHelper.VerifyToken(key, hash), Is.True);
            Assert.That(ApiKeyHelper.VerifyToken(key + "x", hash), Is.False);
            Assert.That(ApiKeyHelper.VerifyToken(key, hash.ToLowerInvariant()), Is.False, "stored hash comparison must be case-sensitive");
        });
    }
}
