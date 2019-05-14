using BizObjects;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LineParserTests
{
    [TestClass]
    public class ChatParserTests
    {
        private ChatParser _parser = new ChatParser(new YouResolver("Khadaji"));

        [DataTestMethod]
        [DataRow("[Tue May 27 17:26:54 2003] You say to your guild, 'hey all'", "Khadaji", "Guild", "hey all")]
        [DataRow("[Tue May 27 21:05:28 2003] You say, 'Hail, a shadow steed'", "Khadaji", "Say", "Hail, a shadow steed")]
        [DataRow("[Sat Mar 30 08:40:20 2019] You tell your party, 'Pulling a cliknar soldier'", "Khadaji", "Group", "Pulling a cliknar soldier")]
        [DataRow("[Tue May 27 17:33:42 2003] You tell monk:4, 'hey monks'", "Khadaji", "monk", "hey monks")]
        [DataRow("[Tue May 27 19:05:09 2003] You tell your raid, 'VINDI INC'", "Khadaji", "Raid", "VINDI INC")]
        [DataRow("[Thu May 22 19:55:48 2003] You told saxstein, 'I'm too too late? any point in running over there?'", "Khadaji", "saxstein", "I'm too too late? any point in running over there?")]

        [DataRow("[Sat Mar 30 08:39:56 2019] Movanna tells the group, 'Casting Zealous Light on Khadaji.'", "Movanna", "Group", "Casting Zealous Light on Khadaji.")]
        [DataRow("[Sat Mar 30 08:43:21 2019] Kahlel tells General:1, 'lol'", "Kahlel", "General", "lol")]
        [DataRow("[Sat May 17 15:28:12 2003] Tulwain tells the guild, 'There are 3 different camps of them, I believe.'", "Tulwain", "Guild", "There are 3 different camps of them, I believe.")]
        [DataRow("[Sat May 17 15:28:26 2003] Azare tells the group, 'Slowing A Razorfiend Subduer'", "Azare", "Group", "Slowing A Razorfiend Subduer")]
        [DataRow("[Tue May 27 17:26:45 2003] Jemaia tells monk:4, 'there were 22 ppl in maiden's....wtf'", "Jemaia", "monk", "there were 22 ppl in maiden's....wtf")]
        [DataRow("[Tue May 27 17:34:09 2003] Aberfoyle auctions, 'WTS Flamesong send tell with offer'", "Aberfoyle", "Auction", "WTS Flamesong send tell with offer")]
        [DataRow("[Tue May 27 17:38:01 2003] Nirvadian shouts, 'does anyone have the \"Cease\" Spell book forsale?'", "Nirvadian", "Shout", "does anyone have the \"Cease\" Spell book forsale?")]
        [DataRow("[Tue May 27 17:38:02 2003] Rhinael says out of character, 'Nobody \"has\" to have it... sorry'", "Rhinael", "OOC", "Nobody \"has\" to have it... sorry")]
        [DataRow("[Tue May 27 17:42:40 2003] Banker Tawler tells you, 'Welcome to my bank!'", "Banker Tawler", "Tell", "Welcome to my bank!")]
        [DataRow("[Thu May 22 19:55:54 2003] Saxstein tells you, 'Yes, too late'", "Saxstein", "Tell", "Yes, too late")]
        [DataRow("[Tue May 27 18:30:13 2003] Saxstein tells the raid,  'MGB Virtue inc'", "Saxstein", "Raid", "MGB Virtue inc")]
        [DataRow("[Fri May 16 19:39:52 2003] Ganzo says, 'look under your feet :P'", "Ganzo", "Say", "look under your feet :P")]
        [DataRow("[Fri May 16 20:26:41 2003] Layend says out of character, '56 druid and 57 monk looking for more or a group to join'", "Layend", "OOC", "56 druid and 57 monk looking for more or a group to join")]
        [DataRow("[Fri Apr 26 09:25:50 2019] A cliknar adept says, 'Such is the fate of all who stand in our path!'", "a cliknar adept", "Say", "Such is the fate of all who stand in our path!")]
        // [DataRow("something with an apostrophe in it", "wwwww", "ccccc", "zzzzz")]
        // [DataRow("somethign with a backtick", "wwwww", "ccccc", "zzzzz")]
        // [DataRow("xxxxxx", "wwwww", "ccccc", "zzzzz")]

		// Tricky one ... there is no comma before the text body
        // [DataRow("[Fri May 16 20:23:45 2003] Sontalak says 'Ack! I must be careful not to step on that body, it tastes much better when it is still crunchy, not pulped!'", "wwwww", "ccccc", "zzzzz")]
        public void ChatTests(string logLine, string whoName, string channel, string text)
        {
            var logDatum = new LogDatum(logLine);

            var result = _parser.TryParse(logDatum, out ILine lineEntry);

            Assert.IsTrue(result);
            Assert.IsTrue(lineEntry is Chat);
            var entry = lineEntry as Chat;
            Assert.AreEqual(whoName, entry.Who.Name);
            Assert.AreEqual(channel, entry.Channel);
            Assert.AreEqual(text, entry.Text);
        }
    }
}
