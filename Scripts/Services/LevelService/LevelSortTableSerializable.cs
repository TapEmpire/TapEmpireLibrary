using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Services
{
    [System.Serializable]
    public class LevelSortTableSerializable : IRemoteSerializable
    {
        [SerializeField] private LevelSortTable _levelSortTable = null;

        public class LevelSortTableRemoteModel
        {
            public int[] Order;

            public LevelSortTableRemoteModel() {}

            public LevelSortTableRemoteModel(LevelSortTable settings)
            {
                Order = settings.Order.ToArray();
            }
        }

        public string TokenName => "LevelSortTable";

        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<LevelSortTableRemoteModel>();
            _levelSortTable.Order = model.Order;
        }

        public string SerializeJson()
        {
            var model = new LevelSortTableRemoteModel(_levelSortTable);
            var result = JsonConvert.SerializeObject(model);

            return result;
        }

        [Button("Serialize to file")]
        private void SerializeToFile()
        {
            var json = SerializeJson();
            FileUtility.SaveText("Save waves JSON", TokenName, json);
        }

        [Button("Serialize to console")]
        private void SerializeToConsole()
        {
            var json = SerializeJson();
            Debug.Log(json);
        }
    }
}
