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
            CastTime = 1000;
            Range = 99999;
        }

        protected override void ExecuteAbilityInternal(Unit caster, Unit target)
        {
            target.HealDamage(caster, 50);
        }
    }
}
