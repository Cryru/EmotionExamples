#region Using

using BeachPong_World2D.Objects;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Editor;
using Emotion.Game.World2D;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.IO;
using Emotion.Primitives;

#endregion

#nullable enable

namespace BeachPong_World2D;

public class BeachBallMap : Map2D
{
    public Rectangle MapBounds;

    [AssetFileName<AudioAsset>]
    public string AmbientMusicFile;

    private AudioAsset? _ambientBgm;

    protected override async Task PostMapLoad()
    {
        // Setup audio
        _ambientBgm = await Engine.AssetLoader.GetAsync<AudioAsset>(AmbientMusicFile);

        AudioLayer? bgmLayer = Engine.Audio.CreateLayer("BGM");
        bgmLayer.VolumeModifier = 0.4f;

        AudioLayer? fxLayer = Engine.Audio.CreateLayer("FX");
        fxLayer.VolumeModifier = 0.30f;

        if (_ambientBgm != null)
            bgmLayer.AddToQueue(new AudioTrack(_ambientBgm)
            {
                CrossFade = 5,
                SetLoopingCurrent = true
            });

        // Setup map bounds and initial game state.
        var net = GetObjectByType<VolleyBallNet>();
        MapBounds = net?.Bounds ?? Rectangle.Empty;

        var ball = GetObjectByType<Ball>();
        if (ball != null)
            foreach (Paddle paddle in GetObjectsByType<Paddle>())
            {
                if (paddle.PlayerControlled) paddle.AttachBall(ball);
            }
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        var ball = GetObjectByType<Ball>();
        if (ball == null) return;

        foreach (Paddle paddle in GetObjectsByType<Paddle>())
        {
            ball.CollideWithPaddle(paddle);
        }
    }
}