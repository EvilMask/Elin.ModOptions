using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EvilMask.Elin.ModOptions.UI;


public struct PixelOrPercentage
{
    public bool IsPixel { get; private set; } = true;
    public readonly int Pixel { get => (int)Math.Round(Percentage); }
    public float Percentage { get; private set; } = 0;

    public PixelOrPercentage(int value)
    {
        IsPixel = true;
        Percentage = Math.Max(value, 0);
    }
    public PixelOrPercentage(float value)
    {
        IsPixel = false;
        Percentage = Math.Max(value, 0);
    }

    public static implicit operator PixelOrPercentage(int value) => new(value);
    public static implicit operator PixelOrPercentage(float value) => new(value);
}

public abstract class OptUIElement
{
    public int PrefferedHeight
    {
        set
        {
            if (!Builder.Valid) return;
            Element.preferredHeight = value;
        }
    }
    public PixelOrPercentage PrefferedWidth
    {
        set
        {
            if (!Builder.Valid) return;
            if (value.IsPixel)
                Element.preferredWidth = value.Pixel;
            else
                Element.flexibleWidth = value.Percentage;
        }
    }
    public virtual bool Enabled { get; set; }

    internal OptionUIBuilder Builder { get; set; }
    internal LayoutElement Element { get; set; }
}

