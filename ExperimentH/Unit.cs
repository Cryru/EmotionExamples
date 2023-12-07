using Emotion.Common;
using Emotion.Game;
using Emotion.Game.Time;
using Emotion.Game.Time.Routines;
using Emotion.Game.World2D;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Testing;
using Emotion.Utility;
using ExperimentH.Combat;
using ExperimentH.CombatScript;
using ExperimentH.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ExperimentH.BossBleed;

namespace ExperimentH
{
    public class Unit : GameObject2D
    {
        public int Hp = 100;
        public int Strength = 1;
        public int Armor = 0;

        public float CurrentHp = 0;
        public float SpeedPerMs = 0.06f;

        public int AITimeBetweenAbilities = 5000;
        protected After? _aiTimeBetweenAbilitiesCooldown;

        protected Coroutine? _behaviorRoutine = null;
        public List<Ability> _abilities = new List<Ability>();
        public List<Aura> _auras = new List<Aura>();

        public float GlobalCooldownProgress { get => _globalCooldown.Progress; }
        public float CastProgress { get => _castTimer?.Progress ?? 0f; }

        private After _globalCooldown = new After(500);
        private After? _castTimer;
        private Action? _onCastCallback;

        private Ability? _lastAbilityUsed;
        private int _lastAbilityTimesUsed;

        public Unit()
        {
            Size = new Vector2(10, 20);
            Tint = Color.PrettyOrange;
            OriginPos = Emotion.Game.Animation2D.OriginPosition.BottomCenter;
            _globalCooldown.End();
        }

        public override void Init()
        {
            base.Init();

            CurrentHp = Hp;
        }

        protected override void UpdateInternal(float dt)
        {
            base.UpdateInternal(dt);

            _globalCooldown.Update(dt);
            _aiTimeBetweenAbilitiesCooldown?.Update(dt);

            if (_castTimer != null)
            {
                _castTimer.Update(dt);
                if (_castTimer.Finished)
                {
                    Assert.NotNull(_onCastCallback);

                    _onCastCallback();
                    _castTimer = null;
                    _onCastCallback = null;
                }
            }

            if (_behaviorRoutine == null || _behaviorRoutine.Finished)
            {
                _behaviorRoutine = GetNextBehavior();
            }
        }

        public virtual void RenderShadow(RenderComposer c)
        {
            c.RenderEllipse(Position.ToVec2().ToVec3(-5), new Vector2(Width, Width * 0.33f), Color.Black * 0.3f, true);
        }

        #region Movement and Collision

        private List<GameObject2D> _list = new();

        public IEnumerable<Collision.CollisionNode<Rectangle>> CollisionTest(GameObject2D thisObj)
        {
            _list.Clear();
            Map.GetObjects(_list, 0, new Circle(Position2, 20f));

            for (int i = 0; i < _list.Count; i++)
            {
                var obj = _list[i];
                if (obj == thisObj) continue;
                if (obj is Unit u && u.IsDead()) continue;
                if (obj is AoEEffectUnit) continue;

                var bounds = obj.Bounds.Inflate(1, 1);

                // Keep party units far from each other for easier healing.
                if (obj is PartyUnit && thisObj is PartyUnit)
                {
                    bounds = bounds.Inflate(10, 10);

                    Vector2 center = bounds.Center;
                    Vector2 top = new Vector2(center.X, center.Y - bounds.Height / 2f);
                    Vector2 right = new Vector2(center.X + bounds.Width / 2f, center.Y);
                    Vector2 bottom = new Vector2(center.X, center.Y + bounds.Height / 2f);
                    Vector2 left = new Vector2(center.X - bounds.Width / 2f, center.Y);

                    yield return new Collision.CollisionNode<Rectangle>()
                    {
                        Surface = new LineSegment(top, right)
                    };

                    yield return new Collision.CollisionNode<Rectangle>()
                    {
                        Surface = new LineSegment(right, bottom)
                    };

                    yield return new Collision.CollisionNode<Rectangle>()
                    {
                        Surface = new LineSegment(bottom, left)
                    };

                    yield return new Collision.CollisionNode<Rectangle>()
                    {
                        Surface = new LineSegment(left, top)
                    };

                    continue;
                }

                var segments = bounds.GetLineSegments();
                for (int s = 0; s < segments.Length; s++)
                {
                    var segment = segments[s];

                    yield return new Collision.CollisionNode<Rectangle>()
                    {
                        Surface = segment
                    };
                }
            }
        }

