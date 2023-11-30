using Emotion.Common;
using Emotion.Game;
using Emotion.Game.Time.Routines;
using Emotion.Game.World2D;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Utility;
using ExperimentH.Combat;
using ExperimentH.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH
{
    public class Unit : GameObject2D
    {
        public int Hp = 100;
        public int Strength = 1;
        public int Armor = 0;

        public float CurrentHp = 0;
        public float SpeedPerMs = 0.06f;

        protected Coroutine? _behaviorRoutine = null;
        protected List<Ability> _abilities = new List<Ability>();
        protected List<Aura> _auras = new List<Aura>();
        protected List<FloatingText> _floatingTexts = new List<FloatingText>();

        public Unit()
        {
            Size = new Vector2(10, 20);
            Tint = Color.PrettyOrange;
            OriginPos = Emotion.Game.Animation2D.OriginPosition.BottomCenter;
        }

        public override void Init()
        {
            base.Init();

            CurrentHp = Hp;
        }

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

                var bounds = obj.Bounds.Inflate(1, 1);
                if (obj is PartyUnit && thisObj is PartyUnit)
                {
                    bounds = bounds.Inflate(5, 5);
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

        public void MoveDirection(Vector2 dir, float dt)
        {
            if (Map.EditorMode) return; // todo: dont run unit routines in editor mode
            if (dir == Vector2.Zero) return;

            float amount = SpeedPerMs * dt;
            var moveResult = Collision.IncrementalGenericSegmentCollision(dir * amount, Bounds, CollisionTest(this));

            if (moveResult.UnobstructedMovement != Vector2.Zero)
            {
                Position2 += moveResult.UnobstructedMovement;
                return;
            }

            Vector2 dirPerpen = dir.Perpendicular();
            moveResult = Collision.IncrementalGenericSegmentCollision(dirPerpen * amount, Bounds, CollisionTest(this));
            if (moveResult.UnobstructedMovement != Vector2.Zero)
            {
                Position2 += moveResult.UnobstructedMovement;
                return;
            }

            dirPerpen = -dirPerpen;
            moveResult = Collision.IncrementalGenericSegmentCollision(dirPerpen * amount, Bounds, CollisionTest(this));
            if (moveResult.UnobstructedMovement != Vector2.Zero)
            {
                Position2 += moveResult.UnobstructedMovement;
                return;
            }
        }

        protected override void UpdateInternal(float dt)
        {
            base.UpdateInternal(dt);

            if (_behaviorRoutine == null || _behaviorRoutine.Finished)
            {
                _behaviorRoutine = GetNextBehavior();
            }

            for (int i = _floatingTexts.Count - 1; i >= 0; i--)
            {
                var text = _floatingTexts[i];
                text.Timer.Update(dt);
                if (text.Timer.Finished)
                {
                    _floatingTexts.Remove(text);
                }
            }
        }

        protected override void RenderInternal(RenderComposer c)
        {
            c.RenderEllipse(Position.ToVec2().ToVec3(-5), new Vector2(Width, 3f), Color.Black * 0.3f, true);
            c.RenderSprite(Bounds.PositionZ(Z), Size, Tint);

            var abovePoint = Center - new Vector2(0, (Height / 2f) + 6);
            var hpBarSize = new Vector2(Width + Width * 0.8f, 5);

            var barRect = new Rectangle(0, 0, hpBarSize);
            barRect.Center = abovePoint;

            for (int i = 0; i < _floatingTexts.Count; i++)
            {
                var textInstance = _floatingTexts[i];

                float y = 30 * textInstance.Timer.Progress;
                float opacity = 1f;
                if (textInstance.Timer.Progress > 0.5f)
                {
                    opacity = 1.0f - ((textInstance.Timer.Progress - 0.5f) / 0.5f);
                }

                c.RenderString(textInstance.Position - new Vector3(0, y, 0), textInstance.Color * opacity, textInstance.Text,
                    FontAsset.GetDefaultBuiltIn().GetAtlas(10), null, Emotion.Graphics.Text.FontEffect.Outline, 0.9f, Color.Black * opacity);
            }

            float hpPercent = CurrentHp / (float)Hp;
            if (hpPercent > 0)
            {
                c.RenderSprite(barRect.Position + new Vector2(0.5f), barRect.Size - new Vector2(1f), new Color(32, 32, 32));
                c.RenderSprite(barRect.Position + new Vector2(0.5f), barRect.Size * new Vector2(hpPercent, 1f) - new Vector2(1f), Color.Green);
                c.RenderOutline(barRect, Color.Black);
            }
        }

        public bool IsDead()
        {
            return CurrentHp <= 0;
        }

        public void AddFloatingText(string text, Unit source, Unit target, Color? color)
        {
            Vector2 midPoint = target.Bounds.Center;
            if (source != target)
            {
                Vector2 dirTowardsSource = Vector2.Normalize(source.Bounds.Center - target.Bounds.Center);
                midPoint = midPoint + dirTowardsSource * target.Size / 2.3f;
            }

            _floatingTexts.Add(new FloatingText(text, midPoint.ToVec3(source.Z), color));
        }

        public IEnumerator AIBehaviorUseAbility(Ability ability, Unit target, int times = -1)
        {
            if (IsDead() || target.IsDead()) yield break;

            while (times > 0 || times == -1)
            {
                while (ability.IsOnCooldown())
                {
                    yield return null;
                    if (IsDead() || target.IsDead()) yield break;
                }

                var inRange = ability.InRangeToUseAbility(this, target);
                while (!inRange)
                {
                    Vector2 pointToMoveTo = target.Bounds.Center;
                    Vector2 dir = Vector2.Normalize(pointToMoveTo - Position2);
                    MoveDirection(dir, Engine.DeltaTime);

                    yield return null;
                    if (IsDead() || target.IsDead()) yield break;

                    inRange = ability.InRangeToUseAbility(this, target);
                }

                ability.ExecuteAbility(this, target);

                yield return null;
                if (IsDead() || target.IsDead()) yield break;

                times--;
            }
        }

        protected virtual Coroutine? GetNextBehavior()
        {
            if (IsDead())
            {
                return Engine.CoroutineManager.StartCoroutine(BehaviorDead());
            }

            return null;
        }

        public IEnumerator BehaviorDead()
        {
            Tint = new Color(62, 62, 62);
            (Width, Height) = (Height, Width);

            while (true)
            {
                yield return null;
            }
        }

        public virtual void TakeDamage(Unit source, float amount)
        {
            AddFloatingText($"-{amount:00}", source, this, source is PartyUnit ? Color.White : Color.PrettyRed);

            CurrentHp -= amount;
            if (CurrentHp < 0f)
            {
                CurrentHp = 0;
                return;
            }
        }

        public virtual void HealDamage(Unit source, float amount)
        {
            AddFloatingText($"+{amount:00}", this, this, Color.Green);
            CurrentHp = Maths.Clamp(CurrentHp + amount, 0, Hp);
        }

        public void ApplyAura(Unit caster, Aura aura)
        {
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
            aura.Init();
        }

        public void RemoveAura(Aura aura)
        {
            _auras.Remove(aura);
            aura.Dispose();
        }
    }
}
