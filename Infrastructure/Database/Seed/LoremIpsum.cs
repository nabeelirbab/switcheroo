using System;
using System.Linq;
using System.Text;

namespace Infrastructure.Database.Seed
{
    public static class LoremIpsum
    {
        public static string GetWordsBetween(int minWords, int maxWords, int minSentences = 1, int maxSentences = 1)
        {
            var words = new[]{
        "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
        "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
        "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"
      };

            var rand = new Random();

            var numSentences = rand.Next(minSentences, maxSentences);
            var numWords = rand.Next(minWords, maxWords);

            var result = new StringBuilder();

            for (var s = 0; s < numSentences; s++)
            {
                for (var w = 0; w < numWords; w++)
                {
                    if (w > 0) { result.Append(" "); }
                    result.Append(words[rand.Next(words.Length)]);
                }

                result.Append(". ");
            }

            return result.ToString();
        }
    }
}