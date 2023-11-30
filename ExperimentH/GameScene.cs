using Emotion.Common;
using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH
{
    public class GameScene : World2DBaseScene<Map2D>
    {
        public override async Task LoadAsync()
        {
            var gameMap = await Engine.AssetLoader.GetAsync<XMLAsset<Map2D>>("game_map.xml");
            if (gameMap?.Content != null) await ChangeMapAsync(gameMap.Content);
        }

        public override void Draw(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.PrettyGreen);
            composer.ClearDepth();
            composer.SetUseViewMatrix(true);

            base.Draw(composer);
        }
    }
}