using Emotion.Common;
using Emotion.Game.Time;
using Emotion.Game.Time.Routines;
using Emotion.Game.World2D;
using Emotion.Graphics;
using Emotion.Primitives;
using ExperimentH.Combat;
using System.Collections;
using System.Numerics;

namespace ExperimentH
{
    public class BossStomp : Ability
    {
        public BossStomp()
        {
            CastTime = 2000;
            Cooldown = 10000;
            Range = 100;
            StartCooldown();
        }

        protected override void ExecuteAbilityInternal(Unit caster, Unit target)
        {
            List<GameObject2D> objs = new List<GameObject2D>(); 
            caster.Map.GetObjects(objs, 0, new Circle(caster.Position2, Range));

            Engine.CoroutineManager.StartCoroutine(DealDamageCoroutine(caster, objs));
        }

        private IEnumerator DealDamageCoroutine(Unit caster, List<GameObject2D> objs)
        {
            int stompCount = 5;
            int timeBetweenStomps = 1000;

            List<After> stompVisualTimers = new List<After>();

            void RenderVisuals(RenderComposer c, Unit caster)
            {
                for (int i = 0; i < stompVisualTimers.Count; i++)
                {
                    var stompTimer = stompVisualTimers[i];
                    stompTimer.Update(Engine.DeltaTime);
                    c.RenderCircle(caster.Position, Range * stompTimer.Progress, new Color(0, 50, 0) * 0.5f * (1.0f - stompTimer.Progress), true);
                }
            }

            for (int i = 0; i < stompCount; i++)
            {
                stompVisualTimers.Add(new After(timeBetweenStomps * i));
            }

            caster.RegisterDrawable(RenderVisuals);

            //for (int i = 0; i < objs.Count; i++)
            //{
            //    var obj = objs[i];
            //    if (obj is Unit u && obj != caster && !u.IsDead())
            //    {
            //        if (u is not TankPartyUnit)
            //            u.SetCustomBehavior(PushBackUnit(u, caster.Position2, 40));
            //    }
            //}

            for (int c = 0; c < stompCount; c++)
            {
                for (int i = 0; i < objs.Count; i++)
                {
                    var obj = objs[i];
                    if (obj is Unit u && obj != caster && !u.IsDead())
                    {
                        u.TakeDamage(caster, 17, false);
                    }
                }
                
                yield return new After(timeBetweenStomps);

                if (caster.IsDead()) break;
            }

            caster.UnregisterDrawable(RenderVisuals);
            stompVisualTimers.Clear();
        }

        private IEnumerator PushBackUnit(Unit u, Vector2 startPoint, float amount)
        {
            float overTime = 250;
            float pushPerMs = amount / overTime;
            float pushed = 0;
            while (pushed < amount)
            {
                Vector2 dir = -Vector2.Normalize(startPoint - u.Position2);
                u.MoveDirection(dir, pushPerMs, Engine.DeltaTime);
                pushed += pushPerMs * Engine.DeltaTime;
                yield return null;
            }
        }
    }
}
