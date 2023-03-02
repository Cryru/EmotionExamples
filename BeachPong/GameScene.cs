#region Using

using System.Numerics;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Game.Text;
using Emotion.Game.Time;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;

#endregion

namespace BeachPong
{
	public class GameScene : Scene
	{
		public Rectangle MapBounds;

		// Render and Update will never be called before LoadAsync has finished, or if it has faulted. Therefore this cannot be null.
		public Paddle LeftPaddle = null!;
		public Paddle RightPaddle = null!;
		public Ball Ball = null!;

		public AudioAsset? HitWallFx;
		public TextureAsset? Background;
		public TextureAsset? BackgroundTop;
		public TextureAsset? Water;
		public TextureAsset? WaterFoam;

		private Vector2 _paddleInput;
		private FontAsset? _font;

		// The timer used to animate the water tide. After will linearly progress while AfterAndBack is used for InOut animations.
		private AfterAndBack _waterAnimation = new AfterAndBack(3000);
		private After _waterAnimationCooldown = new After(500);

		public override async Task LoadAsync()
		{
			// Start loading assets.
			Task<FontAsset?> fontLoad = Engine.AssetLoader.GetAsync<FontAsset>("Editor/UbuntuMono-Regular.ttf");
			Task<AudioAsset?> ambianceLoad = Engine.AssetLoader.GetAsync<AudioAsset>("ambiance.wav");
			Task<AudioAsset?> hitLeftLoad = Engine.AssetLoader.GetAsync<AudioAsset>("hit_left.wav");
			Task<AudioAsset?> hitRightLoad = Engine.AssetLoader.GetAsync<AudioAsset>("hit_right.wav");
			Task<AudioAsset?> hitWallLoad = Engine.AssetLoader.GetAsync<AudioAsset>("hit_wall.wav");
			Task<TextureAsset?> backgroundLoad = Engine.AssetLoader.GetAsync<TextureAsset>("background.png");
			Task<TextureAsset?> backgroundTopLoad = Engine.AssetLoader.GetAsync<TextureAsset>("background_top.png");
			Task<TextureAsset?> waterLoad = Engine.AssetLoader.GetAsync<TextureAsset>("water.png");
			Task<TextureAsset?> waterFoamLoad = Engine.AssetLoader.GetAsync<TextureAsset>("water_foam.png");

			// Attach a callback to receive input. Callbacks are called in a priority order, with Game being the lowest one.
			// This allows for a game UI to consume inputs before they reach the game.
			Engine.Host.OnKey.AddListener(KeyHandler, KeyListenerType.Game);

			// The map bounds are in world space coordinates. Since we don't have camera movement we will instead take the
			// internal "RenderSize" as the size. This metric is equal to target resolution and therefore if we use it as a metric
			// the game should scale to all resolutions.
			const float marginLeft = 100;
			const float marginTop = 30;
			const float marginBottom = 70;
			MapBounds = new Rectangle(marginLeft, marginTop, Engine.Configuration.RenderSize - new Vector2(marginLeft * 2f, marginTop + marginBottom));

			// The camera position is in the center of the screen.
			Engine.Renderer.Camera.Position2 = new Rectangle(0, 0, Engine.Configuration.RenderSize).Center;

			const float paddleMarginLeft = 20;
			LeftPaddle = new Paddle(this);
			LeftPaddle.Center = new Vector2(MapBounds.X + paddleMarginLeft, MapBounds.Center.Y);

			RightPaddle = new Paddle(this);
			RightPaddle.Center = new Vector2(MapBounds.Right - paddleMarginLeft, MapBounds.Center.Y);

			Ball = new Ball(this);
			Task ballLoading = Ball.LoadAssets();
			LeftPaddle.AttachBall(Ball);

			// Wait for all assets to be loaded.
			// It doesn't matter in what order we await them as all thread should be fired.
			_font = await fontLoad;
			AudioAsset? ambianceMusic = await ambianceLoad;
			AudioAsset? hitLeftFx = await hitLeftLoad;
			AudioAsset? hitRightFx = await hitRightLoad;
			HitWallFx = await hitWallLoad;
			await ballLoading;
			Background = await backgroundLoad;
			if (Background?.Texture != null) Background.Texture.Smooth = true;
			BackgroundTop = await backgroundTopLoad;
			//if (BackgroundTop?.Texture != null) BackgroundTop.Texture.Smooth = true;
			Water = await waterLoad;
			if (Water?.Texture != null) Water.Texture.Smooth = true;
			WaterFoam = await waterFoamLoad;
			if (WaterFoam?.Texture != null) WaterFoam.Texture.Smooth = true;

			// Setup audio layers for the background music and paddle hit fx.
			if (ambianceMusic != null)
			{
				AudioLayer? bgmLayer = Engine.Audio.CreateLayer("BGM");
				bgmLayer.VolumeModifier = 0.5f;
				bgmLayer.AddToQueue(new AudioTrack(ambianceMusic)
				{
					CrossFade = 5,
					SetLoopingCurrent = true
				});
			}

			AudioLayer? fxLayer = Engine.Audio.CreateLayer("FX");
			fxLayer.VolumeModifier = 0.25f;
			LeftPaddle.HitFx = hitLeftFx;
			RightPaddle.HitFx = hitRightFx;
		}

