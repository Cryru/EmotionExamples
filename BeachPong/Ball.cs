#region Using

using System.Numerics;
using System.Reflection.Metadata;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;

#endregion

namespace BeachPong;

public class Ball : Transform
{
	public Vector2 Velocity;

	private GameScene _scene;
	private int _timesJumped;

	private TextureAsset? _beachBall;
	private Vector2[] _previousPositions = new Vector2[10];
	private int _previousPositionPointer = 0;

	public Ball(GameScene scene)
	{
		Size = new Vector2(16, 16);
		_scene = scene;
	}

	public Task LoadAssets()
	{
		return Engine.AssetLoader.GetAsync<TextureAsset>("beach_ball.png").ContinueWith(asset =>
		{
			if (asset.Result != null)
			{
				_beachBall = asset.Result;
				_beachBall.Texture.Smooth = true;
			}
		});
	}

	public void Render(RenderComposer c)
	{
		c.RenderSprite(Position, Size, Color.White, _beachBall?.Texture);
	}

	public void Update()
	{
		// Note: updating the position all at once like this might cause tunneling - as in the
		// ball might teleport through the paddle. There are better ways of handling this kind of collision,
		// some of which are implemented in the Engine, but not within the scope of the example game.
		const float speed = 0.10f;
		Position2 += Velocity * (speed + 0.02f * _timesJumped) * Engine.DeltaTime;

		CollideWithPaddle(_scene.LeftPaddle);
		CollideWithPaddle(_scene.RightPaddle);

		// Collide with the map.
		Rectangle mapBounds = _scene.MapBounds;
		if (Center.X < mapBounds.X)
		{
			_scene.RightPaddle.Score++;
			_scene.LeftPaddle.AttachBall(this);
			_timesJumped = 0;
		}

		if (Center.X > mapBounds.Right)
		{
			_scene.LeftPaddle.Score++;
			_scene.RightPaddle.AttachBall(this);
			_timesJumped = 0;
		}

		var hitWall = false;
		if (Center.Y < mapBounds.Y)
		{
			Vector2 center = Center;
			center.Y = mapBounds.Y;
			Center = center;
			Velocity *= new Vector2(1, -1); // Reverse vertical velocity.
			hitWall = true;
		}

		if (Center.Y > mapBounds.Bottom)
		{
			Vector2 center = Center;
			center.Y = mapBounds.Bottom;
			Center = center;
			Velocity *= new Vector2(1, -1); // Reverse vertical velocity.
			hitWall = true;
		}

		if (hitWall && _scene.HitWallFx != null)
		{
			AudioLayer? layer = Engine.Audio.GetLayer("FX");
			layer.QuickPlay(_scene.HitWallFx);
		}
	}

	private void CollideWithPaddle(Paddle paddle)
	{
		if (Bounds.Intersects(paddle.Bounds))
		{
			const float diagonalVelocityPaddlePercent = 0.05f;
			float thresholdSize = paddle.Height * diagonalVelocityPaddlePercent;
			float upperThreshold = paddle.Y + thresholdSize;
			float lowerThreshold = paddle.Y + paddle.Height - thresholdSize;

			// If the ball hit the upper or lower part 5% of the paddle then
			// generate vertical velocity for it. This makes the game more interesting.
			float direction = paddle.X < _scene.MapBounds.Center.X ? 1 : -1;
			if (Y < upperThreshold)
				Velocity = new Vector2(direction, -1);
			else if (Y > lowerThreshold)
				Velocity = new Vector2(direction, 1);
			else
				Velocity = new Vector2(direction, 1);

			Velocity = Vector2.Normalize(Velocity);
			_timesJumped++;

			if (paddle.HitFx != null)
			{
				AudioLayer? layer = Engine.Audio.GetLayer("FX");
				layer.QuickPlay(paddle.HitFx);
			}
		}
	}
}