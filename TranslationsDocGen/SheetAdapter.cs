using System.Collections.Generic;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace TranslationsDocGen
{
    public class SheetAdapter
    {
        
        private readonly SheetsService _service;
        private readonly string _spreadsheetId;
        public readonly Sheet Sheet;

        public string Title() => Sheet.Properties.Title;
        public int Id() => Sheet.Properties.SheetId.Value;

        public SheetAdapter(SheetsService service, string spreadsheetId, Sheet sheet)
        {
            _service = service;
            _spreadsheetId = spreadsheetId;
            Sheet = sheet;
        }

        private IList<IList<object>> _values;
        public IList<IList<object>> Values()
        {
            if (_values == null)
            {
////            String range = "Class Data!A2:E";
                _values = _service.Spreadsheets.Values
                    .Get(_spreadsheetId, Sheet.Properties.Title)
                    .Execute()
                    .Values;
            }

            return _values;
        }
        
        public Request UpdateRangeRequest(IList<IList<object>> values, int startRow, int startColumn)
        {
            return GoogleSheetsHelper.UpdateRequest(values, Sheet.Properties.SheetId.Value, startRow, startColumn);
        }

        public Request UpdateCellRequest(string value, int row, int column) 
        {
            var values = new List<IList<object>>()
            {
                new List<object>() {value},
            };
            
            return UpdateRangeRequest(values, row, column);
        }
    }
}