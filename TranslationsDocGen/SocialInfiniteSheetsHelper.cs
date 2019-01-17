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
            
            var res = new List<IList<object>>();
            res.Add(sheet.Values().First());
            res.Add(new List<object>());

            if (IsItemsSheet(sheet))
            {
                res = res
                    .Concat(RowsWithoutLocaleItems(sheet, locale))
                    .ToList();
            }
            else
            {
                res = res
                    .Concat(RowsWithoutLocaleText(sheet, locale, separateRowsCount))
                    .ToList();
            }

            return res;
        }

        private static IList<IList<object>> RowsWithoutLocaleText(SheetAdapter sheet, string locale, int separateRowsCount)
        {
            int localeColumn = sheet.LocaleColumn(locale);
            
            bool withLocale(IList<object> row) => !row.IsRowEmpty() &&
                                                  String.IsNullOrWhiteSpace(row.CellValue(localeColumn));
            
            var res = new List<IList<object>>();

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

        private static IList<IList<object>> RowsWithoutLocaleItems(SheetAdapter sheet, string locale)
        {
            return ItemsWithoutLocaleItems(sheet, locale)
                .SelectMany(i => i.RowsWithName())
                .ToList();
        }

        private static IEnumerable<LocalizationItem> ItemsWithoutLocaleItems(SheetAdapter sheet, string locale)
        {
            int localeColumn = sheet.LocaleColumn(locale);
            int ruColumn = sheet.LocaleColumn("ru_RU");
            
            var items = ItemList(sheet);

            return items
                .Where(item => item.Rows.Any(
                    row => !String.IsNullOrWhiteSpace(row.CellValue(ruColumn)) &&
                           String.IsNullOrWhiteSpace(row.CellValue(localeColumn))
                ));
        }

        private static List<LocalizationItem> ItemList(SheetAdapter sheet)
        {
            var res = new List<LocalizationItem>();
            
            LocalizationItem curItem = null;
            
            void TryAddCurItem()
            {
                if (curItem != null && curItem.Rows.Count > 0)
                {
                    res.Add(curItem);
                }
                curItem = null;
            }
            
            
            foreach (IList<object> row in sheet.Values().Skip(1))
            {
                bool firstEmpty() => String.IsNullOrWhiteSpace(row.CellValue(0));
                bool secondEmpty() => String.IsNullOrWhiteSpace(row.CellValue(1));    //TODO: required second column keys
                
                if (!firstEmpty() || secondEmpty())
                {
                    TryAddCurItem();
                }
                
                if (!firstEmpty())
                {
                    curItem = new LocalizationItem(row.CellValue(0));
                }
                
                if (curItem != null && !secondEmpty())
                {
                    curItem.Rows.Add(row);
                }
            }
            
            TryAddCurItem();

            return res;
        }

        public static bool IsItemsSheet(this SheetAdapter sheet)
        {
            return sheet.CellValue(0, 1) == "keys";        //TODO: required "keys"
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

        public static List<Request> CopySpreadsheetLocale(SpreadsheetAdapter spreadsheetFrom, SpreadsheetAdapter spreadsheetTo, string locale)
        {
            IEnumerable<Request> res = new List<Request>();
            
            foreach (var sheetFrom in spreadsheetFrom.Sheets())
            {
                var sheetsWithTitle = spreadsheetTo.SheetsByTitle(sheetFrom.Title());
                if (sheetsWithTitle.Count != 1) continue;                                //TODO: title duplication
                var sheetTo = sheetsWithTitle[0];
                
                res = res.Concat(CopySheetsLocale(sheetFrom, sheetTo, locale));
            }

            return res.ToList();
        }

        public static List<Request> CopySheetsLocale(SheetAdapter sheetFrom, SheetAdapter sheetTo, string locale)
        {
            if(IsItemsSheet(sheetFrom))
            {
                return CopySheetsLocaleItems(sheetFrom, sheetTo, locale);
            }
            else
            {
                return CopySheetsLocaleText(sheetFrom, sheetTo, locale);
            }
        }

        private static List<Request> CopySheetsLocaleText(SheetAdapter sheetFrom, SheetAdapter sheetTo, string locale)
        {
            int localeColumnFrom = LocaleColumn(sheetFrom, locale);
            int localeColumnTo = LocaleColumn(sheetTo, locale);
            
            var valuesFrom = sheetFrom.Values().Skip(1);

            var res = new List<Request>();

            bool keyEmpty(IList<object> row) => String.IsNullOrWhiteSpace(row.CellValue(0));
            bool localeFromEmpty(IList<object> row) => String.IsNullOrWhiteSpace(row.CellValue(localeColumnFrom));

            foreach (IList<object> rowFrom in valuesFrom)
            {
                if (!keyEmpty(rowFrom) && !localeFromEmpty(rowFrom))
                {
                    string key = rowFrom.CellValue(0);
                    int rowIndexTo = RowIndexByKey(sheetTo, key);
                    if (rowIndexTo >= 0)
                    {
                        res.Add(sheetTo.UpdateCellRequest(
                            value: rowFrom.CellValue(localeColumnFrom), 
                            row: rowIndexTo, 
                            column: localeColumnTo
                            ));
                    }
                }
            }

            return res;
        }

        private static List<Request> CopySheetsLocaleItems(SheetAdapter sheetFrom, SheetAdapter sheetTo, string locale)
        {
            var itemsFrom = ItemList(sheetFrom);
            int localeColumnFrom = LocaleColumn(sheetFrom, locale);
            int ruColumnFrom = LocaleColumn(sheetFrom, "ru_RU");
            
            return itemsFrom
                .SelectMany(item => CopyItemLocale(item, sheetTo, locale, localeColumnFrom, ruColumnFrom))
                .ToList();
        }

        private static List<Request> CopyItemLocale(LocalizationItem item, SheetAdapter sheetTo, string locale, int localeColumnFrom, int ruColumnFrom)
        {
            int localeColumnTo = LocaleColumn(sheetTo, locale);
            
            bool localeFromEmpty(IList<object> row) => String.IsNullOrWhiteSpace(row.CellValue(localeColumnFrom));
            bool ruFromEmpty(IList<object> row) => String.IsNullOrWhiteSpace(row.CellValue(ruColumnFrom));

            return item.Rows
                .Where(row => !localeFromEmpty(row) && !ruFromEmpty(row))    //TODO: no checking itemTo
                .Select((row, i) => sheetTo.UpdateCellRequest(
                    value: row.CellValue(localeColumnFrom), 
                    row: RowIndexByKey(sheetTo, item.ItemName) + 1 + i, 
                    column: localeColumnTo
                ))
                .ToList();
        }

        private static Dictionary<SheetAdapter, Dictionary<string, int>> _rowIndexByKeyCache = new Dictionary<SheetAdapter, Dictionary<string, int>>();
        private static int RowIndexByKey(SheetAdapter sheet, string key)
        {
            if (!_rowIndexByKeyCache.ContainsKey(sheet))
            {
                _rowIndexByKeyCache[sheet] = sheet.Values().Skip(1)
                    .Select((row, i) => new {i, key = row.CellValue(0)})    //TODO: key duplication
                    .GroupBy(pair => pair.key)
                    .Select(group => new {key = group.Key, i = group.First().i})
                    .ToDictionary(pair => pair.key, pair => pair.i + 1);
            }

            return _rowIndexByKeyCache[sheet].ContainsKey(key)
                ? _rowIndexByKeyCache[sheet][key]
                : -1;
        }
    }
}