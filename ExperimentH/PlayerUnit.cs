using Emotion.Common;
using Emotion.Game.Time;
using Emotion.Game.World2D;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Utility;
using ExperimentH.Combat;
using ExperimentH.CombatScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH
{
    public class PlayerUnit : Unit
    {
        private Vector2 _inputDirection;

        private Ability _rejuv;
        private Ability _healTouch;
        private Ability _lifebloom;

        private Action? _queuedAbility;
        private After? _queueTime;

        public PlayerUnit()
        {
            Tint = Color.PrettyPurple;
            _lifebloom = new Lifebloom();
            _rejuv = new Rejuvenation();
            _healTouch = new HealingTouch();
        }

        public override void Init()
        {
            Engine.Host.OnKey.AddListener(PlayerInput);
            base.Init();

            if (Map is GameMap gameMap)
            {
                gameMap.AddHealthBarToPartyUI(this);
            }
        }

        public override void Destroy()
        {
            Engine.Host.OnKey.RemoveListener(PlayerInput);
        }

        protected override void UpdateInternal(float dt)
        {
            base.UpdateInternal(dt);
            MoveDirection(_inputDirection, SpeedPerMs, dt);

            _queueTime?.Update(dt);
            _queuedAbility?.Invoke();

            Unit targetUnderMouse = null;
            var mouseWorld = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition);
            var objs = new List<GameObject2D>();
            Map.GetObjects(objs, 0, new Circle(mouseWorld.ToVec2(), 1f));
            for (int i = 0; i < objs.Count; i++)
            {
                var obj = objs[i];
                if (obj is Unit unit && !unit.IsDead() && unit is not BossUnit)
                {
                    targetUnderMouse = unit;
                    break;
                }
            }
        }

        protected override void RenderInternal(RenderComposer c)
        {
            base.RenderInternal(c);

            //if (_targetUnderMouse != null && !_targetUnderMouse.IsDead())
            //{
            //    c.RenderCircleOutline(_targetUnderMouse.Center.ToVec3(100), _targetUnderMouse.Height / 2f, Color.Cyan, true);
            //}
        }

        protected bool PlayerInput(Key key, KeyStatus status)
        {
            Vector2 keyAxisPart = Engine.Host.GetKeyAxisPart(key, Key.AxisWASD);
            if (keyAxisPart != Vector2.Zero)
            {
                if (status == KeyStatus.Down)
                    _inputDirection += keyAxisPart;
                else if (status == KeyStatus.Up)
                    _inputDirection -= keyAxisPart;
                return false;
            }

            Unit? uiTargetUnit = null;
            if (Map is GameMap gm)
            {
                uiTargetUnit = gm.UIUnitTarget;
            }

            if (status == KeyStatus.Up && uiTargetUnit != null)
            {
                if (key == Key.Num1)
                {
                    InputUseAbility(_rejuv, uiTargetUnit);
                }
                else if (key == Key.Num2)
                {
                    InputUseAbility(_healTouch, uiTargetUnit);
                }
                else if (key == Key.Num3)
                {
                    InputUseAbility(_lifebloom, uiTargetUnit);
                }
            }

            return true;
        }

        public void InputUseAbility(Ability ability, Unit target)
        {
            if (CanUseAbility(ability, target) == AbilityReason.OnCooldown)
            {
                _queueTime = new After(400, () =>
                {
                    _queuedAbility = null;
                    _queueTime = null;
                });
                _queuedAbility = () =>
                {
                    if (UseAbility(ability, target))
                    {
                        _queuedAbility = null;
                        _queueTime = null;
                    }
                };
                return;
            }
            else
            {
                _queuedAbility = null;
                _queueTime = null;
            }

            UseAbility(ability, target);
        }
    }
}
