using ExperimentH.Combat;

namespace ExperimentH.CombatScript
{
    // Prevents death by becoming invulnerable briefly.
    public class TankShield : Ability
    {
        public TankShield()
        {
            Cooldown = 30000;
            Icon = "Icons/TankShield.png";
        }

        public class TankShieldAura : Aura
        {
            public TankShieldAura()
            {
                Duration = 4000;
                TimeBetweenTicks = Duration;
                Icon = "Icons/TankShield.png";
            }

            protected override void OnApply()
            {
                OnUnit.Armor += 999;
                OnUnit.HealDamage(Caster, 25);
            }

            protected override void OnRemove()
            {
                OnUnit.Armor -= 999;
            }

            protected override void TickAuraInternal()
            {
            }
        }

        public override Unit GetAITarget(Unit user, Unit currentTarget)
        {
            return user;
        }

        public override bool CheckAICondition(Unit user, Unit target)
        {
            return target.CurrentHp / target.Hp < 0.25f;
        }

        protected override void ExecuteAbilityInternal(Unit caster, Unit target)
        {
            caster.ApplyAura(caster, new TankShieldAura());
        }
    }
}
