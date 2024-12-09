using EvilMask.Elin.ModOptions.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace EvilMask.Elin.ModOptions;

public sealed class ModOptionController
{
    public static ModOptionController Register(string guid, string tooptipId = null)
    {
        if (!Plugin.Instance.Ready) return null;
        if (Plugin.Instance.ManagedMods.ContainsKey(guid))
        {
            Plugin.Warn($"Mod ID [{guid}] has already been registed! Ignoring ...");
            return null;
        }
        var result = new ModOptionController(guid);
        result.SetModTooltipId(tooptipId);
        Plugin.Instance.ManagedMods.Add(guid, result);
        Plugin.Message($"Mod ID [{guid}] - registration successful .");
        return result;
    }

    public string Name => Tr(Guid);
    public string Tooltip => m_tooltip_id?.IsEmpty() ?? true ? null : Tr(m_tooltip_id);

    public event Action<OptionUIBuilder> OnBuildUI;

    public void SetModTooltipId(string tooltipId)
    {
        m_tooltip_id = tooltipId;
    }
    public void SetTranslation(string langCode, string id, string trans)
    {
        if (langCode == null) return;
        if (id == null) return;
        if (!Dict.ContainsKey(langCode)) Dict.Add(langCode, []);
        var dict = Dict[langCode];
        if (dict.ContainsKey(id))
            dict[id] = trans;
        else
            dict.Add(id, trans);
    }
    public void SetTranslation(string id, string en = null, string jp = null, string cn = null)
    {
        if (id?.IsEmpty() ?? true) return;
        if (en != null) SetTranslation("EN", id, en);
        if (jp != null) SetTranslation("JP", id, jp);
        if (cn != null) SetTranslation("CN", id, cn);
    }
    public string Tr(string contentId) => Tr(contentId, null);
    public string Tr(string contentId, params string[] args)
    {
        if (contentId == null) return null;
        if (!TryTr(contentId, out var trans)) return contentId;
        if (args != null && args.Length > 0) return TrPattern(trans, args);
        return trans;
    }
    public void SetPreBuildWithXml(string xml) => ParseXML(xml);

    #region internal
    internal ModOptionController(string guid) { Guid = guid; }
    internal string Guid;
    internal Dictionary<string, Dictionary<string, string>> Dict { get; } = [];
    internal Dictionary<string, OptUIElement> PreBuildElements { get; } = [];
    internal VLayoutInfo PreBuildRoot { set; get; } = null;
    internal OptionUIBuilder Builder { get; set; } = null;

    internal void CreateBuilder(ContentConfigModOptions options, OptVLayout root)
    {
        Builder = new OptionUIBuilder(options) { Controller = this, Root = root };
        root.Builder = Builder;
    }

    internal void BuildUI()
    {
        Plugin.Log($"[{Name}] Start building UI.");
        OnBuildUI?.Invoke(Builder);
        Plugin.Log($"[{Name}] Finished building UI.");
    }
    
    private string m_tooltip_id = null;

    private bool TryTr(string contentId, out string result)
    {
        result = null;
        if (!Dict.ContainsKey(Lang.langCode)) return false;
        var dict = Dict[Lang.langCode];
        if (!dict.TryGetValue(contentId, out var trans)) return false;
        result = trans;
        return true;
    }
    private string TrPattern(string pattern, params string[] args)
    {
        if (args == null || args.Length == 0) return pattern;
        StringBuilder sb = new(pattern);
        int idx = 1;
        foreach (var arg in args)
        {
            if (TryTr(arg, out var arg_trans))
                sb.Replace($"{{{idx}}}", arg_trans);
            else
                sb.Replace($"{{{idx}}}", arg);
        }
        return sb.ToString();
    }

