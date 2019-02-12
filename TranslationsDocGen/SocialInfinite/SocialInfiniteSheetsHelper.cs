using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace TranslationsDocGen.SocialInfinite
{
    public static class SocialInfiniteSheetsHelper
    {
        public static int ITEM_NAME_COLUMN = 0;
        
        public static bool IsRowEmpty(this IList<object> row)
        {
            return row.Count == 0 ||
                   (row[0] as string).StartsWith("#");
        }
        
        public static bool IsEmptyCell(this object cell)
        {
            var s = cell as string;
            if (s == null) return true;
            return String.IsNullOrWhiteSpace(s) ||
                   s.StartsWith("#");
        }

        public static IList<IList<object>> RowsWithoutLocale(this SheetAdapter sheet, string locale, string defaultLocale)
        {
            
            var res = new List<IList<object>>();
            res.Add(sheet.Values().First());
            res.Add(new List<object>());

            if (IsItemsSheet(sheet))
            {
                res = res
                    .Concat(RowsWithoutLocaleItems(sheet, locale, defaultLocale))
                    .ToList();
            }
            else
            {
                res = res
                    .Concat(RowsWithoutLocaleText(sheet, locale, defaultLocale))
                    .ToList();
            }

            return res;
        }

        private static IList<IList<object>> RowsWithoutLocaleText(SheetAdapter sheet, string locale, string defaultLocale)
        {
            int localeColumn = sheet.LocaleColumn(locale);
            int defaultLocaleColumn = sheet.LocaleColumn(defaultLocale);
            
            bool withoutLocale(IList<object> row) => !row.IsRowEmpty() &&
                                                     !row.CellValue(defaultLocaleColumn).IsEmptyCell() &&
                                                      row.CellValue(localeColumn).IsEmptyCell();
            
            var res = new List<IList<object>>();

            foreach (IList<object> row in sheet.Values().Skip(1))
            {
                if (withoutLocale(row))
                {
                    res.Add(row);
                }
            }

            return res;
        }

        private static IList<IList<object>> RowsWithoutLocaleItems(SheetAdapter sheet, string locale, string defaultLocale)
        {
            return ItemsWithoutLocaleItems(sheet, locale, defaultLocale)
                .SelectMany(i => i.RowsWithName())
                .ToList();
        }

        private static IEnumerable<LocalizationItem> ItemsWithoutLocaleItems(SheetAdapter sheet, string locale, string defaultLocale)
        {
            var items = ItemList(sheet);

            return items
                .Where(item => item.SubkeyRows.Any(
                    row => !row.Translation(defaultLocale).IsEmptyCell() &&
                           row.Translation(locale).IsEmptyCell()
                ));
        }

        private static Dictionary<SheetAdapter, List<LocalizationItem>> _itemsListCache = new Dictionary<SheetAdapter, List<LocalizationItem>>();
        private static List<LocalizationItem> ItemList(SheetAdapter sheet)
        {
            if (!_itemsListCache.ContainsKey(sheet))
            {      
                var res = new List<LocalizationItem>();
            
                for (int i = 1; i < sheet.Values().Count; ++i)
                {
                    IList<object> row = sheet.Values()[i];
                
                    bool itemNameEmpty() => row.CellValue(ITEM_NAME_COLUMN).IsEmptyCell();
                    bool itemNameLineEmpty() => row.CellValue(ITEM_NAME_COLUMN + 1).IsEmptyCell();    // if textKeyColumn == subKeyColumn
                
                    if (!itemNameEmpty() && itemNameLineEmpty())
                    {
                        if (!itemNameEmpty() && !itemNameLineEmpty())
                        {
                            throw new Exception($"ItemsList-> item_name skipped, sheet = '{sheet.Title()}', keyRow = '{i}'");
                        }
                    
                        var item = LocalizationItem.GetItem(sheet, i);
                        res.Add(item);
                        i = item.RowLast;    // after i++ next = item.RowLast + 1
                    }
                }

                _itemsListCache[sheet] = res;
            }

            return _itemsListCache[sheet];
        }

        public static bool IsItemsSheet(this SheetAdapter sheet)
        {
            return SubKeyColumn(sheet).HasValue;
        }

        public static int? SubKeyColumn(this SheetAdapter sheet)    //TODO: required "keys"
        {
            if(sheet.CellValue(0, 0) == "keys") return 0;        
            if(sheet.CellValue(0, 1) == "keys") return 1;        
            return null;        
        }

        public static SpreadsheetAdapter UploadMissingLocaleSpreadsheet(this SheetsService service, string originalSpreadsheetId, string locale, string defaultLocale, string newSpreadsheetTitle)
        {
            var spreadsheet = service.DownloadSpredsheet(originalSpreadsheetId);

            var newSheetsData = spreadsheet.Sheets()
                .Select(sheet => new SheetData()
                {
                    Title = sheet.Title(),
                    Values = sheet.RowsWithoutLocale(locale, defaultLocale),
                    GridProperties = new GridProperties(){FrozenRowCount = 1},
                })
                .Where(s => s.Values.Count > 2)
                .ToList();

            foreach (var sheetData in newSheetsData)
            {
                Logger.Log($"New sheet = {sheetData.Title}, rows count = {sheetData.Values.Count}"); 
            }
            
            return service.UploadSpreadsheet(newSpreadsheetTitle, newSheetsData);
        }

        public static List<Request> CopySpreadsheetLocale(SpreadsheetAdapter spreadsheetFrom, SpreadsheetAdapter spreadsheetTo, string locale, bool canRewrite)
        {
            IEnumerable<Request> res = new List<Request>();
            
            foreach (var sheetFrom in spreadsheetFrom.Sheets())
            {
                var sheetTo = spreadsheetTo.SheetByTitle(sheetFrom.Title());
                if (sheetTo == null) continue;
                
                res = res.Concat(CopySheetsLocale(sheetFrom, sheetTo, locale, canRewrite));
            }

            return res.ToList();
        }

        public static List<Request> CopySheetsLocale(SheetAdapter sheetFrom, SheetAdapter sheetTo, string locale, bool canRewrite)
        {
            if(IsItemsSheet(sheetFrom))
            {
                return CopySheetsLocaleItems(sheetFrom, sheetTo, locale, canRewrite);
            }
            else
            {
                return CopySheetsLocaleText(sheetFrom, sheetTo, locale, canRewrite);
            }
        }

        private static List<Request> CopySheetsLocaleText(SheetAdapter sheetFrom, SheetAdapter sheetTo, string locale, bool canRewrite)
        {
            var keyColumn = 0;
            
            var res = new List<Request>();

            bool keyEmpty(IList<object> row) => row.CellValue(keyColumn).IsEmptyCell();

            for (int i = 1; i < sheetFrom.Values().Count; ++i)
            {
                IList<object> rowFrom = sheetFrom.Values()[i];
                
                if (!keyEmpty(rowFrom))
                {
                    var text = new LocalizationText(sheetFrom, i, keyColumn);
                    
                    var request = CopyTextLocale(text, sheetTo, locale, keyColumn, canRewrite);
                    if(request != null) res.Add(request);
                }
            }
            
            Logger.Log($"Copying {res.Count} keys, sheetTo = {sheetTo.Title()}"); 

            return res;
        }

        private static Request CopyTextLocale(LocalizationText textFrom, SheetAdapter sheetTo, string locale, int keyColumn, bool canRewrite)
        {
            string itemLog() => textFrom.Item == null ? "" : $"item = {textFrom.Item.ItemName}, ";
            

//            if (textFrom.Translation("ru_RU").IsEmptyCell())
//            {
//                Logger.Log($"CopyTextLocale-> empty default locale, sheetFrom = {textFrom.Sheet.Title()}, {itemLog()}key = {textFrom.Key}, locale = {locale}");    
//                return null;
//            }
            
            if (textFrom.Translation(locale).IsEmptyCell())
            {
                Logger.Log($"CopyTextLocale-> empty locale, sheetFrom = {textFrom.Sheet.Title()}, {itemLog()}key = {textFrom.Key}, locale = {locale}");    
                return null;
            }
            
            int localeColumnTo = LocaleColumn(sheetTo, locale);
            
            int? rowIndexTo = TextRowIndex(sheetTo, keyColumn, textFrom);
            
            if (rowIndexTo.HasValue)
            {
                bool writingToEmptyCell = sheetTo.CellValue(rowIndexTo.Value, localeColumnTo).IsEmptyCell();
                if (writingToEmptyCell)
                {
                    Logger.Log($"Copy to empty text, sheetTo = {sheetTo.Title()}, {itemLog()}key = {textFrom.Key}, locale = {locale}");    
                }

                bool changingCell = sheetTo.CellValue(rowIndexTo.Value, localeColumnTo) != textFrom.Translation(locale);
                if (!writingToEmptyCell && changingCell)
                {
                    if (!canRewrite) return null;
                    Logger.Log($"Rewrite text value, sheetTo = {sheetTo.Title()}, {itemLog()}key = {textFrom.Key}, locale = {locale}");    
                }
                
                return sheetTo.UpdateCellRequest(
                    value: textFrom.Translation(locale), 
                    row: rowIndexTo.Value, 
                    column: localeColumnTo
                );
            }
            else
            {
                Logger.Log($"Not found text, {itemLog()}key = {textFrom.Key}, sheetTo = {sheetTo.Title()}"); 
                return null;
            }
        }

        private static int? TextRowIndex(SheetAdapter sheetTo, int textKeyColumn, LocalizationText textFrom)
        {
            if (textFrom.Item == null)
            {
                return sheetTo.RowIndexByKey(keyColumn: textKeyColumn, isSubColumn: textFrom.Item != null, key: textFrom.Key);
            }
            else
            {
                int? itemToRowIndex = sheetTo.RowIndexByKey(keyColumn: ITEM_NAME_COLUMN, isSubColumn: false, key: textFrom.Item.ItemName);    
                if (!itemToRowIndex.HasValue)
                {
                    Logger.Log($"Not found item_name = {textFrom.Item.ItemName}, sheetTo = {sheetTo.Title()}"); 
                    return null;
                }
            
                var item = LocalizationItem.GetItem(sheetTo, itemToRowIndex.Value);
                    
                var textTo = item.SubkeyRows.FirstOrDefault(curTextTo => curTextTo.Key == textFrom.Key);
                return textTo == null ? null : new int?(textTo.Row);
            }
        }

        private static List<Request> CopySheetsLocaleItems(SheetAdapter sheetFrom, SheetAdapter sheetTo, string locale, bool canRewrite)
        {
            var itemsFrom = ItemList(sheetFrom);
            
            Logger.Log($"Copying {itemsFrom.Count} items, sheetTo = {sheetTo.Title()}"); 
            
            return itemsFrom
                .SelectMany(item => CopyItemLocale(item, sheetTo, locale, canRewrite))
                .ToList();
        }

        private static List<Request> CopyItemLocale(LocalizationItem itemFrom, SheetAdapter sheetTo, string locale, bool canRewrite)
        {
            int? subkeyColumnTemp = SubKeyColumn(sheetTo);
            if(!subkeyColumnTemp.HasValue) throw new Exception($"CopyItemLocale-> sheet have not subkeyColumn, sheetTo = '{sheetTo.Title()}''");
            int subKeyColumnTo = subkeyColumnTemp.Value;         
            
            int? itemToRowIndex = sheetTo.RowIndexByKey(keyColumn: ITEM_NAME_COLUMN, isSubColumn: false, key: itemFrom.ItemName);

            if (!itemToRowIndex.HasValue)
            {
                Logger.Log($"Not found item_name = {itemFrom.ItemName}, sheetTo = {sheetTo.Title()}"); 
                return new List<Request>();
            }

            return itemFrom.SubkeyRows
                .Select(text => CopyTextLocale(text, sheetTo, locale, subKeyColumnTo, canRewrite))
                .Where(request => request != null)
                .ToList();
        }

        private static Dictionary<SheetAdapter, Dictionary<int, Dictionary<string, int?>>> _rowIndexByKeyCache = new Dictionary<SheetAdapter, Dictionary<int, Dictionary<string, int?>>>();
        private static int? RowIndexByKey(this SheetAdapter sheet, int keyColumn, bool isSubColumn, string key)
        {

            if (!_rowIndexByKeyCache.ContainsKey(sheet))
            {
                _rowIndexByKeyCache[sheet] = new Dictionary<int, Dictionary<string, int?>>();
            }

            if (!_rowIndexByKeyCache[sheet].ContainsKey(keyColumn))
            {
                _rowIndexByKeyCache[sheet][keyColumn] = sheet.Values().Skip(1)
                    .Select((row, i) => new {key = row.CellValue(keyColumn), i})
                    .GroupBy(pair => pair.key)
                    .Select(group =>
                    {
                        if (!isSubColumn && !IsEmptyCell(group.Key) && group.Count() > 1)
                        {
                            throw new Exception($"Double key = {group.Key}, sheet = {sheet.Title()}");
                        }
                        return new {key = @group.Key, i = @group.First().i};
                    })
                    .ToDictionary(pair => pair.key, pair => new int?(pair.i + 1));
            }

            return _rowIndexByKeyCache[sheet][keyColumn].ContainsKey(key)
                ? _rowIndexByKeyCache[sheet][keyColumn][key]
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