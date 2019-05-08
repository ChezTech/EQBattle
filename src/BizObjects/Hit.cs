
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

        public string AttackVerb { get; private set; }
        public int Damage { get; private set; }
        public string DamageType { get; private set; }
        public string DamageBy { get; private set; }
        public string DamageQualifier { get; private set; }
    }
}
