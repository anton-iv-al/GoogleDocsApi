using System;
using System.Collections.Generic;

namespace TranslationsDocGen
{
    class Program
    {

        static void Main(string[] args)
        {
            var service = GoogleSheetsHelper.Service();
            
            string spreadsheetId = "1W3xvpot628w5JmsW4ZHJ8Z3NVYflFIBq1VHrUjX1II8";
            var spreadsheet = new SpreadsheetAdapter(service, spreadsheetId);

            spreadsheet.WriteToConsole();
        }

    }
}