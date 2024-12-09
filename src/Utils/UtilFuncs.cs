using System;
using System.Collections.Generic;
using System.Text;

namespace EvilMask.Elin.ModOptions;

internal static class Utils
{
    public static bool AddGeneralTranslation(string idLang, string other_lang = null, string jp = null, string en = null)
    {
        if (Lang.Has(idLang)) return false;
        if (other_lang?.IsEmpty() ?? jp?.IsEmpty() ?? en?.IsEmpty() ?? true) return false;
        LangGeneral.Row row = Lang.General.CreateRow();
        row.id = idLang;
        row.text = other_lang;
        row.text_JP = jp;
        row.text_L = en;
        Lang.General.SetRow(row);

        return true;
    }
}
