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
            return new Vector2(75, 39) * GetScale();
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            float hpPercent = Unit.CurrentHp / (float)Unit.Hp;

            Vector3 colorArea = new Vector3(5 * GetScale(), Height, 0);
            Vector2 colorArea2 = colorArea.ToVec2();
            Vector2 colorAreaWidth = new Vector2(colorArea.X, 0);
            Vector3 colorAreaWidth3 = colorAreaWidth.ToVec3();

            c.RenderSprite(Position, Size, new Color(32, 32, 32));
            c.RenderSprite(Position + new Vector3(1f), colorArea2 - new Vector2(0, 2), Unit.Tint);
            c.RenderSprite(Position + colorAreaWidth3 + new Vector3(2f, 1f, 0f), (Size - colorAreaWidth - new Vector2(3f, 2f)) * new Vector2(hpPercent, 1f), Color.Green);

            // Auras
            var auras = Unit._auras;

            float iconSize = 13 * GetScale();
            float pen = 0;
            for (int i = 0; i < auras.Count; i++)
            {
                var aura = auras[i];
                float progress = 1.0f - ((float)aura.TimePassed / aura.Duration);

                Texture? auraIcon = null;
                if (!string.IsNullOrEmpty(aura.Icon))
                {
                    var auraIconAsset = Engine.AssetLoader.Get<TextureAsset>(aura.Icon);
                    if (auraIconAsset != null)
                    {
                        if (!auraIconAsset.Texture.Smooth) auraIconAsset.Texture.Smooth = true;
                        auraIcon = auraIconAsset.Texture;
                    }
                }

                c.RenderSprite(Position + new Vector3(Width + 1, pen, 0), new Vector2(iconSize), Color.White, auraIcon);
                c.RenderSprite(Position + new Vector3(Width + 1, pen, 0), new Vector2(iconSize), Color.Black * 0.65f);
                RenderProgress(c, Position + new Vector3(Width + 1, pen, 0), new Vector2(iconSize), Color.White, progress, auraIcon);
                pen += iconSize + 1;
            }

            return base.RenderInternal(c);
        }

        private void RenderProgress(RenderComposer composer, Vector3 position, Vector2 radius, Color color, float progress, Texture? t = null)
        {
            float progressAsAngle = Maths.Map(progress, 1f, 0f, 0, 359);

            Span<VertexData> vertices = composer.RenderStream.GetStreamMemory(8 * 3, BatchMode.SequentialTriangles, t);
            Debug.Assert(vertices != null);

            uint c = color.ToUint();
            //for (var i = 0; i < vertices.Length; i++)
            //{
            //    vertices[i].Color = c;
            //    vertices[i].UV = Vector2.Zero;
            //    vertices[i].Vertex = Vector3.Zero;
            //}

            Vector3[] rectPoints = new Vector3[]
            {
                position + new Vector3(radius.X / 2f, 0, 0),
                position + new Vector3(radius.X, 0, 0),
                position + new Vector3(radius.X, radius.Y / 2f, 0),
                position + new Vector3(radius.X, radius.Y, 0),
                position + new Vector3(radius.X / 2f, radius.Y, 0),
                position + new Vector3(0, radius.Y, 0),
                position + new Vector3(0, radius.Y / 2f, 0),
                position + new Vector3(0, 0, 0),
                position + new Vector3(radius.X / 2f, 0, 0),
            };

            Vector2[] rectUvs = new Vector2[]
            {
                new Vector2(0.5f, 0),
                new Vector2(1f, 0),
                new Vector2(1f, 0.5f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0, 1f),
                new Vector2(0, 0.5f),
                new Vector2(0, 0),
                new Vector2(0.5f, 0),
            };

            int vertId = 0;
            for (int i = 0; i < 8; i++)
            {
                ref var v0 = ref vertices[vertId];
                ref var v1 = ref vertices[vertId + 1];
                ref var v2 = ref vertices[vertId + 2];

                v0.Color = c;
                v0.UV = rectUvs[i];
                v0.Vertex = rectPoints[i];

                v1.Color = c;
                v1.UV = rectUvs[i + 1];
                v1.Vertex = rectPoints[i + 1];

                v2.Color = c;
                v2.UV = new Vector2(0.5f);
                v2.Vertex = position + new Vector3(radius.X / 2f, radius.Y / 2f, 0);

                float percent = Maths.Map(progressAsAngle, i * 45, i * 45 + 45, 0, 1f);
                percent = Maths.Clamp01(percent);
                v0.Vertex = Vector3.Lerp(v0.Vertex, v1.Vertex, percent);
                v0.UV = Vector2.Lerp(v0.UV, v1.UV, percent);

                vertId += 3;
            }
        }
    }
}
