using System;
using System.Collections.Generic;
using System.Linq;

namespace TranslationsDocGen.SocialInfinite
{
    public class LocalizationItem
    {
        public readonly SheetAdapter Sheet;

        public readonly int RowFirst;
        public int RowLast => RowFirst + SubkeyRows.Count;
        
        public readonly string ItemName;
        public readonly List<LocalizationText> SubkeyRows = new List<LocalizationText>();

        private LocalizationItem(SheetAdapter sheet, int keyRow)
        {
            if (keyRow <= 0 || keyRow >= sheet.Values().Count)
            {
                throw new Exception($"LocaliztionText-> row out of range, sheet = '{sheet.Title()}', keyRow = '{keyRow}'");
            }

            int keyColumn = SocialInfiniteSheetsHelper.ITEM_NAME_COLUMN;
            if (sheet.CellValue(keyRow, keyColumn) == "")
            {
                throw new Exception($"LocaliztionText-> Key empty, sheet = '{sheet.Title()}', keyColumn = '{keyColumn}', row = '{keyRow}'");
            }
            
            
            int? subkeyColumnTemp = sheet.SubKeyColumn();
            if(!subkeyColumnTemp.HasValue) throw new Exception($"LocaliztionText-> sheet have not subkeyColumn, sheet = '{sheet.Title()}''");
            int subKeyColumn = subkeyColumnTemp.Value;  
            
            this.Sheet = sheet;
            this.RowFirst = keyRow;
            this.ItemName = sheet.CellValue(keyRow, keyColumn);
            
            bool itemNameLineEmpty(int row) => sheet.CellValue(row, keyColumn + 1).IsEmptyCell();    // if keyColumn == subKeyColumn
            bool subKeyEmpty(int row) => sheet.CellValue(row, subKeyColumn).IsEmptyCell();

            int curRow = keyRow + 1;
            while (curRow < sheet.Values().Count && !subKeyEmpty(curRow) && !itemNameLineEmpty(curRow))
            {
                var text = new LocalizationText(sheet, curRow, subKeyColumn);
                text.Item = this;
                this.SubkeyRows.Add(text);
                curRow++;
            }
            
        }

        private static Dictionary<SheetAdapter, Dictionary<int, LocalizationItem>> _getItemCache = new Dictionary<SheetAdapter, Dictionary<int, LocalizationItem>>();
        public static LocalizationItem GetItem(SheetAdapter sheet, int keyRow)
        {
            if (!_getItemCache.ContainsKey(sheet))
            {
                _getItemCache[sheet] = new Dictionary<int, LocalizationItem>();
            }
            
            if (!_getItemCache[sheet].ContainsKey(keyRow))
            {
                _getItemCache[sheet][keyRow] = new LocalizationItem(sheet, keyRow);
            }

            return _getItemCache[sheet][keyRow];
        }

        public IList<IList<object>> RowsWithName()
        {
            var res = new List<IList<object>>();

            for (int i = RowFirst; i <= RowLast; i++)
            {
                res.Add(Sheet.Values()[i]);
            }

            return res;
        }
    }
}