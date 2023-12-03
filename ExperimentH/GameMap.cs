using Emotion.Common.Serialization;
using Emotion.Game.Time.Routines;
using Emotion.Game.World2D;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.UI;
using ExperimentH.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH
{
    public class GameMap : Map2D
    {
        [DontSerialize]
        public UIController UI = null!;

        [DontSerialize]
        public CoroutineManager CoroutineManager = null!;

        [DontSerialize]
        public Unit? UIUnitTarget;

        protected override Task InitAsyncInternal()
        {
            UI?.Dispose();
            UI = new UIController();

            {
                var partyUI = new UISolidColor();
                partyUI.StretchX = true;
                partyUI.StretchY = true;
                partyUI.Anchor = UIAnchor.CenterLeft;
                partyUI.ParentAnchor = UIAnchor.CenterLeft;
                partyUI.Margins = new Rectangle(5, 0, 0, 0);
                partyUI.Paddings = new Rectangle(0, 0, 0, 0);

                //UISolidColor partyUIBG = new UISolidColor();
                partyUI.WindowColor = Color.Black * 0.5f;
                //partyUI.AddChild(partyUIBG);

                var hpBarContainer = new UIBaseWindow();
                hpBarContainer.StretchX = true;
                hpBarContainer.StretchY = true;
                hpBarContainer.LayoutMode = LayoutMode.VerticalList;
                hpBarContainer.Id = "HPContainer";
                hpBarContainer.ListSpacing = new Vector2(0, 2);
                hpBarContainer.IgnoreParentColor = true;
                partyUI.AddChild(hpBarContainer);
                UI.AddChild(partyUI);
            }

            CoroutineManager = new CoroutineManager();

            return base.InitAsyncInternal();
        }

        public override void Update(float dt)
        {
            if (!EditorMode) CoroutineManager.Update();
            UI.Update();
            base.Update(dt);
        }

        public override void Render(RenderComposer c)
        {
            c.SetUseViewMatrix(false);
            c.RenderSprite(Vector3.Zero, c.CurrentTarget.Size, Color.PrettyGreen);
            c.ClearDepth();
            c.SetUseViewMatrix(true);

            base.Render(c);

            c.SetUseViewMatrix(false);
            c.ClearDepth();
            UI.Render(c);
        }

        #region Game UI Stuff

        public void AddHealthBarToPartyUI(Unit u)
        {
            var hpBar = new UnitHealthBar(this, u);
            var container = UI.GetWindowById("HPContainer");
            container?.AddChild(hpBar);
        }

        #endregion
    }
}