public abstract class OptLayout : OptUIElement
{
    public OptLabel AddText(string text, TextAnchor align = TextAnchor.MiddleCenter, int size = 16, Color? color = null)
    {
        if (!Builder.Valid) return null;
        var result = new OptLabel()
        {
            Label = Builder.Config.CreateText(GetRect(), text, align, size, color),
            Builder = Builder,
        };
        result.Element = result.Label.GetComponent<LayoutElement>();
        return result;
    }
    public OptTopic AddTopic(string text)
    {
        if (!Builder.Valid) return null;
        var result = new OptTopic()
        {
            Topic = Builder.Config.CreateTopic(GetRect(), text),
            Builder = Builder,
        };
        result.Element = result.Topic.GetComponent<LayoutElement>();
        return result;
    }
    public OptButton AddButton(string text, string tooltip = null)
    {
        if (!Builder.Valid) return null;
        var result = new OptButton()
        {
            Button = Builder.Config.CreateButton(GetRect(), text, tooltip),
            Builder = Builder,
        };
        result.Element = result.Button.GetComponent<LayoutElement>();
        result.Button.onClick.AddListener(result.OnClick);
        return result;
    }
    public OptToggleGroup AddToggleGroup(string title = null, int min = 1, int max = 1, int size = 16, params (string, string, bool)[] items)
    {
        if (!Builder.Valid) return null;
        var result = new OptToggleGroup()
        {
            Group = Builder.Config.CreateToggleGroup(GetRect(), title, min, max, size, items),
            Builder = Builder,
        };
        result.Element = result.Group.GetComponent<LayoutElement>();
        result.Group.OnValueChanged.AddListener(result.OnChangeValue);
        return result;
    }
    public OptToggleGroup AddToggleGroup(string title = null, int min = 1, int max = 1, int size = 16, params string[] items)
    {
        if (!Builder.Valid) return null;
        static (string, string, bool) selectFunc(string v) => (v, null, false);
        return AddToggleGroup(title, min, max, size, items.Select(selectFunc).ToArray());
    }
    public OptToggle AddToggle(string text, bool isChecked = false, int size = 16, string tooltip = null)
    {
        if (!Builder.Valid) return null;
        var result = new OptToggle()
        {
            Toggle = Builder.Config.CreateToggle(GetRect(), text, isChecked, size, tooltip),
            Builder = Builder,
        };
        result.Element = result.Toggle.GetComponent<LayoutElement>();
        result.Toggle.onClick.AddListener(result.OnClick);
        return result;
    }
    public OptSlider AddSlider(string text, bool sideButtons = false, float min = 0, float max = 1, float value = 0)
    {
        if (!Builder.Valid) return null;
        var result = new OptSlider()
        {
            Slider = Builder.Config.CreateSlider(GetRect(), text, sideButtons),
            Builder = Builder,
            Min = min,
            Max = Math.Max(max, min),
            Value = Mathf.Clamp(value, min, max),
        };
        result.Element = result.Slider.transform.parent.GetComponent<LayoutElement>();
        result.Slider.onValueChanged.AddListener(result.OnChangeValue);
        if (sideButtons) Builder.Config.SetSliderButtonActions(result.Slider, result.OnAboutToChangeValue);
        return result;
    }
    public OptDropdown AddDropdown(List<string> itemTexts, int current = 0)
    {
        if (!Builder.Valid) return null;
        static (string, string) selectFunc(string v) => (v, null);
        return AddDropdown(itemTexts.Select(selectFunc).ToList(), current);
    }
    public OptDropdown AddDropdown(List<(string, string)> itemTextsAndTooltips, int current = 0)
    {
        if (!Builder.Valid) return null;
        var result = new OptDropdown()
        {
            Dropdown = Builder.Config.CreateDropdown(GetRect(), itemTextsAndTooltips, current),
            Builder = Builder,
        };
        result.Element = result.Dropdown.GetComponent<LayoutElement>();
        result.Dropdown.onValueChanged.AddListener(result.OnChangeValue);
        return result;
    }
    public OptLRSelect AddLRSelect(List<string> itemTexts, int current = 0)
    {
        if (!Builder.Valid) return null;
        static (string, string) selectFunc(string v) => (v, null);
        return AddLRSelect(itemTexts.Select(selectFunc).ToList(), current);
    }
    public OptLRSelect AddLRSelect(List<(string, string)> itemTextsAndTooltips, int current = 0)
    {
        if (!Builder.Valid) return null;
        var result = new OptLRSelect()
        {
            Builder = Builder,
        };
        result.Select = Builder.Config.CreateButtonLR(GetRect(), itemTextsAndTooltips, current, result.OnChangeValue);
        result.Element = result.Select.GetComponent<LayoutElement>();
        return result;
    }
    public OptInput AddInput(string value = null, string placeholder = null, int lengthLimit = 0, InputField.CharacterValidation validation = InputField.CharacterValidation.None)
    {
        if (!Builder.Valid) return null;
        var result = new OptInput()
        {
            Input = Builder.Config.CreateInput(GetRect(), value, placeholder, lengthLimit, validation),
            Builder = Builder,
        };
        result.Element = result.Input.GetComponent<LayoutElement>();
        result.Input.onValueChanged.AddListener(result.OnChangeValue);
        result.Input.onValidateInput = result.OnValidateInput;
        return result;
    }
    public OptHLayout AddHLayout(PixelOrPercentage? width = null)
    {
        if (!Builder.Valid) return null;
        var result = new OptHLayout()
        {
            Layout = Builder.Config.CreateHLayout(GetRect(), width),
            Builder = Builder,
        };
        result.Element = result.Layout.GetComponent<LayoutElement>();
        return result;
    }
    public OptVLayout AddVLayout(int? height)
    {
        if (!Builder.Valid) return null;
        var result = new OptVLayout()
        {
            Layout = Builder.Config.CreateVLayout(GetRect(), height),
            Builder = Builder,
        };
        result.Element = result.Layout.GetComponent<LayoutElement>();
        return result;
    }
    public OptVLayout AddVLayoutWithBorder(string title, int? height = null)
    {
        if (!Builder.Valid) return null;
        var result = new OptVLayout()
        {
            Layout = Builder.Config.CreateGroup(GetRect(), title, height),
            Builder = Builder,
        };
        result.Element = result.Layout.GetComponent<LayoutElement>();
        return result;
    }

    internal abstract RectTransform GetRect();
}

public sealed class OptHLayout : OptLayout
{
    public HorizontalLayoutGroup Base { get => Builder.Valid ? Layout : null; }