		private bool KeyHandler(Key key, KeyStatus status)
		{
			// We check if the key is a part of the W/S axis (which we use to control the paddle up and down).
			// If the key is part of the axis it will return a non-zero vector specifying the position of the key
			// within the axis. The W/S axis is mapped to the Y axis, so it will either be Vector2(0, 1) for S or Vector2(0, -1) for W.
			// Other axes such as WASD will return a value in both the X and Y direction.
			Vector2 playerInput = Engine.Host.GetKeyAxisPart(key, Key.AxisWS);
			if (playerInput != Vector2.Zero)
			{
				// Based on whether the key is pressed up or down we add or subtract it's axis value frm the paddle input.
				if (status == KeyStatus.Down)
					_paddleInput += playerInput;
				else if (status == KeyStatus.Up)
					_paddleInput -= playerInput;

				return false; // We consume this input preventing it from reaching other listeners.
			}

			if (key == Key.Space && status == KeyStatus.Down) LeftPaddle.ShootBall();

			return true; // This means the key will propagate to further listeners.
		}

		// This is called in a semi-fixed time step, about once every 16 milliseconds by default.
		// Use Engine.DeltaTime to determine how much time has passed. It is possible for this function to be
		// called multiple times per frame.
		public override void Update()
		{
			LeftPaddle.ApplyInput(_paddleInput);
			RightPaddle.UpdateAI();
			Ball.Update();

			// Update the timer and if it finished, restart it in the opposite direction.
			_waterAnimation.Update(Engine.DeltaTime);
			if (_waterAnimation.Finished)
			{
				_waterAnimationCooldown.Update(Engine.DeltaTime);
				if (_waterAnimationCooldown.Finished)
				{
					_waterAnimation.GoInOpposite();
					_waterAnimationCooldown.Restart();
				}
			}
		}

