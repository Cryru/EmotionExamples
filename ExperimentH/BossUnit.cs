using Emotion.Common;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using ExperimentH.Combat;
using ExperimentH.CombatScript;
using Silk.NET.Core.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH
{

    public class BossUnit : Unit
    {
        public List<Unit> _threatTable = new();

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
            _abilities.Add(new BossFireSpit());
        }

        protected override Coroutine? GetNextBehavior()
        {
            var baseBehavior = base.GetNextBehavior();
            if (baseBehavior != null) return baseBehavior;

            var aggroUnit = GetAggroUnit();
            if (aggroUnit == null) return null;

            return Engine.CoroutineManager.StartCoroutine(AIBehaviorFightTarget(aggroUnit));
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
        protected void RenderBar(RenderComposer c, Rectangle barRect, Color color, float progress)
        {
            c.RenderSprite(barRect.Position + new Vector2(0.5f), barRect.Size - new Vector2(1f), new Color(32, 32, 32));
            c.RenderSprite(barRect.Position + new Vector2(0.5f), barRect.Size * new Vector2(progress, 1f) - new Vector2(1f), color);
            c.RenderOutline(barRect, Color.Black);
        }

        protected override void RenderInternal(RenderComposer c)
        {
            base.RenderInternal(c);

            var abovePoint = Center - new Vector2(0, (Height / 2f) + 6);
            var hpBarSize = new Vector2(Width + Width * 0.8f, 5);

            var barRect = new Rectangle(0, 0, hpBarSize);
            barRect.Center = abovePoint;

            float spaceBetweenBars = -(barRect.Height + 2);
            float extraBarPen = spaceBetweenBars;

            float hpPercent = CurrentHp / (float)Hp;
            if (hpPercent > 0)
            {
                RenderBar(c, barRect, Color.Green, hpPercent);
            }

            for (int i = 0; i < _auras.Count; i++)
            {
                var aura = _auras[i];
                float progress = 1.0f - ((float)aura.TimePassed / aura.Duration);

                if (!string.IsNullOrEmpty(aura.Icon))
                {
                    var auraIcon = Engine.AssetLoader.Get<TextureAsset>(aura.Icon);
                    auraIcon.Texture.Smooth = true;
                    c.RenderSprite(barRect.Position + new Vector2(extraBarPen, -8), new Vector2(8), Color.White, auraIcon.Texture);
                }
                else
                {
                    c.RenderSprite(barRect.Position + new Vector2(extraBarPen, -8), new Vector2(8), Color.PrettyPink);
                }

                extraBarPen += 9;
            }
        }
    }
}
