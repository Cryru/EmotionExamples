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

        public BossUnit()
        {
            Hp = 15_000;
            Strength = 55;
            AITimeBetweenAbilities = 10000;

            Size = new System.Numerics.Vector2(70, 80);
            Tint = new Color(139, 69, 19);

            _abilities.Add(new MeleeAttack(1000));
            _abilities.Add(new BossStomp());
            _abilities.Add(new BossBleed());
        }

        protected override Coroutine? GetNextBehavior()
        {
            var baseBehavior = base.GetNextBehavior();
            if (baseBehavior != null) return baseBehavior;

            var aggroUnit = GetAggroUnit();
            if (aggroUnit == null) return null;

            if (_currentState == 0)
            {
                return Engine.CoroutineManager.StartCoroutine(AIBehaviorFightTarget(aggroUnit));
            }

            return null;
        }

        public override void TakeDamage(Unit source, float amount, bool canCrit = true)
        {
            base.TakeDamage(source, amount, canCrit);

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
