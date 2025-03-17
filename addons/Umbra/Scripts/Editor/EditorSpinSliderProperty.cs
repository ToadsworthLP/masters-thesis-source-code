#if TOOLS
using Godot;

namespace Umbra.Editor;

public partial class EditorSpinSliderProperty : EditorProperty
{
    private EditorSpinSlider spinSlider = new EditorSpinSlider();
    private float currentValue = 0;
    private bool updating = false;

    public EditorSpinSliderProperty()
    {
        AddChild(spinSlider);
        AddFocusable(spinSlider);
        RefreshControlText();

        spinSlider.MinValue = -20;
        spinSlider.MaxValue = 20;
        spinSlider.Rounded = false;
        spinSlider.AllowGreater = true;
        spinSlider.AllowLesser = true;
        spinSlider.Step = 0.01f;
        spinSlider.Suffix = "m";
        spinSlider.ValueChanged += OnValueChanged;
    }
    
    public EditorSpinSliderProperty(double minValue, double maxValue, string suffix)
    {
        AddChild(spinSlider);
        AddFocusable(spinSlider);
        RefreshControlText();

        spinSlider.MinValue = minValue;
        spinSlider.MaxValue = maxValue;
        spinSlider.Rounded = false;
        spinSlider.AllowGreater = true;
        spinSlider.AllowLesser = true;
        spinSlider.Step = 0.01f;
        spinSlider.Suffix = suffix;
        spinSlider.ValueChanged += OnValueChanged;
    }

    private void OnValueChanged(double value)
    {
        if (updating)
        {
            return;
        }

        currentValue = (float)value;
        RefreshControlText();
        EmitChanged(GetEditedProperty(), currentValue);
    }

    public override void _UpdateProperty()
    {
        var newValue = GetEditedObject().Get(GetEditedProperty()).AsDouble();
        if (newValue == currentValue)
        {
            return;
        }

        updating = true;
        currentValue = (float)newValue;
        RefreshControlText();
        updating = false;
    }

    private void RefreshControlText()
    {
        spinSlider.Value = currentValue;
    }
}
#endif