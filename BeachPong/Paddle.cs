#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Game.Time;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace BeachPong;

public class Paddle : Transform
{
	public Ball? Ball;
	public int Score;
	public AudioAsset? HitFx;

	private GameScene _scene;

	public Paddle(GameScene scene)
	{
		Size = new Vector2(5, 20);
		_scene = scene;
	}

	public void Render(RenderComposer c)
	{
		c.RenderSprite(Position, Size, Color.White);
		c.RenderOutline(Position, Size, Color.Black);
	}

	public void ApplyInput(Vector2 input)
	{
		float direction = input.Y;
		const float speed = 0.15f;

		// The speed signifies how much we want to move in a millisecond.
		// We then multiply it by Engine.DeltaTime to get how much time has passed since the
		// last update. This way our movement speed is uncoupled from the game's frame rate.
		Y += speed * Engine.DeltaTime * direction;

		if (Y < _scene.MapBounds.Y) Y = _scene.MapBounds.Y;
		if (Bounds.Bottom > _scene.MapBounds.Bottom) Y = _scene.MapBounds.Bottom - Height;
		UpdateAttachedBallPos();
	}

	public void AttachBall(Ball ball)
	{
		ball.Velocity = Vector2.Zero;
		Ball = ball;
		UpdateAttachedBallPos();
	}

	public void ShootBall()
	{
		if (Ball == null) return;
		Ball.Velocity = new Vector2(X < _scene.MapBounds.Center.X ? 1 : -1, 0);
		Ball = null;
	}

	protected void UpdateAttachedBallPos()
	{
		if (Ball != null)
		{
			float ballOffset = Ball.Width / 2f;
			if (X < _scene.MapBounds.Center.X)
				Ball.Center = Center + new Vector2(Width / 2 + ballOffset, 0);
			else
				Ball.Center = Center - new Vector2(Width / 2 + ballOffset, 0);
		}
	}

	private After _aiBallShootTimer = new After(500);
	private After _aiRecalculateCooldown = new After(30);
	private Vector2 _lastBallPosition;
	private Vector2 _ballPositionThrottled;

	public void UpdateAI()
	{
		// The AI has the ball, wait some time before shooting it out.
		if (Ball != null)
		{
			_aiBallShootTimer.Update(Engine.DeltaTime);
			if (!_aiBallShootTimer.Finished)
			{
				float diff = _scene.MapBounds.Center.Y - Center.Y;
				if(diff > 10) ApplyInput(new Vector2(0, MathF.Sign(diff)));
				return;
			}

			ShootBall();
			_lastBallPosition = _scene.Ball.Center;
			_aiBallShootTimer.Restart();
			return;
		}

		// We use a timer to slow down how often the AI will update its ball position knowledge.
		_aiRecalculateCooldown.Update(Engine.DeltaTime);
		if (_aiRecalculateCooldown.Finished)
		{
			_aiRecalculateCooldown.Restart();
			_lastBallPosition = _ballPositionThrottled;
			_ballPositionThrottled = _scene.Ball.Center;
		}

		Vector2 newBallPos = _ballPositionThrottled; // _scene.Ball.Center;
		Vector2 ballMovingVector = Vector2.Normalize(newBallPos - _lastBallPosition);

		Vector2 myDirection = X < _scene.MapBounds.Center.X ? new Vector2(1, 0) : new Vector2(-1, 0);
		float dot = Vector2.Dot(ballMovingVector, myDirection);
		if (dot < 0) // Moving towards me
		{
			float distanceBallToMe = Vector2.Distance(newBallPos, Center);
			float dontMoveTooClose = (Width + _scene.Ball.Width) * 2;
			float dontMoveTooFar = _scene.MapBounds.Width / 2f;
			if (distanceBallToMe > dontMoveTooClose && distanceBallToMe < dontMoveTooFar)
			{
				Vector2 ballEndPos = newBallPos + ballMovingVector * distanceBallToMe;
				Vector2 movementVectorToMeet = ballEndPos - Center;
				if (movementVectorToMeet.Length() > 10)
					ApplyInput(new Vector2(0, MathF.Sign(movementVectorToMeet.Y)));
			}
		}
	}
}