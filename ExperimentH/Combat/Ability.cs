using Emotion.Common;
using Emotion.Game.Time;
using Emotion.Game.Time.Routines;
using System.Collections;
using System.Numerics;

namespace ExperimentH.Combat
{
    public abstract class Ability
    {
        public int Range;
        public int Cooldown;
        public int CastTime;

        protected Coroutine? _cooldownRoutine;

        public bool InRangeToUseAbility(Unit user, Unit other)
        {
            var p1 = user.Bounds.GetClosestPointOnRectangleBorderToPoint(other.Bounds.Center);
            var p2 = other.Bounds.GetClosestPointOnRectangleBorderToPoint(user.Bounds.Center);

            float dist = Vector2.Distance(p1, p2);
            return dist < Range;
        }

        public void ExecuteAbility(Unit caster, Unit target)
        {
            ExecuteAbilityInternal(caster, target);
            _cooldownRoutine = Engine.CoroutineManager.StartCoroutine(RunCooldown());
        }

        public abstract void ExecuteAbilityInternal(Unit caster, Unit target);

        private IEnumerator RunCooldown()
        {
            yield return new After(Cooldown);
        }

        public bool IsOnCooldown()
        {
            return _cooldownRoutine != null && !_cooldownRoutine.Finished;
        }
    }
}
