#region Using

using Emotion.Common;

#endregion

namespace BeachPong_World2D
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var config = new Configurator
			{
				DebugMode = true, // Enables logging in the console and a variety of other functionality.
				HostTitle = "Beach Pong (World2D)" // The window name.
			};

			// First we setup the engine which prepares all modules to be used.
			Engine.Setup(config);
			// We then set our prepared scene (class which inherits "Scene") as the current scene in the SceneManager module.
			// This will start loading it asynchronously.
			Engine.SceneManager.SetScene(new GameScene());
			// Run the engine. This is a blocking call which will finish when the game is closed or it crashes.
			Engine.Run();
		}
	}
}