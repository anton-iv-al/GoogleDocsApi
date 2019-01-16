using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Sheets.v4.Data;

namespace TranslationsDocGen
{
    public static class SocialInfiniteSheetsHelper
    {
        public static bool IsRowEmpty(this IList<object> row)
        {
            return row.Count == 0 ||
                   (row[0] as string).StartsWith("#");
        }

        public static int LocaleColumn(this SheetWrapper sheet, string locale)
        {
            void Raise() => throw new Exception($"Sheet: '{sheet.Sheet.Properties.Title}' have not locale: '{locale}'");

            if (sheet.Values().Count == 0) Raise();
            
            var column = sheet.Values()[0].IndexOf(locale);
            if (column < 0) Raise();

            return column;
        }

        public static IList<IList<object>> RowsWithoutLocale(this SheetWrapper sheet, string locale, int separateRowsCount)
        {
            int column = sheet.LocaleColumn(locale);
            
            bool withLocale(IList<object> row) => !row.IsRowEmpty() &&
                                                  String.IsNullOrWhiteSpace(row.CellValue(column));

            var res = new List<IList<object>>();
            res.Add(sheet.Values().First());

            bool prevWithLocale = false;
            foreach (IList<object> row in sheet.Values().Skip(1))
            {
                bool curWithLocale = withLocale(row);
                
                if (curWithLocale)
                {
                    res.Add(row);
                }
                
                if (!curWithLocale && prevWithLocale)
                {
                    for (int i = 0; i < separateRowsCount; i++)
                    {
                        res.Add(new List<object>());
                    }
                }

                prevWithLocale = curWithLocale;
            }

            return res;
        }
    }
}