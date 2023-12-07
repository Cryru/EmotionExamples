using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.UI;
using ExperimentH.Combat;
using Silk.NET.Core.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExperimentH.UI
{
    public class UnitSkillBarAbility : UIBaseWindow
    {
        public Unit Unit;
        public Ability Ability;

        public UnitSkillBarAbility(Unit unit, Ability ability)
        {
            Unit = unit;
            Ability = ability;
        }

        protected override Vector2 InternalMeasure(Vector2 space)
        {
            return new Vector2(30, 30) * GetScale();
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            AbilityIcon.RenderAbility(c, Unit, Ability, Position, Size);

            return base.RenderInternal(c);
        }
    }

    public class UnitSkillBar : UIBaseWindow
    {
        public Unit Unit;

        public UnitSkillBar(Unit u)
        {
            Unit = u;
            Anchor = UIAnchor.BottomCenter;
            ParentAnchor = UIAnchor.BottomCenter;
            Margins = new Rectangle(0, 0, 0, 5);
        }

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);

            var abilityList = new UIBaseWindow();
            abilityList.LayoutMode = LayoutMode.HorizontalList;
            abilityList.ListSpacing = new Vector2(1, 0);
            abilityList.Paddings = new Emotion.Primitives.Rectangle(1, 1, 1, 1);
            abilityList.StretchX = true;
            abilityList.StretchY = true;
            abilityList.Margins = new Emotion.Primitives.Rectangle(0, 5, 0, 0);
            AddChild(abilityList);

            var abilities = Unit._abilities;
            for (int i = 0; i < abilities.Count; i++)
            {
                var ability = abilities[i];

                var abilityIcon = new UnitSkillBarAbility(Unit, ability);
                abilityList.AddChild(abilityIcon);
            }
        }

        protected override Vector2 InternalMeasure(Vector2 space)
        {
            int abilityCount = Unit._abilities.Count;
            return new Vector2(abilityCount * (30 + 2) + 2, 32 + 5) * GetScale();
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            float scale = GetScale();

            float casting = Unit.CastProgress;
            if(casting != 0f)
            {
                Rectangle barRect = new Rectangle(Position2, new Vector2(Width, 5 * scale));

                c.RenderSprite(barRect.Position + new Vector2(0.5f), barRect.Size - new Vector2(1f), new Color(32, 32, 32));
                c.RenderSprite(barRect.Position + new Vector2(0.5f), barRect.Size * new Vector2(casting, 1f) - new Vector2(1f), Color.PrettyYellow);
                c.RenderOutline(barRect, Color.Black);
            }

            return base.RenderInternal(c);
        }
    }
}
