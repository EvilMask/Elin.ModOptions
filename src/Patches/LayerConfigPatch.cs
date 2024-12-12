using HarmonyLib;
using EvilMask.Elin.ModOptions.UI;
using UnityEngine;

namespace EvilMask.Elin.ModOptions;

[HarmonyPatch]
internal sealed class LayerConfigPatch
{
    private static ContentConfigModOptions m_content = null;

    [HarmonyPatch(typeof(LayerConfig), nameof(LayerConfig.OnInit))]
    internal sealed class OnInit
    {
        public static bool Prefix(LayerConfig __instance)
        {
            if (!Plugin.Instance.Ready) return true;
            Utils.AddGeneralTranslation(UILang.TabName, "Mods", "Mods", "模组");
            Plugin.Dict.SetTranslation(UILang.NoManagedMod,
                "No mod option is managed by Mod Options",
                "Mod Optionsが管理されているMod設定はありません",
                "没有需要Mod Options管理的模组设定");
            ContentConfigModOptions.Init(__instance.windows[0]);
            m_content = new GameObject("Mod Options").AddComponent<ContentConfigModOptions>();
            m_content.gameObject.transform.SetParent(__instance.windows[0].view.transform);
            m_content.BuildUI();
            __instance.windows[0].AddTab(UILang.TabName, m_content, sprite: SpriteSheet.Get("icon_menu_82"));
            return true;
        }
    }

    [HarmonyPatch(typeof(LayerConfig), nameof(LayerConfig.OnSwitchContent))]
    internal sealed class OnSwitchContent
    {
        public static bool Prefix(LayerConfig __instance, Window window)
        {
            if (!Plugin.Instance.Ready) return true;
            if (m_content != null && window.CurrentTab.idLang == UILang.TabName)
            {
                window.rectBottom.SetActive(false);
                window.CurrentContent.RebuildLayout(recursive: true);
            }
            else window.rectBottom.SetActive(true);
            return true;
        }
    }

    [HarmonyPatch(typeof(Layer), nameof(Layer.RemoveLayer), [typeof(Layer)])]
    internal sealed class RemoveLayer
    {
        public static bool Prefix(Layer l)
        {
            if (m_content != null && l?.name == "LayerConfig")
            {
                var ctrl = m_content.Controlller;
                if (ctrl != null && ctrl.Builder != null)
                {
                    ctrl.Builder.Deleted = true;
                    ctrl.Builder = null;
                }
            }
            return true;
        }
    }
}