    internal HorizontalLayoutGroup Layout { get; set; }
    internal override RectTransform GetRect() => Layout.Rect();
    internal OptHLayout(){}
}
public sealed class OptVLayout : OptLayout
{
    public VerticalLayoutGroup Base { get => Builder.Valid ? Layout : null; }

    internal VerticalLayoutGroup Layout { get; set; }
    internal override RectTransform GetRect() => Layout.Rect();
    internal OptVLayout(){}
}
public class OptLabel : OptUIElement
{
    public UIItem Base { get => Builder.Valid ? Label : null; }
    public string Text
    { 
        get => Builder.Valid ? Label.text1.text : null;
        set { if (Builder.Valid) Label.text1.text = value; }
    }
    public TextAnchor Align
    { 
        get => Builder.Valid ? Label.text1.alignment : TextAnchor.MiddleCenter;
        set { if (Builder.Valid) Label.text1.alignment = value; }
    }
    public int Size
    {
        get => Builder.Valid ? Label.text1.fontSize : 0;
        set { if (Builder.Valid) Label.text1.fontSize = value; }
    }
    public Color Color
    {
        get => Builder.Valid ? Label.text1.color : Color.white;
        set { if (Builder.Valid) Label.text1.color = value; }
    }
    public override bool Enabled
    { 
        get => Builder.Valid && Label.enabled;
        set { if (Builder.Valid) Label.enabled = value; }
    }

    internal UIItem Label { get; set; }
    internal OptLabel(){}
}
public class OptTopic : OptUIElement
{
    public UIItem Base { get => Builder.Valid ? Topic : null; }
    public string Text
    {
        get => Builder.Valid ? Topic.text1.text : null;
        set { if (Builder.Valid) Topic.text1.text = value; }
    }
    public override bool Enabled
    {
        get => Builder.Valid && Topic.enabled;
        set { if (Builder.Valid) Topic.enabled = value; }
    }

    internal UIItem Topic { get; set; }
    internal OptTopic(){}
}
public class OptToggle : OptUIElement
{
    public UIButton Base { get => Builder.Valid ? Toggle : null; }
    public event Action<bool> OnValueChanged;
    public bool Checked
    {
        get => Builder.Valid && Toggle.isChecked;
        set { if (Builder.Valid) Toggle.SetCheck(value); }
    }
    public override bool Enabled
    {
        get => Builder.Valid && Toggle.IsInteractable();
        set { if (Builder.Valid) Toggle.SetInteractableWithAlpha(value); }
    }

    internal UIButton Toggle { get; set; }
    internal void OnClick() => OnValueChanged?.Invoke(Toggle.isChecked);
    internal OptToggle() { }
}
public class OptButton : OptUIElement
{
    public UIButton Base { get => Builder.Valid ? Button : null; }
    public event Action OnClicked;
    public string Text
    {
        get => Builder.Valid ? Button.mainText.text : null;
        set { if (Builder.Valid) Button.mainText.text = value; }
    }
    public override bool Enabled
    {
        get => Builder.Valid && Button.IsInteractable();
        set { if (Builder.Valid) Button.SetInteractableWithAlpha(value); }
    }

    internal UIButton Button { get; set; }
    internal void OnClick() => OnClicked?.Invoke();
    internal OptButton() { }
}
public class OptSlider : OptUIElement
{
    public UISlider Base { get => Builder.Valid ? Slider : null; }
    public event Action<float> OnValueChanged;
    public float Min
    {
        get => Builder.Valid ? Slider.minValue : 0;
        set { if (Builder.Valid) Slider.minValue = Math.Min(value, Max); }
    }
    public float Max
    {
        get => Builder.Valid ? Slider.maxValue : 0;
        set { if (Builder.Valid) Slider.maxValue = Math.Max(Min, value); }
    }
    public float Value
    { 
        get => Builder.Valid ? Slider.value : 0;
        set { if (Builder.Valid) Slider.value = Mathf.Clamp(value, Min, Max); }
    }
    public float Step { set; private get; } = 0.1f;
    public string Title
    { 
        get => Builder.Valid ? Slider.textMain.text : null;
        set { if (Builder.Valid) Slider.textMain.text = value; }
    }
    public override bool Enabled
    {
        get => Builder.Valid && Slider.IsInteractable();
    }

