using Emotion.Common;
using Emotion.Game.World3D;
using Emotion.Game.World3D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiniCom
{
    public class GameScene : World3DBaseScene<MiniComMap>
    {
        public override async Task LoadAsync()
        {
            var cam3D = new Camera3D(new Vector3(100));
            cam3D.LookAtPoint(Vector3.Zero);
            Engine.Renderer.Camera = cam3D;

            var gameMap = await Engine.AssetLoader.GetAsync<XMLAsset<MiniComMap>>("game_map.xml");
            if (gameMap?.Content != null) await ChangeMapAsync(gameMap.Content);
        }

        public override void Draw(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
            composer.ClearDepth();
            composer.SetUseViewMatrix(true);

            base.Draw(composer);
        }
    }
}
