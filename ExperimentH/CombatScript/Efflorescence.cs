using Emotion.Common;
using Emotion.Game.World2D;
using Emotion.Primitives;
using Emotion.Utility;
using ExperimentH.Combat;
using System.Numerics;

namespace ExperimentH.CombatScript
{
    public class Efflorescence : Ability
    {
        private EfflorescenceUnit? _efloUnit;

        public Efflorescence()
        {
            Range = 9999;
            Icon = "Icons/FireSpit.png";
        }

        public class EfflorescenceUnit : AoEEffectUnit
        {
            private Unit Caster;

            public EfflorescenceUnit(Unit caster)
            {
                Duration = 15000;
                TickRate = 250;
                Radius = 20;
                Caster = caster;
                Tint = Color.Green * 0.6f;
            }

            protected override void OnTick()
            {
                var unitsInMe = GetUnitsInside();
                for (int i = 0; i < unitsInMe.Count; i++)
                {
                    var unit = unitsInMe[i];
                    if (unit is PartyUnit or PlayerUnit)
                    {
                        unit.HealDamage(Caster, 5);
                    }
                }
            }
        }

        protected override void ExecuteAbilityInternal(Unit caster, Unit target)
        {
            if (_efloUnit != null && _efloUnit.ObjectState == Emotion.Game.World.ObjectState.Alive)
            {
                caster.Map.RemoveObject(_efloUnit);
            }

            var pos = Engine.Host.MousePosition;
            var worldPos = Engine.Renderer.Camera.ScreenToWorld(pos);
            _efloUnit = new EfflorescenceUnit(caster) { Position = worldPos };
            caster.Map.AddObject(_efloUnit);
        }
    }
}
