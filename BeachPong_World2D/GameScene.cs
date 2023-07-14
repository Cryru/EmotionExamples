#region Using

using Emotion.Common;
using Emotion.Game.World2D.SceneControl;
using Emotion.IO;

#endregion

namespace BeachPong_World2D
{
	public class GameScene : World2DBaseScene<BeachBallMap>
	{
		public override async Task LoadAsync()
		{
			var gameMap = await Engine.AssetLoader.GetAsync<XMLAsset<BeachBallMap>>("game_map.xml");
			if (gameMap?.Content != null) await ChangeMapAsync(gameMap.Content);
		}
	}
}