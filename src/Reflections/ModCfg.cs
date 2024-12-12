using BepInEx.Configuration;
using EvilMask.Elin.ModOptions;
using EvilMask.Elin.ModOptions.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


internal sealed class AutoPreBuildUIInfo(string parent = null, PreBuildInfo info = null,
    Func<OptUIElement, Action> connector = null, Func<string> getValue = null)
{
    public string Parent { get; set; } = parent;
    public Func<string> GetValue { get; set; } = getValue;
    public PreBuildInfo Info { get; set; } = info;
    public Func<OptUIElement, Action> Connector { get; set; } = connector;
}

// Mark the class
public class ModCfg : Attribute
{
    #region internal
    internal static void EnumMarkedFields(object obj, List<AutoPreBuildUIInfo> list)
    {
        foreach (var field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            // Make sure is's BepInEx.Configuration.ConfigEntry<T> T for a enum type.
            var fieldType = field.FieldType;
            if (fieldType.Namespace != "BepInEx.Configuration") continue;
            var end = fieldType.Name.IndexOf('`');
            if (end < 0) continue;
            if (fieldType.Name.Substring(0, end) != "ConfigEntry") continue;
            foreach (var attr in  field.GetCustomAttributes())
            {
                if (attr is ModCfg cfg)
                {
                    var item = cfg.GetPreBuildInfo(field, obj);
                    if (item != null) list.Add(item);
                    break;
                }
            }
        }
    }

