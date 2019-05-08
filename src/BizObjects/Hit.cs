
using LogObjects;

namespace BizObjects
{
    public class Hit : Attack
    {
        public Hit(LogDatum logLine, string attacker, string defender, string attackVerb, int damage, string damageType, string damageBy, string damageQualifier, Zone zone = null) : base(logLine, attacker, defender, zone)
        {
            AttackVerb = attackVerb;
            Damage = damage;
            DamageType = damageType;
            DamageBy = damageBy;
            DamageQualifier = damageQualifier;
        }

        public string AttackVerb { get; }
        public int Damage { get; }
        public string DamageType { get; }
        public string DamageBy { get; }
        public string DamageQualifier { get; }
    }
}
