using Emotion.Common;
using Emotion.Game.Time;
using Emotion.Game.Time.Routines;
using Emotion.Testing;
using System.Collections;
using System.Diagnostics;
using System.Numerics;

namespace ExperimentH.Combat
{
    public abstract class Ability
    {
        public int Range = 1;
        public int Cooldown;
        public int CastTime;
        public bool ActiveGlobalCooldown = true;
        public string Icon;

        protected Coroutine? _cooldownRoutine;

        public bool InRangeToUseAbility(Unit user, Unit other)
        {
            var p1 = user.Bounds.GetClosestPointOnRectangleBorderToPoint(other.Bounds.Center);
            var p2 = other.Bounds.GetClosestPointOnRectangleBorderToPoint(user.Bounds.Center);

            float dist = Vector2.Distance(p1, p2);
            return dist < Range;
        }

        public void ExecuteAbilityInner(Unit caster, Unit target)
        {
            ExecuteAbilityInternal(caster, target);
            StartCooldown();
        }

        protected abstract void ExecuteAbilityInternal(Unit caster, Unit target);

        protected void StartCooldown()
        {
            Debug.Assert(!IsOnCooldown());
            _cooldownRoutine = Engine.CoroutineManager.StartCoroutine(RunCooldown());
        }

        private IEnumerator RunCooldown()
        {
            yield return new After(Cooldown);
        }

        public bool IsOnCooldown()
        {
            return _cooldownRoutine != null && !_cooldownRoutine.Finished;
        }

        #region AI

        public virtual bool CheckAICondition(Unit user, Unit target)
        {
            return true;
        }

        public virtual Unit GetAITarget(Unit user, Unit currentTarget)
        {
            return currentTarget;
        }

        #endregion
    }
}