    internal virtual AutoPreBuildUIInfo GetPreBuildInfo(FieldInfo t, object obj) { return null; }
    #endregion
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class ModCfgSlider : ModCfg
{
    public ModCfgSlider(string titleId = null, float min = 0, float max = 1, float step = 0.1f, bool buttons = false)
    {
        Slider.Min = min;
        Slider.Max = max;
        Slider.Step = step;
        Slider.Buttons = buttons;
        Slider.TitleId = titleId;
    }
    public string Parent { get; set; } = null;
    public string Id { get => Slider.Id; set => Slider.Id = value; }

    #region internal
    internal SliderInfo Slider = new();
    internal override AutoPreBuildUIInfo GetPreBuildInfo(FieldInfo field, object obj)
    {
        var value = field.GetValue(obj);
        if (value == null)
        {
            Plugin.Error($"{field.Name} is null. Ignore ...");
            return null;
        }
        Slider.TitleId ??= field.Name;
        switch (value)
        {
            case ConfigEntry<int> c_int:
                var setter_i = (OptUIElement elmt) => Setter(elmt, c_int, v=>(int)Math.Round(v), v=>v);
                Slider.Value = c_int.Value;
                return new(Parent, Slider, setter_i, GetValue(c_int));
            case ConfigEntry<long> c_long:
                var setter_l = (OptUIElement elmt) => Setter(elmt, c_long, v => (long)Math.Round(v), v => v);
                Slider.Value = c_long.Value;
                return new(Parent, Slider, setter_l, GetValue(c_long));
            case ConfigEntry<float> c_float:
                var setter_f = (OptUIElement elmt) => Setter(elmt, c_float, v => v, v => v);
                Slider.Value = c_float.Value;
                return new(Parent, Slider, setter_f, GetValue(c_float));
        }
        Plugin.Error($"Failed({field.Name}), Slider for a ConfigEntry<{field.FieldType.GenericTypeArguments[0].Name}> is not supported. Ignore...");
        return null;
    }
    internal static Func<string> GetValue<T>(ConfigEntry<T> cfg)
    {
        T a() => cfg.Value;
        return () => a().ToString();
    }
    internal static Action Setter<T>(OptUIElement elmt, ConfigEntry<T> variable, Func<float, T> to_v, Func<T, float> from_v)
        where T : IEquatable<T>
    {
        if (elmt is not OptSlider slider) return null;
        var busy = false;
        slider.OnValueChanged += v =>
        {
            if (busy) return;
            if (variable.Value.Equals(to_v(v))) return;
            busy = true;
            variable.Value = to_v(slider.Value);
            busy = false;
        };
        void onVariableChanged(object _1, EventArgs _2)
        {
            if (busy) return;
            if (slider.Value == from_v(variable.Value)) return;
            busy = true;
            slider.Value = from_v(variable.Value);
            busy = false;
        };
        variable.SettingChanged += onVariableChanged;
        return () => variable.SettingChanged -= onVariableChanged;
    }
    #endregion
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class ModCfgToggle : ModCfg
{
    public ModCfgToggle(string titleId = null, string tooltipId = null)
    {
        Toggle.ContentId = titleId;
        Toggle.TooltipId = tooltipId;
    }
    public string Parent { get; set; } = null;
    public string Id { get => Toggle.Id; set => Toggle.Id = value; }

    #region internal
    internal ToggleInfo Toggle = new();
    internal override AutoPreBuildUIInfo GetPreBuildInfo(FieldInfo field, object obj)
    {
        var value = field.GetValue(obj);
        if (value == null)
        {
            Plugin.Error($"{field.Name} is null. Ignore ...");
            return null;
        }
        Toggle.ContentId ??= field.Name;
        switch (value)
        {
            case ConfigEntry<bool> c_bool:
                var setter_i = (OptUIElement elmt) => Setter(elmt, c_bool, v => v, v => v);
                Toggle.Checked = c_bool.Value;
                return new(Parent, Toggle, setter_i);
        }
        Plugin.Error($"Failed({field.Name}), Toggle for a ConfigEntry<{field.FieldType.GenericTypeArguments[0].Name}> is not supported. Ignore...");
        return null;
    }
    internal static Action Setter<T>(OptUIElement elmt, ConfigEntry<T> variable, Func<bool, T> to_v, Func<T, bool> from_v)
    {
        if (elmt is not OptToggle toggle) return null;
        var busy = false;
        toggle.OnValueChanged += v =>
        {
            if (busy) return;
            busy = true;
            variable.Value = to_v(toggle.Checked);
            busy = false;
        };
        void onVariableChanged(object _1, EventArgs _2)
        {
            if (busy) return;
            busy = true;
            toggle.Checked = from_v(variable.Value);
            busy = false;
        };
        variable.SettingChanged += onVariableChanged;
        return () => variable.SettingChanged -= onVariableChanged;
    }
    #endregion
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class ModCfgDropdown : ModCfg
{
    public ModCfgDropdown(string titleId = null)
    {
        Dropdown.TitleId = titleId;
    }
    public string Parent { get; set; } = null;
    public string Id { get => Dropdown.Id; set => Dropdown.Id = value; }

    #region internal
    internal SelectionsInfo Dropdown = new();
    internal override AutoPreBuildUIInfo GetPreBuildInfo(FieldInfo field, object obj)
    {
        var fieldType = field.FieldType;
        var cfg = field.GetValue(obj);
        if (cfg == null)
        {
            Plugin.Error($"{field.Name} is null. Ignore ...");
            return null;
        }

        if (!fieldType.GenericTypeArguments[0].IsEnum)
        {
            Plugin.Error($"Failed({field.Name}), {field.FieldType.GenericTypeArguments[0].Name} is not an enum. Ignore...");
            return null;
        }

        var enumType = fieldType.GenericTypeArguments[0];
        var values = enumType.GetEnumValues();
        if (values.Length == 0)
        {
            Plugin.Error($"Failed({field.Name}), {field.FieldType.GenericTypeArguments[0].Name} is an empty enum. Ignore...");
            return null;
        }

        var valueAccessor = fieldType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
        var settingChangedEvent = fieldType.GetEvent("SettingChanged", BindingFlags.Public | BindingFlags.Instance);

        (string, string) toDropdownItem(string enumName) => ($"{enumType.Name}.{enumName}", null);
        Dropdown.Selections = enumType.GetEnumNames().Select(toDropdownItem).ToList();
        Dropdown.Current = Array.IndexOf(values, valueAccessor.GetValue(cfg));
        //Plugin.Log($"Adding dropdown for config {field.Name}");
        //Plugin.Log($"Items:");
        //foreach (var item in Dropdown.Selections.Select(v => v.Item1))
        //    Plugin.Log(item);

        Dropdown.TitleId ??= field.Name;

        Action Setter(OptUIElement elmt)
        {
            if (elmt is not OptDropdown dropdown) return null;
            var busy = false;
            dropdown.OnValueChanged += idx =>
            {
                if (busy) return;
                if (valueAccessor.GetValue(cfg) == values.GetValue(idx)) return;
                busy = true;
                valueAccessor.SetValue(cfg, values.GetValue(idx));
                busy = false;
            };
            Action<object, EventArgs> onVariableChanged = (_, _) =>
            {
                if (busy) return;
                var idx = Array.IndexOf(values, valueAccessor.GetValue(cfg));
                if (dropdown.Value == idx) return;
                busy = true;
                dropdown.Value = idx;
                busy = false;
            };
            var d = Delegate.CreateDelegate(settingChangedEvent.EventHandlerType, onVariableChanged.Target, onVariableChanged.Method);
            settingChangedEvent.AddEventHandler(cfg, d);
            return ()=>settingChangedEvent.RemoveEventHandler(cfg, d);
        };
        return new(Parent, Dropdown, Setter);
    }
    #endregion
}