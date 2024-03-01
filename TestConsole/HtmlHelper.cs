using System.Net;
using System.Text.RegularExpressions;

namespace System.ComponentModel.DataAnnotations;

internal static partial class HtmlHelper
{
    private static readonly Regex s_templateRegex = TemplateRegex();
    private static readonly Regex s_htmlTagRegex = HtmlTagRegex();
    private static readonly Regex s_htmlTagWhiteSpaceRegex = HtmlTagWhiteSpaceRegex();

    private const string c_breakTag = "br";
    private static readonly HashSet<string> s_blockTags = new()
    {
        "address", "article", "aside", "blockquote", "canvas", "dd", "div", "dl", "dt", "fieldset",
        "figcaption", "figure", "footer", "form", "h1>-<h6", "header", "hr", "li", "main", "nav",
        "noscript", "ol", "p", "pre", "section", "table", "tfoot", "ul", "video"
    };

    public static string GetPlainText(string html)
    {
        var text = html;

        // Decode html specific characters
        text = WebUtility.HtmlDecode(text);

        // Remove tag whitespace/line breaks
        text = s_htmlTagWhiteSpaceRegex.Replace(text, "><");

        // Find all tags
        var tags = s_htmlTagRegex.Matches(text).ToList();

        // Go reverse since we're replacing one-by-one
        for (var i = tags.Count - 1; i >= 0; i--)
        {
            var match = tags[i];

            // Handle block tags or break tags by adding new line
            if (match.Index > 0 && match.Groups.Count >= 1)
            {
                var tag = match.Groups[1].Value.Trim().ToLower();
                if (s_blockTags.Contains(tag) || tag == c_breakTag)
                {
                    text = string.Concat(text.AsSpan(0, match.Index), "\n", text.AsSpan(match.Index + match.Length));
                    continue;
                }
            }

            // Otherwise, just strip the tag
            text = string.Concat(text.AsSpan(0, match.Index), text.AsSpan(match.Index + match.Length));
        }

        return text;
    }

    public static bool TryFormatHtml(string? html, [NotNullWhen(true)] out string? formatted)
    {
        formatted = null;

        if (html is null)
            return false;

        formatted = html;

        // Replace any template variables with supported placeholders
        var matches = s_templateRegex.Matches(html);
        for (var i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            if (match.Groups.Count <= 1)
                return false;

            var templateVar = match.Groups[1].Value;
            var placeholder = GetPlaceholderFromTemplate(templateVar);
            if (placeholder is null)
                return false;

            formatted = string.Concat(formatted.AsSpan(0, match.Index), placeholder, formatted.AsSpan(match.Index + match.Length));
        }

        return true;
    }

    private static string? GetPlaceholderFromTemplate(string templateName)
    {
        return templateName switch
        {
            "FirstName" => "<rh-placeholder name=\"firstName\">First name</rh-placeholder>",
            _ => null,
        };
    }

    // Matches: {{Foo}}
    [GeneratedRegex("{{(\\w+)}}")]
    private static partial Regex TemplateRegex();

    // Match any character between '<' and '>', even when end tag is missing
    [GeneratedRegex(@"<([^>]*)(/?>|$)", RegexOptions.Multiline)]
    private static partial Regex HtmlTagRegex();

    // Matches one or more (white space or line breaks) between '>' and '<'
    [GeneratedRegex(@"(>|$)(\W|\n|\r)+<", RegexOptions.Multiline)]
    private static partial Regex HtmlTagWhiteSpaceRegex();
}
