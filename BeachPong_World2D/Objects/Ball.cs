#region Using

using System.Numerics;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Editor;
using Emotion.Game.World2D;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace BeachPong_World2D.Objects;

public class Ball : GameObject2D
{
	[DontSerialize] public Vector2 Velocity;

	[AssetFileName<AudioAsset>] public string? HitWallFx;

	private int _timesJumped;

	private TextureAsset? _beachBall;
	private AudioAsset? _hitWallFx;

	public Ball()
	{
		Size = new Vector2(16, 16);
	}

	public override async Task LoadAssetsAsync()
	{
		var ballAsset = await Engine.AssetLoader.GetAsync<TextureAsset>("beach_ball.png");

		if (ballAsset != null)
		{
			ballAsset.Texture.Smooth = true;
			_beachBall = ballAsset;
		}

		_hitWallFx = await Engine.AssetLoader.GetAsync<AudioAsset>(HitWallFx);
	}

	protected override void RenderInternal(RenderComposer c)
	{
		c.RenderSprite(Position, Size, Color.White, _beachBall?.Texture);
	}

	protected override void UpdateInternal(float dt)
	{
		// Note: updating the position all at once like this might cause tunneling - as in the
		// ball might teleport through the paddle. There are better ways of handling this kind of collision,
		// some of which are implemented in the Engine, but not within the scope of the example game.
		const float speed = 0.10f;
		Position2 += Velocity * (speed + 0.02f * _timesJumped) * Engine.DeltaTime;

		var bbMap = (BeachBallMap) Map;
		Rectangle mapBounds = bbMap.MapBounds;

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

		if (hitWall && _hitWallFx != null)
		{
			AudioLayer? layer = Engine.Audio.GetLayer("FX");
			layer.QuickPlay(_hitWallFx);
		}

		// Check if scoring.
		float scoredDirection = 0;
		if (Center.X < mapBounds.X)
			scoredDirection = 1; // Scored right
		else if (Center.X > mapBounds.Right)
			scoredDirection = -1; // Scored left

		if (scoredDirection != 0)
		{
			foreach (Paddle paddle in Map.GetObjectsByType<Paddle>())
			{
				float direction = paddle.X < mapBounds.Center.X ? 1 : -1;
				if (scoredDirection == direction)
					paddle.AttachBall(this);
				else
					paddle.Score++;

				paddle.ResetToStartPos();
			}

			_timesJumped = 0;
		}
	}

	public void CollideWithPaddle(Paddle paddle)
	{
		var bbMap = (BeachBallMap) Map;
		Rectangle mapBounds = bbMap.MapBounds;
		float direction = paddle.X < mapBounds.Center.X ? 1 : -1;

		if (!Bounds.Intersects(paddle.Bounds)) return;

		const float diagonalVelocityPaddlePercent = 0.33f;
		float thresholdSize = paddle.Height * diagonalVelocityPaddlePercent;
		float upperThreshold = paddle.Y + thresholdSize;
		float lowerThreshold = paddle.Y + paddle.Height - thresholdSize;

		float ballCenterY = Center.Y;

		// If the ball hit the upper or lower part (diagonalVelocityPaddlePercent%) of the paddle then
		// generate vertical velocity for it. This makes the game more interesting.
		if (ballCenterY < upperThreshold)
			Velocity = new Vector2(direction, -1);
		else if (ballCenterY > lowerThreshold)
			Velocity = new Vector2(direction, 1);
		else
			Velocity = new Vector2(direction, 0);

		Velocity = Vector2.Normalize(Velocity);
		_timesJumped++;
		_timesJumped = Maths.Clamp(_timesJumped, 0, 10);
		paddle.Hit();
	}
}