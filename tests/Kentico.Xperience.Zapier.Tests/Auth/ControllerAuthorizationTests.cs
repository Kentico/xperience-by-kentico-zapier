using System.Reflection;

using Kentico.Xperience.Zapier.Auth;

using Microsoft.AspNetCore.Mvc;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Auth;

[TestFixture]
public class ControllerAuthorizationTests
{
    [Test]
    public void EveryControllerRequiresTheZapierApiKey()
    {
        var controllers = typeof(ZapierServiceCollectionExtensions).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t))
            .ToList();

        Assume.That(controllers, Is.Not.Empty);

        var unprotected = controllers
            .Where(t => t.GetCustomAttribute<AuthorizeZapierAttribute>(inherit: true) is null)
            .Select(t => t.Name)
            .ToList();

        Assert.That(unprotected, Is.Empty, "every Zapier endpoint must be behind the API key scheme");
    }
}
