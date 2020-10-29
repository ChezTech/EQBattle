using BizObjects.Battle;
using BizObjects.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace BizObjectsTests
{
    public class FightTestBase : ParserTestBase
    {

        protected static void VerifyFighterDuration(IFight fight, string name, TimeSpan fightDuration, TimeSpan fighterDuration)
        {
            Fighter fighter = fight.Fighters.FirstOrDefault(x => x.Character.Name == name);
            Assert.AreEqual(fightDuration, fighter.OffensiveStatistics.Duration.FightDuration);
            Assert.AreEqual(fighterDuration, fighter.OffensiveStatistics.Duration.FighterDuration);
        }

        protected void VerifyDpsStats(IFight iFight, string name, int total, TimeSpan duration, double fighterDps, double fightDps, Func<Fighter, FightStatistics> statChooser, bool isPet = false)
        {
            var fighter = GetFighter(name, iFight, isPet);

            VerifyDpsStats(statChooser(fighter), total, duration, fighterDps, fightDps);
        }

        protected Fighter VerifyFighterStatistics(string name, IFight iFight, int offHit, int offHeal, int offMisses, int offKills, int defHit, int defHeal, int defMisses, int defKills, bool isPet = false)
        {
            var fighter = GetFighter(name, iFight, isPet);

            VerifyFightStats(fighter.OffensiveStatistics, fighter.Character.Name, offHit, offHeal, offMisses, offKills);
            VerifyFightStats(fighter.DefensiveStatistics, fighter.Character.Name, defHit, defHeal, defMisses, defKills);

            return fighter;
        }

        private static Fighter GetFighter(string name, IFight iFight, bool isPet)
        {
            var fighter = iFight.Fighters.FirstOrDefault(x => x.Character.Name == name && x.Character.IsPet == isPet);
            Assert.IsNotNull(fighter, $"Fighter doesn't exist - {name}");
            return fighter;
        }

        protected IFight VerifyFightStatistics(string fightMob, Skirmish skirmish, int hit, int heal, int misses, int kills)
        {
            var fight = skirmish.Fights.FirstOrDefault(x => x.PrimaryMob.Name == fightMob) as Fight;
            VerifyFightStatistics(fightMob, fight, hit, heal, misses, kills);
            return fight;
        }

        protected void VerifyFightStatistics(string fightMob, IFight fight, int hit, int heal, int misses, int kills)
        {
            Assert.IsNotNull(fight, $"Fight doesn't exist - {fightMob}");

            var stats = fight.Statistics;
            Assert.AreEqual(hit, stats.Hit.Total, $"Offensive hit - {fight.PrimaryMob.Name}");
            Assert.AreEqual(heal, stats.Heal.Total, $"Offensive heal - {fight.PrimaryMob.Name}");
            Assert.AreEqual(misses, stats.Miss.Count, $"Offensive misses - {fight.PrimaryMob.Name}");
            Assert.AreEqual(kills, stats.Kill.Count, $"Offensive kills - {fight.PrimaryMob.Name}");
        }

        protected void VerifySkirmishStats(Skirmish skirmish, int hit, int heal, int misses, int kills)
        {
            var stats = skirmish.Statistics;
            Assert.AreEqual(hit, stats.Hit.Total, $"Offensive hit");
            Assert.AreEqual(heal, stats.Heal.Total, $"Offensive heal");
            Assert.AreEqual(misses, stats.Miss.Count, $"Offensive misses");
            Assert.AreEqual(kills, stats.Kill.Count, $"Offensive kills");
        }

        private static void VerifyFightStats(FightStatistics stats, string name, int hitTotal, int healTotal, int missCount, int kilLCount)
        {
            Assert.AreEqual(hitTotal, stats.Hit.Total, $"Fensive hit - {name}");
            Assert.AreEqual(healTotal, stats.Heal.Total, $"Fensive heal - {name}");
            Assert.AreEqual(missCount, stats.Miss.Count, $"Fensive misses - {name}");
            Assert.AreEqual(kilLCount, stats.Kill.Count, $"Fensive kills - {name}");
        }

        protected void VerifyDpsStats(FightStatistics stats, int hitTotal, TimeSpan duration, double fighterDps, double fightDps)
        {
            Assert.AreEqual(hitTotal, stats.Hit.Total);
            Assert.AreEqual(duration, stats.Duration.FighterDuration);
            Assert.AreEqual(fighterDps, stats.PerTime.FighterDPS, 0.01);
            Assert.AreEqual(fightDps, stats.PerTime.FightDPS, 0.01);
        }
    }
}
