using UnityEngine;

namespace EvilMask.Elin.ModOptions.Ext
{
    internal static class ClassExtensions
    {
        public static void SetOffsetToAnchored(this RectTransform trans, float left, float top, float right, float bottom)
        {
            trans.offsetMin = new Vector2(left, bottom);
            trans.offsetMax = new Vector2(-right, -top);
        }

        public static string tr(this string id)
        {
            return Plugin.Dict.Tr(id);
        }
    }
}
