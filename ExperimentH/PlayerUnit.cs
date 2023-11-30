using Emotion.Common;
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

        private Unit? _targetUnderMouse;

        private Ability _rejuv;
        private Ability _healTouch;
        private Ability _lifebloom;

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
        }

        public override void Destroy()
        {
            Engine.Host.OnKey.RemoveListener(PlayerInput);
        }

        protected override void UpdateInternal(float dt)
        {
            base.UpdateInternal(dt);
            MoveDirection(_inputDirection, dt);

            _targetUnderMouse = null;
            var mouseWorld = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition);
            var objs = new List<GameObject2D>();
            Map.GetObjects(objs, 0, new Circle(mouseWorld.ToVec2(), 1f));
            for (int i = 0; i < objs.Count; i++)
            {
                var obj = objs[i];
                if (obj is Unit unit && !unit.IsDead() && unit is not BossUnit)
                {
                    _targetUnderMouse = unit;
                    break;
                }
            }
        }

        protected override void RenderInternal(RenderComposer c)
        {
            base.RenderInternal(c);

            if (_targetUnderMouse != null && !_targetUnderMouse.IsDead())
            {
                c.RenderCircleOutline(_targetUnderMouse.Center.ToVec3(100), _targetUnderMouse.Height, Color.Cyan, true);
            }
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

            if (status == KeyStatus.Up && _targetUnderMouse != null)
            {
                if (key == Key.Num1)
                {
                    _rejuv.ExecuteAbility(this, _targetUnderMouse);
                }
                else if (key == Key.Num2)
                {
                    _healTouch.ExecuteAbility(this, _targetUnderMouse);
                }
                else if (key == Key.Num3)
                {
                    _lifebloom.ExecuteAbility(this, _targetUnderMouse);
                }
            }

            return true;
        }
    }
}
