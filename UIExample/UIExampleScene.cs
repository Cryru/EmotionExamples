using Emotion.Common;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.UI;
using Emotion.WIPUpdates.NewUIUpdate;

namespace UIExample;

public class UIExampleScene : Scene
{
    public UISystem UI = new()
    {
        UseNewLayoutSystem = true // Set to false to use the old UI. Keep note that the demo is setup for the new UI layout.
    };

    public override async Task LoadAsync()
    {
        UISolidColor background = new UISolidColor
        {
            WindowColor = new Color("#285999"),
            Paddings = new Rectangle(5, 5, 5, 5)
        };
        UI.AddChild(background);

        UIBaseWindow listOfControls = new()
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new System.Numerics.Vector2(0, 2)
        };
        background.AddChild(listOfControls);

        TextButton textButton = new TextButton("Click Me!")
        {
            OnClickedUpProxy = (_) =>
            {
                Engine.Log.Info("I was clicked!", "Button");
            }
        };
        listOfControls.AddChild(textButton);

        Checkbox checkBox = new Checkbox(false)
        {
            OnValueChanged = (_, value) =>
            {
                Engine.Log.Info($"Checkbox is now: {(value ? "on" : "off")}!", "Checkbox");
            }
        };
        listOfControls.AddChild(checkBox);

        UISlider slider = new UISlider
        {
            MinSize = new System.Numerics.Vector2(100, 5),
            Horizontal = true,
            DefaultSelectorColor = Color.PrettyYellow,
            SelectorRatio = 5,
            WindowColor = Color.Black,
            OnValueChanged = (value) =>
            {
                Engine.Log.Info($"Scroll value is now: {value}!", "Slider");
            },
            KeepSelectorInside = true
        };
        listOfControls.AddChild(slider);

        await UI.PreloadUI();
    }

    public override void Update()
    {
        UI.Update();
    }

    public override void Draw(RenderComposer composer)
    {
        composer.SetUseViewMatrix(false);
        composer.SetDepthTest(true);
        composer.ClearDepth();
        UI.Render(composer);
    }
}
