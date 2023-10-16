#region Using

using Emotion.Common;

#endregion

namespace MiniCom
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var config = new Configurator
            {
                DebugMode = true,
                HostTitle = "MiniCom"
            };

            Engine.Setup(config);
            Engine.SceneManager.SetScene(new GameScene());
            Engine.Run();
        }
    }
}