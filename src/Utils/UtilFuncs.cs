using NPOI.XSSF.UserModel;
using System.IO;

namespace EvilMask.Elin.ModOptions;

public static class Utils
{
    internal static bool AddGeneralTranslation(string idLang, string other_lang = null, string jp = null, string en = null)
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

    public static void SetTranslationsFromXslx(this ModOptionController controller, string path)
    {
        using (var stream = new FileStream(path, FileMode.Open))
        {
            stream.Position = 0;
            XSSFWorkbook xssWorkbook = new(stream);
            for (int i = 0; i < xssWorkbook.NumberOfSheets; i++)
            {
                var sheet = xssWorkbook.GetSheetAt(i);
                var headerRow = sheet.GetRow(sheet.FirstRowNum);
                if (headerRow == null) continue;
                if ((headerRow.GetCell(0)?.StringCellValue?.ToLower() ?? null) == "id"
                    && (headerRow.GetCell(1)?.StringCellValue?.ToLower() ?? null) == "en"
                    && (headerRow.GetCell(2)?.StringCellValue?.ToLower() ?? null) == "jp"
                    && (headerRow.GetCell(3)?.StringCellValue?.ToLower() ?? null) == "cn")
                {
                    for (int j = sheet.FirstRowNum + 1; j <= sheet.LastRowNum; j++)
                    {
                        var row = sheet.GetRow(j);
                        var id = row.GetCell(0);
                        var en = row.GetCell(1);
                        var jp = row.GetCell(2);
                        var cn = row.GetCell(3);
                        Plugin.Log($"Row {j} - id:{id?.StringCellValue} en:{en?.StringCellValue} jp:{jp?.StringCellValue} cn:{cn?.StringCellValue}");
                        controller.SetTranslation(id?.StringCellValue, en?.StringCellValue, jp?.StringCellValue, cn?.StringCellValue);
                    }
                }
            }
        }
    }
}
