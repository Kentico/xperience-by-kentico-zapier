using Kentico.Xperience.Zapier.Triggers.Extensions;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Triggers;

[TestFixture]
public class ZapierTriggerExtensionsTests
{
    [Test]
    public void GetUniqueCodename_ReturnsEightCharacterHexFragment()
    {
        string codename = ZapierTriggerExtensions.GetUniqueCodename();

        Assert.That(codename, Does.Match("^[0-9a-f]{8}$"));
    }
}
