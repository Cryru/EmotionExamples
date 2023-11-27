using Emotion.Common;
using Emotion.Platform.Input;
using Emotion.Primitives;
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

        public PlayerUnit()
        {
            Tint = Color.PrettyPurple;
        }

        public override void Init()
        {
            Engine.Host.OnKey.AddListener(PlayerInput);
        }

        public override void Destroy()
        {
            Engine.Host.OnKey.RemoveListener(PlayerInput);
        }

        protected override void UpdateInternal(float dt)
        {
            MoveDirection(_inputDirection, dt);
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

            return true;
        }
    }
}
