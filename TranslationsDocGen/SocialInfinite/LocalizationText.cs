using System;
using System.Collections.Generic;
using System.Linq;

namespace TranslationsDocGen.SocialInfinite
{
    public class LocalizationText
    {
        public readonly SheetAdapter Sheet;
        public LocalizationItem Item;

        public readonly string Key;
        public readonly int Row;

        private readonly Dictionary<string, string> _translationsByLocale;    //TODO: try flyweight
        public string Translation(string locale)
        {
            return _translationsByLocale.ContainsKey(locale)
                ?_translationsByLocale[locale]
                : null;
        }

        public LocalizationText(SheetAdapter sheet, int row, int keyColumn)
        {
            if (row <= 1 || row >= sheet.Values().Count)
            {
                throw new Exception($"LocaliztionText-> row out of range, sheet = '{sheet.Title()}', row = '{row}'");
            }
            
            if (sheet.CellValue(row, keyColumn) == "")
            {
                throw new Exception($"LocaliztionText-> Key empty, sheet = '{sheet.Title()}', keyColumn = '{keyColumn}', row = '{row}'");
            }
            
            this.Sheet = sheet;
            this.Key = sheet.CellValue(row, keyColumn);
            this.Row = row;

            _translationsByLocale = sheet.Values()[row]
                .Select((cell, i) => new {cell = cell as string, i})
                .Where(pair => pair.i != keyColumn &&
                                    !sheet.Values()[0].CellValue(pair.i).IsEmptyCell()
                )
                .ToDictionary(pair => sheet.Values()[0].CellValue(pair.i), pair => pair.cell);
        }
    }
}