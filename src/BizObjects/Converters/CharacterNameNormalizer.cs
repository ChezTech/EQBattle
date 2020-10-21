using System;
using System.Collections.Generic;
using Core.Extensions;

namespace BizObjects.Converters
{
    public class CharacterNameNormalizer
    {
        private static HashSet<string> IgnoreWords = new HashSet<string>()
        {
            "Of",
            "The",
            "A",
            "An",
            "of",
            "the",
            "a",
            "an",
            "pet",
            "warder"
        };

        private static Func<string, string> nameNormalizer = name => NormalizeNameInternal(name);
        private static Func<string, string> memoizedNormalization = nameNormalizer.Memoize();

        public string NormalizeName(string name)
        {
            return memoizedNormalization(name);
        }

        private static string NormalizeNameInternal(string name)
        {
            string normalizedName = CleanName(name);
            normalizedName = RemoveCapitalizedNameDueToSentence(normalizedName);
            return normalizedName;
        }

        private static string CleanName(string name)
        {
            return name
                .Replace("A ", "a ") // Will this get only the "A monster type" at the beginning? Could use RegEx.Replace ....
                .Replace("An ", "an ")

                // Note: this is for an apostrophe corpse ... there are actual mobs that are corpses. Those names use a backtick, e.g. "a mercenary`s corpse".
                // We don't want to remove that term from their name as it is part of their proper name, not a state of being.
                .Replace("'s corpse", "") // dead mobs can still have a DoT in effect, "You have taken 1960 damage from Nature's Searing Wrath by a cliknar sporali farmer's corpse."

                .Replace("'s", "") // apostrophe: Can we replace this by a better Regex? (E.g. "... pierced by a monster's thorns...")

                ;
        }

        // Some names are capitalized due to being at the start of a sentence. For mobs named with the indefinite article ("a", "an") that's pretty easy to deal with.
        // For generic monster names without an indefinite ariticle, it's harder to distinugish them from a named mob.
        // E.g. "Molten steel"
        // What we'll do is look for a lowercase to the rest of their name (excluding "of", "the") and if we find it, we'll lowercase their first word too.
        private static string RemoveCapitalizedNameDueToSentence(string name)
        {
            var words = name.Split(' '); // Don't commit, just for debug so we can ensure an exception correctly stops processing of log file.

            bool shouldLower = false;

            // Skip the first word, since that's the one we'll lowercase if any other words are lower
            for (int i = 1; i < words.Length; i++)
            {
                var word = words[i];

                if (IgnoreWords.Contains(word))
                    continue;

                if (char.IsLower(word[0]))
                    shouldLower = true;
            }

            return shouldLower
                ? char.ToLower(name[0]) + name.Substring(1)
                : name;
        }

        #region Singleton

        // Jon Skeet: https://csharpindepth.com/articles/singleton

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        static CharacterNameNormalizer() { }

        private CharacterNameNormalizer() { }

        public static CharacterNameNormalizer Instance { get; } = new CharacterNameNormalizer();

        #endregion
    }
}
