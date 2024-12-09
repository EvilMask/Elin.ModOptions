using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EvilMask.Elin.ModOptions.UI;

internal class PreBuildInfo
{
    public PixelOrPercentage? Width { get; set; } = null;
    public int? Height { get; set; } = null;
    public string Id { get; set; } = null;
}

internal sealed class TextInfo : PreBuildInfo
{
    public TextAnchor Anchor { get; set; } = TextAnchor.MiddleCenter;
    public string ContentId { get; set; }
    public int? Size { get; set; }
    public Color? Color { get; set; }
}

internal sealed class TopicInfo : PreBuildInfo
{
    public string ContentId { get; set; }
}

internal sealed class ToggleInfo : PreBuildInfo
{
    public string ContentId { get; set; }
    public bool Checked { get; set; } = false;
    public string TooltipId { get; set; } = null;
}
internal sealed class ButtonInfo : PreBuildInfo
{
    public string ContentId { get; set; }
    public string TooltipId { get; set; } = null;
}

internal class SelectionsBase : PreBuildInfo
{
    public string TitleId { get; set; } = null;
    public List<(string, string)> Selections { get; } = [];
}

internal sealed class SelectionsInfo : SelectionsBase
{
    public bool IsDropdown { get; set; } = true;
    public int Current {  get; set; }
}

internal sealed class ToggleGroupInfo : SelectionsBase
{
    public HashSet<int> Current { get; } = [];
    public int MinSelect { get; set; } = 1;
    public int MaxSelect { get; set; } = 1;
}

internal class LayoutInfo : PreBuildInfo
{
    public List<PreBuildInfo> Childs { get; } = [];
}

internal sealed class VLayoutInfo : LayoutInfo
{
    public bool HasBorder { get; set; } = false;
    public string TitleId { get; set; } = null;
}
internal sealed class HLayoutInfo : LayoutInfo
{
}

internal sealed class SliderInfo : LayoutInfo
{
    public bool Buttons { get; set; } = false;
    public float Min { get; set; } = 0f;
    public float Max { get; set; } = 1f;
    public float Value { get; set; } = 0f;
    public float Step { get; set; } = 0.1f;
    public string TitleId { get; set; } = null;
}
internal sealed class InputInfo : LayoutInfo
{
    public string Placeholder { get; set; } = null;
    public string Value { get; set; } = null;
    public int Limit { get; set; } = 0;
    public InputField.CharacterValidation Validation { get; set; } = InputField.CharacterValidation.None;
}