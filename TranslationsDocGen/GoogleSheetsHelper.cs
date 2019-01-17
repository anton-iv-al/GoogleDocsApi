using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace TranslationsDocGen
{
    public static class GoogleSheetsHelper
    {
        
        public static SheetsService Service()
        {
            // to generate credentials.json
            // https://developers.google.com/sheets/api/quickstart/dotnet
            
            // If modifying scopes, delete your previously saved credentials
            // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new []{ SheetsService.Scope.Spreadsheets },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
//                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Awesome Name",
            });
        }


        public static SpreadsheetAdapter DownloadSpredsheet(this SheetsService service, string spreadsheetId)
        {
            var spreadsheet = service.Spreadsheets
                .Get(spreadsheetId)
                .Execute();
            
            return new SpreadsheetAdapter(service, spreadsheet);
        }

        public static SpreadsheetAdapter UploadSpreadsheet(this SheetsService service, string title, IEnumerable<SheetData> sheets)
        {
            var spreadsheet = service.Spreadsheets
                .Create(new Spreadsheet()
                {
                    Properties = new SpreadsheetProperties() {Title = title},
                    Sheets = sheets
                        .Select(s => new Sheet() {Properties = new SheetProperties() {Title = s.Title}})
                        .ToList(),
                })
                .Execute();

            var spreadsheetAdapter = new SpreadsheetAdapter(service, spreadsheet);

            
            if (sheets.Any())
            {
                var requests = sheets
                    .Select((sheet, i) =>
                    {
                        int sheetId = spreadsheet.Sheets[i].Properties.SheetId.Value;
                        return UpdateRequest(sheet.Values, sheetId, 0, 0);
                    })
                    .ToList();

                spreadsheetAdapter.BatchUpdate(requests);
            }

            return spreadsheetAdapter;
        }
        
        
        public static Request UpdateRequest(IList<IList<object>> values, int sheetId, int startRow, int startColumn)
        {
            int rowCount = values.Count;
            int columnCount = values.Select(row => row.Count).Max();
            
            var r = new Request();
            r.UpdateCells = new UpdateCellsRequest()
            {
                Fields = "*",
                Range = new GridRange()
                {
                    SheetId = sheetId,
                    StartRowIndex = startRow,
                    StartColumnIndex = startColumn,
                    EndRowIndex = startRow + rowCount,
                    EndColumnIndex =  startColumn + columnCount,
                },
                Rows = values
                    .Select(row => new RowData()
                    {
                        Values = row
                            .Select(cell => new CellData()
                            {
                                UserEnteredFormat = new CellFormat(){WrapStrategy = "WRAP"},
                                UserEnteredValue = new ExtendedValue()
                                {
                                    StringValue = (cell as string) ?? cell.ToString()
                                }
                            })
                            .ToList()
                    })
                    .ToList()
            };

            return r;
        }
        
        

        public static string CellValue(this IList<object> row, int column)
        {
            if (row.Count <= column)
            {
                return "";
            }
            else
            {
                return row[column] as string;
            }
        }

        public static string CellValue(this IList<IList<object>> sheetValues, int row, int column)
        {
            if (sheetValues.Count <= row)
            {
                return "";
            }
            else
            {
                return sheetValues[row].CellValue(column);
            }
        }
       
    }
}