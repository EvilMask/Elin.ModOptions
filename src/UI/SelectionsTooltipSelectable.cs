using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EvilMask.Elin.ModOptions.UI
{
    internal sealed class SelectionsItemTooltipHelper : MonoBehaviour
    {
        private SelectionsTooltipData m_dropdown = null;
        public void OnPointerEnter()
        {
            var idx = name.Substring(5, Math.Max(name.IndexOf(':') - 5, 0)).ToInt();
            if (m_dropdown == null)
                m_dropdown = transform.parent.parent.parent.parent.GetComponent<SelectionsTooltipData>();
            m_dropdown.ShowTooltip(idx, transform);
        }
        public void OnPointerExit()
        {
            m_dropdown.HideTooltips();
        }
    }

    internal sealed class SelectionsTooltipData : MonoBehaviour
    {
        public List<string> Tooltips { get; set; } = [];
        public TooltipData Data { get; } = new() { enable = true, icon = true };

        public void ShowTooltip(int idx, Transform trans)
        {
            if (idx <= Tooltips.Count && Tooltips[idx] != null)
            {
                Data.text = Tooltips[idx];
                TooltipManager.Instance.ShowTooltip(Data, trans);
            }
        }
        public void HideTooltips(bool immediate = false)
        {
            TooltipManager.Instance.HideTooltips(immediate);
        }
    }

    internal sealed class ButtonLRTooltipData : Selectable
    {
        public List<string> Tooltips { get; set; } = [];
        public TooltipData Data { get; } = new() { enable = true, icon = true };

        public override void OnPointerEnter(PointerEventData _)
        {
            var idx = transform.parent.GetComponent<UIButtonLR>().index;
            if (idx <= Tooltips.Count && Tooltips[idx] != null)
            {
                Data.text = Tooltips[idx];
                TooltipManager.Instance.ShowTooltip(Data, transform);
                TooltipManager.Instance.tooltips[0].hideFunc = () => false; // Do not auto hide.
            }
        }
        public override void OnPointerExit(PointerEventData _)
        {
            TooltipManager.Instance.HideTooltips();
        }
    }
}
