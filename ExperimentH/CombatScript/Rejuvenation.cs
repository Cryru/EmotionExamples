using ExperimentH.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH.CombatScript
{
    public class Rejuvenation : Ability
    {
        public class RejuvenationAura : Aura
        {
            public RejuvenationAura()
            {
                TimeBetweenTicks = 500;
                Duration = 5000;
            }

            protected override void OnApply()
            {
                
            }

            protected override void OnRemove()
            {
                
            }

            protected override void TickAuraInternal()
            {
                OnUnit.HealDamage(Caster, 15);
            }
        }

        public override void ExecuteAbilityInternal(Unit caster, Unit target)
        {
            target.ApplyAura(caster, new RejuvenationAura());
        }
    }
}
