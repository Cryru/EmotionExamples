#region Using

using System.Numerics;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Game.Time;
using Emotion.Game.World2D;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;

#endregion

namespace BeachPong_World2D.Objects;

public class Paddle : GameObject2D
{
	[DontSerialize] public Ball? AttachedBall;

	[DontSerialize] public int Score;

	[AssetFileName] public string? HitFx;
	public bool PlayerControlled;

	private AudioAsset? _hitFx;
	private Vector2 _paddleInput;

	public Paddle()
	{
		Size = new Vector2(5, 20);
	}

	public override async Task LoadAssetsAsync()
	{
		_hitFx = await Engine.AssetLoader.GetAsync<AudioAsset>(HitFx);
	}

	public override void Init()
	{
		base.Init();

		if (PlayerControlled)
			Engine.Host.OnKey.AddListener(ProcessInput, KeyListenerType.Game);
	}

	public override void Destroy()
	{
		base.Destroy();
		Engine.Host.OnKey.RemoveListener(ProcessInput);
	}

	private bool ProcessInput(Key key, KeyStatus status)
	{
		Vector2 playerInput = Engine.Host.GetKeyAxisPart(key, Key.AxisWS);
		if (playerInput != Vector2.Zero)
		{
			if (status == KeyStatus.Down)
				_paddleInput += playerInput;
			else if (status == KeyStatus.Up)
				_paddleInput -= playerInput;

			return false;
		}

		if (AttachedBall != null && key == Key.Space && status == KeyStatus.Down)
			ShootBall();

		return true;
	}

	protected override void RenderInternal(RenderComposer c)
	{
		c.RenderSprite(Position, Size, Color.White);
		c.RenderOutline(Position, Size, Color.Black);
	}

	protected override void UpdateInternal(float dt)
	{
		if (PlayerControlled)
			ApplyInput(_paddleInput);
		else
			UpdateAI();

		base.UpdateInternal(dt);
	}

	public void ApplyInput(Vector2 input)
	{
		float direction = input.Y;
		const float speed = 0.15f;

		// The speed signifies how much we want to move in a millisecond.
		// We then multiply it by Engine.DeltaTime to get how much time has passed since the
		// last update. This way our movement speed is uncoupled from the game's frame rate.
		Y += speed * Engine.DeltaTime * direction;

		var bbMap = (BeachBallMap) Map;
		Rectangle mapBounds = bbMap.MapBounds;
		if (Y < mapBounds.Y) Y = mapBounds.Y;
		if (Bounds.Bottom > mapBounds.Bottom) Y = mapBounds.Bottom - Height;
		UpdateAttachedBallPos();
	}

	public void Hit()
	{
		if (_hitFx != null)
		{
			AudioLayer? layer = Engine.Audio.GetLayer("FX");
			layer.QuickPlay(_hitFx);
		}
	}

	public void AttachBall(Ball ball)
	{
		ball.Velocity = Vector2.Zero;
		AttachedBall = ball;
		UpdateAttachedBallPos();
	}

	public void ShootBall()
	{
		if (AttachedBall == null) return;

		var bbMap = (BeachBallMap) Map;
		Rectangle mapBounds = bbMap.MapBounds;

		float direction = X < mapBounds.Center.X ? 1 : -1;
		AttachedBall.Velocity = new Vector2(direction, 0);
		AttachedBall = null;
	}

	protected void UpdateAttachedBallPos()
	{
		if (AttachedBall == null) return;
		var bbMap = (BeachBallMap) Map;
		Rectangle mapBounds = bbMap.MapBounds;

		float direction = X < mapBounds.Center.X ? 1 : -1;

		float ballOffset = AttachedBall.Width / 2f;
		AttachedBall.Center = Center + new Vector2(Width / 2 + ballOffset, 0) * direction;
	}

	private After _aiBallShootTimer = new After(500);
	private After _aiRecalculateCooldown = new After(30);
	private Vector2 _lastBallPosition;
	private Vector2 _ballPositionThrottled;

	public void UpdateAI()
	{
		var bbMap = (BeachBallMap) Map;
		Rectangle mapBounds = bbMap.MapBounds;
		var ball = bbMap.GetObjectByType<Ball>();
		if (ball == null) return;

		// The AI has the ball, wait some time before shooting it out.
		if (AttachedBall != null)
		{
			_aiBallShootTimer.Update(Engine.DeltaTime);
			if (!_aiBallShootTimer.Finished)
			{
				float diff = mapBounds.Center.Y - Center.Y;
				if (diff > 10) ApplyInput(new Vector2(0, MathF.Sign(diff)));
				return;
			}

			ShootBall();
			_lastBallPosition = Center;
			_aiBallShootTimer.Restart();
			return;
		}

		// We use a timer to slow down how often the AI will update its ball position knowledge.
		_aiRecalculateCooldown.Update(Engine.DeltaTime);
		if (_aiRecalculateCooldown.Finished)
		{
			_aiRecalculateCooldown.Restart();
			_lastBallPosition = _ballPositionThrottled;
			_ballPositionThrottled = ball.Center;
		}

		Vector2 newBallPos = _ballPositionThrottled; // _scene.Ball.Center;
		Vector2 ballMovingVector = Vector2.Normalize(newBallPos - _lastBallPosition);

		Vector2 myDirection = X < mapBounds.Center.X ? new Vector2(1, 0) : new Vector2(-1, 0);
		float dot = Vector2.Dot(ballMovingVector, myDirection);
		if (dot < 0) // Moving towards me
		{
			float distanceBallToMe = Vector2.Distance(newBallPos, Center);
			float dontMoveTooClose = (Width + ball.Width) * 2;
			float dontMoveTooFar = mapBounds.Width / 2f;
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