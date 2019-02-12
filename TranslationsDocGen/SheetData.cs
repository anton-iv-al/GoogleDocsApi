using System.Collections.Generic;
using Google.Apis.Sheets.v4.Data;

namespace TranslationsDocGen
{
    public class SheetData
    {
        public string Title = "sheet";
        public IList<IList<object>> Values = new List<IList<object>>();
        public GridProperties GridProperties;
    }
}