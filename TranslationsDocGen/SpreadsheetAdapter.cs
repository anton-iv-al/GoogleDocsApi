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
        
        public IEnumerable<SheetWrapper> Sheets()
        {
            return _service.Spreadsheets
                .Get(_spreadsheetId)
                .Execute()
                .Sheets
                .Select(sheet => new SheetWrapper(_service, _spreadsheetId, sheet));
        }
    }
}