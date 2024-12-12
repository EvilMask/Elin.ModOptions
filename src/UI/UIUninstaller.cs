using UnityEngine;

namespace EvilMask.Elin.ModOptions.UI;
internal class UIUninstaller : MonoBehaviour
{
    private void OnDestroy()
    {
        // Unsubscribe SettingChanged event for managed config from reflections.
        var controller = GetComponent<ContentConfigModOptions>();
        if ((controller?.Controlller?.CleanersForCfg.Count ?? 0) > 0)
        {
            foreach (var cleaner in controller.Controlller.CleanersForCfg) cleaner();
            controller.Controlller.CleanersForCfg.Clear();
        }
    }
}