    private void ParseXML(string xml)
    {
        PreBuildRoot = null;
        XmlDocument xmlDoc = new();
        try
        {
            xmlDoc.LoadXml(xml);
        }
        catch (Exception e)
        {
            Plugin.Error($"Failed to parse layout xml: {e.Message}");
            return;
        }
        Plugin.Log($"Starting parse the pre-build UI for mod [{Name}]");

        if (xmlDoc.DocumentElement == null)
        {
            Plugin.Error($"Xml has no root? Abort ...");
            return;
        }
        var rootName = xmlDoc.DocumentElement.Name;
        if (rootName != "config" && rootName != "設定" && rootName != "设置")
        {
            Plugin.Error($"The root element is not config? Abort ...");
            return;
        }
        if (xmlDoc.DocumentElement.ChildNodes.Count == 0)
        {
            Plugin.Warn($"The root has no child? Abort ...");
            return;
        }
        PreBuildRoot = ParseVLayout(xmlDoc.DocumentElement, true, true);
    }
    private VLayoutInfo ParseVLayout(XmlNode node, bool parentIsVLayout, bool isRoot = false)
    {
        VLayoutInfo info = new();
        int childStart = 0;
        if (!isRoot)
        {
            var has_boder = node.Attributes.GetNamedItem("border")
                ?? node.Attributes.GetNamedItem("枠")
                ?? node.Attributes.GetNamedItem("边框");
            if (has_boder != null)
            {
                info.HasBorder = has_boder.Value.ToLower() == "true";
                if (info.HasBorder && node.ChildNodes.Count > 0)
                {
                    var firstName = node.ChildNodes[0].Name;
                    if (firstName == "title" || firstName == "タイトル" || firstName == "标题")
                    {
                        childStart = 1;
                        info.TitleId = ParseContentId(node.ChildNodes[0]);
                    }
                }
            }
        }
        ParseLayoutInfo(info, node, true, childStart);
        return info;
    }
    private HLayoutInfo ParseHLayout(XmlNode node, bool parentIsVLayout, bool isRoot = false)
    {
        HLayoutInfo info = new();
        ParseLayoutInfo(info, node);
        return info;
    }

    private void ParseLayoutInfo(LayoutInfo info, XmlNode node, bool parentIsVLayout = false, int start = 0)
    {
        ParseElementContent(node, info, parentIsVLayout);
        for (int i = start; i < node.ChildNodes.Count; i++)
        {
            XmlNode child = node.ChildNodes[i];
            switch (child.Name)
            {
                case "vlayout":
                case "縦組み":
                case "竖排":
                    var vlayout = ParseVLayout(child, parentIsVLayout);
                    if (vlayout != null) info.Childs.Add(vlayout);
                    break;
                case "hlayout":
                case "横組み":
                case "横排":
                    var hlayout = ParseHLayout(child, parentIsVLayout);
                    if (hlayout != null) info.Childs.Add(hlayout);
                    break;
                case "text":
                case "文字":
                case "文本":
                    var text = ParseText(child, parentIsVLayout);
                    if (text != null) info.Childs.Add(text);
                    break;
                case "topic":
                case "トピック":
                case "主题":
                    var topic = ParseTopic(child, parentIsVLayout);
                    if (topic != null) info.Childs.Add(topic);
                    break;
                case "toggle":
                case "スイッチ":
                case "开关":
                    var toggle = ParseToggle(child, parentIsVLayout);
                    if (toggle != null) info.Childs.Add(toggle);
                    break;
                case "button":
                case "ボタン":
                case "按钮":
                    var button = ParseButton(child, parentIsVLayout);
                    if (button != null) info.Childs.Add(button);
                    break;
                case "one_choice":
                case "単一選択":
                case "单选":
                    var selections = ParseOneSelections(child, parentIsVLayout);
                    if (selections != null) info.Childs.Add(selections);
                    break;
                case "t_group":
                case "選択肢グループ":
                case "选项组":
                    var m_selects = ParseMultiSelections(child, parentIsVLayout);
                    if (m_selects != null) info.Childs.Add(m_selects);
                    break;
                case "slider":
                case "スライダー":
                case "滑动条":
                    var slider = ParseSlider(child, parentIsVLayout);
                    if (slider != null) info.Childs.Add(slider);
                    break;
                default:
                    Plugin.Warn($"Unexpected tag <{child.Name}>, ignore ...");
                    break;
            }
        }
    }

