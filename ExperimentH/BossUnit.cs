using Emotion.Common;
using Emotion.Game.Time.Routines;
using Emotion.Primitives;
using ExperimentH.Combat;
using ExperimentH.CombatScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH
{
    public class BossUnit : Unit
    {
        public List<Unit> _threatTable = new();

        private int _currentState = 0;

        private Ability _genericAttack;

        public BossUnit()
        {
            Hp = 10_000;
            Strength = 50;

            Size = new System.Numerics.Vector2(70, 80);
            Tint = new Color(139, 69, 19);

            _genericAttack = new MeleeAttack(1000);
            _abilities.Add(_genericAttack);
        }

        protected override Coroutine? GetNextBehavior()
        {
            var baseBehavior = base.GetNextBehavior();
            if (baseBehavior != null) return baseBehavior;

            var aggroUnit = GetAggroUnit();
            if (aggroUnit == null) return null;

            if (_currentState == 0)
            {
                return Engine.CoroutineManager.StartCoroutine(BehaviorAttackTank());
            }

            return null;
        }

        protected IEnumerator BehaviorAttackTank()
        {
            var aggroUnit = GetAggroUnit();
            while (aggroUnit != null)
            {
                yield return AIBehaviorUseAbility(_genericAttack, aggroUnit, 1);
                if (IsDead()) yield break;

                yield return null;
                aggroUnit = GetAggroUnit();
            }
        }

        public override void TakeDamage(Unit source, float amount)
        {
            base.TakeDamage(source, amount);

            if (!_threatTable.Contains(source))
            {
                if (source is TankPartyUnit)
                {
                    _threatTable.Insert(0, source);
                }
                else
                {
                    _threatTable.Add(source);
                }
            }
        }

        public Unit? GetAggroUnit()
        {
            // todo: threat mechanic, sort by threat etc.
            for (int i = 0; i < _threatTable.Count; i++)
            {
                var u = _threatTable[i];
                if (!u.IsDead())
                {
                    return u;
                }
            }

            return null;
        }
    }
}
