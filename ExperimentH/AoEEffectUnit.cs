using Emotion.Game.Time;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;
using Emotion.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH
{

    public abstract class AoEEffectUnit : Unit
    {
        public float Radius;
        public int Duration;
        public int TickRate = 10;

        public AoEEffectUnit()
        {
            Z = -10;
            OriginPos = Emotion.Game.Animation2D.OriginPosition.CenterCenter;
        }

        protected override Coroutine? GetNextBehavior()
        {
            return Map.CoroutineManager!.StartCoroutine(TickBehavior());
        }

        public IEnumerator TickBehavior()
        {
            int timePassed = 0;
            while (Duration > timePassed)
            {
                yield return new After(TickRate);
                timePassed += TickRate;
                OnTick();
            }

            Map.RemoveObject(this);
        }

        public List<Unit> GetUnitsInside()
        {
            List<Unit> unitsInMe = new List<Unit>();
            Map.GetObjects(unitsInMe, 0, new Circle() { Radius = Radius, Center = Position2 });
            return unitsInMe;
        }

        public override void RenderShadow(RenderComposer c)
        {
            c.RenderEllipse(Position, new System.Numerics.Vector2(Radius, Radius * 0.33f), Tint, true);
        }

        protected override void RenderInternal(RenderComposer c)
        {
            
        }

        public override void TakeDamage(Unit source, float damage, bool canCrit = true)
        {

        }

        protected abstract void OnTick();

        protected override void Moved()
        {
            base.Moved();
            _z = -10;
        }
    }
}
