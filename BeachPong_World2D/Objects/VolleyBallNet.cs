#region Using

using System.Numerics;
using Emotion.Game.World2D;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace BeachPong_World2D.Objects
{
	public class VolleyBallNet : GameObject2D
	{
		public Color OutlineColor = new Color(250, 250, 235);
		public Color Color = new Color(200, 200, 200);

		protected override void RenderInternal(RenderComposer c)
		{
			Rectangle MapBounds = Bounds;

			c.RenderOutline(MapBounds, OutlineColor);
			c.RenderLine(new Vector2(MapBounds.Center.X, MapBounds.Y), new Vector2(MapBounds.Center.X + 10, MapBounds.Y - 20), Color);
			c.RenderLine(new Vector2(MapBounds.Center.X, MapBounds.Bottom), new Vector2(MapBounds.Center.X + 10, MapBounds.Bottom - 20), Color);
			for (var i = 5; i <= 20; i += 5)
			{
				c.RenderLine(new Vector2(MapBounds.Center.X + i / 2, MapBounds.Y - i), new Vector2(MapBounds.Center.X + i / 2, MapBounds.Bottom - i), Color);
			}

			var line = 0;
			for (var i = 10; i <= Height - 10; i += 10)
			{
				c.RenderLine(new Vector2(MapBounds.Center.X + 2, MapBounds.Y + (i + line * 10) / 2), new Vector2(MapBounds.Center.X + 10, MapBounds.Y - 20 + i), Color);
				line++;
			}
		}
	}
}