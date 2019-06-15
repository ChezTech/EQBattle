using LogObjects;

namespace BizObjects.Lines
{
    public class Hit : Attack
    {
        public Hit(LogDatum logLine, string attacker, string defender, string attackVerb, int damage, string damageType, string damageBy, string damageQualifier, Zone zone = null) : base(logLine, attacker, defender, attackVerb, zone)
        {
            Damage = damage;
            DamageType = damageType;
            DamageBy = damageBy;
            DamageQualifier = damageQualifier;
        }

        public int Damage { get; }
        public string DamageType { get; }
        public string DamageBy { get; }
        public string DamageQualifier { get; }
    }
}
