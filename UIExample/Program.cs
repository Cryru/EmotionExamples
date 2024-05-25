using Emotion.Common;

namespace UIExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new Configurator
            {
                DebugMode = true,
                HostTitle = "UI_Example"
            };

            Engine.Setup(config);
            Engine.SceneManager.SetScene(new UIExampleScene());
            Engine.Run();
        }
    }
}