		// Called for every frame. The RenderComposer class is used for immediate mode drawing here.
		public override void Draw(RenderComposer composer)
		{
			composer.RenderSprite(Vector3.Zero, Engine.Configuration.RenderSize, Color.White, Background?.Texture);

			// Animate water
			float waterFlowQuad = _waterAnimation.Progress * _waterAnimation.Progress;
			composer.RenderSprite(new Vector3(0, 20 * waterFlowQuad, 0), Engine.Configuration.RenderSize, Color.White, Water?.Texture);

			float foamAlpha = 1.0f - waterFlowQuad;
			composer.RenderSprite(new Vector3(0, 20 * waterFlowQuad, 0), Engine.Configuration.RenderSize, Color.White * foamAlpha, WaterFoam?.Texture);

			// Draw the net.
			var netColor = new Color(200, 200, 200);
			composer.RenderOutline(MapBounds, new Color(250, 250, 235));
			composer.RenderLine(new Vector2(MapBounds.Center.X, MapBounds.Y), new Vector2(MapBounds.Center.X + 10, MapBounds.Y - 20), netColor);
			composer.RenderLine(new Vector2(MapBounds.Center.X, MapBounds.Bottom), new Vector2(MapBounds.Center.X + 10, MapBounds.Bottom - 20), netColor);
			for (var i = 5; i <= 20; i += 5)
			{
				composer.RenderLine(new Vector2(MapBounds.Center.X + i / 2, MapBounds.Y - i), new Vector2(MapBounds.Center.X + i / 2, MapBounds.Bottom - i), netColor);
			}

			var line = 0;
			for (var i = 10; i <= 250; i += 10)
			{
				composer.RenderLine(new Vector2(MapBounds.Center.X + 2, MapBounds.Y + (i + (line * 10)) / 2), new Vector2(MapBounds.Center.X + 10, MapBounds.Y - 20 + i ), netColor);
				line++;
			}

			composer.RenderSprite(Vector3.Zero, Engine.Configuration.RenderSize, Color.White, BackgroundTop?.Texture);

			LeftPaddle.Render(composer);
			RightPaddle.Render(composer);
			Ball.Render(composer);

			// Disable the "ViewMatrix" which causes subsequent draws to be in screen space rather than world space.
			// This is mostly used for drawing UI.
			composer.SetUseViewMatrix(false);

			// We get the alas of the desired font size. Since we're drawing in screen space
			// we need to perform our own scaling. Multiplying by composer.Scale will ensure that it looks
			// the same on all resolutions with the same aspect ratio as the RenderSize. 
			// Future examples will show how to use the UI system to take care of this.
			DrawableFontAtlas? helpAtlas = _font?.GetAtlas(9 * composer.Scale);
			if (helpAtlas != null) // This should never realistically be null.
				composer.RenderString(new Vector3(5, 5, 0), Color.Black, "W/S - Move\nSpace - Serve", helpAtlas);

			DrawableFontAtlas? scoreAtlas = _font?.GetAtlas(15 * composer.Scale);
			if (scoreAtlas != null)
			{
				var layouter = new TextLayouter(scoreAtlas);

				Vector2 mapStartScreen = composer.Camera.WorldToScreen(MapBounds.Position.ToVec3());
				string scoreTextLeft = $"SCORE {LeftPaddle.Score}";
				Vector2 sizeLeftText = layouter.MeasureString(scoreTextLeft) + new Vector2(5, 5);
				composer.RenderString((mapStartScreen - new Vector2(0, sizeLeftText.Y)).ToVec3(), Color.Black, scoreTextLeft, scoreAtlas);

				// We need to manually calculate the text size and draw it further in in order for it's right edge to
				// reach the map edge. Future examples will show how to use the UI system to do this automatically.
				Vector2 mapEndScreen = composer.Camera.WorldToScreen(MapBounds.TopRight.ToVec3());
				string scoreTextRight = $"SCORE {RightPaddle.Score}";
				layouter.Restart();
				Vector2 sizeRightText = layouter.MeasureString(scoreTextRight) + new Vector2(5, 5);
				composer.RenderString((mapEndScreen - sizeRightText).ToVec3(), Color.Black, scoreTextRight, scoreAtlas);
			}
		}

		// This is called when our scene is changed.
		// We need to unload assets and detach from various events to prevent them from being invoked on a dead scene.
		public override void Unload()
		{
			Engine.Host.OnKey.RemoveListener(KeyHandler);

			// Note: We're not unloading the font asset here since it might be used by another scene.
			// This will be addressed in a future update of Emotion, but currently unloading an asset
			// unloads it for everyone who has loaded it currently and will break their references to it.

			base.Unload();
		}
	}
}