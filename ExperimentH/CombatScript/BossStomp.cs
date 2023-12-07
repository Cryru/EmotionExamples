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
            Icon = "Icons/Stomp.png";
            StartCooldown();
        }

        protected override void ExecuteAbilityInternal(Unit caster, Unit target)
        {
            caster.Map.CoroutineManager.StartCoroutine(DealDamageCoroutine(caster));
        }

        private IEnumerator DealDamageCoroutine(Unit caster)
        {
            int stompCount = 5;
            int timeBetweenStomps = 1000;

            var map = caster.Map as GameMap;

            List<After> stompVisualTimers = new List<After>();

            void RenderVisuals(RenderComposer c)
            {
                for (int i = 0; i < stompVisualTimers.Count; i++)
                {
                    var stompTimer = stompVisualTimers[i];
                    stompTimer.Update(Engine.DeltaTime);

                    float stompSize = Range * 1.2f;
                    c.RenderEllipse(caster.Position, new Vector2(stompSize, stompSize * 0.33f) * stompTimer.Progress, new Color(0, 50, 0) * 0.5f * (1.0f - stompTimer.Progress), true);
                }
            }

            for (int i = 0; i < stompCount; i++)
            {
                stompVisualTimers.Add(new After(timeBetweenStomps * i));
            }

            map?.RegisterDrawable(RenderVisuals);

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
                List<GameObject2D> objs = new List<GameObject2D>();
                caster.Map.GetObjects(objs, 0, new Circle(caster.Position2, Range));

                for (int i = 0; i < objs.Count; i++)
                {
                    var obj = objs[i];
                    if (obj is Unit u && obj != caster && !u.IsDead())
                    {
                        u.TakeDamage(caster, 15, false);
                    }
                }

                yield return new After(timeBetweenStomps);

                if (caster.IsDead()) break;
            }

            map?.UnregisterDrawable(RenderVisuals);
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