    private SliderInfo ParseSlider(XmlNode node, bool parentIsVLayout)
    {
        SliderInfo info = new();
        ParseElementContent(node, info, parentIsVLayout);
        var type = node.Attributes.GetNamedItem("buttons")
            ?? node.Attributes.GetNamedItem("ボタン式")
            ?? node.Attributes.GetNamedItem("按钮");
        if (type != null)
            info.Buttons = type.Value == "true";
        var min = node.Attributes.GetNamedItem("min")
            ?? node.Attributes.GetNamedItem("最小値")
            ?? node.Attributes.GetNamedItem("最小值");
        if (min != null && float.TryParse(min.Value, out var v_min))
            info.Min = v_min;
        var max = node.Attributes.GetNamedItem("max")
            ?? node.Attributes.GetNamedItem("最大値")
            ?? node.Attributes.GetNamedItem("最大值");
        if (min != null && float.TryParse(max.Value, out var v_max))
            info.Max = Math.Max(v_max, info.Min);
        var value = node.Attributes.GetNamedItem("value")
            ?? node.Attributes.GetNamedItem("値")
            ?? node.Attributes.GetNamedItem("值");
        if (value != null && float.TryParse(value.Value, out var v))
            info.Value = Mathf.Clamp(v, info.Min, info.Max);
        var step = node.Attributes.GetNamedItem("step")
            ?? node.Attributes.GetNamedItem("ステップ")
            ?? node.Attributes.GetNamedItem("步长");
        if (step != null && float.TryParse(step.Value, out var v_step))
            info.Step = v_step;
        info.TitleId = ParseContentId(node);
        return info;
    }

    private SelectionsInfo ParseOneSelections(XmlNode node, bool parentIsVLayout)
    {
        SelectionsInfo info = new();
        ParseElementContent(node, info, parentIsVLayout);
        var type = node.Attributes.GetNamedItem("type")
            ?? node.Attributes.GetNamedItem("タイプ")
            ?? node.Attributes.GetNamedItem("样式");
        if (type != null)
        {
            var name = type.Value.ToLower();
            info.IsDropdown = name == "dropdown" || name == "引き出し" || name == "下拉菜单";
        }
        var value = node.Attributes.GetNamedItem("value")
            ?? node.Attributes.GetNamedItem("値")
            ?? node.Attributes.GetNamedItem("值");
        if (value != null)
        {
            var values = value.Value.Split(',').Select(v => int.Parse(v.Trim())).ToArray();
            if (values.Length > 0) info.Current = Math.Max(values[0], 0);
        }
        ParseTitleAndOptions(node, info);
        info.Current = Math.Min(info.Current, info.Selections.Count);
        if (info.Selections.Count == 0)
        {
            Plugin.Error($"No options in Selections, discard ...");
            return null;
        }
        return info;
    }
    private ToggleGroupInfo ParseMultiSelections(XmlNode node, bool parentIsVLayout)
    {
        ToggleGroupInfo info = new();
        ParseElementContent(node, info, parentIsVLayout);
        var min = node.Attributes.GetNamedItem("min_count")
            ?? node.Attributes.GetNamedItem("選択下限")
            ?? node.Attributes.GetNamedItem("选择下限");
        if (min != null && int.TryParse(min.Value, out var v1))
        {
            info.MinSelect = Math.Max(v1, 0);
        }
        var max = node.Attributes.GetNamedItem("max_count")
            ?? node.Attributes.GetNamedItem("選択上限")
            ?? node.Attributes.GetNamedItem("选择上限");
        if (max != null && int.TryParse(max.Value, out var v2))
        {
            info.MaxSelect = Math.Max(v2, 1);
            if (info.MinSelect >= info.MaxSelect)
                info.MinSelect = info.MaxSelect > 1 ? info.MaxSelect - 1 : info.MaxSelect;
        }
        ParseTitleAndOptions(node, info);
        if (info.Selections.Count == 0)
        {
            Plugin.Error($"No options in Selections, discard ...");
            return null;
        }
        var value = node.Attributes.GetNamedItem("value")
            ?? node.Attributes.GetNamedItem("値")
            ?? node.Attributes.GetNamedItem("值");
        if (value != null)
        {
            var values = value.Value.Split(',').Select(v => int.Parse(v.Trim())).ToArray();
            foreach (var v in values)
                info.Current.Add(v.Clamp(info.MinSelect, info.MaxSelect));
        }
        return info;
    }

    private void ParseTitleAndOptions(XmlNode node, SelectionsBase info)
    {
        var firstName = node.ChildNodes[0].Name;
        int childStart = 0;
        if (firstName == "title" || firstName == "タイトル" || firstName == "标题")
        {
            childStart = 1;
            info.TitleId = ParseContentId(node.ChildNodes[0]);
        }
        for (var i = childStart; i < node.ChildNodes.Count; i++)
        {
            var (text, tooltip) = ParseTranslatableWithTooltip(node.ChildNodes[i]);
            if (text != null) info.Selections.Add((text, tooltip));
        }
    }

