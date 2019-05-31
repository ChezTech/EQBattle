using System;
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
    // Notes:
    // https://forums.daybreakgames.com/eq/index.php?threads/dps-parsers.232663/
    // https://github.com/rumstil
    // http://www.eqpixie.com/
    // http://www.eqwatcher.com/
    // https://eq.gimasoft.com/gina/Default.aspx

    [TestClass]
    public class SkirmishTests
    {
        private static readonly YouResolver YouAre = new YouResolver("Khadaji");
        private static readonly CharacterResolver CharResolver = new CharacterResolver();
        private LineParserFactory _parser = new LineParserFactory();
        private readonly IParser _hitParser = new HitParser(YouAre);
        private readonly IParser _missParser = new MissParser(YouAre);
        private readonly IParser _healParser = new HealParser(YouAre);
        private readonly IParser _killParser = new KillParser(YouAre);


        public SkirmishTests()
        {
            _parser.AddParser(_hitParser, null);
            _parser.AddParser(_missParser, null);
            _parser.AddParser(_healParser, null);
            _parser.AddParser(_killParser, null);

            CharResolver.AddNonPlayer("Harvester Collyx");
        }

        [TestMethod]
        public void SmallFight()
        {
            var pc = new Character(YouAre.Name);
            var skirmish = new Skirmish(YouAre, CharResolver);

            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:01:03 2019] Gomphus tries to hit YOU, but YOU block!")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:01:17 2019] Khronick is bathed in a zealous light. Movanna healed Khronick for 9197 (23333) hit points by Zealous Light.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:01:20 2019] Harvester Collyx tries to hit Movanna, but Movanna blocks!")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:01:23 2019] Gomphus hits Movanna for 3870 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:01:23 2019] Harvester Collyx hits Movanna for 2566 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:01:43 2019] You kick Gomphus for 1207 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:02:16 2019] Khronick feels the touch of recuperation. Movanna healed Khronick for 1827 (23605) hit points by Word of Recuperation.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:02:16 2019] Movanna feels the touch of recuperation. Movanna healed herself for 23605 hit points by Word of Recuperation.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:02:16 2019] You feel the touch of recuperation. Movanna healed you for 20053 (23605) hit points by Word of Recuperation.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:02:16 2019] Movanna has taken 3000 damage by Noxious Visions.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:02:56 2019] Movanna crushes Gomphus for 322 points of damage. (Riposte)")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:03:01 2019] Khronick healed you over time for 1518 hit points by Healing Counterbias Effect.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:03:19 2019] Harvester Collyx has taken 8206 damage from Blood of Jaled'Dar by Khronick. (Critical)")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:03:30 2019] Khronick has taken 1950 damage from Noxious Visions by Gomphus.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:03:53 2019] You kick Harvester Collyx for 13333 points of damage. (Riposte Critical)")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:03:53 2019] You have slain Harvester Collyx!")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:04:04 2019] Gomphus hits Movanna for 2076 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:04:04 2019] Movanna has been slain by Gomphus!")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:04:36 2019] Gomphus hits Khronick for 6177 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Tue May 28 06:04:36 2019] Khronick has been slain by Gomphus!")));

            // Skirmish stats
            VerifySkirmishStats(skirmish, 42707, 56200, 2, 3);

            // Skirmish Fighters
            Assert.AreEqual(6, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 14540, 0, 0, 1, 0, 21571, 1, 0);
            VerifyFighterStatistics("Movanna", skirmish, 322, 54682, 0, 0, 11512, 23605, 1, 1);
            VerifyFighterStatistics("Khronick", skirmish, 8206, 1518, 0, 0, 8127, 11024, 0, 1);
            VerifyFighterStatistics("Gomphus", skirmish, 14073, 0, 1, 2, 1529, 0, 0, 0);
            VerifyFighterStatistics("Harvester Collyx", skirmish, 2566, 0, 1, 0, 21539, 0, 0, 1);
            VerifyFighterStatistics("Unknown", skirmish, 3000, 0, 0, 0, 0, 0, 0, 0);

            // Skirmish Fights
            Assert.AreEqual(2, skirmish.Fights.Count);
            VerifyFightStatistics("Gomphus", skirmish, 18602, 56200, 1, 2);
            VerifyFightStatistics("Harvester Collyx", skirmish, 24105, 0, 1, 1);
        }

        [TestMethod]
        public void FightWithAddOfSameTypeThenOneDies()
        {
            var pc = new Character(YouAre.Name);
            var skirmish = new Skirmish(YouAre, CharResolver);

            // Setup a fight and get an add (no way to tell it's an add)
            // Then have the first die and a new fight should be made
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:28:43 2019] You hit a cliknar sporali farmer for 2 points of magic damage by Distant Strike I.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:28:49 2019] A cliknar sporali farmer hits YOU for 1048 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:28:51 2019] A cliknar sporali farmer hits Khronick for 3791 points of damage. (Strikethrough)"))); // Let's pretend this is the add ... going after the healer
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:28:56 2019] You kick a cliknar sporali farmer for 2044 points of damage. (Riposte Critical)")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:29:58 2019] You kick a cliknar sporali farmer for 75472 points of damage. (Riposte Finishing Blow)")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:29:58 2019] You have slain a cliknar sporali farmer!"))); // First mob dies
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:31:14 2019] A cliknar sporali farmer hits YOU for 4342 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:31:19 2019] You kick a cliknar sporali farmer for 3224 points of damage. (Riposte)")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:32:32 2019] You crush a cliknar sporali farmer for 75203 points of damage. (Finishing Blow)")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:32:32 2019] You have slain a cliknar sporali farmer!"))); // Second mob dies

            // Skirmish stats
            VerifySkirmishStats(skirmish, 165126, 0, 0, 2);

            // Skirmish Fighters
            Assert.AreEqual(3, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 155945, 0, 0, 2, 5390, 0, 0, 0);
            VerifyFighterStatistics("Khronick", skirmish, 0, 0, 0, 0, 3791, 0, 0, 0);
            VerifyFighterStatistics("a cliknar sporali farmer", skirmish, 9181, 0, 0, 0, 155945, 0, 0, 2);

            // Skirmish Fights
            Assert.AreEqual(2, skirmish.Fights.Count);
            VerifyFightStatistics("a cliknar sporali farmer", skirmish, 82357, 0, 0, 1);

            // The helper method just grabs the first match of the mobName which would be the fight above, so get the second/last fight our own way
            var lastFight = skirmish.Fights.LastOrDefault(x => x.PrimaryMob.Name == "a cliknar sporali farmer");
            VerifyFightStatistics(lastFight, "a cliknar sporali farmer", skirmish, 82769, 0, 0, 1);
        }

        [TestMethod]
        public void FightWithAddSameSingleName()
        {
            var pc = new Character(YouAre.Name);
            var skirmish = new Skirmish(YouAre, CharResolver);

            // Setup a fight and get an add (no way to tell it's an add)
            // Then have the first die and a new fight should be made
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:28:43 2019] You hit Bob for 2 points of magic damage by Distant Strike I.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:28:49 2019] Bob hits YOU for 10 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:28:56 2019] You kick Bob for 6 points of damage. (Riposte Critical)")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:29:58 2019] You have slain Bob!")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:31:14 2019] Bob hits YOU for 3 points of damage.")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:31:19 2019] You kick Bob for 7 points of damage. (Riposte)")));
            skirmish.AddLine((dynamic)_parser.ParseLine(new LogDatum("[Mon May 27 09:32:32 2019] You have slain Bob!"))); // Second mob dies

            // Skirmish stats
            VerifySkirmishStats(skirmish, 28, 0, 0, 2);

            // Skirmish Fighters
            Assert.AreEqual(2, skirmish.Fighters.Count());
            VerifyFighterStatistics("Khadaji", skirmish, 15, 0, 0, 2, 13, 0, 0, 0);
            VerifyFighterStatistics("Bob", skirmish, 13, 0, 0, 0, 15, 0, 0, 2);

            // Skirmish Fights
            Assert.AreEqual(2, skirmish.Fights.Count);
            VerifyFightStatistics("Bob", skirmish, 18, 0, 0, 1);

            // The helper method just grabs the first match of the mobName which would be the fight above, so get the second/last fight our own way
            var lastFight = skirmish.Fights.LastOrDefault(x => x.PrimaryMob.Name == "Bob");
            VerifyFightStatistics(lastFight, "Bob", skirmish, 10, 0, 0, 1);
        }

        private void VerifySkirmishStats(Skirmish skirmish, int hit, int heal, int misses, int kills)
        {
            var stats = skirmish.OffensiveStatistics;
            Assert.AreEqual(hit, stats.Hit.Total, $"Offensive hit");
            Assert.AreEqual(heal, stats.Heal.Total, $"Offensive heal");
            Assert.AreEqual(misses, stats.Miss.Count, $"Offensive misses");
            Assert.AreEqual(kills, stats.Kill.Count, $"Offensive kills");

            stats = skirmish.DefensiveStatistics;
            Assert.AreEqual(hit, stats.Hit.Total, $"Defensive hit");
            Assert.AreEqual(heal, stats.Heal.Total, $"Defensive heal");
            Assert.AreEqual(misses, stats.Miss.Count, $"Defensive misses");
            Assert.AreEqual(kills, stats.Kill.Count, $"Defensive kills");
        }

        private void VerifyFighterStatistics(string name, Skirmish skirmish, int offHit, int offHeal, int offMisses, int offKills, int defHit, int defHeal, int defMisses, int defKills)
        {
            var fighter = skirmish.Fighters.FirstOrDefault(x => x.Character.Name == name);
            Assert.IsNotNull(fighter, $"Fighter doesn't exist - {name}");

            var stats = fighter.OffensiveStatistics;
            Assert.AreEqual(offHit, stats.Hit.Total, $"Offensive hit - {fighter.Character.Name}");
            Assert.AreEqual(offHeal, stats.Heal.Total, $"Offensive heal - {fighter.Character.Name}");
            Assert.AreEqual(offMisses, stats.Miss.Count, $"Offensive misses - {fighter.Character.Name}");
            Assert.AreEqual(offKills, stats.Kill.Count, $"Offensive kills - {fighter.Character.Name}");

            stats = fighter.DefensiveStatistics;
            Assert.AreEqual(defHit, stats.Hit.Total, $"Defensive hit - {fighter.Character.Name}");
            Assert.AreEqual(defHeal, stats.Heal.Total, $"Defensive heal - {fighter.Character.Name}");
            Assert.AreEqual(defMisses, stats.Miss.Count, $"Defensive misses - {fighter.Character.Name}");
            Assert.AreEqual(defKills, stats.Kill.Count, $"Defensive kills - {fighter.Character.Name}");
        }

        private void VerifyFightStatistics(string fightMob, Skirmish skirmish, int hit, int heal, int misses, int kills)
        {
            var fight = skirmish.Fights.FirstOrDefault(x => x.PrimaryMob.Name == fightMob);
            VerifyFightStatistics(fight, fightMob, skirmish, hit, heal, misses, kills);
        }

        private void VerifyFightStatistics(IFight fight, string fightMob, Skirmish skirmish, int hit, int heal, int misses, int kills)
        {
            Assert.IsNotNull(fight, $"Fight doesn't exist - {fightMob}");

            var stats = fight.OffensiveStatistics;
            Assert.AreEqual(hit, stats.Hit.Total, $"Offensive hit - {fight.PrimaryMob.Name}");
            Assert.AreEqual(heal, stats.Heal.Total, $"Offensive heal - {fight.PrimaryMob.Name}");
            Assert.AreEqual(misses, stats.Miss.Count, $"Offensive misses - {fight.PrimaryMob.Name}");
            Assert.AreEqual(kills, stats.Kill.Count, $"Offensive kills - {fight.PrimaryMob.Name}");

            stats = fight.DefensiveStatistics;
            Assert.AreEqual(hit, stats.Hit.Total, $"Defensive hit - {fight.PrimaryMob.Name}");
            Assert.AreEqual(heal, stats.Heal.Total, $"Defensive heal - {fight.PrimaryMob.Name}");
            Assert.AreEqual(misses, stats.Miss.Count, $"Defensive misses - {fight.PrimaryMob.Name}");
            Assert.AreEqual(kills, stats.Kill.Count, $"Defensive kills - {fight.PrimaryMob.Name}");
        }
    }
}
