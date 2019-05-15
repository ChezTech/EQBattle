using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BizObjects;
using LineParser;
using LineParser.Parsers;
using LogObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizObjectsTests
{
    [TestClass]
    public class BattleTests
    {
        private static readonly YouResolver YouAre = new YouResolver("Khadaji");
        private LineParserFactory _parser = new LineParserFactory();
        private readonly IParser _hitParser = new HitParser(YouAre);
        private readonly IParser _missParser = new MissParser(YouAre);
        private readonly IParser _healParser = new HealParser(YouAre);
        private readonly IParser _killParser = new KillParser(YouAre);


        public BattleTests()
        {
            _parser.AddParser(_hitParser, null);
            _parser.AddParser(_missParser, null);
            _parser.AddParser(_healParser, null);
            _parser.AddParser(_killParser, null);
        }

        [TestMethod]
        public void SmallFight()
        {
            var pc = new Character(YouAre.Name);
            var battle = new Battle();

            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:42 2019] Khadaji hit a dwarf disciple for 2 points of magic damage by Distant Strike I.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:45 2019] A dwarf disciple is pierced by YOUR thorns for 60 points of non-melee damage.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:45 2019] A dwarf disciple punches YOU for 3241 points of damage.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:47 2019] A dwarf disciple tries to punch YOU, but YOU riposte!")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:49 2019] You kick a dwarf disciple for 3041 points of damage. (Strikethrough)")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:50 2019] You try to crush a dwarf disciple, but miss!")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:50 2019] Movanna healed you over time for 2335 hit points by Elixir of the Ardent.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:51 2019] Khadaji hit a dwarf disciple for 892 points of poison damage by Strike of Venom IV. (Critical)")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:52 2019] A dwarf disciple punches YOU for 865 points of damage.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:20 2019] Khadaji hit a dwarf disciple for 512 points of chromatic damage by Lynx Maw.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:21 2019] Khronick healed you over time for 3036 hit points by Healing Counterbias Effect. (Critical)")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:38 2019] Bealica hit a dwarf disciple for 11481 points of cold damage by Glacial Cascade.")));
            battle.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:57 2019] A dwarf disciple has been slain by Bealica!")));

            var chars = battle.Characters;
            Assert.AreEqual(5, chars.Count);
            Assert.IsTrue(chars.Any(x => x.Name == "Khadaji"));
            Assert.IsTrue(chars.Any(x => x.Name == "Movanna"));
            Assert.IsTrue(chars.Any(x => x.Name == "Khronick"));
            Assert.IsTrue(chars.Any(x => x.Name == "Bealica"));
            Assert.IsTrue(chars.Any(x => x.Name == "a dwarf disciple"));

        }

    }
}
