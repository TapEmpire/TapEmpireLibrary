using System;
using System.Collections.Generic;

namespace TapEmpire.Utility.GoogleSheet
{
    [Serializable]
    public class GoogleSheetSelector
    {
        public List<SpreadSheetData> spreadSheetDatas;
        public string LevelEnd;

        public string GetSpreadSheet(string levelName)
        {
            var data = GetSpreadSheetData(levelName);
            return data.Id;
        }

        public SpreadSheetData GetSpreadSheetData(string levelName)
        {
            if (!levelName.EndsWith(LevelEnd))
            {
                return spreadSheetDatas[0];
            }

            var levelNumber = int.Parse(levelName.Split('_')[0]);

            return spreadSheetDatas.FindLast(data => data.Level <= levelNumber);
        }
    }

    [Serializable]
    public struct SpreadSheetData
    {
        public string Id;
        public int Level;
        public int TemplateSheetId;
    }
}