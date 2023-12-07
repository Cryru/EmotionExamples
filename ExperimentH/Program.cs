#region Using

using Emotion.Common;

#endregion

namespace ExperimentH
{
    // todo:
    // make boss jump when stomping
    // fix icons for abilities
    // cast bar not centered
    // stomp dot not visible
    // healing circle duration not visible
    // pathing problems
    // balancing problems
    // tutorial (skill keys on ability icons)
    // stomp damage is not ellipse but rather circle
    // split auras into ticking and unticking
    // convert timers to map timers
    // do some things need to be coroutines? ex. cooldown timer

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