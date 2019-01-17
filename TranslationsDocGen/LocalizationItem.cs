using System.Collections.Generic;
using System.Linq;

namespace TranslationsDocGen
{
    public class LocalizationItem
    {
        public readonly string ItemName;
        public List<IList<object>> Rows = new List<IList<object>>();
        
        public LocalizationItem(string itemName)
        {
            ItemName = itemName;
        }

        public List<IList<object>> RowsWithName()
        {
            return new List<IList<object>>()
                {
                    new List<object>() {ItemName}
                }
                .Concat(Rows)
                .ToList();
        }
    }
}