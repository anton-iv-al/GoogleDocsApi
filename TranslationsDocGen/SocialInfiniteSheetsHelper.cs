using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Sheets.v4;
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

        public static int LocaleColumn(this SheetAdapter sheet, string locale)
        {
            void Raise() => throw new Exception($"Sheet: '{sheet.Sheet.Properties.Title}' have not locale: '{locale}'");

            if (sheet.Values().Count == 0) Raise();
            
            var column = sheet.Values()[0].IndexOf(locale);
            if (column < 0) Raise();

            return column;
        }

        public static IList<IList<object>> RowsWithoutLocale(this SheetAdapter sheet, string locale, int separateRowsCount)
        {
            int localeColumn = sheet.LocaleColumn(locale);
            int ruColumn = sheet.LocaleColumn("ru_RU");
            
            var res = new List<IList<object>>();
            res.Add(sheet.Values().First());
            res.Add(new List<object>());

            if (IsItemsSheet(sheet))
            {
                res = res
                    .Concat(RowsWithoutLocaleItems(sheet.Values().Skip(1), localeColumn, ruColumn))
                    .ToList();
            }
            else
            {
                res = res
                    .Concat(RowsWithoutLocaleText(sheet.Values().Skip(1), localeColumn, separateRowsCount))
                    .ToList();
            }

            return res;
        }

        private static IList<IList<object>> RowsWithoutLocaleText(IEnumerable<IList<object>> values, int localeColumn, int separateRowsCount)
        {
            bool withLocale(IList<object> row) => !row.IsRowEmpty() &&
                                                  String.IsNullOrWhiteSpace(row.CellValue(localeColumn));
            
            var res = new List<IList<object>>();

            bool prevWithLocale = false;
            foreach (IList<object> row in values)
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

        private static IList<IList<object>> RowsWithoutLocaleItems(IEnumerable<IList<object>> values, int localeColumn, int ruColumn)
        {
            var items = ItemList(values);
            
            return items
                .Where(item => item.Skip(1).Any(
                    row => !String.IsNullOrWhiteSpace(row.CellValue(ruColumn)) &&
                           String.IsNullOrWhiteSpace(row.CellValue(localeColumn))
                ))
                .SelectMany(i => i)
                .ToList();
        
                
        }

        private static List<IList<IList<object>>> ItemList(IEnumerable<IList<object>> values)
        {
            var res = new List<IList<IList<object>>>();
            
            IList<IList<object>> curItem = null;
            
            void TryAddCurItem()
            {
                if (curItem != null && curItem.Count > 1)
                {
                    res.Add(curItem);
                }
                curItem = null;
            }
            
            
            foreach (IList<object> row in values)
            {
                bool firstEmpty() => String.IsNullOrWhiteSpace(row.CellValue(0));
                bool secondEmpty() => String.IsNullOrWhiteSpace(row.CellValue(1));
                
                if (!firstEmpty() || secondEmpty())
                {
                    TryAddCurItem();
                }
                
                if (!firstEmpty())
                {
                    curItem = new List<IList<object>>();
                    curItem.Add(row);
                }
                
                if (curItem != null && !secondEmpty())
                {
                    curItem.Add(row);
                }
            }
            
            TryAddCurItem();

            return res;
        }

        public static bool IsItemsSheet(this SheetAdapter sheet)
        {
            return sheet.CellValue(0, 1) == "keys";
        }

        public static SpreadsheetAdapter UploadMissingLocaleSpreadsheet(this SheetsService service, string originalSpreadsheetId, string locale, string newSpreadsheetTitle)
        {
            var spreadsheet = service.DownloadSpredsheet(originalSpreadsheetId);

            var newSheetsData = spreadsheet.Sheets()
                .Select(sheet => new SheetData()
                {
                    Title = sheet.Title(),
                    Values = sheet.RowsWithoutLocale(locale, 2)
                })
                .ToList();
            
            return service.UploadSpreadsheet(newSpreadsheetTitle, newSheetsData);
        }
    }
}