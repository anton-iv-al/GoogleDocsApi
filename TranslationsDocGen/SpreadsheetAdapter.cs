using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace TranslationsDocGen
{
    public class SpreadsheetAdapter
    {
        private readonly SheetsService _service;
        private readonly string _spreadsheetId;
        
        public SpreadsheetAdapter(SheetsService service, string spreadsheetId)
        {
            _service = service;
            _spreadsheetId = spreadsheetId;
        }

        private IEnumerable<SheetWrapper> _sheetsCache;
        public IEnumerable<SheetWrapper> Sheets()
        {
            if (_sheetsCache == null)
            {
                _sheetsCache = _service.Spreadsheets
                    .Get(_spreadsheetId)
                    .Execute()
                    .Sheets
                    .Select(sheet => new SheetWrapper(_service, _spreadsheetId, sheet));
            }

            return _sheetsCache;
        }
        
        public void WriteToConsole()
        {
            foreach (var sheet in Sheets())
            {
                Console.WriteLine(sheet.Sheet.Properties.Title);
                
                foreach (IList<object> row in sheet.Values())
                {
                    foreach (string cell in row)
                    {
                        Console.Write(cell + ", ");
                    }
                    Console.WriteLine();
                }
                
                Console.WriteLine();
            }
        }
    }
}