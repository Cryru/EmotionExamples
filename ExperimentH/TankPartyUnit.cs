using Emotion.Game.Time.Routines;
using Emotion.Primitives;
using ExperimentH.CombatScript;
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
            Hp = 350;
            Armor = 10;
            Strength = 10;

            _abilities.Add(new TankShield());
        }
    }
}
