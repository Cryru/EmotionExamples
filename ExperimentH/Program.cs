#region Using

using Emotion.Common;

#endregion

namespace ExperimentH
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var config = new Configurator
            {
                DebugMode = true,
                HostTitle = "Experiment H"
            };

            Engine.Setup(config);
            Engine.SceneManager.SetScene(new GameScene());
            Engine.Run();
        }
    }
}