using ExperimentH.Combat;

namespace ExperimentH.CombatScript
{
    public class Lifebloom : Ability
    {
        private LifebloomAura? _lastAuraApplied;

        public Lifebloom()
        {
            Range = 99999;
            Icon = "Icons/Lifebloom.png";
        }

        public class LifebloomAura : Aura
        {
            public LifebloomAura()
            {
                TimeBetweenTicks = 700;
                Duration = 15000;
                Icon = "Icons/Lifebloom.png";
            }

            protected override void OnApply()
            {
                OnUnit.Armor += 10;
            }

            protected override void OnRemove()
            {
                OnUnit.Armor -= 10;
            }

            protected override void TickAuraInternal()
            {
                OnUnit.HealDamage(Caster, 30);
            }
        }

        protected override void ExecuteAbilityInternal(Unit caster, Unit target)
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
