
namespace Flatliner.Phone.Data
{
    using System;
    using System.Text.RegularExpressions;

    public static class Html
    {
        private const string Pattern = @"<(.|\n)*?>";

        public static string ConvertToPlainText(string html)
        {
            // This pattern Matches everything found inside html tags;
            // (.|\n) - > Look for any character or a new line
            // *?  -> 0 or more occurences, and make a non-greedy search meaning
            // That the match will stop at the first available '>' it sees, and not at the last one
            // (if it stopped at the last one we could have overlooked 
            // nested HTML tags inside a bigger HTML tag..)
            // Thanks to Oisin and Hugh Brown for helping on this one...

            if (html == null)
            {
                return string.Empty;
            }

            var plainText = Regex.Replace(html, Pattern, string.Empty);
            return plainText.Replace(@"&amp;", "&").Replace(@"&nbsp;", " ").Replace(@"&quot;", "\"").Replace(@"&lt;", "<").Replace(@"&gt;", ">");
        }
    }
}
