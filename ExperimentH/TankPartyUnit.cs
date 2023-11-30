using Emotion.Primitives;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH
{
    public class TankPartyUnit : PartyUnit
    {
        public TankPartyUnit()
        {
            Tint = Color.Red;
            Hp = 300;
            Armor = 5;
        }
    }
}
