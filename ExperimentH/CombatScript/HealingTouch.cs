using ExperimentH.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH.CombatScript
{
    public class HealingTouch : Ability
    {
        public HealingTouch()
        {
            CastTime = 1500;
        }

        public override void ExecuteAbilityInternal(Unit caster, Unit target)
        {
            target.HealDamage(caster, 50);
        }
    }
}
