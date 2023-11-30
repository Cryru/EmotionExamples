using Emotion.Common;
using Emotion.Game.Time;
using Emotion.Game.Time.Routines;
using System.Collections;

namespace ExperimentH.Combat
{
    public abstract class Aura
    {
        public int Duration = 1;
        public int TimeBetweenTicks = 1;

        public int TimePassed { get; protected set; }

        public Unit Caster;
        public Unit OnUnit;

        protected Coroutine? _routine;

        public void Init()
        {
            _routine = Engine.CoroutineManager.StartCoroutine(AuraCoroutine());
            OnApply();
        }

        public void Dispose()
        {
            Engine.CoroutineManager.StopCoroutine(_routine);
            OnRemove();
        }

        public bool IsActive()
        {
            return _routine != null && !_routine.Finished && !_routine.Stopped;
        }

        public void SetMeta(Unit caster, Unit onUnit)
        {
            Caster = caster;
            OnUnit = onUnit;
        }

        public IEnumerator AuraCoroutine()
        {
            int ticks = Duration / TimeBetweenTicks;
            for (int i = 0; i < ticks; i++)
            {
                if (OnUnit.IsDead()) yield break;
                TickAuraInternal();
                yield return new After(TimeBetweenTicks);
                TimePassed += TimeBetweenTicks;
            }
            OnUnit.RemoveAura(this);
        }

        protected abstract void OnApply();
        protected abstract void OnRemove();
        protected abstract void TickAuraInternal();
    }
}
