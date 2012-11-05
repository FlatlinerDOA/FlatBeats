namespace Flatliner.Phone.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class Censorship
    {
        private static readonly List<string> BadWords = new List<string>
            {
                "aryan",
                "ass",
                "asshole",
                "anus",
                "bastard",
                "bastards",
                "bitch",
                "bitches",
                "bitching",
                "bitchy",
                "boob",
                "boobie",
                "booby",
                "boobs",
                "boobies",
                "boobys",
                "bollock",
                "bollocks",
                "bullshit",
                "bullshitter",
                "bullshitters",
                "bullshitting",
                "bugger",
                "buggerize",
                "chickenshit",
                "chickenshits",
                "clit",
                "cock",
                "cockhead",
                "cocks",
                "cocksuck",
                "cocksucker",
                "cocksucking",
                "cum",
                "cums",
                "cumming",
                "cunt",
                "cuntree",
                "cuntry",
                "cunts",
                "dipshit",
                "dipshits",
                "dick", 
                "dicks", 
                "dumbfuck",
                "dumbfucks",
                "dumbshit",
                "dumbshits",
                "fag",
                "fags",
                "faggy",
                "faggot",
                "faggots",
                "fuck",
                "fucks",
                "fukk",
                "fukka",
                "fuk",
                "fucka",
                "fucke",
                "fucker",
                "fuckers",
                "fucked",
                "fuckin",
                "fucken",
                "fucking",
                "fuckup",
                "fuckups",
                "fuckhead",
                "fuckhed",
                "fuckheads",
                "fuckface",
                "golem",
                "goniff",
                "hebe",
                "hebes",
                "heb",
                "hoar",
                "hoes", 
                "kike",
                "kikes",
                "kunt",
                "kuntree",
                "kuntry",
                "kunts",
                "motherfuck",
                "motherfucker",
                "motherfuckers",
                "motherfucking",
                "motherfuckin",
                "motherfucken",
                "nazi",
                "nigger",
                "niggers",
                "nigga",
                "niggas",
                "niggaz",
                "niggah",
                "niggahs",
                "niggard",
                "niggardly",
                "orgazm",
                "orgasm",
                "orgasmic",
                "orgazmic",
                "penis",
                "penises",
                "piss",
                "porn",
                "porno",
                "pornography",
                "pussy",
                "pussies",
                "prick",
                "schlimiel",
                "schlimazel",
                "shit",
                "shits",
                "shitty",
                "shitting",
                "shitface",
                "shitfaced",
                "shithead",
                "shithed",
                "shitheads",
                "slut",
                "sluts",
                "slutty",
                "snatch",
                "snatches",
                "tit",
                "tits",
                "titty",
                "titties",
                "vagina",
                "vaginal",
                "whore",
                "whores",
                "whoring",
                "whoar", 
            };

        private static readonly List<string> BadPartials = new List<string>() { "cunt", "fuck", "shit" };

        private static readonly List<char> Vowels = new List<char>() { 'a', 'A', 'e','E', 'i', 'I', 'o', 'O', 'u', 'U' };

        private const char CensorChar = '*';

        public static string Censor(string text)
        {
            var sb = new StringBuilder(text);
            foreach (var censor in BadWords)
            {
                sb.CensorWord(text, censor, false, StringComparison.OrdinalIgnoreCase);
            }

            foreach (var censor in BadPartials)
            {
                sb.CensorWord(text, censor, true, StringComparison.OrdinalIgnoreCase);
            }

            return sb.ToString();
        }

        public static string CensorWord(this string text, string word, StringComparison comparison)
        {
            var sb = new StringBuilder(text);
            sb.CensorWord(text, word, false, comparison);
            return sb.ToString();
        }

        private static void CensorWord(this StringBuilder sb, string text, string word, bool allowPartial, StringComparison comparison)
        {
            int position = 0;
            while (position != -1)
            {
                position = text.IndexOf(word, position, comparison);
                if (position == -1)
                {
                    return;
                }

                if (!allowPartial && (!IsWordBoundary(sb, position) || !IsWordBoundary(sb, position + word.Length - 1)))
                {
                    return;
                }

                for (int i = position + 1; i < Math.Min(sb.Length, position + word.Length - 1); i++)
                {
                    if (IsVowel(sb, i))
                    {
                        sb.Replace(sb[i], CensorChar, i, 1);
                    }
                }

                position += word.Length;
            }
        }

        private static bool IsWordBoundary(StringBuilder sb, int i)
        {
            if (i == 0 || i == sb.Length - 1)
            {
                return true;
            }

            if (char.IsPunctuation(sb[i - 1]) || char.IsWhiteSpace(sb[i - 1]))
            {
                return true;
            }

            if (char.IsPunctuation(sb[i + 1]) || char.IsWhiteSpace(sb[i + 1]))
            {
                return true;
            }

            return false;
        }

        private static bool IsVowel(StringBuilder sb, int i)
        {
            return Vowels.Contains(sb[i]) || char.IsDigit(sb[i]);
        }
    }
}
