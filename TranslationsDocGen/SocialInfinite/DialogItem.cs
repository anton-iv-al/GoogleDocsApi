using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationsDocGen.SocialInfinite
{
    public class DialogItem
    {
        public readonly int Id;
        public readonly string ItemName;
        public readonly IEnumerable<Speech> Speeches;
        public readonly bool IsBig;
        public int? CompleteDialog;
        
        public DialogItem(int id, string itemName, IEnumerable<Speech> speeches, bool isBig)
        {
            Id = id;
            ItemName = itemName;
            Speeches = speeches;
            IsBig = isBig;
        }

        public string Config()
        {
            var res = new StringBuilder();

            void append(int tabsCount, string str)
            {
                for(int i = 0; i < tabsCount; ++i) res.Append("  ");
                res.Append(str + "\n");
            }

            append(0, ItemName + ":");
            append(1, "<<: *default_dialog");
            append(1, $"id: {this.Id}");
            append(1, $"big_dialog: {this.IsBig.ToString().ToLower()}");
            if(this.CompleteDialog.HasValue) append(1, $"complete_dialog: {this.CompleteDialog.Value}");
            append(1, "messages:");

            int speechNum = 1;
            foreach (var speech in this.Speeches)
            {
                res.Append(speech.Config(this.ItemName, speechNum));
                speechNum++;
            }
            
            return res.ToString();
        }

        public IList<IList<object>> Localization()
        {
            return this.Speeches.SelectMany((s, i) => s.Localization(this.ItemName, i+1)).ToList();
        }
    }
}