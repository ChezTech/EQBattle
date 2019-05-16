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
    public class FightStatisticsTests
    {
        private static readonly YouResolver YouAre = new YouResolver("Khadaji");
        private LineParserFactory _parser = new LineParserFactory();
        private readonly IParser _hitParser = new HitParser(YouAre);
        private readonly IParser _missParser = new MissParser(YouAre);
        private readonly IParser _healParser = new HealParser(YouAre);
        private readonly IParser _killParser = new KillParser(YouAre);


        public FightStatisticsTests()
        {
            _parser.AddParser(_hitParser, null);
            _parser.AddParser(_missParser, null);
            _parser.AddParser(_healParser, null);
            _parser.AddParser(_killParser, null);
        }

        [TestMethod]
        public void SmallFightBasicStats()
        {
            var fightStats = new FightStatistics();

            // Offensive LogLines only
            fightStats.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:42 2019] Khadaji hit a dwarf disciple for 2 points of magic damage by Distant Strike I.")));
            fightStats.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:45 2019] A dwarf disciple is pierced by YOUR thorns for 60 points of non-melee damage.")));
            fightStats.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:49 2019] You kick a dwarf disciple for 3041 points of damage. (Strikethrough)")));
            fightStats.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:50 2019] You try to crush a dwarf disciple, but miss!")));
            fightStats.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:16:51 2019] Khadaji hit a dwarf disciple for 892 points of poison damage by Strike of Venom IV. (Critical)")));
            fightStats.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Fri Apr 05 16:17:20 2019] Khadaji hit a dwarf disciple for 512 points of chromatic damage by Lynx Maw.")));

            Assert.AreEqual(4507, fightStats.TotalDamage);
            Assert.AreEqual(3041, fightStats.MaxHitDamage);
            Assert.AreEqual(2, fightStats.MinHitDamage);
            Assert.AreEqual(5, fightStats.HitCount);
            Assert.AreEqual(1, fightStats.MissCount);
            Assert.AreEqual(0, fightStats.KillCount);
            Assert.AreEqual(0.833, fightStats.HitPercentage, 0.001);
        }
    }
}
