using Emotion.Common;
using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentH
{
    public class GameScene : World2DBaseScene<GameMap>
    {
        public override async Task LoadAsync()
        {
            var gameMap = await Engine.AssetLoader.GetAsync<XMLAsset<GameMap>>("game_map.xml");
            if (gameMap?.Content != null) await ChangeMapAsync(gameMap.Content);
        }
    }
}