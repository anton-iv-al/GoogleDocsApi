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
            
            void tryAddCurItem()
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
                    tryAddCurItem();
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
            
            tryAddCurItem();

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
                .Where(s => s.Values.Count > 2)
                .ToList();

            foreach (var sheetData in newSheetsData)
            {
                Console.WriteLine($"New sheet = {sheetData.Title}, rows count = {sheetData.Values.Count}"); //TODO: log
            }
            
            return service.UploadSpreadsheet(newSpreadsheetTitle, newSheetsData);
        }

        public static List<Request> CopySpreadsheetLocale(SpreadsheetAdapter spreadsheetFrom, SpreadsheetAdapter spreadsheetTo, string locale)
        {
            IEnumerable<Request> res = new List<Request>();
            
            foreach (var sheetFrom in spreadsheetFrom.Sheets())
            {
                var sheetTo = spreadsheetTo.SheetByTitle(sheetFrom.Title());
                if (sheetTo == null) continue;
                
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
                    int? rowIndexTo = sheetTo.RowIndexByKey(key);
                    if (rowIndexTo.HasValue)
                    {
                        if (String.IsNullOrWhiteSpace(sheetTo.CellValue(rowIndexTo.Value, localeColumnTo)))
                        {
                            Console.WriteLine($"Copy to empty sheetTo = {sheetTo.Title()}, key = {key}");    //TODO: log
                        }
                        res.Add(sheetTo.UpdateCellRequest(
                            value: rowFrom.CellValue(localeColumnFrom), 
                            row: rowIndexTo.Value, 
                            column: localeColumnTo
                            ));
                    }
                    else
                    {
                        Console.WriteLine($"Not found text key = {key}, sheetTo = {sheetTo.Title()}"); //TODO: log
                    }
                }
            }
            
            Console.WriteLine($"Copying {res.Count} keys, sheetTo = {sheetTo.Title()}"); //TODO: log

            return res;
        }

        private static List<Request> CopySheetsLocaleItems(SheetAdapter sheetFrom, SheetAdapter sheetTo, string locale)
        {
            var itemsFrom = ItemList(sheetFrom);
            int localeColumnFrom = LocaleColumn(sheetFrom, locale);
            int ruColumnFrom = LocaleColumn(sheetFrom, "ru_RU");
            
            Console.WriteLine($"Copying {itemsFrom.Count} items, sheetTo = {sheetTo.Title()}"); //TODO: log
            
            return itemsFrom
                .SelectMany(item => CopyItemLocale(item, sheetTo, locale, localeColumnFrom, ruColumnFrom))
                .ToList();
        }

        private static List<Request> CopyItemLocale(LocalizationItem item, SheetAdapter sheetTo, string locale, int localeColumnFrom, int ruColumnFrom)
        {
            int localeColumnTo = LocaleColumn(sheetTo, locale);
            int? itemToRowIndex = sheetTo.RowIndexByKey(item.ItemName);

            if (!itemToRowIndex.HasValue)
            {
                Console.WriteLine($"Not found item_name = {item.ItemName}, sheetTo = {sheetTo.Title()}"); //TODO: log
                return new List<Request>();
            }
            
            if (String.IsNullOrWhiteSpace(sheetTo.CellValue(itemToRowIndex.Value + 1, localeColumnTo)))
            {
                Console.WriteLine($"Copy to empty sheetTo = {sheetTo.Title()}, itemName = {item.ItemName}"); //TODO: log
            }
            
            bool localeFromEmpty(IList<object> row) => String.IsNullOrWhiteSpace(row.CellValue(localeColumnFrom));
            bool ruFromEmpty(IList<object> row) => String.IsNullOrWhiteSpace(row.CellValue(ruColumnFrom));

            return item.Rows
                .Where(row => !localeFromEmpty(row) && !ruFromEmpty(row))    //TODO: no checking itemTo
                .Select((row, i) => sheetTo.UpdateCellRequest(
                    value: row.CellValue(localeColumnFrom), 
                    row: itemToRowIndex.Value + 1 + i, 
                    column: localeColumnTo
                ))
                .ToList();
        }

        private static Dictionary<SheetAdapter, Dictionary<string, int?>> _rowIndexByKeyCache = new Dictionary<SheetAdapter, Dictionary<string, int?>>();
        private static int? RowIndexByKey(this SheetAdapter sheet, string key)
        {
            if (!_rowIndexByKeyCache.ContainsKey(sheet))
            {
                _rowIndexByKeyCache[sheet] = sheet.Values().Skip(1)
                    .Select((row, i) => new {key = row.CellValue(0), i})
                    .GroupBy(pair => pair.key)
                    .Select(group =>
                    {
                        if (!String.IsNullOrWhiteSpace(group.Key) && group.Count() > 1)
                        {
                            throw new Exception($"Double key = {group.Key}, sheet = {sheet.Title()}");
                        }
                        return new {key = @group.Key, i = @group.First().i};
                    })
                    .ToDictionary(pair => pair.key, pair => new int?(pair.i + 1));
            }

            return _rowIndexByKeyCache[sheet].ContainsKey(key)
                ? _rowIndexByKeyCache[sheet][key]
                : null;
        }

        private static Dictionary<SheetAdapter, Dictionary<string, int>> _localeColumnCache = new Dictionary<SheetAdapter, Dictionary<string, int>>();
        private static int LocaleColumn(this SheetAdapter sheet, string locale)
        {
            void raiseNoLocale() => throw new Exception($"Sheet: '{sheet.Sheet.Properties.Title}' have not locale: '{locale}'");
            void raiseDoubleLocale() => throw new Exception($"Double locale = {locale}, sheet = {sheet.Title()}");
            
            if (!_localeColumnCache.ContainsKey(sheet))
            {
                if (sheet.Values().Count == 0) raiseNoLocale();
                
                _localeColumnCache[sheet] = new Dictionary<string, int>();
            }
            
            if (!_localeColumnCache[sheet].ContainsKey(locale))
            {
                var locales = sheet.Values()[0].ToList();
                var column = locales.IndexOf(locale);
                
                if (column < 0) raiseNoLocale();
                if (locales.LastIndexOf(locale) != column) raiseDoubleLocale();
                
                _localeColumnCache[sheet][locale] = column;
            }

            return _localeColumnCache[sheet][locale];
        }
    }
}