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
            ActiveGlobalCooldown = false;
        }

        protected override void ExecuteAbilityInternal(Unit caster, Unit target)
        {
            float damage = caster.Strength;

          

            target.TakeDamage(caster, damage);
        }
    }
}
