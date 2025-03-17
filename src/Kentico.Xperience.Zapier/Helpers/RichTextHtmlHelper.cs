using System.Text.RegularExpressions;

namespace Kentico.Xperience.Zapier.Helpers;

internal static class RichTextHtmlHelper
{
    internal static string EnsureValidHtmlValue(string content)
    {
        if (!Regex.IsMatch(content, @"<\/?[^>]+>"))
        {
            string[] lines = content.Split('\n');
            string result = string.Join("", Array.ConvertAll(lines, line => $"<p>{line}</p>"));

            return result;
        }

        return content;
    }
}

