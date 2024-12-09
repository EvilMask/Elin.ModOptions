
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EvilMask.Elin.ModOptions.UI;

public class UIToggleGroup : MonoBehaviour
{
    internal List<UIButton> Toggles { get; } = [];
    private bool m_enabled = true;

    public int Min { get; set; } = 1;
    public bool Enabled
    {
        get => m_enabled;
        set
        {
            if (value != m_enabled) return;
            m_enabled = value;
            if (value)
            {
                foreach (var toggle in Toggles)
                {
                    if (toggle.isChecked) toggle.SetInteractableWithAlpha(true);
                    else if (Min == 1 || CurrentSelected < Max) toggle.SetInteractableWithAlpha(true);
                }
            }
            else
            {
                foreach (var toggle in Toggles)
                    toggle.SetInteractableWithAlpha(false);
            }
        }
    }
    public int Max
    {
        get => m_max;
        set
        {
            m_max = Math.Max(Min, value);
            for (int i = Toggles.Count - 1; i >= 0; i--)
            {
                if (CurrentSelected > m_max)
                {
                    if (!Toggles[i].isChecked) continue;
                    Toggles[i].isChecked = false;
                    if (Min > 1) Toggles[i].enabled = false;
                    CurrentSelected--;
                }
                else break;
            }
        }
    }
    private int m_max = 1;
    private bool m_busy = false;
    public int CurrentSelected { get; set; } = 0;
    public void AddToggle(UIButton toggle)
    {
        if (CurrentSelected == Max)
        {
            toggle.isChecked = false;
            if (Min > 1) toggle.enabled = false;
        }
        else if (toggle.isChecked) CurrentSelected++;
        Toggles.Add(toggle);
    }
    public void SetToggle(int index, bool value, bool setValue = true)
    {
        if (m_busy) return;
        // Plugin.Log($"try set {index} to {value} selected = {CurrentSelected}");
        if (index < 0 || index >= Toggles.Count) return;
        if (setValue) Toggles[index].SetCheck(value);
        m_busy = true;
        if (value)
        {
            if (CurrentSelected == Max)
            {
                if (Max == 1)
                {
                    for (int i = 0; i < Toggles.Count; i++)
                        if (i != index) Toggles[i].SetCheck(false);
                }
                else
                    Toggles[index].SetCheck(false);
                // Plugin.Log($"reversed.");
                m_busy = false;
                return;
            }
            CurrentSelected++;
            if (CurrentSelected == Max && Max > 1)
            {
                foreach (var toggle in Toggles)
                    if (!toggle.isChecked) toggle.SetInteractableWithAlpha(false);
                // Plugin.Log($"locked others.");
            }
        }
        else
        {
            if (CurrentSelected <= Min)
            {
                Toggles[index].SetCheck(true);
                m_busy = false;
                // Plugin.Log($"reversed.");
                return;
            }
            CurrentSelected--;
            if (Max > 1 && CurrentSelected < Max)
            {
                foreach (var toggle in Toggles)
                    if (!toggle.isChecked) toggle.SetInteractableWithAlpha(true);
                // Plugin.Log($"unlocked others.");
            }
        }
        // Plugin.Log($"accepted.");
        m_busy = false;
        OnValueChanged.Invoke(index, value);
    }
    public class ToggleGroupEvent : UnityEvent<int, bool> { }
    public ToggleGroupEvent OnValueChanged = new();
}
