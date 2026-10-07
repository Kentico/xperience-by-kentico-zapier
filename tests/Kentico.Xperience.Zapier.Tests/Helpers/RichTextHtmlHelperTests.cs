using Kentico.Xperience.Zapier.Helpers;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Helpers;

[TestFixture]
public class RichTextHtmlHelperTests
{
    [Test]
    public void EnsureValidHtmlValue_WrapsPlainTextLinesInParagraphs()
    {
        string result = RichTextHtmlHelper.EnsureValidHtmlValue("first line\nsecond line");

        Assert.That(result, Is.EqualTo("<p>first line</p><p>second line</p>"));
    }


    [Test]
    public void EnsureValidHtmlValue_LeavesExistingHtmlUntouched()
    {
        const string html = "<p>Hello</p>\n<ul><li>item</li></ul>";

        Assert.That(RichTextHtmlHelper.EnsureValidHtmlValue(html), Is.EqualTo(html));
    }
}
