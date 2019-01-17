﻿using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

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


            service.UploadMissingLocaleSpreadsheet(testId, "lv_LV", "ApiTestCopy");
            
            
//            var spreadsheet = service.DownloadSpredsheet("1mSAmOmKz6XRRXPvRX7AZnNE9hb6hFDQlF8QZMXGIoRc");
//
//            var requests = new List<Request>()
//            {
//                spreadsheet.SheetsByTitle("MySheet1")[0].UpdateCellRequest("200000000000000000000000000", 0, 0),
////                spreadsheet.SheetsByTitle("MySheet1")[0].UpdateCellRequest("115", 12, 4),
////                spreadsheet.SheetsByTitle("MySheet1")[0].UpdateCellRequest("116", 12, 2),
//            };
//            spreadsheet.BatchUpdate(requests);
            
            
            Console.WriteLine("Done");
        }

        
    }
}