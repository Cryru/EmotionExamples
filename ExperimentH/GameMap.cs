using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Game.Time.Routines;
using Emotion.Game.World;
using Emotion.Game.World2D;
using Emotion.Graphics;
using Emotion.IO;
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
        public Unit? UIUnitTarget;

        protected List<Action<RenderComposer>> _drawables = new();
        protected List<FloatingText> _floatingTexts = new List<FloatingText>();

        protected override Task InitAsyncInternal()
        {
            UI?.Dispose();
            UI = new UIController();

            {
                var partyUI = new UIBaseWindow();
                partyUI.StretchX = true;
                partyUI.StretchY = true;
                partyUI.Anchor = UIAnchor.CenterLeft;
                partyUI.ParentAnchor = UIAnchor.CenterLeft;
                partyUI.Margins = new Rectangle(5, 0, 0, 0);
                partyUI.Paddings = new Rectangle(0, 0, 0, 0);

                //UISolidColor partyUIBG = new UISolidColor();
                //partyUI.WindowColor = Color.Black * 0.5f;
                //partyUI.AddChild(partyUIBG);

                var hpBarContainer = new UIBaseWindow();
                hpBarContainer.StretchX = true;
                hpBarContainer.StretchY = true;
                hpBarContainer.LayoutMode = LayoutMode.VerticalList;
                hpBarContainer.Id = "HPContainer";
                hpBarContainer.ListSpacing = new Vector2(0, 5);
                hpBarContainer.IgnoreParentColor = true;
                partyUI.AddChild(hpBarContainer);
                UI.AddChild(partyUI);
            }

            return base.InitAsyncInternal();
        }

        public override void Update(float dt)
        {
            for (int i = _floatingTexts.Count - 1; i >= 0; i--)
            {
                var text = _floatingTexts[i];
                text.Timer.Update(Engine.DeltaTime);
                if (text.Timer.Finished)
                {
                    _floatingTexts.Remove(text);
                }
            }

            UI.Update();
            base.Update(dt);
        }

        public override void Render(RenderComposer c)
        {
            if (!Initialized) return;

            c.SetUseViewMatrix(false);
            c.RenderSprite(Vector3.Zero, c.CurrentTarget.Size, Color.PrettyGreen);
            c.ClearDepth();
            c.SetUseViewMatrix(true);

            Rectangle clipArea = c.Camera.GetCameraFrustum();
            TileData?.RenderTileMap(c, clipArea);

            var renderObjectsList = new List<BaseGameObject>();
            GetObjects(renderObjectsList, 0, clipArea);
            renderObjectsList.Sort(ObjectComparison);
            for (var i = 0; i < renderObjectsList.Count; i++)
            {
                BaseGameObject obj = renderObjectsList[i];
                if (obj is Unit u)
                {
                    u.RenderShadow(c);
                }
            }

            for (int i = 0; i < _drawables.Count; i++)
            {
                var drawable = _drawables[i];
                drawable(c);
            }

            for (var i = 0; i < renderObjectsList.Count; i++)
            {
                BaseGameObject obj = renderObjectsList[i];
                obj.Render(c);
            }

            for (int i = 0; i < _floatingTexts.Count; i++)
            {
                var textInstance = _floatingTexts[i];

                float y = 30 * textInstance.Timer.Progress;
                float opacity = 1f;
                if (textInstance.Timer.Progress > 0.5f)
                {
                    opacity = 1.0f - ((textInstance.Timer.Progress - 0.5f) / 0.5f);
                }

                c.RenderString(textInstance.Position - new Vector3(0, y, 0), textInstance.Color * opacity, textInstance.Text,
                    FontAsset.GetDefaultBuiltIn().GetAtlas(8), null, Emotion.Graphics.Text.FontEffect.Outline, 0.6f, Color.Black * opacity);
            }

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

        public void AddUnitSkillBar(Unit u)
        {
            var abilityBar = new UnitSkillBar(u);
            UI.AddChild(abilityBar);
        }

        #endregion

        #region FX

        public void AddFloatingText(string text, Unit source, Unit target, Color? color)
        {
            Vector2 midPoint = target.Bounds.Center - new Vector2(7, 0);
            if (source != target)
            {
                Vector2 dirTowardsSource = Vector2.Normalize(source.Bounds.Center - target.Bounds.Center);
                midPoint = midPoint + dirTowardsSource * target.Size / 2.3f;
            }

            _floatingTexts.Add(new FloatingText(text, midPoint.ToVec3(source.Z), color));
        }

        public void RegisterDrawable(Action<RenderComposer> drawFunc)
        {
            _drawables.Add(drawFunc);
        }

        public void UnregisterDrawable(Action<RenderComposer> drawFunc)
        {
            _drawables.Remove(drawFunc);
        }

        #endregion
    }
}
