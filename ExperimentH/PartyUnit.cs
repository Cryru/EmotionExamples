﻿using Emotion.Common;
using Emotion.Game;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;
using Emotion.Primitives;
using ExperimentH.Combat;
using ExperimentH.CombatScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH
{
    public class PartyUnit : Unit
    {
        private Ability _genericAttack;

        public PartyUnit()
        {
            _genericAttack = new MeleeAttack(500);
            Strength = 24;
            _abilities.Add(_genericAttack);
        }

        public override void Init()
        {
            base.Init();

            if (Map is GameMap gameMap)
            {
                gameMap.AddHealthBarToPartyUI(this);
            }
        }

        protected override Coroutine? GetNextBehavior()
        {
            var baseBehavior = base.GetNextBehavior();
            if (baseBehavior != null) return baseBehavior;

            Unit? enemy = null;
            var allObjects = Map.GetAllObjectsCoroutine(); // todo: multiple, find closest
            while (allObjects.MoveNext())
            {
                var obj = allObjects.Current;
                if (obj is BossUnit bu && !bu.IsDead()) // todo: faction
                {
                    enemy = bu;
                    break;
                }
            }

            // No enemy on map.
            if (enemy == null) return null;

            return Engine.CoroutineManager.StartCoroutine(AIBehaviorFightTarget(enemy));
        }
    }
}
