﻿using System;
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
            DialogMain();
        }

        static void DialogMain()
        {
            var service = GoogleSheetsHelper.Service();
            
            
            int startId = 230381;
            string dialogKey = "dialog_blooming_cave7";
            

            string spredsheetId = "1KNGKOb25zUfvYDXEsbYyuVfjLIJOrgBy_sP4Leo0kEw";
            string sheetName = "Dialogs";
            
            var characters = new Dictionary<string, string>()
            {
                {"Дональд", "task_supplier"},
                {"Кэтрин", "default_character"},
                {"Андервуд", "english_customer_character1"},
                {"Марит", "indian_customer_character1"},
                {"Митрофан", "russian_customer_character1"},
                {"Липкий", "pirate_jack_character"},
                {"Беатрис", "beatrix"},
                {"Ферн", "fern"},
                {"Макс", "maks_character"},
            };
            string bigDialogMarker = "большой";
            bool isSpeechOnTwoRows = false;
            
            service.GenerateDialog(spredsheetId, sheetName, startId, dialogKey, characters, bigDialogMarker, isSpeechOnTwoRows);
        }

        

        static void TransationMain()
        {
            var service = GoogleSheetsHelper.Service();

            using (new FileLogListener("log"))
            {
//                string translateDocId = "1qdG06KX6-60yJFI-umM9tMvZH7IjvS_Gr2QC0HMPhqw";
//
//                string testId = "1W3xvpot628w5JmsW4ZHJ8Z3NVYflFIBq1VHrUjX1II8"; // test
//                string test2Id = "1pB3DCuq2T6tuZB1TArYfjgzhTtgO6Z2sipxvzfIYfN8"; // test copy

            string royalItemsId = "1Cfb3MR8pmKBlIi4rSwhkY157A9E9ttqAKgp0vFgh958";    // FamilyNest2 Localization items
            string royalTextId = "1Esa52xsi64tPOgqakibdx_ISvOroyXKajDu3-DQwbp8";    // FamilyNest2 Game Text Localization

//            string farmdaysItemsId = "1QwMS32adbemjNFptjGO9hJa6SQf5D_nVKPDC9PDYJTQ";    // Farmdays items copy
//            string farmdaysTextId = "119Ep1_oLQRJz7akYR6YES43-fzfkHYswumJvRIJ2xh4";    // Farmdays texts copy

//            string mainItemsJapanId = "1-7Plj2vXP2ZLQt1zSiTM3n_3rapR4ZF_tJYSQUFqv6s";    // Japan Items Localization
//            string mainTextJapanId = "1ELMCFvAHGYZ5E_tjuWBbbYBTYxlNng3fn8PwmWvt6qo";    // Japan Items Localization


//            service.UploadMissingLocaleSpreadsheet(testId, locale: "ja_JP", defaultLocale: "ru_RU", newSpreadsheetTitle: "ApiTestCopy2"); 
            service.UploadMissingLocaleSpreadsheet(royalItemsId, "ja_JP", "en_US", "FN2_Items_2019_05_16"); 
            service.UploadMissingLocaleSpreadsheet(royalTextId, "ja_JP", "en_US", "FN2_Text_2019_05_16"); 


//            var spreadsheetFrom = service.DownloadSpredsheet(translateDocId);
//            var spreadsheetTo = service.DownloadSpredsheet(royalTextId);
////
//                var requests = new List<Request>();
//                requests.AddRange(SocialInfiniteSheetsHelper.CopySpreadsheetLocale(spreadsheetFrom, spreadsheetTo, "ru_RU", true));
//                requests.AddRange(SocialInfiniteSheetsHelper.CopySpreadsheetLocale(spreadsheetFrom, spreadsheetTo, "lv_LV", true));
//                requests.AddRange(SocialInfiniteSheetsHelper.CopySpreadsheetLocale(spreadsheetFrom, spreadsheetTo, "en_US", true));
//                requests.AddRange(SocialInfiniteSheetsHelper.CopySpreadsheetLocale(spreadsheetFrom, spreadsheetTo, "ja_JP", true));
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