using Emotion.Game.World2D;
using Emotion.Primitives;
using Emotion.Utility;
using ExperimentH.Combat;

namespace ExperimentH
{
    public class BossBleed : Ability
    {
        public BossBleed()
        {
            CastTime = 4000;
            Cooldown = 10000;
            Range = 50;
            Icon = "Icons/DoT.png";
            StartCooldown();
        }

        public class BossBleedAura : Aura
        {
            public BossBleedAura()
            {
                TimeBetweenTicks = 1000;
                Duration = 15000;
                Icon = "Icons/DoT.png";
            }

            protected override void OnApply()
            {
               
            }

            protected override void OnRemove()
            {
                
            }

            protected override void TickAuraInternal()
            {
                OnUnit.TakeDamage(Caster, 15, false);
            }
        }

        protected override void ExecuteAbilityInternal(Unit caster, Unit target)
        {
            List<GameObject2D> objs = new List<GameObject2D>();
            caster.Map.GetObjects(objs, 0, new Circle(caster.Position2, Range));

            List<Unit> validTargets = new List<Unit>();
            List<Unit> secondaryTargets = new List<Unit>();
            for (int i = 0; i < objs.Count; i++)
            {
                var obj = objs[i];
                if (obj is Unit u && obj != caster && !u.IsDead())
                {
                    if(u is TankPartyUnit || u is PlayerUnit)
                    {
                        secondaryTargets.Add(u);
                    }
                    else
                    {
                        validTargets.Add(u);
                    }
                }
            }

            if (validTargets.Count == 0) validTargets = secondaryTargets;
            if (validTargets.Count == 0) return;

            int randomUnit = Helpers.GenerateRandomNumber(0, validTargets.Count - 1);
            var unit = validTargets[randomUnit];
            unit.ApplyAura(caster, new BossBleedAura());
        }
    }
}