    private ToggleInfo ParseToggle(XmlNode node, bool parentIsVLayout)
    {
        ToggleInfo info = new();
        ParseElementContent(node, info, parentIsVLayout);
        (info.ContentId, info.TooltipId) = ParseTranslatableWithTooltip(node);
        var isChecked = node.Attributes.GetNamedItem("checked")
            ?? node.Attributes.GetNamedItem("チェック")
            ?? node.Attributes.GetNamedItem("勾选");
        if (isChecked != null) info.Checked = isChecked.Value.ToLower() == "true";
        return info;
    }
    private ButtonInfo ParseButton(XmlNode node, bool parentIsVLayout)
    {
        ButtonInfo info = new();
        ParseElementContent(node, info, parentIsVLayout);
        (info.ContentId, info.TooltipId) = ParseTranslatableWithTooltip(node);
        return info;
    }

    private TextInfo ParseText(XmlNode node, bool parentIsVLayout)
    {
        TextInfo info = new() { ContentId = ParseContentId(node) };
        ParseElementContent(node, info, parentIsVLayout);
        var color = node.Attributes.GetNamedItem("color")
            ?? node.Attributes.GetNamedItem("色")
            ?? node.Attributes.GetNamedItem("颜色");
        if (color != null && !color.Value.IsEmpty() && color.Value[0] == '#'
             && int.TryParse(color.Value.Substring(1),
            System.Globalization.NumberStyles.HexNumber, null, out var num))
        {
            if (num > 0xffffff)
            {
                float r = ((num & 0xffffffff)>> 24) * 1.0f / 0xff;
                float g = ((num & 0xffffff) >> 16) * 1.0f / 0xff;
                float b = ((num & 0xffff) >> 8) * 1.0f / 0xff;
                float a = (num & 0xff) * 1.0f / 0xff;
                info.Color = new(r, g, b, a);
            }
            else
            {
                float r = ((num & 0xffffff) >> 16) * 1.0f / 0xff;
                float g = ((num & 0xffff) >> 8) * 1.0f / 0xff;
                float b = (num & 0xff) * 1.0f / 0xff;
                info.Color = new(r, g, b);
            }
        }
        return info;
    }

    private TopicInfo ParseTopic(XmlNode node, bool parentIsVLayout)
    {
        TopicInfo info = new() { ContentId = ParseContentId(node) };
        ParseElementContent(node, info, parentIsVLayout);
        return info;
    }

    private void ParseElementContent(XmlNode node, PreBuildInfo info, bool parentIsVLayout)
    {
        if (parentIsVLayout)
        {
            // Ignore width setting for VLayout parent.
            var height = node.Attributes.GetNamedItem("height")
                ?? node.Attributes.GetNamedItem("高さ")
                ?? node.Attributes.GetNamedItem("高度");
            if (height != null && int.TryParse(height.Value, out var value))
                info.Height = value;
        }
        else
        {
            // Ignore height setting for VLayout parent.
            var width = node.Attributes.GetNamedItem("width")
                ?? node.Attributes.GetNamedItem("幅")
                ?? node.Attributes.GetNamedItem("宽度");
            if (width != null)
            {
                if (int.TryParse(width.Value, out var value))
                    info.Width = value;
                else if (float.TryParse(width.Value, out var f_value))
                    info.Width = f_value;
            }
        }
        var id = node.Attributes.GetNamedItem("id");
        if (id != null)
            info.Id = id.Value;
    }

    private string ParseContentId(XmlNode node, bool allowNull = false)
    {
        var result = node.InnerXml.Trim();
        if (result.IsEmpty())
        {
            if (!allowNull)
                Plugin.Warn($"The conentId is empty, treat as a empty string ...");
            return null;
        }
        return result;
    }

    private (string, string) ParseTranslatableWithTooltip(XmlNode node)
    {
        if (node.ChildNodes.Count == 0) return (null, null);
        string id = null;
        {
            var name = node.ChildNodes[0].Name;
            if (name != "contentId" && name != "内容ID" && name != "文本ID")
                Plugin.Warn($"The Translatable doesn't have a conentId section, treat as a empty string.");
            else
                id = ParseContentId(node.ChildNodes[0]);
        }
        if (node.ChildNodes.Count == 1) return (id, null);
        {
            var name = node.ChildNodes[1].Name;
            if (name == "tooltip" || name == "ツールチップ" || name == "提示文本")
            {
                var tooltip = ParseContentId(node.ChildNodes[1]);
                if (tooltip.IsEmpty()) return (id, null);
                return (id, tooltip);
            }
        }
        return (id, null);
    }
    #endregion
}