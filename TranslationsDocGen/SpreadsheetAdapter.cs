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
        private readonly Spreadsheet _spreadsheet;

        public SpreadsheetAdapter(SheetsService service, Spreadsheet spreadsheet)
        {
            _service = service;
            _spreadsheet = spreadsheet;
        }
        
        private List<SheetAdapter> _sheetsCache;
        public List<SheetAdapter> Sheets()
        {
            if (_sheetsCache == null)
            {
                _sheetsCache = _spreadsheet
                    .Sheets
                    .Select(sheet => new SheetAdapter(_service, _spreadsheet.SpreadsheetId, sheet))
                    .ToList();
            }

            return _sheetsCache;
        }
        
        private Dictionary<int, SheetAdapter> _sheetByIdCache;
        public SheetAdapter SheetById(int id)
        {
            if (_sheetByIdCache == null)
            {
                _sheetByIdCache = Sheets().ToDictionary(s => s.Id());
            }

            return _sheetByIdCache.ContainsKey(id)
                ? _sheetByIdCache[id]
                : null;
        }
        
        private Dictionary<string, SheetAdapter> _sheetByTitleCache;
        public SheetAdapter SheetByTitle(string title)
        {
            if (_sheetByTitleCache == null)
            {
                _sheetByTitleCache = Sheets()
                    .ToDictionary(s => s.Title());
            }

            return _sheetByTitleCache.ContainsKey(title)
                ? _sheetByTitleCache[title]
                : null;
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

        public void BatchUpdate(IList<Request> requests)
        {
            if (!requests.Any()) return;
            
            _service.Spreadsheets.BatchUpdate(
                    new BatchUpdateSpreadsheetRequest() {Requests = requests},
                    _spreadsheet.SpreadsheetId
                )
                .Execute();
        }

//        public Request AppendRequest(SheetData sheet)
//        {
//            var request = _service.Spreadsheets.Values.Append(
//                new ValueRange() { Values = sheet.Values },
//                _spreadsheetId,
//                sheet.Title
//            );
//            
//            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
////            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
//
//            var r = new Request();
//            r.AppendCells = 
//            r.UpdateSpreadsheetProperties = new UpdateSpreadsheetPropertiesRequest(){Properties = };
//            r.UpdateCells = new UpdateCellsRequest() {};
//
//            return r;
//        }

    }
}