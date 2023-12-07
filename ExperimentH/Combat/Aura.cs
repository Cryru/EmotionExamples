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
        public string Icon;

        public float TimeStampStartedAt;

        public int TimePassed
        {
            get
            {
                if (_map == null) return 0;

                float currentTime = _map.CoroutineManager.CurrentTime;
                float timeSinceStart = currentTime - TimeStampStartedAt;
                if (timeSinceStart > Duration) timeSinceStart = Duration;
                return (int)timeSinceStart;
            }
        }

        public Unit Caster;
        public Unit OnUnit;

        protected Coroutine? _routine;
        protected Coroutine _durationRoutine;
        protected GameMap _map; 

        public void Init(GameMap map)
        {
            _map = map;

            TimeStampStartedAt = map.CoroutineManager.CurrentTime;
            _routine = map.CoroutineManager.StartCoroutine(TickCoroutine());
            _durationRoutine = map.CoroutineManager.StartCoroutine(DurationCoroutine());
            OnApply();
        }

        public void Dispose()
        {
            if (!_durationRoutine.Finished && !_durationRoutine.Stopped)
            {
                OnRemove();
            }

            _map.CoroutineManager.StopCoroutine(_routine);
            _map.CoroutineManager.StopCoroutine(_durationRoutine);
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

        public IEnumerator TickCoroutine()
        {
            int ticks = Duration / TimeBetweenTicks;
            for (int i = 0; i < ticks; i++)
            {
                if (OnUnit.IsDead()) yield break;
                TickAuraInternal();
                yield return new After(TimeBetweenTicks);
            }
        }

        public IEnumerator DurationCoroutine()
        {
            yield return new After(Duration);
            OnUnit.RemoveAura(this);
        }

        protected abstract void OnApply();
        protected abstract void OnRemove();
        protected abstract void TickAuraInternal();
    }
}