    internal UISlider Slider { get; set; }
    internal void OnChangeValue(float value) => OnValueChanged?.Invoke(value);
    internal void OnAboutToChangeValue(float modifier = 1.0f)
    {
        if (busy) return;
        busy = true;
        Value += Step * modifier;
        Value = Mathf.Clamp(Value, Min, Max);
        busy = false;
        OnChangeValue(Value);
    }
    internal OptSlider(){}
    private bool busy = false;
}
public class OptDropdown : OptUIElement
{
    public UIDropdown Base { get => Builder.Valid ? Dropdown : null; }
    public event Action<int> OnValueChanged;
    public int Value
    {
        get => Builder.Valid ? Dropdown.value : 0;
        set { if (Builder.Valid) Dropdown.value = value; }
    }
    public override bool Enabled
    {
        get => Builder.Valid && Dropdown.IsInteractable();
    }

    internal UIDropdown Dropdown { get; set; }
    internal void OnChangeValue(int index) => OnValueChanged?.Invoke(index);
    internal OptDropdown(){}
}
public class OptLRSelect : OptUIElement
{
    public UIButtonLR Base { get => Builder.Valid ? Select : null; }
    public event Action<int> OnValueChanged;
    public int Value
    {
        get => Builder.Valid ? Select.index : 0;
        set { if (Builder.Valid) Select.index = value; }
    }
    public override bool Enabled
    {
        get => Builder.Valid && Select.IsInteractable();
        set { if (Builder.Valid) Select.SetInteractableWithAlpha(value); }
    }
    public float Step
    {
        get => m_step;
        set
        {
            if (m_step == value) return;
            m_step = value;
        }
    }

    internal UIButtonLR Select { get; set; }
    internal void OnChangeValue(int index) => OnValueChanged?.Invoke(index);
    internal OptLRSelect(){}
    private float m_step = 0.1f;
}
public class OptInput : OptUIElement
{
    public InputField Base { get => Builder.Valid ? Input : null; }
    public event Action<string> OnValueChanged;
    public event Func<string, int, char, char> OnChangingValue;
    public string Text
    {
        get => Builder.Valid ? Input.text : null;
        set { if (Builder.Valid) Input.text = value; }
    }
    public string Placeholder
    {
        get => Builder.Valid ? Input.placeholder.GetComponent<Text>().text : null;
        set { if (Builder.Valid) Input.placeholder.GetComponent<Text>().text = value; }
    }
    public InputField.ContentType ContentType
    {
        get => Builder.Valid ? Input.contentType : InputField.ContentType.Standard;
        set { if (Builder.Valid) Input.contentType = value; }
    }
    public int CharacterLimit
    {
        get => Builder.Valid ? Input.characterLimit : 0;
        set { if (Builder.Valid) Input.characterLimit = value; }
    }
    public override bool Enabled
    {
        get => Builder.Valid && Input.IsInteractable();
    }

    internal InputField Input { get; set; }
    internal void OnChangeValue(string value) => OnValueChanged?.Invoke(value);
    internal char OnValidateInput(string text, int index, char added) => OnChangingValue?.Invoke(text, index, added) ?? added;
    internal OptInput(){}
}
public class OptToggleGroup : OptUIElement
{
    public UIToggleGroup Base { get => Builder.Valid ? Group : null; }
    public event Action<int, bool> OnValueChanged;
    public void SetToggle(int index, bool value) => Group.SetToggle(index, value);
    public override bool Enabled
    {
        get => Builder.Valid && Group.Enabled;
        set { if (Builder.Valid) Group.Enabled = value; }
    }

    internal UIToggleGroup Group { get; set; }
    internal void OnChangeValue(int index, bool value) => OnValueChanged?.Invoke(index, value);
    internal OptToggleGroup(){}
}