        public void MoveDirection(Vector2 dir, float speed, float dt)
        {
            if (Map == null || Map.EditorMode) return; // todo: dont run unit routines in editor mode
            if (dir == Vector2.Zero) return;

            var boundCircle = new Circle(Position2, MathF.Max(Width, Height) / 2f);

            float amount = speed * dt;
            var moveResult = Collision.IncrementalGenericSegmentCollision(dir * amount, boundCircle, CollisionTest(this));

            if (moveResult.UnobstructedMovement != Vector2.Zero)
            {
                Position2 += moveResult.UnobstructedMovement;
                return;
            }

            Vector2 dirPerpen = dir.Perpendicular();
            moveResult = Collision.IncrementalGenericSegmentCollision(dirPerpen * amount, boundCircle, CollisionTest(this));
            if (moveResult.UnobstructedMovement != Vector2.Zero)
            {
                Position2 += moveResult.UnobstructedMovement;
                return;
            }

            dirPerpen = -dirPerpen;
            moveResult = Collision.IncrementalGenericSegmentCollision(dirPerpen * amount, boundCircle, CollisionTest(this));
            if (moveResult.UnobstructedMovement != Vector2.Zero)
            {
                Position2 += moveResult.UnobstructedMovement;
                return;
            }
        }

        #endregion

        #region AI and Behavior

        public IEnumerator AIBehaviorFightTarget(Unit aggroTarget)
        {
            if (_abilities.Count == 0) yield break;

            // basic attack
            var genericAttack = _abilities[0];

            // Use abilities you can use, if none can be used move/change state to use
            // generic ability
            while (aggroTarget != null)
            {
                if (IsDead()) yield break;

                bool canUseAbilities = _abilities.Count > 1;
                if (_aiTimeBetweenAbilitiesCooldown != null && !_aiTimeBetweenAbilitiesCooldown.Finished)
                    canUseAbilities = false;

                // Try to use any ability that doesn't require moving or
                // changing anything.
                bool abilityUsed = false;
                if (canUseAbilities)
                {
                    _aiTimeBetweenAbilitiesCooldown = null;

                    List<(Ability, Unit)> usableAbilities = new ();
                    for (int i = 1; i < _abilities.Count; i++)
                    {
                        var ability = _abilities[i];

                        Unit target = ability.GetAITarget(this, aggroTarget);
                        if (target == null) continue;

                        if (!ability.CheckAICondition(this, target)) continue;

                        if (CanUseAbility(ability, target) == AbilityReason.CanUse &&
                                (_lastAbilityUsed != ability || _lastAbilityTimesUsed < 2))
                        {
                            abilityUsed = true;
                            usableAbilities.Add((ability, target));
                        }
                    }

                    if (usableAbilities.Count > 0)
                    {
                        var abilityPair = Helpers.GetRandomArrayItem(usableAbilities);
                        UseAbility(abilityPair.Item1, abilityPair.Item2);
                        if (_castTimer != null) yield return new PassiveRoutineObserver(_castTimer); // Wait for cast.
                    }
                }

                if (abilityUsed)
                {
                    _aiTimeBetweenAbilitiesCooldown = new After(AITimeBetweenAbilities);
                }

                // If no ability was usable, then use generic attack.
                // This will also move the boss towards the aggro unit.
                if (!abilityUsed)
                {
                    yield return AIBehaviorAccommodateAndUseAbility(genericAttack, aggroTarget);
                }

                if (IsDead() || aggroTarget.IsDead()) yield break;
                yield return null;
            }
        }

        private IEnumerator AIBehaviorAccommodateAndUseAbility(Ability ability, Unit target)
        {
            if (IsDead() || target.IsDead()) yield break;

            var canUseAbility = CanUseAbility(ability, target);
            while (canUseAbility != AbilityReason.CanUse)
            {
                if (canUseAbility == AbilityReason.OnCooldown)
                {
                    yield return null;
                }
                else if (canUseAbility == AbilityReason.OutOfRange)
                {
                    Vector2 pointToMoveTo = target.Bounds.Center;
                    Vector2 dir = Vector2.Normalize(pointToMoveTo - Position2);
                    MoveDirection(dir, SpeedPerMs, Engine.DeltaTime);
                    yield return null;
                }

                if (IsDead() || target.IsDead()) yield break;
                canUseAbility = CanUseAbility(ability, target);
            }

            UseAbility(ability, target);
            if (_castTimer != null) yield return new PassiveRoutineObserver(_castTimer); // Wait for cast.
        }

        protected virtual Coroutine? GetNextBehavior()
        {
            if (IsDead())
            {
                return Map.CoroutineManager!.StartCoroutine(BehaviorDead());
            }

            return null;
        }

