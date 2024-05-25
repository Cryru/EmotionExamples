#nullable enable

#region Using

using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.UI;
using System.Numerics;

#endregion

namespace UIExample;

public class TextButton : UICallbackButton
{
    #region Theme

    public Color NormalColor = Color.PrettyYellow;
    public Color RolloverColor = new Color("#f7e08f");
    public Color DisabledColor = new Color("#888888");

    #endregion

    public string? Text
    {
        get => _text;
        set
        {
            _text = value;
            if (_label != null) _label.Text = _text;
        }
    }

    private string? _text;

    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;

            if (Controller == null) return;
            RecalculateButtonColor();
        }
    }

    private bool _enabled = true;

    private UIText _label = null!;

    public TextButton(string label) : this()
    {
        Text = label;
    }

    public TextButton()
    {
        ScaleMode = UIScaleMode.FloatScale;
        FillX = false;
        FillY = false;

        StretchX = true;
        StretchY = true;
        Paddings = new Rectangle(2, 1, 2, 1);
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        WindowColor = NormalColor;

        var txt = new UIText
        {
            ParentAnchor = UIAnchor.CenterLeft,
            Anchor = UIAnchor.CenterLeft,
            ScaleMode = UIScaleMode.FloatScale,
            WindowColor = Color.Black,
            Id = "buttonText",
            FontSize = 12,
            IgnoreParentColor = true,
            Text = _text
        };
        _label = txt;
        AddChild(txt);

        RecalculateButtonColor();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        c.RenderSprite(Position, Size, _calculatedColor);
        return base.RenderInternal(c);
    }

    public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
    {
        if (!Enabled) return false;
        return base.OnKey(key, status, mousePos);
    }

    public override void OnMouseEnter(Vector2 _)
    {
        if (!Enabled) return;
        base.OnMouseEnter(_);
        RecalculateButtonColor();
    }

    public override void OnMouseLeft(Vector2 _)
    {
        if (!Enabled) return;
        base.OnMouseLeft(_);
        RecalculateButtonColor();
    }

    private void RecalculateButtonColor()
    {
        _label.IgnoreParentColor = Enabled;
        if (!Enabled)
        {
            WindowColor = DisabledColor;
            return;
        }

        WindowColor = MouseInside ? RolloverColor : NormalColor;
    }
}