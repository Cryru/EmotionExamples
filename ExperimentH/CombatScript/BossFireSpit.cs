using Emotion.Game.World2D;
using Emotion.Primitives;
using Emotion.Utility;
using ExperimentH.Combat;
using System.Numerics;

namespace ExperimentH.CombatScript
{
    public class BossFireSpit : Ability
    {
        public BossFireSpit()
        {
            CastTime = 2000;
            Cooldown = 5000;
            Range = 9999;
            Icon = "Icons/FireSpit.png";
            StartCooldown();
        }

        public class BossFireSpitUnit : AoEEffectUnit
        {
            private Unit Caster;

            public BossFireSpitUnit(Unit caster)
            {
                Duration = 10000;
                TickRate = 250;
                Radius = 30;
                Caster = caster;
                Tint = Color.PrettyRed * 0.6f;
            }

            protected override void OnTick()
            {
                var unitsInMe = GetUnitsInside();
                for (int i = 0; i < unitsInMe.Count; i++)
                {
                    var unit = unitsInMe[i];
                    if (unit != Caster)
                    {
                        unit.TakeDamage(Caster, 5, false);
                    }
                }
            }
        }

        protected override void ExecuteAbilityInternal(Unit caster, Unit target)
        {
            int spits = 3;
            List<Vector3> positions = new List<Vector3>();

            List<GameObject2D> objs = new List<GameObject2D>();
            caster.Map.GetObjects(objs, 0);
            for (int i = 0; i < objs.Count; i++)
            {
                var obj = objs[i];
                if (obj != caster && (positions.Count < spits - 1 || obj is PlayerUnit))
                {
                    int distance = Helpers.GenerateRandomNumber(10, 100);
                    Vector2 direction = Helpers.GetRandomArrayItem(Maths.CardinalDirectionsAndDiagonals2D);
                    positions.Add(obj.Position + direction.ToVec3() * distance);
                }
            }

            for (int i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                caster.Map.AddObject(new BossFireSpitUnit(caster) { Position = pos });
            }
        }
    }
}
