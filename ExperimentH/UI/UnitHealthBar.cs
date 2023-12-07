using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.UI;
using Emotion.Utility;
using ExperimentH.Combat;
using Silk.NET.Core.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH.UI
{
    public class UnitHealthBar : UIBaseWindow
    {
        public GameMap Map;
        public Unit Unit;

        public UnitHealthBar(GameMap m, Unit unit)
        {
            Map = m;
            Unit = unit;
            HandleInput = true;
            ChildrenHandleInput = false;
        }

        public override void OnMouseEnter(Vector2 mousePos)
        {
            base.OnMouseEnter(mousePos);
            Map.UIUnitTarget = Unit;
        }

        public override void OnMouseLeft(Vector2 mousePos)
        {
            base.OnMouseLeft(mousePos);
            Map.UIUnitTarget = null;
        }

        protected override Vector2 InternalMeasure(Vector2 space)
        {
            return new Vector2(75, 35) * GetScale();
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            float iconSize = 15 * GetScale();
            float hpPercent = Unit.CurrentHp / (float)Unit.Hp;

            Vector3 colorArea = new Vector3(5 * GetScale(), Height, 0);
            Vector2 colorArea2 = colorArea.ToVec2();
            Vector2 colorAreaWidth = new Vector2(colorArea.X, 0);
            Vector3 colorAreaWidth3 = colorAreaWidth.ToVec3();

            Vector2 barSize = new Vector2(Width, Height - iconSize);

            c.RenderSprite(Position, Size, Color.Black * 0.5f);
            c.RenderSprite(Position, barSize, new Color(32, 32, 32));
            c.RenderSprite(Position, colorArea2 + new Vector2(2, 0), new Color(32, 32, 32));
            c.RenderSprite(Position + new Vector3(1f), colorArea2 - new Vector2(0, 2), Unit.Tint);
            c.RenderSprite(Position + colorAreaWidth3 + new Vector3(2f, 1f, 0f), (barSize - colorAreaWidth - new Vector2(3f, 2f)) * new Vector2(hpPercent, 1f), Color.Green);

            // Auras
            var auras = Unit._auras;

            float pen = colorArea.X + 2;
            for (int i = 0; i < auras.Count; i++)
            {
                var aura = auras[i];
                AbilityIcon.RenderAura(c, aura, Position + new Vector3(pen, Height - iconSize, 0), new Vector2(iconSize));
                pen += iconSize + 1 * GetScale();
            }

            return base.RenderInternal(c);
        }
    }
}
