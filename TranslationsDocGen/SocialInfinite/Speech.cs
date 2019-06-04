using System;
using System.Collections.Generic;
using System.Text;

namespace TranslationsDocGen.SocialInfinite {
    public class Speech
    {
        public readonly string CharacterName;
        public readonly string Text;
        public readonly bool IsBig;

        public Speech(SheetAdapter sheet, int startRow, int column, Dictionary<string, string> characters,
            string bigDialogMarker, bool isSpeechOnTwoRows)
        {
            string nameCell;
                
            if (isSpeechOnTwoRows)
            {
                Text = sheet.CellValue(startRow + 1, column);
                nameCell = sheet.CellValue(startRow, column);
            }
            else
            {
                string wholeCell = sheet.CellValue(startRow, column);
                string[] nameAndText =  wholeCell.Split(new []{':'}, 2);
                if (nameAndText.Length != 2) throw new Exception($"Speech-> cant split cell: {wholeCell}");

                nameCell = nameAndText[0];
                Text = nameAndText[1];
            }

            foreach (KeyValuePair<string,string> pair in characters)
            {
                string rowName = pair.Key.ToLower();
                if (nameCell.ToLower().IndexOf(rowName) >= 0)
                {
                    CharacterName = pair.Value;
                }
            }

            if (CharacterName == null) throw new Exception($"Speech-> unknown character: {nameCell}");

            IsBig = nameCell.ToLower().IndexOf(bigDialogMarker.ToLower()) >= 0;


            if (String.IsNullOrWhiteSpace(CharacterName) || String.IsNullOrWhiteSpace(CharacterName) )    {
                if (CharacterName == null) throw new Exception($"Speech-> empty Speech Name: {nameCell}, Text: {Text}");
            }
        }

        private string LocalizationKey(string dialogName, int speechNum)
        {
            return dialogName + "_" + speechNum;
        }

        public string Config(string dialogName, int speechNum)
        {
            var res = new StringBuilder();

            void append(int tabsCount, string str)
            {
                for(int i = 0; i < tabsCount; ++i) res.Append("  ");
                res.Append(str + "\n");
            }
            
            append(2, $"- character: \"{CharacterName}\"");
            append(3, $"text_key: \"{LocalizationKey(dialogName, speechNum)}\"");
            
            return res.ToString();
        }

        public IList<IList<object>> Localization(string dialogName, int speechNum)
        {
            return new List<IList<object>>()
            {
                new List<object>() {LocalizationKey(dialogName, speechNum), Text}
            };
        }
    }
}