        public IEnumerator BehaviorDead()
        {
            Tint = new Color(62, 62, 62);
            (Width, Height) = (Height, Width);

            for (int i = _auras.Count - 1; i >= 0; i--)
            {
                RemoveAura(_auras[i]);
            }

            while (true)
            {
                yield return null;
            }
        }

        public void SetCustomBehavior(IEnumerator behavior)
        {
            Map.CoroutineManager!.StopCoroutine(_behaviorRoutine);
            _behaviorRoutine = Map.CoroutineManager.StartCoroutine(behavior);
        }

        public override void Destroy()
        {
            Map.CoroutineManager.StopCoroutine(_behaviorRoutine);
            base.Destroy();
        }

        #endregion

        #region Healing/Damage

        public bool IsDead()
        {
            return CurrentHp <= 0;
        }

        public virtual void TakeDamage(Unit source, float damage, bool canCrit = true)
        {
            if (IsDead()) return;

            // Crit
            if (canCrit)
            {
                var num = Helpers.GenerateRandomNumber(0, 100);
                if (num < 10) damage *= 1.5f;
            }

            // 5% variation up or down
            float variation = Helpers.GenerateRandomNumber(-5, 5);
            variation /= 100f;
            damage = damage + (damage * variation);

            // Reduced by armor
            damage = damage - Armor;

            if (damage < 0) return;

            var map = Map as GameMap;
            map?.AddFloatingText($"-{damage:0}", source, this, source is PartyUnit ? Color.White : Color.PrettyRed);

            CurrentHp -= damage;
            if (CurrentHp < 0f)
            {
                CurrentHp = 0;
                return;
            }
        }

        public virtual void HealDamage(Unit source, float amount, bool canCrit = true)
        {
            if (IsDead()) return;

            // Crit
            if (canCrit)
            {
                var num = Helpers.GenerateRandomNumber(0, 100);
                if (num < 10) amount *= 1.5f;
            }

            // 5% variation up or down
            float variation = Helpers.GenerateRandomNumber(-5, 5);
            variation /= 100f;
            amount = amount + (amount * variation);

            if (amount < 0) return;

            var map = Map as GameMap;
            map?.AddFloatingText($"+{amount:0}", this, this, Color.Green);
            CurrentHp = Maths.Clamp(CurrentHp + amount, 0, Hp);
        }

        #endregion

        #region Aura API

        public void ApplyAura(Unit caster, Aura aura)
        {
            if (IsDead()) return;

            var map = Map as GameMap;
            if (map == null) return;

            for (int i = 0; i < _auras.Count; i++)
            {
                var existingAura = _auras[i];
                if (existingAura.GetType() == aura.GetType() && caster == existingAura.Caster)
                {
                    RemoveAura(existingAura);
                    break;
                }
            }

            aura.SetMeta(caster, this);
            _auras.Add(aura);
            aura.Init(map);
        }

        public void RemoveAura(Aura aura)
        {
            _auras.Remove(aura);
            aura.Dispose();
        }

        #endregion

        #region Ability API

        public enum AbilityReason : byte
        {
            CanUse,
            OnCooldown,
            OutOfRange,
            AlreadyCasting
        }

        public AbilityReason CanUseAbility(Ability ability, Unit target)
        {
            if (ability.IsOnCooldown()) return AbilityReason.OnCooldown;
            if (!_globalCooldown.Finished) return AbilityReason.OnCooldown;
            if (target != null && !ability.InRangeToUseAbility(this, target)) return AbilityReason.OutOfRange;
            if (_castTimer != null) return AbilityReason.AlreadyCasting;

            return AbilityReason.CanUse;
        }

        public bool UseAbility(Ability ability, Unit target)
        {
            // Already casting.
            if (_castTimer != null) return false;

            if (_lastAbilityUsed == ability)
            {
                _lastAbilityTimesUsed++;
            }
            else
            {
                _lastAbilityUsed = ability;
                _lastAbilityTimesUsed = 1;
            }

            // Ability reasons - cooldown, range etc.
            var canUse = CanUseAbility(ability, target);
            if (canUse != AbilityReason.CanUse) return false;

            // Start casting or use now.
            if (ability.CastTime != 0)
            {
                _castTimer = new After(ability.CastTime);
                _onCastCallback = () =>
                {
                    ability.ExecuteAbilityInner(this, target);
                };
            }
            else
            {
                ability.ExecuteAbilityInner(this, target);
            }

            if (ability.ActiveGlobalCooldown) _globalCooldown.Restart();
            return true;
        }

        #endregion
    }
}
