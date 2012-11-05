namespace Flatliner.Portable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Common string parsing and manipulation functions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// A non-definitive list of words that should not have their first letter capitalized in a title.
        /// </summary>
        public static readonly string[] NonTitleCaseWords = new[]
            {
                "a", "in", "the", "if", "be", "of", "on", "is", "but", "and", "my", "as", "are", "has", "was", "de", "or", "it"
            };

        /// <summary>
        /// A list of strings that are used to determine the start or end of words.
        /// </summary>
        public static readonly string[] WordSeparators = new[]
            {
                " ", ",", ".", "\t", "\r", "\n", "!", "?"
            };

        /// <summary>
        /// A list of punctuation marks to remove from the start and end of words.
        /// </summary>
        public static readonly char[] CleanWordsOfCharacters = new[]
            {
                ' ', ',', '.', '\t', '\r', '\n', '-', '!', '?', '*', '&'
            };

        /// <summary>
        /// Enumerates all the words from a string, cleaning the words of any 
        /// trailing or leading whitespace or punctuation as it goes.
        /// NOTE: This is not safe to be used with cultures other than English.
        /// </summary>
        /// <param name="text">The text to extract the words from.</param>
        /// <returns>Returns an enumerable sequence of strings, one per word.</returns>
        public static IEnumerable<string> Words(this string text)
        {
            return text.Words(true);
        }

        /// <summary>
        /// Enumerates all the words from a string, optionally cleaning the words 
        /// of any trailing or leading whitespace or punctuation as it goes.
        /// NOTE: This is not safe to be used with cultures other than English.
        /// </summary>
        /// <param name="text">The text to extract the words from.</param>
        /// <param name="cleanWords">A value indicating whether each words should be cleaned of any trailing or leading whitespace or punctuation.</param>
        /// <returns>Returns an enumerable sequence of strings, one per word.</returns>
        public static IEnumerable<string> Words(this string text, bool cleanWords)
        {
            if (text == null)
            {
                return Enumerable.Empty<string>();
            }

            IEnumerable<string> result = text.SplitAfter(WordSeparators);
            if (cleanWords)
            {
                result = result.Select(
                    w => w.Trim(CleanWordsOfCharacters)).Where(s => !s.IsNullOrWhitespace());
            }

            return result;
        }

        /// <summary>
        /// Gets a value indicating whether the specified <see cref="String"/> occurs within this string.
        /// </summary>
        /// <param name="text">The string to search in.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="comparisonType">A parameter that specifies the type of search to perform for the specified string.</param>
        /// <returns>Returns a value indicating whether the specified <see cref="String"/> occurs within this string.</returns>
        public static bool Contains(this string text, string value, StringComparison comparisonType)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return text.IndexOf(value, comparisonType) != -1;
        }

        /// <summary>
        /// Enumerates all the sentences from a string, cleaning the sentences of any 
        /// trailing or leading whitespace or punctuation (other than the trailing period) as it goes.
        /// Excludes any null or whitespace sentences.
        /// NOTE: This is not safe to be used with cultures other than English.
        /// </summary>
        /// <param name="text">The text to extract the sentences from.</param>
        /// <returns>Returns an enumerable sequence of strings, one per sentence.</returns>
        public static IEnumerable<string> Sentences(this string text)
        {
            if (text == null)
            {
                return Enumerable.Empty<string>();
            }

            return from rawSentence in text.SplitAfter(".")
                   let cleanedSentence = rawSentence.Trim(' ', '\t', '\r', '\n')
                   where !cleanedSentence.IsNullOrWhitespace()
                   select cleanedSentence;
        }

        /// <summary>
        /// Performs a non-consuming split of a string after each occurance of 
        /// any of the specified delimiters. Each string will keep it's delimiter 
        /// at the end of it. This overload performs a case-sensitive search.
        /// </summary>
        /// <param name="text">The text to be split.</param>
        /// <param name="delimiters">The list of delimiters to split by.</param>
        /// <returns>Returns an enumerable sequence of strings.</returns>
        public static IEnumerable<string> SplitAfter(this string text, params string[] delimiters)
        {
            return text.SplitAfter(StringComparison.Ordinal, delimiters);
        }

        /// <summary>
        /// Performs a non-consuming split of a string after each occurance of 
        /// any of the specified delimiters. Each string will keep it's delimiter 
        /// at the end of it.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <param name="comparisonType">The string comparison to perform on the delimiters, 
        /// this allows for case insensitive splits etc.</param>
        /// <param name="delimiters">The list of delimiters to split by.</param>
        /// <returns>Returns an enumerable sequence of strings.</returns>
        public static IEnumerable<string> SplitAfter(this string text, StringComparison comparisonType, params string[] delimiters)
        {
            int lastIndex = 0;
            while (lastIndex != -1)
            {
                var index = lastIndex;
                var nextIndex = (from delim in delimiters
                                 let delimIndex = text.IndexOf(delim, index, comparisonType)
                                 where delimIndex != -1
                                 select delimIndex + delim.Length).DefaultIfEmpty(-1).Min();
                if (nextIndex != -1)
                {
                    yield return text.Substring(lastIndex, nextIndex - lastIndex);
                }
                else
                {
                    yield return text.Substring(lastIndex);
                }

                lastIndex = nextIndex;
            }
        }

        /// <summary>
        /// Performs a non-consuming split of a string before each occurance of 
        /// any of the specified delimiters. Each string will keep it's delimiter 
        /// at the end of it.  This overload performs a case-sensitive search.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <param name="delimiters">The list of delimiters to split by.</param>
        /// <returns>Returns an enumerable sequence of strings.</returns>
        public static IEnumerable<string> SplitBefore(this string text, params string[] delimiters)
        {
            return text.SplitBefore(StringComparison.Ordinal, delimiters);
        }

        /// <summary>
        /// Performs a non-consuming split of a string before each occurance of 
        /// any of the specified delimiters. Each string will keep it's delimiter 
        /// at the end of it.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <param name="comparisonType">The string comparison to perform on the delimiters, 
        /// this allows for case insensitive splits etc.</param>
        /// <param name="delimiters">The list of delimiters to split by.</param>
        /// <returns>Returns an enumerable sequence of strings.</returns>
        public static IEnumerable<string> SplitBefore(this string text, StringComparison comparisonType, params string[] delimiters)
        {
            int lastIndex = 0;
            int lastSearchIndex = 0;
            while (lastIndex != -1)
            {
                var index = lastSearchIndex;
                var nextIndex = (from delim in delimiters
                                 let delimIndex = text.IndexOf(delim, index, comparisonType)
                                 where delimIndex != -1
                                 orderby delimIndex
                                 select new { Index = delimIndex, Length = delim.Length }).DefaultIfEmpty(new { Index = -1, Length = 0}).First();

                if (nextIndex.Index != -1)
                {
                    yield return text.Substring(lastIndex, nextIndex.Index - lastIndex);
                    lastIndex = nextIndex.Index;
                    lastSearchIndex = nextIndex.Index + nextIndex.Length;
                }
                else
                {
                    yield return text.Substring(lastIndex);
                    yield break;
                }
            }
        }

        /// <summary>
        /// Extension shorthand for static method <see cref="string.IsNullOrWhiteSpace"/>.
        /// </summary>
        /// <param name="text">The text to check for null or whitespace.</param>
        /// <returns>Returns true if null, empty or whitespace, otherwise false.</returns>
        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        /// <summary>
        /// Extension shorthand for static method <see cref="string.IsNullOrWhiteSpace"/>.
        /// </summary>
        /// <param name="text">The text to check for null or whitespace.</param>
        /// <returns>Returns true if null, empty or whitespace, otherwise false.</returns>
        public static bool IsNullOrWhitespace(this string text)
        {
            return string.IsNullOrEmpty(text) || text.Trim().Length == 0;
        }

        /// <summary>
        /// Counts the number of spaces in the given text, ignoring double spacings.
        /// </summary>
        /// <param name="text">The text to count the number of words in.</param>
        /// <returns>If the text is not null returns the number of non-empty words, otherwise returns zero.</returns>
        [Obsolete("Use Words() and Count() extension method instead, which allows you to exclude small words etc.", false)]
        public static int WordCount(this string text)
        {
            if (text == null)
            {
                return 0;
            }

            return text.Words().Count();
        }

        /// <summary>
        /// Counts the number sentences by counting the number of full-stops (.) in the given text. Does not count empty sentences.
        /// </summary>
        /// <param name="text">The text to count the number of sentences in.</param>
        /// <returns>If the text is not null, returns the number of full-stops. Otherwise returns zero.</returns>
        [Obsolete("Use Sentences() and Count() extension method, which allows you to exclude small sentences etc.", false)]
        public static int SentenceCount(this string text)
        {
            if (text == null)
            {
                return 0;
            }

            return text.Sentences().Count();
        }

        /// <summary>
        /// Converts text to sentence case (uppercases the first letter of the first word).
        /// NOTE: This is not safe to be used with cultures other than English.
        /// </summary>
        /// <param name="text">Text to be sentence cased.</param>
        /// <returns>If text is not null, returns the input text with the first letter uppercased, otherwise returns null.</returns>
        public static string SentenceCase(this string text)
        {
            if (text == null)
            {
                return null;
            }

            if (text.Length == 0)
            {
                return string.Empty;
            }

            var sentences = from sentence in text.Sentences()
                            let casedSentence =
                                sentence.Substring(0, 1).ToUpper() + sentence.Substring(1, sentence.Length - 1)
                            select casedSentence;

            return sentences.Join(". ");
        }

        /// <summary>
        /// Converts the casing of lower case text into a title case. 
        /// Does not capitalize small reserved words such as 'in', 'if', 'is' etc.
        /// NOTE: This is not safe to be used with cultures other than English.
        /// </summary>
        /// <param name="text">The lower case text to convert to title case.</param>
        /// <returns>Returns a string with most words capitalized.</returns>
        public static string TitleCase(this string text)
        {
            if (text == null)
            {
                return null;
            }

            if (text.Length == 0)
            {
                return string.Empty;
            }

            var words = text.Words(false);
            var resultWords = (from word in words.Take(1)
                               select word.Substring(0, 1).ToUpper() + word.Substring(1, word.Length - 1)).Concat(
                        from word in words.Skip(1)
                        let cleanedWord = word.Trim(CleanWordsOfCharacters)
                        let casedWord =
                            NonTitleCaseWords.Contains(cleanedWord)
                                ? word.ToLower()
                                : word.Length == 0 ? string.Empty :
                                word.Substring(0, 1).ToUpper() + word.Substring(1, word.Length - 1)
                        select casedWord);
            return resultWords.Concat();
        }

        /// <summary>
        /// Concatenates all of the strings in a enumerable sequence into a single string. Excludes any null strings from the resulting string.
        /// </summary>
        /// <param name="items">The enumerable sequence of strings to concatenate, </param>
        /// <returns>Returns a non null string if the enumerable sequence itself is not null, otherwise returns null.</returns>
        public static string Concat(this IEnumerable<string> items)
        {
            if (items == null)
            {
                return null;
            }

            var sb = new StringBuilder();
            foreach (var item in items)
            {
                if (item != null)
                {
                    sb.Append(item);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Joins a sequence of strings with a separator (Enumerable equivalent of a call to string.Join() ) 
        /// </summary>
        /// <param name="items">The strings to join together with a separator between each</param>
        /// <param name="separator">The separator to be placed between them</param>
        /// <returns>A single string with all the passed in strings joined, separated by the separator</returns>
        public static string Join(this IEnumerable<string> items, string separator)
        {
            if (separator == null)
            {
                throw new ArgumentNullException("separator");
            }

            if (items == null)
            {
                return null;
            }

            bool first = true;
            var sb = new StringBuilder();
            foreach (var item in items)
            {
                if (item != null)
                {
                    if (!first)
                    {
                        sb.Append(separator);
                    }
                    else
                    {
                        first = false;
                    }

                    sb.Append(item);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Joins a sequence of strings with a separator (Enumerable equivalent of a call to string.Join() ) 
        /// </summary>
        /// <typeparam name="T">The type of items to join as a list of strings</typeparam>
        /// <param name="items">The strings to join together with a separator between each</param>
        /// <param name="selector">The function to use to extract the string to be joined.</param>
        /// <param name="separator">The separator to be placed between them</param>
        /// <returns>A single string with all the passed in strings joined, separated by the separator</returns>
        public static string Join<T>(this IEnumerable<T> items, Func<T, string> selector, string separator)
        {
            if (separator == null)
            {
                throw new ArgumentNullException("separator");
            }

            if (items == null)
            {
                return null;
            }

            return items.Select(selector).Join(separator);
        }

        /// <summary>
        /// Removes the start of the text if it equals the specified text to be trimmed
        /// </summary>
        /// <param name="text">The text to be trimmed</param>
        /// <param name="trimText">The text to remove from the start</param>
        /// <returns>If the text is not null then returns the text without the 
        /// trimmed text at the start, otherwise returns null</returns>
        public static string TrimStart(this string text, string trimText)
        {
            if (text == null)
            {
                return null;
            }

            return text.StartsWith(trimText) ? text.Substring(trimText.Length) : text;
        }

        /// <summary>
        /// Removes the start of the text if it equals the specified text to be trimmed
        /// </summary>
        /// <param name="text">The text to be trimmed</param>
        /// <param name="trimText">The text to remove from the start</param>
        /// <param name="comparisonType">The type of string comparison to perform.</param>
        /// <returns>If the text is not null then returns the text without the 
        /// trimmed text at the start, otherwise returns null</returns>
        public static string TrimStart(this string text, string trimText, StringComparison comparisonType)
        {
            if (text == null)
            {
                return null;
            }

            return text.StartsWith(trimText, comparisonType) ? text.Substring(trimText.Length) : text;
        }

        /// <summary>
        /// Removes the end of the text if it equals the specified text to be trimmed
        /// </summary>
        /// <param name="text">The text to be trimmed</param>
        /// <param name="trimText">The text to remove from the end</param>
        /// <returns>If the text is not null then returns the text without the 
        /// trimmed text at the end, otherwise returns null</returns>
        public static string TrimEnd(this string text, string trimText)
        {
            if (text == null)
            {
                return null;
            }

            return text.EndsWith(trimText) ? text.Remove(text.Length - trimText.Length) : text;
        }

        /// <summary>
        /// Removes the end of the text if it equals the specified text to be trimmed
        /// </summary>
        /// <param name="text">The text to be trimmed</param>
        /// <param name="trimText">The text to remove from the end</param>
        /// <param name="comparisonType">The type of string comparison to perform.</param>
        /// <returns>If the text is not null then returns the text without the 
        /// trimmed text at the end, otherwise returns null</returns>
        public static string TrimEnd(this string text, string trimText, StringComparison comparisonType)
        {
            if (text == null)
            {
                return null;
            }

            return text.EndsWith(trimText, comparisonType) ? text.Remove(text.Length - trimText.Length) : text;
        }

        /// <summary>
        /// Gets the part of the string leading up to the last occurance of the specified delimiter 
        /// (does not include the last delimiter in the result).
        /// </summary>
        /// <param name="text">The text to get the substring of.</param>
        /// <param name="delimiter">The delimiter to search for the last occurance of.</param>
        /// <returns>Returns the part of the text before the last occurance of the delimiter if present, 
        /// otherwise returns all of the text, 
        /// if text is null then returns null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if delimiter is null.</exception>
        public static string SubstringToLast(this string text, string delimiter)
        {
            return SubstringToLast(text, delimiter, StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the part of the string leading up to the last occurance of the specified delimiter 
        /// (does not include the last delimiter in the result).
        /// </summary>
        /// <param name="text">The text to get the substring of.</param>
        /// <param name="delimiter">The delimiter to search for the last occurance of.</param>
        /// <param name="comparison">The kind of string comparison to perform on the delimeter against the text</param>
        /// <returns>Returns the part of the text before the last occurance of the delimiter if present, 
        /// otherwise returns all of the text, 
        /// if text is null then returns null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if delimiter is null.</exception>
        public static string SubstringToLast(this string text, string delimiter, StringComparison comparison)
        {
            if (text == null)
            {
                return null;
            }

            if (delimiter == null)
            {
                throw new ArgumentNullException("delimiter");
            }

            var lastIndex = text.LastIndexOf(delimiter, comparison);
            return lastIndex == -1 ? text : text.Substring(0, lastIndex);
        }

        /// <summary>
        /// Gets the part of the string starting from the last occurance of the 
        /// specified delimiter excluding the delimeter from the result.
        /// This overload performs a case-sensitive match.
        /// </summary>
        /// <param name="text">The text to get the substring of.</param>
        /// <param name="delimiter">The delimiter to search for the last occurance of.</param>
        /// <returns>Returns the part of the text before the last occurance of the delimiter if present, 
        /// otherwise returns all of the text, 
        /// if text is null then returns null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if delimiter is null.</exception>
        public static string SubstringFromLast(this string text, string delimiter)
        {
            return SubstringFromLast(text, delimiter, StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the part of the string starting from the last occurance of the 
        /// specified delimiter excluding the delimeter from the result.
        /// </summary>
        /// <param name="text">The text to get the substring of.</param>
        /// <param name="delimiter">The delimiter to search for the last occurance of.</param>
        /// <param name="comparison">The kind of string comparison to perform on the delimeter against the text</param>
        /// <returns>Returns the part of the text before the last occurance of the delimiter if present, 
        /// otherwise returns all of the text, 
        /// if text is null then returns null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if delimiter is null.</exception>
        public static string SubstringFromLast(this string text, string delimiter, StringComparison comparison)
        {
            if (text == null)
            {
                return null;
            }

            if (delimiter == null)
            {
                throw new ArgumentNullException("delimiter");
            }

            var lastIndex = text.LastIndexOf(delimiter, comparison);
            return lastIndex == -1 ? text : lastIndex >= text.Length ? text : text.Substring(lastIndex + delimiter.Length);
        }

        /// <summary>
        /// Gets the part of the string leading up to the first occurance of the 
        /// specified delimiter excluding the delimiter from the result.
        /// This overload performs a case sensitive match.
        /// </summary>
        /// <param name="text">The text to get the substring of.</param>
        /// <param name="delimiter">The delimiter to search for the last occurance of.</param>
        /// <returns>Returns the part of the text before the last occurance of the delimiter if present, 
        /// otherwise returns all of the text, 
        /// if text is null then returns null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if delimiter is null.</exception>
        public static string SubstringToFirst(this string text, string delimiter)
        {
            return SubstringToFirst(text, delimiter, StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the part of the string leading up to the first occurance of the 
        /// specified delimiter excluding the delimiter from the result.
        /// </summary>
        /// <param name="text">The text to get the substring of.</param>
        /// <param name="delimiter">The delimiter to search for the last occurance of.</param>
        /// <param name="comparison">The kind of string comparison to perform on the delimeter against the text</param>
        /// <returns>Returns the part of the text before the last occurance of the delimiter if present, 
        /// otherwise returns all of the text, 
        /// if text is null then returns null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if delimiter is null.</exception>
        public static string SubstringToFirst(this string text, string delimiter, StringComparison comparison)
        {
            if (text == null)
            {
                return null;
            }

            if (delimiter == null)
            {
                throw new ArgumentNullException("delimiter");
            }

            var firstIndex = text.IndexOf(delimiter, comparison);
            return firstIndex == -1 ? text : text.Substring(0, firstIndex);
        }

        /// <summary>
        /// Gets the part of the string starting from the first occurance of 
        /// the specified delimiter excluding the delimiter from the result.
        /// This overload performs a case sensitive match.
        /// </summary>
        /// <param name="text">The text to get the substring of.</param>
        /// <param name="delimiter">The delimiter to search for the last occurance of.</param>
        /// <returns>Returns the part of the text before the last occurance of the delimiter if present, 
        /// otherwise returns all of the text, 
        /// if text is null then returns null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if delimiter is null.</exception>
        public static string SubstringFromFirst(this string text, string delimiter)
        {
            return SubstringFromFirst(text, delimiter, StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the part of the string starting from the first occurance of 
        /// the specified delimiter excluding the delimiter from the result.
        /// </summary>
        /// <param name="text">The text to get the substring of.</param>
        /// <param name="delimiter">The delimiter to search for the last occurance of.</param>
        /// <param name="comparison">The kind of string comparison to perform on the delimeter against the text</param>
        /// <returns>Returns the part of the text before the last occurance of the delimiter if present, 
        /// otherwise returns all of the text, 
        /// if text is null then returns null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if delimiter is null.</exception>
        public static string SubstringFromFirst(this string text, string delimiter, StringComparison comparison)
        {
            if (text == null)
            {
                return null;
            }

            if (delimiter == null)
            {
                throw new ArgumentNullException("delimiter");
            }

            var firstIndex = text.IndexOf(delimiter, comparison);
            return firstIndex == -1 || firstIndex >= text.Length ? text : text.Substring(firstIndex + 1);
        }
    }
}
