using System;
using UnityEngine;
using UnityEngine.EventSystems;

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
        public static EventTrigger.Entry AddPointerListener(this EventTrigger trigger, EventTriggerType type, Action<PointerEventData> action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener(eventData => action?.Invoke(eventData as PointerEventData));
            trigger.triggers.Add(entry);
            return entry;
        }
    }
}
