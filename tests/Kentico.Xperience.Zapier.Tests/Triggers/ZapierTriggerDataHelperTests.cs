using CMS.Tests;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Triggers;

[TestFixture]
public class ZapierTriggerDataHelperTests : UnitTests
{
    [SetUp]
    public void SetUp() => Fake<ZapierTriggerInfo>();


    [Test]
    public void TozapierDictionary_ExportsEveryColumnIncludingUnsetOnes()
    {
        // Zapier field mappings rely on a stable payload shape, so columns without a value must still be present.
        var info = new ZapierTriggerInfo
        {
            ZapierTriggerID = 3,
            ZapierTriggerCodeName = "abc12345",
            ZapierTriggerObjectType = "BizForm.DancingGoatContactUs",
            ZapierTriggerObjectClassType = "Form",
            ZapierTriggerEventType = "Create",
            // ZapierTriggerZapierURL intentionally left unset
        };

        var dictionary = info.TozapierDictionary();

        Assert.That(dictionary, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dictionary!["ZapierTriggerID"], Is.EqualTo(3));
            Assert.That(dictionary["ZapierTriggerObjectType"], Is.EqualTo("BizForm.DancingGoatContactUs"));
            Assert.That(dictionary.ContainsKey("ZapierTriggerZapierURL"), Is.True);
            Assert.That(dictionary["ZapierTriggerZapierURL"], Is.Null);
        });
    }
}
