using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using TranslationsDocGen.SocialInfinite;

namespace TranslationsDocGen
{
    class Program
    {

        static void Main(string[] args)
        {
            var service = GoogleSheetsHelper.Service();

            using (new FileLogListener("log"))
            {
                string translateDocId = "1dLoAum5bUYS6IpdoAqCwEH8pzd1UOzyHCLH8LLuVZbg";

                string testId = "1W3xvpot628w5JmsW4ZHJ8Z3NVYflFIBq1VHrUjX1II8"; // test
//                string test2Id = "1pB3DCuq2T6tuZB1TArYfjgzhTtgO6Z2sipxvzfIYfN8"; // test copy

            string royalItemsId = "1Cfb3MR8pmKBlIi4rSwhkY157A9E9ttqAKgp0vFgh958";    // FamilyNest2 Localization items
            string royalTextId = "1Esa52xsi64tPOgqakibdx_ISvOroyXKajDu3-DQwbp8";    // FamilyNest2 Game Text Localization

//            string farmdaysItemsId = "1QwMS32adbemjNFptjGO9hJa6SQf5D_nVKPDC9PDYJTQ";    // Farmdays items copy
//            string farmdaysTextId = "119Ep1_oLQRJz7akYR6YES43-fzfkHYswumJvRIJ2xh4";    // Farmdays texts copy

//            string mainItemsJapanId = "1utJVFhmdUM9DL9IgsGe6PkMFBBBPlLe0x9EVlpZYN10";    // Japan Items Localization
//            string mainTextJapanId = "1v6NVFcHYn1mf9SmwpsojOYKlVnyC4R30o1YfeQJGYzo";    // Japan Items Localization


            service.UploadMissingLocaleSpreadsheet(testId, locale: "ja_JP", defaultLocale: "ru_RU", newSpreadsheetTitle: "ApiTestCopy2"); 
//            service.UploadMissingLocaleSpreadsheet(royalItemsId, "ja_JP", "en_US", "FN2_Items_2019_01_29_9eac592"); 
//            service.UploadMissingLocaleSpreadsheet(royalTextId, "ja_JP", "en_US", "FN2_Text_2019_01_29_9eac592"); 


//                var spreadsheetFrom = service.DownloadSpredsheet(translateDocId);
//                var spreadsheetTo = service.DownloadSpredsheet(royalTextId);
//
//                var requests =
//                    SocialInfiniteSheetsHelper.CopySpreadsheetLocale(spreadsheetFrom, spreadsheetTo, "en_US", true);
//                spreadsheetTo.BatchUpdate(requests);

//            var sheetFrom = spreadsheetFrom.SheetByTitle("Quests")[0];
//            var sheetTo = spreadsheetTo.SheetByTitle("Quests")[0];

//            var requests = SocialInfiniteSheetsHelper.CopySheetsLocale(sheetFrom, sheetTo, "ja_JP");
//            spreadsheetTo.BatchUpdate(requests);

            }


            Console.WriteLine("Done");
        }

        
    }
}