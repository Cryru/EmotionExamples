using Emotion.Game.World2D;
using Emotion.Graphics;
using Emotion.Primitives;
using System;
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
        public float SpeedPerMs = 0.06f;

        public Unit()
        {
            Size = new System.Numerics.Vector2(10, 20);
            Tint = Color.PrettyOrange;
        }

        public void MoveDirection(Vector2 dir, float dt)
        {
            Position2 += dir * SpeedPerMs * dt;
        }
    }
}
