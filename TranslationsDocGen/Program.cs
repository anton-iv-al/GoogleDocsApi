using System;
using System.Collections.Generic;
using System.Linq;

namespace TranslationsDocGen
{
    class Program
    {

        static void Main(string[] args)
        {
            var service = GoogleSheetsHelper.Service();
            
            string testId = "1W3xvpot628w5JmsW4ZHJ8Z3NVYflFIBq1VHrUjX1II8";    // test
//            string mainItemsId = "1Cfb3MR8pmKBlIi4rSwhkY157A9E9ttqAKgp0vFgh958";    // FamilyNest2 Localization items
//            string mainTextId = "1Esa52xsi64tPOgqakibdx_ISvOroyXKajDu3-DQwbp8";    // FamilyNest2 Game Text Localization
            
            var spreadsheet = new SpreadsheetAdapter(service, testId);

            var sheet = spreadsheet.Sheets().ToList()[2];

            var filteredRows = sheet.RowsWithoutLocale("ja_JP", 2);
            
            foreach (IList<object> row in filteredRows)
            {
                foreach (string cell in row)
                {
                    Console.Write(cell + ", ");
                }
                Console.WriteLine();
            }
            
            Console.WriteLine();

//            spreadsheet.WriteToConsole();
        }

        
    }
}