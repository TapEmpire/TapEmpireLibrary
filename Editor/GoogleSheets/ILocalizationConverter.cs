using System.Collections.Generic;
using System.Text;

namespace TapEmpire.Utility.GoogleSheet
{
    public interface ILocalizationConverter
    {
        List<List<string>> Convert(string tableName, List<List<string>> data);
        List<TranslationEntryContainer> Deconvert(string tableName, List<TranslationEntryContainer> data, StringBuilder stringBuilder);
    }
}