﻿using System.Collections.Generic;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace TranslationsDocGen
{
    public class SheetWrapper
    {
        
        private readonly SheetsService _service;
        private readonly string _spreadsheetId;
        private readonly Sheet _sheet;

        public Sheet Sheet => _sheet;

        public SheetWrapper(SheetsService service, string spreadsheetId, Sheet sheet)
        {
            _service = service;
            _spreadsheetId = spreadsheetId;
            _sheet = sheet;
        }

        private IList<IList<object>> _values;
        public IList<IList<object>> Values()
        {
            if (_values == null)
            {
////            String range = "Class Data!A2:E";
                _values = _service.Spreadsheets.Values
                    .Get(_spreadsheetId, _sheet.Properties.Title)
                    .Execute()
                    .Values;
            }

            return _values;
        }
    }
}