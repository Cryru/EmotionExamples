using ExperimentH.Combat;

namespace ExperimentH.CombatScript
{
    public class Lifebloom : Ability
    {
        private LifebloomAura? _lastAuraApplied;

        public class LifebloomAura : Aura
        {
            public LifebloomAura()
            {
                TimeBetweenTicks = 1000;
                Duration = 10000;
            }

            protected override void OnApply()
            {
                OnUnit.Armor += 5;
            }

            protected override void OnRemove()
            {
                OnUnit.Armor -= 5;
            }

            protected override void TickAuraInternal()
            {
                OnUnit.HealDamage(Caster, 35);
            }
        }

        public override void ExecuteAbilityInternal(Unit caster, Unit target)
        {
            // Can be active only on one unit.
            if (_lastAuraApplied != null && _lastAuraApplied.IsActive())
            {
                _lastAuraApplied.OnUnit.RemoveAura(_lastAuraApplied);
                _lastAuraApplied = null;
            }

            _lastAuraApplied = new LifebloomAura();
            target.ApplyAura(caster, _lastAuraApplied);
        }
    }
}
