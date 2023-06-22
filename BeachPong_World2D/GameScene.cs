#region Using

using Emotion.Game.World2D.SceneControl;

#endregion

namespace BeachPong_World2D
{
	public class GameScene : World2DBaseScene<BeachBallMap>
	{
		public override Task LoadAsync()
		{
			return Task.CompletedTask;
		}
	}
}