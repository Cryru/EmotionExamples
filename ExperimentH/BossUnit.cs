using Emotion.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH
{
    public class BossUnit : Unit
    {
        public BossUnit()
        {
            Hp = 10_000;
            Size = new System.Numerics.Vector2(70, 80);
            Tint = new Color(139, 69, 19);
        }
    }
}
