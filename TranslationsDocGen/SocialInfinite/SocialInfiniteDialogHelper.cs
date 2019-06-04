using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace TranslationsDocGen.SocialInfinite
{
    public static class SocialInfiniteDialogHelper
    {
        public static void GenerateDialog(this SheetsService service, string spredsheetId, string sheetName,
            int startId, string dialogKey, Dictionary<string, string> characters, string bigDialogMarker,
            bool isSpeechOnTwoRows)
        {
            var spreadsheet = service.DownloadSpredsheet(spredsheetId);
            var rowDialogSheet = spreadsheet.SheetByTitle(sheetName);

            var column = 0;
            
            var speeches = Speeches(rowDialogSheet, column, characters, bigDialogMarker, isSpeechOnTwoRows);
            var dialogItems = DialogItems(speeches, startId, dialogKey);

            foreach (var dialogItem in dialogItems)
            {
                Console.WriteLine(dialogItem.Config());
            }

            spreadsheet.BatchUpdate(
                dialogItems
                    .Select(d => rowDialogSheet.AppendRequest(d.Localization()))
                    .ToList()
            );
        }

        private static IEnumerable<Speech> Speeches(SheetAdapter sheet, int column,
            Dictionary<string, string> characters, string bigDialogMarker, bool isSpeechOnTwoRows)
        {
            var res = new List<Speech>();
            
            for (int row = 0; row < sheet.Values().Count; ++row)
            {
                if (String.IsNullOrWhiteSpace(sheet.CellValue(row, column)))
                {
                    continue;
                }
                
                res.Add(new Speech(sheet, row, column, characters, bigDialogMarker, isSpeechOnTwoRows));
                
                if (isSpeechOnTwoRows)
                {
                    row += 1;
                }
            }

            return res;
        }

        private static IEnumerable<DialogItem> DialogItems(IEnumerable<Speech> speeches, int startId, string dialogKey)
        {
            var result = new List<DialogItem>();

            int currentId = startId;
            int currentDialogNum = 1;
            var currentDialogSpeeches = new List<Speech>();

            void continueFromLast()
            {
                if(result.Any()) result.Last().CompleteDialog = currentId;
            }

            void tryAddCurrentToResult(bool isBig)
            {
                if (!currentDialogSpeeches.Any()) return;
                
                continueFromLast();
                
                string name = dialogKey + "_" + currentDialogNum.ToString();
                result.Add(new DialogItem(currentId, name, currentDialogSpeeches, isBig));
                
                currentId++;
                currentDialogNum++;
                currentDialogSpeeches = new List<Speech>();
            }

            foreach (var speech in speeches)
            {
                if (speech.IsBig)
                {
                    tryAddCurrentToResult(false);
                    currentDialogSpeeches.Add(speech);
                    tryAddCurrentToResult(true);
                }
                else
                {
                    currentDialogSpeeches.Add(speech);
                }
            }
            
            tryAddCurrentToResult(false);

            return result;
        }
    }
}