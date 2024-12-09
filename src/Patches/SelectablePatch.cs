using EvilMask.Elin.ModOptions.UI;
using HarmonyLib;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EvilMask.Elin.ModOptions;

[HarmonyPatch]
internal sealed class SelectablePatch
{
    [HarmonyPatch(typeof(Selectable), nameof(Selectable.OnPointerEnter))]
    internal sealed class OnPointerEnter
    {
        public static bool Prefix(Selectable __instance, PointerEventData eventData)
        {
            if (!Plugin.Instance.Ready) return true;
            var drop = __instance as UIDropdown;
            var toggle = __instance as Toggle;
            var data = __instance.GetComponent<SelectionsTooltipData>();
            var helper = __instance.GetComponent<SelectionsItemTooltipHelper>();
            if ((drop == null || data == null) && (toggle == null || helper == null)) return true;
            if (data != null)
                data.ShowTooltip(drop.value, drop.transform);
            else helper.OnPointerEnter();
            return true;
        }
    }
    [HarmonyPatch(typeof(Selectable), nameof(Selectable.OnPointerExit))]
    internal sealed class OnPointerExit
    {
        public static bool Prefix(Selectable __instance, PointerEventData eventData)
        {
            if (!Plugin.Instance.Ready) return true;
            var drop = __instance as UIDropdown;
            var toggle = __instance as Toggle;
            var data = __instance.GetComponent<SelectionsTooltipData>();
            var helper = __instance.GetComponent<SelectionsItemTooltipHelper>();
            if ((drop == null || data == null) && (toggle == null || helper == null)) return true;

            if (data != null)
                data.HideTooltips();
            else helper.OnPointerExit();
            return true;
        }
    }
}