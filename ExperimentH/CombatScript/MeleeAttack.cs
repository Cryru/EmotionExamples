using Emotion.Utility;
using ExperimentH.Combat;

namespace ExperimentH.CombatScript
{
    public class MeleeAttack : Ability
    {
        public MeleeAttack(int cooldown)
        {
            Range = 8;
            Cooldown = cooldown;
        }

        public override void ExecuteAbilityInternal(Unit caster, Unit target)
        {
            float damage = caster.Strength;

            // Crit
            var num = Helpers.GenerateRandomNumber(0, 100);
            if (num < 10) damage *= 1.5f;

            // 5% variation up or down
            float variation = Helpers.GenerateRandomNumber(-5, 5);
            variation /= 100f;
            damage = damage + (damage * variation);

            damage = damage - target.Armor;

            target.TakeDamage(caster, damage);
        }
    }
}
